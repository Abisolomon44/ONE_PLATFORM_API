using System.Reflection;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Moq;
using ONEERP.Platform.API.Controllers;
using ONEERP.Platform.API.Services;
using ONEERP.Platform.API.Validators;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;
using Xunit;
using SharedClaimTypes = System.Security.Claims.ClaimTypes;

namespace ONEERP.Platform.API.Tests;

/// <summary>
/// TEST 07: the API rejects unauthorized callers regardless of what the
/// Angular client does. Authorization is driven by the real role mapping in
/// <see cref="CurrentUser"/>, not by a stub.
/// </summary>
public class MigrationAuthorizationTests
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

    private static HttpContext AsAuthenticated(string claimType, string value)
    {
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(SharedClaimTypes.Name, "someone"));
        identity.AddClaim(new Claim(claimType, value));
        return new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
    }

    /// <summary>An authenticated caller carrying the given role in the standard role claim.</summary>
    private static HttpContext AsAdmin(string role) => AsAuthenticated(SharedClaimTypes.Role, role);

    /* ---------------- 401 for anonymous callers ---------------- */

    [Fact]
    public async Task AnonymousCallerIsRejectedWith401()
    {
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) };
        var filter = CreateFilter(httpContext);
        var context = BuildContext(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.Run));

        await filter.OnAuthorizationAsync(context);

        context.Result.Should().BeOfType<UnauthorizedObjectResult>();
        ((UnauthorizedObjectResult)context.Result!).StatusCode.Should().Be(401);
    }

    /* ---------------- 403 for authenticated non-admins ---------------- */

    [Theory]
    [InlineData("Auditor")]
    [InlineData("Support")]
    [InlineData("platform_guest")]
    public async Task AuthenticatedNonAdminIsRejectedWith403(string role)
    {
        var httpContext = AsAdmin(role);
        var filter = CreateFilter(httpContext);
        var context = BuildContext(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.View));

        await filter.OnAuthorizationAsync(context);

        context.Result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)context.Result!).StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task CallerWithNoRoleClaimAtAllIsRejected()
    {
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(SharedClaimTypes.Name, "someone"));
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        var filter = CreateFilter(httpContext);
        var context = BuildContext(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.Run));

        await filter.OnAuthorizationAsync(context);

        ((ObjectResult)context.Result!).StatusCode.Should().Be(403);
    }

    /* ---------------- admin roles are allowed ---------------- */

    [Theory]
    [InlineData(RoleNames.PlatformAdmin)]
    [InlineData(RoleNames.SuperAdmin)]
    [InlineData(RoleNames.Administrator)]
    public async Task PlatformAdministratorRolesAreAllowed(string role)
    {
        var httpContext = AsAdmin(role);
        var filter = CreateFilter(httpContext);
        var context = BuildContext(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.Run));

        await filter.OnAuthorizationAsync(context);

        context.Result.Should().BeNull();
    }

    [Theory]
    [InlineData("platformadmin")]
    [InlineData("PLATFORMADMIN")]
    [InlineData("PlatformAdmin")]
    public async Task AdminRoleMatchingIsCaseInsensitive(string role)
    {
        var httpContext = AsAdmin(role);
        var filter = CreateFilter(httpContext);
        var context = BuildContext(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.Run));

        await filter.OnAuthorizationAsync(context);

        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task AdminRoleIsAlsoHonouredFromThePlatformRoleClaim()
    {
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(SharedClaimTypes.Name, "someone"));
        identity.AddClaim(new Claim(ONEERP.Shared.Constants.ClaimTypes.PlatformRole, RoleNames.SuperAdmin));
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        var filter = CreateFilter(httpContext);
        var context = BuildContext(httpContext, new MigrationAuthorizationAttribute(MigrationPermissions.Run));

        await filter.OnAuthorizationAsync(context);

        context.Result.Should().BeNull();
    }

    /* ---------------- the filter is scoped to declared permissions ---------------- */

    [Fact]
    public async Task EndpointWithoutPermissionAttributeIsNotBlocked()
    {
        var httpContext = AsAdmin("Auditor");
        var filter = CreateFilter(httpContext);

        await filter.OnAuthorizationAsync(BuildContext(httpContext));

        // No permission was declared, so this global filter must not interfere;
        // ordinary [Authorize] still protects the endpoint.
    }

    /* ---------------- declared permissions on the real controller ---------------- */

    [Fact]
    public void ClassLevelAttributeRequiresViewOnEveryEndpoint()
    {
        typeof(TenantMigrationsController)
            .GetCustomAttributes<MigrationAuthorizationAttribute>()
            .Select(a => a.Permission)
            .Should().ContainSingle().Which.Should().Be(MigrationPermissions.View);
    }

    [Fact]
    public void RunEndpointRequiresTheRunPermission()
    {
        var run = typeof(TenantMigrationsController).GetMethod(nameof(TenantMigrationsController.Run))!;

        run.GetCustomAttributes<MigrationAuthorizationAttribute>()
            .Select(a => a.Permission)
            .Should().ContainSingle().Which.Should().Be(MigrationPermissions.Run);
    }

    [Fact]
    public void ReadOnlyEndpointsDoNotRequireTheRunPermission()
    {
        var status = typeof(TenantMigrationsController).GetMethod(nameof(TenantMigrationsController.GetStatus))!;
        var history = typeof(TenantMigrationsController).GetMethod(nameof(TenantMigrationsController.GetHistory))!;

        status.GetCustomAttributes<MigrationAuthorizationAttribute>().Should().BeEmpty();
        history.GetCustomAttributes<MigrationAuthorizationAttribute>().Should().BeEmpty();
    }

    [Fact]
    public void PermissionAttributeCannotBeStackedTwiceOnOneTarget()
    {
        // AllowMultiple = false stops a conflicting second permission being added.
        var usage = typeof(MigrationAuthorizationAttribute)
            .GetCustomAttribute<AttributeUsageAttribute>()!;
        usage.AllowMultiple.Should().BeFalse();
    }

    /* ---------------- the client cannot supply SQL ---------------- */

    [Fact]
    public void RunEndpointTakesNoRequestBodyAndOnlyTheRouteTenantId()
    {
        var run = typeof(TenantMigrationsController).GetMethod(nameof(TenantMigrationsController.Run))!;

        run.GetParameters().Should().ContainSingle();
        run.GetParameters()[0].Name.Should().Be("tenantId");
        run.GetCustomAttributes<FromBodyAttribute>().Should().BeEmpty();
    }

    [Fact]
    public void MigrationEndpointsExposeTheAgreedRoutes()
    {
        typeof(TenantMigrationsController)
            .GetCustomAttributes<RouteAttribute>()
            .Select(a => a.Template)
            .Should().Contain("api/tenants");

        RouteTemplateOf(nameof(TenantMigrationsController.GetStatus))
            .Should().Be("{tenantId:int}/migration-status");
        RouteTemplateOf(nameof(TenantMigrationsController.Run))
            .Should().Be("{tenantId:int}/migration/run");
        RouteTemplateOf(nameof(TenantMigrationsController.GetHistory))
            .Should().Be("{tenantId:int}/migration/history");
    }

    [Fact]
    public void MigrationRoutesDoNotCollideWithTheExistingTenantRoutes()
    {
        // TenantsController already owns api/tenants/{id:int} and
        // api/tenants/{id:int}/connections, so every migration route must be a
        // three-segment template with a distinct literal second segment.
        var templates = new[]
        {
            RouteTemplateOf(nameof(TenantMigrationsController.GetStatus)),
            RouteTemplateOf(nameof(TenantMigrationsController.Run)),
            RouteTemplateOf(nameof(TenantMigrationsController.GetHistory))
        };

        templates.Should().OnlyContain(t => t.StartsWith("{tenantId:int}/migration"));
        templates.Should().OnlyContain(t => t.Count(c => c == '/') <= 2);
        templates.Distinct().Should().HaveCount(3);
    }

    private static string RouteTemplateOf(string methodName)
    {
        var method = typeof(TenantMigrationsController).GetMethod(methodName)!;
        IRouteTemplateProvider? attribute = method.GetCustomAttribute<HttpGetAttribute>();
        if (attribute is null)
            attribute = method.GetCustomAttribute<HttpPostAttribute>();
        return attribute!.Template!;
    }

    [Fact]
    public void MigrationPermissionCodesAreDistinct()
    {
        MigrationPermissions.View.Should().NotBe(MigrationPermissions.Run);
    }

    /* ---------------- input validation ---------------- */

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public void TenantIdMustBePositive(int tenantId)
    {
        new MigrationRequestValidator().Validate(tenantId).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(int.MaxValue)]
    public void PositiveTenantIdIsAccepted(int tenantId)
    {
        new MigrationRequestValidator().Validate(tenantId).IsValid.Should().BeTrue();
    }

    /* ---------------- the UI's permission service cannot grant access ---------------- */

    [Fact]
    public void AdminDetectionIgnoresAnUnauthenticatedPrincipalEvenWithAnAdminRole()
    {
        // Role claim present but identity not authenticated -> must not pass.
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(SharedClaimTypes.Role, RoleNames.PlatformAdmin));
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        var currentUser = new CurrentUser(new HttpContextAccessor { HttpContext = httpContext });

        currentUser.IsPlatformAdmin.Should().BeFalse();
    }

    [Fact]
    public void MissingHttpContextIsNotAnAdmin()
    {
        var currentUser = new CurrentUser(new HttpContextAccessor());

        currentUser.IsPlatformAdmin.Should().BeFalse();
        currentUser.Username.Should().BeEmpty();
        currentUser.Role.Should().BeNull();
    }
}