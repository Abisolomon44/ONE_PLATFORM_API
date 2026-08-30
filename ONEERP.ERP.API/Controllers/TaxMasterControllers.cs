using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[ApiController]
[Route("api/tax-type-systems")]
[Authorize]
public class TaxTypeSystemsController : ControllerBase
{
    private readonly ITaxTypeSystemService _service;
    public TaxTypeSystemsController(ITaxTypeSystemService service) => _service = service;

    [HttpGet]
    [Permission(Permissions.TaxTypeSystemsView)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<TaxTypeSystemDto>>>> GetPaged(
        [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var data = await _service.GetPagedAsync(page, size, search);
        return Ok(ApiResponse<PaginatedResult<TaxTypeSystemDto>>.Ok(data));
    }

    [HttpGet("{id:long}")]
    [Permission(Permissions.TaxTypeSystemsView)]
    public async Task<ActionResult<ApiResponse<TaxTypeSystemDto>>> GetById(long id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity == null) return NotFound(ApiResponse<TaxTypeSystemDto>.Fail("Tax type system not found."));
        return Ok(ApiResponse<TaxTypeSystemDto>.Ok(entity));
    }

    [HttpGet("next-code")]
    [Permission(Permissions.TaxTypeSystemsView)]
    public async Task<IActionResult> GetNextCode()
    {
        var code = await _service.GetNextCodeAsync();
        return Ok(ApiResponse<string>.Ok(code));
    }

    [HttpPost]
    [Permission(Permissions.TaxTypeSystemsCreate)]
    public async Task<ActionResult<ApiResponse<TaxTypeSystemDto>>> Create([FromBody] CreateTaxTypeSystemRequest request)
    {
        try
        {
            var entity = await _service.CreateAsync(request);
            return Ok(ApiResponse<TaxTypeSystemDto>.Ok(entity, "Tax type system created."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TaxTypeSystemDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.TaxTypeSystemsEdit)]
    public async Task<ActionResult<ApiResponse<TaxTypeSystemDto>>> Update(long id, [FromBody] UpdateTaxTypeSystemRequest request)
    {
        try
        {
            var entity = await _service.UpdateAsync(id, request);
            return Ok(ApiResponse<TaxTypeSystemDto>.Ok(entity, "Tax type system updated."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaxTypeSystemDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TaxTypeSystemDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.TaxTypeSystemsDelete)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(long id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(new { }, "Tax type system deleted."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }
}

[ApiController]
[Route("api/taxes")]
[Authorize]
public class TaxesController : ControllerBase
{
    private readonly ITaxService _service;
    private readonly ICurrentUser _currentUser;
    public TaxesController(ITaxService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Permission(Permissions.TaxesView)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<TaxDto>>>> GetPaged(
        [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var data = await _service.GetPagedAsync(_currentUser.CompanyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<TaxDto>>.Ok(data));
    }

    [HttpGet("{id:long}")]
    [Permission(Permissions.TaxesView)]
    public async Task<ActionResult<ApiResponse<TaxDto>>> GetById(long id)
    {
        try
        {
            var entity = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<TaxDto>.Ok(entity));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<TaxDto>.Fail(ex.Message));
        }
    }

    [HttpGet("next-code")]
    [Permission(Permissions.TaxesView)]
    public async Task<IActionResult> GetNextCode()
    {
        var code = await _service.GetNextCodeAsync();
        return Ok(ApiResponse<string>.Ok(code));
    }

    [HttpPost]
    [Permission(Permissions.TaxesCreate)]
    public async Task<ActionResult<ApiResponse<TaxDto>>> Create([FromBody] CreateTaxRequest request)
    {
        try
        {
            var entity = await _service.CreateAsync(_currentUser.CompanyId, request);
            return Ok(ApiResponse<TaxDto>.Ok(entity, "Tax created."));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<TaxDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.TaxesEdit)]
    public async Task<ActionResult<ApiResponse<TaxDto>>> Update(long id, [FromBody] UpdateTaxRequest request)
    {
        try
        {
            var entity = await _service.UpdateAsync(id, _currentUser.CompanyId, request);
            return Ok(ApiResponse<TaxDto>.Ok(entity, "Tax updated."));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<TaxDto>.Fail(ex.Message));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<TaxDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.TaxesDelete)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(long id)
    {
        try
        {
            await _service.DeleteAsync(id, _currentUser.CompanyId);
            return Ok(ApiResponse<object>.Ok(new { }, "Tax deleted."));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
