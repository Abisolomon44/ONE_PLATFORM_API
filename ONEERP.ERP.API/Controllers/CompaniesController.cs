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
public class CompaniesController : BaseController
{
    private readonly ICompanyService _companyService;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<CreateCompanyRequest> _createValidator;
    private readonly IValidator<UpdateCompanyRequest> _updateValidator;

    public CompaniesController(
        ICompanyService companyService,
        ICurrentUser currentUser,
        IValidator<CreateCompanyRequest> createValidator,
        IValidator<UpdateCompanyRequest> updateValidator)
    {
        _companyService = companyService;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.CompaniesView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<CompanyDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _companyService.GetPagedAsync(page, size, search);
        return Ok(ApiResponse<PaginatedResult<CompanyDto>>.Ok(result));
    }

    [HttpGet("current")]
    [Permission(Permissions.CompaniesView)]
    [ProducesResponseType(typeof(ApiResponse<CompanyDto>), 200)]
    public async Task<IActionResult> GetCurrent()
    {
        var result = await _companyService.GetByIdAsync(_currentUser.CompanyId);
        return Ok(ApiResponse<CompanyDto>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.CompaniesView)]
    [ProducesResponseType(typeof(ApiResponse<CompanyDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _companyService.GetByIdAsync(id);
        return Ok(ApiResponse<CompanyDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.CompaniesCreate)]
    [ProducesResponseType(typeof(ApiResponse<CompanyDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CompanyDto>.Fail("Validation failed", errors));

        var result = await _companyService.CreateAsync(request, _currentUser.Username);
        return Ok(ApiResponse<CompanyDto>.Ok(result, "Company created successfully"));
    }

    [HttpPut("current")]
    [Permission(Permissions.CompaniesEdit)]
    [ProducesResponseType(typeof(ApiResponse<CompanyDto>), 200)]
    public async Task<IActionResult> UpdateCurrent([FromBody] UpdateCompanyRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CompanyDto>.Fail("Validation failed", errors));

        var result = await _companyService.UpdateAsync(_currentUser.CompanyId, request, _currentUser.Username);
        return Ok(ApiResponse<CompanyDto>.Ok(result, "Company updated successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.CompaniesEdit)]
    [ProducesResponseType(typeof(ApiResponse<CompanyDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCompanyRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CompanyDto>.Fail("Validation failed", errors));

        var result = await _companyService.UpdateAsync(id, request, _currentUser.Username);
        return Ok(ApiResponse<CompanyDto>.Ok(result, "Company updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.CompaniesEdit)]
    public async Task<IActionResult> Delete(int id)
    {
        await _companyService.DeleteAsync(id, _currentUser.Username);
        return Ok(ApiResponse.Ok("Company deleted successfully"));
    }
}
