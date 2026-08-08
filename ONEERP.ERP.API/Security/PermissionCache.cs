using System.Collections.Concurrent;
using ONEERP.Shared.Constants;

namespace ONEERP.ERP.API.Security;

/// <summary>
/// Data scope restriction for a role (branch, department, warehouse, etc.)
/// </summary>
public sealed class RoleDataScopeEntry
{
    public int RoleId { get; init; }
    public int? ModuleId { get; init; }
    public int? ScreenId { get; init; }
    public int? BranchId { get; init; }
    public int? DepartmentId { get; init; }
    public int? WarehouseId { get; init; }
    public bool CanView { get; init; }
    public bool CanCreate { get; init; }
    public bool CanEdit { get; init; }
    public bool CanDelete { get; init; }
}

/// <summary>
/// User-level data scope override.
/// </summary>
public sealed class UserDataScopeOverrideEntry
{
    public int UserId { get; init; }
    public int? ModuleId { get; init; }
    public int? ScreenId { get; init; }
    public string ScopeType { get; init; } = string.Empty;
    public string ScopeValue { get; init; } = string.Empty;
    public string PermissionType { get; init; } = string.Empty;
    public bool Allow { get; init; }
    public DateTime EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
}

/// <summary>
/// Resolved permission set for a user in a specific tenant context.
/// </summary>
public sealed class ResolvedPermissions
{
    public int UserId { get; init; }
    public int TenantId { get; init; }
    public int CompanyId { get; init; }
    public HashSet<string> PermissionCodes { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public List<RoleDataScopeEntry> DataScopes { get; init; } = new();
    public List<UserDataScopeOverrideEntry> UserOverrides { get; init; } = new();
    public int Version { get; init; }
    public DateTimeOffset Expiry { get; set; }
}

/// <summary>
/// Server-side permission cache.  Key = "userId:tenantId:companyId".
/// </summary>
public interface IPermissionCache
{
    Task<ResolvedPermissions?> GetAsync(int userId, int tenantId, int companyId);
    Task SetAsync(ResolvedPermissions permissions);
    void Invalidate(int userId, int tenantId);
    void InvalidateAll();
}

public class InMemoryPermissionCache : IPermissionCache
{
    private readonly ConcurrentDictionary<string, ResolvedPermissions> _cache = new();
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(15);

    public Task<ResolvedPermissions?> GetAsync(int userId, int tenantId, int companyId)
    {
        var key = MakeKey(userId, tenantId, companyId);
        if (_cache.TryGetValue(key, out var entry) && entry.Expiry > DateTimeOffset.UtcNow)
            return Task.FromResult<ResolvedPermissions?>(entry);
        _cache.TryRemove(key, out _);
        return Task.FromResult<ResolvedPermissions?>(null);
    }

    public Task SetAsync(ResolvedPermissions permissions)
    {
        var key = MakeKey(permissions.UserId, permissions.TenantId, permissions.CompanyId);
        permissions.Expiry = DateTimeOffset.UtcNow.Add(DefaultTtl);
        _cache[key] = permissions;
        return Task.CompletedTask;
    }

    public void Invalidate(int userId, int tenantId)
    {
        var prefix = $"{userId}:{tenantId}:";
        foreach (var k in _cache.Keys.Where(k => k.StartsWith(prefix)))
            _cache.TryRemove(k, out _);
    }

    public void InvalidateAll() => _cache.Clear();

    private static string MakeKey(int userId, int tenantId, int companyId)
        => $"{userId}:{tenantId}:{companyId}";
}
