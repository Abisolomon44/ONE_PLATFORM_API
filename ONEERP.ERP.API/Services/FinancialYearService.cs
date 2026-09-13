using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface IFinancialYearService
{
    Task<PaginatedResult<FinancialYearDto>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search);
    Task<FinancialYearDto> GetByIdAsync(long id);
    Task<IEnumerable<FinancialYearDto>> GetAllAsync(bool includeInactive);
    Task<FinancialYearDto> CreateAsync(CreateFinancialYearRequest request);
    Task<FinancialYearDto> UpdateAsync(long id, UpdateFinancialYearRequest request);
    Task<bool> DeleteAsync(long id);
}

public class FinancialYearService : IFinancialYearService
{
    private readonly IFinancialYearRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public FinancialYearService(IFinancialYearRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<FinancialYearDto>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(companyId, search);
        return new PaginatedResult<FinancialYearDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<FinancialYearDto> GetByIdAsync(long id)
    {
        var fy = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Financial year '{id}' was not found.");
        return ToDto(fy);
    }

    public async Task<IEnumerable<FinancialYearDto>> GetAllAsync(bool includeInactive)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(ToDto);
    }

    public async Task<FinancialYearDto> CreateAsync(CreateFinancialYearRequest request)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await _repository.CodeInUseAsync(request.CompanyId, code))
            throw new DomainException($"Financial year code '{code}' is already in use.");

        var fy = new FinancialYear
        {
            CompanyId = request.CompanyId,
            Code = code,
            Name = request.Name.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsCurrent = request.IsCurrent,
            IsClosed = request.IsClosed,
            IsActive = request.IsActive,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        fy.FinancialYearId = await _repository.InsertAsync(fy);
        await _auditService.WriteAsync("FinancialYear", fy.FinancialYearId.ToString(), "Create", _currentUser.Username);
        return ToDto(fy);
    }

    public async Task<FinancialYearDto> UpdateAsync(long id, UpdateFinancialYearRequest request)
    {
        var fy = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Financial year '{id}' was not found.");

        var code = request.Code.Trim().ToUpperInvariant();
        var existingByCode = await _repository.GetByCodeAsync(fy.CompanyId, code);
        if (existingByCode != null && existingByCode.FinancialYearId != id)
            throw new DomainException($"Financial year code '{code}' is already in use.");

        fy.Code = code;
        fy.Name = request.Name.Trim();
        fy.StartDate = request.StartDate;
        fy.EndDate = request.EndDate;
        fy.IsCurrent = request.IsCurrent;
        fy.IsClosed = request.IsClosed;
        fy.IsActive = request.IsActive;
        fy.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(fy);
        await _auditService.WriteAsync("FinancialYear", id.ToString(), "Update", _currentUser.Username);
        return ToDto(fy);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var fy = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Financial year '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("FinancialYear", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static FinancialYearDto ToDto(FinancialYear f) => new()
    {
        FinancialYearId = f.FinancialYearId, CompanyId = f.CompanyId, Code = f.Code, Name = f.Name,
        StartDate = f.StartDate, EndDate = f.EndDate, IsCurrent = f.IsCurrent, IsClosed = f.IsClosed,
        IsActive = f.IsActive, CreatedBy = f.CreatedBy, CreatedAt = f.CreatedAt,
        ModifiedBy = f.ModifiedBy, ModifiedAt = f.ModifiedAt
    };
}