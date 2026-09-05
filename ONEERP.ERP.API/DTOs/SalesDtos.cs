namespace ONEERP.ERP.API.DTOs;

public class ProductStockDto
{
    public long UnitId { get; set; }
    public string? UnitName { get; set; }
    public decimal Quantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
}

/* ---------------- Sales ---------------- */

public class SalesLookupsDto
{
    public List<LookupItem> Companies { get; set; } = new();
    public List<LookupItem> Branches { get; set; } = new();
    public List<LookupItem> Warehouses { get; set; } = new();
    public List<LookupItem> Customers { get; set; } = new();
    public List<LookupItem> Products { get; set; } = new();
    public List<LookupItem> Units { get; set; } = new();
    public List<LookupItem> PaymentTypes { get; set; } = new();
    public List<LookupItem> PaymentMethods { get; set; } = new();
    public long CurrentCompanyId { get; set; }
}

public class SalesItemDto
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

public class SalesInvoiceDto
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
    public bool IsActive { get; set; }
    public long CreatedByUserID { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SalesItemDto> Items { get; set; } = new();
}

public class CreateSalesItemInput
{
    public long ProductId { get; set; }
    public long UnitID { get; set; }
    public decimal Quantity { get; set; }
    public decimal FreeQuantity { get; set; } = 0;
    public decimal Rate { get; set; }
    public long? BatchId { get; set; }
    public decimal DiscountPercentage { get; set; } = 0;
    public decimal GSTPercent { get; set; } = 0;
    public decimal CGSTPercent { get; set; } = 0;
    public decimal SGSTPercent { get; set; } = 0;
    public decimal IGSTPercent { get; set; } = 0;
    public decimal CESSPercent { get; set; } = 0;
    public string? Remarks { get; set; }
}

public class CreateSalesPaymentInput
{
    public decimal Amount { get; set; }
    public long? PaymentTypeID { get; set; }
    public long? PaymentMethodID { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Remarks { get; set; }
}

public record CreateSalesRequest(
    long BranchId,
    long WarehouseId,
    long CustomerId,
    string InvoiceNumber,
    string InvoiceDate,
    string SourceType = "SALES",
    int? SalesTypeId = null,
    long? PriceListId = null,
    string? ReferenceNo = null,
    string? ReferenceDate = null,
    long? PaymentTypeID = null,
    long? PaymentMethodID = null,
    string? Remarks = null,
    List<CreateSalesItemInput> Items = null!,
    CreateSalesPaymentInput? Payment = null,
    long CompanyId = 0);

public record UpdateSalesRequest(
    long BranchId,
    long WarehouseId,
    long CustomerId,
    string InvoiceNumber,
    string InvoiceDate,
    int? SalesTypeId,
    long? PriceListId,
    string? ReferenceNo,
    string? ReferenceDate,
    long? PaymentTypeID,
    long? PaymentMethodID,
    string? Remarks,
    List<CreateSalesItemInput> Items,
    long CompanyId = 0);
