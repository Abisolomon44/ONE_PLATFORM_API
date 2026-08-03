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
[Route("api/address-types")]
public class AddressTypesController : BaseController
{
    private readonly IAddressTypeService _service;
    private readonly IValidator<CreateAddressTypeRequest> _createValidator;
    private readonly IValidator<UpdateAddressTypeRequest> _updateValidator;

    public AddressTypesController(
        IAddressTypeService service,
        IValidator<CreateAddressTypeRequest> createValidator,
        IValidator<UpdateAddressTypeRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.AddressTypesView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AddressTypeDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<AddressTypeDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.AddressTypesView)]
    [ProducesResponseType(typeof(ApiResponse<AddressTypeDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<AddressTypeDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.AddressTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<AddressTypeDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateAddressTypeRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<AddressTypeDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<AddressTypeDto>.Ok(result, "Address type created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.AddressTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<AddressTypeDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAddressTypeRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<AddressTypeDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<AddressTypeDto>.Ok(result, "Address type updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.AddressTypesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Address type deleted successfully"));
    }
}
