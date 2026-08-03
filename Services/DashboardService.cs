using ONEERP.Platform.API.DTOs;
using ONEERP.Platform.API.Repositories;
using ONEERP.Shared.Constants;

namespace ONEERP.Platform.API.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}

public class DashboardService : IDashboardService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IPlanRepository _planRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public DashboardService(
        ITenantRepository tenantRepository,
        IPlanRepository planRepository,
        ISubscriptionRepository subscriptionRepository)
    {
        _tenantRepository = tenantRepository;
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var totalTenants = await _tenantRepository.CountAllAsync();
        var activeTenants = await _tenantRepository.CountByStatusAsync(TenantStatus.Active);
        var totalPlans = await _planRepository.CountActiveAsync();
        var totalSubscriptions = await _subscriptionRepository.CountAsync(string.Empty);
        var activeSubscriptions = await _subscriptionRepository.CountActiveAsync();
        var monthlyRevenue = await _subscriptionRepository.SumMonthlyAsync();
        var expiringSoon = await _subscriptionRepository.GetExpiringSoonAsync(30);
        var recentTenants = await _tenantRepository.GetPagedAsync(1, 5, string.Empty);

        return new DashboardStatsDto
        {
            TotalTenants = totalTenants,
            ActiveTenants = activeTenants,
            TotalPlans = totalPlans,
            TotalSubscriptions = totalSubscriptions,
            ActiveSubscriptions = activeSubscriptions,
            ExpiringSoonCount = expiringSoon.Count(),
            MonthlyRecurringRevenue = monthlyRevenue,
            RecentTenants = recentTenants.Select(t => new RecentTenantDto
            {
                TenantId = t.TenantId,
                TenantCode = t.TenantCode,
                TenantName = t.TenantName,
                Status = t.Status,
                CreatedDate = t.CreatedDate
            }).ToList()
        };
    }
}
