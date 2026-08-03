using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.Platform.API.DTOs;
using ONEERP.Platform.API.Services;
using ONEERP.Shared.Models;

namespace ONEERP.Platform.API.Controllers;

[Authorize]
public class SubscriptionsController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<CreateSubscriptionRequest> _createValidator;

    public SubscriptionsController(
        ISubscriptionService subscriptionService,
        ICurrentUser currentUser,
        IValidator<CreateSubscriptionRequest> createValidator)
    {
        _subscriptionService = subscriptionService;
        _currentUser = currentUser;
        _createValidator = createValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<SubscriptionDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _subscriptionService.GetPagedAsync(page, size, search);
        return Ok(ApiResponse<PaginatedResult<SubscriptionDto>>.Ok(result));
    }

    [HttpGet("tenant/{tenantId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SubscriptionDto>>), 200)]
    public async Task<IActionResult> GetByTenant(int tenantId)
    {
        var result = await _subscriptionService.GetByTenantIdAsync(tenantId);
        return Ok(ApiResponse<IEnumerable<SubscriptionDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _subscriptionService.GetByIdAsync(id);
        return Ok(ApiResponse<SubscriptionDto>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<SubscriptionDto>.Fail("Validation failed", errors));

        var result = await _subscriptionService.CreateAsync(request, _currentUser.Username);
        return Ok(ApiResponse<SubscriptionDto>.Ok(result, "Subscription created successfully"));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubscriptionRequest request)
    {
        var result = await _subscriptionService.UpdateAsync(id, request, _currentUser.Username);
        return Ok(ApiResponse<SubscriptionDto>.Ok(result, "Subscription updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _subscriptionService.DeleteAsync(id, _currentUser.Username);
        return Ok(ApiResponse.Ok("Subscription deleted successfully"));
    }
}
