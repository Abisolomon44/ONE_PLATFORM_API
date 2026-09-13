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

    // No [Permission] gate: simple lookup used to populate dropdowns app-wide.
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BusinessTypeDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<BusinessTypeDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
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
[Route("api/business-partner-roles")]
public class BusinessPartnerRolesController : BaseController
{
    private readonly IBusinessPartnerRoleService _service;
    private readonly IValidator<CreateBusinessPartnerRoleRequest> _createValidator;
    private readonly IValidator<UpdateBusinessPartnerRoleRequest> _updateValidator;

    public BusinessPartnerRolesController(
        IBusinessPartnerRoleService service,
        IValidator<CreateBusinessPartnerRoleRequest> createValidator,
        IValidator<UpdateBusinessPartnerRoleRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.BusinessPartnerRolesView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BusinessPartnerRoleDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<BusinessPartnerRoleDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.BusinessPartnerRolesView)]
    [ProducesResponseType(typeof(ApiResponse<BusinessPartnerRoleDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<BusinessPartnerRoleDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.BusinessPartnerRolesManage)]
    [ProducesResponseType(typeof(ApiResponse<BusinessPartnerRoleDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateBusinessPartnerRoleRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<BusinessPartnerRoleDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<BusinessPartnerRoleDto>.Ok(result, "Business partner role created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.BusinessPartnerRolesManage)]
    [ProducesResponseType(typeof(ApiResponse<BusinessPartnerRoleDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBusinessPartnerRoleRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<BusinessPartnerRoleDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<BusinessPartnerRoleDto>.Ok(result, "Business partner role updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.BusinessPartnerRolesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Business partner role deleted successfully"));
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

    // No [Permission] gate: simple lookup used to populate dropdowns app-wide.
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<IndustryTypeDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<IndustryTypeDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
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

    // No [Permission] gate: simple lookup used to populate dropdowns app-wide.
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CompanyGroupDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<CompanyGroupDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
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

[Authorize]
[Route("api/business-partners")]
public class BusinessPartnersController : BaseController
{
    private readonly IBusinessPartnerService _service;
    private readonly IValidator<CreateBusinessPartnerRequest> _createValidator;
    private readonly IValidator<UpdateBusinessPartnerRequest> _updateValidator;

    public BusinessPartnersController(
        IBusinessPartnerService service,
        IValidator<CreateBusinessPartnerRequest> createValidator,
        IValidator<UpdateBusinessPartnerRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.BusinessPartnersView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BusinessPartnerDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<BusinessPartnerDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Permission(Permissions.BusinessPartnersView)]
    [ProducesResponseType(typeof(ApiResponse<BusinessPartnerDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<BusinessPartnerDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.BusinessPartnersManage)]
    [ProducesResponseType(typeof(ApiResponse<BusinessPartnerDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateBusinessPartnerRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<BusinessPartnerDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<BusinessPartnerDto>.Ok(result, "Business partner created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.BusinessPartnersManage)]
    [ProducesResponseType(typeof(ApiResponse<BusinessPartnerDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateBusinessPartnerRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<BusinessPartnerDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<BusinessPartnerDto>.Ok(result, "Business partner updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.BusinessPartnersManage)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Business partner deleted successfully"));
    }
}
