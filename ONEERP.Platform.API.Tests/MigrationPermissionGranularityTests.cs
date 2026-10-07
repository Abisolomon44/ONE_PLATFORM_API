using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using ONEERP.Platform.API.Controllers;
using ONEERP.Platform.API.Services;
using ONEERP.Shared.Constants;
using Xunit;
using SharedClaimTypes = ONEERP.Shared.Constants.ClaimTypes;

namespace ONEERP.Platform.API.Tests;

/// <summary>
/// Real permission granularity: the declared permission is evaluated against
/// the <c>permission</c> claims carried by the caller, so a read-only
/// platform account can inspect migration status but cannot execute a run.
/// </summary>
public class MigrationPermissionGranularityTests
{
    private static MigrationAuthorizationFilter CreateFilter(HttpContext httpContext) =>
        new(new CurrentUser(new HttpContextAccessor { HttpContext = httpContext }));

    private static AuthorizationFilterContext BuildContext(
        HttpContext httpContext,
        params MigrationAuthorizationAttribute[] attributes)
    {
        httpContext.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(attributes),
            "test"));

        return new AuthorizationFilterContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>());
    }

    /// <summary>An authenticated non-admin caller holding the given permission codes.</summary>
    private static HttpContext AsNonAdminWith(params string[] permissions)
    {
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(System.Security.Claims.ClaimTypes.Name, "auditor"));
        identity.AddClaim(new Claim(System.Security.Claims.ClaimTypes.Role, "MigrationAuditor"));
        foreach (var permission in permissions)
            identity.AddClaim(new Claim(SharedClaimTypes.Permission, permission));
        return new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
    }

    private static async Task<int?> StatusForAsync(
        HttpContext httpContext,
        params MigrationAuthorizationAttribute[] attributes)
    {
        var context = BuildContext(httpContext, attributes);
        await CreateFilter(httpContext).OnAuthorizationAsync(context);
        return context.Result switch
        {
            null => null,
            ObjectResult result => result.StatusCode,
            _ => -1
        };
    }

    /* ---------------- read-only access ---------------- */

    [Fact]
    public async Task ViewOnlyCallerMayReadStatus()
    {
        var httpContext = AsNonAdminWith(MigrationPermissions.View);

        var status = await StatusForAsync(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.View));

        status.Should().BeNull();
    }

    [Fact]
    public async Task ViewOnlyCallerMayReadHistory()
    {
        var httpContext = AsNonAdminWith(MigrationPermissions.View);

        var status = await StatusForAsync(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.View));

        status.Should().BeNull();
    }

    [Fact]
    public async Task ViewOnlyCallerIsForbiddenFromRunning()
    {
        var httpContext = AsNonAdminWith(MigrationPermissions.View);

        var status = await StatusForAsync(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.Run));

        status.Should().Be(StatusCodes.Status403Forbidden);
    }

    /* ---------------- run permission is independent of view ---------------- */

    [Fact]
    public async Task RunOnlyCallerMayRun()
    {
        var httpContext = AsNonAdminWith(MigrationPermissions.Run);

        (await StatusForAsync(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.Run)))
            .Should().BeNull();
    }

    [Fact]
    public async Task RunOnlyCallerCannotReadStatus()
    {
        // Granularity is real: the two permissions are not interchangeable.
        var httpContext = AsNonAdminWith(MigrationPermissions.Run);

        var status = await StatusForAsync(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.View));

        status.Should().Be(StatusCodes.Status403Forbidden);
    }

    /* ---------------- a caller needs every declared permission ---------------- */

    [Fact]
    public async Task RunEndpointNeedsViewAndRunWhenBothAreDeclared()
    {
        var viewOnly = AsNonAdminWith(MigrationPermissions.View);

        var denied = await StatusForAsync(
            viewOnly,
            new MigrationAuthorizationAttribute(MigrationPermissions.View),
            new MigrationAuthorizationAttribute(MigrationPermissions.Run));

        denied.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task RunEndpointIsAllowedWhenBothPermissionsAreHeld()
    {
        var operatorContext = AsNonAdminWith(MigrationPermissions.View, MigrationPermissions.Run);

        var allowed = await StatusForAsync(
            operatorContext,
            new MigrationAuthorizationAttribute(MigrationPermissions.View),
            new MigrationAuthorizationAttribute(MigrationPermissions.Run));

        allowed.Should().BeNull();
    }

    /* ---------------- wildcard and claim parsing ---------------- */

    [Fact]
    public async Task WildcardClaimGrantsEveryPlatformPermission()
    {
        var httpContext = AsNonAdminWith(MigrationPermissions.Wildcard);

        (await StatusForAsync(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.Run)))
            .Should().BeNull();
    }

    [Fact]
    public async Task CommaSeparatedPermissionsInOneClaimAreHonoured()
    {
        var httpContext = AsNonAdminWith($"{MigrationPermissions.View},{MigrationPermissions.Run}");

        (await StatusForAsync(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.Run)))
            .Should().BeNull();
    }

    [Fact]
    public async Task PermissionMatchingIsCaseInsensitive()
    {
        var httpContext = AsNonAdminWith(MigrationPermissions.Run.ToUpperInvariant());

        (await StatusForAsync(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.Run)))
            .Should().BeNull();
    }

    [Fact]
    public async Task UnrelatedPermissionDoesNotGrantMigrationAccess()
    {
        var httpContext = AsNonAdminWith("tenants.view", "settings.edit");

        (await StatusForAsync(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.View)))
            .Should().Be(StatusCodes.Status403Forbidden);
    }

    /* ---------------- CurrentUser surface ---------------- */

    [Fact]
    public void CurrentUserExposesThePermissionClaims()
    {
        var currentUser = new CurrentUser(new HttpContextAccessor
        {
            HttpContext = AsNonAdminWith(MigrationPermissions.View, MigrationPermissions.Run)
        });

        currentUser.Permissions.Should().BeEquivalentTo(
            new[] { MigrationPermissions.View, MigrationPermissions.Run });
    }

    [Fact]
    public void CurrentUserHasNoPermissionsWhenUnauthenticated()
    {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(SharedClaimTypes.Permission, MigrationPermissions.Wildcard));

        var currentUser = new CurrentUser(new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        });

        currentUser.Permissions.Should().BeEmpty();
        currentUser.HasPermission(MigrationPermissions.Run).Should().BeFalse();
    }

    [Fact]
    public void BlankPermissionIsNeverGranted()
    {
        var currentUser = new CurrentUser(new HttpContextAccessor
        {
            HttpContext = AsNonAdminWith(MigrationPermissions.Wildcard)
        });

        currentUser.HasPermission("").Should().BeFalse();
        currentUser.HasPermission("   ").Should().BeFalse();
    }

    [Fact]
    public void AdminRoleHoldsEveryPermissionImplicitly()
    {
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(System.Security.Claims.ClaimTypes.Role, RoleNames.PlatformAdmin));

        var currentUser = new CurrentUser(new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        });

        currentUser.Permissions.Should().BeEmpty();
        currentUser.HasPermission(MigrationPermissions.Run).Should().BeTrue();
    }

    [Fact]
    public void NonAdminPermissionCheckDoesNotDependOnTheRoleName()
    {
        // A non-admin role that happens to be named "Admin" must not inherit grants.
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(System.Security.Claims.ClaimTypes.Role, "TenantAdmin"));

        var currentUser = new CurrentUser(new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        });

        currentUser.IsPlatformAdmin.Should().BeFalse();
        currentUser.HasPermission(MigrationPermissions.View).Should().BeFalse();
    }

    /* ---------------- catalogue integrity ---------------- */

    [Fact]
    public void WildcardIsNotAConcretePermission()
    {
        MigrationPermissions.All.Should().NotContain(MigrationPermissions.Wildcard);
        MigrationPermissions.All.Should().BeEquivalentTo(
            new[] { MigrationPermissions.View, MigrationPermissions.Run });
    }
}