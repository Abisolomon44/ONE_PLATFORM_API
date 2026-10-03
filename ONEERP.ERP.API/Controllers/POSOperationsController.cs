using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

/// <summary>
/// Stage 3 POS operations (T041–T055). Every command re-validates the session
/// server-side — the frontend never holds session authority.
/// </summary>
[Authorize]
[Route("api/pos")]
public class POSOperationsController : BaseController
{
    private readonly IPOSOperationsService _service;
    private readonly ICurrentUser _currentUser;

    public POSOperationsController(IPOSOperationsService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    /// <summary>T041 — POS dashboard for the current operator's session.</summary>
    [HttpGet("dashboard")]
    [Permission(Permissions.SalesPOSView, Permissions.POSSessionView)]
    [ProducesResponseType(typeof(ApiResponse<POSDashboardDto>), 200)]
    public async Task<IActionResult> Dashboard([FromQuery] long? branchId)
        => Ok(ApiResponse<POSDashboardDto>.Ok(await _service.GetDashboardAsync(_currentUser.CompanyId, branchId)));

    /// <summary>T043/T044 — current OPEN session with validation chain
    /// (JWT → company → counter → OPEN). 400 when none.</summary>
    [HttpGet("sessions/current")]
    [Permission(Permissions.SalesPOSView, Permissions.POSSessionView)]
    [ProducesResponseType(typeof(ApiResponse<POSSession>), 200)]
    public async Task<IActionResult> CurrentSession([FromQuery] long? counterId)
    {
        var session = await _service.GetCurrentSessionAsync(_currentUser.CompanyId, counterId);
        return Ok(ApiResponse<POSSession>.Ok(session));
    }

    /// <summary>T053 — single authoritative close command (optimistic concurrency).</summary>
    [HttpPost("sessions/{id:long}/close")]
    [Permission(Permissions.POSSessionEdit)]
    [ProducesResponseType(typeof(ApiResponse<POSSession>), 200)]
    public async Task<IActionResult> CloseSession(long id, [FromBody] ClosePOSSessionRequest request)
        => Ok(ApiResponse<POSSession>.Ok(
            await _service.CloseSessionAsync(id, _currentUser.UserId, request),
            "POS session closed successfully"));

    /// <summary>T049 — Cash In on an OPEN session.</summary>
    [HttpPost("cash-in")]
    [Permission(Permissions.SalesPOSView, Permissions.SalesManage)]
    [ProducesResponseType(typeof(ApiResponse<POSCashMovementDto>), 200)]
    public async Task<IActionResult> CashIn([FromBody] POSCashMovementRequest request)
        => Ok(ApiResponse<POSCashMovementDto>.Ok(
            await _service.AddCashMovementAsync(_currentUser.CompanyId, _currentUser.UserId, request, "IN"),
            "Cash in recorded successfully"));

    /// <summary>T050 — Cash Out on an OPEN session.</summary>
    [HttpPost("cash-out")]
    [Permission(Permissions.SalesPOSView, Permissions.SalesManage)]
    [ProducesResponseType(typeof(ApiResponse<POSCashMovementDto>), 200)]
    public async Task<IActionResult> CashOut([FromBody] POSCashMovementRequest request)
        => Ok(ApiResponse<POSCashMovementDto>.Ok(
            await _service.AddCashMovementAsync(_currentUser.CompanyId, _currentUser.UserId, request, "OUT"),
            "Cash out recorded successfully"));

    /// <summary>T047 — persist a held cart (replaces browser-tab holds).</summary>
    [HttpPost("holds")]
    [Permission(Permissions.SalesPOSView, Permissions.SalesManage)]
    [ProducesResponseType(typeof(ApiResponse<POSHoldBillDto>), 200)]
    public async Task<IActionResult> Hold([FromBody] CreatePOSHoldRequest request)
        => Ok(ApiResponse<POSHoldBillDto>.Ok(
            await _service.CreateHoldAsync(_currentUser.CompanyId, _currentUser.UserId, request),
            "Bill held successfully"));

    /// <summary>T048 — list HELD bills (scoped to company, optional branch).</summary>
    [HttpGet("holds")]
    [Permission(Permissions.SalesPOSView, Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<POSHoldBillDto>>), 200)]
    public async Task<IActionResult> GetHolds([FromQuery] long branchId = 0, [FromQuery] int page = 1, [FromQuery] int size = 25)
    {
        var (items, total) = await _service.GetHoldsAsync(_currentUser.CompanyId, branchId, page, size);
        return Ok(ApiResponse<PaginatedResult<POSHoldBillDto>>.Ok(new PaginatedResult<POSHoldBillDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = size,
        }));
    }

    /// <summary>T048 — recall a held bill; returns the cart JSON. Never auto-posts.</summary>
    [HttpPost("holds/{id:long}/recall")]
    [Permission(Permissions.SalesPOSView, Permissions.SalesManage)]
    [ProducesResponseType(typeof(ApiResponse<POSHoldBillDto>), 200)]
    public async Task<IActionResult> Recall(long id)
        => Ok(ApiResponse<POSHoldBillDto>.Ok(
            await _service.RecallHoldAsync(id, _currentUser.UserId, _currentUser.CompanyId),
            "Held bill recalled"));

    [HttpPost("holds/{id:long}/cancel")]
    [Permission(Permissions.SalesPOSView, Permissions.SalesManage)]
    public async Task<IActionResult> CancelHold(long id)
    {
        await _service.CancelHoldAsync(id, _currentUser.UserId, _currentUser.CompanyId);
        return Ok(ApiResponse.Ok("Held bill cancelled"));
    }

    /// <summary>T055 — shift summary for a session.</summary>
    [HttpGet("sessions/{id:long}/summary")]
    [Permission(Permissions.SalesPOSView, Permissions.POSSessionView)]
    [ProducesResponseType(typeof(ApiResponse<POSShiftSummaryDto>), 200)]
    public async Task<IActionResult> ShiftSummary(long id)
        => Ok(ApiResponse<POSShiftSummaryDto>.Ok(await _service.GetShiftSummaryAsync(_currentUser.CompanyId, id)));
}
