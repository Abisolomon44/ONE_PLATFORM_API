namespace ONEERP.ERP.API.DTOs;

public record CreateRoleRequest(string Name, string Code, string? Description, List<string>? Permissions = null);

public record UpdateRoleRequest(string Name, string? Description, bool IsActive, List<string>? Permissions = null);

public record SetRolePermissionsRequest(List<string> PermissionCodes);

public record CreateCompanyRequest(
    string CompanyCode,
    string CompanyName,
    string? ShortName,
    string? Abbreviation,
    int BusinessTypeId,
    int IndustryTypeId,
    int? GSTRegistrationTypeId,
    string? GSTNumber,
    string? PANNumber,
    string? TANNumber,
    string? CINNumber,
    string? RegistrationNumber,
    int CurrencyId,
    int LanguageId,
    int TimeZoneId);

public record UpdateCompanyRequest(
    string CompanyName,
    string? ShortName,
    string? Abbreviation,
    int BusinessTypeId,
    int IndustryTypeId,
    int? GSTRegistrationTypeId,
    string? GSTNumber,
    string? PANNumber,
    string? TANNumber,
    string? CINNumber,
    string? RegistrationNumber,
    int CurrencyId,
    int LanguageId,
    int TimeZoneId,
    bool IsActive,
    bool IsBlocked);

public record UpdateSettingsRequest(Dictionary<string, string> Settings);

public class PermissionDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}

public class BranchDto
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

public record CreateBranchRequest(
    int CompanyId,
    string BranchCode,
    string BranchName,
    string? ShortName,
    int? BranchTypeId,
    int? ParentBranchId,
    int? ManagerEmployeeId,
    int? DefaultWarehouseId,
    string? GSTNumber,
    string? RegistrationNumber,
    bool IsHeadOffice,
    bool IsSalesBranch,
    bool IsPurchaseBranch,
    bool IsServiceBranch,
    int SortOrder,
    bool IsActive);

public record UpdateBranchRequest(
    string BranchCode,
    string BranchName,
    string? ShortName,
    int? BranchTypeId,
    int? ParentBranchId,
    int? ManagerEmployeeId,
    int? DefaultWarehouseId,
    string? GSTNumber,
    string? RegistrationNumber,
    bool IsHeadOffice,
    bool IsSalesBranch,
    bool IsPurchaseBranch,
    bool IsServiceBranch,
    int SortOrder,
    bool IsActive,
    bool IsBlocked);

public class DepartmentDto
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

public record CreateDepartmentRequest(
    int CompanyId,
    int BranchId,
    string DepartmentCode,
    string DepartmentName,
    string? ShortName,
    int? ParentDepartmentId,
    int? ManagerEmployeeId,
    int SortOrder,
    bool IsActive,
    string? Remarks);

public record UpdateDepartmentRequest(
    string DepartmentCode,
    string DepartmentName,
    string? ShortName,
    int? ParentDepartmentId,
    int? ManagerEmployeeId,
    int SortOrder,
    bool IsActive,
    bool IsBlocked,
    string? Remarks);

public class DesignationDto
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

public record CreateDesignationRequest(
    int CompanyId,
    int? DepartmentId,
    string DesignationCode,
    string DesignationName,
    string? ShortName,
    int? LevelNo,
    string? Grade,
    string? Description,
    int SortOrder,
    bool IsDefault,
    bool IsActive);

public record UpdateDesignationRequest(
    string DesignationCode,
    string DesignationName,
    string? ShortName,
    int? LevelNo,
    string? Grade,
    string? Description,
    int SortOrder,
    bool IsDefault,
    bool IsActive,
    bool IsBlocked);

public class EmployeeDto
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

public record CreateEmployeeRequest(
    int CompanyId,
    int? BranchId,
    int? DepartmentId,
    int? DesignationId,
    string EmployeeCode,
    string? EmployeeNumber,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? DisplayName,
    int? GenderId,
    int? MaritalStatusId,
    DateTime? DateOfBirth,
    DateTime DateOfJoining,
    DateTime? DateOfLeaving,
    string? OfficialEmail,
    string? PersonalEmail,
    string? MobileNo,
    string? AlternateMobileNo,
    int? ReportingManagerId,
    int? EmploymentTypeId,
    bool IsActive,
    string? Remarks);

public record UpdateEmployeeRequest(
    string EmployeeCode,
    string? EmployeeNumber,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? DisplayName,
    int? GenderId,
    int? MaritalStatusId,
    DateTime? DateOfBirth,
    DateTime DateOfJoining,
    DateTime? DateOfLeaving,
    string? OfficialEmail,
    string? PersonalEmail,
    string? MobileNo,
    string? AlternateMobileNo,
    int? ReportingManagerId,
    int? EmploymentTypeId,
    bool IsActive,
    bool IsBlocked,
    string? Remarks);

public class WarehouseDto
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

public record CreateWarehouseRequest(
    int CompanyId,
    int? BranchId,
    string WarehouseCode,
    string WarehouseName,
    string? ShortName,
    int? WarehouseTypeId,
    int? ParentWarehouseId,
    int? ManagerEmployeeId,
    bool AllowNegativeStock,
    bool IsDefault,
    int SortOrder,
    string? Remarks,
    bool IsActive);

public record UpdateWarehouseRequest(
    string WarehouseCode,
    string WarehouseName,
    string? ShortName,
    int? WarehouseTypeId,
    int? ParentWarehouseId,
    int? ManagerEmployeeId,
    bool AllowNegativeStock,
    bool IsDefault,
    int SortOrder,
    string? Remarks,
    bool IsActive,
    bool IsBlocked);
