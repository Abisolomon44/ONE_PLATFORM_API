using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Security;

/// <summary>
/// Declares the permissions required to invoke a controller action.
/// The filter allows any of the listed permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class PermissionAttribute : Attribute
{
    public string[] Permissions { get; }

    public PermissionAttribute(params string[] permissions)
    {
        Permissions = permissions;
    }
}

/// <summary>
/// Global authorization filter that enforces permission-based access.
/// Actions without a [Permission] attribute only require authentication.
/// </summary>
public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        var requiredPermissions = endpoint?.Metadata
            .GetOrderedMetadata<PermissionAttribute>()
            .SelectMany(a => a.Permissions)
            .Distinct()
            .ToArray() ?? Array.Empty<string>();

        if (requiredPermissions.Length == 0)
            return;

        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (user.HasClaim(ClaimTypes.Role, "Super Admin"))
            return;

        var hasAny = requiredPermissions.Any(p => user.HasClaim(ClaimTypes.Permission, p));

        if (!hasAny)
        {
            context.Result = new ObjectResult(ApiResponse.Fail("You do not have permission to perform this action."))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        await Task.CompletedTask;
    }
}
