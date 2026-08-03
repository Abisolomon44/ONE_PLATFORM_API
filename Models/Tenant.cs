namespace ONEERP.Platform.API.Models;

public class Tenant
{
    public int TenantId { get; set; }
    public string TenantCode { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public int? PlanId { get; set; }
    public string? ContactEmail { get; set; }
    public string? AdminUsername { get; set; }
    public string? AdminPassword { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}
