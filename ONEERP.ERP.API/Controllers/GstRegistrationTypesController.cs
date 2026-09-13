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
[Route("api/gst-registration-types")]
public class GstRegistrationTypesController : BaseController
{
    private readonly IGstRegistrationTypeService _service;
    private readonly IValidator<CreateGstRegistrationTypeRequest> _createValidator;
    private readonly IValidator<UpdateGstRegistrationTypeRequest> _updateValidator;

    public GstRegistrationTypesController(
        IGstRegistrationTypeService service,
        IValidator<CreateGstRegistrationTypeRequest> createValidator,
        IValidator<UpdateGstRegistrationTypeRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // No [Permission] gate: simple lookup used to populate dropdowns app-wide.
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GstRegistrationTypeDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<GstRegistrationTypeDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<GstRegistrationTypeDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<GstRegistrationTypeDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.GstRegistrationTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<GstRegistrationTypeDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateGstRegistrationTypeRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<GstRegistrationTypeDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<GstRegistrationTypeDto>.Ok(result, "GST registration type created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.GstRegistrationTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<GstRegistrationTypeDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGstRegistrationTypeRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<GstRegistrationTypeDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<GstRegistrationTypeDto>.Ok(result, "GST registration type updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.GstRegistrationTypesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("GST registration type deleted successfully"));
    }
}
