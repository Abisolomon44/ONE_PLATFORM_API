using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/payments")]
public class PaymentsController : BaseController
{
    private readonly IPaymentService _service;
    private readonly ICurrentUser _currentUser;

    public PaymentsController(IPaymentService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.PaymentsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PaymentDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<PaymentDto>>.Ok(result));
    }

    [HttpGet("lookups")]
    [Permission(Permissions.PaymentsView)]
    [ProducesResponseType(typeof(ApiResponse<PaymentLookupsDto>), 200)]
    public async Task<IActionResult> Lookups()
        => Ok(ApiResponse<PaymentLookupsDto>.Ok(await _service.GetLookupsAsync(_currentUser.CompanyId)));

    [HttpGet("next-number")]
    [Permission(Permissions.PaymentsView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> NextNumber()
        => Ok(ApiResponse<string>.Ok(await _service.GetNextPaymentNoAsync(_currentUser.CompanyId)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.PaymentsView)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<PaymentDto>.Fail("Payment not found"));
        return Ok(ApiResponse<PaymentDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.PaymentsManage)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
        => Ok(ApiResponse<PaymentDto>.Ok(await _service.CreateAsync(_currentUser.CompanyId, _currentUser.UserId, request), "Payment created successfully"));

    [HttpPut("{id:long}")]
    [Permission(Permissions.PaymentsManage)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePaymentRequest request)
        => Ok(ApiResponse<PaymentDto>.Ok(await _service.UpdateAsync(id, _currentUser.UserId, request), "Payment updated successfully"));

    [HttpDelete("{id:long}")]
    [Permission(Permissions.PaymentsManage)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Payment deleted successfully"));
    }
}
