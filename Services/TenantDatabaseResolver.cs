using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using ONEERP.Platform.API.Data;
using ONEERP.Platform.API.Models;
using ONEERP.Platform.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.Platform.API.Services;

/// <summary>A tenant database that the platform has resolved and validated.</summary>
public record ResolvedTenantDatabase(
    Tenant Tenant,
    string DatabaseName,
    DbConnection Connection);

/// <summary>
/// Trusted platform-side resolver for tenant databases.
/// <para>
/// The connection is always derived from the platform's own tenant record
/// (<c>dbo.Tenants.DatabaseName</c> and the active <c>dbo.TenantConnections</c>
/// row). A connection string is never accepted from the client, and a stored
/// connection string whose Initial Catalog disagrees with the platform tenant
/// record is rejected, so a request for tenant A can never reach tenant B.
/// </para>
/// </summary>
public interface ITenantDatabaseResolver
{
    Task<ResolvedTenantDatabase> ResolveAsync(int tenantId, CancellationToken ct = default);

    /// <summary>Validates the tenant record without opening the tenant database.</summary>
    Task<Tenant> ResolveTenantAsync(int tenantId, CancellationToken ct = default);
}

public class TenantDatabaseResolver : ITenantDatabaseResolver
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantConnectionRepository _connectionRepository;
    private readonly IDbConnectionFactory _factory;
    private readonly ILogger<TenantDatabaseResolver> _logger;

    public TenantDatabaseResolver(
        ITenantRepository tenantRepository,
        ITenantConnectionRepository connectionRepository,
        IDbConnectionFactory factory,
        ILogger<TenantDatabaseResolver> logger)
    {
        _tenantRepository = tenantRepository;
        _connectionRepository = connectionRepository;
        _factory = factory;
        _logger = logger;
    }

    public async Task<Tenant> ResolveTenantAsync(int tenantId, CancellationToken ct = default)
    {
        if (tenantId <= 0)
            throw new DomainException("Tenant id must be greater than zero.", 400);

        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant is null || tenant.IsDeleted)
            throw new NotFoundException($"Tenant {tenantId} was not found.");

        if (string.IsNullOrWhiteSpace(tenant.DatabaseName))
            throw new DomainException($"Tenant '{tenant.TenantName}' has no database configured.", 409);

        return tenant;
    }

    public async Task<ResolvedTenantDatabase> ResolveAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await ResolveTenantAsync(tenantId, ct);
        var databaseName = tenant.DatabaseName.Trim();

        if (!await DatabaseExistsAsync(databaseName, ct))
            throw new DomainException($"Tenant database '{databaseName}' is not available on the server.", 503);

        var connectionString = await BuildConnectionStringAsync(tenant, ct);
        var connection = new SqlConnection(connectionString);

        try
        {
            await connection.OpenAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not connect to tenant database {Database} for tenant {TenantId}.",
                databaseName, tenantId);
            await connection.DisposeAsync();
            throw new DomainException($"Tenant database '{databaseName}' could not be reached.", 503);
        }

        return new ResolvedTenantDatabase(tenant, databaseName, connection);
    }

    private async Task<bool> DatabaseExistsAsync(string databaseName, CancellationToken ct)
    {
        using var master = _factory.CreateMasterConnection();
        return await master.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(1) FROM sys.databases WHERE name = @name AND state_desc = 'ONLINE'",
            new { name = databaseName },
            cancellationToken: ct)) > 0;
    }

    private async Task<string> BuildConnectionStringAsync(Tenant tenant, CancellationToken ct)
    {
        var stored = await _connectionRepository.GetActiveByTenantIdAsync(tenant.TenantId);

        if (TryUseStoredConnection(stored?.ConnectionString, tenant.DatabaseName, out var connectionString))
            return connectionString;

        var derived = new SqlConnectionStringBuilder(_factory.MasterConnectionString)
        {
            InitialCatalog = tenant.DatabaseName.Trim(),
            MultipleActiveResultSets = true,
            ConnectTimeout = 15
        };
        return derived.ConnectionString;
    }

    /// <summary>
    /// Tenant isolation guard. A stored connection string is only adopted when
    /// its Initial Catalog matches the platform tenant record; anything else is
    /// discarded in favour of a connection derived from the tenant record, so a
    /// request can never reach another tenant's database.
    /// </summary>
    public static bool TryUseStoredConnection(
        string? storedConnectionString,
        string expectedDatabaseName,
        out string connectionString)
    {
        connectionString = string.Empty;

        if (string.IsNullOrWhiteSpace(storedConnectionString)
            || string.IsNullOrWhiteSpace(expectedDatabaseName))
            return false;

        SqlConnectionStringBuilder builder;
        try
        {
            builder = new SqlConnectionStringBuilder(storedConnectionString);
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (!string.Equals(builder.InitialCatalog?.Trim(), expectedDatabaseName.Trim(),
                StringComparison.OrdinalIgnoreCase))
            return false;

        builder.MultipleActiveResultSets = true;
        if (builder.ConnectTimeout <= 0)
            builder.ConnectTimeout = 15;

        connectionString = builder.ConnectionString;
        return true;
    }
}
