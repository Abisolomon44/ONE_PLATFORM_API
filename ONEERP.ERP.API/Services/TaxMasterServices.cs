using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface ITaxTypeSystemService
{
    Task<PaginatedResult<TaxTypeSystemDto>> GetPagedAsync(int page, int size, string search);
    Task<TaxTypeSystemDto?> GetByIdAsync(long id);
    Task<IEnumerable<TaxTypeSystemDto>> GetAllAsync(bool includeInactive = false);
    Task<TaxTypeSystemDto> CreateAsync(CreateTaxTypeSystemRequest request);
    Task<TaxTypeSystemDto> UpdateAsync(long id, UpdateTaxTypeSystemRequest request);
    Task DeleteAsync(long id);
    Task<string> GetNextCodeAsync(string prefix = "TAXTYPE");
}

public class TaxTypeSystemService : ITaxTypeSystemService
{
    private readonly ITaxTypeSystemRepository _repo;
    public TaxTypeSystemService(ITaxTypeSystemRepository repo) => _repo = repo;

    public async Task<PaginatedResult<TaxTypeSystemDto>> GetPagedAsync(int page, int size, string search)
    {
        var (items, total) = await _repo.GetPagedAsync(page, size, search ?? string.Empty);
        var data = items.Select(Map).ToList();
        return new PaginatedResult<TaxTypeSystemDto>
        {
            Items = data,
            TotalCount = total,
            PageNumber = page,
            PageSize = size,
        };
    }

    public async Task<TaxTypeSystemDto?> GetByIdAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity == null ? null : Map(entity);
    }

    public async Task<IEnumerable<TaxTypeSystemDto>> GetAllAsync(bool includeInactive = false)
    {
        var (items, _) = await _repo.GetPagedAsync(1, 10000, string.Empty);
        return items.Select(Map).ToList();
    }

    public async Task<TaxTypeSystemDto> CreateAsync(CreateTaxTypeSystemRequest request)
    {
        if (await _repo.CodeInUseAsync(request.Code))
            throw new InvalidOperationException("Tax type system code already exists.");
        var entity = new TaxTypeSystem
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
        };
        var id = await _repo.InsertAsync(entity);
        entity.Id = id;
        return Map(entity);
    }

    public async Task<TaxTypeSystemDto> UpdateAsync(long id, UpdateTaxTypeSystemRequest request)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Tax type system not found.");
        if (await _repo.CodeInUseAsync(request.Code, id))
            throw new InvalidOperationException("Tax type system code already exists.");
        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        await _repo.UpdateAsync(entity);
        return Map(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Tax type system not found.");
        await _repo.SoftDeleteAsync(id);
    }

    public async Task<string> GetNextCodeAsync(string prefix = "TAXTYPE")
        => await _repo.GetNextCodeAsync(prefix);

    private static TaxTypeSystemDto Map(TaxTypeSystem e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
    };
}

/* ---------------- Taxes (rates) ---------------- */

public interface ITaxService
{
    Task<PaginatedResult<TaxDto>> GetPagedAsync(long companyId, int page, int size, string search);
    Task<TaxDto> GetByIdAsync(long id);
    Task<string> GetNextCodeAsync();
    Task<TaxDto> CreateAsync(long companyId, CreateTaxRequest request);
    Task<TaxDto> UpdateAsync(long id, long companyId, UpdateTaxRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class TaxService : ITaxService
{
    private readonly ITaxRepository _repo;
    private readonly ITaxTypeSystemRepository _taxTypeSystemRepo;
    private readonly ICurrentUser _currentUser;

    public TaxService(ITaxRepository repo, ITaxTypeSystemRepository taxTypeSystemRepo, ICurrentUser currentUser)
    {
        _repo = repo;
        _taxTypeSystemRepo = taxTypeSystemRepo;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<TaxDto>> GetPagedAsync(long companyId, int page, int size, string search)
    {
        var items = await _repo.GetPagedAsync(companyId, page, size, search);
        var total = await _repo.CountAsync(companyId, search);
        var names = await _taxTypeSystemRepo.GetNamesAsync(items.Select(i => i.TaxTypeSystemId).Distinct());
        var data = items.Select(e => Map(e, names)).ToList();
        return new PaginatedResult<TaxDto>
        {
            Items = data,
            TotalCount = total,
            PageNumber = page,
            PageSize = size,
        };
    }

    public async Task<TaxDto> GetByIdAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Tax '{id}' was not found.");
        var names = await _taxTypeSystemRepo.GetNamesAsync(new[] { e.TaxTypeSystemId });
        return Map(e, names);
    }

    public async Task<string> GetNextCodeAsync()
        => await _repo.GetNextCodeAsync(_currentUser.CompanyId);

    public async Task<TaxDto> CreateAsync(long companyId, CreateTaxRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.TaxCode)
            ? await _repo.GetNextCodeAsync(companyId)
            : request.TaxCode.Trim().ToUpper();
        if (await _repo.CodeInUseAsync(companyId, code))
            throw new DomainException($"Tax code '{code}' is already in use.");

        var e = new Tax
        {
            CompanyId = companyId,
            BranchId = request.BranchId,
            TaxTypeSystemId = request.TaxTypeSystemId,
            TaxCode = code,
            TaxName = request.TaxName.Trim(),
            TaxRate = request.TaxRate,
            IsInclusive = request.IsInclusive,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            Description = request.Description,
            IsActive = true,
        };
        var id = await _repo.InsertAsync(e);
        e.Id = id;
        var names = await _taxTypeSystemRepo.GetNamesAsync(new[] { e.TaxTypeSystemId });
        return Map(e, names);
    }

    public async Task<TaxDto> UpdateAsync(long id, long companyId, UpdateTaxRequest request)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Tax '{id}' was not found.");
        var code = string.IsNullOrWhiteSpace(request.TaxCode) ? e.TaxCode : request.TaxCode.Trim().ToUpper();
        if (code != e.TaxCode && await _repo.CodeInUseAsync(companyId, code, id))
            throw new DomainException($"Tax code '{code}' is already in use.");

        e.BranchId = request.BranchId;
        e.TaxTypeSystemId = request.TaxTypeSystemId;
        e.TaxCode = code;
        e.TaxName = request.TaxName.Trim();
        e.TaxRate = request.TaxRate;
        e.IsInclusive = request.IsInclusive;
        e.EffectiveFrom = request.EffectiveFrom;
        e.EffectiveTo = request.EffectiveTo;
        e.Description = request.Description;
        e.IsActive = request.IsActive;
        e.ModifiedBy = _currentUser.UserId;
        await _repo.UpdateAsync(e);
        var names = await _taxTypeSystemRepo.GetNamesAsync(new[] { e.TaxTypeSystemId });
        return Map(e, names);
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Tax '{id}' was not found.");
        return await _repo.SoftDeleteAsync(id, companyId, _currentUser.UserId);
    }

    private static TaxDto Map(Tax e, Dictionary<long, string> names) => new()
    {
        Id = e.Id,
        CompanyId = e.CompanyId,
        BranchId = e.BranchId,
        TaxTypeSystemId = e.TaxTypeSystemId,
        TaxCode = e.TaxCode,
        TaxName = e.TaxName,
        TaxRate = e.TaxRate,
        IsInclusive = e.IsInclusive,
        EffectiveFrom = e.EffectiveFrom,
        EffectiveTo = e.EffectiveTo,
        IsActive = e.IsActive,
        Description = e.Description,
        TaxTypeSystemName = names.TryGetValue(e.TaxTypeSystemId, out var n) ? n : null,
        CreatedAt = e.CreatedAt,
    };
}
