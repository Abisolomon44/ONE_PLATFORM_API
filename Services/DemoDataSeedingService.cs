using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using ONEERP.Platform.API.Data;
using ONEERP.Platform.API.Repositories;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;

namespace ONEERP.Platform.API.Services;

public class DemoSeedResultDto
{
    public int TenantId { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    /// <summary>APPLIED / ALREADY_SEEDED / FAILED</summary>
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Checksum { get; set; }
}

/// <summary>
/// Seeds demo/sample data into a tenant ERP database by executing the
/// embedded, idempotent erp_demo_data.sql script with the same safety
/// machinery as migrations: checksum, version-recorded history row and
/// per-script transaction. No SQL is accepted from the client.
/// </summary>
public interface IDemoDataSeedingService
{
    Task<DemoSeedResultDto> SeedAsync(int tenantId, string executedBy, CancellationToken ct = default);
}

public class DemoDataSeedingService : IDemoDataSeedingService
{
    private readonly ITenantDatabaseResolver _resolver;
    private readonly IDemoDataScriptProvider _scriptProvider;
    private readonly ITenantMigrationLock _migrationLock;
    private readonly ITenantMigrationHistoryRepository _historyRepository;
    private readonly ITenantDatabaseHealthService _healthService;
    private readonly IAuditService _auditService;
    private readonly ILogger<DemoDataSeedingService> _logger;

    public DemoDataSeedingService(
        ITenantDatabaseResolver resolver,
        IDemoDataScriptProvider scriptProvider,
        ITenantMigrationLock migrationLock,
        ITenantMigrationHistoryRepository historyRepository,
        ITenantDatabaseHealthService healthService,
        IAuditService auditService,
        ILogger<DemoDataSeedingService> logger)
    {
        _resolver = resolver;
        _scriptProvider = scriptProvider;
        _migrationLock = migrationLock;
        _historyRepository = historyRepository;
        _healthService = healthService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<DemoSeedResultDto> SeedAsync(int tenantId, string executedBy, CancellationToken ct = default)
    {
        var resolvedTenant = await _resolver.ResolveTenantAsync(tenantId, ct);
        var resolved = await _resolver.ResolveAsync(tenantId, ct);
        await using var connection = resolved.Connection;

        var script = _scriptProvider.GetScript();
        var checksum = _scriptProvider.ComputeChecksum(script);

        // Demo writes stock-neutral master rows only; still guard against
        // colliding with a migration execution on the same tenant database.
        await using var lockHandle = await _migrationLock.AcquireAsync(tenantId, 0, ct);
        if (lockHandle is null)
            throw new DomainException(
                $"A migration is running for tenant '{resolvedTenant.TenantName}'. Try demo seeding again after it completes.", 409);

        var startedAt = DateTime.UtcNow;
        long historyId = await _historyRepository.InsertRunningAsync(new Models.TenantMigrationHistory
        {
            TenantId = tenantId,
            MigrationId = 0,
            ExecutionId = Guid.NewGuid(),
            Version = 0,
            MigrationCode = "ERP-DEMO-001",
            MigrationName = "Demo data seed",
            Status = MigrationStatus.Running,
            Checksum = checksum,
            StartedAt = startedAt,
            ExecutedBy = executedBy
        });

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                script,
                commandTimeout: 300,
                cancellationToken: ct));

            var completedAt = DateTime.UtcNow;
            await _historyRepository.CompleteAsync(
                historyId, MigrationStatus.Success, completedAt,
                (int)Math.Max(0, (completedAt - startedAt).TotalMilliseconds), null);

            // Re-read demo health so the UI gets an honest READY/MISSING after seeding.
            var health = await _healthService.GetHealthAsync(tenantId, ct);
            var status = health.DemoDataStatus == "READY" ? "APPLIED" : "ALREADY_SEEDED";
            var message = status == "APPLIED"
                ? "Demo data seeded successfully (category, brand, units, 5 products)."
                : "Demo seed script executed; some demo keys still missing (see health check).";

            await _auditService.WriteAsync("TenantDemoSeed", tenantId.ToString(), "SEED",
                executedBy, tenantId, $"Checksum={checksum}; Status={status}");

            return new DemoSeedResultDto
            {
                TenantId = tenantId,
                DatabaseName = resolved.DatabaseName,
                Status = status,
                Message = message,
                Checksum = checksum,
            };
        }
        catch (Exception ex)
        {
            var completedAt = DateTime.UtcNow;
            var safeError = ex is SqlException sql
                ? $"SQL error {sql.Number}: {FirstLine(sql.Message)}"
                : FirstLine(ex.Message);
            await _historyRepository.CompleteAsync(historyId, MigrationStatus.Failed, completedAt,
                (int)Math.Max(0, (completedAt - startedAt).TotalMilliseconds), safeError);
            throw new DomainException($"Demo seeding failed: {safeError}", 500);
        }
    }

    private static string FirstLine(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return string.Empty;
        var newline = message.IndexOfAny(new[] { '\r', '\n' });
        return (newline >= 0 ? message[..newline] : message).Trim();
    }
}
