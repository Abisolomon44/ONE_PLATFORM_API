using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/sales")]
public class SalesController : BaseController
{
    private readonly ISalesService _service;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessPartnerService _businessPartnerService;
    private readonly IProductService _productService;
    private readonly IProductUnitService _unitService;
    private readonly IPaymentTypeService _paymentTypeService;
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly IBranchService _branchService;
    private readonly IWarehouseService _warehouseService;
    private readonly ICompanyService _companyService;

    public SalesController(
        ISalesService service,
        ICurrentUser currentUser,
        IBusinessPartnerService businessPartnerService,
        IProductService productService,
        IProductUnitService unitService,
        IPaymentTypeService paymentTypeService,
        IPaymentMethodService paymentMethodService,
        IBranchService branchService,
        IWarehouseService warehouseService,
        ICompanyService companyService)
    {
        _service = service;
        _currentUser = currentUser;
        _businessPartnerService = businessPartnerService;
        _productService = productService;
        _unitService = unitService;
        _paymentTypeService = paymentTypeService;
        _paymentMethodService = paymentMethodService;
        _branchService = branchService;
        _warehouseService = warehouseService;
        _companyService = companyService;
    }

    [HttpGet]
    [Permission(Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<SalesInvoiceDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<SalesInvoiceDto>>.Ok(result));
    }

    [HttpGet("lookups")]
    [Permission(Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Lookups()
    {
        var customers = (await _businessPartnerService.GetAllAsync(true))
            .Select(p => new LookupItem { Id = p.Id, Code = p.PartnerCode, Name = p.PartnerName }).ToList();
        var products = (await _productService.GetPagedAsync(_currentUser.CompanyId, 1, 10000, "")).Items
            .Select(p => new LookupItem { Id = p.Id, Code = p.ProductCode, Name = p.ProductName }).ToList();
        var units = (await _unitService.GetAllAsync(_currentUser.CompanyId, true))
            .Select(u => new LookupItem { Id = u.Id, Code = u.UnitCode, Name = u.UnitName }).ToList();
        var paymentTypes = (await _paymentTypeService.GetAllAsync(true))
            .Select(t => new LookupItem { Id = t.PaymentTypeId, Code = t.Code, Name = t.Name }).ToList();
        var paymentMethods = (await _paymentMethodService.GetAllAsync(true))
            .Select(m => new LookupItem { Id = m.PaymentMethodId, Code = m.Code, Name = m.Name }).ToList();
        var branches = (await _branchService.GetPagedAsync(_currentUser.CompanyId, 1, 1000, "")).Items
            .Select(b => new LookupItem { Id = b.Id, Code = b.BranchCode, Name = b.BranchName }).ToList();
        var warehouses = (await _warehouseService.GetAllAsync(true))
            .Select(w => new LookupItem { Id = w.Id, Code = w.WarehouseCode, Name = w.WarehouseName }).ToList();
        var companies = (await _companyService.GetPagedAsync(1, 1000, "")).Items
            .Select(c => new LookupItem { Id = c.Id, Code = c.CompanyCode, Name = c.CompanyName }).ToList();
        return Ok(ApiResponse<object>.Ok(new
        {
            Customers = customers,
            Products = products,
            Units = units,
            PaymentTypes = paymentTypes,
            PaymentMethods = paymentMethods,
            Branches = branches,
            Warehouses = warehouses,
            Companies = companies,
            CurrentCompanyId = _currentUser.CompanyId,
        }));
    }

    [HttpGet("next-number")]
    [Permission(Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> NextNumber()
        => Ok(ApiResponse<string>.Ok(await _service.GetNextSalesNoAsync(_currentUser.CompanyId)));

    [HttpGet("products/{productId:long}/stock")]
    [Permission(Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<List<ProductStockDto>>), 200)]
    public async Task<IActionResult> GetProductStock(long productId)
        => Ok(ApiResponse<List<ProductStockDto>>.Ok(await _service.GetStockAvailabilityAsync(_currentUser.CompanyId, productId)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<SalesInvoiceDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<SalesInvoiceDto>.Fail("Sales invoice not found"));
        return Ok(ApiResponse<SalesInvoiceDto>.Ok(result));
    }

    [HttpGet("{id:long}/items")]
    [Permission(Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<List<SalesItemDto>>), 200)]
    public async Task<IActionResult> GetItems(long id)
        => Ok(ApiResponse<List<SalesItemDto>>.Ok(await _service.GetItemsAsync(id)));

    [HttpGet("{id:long}/payments")]
    [Permission(Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<List<PaymentAllocationDto>>), 200)]
    public async Task<IActionResult> GetPayments(long id)
        => Ok(ApiResponse<List<PaymentAllocationDto>>.Ok(await _service.GetAllocationsAsync(id)));

    [HttpGet("{id:long}/stock")]
    [Permission(Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<List<StockTransaction>>), 200)]
    public async Task<IActionResult> GetStock(long id)
        => Ok(ApiResponse<List<StockTransaction>>.Ok(await _service.GetStockTransactionsAsync(id)));

    [HttpPost]
    [Permission(Permissions.SalesManage)]
    [ProducesResponseType(typeof(ApiResponse<SalesInvoiceDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateSalesRequest request)
        => Ok(ApiResponse<SalesInvoiceDto>.Ok(await _service.CreateAsync(_currentUser.CompanyId, _currentUser.UserId, request), "Sales invoice created successfully"));

    [HttpPut("{id:long}")]
    [Permission(Permissions.SalesManage)]
    [ProducesResponseType(typeof(ApiResponse<SalesInvoiceDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateSalesRequest request)
        => Ok(ApiResponse<SalesInvoiceDto>.Ok(await _service.UpdateAsync(id, _currentUser.UserId, request), "Sales invoice updated successfully"));

    [HttpDelete("{id:long}")]
    [Permission(Permissions.SalesManage)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Sales invoice deleted successfully"));
    }
}
