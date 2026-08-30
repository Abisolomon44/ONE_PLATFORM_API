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

    public PurchaseReturnsController(IPurchaseReturnService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.PurchasesReturnView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PurchaseReturnDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<PurchaseReturnDto>>.Ok(result));
    }

    [HttpGet("next-number")]
    [Permission(Permissions.PurchasesReturnView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> NextNumber()
        => Ok(ApiResponse<string>.Ok(await _service.GetNextReturnNoAsync(_currentUser.CompanyId)));

    [HttpGet("{id:long}")]
    [Permission(Permissions.PurchasesReturnView)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReturnDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<PurchaseReturnDto>.Fail("Purchase return not found"));
        return Ok(ApiResponse<PurchaseReturnDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.PurchasesReturnManage)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReturnDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseReturnRequest request)
        => Ok(ApiResponse<PurchaseReturnDto>.Ok(await _service.CreateAsync(_currentUser.CompanyId, _currentUser.UserId, request), "Purchase return created successfully"));
}
