using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
public class SettingsController : BaseController
{
    private readonly ISettingsService _settingsService;
    private readonly IValidator<UpdateSettingsRequest> _validator;

    public SettingsController(ISettingsService settingsService, IValidator<UpdateSettingsRequest> validator)
    {
        _settingsService = settingsService;
        _validator = validator;
    }

    [HttpGet]
    [Permission(Permissions.SettingsView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ApplicationSetting>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _settingsService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ApplicationSetting>>.Ok(result));
    }

    [HttpPut]
    [Permission(Permissions.SettingsEdit)]
    public async Task<IActionResult> Update([FromBody] UpdateSettingsRequest request)
    {
        var errors = await ValidateAsync(_validator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<bool>.Fail("Validation failed", errors));

        await _settingsService.UpdateAsync(request, "system");
        return Ok(ApiResponse.Ok("Settings updated successfully"));
    }
}
