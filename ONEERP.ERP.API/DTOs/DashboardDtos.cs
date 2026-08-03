namespace ONEERP.ERP.API.DTOs;

public class DashboardDto
{
    public CompanyDto Company { get; set; } = new();
    public UserDto User { get; set; } = new();
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public string TenantCode { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string? PlanCode { get; set; }
    public string? PlanName { get; set; }
    public DateTime? SubscriptionEnd { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalRoles { get; set; }
    public List<RecentUserDto> RecentUsers { get; set; } = new();
}

public class RecentUserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class ProfileDto
{
    public UserDto User { get; set; } = new();
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public CompanyDto Company { get; set; } = new();
}
