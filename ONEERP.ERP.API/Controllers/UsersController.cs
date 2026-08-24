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
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;
    private readonly IValidator<ResetUserPasswordRequest> _resetValidator;

    public UsersController(
        IUserService userService,
        ICurrentUser currentUser,
        IValidator<CreateUserRequest> createValidator,
        IValidator<UpdateUserRequest> updateValidator,
        IValidator<ResetUserPasswordRequest> resetValidator)
    {
        _userService = userService;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _resetValidator = resetValidator;
    }

    [HttpGet]
    [Permission(Permissions.UsersView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<UserWithRolesDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _userService.GetPagedAsync(page, size, search);
        return Ok(ApiResponse<PaginatedResult<UserWithRolesDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.UsersView)]
    [ProducesResponseType(typeof(ApiResponse<UserWithRolesDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return Ok(ApiResponse<UserWithRolesDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.UsersCreate)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<UserDto>.Fail("Validation failed", errors));

        var result = await _userService.CreateAsync(request, _currentUser.CompanyId, _currentUser.Username);
        return Ok(ApiResponse<UserDto>.Ok(result, "User created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.UsersEdit)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<UserDto>.Fail("Validation failed", errors));

        var result = await _userService.UpdateAsync(id, request, _currentUser.Username);
        return Ok(ApiResponse<UserDto>.Ok(result, "User updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.UsersDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteAsync(id, _currentUser.Username);
        return Ok(ApiResponse.Ok("User deleted successfully"));
    }

    [HttpPut("{id:int}/password")]
    [Permission(Permissions.UsersEdit)]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetUserPasswordRequest request)
    {
        var errors = await ValidateAsync(_resetValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse.Fail("Validation failed", errors));

        await _userService.ResetPasswordAsync(id, request.Password, _currentUser.Username);
        return Ok(ApiResponse.Ok("Password reset successfully"));
    }
}
