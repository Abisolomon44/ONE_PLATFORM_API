using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface IAddressTypeService
{
    Task<IEnumerable<AddressTypeDto>> GetAllAsync(bool includeInactive = false);
    Task<AddressTypeDto> GetByIdAsync(int id);
    Task<AddressTypeDto> CreateAsync(CreateAddressTypeRequest request);
    Task<AddressTypeDto> UpdateAsync(int id, UpdateAddressTypeRequest request);
    Task<bool> DeleteAsync(int id);
}

public class AddressTypeService : IAddressTypeService
{
    private readonly IAddressTypeRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public AddressTypeService(IAddressTypeRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<AddressTypeDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(Map).ToList();
    }

    public async Task<AddressTypeDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Address type '{id}' was not found.");
        return Map(item);
    }

    public async Task<AddressTypeDto> CreateAsync(CreateAddressTypeRequest request)
    {
        var name = request.Name.Trim();
        if (await _repository.GetByNameAsync(name) is not null)
            throw new DomainException($"An address type with name '{name}' already exists.");

        var entity = new AddressType
        {
            Name = name,
            Description = request.Description,
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        entity.AddressTypeId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("AddressType", entity.AddressTypeId.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<AddressTypeDto> UpdateAsync(int id, UpdateAddressTypeRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Address type '{id}' was not found.");

        var name = request.Name.Trim();
        var duplicate = await _repository.GetByNameAsync(name);
        if (duplicate is not null && duplicate.AddressTypeId != id)
            throw new DomainException($"An address type with name '{name}' already exists.");

        entity.Name = name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("AddressType", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Address type '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("AddressType", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static AddressTypeDto Map(AddressType entity) => new()
    {
        AddressTypeId = entity.AddressTypeId,
        Name = entity.Name,
        Description = entity.Description,
        IsActive = entity.IsActive
    };
}
