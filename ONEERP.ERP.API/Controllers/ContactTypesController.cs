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
[Route("api/contact-types")]
public class ContactTypesController : BaseController
{
    private readonly IContactTypeService _service;
    private readonly IValidator<CreateContactTypeRequest> _createValidator;
    private readonly IValidator<UpdateContactTypeRequest> _updateValidator;

    public ContactTypesController(
        IContactTypeService service,
        IValidator<CreateContactTypeRequest> createValidator,
        IValidator<UpdateContactTypeRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.ContactTypesView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ContactTypeDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<ContactTypeDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.ContactTypesView)]
    [ProducesResponseType(typeof(ApiResponse<ContactTypeDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<ContactTypeDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.ContactTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<ContactTypeDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateContactTypeRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ContactTypeDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<ContactTypeDto>.Ok(result, "Contact type created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.ContactTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<ContactTypeDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateContactTypeRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ContactTypeDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<ContactTypeDto>.Ok(result, "Contact type updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.ContactTypesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Contact type deleted successfully"));
    }
}
