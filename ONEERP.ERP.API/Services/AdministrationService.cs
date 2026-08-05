using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.ERP.API.Requests;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

/// <summary>
/// Service layer for currency administration.
/// Business rules:
/// <list type="bullet">
///   <item>CurrencyCode must be unique (enforced on create and update).</item>
///   <item>Only one base currency may exist at a time.</item>
///   <item>Soft delete sets IsActive = 0.</item>
///   <item>CreatedDate and ModifiedDate are managed by the database (SYSUTCDATETIME).</item>
/// </list>
/// </summary>
public class AdministrationService : IAdministrationService
{
    private readonly IAdministrationRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public AdministrationService(IAdministrationRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<CurrencyDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(Map).ToList();
    }

    public async Task<CurrencyDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Currency '{id}' was not found.");
        return Map(item);
    }

    public async Task<CurrencyDto> CreateAsync(SaveCurrencyRequest request)
    {
        var code = request.CurrencyCode.Trim().ToUpperInvariant();
        if (await _repository.GetByCodeAsync(code) is not null)
            throw new DomainException($"A currency with code '{code}' already exists.");

        if (request.IsBaseCurrency && await HasBaseCurrencyAsync())
            throw new DomainException("Only one base currency can be defined.");

        var entity = new Currency
        {
            CurrencyCode = code,
            CurrencyName = request.CurrencyName.Trim(),
            Symbol = request.Symbol.Trim(),
            ISOCode = string.IsNullOrWhiteSpace(request.ISOCode) ? null : request.ISOCode.Trim().ToUpperInvariant(),
            DecimalPlaces = request.DecimalPlaces,
            IsBaseCurrency = request.IsBaseCurrency,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId,
        };

        entity.Id = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("Currency", entity.Id.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<CurrencyDto> UpdateAsync(int id, SaveCurrencyRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Currency '{id}' was not found.");

        var code = request.CurrencyCode.Trim().ToUpperInvariant();
        var duplicate = await _repository.GetByCodeAsync(code);
        if (duplicate is not null && duplicate.Id != id)
            throw new DomainException($"A currency with code '{code}' already exists.");

        if (request.IsBaseCurrency && await HasBaseCurrencyAsync(excludeId: id))
            throw new DomainException("Only one base currency can be defined.");

        entity.CurrencyCode = code;
        entity.CurrencyName = request.CurrencyName.Trim();
        entity.Symbol = request.Symbol.Trim();
        entity.ISOCode = string.IsNullOrWhiteSpace(request.ISOCode) ? null : request.ISOCode.Trim().ToUpperInvariant();
        entity.DecimalPlaces = request.DecimalPlaces;
        entity.IsBaseCurrency = request.IsBaseCurrency;
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("Currency", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Currency '{id}' was not found.");

        if (existing.IsBaseCurrency)
            throw new DomainException("The base currency cannot be deleted.");

        await _repository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("Currency", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private async Task<bool> HasBaseCurrencyAsync(int? excludeId = null)
    {
        var all = await _repository.GetAllAsync(true);
        return all.Any(c => c.IsBaseCurrency && c.Id != (excludeId ?? 0));
    }

    private static CurrencyDto Map(Currency entity) => new()
    {
        Id = entity.Id,
        CurrencyCode = entity.CurrencyCode,
        CurrencyName = entity.CurrencyName,
        Symbol = entity.Symbol,
        ISOCode = entity.ISOCode,
        DecimalPlaces = entity.DecimalPlaces,
        IsBaseCurrency = entity.IsBaseCurrency,
        SortOrder = entity.SortOrder,
        IsActive = entity.IsActive
    };
}
