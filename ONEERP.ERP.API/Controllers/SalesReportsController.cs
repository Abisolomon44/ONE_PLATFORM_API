using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/sales-reports")]
public class SalesReportsController : BaseController
{
    private readonly ISalesReportService _service;
    private readonly ICurrentUser _currentUser;

    public SalesReportsController(ISalesReportService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Runs any of the 14 sales report screens. The company is always resolved
    /// server-side from the authenticated user — callers cannot cross companies
    /// by manipulating query parameters.
    /// </summary>
    [HttpGet]
    [Permission(Permissions.SalesView)]
    [ProducesResponseType(typeof(ApiResponse<SalesReportResult>), 200)]
    public async Task<IActionResult> Run([FromQuery] SalesReportFilter filter)
    {
        var result = await _service.RunAsync(_currentUser.CompanyId, filter);
        return Ok(ApiResponse<SalesReportResult>.Ok(result));
    }
}
