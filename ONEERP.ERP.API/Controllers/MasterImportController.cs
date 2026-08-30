using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/import")]
public class MasterImportController : BaseController
{
    private readonly IMasterImportService _service;
    private readonly ICurrentUser _currentUser;

    public MasterImportController(IMasterImportService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("masters")]
    [Permission(Permissions.MasterImportView)]
    public IActionResult GetMasters()
        => Ok(ApiResponse<List<MasterImportMetaDto>>.Ok(_service.GetMasters()));

    [HttpPost("preview")]
    [Permission(Permissions.MasterImportView)]
    public async Task<IActionResult> Preview([FromBody] ImportPreviewRequest request)
    {
        var result = await _service.PreviewAsync(request.EntityName, request.Rows, _currentUser.CompanyId);
        return Ok(ApiResponse<ImportPreviewResponse>.Ok(result));
    }

    [HttpPost("confirm")]
    [Permission(Permissions.MasterImportManage)]
    public async Task<IActionResult> Confirm([FromBody] ImportConfirmRequest request)
    {
        var result = await _service.ConfirmAsync(request, _currentUser);
        return Ok(ApiResponse<ImportConfirmResponse>.Ok(result));
    }
}
