using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/purchase-returns")]
public class PurchaseReturnsController : BaseController
{
    private readonly IPurchaseReturnService _service;
    private readonly ICurrentUser _currentUser;
    private readonly IIdempotencyService _idempotency;

    public PurchaseReturnsController(IPurchaseReturnService service, ICurrentUser currentUser, IIdempotencyService idempotency)
    {
        _service = service;
        _currentUser = currentUser;
        _idempotency = idempotency;
    }

    [HttpGet]
    [Permission(Permissions.PurchasesReturnView, Permissions.PurchasesReturnManage,
        Permissions.PurchaseReturnView, Permissions.PurchaseReturnCreate, Permissions.PurchaseReturnEdit,
        Permissions.PurchaseReturnCancel, Permissions.PurchaseReturnDelete)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PurchaseReturnDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<PurchaseReturnDto>>.Ok(result));
    }

    [HttpGet("next-number")]
    [Permission(Permissions.PurchasesReturnView, Permissions.PurchasesReturnManage,
        Permissions.PurchaseReturnView, Permissions.PurchaseReturnCreate, Permissions.PurchaseReturnEdit,
        Permissions.PurchaseReturnCancel, Permissions.PurchaseReturnDelete)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> NextNumber()
        => Ok(ApiResponse<string>.Ok(await _service.GetNextReturnNoAsync(_currentUser.CompanyId)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.PurchasesReturnView, Permissions.PurchasesReturnManage,
        Permissions.PurchaseReturnView, Permissions.PurchaseReturnCreate, Permissions.PurchaseReturnEdit,
        Permissions.PurchaseReturnCancel, Permissions.PurchaseReturnDelete)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReturnDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<PurchaseReturnDto>.Fail("Purchase return not found"));
        return Ok(ApiResponse<PurchaseReturnDto>.Ok(result));
    }

    [HttpGet("{id:long}/delete-check")]
    [Permission(Permissions.PurchasesReturnView, Permissions.PurchasesReturnManage,
        Permissions.PurchaseReturnView, Permissions.PurchaseReturnDelete)]
    [ProducesResponseType(typeof(ApiResponse<DeleteCheckDto>), 200)]
    public async Task<IActionResult> DeleteCheck(long id)
        => Ok(ApiResponse<DeleteCheckDto>.Ok(await _service.GetDeleteCheckAsync(id)));

    [HttpPost]
    [Permission(Permissions.PurchasesReturnManage, Permissions.PurchaseReturnCreate)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReturnDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseReturnRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null)
    {
        const string endpoint = "POST:/api/purchase-returns";
        var begin = await _idempotency.TryBeginCreateAsync(_currentUser.CompanyId, endpoint, idempotencyKey);
        if (begin.InProgress)
            return Conflict(ApiResponse<PurchaseReturnDto>.Fail("A request with this Idempotency-Key is still processing. Retry shortly."));
        if (begin.ReferenceId is long priorId)
        {
            var prior = await _service.GetByIdAsync(priorId);
            if (prior is null || prior.CompanyId != _currentUser.CompanyId)
                return NotFound(ApiResponse<PurchaseReturnDto>.Fail("The idempotent purchase-return result is not available in this company."));
            return Ok(ApiResponse<PurchaseReturnDto>.Ok(prior, "Purchase return already created for this request."));
        }
        try
        {
            var result = await _service.CreateAsync(_currentUser.CompanyId, _currentUser.UserId, request);
            await _idempotency.CompleteAsync(_currentUser.CompanyId, endpoint, idempotencyKey ?? string.Empty, result.PurchaseReturnId);
            return Ok(ApiResponse<PurchaseReturnDto>.Ok(result, "Purchase return created successfully"));
        }
        catch
        {
            await _idempotency.AbandonAsync(_currentUser.CompanyId, endpoint, idempotencyKey);
            throw;
        }
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.PurchasesReturnManage, Permissions.PurchaseReturnEdit)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReturnDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePurchaseReturnRequest request)
        => Ok(ApiResponse<PurchaseReturnDto>.Ok(await _service.UpdateAsync(id, _currentUser.UserId, request), "Purchase return updated successfully"));

    [HttpPost("{id:long}/cancel")]
    [Permission(Permissions.PurchasesReturnManage, Permissions.PurchaseReturnCancel)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReturnDto>), 200)]
    public async Task<IActionResult> Cancel(long id, [FromBody] CancelTransactionRequest request)
        => Ok(ApiResponse<PurchaseReturnDto>.Ok(await _service.CancelAsync(id, _currentUser.UserId, request.Reason), "Purchase return cancelled successfully"));

    [HttpDelete("{id:long}")]
    [Permission(Permissions.PurchasesReturnManage, Permissions.PurchaseReturnDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Purchase return deleted successfully"));
    }
}
