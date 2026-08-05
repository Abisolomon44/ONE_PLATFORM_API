namespace ONEERP.ERP.API.Models;

public class Company
{
    public int Id { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Abbreviation { get; set; }
    public int BusinessTypeId { get; set; }
    public int IndustryTypeId { get; set; }
    public int? GSTRegistrationTypeId { get; set; }
    public string? GSTNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? TANNumber { get; set; }
    public string? CINNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public int CurrencyId { get; set; }
    public int LanguageId { get; set; }
    public int TimeZoneId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsBlocked { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime? LastLoginDate { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
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

public class Branch
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string BranchCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public int? BranchTypeId { get; set; }
    public int? ParentBranchId { get; set; }
    public int? ManagerEmployeeId { get; set; }
    public int? DefaultWarehouseId { get; set; }
    public string? GSTNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public bool IsHeadOffice { get; set; }
    public bool IsSalesBranch { get; set; }
    public bool IsPurchaseBranch { get; set; }
    public bool IsServiceBranch { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsDeleted { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class Department
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int BranchId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public int? ParentDepartmentId { get; set; }
    public int? ManagerEmployeeId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsDeleted { get; set; }
    public string? Remarks { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class Designation
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int? DepartmentId { get; set; }
    public string DesignationCode { get; set; } = string.Empty;
    public string DesignationName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public int? LevelNo { get; set; }
    public string? Grade { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsDeleted { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class Employee
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string? EmployeeNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public int? GenderId { get; set; }
    public int? MaritalStatusId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime DateOfJoining { get; set; }
    public DateTime? DateOfLeaving { get; set; }
    public string? OfficialEmail { get; set; }
    public string? PersonalEmail { get; set; }
    public string? MobileNo { get; set; }
    public string? AlternateMobileNo { get; set; }
    public int? ReportingManagerId { get; set; }
    public int? EmploymentTypeId { get; set; }
    public bool IsActive { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsDeleted { get; set; }
    public string? Remarks { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class Warehouse
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public int? WarehouseTypeId { get; set; }
    public int? ParentWarehouseId { get; set; }
    public int? ManagerEmployeeId { get; set; }
    public bool AllowNegativeStock { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public string? Remarks { get; set; }
    public bool IsActive { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsDeleted { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
