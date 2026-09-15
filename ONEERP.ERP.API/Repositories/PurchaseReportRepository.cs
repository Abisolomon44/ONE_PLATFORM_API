using System.Data;
using Dapper;
using ONEERP.ERP.API.DTOs;

namespace ONEERP.ERP.API.Repositories;

public interface IPurchaseReportRepository
{
    Task<PurchaseReportResult> RunAsync(long companyId, PurchaseReportFilter filter);
}

public partial class PurchaseReportRepository : TenantRepositoryBase, IPurchaseReportRepository
{
    public PurchaseReportRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<PurchaseReportResult> RunAsync(long companyId, PurchaseReportFilter f)
    {
        using var conn = OpenTenant();
        var dp = BuildParams(companyId, f);
        return f.Report switch
        {
            "dashboard" => await DashboardAsync(conn, f, dp),
            "items" => await ItemsAsync(conn, f, dp),
            "supplier" => await SupplierAsync(conn, f, dp),
            "product-price" => await ProductPriceAsync(conn, f, dp),
            "quantity" => await QuantityAsync(conn, f, dp),
            "tax" => await TaxAsync(conn, f, dp),
            "payment" => await PaymentAsync(conn, f, dp),
            "returns" => await ReturnsAsync(conn, f, dp),
            "reconciliation" => await ReconciliationAsync(conn, f, dp),
            _ => await RegisterAsync(conn, f, dp),
        };
    }

    // ============================================================
    // Shared helpers
    // ============================================================

    private static DynamicParameters BuildParams(long companyId, PurchaseReportFilter f)
    {
        var dp = new DynamicParameters();
        dp.Add("companyId", companyId);
        if (f.DateFrom.HasValue) dp.Add("dateFrom", f.DateFrom.Value);
        if (f.DateTo.HasValue) dp.Add("dateTo", f.DateTo.Value);
        if (f.BranchId.HasValue) dp.Add("branchId", f.BranchId.Value);
        if (f.WarehouseId.HasValue) dp.Add("warehouseId", f.WarehouseId.Value);
        if (f.SupplierId.HasValue) dp.Add("supplierId", f.SupplierId.Value);
        if (f.ProductId.HasValue) dp.Add("productId", f.ProductId.Value);
        if (f.CategoryId.HasValue) dp.Add("categoryId", f.CategoryId.Value);
        if (f.SubCategoryId.HasValue) dp.Add("subCategoryId", f.SubCategoryId.Value);
        if (f.BrandId.HasValue) dp.Add("brandId", f.BrandId.Value);
        if (f.UnitId.HasValue) dp.Add("unitId", f.UnitId.Value);
        if (f.HsnId.HasValue) dp.Add("hsnId", f.HsnId.Value);
        if (f.TaxId.HasValue) dp.Add("taxId", f.TaxId.Value);
        if (f.GstRate.HasValue) dp.Add("gstRate", f.GstRate.Value);
        if (f.PaymentTypeID.HasValue) dp.Add("paymentTypeID", f.PaymentTypeID.Value);
        if (f.PaymentMethodID.HasValue) dp.Add("paymentMethodID", f.PaymentMethodID.Value);
        if (f.StatusId.HasValue) dp.Add("statusId", f.StatusId.Value);
        if (f.AccountingYearId.HasValue) dp.Add("accountingYearId", f.AccountingYearId.Value);
        if (f.PurchaseTypeId.HasValue) dp.Add("purchaseTypeId", f.PurchaseTypeId.Value);
        if (!string.IsNullOrWhiteSpace(f.PurchaseNumber)) dp.Add("purchaseNumber", $"%{f.PurchaseNumber.Trim()}%");
        if (!string.IsNullOrWhiteSpace(f.ReturnNumber)) dp.Add("returnNumber", $"%{f.ReturnNumber.Trim()}%");
        if (!string.IsNullOrWhiteSpace(f.SupplierInvoiceNumber)) dp.Add("supplierInvoiceNumber", $"%{f.SupplierInvoiceNumber.Trim()}%");
        if (!string.IsNullOrWhiteSpace(f.ReferenceNumber)) dp.Add("referenceNumber", $"%{f.ReferenceNumber.Trim()}%");
        if (!string.IsNullOrWhiteSpace(f.Search)) dp.Add("search", $"%{f.Search.Trim()}%");
        return dp;
    }

    /// <summary>
    /// WHERE clause for purchase-header scoped reports. Alsased table is always
    /// <c>p</c> (the Purchase header); item-level predicates are applied on
    /// <c>pi</c>. Alias prefixes are parameterized so the same builder can feed
    /// correlated subqueries with their own aliases (p2/pi2).
    /// </summary>
    private static string BuildWhere(PurchaseReportFilter f, string dateCol, string numCol, string ph = "p", string pit = "pi")
        => "WHERE " + BuildConditions(f, dateCol, numCol, ph, pit);

    private static string BuildConditions(PurchaseReportFilter f, string dateCol, string numCol, string ph = "p", string pit = "pi")
    {
        var c = new List<string> { $"{ph}.CompanyId = @companyId" };
        if (f.DateFrom.HasValue) c.Add($"{ph}.{dateCol} >= @dateFrom");
        if (f.DateTo.HasValue) c.Add($"{ph}.{dateCol} < DATEADD(DAY, 1, @dateTo)");
        if (f.BranchId.HasValue) c.Add($"{ph}.BranchId = @branchId");
        if (f.WarehouseId.HasValue) c.Add($"{ph}.WarehouseId = @warehouseId");
        if (f.SupplierId.HasValue) c.Add($"{ph}.SupplierId = @supplierId");
        if (f.StatusId.HasValue) c.Add($"{ph}.StatusID = @statusId");
        if (f.AccountingYearId.HasValue) c.Add($"{ph}.AccountingYearId = @accountingYearId");
        if (f.PurchaseTypeId.HasValue) c.Add($"{ph}.PurchaseTypeId = @purchaseTypeId");
        if (f.TaxId.HasValue) c.Add($"{ph}.TaxId = @taxId");
        if (f.PaymentTypeID.HasValue) c.Add($"{ph}.PaymentTypeID = @paymentTypeID");
        if (f.PaymentMethodID.HasValue) c.Add($"{ph}.PaymentMethodID = @paymentMethodID");
        if (!string.IsNullOrWhiteSpace(f.PurchaseNumber)) c.Add($"{ph}.{numCol} LIKE @purchaseNumber");
        if (!string.IsNullOrWhiteSpace(f.SupplierInvoiceNumber)) c.Add($"{ph}.SupplierInvoiceNumber LIKE @supplierInvoiceNumber");
        if (!string.IsNullOrWhiteSpace(f.ReferenceNumber)) c.Add($"{ph}.ReferenceNumber LIKE @referenceNumber");
        if (!string.IsNullOrWhiteSpace(f.Search))
            c.Add($"({ph}.{numCol} LIKE @search OR {ph}.SupplierNameSnapshot LIKE @search OR {ph}.SupplierInvoiceNumber LIKE @search)");
        if (f.ProductId.HasValue || f.CategoryId.HasValue || f.SubCategoryId.HasValue || f.BrandId.HasValue || f.UnitId.HasValue || f.HsnId.HasValue || f.GstRate.HasValue)
        {
            var item = new List<string>();
            if (f.ProductId.HasValue) item.Add("ProductId = @productId");
            if (f.CategoryId.HasValue) item.Add("ProductId IN (SELECT Id FROM dbo.Products WHERE CategoryId = @categoryId)");
            if (f.SubCategoryId.HasValue) item.Add("ProductId IN (SELECT Id FROM dbo.Products WHERE SubCategoryId = @subCategoryId)");
            if (f.BrandId.HasValue) item.Add("ProductId IN (SELECT Id FROM dbo.Products WHERE BrandId = @brandId)");
            if (f.UnitId.HasValue) item.Add("UnitID = @unitId");
            if (f.HsnId.HasValue) item.Add("HSNID = @hsnId");
            if (f.GstRate.HasValue) item.Add("GSTRate = @gstRate");
            c.Add($"{ph}.PurchaseId IN (SELECT PurchaseId FROM dbo.PurchaseItem WHERE " + string.Join(" AND ", item) + ")");
        }
        if (!string.IsNullOrWhiteSpace(f.Search))
            c.Add($"{ph}.PurchaseId IN (SELECT PurchaseId FROM dbo.PurchaseItem WHERE ProductNameSnapshot LIKE @search OR ProductCodeSnapshot LIKE @search)");
        return string.Join(" AND ", c);
    }

    private static string BuildReturnWhere(PurchaseReportFilter f)
    {
        var c = new List<string> { "pr.CompanyId = @companyId" };
        if (f.DateFrom.HasValue) c.Add("pr.ReturnDate >= @dateFrom");
        if (f.DateTo.HasValue) c.Add("pr.ReturnDate < DATEADD(DAY, 1, @dateTo)");
        if (f.BranchId.HasValue) c.Add("pr.BranchId = @branchId");
        if (f.WarehouseId.HasValue) c.Add("pr.WarehouseId = @warehouseId");
        if (f.SupplierId.HasValue) c.Add("pr.SupplierId = @supplierId");
        if (f.StatusId.HasValue) c.Add("pr.StatusID = @statusId");
        if (f.PaymentTypeID.HasValue) c.Add("pr.PaymentTypeId = @paymentTypeID");
        if (f.PaymentMethodID.HasValue) c.Add("pr.PaymentMethodId = @paymentMethodID");
        if (!string.IsNullOrWhiteSpace(f.ReturnNumber)) c.Add("pr.ReturnNumber LIKE @returnNumber");
        if (!string.IsNullOrWhiteSpace(f.SupplierInvoiceNumber)) c.Add("pr.SupplierInvoiceNumber LIKE @supplierInvoiceNumber");
        if (!string.IsNullOrWhiteSpace(f.Search))
            c.Add("(pr.ReturnNumber LIKE @search OR pr.SupplierNameSnapshot LIKE @search)");
        if (f.ProductId.HasValue || f.CategoryId.HasValue || f.SubCategoryId.HasValue || f.BrandId.HasValue || f.UnitId.HasValue || f.HsnId.HasValue || f.GstRate.HasValue)
        {
            var item = new List<string> { "pri.PurchaseReturnId = pr.PurchaseReturnId" };
            if (f.ProductId.HasValue) item.Add("pri.ProductId = @productId");
            if (f.CategoryId.HasValue) item.Add("pri.ProductId IN (SELECT Id FROM dbo.Products WHERE CategoryId = @categoryId)");
            if (f.SubCategoryId.HasValue) item.Add("pri.ProductId IN (SELECT Id FROM dbo.Products WHERE SubCategoryId = @subCategoryId)");
            if (f.BrandId.HasValue) item.Add("pri.ProductId IN (SELECT Id FROM dbo.Products WHERE BrandId = @brandId)");
            if (f.UnitId.HasValue) item.Add("pri.UnitId = @unitId");
            if (f.HsnId.HasValue) item.Add("pri.HSNId = @hsnId");
            if (f.GstRate.HasValue) item.Add("pri.GSTRate = @gstRate");
            c.Add("EXISTS (SELECT 1 FROM dbo.PurchaseReturnItem pri WHERE " + string.Join(" AND ", item) + ")");
        }
        return "WHERE " + string.Join(" AND ", c);
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

    /// <summary>Runs a count + ROW_NUMBER windowed page (used when the query already contains a window func).</summary>
    private async Task<(List<Dictionary<string, object?>> Rows, int Total)> WindowedPageAsync(
        IDbConnection conn, string innerSql, DynamicParameters ps, int page, int size)
    {
        var total = await Sql.ExecuteScalarAsync<int>(conn, $"SELECT COUNT(*) FROM ({innerSql}) t", ps);
        var offset = (page - 1) * size;
        var pager = new DynamicParameters();
        pager.AddDynamicParams(ps);
        pager.Add("offset", offset);
        pager.Add("size", size);
        var sql = $"SELECT * FROM ({innerSql}) t WHERE t._rn > @offset AND t._rn <= @offset + @size ORDER BY t._rn";
        var rows = await Sql.QueryAsync<dynamic>(conn, sql, pager);
        return (rows.Select(ToRow).ToList(), total);
    }

    private static PurchaseReportKpi Kpi(string key, string label, decimal? value, string? display = null)
        => new() { Key = key, Label = label, Value = value, Display = display ?? (value.HasValue ? value.Value.ToString("N2") : null) };

    private static PurchaseReportChart Chart(string label, decimal value) => new() { Label = label, Value = value };

    private static decimal? Rnd(decimal? v) => v.HasValue ? Math.Round(v.Value, 2) : null;

    private static decimal DVal(IDictionary<string, object> d, string key)
        => d.TryGetValue(key, out var v) && v is not DBNull && v is not null ? Convert.ToDecimal(v) : 0m;

    private static int IVal(IDictionary<string, object> d, string key)
        => d.TryGetValue(key, out var v) && v is not DBNull && v is not null ? Convert.ToInt32(v) : 0;

    private static string Str(IDictionary<string, object> d, string key)
        => d.TryGetValue(key, out var v) && v is not DBNull && v is not null ? Convert.ToString(v) ?? "" : "";
}