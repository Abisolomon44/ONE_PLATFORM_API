namespace ONEERP.ERP.API.Models;

public class SalesInvoice
{
    public long SalesInvoiceId { get; set; }
    public string SalesInvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string SourceType { get; set; } = "SALES";

    public long CompanyId { get; set; }
    public string? CompanyNameSnapshot { get; set; }
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public long CustomerId { get; set; }
    public string? CustomerNameSnapshot { get; set; }

    public int? SalesTypeId { get; set; }
    public long? PriceListId { get; set; }

    public string? ReferenceNo { get; set; }
    public DateTime? ReferenceDate { get; set; }

    public decimal TotalGrossAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalTaxableAmount { get; set; }
    public decimal TotalCGSTAmount { get; set; }
    public decimal TotalSGSTAmount { get; set; }
    public decimal TotalIGSTAmount { get; set; }
    public decimal TotalCESSAmount { get; set; }
    public decimal TotalRoundOff { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }

    public long? PaymentTypeID { get; set; }
    public long? PaymentMethodID { get; set; }
    public long StatusID { get; set; }
    public string InvoiceStatus { get; set; } = "POSTED";

    public string? Remarks { get; set; }
    public bool IsActive { get; set; } = true;
    public long CreatedByUserID { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? UpdatedByUserID { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<SalesInvoiceItem> Items { get; set; } = new();
}

public class SalesInvoiceItem
{
    public long SalesInvoiceItemId { get; set; }
    public long SalesInvoiceId { get; set; }

    public long ProductId { get; set; }
    public string? ProductCodeSnapshot { get; set; }
    public string? ProductNameSnapshot { get; set; }
    public long UnitID { get; set; }
    public string? UnitNameSnapshot { get; set; }
    public long? BatchId { get; set; }
    public long? HSNID { get; set; }
    public string? HSNCodeSnapshot { get; set; }
    public string? BarcodeSnapshot { get; set; }

    public decimal Quantity { get; set; }
    public decimal FreeQuantity { get; set; }
    public decimal Rate { get; set; }
    public decimal GrossAmount { get; set; }

    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }

    public decimal GSTPercent { get; set; }
    public decimal CGSTPercent { get; set; }
    public decimal SGSTPercent { get; set; }
    public decimal IGSTPercent { get; set; }
    public decimal CESSPercent { get; set; }

    public decimal CGSTAmount { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal CESSAmount { get; set; }

    public decimal LineTotal { get; set; }
    public string? Remarks { get; set; }
}
