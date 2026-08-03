using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface IDocumentTypeService
{
    Task<IEnumerable<DocumentTypeDto>> GetAllAsync(bool includeInactive = false);
    Task<DocumentTypeDto> GetByIdAsync(int id);
    Task<DocumentTypeDto> CreateAsync(CreateDocumentTypeRequest request);
    Task<DocumentTypeDto> UpdateAsync(int id, UpdateDocumentTypeRequest request);
    Task<bool> DeleteAsync(int id);
}

public class DocumentTypeService : IDocumentTypeService
{
    private readonly IDocumentTypeRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public DocumentTypeService(IDocumentTypeRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<DocumentTypeDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(Map).ToList();
    }

    public async Task<DocumentTypeDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Document type '{id}' was not found.");
        return Map(item);
    }

    public async Task<DocumentTypeDto> CreateAsync(CreateDocumentTypeRequest request)
    {
        var name = request.Name.Trim();
        if (await _repository.GetByNameAsync(name) is not null)
            throw new DomainException($"A document type with name '{name}' already exists.");

        var entity = new DocumentType
        {
            Name = name,
            Description = request.Description,
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        entity.DocumentTypeId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("DocumentType", entity.DocumentTypeId.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<DocumentTypeDto> UpdateAsync(int id, UpdateDocumentTypeRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Document type '{id}' was not found.");

        var name = request.Name.Trim();
        var duplicate = await _repository.GetByNameAsync(name);
        if (duplicate is not null && duplicate.DocumentTypeId != id)
            throw new DomainException($"A document type with name '{name}' already exists.");

        entity.Name = name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("DocumentType", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Document type '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("DocumentType", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static DocumentTypeDto Map(DocumentType entity) => new()
    {
        DocumentTypeId = entity.DocumentTypeId,
        Name = entity.Name,
        Description = entity.Description,
        IsActive = entity.IsActive
    };
}
