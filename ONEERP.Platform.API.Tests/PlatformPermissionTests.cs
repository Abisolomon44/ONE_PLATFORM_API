using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ONEERP.Platform.API.Models;
using ONEERP.Platform.API.Services;
using ONEERP.Shared.Constants;
using Xunit;
using SharedClaimTypes = ONEERP.Shared.Constants.ClaimTypes;
using SysClaimTypes = System.Security.Claims.ClaimTypes;

namespace ONEERP.Platform.API.Tests;

/// <summary>
/// Role-to-permission mapping and the claims actually written into the access
/// token. This is what makes server-side permission enforcement possible.
/// </summary>
public class PlatformPermissionTests
{
    private const string SigningKey = "ONEERP_TEST_SIGNING_KEY_THAT_IS_LONG_ENOUGH_0123456789";

    private static IConfiguration Config(params string[] permissionGrants)
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "ONEERP.Platform",
            ["Jwt:Audience"] = "ONEERP.Platform.API",
            ["Jwt:Key"] = SigningKey,
            ["Jwt:AccessTokenMinutes"] = "15"
        };

        for (var i = 0; i + 1 < permissionGrants.Length; i += 2)
            values[$"PlatformPermissions:Roles:{permissionGrants[i]}"] = permissionGrants[i + 1];

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private static string[] PermissionsIn(string accessToken) =>
        new JwtSecurityTokenHandler().ReadJwtToken(accessToken).Claims
            .Where(c => c.Type == SharedClaimTypes.Permission)
            .Select(c => c.Value)
            .ToArray();

    private static PlatformUser UserWithRole(string role) => new()
    {
        PlatformUserId = 1,
        Username = "someone",
        FullName = "Some One",
        Role = role
    };

    /* ---------------- resolver ---------------- */

    [Theory]
    [InlineData(RoleNames.PlatformAdmin)]
    [InlineData(RoleNames.SuperAdmin)]
    [InlineData(RoleNames.Administrator)]
    public void AdminRolesResolveToTheWildcard(string role)
    {
        new PlatformPermissionResolver(Config()).Resolve(role)
            .Should().BeEquivalentTo(new[] { MigrationPermissions.Wildcard });
    }

    [Fact]
    public void ConfiguredRoleResolvesToItsGrantedPermissions()
    {
        var resolver = new PlatformPermissionResolver(Config(
            "MigrationOperator", "migrations.view, migrations.run"));

        resolver.Resolve("MigrationOperator").Should().BeEquivalentTo(
            new[] { MigrationPermissions.View, MigrationPermissions.Run });
    }

    [Fact]
    public void ReadOnlyRoleGrantsOnlyView()
    {
        var resolver = new PlatformPermissionResolver(Config("MigrationAuditor", "migrations.view"));

        var granted = resolver.Resolve("MigrationAuditor").ToArray();

        granted.Should().ContainSingle().Which.Should().Be(MigrationPermissions.View);
        granted.Should().NotContain(MigrationPermissions.Run);
    }

    [Fact]
    public void UnconfiguredRoleGrantsNothing()
    {
        new PlatformPermissionResolver(Config()).Resolve("Support").Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankRoleGrantsNothing(string? role)
    {
        new PlatformPermissionResolver(Config()).Resolve(role).Should().BeEmpty();
    }

    [Fact]
    public void RoleLookupIsCaseInsensitive()
    {
        new PlatformPermissionResolver(Config("MigrationAuditor", "migrations.view"))
            .Resolve("migrationauditor").Should().ContainSingle();
    }

    /* ---------------- token claims ---------------- */

    [Fact]
    public void AccessTokenCarriesOneClaimPerGrantedPermission()
    {
        var token = new TokenService(Config(), new PlatformPermissionResolver(Config(
            "MigrationOperator", "migrations.view,migrations.run")))
            .GenerateAccessToken(UserWithRole("MigrationOperator"));

        PermissionsIn(token).Should().BeEquivalentTo(
            new[] { MigrationPermissions.View, MigrationPermissions.Run });
    }

    [Fact]
    public void ReadOnlyAccessTokenCannotGrantRun()
    {
        var token = new TokenService(Config(), new PlatformPermissionResolver(Config(
            "MigrationAuditor", "migrations.view")))
            .GenerateAccessToken(UserWithRole("MigrationAuditor"));

        var permissions = PermissionsIn(token);

        permissions.Should().ContainSingle().Which.Should().Be(MigrationPermissions.View);
        permissions.Should().NotContain(MigrationPermissions.Run);
    }

    [Fact]
    public void AdministratorAccessTokenCarriesTheWildcard()
    {
        var token = new TokenService(Config(), new PlatformPermissionResolver(Config()))
            .GenerateAccessToken(UserWithRole(RoleNames.PlatformAdmin));

        PermissionsIn(token).Should().BeEquivalentTo(new[] { MigrationPermissions.Wildcard });
    }

    [Fact]
    public void UnconfiguredRoleProducesATokenWithNoPermissions()
    {
        var token = new TokenService(Config(), new PlatformPermissionResolver(Config()))
            .GenerateAccessToken(UserWithRole("Support"));

        PermissionsIn(token).Should().BeEmpty();
    }

    [Fact]
    public void AccessTokenStillCarriesIdentityAndRoleClaims()
    {
        var token = new TokenService(Config(), new PlatformPermissionResolver(Config(
            "MigrationAuditor", "migrations.view")))
            .GenerateAccessToken(UserWithRole("MigrationAuditor"));

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        parsed.Claims.Should().Contain(c => c.Type == SysClaimTypes.NameIdentifier && c.Value == "1");
        parsed.Claims.Should().Contain(c => c.Type == SysClaimTypes.Role && c.Value == "MigrationAuditor");
        parsed.Claims.Should().Contain(c => c.Type == SharedClaimTypes.PlatformRole);
    }

    /* ---------------- a token issued before this change fails closed ---------------- */

    [Fact]
    public void TokenWithoutPermissionClaimsGrantsNothingToANonAdmin()
    {
        // Simulates a token minted before the permissions claim existed.
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var legacy = new JwtSecurityToken(
            issuer: "ONEERP.Platform",
            audience: "ONEERP.Platform.API",
            claims: new[]
            {
                new Claim(SysClaimTypes.NameIdentifier, "1"),
                new Claim(SysClaimTypes.Role, "MigrationAuditor")
            },
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(legacy);

        PermissionsIn(accessToken).Should().BeEmpty();

        var claims = new JwtSecurityTokenHandler().ReadJwtToken(accessToken).Claims;
        var identity = new ClaimsIdentity(claims, "Test");
        var currentUser = new CurrentUser(
            new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            });

        currentUser.HasPermission(MigrationPermissions.View).Should().BeFalse();
    }
}