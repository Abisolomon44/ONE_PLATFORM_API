using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Repositories;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/sales-returns")]
public class SalesReturnsController : BaseController
{
    private readonly ISalesReturnService _service;
    private readonly IIdempotencyService _idempotency;
    private readonly ICurrentUser _currentUser;

    public SalesReturnsController(ISalesReturnService service, IIdempotencyService idempotency, ICurrentUser currentUser)
    {
        _service = service;
        _idempotency = idempotency;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.SalesReturnView, Permissions.SalesReturnManage)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<SalesReturnDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 25, [FromQuery] string? search = null)
        => Ok(ApiResponse<PaginatedResult<SalesReturnDto>>.Ok(await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search ?? string.Empty)));

    [HttpGet("next-number")]
    [Permission(Permissions.SalesReturnView, Permissions.SalesReturnManage)]
    public async Task<IActionResult> NextNumber()
        => Ok(ApiResponse<string>.Ok(await _service.GetNextReturnNoAsync(_currentUser.CompanyId)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.SalesReturnView, Permissions.SalesReturnManage)]
    [ProducesResponseType(typeof(ApiResponse<SalesReturnDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(ApiResponse<SalesReturnDto>.Ok(result));
    }

    /// <summary>T027/T028 — create a sales return against a posted invoice.
    /// Supports X-Idempotency-Key replay (T039).</summary>
    [HttpPost]
    [Permission(Permissions.SalesReturnManage)]
    [ProducesResponseType(typeof(ApiResponse<SalesReturnDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateSalesReturnRequest request)
    {
        const string endpoint = "POST /api/sales-returns";
        var key = Request.Headers["X-Idempotency-Key"].FirstOrDefault();
        var replay = await _idempotency.TryBeginAsync(_currentUser.CompanyId, endpoint, key ?? string.Empty);
        if (replay.HasValue)
        {
            var existing = await _service.GetByIdAsync(replay.Value);
            if (existing != null)
                return Ok(ApiResponse<SalesReturnDto>.Ok(existing, "Sales return already created (idempotent replay)"));
        }

        var created = await _service.CreateAsync(_currentUser.CompanyId, _currentUser.UserId, request);
        if (!string.IsNullOrWhiteSpace(key))
            await _idempotency.CompleteAsync(_currentUser.CompanyId, endpoint, key, created.SalesReturnId);
        return Ok(ApiResponse<SalesReturnDto>.Ok(created, "Sales return created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.SalesReturnManage)]
    [ProducesResponseType(typeof(ApiResponse<SalesReturnDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateSalesReturnRequest request)
        => Ok(ApiResponse<SalesReturnDto>.Ok(
            await _service.UpdateAsync(id, _currentUser.UserId, request),
            "Sales return updated successfully"));

    [HttpPost("{id:long}/cancel")]
    [Permission(Permissions.SalesReturnManage)]
    [ProducesResponseType(typeof(ApiResponse<SalesReturnDto>), 200)]
    public async Task<IActionResult> Cancel(long id, [FromBody] CancelTransactionRequest request)
        => Ok(ApiResponse<SalesReturnDto>.Ok(
            await _service.CancelAsync(id, _currentUser.UserId, request.Reason),
            "Sales return cancelled successfully"));
}

/// <summary>T030 — customer refunds against sales returns (Payment rows with ReferenceType='SALES_RETURN').</summary>
[Authorize]
[Route("api/payments/refunds")]
public class RefundsController : BaseController
{
    private readonly ISalesReturnService _service;
    private readonly ISalesReturnRepository _returnRepo;
    private readonly ISalesRepository _salesRepo;
    private readonly ICurrentUser _currentUser;

    public RefundsController(ISalesReturnService service, ISalesReturnRepository returnRepo,
        ISalesRepository salesRepo, ICurrentUser currentUser)
    {
        _service = service;
        _returnRepo = returnRepo;
        _salesRepo = salesRepo;
        _currentUser = currentUser;
    }

    [HttpPost]
    [Permission(Permissions.SalesReturnManage, Permissions.PaymentsManage)]
    [ProducesResponseType(typeof(ApiResponse<RefundDto>), 200)]
    public async Task<IActionResult> CreateRefund([FromBody] CreateRefundRequest request)
        => Ok(ApiResponse<RefundDto>.Ok(
            await _service.CreateRefundAsync(_currentUser.CompanyId, _currentUser.UserId, request),
            "Refund recorded successfully"));

    [HttpGet("{id:long}")]
    [Permission(Permissions.SalesReturnView, Permissions.PaymentsView)]
    [ProducesResponseType(typeof(ApiResponse<RefundDto>), 200)]
    public async Task<IActionResult> GetRefund(long id)
    {
        var ret = await _returnRepo.GetByRefundPaymentAsync(id);
        if (ret == null || ret.CompanyId != _currentUser.CompanyId) return NotFound();
        var payment = await _salesRepo.GetPaymentByIdAsync(id);
        if (payment == null) return NotFound();
        return Ok(ApiResponse<RefundDto>.Ok(new RefundDto
        {
            PaymentId = payment.PaymentId,
            PaymentNo = payment.PaymentNo,
            PaymentDate = payment.PaymentDate,
            ReferenceType = payment.ReferenceType,
            ReferenceId = payment.ReferenceId,
            BusinessPartnerId = payment.BusinessPartnerId,
            Amount = payment.Amount,
            ReferenceNo = payment.ReferenceNo,
            Remarks = payment.Remarks,
        }));
    }
}
