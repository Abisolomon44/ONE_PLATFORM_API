    namespace ONEERP.ERP.API.DTOs;

/* ---------------------------------------------------------------------------
   Workspace DTOs
   --------------------------------------------------------------------------- */
public class WorkspaceDto
{
    public int Id { get; set; }
    public string WorkspaceCode { get; set; } = string.Empty;
    public string WorkspaceName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record CreateWorkspaceRequest(string WorkspaceCode, string WorkspaceName, string? Icon, string? Route, int SortOrder, bool IsActive = true);
public record UpdateWorkspaceRequest(string WorkspaceCode, string WorkspaceName, string? Icon, string? Route, int SortOrder, bool IsActive);

/* ---------------------------------------------------------------------------
   Domain DTOs
   --------------------------------------------------------------------------- */
public class DomainDto
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string? WorkspaceName { get; set; }
    public string DomainCode { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record CreateDomainRequest(int WorkspaceId, string DomainCode, string DomainName, string? Icon, int SortOrder, bool IsActive = true);
public record UpdateDomainRequest(string DomainCode, string DomainName, string? Icon, int SortOrder, bool IsActive);

/* ---------------------------------------------------------------------------
   Module DTOs
   --------------------------------------------------------------------------- */
public class ModuleDto
{
    public int Id { get; set; }
    public int DomainId { get; set; }
    public string? DomainName { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? RouteUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record CreateModuleRequest(int DomainId, string ModuleCode, string ModuleName, string? Icon, string? RouteUrl, int SortOrder, bool IsActive = true);
public record UpdateModuleRequest(string ModuleCode, string ModuleName, string? Icon, string? RouteUrl, int SortOrder, bool IsActive);

/* ---------------------------------------------------------------------------
   SubModule DTOs
   --------------------------------------------------------------------------- */
public class SubModuleDto
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public string? ModuleName { get; set; }
    public string SubModuleCode { get; set; } = string.Empty;
    public string SubModuleName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? RouteUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record CreateSubModuleRequest(int ModuleId, string SubModuleCode, string SubModuleName, string? Icon, string? RouteUrl, int SortOrder, bool IsActive = true);
public record UpdateSubModuleRequest(string SubModuleCode, string SubModuleName, string? Icon, string? RouteUrl, int SortOrder, bool IsActive);

/* ---------------------------------------------------------------------------
   Screen DTOs
   --------------------------------------------------------------------------- */
public class ScreenDto
{
    public int Id { get; set; }
    public int SubModuleId { get; set; }
    public string? SubModuleName { get; set; }
    public string ScreenCode { get; set; } = string.Empty;
    public string ScreenName { get; set; } = string.Empty;
    public string? PermissionCode { get; set; }
    public string ScreenType { get; set; } = "MASTER";
    public string? RouteUrl { get; set; }
    public string? ComponentName { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record CreateScreenRequest(int SubModuleId, string ScreenCode, string ScreenName, string? PermissionCode, string ScreenType, string? RouteUrl, string? ComponentName, int SortOrder, bool IsActive = true);
public record UpdateScreenRequest(string ScreenCode, string ScreenName, string? PermissionCode, string ScreenType, string? RouteUrl, string? ComponentName, int SortOrder, bool IsActive);

/* ---------------------------------------------------------------------------
   Field DTOs
   --------------------------------------------------------------------------- */
public class FieldDto
{
    public int Id { get; set; }
    public int ScreenId { get; set; }
    public string? ScreenName { get; set; }
    public string FieldCode { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = "text";
    public int DisplayOrder { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsSystemField { get; set; }
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record CreateFieldRequest(int ScreenId, string FieldCode, string FieldName, string DisplayName, string DataType, int DisplayOrder, string? DefaultValue, bool IsSystemField, bool IsRequired, bool IsActive = true);
public record UpdateFieldRequest(string FieldCode, string FieldName, string DisplayName, string DataType, int DisplayOrder, string? DefaultValue, bool IsRequired, bool IsActive);

/* ---------------------------------------------------------------------------
   Action DTOs
   --------------------------------------------------------------------------- */
public class ActionDto
{
    public int Id { get; set; }
    public string ActionCode { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public record CreateActionRequest(string ActionCode, string ActionName, int DisplayOrder, bool IsActive = true);
public record UpdateActionRequest(string ActionCode, string ActionName, int DisplayOrder, bool IsActive);

/* ---------------------------------------------------------------------------
   RolePermissionEntry DTOs (new hierarchical)
   --------------------------------------------------------------------------- */
public class RolePermissionEntryDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public int WorkspaceId { get; set; }
    public string? WorkspaceName { get; set; }
    public int DomainId { get; set; }
    public string? DomainName { get; set; }
    public int ModuleId { get; set; }
    public string? ModuleName { get; set; }
    public int SubModuleId { get; set; }
    public string? SubModuleName { get; set; }
    public int ScreenId { get; set; }
    public string? ScreenName { get; set; }
    public int ActionId { get; set; }
    public string? ActionName { get; set; }
    public bool Allow { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record AssignRolePermissionRequest(int RoleId, int WorkspaceId, int DomainId, int ModuleId, int SubModuleId, int ScreenId, int ActionId, bool Allow = true, int DisplayOrder = 0);
public record BulkAssignRolePermissionRequest(int RoleId, List<RolePermissionItem> Permissions);
public record RolePermissionItem(int WorkspaceId, int DomainId, int ModuleId, int SubModuleId, int ScreenId, int ActionId, bool Allow = true);

/* ---------------------------------------------------------------------------
   UserPermissionOverride DTOs
   --------------------------------------------------------------------------- */
public class UserPermissionOverrideDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int WorkspaceId { get; set; }
    public string? WorkspaceName { get; set; }
    public int DomainId { get; set; }
    public string? DomainName { get; set; }
    public int ModuleId { get; set; }
    public string? ModuleName { get; set; }
    public int SubModuleId { get; set; }
    public string? SubModuleName { get; set; }
    public int ScreenId { get; set; }
    public string? ScreenName { get; set; }
    public int ActionId { get; set; }
    public string? ActionName { get; set; }
    public string PermissionType { get; set; } = "Grant";
    public bool Allow { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Remarks { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record CreateUserPermissionOverrideRequest(int UserId, int WorkspaceId, int DomainId, int ModuleId, int SubModuleId, int ScreenId, int ActionId, string PermissionType, bool Allow, DateTime? EffectiveFrom, DateTime? EffectiveTo, string? Remarks);
public record UpdateUserPermissionOverrideRequest(string PermissionType, bool Allow, DateTime? EffectiveFrom, DateTime? EffectiveTo, string? Remarks, bool IsActive);

/* ---------------------------------------------------------------------------
   RoleFieldPermissionEntry DTOs
   --------------------------------------------------------------------------- */
public class RoleFieldPermissionEntryDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public int ScreenId { get; set; }
    public string? ScreenName { get; set; }
    public int FieldId { get; set; }
    public string? FieldName { get; set; }
    public string? DisplayName { get; set; }
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool IsHidden { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsMandatory { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record SetRoleFieldPermissionRequest(int RoleId, int ScreenId, int FieldId, bool CanView, bool CanEdit, bool IsHidden, bool IsReadOnly, bool IsMandatory, int DisplayOrder = 0, bool IsActive = true);
public record BulkSetRoleFieldPermissionRequest(int RoleId, int ScreenId, List<RoleFieldPermissionItem> Fields);
public record RoleFieldPermissionItem(int FieldId, bool CanView, bool CanEdit, bool IsHidden, bool IsReadOnly, bool IsMandatory, int DisplayOrder = 0);

/* ---------------------------------------------------------------------------
   UserFieldPermissionEntry DTOs
   --------------------------------------------------------------------------- */
public class UserFieldPermissionEntryDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int ScreenId { get; set; }
    public string? ScreenName { get; set; }
    public int FieldId { get; set; }
    public string? FieldName { get; set; }
    public string? DisplayName { get; set; }
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool IsHidden { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record SetUserFieldPermissionRequest(int UserId, int ScreenId, int FieldId, bool CanView, bool CanEdit, bool IsHidden, bool IsReadOnly, bool IsMandatory, bool IsActive = true);

/* ---------------------------------------------------------------------------
    DataScope DTOs
   --------------------------------------------------------------------------- */
public class DataScopeDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public int? ModuleId { get; set; }
    public string? ModuleName { get; set; }
    public int? ScreenId { get; set; }
    public string? ScreenName { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public int? BusinessUnitId { get; set; }
    public int? CostCenterId { get; set; }
    public int? ProfitCenterId { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record SetDataScopeRequest(int RoleId, int? ModuleId, int? ScreenId, int? CompanyId, int? BranchId, int? DepartmentId, int? WarehouseId, int? BusinessUnitId, int? CostCenterId, int? ProfitCenterId, bool CanView, bool CanCreate, bool CanEdit, bool CanDelete, bool IsActive = true);

/* ---------------------------------------------------------------------------
   UserDataScopeOverride DTOs
   --------------------------------------------------------------------------- */
public class UserDataScopeOverrideDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }
    public int? ModuleId { get; set; }
    public string? ModuleName { get; set; }
    public int? ScreenId { get; set; }
    public string? ScreenName { get; set; }
    public string ScopeType { get; set; } = string.Empty;
    public string ScopeValue { get; set; } = string.Empty;
    public string PermissionType { get; set; } = string.Empty;
    public bool Allow { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Remarks { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record SetUserDataScopeOverrideRequest(int UserId, int? ModuleId, int? ScreenId, string ScopeType, string ScopeValue, string PermissionType, bool Allow, DateTime? EffectiveFrom, DateTime? EffectiveTo, string? Remarks, bool IsActive = true);

/* ---------------------------------------------------------------------------
   WorkflowPermissionEntry DTOs
   --------------------------------------------------------------------------- */
public class WorkflowPermissionEntryDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public int ModuleId { get; set; }
    public string? ModuleName { get; set; }
    public int SubModuleId { get; set; }
    public string? SubModuleName { get; set; }
    public int ScreenId { get; set; }
    public string? ScreenName { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
    public bool CanCancel { get; set; }
    public bool CanClose { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record SetWorkflowPermissionRequest(int RoleId, int ModuleId, int SubModuleId, int ScreenId, bool CanSubmit, bool CanApprove, bool CanReject, bool CanCancel, bool CanClose, bool IsActive = true);
