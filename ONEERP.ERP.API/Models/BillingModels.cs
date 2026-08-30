namespace ONEERP.ERP.API.Models;

public class Status
{
    public long StatusId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Module { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Stock
{
    public long StockId { get; set; }
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public long ProductId { get; set; }
    public long UnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal LastPurchaseRate { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class StockTransaction
{
    public long StockTransactionId { get; set; }
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public long ProductId { get; set; }
    public long UnitId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public long ReferenceId { get; set; }
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public decimal Rate { get; set; }
    public decimal BalanceQuantity { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Remarks { get; set; }
    public long CreatedByUserID { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PurchaseReturn
{
    public long PurchaseReturnId { get; set; }
    public long PurchaseId { get; set; }
    public long CompanyId { get; set; }
    public string? CompanyNameSnapshot { get; set; }
    public long BranchId { get; set; }
    public string? BranchNameSnapshot { get; set; }
    public long WarehouseId { get; set; }
    public long SupplierId { get; set; }
    public string? SupplierNameSnapshot { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public decimal TotalGrossAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalTaxableAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalCessAmount { get; set; }
    public decimal TotalRoundOff { get; set; }
    public decimal GrandTotal { get; set; }
    public long StatusID { get; set; }
    public string? Reason { get; set; }
    public string? Remarks { get; set; }
    public long CreatedByUserID { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? UpdatedByUserID { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PurchaseReturnItem> Items { get; set; } = new();
}

public class PurchaseReturnItem
{
    public long PurchaseReturnItemId { get; set; }
    public long PurchaseReturnId { get; set; }
    public long PurchaseItemId { get; set; }
    public long ProductId { get; set; }
    public string? ProductCodeSnapshot { get; set; }
    public string? ProductNameSnapshot { get; set; }
    public long UnitId { get; set; }
    public string? UnitNameSnapshot { get; set; }
    public long? HSNId { get; set; }
    public string? HSNCodeSnapshot { get; set; }
    public decimal ReturnQuantity { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal DiscountAmount { get; set; }
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
}

public class PaymentAllocation
{
    public long PaymentAllocationId { get; set; }
    public long PaymentId { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public long ReferenceId { get; set; }
    public decimal AllocatedAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
