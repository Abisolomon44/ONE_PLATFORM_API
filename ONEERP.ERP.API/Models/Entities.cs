namespace ONEERP.ERP.API.Models;

public class Company
{
    public int Id { get; set; }
    public long? EntityId { get; set; }
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

    public int? CompanyGroupId { get; set; }
    public int? BusinessUnitId { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? LogoUrl { get; set; }
    public int? DefaultFinancialYearId { get; set; }
    public bool MultiBranchEnabled { get; set; } = true;
    public bool MultiWarehouseEnabled { get; set; } = true;
    public bool MultiCurrencyEnabled { get; set; } = false;
    public string? DateFormat { get; set; }
    public string? TimeFormat { get; set; }
    public string? NumberFormat { get; set; }
    public int? DefaultWarehouseId { get; set; }
    public string? Theme { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? Remarks { get; set; }
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

public class BusinessPartnerRole
{
    public int BusinessPartnerRoleId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}

public class BusinessPartner
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string PartnerCode { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public string PatnerRoleIds { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public string? TaxRegistrationNo { get; set; }
    public decimal CreditLimit { get; set; }
    public int CreditDays { get; set; }
    public int? PaymentTermId { get; set; }
    public int? CurrencyId { get; set; }
    public int? PriceListId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
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
    public long? EntityId { get; set; }
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
    public long? EntityId { get; set; }
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
    public long? EntityId { get; set; }
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

public class Store
{
    public int StoreId { get; set; }
    public long? EntityId { get; set; }
    public int CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string StoreCode { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? StoreType { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class FinancialYear
{
    public long FinancialYearId { get; set; }
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsClosed { get; set; }
    public bool IsActive { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class Counter
{
    public int CounterId { get; set; }
    public int StoreId { get; set; }
    public string CounterCode { get; set; } = string.Empty;
    public string CounterName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class POSSession
{
    public long POSSessionId { get; set; }
    public int CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? StoreId { get; set; }
    public string? StoreName { get; set; }
    public int? CounterId { get; set; }
    public string? CounterName { get; set; }
    public int? CashierUserId { get; set; }
    public string? CashierUserName { get; set; }
    public string SessionNumber { get; set; } = string.Empty;
    public decimal OpeningCash { get; set; }
    public decimal? ClosingCash { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public byte Status { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/* ---------------------------------------------------------------------------
    Permission System Entities
    --------------------------------------------------------------------------- */

public class PermissionModule
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public string Level { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
    public string? Icon { get; set; }
    public string? RoutePath { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}

public enum PermissionScope
{
    Platform,
    Tenant,
    Company,
    Branch,
    User
}

public class PermissionAction
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class ModulePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int PermissionModuleId { get; set; }
    public int PermissionActionId { get; set; }
    public string Scope { get; set; } = string.Empty;
    public int? ScopeId { get; set; }
    public string? GrantedBy { get; set; }
    public DateTime GrantedDate { get; set; }
    public bool IsRevoked { get; set; }
    public string? RevokedBy { get; set; }
    public DateTime? RevokedDate { get; set; }
}

public class FieldPermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int PermissionModuleId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public bool CanView { get; set; } = true;
    public bool CanEdit { get; set; } = true;
    public bool IsMandatory { get; set; }
    public bool IsHidden { get; set; }
    public string Scope { get; set; } = string.Empty;
    public int? ScopeId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

/* ---------------------------------------------------------------------------
    Enterprise Permission Engine Entities
    --------------------------------------------------------------------------- */

public class Workspace
{
    public int Id { get; set; }
    public string WorkspaceCode { get; set; } = string.Empty;
    public string WorkspaceName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class Domain
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string DomainCode { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class Module
{
    public int Id { get; set; }
    public int DomainId { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? RouteUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class SubModule
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public string SubModuleCode { get; set; } = string.Empty;
    public string SubModuleName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? RouteUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class Screen
{
    public int Id { get; set; }
    public int SubModuleId { get; set; }
    public string ScreenCode { get; set; } = string.Empty;
    public string ScreenName { get; set; } = string.Empty;
    public string? PermissionCode { get; set; }
    public string ScreenType { get; set; } = "MASTER";
    public string? RouteUrl { get; set; }
    public string? ComponentName { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class Field
{
    public int Id { get; set; }
    public int ScreenId { get; set; }
    public string FieldCode { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = "text";
    public int DisplayOrder { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsSystemField { get; set; }
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class PermissionActionEntry
{
    public int Id { get; set; }
    public string ActionCode { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class RolePermissionEntry
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int WorkspaceId { get; set; }
    public int DomainId { get; set; }
    public int ModuleId { get; set; }
    public int SubModuleId { get; set; }
    public int ScreenId { get; set; }
    public int ActionId { get; set; }
    public bool Allow { get; set; } = true;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class UserPermissionOverride
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int WorkspaceId { get; set; }
    public int DomainId { get; set; }
    public int ModuleId { get; set; }
    public int SubModuleId { get; set; }
    public int ScreenId { get; set; }
    public int ActionId { get; set; }
    public string PermissionType { get; set; } = "Grant";
    public bool Allow { get; set; } = true;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Remarks { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class RoleFieldPermissionEntry
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int ScreenId { get; set; }
    public int FieldId { get; set; }
    public bool CanView { get; set; } = true;
    public bool CanEdit { get; set; } = true;
    public bool IsHidden { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsMandatory { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class UserFieldPermissionEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ScreenId { get; set; }
    public int FieldId { get; set; }
    public bool CanView { get; set; } = true;
    public bool CanEdit { get; set; } = true;
    public bool IsHidden { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class DataScope
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int? ModuleId { get; set; }
    public int? ScreenId { get; set; }
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public int? WarehouseId { get; set; }
    public int? BusinessUnitId { get; set; }
    public int? CostCenterId { get; set; }
    public int? ProfitCenterId { get; set; }
    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class UserDataScopeOverride
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ModuleId { get; set; }
    public int? ScreenId { get; set; }
    public string ScopeType { get; set; } = string.Empty;   // Company, Branch, Department, Warehouse, etc.
    public string ScopeValue { get; set; } = string.Empty;   // The ID value as string
    public string PermissionType { get; set; } = string.Empty; // Grant, Deny
    public bool Allow { get; set; }
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }
    public string? Remarks { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class WorkflowPermissionEntry
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int ModuleId { get; set; }
    public int SubModuleId { get; set; }
    public int ScreenId { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
    public bool CanCancel { get; set; }
    public bool CanClose { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
}
