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
[Route("api/business-types")]
public class BusinessTypesController : BaseController
{
    private readonly IBusinessTypeService _service;
    private readonly IValidator<CreateBusinessTypeRequest> _createValidator;
    private readonly IValidator<UpdateBusinessTypeRequest> _updateValidator;

    public BusinessTypesController(
        IBusinessTypeService service,
        IValidator<CreateBusinessTypeRequest> createValidator,
        IValidator<UpdateBusinessTypeRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.BusinessTypesView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BusinessTypeDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<BusinessTypeDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.BusinessTypesView)]
    [ProducesResponseType(typeof(ApiResponse<BusinessTypeDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<BusinessTypeDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.BusinessTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<BusinessTypeDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateBusinessTypeRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<BusinessTypeDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<BusinessTypeDto>.Ok(result, "Business type created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.BusinessTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<BusinessTypeDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBusinessTypeRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<BusinessTypeDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<BusinessTypeDto>.Ok(result, "Business type updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.BusinessTypesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Business type deleted successfully"));
    }
}

[Authorize]
[Route("api/industry-types")]
public class IndustryTypesController : BaseController
{
    private readonly IIndustryTypeService _service;
    private readonly IValidator<CreateIndustryTypeRequest> _createValidator;
    private readonly IValidator<UpdateIndustryTypeRequest> _updateValidator;

    public IndustryTypesController(
        IIndustryTypeService service,
        IValidator<CreateIndustryTypeRequest> createValidator,
        IValidator<UpdateIndustryTypeRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.IndustryTypesView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<IndustryTypeDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<IndustryTypeDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.IndustryTypesView)]
    [ProducesResponseType(typeof(ApiResponse<IndustryTypeDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<IndustryTypeDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.IndustryTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<IndustryTypeDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateIndustryTypeRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<IndustryTypeDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<IndustryTypeDto>.Ok(result, "Industry type created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.IndustryTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<IndustryTypeDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateIndustryTypeRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<IndustryTypeDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<IndustryTypeDto>.Ok(result, "Industry type updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.IndustryTypesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Industry type deleted successfully"));
    }
}

[Authorize]
[Route("api/company-groups")]
public class CompanyGroupsController : BaseController
{
    private readonly ICompanyGroupService _service;
    private readonly IValidator<CreateCompanyGroupRequest> _createValidator;
    private readonly IValidator<UpdateCompanyGroupRequest> _updateValidator;

    public CompanyGroupsController(
        ICompanyGroupService service,
        IValidator<CreateCompanyGroupRequest> createValidator,
        IValidator<UpdateCompanyGroupRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.CompanyGroupsView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CompanyGroupDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<CompanyGroupDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.CompanyGroupsView)]
    [ProducesResponseType(typeof(ApiResponse<CompanyGroupDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<CompanyGroupDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.CompanyGroupsManage)]
    [ProducesResponseType(typeof(ApiResponse<CompanyGroupDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateCompanyGroupRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CompanyGroupDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<CompanyGroupDto>.Ok(result, "Company group created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.CompanyGroupsManage)]
    [ProducesResponseType(typeof(ApiResponse<CompanyGroupDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCompanyGroupRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CompanyGroupDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<CompanyGroupDto>.Ok(result, "Company group updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.CompanyGroupsManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Company group deleted successfully"));
    }
}
