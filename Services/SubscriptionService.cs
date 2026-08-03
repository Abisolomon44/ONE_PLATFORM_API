using ONEERP.Platform.API.DTOs;
using ONEERP.Platform.API.Models;
using ONEERP.Platform.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.Platform.API.Services;

public interface ISubscriptionService
{
    Task<PaginatedResult<SubscriptionDto>> GetPagedAsync(int pageNumber, int pageSize, string search);
    Task<SubscriptionDto> GetByIdAsync(int subscriptionId);
    Task<IEnumerable<SubscriptionDto>> GetByTenantIdAsync(int tenantId);
    Task<SubscriptionDto> CreateAsync(CreateSubscriptionRequest request, string? currentUser);
    Task<SubscriptionDto> UpdateAsync(int subscriptionId, UpdateSubscriptionRequest request, string? currentUser);
    Task<bool> DeleteAsync(int subscriptionId, string? currentUser);
}

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _repository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IAuditService _auditService;

    public SubscriptionService(ISubscriptionRepository repository, ITenantRepository tenantRepository, IAuditService auditService)
    {
        _repository = repository;
        _tenantRepository = tenantRepository;
        _auditService = auditService;
    }

    public async Task<PaginatedResult<SubscriptionDto>> GetPagedAsync(int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;

        var items = await _repository.GetPagedAsync(normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(search);

        return new PaginatedResult<SubscriptionDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<SubscriptionDto> GetByIdAsync(int subscriptionId)
    {
        var subscription = await _repository.GetByIdAsync(subscriptionId)
            ?? throw new NotFoundException($"Subscription '{subscriptionId}' was not found.");
        return ToDto(subscription);
    }

    public async Task<IEnumerable<SubscriptionDto>> GetByTenantIdAsync(int tenantId)
    {
        var subscriptions = await _repository.GetByTenantIdAsync(tenantId);
        return subscriptions.Select(ToDto);
    }

    public async Task<SubscriptionDto> CreateAsync(CreateSubscriptionRequest request, string? currentUser)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId)
            ?? throw new NotFoundException($"Tenant '{request.TenantId}' was not found.");

        var subscription = new Subscription
        {
            TenantId = request.TenantId,
            PlanId = request.PlanId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Amount = request.Amount,
            Status = request.Status,
            CreatedBy = currentUser,
            ModifiedBy = currentUser
        };

        subscription.SubscriptionId = await _repository.InsertAsync(subscription);
        await _tenantRepository.UpdatePlanAsync(request.TenantId, request.PlanId, null, null);
        await _auditService.WriteAsync("Subscription", subscription.SubscriptionId.ToString(), "Create", currentUser, request.TenantId);

        return ToDto(subscription);
    }

    public async Task<SubscriptionDto> UpdateAsync(int subscriptionId, UpdateSubscriptionRequest request, string? currentUser)
    {
        var subscription = await _repository.GetByIdAsync(subscriptionId)
            ?? throw new NotFoundException($"Subscription '{subscriptionId}' was not found.");

        subscription.PlanId = request.PlanId;
        subscription.StartDate = request.StartDate;
        subscription.EndDate = request.EndDate;
        subscription.Amount = request.Amount;
        subscription.Status = request.Status;
        subscription.ModifiedBy = currentUser;

        await _repository.UpdateAsync(subscription);
        await _tenantRepository.UpdatePlanAsync(subscription.TenantId, request.PlanId, null, null);
        await _auditService.WriteAsync("Subscription", subscriptionId.ToString(), "Update", currentUser, subscription.TenantId);

        return ToDto(subscription);
    }

    public async Task<bool> DeleteAsync(int subscriptionId, string? currentUser)
    {
        var subscription = await _repository.GetByIdAsync(subscriptionId)
            ?? throw new NotFoundException($"Subscription '{subscriptionId}' was not found.");

        await _repository.SoftDeleteAsync(subscriptionId, currentUser);
        await _auditService.WriteAsync("Subscription", subscriptionId.ToString(), "Delete", currentUser, subscription.TenantId);
        return true;
    }

    private static SubscriptionDto ToDto(SubscriptionRow s) => new()
    {
        SubscriptionId = s.SubscriptionId,
        TenantId = s.TenantId,
        TenantCode = s.TenantCode,
        TenantName = s.TenantName,
        PlanId = s.PlanId,
        PlanCode = s.PlanCode,
        PlanName = s.PlanName,
        StartDate = s.StartDate,
        EndDate = s.EndDate,
        Amount = s.Amount,
        Status = s.Status,
        CreatedDate = s.CreatedDate
    };

    private static SubscriptionDto ToDto(Subscription s) => new()
    {
        SubscriptionId = s.SubscriptionId,
        TenantId = s.TenantId,
        PlanId = s.PlanId,
        StartDate = s.StartDate,
        EndDate = s.EndDate,
        Amount = s.Amount,
        Status = s.Status,
        CreatedDate = s.CreatedDate
    };
}
