using ONEERP.Platform.API.Models;

namespace ONEERP.Platform.API.Repositories;

public interface IAuditLogRepository
{
    Task<long> InsertAsync(AuditLog log);
    Task<IEnumerable<AuditLog>> GetPagedAsync(int pageNumber, int pageSize);
}

public class AuditLogRepository : BaseRepository, IAuditLogRepository
{
    public AuditLogRepository(IDbConnectionFactory factory, ISqlHelper sql) : base(factory, sql)
    {
    }

    public async Task<long> InsertAsync(AuditLog log)
    {
        using var connection = OpenPlatform();
        const string sql = @"
            INSERT INTO dbo.AuditLogs (TenantId, EntityName, EntityId, [Action], PerformedBy, PerformedDate, OldValues, NewValues, IpAddress)
            VALUES (@TenantId, @EntityName, @EntityId, @Action, @PerformedBy, SYSUTCDATETIME(), @OldValues, @NewValues, @IpAddress);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(connection, sql, log);
    }

    public async Task<IEnumerable<AuditLog>> GetPagedAsync(int pageNumber, int pageSize)
    {
        using var connection = OpenPlatform();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<AuditLog>(connection,
            "SELECT * FROM dbo.AuditLogs ORDER BY PerformedDate DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { offset, pageSize });
    }
}
