namespace ONEERP.ERP.API.Models;

/// <summary>
/// Per-tenant configuration that drives the dynamic Purchase / Sale / Billing screens
/// (PO → GRN → Invoice, SO → Delivery → Invoice, POS, ...).
/// </summary>
public class TenantConfiguration
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string ApplicationType { get; set; } = string.Empty;
    public string? TransactionType { get; set; }
    public string? FlowType { get; set; }
    public string? PageCode { get; set; }
    public string? FieldCode { get; set; }
    public int? SequenceNo { get; set; }
    public bool IsPageEnabled { get; set; } = true;
    public bool IsVisible { get; set; } = true;
    public bool IsRequired { get; set; }
    public bool IsReadonly { get; set; }
    public int? DisplayOrder { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsActive { get; set; } = true;
    public long CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
