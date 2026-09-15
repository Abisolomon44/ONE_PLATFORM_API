namespace ONEERP.ERP.API.DTOs;

/// <summary>
/// Filter + paging contract for the purchase report endpoints.
/// Company is never part of this DTO — it is always resolved server-side
/// from <see cref="ICurrentUser.CompanyId"/> so callers cannot cross the
/// company boundary by manipulating query parameters.
/// </summary>
public class PurchaseReportFilter
{
    /// <summary>dashboard | register | items | supplier | product-price | quantity | tax | payment | returns | reconciliation</summary>
    public string Report { get; set; } = "register";

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public long? BranchId { get; set; }
    public long? WarehouseId { get; set; }
    public long? SupplierId { get; set; }
    public long? ProductId { get; set; }
    public long? CategoryId { get; set; }
    public long? SubCategoryId { get; set; }
    public long? BrandId { get; set; }
    public long? UnitId { get; set; }
    public long? HsnId { get; set; }
    public long? TaxId { get; set; }
    public decimal? GstRate { get; set; }
    public long? PaymentTypeID { get; set; }
    public long? PaymentMethodID { get; set; }
    public long? StatusId { get; set; }
    public long? AccountingYearId { get; set; }
    public long? PurchaseTypeId { get; set; }
    public long? CurrencyId { get; set; }
    public string? PurchaseNumber { get; set; }
    public string? SupplierInvoiceNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ReturnNumber { get; set; }
    public string? Search { get; set; }

    /// <summary>Group dimension: none | month | quarter | year | supplier | product | category | brand | uom | hsn | tax | gst-rate | payment-type | payment-method | branch | warehouse | purchase-order | grn | reason | purchase-type | status</summary>
    public string? GroupBy { get; set; }

    /// <summary>Report-specific sub mode, e.g. product-price: history | rate | supplier; reconciliation: amount | quantity | tax | payment | return</summary>
    public string? Mode { get; set; }

    public int Page { get; set; } = 1;
    public int Size { get; set; } = 25;
    public string? SortBy { get; set; }
    public string? SortDir { get; set; } = "asc";
}

public class PurchaseReportKpi
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public decimal? Value { get; set; }
    public string? Display { get; set; }
}

public class PurchaseReportChart
{
    public string Label { get; set; } = "";
    public decimal Value { get; set; }
}

/// <summary>
/// Generic report envelope: server-computed KPIs + server-paged rows +
/// dashboard widget series. Rows are dynamic so the same grid shell can
/// render every report without client-side aggregation of financial data.
/// </summary>
public class PurchaseReportResult
{
    public List<PurchaseReportKpi> Kpis { get; set; } = new();
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 25;

    // Dashboard widgets
    public List<PurchaseReportChart> Trend { get; set; } = new();
    public List<PurchaseReportChart> SupplierRanking { get; set; } = new();
    public List<PurchaseReportChart> ProductRanking { get; set; } = new();
    public List<PurchaseReportChart> CategoryBreakdown { get; set; } = new();
    public List<PurchaseReportChart> BranchBreakdown { get; set; } = new();
    public List<PurchaseReportChart> WarehouseBreakdown { get; set; } = new();
    public List<PurchaseReportChart> PaymentStatus { get; set; } = new();
    public List<PurchaseReportChart> GstSummary { get; set; } = new();
    public List<PurchaseReportChart> ReturnsBreakdown { get; set; } = new();
}