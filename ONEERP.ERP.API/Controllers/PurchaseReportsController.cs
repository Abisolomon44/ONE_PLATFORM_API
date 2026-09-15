using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/purchase-reports")]
public class PurchaseReportsController : BaseController
{
    private readonly IPurchaseReportService _service;
    private readonly ICurrentUser _currentUser;

    public PurchaseReportsController(IPurchaseReportService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Runs any of the 10 purchase report screens. The company is always resolved
    /// server-side from the authenticated user — callers cannot cross companies by
    /// manipulating query parameters.
    /// </summary>
    [HttpGet]
    [Permission(Permissions.PurchasesView, Permissions.PurchasesReturnView, Permissions.PurchaseReturnView)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReportResult>), 200)]
    public async Task<IActionResult> Run([FromQuery] PurchaseReportFilter filter)
    {
        var result = await _service.RunAsync(_currentUser.CompanyId, filter);
        return Ok(ApiResponse<PurchaseReportResult>.Ok(result));
    }
}