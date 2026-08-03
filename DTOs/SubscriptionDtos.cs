namespace ONEERP.Platform.API.DTOs;

public class SubscriptionDto
{
    public int SubscriptionId { get; set; }
    public int TenantId { get; set; }
    public string TenantCode { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public int PlanId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public record CreateSubscriptionRequest(
    int TenantId,
    int PlanId,
    DateTime StartDate,
    DateTime EndDate,
    decimal Amount,
    string Status = "Active");

public record UpdateSubscriptionRequest(
    int PlanId,
    DateTime StartDate,
    DateTime EndDate,
    decimal Amount,
    string Status);
