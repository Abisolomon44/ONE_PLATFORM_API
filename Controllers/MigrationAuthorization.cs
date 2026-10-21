using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ONEERP.Platform.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.Platform.API.Controllers;

/// <summary>
/// Declares the platform permission an endpoint requires. Enforced by
/// <see cref="MigrationAuthorizationFilter"/>, so authorization is decided by
/// the API regardless of what the Angular client does.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class MigrationAuthorizationAttribute : Attribute
{
    public MigrationAuthorizationAttribute(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}

/// <summary>
/// Server-side authorization filter for the migration endpoints. The declared
/// permission is evaluated against the <c>permission</c> claims carried by the
/// JWT, so a platform account can be granted read-only migration access
/// (migrations.view) without being able to execute schema changes.
/// </summary>
public class MigrationAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly ICurrentUser _currentUser;

    public MigrationAuthorizationFilter(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        var required = endpoint?.Metadata.GetOrderedMetadata<MigrationAuthorizationAttribute>()
            .Select(a => a.Permission)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();

        if (required.Length == 0)
            return Task.CompletedTask;

        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(ApiResponse.Fail("Authentication is required."));
            return Task.CompletedTask;
        }

        var missing = required.Where(p => !_currentUser.HasPermission(p)).ToArray();
        if (missing.Length > 0)
        {
            context.Result = new ObjectResult(ApiResponse.Fail(
                $"You do not have permission to perform this action. Required: {string.Join(", ", missing)}."))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        return Task.CompletedTask;
    }
}
