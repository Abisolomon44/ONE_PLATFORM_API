using System;
using System.Linq;
using ONEERP.ERP.API.Security;

namespace ONEERP.ERP.API.Services;

/// <summary>
/// Resolves the effective data scope (Company / Branch / Warehouse) for the current user
/// from the server-side permission cache. Returns null when a level is unrestricted.
/// </summary>
public interface IDataScopeResolver
{
    /// <summary>Allowed company ids, or null when unrestricted (super admin / no company scope).</summary>
    Task<IReadOnlyCollection<int>?> GetAllowedCompanyIdsAsync();

    /// <summary>Allowed branch ids, or null when unrestricted.</summary>
    Task<IReadOnlyCollection<int>?> GetAllowedBranchIdsAsync();

    /// <summary>Allowed warehouse ids, or null when unrestricted.</summary>
    Task<IReadOnlyCollection<int>?> GetAllowedWarehouseIdsAsync();

    Task<bool> CanAccessCompanyAsync(int companyId);
    Task<bool> CanAccessBranchAsync(int branchId);
    Task<bool> CanAccessWarehouseAsync(int warehouseId);
}

public class DataScopeResolver : IDataScopeResolver
{
    private readonly IPermissionCache _permissionCache;
    private readonly ICurrentUser _currentUser;

    public DataScopeResolver(IPermissionCache permissionCache, ICurrentUser currentUser)
    {
        _permissionCache = permissionCache;
        _currentUser = currentUser;
    }

    private Task<ResolvedPermissions?> GetResolvedAsync()
        => _permissionCache.GetAsync(_currentUser.UserId, _currentUser.TenantId, _currentUser.CompanyId);

    public async Task<IReadOnlyCollection<int>?> GetAllowedCompanyIdsAsync()
    {
        if (_currentUser.IsSuperAdmin)
            return null;

        var resolved = await GetResolvedAsync();
        if (resolved is null)
            return new[] { _currentUser.CompanyId };

        var ids = new HashSet<int>();
        foreach (var d in resolved.DataScopes)
        {
            if (d.CanView && d.CompanyId.HasValue)
                ids.Add(d.CompanyId.Value);
        }
        foreach (var id in ResolveOverrideIds(resolved, "Company"))
            ids.Add(id);

        if (string.Equals(resolved.EffectiveScope?.Level, "Company", StringComparison.OrdinalIgnoreCase) && resolved.EffectiveScope!.Id > 0)
            ids.Add(resolved.EffectiveScope.Id);

        return ids.Count > 0 ? ids.ToArray() : new[] { _currentUser.CompanyId };
    }

    public async Task<IReadOnlyCollection<int>?> GetAllowedBranchIdsAsync()
    {
        if (_currentUser.IsSuperAdmin)
            return null;

        var resolved = await GetResolvedAsync();
        if (resolved is null)
            return null;

        var ids = new HashSet<int>();
        foreach (var d in resolved.DataScopes)
        {
            if (d.CanView && d.BranchId.HasValue)
                ids.Add(d.BranchId.Value);
        }
        foreach (var id in ResolveOverrideIds(resolved, "Branch"))
            ids.Add(id);

        if (string.Equals(resolved.EffectiveScope?.Level, "Branch", StringComparison.OrdinalIgnoreCase) && resolved.EffectiveScope!.Id > 0)
            ids.Add(resolved.EffectiveScope.Id);

        return ids.Count > 0 ? ids.ToArray() : null;
    }

    public async Task<IReadOnlyCollection<int>?> GetAllowedWarehouseIdsAsync()
    {
        if (_currentUser.IsSuperAdmin)
            return null;

        var resolved = await GetResolvedAsync();
        if (resolved is null)
            return null;

        var ids = new HashSet<int>();
        foreach (var d in resolved.DataScopes)
        {
            if (d.CanView && d.WarehouseId.HasValue)
                ids.Add(d.WarehouseId.Value);
        }
        foreach (var id in ResolveOverrideIds(resolved, "Warehouse"))
            ids.Add(id);

        if (string.Equals(resolved.EffectiveScope?.Level, "Warehouse", StringComparison.OrdinalIgnoreCase) && resolved.EffectiveScope!.Id > 0)
            ids.Add(resolved.EffectiveScope.Id);

        return ids.Count > 0 ? ids.ToArray() : null;
    }

    public async Task<bool> CanAccessCompanyAsync(int companyId)
    {
        var allowed = await GetAllowedCompanyIdsAsync();
        return allowed is null || allowed.Contains(companyId);
    }

    public async Task<bool> CanAccessBranchAsync(int branchId)
    {
        var allowed = await GetAllowedBranchIdsAsync();
        return allowed is null || allowed.Contains(branchId);
    }

    public async Task<bool> CanAccessWarehouseAsync(int warehouseId)
    {
        var allowed = await GetAllowedWarehouseIdsAsync();
        return allowed is null || allowed.Contains(warehouseId);
    }

    private static IEnumerable<int> ResolveOverrideIds(ResolvedPermissions resolved, string scopeType)
    {
        foreach (var o in resolved.UserOverrides)
        {
            if (string.Equals(o.ScopeType, scopeType, StringComparison.OrdinalIgnoreCase)
                && o.Allow
                && int.TryParse(o.ScopeValue, out var id)
                && id > 0)
            {
                yield return id;
            }
        }
    }
}