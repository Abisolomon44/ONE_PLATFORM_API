namespace ONEERP.Platform.API.DTOs;

public class TenantDto
{
    public int TenantId { get; set; }
    public string TenantCode { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public int? PlanId { get; set; }
    public string? PlanName { get; set; }
    public string? CurrencyCode { get; set; }
    public string? ContactEmail { get; set; }
    public string? AdminUsername { get; set; }
    public string? AdminPassword { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SubscriptionStatus { get; set; }
    public DateTime? SubscriptionStart { get; set; }
    public DateTime? SubscriptionEnd { get; set; }
    public bool DatabaseProvisioned { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record CreateTenantRequest(
    string TenantName,
    string TenantCode,
    string DatabaseName,
    int PlanId,
    DateTime SubscriptionStart,
    DateTime SubscriptionEnd,
    string CompanyName,
    string AdminUsername,
    string AdminPassword,
    string? ContactEmail = null,
    string Status = "Active");

public record UpdateTenantRequest(
    string TenantName,
    string CompanyName,
    string? ContactEmail,
    string Status);

public record UpdateStatusRequest(string Status);

public class TenantConnectionDto
{
    public int ConnectionId { get; set; }
    public int TenantId { get; set; }
    public string ServerName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
