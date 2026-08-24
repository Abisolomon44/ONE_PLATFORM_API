using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IRefreshTokenRepository
{
    Task<long> InsertAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<bool> RevokeAsync(string token);
    Task<bool> RevokeAllForUserAsync(int userId);
    Task<bool> ExtendExpiryAsync(string token, DateTime newExpiry);
}

public class RefreshTokenRepository : TenantRepositoryBase, IRefreshTokenRepository
{
    public RefreshTokenRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<long> InsertAsync(RefreshToken token)
    {
        using var connection = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.RefreshTokens (UserId, Token, ExpiryDate, IsRevoked, CreatedDate)
            VALUES (@UserId, @Token, @ExpiryDate, 0, SYSUTCDATETIME());
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(connection, sql, token);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<RefreshToken>(connection,
            "SELECT * FROM dbo.RefreshTokens WHERE Token = @token", new { token });
    }

    public async Task<bool> RevokeAsync(string token)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteAsync(connection,
            "UPDATE dbo.RefreshTokens SET IsRevoked = 1, RevokedDate = SYSUTCDATETIME() WHERE Token = @token", new { token }) > 0;
    }

    public async Task<bool> RevokeAllForUserAsync(int userId)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteAsync(connection,
            "UPDATE dbo.RefreshTokens SET IsRevoked = 1, RevokedDate = SYSUTCDATETIME() WHERE UserId = @userId AND IsRevoked = 0",
            new { userId }) > 0;
    }

    public async Task<bool> ExtendExpiryAsync(string token, DateTime newExpiry)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteAsync(connection,
            "UPDATE dbo.RefreshTokens SET ExpiryDate = @newExpiry WHERE Token = @token AND IsRevoked = 0",
            new { token, newExpiry }) > 0;
    }
}

public interface IAuditLogRepository
{
    Task<long> InsertAsync(AuditLog log);
    Task<IEnumerable<AuditLog>> GetPagedAsync(int pageNumber, int pageSize);
}

public class AuditLogRepository : TenantRepositoryBase, IAuditLogRepository
{
    public AuditLogRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<long> InsertAsync(AuditLog log)
    {
        using var connection = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.AuditLogs (EntityName, EntityId, [Action], PerformedBy, OldValues, NewValues, IpAddress)
            VALUES (@EntityName, @EntityId, @Action, @PerformedBy, @OldValues, @NewValues, @IpAddress);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(connection, sql, log);
    }

    public async Task<IEnumerable<AuditLog>> GetPagedAsync(int pageNumber, int pageSize)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<AuditLog>(connection,
            "SELECT * FROM dbo.AuditLogs ORDER BY PerformedDate DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { offset, pageSize });
    }
}
