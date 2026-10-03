namespace ONEERP.ERP.API.DTOs;

/// <summary>T074 — inventory dashboard KPIs, all server-computed.</summary>
public class InventoryDashboardDto
{
    public decimal StockValue { get; set; }
    public int ProductsWithStock { get; set; }
    public int OutOfStockCount { get; set; }
    public int LowStockCount { get; set; }
    public decimal QtyInToday { get; set; }
    public decimal QtyOutToday { get; set; }
    public int TransactionsToday { get; set; }
}

/// <summary>T076 — opening stock line.</summary>
public class OpeningStockItemInput
{
    public long ProductId { get; set; }
    public long UnitId { get; set; }
    public long WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
}

public class OpeningStockRequest
{
    public long BranchId { get; set; }
    public DateTime? OpeningDate { get; set; }
    public string? Remarks { get; set; }
    public List<OpeningStockItemInput> Items { get; set; } = new();
}

// ============================================================
// T079 — Stock Adjustment
// ============================================================
public class StockAdjustmentItemInput
{
    public long ProductId { get; set; }
    public long UnitId { get; set; }
    public decimal QuantityDelta { get; set; }
    public decimal Rate { get; set; }
    public string? Reason { get; set; }
}

public class CreateStockAdjustmentRequest
{
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public string AdjustmentDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string? Reason { get; set; }
    public string? Remarks { get; set; }
    public List<StockAdjustmentItemInput> Items { get; set; } = new();
}

public class StockAdjustmentDto
{
    public long StockAdjustmentId { get; set; }
    public string AdjustmentNumber { get; set; } = "";
    public long WarehouseId { get; set; }
    public DateTime AdjustmentDate { get; set; }
    public string? Reason { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; } = "DRAFT";
    public List<StockAdjustmentItemInput> Items { get; set; } = new();
}

// ============================================================
// T080/T081 — Stock Transfer
// ============================================================
public class StockTransferItemInput
{
    public long ProductId { get; set; }
    public long UnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public string? Remarks { get; set; }
}

public class CreateStockTransferRequest
{
    public long BranchId { get; set; }
    public long FromWarehouseId { get; set; }
    public long ToWarehouseId { get; set; }
    public string TransferDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string? Remarks { get; set; }
    public List<StockTransferItemInput> Items { get; set; } = new();
}

public class StockTransferDto
{
    public long StockTransferId { get; set; }
    public string TransferNumber { get; set; } = "";
    public long FromWarehouseId { get; set; }
    public long ToWarehouseId { get; set; }
    public DateTime TransferDate { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; } = "DRAFT";
    public List<StockTransferItemInput> Items { get; set; } = new();
}

// ============================================================
// T082 — Stock Count
// ============================================================
public class StockCountItemInput
{
    public long ProductId { get; set; }
    public long UnitId { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal Rate { get; set; }
}

public class CreateStockCountRequest
{
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public string CountDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string? Remarks { get; set; }
    public List<StockCountItemInput> Items { get; set; } = new();
}

public class StockCountDto
{
    public long StockCountId { get; set; }
    public string CountNumber { get; set; } = "";
    public long WarehouseId { get; set; }
    public DateTime CountDate { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; } = "DRAFT";
    public List<StockCountItemDto> Items { get; set; } = new();
}

public class StockCountItemDto
{
    public long ProductId { get; set; }
    public long UnitId { get; set; }
    public decimal BookQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal Variance { get; set; }
    public decimal Rate { get; set; }
}

// ============================================================
// T083/T084/T085 — read models (no new tables)
// ============================================================
public class StockReconciliationRow
{
    public long ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public long WarehouseId { get; set; }
    public decimal BookQuantity { get; set; }        // Stock.Quantity
    public decimal LedgerQuantity { get; set; }      // Σ In − Σ Out from StockTransaction
    public decimal Variance { get; set; }
}

public class StockValuationRow
{
    public long ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public long WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Value { get; set; }
}

public class LowStockRow
{
    public long ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public long WarehouseId { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal ReorderLevel { get; set; }
}
