namespace ONEERP.Shared.Constants;

public static class EntityStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
    public const string Suspended = "Suspended";
    public const string Locked = "Locked";
}

public static class SubscriptionStatus
{
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string Cancelled = "Cancelled";
}

public static class TenantStatus
{
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    public const string Expired = "Expired";
}

/// <summary>
/// Lifecycle statuses for a tenant database migration run and for each
/// individual migration execution recorded in TenantMigrationHistory.
/// Values match the CHECK constraint on dbo.TenantMigrationHistory.Status.
/// </summary>
public static class MigrationStatus
{
    public const string UpToDate = "UP_TO_DATE";
    public const string Pending = "PENDING";
    public const string Running = "RUNNING";
    public const string Success = "SUCCESS";
    public const string Failed = "FAILED";
}

/// <summary>
/// Platform-level permission codes for the high-privilege tenant migration
/// operations. Enforced server-side only; the Angular UI never decides
/// authorization.
/// </summary>
public static class MigrationPermissions
{
    public const string View = "migrations.view";
    public const string Run = "migrations.run";

    /// <summary>Wildcard grant meaning "every platform-console permission".</summary>
    public const string Wildcard = "*";

    /// <summary>Every platform-console permission, for role configuration and tests.</summary>
    public static readonly string[] All = { View, Run };
}

public static class ClaimTypes
{
    public const string TenantId = "tenant_id";
    public const string TenantCode = "tenant_code";
    public const string CompanyId = "company_id";
    public const string Permission = "permission";
    public const string PlatformRole = "platform_role";
    public const string IsSuperAdmin = "is_super_admin";
    public const string RoleId = "role_id";
    public const string PermissionVersion = "permission_version";
}
