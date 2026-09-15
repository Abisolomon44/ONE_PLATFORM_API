using System.Data;
using Dapper;
using ONEERP.ERP.API.DTOs;

namespace ONEERP.ERP.API.Repositories;

public partial class PurchaseReportRepository
{
    // ============================================================
    // 1. Dashboard — KPI tiles + trend/ranking/breakdown widgets
    // ============================================================
    private async Task<PurchaseReportResult> DashboardAsync(IDbConnection conn, PurchaseReportFilter f, DynamicParameters dp)
    {
        var result = new PurchaseReportResult();
        result.Rows = new List<Dictionary<string, object?>>();
        result.TotalCount = 0;

        // Header-level KPIs
        var head = await Sql.QueryFirstOrDefaultAsync<dynamic>(conn,
            $@"SELECT COUNT(*) AS PurchaseCount,
                      COUNT(DISTINCT p.SupplierId) AS SupplierCount,
                      COALESCE(SUM(p.TotalGrossAmount),0) AS Gross,
                      COALESCE(SUM(p.TotalDiscountAmount),0) AS Discount,
                      COALESCE(SUM(p.TotalTaxableAmount),0) AS Taxable,
                      COALESCE(SUM(p.TotalTaxAmount),0) AS GST,
                      COALESCE(SUM(p.TotalCessAmount),0) AS CESS,
                      COALESCE(SUM(p.GrandTotal),0) AS GrandTotal,
                      COALESCE(SUM(p.PaidAmount),0) AS Paid,
                      COALESCE(SUM(p.BalanceAmount),0) AS Balance
               FROM dbo.Purchase p
               {BuildWhere(f, "PurchaseDate", "PurchaseNumber")}",
            dp);
        var dk = (IDictionary<string, object>)head!;
        var iNum = IVal(dk, "PurchaseCount");

        result.Kpis = new List<PurchaseReportKpi>
        {
            Kpi("purchases", "Total Purchases", null, iNum.ToString()),
            Kpi("suppliers", "Suppliers", null, IVal(dk, "SupplierCount").ToString()),
            Kpi("gross", "Total Gross", Rnd(DVal(dk, "Gross"))),
            Kpi("discount", "Total Discount", Rnd(DVal(dk, "Discount"))),
            Kpi("taxable", "Taxable Value", Rnd(DVal(dk, "Taxable"))),
            Kpi("gst", "GST", Rnd(DVal(dk, "GST"))),
            Kpi("cess", "CESS", Rnd(DVal(dk, "CESS"))),
            Kpi("grand", "Grand Total", Rnd(DVal(dk, "GrandTotal"))),
            Kpi("paid", "Paid", Rnd(DVal(dk, "Paid"))),
            Kpi("balance", "Outstanding", Rnd(DVal(dk, "Balance"))),
            Kpi("avgInvoice", "Avg Invoice Value", iNum > 0 ? Rnd(DVal(dk, "GrandTotal") / iNum) : 0),
        };

        var itemAgg = await Sql.QueryFirstOrDefaultAsync<dynamic>(conn,
            $@"SELECT COALESCE(SUM(pi.Quantity),0) AS Quantity, COUNT(DISTINCT pi.ProductId) AS ProductCount
               FROM dbo.PurchaseItem pi
               INNER JOIN dbo.Purchase p ON p.PurchaseId = pi.PurchaseId
               {BuildWhere(f, "PurchaseDate", "PurchaseNumber")}",
            dp);
        var ik = (IDictionary<string, object>)itemAgg!;
        result.Kpis.Add(Kpi("quantity", "Quantity Purchased", Rnd(DVal(ik, "Quantity"))));
        result.Kpis.Add(Kpi("products", "Products", null, IVal(ik, "ProductCount").ToString()));

        async Task<List<PurchaseReportChart>> CharsAsync(string sql)
            => (await Sql.QueryAsync<dynamic>(conn, sql, dp)).Select(x => Chart(
                    Str((IDictionary<string, object>)x, "Label"),
                    DVal((IDictionary<string, object>)x, "Value"))).ToList();

        result.Trend = await CharsAsync(
            $@"SELECT FORMAT(p.PurchaseDate, 'yyyy-MM') AS Label, SUM(p.GrandTotal) AS Value
               FROM dbo.Purchase p {BuildWhere(f, "PurchaseDate", "PurchaseNumber")}
               GROUP BY FORMAT(p.PurchaseDate, 'yyyy-MM') ORDER BY Label");
        result.SupplierRanking = await CharsAsync(
            $@"SELECT TOP 8 p.SupplierNameSnapshot AS Label, SUM(p.GrandTotal) AS Value
               FROM dbo.Purchase p {BuildWhere(f, "PurchaseDate", "PurchaseNumber")}
               GROUP BY p.SupplierNameSnapshot ORDER BY Value DESC");
        result.ProductRanking = await CharsAsync(
            $@"SELECT TOP 8 pi.ProductNameSnapshot AS Label, SUM(pi.LineTotal) AS Value
               FROM dbo.PurchaseItem pi INNER JOIN dbo.Purchase p ON p.PurchaseId = pi.PurchaseId
               {BuildWhere(f, "PurchaseDate", "PurchaseNumber")}
               GROUP BY pi.ProductNameSnapshot ORDER BY Value DESC");
        result.CategoryBreakdown = await CharsAsync(
            $@"SELECT COALESCE(c.CategoryName, 'Uncategorised') AS Label, SUM(pi.LineTotal) AS Value
               FROM dbo.PurchaseItem pi INNER JOIN dbo.Purchase p ON p.PurchaseId = pi.PurchaseId
               LEFT JOIN dbo.ProductCategories c ON c.Id = pi.CategoryID
               {BuildWhere(f, "PurchaseDate", "PurchaseNumber")}
               GROUP BY COALESCE(c.CategoryName, 'Uncategorised') ORDER BY Value DESC");
        result.BranchBreakdown = await CharsAsync(
            $@"SELECT COALESCE(p.BranchNameSnapshot, 'Unknown') AS Label, SUM(p.GrandTotal) AS Value
               FROM dbo.Purchase p {BuildWhere(f, "PurchaseDate", "PurchaseNumber")}
               GROUP BY COALESCE(p.BranchNameSnapshot, 'Unknown') ORDER BY Value DESC");
        result.WarehouseBreakdown = await CharsAsync(
            $@"SELECT COALESCE(w.WarehouseName, 'Unknown') AS Label, SUM(p.GrandTotal) AS Value
               FROM dbo.Purchase p
               LEFT JOIN dbo.Warehouses w ON w.Id = p.WarehouseId
               {BuildWhere(f, "PurchaseDate", "PurchaseNumber")}
               GROUP BY COALESCE(w.WarehouseName, 'Unknown') ORDER BY Value DESC");
        result.PaymentStatus = await CharsAsync(
            $@"SELECT CASE WHEN p.BalanceAmount <= 0.005 THEN 'Paid' WHEN p.PaidAmount > 0 THEN 'Partial' ELSE 'Unpaid' END AS Label,
                      SUM(p.GrandTotal) AS Value
               FROM dbo.Purchase p {BuildWhere(f, "PurchaseDate", "PurchaseNumber")}
               GROUP BY CASE WHEN p.BalanceAmount <= 0.005 THEN 'Paid' WHEN p.PaidAmount > 0 THEN 'Partial' ELSE 'Unpaid' END");
        result.GstSummary = await CharsAsync(
            $@"SELECT CONCAT(CAST(pi.GSTRate AS decimal(8,1)), '%') AS Label, SUM(pi.GSTAmount) AS Value
               FROM dbo.PurchaseItem pi INNER JOIN dbo.Purchase p ON p.PurchaseId = pi.PurchaseId
               {BuildWhere(f, "PurchaseDate", "PurchaseNumber")}
               GROUP BY pi.GSTRate ORDER BY pi.GSTRate");
        result.ReturnsBreakdown = await CharsAsync(
            $@"SELECT FORMAT(pr.ReturnDate, 'yyyy-MM') AS Label, SUM(pr.GrandTotal) AS Value
               FROM dbo.PurchaseReturn pr
               {BuildReturnWhere(f)}
               GROUP BY FORMAT(pr.ReturnDate, 'yyyy-MM') ORDER BY Label");
        return result;
    }

    // ============================================================
    // 2. Purchase Register — one row per purchase header
    // ============================================================
    private async Task<PurchaseReportResult> RegisterAsync(IDbConnection conn, PurchaseReportFilter f, DynamicParameters dp)
    {
        var result = new PurchaseReportResult();
        var select = $@"SELECT p.PurchaseId, p.PurchaseDate, p.PurchaseNumber,
            p.SupplierInvoiceNumber, p.SupplierNameSnapshot AS Supplier, p.BranchNameSnapshot AS Branch,
            COALESCE(w.WarehouseName, '') AS Warehouse,
            pt.Name AS PaymentType, pm.Name AS PaymentMethod, st.Name AS Status,
            p.TotalGrossAmount AS Gross, p.TotalDiscountAmount AS Discount, p.TotalTaxableAmount AS Taxable,
            p.TotalTaxAmount AS GST, p.TotalCessAmount AS CESS, p.TotalRoundOff AS RoundOff,
            p.GrandTotal, p.PaidAmount, p.BalanceAmount
            FROM dbo.Purchase p
            LEFT JOIN dbo.Warehouses w ON w.Id = p.WarehouseId
            LEFT JOIN dbo.PaymentType pt ON pt.PaymentTypeId = p.PaymentTypeID
            LEFT JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId = p.PaymentMethodID
            LEFT JOIN dbo.[Status] st ON st.StatusId = p.StatusID
            {BuildWhere(f, "PurchaseDate", "PurchaseNumber")}";
        var (rows, total) = await PageAsync(conn, select, "p.PurchaseDate DESC, p.PurchaseId DESC", dp, f.Page, f.Size);
        result.Rows = rows;
        result.TotalCount = total;
        return result;
    }

    // ============================================================
    // 3. Item Analysis — one row per purchase line, optional grouping
    // ============================================================
    private async Task<PurchaseReportResult> ItemsAsync(IDbConnection conn, PurchaseReportFilter f, DynamicParameters dp)
    {
        var result = new PurchaseReportResult();
        var where = BuildWhere(f, "PurchaseDate", "PurchaseNumber");
        var join = @"FROM dbo.PurchaseItem pi
            INNER JOIN dbo.Purchase p ON p.PurchaseId = pi.PurchaseId
            LEFT JOIN dbo.ProductCategories c ON c.Id = pi.CategoryID
            LEFT JOIN dbo.ProductSubCategories s ON s.Id = pi.SubCategoryID
            LEFT JOIN dbo.ProductBrands b ON b.Id = pi.BrandID ";

        string order;
        if (string.IsNullOrWhiteSpace(f.GroupBy) || f.GroupBy == "none")
        {
            var select = $@"SELECT pi.PurchaseId, p.PurchaseDate, p.PurchaseNumber, p.SupplierNameSnapshot AS Supplier,
                pi.ProductId, pi.ProductCodeSnapshot AS ProductCode, pi.ProductNameSnapshot AS Product,
                c.CategoryName AS Category, s.SubCategoryName AS SubCategory, b.BrandName AS Brand,
                pi.UnitNameSnapshot AS Unit, pi.HSNCodeSnapshot AS HSN,
                pi.Quantity, pi.FreeQuantity, pi.PurchaseRate, pi.DiscountPercentage, pi.DiscountAmount,
                pi.TaxableValue AS Taxable, pi.GSTAmount AS GST, pi.CESSAmount AS CESS, pi.LineTotal
                {join}{where}";
            order = "p.PurchaseDate DESC, p.PurchaseId DESC";
            var (rows, total) = await PageAsync(conn, select, order, dp, f.Page, f.Size);
            result.Rows = rows;
            result.TotalCount = total;
            return result;
        }

        var (dim, gb, ob) = f.GroupBy switch
        {
            "product" => ("pi.ProductId, pi.ProductNameSnapshot AS Product", "pi.ProductId, pi.ProductNameSnapshot", "Product"),
            "category" => ("pi.CategoryID, COALESCE(c.CategoryName, 'Uncategorised') AS Category", "pi.CategoryID, COALESCE(c.CategoryName, 'Uncategorised')", "Category"),
            "brand" => ("pi.BrandID, COALESCE(b.BrandName, 'Unknown') AS Brand", "pi.BrandID, COALESCE(b.BrandName, 'Unknown')", "Brand"),
            "uom" => ("pi.UnitID, COALESCE(pi.UnitNameSnapshot, 'Unknown') AS Unit", "pi.UnitID, COALESCE(pi.UnitNameSnapshot, 'Unknown')", "Unit"),
            "hsn" => ("COALESCE(pi.HSNCodeSnapshot, 'Unknown') AS HSN", "COALESCE(pi.HSNCodeSnapshot, 'Unknown')", "HSN"),
            "month" => ("FORMAT(p.PurchaseDate, 'yyyy-MM') AS Period", "FORMAT(p.PurchaseDate, 'yyyy-MM')", "Period"),
            "quarter" => ("CONCAT(YEAR(p.PurchaseDate), '-Q', DATEPART(QUARTER, p.PurchaseDate)) AS Period", "CONCAT(YEAR(p.PurchaseDate), '-Q', DATEPART(QUARTER, p.PurchaseDate))", "Period"),
            "year" => ("YEAR(p.PurchaseDate) AS Period", "YEAR(p.PurchaseDate)", "Period"),
            _ => ("pi.ProductNameSnapshot AS Product", "pi.ProductNameSnapshot", "Product"),
        };
        var sel = $@"SELECT {dim},
            COUNT(*) AS Lines, SUM(pi.Quantity) AS Quantity, SUM(pi.FreeQuantity) AS FreeQuantity,
            SUM(pi.DiscountAmount) AS Discount, SUM(pi.TaxableValue) AS Taxable,
            SUM(pi.GSTAmount) AS GST, SUM(pi.CESSAmount) AS CESS, SUM(pi.LineTotal) AS LineTotal
            {join}{where} GROUP BY {gb}";
        var (rows1, total1) = await PageAsync(conn, sel, ob, dp, f.Page, f.Size);
        result.Rows = rows1;
        result.TotalCount = total1;
        return result;
    }

    // ============================================================
    // 4. Supplier Analysis — per supplier (and optional dimension)
    // ============================================================
    private async Task<PurchaseReportResult> SupplierAsync(IDbConnection conn, PurchaseReportFilter f, DynamicParameters dp)
    {
        var result = new PurchaseReportResult();
        var where = BuildWhere(f, "PurchaseDate", "PurchaseNumber");

        var (dim, gb, ob) = f.GroupBy switch
        {
            "month" => ("FORMAT(p.PurchaseDate, 'yyyy-MM') AS Period", "FORMAT(p.PurchaseDate, 'yyyy-MM')", "Period"),
            "quarter" => ("CONCAT(YEAR(p.PurchaseDate), '-Q', DATEPART(QUARTER, p.PurchaseDate)) AS Period", "CONCAT(YEAR(p.PurchaseDate), '-Q', DATEPART(QUARTER, p.PurchaseDate))", "Period"),
            "year" => ("YEAR(p.PurchaseDate) AS Period", "YEAR(p.PurchaseDate)", "Period"),
            "branch" => ("COALESCE(p.BranchNameSnapshot, 'Unknown') AS Branch", "COALESCE(p.BranchNameSnapshot, 'Unknown')", "Branch"),
            "warehouse" => ("COALESCE(w.WarehouseName, '') AS Warehouse", "COALESCE(w.WarehouseName, '')", "Warehouse"),
            "purchase-type" => ("CONCAT('Type-', ISNULL(CAST(p.PurchaseTypeId AS nvarchar(10)), '-')) AS PurchaseType", "CONCAT('Type-', ISNULL(CAST(p.PurchaseTypeId AS nvarchar(10)), '-'))", "PurchaseType"),
            "status" => ("COALESCE(st.Name, 'Unknown') AS Status", "COALESCE(st.Name, 'Unknown')", "Status"),
            _ => ("p.SupplierId, p.SupplierNameSnapshot AS Supplier", "p.SupplierId, p.SupplierNameSnapshot", "Supplier"),
        };

        var select = $@"SELECT {dim},
            COUNT(DISTINCT p.PurchaseId) AS Purchases,
            COALESCE(SUM(ii.ItemQty),0) AS Quantity,
            SUM(p.TotalGrossAmount) AS Gross, SUM(p.TotalDiscountAmount) AS Discount,
            SUM(p.TotalTaxableAmount) AS Taxable, SUM(p.TotalTaxAmount) AS GST, SUM(p.TotalCessAmount) AS CESS,
            SUM(p.GrandTotal) AS GrandTotal, SUM(p.PaidAmount) AS Paid, SUM(p.BalanceAmount) AS Balance,
            AVG(p.GrandTotal) AS AvgInvoice, MAX(p.PurchaseDate) AS LastPurchaseDate
            FROM dbo.Purchase p
            LEFT JOIN (SELECT PurchaseId, SUM(Quantity) AS ItemQty FROM dbo.PurchaseItem GROUP BY PurchaseId) ii ON ii.PurchaseId = p.PurchaseId
            LEFT JOIN dbo.Warehouses w ON w.Id = p.WarehouseId
            LEFT JOIN dbo.[Status] st ON st.StatusId = p.StatusID
            {where} GROUP BY {gb}";
        var (rows, total) = await PageAsync(conn, select, ob, dp, f.Page, f.Size);
        result.Rows = rows;
        result.TotalCount = total;
        return result;
    }
}