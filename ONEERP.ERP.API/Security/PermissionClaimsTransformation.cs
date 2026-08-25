using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using SharedClaimTypes = ONEERP.Shared.Constants.ClaimTypes;
using ONEERP.ERP.API.Data;
using ONEERP.ERP.API.Repositories;

namespace ONEERP.ERP.API.Security;

/// <summary>
/// Resolves a user's effective permission codes and injects them as
/// <see cref="SharedClaimTypes.Permission"/> claims on the authenticated
/// principal, so <see cref="PermissionAuthorizationFilter"/> can enforce them.
///
/// The API's authoritative permission codes are stored in
/// <c>dbo.RolePermissionsLegacy</c> as <c>PermissionCode</c> strings
/// (e.g. "companies.view", "branches.view"), which is exactly what the
/// <c>[Permission]</c> attributes on controllers require. The JWT deliberately
/// omits these claims (see TokenService), so they must be re-derived per
/// request here. Super admins bypass authorization and need no claims.
/// </summary>
public class PermissionClaimsTransformation : IClaimsTransformation
{
    private readonly TenantAccessor _tenant;
    private readonly ITenantConnectionResolver _resolver;
    private readonly IRoleRepository _roleRepository;

    public PermissionClaimsTransformation(
        TenantAccessor tenant,
        ITenantConnectionResolver resolver,
        IRoleRepository roleRepository)
    {
        _tenant = tenant;
        _resolver = resolver;
        _roleRepository = roleRepository;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return principal;

        // Super admins are bypassed by the authorization filter.
        if (principal.HasClaim(SharedClaimTypes.IsSuperAdmin, "true"))
            return principal;

        var tenantCode = principal.FindFirst(SharedClaimTypes.TenantCode)?.Value;
        if (string.IsNullOrWhiteSpace(tenantCode))
            return principal;

        // TenantContextMiddleware normally populates TenantAccessor, but it runs
        // after authentication. Resolve it here from the JWT claim so the role
        // repository can open the tenant database.
        _tenant.TenantCode = tenantCode;
        _tenant.ConnectionString = await _resolver.GetConnectionStringAsync(tenantCode);

        var userIdRaw = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdRaw, out var userId))
            return principal;

        var permissions = await _roleRepository.GetPermissionsForUserAsync(userId);
        var identity = (ClaimsIdentity)principal.Identity!;
        foreach (var permission in permissions)
        {
            if (!identity.HasClaim(SharedClaimTypes.Permission, permission))
                identity.AddClaim(new Claim(SharedClaimTypes.Permission, permission));
        }

        return principal;
    }
}
