using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

/// <summary>
/// Role-based screen navigation for the signed-in user.
///
/// <see cref="MyNavigation"/> is the single endpoint the Angular sidebar and
/// workspace drill-down consume. It returns ONLY the screens the user can view
/// (CanView = any role grants the "view" action, minus explicit user-level
/// denials), nested under their workspace/domain/module/sub-module hierarchy,
/// with empty parents pruned. It never exposes the full master screen list.
/// </summary>
[ApiController]
[Route("api/permissions")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly INavigationService _service;
    private readonly ICurrentUser _user;

    public PermissionsController(INavigationService service, ICurrentUser user)
    {
        _service = service;
        _user = user;
    }

    [HttpGet("my-navigation")]
    [ProducesResponseType(typeof(ApiResponse<NavigationResponse>), 200)]
    public async Task<IActionResult> MyNavigation()
        => Ok(ApiResponse<NavigationResponse>.Ok(
            await _service.GetNavigationAsync(_user.UserId, _user.TenantId, _user.CompanyId, _user.IsSuperAdmin)));
}