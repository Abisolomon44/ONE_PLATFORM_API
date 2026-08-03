using System.Data;
using ONEERP.Platform.API.Models;

namespace ONEERP.Platform.API.Repositories;

public interface IPlatformUserRepository
{
    Task<PlatformUser?> GetByUsernameAsync(string username);
    Task<PlatformUser?> GetByIdAsync(int platformUserId);
    Task<int> InsertAsync(PlatformUser user, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateLastLoginAsync(int platformUserId);
}

public class PlatformUserRepository : BaseRepository, IPlatformUserRepository
{
    public PlatformUserRepository(IDbConnectionFactory factory, ISqlHelper sql) : base(factory, sql)
    {
    }

    public async Task<PlatformUser?> GetByUsernameAsync(string username)
    {
        using var connection = OpenPlatform();
        return await Sql.QuerySingleOrDefaultAsync<PlatformUser>(
            connection, "SELECT * FROM dbo.PlatformUsers WHERE Username = @username AND IsDeleted = 0", new { username });
    }

    public async Task<PlatformUser?> GetByIdAsync(int platformUserId)
    {
        using var connection = OpenPlatform();
        return await Sql.QuerySingleOrDefaultAsync<PlatformUser>(
            connection, "SELECT * FROM dbo.PlatformUsers WHERE PlatformUserId = @platformUserId AND IsDeleted = 0", new { platformUserId });
    }

    public async Task<int> InsertAsync(PlatformUser user, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenPlatform();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.PlatformUsers (Username, Email, FullName, PasswordHash, Role, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@Username, @Email, @FullName, @PasswordHash, @Role, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, user, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateLastLoginAsync(int platformUserId)
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteAsync(connection,
            "UPDATE dbo.PlatformUsers SET LastLoginDate = SYSUTCDATETIME() WHERE PlatformUserId = @platformUserId", new { platformUserId }) > 0;
    }
}
