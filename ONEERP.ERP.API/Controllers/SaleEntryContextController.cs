using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.ERP.API.Validators;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/sales")]
public class SaleEntryContextController : BaseController
{
    private readonly ISaleEntryContextService _contextService;
    private readonly IValidator<SaleEntryContextValidateRequest> _validateValidator;

    public SaleEntryContextController(
        ISaleEntryContextService contextService,
        IValidator<SaleEntryContextValidateRequest> validateValidator)
    {
        _contextService = contextService;
        _validateValidator = validateValidator;
    }

    [HttpGet("sale-entry-context")]
    [Permission(Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<SaleEntryContextResponse>), 200)]
    public async Task<IActionResult> GetSaleEntryContext(
        [FromQuery] int? companyId,
        [FromQuery] int? branchId,
        [FromQuery] int? storeId)
    {
        var result = await _contextService.GetContextAsync(companyId, branchId, storeId);
        return Ok(ApiResponse<SaleEntryContextResponse>.Ok(result));
    }

    [HttpPost("sale-entry-context/validate")]
    [Permission(Permissions.SalesManage)]
    [ProducesResponseType(typeof(ApiResponse<SaleEntryContextValidateResponse>), 200)]
    public async Task<IActionResult> ValidateSaleEntryContext([FromBody] SaleEntryContextValidateRequest request)
    {
        var errors = await ValidateAsync(_validateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<SaleEntryContextValidateResponse>.Fail("Validation failed", errors));

        var result = await _contextService.ValidateContextAsync(request);
        return Ok(ApiResponse<SaleEntryContextValidateResponse>.Ok(result));
    }
}