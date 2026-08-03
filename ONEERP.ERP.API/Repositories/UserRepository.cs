using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> UsernameInUseAsync(string username);
    Task<bool> EmailInUseAsync(string email);
    Task<User?> GetByIdAsync(int userId);
    Task<IEnumerable<User>> GetPagedAsync(int pageNumber, int pageSize, string search);
    Task<int> CountAsync(string search);
    Task<int> CountByStatusAsync(string status);
    Task<int> InsertAsync(User user, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(User user, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int userId, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateLastLoginAsync(int userId);
    Task<bool> UpdatePasswordAsync(int userId, string passwordHash, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> AssignRolesAsync(int userId, IEnumerable<int> roleIds, string? createdBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> RemoveRolesAsync(int userId, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class UserRepository : TenantRepositoryBase, IUserRepository
{
    public UserRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<User>(connection,
            "SELECT * FROM dbo.Users WHERE Username = @username AND IsDeleted = 0", new { username });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<User>(connection,
            "SELECT * FROM dbo.Users WHERE Email = @email AND IsDeleted = 0", new { email });
    }

    public async Task<bool> UsernameInUseAsync(string username)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @username) THEN 1 ELSE 0 END", new { username });
    }

    public async Task<bool> EmailInUseAsync(string email)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @email) THEN 1 ELSE 0 END", new { email });
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<User>(connection,
            "SELECT * FROM dbo.Users WHERE UserId = @userId AND IsDeleted = 0", new { userId });
    }

    public async Task<IEnumerable<User>> GetPagedAsync(int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<User>(connection, @"
            SELECT * FROM dbo.Users
            WHERE IsDeleted = 0
              AND (@search = '' OR Username LIKE '%' + @search + '%' OR FullName LIKE '%' + @search + '%' OR Email LIKE '%' + @search + '%')
            ORDER BY UserId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { search, offset, pageSize });
    }

    public async Task<int> CountAsync(string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Users
            WHERE IsDeleted = 0
              AND (@search = '' OR Username LIKE '%' + @search + '%' OR FullName LIKE '%' + @search + '%' OR Email LIKE '%' + @search + '%')",
            new { search });
    }

    public async Task<int> CountByStatusAsync(string status)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection,
            "SELECT COUNT(1) FROM dbo.Users WHERE IsDeleted = 0 AND Status = @status", new { status });
    }

    public async Task<int> InsertAsync(User user, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Users (CompanyId, Username, Email, Mobile, FullName, PasswordHash, Status, IsSuperAdmin, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@CompanyId, @Username, @Email, @Mobile, @FullName, @PasswordHash, @Status, @IsSuperAdmin, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, user, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(User user, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Users
                SET FullName = @FullName,
                    Email = @Email,
                    Mobile = @Mobile,
                    Status = @Status,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE UserId = @UserId;";
            return await Sql.ExecuteAsync(conn, sql, user, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int userId, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Users SET IsDeleted = 1, Status = 'Inactive', ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE UserId = @userId;";
            return await Sql.ExecuteAsync(conn, sql, new { userId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateLastLoginAsync(int userId)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteAsync(connection,
            "UPDATE dbo.Users SET LastLoginDate = SYSUTCDATETIME() WHERE UserId = @userId", new { userId }) > 0;
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string passwordHash, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Users SET PasswordHash = @passwordHash, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE UserId = @userId;";
            return await Sql.ExecuteAsync(conn, sql, new { userId, passwordHash, modifiedBy = "user" }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> AssignRolesAsync(int userId, IEnumerable<int> roleIds, string? createdBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            var list = roleIds.Distinct().ToList();
            if (list.Count == 0)
                return true;

            foreach (var roleId in list)
            {
                await Sql.ExecuteAsync(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = @userId AND RoleId = @roleId)
                        INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedBy) VALUES (@userId, @roleId, @createdBy);",
                    new { userId, roleId, createdBy }, transaction);
            }

            return true;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> RemoveRolesAsync(int userId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            return await Sql.ExecuteAsync(conn, "DELETE FROM dbo.UserRoles WHERE UserId = @userId", new { userId }, transaction) >= 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
