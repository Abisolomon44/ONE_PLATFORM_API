using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface IContactTypeService
{
    Task<IEnumerable<ContactTypeDto>> GetAllAsync(bool includeInactive = false);
    Task<ContactTypeDto> GetByIdAsync(int id);
    Task<ContactTypeDto> CreateAsync(CreateContactTypeRequest request);
    Task<ContactTypeDto> UpdateAsync(int id, UpdateContactTypeRequest request);
    Task<bool> DeleteAsync(int id);
}

public class ContactTypeService : IContactTypeService
{
    private readonly IContactTypeRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public ContactTypeService(IContactTypeRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<ContactTypeDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(Map).ToList();
    }

    public async Task<ContactTypeDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Contact type '{id}' was not found.");
        return Map(item);
    }

    public async Task<ContactTypeDto> CreateAsync(CreateContactTypeRequest request)
    {
        var name = request.Name.Trim();
        if (await _repository.GetByNameAsync(name) is not null)
            throw new DomainException($"A contact type with name '{name}' already exists.");

        var entity = new ContactType
        {
            Name = name,
            Description = request.Description,
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        entity.ContactTypeId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("ContactType", entity.ContactTypeId.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<ContactTypeDto> UpdateAsync(int id, UpdateContactTypeRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Contact type '{id}' was not found.");

        var name = request.Name.Trim();
        var duplicate = await _repository.GetByNameAsync(name);
        if (duplicate is not null && duplicate.ContactTypeId != id)
            throw new DomainException($"A contact type with name '{name}' already exists.");

        entity.Name = name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("ContactType", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Contact type '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("ContactType", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static ContactTypeDto Map(ContactType entity) => new()
    {
        ContactTypeId = entity.ContactTypeId,
        Name = entity.Name,
        Description = entity.Description,
        IsActive = entity.IsActive
    };
}
