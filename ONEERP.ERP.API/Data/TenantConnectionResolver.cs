using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Data;

/// <summary>
/// Information about a tenant resolved from the platform database.
/// </summary>
public class TenantSession
{
    public int TenantId { get; set; }
    public string TenantCode { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string TenantStatus { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public DateTime? SubscriptionEnd { get; set; }
    public int? PlanId { get; set; }
    public string? PlanCode { get; set; }
    public string? PlanName { get; set; }
}

/// <summary>
/// Resolves tenant connection strings (and validates subscription health) by
/// querying the ONE ERP platform database. Connection strings are cached in
/// memory so authorized requests stay fast.
/// </summary>
public interface ITenantConnectionResolver
{
    Task<string?> GetConnectionStringAsync(string tenantCode);
    Task<TenantSession> ResolveTenantAsync(string tenantCode);
    Task<string?> ResolveTenantCodeByUsernameAsync(string username);
}

public class TenantConnectionResolver : ITenantConnectionResolver
{
    private static readonly TimeSpan ConnectionCacheDuration = TimeSpan.FromHours(1);

    private readonly IPlatformDbConnectionFactory _factory;
    private readonly ISqlHelper _sql;
    private readonly IMemoryCache _cache;

    public TenantConnectionResolver(
        IPlatformDbConnectionFactory factory,
        ISqlHelper sql,
        IMemoryCache cache)
    {
        _factory = factory;
        _sql = sql;
        _cache = cache;
    }

    public async Task<string?> GetConnectionStringAsync(string tenantCode)
    {
        if (_cache.TryGetValue<string>(CacheKey(tenantCode), out var cached))
            return cached;

        using var connection = _factory.CreatePlatformConnection();
        var connectionString = await _sql.ExecuteScalarAsync<string>(connection, @"
            SELECT TOP 1 tc.ConnectionString
            FROM dbo.Tenants t
            INNER JOIN dbo.TenantConnections tc ON tc.TenantId = t.TenantId AND tc.IsActive = 1
            WHERE t.TenantCode = @tenantCode AND t.IsDeleted = 0",
            new { tenantCode });

        if (!string.IsNullOrWhiteSpace(connectionString))
            _cache.Set(CacheKey(tenantCode), connectionString, ConnectionCacheDuration);

        return connectionString;
    }

    public async Task<TenantSession> ResolveTenantAsync(string tenantCode)
    {
        using var connection = _factory.CreatePlatformConnection();
        var session = await _sql.QuerySingleOrDefaultAsync<TenantSession>(connection, @"
            SELECT t.TenantId,
                   t.TenantCode,
                   t.TenantName,
                   t.DatabaseName,
                   tc.ConnectionString,
                   t.Status AS TenantStatus,
                   s.Status AS SubscriptionStatus,
                   s.EndDate AS SubscriptionEnd,
                   s.PlanId,
                   p.PlanCode,
                   p.PlanName
            FROM dbo.Tenants t
            LEFT JOIN dbo.TenantConnections tc ON tc.TenantId = t.TenantId AND tc.IsActive = 1
            OUTER APPLY (SELECT TOP 1 Status, EndDate, PlanId FROM dbo.Subscriptions
                         WHERE TenantId = t.TenantId AND IsDeleted = 0 ORDER BY SubscriptionId DESC) s
            LEFT JOIN dbo.Plans p ON p.PlanId = s.PlanId
            WHERE t.TenantCode = @tenantCode AND t.IsDeleted = 0",
            new { tenantCode });

        if (session is null)
            throw new NotFoundException($"Tenant '{tenantCode}' was not found.");

        if (session.TenantStatus != TenantStatus.Active)
            throw new DomainException($"Tenant '{tenantCode}' is not active.");

        if (string.IsNullOrWhiteSpace(session.ConnectionString))
            throw new DomainException($"No active database connection is registered for tenant '{tenantCode}'.");

        return session;
    }

    /// <summary>
    /// Finds the tenant code that owns the given (globally unique) tenant admin
    /// username. Used to support username-only ERP login across tenant databases.
    /// </summary>
    public async Task<string?> ResolveTenantCodeByUsernameAsync(string username)
    {
        using var connection = _factory.CreatePlatformConnection();
        return await _sql.ExecuteScalarAsync<string>(connection, @"
            SELECT TOP 1 TenantCode FROM (
                SELECT TenantCode FROM dbo.TenantUserMaps WHERE Username = @username
                UNION ALL
                SELECT TenantCode FROM dbo.Tenants WHERE AdminUsername = @username AND IsDeleted = 0
            ) t",
            new { username });
    }

    private static string CacheKey(string tenantCode) => $"tenant-conn:{tenantCode.ToLowerInvariant()}";
}

/// <summary>
/// Mutable, request-scoped holder of the current tenant context.
/// </summary>
public class TenantAccessor
{
    public string? TenantCode { get; set; }
    public string? ConnectionString { get; set; }

    public IDbConnection OpenTenantConnection()
    {
        var connectionString = ConnectionString
            ?? throw new DomainException("Tenant context was not resolved for this request.");
        return new SqlConnection(connectionString);
    }
}
