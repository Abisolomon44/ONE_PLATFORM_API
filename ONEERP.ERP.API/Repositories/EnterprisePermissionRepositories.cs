using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

/* ---------------------------------------------------------------------------
   Workspace Repository
   --------------------------------------------------------------------------- */
public interface IWorkspaceRepository
{
    Task<IEnumerable<Workspace>> GetAllAsync(bool includeInactive = false);
    Task<Workspace?> GetByIdAsync(int id);
    Task<int> InsertAsync(Workspace entity);
    Task<bool> UpdateAsync(Workspace entity);
    Task<bool> DeleteAsync(int id);
}

public class WorkspaceRepository : TenantRepositoryBase, IWorkspaceRepository
{
    public WorkspaceRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<Workspace>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.Workspaces ORDER BY SortOrder, WorkspaceCode"
            : "SELECT * FROM dbo.Workspaces WHERE IsActive = 1 ORDER BY SortOrder, WorkspaceCode";
        return await Sql.QueryAsync<Workspace>(conn, sql);
    }

    public async Task<Workspace?> GetByIdAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Workspace>(conn,
            "SELECT * FROM dbo.Workspaces WHERE Id = @id", new { id });
    }

    public async Task<int> InsertAsync(Workspace entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.Workspaces (WorkspaceCode, WorkspaceName, Icon, Route, SortOrder, IsActive, CreatedBy)
            VALUES (@WorkspaceCode, @WorkspaceName, @Icon, @Route, @SortOrder, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity);
    }

    public async Task<bool> UpdateAsync(Workspace entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.Workspaces
            SET WorkspaceCode = @WorkspaceCode, WorkspaceName = @WorkspaceName, Icon = @Icon,
                Route = @Route, SortOrder = @SortOrder, IsActive = @IsActive
            WHERE Id = @Id;";
        return await Sql.ExecuteAsync(conn, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = OpenTenant();
        await Sql.ExecuteAsync(conn, @"
            DELETE FROM dbo.Fields WHERE ScreenId IN (SELECT Id FROM dbo.Screens WHERE SubModuleId IN (SELECT Id FROM dbo.SubModules WHERE ModuleId IN (SELECT Id FROM dbo.Modules WHERE DomainId IN (SELECT Id FROM dbo.Domains WHERE WorkspaceId = @id))));
            DELETE FROM dbo.Screens WHERE SubModuleId IN (SELECT Id FROM dbo.SubModules WHERE ModuleId IN (SELECT Id FROM dbo.Modules WHERE DomainId IN (SELECT Id FROM dbo.Domains WHERE WorkspaceId = @id)));
            DELETE FROM dbo.SubModules WHERE ModuleId IN (SELECT Id FROM dbo.Modules WHERE DomainId IN (SELECT Id FROM dbo.Domains WHERE WorkspaceId = @id));
            DELETE FROM dbo.Modules WHERE DomainId IN (SELECT Id FROM dbo.Domains WHERE WorkspaceId = @id);
            DELETE FROM dbo.Domains WHERE WorkspaceId = @id;
            DELETE FROM dbo.Workspaces WHERE Id = @id;", new { id });
        return true;
    }
}

/* ---------------------------------------------------------------------------
   Domain Repository
   --------------------------------------------------------------------------- */
public interface IDomainRepository
{
    Task<IEnumerable<Domain>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<Domain>> GetByWorkspaceAsync(int workspaceId);
    Task<Domain?> GetByIdAsync(int id);
    Task<int> InsertAsync(Domain entity);
    Task<bool> UpdateAsync(Domain entity);
    Task<bool> DeleteAsync(int id);
}

public class DomainRepository : TenantRepositoryBase, IDomainRepository
{
    public DomainRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<Domain>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.Domains ORDER BY SortOrder, DomainCode"
            : "SELECT * FROM dbo.Domains WHERE IsActive = 1 ORDER BY SortOrder, DomainCode";
        return await Sql.QueryAsync<Domain>(conn, sql);
    }

    public async Task<IEnumerable<Domain>> GetByWorkspaceAsync(int workspaceId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<Domain>(conn,
            "SELECT * FROM dbo.Domains WHERE WorkspaceId = @workspaceId AND IsActive = 1 ORDER BY SortOrder",
            new { workspaceId });
    }

    public async Task<Domain?> GetByIdAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Domain>(conn,
            "SELECT * FROM dbo.Domains WHERE Id = @id", new { id });
    }

    public async Task<int> InsertAsync(Domain entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
            VALUES (@WorkspaceId, @DomainCode, @DomainName, @Icon, @SortOrder, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity);
    }

    public async Task<bool> UpdateAsync(Domain entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.Domains
            SET DomainCode = @DomainCode, DomainName = @DomainName, Icon = @Icon,
                SortOrder = @SortOrder, IsActive = @IsActive
            WHERE Id = @Id;";
        return await Sql.ExecuteAsync(conn, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = OpenTenant();
        await Sql.ExecuteAsync(conn, @"
            DELETE FROM dbo.Fields WHERE ScreenId IN (SELECT Id FROM dbo.Screens WHERE SubModuleId IN (SELECT Id FROM dbo.SubModules WHERE ModuleId IN (SELECT Id FROM dbo.Modules WHERE DomainId = @id)));
            DELETE FROM dbo.Screens WHERE SubModuleId IN (SELECT Id FROM dbo.SubModules WHERE ModuleId IN (SELECT Id FROM dbo.Modules WHERE DomainId = @id));
            DELETE FROM dbo.SubModules WHERE ModuleId IN (SELECT Id FROM dbo.Modules WHERE DomainId = @id);
            DELETE FROM dbo.Modules WHERE DomainId = @id;
            DELETE FROM dbo.Domains WHERE Id = @id;", new { id });
        return true;
    }
}

/* ---------------------------------------------------------------------------
   Module Repository
   --------------------------------------------------------------------------- */
public interface IModuleRepository
{
    Task<IEnumerable<Module>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<Module>> GetByDomainAsync(int domainId);
    Task<Module?> GetByIdAsync(int id);
    Task<int> InsertAsync(Module entity);
    Task<bool> UpdateAsync(Module entity);
    Task<bool> DeleteAsync(int id);
}

public class ModuleRepository : TenantRepositoryBase, IModuleRepository
{
    public ModuleRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<Module>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.Modules ORDER BY SortOrder, ModuleCode"
            : "SELECT * FROM dbo.Modules WHERE IsActive = 1 ORDER BY SortOrder, ModuleCode";
        return await Sql.QueryAsync<Module>(conn, sql);
    }

    public async Task<IEnumerable<Module>> GetByDomainAsync(int domainId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<Module>(conn,
            "SELECT * FROM dbo.Modules WHERE DomainId = @domainId AND IsActive = 1 ORDER BY SortOrder",
            new { domainId });
    }

    public async Task<Module?> GetByIdAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Module>(conn,
            "SELECT * FROM dbo.Modules WHERE Id = @id", new { id });
    }

    public async Task<int> InsertAsync(Module entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
            VALUES (@DomainId, @ModuleCode, @ModuleName, @Icon, @RouteUrl, @SortOrder, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity);
    }

    public async Task<bool> UpdateAsync(Module entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.Modules
            SET ModuleCode = @ModuleCode, ModuleName = @ModuleName, Icon = @Icon,
                RouteUrl = @RouteUrl, SortOrder = @SortOrder, IsActive = @IsActive
            WHERE Id = @Id;";
        return await Sql.ExecuteAsync(conn, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = OpenTenant();
        await Sql.ExecuteAsync(conn, @"
            DELETE FROM dbo.Fields WHERE ScreenId IN (SELECT Id FROM dbo.Screens WHERE SubModuleId IN (SELECT Id FROM dbo.SubModules WHERE ModuleId = @id));
            DELETE FROM dbo.Screens WHERE SubModuleId IN (SELECT Id FROM dbo.SubModules WHERE ModuleId = @id);
            DELETE FROM dbo.SubModules WHERE ModuleId = @id;
            DELETE FROM dbo.Modules WHERE Id = @id;", new { id });
        return true;
    }
}

/* ---------------------------------------------------------------------------
   SubModule Repository
   --------------------------------------------------------------------------- */
public interface ISubModuleRepository
{
    Task<IEnumerable<SubModule>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<SubModule>> GetByModuleAsync(int moduleId);
    Task<SubModule?> GetByIdAsync(int id);
    Task<int> InsertAsync(SubModule entity);
    Task<bool> UpdateAsync(SubModule entity);
    Task<bool> DeleteAsync(int id);
}

public class SubModuleRepository : TenantRepositoryBase, ISubModuleRepository
{
    public SubModuleRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<SubModule>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.SubModules ORDER BY SortOrder, SubModuleCode"
            : "SELECT * FROM dbo.SubModules WHERE IsActive = 1 ORDER BY SortOrder, SubModuleCode";
        return await Sql.QueryAsync<SubModule>(conn, sql);
    }

    public async Task<IEnumerable<SubModule>> GetByModuleAsync(int moduleId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<SubModule>(conn,
            "SELECT * FROM dbo.SubModules WHERE ModuleId = @moduleId AND IsActive = 1 ORDER BY SortOrder",
            new { moduleId });
    }

    public async Task<SubModule?> GetByIdAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<SubModule>(conn,
            "SELECT * FROM dbo.SubModules WHERE Id = @id", new { id });
    }

    public async Task<int> InsertAsync(SubModule entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
            VALUES (@ModuleId, @SubModuleCode, @SubModuleName, @Icon, @RouteUrl, @SortOrder, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity);
    }

    public async Task<bool> UpdateAsync(SubModule entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.SubModules
            SET SubModuleCode = @SubModuleCode, SubModuleName = @SubModuleName, Icon = @Icon,
                RouteUrl = @RouteUrl, SortOrder = @SortOrder, IsActive = @IsActive,
                ModifiedBy = @ModifiedBy, ModifiedDate = SYSUTCDATETIME()
            WHERE Id = @Id;";
        return await Sql.ExecuteAsync(conn, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = OpenTenant();
        await Sql.ExecuteAsync(conn, @"
            DELETE FROM dbo.Fields WHERE ScreenId IN (SELECT Id FROM dbo.Screens WHERE SubModuleId = @id);
            DELETE FROM dbo.Screens WHERE SubModuleId = @id;
            DELETE FROM dbo.SubModules WHERE Id = @id;", new { id });
        return true;
    }
}

/* ---------------------------------------------------------------------------
   Screen Repository
   --------------------------------------------------------------------------- */
public interface IScreenRepository
{
    Task<IEnumerable<Screen>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<Screen>> GetBySubModuleAsync(int subModuleId);
    Task<Screen?> GetByIdAsync(int id);
    Task<int> InsertAsync(Screen entity);
    Task<bool> UpdateAsync(Screen entity);
    Task<bool> DeleteAsync(int id);
}

public class ScreenRepository : TenantRepositoryBase, IScreenRepository
{
    public ScreenRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<Screen>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.Screens ORDER BY SortOrder, ScreenCode"
            : "SELECT * FROM dbo.Screens WHERE IsActive = 1 ORDER BY SortOrder, ScreenCode";
        return await Sql.QueryAsync<Screen>(conn, sql);
    }

    public async Task<IEnumerable<Screen>> GetBySubModuleAsync(int subModuleId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<Screen>(conn,
            "SELECT * FROM dbo.Screens WHERE SubModuleId = @subModuleId AND IsActive = 1 ORDER BY SortOrder",
            new { subModuleId });
    }

    public async Task<Screen?> GetByIdAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Screen>(conn,
            "SELECT * FROM dbo.Screens WHERE Id = @id", new { id });
    }

    public async Task<int> InsertAsync(Screen entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, PermissionCode, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
            VALUES (@SubModuleId, @ScreenCode, @ScreenName, @PermissionCode, @ScreenType, @RouteUrl, @ComponentName, @SortOrder, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity);
    }

    public async Task<bool> UpdateAsync(Screen entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.Screens
            SET ScreenCode = @ScreenCode, ScreenName = @ScreenName, PermissionCode = @PermissionCode, ScreenType = @ScreenType,
                RouteUrl = @RouteUrl, ComponentName = @ComponentName, SortOrder = @SortOrder, IsActive = @IsActive
            WHERE Id = @Id;";
        return await Sql.ExecuteAsync(conn, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = OpenTenant();
        await Sql.ExecuteAsync(conn, @"
            DELETE FROM dbo.Fields WHERE ScreenId = @id;
            DELETE FROM dbo.Screens WHERE Id = @id;", new { id });
        return true;
    }
}

/* ---------------------------------------------------------------------------
   Field Repository
   --------------------------------------------------------------------------- */
public interface IFieldRepository
{
    Task<IEnumerable<Field>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<Field>> GetByScreenAsync(int screenId);
    Task<Field?> GetByIdAsync(int id);
    Task<int> InsertAsync(Field entity);
    Task<bool> UpdateAsync(Field entity);
    Task<bool> DeleteAsync(int id);
}

public class FieldRepository : TenantRepositoryBase, IFieldRepository
{
    public FieldRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<Field>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.Fields ORDER BY DisplayOrder, FieldCode"
            : "SELECT * FROM dbo.Fields WHERE IsActive = 1 ORDER BY DisplayOrder, FieldCode";
        return await Sql.QueryAsync<Field>(conn, sql);
    }

    public async Task<IEnumerable<Field>> GetByScreenAsync(int screenId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<Field>(conn,
            "SELECT * FROM dbo.Fields WHERE ScreenId = @screenId AND IsActive = 1 ORDER BY DisplayOrder",
            new { screenId });
    }

    public async Task<Field?> GetByIdAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Field>(conn,
            "SELECT * FROM dbo.Fields WHERE Id = @id", new { id });
    }

    public async Task<int> InsertAsync(Field entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.Fields (ScreenId, FieldCode, FieldName, DisplayName, DataType, DisplayOrder, DefaultValue, IsSystemField, IsRequired, IsActive, CreatedBy)
            VALUES (@ScreenId, @FieldCode, @FieldName, @DisplayName, @DataType, @DisplayOrder, @DefaultValue, @IsSystemField, @IsRequired, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity);
    }

    public async Task<bool> UpdateAsync(Field entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.Fields
            SET FieldCode = @FieldCode, FieldName = @FieldName, DisplayName = @DisplayName,
                DataType = @DataType, DisplayOrder = @DisplayOrder, DefaultValue = @DefaultValue,
                IsRequired = @IsRequired, IsActive = @IsActive
            WHERE Id = @Id;";
        return await Sql.ExecuteAsync(conn, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "DELETE FROM dbo.Fields WHERE Id = @id", new { id }) > 0;
    }
}

/* ---------------------------------------------------------------------------
    Action Repository
    --------------------------------------------------------------------------- */
public interface IActionRepository
{
    Task<IEnumerable<PermissionActionEntry>> GetAllAsync(bool includeInactive = false);
    Task<PermissionActionEntry?> GetByIdAsync(int id);
    Task<PermissionActionEntry?> GetByCodeAsync(string code);
    Task<int> InsertAsync(PermissionActionEntry entity);
    Task<bool> UpdateAsync(PermissionActionEntry entity);
    Task<bool> DeleteAsync(int id);
}

public class ActionRepository : TenantRepositoryBase, IActionRepository
{
    public ActionRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<PermissionActionEntry>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.Actions ORDER BY DisplayOrder, ActionCode"
            : "SELECT * FROM dbo.Actions WHERE IsActive = 1 ORDER BY DisplayOrder, ActionCode";
        return await Sql.QueryAsync<PermissionActionEntry>(conn, sql);
    }

    public async Task<PermissionActionEntry?> GetByIdAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<PermissionActionEntry>(conn,
            "SELECT * FROM dbo.Actions WHERE Id = @id", new { id });
    }

    public async Task<PermissionActionEntry?> GetByCodeAsync(string code)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<PermissionActionEntry>(conn,
            "SELECT * FROM dbo.Actions WHERE ActionCode = @code", new { code });
    }

    public async Task<int> InsertAsync(PermissionActionEntry entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.Actions (ActionCode, ActionName, DisplayOrder, IsActive)
            VALUES (@ActionCode, @ActionName, @DisplayOrder, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity);
    }

    public async Task<bool> UpdateAsync(PermissionActionEntry entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.Actions
            SET ActionCode = @ActionCode, ActionName = @ActionName,
                DisplayOrder = @DisplayOrder, IsActive = @IsActive
            WHERE Id = @Id;";
        return await Sql.ExecuteAsync(conn, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "DELETE FROM dbo.Actions WHERE Id = @id", new { id }) > 0;
    }
}

/* ---------------------------------------------------------------------------
   RolePermissionEntry Repository (new hierarchical RolePermissions table)
   --------------------------------------------------------------------------- */
public interface IRolePermissionEntryRepository
{
    Task<IEnumerable<RolePermissionEntry>> GetByRoleAsync(int roleId);
    Task<RolePermissionEntry?> GetByIdAsync(int id);
    Task<int> InsertAsync(RolePermissionEntry entity);
    Task<bool> BulkInsertAsync(IEnumerable<RolePermissionEntry> entities);
    Task<bool> BulkReplaceAsync(int roleId, IEnumerable<RolePermissionEntry> entities);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAllForRoleAsync(int roleId);
}

public class RolePermissionEntryRepository : TenantRepositoryBase, IRolePermissionEntryRepository
{
    public RolePermissionEntryRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<RolePermissionEntry>> GetByRoleAsync(int roleId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<RolePermissionEntry>(conn,
            "SELECT * FROM dbo.RolePermissions WHERE RoleId = @roleId AND IsActive = 1 ORDER BY DisplayOrder",
            new { roleId });
    }

    public async Task<RolePermissionEntry?> GetByIdAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<RolePermissionEntry>(conn,
            "SELECT * FROM dbo.RolePermissions WHERE Id = @id", new { id });
    }

    public async Task<int> InsertAsync(RolePermissionEntry entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.RolePermissions (RoleId, WorkspaceId, DomainId, ModuleId, SubModuleId, ScreenId, ActionId, Allow, DisplayOrder, IsActive, CreatedBy)
            VALUES (@RoleId, @WorkspaceId, @DomainId, @ModuleId, @SubModuleId, @ScreenId, @ActionId, @Allow, @DisplayOrder, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity);
    }

    public async Task<bool> BulkInsertAsync(IEnumerable<RolePermissionEntry> entities)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.RolePermissions (RoleId, WorkspaceId, DomainId, ModuleId, SubModuleId, ScreenId, ActionId, Allow, DisplayOrder, IsActive, CreatedBy)
            VALUES (@RoleId, @WorkspaceId, @DomainId, @ModuleId, @SubModuleId, @ScreenId, @ActionId, @Allow, @DisplayOrder, @IsActive, @CreatedBy);";
        foreach (var entity in entities)
            await Sql.ExecuteAsync(conn, sql, entity);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "DELETE FROM dbo.RolePermissions WHERE Id = @id", new { id }) > 0;
    }

    public async Task<bool> DeleteAllForRoleAsync(int roleId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "DELETE FROM dbo.RolePermissions WHERE RoleId = @roleId", new { roleId }) > 0;
    }

    /// <summary>
    /// Atomically replaces a role's entire permission set: deletes all existing
    /// entries for the role, then bulk-inserts the supplied entries within a
    /// single transaction. Prevents UNIQUE KEY (UQ_RolePermissions_Matrix)
    /// violations when the matrix is re-saved.
    /// </summary>
    public async Task<bool> BulkReplaceAsync(int roleId, IEnumerable<RolePermissionEntry> entities)
    {
        var list = entities?.ToList() ?? new List<RolePermissionEntry>();
        using var conn = OpenTenant();
        if (conn.State != ConnectionState.Open)
            conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            await Sql.ExecuteAsync(conn,
                "DELETE FROM dbo.RolePermissions WHERE RoleId = @roleId", new { roleId }, tx);

            const string sql = @"
                INSERT INTO dbo.RolePermissions (RoleId, WorkspaceId, DomainId, ModuleId, SubModuleId, ScreenId, ActionId, Allow, DisplayOrder, IsActive, CreatedBy)
                VALUES (@RoleId, @WorkspaceId, @DomainId, @ModuleId, @SubModuleId, @ScreenId, @ActionId, @Allow, @DisplayOrder, @IsActive, @CreatedBy);";
            foreach (var entity in list)
                await Sql.ExecuteAsync(conn, sql, entity, tx);

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}

/* ---------------------------------------------------------------------------
   UserPermissionOverride Repository
   --------------------------------------------------------------------------- */
public interface IUserPermissionOverrideRepository
{
    Task<IEnumerable<UserPermissionOverride>> GetByUserAsync(int userId);
    Task<UserPermissionOverride?> GetByIdAsync(int id);
    Task<int> InsertAsync(UserPermissionOverride entity);
    Task<bool> UpdateAsync(UserPermissionOverride entity);
    Task<bool> DeleteAsync(int id);
}

public class UserPermissionOverrideRepository : TenantRepositoryBase, IUserPermissionOverrideRepository
{
    public UserPermissionOverrideRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<UserPermissionOverride>> GetByUserAsync(int userId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<UserPermissionOverride>(conn,
            "SELECT * FROM dbo.UserPermissionOverrides WHERE UserId = @userId AND IsActive = 1",
            new { userId });
    }

    public async Task<UserPermissionOverride?> GetByIdAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<UserPermissionOverride>(conn,
            "SELECT * FROM dbo.UserPermissionOverrides WHERE Id = @id", new { id });
    }

    public async Task<int> InsertAsync(UserPermissionOverride entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.UserPermissionOverrides (UserId, WorkspaceId, DomainId, ModuleId, SubModuleId, ScreenId, ActionId, PermissionType, Allow, EffectiveFrom, EffectiveTo, Remarks, IsActive, CreatedBy)
            VALUES (@UserId, @WorkspaceId, @DomainId, @ModuleId, @SubModuleId, @ScreenId, @ActionId, @PermissionType, @Allow, @EffectiveFrom, @EffectiveTo, @Remarks, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity);
    }

    public async Task<bool> UpdateAsync(UserPermissionOverride entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.UserPermissionOverrides
            SET Allow = @Allow, PermissionType = @PermissionType, EffectiveFrom = @EffectiveFrom,
                EffectiveTo = @EffectiveTo, Remarks = @Remarks, IsActive = @IsActive,
                ModifiedBy = @ModifiedBy, ModifiedDate = SYSUTCDATETIME()
            WHERE Id = @Id;";
        return await Sql.ExecuteAsync(conn, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "DELETE FROM dbo.UserPermissionOverrides WHERE Id = @id", new { id }) > 0;
    }
}

/* ---------------------------------------------------------------------------
   RoleFieldPermissionEntry Repository (new RoleFieldPermissions table)
   --------------------------------------------------------------------------- */
public interface IRoleFieldPermissionEntryRepository
{
    Task<IEnumerable<RoleFieldPermissionEntry>> GetByRoleAsync(int roleId);
    Task<IEnumerable<RoleFieldPermissionEntry>> GetByRoleScreenAsync(int roleId, int screenId);
    Task<RoleFieldPermissionEntry?> GetByIdAsync(int id);
    Task<int> InsertAsync(RoleFieldPermissionEntry entity);
    Task<bool> UpdateAsync(RoleFieldPermissionEntry entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> BulkInsertAsync(IEnumerable<RoleFieldPermissionEntry> entities);
}

public class RoleFieldPermissionEntryRepository : TenantRepositoryBase, IRoleFieldPermissionEntryRepository
{
    public RoleFieldPermissionEntryRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<RoleFieldPermissionEntry>> GetByRoleAsync(int roleId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<RoleFieldPermissionEntry>(conn,
            "SELECT * FROM dbo.RoleFieldPermissions WHERE RoleId = @roleId AND IsActive = 1",
            new { roleId });
    }

    public async Task<IEnumerable<RoleFieldPermissionEntry>> GetByRoleScreenAsync(int roleId, int screenId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<RoleFieldPermissionEntry>(conn,
            "SELECT * FROM dbo.RoleFieldPermissions WHERE RoleId = @roleId AND ScreenId = @screenId AND IsActive = 1",
            new { roleId, screenId });
    }

    public async Task<RoleFieldPermissionEntry?> GetByIdAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<RoleFieldPermissionEntry>(conn,
            "SELECT * FROM dbo.RoleFieldPermissions WHERE Id = @id", new { id });
    }

    public async Task<int> InsertAsync(RoleFieldPermissionEntry entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.RoleFieldPermissions (RoleId, ScreenId, FieldId, CanView, CanEdit, IsHidden, IsReadOnly, IsMandatory, DisplayOrder, IsActive, CreatedBy)
            VALUES (@RoleId, @ScreenId, @FieldId, @CanView, @CanEdit, @IsHidden, @IsReadOnly, @IsMandatory, @DisplayOrder, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity);
    }

    public async Task<bool> UpdateAsync(RoleFieldPermissionEntry entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.RoleFieldPermissions
            SET CanView = @CanView, CanEdit = @CanEdit, IsHidden = @IsHidden,
                IsReadOnly = @IsReadOnly, IsMandatory = @IsMandatory, DisplayOrder = @DisplayOrder,
                IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedDate = SYSUTCDATETIME()
            WHERE Id = @Id;";
        return await Sql.ExecuteAsync(conn, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "DELETE FROM dbo.RoleFieldPermissions WHERE Id = @id", new { id }) > 0;
    }

    public async Task<bool> BulkInsertAsync(IEnumerable<RoleFieldPermissionEntry> entities)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.RoleFieldPermissions (RoleId, ScreenId, FieldId, CanView, CanEdit, IsHidden, IsReadOnly, IsMandatory, DisplayOrder, IsActive, CreatedBy)
            VALUES (@RoleId, @ScreenId, @FieldId, @CanView, @CanEdit, @IsHidden, @IsReadOnly, @IsMandatory, @DisplayOrder, @IsActive, @CreatedBy);";
        foreach (var entity in entities)
            await Sql.ExecuteAsync(conn, sql, entity);
        return true;
    }
}

/* ---------------------------------------------------------------------------
   UserFieldPermissionEntry Repository
   --------------------------------------------------------------------------- */
public interface IUserFieldPermissionEntryRepository
{
    Task<IEnumerable<UserFieldPermissionEntry>> GetByUserAsync(int userId);
    Task<IEnumerable<UserFieldPermissionEntry>> GetByUserScreenAsync(int userId, int screenId);
    Task<UserFieldPermissionEntry?> GetByIdAsync(int id);
    Task<int> InsertAsync(UserFieldPermissionEntry entity);
    Task<bool> UpdateAsync(UserFieldPermissionEntry entity);
    Task<bool> DeleteAsync(int id);
}

public class UserFieldPermissionEntryRepository : TenantRepositoryBase, IUserFieldPermissionEntryRepository
{
    public UserFieldPermissionEntryRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<UserFieldPermissionEntry>> GetByUserAsync(int userId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<UserFieldPermissionEntry>(conn,
            "SELECT * FROM dbo.UserFieldPermissions WHERE UserId = @userId AND IsActive = 1",
            new { userId });
    }

    public async Task<IEnumerable<UserFieldPermissionEntry>> GetByUserScreenAsync(int userId, int screenId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<UserFieldPermissionEntry>(conn,
            "SELECT * FROM dbo.UserFieldPermissions WHERE UserId = @userId AND ScreenId = @screenId AND IsActive = 1",
            new { userId, screenId });
    }

    public async Task<UserFieldPermissionEntry?> GetByIdAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<UserFieldPermissionEntry>(conn,
            "SELECT * FROM dbo.UserFieldPermissions WHERE Id = @id", new { id });
    }

    public async Task<int> InsertAsync(UserFieldPermissionEntry entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.UserFieldPermissions (UserId, ScreenId, FieldId, CanView, CanEdit, IsHidden, IsReadOnly, IsMandatory, IsActive, CreatedBy)
            VALUES (@UserId, @ScreenId, @FieldId, @CanView, @CanEdit, @IsHidden, @IsReadOnly, @IsMandatory, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity);
    }

    public async Task<bool> UpdateAsync(UserFieldPermissionEntry entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.UserFieldPermissions
            SET CanView = @CanView, CanEdit = @CanEdit, IsHidden = @IsHidden,
                IsReadOnly = @IsReadOnly, IsMandatory = @IsMandatory, IsActive = @IsActive,
                ModifiedBy = @ModifiedBy, ModifiedDate = SYSUTCDATETIME()
            WHERE Id = @Id;";
        return await Sql.ExecuteAsync(conn, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "DELETE FROM dbo.UserFieldPermissions WHERE Id = @id", new { id }) > 0;
    }
}

/* ---------------------------------------------------------------------------
   DataScope Repository (RoleDataScopes - multi-row per role)
   --------------------------------------------------------------------------- */
public interface IDataScopeRepository
{
    Task<IEnumerable<DataScope>> GetByRoleAsync(int roleId);
    Task<DataScope?> GetByIdAsync(int id);
    Task<int> InsertAsync(DataScope entity);
    Task<bool> UpdateAsync(DataScope entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAllByRoleAsync(int roleId);
    Task<bool> InsertManyAsync(IEnumerable<DataScope> entities);
}

public class DataScopeRepository : TenantRepositoryBase, IDataScopeRepository
{
    public DataScopeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<DataScope>> GetByRoleAsync(int roleId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<DataScope>(conn,
            "SELECT * FROM dbo.RoleDataScopes WHERE RoleId = @roleId AND IsActive = 1", new { roleId });
    }

    public async Task<DataScope?> GetByIdAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<DataScope>(conn,
            "SELECT * FROM dbo.RoleDataScopes WHERE Id = @id", new { id });
    }

    public async Task<int> InsertAsync(DataScope entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.RoleDataScopes (RoleId, ModuleId, ScreenId, CompanyId, BranchId, WarehouseId, CanView, CanCreate, CanEdit, CanDelete,
                IsActive, CreatedBy)
            VALUES (@RoleId, @ModuleId, @ScreenId, @CompanyId, @BranchId, @WarehouseId, @CanView, @CanCreate, @CanEdit, @CanDelete,
                @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity);
    }

    public async Task<bool> UpdateAsync(DataScope entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.RoleDataScopes
            SET ModuleId = @ModuleId, ScreenId = @ScreenId, CompanyId = @CompanyId, BranchId = @BranchId,
                WarehouseId = @WarehouseId, CanView = @CanView, CanCreate = @CanCreate, CanEdit = @CanEdit, CanDelete = @CanDelete,
                IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedDate = SYSUTCDATETIME()
            WHERE Id = @Id;";
        return await Sql.ExecuteAsync(conn, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "DELETE FROM dbo.RoleDataScopes WHERE Id = @id", new { id }) > 0;
    }

    public async Task<bool> DeleteAllByRoleAsync(int roleId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "DELETE FROM dbo.RoleDataScopes WHERE RoleId = @roleId", new { roleId }) >= 0;
    }

    public async Task<bool> InsertManyAsync(IEnumerable<DataScope> entities)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.RoleDataScopes (RoleId, ModuleId, ScreenId, CompanyId, BranchId, WarehouseId, CanView, CanCreate, CanEdit, CanDelete,
                IsActive, CreatedBy)
            VALUES (@RoleId, @ModuleId, @ScreenId, @CompanyId, @BranchId, @WarehouseId, @CanView, @CanCreate, @CanEdit, @CanDelete,
                @IsActive, @CreatedBy);";
        foreach (var entity in entities)
            await Sql.ExecuteAsync(conn, sql, entity);
        return true;
    }
}

/* ---------------------------------------------------------------------------
   UserDataScopeOverride Repository
   --------------------------------------------------------------------------- */
public interface IUserDataScopeOverrideRepository
{
    Task<IEnumerable<UserDataScopeOverride>> GetByUserAsync(int userId);
    Task<UserDataScopeOverride?> GetByIdAsync(int id);
    Task<int> InsertAsync(UserDataScopeOverride entity);
    Task<bool> UpdateAsync(UserDataScopeOverride entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAllByUserAsync(int userId);
    Task<bool> InsertManyAsync(IEnumerable<UserDataScopeOverride> entities);
}

public class UserDataScopeOverrideRepository : TenantRepositoryBase, IUserDataScopeOverrideRepository
{
    public UserDataScopeOverrideRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<UserDataScopeOverride>> GetByUserAsync(int userId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<UserDataScopeOverride>(conn,
            "SELECT * FROM dbo.UserDataScopeOverrides WHERE UserId = @userId AND IsActive = 1", new { userId });
    }

    public async Task<UserDataScopeOverride?> GetByIdAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<UserDataScopeOverride>(conn,
            "SELECT * FROM dbo.UserDataScopeOverrides WHERE Id = @id", new { id });
    }

    public async Task<int> InsertAsync(UserDataScopeOverride entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.UserDataScopeOverrides (UserId, ModuleId, ScreenId, ScopeType, ScopeValue,
                PermissionType, Allow, EffectiveFrom, EffectiveTo, Remarks, IsActive, CreatedBy)
            VALUES (@UserId, @ModuleId, @ScreenId, @ScopeType, @ScopeValue,
                @PermissionType, @Allow, @EffectiveFrom, @EffectiveTo, @Remarks, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity);
    }

    public async Task<bool> UpdateAsync(UserDataScopeOverride entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.UserDataScopeOverrides
            SET ScopeType = @ScopeType, ScopeValue = @ScopeValue, PermissionType = @PermissionType,
                Allow = @Allow, EffectiveFrom = @EffectiveFrom, EffectiveTo = @EffectiveTo,
                Remarks = @Remarks, IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedDate = SYSUTCDATETIME()
            WHERE Id = @Id;";
        return await Sql.ExecuteAsync(conn, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "DELETE FROM dbo.UserDataScopeOverrides WHERE Id = @id", new { id }) > 0;
    }

    public async Task<bool> DeleteAllByUserAsync(int userId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "DELETE FROM dbo.UserDataScopeOverrides WHERE UserId = @userId", new { userId }) >= 0;
    }

    public async Task<bool> InsertManyAsync(IEnumerable<UserDataScopeOverride> entities)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.UserDataScopeOverrides (UserId, ModuleId, ScreenId, ScopeType, ScopeValue,
                PermissionType, Allow, EffectiveFrom, EffectiveTo, Remarks, IsActive, CreatedBy)
            VALUES (@UserId, @ModuleId, @ScreenId, @ScopeType, @ScopeValue,
                @PermissionType, @Allow, @EffectiveFrom, @EffectiveTo, @Remarks, @IsActive, @CreatedBy);";
        foreach (var entity in entities)
            await Sql.ExecuteAsync(conn, sql, entity);
        return true;
    }
}

/* ---------------------------------------------------------------------------
   WorkflowPermissionEntry Repository
   --------------------------------------------------------------------------- */
public interface IWorkflowPermissionEntryRepository
{
    Task<IEnumerable<WorkflowPermissionEntry>> GetByRoleAsync(int roleId);
    Task<WorkflowPermissionEntry?> GetByIdAsync(int id);
    Task<int> InsertAsync(WorkflowPermissionEntry entity);
    Task<bool> UpdateAsync(WorkflowPermissionEntry entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> BulkInsertAsync(IEnumerable<WorkflowPermissionEntry> entities);
}

public class WorkflowPermissionEntryRepository : TenantRepositoryBase, IWorkflowPermissionEntryRepository
{
    public WorkflowPermissionEntryRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<WorkflowPermissionEntry>> GetByRoleAsync(int roleId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<WorkflowPermissionEntry>(conn,
            "SELECT * FROM dbo.WorkflowPermissions WHERE RoleId = @roleId AND IsActive = 1",
            new { roleId });
    }

    public async Task<WorkflowPermissionEntry?> GetByIdAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<WorkflowPermissionEntry>(conn,
            "SELECT * FROM dbo.WorkflowPermissions WHERE Id = @id", new { id });
    }

    public async Task<int> InsertAsync(WorkflowPermissionEntry entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.WorkflowPermissions (RoleId, ModuleId, SubModuleId, ScreenId, CanSubmit, CanApprove, CanReject, CanCancel, CanClose, IsActive, CreatedBy)
            VALUES (@RoleId, @ModuleId, @SubModuleId, @ScreenId, @CanSubmit, @CanApprove, @CanReject, @CanCancel, @CanClose, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity);
    }

    public async Task<bool> UpdateAsync(WorkflowPermissionEntry entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.WorkflowPermissions
            SET CanSubmit = @CanSubmit, CanApprove = @CanApprove, CanReject = @CanReject,
                CanCancel = @CanCancel, CanClose = @CanClose, IsActive = @IsActive
            WHERE Id = @Id;";
        return await Sql.ExecuteAsync(conn, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "DELETE FROM dbo.WorkflowPermissions WHERE Id = @id", new { id }) > 0;
    }

    public async Task<bool> BulkInsertAsync(IEnumerable<WorkflowPermissionEntry> entities)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.WorkflowPermissions (RoleId, ModuleId, SubModuleId, ScreenId, CanSubmit, CanApprove, CanReject, CanCancel, CanClose, IsActive, CreatedBy)
            VALUES (@RoleId, @ModuleId, @SubModuleId, @ScreenId, @CanSubmit, @CanApprove, @CanReject, @CanCancel, @CanClose, @IsActive, @CreatedBy);";
        foreach (var entity in entities)
            await Sql.ExecuteAsync(conn, sql, entity);
        return true;
    }
}
