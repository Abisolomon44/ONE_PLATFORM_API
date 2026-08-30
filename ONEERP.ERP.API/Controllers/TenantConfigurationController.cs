using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/tenant-configuration")]
public class TenantConfigurationController : BaseController
{
    private readonly ITenantConfigurationService _service;
    private readonly ICurrentUser _currentUser;

    public TenantConfigurationController(ITenantConfigurationService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.TenantConfigView)]
    [ProducesResponseType(typeof(ApiResponse<List<TenantConfigurationDto>>), 200)]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<TenantConfigurationDto>>.Ok(await _service.GetAllAsync(_currentUser.TenantId)));

    [HttpGet("grouped")]
    [Permission(Permissions.TenantConfigView)]
    [ProducesResponseType(typeof(ApiResponse<List<TenantConfigApplicationDto>>), 200)]
    public async Task<IActionResult> GetGrouped()
        => Ok(ApiResponse<List<TenantConfigApplicationDto>>.Ok(await _service.GetGroupedAsync(_currentUser.TenantId)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.TenantConfigView)]
    [ProducesResponseType(typeof(ApiResponse<TenantConfigurationDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<TenantConfigurationDto>.Fail("Configuration not found"));
        return Ok(ApiResponse<TenantConfigurationDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.TenantConfigManage)]
    [ProducesResponseType(typeof(ApiResponse<TenantConfigurationDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateTenantConfigurationRequest request)
    {
        var result = await _service.CreateAsync(_currentUser.TenantId, _currentUser.UserId, request);
        return Ok(ApiResponse<TenantConfigurationDto>.Ok(result, "Configuration created successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.TenantConfigManage)]
    [ProducesResponseType(typeof(ApiResponse<TenantConfigurationDto>), 200)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateTenantConfigurationRequest request)
    {
        var result = await _service.UpdateAsync(id, _currentUser.UserId, request);
        return Ok(ApiResponse<TenantConfigurationDto>.Ok(result, "Configuration updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.TenantConfigManage)]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Configuration deleted successfully"));
    }
}
