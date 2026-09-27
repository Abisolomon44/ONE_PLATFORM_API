namespace ONEERP.ERP.API.DTOs;

/* ---------------------------------------------------------------------------
   Sales Invoice PRINT data (read-model). Fetched straight from
   SalesInvoice / SalesInvoiceItem / BusinessPartners / Companies so the
   document designer preview + print render REAL database values - never
   Angular form state. Consumed by DocumentPreviewHtml.
   --------------------------------------------------------------------------- */

public class SalesInvoicePrintDto
{
    public long SalesInvoiceId { get; set; }
    public string SalesInvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }

    public long CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyGstin { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyEmail { get; set; }
    public string? CompanyLogoUrl { get; set; }

    public long CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerGstin { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerPhone { get; set; }

    public long BranchId { get; set; }
    public string? BranchName { get; set; }
    public long WarehouseId { get; set; }
    public string? WarehouseName { get; set; }

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

    public string? PaymentMode { get; set; }
    public string? Remarks { get; set; }

    public List<SalesInvoicePrintItemDto> Items { get; set; } = new();
}

public class SalesInvoicePrintItemDto
{
    public long SalesInvoiceItemId { get; set; }
    public int SlNo { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? HsnCode { get; set; }
    public string? UnitName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}
