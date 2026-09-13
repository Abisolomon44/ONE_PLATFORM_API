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
public class EntitiesController : BaseController
{
    private readonly IEntityService _entityService;
    private readonly IValidator<CreateEntityRequest> _createValidator;

    public EntitiesController(IEntityService entityService, IValidator<CreateEntityRequest> createValidator)
    {
        _entityService = entityService;
        _createValidator = createValidator;
    }

    [HttpPost]
    [Permission(Permissions.EntitiesManage)]
    [ProducesResponseType(typeof(ApiResponse<EntityDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateEntityRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<EntityDto>.Fail("Validation failed", errors));

        var result = await _entityService.CreateAsync(request);
        return Ok(ApiResponse<EntityDto>.Ok(result, "Entity created successfully"));
    }

    [HttpGet("{id:long}")]
    [Permission(Permissions.EntitiesView)]
    [ProducesResponseType(typeof(ApiResponse<EntityDto>), 200)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _entityService.GetByIdAsync(id);
        if (result is null)
            return NotFound(ApiResponse<EntityDto>.Fail($"Entity '{id}' was not found."));
        return Ok(ApiResponse<EntityDto>.Ok(result));
    }

    [HttpPut("{id:long}/addresses")]
    [Permission(Permissions.EntitiesManage)]
    [ProducesResponseType(typeof(ApiResponse<EntityDto>), 200)]
    public async Task<IActionResult> ReplaceAddresses(long id, [FromBody] List<CreateAddressRequest> addresses)
    {
        var result = await _entityService.ReplaceAddressesAsync(id, addresses ?? new List<CreateAddressRequest>());
        return Ok(ApiResponse<EntityDto>.Ok(result, "Addresses saved"));
    }

    [HttpPut("{id:long}/contacts")]
    [Permission(Permissions.EntitiesManage)]
    [ProducesResponseType(typeof(ApiResponse<EntityDto>), 200)]
    public async Task<IActionResult> ReplaceContacts(long id, [FromBody] List<CreateContactRequest> contacts)
    {
        var result = await _entityService.ReplaceContactsAsync(id, contacts ?? new List<CreateContactRequest>());
        return Ok(ApiResponse<EntityDto>.Ok(result, "Contacts saved"));
    }

    [HttpPut("{id:long}/files")]
    [Permission(Permissions.EntitiesManage)]
    [ProducesResponseType(typeof(ApiResponse<EntityDto>), 200)]
    public async Task<IActionResult> ReplaceFiles(long id, [FromBody] List<CreateFileRequest> files)
    {
        var result = await _entityService.ReplaceFilesAsync(id, files ?? new List<CreateFileRequest>());
        return Ok(ApiResponse<EntityDto>.Ok(result, "Files saved"));
    }

    [HttpPut("{id:long}/notes")]
    [Permission(Permissions.EntitiesManage)]
    [ProducesResponseType(typeof(ApiResponse<EntityDto>), 200)]
    public async Task<IActionResult> ReplaceNotes(long id, [FromBody] List<CreateNoteRequest> notes)
    {
        var result = await _entityService.ReplaceNotesAsync(id, notes ?? new List<CreateNoteRequest>());
        return Ok(ApiResponse<EntityDto>.Ok(result, "Notes saved"));
    }

    [HttpPut("{id:long}/tags")]
    [Permission(Permissions.EntitiesManage)]
    [ProducesResponseType(typeof(ApiResponse<EntityDto>), 200)]
    public async Task<IActionResult> ReplaceTags(long id, [FromBody] List<CreateTagRequest> tags)
    {
        var result = await _entityService.ReplaceTagsAsync(id, tags ?? new List<CreateTagRequest>());
        return Ok(ApiResponse<EntityDto>.Ok(result, "Tags saved"));
    }
}