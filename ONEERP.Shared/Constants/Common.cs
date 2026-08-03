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

public static class ClaimTypes
{
    public const string TenantId = "tenant_id";
    public const string TenantCode = "tenant_code";
    public const string CompanyId = "company_id";
    public const string Permission = "permission";
    public const string PlatformRole = "platform_role";
}
