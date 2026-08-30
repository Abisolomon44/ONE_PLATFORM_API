namespace ONEERP.ERP.API.DTOs;

public class TenantConfigurationDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string ApplicationType { get; set; } = string.Empty;
    public string? TransactionType { get; set; }
    public string? FlowType { get; set; }
    public string? PageCode { get; set; }
    public string? FieldCode { get; set; }
    public int? SequenceNo { get; set; }
    public bool IsPageEnabled { get; set; }
    public bool IsVisible { get; set; }
    public bool IsRequired { get; set; }
    public bool IsReadonly { get; set; }
    public int? DisplayOrder { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsActive { get; set; }
}

public record CreateTenantConfigurationRequest(
    string ApplicationType,
    string? TransactionType,
    string? FlowType,
    string? PageCode,
    string? FieldCode,
    int? SequenceNo,
    bool IsPageEnabled = true,
    bool IsVisible = true,
    bool IsRequired = false,
    bool IsReadonly = false,
    int? DisplayOrder = null,
    string? DefaultValue = null,
    bool IsActive = true);

public record UpdateTenantConfigurationRequest(
    string ApplicationType,
    string? TransactionType,
    string? FlowType,
    string? PageCode,
    string? FieldCode,
    int? SequenceNo,
    bool IsPageEnabled,
    bool IsVisible,
    bool IsRequired,
    bool IsReadonly,
    int? DisplayOrder,
    string? DefaultValue,
    bool IsActive);

/* ---------------- Grouped view (for the settings UI) ---------------- */

public class TenantConfigPageDto
{
    public string? PageCode { get; set; }
    public List<TenantConfigurationDto> Fields { get; set; } = new();
}

public class TenantConfigFlowDto
{
    public string? FlowType { get; set; }
    public List<TenantConfigPageDto> Pages { get; set; } = new();
}

public class TenantConfigTransactionDto
{
    public string? TransactionType { get; set; }
    public List<TenantConfigFlowDto> Flows { get; set; } = new();
}

public class TenantConfigApplicationDto
{
    public string ApplicationType { get; set; } = string.Empty;
    public List<TenantConfigTransactionDto> Transactions { get; set; } = new();
}
