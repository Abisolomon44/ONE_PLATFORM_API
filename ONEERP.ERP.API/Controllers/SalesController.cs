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
    private readonly ISalesReturnService _returnService;
    private readonly IIdempotencyService _idempotency;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessPartnerService _businessPartnerService;
    private readonly IProductService _productService;
    private readonly IProductUnitService _unitService;
    private readonly IPaymentTypeService _paymentTypeService;
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly IBranchService _branchService;
    private readonly IWarehouseService _warehouseService;
    private readonly ICompanyService _companyService;
    private readonly IDataScopeResolver _dataScopeResolver;

    public SalesController(
        ISalesService service,
        ISalesReturnService returnService,
        IIdempotencyService idempotency,
        ICurrentUser currentUser,
        IBusinessPartnerService businessPartnerService,
        IProductService productService,
        IProductUnitService unitService,
        IPaymentTypeService paymentTypeService,
        IPaymentMethodService paymentMethodService,
        IBranchService branchService,
        IWarehouseService warehouseService,
        ICompanyService companyService,
        IDataScopeResolver dataScopeResolver)
    {
        _service = service;
        _returnService = returnService;
        _idempotency = idempotency;
        _currentUser = currentUser;
        _businessPartnerService = businessPartnerService;
        _productService = productService;
        _unitService = unitService;
        _paymentTypeService = paymentTypeService;
        _paymentMethodService = paymentMethodService;
        _branchService = branchService;
        _warehouseService = warehouseService;
        _companyService = companyService;
        _dataScopeResolver = dataScopeResolver;
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
    public async Task<IActionResult> Lookups([FromQuery] int? companyId = null)
    {
        var effectiveCompanyId = companyId is > 0 ? companyId.Value : _currentUser.CompanyId;
        if (!await _dataScopeResolver.CanAccessCompanyAsync(effectiveCompanyId))
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail($"You do not have access to company {effectiveCompanyId}."));

        var customers = (await _businessPartnerService.GetByRoleCodeAsync(effectiveCompanyId, "CUSTOMER", true))
            .Select(p => new LookupItem { Id = p.Id, Code = p.PartnerCode, Name = p.PartnerName }).ToList();
        var products = (await _productService.GetPagedAsync(effectiveCompanyId, 1, 10000, "")).Items
            .Select(p => new LookupItem { Id = p.Id, Code = p.ProductCode, Name = p.ProductName }).ToList();
        var units = (await _unitService.GetAllAsync(effectiveCompanyId, true))
            .Select(u => new LookupItem { Id = u.Id, Code = u.UnitCode, Name = u.UnitName }).ToList();
        var paymentTypes = (await _paymentTypeService.GetAllAsync(true))
            .Select(t => new LookupItem { Id = t.PaymentTypeId, Code = t.Code, Name = t.Name }).ToList();
        var paymentMethods = (await _paymentMethodService.GetAllAsync(true))
            .Select(m => new LookupItem { Id = m.PaymentMethodId, Code = m.Code, Name = m.Name }).ToList();
        var branches = (await _branchService.GetPagedAsync(effectiveCompanyId, 1, 1000, "")).Items
            .Select(b => new LookupItem { Id = b.Id, Code = b.BranchCode, Name = b.BranchName }).ToList();
        var warehouses = (await _warehouseService.GetAllAsync(true))
            .Select(w => new LookupItem { Id = w.Id, Code = w.WarehouseCode, Name = w.WarehouseName }).ToList();
        var allowedCompanyIds = await _dataScopeResolver.GetAllowedCompanyIdsAsync();
        var companies = (await _companyService.GetPagedAsync(1, 1000, "")).Items
            .Where(c => allowedCompanyIds is null || allowedCompanyIds.Contains(c.Id))
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
            CurrentCompanyId = effectiveCompanyId,
        }));
    }

    [HttpGet("next-number")]
    [Permission(Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> NextNumber([FromQuery] long? companyId = null)
        => Ok(ApiResponse<string>.Ok(await _service.GetNextSalesNoAsync(companyId is > 0 ? companyId.Value : _currentUser.CompanyId)));

    [HttpGet("products/search")]
    [Permission(Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<List<SalesProductSearchDto>>), 200)]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string search = "",
        [FromQuery] long branchId = 0,
        [FromQuery] long warehouseId = 0,
        [FromQuery] long? priceListId = null,
        [FromQuery] int size = 50,
        [FromQuery] bool includeOutOfStock = false,
        [FromQuery] long? companyId = null)
        => Ok(ApiResponse<List<SalesProductSearchDto>>.Ok(await _service.SearchProductsAsync(
            companyId is > 0 ? companyId.Value : _currentUser.CompanyId,
            branchId, warehouseId, priceListId, search, size, includeOutOfStock)));

    [HttpGet("products/{productId:long}/stock")]
    [Permission(Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<List<ProductStockDto>>), 200)]
    public async Task<IActionResult> GetProductStock(long productId, [FromQuery] long? companyId = null)
        => Ok(ApiResponse<List<ProductStockDto>>.Ok(await _service.GetStockAvailabilityAsync(
            companyId is > 0 ? companyId.Value : _currentUser.CompanyId, productId)));

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

    /// <summary>Read-model for the document-designer print/preview pipeline.
    /// Returns the invoice with company / customer / items straight from the DB.</summary>
    [HttpGet("{id:long}/print-data")]
    [Permission(Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<SalesInvoicePrintDto>), 200)]
    public async Task<IActionResult> GetPrintData(long id)
    {
        var result = await _service.GetPrintDataAsync(id);
        if (result == null) return NotFound(ApiResponse<SalesInvoicePrintDto>.Fail("Sales invoice not found"));
        return Ok(ApiResponse<SalesInvoicePrintDto>.Ok(result));
    }

    /// <summary>
    /// T039 — supports an optional X-Idempotency-Key header: a retried POST
    /// with the same key replays the originally created invoice instead of
    /// creating a duplicate.
    /// </summary>
    [HttpPost]
    [Permission(Permissions.SalesManage)]
    [ProducesResponseType(typeof(ApiResponse<SalesInvoiceDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateSalesRequest request)
    {
        const string endpoint = "POST /api/sales";
        var key = Request.Headers["X-Idempotency-Key"].FirstOrDefault();
        var replay = await _idempotency.TryBeginAsync(_currentUser.CompanyId, endpoint, key ?? string.Empty);
        if (replay.HasValue)
        {
            var existing = await _service.GetByIdAsync(replay.Value);
            if (existing != null)
                return Ok(ApiResponse<SalesInvoiceDto>.Ok(existing, "Sales invoice already created (idempotent replay)"));
        }

        var created = await _service.CreateAsync(_currentUser.CompanyId, _currentUser.UserId, request);
        if (!string.IsNullOrWhiteSpace(key))
            await _idempotency.CompleteAsync(_currentUser.CompanyId, endpoint, key, created.SalesInvoiceId);
        return Ok(ApiResponse<SalesInvoiceDto>.Ok(created, "Sales invoice created successfully"));
    }

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

    /// <summary>
    /// T031 — cancel a posted invoice. Financial history is never physically
    /// deleted; the invoice is marked CANCELLED and stock is reversed.
    /// </summary>
    [HttpPost("{id:long}/cancel")]
    [Permission(Permissions.SalesManage)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> Cancel(long id, [FromBody] CancelTransactionRequest request)
    {
        await _service.CancelAsync(id, _currentUser.UserId, request.Reason);
        return Ok(ApiResponse<bool>.Ok(true, "Sales invoice cancelled successfully"));
    }

    /// <summary>T035 — replace the invoice's payments and re-derive paid/balance.</summary>
    [HttpPut("{id:long}/payment")]
    [Permission(Permissions.SalesManage)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> UpdatePayment(long id, [FromBody] UpdateSalesPaymentRequest request)
    {
        await _service.UpdatePaymentsAsync(id, _currentUser.UserId, request.Payments);
        return Ok(ApiResponse<bool>.Ok(true, "Sales payment updated successfully"));
    }

    /// <summary>T027 alias — create a return scoped to a specific invoice.</summary>
    [HttpPost("{salesInvoiceId:long}/return")]
    [Permission(Permissions.SalesReturnManage)]
    [ProducesResponseType(typeof(ApiResponse<SalesReturnDto>), 200)]
    public async Task<IActionResult> CreateReturn(long salesInvoiceId, [FromBody] CreateSalesReturnRequest request)
    {
        request.SalesInvoiceId = salesInvoiceId;
        var result = await _returnService.CreateAsync(_currentUser.CompanyId, _currentUser.UserId, request);
        return Ok(ApiResponse<SalesReturnDto>.Ok(result, "Sales return created successfully"));
    }
}
