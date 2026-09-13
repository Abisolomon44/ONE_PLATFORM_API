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
[Route("api/organization-types")]
public class OrganizationTypesController : BaseController
{
    private readonly IOrganizationTypeService _service;
    private readonly IValidator<CreateOrganizationTypeRequest> _createValidator;
    private readonly IValidator<UpdateOrganizationTypeRequest> _updateValidator;

    public OrganizationTypesController(
        IOrganizationTypeService service,
        IValidator<CreateOrganizationTypeRequest> createValidator,
        IValidator<UpdateOrganizationTypeRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // No [Permission] gate: simple lookup used to populate dropdowns app-wide.
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrganizationTypeDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<OrganizationTypeDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationTypeDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<OrganizationTypeDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.OrganizationTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<OrganizationTypeDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationTypeRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<OrganizationTypeDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<OrganizationTypeDto>.Ok(result, "Organization type created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.OrganizationTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<OrganizationTypeDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrganizationTypeRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<OrganizationTypeDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<OrganizationTypeDto>.Ok(result, "Organization type updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.OrganizationTypesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Organization type deleted successfully"));
    }
}
