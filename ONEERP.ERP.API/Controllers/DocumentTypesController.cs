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
[Route("api/document-types")]
public class DocumentTypesController : BaseController
{
    private readonly IDocumentTypeService _service;
    private readonly IValidator<CreateDocumentTypeRequest> _createValidator;
    private readonly IValidator<UpdateDocumentTypeRequest> _updateValidator;

    public DocumentTypesController(
        IDocumentTypeService service,
        IValidator<CreateDocumentTypeRequest> createValidator,
        IValidator<UpdateDocumentTypeRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // No [Permission] gate: simple lookup used to populate dropdowns app-wide.
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DocumentTypeDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<DocumentTypeDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DocumentTypeDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<DocumentTypeDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.DocumentTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<DocumentTypeDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateDocumentTypeRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<DocumentTypeDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<DocumentTypeDto>.Ok(result, "Document type created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.DocumentTypesManage)]
    [ProducesResponseType(typeof(ApiResponse<DocumentTypeDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentTypeRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<DocumentTypeDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<DocumentTypeDto>.Ok(result, "Document type updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.DocumentTypesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Document type deleted successfully"));
    }
}
