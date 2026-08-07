using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Route("api/[controller]")]
public class PermissionModulesController : BaseController
{
    private readonly IPermissionModuleService _service;
    private readonly IValidator<CreatePermissionModuleRequest> _createValidator;
    private readonly IValidator<UpdatePermissionModuleRequest> _updateValidator;

    public PermissionModulesController(
        IPermissionModuleService service,
        IValidator<CreatePermissionModuleRequest> createValidator,
        IValidator<UpdatePermissionModuleRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission("permission-modules.view")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionModuleDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<PermissionModuleDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission("permission-modules.view")]
    [ProducesResponseType(typeof(ApiResponse<PermissionModuleDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<PermissionModuleDto>.Ok(result));
    }

    [HttpGet("code/{code}")]
    [Permission("permission-modules.view")]
    [ProducesResponseType(typeof(ApiResponse<PermissionModuleDto>), 200)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _service.GetByCodeAsync(code);
        return Ok(ApiResponse<PermissionModuleDto>.Ok(result));
    }

    [HttpPost]
    [Permission("permission-modules.manage")]
    [ProducesResponseType(typeof(ApiResponse<PermissionModuleDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreatePermissionModuleRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<PermissionModuleDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<PermissionModuleDto>.Ok(result, "Permission module created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission("permission-modules.manage")]
    [ProducesResponseType(typeof(ApiResponse<PermissionModuleDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePermissionModuleRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<PermissionModuleDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<PermissionModuleDto>.Ok(result, "Permission module updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission("permission-modules.manage")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Permission module deleted successfully"));
    }
}

[Route("api/[controller]")]
public class PermissionActionsController : BaseController
{
    private readonly IPermissionActionService _service;
    private readonly IValidator<CreatePermissionActionRequest> _createValidator;
    private readonly IValidator<UpdatePermissionActionRequest> _updateValidator;

    public PermissionActionsController(
        IPermissionActionService service,
        IValidator<CreatePermissionActionRequest> createValidator,
        IValidator<UpdatePermissionActionRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission("permission-actions.view")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionActionDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<PermissionActionDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission("permission-actions.view")]
    [ProducesResponseType(typeof(ApiResponse<PermissionActionDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<PermissionActionDto>.Ok(result));
    }

    [HttpPost]
    [Permission("permission-actions.manage")]
    [ProducesResponseType(typeof(ApiResponse<PermissionActionDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreatePermissionActionRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<PermissionActionDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<PermissionActionDto>.Ok(result, "Permission action created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission("permission-actions.manage")]
    [ProducesResponseType(typeof(ApiResponse<PermissionActionDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePermissionActionRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<PermissionActionDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<PermissionActionDto>.Ok(result, "Permission action updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission("permission-actions.manage")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Permission action deleted successfully"));
    }
}

[Route("api/[controller]")]
public class ModulePermissionsController : BaseController
{
    private readonly IModulePermissionService _service;

    public ModulePermissionsController(IModulePermissionService service)
    {
        _service = service;
    }

    [HttpGet("role/{roleId:int}")]
    [Permission("roles.view")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ModulePermissionDto>>), 200)]
    public async Task<IActionResult> GetByRole(int roleId)
    {
        var result = await _service.GetByRoleAsync(roleId);
        return Ok(ApiResponse<IEnumerable<ModulePermissionDto>>.Ok(result));
    }

    [HttpGet("module/{moduleId:int}")]
    [Permission("permission-modules.view")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ModulePermissionDto>>), 200)]
    public async Task<IActionResult> GetByModule(int moduleId)
    {
        var result = await _service.GetByModuleAsync(moduleId);
        return Ok(ApiResponse<IEnumerable<ModulePermissionDto>>.Ok(result));
    }

    [HttpPost("assign")]
    [Permission("roles.manage")]
    public async Task<IActionResult> Assign([FromBody] AssignModulePermissionRequest request)
    {
        await _service.AssignAsync(request);
        return Ok(ApiResponse.Ok("Permission assigned successfully"));
    }

    [HttpPost("revoke")]
    [Permission("roles.manage")]
    public async Task<IActionResult> Revoke([FromBody] RevokeModulePermissionRequest request)
    {
        await _service.RevokeAsync(request);
        return Ok(ApiResponse.Ok("Permission revoked successfully"));
    }

    [HttpDelete("role/{roleId:int}/all")]
    [Permission("roles.manage")]
    public async Task<IActionResult> RevokeAllForRole(int roleId)
    {
        await _service.RevokeAllForRoleAsync(roleId);
        return Ok(ApiResponse.Ok("All permissions revoked for role"));
    }
}

[Route("api/[controller]")]
public class FieldPermissionsController : BaseController
{
    private readonly IFieldPermissionService _service;

    public FieldPermissionsController(IFieldPermissionService service)
    {
        _service = service;
    }

    [HttpGet("role/{roleId:int}/module/{moduleId:int}")]
    [Permission("roles.view")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FieldPermissionDto>>), 200)]
    public async Task<IActionResult> GetByRoleModule(int roleId, int moduleId)
    {
        var result = await _service.GetByRoleModuleAsync(roleId, moduleId);
        return Ok(ApiResponse<IEnumerable<FieldPermissionDto>>.Ok(result));
    }

    [HttpPost]
    [Permission("roles.manage")]
    public async Task<IActionResult> Set([FromBody] SetFieldPermissionRequest request)
    {
        var result = await _service.SetAsync(request);
        return Ok(ApiResponse<FieldPermissionDto>.Ok(result, "Field permission saved successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission("roles.manage")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Field permission deleted successfully"));
    }
}

[Route("api/permission/[controller]")]
public class UserPermissionsController : BaseController
{
    private readonly IModulePermissionService _service;
    private readonly ICurrentUser _currentUser;

    public UserPermissionsController(IModulePermissionService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("user-permissions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserModulePermissionDto>>), 200)]
    public async Task<IActionResult> GetCurrentUserPermissions()
    {
        var result = await _service.GetUserPermissionsAsync(_currentUser.UserId);
        return Ok(ApiResponse<IEnumerable<UserModulePermissionDto>>.Ok(result));
    }
}