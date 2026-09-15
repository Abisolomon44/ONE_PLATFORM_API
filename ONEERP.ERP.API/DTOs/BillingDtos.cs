namespace ONEERP.ERP.API.DTOs;

/* ---------------- Purchase ---------------- */

public class PurchaseItemDto
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
    public long? TaxId { get; set; }
    public long? CessId { get; set; }
    public decimal? OrderedQuantity { get; set; }
    public decimal? ReceivedQuantity { get; set; }
    public decimal? ReturnedQuantity { get; set; }
    public decimal? RemainingQuantity { get; set; }
    public long? PurchaseOrderId { get; set; }
    public long? PurchaseOrderItemId { get; set; }
    public long? GRNId { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
}

public class PurchaseDto
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
    public bool IsActive { get; set; }
    public long CreatedByUserID { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? SupplierPONumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public long? CurrencyId { get; set; }
    public long? PurchaseTypeId { get; set; }
    public long? AccountingYearId { get; set; }
    public long? TaxId { get; set; }
    public bool? IsGSTInclusive { get; set; }
    public long? CancelledByUserID { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public List<PurchaseItemDto> Items { get; set; } = new();
}

public class CreatePurchaseItemInput
{
    public long ProductId { get; set; }
    public long UnitID { get; set; }
    public decimal Quantity { get; set; }
    public decimal FreeQuantity { get; set; } = 0;
    public decimal PurchaseRate { get; set; }
    public decimal? MRP { get; set; }
    public decimal? RetailPrice { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? SaleRate { get; set; }
    public decimal DiscountPercentage { get; set; } = 0;
     public bool? IsGSTInclusive { get; set; } = false;
    public decimal GSTRate { get; set; } = 0;
    public decimal CGSTRate { get; set; } = 0;
    public decimal SGSTRate { get; set; } = 0;
    public decimal IGSTRate { get; set; } = 0;
    public decimal CESSRate { get; set; } = 0;
    public long? BrandID { get; set; }
    public long? CategoryID { get; set; }
    public long? SubCategoryID { get; set; }
    public long? HSNID { get; set; }
    public string? HSNCode { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Remarks { get; set; }
    public long? TaxId { get; set; }
    public long? CessId { get; set; }
    public decimal? OrderedQuantity { get; set; }
    public decimal? ReceivedQuantity { get; set; }
    public decimal? ReturnedQuantity { get; set; }
    public decimal? RemainingQuantity { get; set; }
    public long? PurchaseOrderId { get; set; }
    public long? PurchaseOrderItemId { get; set; }
    public long? GRNId { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
}

public class CreatePurchasePaymentInput
{
    public decimal Amount { get; set; }
    public long? PaymentTypeID { get; set; }
    public long? PaymentMethodID { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Remarks { get; set; }
}

public record CreatePurchaseRequest(
    long BranchId,
    long WarehouseId,
    long SupplierId,
    string PurchaseNumber,
    string PurchaseDate,
    List<CreatePurchaseItemInput> Items,
    string? SupplierInvoiceNumber = null,
    string? SupplierInvoiceDate = null,
    string? SupplierPONumber = null,
    string? ReferenceNumber = null,
    long? CurrencyId = null,
    long? PurchaseTypeId = null,
    long? AccountingYearId = null,
    long? TaxId = null,
    bool? IsGSTInclusive = null,
    long? PaymentTypeID = null,
    long? PaymentMethodID = null,
    decimal PaidAmount = 0,
    decimal BalanceAmount = 0,
    string? Remarks = null,
    CreatePurchasePaymentInput? Payment = null,
    long CompanyId = 0);

public record UpdatePurchaseRequest(
    long BranchId,
    long WarehouseId,
    long SupplierId,
    string PurchaseNumber,
    string PurchaseDate,
    List<CreatePurchaseItemInput> Items,
    string? SupplierInvoiceNumber = null,
    string? SupplierInvoiceDate = null,
    long? PaymentTypeID = null,
    long? PaymentMethodID = null,
    decimal PaidAmount = 0,
    decimal BalanceAmount = 0,
    string? Remarks = null,
    long CompanyId = 0,
    string? SupplierPONumber = null,
    string? ReferenceNumber = null,
    long? CurrencyId = null,
    long? PurchaseTypeId = null,
    long? AccountingYearId = null,
    long? TaxId = null,
    bool? IsGSTInclusive = null);

/* ---------------- Stock ---------------- */

public class StockDto
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

public class StockTransactionDto
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
}

/* ---------------- Purchase Return ---------------- */

public class PurchaseReturnItemDto
{
    public long PurchaseReturnItemId { get; set; }
    public long PurchaseReturnId { get; set; }
    public long PurchaseItemId { get; set; }
    public long ProductId { get; set; }
    public string? ProductCodeSnapshot { get; set; }
    public string? ProductNameSnapshot { get; set; }
    public long UnitId { get; set; }
    public string? UnitNameSnapshot { get; set; }
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

public class PurchaseReturnDto
{
    public long PurchaseReturnId { get; set; }
    public long PurchaseId { get; set; }
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
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
    public long? CancelledByUserID { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public List<PurchaseReturnItemDto> Items { get; set; } = new();
}

public class CreatePurchaseReturnItemInput
{
    public long PurchaseItemId { get; set; }
    public long ProductId { get; set; }
    public long UnitId { get; set; }
    public decimal ReturnQuantity { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxableValue { get; set; }
    public decimal GSTRate { get; set; } = 0;
    public decimal GSTAmount { get; set; } = 0;
    public decimal CGSTRate { get; set; } = 0;
    public decimal CGSTAmount { get; set; } = 0;
    public decimal SGSTRate { get; set; } = 0;
    public decimal SGSTAmount { get; set; } = 0;
    public decimal IGSTRate { get; set; } = 0;
    public decimal IGSTAmount { get; set; } = 0;
    public decimal CESSRate { get; set; } = 0;
    public decimal CESSAmount { get; set; } = 0;
}

public record CreatePurchaseReturnRequest(
    long PurchaseId,
    string ReturnDate,
    List<CreatePurchaseReturnItemInput> Items,
    string? Reason = null,
    string? Remarks = null);

public record UpdatePurchaseReturnRequest(
    string ReturnDate,
    List<CreatePurchaseReturnItemInput> Items,
    string? Reason = null,
    string? Remarks = null);

public record CancelTransactionRequest(string Reason);

public class DeleteCheckDto
{
    public bool Allowed { get; set; }
    public List<string> Reasons { get; set; } = new();
    public string? StatusCode { get; set; }
    public int PaymentCount { get; set; }
    public int StockTransactionCount { get; set; }
    public int ReturnCount { get; set; }
}

/* ---------------- Payment Allocation ---------------- */

public class PaymentAllocationDto
{
    public long PaymentAllocationId { get; set; }
    public long PaymentId { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public long ReferenceId { get; set; }
    public decimal AllocatedAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
