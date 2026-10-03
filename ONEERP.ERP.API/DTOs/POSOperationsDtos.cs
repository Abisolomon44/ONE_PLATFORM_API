namespace ONEERP.ERP.API.DTOs;

/// <summary>T041 — POS dashboard: only data the backend actually supports.</summary>
public class POSDashboardDto
{
    public long? CurrentSessionId { get; set; }
    public string? CurrentSessionNumber { get; set; }
    public string? SessionStatus { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal CashIn { get; set; }
    public decimal CashOut { get; set; }
    public decimal PosSalesToday { get; set; }
    public int InvoiceCountToday { get; set; }
    public decimal ExpectedClosingCash { get; set; }
}

public class ClosePOSSessionRequest
{
    public decimal ActualClosingCash { get; set; }
    public string? ClosingRemarks { get; set; }
}

public class POSCashMovementRequest
{
    public long POSSessionId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNo { get; set; }
}

public class POSCashMovementDto
{
    public long POSCashMovementId { get; set; }
    public long POSSessionId { get; set; }
    public string Direction { get; set; } = "";
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime MovementDate { get; set; }
}

/// <summary>T047 — persistent hold payload. CartJson reconstructs the SalesEntry cart.</summary>
public class CreatePOSHoldRequest
{
    public long BranchId { get; set; }
    public long? StoreId { get; set; }
    public long? CounterId { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string CartJson { get; set; } = string.Empty;
}

public class POSHoldBillDto
{
    public long POSHoldBillId { get; set; }
    public string HoldNumber { get; set; } = "";
    public DateTime HoldDate { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "";
    public string? CartJson { get; set; }
}

/// <summary>T055 — shift summary aggregated from real tables only.</summary>
public class POSShiftSummaryDto
{
    public long POSSessionId { get; set; }
    public string SessionNumber { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal CashIn { get; set; }
    public decimal CashOut { get; set; }
    public int InvoiceCount { get; set; }
    public decimal GrossSales { get; set; }
    public decimal Discount { get; set; }
    public decimal Taxable { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal Paid { get; set; }
    public decimal Balance { get; set; }
    public int ReturnCount { get; set; }
    public decimal ReturnTotal { get; set; }
    public decimal ExpectedClosingCash { get; set; }
    public decimal ActualClosingCash { get; set; }
    public decimal CashDifference { get; set; }
}
