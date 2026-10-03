using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

/// <summary>
/// Stage 5 Inventory (T074–T085). Reads are StockView; writes are StockManage.
/// All stock mutation flows through the InventoryRepository ledger writer.
/// </summary>
[Authorize]
[Route("api/inventory")]
public class InventoryController : BaseController
{
    private readonly IInventoryService _service;
    private readonly ICurrentUser _currentUser;

    public InventoryController(IInventoryService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    /// <summary>T074 — inventory dashboard KPIs.</summary>
    [HttpGet("dashboard")]
    [Permission(Permissions.StockView)]
    [ProducesResponseType(typeof(ApiResponse<InventoryDashboardDto>), 200)]
    public async Task<IActionResult> Dashboard()
        => Ok(ApiResponse<InventoryDashboardDto>.Ok(await _service.GetDashboardAsync(_currentUser.CompanyId)));

    /// <summary>T076 — post opening stock (once per product/warehouse key).</summary>
    [HttpPost("opening")]
    [Permission(Permissions.StockManage)]
    public async Task<IActionResult> Opening([FromBody] OpeningStockRequest request)
    {
        await _service.PostOpeningStockAsync(_currentUser.CompanyId, _currentUser.UserId, request);
        return Ok(ApiResponse.Ok("Opening stock recorded"));
    }

    // ---------------- T079 — Adjustment ----------------

    [HttpGet("adjustments")]
    [Permission(Permissions.StockView)]
    public async Task<IActionResult> GetAdjustments([FromQuery] int page = 1, [FromQuery] int size = 25)
        => Ok(ApiResponse<PaginatedResult<StockAdjustmentDto>>.Ok(
            await _service.GetAdjustmentsAsync(_currentUser.CompanyId, page, size)));

    [HttpPost("adjustments")]
    [Permission(Permissions.StockManage)]
    [ProducesResponseType(typeof(ApiResponse<StockAdjustmentDto>), 200)]
    public async Task<IActionResult> CreateAdjustment([FromBody] CreateStockAdjustmentRequest request)
        => Ok(ApiResponse<StockAdjustmentDto>.Ok(
            await _service.CreateAdjustmentAsync(_currentUser.CompanyId, _currentUser.UserId, request),
            "Stock adjustment created"));

    [HttpPost("adjustments/{id:long}/post")]
    [Permission(Permissions.StockManage)]
    [ProducesResponseType(typeof(ApiResponse<StockAdjustmentDto>), 200)]
    public async Task<IActionResult> PostAdjustment(long id)
        => Ok(ApiResponse<StockAdjustmentDto>.Ok(
            await _service.PostAdjustmentAsync(_currentUser.CompanyId, id, _currentUser.UserId),
            "Stock adjustment posted"));

    // ---------------- T080/T081 — Transfer ----------------

    [HttpGet("transfers")]
    [Permission(Permissions.StockView)]
    public async Task<IActionResult> GetTransfers([FromQuery] int page = 1, [FromQuery] int size = 25)
        => Ok(ApiResponse<PaginatedResult<StockTransferDto>>.Ok(
            await _service.GetTransfersAsync(_currentUser.CompanyId, page, size)));

    [HttpPost("transfers")]
    [Permission(Permissions.StockManage)]
    [ProducesResponseType(typeof(ApiResponse<StockTransferDto>), 200)]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateStockTransferRequest request)
        => Ok(ApiResponse<StockTransferDto>.Ok(
            await _service.CreateTransferAsync(_currentUser.CompanyId, _currentUser.UserId, request),
            "Stock transfer created"));

    [HttpPost("transfers/{id:long}/post")]
    [Permission(Permissions.StockManage)]
    [ProducesResponseType(typeof(ApiResponse<StockTransferDto>), 200)]
    public async Task<IActionResult> PostTransfer(long id)
        => Ok(ApiResponse<StockTransferDto>.Ok(
            await _service.PostTransferAsync(_currentUser.CompanyId, id, _currentUser.UserId),
            "Stock transfer posted"));

    // ---------------- T082 — Count ----------------

    [HttpGet("counts")]
    [Permission(Permissions.StockView)]
    public async Task<IActionResult> GetCounts([FromQuery] int page = 1, [FromQuery] int size = 25)
        => Ok(ApiResponse<PaginatedResult<StockCountDto>>.Ok(
            await _service.GetCountsAsync(_currentUser.CompanyId, page, size)));

    [HttpPost("counts")]
    [Permission(Permissions.StockManage)]
    [ProducesResponseType(typeof(ApiResponse<StockCountDto>), 200)]
    public async Task<IActionResult> CreateCount([FromBody] CreateStockCountRequest request)
        => Ok(ApiResponse<StockCountDto>.Ok(
            await _service.CreateCountAsync(_currentUser.CompanyId, _currentUser.UserId, request),
            "Stock count created"));

    [HttpPost("counts/{id:long}/post")]
    [Permission(Permissions.StockManage)]
    [ProducesResponseType(typeof(ApiResponse<StockCountDto>), 200)]
    public async Task<IActionResult> PostCount(long id)
        => Ok(ApiResponse<StockCountDto>.Ok(
            await _service.PostCountAsync(_currentUser.CompanyId, id, _currentUser.UserId),
            "Stock count posted"));

    // ---------------- T083/T084/T085 — Reports ----------------

    [HttpGet("reconciliation")]
    [Permission(Permissions.StockView)]
    public async Task<IActionResult> Reconciliation([FromQuery] long? warehouseId, [FromQuery] long? productId, [FromQuery] int page = 1, [FromQuery] int size = 50)
        => Ok(ApiResponse<PaginatedResult<StockReconciliationRow>>.Ok(
            await _service.GetReconciliationAsync(_currentUser.CompanyId, warehouseId, productId, page, size)));

    [HttpGet("valuation")]
    [Permission(Permissions.StockView)]
    public async Task<IActionResult> Valuation([FromQuery] long? warehouseId, [FromQuery] int page = 1, [FromQuery] int size = 50)
    {
        var (rows, totalValue) = await _service.GetValuationAsync(_currentUser.CompanyId, warehouseId, page, size);
        return Ok(ApiResponse<object>.Ok(new { rows, totalValue }));
    }

    [HttpGet("low-stock")]
    [Permission(Permissions.StockView)]
    public async Task<IActionResult> LowStock([FromQuery] long? warehouseId, [FromQuery] int page = 1, [FromQuery] int size = 50)
        => Ok(ApiResponse<PaginatedResult<LowStockRow>>.Ok(
            await _service.GetLowStockAsync(_currentUser.CompanyId, warehouseId, page, size)));
}
