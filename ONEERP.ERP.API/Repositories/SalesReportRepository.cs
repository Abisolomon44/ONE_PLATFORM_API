using System.Data;
using Dapper;
using ONEERP.ERP.API.DTOs;

namespace ONEERP.ERP.API.Repositories;

public interface ISalesReportRepository
{
    Task<SalesReportResult> RunAsync(long companyId, SalesReportFilter filter);
}

/// <summary>
/// Sales reports over dbo.SalesInvoice / dbo.SalesInvoiceItem / dbo.Payment /
/// dbo.PaymentAllocation. Only stored columns are read — stored financial
/// totals (GrandTotal, PaidAmount, BalanceAmount, tax amounts…) are never
/// recomputed in C#, so every screen reconciles exactly with the ledger.
/// The company boundary comes from the authenticated user, never the query.
/// </summary>
public partial class SalesReportRepository : TenantRepositoryBase, ISalesReportRepository
{
    public SalesReportRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<SalesReportResult> RunAsync(long companyId, SalesReportFilter f)
    {
        using var conn = OpenTenant();
        var dp = BuildParams(companyId, f);
        return f.Report switch
        {
            "overview" => await OverviewAsync(conn, f, dp),
            "detail" => await DetailAsync(conn, f, dp),
            "product" => await ProductAsync(conn, f, dp),
            "customer" => await CustomerAsync(conn, f, dp),
            "daily" => await DailyAsync(conn, f, dp),
            "monthly" => await MonthlyAsync(conn, f, dp),
            "tax" => await TaxAsync(conn, f, dp),
            "hsn" => await HsnAsync(conn, f, dp),
            "payments" => await PaymentsAsync(conn, f, dp),
            "outstanding" => await OutstandingAsync(conn, f, dp),
            "pos" => await PosAsync(conn, f, dp),
            "price-list" => await PriceListAsync(conn, f, dp),
            "allocation" => await AllocationAsync(conn, f, dp),
            _ => await RegisterAsync(conn, f, dp),
        };
    }

    // ============================================================
    // Shared helpers
    // ============================================================

    private static DynamicParameters BuildParams(long companyId, SalesReportFilter f)
    {
        var dp = new DynamicParameters();
        dp.Add("companyId", companyId);
        if (f.DateFrom.HasValue) dp.Add("dateFrom", f.DateFrom.Value);
        if (f.DateTo.HasValue) dp.Add("dateTo", f.DateTo.Value);
        if (f.BranchId.HasValue) dp.Add("branchId", f.BranchId.Value);
        if (f.WarehouseId.HasValue) dp.Add("warehouseId", f.WarehouseId.Value);
        if (f.CustomerId.HasValue) dp.Add("customerId", f.CustomerId.Value);
        if (f.SalesTypeId.HasValue) dp.Add("salesTypeId", f.SalesTypeId.Value);
        if (f.PriceListId.HasValue) dp.Add("priceListId", f.PriceListId.Value);
        if (f.PaymentTypeID.HasValue) dp.Add("paymentTypeID", f.PaymentTypeID.Value);
        if (f.PaymentMethodID.HasValue) dp.Add("paymentMethodID", f.PaymentMethodID.Value);
        if (f.HsnId.HasValue) dp.Add("hsnId", f.HsnId.Value);
        if (f.GstRate.HasValue) dp.Add("gstRate", f.GstRate.Value);
        if (!string.IsNullOrWhiteSpace(f.SourceType)) dp.Add("sourceType", f.SourceType.Trim());
        if (!string.IsNullOrWhiteSpace(f.InvoiceStatus)) dp.Add("invoiceStatus", f.InvoiceStatus.Trim());
        if (!string.IsNullOrWhiteSpace(f.InvoiceNumber)) dp.Add("invoiceNumber", $"%{f.InvoiceNumber.Trim()}%");
        if (!string.IsNullOrWhiteSpace(f.Search)) dp.Add("search", $"%{f.Search.Trim()}%");
        return dp;
    }

    /// <summary>
    /// WHERE clause over the SalesInvoice header (alias <c>si</c>). Item-level
    /// predicates (product, HSN, GST rate) are pushed into the item table via
    /// an IN subquery so header aggregates stay duplicate-free.
    /// </summary>
    private static string BuildWhere(SalesReportFilter f, string dateCol = "InvoiceDate", string si = "si")
    {
        var c = new List<string> { $"{si}.CompanyId = @companyId" };
        if (f.DateFrom.HasValue) c.Add($"{si}.{dateCol} >= @dateFrom");
        if (f.DateTo.HasValue) c.Add($"{si}.{dateCol} < DATEADD(DAY, 1, @dateTo)");
        if (f.BranchId.HasValue) c.Add($"{si}.BranchId = @branchId");
        if (f.WarehouseId.HasValue) c.Add($"{si}.WarehouseId = @warehouseId");
        if (f.CustomerId.HasValue) c.Add($"{si}.CustomerId = @customerId");
        if (f.SalesTypeId.HasValue) c.Add($"{si}.SalesTypeId = @salesTypeId");
        if (f.PriceListId.HasValue) c.Add($"{si}.PriceListId = @priceListId");
        if (f.PaymentTypeID.HasValue) c.Add($"{si}.PaymentTypeID = @paymentTypeID");
        if (f.PaymentMethodID.HasValue) c.Add($"{si}.PaymentMethodID = @paymentMethodID");
        if (!string.IsNullOrWhiteSpace(f.SourceType)) c.Add($"{si}.SourceType = @sourceType");
        if (!string.IsNullOrWhiteSpace(f.InvoiceStatus)) c.Add($"{si}.InvoiceStatus = @invoiceStatus");
        if (!string.IsNullOrWhiteSpace(f.InvoiceNumber)) c.Add($"{si}.SalesInvoiceNo LIKE @invoiceNumber");
        if (!string.IsNullOrWhiteSpace(f.Search))
            c.Add($"({si}.SalesInvoiceNo LIKE @search OR {si}.CustomerNameSnapshot LIKE @search OR {si}.ReferenceNo LIKE @search)");
        if (f.ProductId.HasValue || f.HsnId.HasValue || f.GstRate.HasValue)
        {
            var item = new List<string> { "sii.SalesInvoiceId = si.SalesInvoiceId" };
            if (f.ProductId.HasValue) item.Add("sii.ProductId = @productId");
            if (f.HsnId.HasValue) item.Add("sii.HSNID = @hsnId");
            if (f.GstRate.HasValue) item.Add("sii.GSTPercent = @gstRate");
            c.Add($"EXISTS (SELECT 1 FROM dbo.SalesInvoiceItem sii WHERE {string.Join(" AND ", item)})");
        }
        return "WHERE " + string.Join(" AND ", c);
    }

    /// <summary>Applies the per-report sort whitelist; falls back to the default order when unset/unknown.</summary>
    private static string OrderBy(SalesReportFilter f, string fallback, Dictionary<string, string> map)
    {
        if (!string.IsNullOrWhiteSpace(f.SortBy) && map.TryGetValue(f.SortBy, out var col))
        {
            var dir = string.Equals(f.SortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
            return $"{col} {dir}";
        }
        return fallback;
    }

    /// <summary>Maps a Dapper dynamic row (DapperRow) to a plain dictionary of column values.</summary>
    private static Dictionary<string, object?> ToRow(dynamic d)
    {
        if (d is not IDictionary<string, object> dict) return new Dictionary<string, object?>();
        return dict.ToDictionary(kv => kv.Key, kv => kv.Value is DBNull ? null : kv.Value);
    }

    /// <summary>Runs a count + OFFSET/FETCH page over a full inner SELECT (with any GROUP BY).</summary>
    private async Task<(List<Dictionary<string, object?>> Rows, int Total)> PageAsync(
        IDbConnection conn, string selectSql, string orderBy, DynamicParameters ps, int page, int size)
    {
        var total = await Sql.ExecuteScalarAsync<int>(conn, $"SELECT COUNT(*) FROM ({selectSql}) t", ps);
        var offset = (page - 1) * size;
        var pager = new DynamicParameters();
        pager.AddDynamicParams(ps);
        pager.Add("offset", offset);
        pager.Add("size", size);
        var rows = await Sql.QueryAsync<dynamic>(conn, $"{selectSql} ORDER BY {orderBy} OFFSET @offset ROWS FETCH NEXT @size ROWS ONLY", pager);
        return (rows.Select(ToRow).ToList(), total);
    }

    private static SalesReportKpi Kpi(string key, string label, decimal? value, string? display = null)
        => new() { Key = key, Label = label, Value = value, Display = display ?? (value.HasValue ? value.Value.ToString("N2") : null) };

    private static SalesReportChart Chart(string label, decimal value) => new() { Label = label, Value = value };

    private static decimal? Rnd(decimal? v) => v.HasValue ? Math.Round(v.Value, 2) : null;

    private static decimal DVal(IDictionary<string, object> d, string key)
        => d.TryGetValue(key, out var v) && v is not DBNull && v is not null ? Convert.ToDecimal(v) : 0m;

    private static int IVal(IDictionary<string, object> d, string key)
        => d.TryGetValue(key, out var v) && v is not DBNull && v is not null ? Convert.ToInt32(v) : 0;

    private static string Str(IDictionary<string, object> d, string key)
        => d.TryGetValue(key, out var v) && v is not DBNull && v is not null ? Convert.ToString(v) ?? "" : "";
}
