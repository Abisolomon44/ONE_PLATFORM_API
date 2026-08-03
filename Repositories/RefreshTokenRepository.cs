using System.Data;
using ONEERP.Platform.API.Models;

namespace ONEERP.Platform.API.Repositories;

public interface IRefreshTokenRepository
{
    Task<int> InsertAsync(RefreshToken token, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<bool> RevokeAsync(string token);
    Task<bool> RevokeAllForUserAsync(int platformUserId);
}

public class RefreshTokenRepository : BaseRepository, IRefreshTokenRepository
{
    public RefreshTokenRepository(IDbConnectionFactory factory, ISqlHelper sql) : base(factory, sql)
    {
    }

    public async Task<int> InsertAsync(RefreshToken token, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenPlatform();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.RefreshTokens (PlatformUserId, Token, ExpiryDate, IsRevoked, CreatedDate)
                VALUES (@PlatformUserId, @Token, @ExpiryDate, 0, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, token, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        using var connection = OpenPlatform();
        return await Sql.QuerySingleOrDefaultAsync<RefreshToken>(
            connection, "SELECT * FROM dbo.RefreshTokens WHERE Token = @token", new { token });
    }

    public async Task<bool> RevokeAsync(string token)
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteAsync(connection,
            "UPDATE dbo.RefreshTokens SET IsRevoked = 1, RevokedDate = SYSUTCDATETIME() WHERE Token = @token", new { token }) > 0;
    }

    public async Task<bool> RevokeAllForUserAsync(int platformUserId)
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteAsync(connection,
            "UPDATE dbo.RefreshTokens SET IsRevoked = 1, RevokedDate = SYSUTCDATETIME() WHERE PlatformUserId = @platformUserId AND IsRevoked = 0",
            new { platformUserId }) > 0;
    }
}
