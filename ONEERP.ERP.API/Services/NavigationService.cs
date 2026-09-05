using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ONEERP.ERP.API.Data;
using ONEERP.ERP.API.Security;

namespace ONEERP.ERP.API.Services;

/* ---------------------------------------------------------------------------
   Navigation DTOs
   --------------------------------------------------------------------------- */
public record NavigationScreenDto(int Id, string Code, string Name, string? RouteUrl, string? ComponentName, string ScreenType, int SubModuleId);
public record NavigationSubModuleDto(int Id, string Code, string Name, string? Icon, List<NavigationScreenDto> Screens, int ModuleId);
public record NavigationModuleDto(int Id, string Code, string Name, string? Icon, List<NavigationSubModuleDto> SubModules, int DomainId);
public record NavigationDomainDto(int Id, string Code, string Name, string? Icon, List<NavigationModuleDto> Modules, int WorkspaceId);
public record NavigationWorkspaceDto(int Id, string Code, string Name, string? Icon, List<NavigationDomainDto> Domains);
public record NavigationResponse(List<NavigationWorkspaceDto> Workspaces, int PermissionVersion, bool HasAccess);

/* ---------------------------------------------------------------------------
   Navigation Service
   --------------------------------------------------------------------------- */
public interface INavigationService
{
    Task<NavigationResponse> GetNavigationAsync(int userId, int tenantId, int companyId, bool isSuperAdmin);
}

public class NavigationService : INavigationService
{
    private readonly ISqlHelper _sql;
    private readonly TenantAccessor _accessor;
    private readonly IPlatformDbConnectionFactory _platformFactory;
    private readonly IPermissionCache _cache;

    public NavigationService(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory, IPermissionCache cache)
    {
        _sql = sql;
        _accessor = accessor;
        _platformFactory = platformFactory;
        _cache = cache;
    }

    private System.Data.Common.DbConnection OpenTenant()
    {
        var connStr = _accessor.ConnectionString ?? throw new InvalidOperationException("No tenant connection.");
        var conn = new SqlConnection(connStr);
        conn.Open();
        return conn;
    }

    public async Task<NavigationResponse> GetNavigationAsync(int userId, int tenantId, int companyId, bool isSuperAdmin)
    {
        using var conn = OpenTenant();

        const string sql = @"
            SELECT DISTINCT ws.Id, ws.SortOrder, ws.WorkspaceCode AS Code, ws.WorkspaceName AS Name, ws.Icon
            FROM dbo.Workspaces ws
            INNER JOIN dbo.Domains d ON d.WorkspaceId = ws.Id
            INNER JOIN dbo.Modules m ON m.DomainId = d.Id
            INNER JOIN dbo.SubModules sm ON sm.ModuleId = m.Id
            INNER JOIN dbo.Screens sc ON sc.SubModuleId = sm.Id
            WHERE ws.IsActive = 1 AND d.IsActive = 1 AND m.IsActive = 1 AND sm.IsActive = 1 AND sc.IsActive = 1
            ORDER BY ws.SortOrder, ws.WorkspaceCode;

            SELECT d.Id, d.DomainCode AS Code, d.DomainName AS Name, d.Icon, d.WorkspaceId
            FROM dbo.Domains d WHERE d.IsActive = 1 ORDER BY d.SortOrder, d.DomainCode;

            SELECT m.Id, m.ModuleCode AS Code, m.ModuleName AS Name, m.Icon, m.DomainId
            FROM dbo.Modules m WHERE m.IsActive = 1 ORDER BY m.SortOrder, m.ModuleCode;

            SELECT sm.Id, sm.SubModuleCode AS Code, sm.SubModuleName AS Name, sm.Icon, sm.ModuleId
            FROM dbo.SubModules sm WHERE sm.IsActive = 1 ORDER BY sm.SortOrder, sm.SubModuleCode;

            SELECT sc.Id, sc.ScreenCode AS Code, sc.ScreenName AS Name, sc.RouteUrl, sc.ComponentName, sc.ScreenType, sc.SubModuleId
            FROM dbo.Screens sc WHERE sc.IsActive = 1 ORDER BY sc.SortOrder, sc.ScreenCode;";

        using var multi = await conn.QueryMultipleAsync(sql);

        var workspaces = (await multi.ReadAsync<WsRow>()).Select(r =>
            new NavigationWorkspaceDto(r.Id, r.Code, r.Name, r.Icon, new List<NavigationDomainDto>())).ToList();

        var domains = (await multi.ReadAsync<DomRow>()).Select(r =>
            new NavigationDomainDto(r.Id, r.Code, r.Name, r.Icon, new List<NavigationModuleDto>(), r.WorkspaceId)).ToList();

        var modules = (await multi.ReadAsync<ModRow>()).Select(r =>
            new NavigationModuleDto(r.Id, r.Code, r.Name, r.Icon, new List<NavigationSubModuleDto>(), r.DomainId)).ToList();

        var subModules = (await multi.ReadAsync<SmRow>()).Select(r =>
            new NavigationSubModuleDto(r.Id, r.Code, r.Name, r.Icon, new List<NavigationScreenDto>(), r.ModuleId)).ToList();

        var screens = (await multi.ReadAsync<ScRow>())
            .Select(r => new NavigationScreenDto(r.Id, r.Code, r.Name, r.RouteUrl, r.ComponentName, r.ScreenType, r.SubModuleId))
            .ToList();

        // Resolve the set of screen-level permissions granted (or denied) to the
        // user so the navigation tree only surfaces the workspaces/screens the
        // user is actually entitled to see (screen + action level).
        var accessibleScreens = isSuperAdmin ? null : await GetAccessibleScreenIdsAsync(conn, userId);

        var visible = isSuperAdmin
            ? screens
            : screens.Where(s => accessibleScreens!.Contains(s.Id)).ToList();

        // Build tree
        foreach (var s in visible)
        {
            var p = subModules.FirstOrDefault(sm => sm.Id == s.SubModuleId);
            p?.Screens.Add(new NavigationScreenDto(s.Id, s.Code, s.Name, s.RouteUrl, s.ComponentName, s.ScreenType, s.SubModuleId));
        }
        foreach (var sm in subModules)
        {
            var p = modules.FirstOrDefault(m => m.Id == sm.ModuleId);
            p?.SubModules.Add(new NavigationSubModuleDto(sm.Id, sm.Code, sm.Name, sm.Icon, sm.Screens, sm.ModuleId));
        }
        foreach (var m in modules)
        {
            var p = domains.FirstOrDefault(d => d.Id == m.DomainId);
            p?.Modules.Add(new NavigationModuleDto(m.Id, m.Code, m.Name, m.Icon, m.SubModules, m.DomainId));
        }
        foreach (var d in domains)
        {
            var p = workspaces.FirstOrDefault(w => w.Id == d.WorkspaceId);
            p?.Domains.Add(new NavigationDomainDto(d.Id, d.Code, d.Name, d.Icon, d.Modules, d.WorkspaceId));
        }

        var accessible = workspaces
            .Where(w => w.Domains.Any(d => d.Modules.Any(m => m.SubModules.Any(sm => sm.Screens.Any()))))
            .ToList();
        var hasAccess = accessible.Count > 0;
        var cached = await _cache.GetAsync(userId, tenantId, companyId);
        return new NavigationResponse(accessible, cached?.Version ?? 1, hasAccess);
    }

    /// <summary>
    /// Resolves the set of ScreenIds the user is entitled to access, applying
    /// role-level grants and then user-level allow/deny overrides. This mirrors
    /// the hierarchical permission resolution used during login so the sidebar
    /// navigation stays consistent with the authorization engine.
    /// </summary>
    private async Task<HashSet<int>> GetAccessibleScreenIdsAsync(System.Data.Common.DbConnection conn, int userId)
    {
        var roleIds = (await _sql.QueryAsync<int>(conn,
            "SELECT RoleId FROM dbo.UserRoles WHERE UserId = @userId", new { userId })).ToList();

        var allowed = new HashSet<int>();
        var denied = new HashSet<int>();

        if (roleIds.Count > 0)
        {
            var granted = await _sql.QueryAsync<int>(conn, @"
                SELECT DISTINCT ScreenId
                FROM dbo.RolePermissions
                WHERE RoleId IN @roleIds AND IsActive = 1 AND Allow = 1 AND ScreenId IS NOT NULL",
                new { roleIds });
            foreach (var s in granted)
                allowed.Add(s);
        }

        var overrides = await _sql.QueryAsync<OverrideRow>(conn, @"
            SELECT ScreenId, Allow
            FROM dbo.UserPermissionOverrides
            WHERE UserId = @userId AND IsActive = 1
              AND EffectiveFrom <= SYSUTCDATETIME()
              AND (EffectiveTo IS NULL OR EffectiveTo > SYSUTCDATETIME())",
            new { userId });

        foreach (var o in overrides)
        {
            if (o.Allow) allowed.Add(o.ScreenId);
            else { denied.Add(o.ScreenId); allowed.Remove(o.ScreenId); }
        }

        allowed.ExceptWith(denied);
        return allowed;
    }

    private record OverrideRow(int ScreenId, bool Allow);

    private record WsRow(int Id, int SortOrder, string Code, string Name, string? Icon);
    private record DomRow(int Id, string Code, string Name, string? Icon, int WorkspaceId);
    private record ModRow(int Id, string Code, string Name, string? Icon, int DomainId);
    private record SmRow(int Id, string Code, string Name, string? Icon, int ModuleId);
    private record ScRow(int Id, string Code, string Name, string? RouteUrl, string? ComponentName, string ScreenType, int SubModuleId);
}
