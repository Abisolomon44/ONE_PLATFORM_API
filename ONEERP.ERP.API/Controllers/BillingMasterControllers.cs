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
[Route("api/price-types")]
public class PriceTypesController : BaseController
{
    private readonly IPriceTypeService _service;
    private readonly IValidator<CreatePriceTypeRequest> _createValidator;
    private readonly IValidator<UpdatePriceTypeRequest> _updateValidator;

    public PriceTypesController(
        IPriceTypeService service,
        IValidator<CreatePriceTypeRequest> createValidator,
        IValidator<UpdatePriceTypeRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.PriceTypesView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PriceTypeDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        => Ok(ApiResponse<IEnumerable<PriceTypeDto>>.Ok(await _service.GetAllAsync(includeInactive)));

    [HttpGet("next-code")]
    [Permission(Permissions.PriceTypesView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> GetNextCode([FromQuery] string? prefix = null)
        => Ok(ApiResponse<string>.Ok(await _service.GetNextCodeAsync(prefix ?? "PRICETYPE")));

    [HttpGet("{id:long}")]
    [Permission(Permissions.PriceTypesView)]
    [ProducesResponseType(typeof(ApiResponse<PriceTypeDto>), 200)]
    public async Task<IActionResult> GetById(long id)
        => Ok(ApiResponse<PriceTypeDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission(Permissions.PriceTypesCreate)]
    [ProducesResponseType(typeof(ApiResponse<PriceTypeDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreatePriceTypeRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<PriceTypeDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<PriceTypeDto>.Ok(result, "Price type created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.PriceTypesEdit)]
    [ProducesResponseType(typeof(ApiResponse<PriceTypeDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePriceTypeRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<PriceTypeDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<PriceTypeDto>.Ok(result, "Price type updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.PriceTypesDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Price type deleted successfully"));
    }
}

[Authorize]
[Route("api/unit-conversions")]
public class UnitConversionsController : BaseController
{
    private readonly IUnitConversionService _service;
    private readonly IValidator<CreateUnitConversionRequest> _createValidator;
    private readonly IValidator<UpdateUnitConversionRequest> _updateValidator;
    private readonly ICurrentUser _currentUser;

    public UnitConversionsController(
        IUnitConversionService service,
        IValidator<CreateUnitConversionRequest> createValidator,
        IValidator<UpdateUnitConversionRequest> updateValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.UnitConversionsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<UnitConversionDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<UnitConversionDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Permission(Permissions.UnitConversionsView)]
    [ProducesResponseType(typeof(ApiResponse<UnitConversionDto>), 200)]
    public async Task<IActionResult> GetById(long id)
        => Ok(ApiResponse<UnitConversionDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission(Permissions.UnitConversionsCreate)]
    [ProducesResponseType(typeof(ApiResponse<UnitConversionDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateUnitConversionRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<UnitConversionDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<UnitConversionDto>.Ok(result, "Unit conversion created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.UnitConversionsEdit)]
    [ProducesResponseType(typeof(ApiResponse<UnitConversionDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateUnitConversionRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<UnitConversionDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<UnitConversionDto>.Ok(result, "Unit conversion updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.UnitConversionsDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("Unit conversion deleted successfully"));
    }
}

[Authorize]
[Route("api/barcodes")]
public class BarcodesController : BaseController
{
    private readonly IBarcodeService _service;
    private readonly IValidator<CreateBarcodeRequest> _createValidator;
    private readonly IValidator<UpdateBarcodeRequest> _updateValidator;
    private readonly ICurrentUser _currentUser;

    public BarcodesController(
        IBarcodeService service,
        IValidator<CreateBarcodeRequest> createValidator,
        IValidator<UpdateBarcodeRequest> updateValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.BarcodesView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<BarcodeDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<BarcodeDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Permission(Permissions.BarcodesView)]
    [ProducesResponseType(typeof(ApiResponse<BarcodeDto>), 200)]
    public async Task<IActionResult> GetById(long id)
        => Ok(ApiResponse<BarcodeDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission(Permissions.BarcodesCreate)]
    [ProducesResponseType(typeof(ApiResponse<BarcodeDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateBarcodeRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<BarcodeDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<BarcodeDto>.Ok(result, "Barcode created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.BarcodesEdit)]
    [ProducesResponseType(typeof(ApiResponse<BarcodeDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateBarcodeRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<BarcodeDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<BarcodeDto>.Ok(result, "Barcode updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.BarcodesDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("Barcode deleted successfully"));
    }
}

[Authorize]
[Route("api/hsn-sacs")]
public class HsnSacsController : BaseController
{
    private readonly IHsnSacService _service;
    private readonly IValidator<CreateHsnSacRequest> _createValidator;
    private readonly IValidator<UpdateHsnSacRequest> _updateValidator;
    private readonly ICurrentUser _currentUser;

    public HsnSacsController(
        IHsnSacService service,
        IValidator<CreateHsnSacRequest> createValidator,
        IValidator<UpdateHsnSacRequest> updateValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.HsnSacsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<HsnSacDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<HsnSacDto>>.Ok(result));
    }

    [HttpGet("next-code")]
    [Permission(Permissions.HsnSacsView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> GetNextCode([FromQuery] string prefix = "HSN")
        => Ok(ApiResponse<string>.Ok(await _service.GetNextCodeAsync(prefix)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.HsnSacsView)]
    [ProducesResponseType(typeof(ApiResponse<HsnSacDto>), 200)]
    public async Task<IActionResult> GetById(long id)
        => Ok(ApiResponse<HsnSacDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission(Permissions.HsnSacsCreate)]
    [ProducesResponseType(typeof(ApiResponse<HsnSacDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateHsnSacRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<HsnSacDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<HsnSacDto>.Ok(result, "HSN/SAC created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.HsnSacsEdit)]
    [ProducesResponseType(typeof(ApiResponse<HsnSacDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateHsnSacRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<HsnSacDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<HsnSacDto>.Ok(result, "HSN/SAC updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.HsnSacsDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("HSN/SAC deleted successfully"));
    }
}

[Authorize]
[Route("api/service-categories")]
public class ServiceCategoriesController : BaseController
{
    private readonly IServiceCategoryService _service;
    private readonly IValidator<CreateServiceCategoryRequest> _createValidator;
    private readonly IValidator<UpdateServiceCategoryRequest> _updateValidator;
    private readonly ICurrentUser _currentUser;

    public ServiceCategoriesController(
        IServiceCategoryService service,
        IValidator<CreateServiceCategoryRequest> createValidator,
        IValidator<UpdateServiceCategoryRequest> updateValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.ServiceCategoriesView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ServiceCategoryDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<ServiceCategoryDto>>.Ok(result));
    }

    [HttpGet("next-code")]
    [Permission(Permissions.ServiceCategoriesView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> GetNextCode([FromQuery] string prefix = "SVCAT")
        => Ok(ApiResponse<string>.Ok(await _service.GetNextCodeAsync(prefix)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.ServiceCategoriesView)]
    [ProducesResponseType(typeof(ApiResponse<ServiceCategoryDto>), 200)]
    public async Task<IActionResult> GetById(long id)
        => Ok(ApiResponse<ServiceCategoryDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission(Permissions.ServiceCategoriesCreate)]
    [ProducesResponseType(typeof(ApiResponse<ServiceCategoryDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateServiceCategoryRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ServiceCategoryDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<ServiceCategoryDto>.Ok(result, "Service category created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.ServiceCategoriesEdit)]
    [ProducesResponseType(typeof(ApiResponse<ServiceCategoryDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateServiceCategoryRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ServiceCategoryDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<ServiceCategoryDto>.Ok(result, "Service category updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.ServiceCategoriesDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("Service category deleted successfully"));
    }
}

[Authorize]
[Route("api/services")]
public class ServicesController : BaseController
{
    private readonly IServiceService _service;
    private readonly IValidator<CreateServiceRequest> _createValidator;
    private readonly IValidator<UpdateServiceRequest> _updateValidator;
    private readonly ICurrentUser _currentUser;

    public ServicesController(
        IServiceService service,
        IValidator<CreateServiceRequest> createValidator,
        IValidator<UpdateServiceRequest> updateValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.ServicesView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ServiceDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<ServiceDto>>.Ok(result));
    }

    [HttpGet("next-code")]
    [Permission(Permissions.ServicesView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> GetNextCode([FromQuery] string prefix = "SRV")
        => Ok(ApiResponse<string>.Ok(await _service.GetNextCodeAsync(prefix)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.ServicesView)]
    [ProducesResponseType(typeof(ApiResponse<ServiceDto>), 200)]
    public async Task<IActionResult> GetById(long id)
        => Ok(ApiResponse<ServiceDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission(Permissions.ServicesCreate)]
    [ProducesResponseType(typeof(ApiResponse<ServiceDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ServiceDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<ServiceDto>.Ok(result, "Service created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.ServicesEdit)]
    [ProducesResponseType(typeof(ApiResponse<ServiceDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateServiceRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ServiceDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<ServiceDto>.Ok(result, "Service updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.ServicesDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("Service deleted successfully"));
    }
}

[Authorize]
[Route("api/price-lists")]
public class PriceListsController : BaseController
{
    private readonly IPriceListService _service;
    private readonly IPriceListDetailService _detailService;
    private readonly IValidator<CreatePriceListRequest> _createValidator;
    private readonly IValidator<UpdatePriceListRequest> _updateValidator;
    private readonly IValidator<PriceListDetailsReplaceRequest> _replaceValidator;
    private readonly ICurrentUser _currentUser;

    public PriceListsController(
        IPriceListService service,
        IPriceListDetailService detailService,
        IValidator<CreatePriceListRequest> createValidator,
        IValidator<UpdatePriceListRequest> updateValidator,
        IValidator<PriceListDetailsReplaceRequest> replaceValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _detailService = detailService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _replaceValidator = replaceValidator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.PriceListsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PriceListDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<PriceListDto>>.Ok(result));
    }

    [HttpGet("next-code")]
    [Permission(Permissions.PriceListsView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> GetNextCode([FromQuery] string prefix = "PL")
        => Ok(ApiResponse<string>.Ok(await _service.GetNextCodeAsync(prefix)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.PriceListsView)]
    [ProducesResponseType(typeof(ApiResponse<PriceListDto>), 200)]
    public async Task<IActionResult> GetById(long id)
        => Ok(ApiResponse<PriceListDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpGet("{id:long}/details")]
    [Permission(Permissions.PriceListsView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PriceListDetailDto>>), 200)]
    public async Task<IActionResult> GetDetails(long id)
        => Ok(ApiResponse<IEnumerable<PriceListDetailDto>>.Ok(await _detailService.GetByPriceListAsync(id)));

    [HttpPost("{id:long}/details/replace")]
    [Permission(Permissions.PriceListsEdit)]
    [ProducesResponseType(typeof(ApiResponse<int>), 200)]
    public async Task<IActionResult> ReplaceDetails(long id, [FromBody] PriceListDetailsReplaceRequest request)
    {
        var errors = await ValidateAsync(_replaceValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<int>.Fail("Validation failed", errors));
        var count = await _detailService.ReplaceAsync(_currentUser.CompanyId, id, request);
        return Ok(ApiResponse<int>.Ok(count, "Price list details replaced successfully"));
    }

    [HttpPost]
    [Permission(Permissions.PriceListsCreate)]
    [ProducesResponseType(typeof(ApiResponse<PriceListDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreatePriceListRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<PriceListDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<PriceListDto>.Ok(result, "Price list created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.PriceListsEdit)]
    [ProducesResponseType(typeof(ApiResponse<PriceListDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePriceListRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<PriceListDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<PriceListDto>.Ok(result, "Price list updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.PriceListsDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("Price list deleted successfully"));
    }
}

[Authorize]
[Route("api/price-list-details")]
public class PriceListDetailsController : BaseController
{
    private readonly IPriceListDetailService _service;
    private readonly IValidator<CreatePriceListDetailRequest> _createValidator;
    private readonly IValidator<UpdatePriceListDetailRequest> _updateValidator;
    private readonly ICurrentUser _currentUser;

    public PriceListDetailsController(
        IPriceListDetailService service,
        IValidator<CreatePriceListDetailRequest> createValidator,
        IValidator<UpdatePriceListDetailRequest> updateValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
    }

    [HttpGet("{id:long}")]
    [Permission(Permissions.PriceListsView)]
    [ProducesResponseType(typeof(ApiResponse<PriceListDetailDto>), 200)]
    public async Task<IActionResult> GetById(long id)
        => Ok(ApiResponse<PriceListDetailDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission(Permissions.PriceListsCreate)]
    [ProducesResponseType(typeof(ApiResponse<PriceListDetailDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreatePriceListDetailRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<PriceListDetailDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<PriceListDetailDto>.Ok(result, "Price list detail created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.PriceListsEdit)]
    [ProducesResponseType(typeof(ApiResponse<PriceListDetailDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePriceListDetailRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<PriceListDetailDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<PriceListDetailDto>.Ok(result, "Price list detail updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.PriceListsDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Price list detail deleted successfully"));
    }
}

[Authorize]
[Route("api/discount-rules")]
public class DiscountRulesController : BaseController
{
    private readonly IDiscountRuleService _service;
    private readonly IValidator<CreateDiscountRuleRequest> _createValidator;
    private readonly IValidator<UpdateDiscountRuleRequest> _updateValidator;
    private readonly ICurrentUser _currentUser;

    public DiscountRulesController(
        IDiscountRuleService service,
        IValidator<CreateDiscountRuleRequest> createValidator,
        IValidator<UpdateDiscountRuleRequest> updateValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.DiscountRulesView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<DiscountRuleDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<DiscountRuleDto>>.Ok(result));
    }

    [HttpGet("next-code")]
    [Permission(Permissions.DiscountRulesView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> GetNextCode([FromQuery] string prefix = "DR")
        => Ok(ApiResponse<string>.Ok(await _service.GetNextCodeAsync(prefix)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.DiscountRulesView)]
    [ProducesResponseType(typeof(ApiResponse<DiscountRuleDto>), 200)]
    public async Task<IActionResult> GetById(long id)
        => Ok(ApiResponse<DiscountRuleDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission(Permissions.DiscountRulesCreate)]
    [ProducesResponseType(typeof(ApiResponse<DiscountRuleDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateDiscountRuleRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<DiscountRuleDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<DiscountRuleDto>.Ok(result, "Discount rule created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.DiscountRulesEdit)]
    [ProducesResponseType(typeof(ApiResponse<DiscountRuleDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateDiscountRuleRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<DiscountRuleDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<DiscountRuleDto>.Ok(result, "Discount rule updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.DiscountRulesDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("Discount rule deleted successfully"));
    }
}

[Authorize]
[Route("api/offers")]
public class OffersController : BaseController
{
    private readonly IOfferService _service;
    private readonly IOfferDetailService _detailService;
    private readonly IValidator<CreateOfferRequest> _createValidator;
    private readonly IValidator<UpdateOfferRequest> _updateValidator;
    private readonly IValidator<OfferDetailsReplaceRequest> _replaceValidator;
    private readonly ICurrentUser _currentUser;

    public OffersController(
        IOfferService service,
        IOfferDetailService detailService,
        IValidator<CreateOfferRequest> createValidator,
        IValidator<UpdateOfferRequest> updateValidator,
        IValidator<OfferDetailsReplaceRequest> replaceValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _detailService = detailService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _replaceValidator = replaceValidator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.OffersView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<OfferDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<OfferDto>>.Ok(result));
    }

    [HttpGet("next-code")]
    [Permission(Permissions.OffersView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> GetNextCode([FromQuery] string prefix = "OFFER")
        => Ok(ApiResponse<string>.Ok(await _service.GetNextCodeAsync(prefix)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.OffersView)]
    [ProducesResponseType(typeof(ApiResponse<OfferDto>), 200)]
    public async Task<IActionResult> GetById(long id)
        => Ok(ApiResponse<OfferDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpGet("{id:long}/details")]
    [Permission(Permissions.OffersView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OfferDetailDto>>), 200)]
    public async Task<IActionResult> GetDetails(long id)
        => Ok(ApiResponse<IEnumerable<OfferDetailDto>>.Ok(await _detailService.GetByOfferAsync(id)));

    [HttpPost("{id:long}/details/replace")]
    [Permission(Permissions.OffersEdit)]
    [ProducesResponseType(typeof(ApiResponse<int>), 200)]
    public async Task<IActionResult> ReplaceDetails(long id, [FromBody] OfferDetailsReplaceRequest request)
    {
        var errors = await ValidateAsync(_replaceValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<int>.Fail("Validation failed", errors));
        var count = await _detailService.ReplaceAsync(_currentUser.CompanyId, id, request);
        return Ok(ApiResponse<int>.Ok(count, "Offer details replaced successfully"));
    }

    [HttpPost]
    [Permission(Permissions.OffersCreate)]
    [ProducesResponseType(typeof(ApiResponse<OfferDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateOfferRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<OfferDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<OfferDto>.Ok(result, "Offer created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.OffersEdit)]
    [ProducesResponseType(typeof(ApiResponse<OfferDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateOfferRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<OfferDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<OfferDto>.Ok(result, "Offer updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.OffersDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("Offer deleted successfully"));
    }
}

[Authorize]
[Route("api/offer-details")]
public class OfferDetailsController : BaseController
{
    private readonly IOfferDetailService _service;
    private readonly IValidator<CreateOfferDetailRequest> _createValidator;
    private readonly IValidator<UpdateOfferDetailRequest> _updateValidator;
    private readonly ICurrentUser _currentUser;

    public OfferDetailsController(
        IOfferDetailService service,
        IValidator<CreateOfferDetailRequest> createValidator,
        IValidator<UpdateOfferDetailRequest> updateValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
    }

    [HttpGet("{id:long}")]
    [Permission(Permissions.OffersView)]
    [ProducesResponseType(typeof(ApiResponse<OfferDetailDto>), 200)]
    public async Task<IActionResult> GetById(long id)
        => Ok(ApiResponse<OfferDetailDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission(Permissions.OffersCreate)]
    [ProducesResponseType(typeof(ApiResponse<OfferDetailDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateOfferDetailRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<OfferDetailDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<OfferDetailDto>.Ok(result, "Offer detail created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.OffersEdit)]
    [ProducesResponseType(typeof(ApiResponse<OfferDetailDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateOfferDetailRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<OfferDetailDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<OfferDetailDto>.Ok(result, "Offer detail updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.OffersDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Offer detail deleted successfully"));
    }
}

[Authorize]
[Route("api/coupons")]
public class CouponsController : BaseController
{
    private readonly ICouponService _service;
    private readonly IValidator<CreateCouponRequest> _createValidator;
    private readonly IValidator<UpdateCouponRequest> _updateValidator;
    private readonly ICurrentUser _currentUser;

    public CouponsController(
        ICouponService service,
        IValidator<CreateCouponRequest> createValidator,
        IValidator<UpdateCouponRequest> updateValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.CouponsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<CouponDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<CouponDto>>.Ok(result));
    }

    [HttpGet("next-code")]
    [Permission(Permissions.CouponsView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> GetNextCode([FromQuery] string prefix = "COUPON")
        => Ok(ApiResponse<string>.Ok(await _service.GetNextCodeAsync(prefix)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.CouponsView)]
    [ProducesResponseType(typeof(ApiResponse<CouponDto>), 200)]
    public async Task<IActionResult> GetById(long id)
        => Ok(ApiResponse<CouponDto>.Ok(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Permission(Permissions.CouponsCreate)]
    [ProducesResponseType(typeof(ApiResponse<CouponDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateCouponRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CouponDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<CouponDto>.Ok(result, "Coupon created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.CouponsEdit)]
    [ProducesResponseType(typeof(ApiResponse<CouponDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCouponRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CouponDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<CouponDto>.Ok(result, "Coupon updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.CouponsDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("Coupon deleted successfully"));
    }
}