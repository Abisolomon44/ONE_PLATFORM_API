using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync(bool includeInactive = false);
    Task<Dictionary<int, string>> GetAllNamesAsync();
    Task<Role?> GetByIdAsync(int roleId);
    Task<Role?> GetByCodeAsync(string code);
    Task<int> InsertAsync(Role role, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Role role, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int roleId, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<IEnumerable<Role>> GetRolesForUserAsync(int userId);
    Task<IEnumerable<int>> GetRoleIdsForUserAsync(int userId);
    Task<IEnumerable<string>> GetPermissionsForRoleAsync(int roleId);
    Task<IEnumerable<string>> GetPermissionsForUserAsync(int userId);
    Task<IEnumerable<RolePermission>> GetAllPermissionsAsync();
    Task<IEnumerable<UserRoleAssignment>> GetRoleAssignmentsForUsersAsync(IEnumerable<int> userIds);
    Task<bool> SetPermissionsAsync(int roleId, IEnumerable<string> permissionCodes, string? createdBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class UserRoleAssignment
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

public class RoleRepository : TenantRepositoryBase, IRoleRepository
{
    public RoleRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<Role>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.Roles WHERE IsDeleted = 0 ORDER BY RoleId"
            : "SELECT * FROM dbo.Roles WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY RoleId";
        return await Sql.QueryAsync<Role>(connection, sql);
    }

    public async Task<Dictionary<int, string>> GetAllNamesAsync()
    {
        using var connection = OpenTenant();
        var roles = await Sql.QueryAsync<Role>(connection,
            "SELECT RoleId, [Name] FROM dbo.Roles WHERE IsDeleted = 0");
        return roles.ToDictionary(r => r.RoleId, r => r.Name);
    }

    public async Task<Role?> GetByIdAsync(int roleId)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Role>(connection,
            "SELECT * FROM dbo.Roles WHERE RoleId = @roleId AND IsDeleted = 0", new { roleId });
    }

    public async Task<Role?> GetByCodeAsync(string code)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Role>(connection,
            "SELECT * FROM dbo.Roles WHERE Code = @code AND IsDeleted = 0", new { code });
    }

    public async Task<int> InsertAsync(Role role, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Roles ([Name], Code, [Description], IsSystem, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@Name, @Code, @Description, @IsSystem, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, role, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Role role, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Roles
                SET [Name] = @Name,
                    [Description] = @Description,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE RoleId = @RoleId;";
            return await Sql.ExecuteAsync(conn, sql, role, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int roleId, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Roles SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE RoleId = @roleId;";
            return await Sql.ExecuteAsync(conn, sql, new { roleId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<IEnumerable<Role>> GetRolesForUserAsync(int userId)
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<Role>(connection, @"
            SELECT r.*
            FROM dbo.Roles r
            INNER JOIN dbo.UserRoles ur ON ur.RoleId = r.RoleId
            WHERE ur.UserId = @userId AND r.IsDeleted = 0 AND r.IsActive = 1",
            new { userId });
    }

    public async Task<IEnumerable<int>> GetRoleIdsForUserAsync(int userId)
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<int>(connection,
            "SELECT RoleId FROM dbo.UserRoles WHERE UserId = @userId", new { userId });
    }

    public async Task<IEnumerable<string>> GetPermissionsForRoleAsync(int roleId)
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<string>(connection,
            "SELECT PermissionCode FROM dbo.RolePermissionsLegacy WHERE RoleId = @roleId ORDER BY PermissionCode", new { roleId });
    }

    public async Task<IEnumerable<string>> GetPermissionsForUserAsync(int userId)
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<string>(connection, @"
            SELECT DISTINCT rp.PermissionCode
            FROM dbo.RolePermissionsLegacy rp
            INNER JOIN dbo.UserRoles ur ON ur.RoleId = rp.RoleId
            WHERE ur.UserId = @userId
            ORDER BY rp.PermissionCode",
            new { userId });
    }

    public async Task<IEnumerable<RolePermission>> GetAllPermissionsAsync()
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<RolePermission>(connection,
            "SELECT * FROM dbo.RolePermissionsLegacy ORDER BY RoleId, PermissionCode");
    }

    public async Task<IEnumerable<UserRoleAssignment>> GetRoleAssignmentsForUsersAsync(IEnumerable<int> userIds)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0)
            return Enumerable.Empty<UserRoleAssignment>();

        using var connection = OpenTenant();
        return await Sql.QueryAsync<UserRoleAssignment>(connection, @"
            SELECT ur.UserId, r.RoleId, r.[Name] AS RoleName
            FROM dbo.UserRoles ur
            INNER JOIN dbo.Roles r ON r.RoleId = ur.RoleId
            WHERE ur.UserId IN @ids",
            new { ids });
    }

    public async Task<bool> SetPermissionsAsync(int roleId, IEnumerable<string> permissionCodes, string? createdBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            await Sql.ExecuteAsync(conn, "DELETE FROM dbo.RolePermissionsLegacy WHERE RoleId = @roleId", new { roleId }, transaction);

            foreach (var permission in permissionCodes.Distinct())
            {
                await Sql.ExecuteAsync(conn, @"
                    INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode, CreatedBy)
                    VALUES (@roleId, @permission, @createdBy);",
                    new { roleId, permission, createdBy }, transaction);
            }

            return true;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
