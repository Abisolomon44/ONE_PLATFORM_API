using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.Platform.API.DTOs;
using ONEERP.Platform.API.Services;
using ONEERP.Shared.Models;

namespace ONEERP.Platform.API.Controllers;

[Authorize]
public class TenantsController : BaseController
{
    private readonly ITenantService _tenantService;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<CreateTenantRequest> _createValidator;
    private readonly IValidator<UpdateTenantRequest> _updateValidator;

    public TenantsController(
        ITenantService tenantService,
        ICurrentUser currentUser,
        IValidator<CreateTenantRequest> createValidator,
        IValidator<UpdateTenantRequest> updateValidator)
    {
        _tenantService = tenantService;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<TenantDto>>), 200)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _tenantService.GetPagedAsync(page, size, search);
        return Ok(ApiResponse<PaginatedResult<TenantDto>>.Ok(result));
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TenantDto>>), 200)]
    public async Task<IActionResult> GetActive()
    {
        var result = await _tenantService.GetActiveAsync();
        return Ok(ApiResponse<IEnumerable<TenantDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _tenantService.GetByIdAsync(id);
        return Ok(ApiResponse<TenantDto>.Ok(result));
    }

    [HttpGet("{id:int}/connections")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TenantConnectionDto>>), 200)]
    public async Task<IActionResult> GetConnections(int id)
    {
        var result = await _tenantService.GetConnectionsAsync(id);
        return Ok(ApiResponse<IEnumerable<TenantConnectionDto>>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<TenantDto>.Fail("Validation failed", errors));

        var result = await _tenantService.CreateAsync(request, _currentUser.Username);
        return Ok(ApiResponse<TenantDto>.Ok(result, "Tenant created and database provisioned successfully"));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTenantRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<TenantDto>.Fail("Validation failed", errors));

        var result = await _tenantService.UpdateAsync(id, request, _currentUser.Username);
        return Ok(ApiResponse<TenantDto>.Ok(result, "Tenant updated successfully"));
    }

    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), 200)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var result = await _tenantService.UpdateStatusAsync(id, request.Status, _currentUser.Username);
        return Ok(ApiResponse<TenantDto>.Ok(result,
            result.Status == "Active" ? "Tenant activated successfully" : "Tenant deactivated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] bool dropDatabase = false)
    {
        await _tenantService.DeleteAsync(id, _currentUser.Username, dropDatabase);
        return Ok(ApiResponse.Ok(dropDatabase ? "Tenant and database deleted successfully" : "Tenant deleted successfully"));
    }
}
