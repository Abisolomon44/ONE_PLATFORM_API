using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/payment-types")]
public class PaymentTypesController : BaseController
{
    private readonly IPaymentTypeService _service;

    public PaymentTypesController(IPaymentTypeService service) => _service = service;

    // No [Permission] gate: simple lookup used to populate dropdowns app-wide.
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentTypeDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        => Ok(ApiResponse<IEnumerable<PaymentTypeDto>>.Ok(await _service.GetAllAsync(includeInactive)));

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentTypeDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<PaymentTypeDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.PaymentTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<PaymentTypeDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentTypeRequest request)
        => Ok(ApiResponse<PaymentTypeDto>.Ok(await _service.CreateAsync(request), "Payment type created successfully"));

    [HttpPut("{id:long}")]
    [Permission(Permissions.PaymentTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<PaymentTypeDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePaymentTypeRequest request)
        => Ok(ApiResponse<PaymentTypeDto>.Ok(await _service.UpdateAsync(id, request), "Payment type updated successfully"));

    [HttpDelete("{id:long}")]
    [Permission(Permissions.PaymentTypesManage)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Payment type deleted successfully"));
    }
}

[Authorize]
[Route("api/payment-method-details")]
public class PaymentMethodDetailsController : BaseController
{
    private readonly IPaymentMethodDetailService _service;
    private readonly ICurrentUser _currentUser;

    public PaymentMethodDetailsController(IPaymentMethodDetailService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.PaymentMethodDetailsView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentMethodDetailDto>>), 200)]
    public async Task<IActionResult> GetByPaymentMethod([FromQuery] long paymentMethodId, [FromQuery] bool includeInactive = false)
        => Ok(ApiResponse<IEnumerable<PaymentMethodDetailDto>>.Ok(await _service.GetByPaymentMethodIdAsync(paymentMethodId, includeInactive)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.PaymentMethodDetailsView)]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodDetailDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<PaymentMethodDetailDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.PaymentMethodDetailsManage)]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodDetailDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentMethodDetailRequest request)
        => Ok(ApiResponse<PaymentMethodDetailDto>.Ok(await _service.CreateAsync(_currentUser.UserId, request), "Payment method detail created successfully"));

    [HttpPut("{id:long}")]
    [Permission(Permissions.PaymentMethodDetailsManage)]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodDetailDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePaymentMethodDetailRequest request)
        => Ok(ApiResponse<PaymentMethodDetailDto>.Ok(await _service.UpdateAsync(id, _currentUser.UserId, request), "Payment method detail updated successfully"));

    [HttpDelete("{id:long}")]
    [Permission(Permissions.PaymentMethodDetailsManage)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Payment method detail deleted successfully"));
    }
}

[Authorize]
[Route("api/payment-methods")]
public class PaymentMethodsController : BaseController
{
    private readonly IPaymentMethodService _service;

    public PaymentMethodsController(IPaymentMethodService service) => _service = service;

    // No [Permission] gate: simple lookup used to populate dropdowns app-wide.
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentMethodDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        => Ok(ApiResponse<IEnumerable<PaymentMethodDto>>.Ok(await _service.GetAllAsync(includeInactive)));

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<PaymentMethodDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.PaymentMethodsManage)]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentMethodRequest request)
        => Ok(ApiResponse<PaymentMethodDto>.Ok(await _service.CreateAsync(request), "Payment method created successfully"));

    [HttpPut("{id:long}")]
    [Permission(Permissions.PaymentMethodsManage)]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdatePaymentMethodRequest request)
        => Ok(ApiResponse<PaymentMethodDto>.Ok(await _service.UpdateAsync(id, request), "Payment method updated successfully"));

    [HttpDelete("{id:long}")]
    [Permission(Permissions.PaymentMethodsManage)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Payment method deleted successfully"));
    }
}
