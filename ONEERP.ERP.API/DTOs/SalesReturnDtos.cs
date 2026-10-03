using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.DTOs;

public class CreateSalesReturnItemInput
{
    public long SalesInvoiceItemId { get; set; }
    public long ProductId { get; set; }
    public long UnitID { get; set; }
    public decimal ReturnQuantity { get; set; }
    public decimal FreeQuantity { get; set; }
    public decimal Rate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GSTPercent { get; set; }
    public decimal CGSTPercent { get; set; }
    public decimal SGSTPercent { get; set; }
    public decimal IGSTPercent { get; set; }
    public decimal CESSPercent { get; set; }
}

public class SalesReturnRefundInput
{
    public long? PaymentTypeId { get; set; }
    public long? PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
}

public class CreateSalesReturnRequest
{
    public long SalesInvoiceId { get; set; }
    public string ReturnDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public List<CreateSalesReturnItemInput> Items { get; set; } = new();
    public string? Reason { get; set; }
    public string? Remarks { get; set; }
    public SalesReturnRefundInput? Refund { get; set; }
}

public class UpdateSalesReturnRequest
{
    public string ReturnDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public List<CreateSalesReturnItemInput> Items { get; set; } = new();
    public string? Reason { get; set; }
    public string? Remarks { get; set; }
    public SalesReturnRefundInput? Refund { get; set; }
}

public class SalesReturnItemDto
{
    public long SalesReturnItemId { get; set; }
    public long SalesReturnId { get; set; }
    public long SalesInvoiceItemId { get; set; }
    public long ProductId { get; set; }
    public string? ProductCodeSnapshot { get; set; }
    public string? ProductNameSnapshot { get; set; }
    public long UnitID { get; set; }
    public string? UnitNameSnapshot { get; set; }
    public string? HSNCodeSnapshot { get; set; }
    public decimal ReturnQuantity { get; set; }
    public decimal FreeQuantity { get; set; }
    public decimal Rate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal GSTPercent { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal CESSAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public class SalesReturnDto
{
    public long SalesReturnId { get; set; }
    public long SalesInvoiceId { get; set; }
    public string SalesInvoiceNo { get; set; } = string.Empty;
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public long CustomerId { get; set; }
    public string? CustomerNameSnapshot { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public decimal TotalGrossAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalTaxableAmount { get; set; }
    public decimal TotalCGSTAmount { get; set; }
    public decimal TotalSGSTAmount { get; set; }
    public decimal TotalIGSTAmount { get; set; }
    public decimal TotalCESSAmount { get; set; }
    public decimal TotalRoundOff { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal RefundAmount { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public string? Remarks { get; set; }
    public string? CancellationReason { get; set; }
    public List<SalesReturnItemDto> Items { get; set; } = new();
}

/// <summary>T030 — customer refund against a sales return.</summary>
public class CreateRefundRequest
{
    public long SalesReturnId { get; set; }
    public string PaymentDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public long PaymentTypeId { get; set; }
    public long PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Remarks { get; set; }
}

public class RefundDto
{
    public long PaymentId { get; set; }
    public string PaymentNo { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public long ReferenceId { get; set; }
    public long? BusinessPartnerId { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Remarks { get; set; }
}
