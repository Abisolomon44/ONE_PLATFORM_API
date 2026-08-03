namespace ONEERP.ERP.API.Models;

public class Company
{
    public int CompanyId { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? GST { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}

public class User
{
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}

public class Role
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}

public class UserRole
{
    public int UserRoleId { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class RolePermission
{
    public int RolePermissionId { get; set; }
    public int RoleId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class ApplicationSetting
{
    public int SettingId { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string? SettingValue { get; set; }
    public string? Description { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class RefreshToken
{
    public long RefreshTokenId { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? RevokedDate { get; set; }
}

public class AuditLog
{
    public long AuditLogId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? PerformedBy { get; set; }
    public DateTime PerformedDate { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
}

public class BusinessType
{
    public int BusinessTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}

public class IndustryType
{
    public int IndustryTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}

public class CompanyGroup
{
    public int CompanyGroupId { get; set; }
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public int? ParentGroupId { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}

public class Country
{
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ISOCode2 { get; set; } = string.Empty;
    public string ISOCode3 { get; set; } = string.Empty;
    public string? PhoneCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Nationality { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}

public class State
{
    public int StateId { get; set; }
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string? GSTStateCode { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
    public string? CountryName { get; set; }
}

public class City
{
    public int CityId { get; set; }
    public int CountryId { get; set; }
    public int StateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
    public string? CountryName { get; set; }
    public string? StateName { get; set; }
}
