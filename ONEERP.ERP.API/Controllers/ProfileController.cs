using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/profile")]
public class ProfileController : BaseController
{
    private readonly IProfileService _profileService;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;

    public ProfileController(IProfileService profileService, IValidator<ChangePasswordRequest> changePasswordValidator)
    {
        _profileService = profileService;
        _changePasswordValidator = changePasswordValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ProfileDto>), 200)]
    public async Task<IActionResult> Get()
    {
        var result = await _profileService.GetAsync();
        return Ok(ApiResponse<ProfileDto>.Ok(result));
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var errors = await ValidateAsync(_changePasswordValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<bool>.Fail("Validation failed", errors));

        await _profileService.ChangePasswordAsync(request.CurrentPassword, request.NewPassword);
        return Ok(ApiResponse.Ok("Password changed successfully"));
    }
}
