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
    private readonly IMasterExportService _exportService;
    private readonly ICurrentUser _currentUser;

    public MasterImportController(IMasterImportService service, IMasterExportService exportService, ICurrentUser currentUser)
    {
        _service = service;
        _exportService = exportService;
        _currentUser = currentUser;
    }

    [HttpGet("masters")]
    [Permission(Permissions.MasterImportView)]
    public IActionResult GetMasters()
        => Ok(ApiResponse<List<MasterImportMetaDto>>.Ok(_service.GetMasters()));

    [HttpGet("{entityName}/template")]
    [Permission(Permissions.MasterImportView)]
    public async Task<IActionResult> GetTemplate(string entityName)
    {
        var bytes = await _service.GenerateTemplateAsync(entityName, _currentUser.CompanyId);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{entityName}_template.xlsx");
    }

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

    /* ================================================================
       Export endpoints
       ================================================================ */

    [HttpGet("{entityName}/export/meta")]
    [Permission(Permissions.MasterImportView)]
    public async Task<IActionResult> GetExportMeta(string entityName)
    {
        var result = await _exportService.GetMetaAsync(entityName);
        return Ok(ApiResponse<MasterExportMetaDto>.Ok(result));
    }

    [HttpPost("{entityName}/export/options")]
    [Permission(Permissions.MasterImportView)]
    public async Task<IActionResult> GetExportOptions(string entityName)
    {
        var result = await _exportService.GetFilterOptionsAsync(entityName, _currentUser.CompanyId);
        return Ok(ApiResponse<Dictionary<string, List<ExportFilterOptionDto>>>.Ok(result));
    }

    [HttpPost("{entityName}/export/preview")]
    [Permission(Permissions.MasterImportView)]
    public async Task<IActionResult> ExportPreview([FromBody] ExportQueryDto query)
    {
        var result = await _exportService.PreviewAsync(query, _currentUser.CompanyId);
        return Ok(ApiResponse<ExportPreviewResponseDto>.Ok(result));
    }

    [HttpPost("{entityName}/export")]
    [Permission(Permissions.MasterImportView)]
    public async Task<IActionResult> Export([FromBody] ExportQueryDto query)
    {
        var result = await _exportService.ExportAsync(query, _currentUser.CompanyId);
        return File(result.Bytes, result.ContentType, result.FileName);
    }
}
