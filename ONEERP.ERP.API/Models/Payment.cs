namespace ONEERP.ERP.API.Models;

public class PaymentType
{
    public long PaymentTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentMethod
{
    public long PaymentMethodId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PaymentCategory { get; set; } = string.Empty;
    public bool IsCash { get; set; }
    public bool IsCredit { get; set; }
    public bool RequiresReferenceNo { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentMethodDetail
{
    public long PaymentMethodDetailId { get; set; }
    public long PaymentMethodId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? UPIId { get; set; }
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? IFSCCode { get; set; }
    public string? TerminalName { get; set; }
    public string? CashCounterName { get; set; }
    public string? ReferenceValue { get; set; }
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public long CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class Payment
{
    public long PaymentId { get; set; }
    public long CompanyId { get; set; }
    public string PaymentNo { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public long PaymentTypeID { get; set; }
    public long PaymentMethodID { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public long ReferenceId { get; set; }
    public long? BusinessPartnerId { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Remarks { get; set; }
    public long StatusID { get; set; }
    public long CreatedByUserID { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? UpdatedByUserID { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
