using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
public class RolesController : BaseController
{
    private readonly IRoleService _roleService;
    private readonly IValidator<CreateRoleRequest> _createValidator;
    private readonly IValidator<UpdateRoleRequest> _updateValidator;

    public RolesController(
        IRoleService roleService,
        IValidator<CreateRoleRequest> createValidator,
        IValidator<UpdateRoleRequest> updateValidator)
    {
        _roleService = roleService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.RolesView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roleService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<RoleDto>>.Ok(result));
    }

    // No [Permission] gate: any signed-in user may resolve their own roleIds.
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleDto>>), 200)]
    public async Task<IActionResult> GetMyRoles()
    {
        var result = await _roleService.GetMyRolesAsync();
        return Ok(ApiResponse<IEnumerable<RoleDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.RolesView)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _roleService.GetByIdAsync(id);
        return Ok(ApiResponse<RoleDto>.Ok(result));
    }

    [HttpGet("permissions")]
    [Permission(Permissions.RolesView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionDto>>), 200)]
    public async Task<IActionResult> GetAllPermissions()
    {
        var result = await _roleService.GetAllPermissionsAsync();
        return Ok(ApiResponse<IEnumerable<PermissionDto>>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.RolesManage)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<RoleDto>.Fail("Validation failed", errors));

        var result = await _roleService.CreateAsync(request);
        return Ok(ApiResponse<RoleDto>.Ok(result, "Role created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.RolesManage)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<RoleDto>.Fail("Validation failed", errors));

        var result = await _roleService.UpdateAsync(id, request);
        return Ok(ApiResponse<RoleDto>.Ok(result, "Role updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.RolesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _roleService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Role deleted successfully"));
    }

    [HttpPut("{id:int}/permissions")]
    [Permission(Permissions.RolesManage)]
    public async Task<IActionResult> SetPermissions(int id, [FromBody] SetRolePermissionsRequest request)
    {
        await _roleService.SetPermissionsAsync(id, request);
        return Ok(ApiResponse.Ok("Role permissions updated successfully"));
    }
}
