using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Requests;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

/// <summary>
/// REST API for currency administration under the Administration module.
/// </summary>
[Authorize]
[Route("api/Administration")]
public class AdministrationController : BaseController
{
    private readonly IAdministrationService _service;
    private readonly IValidator<SaveCurrencyRequest> _validator;

    public AdministrationController(IAdministrationService service, IValidator<SaveCurrencyRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpGet("currencies")]
    [Permission(Permissions.CurrenciesView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CurrencyDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<CurrencyDto>>.Ok(result));
    }

    [HttpGet("currency/{id:int}")]
    [Permission(Permissions.CurrenciesView)]
    [ProducesResponseType(typeof(ApiResponse<CurrencyDto>), 200)]
    public async Task<IActionResult> GetCurrency(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<CurrencyDto>.Ok(result));
    }

    [HttpPost("currency")]
    [Permission(Permissions.CurrenciesManage)]
    [ProducesResponseType(typeof(ApiResponse<CurrencyDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<CurrencyDto>), 400)]
    public async Task<IActionResult> Create([FromBody] SaveCurrencyRequest request)
    {
        var errors = await ValidateAsync(_validator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CurrencyDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetCurrency), new { id = result.Id },
            ApiResponse<CurrencyDto>.Ok(result, "Currency created successfully"));
    }

    [HttpPut("currency/{id:int}")]
    [Permission(Permissions.CurrenciesManage)]
    [ProducesResponseType(typeof(ApiResponse<CurrencyDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<CurrencyDto>), 400)]
    public async Task<IActionResult> Update(int id, [FromBody] SaveCurrencyRequest request)
    {
        var errors = await ValidateAsync(_validator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CurrencyDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<CurrencyDto>.Ok(result, "Currency updated successfully"));
    }

    [HttpDelete("currency/{id:int}")]
    [Permission(Permissions.CurrenciesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Currency deleted successfully"));
    }
}
