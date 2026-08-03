using System.Data;
using ONEERP.Platform.API.Models;

namespace ONEERP.Platform.API.Repositories;

public class SubscriptionRow : Subscription
{
    public string TenantCode { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
}

public interface ISubscriptionRepository
{
    Task<IEnumerable<SubscriptionRow>> GetPagedAsync(int pageNumber, int pageSize, string search);
    Task<int> CountAsync(string search);
    Task<int> CountActiveAsync();
    Task<decimal> SumMonthlyAsync();
    Task<Subscription?> GetByIdAsync(int subscriptionId);
    Task<Subscription?> GetActiveByTenantIdAsync(int tenantId);
    Task<IEnumerable<Subscription>> GetByTenantIdAsync(int tenantId);
    Task<int> InsertAsync(Subscription subscription, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Subscription subscription, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int subscriptionId, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<IEnumerable<Subscription>> GetExpiringSoonAsync(int days);
}

public class SubscriptionRepository : BaseRepository, ISubscriptionRepository
{
    private const string ViewSelect = @"
        SELECT s.*,
               t.TenantCode,
               t.TenantName,
               p.PlanCode,
               p.PlanName
        FROM dbo.Subscriptions s
        INNER JOIN dbo.Tenants t ON t.TenantId = s.TenantId
        INNER JOIN dbo.Plans p ON p.PlanId = s.PlanId";

    public SubscriptionRepository(IDbConnectionFactory factory, ISqlHelper sql) : base(factory, sql)
    {
    }

    public async Task<IEnumerable<SubscriptionRow>> GetPagedAsync(int pageNumber, int pageSize, string search)
    {
        using var connection = OpenPlatform();
        var offset = (pageNumber - 1) * pageSize;
        const string sql = $@"
            {ViewSelect}
            WHERE s.IsDeleted = 0
              AND (@search = '' OR t.TenantName LIKE '%' + @search + '%' OR t.TenantCode LIKE '%' + @search + '%' OR p.PlanName LIKE '%' + @search + '%')
            ORDER BY s.SubscriptionId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";
        return await Sql.QueryAsync<SubscriptionRow>(connection, sql, new { search, offset, pageSize });
    }

    public async Task<int> CountAsync(string search)
    {
        using var connection = OpenPlatform();
        const string sql = @"
            SELECT COUNT(1)
            FROM dbo.Subscriptions s
            INNER JOIN dbo.Tenants t ON t.TenantId = s.TenantId
            INNER JOIN dbo.Plans p ON p.PlanId = s.PlanId
            WHERE s.IsDeleted = 0
              AND (@search = '' OR t.TenantName LIKE '%' + @search + '%' OR t.TenantCode LIKE '%' + @search + '%' OR p.PlanName LIKE '%' + @search + '%');";
        return await Sql.ExecuteScalarAsync<int>(connection, sql, new { search });
    }

    public async Task<int> CountActiveAsync()
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteScalarAsync<int>(connection,
            "SELECT COUNT(1) FROM dbo.Subscriptions WHERE IsDeleted = 0 AND Status = 'Active'");
    }

    public async Task<decimal> SumMonthlyAsync()
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteScalarAsync<decimal>(connection,
            @"SELECT COALESCE(SUM(p.MonthlyPrice), 0)
              FROM dbo.Subscriptions s
              INNER JOIN dbo.Plans p ON p.PlanId = s.PlanId
              WHERE s.IsDeleted = 0 AND s.Status = 'Active'");
    }

    public async Task<Subscription?> GetByIdAsync(int subscriptionId)
    {
        using var connection = OpenPlatform();
        return await Sql.QuerySingleOrDefaultAsync<Subscription>(
            connection, "SELECT * FROM dbo.Subscriptions WHERE SubscriptionId = @subscriptionId AND IsDeleted = 0", new { subscriptionId });
    }

    public async Task<Subscription?> GetActiveByTenantIdAsync(int tenantId)
    {
        using var connection = OpenPlatform();
        return await Sql.QuerySingleOrDefaultAsync<Subscription>(
            connection, "SELECT * FROM dbo.Subscriptions WHERE TenantId = @tenantId AND IsDeleted = 0 AND Status = 'Active' ORDER BY SubscriptionId DESC",
            new { tenantId });
    }

    public async Task<IEnumerable<Subscription>> GetByTenantIdAsync(int tenantId)
    {
        using var connection = OpenPlatform();
        return await Sql.QueryAsync<Subscription>(
            connection, "SELECT * FROM dbo.Subscriptions WHERE TenantId = @tenantId AND IsDeleted = 0 ORDER BY SubscriptionId DESC", new { tenantId });
    }

    public async Task<int> InsertAsync(Subscription subscription, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenPlatform();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Subscriptions (TenantId, PlanId, StartDate, EndDate, Amount, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@TenantId, @PlanId, @StartDate, @EndDate, @Amount, @Status, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, subscription, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Subscription subscription, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenPlatform();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Subscriptions
                SET PlanId = @PlanId,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    Amount = @Amount,
                    Status = @Status,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE SubscriptionId = @SubscriptionId;";
            return await Sql.ExecuteAsync(conn, sql, subscription, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int subscriptionId, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenPlatform();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Subscriptions SET IsDeleted = 1, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE SubscriptionId = @subscriptionId;";
            return await Sql.ExecuteAsync(conn, sql, new { subscriptionId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<IEnumerable<Subscription>> GetExpiringSoonAsync(int days)
    {
        using var connection = OpenPlatform();
        const string sql = @"
            SELECT s.*
            FROM dbo.Subscriptions s
            WHERE s.IsDeleted = 0
              AND s.Status = 'Active'
              AND s.EndDate > SYSUTCDATETIME()
              AND s.EndDate <= DATEADD(day, @days, SYSUTCDATETIME());";
        return await Sql.QueryAsync<Subscription>(connection, sql, new { days });
    }
}
