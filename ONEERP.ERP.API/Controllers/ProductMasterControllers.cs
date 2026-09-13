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
[Route("api/product-categories")]
public class ProductCategoriesController : BaseController
{
    private readonly IProductCategoryService _service;
    private readonly IValidator<CreateProductCategoryRequest> _createValidator;
    private readonly IValidator<UpdateProductCategoryRequest> _updateValidator;
    private readonly ICurrentUser _currentUser;

    public ProductCategoriesController(
        IProductCategoryService service,
        IValidator<CreateProductCategoryRequest> createValidator,
        IValidator<UpdateProductCategoryRequest> updateValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.ProductCategoriesView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ProductCategoryDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<ProductCategoryDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.ProductCategoriesView)]
    [ProducesResponseType(typeof(ApiResponse<ProductCategoryDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<ProductCategoryDto>.Ok(result));
    }

    [HttpGet("next-code")]
    [Permission(Permissions.ProductCategoriesView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> GetNextCode()
    {
        var code = await _service.GetNextCodeAsync();
        return Ok(ApiResponse<string>.Ok(code));
    }

    [HttpPost]
    [Permission(Permissions.ProductCategoriesCreate)]
    [ProducesResponseType(typeof(ApiResponse<ProductCategoryDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ProductCategoryDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<ProductCategoryDto>.Ok(result, "Product category created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.ProductCategoriesEdit)]
    [ProducesResponseType(typeof(ApiResponse<ProductCategoryDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductCategoryRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ProductCategoryDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<ProductCategoryDto>.Ok(result, "Product category updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.ProductCategoriesDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("Product category deleted successfully"));
    }
}

[Authorize]
[Route("api/product-subcategories")]
public class ProductSubCategoriesController : BaseController
{
    private readonly IProductSubCategoryService _service;
    private readonly IValidator<CreateProductSubCategoryRequest> _createValidator;
    private readonly IValidator<UpdateProductSubCategoryRequest> _updateValidator;
    private readonly ICurrentUser _currentUser;

    public ProductSubCategoriesController(
        IProductSubCategoryService service,
        IValidator<CreateProductSubCategoryRequest> createValidator,
        IValidator<UpdateProductSubCategoryRequest> updateValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.ProductSubCategoriesView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ProductSubCategoryDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<ProductSubCategoryDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.ProductSubCategoriesView)]
    [ProducesResponseType(typeof(ApiResponse<ProductSubCategoryDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<ProductSubCategoryDto>.Ok(result));
    }

    [HttpGet("next-code")]
    [Permission(Permissions.ProductSubCategoriesView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> GetNextCode()
    {
        var code = await _service.GetNextCodeAsync();
        return Ok(ApiResponse<string>.Ok(code));
    }

    [HttpPost]
    [Permission(Permissions.ProductSubCategoriesCreate)]
    [ProducesResponseType(typeof(ApiResponse<ProductSubCategoryDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateProductSubCategoryRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ProductSubCategoryDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<ProductSubCategoryDto>.Ok(result, "Product sub category created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.ProductSubCategoriesEdit)]
    [ProducesResponseType(typeof(ApiResponse<ProductSubCategoryDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductSubCategoryRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ProductSubCategoryDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<ProductSubCategoryDto>.Ok(result, "Product sub category updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.ProductSubCategoriesDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("Product sub category deleted successfully"));
    }
}

[Authorize]
[Route("api/brands")]
public class ProductBrandsController : BaseController
{
    private readonly IProductBrandService _service;
    private readonly IValidator<CreateProductBrandRequest> _createValidator;
    private readonly IValidator<UpdateProductBrandRequest> _updateValidator;
    private readonly ICurrentUser _currentUser;

    public ProductBrandsController(
        IProductBrandService service,
        IValidator<CreateProductBrandRequest> createValidator,
        IValidator<UpdateProductBrandRequest> updateValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.BrandsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ProductBrandDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<ProductBrandDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.BrandsView)]
    [ProducesResponseType(typeof(ApiResponse<ProductBrandDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<ProductBrandDto>.Ok(result));
    }

    [HttpGet("next-code")]
    [Permission(Permissions.BrandsView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> GetNextCode()
    {
        var code = await _service.GetNextCodeAsync();
        return Ok(ApiResponse<string>.Ok(code));
    }

    [HttpPost]
    [Permission(Permissions.BrandsCreate)]
    [ProducesResponseType(typeof(ApiResponse<ProductBrandDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateProductBrandRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ProductBrandDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<ProductBrandDto>.Ok(result, "Brand created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.BrandsEdit)]
    [ProducesResponseType(typeof(ApiResponse<ProductBrandDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductBrandRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ProductBrandDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<ProductBrandDto>.Ok(result, "Brand updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.BrandsDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("Brand deleted successfully"));
    }
}

[Authorize]
[Route("api/units")]
public class ProductUnitsController : BaseController
{
    private readonly IProductUnitService _service;
    private readonly IValidator<CreateProductUnitRequest> _createValidator;
    private readonly IValidator<UpdateProductUnitRequest> _updateValidator;
    private readonly ICurrentUser _currentUser;

    public ProductUnitsController(
        IProductUnitService service,
        IValidator<CreateProductUnitRequest> createValidator,
        IValidator<UpdateProductUnitRequest> updateValidator,
        ICurrentUser currentUser)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.UnitsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ProductUnitDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<ProductUnitDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.UnitsView)]
    [ProducesResponseType(typeof(ApiResponse<ProductUnitDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<ProductUnitDto>.Ok(result));
    }

    [HttpGet("next-code")]
    [Permission(Permissions.UnitsView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> GetNextCode()
    {
        var code = await _service.GetNextCodeAsync();
        return Ok(ApiResponse<string>.Ok(code));
    }

    [HttpPost]
    [Permission(Permissions.UnitsCreate)]
    [ProducesResponseType(typeof(ApiResponse<ProductUnitDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateProductUnitRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ProductUnitDto>.Fail("Validation failed", errors));
        var result = await _service.CreateAsync(_currentUser.CompanyId, request);
        return Ok(ApiResponse<ProductUnitDto>.Ok(result, "Unit created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.UnitsEdit)]
    [ProducesResponseType(typeof(ApiResponse<ProductUnitDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductUnitRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ProductUnitDto>.Fail("Validation failed", errors));
        var result = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
        return Ok(ApiResponse<ProductUnitDto>.Ok(result, "Unit updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.UnitsDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("Unit deleted successfully"));
    }
}

[Authorize]
[Route("api/products")]
public class ProductsController : BaseController
{
    private readonly IProductService _service;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;
    private readonly ICurrentUser _currentUser;
    private readonly IDataScopeResolver _dataScopeResolver;

    public ProductsController(
        IProductService service,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator,
        ICurrentUser currentUser,
        IDataScopeResolver dataScopeResolver)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
        _dataScopeResolver = dataScopeResolver;
    }

    private async Task<long> ResolveCompanyIdAsync(long requestedCompanyId)
    {
        var companyIdResolved = requestedCompanyId > 0 ? requestedCompanyId : _currentUser.CompanyId;
        if (!await _dataScopeResolver.CanAccessCompanyAsync((int)companyIdResolved))
            throw new ONEERP.Shared.Exceptions.UnauthorizedAccess("You do not have access to the selected company.");
        return companyIdResolved;
    }

    [HttpGet]
    [Permission(Permissions.ProductsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<ProductDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "", [FromQuery] long? companyId = null)
    {
        var resolvedCompanyId = (companyId.HasValue && companyId.Value > 0) ? companyId.Value : _currentUser.CompanyId;
        if (!await _dataScopeResolver.CanAccessCompanyAsync((int)resolvedCompanyId))
            return Ok(ApiResponse<PaginatedResult<ProductDto>>.Ok(new PaginatedResult<ProductDto>
            {
                Items = new List<ProductDto>(),
                TotalCount = 0,
                PageNumber = page < 1 ? 1 : page,
                PageSize = size < 1 ? 10 : size
            }));
        var result = await _service.GetPagedAsync(resolvedCompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<ProductDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.ProductsView)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<ProductDto>.Ok(result));
    }

    [HttpGet("next-code")]
    [Permission(Permissions.ProductsView)]
    public async Task<IActionResult> GetNextCode()
    {
        var code = await _service.GetNextCodeAsync();
        return Ok(ApiResponse<string>.Ok(code));
    }

    [HttpPost]
    [Permission(Permissions.ProductsCreate)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ProductDto>.Fail("Validation failed", errors));
        var companyIdResolved = await ResolveCompanyIdAsync(request.CompanyId);
        var result = await _service.CreateAsync(companyIdResolved, request);
        return Ok(ApiResponse<ProductDto>.Ok(result, "Product created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.ProductsEdit)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<ProductDto>.Fail("Validation failed", errors));
        var companyIdResolved = await ResolveCompanyIdAsync(request.CompanyId);
        var result = await _service.UpdateAsync(id, companyIdResolved, request);
        return Ok(ApiResponse<ProductDto>.Ok(result, "Product updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.ProductsDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("Product deleted successfully"));
    }
}
