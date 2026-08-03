using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface ILanguageService
{
    Task<IEnumerable<LanguageDto>> GetAllAsync(bool includeInactive = false);
    Task<LanguageDto> GetByIdAsync(int id);
    Task<LanguageDto> CreateAsync(CreateLanguageRequest request);
    Task<LanguageDto> UpdateAsync(int id, UpdateLanguageRequest request);
    Task<bool> DeleteAsync(int id);
}

public class LanguageService : ILanguageService
{
    private readonly ILanguageRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public LanguageService(ILanguageRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<LanguageDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(Map).ToList();
    }

    public async Task<LanguageDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Language '{id}' was not found.");
        return Map(item);
    }

    public async Task<LanguageDto> CreateAsync(CreateLanguageRequest request)
    {
        var code = request.Code.Trim();
        if (await _repository.GetByCodeAsync(code) is not null)
            throw new DomainException($"A language with code '{code}' already exists.");

        var entity = new Language
        {
            Name = request.Name.Trim(),
            Code = code,
            CultureCode = request.CultureCode,
            IsRTL = request.IsRTL,
            IsDefault = request.IsDefault,
            SortOrder = request.SortOrder,
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        entity.LanguageId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("Language", entity.LanguageId.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<LanguageDto> UpdateAsync(int id, UpdateLanguageRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Language '{id}' was not found.");

        var code = request.Code.Trim();
        var duplicate = await _repository.GetByCodeAsync(code);
        if (duplicate is not null && duplicate.LanguageId != id)
            throw new DomainException($"A language with code '{code}' already exists.");

        entity.Name = request.Name.Trim();
        entity.Code = code;
        entity.CultureCode = request.CultureCode;
        entity.IsRTL = request.IsRTL;
        entity.IsDefault = request.IsDefault;
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("Language", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Language '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("Language", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static LanguageDto Map(Language entity) => new()
    {
        LanguageId = entity.LanguageId,
        Name = entity.Name,
        Code = entity.Code,
        CultureCode = entity.CultureCode,
        IsRTL = entity.IsRTL,
        IsDefault = entity.IsDefault,
        SortOrder = entity.SortOrder,
        IsActive = entity.IsActive
    };
}
