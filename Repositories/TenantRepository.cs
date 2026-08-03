using System.Data;
using ONEERP.Platform.API.Models;

namespace ONEERP.Platform.API.Repositories;

/// <summary>
/// Tenant projection that also carries plan and latest-subscription info so the
/// tenant list is loaded in a single optimized query (no N+1).
/// </summary>
public class TenantRow : Tenant
{
    public string? PlanName { get; set; }
    public string? PlanCurrencyCode { get; set; }
    public string? SubscriptionStatus { get; set; }
    public DateTime? SubscriptionStart { get; set; }
    public DateTime? SubscriptionEnd { get; set; }
    public bool DatabaseProvisioned { get; set; }
}

public interface ITenantRepository
{
    Task<IEnumerable<TenantRow>> GetPagedAsync(int pageNumber, int pageSize, string search);
    Task<int> CountAsync(string search);
    Task<int> CountAllAsync();
    Task<int> CountByStatusAsync(string status);
    Task<TenantRow?> GetByIdAsync(int tenantId);
    Task<Tenant?> GetByCodeAsync(string tenantCode);
    Task<Tenant?> GetByDatabaseNameAsync(string databaseName);
    Task<bool> CodeExistsAsync(string tenantCode);
    Task<bool> DatabaseNameExistsAsync(string databaseName);
    Task<bool> AdminUsernameExistsAsync(string adminUsername);
    Task<int> InsertAsync(Tenant tenant, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Tenant tenant, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdatePlanAsync(int tenantId, int? planId, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int tenantId, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<IEnumerable<Tenant>> GetActiveAsync();
}

public class TenantRepository : BaseRepository, ITenantRepository
{
    private const string ViewSelect = @"
        SELECT t.*,
               p.PlanName,
               p.CurrencyCode AS PlanCurrencyCode,
               s.Status AS SubscriptionStatus,
               s.StartDate AS SubscriptionStart,
               s.EndDate AS SubscriptionEnd,
               CASE WHEN EXISTS (SELECT 1 FROM dbo.TenantConnections tc WHERE tc.TenantId = t.TenantId AND tc.IsActive = 1)
                    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS DatabaseProvisioned
        FROM dbo.Tenants t
        LEFT JOIN dbo.Plans p ON p.PlanId = t.PlanId
        OUTER APPLY (SELECT TOP 1 Status, StartDate, EndDate FROM dbo.Subscriptions
                     WHERE TenantId = t.TenantId AND IsDeleted = 0 ORDER BY SubscriptionId DESC) s";

    public TenantRepository(IDbConnectionFactory factory, ISqlHelper sql) : base(factory, sql)
    {
    }

    public async Task<IEnumerable<TenantRow>> GetPagedAsync(int pageNumber, int pageSize, string search)
    {
        using var connection = OpenPlatform();
        var offset = (pageNumber - 1) * pageSize;
        const string sql = $@"
            {ViewSelect}
            WHERE t.IsDeleted = 0
              AND (@search = '' OR t.TenantName LIKE '%' + @search + '%' OR t.TenantCode LIKE '%' + @search + '%' OR t.DatabaseName LIKE '%' + @search + '%')
            ORDER BY t.CreatedDate DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";
        return await Sql.QueryAsync<TenantRow>(connection, sql, new { search, offset, pageSize });
    }

    public async Task<int> CountAsync(string search)
    {
        using var connection = OpenPlatform();
        const string sql = @"
            SELECT COUNT(1) FROM dbo.Tenants t
            WHERE t.IsDeleted = 0
              AND (@search = '' OR t.TenantName LIKE '%' + @search + '%' OR t.TenantCode LIKE '%' + @search + '%' OR t.DatabaseName LIKE '%' + @search + '%');";
        return await Sql.ExecuteScalarAsync<int>(connection, sql, new { search });
    }

    public async Task<int> CountAllAsync()
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteScalarAsync<int>(connection, "SELECT COUNT(1) FROM dbo.Tenants WHERE IsDeleted = 0");
    }

    public async Task<int> CountByStatusAsync(string status)
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteScalarAsync<int>(connection,
            "SELECT COUNT(1) FROM dbo.Tenants WHERE IsDeleted = 0 AND Status = @status", new { status });
    }

    public async Task<TenantRow?> GetByIdAsync(int tenantId)
    {
        using var connection = OpenPlatform();
        return await Sql.QuerySingleOrDefaultAsync<TenantRow>(connection, $"{ViewSelect} WHERE t.TenantId = @tenantId", new { tenantId });
    }

    public async Task<Tenant?> GetByCodeAsync(string tenantCode)
    {
        using var connection = OpenPlatform();
        return await Sql.QuerySingleOrDefaultAsync<Tenant>(
            connection, "SELECT * FROM dbo.Tenants WHERE TenantCode = @tenantCode AND IsDeleted = 0", new { tenantCode });
    }

    public async Task<Tenant?> GetByDatabaseNameAsync(string databaseName)
    {
        using var connection = OpenPlatform();
        return await Sql.QuerySingleOrDefaultAsync<Tenant>(
            connection, "SELECT * FROM dbo.Tenants WHERE DatabaseName = @databaseName AND IsDeleted = 0", new { databaseName });
    }

    public async Task<bool> CodeExistsAsync(string tenantCode)
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteScalarAsync<int>(connection,
            "SELECT COUNT(1) FROM dbo.Tenants WHERE TenantCode = @tenantCode AND IsDeleted = 0", new { tenantCode }) > 0;
    }

    public async Task<bool> DatabaseNameExistsAsync(string databaseName)
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteScalarAsync<int>(connection,
            "SELECT COUNT(1) FROM dbo.Tenants WHERE DatabaseName = @databaseName AND IsDeleted = 0", new { databaseName }) > 0;
    }

    public async Task<bool> AdminUsernameExistsAsync(string adminUsername)
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteScalarAsync<int>(connection,
            "SELECT COUNT(1) FROM dbo.Tenants WHERE AdminUsername = @adminUsername AND IsDeleted = 0", new { adminUsername }) > 0;
    }

    public async Task<int> InsertAsync(Tenant tenant, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenPlatform();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Tenants (TenantCode, TenantName, CompanyName, DatabaseName, PlanId, ContactEmail, AdminUsername, AdminPassword, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@TenantCode, @TenantName, @CompanyName, @DatabaseName, @PlanId, @ContactEmail, @AdminUsername, @AdminPassword, @Status, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, tenant, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Tenant tenant, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenPlatform();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Tenants
                SET TenantName = @TenantName,
                    CompanyName = @CompanyName,
                    ContactEmail = @ContactEmail,
                    Status = @Status,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE TenantId = @TenantId;";
            return await Sql.ExecuteAsync(conn, sql, tenant, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdatePlanAsync(int tenantId, int? planId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenPlatform();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Tenants SET PlanId = @planId, ModifiedDate = SYSUTCDATETIME() WHERE TenantId = @tenantId;";
            return await Sql.ExecuteAsync(conn, sql, new { tenantId, planId }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int tenantId, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenPlatform();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Tenants SET IsDeleted = 1, Status = 'Suspended', ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE TenantId = @tenantId;";
            return await Sql.ExecuteAsync(conn, sql, new { tenantId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<IEnumerable<Tenant>> GetActiveAsync()
    {
        using var connection = OpenPlatform();
        return await Sql.QueryAsync<Tenant>(connection, "SELECT * FROM dbo.Tenants WHERE IsDeleted = 0 AND Status = 'Active' ORDER BY TenantName");
    }
}
