using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface IGstRegistrationTypeService
{
    Task<IEnumerable<GstRegistrationTypeDto>> GetAllAsync(bool includeInactive = false);
    Task<GstRegistrationTypeDto> GetByIdAsync(int id);
    Task<GstRegistrationTypeDto> CreateAsync(CreateGstRegistrationTypeRequest request);
    Task<GstRegistrationTypeDto> UpdateAsync(int id, UpdateGstRegistrationTypeRequest request);
    Task<bool> DeleteAsync(int id);
}

public class GstRegistrationTypeService : IGstRegistrationTypeService
{
    private readonly IGstRegistrationTypeRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public GstRegistrationTypeService(IGstRegistrationTypeRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<GstRegistrationTypeDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(Map).ToList();
    }

    public async Task<GstRegistrationTypeDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"GST registration type '{id}' was not found.");
        return Map(item);
    }

    public async Task<GstRegistrationTypeDto> CreateAsync(CreateGstRegistrationTypeRequest request)
    {
        var name = request.Name.Trim();
        if (await _repository.GetByNameAsync(name) is not null)
            throw new DomainException($"A GST registration type with name '{name}' already exists.");

        var entity = new GstRegistrationType
        {
            Name = name,
            Description = request.Description,
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        entity.GstRegistrationTypeId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("GSTRegistrationType", entity.GstRegistrationTypeId.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<GstRegistrationTypeDto> UpdateAsync(int id, UpdateGstRegistrationTypeRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"GST registration type '{id}' was not found.");

        var name = request.Name.Trim();
        var duplicate = await _repository.GetByNameAsync(name);
        if (duplicate is not null && duplicate.GstRegistrationTypeId != id)
            throw new DomainException($"A GST registration type with name '{name}' already exists.");

        entity.Name = name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("GSTRegistrationType", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"GST registration type '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("GSTRegistrationType", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static GstRegistrationTypeDto Map(GstRegistrationType entity) => new()
    {
        GstRegistrationTypeId = entity.GstRegistrationTypeId,
        Name = entity.Name,
        Description = entity.Description,
        IsActive = entity.IsActive
    };
}
