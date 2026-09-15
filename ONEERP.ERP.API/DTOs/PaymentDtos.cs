namespace ONEERP.ERP.API.DTOs;

/* ---------------- PaymentType ---------------- */

public record CreatePaymentTypeRequest(
    string Code,
    string Name,
    int DisplayOrder = 0,
    bool IsActive = true);

public record UpdatePaymentTypeRequest(
    string Code,
    string Name,
    int DisplayOrder,
    bool IsActive);

public class PaymentTypeDto
{
    public long PaymentTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

/* ---------------- PaymentMethod ---------------- */

public record CreatePaymentMethodRequest(
    string Code,
    string Name,
    string PaymentCategory,
    bool IsCash,
    bool IsCredit,
    bool RequiresReferenceNo,
    int DisplayOrder = 0,
    bool IsActive = true);

public record UpdatePaymentMethodRequest(
    string Code,
    string Name,
    string PaymentCategory,
    bool IsCash,
    bool IsCredit,
    bool RequiresReferenceNo,
    int DisplayOrder,
    bool IsActive);

public class PaymentMethodDto
{
    public long PaymentMethodId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PaymentCategory { get; set; } = string.Empty;
    public bool IsCash { get; set; }
    public bool IsCredit { get; set; }
    public bool RequiresReferenceNo { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

/* ---------------- PaymentMethodDetail ---------------- */

public class PaymentMethodDetailDto
{
    public long PaymentMethodDetailId { get; set; }
    public long PaymentMethodId { get; set; }
    public string PaymentMethodCode { get; set; } = string.Empty;
    public string PaymentMethodName { get; set; } = string.Empty;
    public string PaymentCategory { get; set; } = string.Empty;
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
    public bool IsActive { get; set; }
    public long CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record CreatePaymentMethodDetailRequest(
    long PaymentMethodId,
    string Code,
    string Name,
    string? DisplayName = null,
    string? UPIId = null,
    string? BankName = null,
    string? AccountNumber = null,
    string? IFSCCode = null,
    string? TerminalName = null,
    string? CashCounterName = null,
    string? ReferenceValue = null,
    bool IsDefault = false,
    int DisplayOrder = 0,
    bool IsActive = true);

public record UpdatePaymentMethodDetailRequest(
    string Code,
    string Name,
    string? DisplayName,
    string? UPIId,
    string? BankName,
    string? AccountNumber,
    string? IFSCCode,
    string? TerminalName,
    string? CashCounterName,
    string? ReferenceValue,
    bool IsDefault,
    int DisplayOrder,
    bool IsActive);

/* ---------------- Payment (transaction) ---------------- */

public class PaymentLookupsDto
{
    public List<LookupItem> PaymentTypes { get; set; } = new();
    public List<LookupItem> PaymentMethods { get; set; } = new();
    public List<LookupItem> BusinessPartners { get; set; } = new();
}

public class PaymentDto
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

public record CreatePaymentRequest(
    long? BusinessPartnerId,
    long PaymentTypeID,
    long PaymentMethodID,
    string ReferenceType,
    long ReferenceId,
    decimal Amount,
    string PaymentDate,
    string PaymentNo,
    string? ReferenceNo = null,
    string? Remarks = null);

public record UpdatePaymentRequest(
    long? BusinessPartnerId,
    long PaymentTypeID,
    long PaymentMethodID,
    string ReferenceType,
    long ReferenceId,
    decimal Amount,
    string PaymentDate,
    string PaymentNo,
    string? ReferenceNo,
    string? Remarks,
    long StatusID);
