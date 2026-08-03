using ONEERP.Platform.API.DTOs;
using ONEERP.Platform.API.Models;
using ONEERP.Platform.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.Platform.API.Services;

public interface IPlanService
{
    Task<PaginatedResult<PlanDto>> GetPagedAsync(int pageNumber, int pageSize, string search);
    Task<PlanDto> GetByIdAsync(int planId);
    Task<IEnumerable<PlanDto>> GetActiveAsync();
    Task<PlanDto> CreateAsync(CreatePlanRequest request, string? currentUser);
    Task<PlanDto> UpdateAsync(int planId, UpdatePlanRequest request, string? currentUser);
    Task<bool> DeleteAsync(int planId, string? currentUser);
}

public class PlanService : IPlanService
{
    private readonly IPlanRepository _repository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IAuditService _auditService;

    public PlanService(IPlanRepository repository, ISubscriptionRepository subscriptionRepository, IAuditService auditService)
    {
        _repository = repository;
        _subscriptionRepository = subscriptionRepository;
        _auditService = auditService;
    }

    public async Task<PaginatedResult<PlanDto>> GetPagedAsync(int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;

        var items = await _repository.GetPagedAsync(normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(search);

        return new PaginatedResult<PlanDto>
        {
            Items = items.Select(PlanMapper.ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<PlanDto> GetByIdAsync(int planId)
    {
        var plan = await _repository.GetByIdAsync(planId)
            ?? throw new NotFoundException($"Plan '{planId}' was not found.");
        return PlanMapper.ToDto(plan);
    }

    public async Task<IEnumerable<PlanDto>> GetActiveAsync()
    {
        var plans = await _repository.GetAllAsync();
        return plans.Select(PlanMapper.ToDto);
    }

    public async Task<PlanDto> CreateAsync(CreatePlanRequest request, string? currentUser)
    {
        if (await _repository.GetByCodeAsync(request.PlanCode) is not null)
            throw new DomainException($"A plan with code '{request.PlanCode}' already exists.");

        var plan = new Plan
        {
            PlanCode = request.PlanCode,
            PlanName = request.PlanName,
            Description = request.Description,
            CountryCode = request.CountryCode,
            CountryName = request.CountryName,
            CurrencyCode = request.CurrencyCode,
            MonthlyPrice = request.MonthlyPrice,
            AnnualPrice = request.AnnualPrice,
            MaxUsers = request.MaxUsers,
            MaxCompanies = request.MaxCompanies,
            IsActive = request.IsActive,
            CreatedBy = currentUser,
            ModifiedBy = currentUser
        };

        plan.PlanId = await _repository.InsertAsync(plan);
        await _auditService.WriteAsync("Plan", plan.PlanId.ToString(), "Create", currentUser);

        return PlanMapper.ToDto(plan);
    }

    public async Task<PlanDto> UpdateAsync(int planId, UpdatePlanRequest request, string? currentUser)
    {
        var plan = await _repository.GetByIdAsync(planId)
            ?? throw new NotFoundException($"Plan '{planId}' was not found.");

        var existing = await _repository.GetByCodeAsync(request.PlanCode);
        if (existing is not null && existing.PlanId != planId)
            throw new DomainException($"A plan with code '{request.PlanCode}' already exists.");

        plan.PlanCode = request.PlanCode;
        plan.PlanName = request.PlanName;
        plan.Description = request.Description;
        plan.CountryCode = request.CountryCode;
        plan.CountryName = request.CountryName;
        plan.CurrencyCode = request.CurrencyCode;
        plan.MonthlyPrice = request.MonthlyPrice;
        plan.AnnualPrice = request.AnnualPrice;
        plan.MaxUsers = request.MaxUsers;
        plan.MaxCompanies = request.MaxCompanies;
        plan.IsActive = request.IsActive;
        plan.ModifiedBy = currentUser;

        await _repository.UpdateAsync(plan);
        await _auditService.WriteAsync("Plan", planId.ToString(), "Update", currentUser);

        return PlanMapper.ToDto(plan);
    }

    public async Task<bool> DeleteAsync(int planId, string? currentUser)
    {
        var plan = await _repository.GetByIdAsync(planId)
            ?? throw new NotFoundException($"Plan '{planId}' was not found.");

        await _repository.SoftDeleteAsync(planId, currentUser);
        await _auditService.WriteAsync("Plan", planId.ToString(), "Delete", currentUser);
        return true;
    }
}
