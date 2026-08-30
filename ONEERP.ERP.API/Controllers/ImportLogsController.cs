using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/import-logs")]
public class ImportLogsController : BaseController
{
    private readonly IImportLogService _service;

    public ImportLogsController(IImportLogService service)
    {
        _service = service;
    }

    [HttpGet]
    [Permission(Permissions.ImportLogsView)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await _service.GetAllAsync(page, pageSize);
        return Ok(ApiResponse<IEnumerable<ImportLogDto>>.Ok(result));
    }
}
