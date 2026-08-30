namespace ONEERP.ERP.API.Models;

public class Purchase
{
    public long PurchaseId { get; set; }
    public long CompanyId { get; set; }
    public string? CompanyNameSnapshot { get; set; }
    public long BranchId { get; set; }
    public string? BranchNameSnapshot { get; set; }
    public long WarehouseId { get; set; }
    public long SupplierId { get; set; }
    public string? SupplierNameSnapshot { get; set; }
    public string PurchaseNumber { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public string? SupplierInvoiceNumber { get; set; }
    public DateTime? SupplierInvoiceDate { get; set; }
    public decimal TotalGrossAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalTaxableAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalCessAmount { get; set; }
    public decimal TotalRoundOff { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public long? PaymentTypeID { get; set; }
    public long? PaymentMethodID { get; set; }
    public long StatusID { get; set; }
    public string? Remarks { get; set; }
    public bool IsActive { get; set; } = true;
    public long CreatedByUserID { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? UpdatedByUserID { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PurchaseItem> Items { get; set; } = new();
}

public class PurchaseItem
{
    public long PurchaseItemId { get; set; }
    public long PurchaseId { get; set; }
    public long ProductId { get; set; }
    public string? ProductCodeSnapshot { get; set; }
    public string? ProductNameSnapshot { get; set; }
    public long? BrandID { get; set; }
    public long? CategoryID { get; set; }
    public long? SubCategoryID { get; set; }
    public long UnitID { get; set; }
    public string? UnitNameSnapshot { get; set; }
    public long? HSNID { get; set; }
    public string? HSNCodeSnapshot { get; set; }
    public string? BarcodeSnapshot { get; set; }
    public decimal Quantity { get; set; }
    public decimal FreeQuantity { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal? MRP { get; set; }
    public decimal? RetailPrice { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? SaleRate { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public bool IsGSTInclusive { get; set; }
    public decimal TaxableValue { get; set; }
    public decimal GSTRate { get; set; }
    public decimal GSTAmount { get; set; }
    public decimal CGSTRate { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTRate { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTRate { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal CESSRate { get; set; }
    public decimal CESSAmount { get; set; }
    public decimal LineTotal { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Remarks { get; set; }
}
