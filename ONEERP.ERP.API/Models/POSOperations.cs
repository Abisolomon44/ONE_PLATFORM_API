namespace ONEERP.ERP.API.Models;

/// <summary>T049/T050 — Cash In / Cash Out against an OPEN POS session.</summary>
public class POSCashMovement
{
    public long POSCashMovementId { get; set; }
    public long POSSessionId { get; set; }
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    public string Direction { get; set; } = "IN"; // IN | OUT
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime MovementDate { get; set; }
    public long CreatedByUserID { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>T047/T048 — persistent held POS cart. The cart is stored as JSON so
/// the existing SalesEntry cart model can be reconstructed verbatim on recall.</summary>
public class POSHoldBill
{
    public long POSHoldBillId { get; set; }
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    public long? StoreId { get; set; }
    public long? CounterId { get; set; }
    public string HoldNumber { get; set; } = string.Empty;
    public DateTime HoldDate { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string CartJson { get; set; } = string.Empty;
    public string Status { get; set; } = "HELD"; // HELD | RECALLED | CANCELLED
    public long CreatedByUserID { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? RecalledByUserID { get; set; }
    public DateTime? RecalledAt { get; set; }
    public long? CancelledByUserID { get; set; }
    public DateTime? CancelledAt { get; set; }
}
