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
[Route("api/purchases")]
public class PurchasesController : BaseController
{
    private readonly IPurchaseService _service;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessPartnerService _businessPartnerService;
    private readonly IProductService _productService;
    private readonly IProductUnitService _unitService;
    private readonly IPaymentTypeService _paymentTypeService;
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly IBranchService _branchService;
    private readonly IWarehouseService _warehouseService;
    private readonly ICompanyService _companyService;
    private readonly ITaxService _taxService;
    private readonly IPriceListService _priceListService;
    private readonly IPriceListDetailService _priceListDetailService;
    private readonly IDataScopeResolver _dataScopeResolver;
    private readonly IIdempotencyService _idempotency;

    public PurchasesController(
        IPurchaseService service,
        ICurrentUser currentUser,
        IBusinessPartnerService businessPartnerService,
        IProductService productService,
        IProductUnitService unitService,
        IPaymentTypeService paymentTypeService,
        IPaymentMethodService paymentMethodService,
        IBranchService branchService,
        IWarehouseService warehouseService,
        ICompanyService companyService,
        ITaxService taxService,
        IPriceListService priceListService,
        IPriceListDetailService priceListDetailService,
        IDataScopeResolver dataScopeResolver,
        IIdempotencyService idempotency)
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
        _taxService = taxService;
        _priceListService = priceListService;
        _priceListDetailService = priceListDetailService;
        _dataScopeResolver = dataScopeResolver;
        _idempotency = idempotency;
    }

    [HttpGet]
    [Permission(Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PurchaseDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<PurchaseDto>>.Ok(result));
    }

    /// <summary>
    /// T060 — purchase context lookups. The requested company (when any) is
    /// authorized against the caller's data scope; suppliers come from the
    /// company-scoped VENDOR role lookup, mirroring the sales flow.
    /// </summary>
    [HttpGet("lookups")]
    [Permission(Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete)]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Lookups([FromQuery] int? companyId = null, [FromQuery] long? priceListId = null)
    {
        var effectiveCompanyId = companyId is > 0 ? companyId.Value : _currentUser.CompanyId;
        if (!await _dataScopeResolver.CanAccessCompanyAsync(effectiveCompanyId))
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail($"You do not have access to company {effectiveCompanyId}."));

        // T062 — supplier-specific pricing: each supplier carries its default price list.
        var suppliers = (await _businessPartnerService.GetByRoleCodeAsync(effectiveCompanyId, "VENDOR", true))
            .Select(p => new SupplierLookupItem { Id = p.Id, Code = p.PartnerCode, Name = p.PartnerName, PriceListId = p.PriceListId }).ToList();
        var productsPaged = await _productService.GetPagedAsync(effectiveCompanyId, 1, 10000, "");
        var _taxRates = (await _taxService.GetPagedAsync(effectiveCompanyId, 1, 10000, ""))
            .Items.ToDictionary(t => t.Id, t => t.TaxRate);
        var products = productsPaged.Items
            .Select(p =>
            {
                var taxRate = p.TaxId.HasValue
                    ? _taxRates.TryGetValue(p.TaxId.Value, out var tr) ? tr : 0
                    : 0;
                return new ProductLookupItem
                {
                    Id = p.Id,
                    Code = p.ProductCode,
                    Name = p.ProductName,
                    UomId = p.UOMId > 0 ? p.UOMId : null,
                    UomName = p.UOMName,
                    HsnCode = p.HsnSacCode,
                    GstRate = taxRate,
                    Barcode = p.Barcode,
                    PurchasePrice = p.PurchasePrice,
                    SalesPrice = p.SalesPrice,
                };
            }).ToList();
        var units = (await _unitService.GetAllAsync(effectiveCompanyId, true))
            .Select(u => new LookupItem { Id = u.Id, Code = u.UnitCode, Name = u.UnitName }).ToList();
        var paymentTypes = (await _paymentTypeService.GetAllAsync(true))
            .Select(t => new LookupItem { Id = t.PaymentTypeId, Code = t.Code, Name = t.Name }).ToList();
        var paymentMethods = (await _paymentMethodService.GetAllAsync(true))
            .Select(m => new LookupItem { Id = m.PaymentMethodId, Code = m.Code, Name = m.Name }).ToList();
        var branches = (await _branchService.GetPagedAsync(effectiveCompanyId, 1, 1000, "")).Items
            .Select(b => new LookupItem { Id = b.Id, Code = b.BranchCode, Name = b.BranchName }).ToList();
        // T061 — warehouses must belong to the effective company, not merely the caller's scope.
        var warehouses = (await _warehouseService.GetPagedAsync(effectiveCompanyId, 0, 1, 10000, "")).Items
            .Select(w => new LookupItem { Id = w.Id, Code = w.WarehouseCode, Name = w.WarehouseName }).ToList();
        // Only companies the caller may actually transact for.
        var allowedCompanyIds = await _dataScopeResolver.GetAllowedCompanyIdsAsync();
        var companies = (await _companyService.GetPagedAsync(1, 1000, "")).Items
            .Where(c => allowedCompanyIds is null || allowedCompanyIds.Contains(c.Id))
            .Select(c => new LookupItem { Id = c.Id, Code = c.CompanyCode, Name = c.CompanyName }).ToList();
        // T062 — price lists (company-scoped) plus the requested list's rates for rate seeding.
        var priceLists = (await _priceListService.GetAllAsync(effectiveCompanyId, false))
            .Select(pl => new LookupItem { Id = pl.PriceListId, Code = pl.Code, Name = pl.Name }).ToList();
        var requestedListId = priceListId ?? suppliers.Select(s => s.PriceListId).FirstOrDefault(id => id.HasValue);
        var priceListRates = new Dictionary<long, decimal>();
        if (requestedListId.HasValue && requestedListId.Value > 0
            && priceLists.Any(pl => pl.Id == requestedListId.Value))
        {
            foreach (var d in await _priceListDetailService.GetByPriceListAsync(requestedListId.Value))
                priceListRates.TryAdd(d.ProductId, d.Price);
        }

        return Ok(ApiResponse<object>.Ok(new
        {
            Suppliers = suppliers,
            Products = products,
            Units = units,
            PaymentTypes = paymentTypes,
            PaymentMethods = paymentMethods,
            Branches = branches,
            Warehouses = warehouses,
            Companies = companies,
            PriceLists = priceLists,
            PriceListRates = priceListRates,
            PriceListId = requestedListId,
            CurrentCompanyId = effectiveCompanyId,
        }));
    }

    [HttpGet("next-number")]
    [Permission(Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> NextNumber()
        => Ok(ApiResponse<string>.Ok(await _service.GetNextPurchaseNoAsync(_currentUser.CompanyId)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<PurchaseDto>.Fail("Purchase not found"));
        return Ok(ApiResponse<PurchaseDto>.Ok(result));
    }

    [HttpGet("{id:long}/items")]
    [Permission(Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete)]
    [ProducesResponseType(typeof(ApiResponse<List<PurchaseItemDto>>), 200)]
    public async Task<IActionResult> GetItems(long id)
        => Ok(ApiResponse<List<PurchaseItemDto>>.Ok(await _service.GetItemsAsync(id)));

    [HttpGet("{id:long}/payments")]
    [Permission(Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete)]
    [ProducesResponseType(typeof(ApiResponse<List<PaymentAllocationDto>>), 200)]
    public async Task<IActionResult> GetPayments(long id)
        => Ok(ApiResponse<List<PaymentAllocationDto>>.Ok(await _service.GetAllocationsAsync(id)));

    [HttpGet("{id:long}/stock")]
    [Permission(Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete)]
    [ProducesResponseType(typeof(ApiResponse<List<StockTransaction>>), 200)]
    public async Task<IActionResult> GetStock(long id)
        => Ok(ApiResponse<List<StockTransaction>>.Ok(await _service.GetStockTransactionsAsync(id)));

    [HttpGet("{id:long}/delete-check")]
    [Permission(Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesDelete)]
    [ProducesResponseType(typeof(ApiResponse<DeleteCheckDto>), 200)]
    public async Task<IActionResult> DeleteCheck(long id)
        => Ok(ApiResponse<DeleteCheckDto>.Ok(await _service.GetDeleteCheckAsync(id)));

    [HttpPost]
    [Permission(Permissions.PurchasesManage, Permissions.PurchasesCreate)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null)
    {
        const string endpoint = "POST:/api/purchases";
        var begin = await _idempotency.TryBeginCreateAsync(_currentUser.CompanyId, endpoint, idempotencyKey);
        if (begin.InProgress)
            return Conflict(ApiResponse<PurchaseDto>.Fail("A request with this Idempotency-Key is still processing. Retry shortly."));
        if (begin.ReferenceId is long priorId)
        {
            var prior = await _service.GetByIdAsync(priorId);
            if (prior is null || prior.CompanyId != _currentUser.CompanyId)
                return NotFound(ApiResponse<PurchaseDto>.Fail("The idempotent purchase result is not available in this company."));
            return Ok(ApiResponse<PurchaseDto>.Ok(prior, "Purchase already created for this request."));
        }
        try
        {
            var result = await _service.CreateAsync(_currentUser.CompanyId, _currentUser.UserId, request);
            await _idempotency.CompleteAsync(_currentUser.CompanyId, endpoint, idempotencyKey ?? string.Empty, result.PurchaseId);
            return Ok(ApiResponse<PurchaseDto>.Ok(result, "Purchase created successfully"));
        }
        catch
        {
            await _idempotency.AbandonAsync(_currentUser.CompanyId, endpoint, idempotencyKey);
            throw;
        }
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.PurchasesManage, Permissions.PurchasesEdit)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePurchaseRequest request)
        => Ok(ApiResponse<PurchaseDto>.Ok(await _service.UpdateAsync(id, _currentUser.UserId, request), "Purchase updated successfully"));

    [HttpPost("{id:long}/cancel")]
    [Permission(Permissions.PurchasesManage, Permissions.PurchasesCancel)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseDto>), 200)]
    public async Task<IActionResult> Cancel(long id, [FromBody] CancelTransactionRequest request)
        => Ok(ApiResponse<PurchaseDto>.Ok(await _service.CancelAsync(id, _currentUser.UserId, request.Reason), "Purchase cancelled successfully"));

    [HttpDelete("{id:long}")]
    [Permission(Permissions.PurchasesManage, Permissions.PurchasesDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Purchase deleted successfully"));
    }
}
