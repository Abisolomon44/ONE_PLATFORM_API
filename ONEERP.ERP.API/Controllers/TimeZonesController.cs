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
[Route("api/timezones")]
public class TimeZonesController : BaseController
{
    private readonly ITimeZoneService _service;
    private readonly IValidator<CreateTimeZoneRequest> _createValidator;
    private readonly IValidator<UpdateTimeZoneRequest> _updateValidator;

    public TimeZonesController(
        ITimeZoneService service,
        IValidator<CreateTimeZoneRequest> createValidator,
        IValidator<UpdateTimeZoneRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // No [Permission] gate: simple lookup used to populate dropdowns app-wide.
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TimeZoneDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<TimeZoneDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TimeZoneDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<TimeZoneDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.TimeZonesManage)]
    [ProducesResponseType(typeof(ApiResponse<TimeZoneDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateTimeZoneRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<TimeZoneDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<TimeZoneDto>.Ok(result, "Time zone created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.TimeZonesManage)]
    [ProducesResponseType(typeof(ApiResponse<TimeZoneDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTimeZoneRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<TimeZoneDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<TimeZoneDto>.Ok(result, "Time zone updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.TimeZonesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Time zone deleted successfully"));
    }
}
