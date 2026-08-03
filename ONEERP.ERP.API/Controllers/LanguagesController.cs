using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/languages")]
public class LanguagesController : BaseController
{
    private readonly ILanguageService _service;
    private readonly IValidator<CreateLanguageRequest> _createValidator;
    private readonly IValidator<UpdateLanguageRequest> _updateValidator;

    public LanguagesController(
        ILanguageService service,
        IValidator<CreateLanguageRequest> createValidator,
        IValidator<UpdateLanguageRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.LanguagesView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LanguageDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<LanguageDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.LanguagesView)]
    [ProducesResponseType(typeof(ApiResponse<LanguageDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<LanguageDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.LanguagesManage)]
    [ProducesResponseType(typeof(ApiResponse<LanguageDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateLanguageRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<LanguageDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<LanguageDto>.Ok(result, "Language created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.LanguagesManage)]
    [ProducesResponseType(typeof(ApiResponse<LanguageDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLanguageRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<LanguageDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<LanguageDto>.Ok(result, "Language updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.LanguagesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Language deleted successfully"));
    }
}
