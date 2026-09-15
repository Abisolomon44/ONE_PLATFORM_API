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
    private readonly IBusinessPartnerRoleService _businessPartnerRoleService;
    private readonly IProductService _productService;
    private readonly IProductUnitService _unitService;
    private readonly IPaymentTypeService _paymentTypeService;
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly IBranchService _branchService;
    private readonly IWarehouseService _warehouseService;
    private readonly ICompanyService _companyService;
    private readonly ITaxService _taxService;

    public PurchasesController(
        IPurchaseService service,
        ICurrentUser currentUser,
        IBusinessPartnerService businessPartnerService,
        IBusinessPartnerRoleService businessPartnerRoleService,
        IProductService productService,
        IProductUnitService unitService,
        IPaymentTypeService paymentTypeService,
        IPaymentMethodService paymentMethodService,
        IBranchService branchService,
        IWarehouseService warehouseService,
        ICompanyService companyService,
        ITaxService taxService)
    {
        _service = service;
        _currentUser = currentUser;
        _businessPartnerService = businessPartnerService;
        _businessPartnerRoleService = businessPartnerRoleService;
        _productService = productService;
        _unitService = unitService;
        _paymentTypeService = paymentTypeService;
        _paymentMethodService = paymentMethodService;
        _branchService = branchService;
        _warehouseService = warehouseService;
        _companyService = companyService;
        _taxService = taxService;
    }

    [HttpGet]
    [Permission(Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PurchaseDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<PurchaseDto>>.Ok(result));
    }

    [HttpGet("lookups")]
    [Permission(Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete)]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Lookups()
    {
        var vendorRole = (await _businessPartnerRoleService.GetAllAsync(true))
            .FirstOrDefault(r => string.Equals(r.Code, "VENDOR", StringComparison.OrdinalIgnoreCase));
        var suppliers = (await _businessPartnerService.GetAllAsync(true))
            .Where(p =>
                vendorRole is not null &&
                p.PatnerRoleIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Contains(vendorRole.BusinessPartnerRoleId.ToString()))
            .Select(p => new LookupItem { Id = p.Id, Code = p.PartnerCode, Name = p.PartnerName }).ToList();
        var productsPaged = await _productService.GetPagedAsync(_currentUser.CompanyId, 1, 10000, "");
        var _taxRates = (await _taxService.GetPagedAsync(_currentUser.CompanyId, 1, 10000, ""))
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
                };
            }).ToList();
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
            Suppliers = suppliers,
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
    public async Task<IActionResult> Create([FromBody] CreatePurchaseRequest request)
        => Ok(ApiResponse<PurchaseDto>.Ok(await _service.CreateAsync(_currentUser.CompanyId, _currentUser.UserId, request), "Purchase created successfully"));

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
