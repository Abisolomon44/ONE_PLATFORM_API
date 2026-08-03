using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.Platform.API.DTOs;
using ONEERP.Platform.API.Services;
using ONEERP.Shared.Models;

namespace ONEERP.Platform.API.Controllers;

[Authorize]
public class PlansController : BaseController
{
    private readonly IPlanService _planService;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<CreatePlanRequest> _createValidator;
    private readonly IValidator<UpdatePlanRequest> _updateValidator;

    public PlansController(
        IPlanService planService,
        ICurrentUser currentUser,
        IValidator<CreatePlanRequest> createValidator,
        IValidator<UpdatePlanRequest> updateValidator)
    {
        _planService = planService;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PlanDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _planService.GetPagedAsync(page, size, search);
        return Ok(ApiResponse<PaginatedResult<PlanDto>>.Ok(result));
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PlanDto>>), 200)]
    public async Task<IActionResult> GetActive()
    {
        var result = await _planService.GetActiveAsync();
        return Ok(ApiResponse<IEnumerable<PlanDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PlanDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _planService.GetByIdAsync(id);
        return Ok(ApiResponse<PlanDto>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PlanDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreatePlanRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<PlanDto>.Fail("Validation failed", errors));

        var result = await _planService.CreateAsync(request, _currentUser.Username);
        return Ok(ApiResponse<PlanDto>.Ok(result, "Plan created successfully"));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PlanDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlanRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<PlanDto>.Fail("Validation failed", errors));

        var result = await _planService.UpdateAsync(id, request, _currentUser.Username);
        return Ok(ApiResponse<PlanDto>.Ok(result, "Plan updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _planService.DeleteAsync(id, _currentUser.Username);
        return Ok(ApiResponse.Ok("Plan deleted successfully"));
    }
}
