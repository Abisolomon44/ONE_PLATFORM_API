using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/stock")]
public class StockController : BaseController
{
    private readonly IStockService _service;
    private readonly ICurrentUser _currentUser;

    public StockController(IStockService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.StockView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<StockDto>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int size = 10,
        [FromQuery] string search = "", [FromQuery] long? warehouseId = null, [FromQuery] long? productId = null)
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search, warehouseId, productId);
        return Ok(ApiResponse<PaginatedResult<StockDto>>.Ok(result));
    }

    [HttpGet("transactions")]
    [Permission(Permissions.StockView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<StockTransactionDto>>), 200)]
    public async Task<IActionResult> Transactions(
        [FromQuery] int page = 1, [FromQuery] int size = 10,
        [FromQuery] long? productId = null, [FromQuery] long? warehouseId = null)
    {
        var result = await _service.GetTransactionsAsync(_currentUser.CompanyId, page, size, productId, warehouseId);
        return Ok(ApiResponse<PaginatedResult<StockTransactionDto>>.Ok(result));
    }

    [HttpGet("price-master-products")]
    [Permission(Permissions.StockView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PriceMasterProductDto>>), 200)]
    public async Task<IActionResult> GetPriceMasterProducts(
        [FromQuery] long? companyId = null,
        [FromQuery] long? branchId = null,
        [FromQuery] long? warehouseId = null,
        [FromQuery] string? search = null,
        [FromQuery] long? categoryId = null,
        [FromQuery] int page = 1,
        [FromQuery] int size = 500)
    {
        var resolvedCompanyId = (companyId.HasValue && companyId.Value > 0) ? companyId.Value : _currentUser.CompanyId;
        var result = await _service.GetPriceMasterPurchaseProductsAsync(
            resolvedCompanyId,
            branchId,
            warehouseId,
            search,
            categoryId,
            page,
            size);

        return Ok(ApiResponse<PaginatedResult<PriceMasterProductDto>>.Ok(result));
    }
}
