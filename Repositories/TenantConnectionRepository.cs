using System.Data;
using ONEERP.Platform.API.Models;

namespace ONEERP.Platform.API.Repositories;

public interface ITenantConnectionRepository
{
    Task<IEnumerable<TenantConnection>> GetByTenantIdAsync(int tenantId);
    Task<TenantConnection?> GetActiveByTenantIdAsync(int tenantId);
    Task<int> InsertAsync(TenantConnection connection, IDbConnection? connectionRef = null, IDbTransaction? transaction = null);
    Task<bool> DeactivateAsync(int connectionId);
}

public class TenantConnectionRepository : BaseRepository, ITenantConnectionRepository
{
    public TenantConnectionRepository(IDbConnectionFactory factory, ISqlHelper sql) : base(factory, sql)
    {
    }

    public async Task<IEnumerable<TenantConnection>> GetByTenantIdAsync(int tenantId)
    {
        using var connection = OpenPlatform();
        return await Sql.QueryAsync<TenantConnection>(
            connection, "SELECT * FROM dbo.TenantConnections WHERE TenantId = @tenantId ORDER BY ConnectionId DESC", new { tenantId });
    }

    public async Task<TenantConnection?> GetActiveByTenantIdAsync(int tenantId)
    {
        using var connection = OpenPlatform();
        return await Sql.QuerySingleOrDefaultAsync<TenantConnection>(
            connection, "SELECT * FROM dbo.TenantConnections WHERE TenantId = @tenantId AND IsActive = 1 ORDER BY ConnectionId DESC", new { tenantId });
    }

    public async Task<int> InsertAsync(TenantConnection entity, IDbConnection? connectionRef = null, IDbTransaction? transaction = null)
    {
        var conn = connectionRef ?? OpenPlatform();
        var own = connectionRef is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.TenantConnections (TenantId, ServerName, DatabaseName, ConnectionString, IsActive, CreatedBy, CreatedDate)
                VALUES (@TenantId, @ServerName, @DatabaseName, @ConnectionString, @IsActive, @CreatedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> DeactivateAsync(int connectionId)
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteAsync(connection,
            "UPDATE dbo.TenantConnections SET IsActive = 0 WHERE ConnectionId = @connectionId", new { connectionId }) > 0;
    }
}
