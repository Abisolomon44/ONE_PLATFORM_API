using System.Security.Claims;
using ONEERP.Shared.Exceptions;
using SharedClaimTypes = ONEERP.Shared.Constants.ClaimTypes;

namespace ONEERP.ERP.API.Services;

/// <summary>
/// Resolves the currently authenticated tenant user from JWT claims.
/// </summary>
public interface ICurrentUser
{
    int UserId { get; }
    string Username { get; }
    string? DisplayName { get; }
    int TenantId { get; }
    string TenantCode { get; }
    int CompanyId { get; }
    bool IsSuperAdmin { get; }
    bool HasPermission(string permission);
    bool HasAnyPermission(params string[] permissions);
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private System.Security.Claims.ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    private string GetClaim(string claimType)
        => Principal?.FindFirstValue(claimType) ?? string.Empty;

    public int UserId => int.TryParse(GetClaim(ClaimTypes.NameIdentifier), out var id) ? id : throw new UnauthorizedAccess();

    public string Username => GetClaim(ClaimTypes.Name);

    public string? DisplayName => GetClaim(ClaimTypes.GivenName);

    public int TenantId => int.TryParse(GetClaim(SharedClaimTypes.TenantId), out var id) ? id : throw new UnauthorizedAccess();

    public string TenantCode => GetClaim(SharedClaimTypes.TenantCode);

    public int CompanyId => int.TryParse(GetClaim(SharedClaimTypes.CompanyId), out var id) ? id : throw new UnauthorizedAccess();

    public bool IsSuperAdmin => Principal?.HasClaim(SharedClaimTypes.IsSuperAdmin, "true") ?? false;

    public bool HasPermission(string permission) => Principal?.HasClaim(SharedClaimTypes.Permission, permission) ?? false;

    public bool HasAnyPermission(params string[] permissions)
        => permissions.Any(p => HasPermission(p));
}
