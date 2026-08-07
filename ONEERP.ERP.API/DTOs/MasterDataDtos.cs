namespace ONEERP.ERP.API.DTOs;

/* ---------------- BusinessTypes ---------------- */

public record CreateBusinessTypeRequest(string Name, string? Description, int SortOrder = 1);

public record UpdateBusinessTypeRequest(string Name, string? Description, int SortOrder, bool IsActive);

public class BusinessTypeDto
{
    public int BusinessTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- IndustryTypes ---------------- */

public record CreateIndustryTypeRequest(string Name, string? Description, int SortOrder = 1);

public record UpdateIndustryTypeRequest(string Name, string? Description, int SortOrder, bool IsActive);

public class IndustryTypeDto
{
    public int IndustryTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- CompanyGroups ---------------- */

public record CreateCompanyGroupRequest(
    string GroupCode,
    string GroupName,
    string? ShortName,
    string? Description,
    int? ParentGroupId = null);

public record UpdateCompanyGroupRequest(
    string GroupCode,
    string GroupName,
    string? ShortName,
    string? Description,
    int? ParentGroupId,
    bool IsActive);

public class CompanyGroupDto
{
    public int CompanyGroupId { get; set; }
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public int? ParentGroupId { get; set; }
    public string? ParentGroupName { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- Permission Modules ---------------- */

public record CreatePermissionModuleRequest(
    string Code,
    string Name,
    int? ParentId,
    string Level,
    string? Icon = null,
    string? RoutePath = null,
    int SortOrder = 0,
    bool IsVisible = true);

public record UpdatePermissionModuleRequest(
    string Code,
    string Name,
    int? ParentId,
    string Level,
    int SortOrder,
    bool IsVisible,
    string? Icon,
    string? RoutePath);

public class PermissionModuleDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string Level { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; }
    public string? Icon { get; set; }
    public string? RoutePath { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

/* ---------------- Permission Actions ---------------- */

public record CreatePermissionActionRequest(
    string Code,
    string Name,
    int SortOrder = 0,
    bool IsActive = true);

public record UpdatePermissionActionRequest(
    string Code,
    string Name,
    int SortOrder,
    bool IsActive);

public class PermissionActionDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- Module Permissions ---------------- */

public record AssignModulePermissionRequest(
    int RoleId,
    int PermissionModuleId,
    int PermissionActionId,
    string Scope,
    int? ScopeId = null);

public record RevokeModulePermissionRequest(
    int RoleId,
    int PermissionModuleId,
    int PermissionActionId,
    string Scope,
    int? ScopeId = null);

public class ModulePermissionDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public int PermissionModuleId { get; set; }
    public string? ModuleName { get; set; }
    public int PermissionActionId { get; set; }
    public string? ActionName { get; set; }
    public string Scope { get; set; } = string.Empty;
    public int? ScopeId { get; set; }
    public string? GrantedBy { get; set; }
    public DateTime GrantedDate { get; set; }
    public bool IsRevoked { get; set; }
}

/* ---------------- Field Permissions ---------------- */

public record SetFieldPermissionRequest(
    int RoleId,
    int PermissionModuleId,
    string FieldName,
    bool CanView,
    bool CanEdit,
    bool IsMandatory = false,
    bool IsHidden = false,
    string Scope = "Company",
    int? ScopeId = null);

public class FieldPermissionDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int PermissionModuleId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsHidden { get; set; }
    public string Scope { get; set; } = string.Empty;
    public int? ScopeId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

/* ---------------- User Permission for UI ---------------- */

public class UserModulePermissionDto
{
    public string Code { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public int? ScopeId { get; set; }
}
