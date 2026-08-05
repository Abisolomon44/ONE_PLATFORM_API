using System.Data;
using Microsoft.Data.SqlClient;
using ONEERP.Platform.API.Data;
using ONEERP.Platform.API.DTOs;
using ONEERP.Platform.API.Models;
using ONEERP.Platform.API.Repositories;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.Platform.API.Services;

public interface ITenantService
{
    Task<PaginatedResult<TenantDto>> GetPagedAsync(int pageNumber, int pageSize, string search);
    Task<TenantDto> GetByIdAsync(int tenantId);
    Task<IEnumerable<TenantDto>> GetActiveAsync();
    Task<TenantDto> CreateAsync(CreateTenantRequest request, string? currentUser);
    Task<TenantDto> UpdateAsync(int tenantId, UpdateTenantRequest request, string? currentUser);
    Task<TenantDto> UpdateStatusAsync(int tenantId, string status, string? currentUser);
    Task<bool> DeleteAsync(int tenantId, string? currentUser, bool dropDatabase = false);
    Task<IEnumerable<TenantConnectionDto>> GetConnectionsAsync(int tenantId);
}

public class TenantService : ITenantService
{
    private readonly IDbConnectionFactory _factory;
    private readonly ITenantRepository _tenantRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ITenantConnectionRepository _connectionRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IDatabaseProvisioningService _provisioningService;
    private readonly IAuditService _auditService;
    private readonly ILogger<TenantService> _logger;
    private readonly string _serverName;

    public TenantService(
        IDbConnectionFactory factory,
        ITenantRepository tenantRepository,
        ISubscriptionRepository subscriptionRepository,
        ITenantConnectionRepository connectionRepository,
        IPlanRepository planRepository,
        IDatabaseProvisioningService provisioningService,
        IAuditService auditService,
        ILogger<TenantService> logger,
        IConfiguration configuration)
    {
        _factory = factory;
        _tenantRepository = tenantRepository;
        _subscriptionRepository = subscriptionRepository;
        _connectionRepository = connectionRepository;
        _planRepository = planRepository;
        _provisioningService = provisioningService;
        _auditService = auditService;
        _logger = logger;
        _serverName = configuration["Provisioning:ServerName"] ?? "localhost";
    }

    public async Task<PaginatedResult<TenantDto>> GetPagedAsync(int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;

        var items = await _tenantRepository.GetPagedAsync(normalizedPage, normalizedSize, search);
        var total = await _tenantRepository.CountAsync(search);

        return new PaginatedResult<TenantDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<TenantDto> GetByIdAsync(int tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId)
            ?? throw new NotFoundException($"Tenant '{tenantId}' was not found.");
        return ToDto(tenant);
    }

    public async Task<IEnumerable<TenantDto>> GetActiveAsync()
    {
        var tenants = await _tenantRepository.GetActiveAsync();
        return tenants.Select(t => new TenantDto
        {
            TenantId = t.TenantId,
            TenantCode = t.TenantCode,
            TenantName = t.TenantName,
            CompanyName = t.CompanyName,
            DatabaseName = t.DatabaseName,
            PlanId = t.PlanId,
            ContactEmail = t.ContactEmail,
            AdminUsername = t.AdminUsername,
            AdminPassword = t.AdminPassword,
            Status = t.Status,
            CreatedDate = t.CreatedDate
        });
    }

    public async Task<TenantDto> CreateAsync(CreateTenantRequest request, string? currentUser)
    {
        // --- Validations (must be satisfied before any database work) ---
        var databaseName = request.DatabaseName.Trim();

        if (await _tenantRepository.CodeExistsAsync(request.TenantCode))
            throw new DomainException($"Tenant code '{request.TenantCode}' is already in use.");

        if (await _tenantRepository.DatabaseNameExistsAsync(databaseName))
            throw new DomainException($"Database name '{databaseName}' is already in use.");

        var adminUsername = request.AdminUsername.Trim();
        if (await _tenantRepository.AdminUsernameExistsAsync(adminUsername))
            throw new DomainException($"Admin username '{adminUsername}' is already in use.");

        if (await _provisioningService.DatabaseExistsAsync(databaseName))
            throw new DomainException($"Database '{databaseName}' already exists on the server.");

        var plan = await _planRepository.GetByIdAsync(request.PlanId)
            ?? throw new NotFoundException($"Plan '{request.PlanId}' was not found.");

        var companyName = request.CompanyName.Trim();
        var companyCode = request.TenantCode.Trim().ToUpperInvariant();
        var adminEmail = string.IsNullOrWhiteSpace(request.ContactEmail)
            ? $"{adminUsername.ToLowerInvariant()}@{companyCode.ToLowerInvariant()}.com"
            : request.ContactEmail.Trim();
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword);

         // --- 1. Persist tenant + initial subscription in a single transaction ---
         int tenantId;
         try
         {
             using (var connection = _factory.CreatePlatformConnection())
             {
                 connection.Open();
                 using var transaction = connection.BeginTransaction();

                 var tenant = new Tenant
                 {
                     TenantCode = request.TenantCode.Trim(),
                     TenantName = request.TenantName.Trim(),
                     CompanyName = companyName,
                     DatabaseName = databaseName,
                     PlanId = request.PlanId,
                     ContactEmail = request.ContactEmail,
                     AdminUsername = adminUsername,
                     AdminPassword = request.AdminPassword,
                     Status = request.Status,
                     CreatedBy = currentUser,
                     ModifiedBy = currentUser
                 };

                 tenantId = await _tenantRepository.InsertAsync(tenant, connection, transaction);

                 var subscription = new Subscription
                 {
                     TenantId = tenantId,
                     PlanId = request.PlanId,
                     StartDate = request.SubscriptionStart,
                     EndDate = request.SubscriptionEnd,
                     Amount = plan.MonthlyPrice,
                     Status = SubscriptionStatus.Active,
                     CreatedBy = currentUser,
                     ModifiedBy = currentUser
                 };

                 await _subscriptionRepository.InsertAsync(subscription, connection, transaction);
                 await _tenantRepository.UpdatePlanAsync(tenantId, request.PlanId, connection, transaction);

                 transaction.Commit();
             }
         }
         catch (SqlException ex) when (ex.Number is 2627 or 2601)
         {
             if (ex.Message.Contains("UQ_Tenants_TenantCode", StringComparison.OrdinalIgnoreCase))
                 throw new DomainException($"Tenant code '{request.TenantCode.Trim()}' is already in use.", 409);
             if (ex.Message.Contains("UQ_Tenants_DatabaseName", StringComparison.OrdinalIgnoreCase))
                 throw new DomainException($"Database name '{databaseName}' is already in use.", 409);
             if (ex.Message.Contains("UQ_Tenants_AdminUsername", StringComparison.OrdinalIgnoreCase))
                 throw new DomainException($"Admin username '{adminUsername}' is already in use.", 409);
             throw new DomainException("A tenant with these details already exists.", 409);
         }

        // --- 2. Provision the tenant database (create DB + schema + seed) ---
        try
        {
            await _provisioningService.CreateDatabaseAsync(databaseName);
            await _provisioningService.ExecuteScriptsAsync(databaseName, new TenantProvisioningOptions
            {
                CompanyCode = companyCode,
                CompanyName = companyName,
                CompanyEmail = adminEmail,
                CurrencyCode = plan.CurrencyCode,
                AdminUsername = adminUsername,
                AdminPasswordHash = adminPasswordHash,
                AdminFullName = "ERP Administrator"
            });
        }
        catch (Exception ex)
        {
            await _provisioningService.DropDatabaseAsync(databaseName);
            await CleanupTenantRecordsAsync(tenantId, currentUser);
            throw new DomainException($"Database provisioning failed: {ex.Message}", 500);
        }

        // --- 3. Persist the tenant connection string ---
        try
        {
            var connectionString = _provisioningService.BuildConnectionString(databaseName);
            await _connectionRepository.InsertAsync(new TenantConnection
            {
                TenantId = tenantId,
                ServerName = _serverName,
                DatabaseName = databaseName,
                ConnectionString = connectionString,
                IsActive = true,
                CreatedBy = currentUser
            });
        }
        catch (Exception ex)
        {
            await _provisioningService.DropDatabaseAsync(databaseName);
            await CleanupTenantRecordsAsync(tenantId, currentUser);
            throw new DomainException($"Unable to register tenant connection: {ex.Message}", 500);
        }

        await _auditService.WriteAsync("Tenant", tenantId.ToString(), "Create", currentUser, tenantId);
        return await GetByIdAsync(tenantId);
    }

    public async Task<TenantDto> UpdateAsync(int tenantId, UpdateTenantRequest request, string? currentUser)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId)
            ?? throw new NotFoundException($"Tenant '{tenantId}' was not found.");

        tenant.TenantName = request.TenantName.Trim();
        tenant.CompanyName = request.CompanyName.Trim();
        tenant.ContactEmail = request.ContactEmail;
        tenant.Status = request.Status;
        tenant.ModifiedBy = currentUser;

        await _tenantRepository.UpdateAsync(tenant);
        await _auditService.WriteAsync("Tenant", tenantId.ToString(), "Update", currentUser, tenantId);

        return await GetByIdAsync(tenantId);
    }

    public async Task<TenantDto> UpdateStatusAsync(int tenantId, string status, string? currentUser)
    {
        if (status is not ("Active" or "Suspended"))
            throw new DomainException("Status must be Active or Suspended.");

        var tenant = await _tenantRepository.GetByIdAsync(tenantId)
            ?? throw new NotFoundException($"Tenant '{tenantId}' was not found.");

        if (string.Equals(tenant.Status, status, StringComparison.OrdinalIgnoreCase))
            return await GetByIdAsync(tenantId);

        tenant.Status = status;
        tenant.ModifiedBy = currentUser;

        await _tenantRepository.UpdateAsync(tenant);
        await _auditService.WriteAsync("Tenant", tenantId.ToString(),
            status == "Active" ? "Activate" : "Deactivate", currentUser, tenantId);

        return await GetByIdAsync(tenantId);
    }

    public async Task<bool> DeleteAsync(int tenantId, string? currentUser, bool dropDatabase = false)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId)
            ?? throw new NotFoundException($"Tenant '{tenantId}' was not found.");

        if (dropDatabase && !string.IsNullOrWhiteSpace(tenant.DatabaseName))
        {
            await _provisioningService.DropDatabaseAsync(tenant.DatabaseName);
        }

        await _tenantRepository.SoftDeleteAsync(tenantId, currentUser);
        await _auditService.WriteAsync("Tenant", tenantId.ToString(), "Delete", currentUser, tenantId);
        return true;
    }

    public async Task<IEnumerable<TenantConnectionDto>> GetConnectionsAsync(int tenantId)
    {
        _ = await _tenantRepository.GetByIdAsync(tenantId)
            ?? throw new NotFoundException($"Tenant '{tenantId}' was not found.");

        var connections = await _connectionRepository.GetByTenantIdAsync(tenantId);
        return connections.Select(c => new TenantConnectionDto
        {
            ConnectionId = c.ConnectionId,
            TenantId = c.TenantId,
            ServerName = c.ServerName,
            DatabaseName = c.DatabaseName,
            IsActive = c.IsActive,
            CreatedDate = c.CreatedDate
        });
    }

    private async Task CleanupTenantRecordsAsync(int tenantId, string? currentUser)
    {
        try
        {
            await _tenantRepository.SoftDeleteAsync(tenantId, currentUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean up tenant record {TenantId} after provisioning error", tenantId);
        }
    }

    private static TenantDto ToDto(TenantRow t) => new()
    {
        TenantId = t.TenantId,
        TenantCode = t.TenantCode,
        TenantName = t.TenantName,
        CompanyName = t.CompanyName,
        DatabaseName = t.DatabaseName,
        PlanId = t.PlanId,
        PlanName = t.PlanName,
        CurrencyCode = t.PlanCurrencyCode,
        ContactEmail = t.ContactEmail,
        AdminUsername = t.AdminUsername,
        AdminPassword = t.AdminPassword,
        Status = t.Status,
        SubscriptionStatus = t.SubscriptionStatus,
        SubscriptionStart = t.SubscriptionStart,
        SubscriptionEnd = t.SubscriptionEnd,
        DatabaseProvisioned = t.DatabaseProvisioned,
        CreatedDate = t.CreatedDate
    };
}
