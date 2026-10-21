using ONEERP.Shared.Constants;

namespace ONEERP.Platform.API.Services;

/// <summary>
/// Maps a platform role to the permission codes granted to it. Grants are read
/// from configuration (<c>PlatformPermissions:Roles:{role}</c>) so a new
/// read-only role can be introduced without a code change. Platform
/// administrator roles always resolve to the wildcard grant.
/// </summary>
public interface IPlatformPermissionResolver
{
    /// <summary>Permission codes granted to <paramref name="role"/>, possibly empty.</summary>
    IReadOnlyList<string> Resolve(string? role);
}

public class PlatformPermissionResolver : IPlatformPermissionResolver
{
    private const string SectionName = "PlatformPermissions:Roles";

    private static readonly string[] AdminRoles =
    {
        RoleNames.PlatformAdmin,
        RoleNames.SuperAdmin,
        RoleNames.Administrator
    };

    private readonly IConfiguration _configuration;

    public PlatformPermissionResolver(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IReadOnlyList<string> Resolve(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return Array.Empty<string>();

        if (AdminRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            return new[] { MigrationPermissions.Wildcard };

        var configured = _configuration
            .GetSection($"{SectionName}:{role}")
            .Get<string[]>() ?? Array.Empty<string>();

        return configured
            .SelectMany(p => (p ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}