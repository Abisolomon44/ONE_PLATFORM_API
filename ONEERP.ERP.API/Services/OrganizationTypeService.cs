using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface IOrganizationTypeService
{
    Task<IEnumerable<OrganizationTypeDto>> GetAllAsync(bool includeInactive = false);
    Task<OrganizationTypeDto> GetByIdAsync(int id);
    Task<OrganizationTypeDto> CreateAsync(CreateOrganizationTypeRequest request);
    Task<OrganizationTypeDto> UpdateAsync(int id, UpdateOrganizationTypeRequest request);
    Task<bool> DeleteAsync(int id);
}

public class OrganizationTypeService : IOrganizationTypeService
{
    private readonly IOrganizationTypeRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public OrganizationTypeService(IOrganizationTypeRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<OrganizationTypeDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(Map).ToList();
    }

    public async Task<OrganizationTypeDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Organization type '{id}' was not found.");
        return Map(item);
    }

    public async Task<OrganizationTypeDto> CreateAsync(CreateOrganizationTypeRequest request)
    {
        var name = request.Name.Trim();
        var code = request.Code.Trim().ToUpperInvariant();
        if (await _repository.GetByNameAsync(name) is not null)
            throw new DomainException($"An organization type with name '{name}' already exists.");
        if (await _repository.GetByCodeAsync(code) is not null)
            throw new DomainException($"An organization type with code '{code}' already exists.");

        var entity = new OrganizationType
        {
            Name = name,
            Code = code,
            Description = request.Description,
            SortOrder = request.SortOrder,
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        entity.OrganizationTypeId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("OrganizationType", entity.OrganizationTypeId.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<OrganizationTypeDto> UpdateAsync(int id, UpdateOrganizationTypeRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Organization type '{id}' was not found.");

        var name = request.Name.Trim();
        var code = request.Code.Trim().ToUpperInvariant();
        var duplicateName = await _repository.GetByNameAsync(name);
        if (duplicateName is not null && duplicateName.OrganizationTypeId != id)
            throw new DomainException($"An organization type with name '{name}' already exists.");
        var duplicateCode = await _repository.GetByCodeAsync(code);
        if (duplicateCode is not null && duplicateCode.OrganizationTypeId != id)
            throw new DomainException($"An organization type with code '{code}' already exists.");

        entity.Name = name;
        entity.Code = code;
        entity.Description = request.Description;
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("OrganizationType", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Organization type '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("OrganizationType", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static OrganizationTypeDto Map(OrganizationType entity) => new()
    {
        OrganizationTypeId = entity.OrganizationTypeId,
        Name = entity.Name,
        Code = entity.Code,
        Description = entity.Description,
        SortOrder = entity.SortOrder,
        IsActive = entity.IsActive
    };
}
