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
[Route("api/financial-years")]
public class FinancialYearsController : BaseController
{
    private readonly IFinancialYearService _service;
    private readonly IValidator<CreateFinancialYearRequest> _createValidator;
    private readonly IValidator<UpdateFinancialYearRequest> _updateValidator;

    public FinancialYearsController(
        IFinancialYearService service,
        IValidator<CreateFinancialYearRequest> createValidator,
        IValidator<UpdateFinancialYearRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.FinancialYearsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<FinancialYearDto>>), 200)]
    public async Task<IActionResult> GetFinancialYears([FromQuery] int companyId, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(companyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<FinancialYearDto>>.Ok(result));
    }

    [HttpGet("all")]
    [Permission(Permissions.FinancialYearsView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FinancialYearDto>>), 200)]
    public async Task<IActionResult> GetAllFinancialYears([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<FinancialYearDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Permission(Permissions.FinancialYearsView)]
    [ProducesResponseType(typeof(ApiResponse<FinancialYearDto>), 200)]
    public async Task<IActionResult> GetFinancialYear(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<FinancialYearDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.FinancialYearsCreate)]
    [ProducesResponseType(typeof(ApiResponse<FinancialYearDto>), 200)]
    public async Task<IActionResult> CreateFinancialYear([FromBody] CreateFinancialYearRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<FinancialYearDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<FinancialYearDto>.Ok(result, "Financial year created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.FinancialYearsEdit)]
    [ProducesResponseType(typeof(ApiResponse<FinancialYearDto>), 200)]
    public async Task<IActionResult> UpdateFinancialYear(long id, [FromBody] UpdateFinancialYearRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<FinancialYearDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<FinancialYearDto>.Ok(result, "Financial year updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.FinancialYearsDelete)]
    public async Task<IActionResult> DeleteFinancialYear(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Financial year deleted successfully"));
    }
}