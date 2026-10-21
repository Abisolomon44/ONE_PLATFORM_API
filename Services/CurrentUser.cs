using System.Security.Claims;
using ONEERP.Shared.Exceptions;
using SharedConstants = ONEERP.Shared.Constants;
using SharedClaimTypes = ONEERP.Shared.Constants.ClaimTypes;

namespace ONEERP.Platform.API.Services;

/// <summary>
/// Resolves the currently authenticated platform user from the HTTP context.
/// </summary>
public interface ICurrentUser
{
    int UserId { get; }
    string Username { get; }
    string? DisplayName { get; }

    /// <summary>Platform role carried by the JWT (PlatformAdmin / SuperAdmin / Administrator).</summary>
    string? Role { get; }

    /// <summary>True when the caller holds a platform administrator role.</summary>
    bool IsPlatformAdmin { get; }

    /// <summary>
    /// Platform permission codes carried by the JWT. Multiple <c>permission</c>
    /// claims are tolerated as well as a single comma/space separated claim.
    /// </summary>
    IReadOnlyList<string> Permissions { get; }

    /// <summary>
    /// True when the caller explicitly holds <paramref name="permission"/>.
    /// Platform administrators hold every platform permission implicitly, so
    /// the check also honours the <c>*</c> wildcard claim and the admin roles.
    /// </summary>
    bool HasPermission(string permission);
}

public class CurrentUser : ICurrentUser
{
    private static readonly string[] AdminRoles =
    {
        SharedConstants.RoleNames.PlatformAdmin,
        SharedConstants.RoleNames.SuperAdmin,
        SharedConstants.RoleNames.Administrator
    };

    private static readonly char[] PermissionSeparators = { ',', ' ' };

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public int UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : throw new UnauthorizedAccess();
        }
    }

    public string Username =>
        Principal?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    public string? DisplayName =>
        Principal?.FindFirstValue(ClaimTypes.GivenName);

    public string? Role =>
        Principal?.FindFirstValue(ClaimTypes.Role)
        ?? Principal?.FindFirstValue(SharedClaimTypes.PlatformRole);

    public bool IsPlatformAdmin
    {
        get
        {
            var principal = Principal;
            if (principal?.Identity?.IsAuthenticated != true)
                return false;

            var role = Role;
            return !string.IsNullOrWhiteSpace(role)
                && AdminRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }
    }

    public IReadOnlyList<string> Permissions
    {
        get
        {
            var principal = Principal;
            if (principal?.Identity?.IsAuthenticated != true)
                return Array.Empty<string>();

            return principal.FindAll(SharedClaimTypes.Permission)
                .SelectMany(c => (c.Value ?? string.Empty)
                    .Split(PermissionSeparators, StringSplitOptions.RemoveEmptyEntries))
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    public bool HasPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return false;

        if (IsPlatformAdmin)
            return true;

        var held = Permissions;
        return held.Contains(SharedConstants.MigrationPermissions.Wildcard, StringComparer.OrdinalIgnoreCase)
            || held.Contains(permission.Trim(), StringComparer.OrdinalIgnoreCase);
    }
}
