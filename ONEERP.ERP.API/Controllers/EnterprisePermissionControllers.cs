using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _service;
    public WorkspacesController(IWorkspaceService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(ApiResponse<IEnumerable<WorkspaceDto>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => Ok(ApiResponse<WorkspaceDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission("workspaces.manage")]
    public async Task<IActionResult> Create([FromBody] CreateWorkspaceRequest request)
        => Ok(ApiResponse<WorkspaceDto>.Ok(await _service.CreateAsync(request)));

    [HttpPut("{id:int}")]
    [Permission("workspaces.manage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkspaceRequest request)
        => Ok(ApiResponse<WorkspaceDto>.Ok(await _service.UpdateAsync(id, request)));

    [HttpDelete("{id:int}")]
    [Permission("workspaces.manage")]
    public async Task<IActionResult> Delete(int id)
        => Ok(ApiResponse<bool>.Ok(await _service.DeleteAsync(id)));
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DomainsController : ControllerBase
{
    private readonly IDomainService _service;
    public DomainsController(IDomainService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(ApiResponse<IEnumerable<DomainDto>>.Ok(await _service.GetAllAsync()));

    [HttpGet("workspace/{workspaceId:int}")]
    public async Task<IActionResult> GetByWorkspace(int workspaceId)
        => Ok(ApiResponse<IEnumerable<DomainDto>>.Ok(await _service.GetByWorkspaceAsync(workspaceId)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => Ok(ApiResponse<DomainDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission("domains.manage")]
    public async Task<IActionResult> Create([FromBody] CreateDomainRequest request)
        => Ok(ApiResponse<DomainDto>.Ok(await _service.CreateAsync(request)));

    [HttpPut("{id:int}")]
    [Permission("domains.manage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDomainRequest request)
        => Ok(ApiResponse<DomainDto>.Ok(await _service.UpdateAsync(id, request)));

    [HttpDelete("{id:int}")]
    [Permission("domains.manage")]
    public async Task<IActionResult> Delete(int id)
        => Ok(ApiResponse<bool>.Ok(await _service.DeleteAsync(id)));
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ModulesController : ControllerBase
{
    private readonly IModuleService _service;
    public ModulesController(IModuleService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(ApiResponse<IEnumerable<ModuleDto>>.Ok(await _service.GetAllAsync()));

    [HttpGet("domain/{domainId:int}")]
    public async Task<IActionResult> GetByDomain(int domainId)
        => Ok(ApiResponse<IEnumerable<ModuleDto>>.Ok(await _service.GetByDomainAsync(domainId)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => Ok(ApiResponse<ModuleDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission("modules.manage")]
    public async Task<IActionResult> Create([FromBody] CreateModuleRequest request)
        => Ok(ApiResponse<ModuleDto>.Ok(await _service.CreateAsync(request)));

    [HttpPut("{id:int}")]
    [Permission("modules.manage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateModuleRequest request)
        => Ok(ApiResponse<ModuleDto>.Ok(await _service.UpdateAsync(id, request)));

    [HttpDelete("{id:int}")]
    [Permission("modules.manage")]
    public async Task<IActionResult> Delete(int id)
        => Ok(ApiResponse<bool>.Ok(await _service.DeleteAsync(id)));
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScreensController : ControllerBase
{
    private readonly IScreenService _service;
    public ScreensController(IScreenService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(ApiResponse<IEnumerable<ScreenDto>>.Ok(await _service.GetAllAsync()));

    [HttpGet("module/{moduleId:int}")]
    public async Task<IActionResult> GetByModule(int moduleId)
        => Ok(ApiResponse<IEnumerable<ScreenDto>>.Ok(await _service.GetByModuleAsync(moduleId)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => Ok(ApiResponse<ScreenDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission("screens.manage")]
    public async Task<IActionResult> Create([FromBody] CreateScreenRequest request)
        => Ok(ApiResponse<ScreenDto>.Ok(await _service.CreateAsync(request)));

    [HttpPut("{id:int}")]
    [Permission("screens.manage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateScreenRequest request)
        => Ok(ApiResponse<ScreenDto>.Ok(await _service.UpdateAsync(id, request)));

    [HttpDelete("{id:int}")]
    [Permission("screens.manage")]
    public async Task<IActionResult> Delete(int id)
        => Ok(ApiResponse<bool>.Ok(await _service.DeleteAsync(id)));
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FieldsController : ControllerBase
{
    private readonly IFieldService _service;
    public FieldsController(IFieldService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(ApiResponse<IEnumerable<FieldDto>>.Ok(await _service.GetAllAsync()));

    [HttpGet("screen/{screenId:int}")]
    public async Task<IActionResult> GetByScreen(int screenId)
        => Ok(ApiResponse<IEnumerable<FieldDto>>.Ok(await _service.GetByScreenAsync(screenId)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => Ok(ApiResponse<FieldDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission("fields.manage")]
    public async Task<IActionResult> Create([FromBody] CreateFieldRequest request)
        => Ok(ApiResponse<FieldDto>.Ok(await _service.CreateAsync(request)));

    [HttpPut("{id:int}")]
    [Permission("fields.manage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFieldRequest request)
        => Ok(ApiResponse<FieldDto>.Ok(await _service.UpdateAsync(id, request)));

    [HttpDelete("{id:int}")]
    [Permission("fields.manage")]
    public async Task<IActionResult> Delete(int id)
        => Ok(ApiResponse<bool>.Ok(await _service.DeleteAsync(id)));
}

[ApiController]
[Route("api/actions")]
[Authorize]
public class ActionsController : ControllerBase
{
    private readonly IActionService _service;
    public ActionsController(IActionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(ApiResponse<IEnumerable<ActionDto>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => Ok(ApiResponse<ActionDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission("actions.manage")]
    public async Task<IActionResult> Create([FromBody] CreateActionRequest request)
        => Ok(ApiResponse<ActionDto>.Ok(await _service.CreateAsync(request)));

    [HttpPut("{id:int}")]
    [Permission("actions.manage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateActionRequest request)
        => Ok(ApiResponse<ActionDto>.Ok(await _service.UpdateAsync(id, request)));

    [HttpDelete("{id:int}")]
    [Permission("actions.manage")]
    public async Task<IActionResult> Delete(int id)
        => Ok(ApiResponse<bool>.Ok(await _service.DeleteAsync(id)));
}

[ApiController]
[Route("api/role-permissions")]
[Authorize]
public class RolePermissionsController : ControllerBase
{
    private readonly IRolePermissionEntryService _service;
    public RolePermissionsController(IRolePermissionEntryService service) => _service = service;

    [HttpGet("role/{roleId:int}")]
    public async Task<IActionResult> GetByRole(int roleId)
        => Ok(ApiResponse<IEnumerable<RolePermissionEntryDto>>.Ok(await _service.GetByRoleAsync(roleId)));

    [HttpPost]
    [Permission("role-permissions.manage")]
    public async Task<IActionResult> Assign([FromBody] AssignRolePermissionRequest request)
        => Ok(ApiResponse<RolePermissionEntryDto>.Ok(await _service.AssignAsync(request)));

    [HttpPost("bulk")]
    [Permission("role-permissions.manage")]
    public async Task<IActionResult> BulkAssign([FromBody] BulkAssignRolePermissionRequest request)
        => Ok(ApiResponse<bool>.Ok(await _service.BulkAssignAsync(request)));

    [HttpDelete("{id:int}")]
    [Permission("role-permissions.manage")]
    public async Task<IActionResult> Delete(int id)
        => Ok(ApiResponse<bool>.Ok(await _service.DeleteAsync(id)));

    [HttpDelete("role/{roleId:int}")]
    [Permission("role-permissions.manage")]
    public async Task<IActionResult> DeleteAllForRole(int roleId)
        => Ok(ApiResponse<bool>.Ok(await _service.DeleteAllForRoleAsync(roleId)));
}

[ApiController]
[Route("api/user-permission-overrides")]
[Authorize]
public class UserPermissionOverridesController : ControllerBase
{
    private readonly IUserPermissionOverrideService _service;
    public UserPermissionOverridesController(IUserPermissionOverrideService service) => _service = service;

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
        => Ok(ApiResponse<IEnumerable<UserPermissionOverrideDto>>.Ok(await _service.GetByUserAsync(userId)));

    [HttpPost]
    [Permission("user-permission-overrides.manage")]
    public async Task<IActionResult> Create([FromBody] CreateUserPermissionOverrideRequest request)
        => Ok(ApiResponse<UserPermissionOverrideDto>.Ok(await _service.CreateAsync(request)));

    [HttpPut("{id:int}")]
    [Permission("user-permission-overrides.manage")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserPermissionOverrideRequest request)
        => Ok(ApiResponse<UserPermissionOverrideDto>.Ok(await _service.UpdateAsync(id, request)));

    [HttpDelete("{id:int}")]
    [Permission("user-permission-overrides.manage")]
    public async Task<IActionResult> Delete(int id)
        => Ok(ApiResponse<bool>.Ok(await _service.DeleteAsync(id)));
}

[ApiController]
[Route("api/role-field-permissions")]
[Authorize]
public class RoleFieldPermissionsController : ControllerBase
{
    private readonly IRoleFieldPermissionEntryService _service;
    public RoleFieldPermissionsController(IRoleFieldPermissionEntryService service) => _service = service;

    [HttpGet("role/{roleId:int}")]
    public async Task<IActionResult> GetByRole(int roleId)
        => Ok(ApiResponse<IEnumerable<RoleFieldPermissionEntryDto>>.Ok(await _service.GetByRoleAsync(roleId)));

    [HttpGet("role/{roleId:int}/screen/{screenId:int}")]
    public async Task<IActionResult> GetByRoleScreen(int roleId, int screenId)
        => Ok(ApiResponse<IEnumerable<RoleFieldPermissionEntryDto>>.Ok(await _service.GetByRoleScreenAsync(roleId, screenId)));

    [HttpPost]
    [Permission("role-field-permissions.manage")]
    public async Task<IActionResult> Set([FromBody] SetRoleFieldPermissionRequest request)
        => Ok(ApiResponse<RoleFieldPermissionEntryDto>.Ok(await _service.SetAsync(request)));

    [HttpPost("bulk")]
    [Permission("role-field-permissions.manage")]
    public async Task<IActionResult> BulkSet([FromBody] BulkSetRoleFieldPermissionRequest request)
        => Ok(ApiResponse<bool>.Ok(await _service.BulkSetAsync(request)));

    [HttpDelete("{id:int}")]
    [Permission("role-field-permissions.manage")]
    public async Task<IActionResult> Delete(int id)
        => Ok(ApiResponse<bool>.Ok(await _service.DeleteAsync(id)));
}

[ApiController]
[Route("api/user-field-permissions")]
[Authorize]
public class UserFieldPermissionsController : ControllerBase
{
    private readonly IUserFieldPermissionEntryService _service;
    public UserFieldPermissionsController(IUserFieldPermissionEntryService service) => _service = service;

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
        => Ok(ApiResponse<IEnumerable<UserFieldPermissionEntryDto>>.Ok(await _service.GetByUserAsync(userId)));

    [HttpGet("user/{userId:int}/screen/{screenId:int}")]
    public async Task<IActionResult> GetByUserScreen(int userId, int screenId)
        => Ok(ApiResponse<IEnumerable<UserFieldPermissionEntryDto>>.Ok(await _service.GetByUserScreenAsync(userId, screenId)));

    [HttpPost]
    [Permission("user-field-permissions.manage")]
    public async Task<IActionResult> Set([FromBody] SetUserFieldPermissionRequest request)
        => Ok(ApiResponse<UserFieldPermissionEntryDto>.Ok(await _service.SetAsync(request)));

    [HttpDelete("{id:int}")]
    [Permission("user-field-permissions.manage")]
    public async Task<IActionResult> Delete(int id)
        => Ok(ApiResponse<bool>.Ok(await _service.DeleteAsync(id)));
}

[ApiController]
[Route("api/data-scopes")]
[Authorize]
public class DataScopesController : ControllerBase
{
    private readonly IDataScopeService _service;
    public DataScopesController(IDataScopeService service) => _service = service;

    [HttpGet("role/{roleId:int}")]
    public async Task<IActionResult> GetByRole(int roleId)
        => Ok(ApiResponse<DataScopeDto?>.Ok(await _service.GetByRoleAsync(roleId)));

    [HttpPost]
    [Permission("data-scopes.manage")]
    public async Task<IActionResult> Set([FromBody] SetDataScopeRequest request)
        => Ok(ApiResponse<DataScopeDto>.Ok(await _service.SetAsync(request)));

    [HttpDelete("role/{roleId:int}")]
    [Permission("data-scopes.manage")]
    public async Task<IActionResult> Delete(int roleId)
        => Ok(ApiResponse<bool>.Ok(await _service.DeleteAsync(roleId)));
}

[ApiController]
[Route("api/workflow-permissions")]
[Authorize]
public class WorkflowPermissionsController : ControllerBase
{
    private readonly IWorkflowPermissionEntryService _service;
    public WorkflowPermissionsController(IWorkflowPermissionEntryService service) => _service = service;

    [HttpGet("role/{roleId:int}")]
    public async Task<IActionResult> GetByRole(int roleId)
        => Ok(ApiResponse<IEnumerable<WorkflowPermissionEntryDto>>.Ok(await _service.GetByRoleAsync(roleId)));

    [HttpPost]
    [Permission("workflow-permissions.manage")]
    public async Task<IActionResult> Set([FromBody] SetWorkflowPermissionRequest request)
        => Ok(ApiResponse<WorkflowPermissionEntryDto>.Ok(await _service.SetAsync(request)));

    [HttpDelete("{id:int}")]
    [Permission("workflow-permissions.manage")]
    public async Task<IActionResult> Delete(int id)
        => Ok(ApiResponse<bool>.Ok(await _service.DeleteAsync(id)));
}
