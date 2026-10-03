namespace ONEERP.ERP.API.DTOs;

/// <summary>
/// Filter + paging contract for the sales report endpoints.
/// Company is never part of this DTO — it is always resolved server-side
/// from the authenticated user so callers cannot cross the company
/// boundary by manipulating query parameters.
/// Only filters backed by real SalesInvoice / SalesInvoiceItem / Payment /
/// PaymentAllocation columns are accepted (no invented fields).
/// </summary>
public class SalesReportFilter
{
    /// <summary>overview | register | detail | product | customer | daily | monthly | tax | hsn | payments | outstanding | pos | price-list | allocation</summary>
    public string Report { get; set; } = "register";

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public long? BranchId { get; set; }
    public long? WarehouseId { get; set; }
    public long? CustomerId { get; set; }
    public long? ProductId { get; set; }
    public long? SalesTypeId { get; set; }
    public long? PriceListId { get; set; }
    public long? PaymentTypeID { get; set; }
    public long? PaymentMethodID { get; set; }
    public long? HsnId { get; set; }
    public decimal? GstRate { get; set; }

    /// <summary>Free-text SourceType on SalesInvoice (e.g. SALES / POS).</summary>
    public string? SourceType { get; set; }

    /// <summary>Free-text InvoiceStatus on SalesInvoice (e.g. POSTED / CANCELLED).</summary>
    public string? InvoiceStatus { get; set; }

    public string? InvoiceNumber { get; set; }
    public string? Search { get; set; }

    /// <summary>Report-specific grouping dimension, e.g. product | hsn | month | customer | branch | gst-rate | source-type.</summary>
    public string? GroupBy { get; set; }

    public int Page { get; set; } = 1;
    public int Size { get; set; } = 25;

    /// <summary>Optional server-side sort on a whitelisted column per report.</summary>
    public string? SortBy { get; set; }
    public string? SortDir { get; set; } = "asc";
}

public class SalesReportKpi
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public decimal? Value { get; set; }
    public string? Display { get; set; }
}

public class SalesReportChart
{
    public string Label { get; set; } = "";
    public decimal Value { get; set; }
}

/// <summary>
/// Generic report envelope: server-computed KPIs + server-paged rows +
/// dashboard widget series. Rows are dynamic so the same grid shell can
/// render every report without client-side aggregation of financial data.
/// </summary>
public class SalesReportResult
{
    public List<SalesReportKpi> Kpis { get; set; } = new();
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 25;

    // Dashboard widgets
    public List<SalesReportChart> Trend { get; set; } = new();
    public List<SalesReportChart> CustomerRanking { get; set; } = new();
    public List<SalesReportChart> ProductRanking { get; set; } = new();
    public List<SalesReportChart> BranchBreakdown { get; set; } = new();
    public List<SalesReportChart> WarehouseBreakdown { get; set; } = new();
    public List<SalesReportChart> PaymentStatus { get; set; } = new();
    public List<SalesReportChart> GstSummary { get; set; } = new();
    public List<SalesReportChart> SourceBreakdown { get; set; } = new();
}
