namespace ONEERP.Platform.API.DTOs;

public class DashboardStatsDto
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int TotalPlans { get; set; }
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int ExpiringSoonCount { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public List<RecentTenantDto> RecentTenants { get; set; } = new();
}

public class RecentTenantDto
{
    public int TenantId { get; set; }
    public string TenantCode { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
