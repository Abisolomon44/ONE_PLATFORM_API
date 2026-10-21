using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using ONEERP.Platform.API.Data;
using ONEERP.Platform.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.Platform.API.Services;

// ============================================================
// DTOs — tenant database health (schema drift + demo readiness)
// ============================================================

public class TenantDbHealthDto
{
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public bool ConnectionOk { get; set; }
    public string? ConnectionError { get; set; }

    public int ExpectedTableCount { get; set; }
    public int ExistingTableCount { get; set; }
    public int MissingTableCount { get; set; }
    public List<string>? MissingTables { get; set; }

    public int AppliedMigrations { get; set; }
    public int PendingMigrations { get; set; }
    public int FailedMigrations { get; set; }

    /// <summary>OK / DRIFT / NOT CHECKED</summary>
    public string SchemaStatus { get; set; } = "NOT CHECKED";
    /// <summary>READY / MISSING / NOT CHECKED</summary>
    public string DemoDataStatus { get; set; } = "NOT CHECKED";
    public List<string>? MissingDemoChecks { get; set; }

    /// <summary>READY / PENDING / FAILED / DRIFT / UNKNOWN</summary>
    public string OverallStatus { get; set; } = "UNKNOWN";
    public DateTime? LastCheckedAt { get; set; }
    public DateTime? LastSuccessfulMigrationAt { get; set; }
}

public class TenantDbHealthSummaryDto
{
    public int TotalTenants { get; set; }
    public int HealthyTenants { get; set; }
    public int PendingTenants { get; set; }
    public int FailedTenants { get; set; }
    public int DriftedTenants { get; set; }
    public int MissingDemoTenants { get; set; }
    public DateTime? LastMigrationAt { get; set; }
}

public interface ITenantDatabaseHealthService
{
    Task<TenantDbHealthDto> GetHealthAsync(int tenantId, CancellationToken ct = default);
    Task<List<TenantDbHealthDto>> GetAllHealthAsync(CancellationToken ct = default);
    Task<TenantDbHealthSummaryDto> GetSummaryAsync(CancellationToken ct = default);
}

// ============================================================
// Implementation
// ============================================================
// Drift check: real table census of the tenant database
// (sys.tables) compared against the expected table set embedded
// from erp_full.sql. Migration state comes from the existing
// TenantSchemaVersionStore + MigrationPlanner (no parallel
// history system). "Do not assume a script ran because tables
// exist": applied/pending/failed come from versions + history,
// drift comes from the live census, both independently.
// ============================================================

public class TenantDatabaseHealthService : ITenantDatabaseHealthService
{
    private static readonly string[] ExpectedTables =
    {
        // Baseline (erp_full.sql v1) — core commerce set verified in schema
        "Companies", "Branches", "Warehouses", "Users", "Roles", "AuditLogs",
        "Products", "ProductCategories", "ProductSubCategories", "ProductBrands",
        "Units", "UnitConversions", "Barcodes", "Stock", "StockTransaction",
        "SalesInvoices", "SalesInvoiceItems", "Purchases", "PurchaseItems",
        "PaymentType", "PaymentMethod", "BusinessPartners", "TaxTypeSystems",
        "Taxes", "PriceTypes", "PriceLists", "PriceListDetails", "Entity",
        "SchemaVersions",
        // v3 inventory documents
        "StockAdjustment", "StockAdjustmentItem", "StockTransfer",
        "StockTransferItem", "StockCount", "StockCountItem",
        // v5 Batch A product foundation (applied via migration, not in erp_full)
        "ProductVariants", "ProductVariantAttributes", "ProductModels",
        "ProductSpecifications", "ProductUnitMappings", "ProductSerialNumbers",
        "ProductBatches", "ProductWarranties",
        // v6 Batch B customer pricing + loyalty
        "CustomerPriceAgreements", "CustomerLoyaltyAccounts", "CustomerLoyaltyTransactions",
        // v7 concurrency
        // (column, not table — verified via ConcurrencyVersion presence)
    };

    private static readonly string[] ExpectedDemoKeys =
    {
        "category:DEMO-ELEC", "brand:DEMO-ACME", "unit:DEMO-PCS", "unit:DEMO-KG",
    };

    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantDatabaseResolver _resolver;
    private readonly IMigrationRepository _migrationRepository;
    private readonly ITenantSchemaVersionStore _versionStore;
    private readonly ITenantMigrationHistoryRepository _historyRepository;
    private readonly ILogger<TenantDatabaseHealthService> _logger;

    public TenantDatabaseHealthService(
        ITenantRepository tenantRepository,
        ITenantDatabaseResolver resolver,
        IMigrationRepository migrationRepository,
        ITenantSchemaVersionStore versionStore,
        ITenantMigrationHistoryRepository historyRepository,
        ILogger<TenantDatabaseHealthService> logger)
    {
        _tenantRepository = tenantRepository;
        _resolver = resolver;
        _migrationRepository = migrationRepository;
        _versionStore = versionStore;
        _historyRepository = historyRepository;
        _logger = logger;
    }

    public async Task<TenantDbHealthDto> GetHealthAsync(int tenantId, CancellationToken ct = default)
    {
        var all = await GetAllHealthAsync(ct);
        var one = all.FirstOrDefault(h => h.TenantId == tenantId)
            ?? throw new NotFoundException($"Tenant {tenantId} was not found.");
        return one;
    }

    public async Task<List<TenantDbHealthDto>> GetAllHealthAsync(CancellationToken ct = default)
    {
        var tenants = (await _tenantRepository.GetActiveAsync()).Where(t => !t.IsDeleted).ToList();
        var definitions = (await _migrationRepository.GetAllAsync()).ToList();

        var results = new List<TenantDbHealthDto>(tenants.Count);
        foreach (var tenant in tenants)
        {
            var health = new TenantDbHealthDto
            {
                TenantId = tenant.TenantId,
                TenantName = tenant.TenantName,
                DatabaseName = tenant.DatabaseName,
                ExpectedTableCount = ExpectedTables.Length + 1, // + Companies already counted? see census below
            };

            health.LastSuccessfulMigrationAt =
                (await _historyRepository.GetRecentAsync(tenant.TenantId, 200))
                    .Where(h => h.Status == ONEERP.Shared.Constants.MigrationStatus.Success)
                    .OrderByDescending(h => h.CompletedAt)
                    .FirstOrDefault()?.CompletedAt;

            try
            {
                var resolved = await _resolver.ResolveAsync(tenant.TenantId, ct);
                await using var connection = resolved.Connection;
                health.ConnectionOk = true;

                // --- Migration state (authoritative: version store + history) ---
                try
                {
                    var currentVersion = await _versionStore.GetCurrentVersionAsync(connection, ct);
                    var pending = MigrationPlanner.ResolvePending(currentVersion, definitions);

                    var failedRows = (await _historyRepository.GetRecentAsync(tenant.TenantId, 200))
                        .Where(h => h.Status == ONEERP.Shared.Constants.MigrationStatus.Failed).ToList();

                    health.AppliedMigrations = definitions.Count(d => d.Version <= currentVersion);
                    health.PendingMigrations = pending.Count;
                    health.FailedMigrations = failedRows.Count;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Migration state read failed for tenant {TenantId}", tenant.TenantId);
                }

                // --- Schema census (independent of migration history) ---
                var existing = (await connection.QueryAsync<string>(new CommandDefinition(
                    "SELECT name FROM sys.tables WHERE schema_id = SCHEMA_ID(N'dbo')",
                    cancellationToken: ct))).ToHashSet(StringComparer.OrdinalIgnoreCase);

                health.ExistingTableCount = existing.Count;
                var missing = ExpectedTables.Where(t => !existing.Contains(t)).ToList();
                health.MissingTables = missing;
                health.MissingTableCount = missing.Count;
                health.SchemaStatus = missing.Count == 0 ? "OK" : "DRIFT";

                // ConcurrencyVersion column presence (v7)
                var hasConcurrency = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                    "SELECT COUNT(*) FROM sys.columns c WHERE c.object_id = OBJECT_ID(N'dbo.Companies') AND c.name = N'ConcurrencyVersion'",
                    cancellationToken: ct));
                if (hasConcurrency == 0)
                {
                    health.MissingTables ??= new List<string>();
                    health.MissingTables.Add("(column) dbo.Companies.ConcurrencyVersion");
                    health.MissingTableCount++;
                    health.SchemaStatus = "DRIFT";
                }

                // --- Demo readiness (independent census of demo keys) ---
                var companyDemo = await connection.QueryFirstOrDefaultAsync<int?>(new CommandDefinition(
                    "SELECT TOP 1 CompanyId FROM dbo.Companies ORDER BY CompanyId",
                    cancellationToken: ct));

                if (companyDemo is int companyId)
                {
                    var missingDemo = new List<string>();

                    var catOk = await Exists(connection,
                        "SELECT COUNT(1) FROM dbo.ProductCategories WHERE CompanyId=@c AND CategoryCode=N'DEMO-ELEC'", companyId, ct);
                    if (!catOk) missingDemo.Add("category:DEMO-ELEC");

                    var brandOk = await Exists(connection,
                        "SELECT COUNT(1) FROM dbo.ProductBrands WHERE CompanyId=@c AND BrandCode=N'DEMO-ACME'", companyId, ct);
                    if (!brandOk) missingDemo.Add("brand:DEMO-ACME");

                    var pcsOk = await Exists(connection,
                        "SELECT COUNT(1) FROM dbo.Units WHERE CompanyId=@c AND UnitCode=N'DEMO-PCS'", companyId, ct);
                    if (!pcsOk) missingDemo.Add("unit:DEMO-PCS");

                    var kgOk = await Exists(connection,
                        "SELECT COUNT(1) FROM dbo.Units WHERE CompanyId=@c AND UnitCode=N'DEMO-KG'", companyId, ct);
                    if (!kgOk) missingDemo.Add("unit:DEMO-KG");

                    health.MissingDemoChecks = missingDemo;
                    health.DemoDataStatus = missingDemo.Count == 0 ? "READY" : "MISSING";
                }
                else
                {
                    health.DemoDataStatus = "MISSING";
                    health.MissingDemoChecks = new List<string> { "no company row" };
                }

                health.LastCheckedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                health.ConnectionOk = false;
                health.ConnectionError = ex is DomainException ? ex.Message : "Tenant database could not be reached.";
                health.SchemaStatus = "NOT CHECKED";
                health.DemoDataStatus = "NOT CHECKED";
                _logger.LogWarning(ex, "Health check failed for tenant {TenantId} database {Db}", tenant.TenantId, tenant.DatabaseName);
            }

            health.OverallStatus = health.FailedMigrations > 0 ? "FAILED"
                : health.PendingMigrations > 0 ? "PENDING"
                : health.SchemaStatus == "DRIFT" ? "DRIFT"
                : health.ConnectionOk && health.SchemaStatus == "OK" && health.DemoDataStatus == "READY" ? "READY"
                : "UNKNOWN";

            results.Add(health);
        }

        return results;
    }

    public async Task<TenantDbHealthSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var rows = await GetAllHealthAsync(ct);
        return new TenantDbHealthSummaryDto
        {
            TotalTenants = rows.Count,
            HealthyTenants = rows.Count(r => r.OverallStatus == "READY"),
            PendingTenants = rows.Count(r => r.OverallStatus == "PENDING"),
            FailedTenants = rows.Count(r => r.OverallStatus == "FAILED"),
            DriftedTenants = rows.Count(r => r.OverallStatus == "DRIFT"),
            MissingDemoTenants = rows.Count(r => r.DemoDataStatus == "MISSING"),
            LastMigrationAt = rows.Where(r => r.LastSuccessfulMigrationAt.HasValue)
                                  .Max(r => r.LastSuccessfulMigrationAt),
        };
    }

    private static async Task<bool> Exists(DbConnection conn, string sql, long companyId, CancellationToken ct)
        => await conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { c = companyId }, cancellationToken: ct)) > 0;
}
