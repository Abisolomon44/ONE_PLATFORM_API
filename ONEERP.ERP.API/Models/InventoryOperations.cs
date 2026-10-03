namespace ONEERP.ERP.API.Models;

/// <summary>T079 — stock adjustment header (DRAFT → POSTED writes the ledger).</summary>
public class StockAdjustment
{
    public long StockAdjustmentId { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public DateTime AdjustmentDate { get; set; }
    public string? Reason { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; } = "DRAFT";
    public long CreatedByUserID { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? PostedByUserID { get; set; }
    public DateTime? PostedAt { get; set; }
    public List<StockAdjustmentItem> Items { get; set; } = new();
}

public class StockAdjustmentItem
{
    public long StockAdjustmentItemId { get; set; }
    public long StockAdjustmentId { get; set; }
    public long ProductId { get; set; }
    public long UnitId { get; set; }
    public decimal QuantityDelta { get; set; }
    public decimal Rate { get; set; }
    public string? Reason { get; set; }
}

/// <summary>T080/T081 — warehouse-to-warehouse transfer (POSTED moves stock both sides).</summary>
public class StockTransfer
{
    public long StockTransferId { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    public long FromWarehouseId { get; set; }
    public long ToWarehouseId { get; set; }
    public DateTime TransferDate { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; } = "DRAFT";
    public long CreatedByUserID { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? PostedByUserID { get; set; }
    public DateTime? PostedAt { get; set; }
    public List<StockTransferItem> Items { get; set; } = new();
}

public class StockTransferItem
{
    public long StockTransferItemId { get; set; }
    public long StockTransferId { get; set; }
    public long ProductId { get; set; }
    public long UnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>T082 — stock count (POSTED writes variance adjustments to the ledger).</summary>
public class StockCount
{
    public long StockCountId { get; set; }
    public string CountNumber { get; set; } = string.Empty;
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public DateTime CountDate { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; } = "DRAFT";
    public long CreatedByUserID { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? PostedByUserID { get; set; }
    public DateTime? PostedAt { get; set; }
    public List<StockCountItem> Items { get; set; } = new();
}

public class StockCountItem
{
    public long StockCountItemId { get; set; }
    public long StockCountId { get; set; }
    public long ProductId { get; set; }
    public long UnitId { get; set; }
    public decimal BookQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal Variance { get; set; }
    public decimal Rate { get; set; }
}
