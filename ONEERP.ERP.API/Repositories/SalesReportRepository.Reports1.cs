using System.Data;
using Dapper;
using ONEERP.ERP.API.DTOs;

namespace ONEERP.ERP.API.Repositories;

public partial class SalesReportRepository
{
    // ============================================================
    // 1. Sales Overview — KPI tiles + trend/ranking/breakdown widgets
    // ============================================================
    private async Task<SalesReportResult> OverviewAsync(IDbConnection conn, SalesReportFilter f, DynamicParameters dp)
    {
        var result = new SalesReportResult { Rows = new(), TotalCount = 0 };
        var where = BuildWhere(f);

        var head = await Sql.QueryFirstOrDefaultAsync<dynamic>(conn,
            $@"SELECT COUNT(*) AS InvoiceCount,
                      COUNT(DISTINCT si.CustomerId) AS CustomerCount,
                      COALESCE(SUM(si.TotalGrossAmount),0) AS Gross,
                      COALESCE(SUM(si.TotalDiscountAmount),0) AS Discount,
                      COALESCE(SUM(si.TotalTaxableAmount),0) AS Taxable,
                      COALESCE(SUM(si.TotalCGSTAmount),0) AS CGST,
                      COALESCE(SUM(si.TotalSGSTAmount),0) AS SGST,
                      COALESCE(SUM(si.TotalIGSTAmount),0) AS IGST,
                      COALESCE(SUM(si.TotalCESSAmount),0) AS CESS,
                      COALESCE(SUM(si.TotalRoundOff),0) AS RoundOff,
                      COALESCE(SUM(si.GrandTotal),0) AS GrandTotal,
                      COALESCE(SUM(si.PaidAmount),0) AS Paid,
                      COALESCE(SUM(si.BalanceAmount),0) AS Balance
               FROM dbo.SalesInvoice si
               {where}",
            dp);
        var dk = (IDictionary<string, object>)head!;
        var iNum = IVal(dk, "InvoiceCount");
        var gst = DVal(dk, "CGST") + DVal(dk, "SGST") + DVal(dk, "IGST") + DVal(dk, "CESS");

        result.Kpis = new List<SalesReportKpi>
        {
            Kpi("invoices", "Invoices", null, iNum.ToString()),
            Kpi("customers", "Customers", null, IVal(dk, "CustomerCount").ToString()),
            Kpi("gross", "Total Gross", Rnd(DVal(dk, "Gross"))),
            Kpi("discount", "Total Discount", Rnd(DVal(dk, "Discount"))),
            Kpi("taxable", "Taxable Value", Rnd(DVal(dk, "Taxable"))),
            Kpi("gst", "GST", Rnd(gst)),
            Kpi("cess", "CESS", Rnd(DVal(dk, "CESS"))),
            Kpi("roundOff", "Round Off", Rnd(DVal(dk, "RoundOff"))),
            Kpi("grand", "Grand Total", Rnd(DVal(dk, "GrandTotal"))),
            Kpi("paid", "Paid", Rnd(DVal(dk, "Paid"))),
            Kpi("balance", "Outstanding", Rnd(DVal(dk, "Balance"))),
            Kpi("avgInvoice", "Avg Invoice Value", iNum > 0 ? Rnd(DVal(dk, "GrandTotal") / iNum) : 0),
        };

        var itemAgg = await Sql.QueryFirstOrDefaultAsync<dynamic>(conn,
            $@"SELECT COALESCE(SUM(sii.Quantity),0) AS Quantity,
                      COALESCE(SUM(sii.FreeQuantity),0) AS FreeQuantity,
                      COUNT(DISTINCT sii.ProductId) AS ProductCount
               FROM dbo.SalesInvoiceItem sii
               INNER JOIN dbo.SalesInvoice si ON si.SalesInvoiceId = sii.SalesInvoiceId
               {where}",
            dp);
        var ik = (IDictionary<string, object>)itemAgg!;
        result.Kpis.Add(Kpi("quantity", "Quantity Sold", Rnd(DVal(ik, "Quantity"))));
        result.Kpis.Add(Kpi("freeQuantity", "Free Quantity", Rnd(DVal(ik, "FreeQuantity"))));
        result.Kpis.Add(Kpi("products", "Products", null, IVal(ik, "ProductCount").ToString()));

        async Task<List<SalesReportChart>> CharsAsync(string sql)
            => (await Sql.QueryAsync<dynamic>(conn, sql, dp)).Select(x => Chart(
                    Str((IDictionary<string, object>)x, "Label"),
                    DVal((IDictionary<string, object>)x, "Value"))).ToList();

        result.Trend = await CharsAsync(
            $@"SELECT FORMAT(si.InvoiceDate, 'yyyy-MM') AS Label, SUM(si.GrandTotal) AS Value
               FROM dbo.SalesInvoice si {where}
               GROUP BY FORMAT(si.InvoiceDate, 'yyyy-MM') ORDER BY Label");
        result.CustomerRanking = await CharsAsync(
            $@"SELECT TOP 8 si.CustomerNameSnapshot AS Label, SUM(si.GrandTotal) AS Value
               FROM dbo.SalesInvoice si {where}
               GROUP BY si.CustomerNameSnapshot ORDER BY Value DESC");
        result.ProductRanking = await CharsAsync(
            $@"SELECT TOP 8 sii.ProductNameSnapshot AS Label, SUM(sii.LineTotal) AS Value
               FROM dbo.SalesInvoiceItem sii INNER JOIN dbo.SalesInvoice si ON si.SalesInvoiceId = sii.SalesInvoiceId
               {where}
               GROUP BY sii.ProductNameSnapshot ORDER BY Value DESC");
        result.BranchBreakdown = await CharsAsync(
            $@"SELECT COALESCE(br.BranchName, 'Unknown') AS Label, SUM(si.GrandTotal) AS Value
               FROM dbo.SalesInvoice si LEFT JOIN dbo.Branches br ON br.Id = si.BranchId
               {where}
               GROUP BY COALESCE(br.BranchName, 'Unknown') ORDER BY Value DESC");
        result.WarehouseBreakdown = await CharsAsync(
            $@"SELECT COALESCE(w.WarehouseName, 'Unknown') AS Label, SUM(si.GrandTotal) AS Value
               FROM dbo.SalesInvoice si LEFT JOIN dbo.Warehouses w ON w.Id = si.WarehouseId
               {where}
               GROUP BY COALESCE(w.WarehouseName, 'Unknown') ORDER BY Value DESC");
        result.PaymentStatus = await CharsAsync(
            $@"SELECT CASE WHEN si.BalanceAmount <= 0.005 THEN 'Paid' WHEN si.PaidAmount > 0 THEN 'Partial' ELSE 'Unpaid' END AS Label,
                      SUM(si.GrandTotal) AS Value
               FROM dbo.SalesInvoice si {where}
               GROUP BY CASE WHEN si.BalanceAmount <= 0.005 THEN 'Paid' WHEN si.PaidAmount > 0 THEN 'Partial' ELSE 'Unpaid' END");
        result.GstSummary = await CharsAsync(
            $@"SELECT CONCAT(CAST(sii.GSTPercent AS decimal(8,1)), '%') AS Label, SUM(sii.CGSTAmount + sii.SGSTAmount + sii.IGSTAmount + sii.CESSAmount) AS Value
               FROM dbo.SalesInvoiceItem sii INNER JOIN dbo.SalesInvoice si ON si.SalesInvoiceId = sii.SalesInvoiceId
               {where}
               GROUP BY sii.GSTPercent ORDER BY sii.GSTPercent");
        result.SourceBreakdown = await CharsAsync(
            $@"SELECT COALESCE(si.SourceType, 'UNKNOWN') AS Label, SUM(si.GrandTotal) AS Value
               FROM dbo.SalesInvoice si {where}
               GROUP BY COALESCE(si.SourceType, 'UNKNOWN') ORDER BY Value DESC");
        return result;
    }

    // ============================================================
    // 2. Sales Register — one row per invoice header
    // ============================================================
    private async Task<SalesReportResult> RegisterAsync(IDbConnection conn, SalesReportFilter f, DynamicParameters dp)
    {
        var result = new SalesReportResult();
        var where = BuildWhere(f);
        var select = $@"SELECT si.SalesInvoiceId, si.SalesInvoiceNo, si.InvoiceDate, si.SourceType,
            si.CustomerNameSnapshot AS Customer, br.BranchName AS Branch, w.WarehouseName AS Warehouse,
            pt.Name AS PaymentType, pm.Name AS PaymentMethod, si.InvoiceStatus,
            si.TotalGrossAmount AS Gross, si.TotalDiscountAmount AS Discount, si.TotalTaxableAmount AS Taxable,
            si.TotalCGSTAmount AS CGST, si.TotalSGSTAmount AS SGST, si.TotalIGSTAmount AS IGST,
            si.TotalCESSAmount AS CESS, si.TotalRoundOff AS RoundOff,
            si.GrandTotal, si.PaidAmount AS Paid, si.BalanceAmount AS Balance
            FROM dbo.SalesInvoice si
            LEFT JOIN dbo.Branches br ON br.Id = si.BranchId
            LEFT JOIN dbo.Warehouses w ON w.Id = si.WarehouseId
            LEFT JOIN dbo.PaymentType pt ON pt.PaymentTypeId = si.PaymentTypeID
            LEFT JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId = si.PaymentMethodID
            {where}";
        var order = OrderBy(f, "si.InvoiceDate DESC, si.SalesInvoiceId DESC", new Dictionary<string, string>
        {
            ["invoiceNo"] = "si.SalesInvoiceNo",
            ["invoiceDate"] = "si.InvoiceDate",
            ["customer"] = "si.CustomerNameSnapshot",
            ["grandTotal"] = "si.GrandTotal",
            ["paid"] = "si.PaidAmount",
            ["balance"] = "si.BalanceAmount",
        });
        var (rows, total) = await PageAsync(conn, select, order, dp, f.Page, f.Size);
        result.Rows = rows;
        result.TotalCount = total;
        return result;
    }

    // ============================================================
    // 3. Sales Detail — one row per invoice line, optional grouping
    // ============================================================
    private async Task<SalesReportResult> DetailAsync(IDbConnection conn, SalesReportFilter f, DynamicParameters dp)
    {
        var result = new SalesReportResult();
        var where = BuildWhere(f);
        var join = @"FROM dbo.SalesInvoiceItem sii
            INNER JOIN dbo.SalesInvoice si ON si.SalesInvoiceId = sii.SalesInvoiceId ";

        if (string.IsNullOrWhiteSpace(f.GroupBy) || f.GroupBy == "none")
        {
            var select = $@"SELECT si.SalesInvoiceId, si.SalesInvoiceNo, si.InvoiceDate, si.CustomerNameSnapshot AS Customer,
                sii.ProductCodeSnapshot AS ProductCode, sii.ProductNameSnapshot AS Product,
                sii.UnitNameSnapshot AS Unit, sii.BarcodeSnapshot AS Barcode, sii.HSNCodeSnapshot AS HSN,
                sii.Quantity, sii.FreeQuantity, sii.Rate,
                sii.GrossAmount AS Gross, sii.DiscountPercentage AS DiscountPct, sii.DiscountAmount AS Discount,
                sii.TaxableAmount AS Taxable, sii.GSTPercent AS GstRate,
                sii.CGSTAmount AS CGST, sii.SGSTAmount AS SGST, sii.IGSTAmount AS IGST, sii.CESSAmount AS CESS,
                sii.LineTotal
                {join}{where}";
            var order = OrderBy(f, "si.InvoiceDate DESC, si.SalesInvoiceId DESC, sii.SalesInvoiceItemId", new Dictionary<string, string>
            {
                ["invoiceNo"] = "si.SalesInvoiceNo",
                ["invoiceDate"] = "si.InvoiceDate",
                ["product"] = "sii.ProductNameSnapshot",
                ["lineTotal"] = "sii.LineTotal",
            });
            var (rows, total) = await PageAsync(conn, select, order, dp, f.Page, f.Size);
            result.Rows = rows;
            result.TotalCount = total;
            return result;
        }

        var (dim, gb, ob) = f.GroupBy switch
        {
            "product" => ("sii.ProductId, sii.ProductCodeSnapshot AS ProductCode, sii.ProductNameSnapshot AS Product",
                "sii.ProductId, sii.ProductCodeSnapshot, sii.ProductNameSnapshot", "Product"),
            "hsn" => ("COALESCE(sii.HSNCodeSnapshot, 'Unknown') AS HSN", "COALESCE(sii.HSNCodeSnapshot, 'Unknown')", "HSN"),
            "month" => ("FORMAT(si.InvoiceDate, 'yyyy-MM') AS Period", "FORMAT(si.InvoiceDate, 'yyyy-MM')", "Period"),
            _ => ("sii.ProductNameSnapshot AS Product", "sii.ProductNameSnapshot", "Product"),
        };
        var sel = $@"SELECT {dim},
            COUNT(*) AS Lines, SUM(sii.Quantity) AS Quantity, SUM(sii.FreeQuantity) AS FreeQuantity,
            SUM(sii.GrossAmount) AS Gross, SUM(sii.DiscountAmount) AS Discount, SUM(sii.TaxableAmount) AS Taxable,
            SUM(sii.CGSTAmount) AS CGST, SUM(sii.SGSTAmount) AS SGST, SUM(sii.IGSTAmount) AS IGST,
            SUM(sii.CESSAmount) AS CESS, SUM(sii.LineTotal) AS LineTotal
            {join}{where} GROUP BY {gb}";
        var (rows1, total1) = await PageAsync(conn, sel, ob, dp, f.Page, f.Size);
        result.Rows = rows1;
        result.TotalCount = total1;
        return result;
    }

    // ============================================================
    // 4. Product-wise Sales — grouped by product snapshot columns
    // ============================================================
    private async Task<SalesReportResult> ProductAsync(IDbConnection conn, SalesReportFilter f, DynamicParameters dp)
    {
        var result = new SalesReportResult();
        var where = BuildWhere(f);
        var join = @"FROM dbo.SalesInvoiceItem sii
            INNER JOIN dbo.SalesInvoice si ON si.SalesInvoiceId = sii.SalesInvoiceId ";

        var (dim, gb, ob) = f.GroupBy switch
        {
            "hsn" => ("COALESCE(sii.HSNCodeSnapshot, 'Unknown') AS HSN", "COALESCE(sii.HSNCodeSnapshot, 'Unknown')", "HSN"),
            "month" => ("FORMAT(si.InvoiceDate, 'yyyy-MM') AS Period", "FORMAT(si.InvoiceDate, 'yyyy-MM')", "Period"),
            "customer" => ("si.CustomerId, si.CustomerNameSnapshot AS Customer", "si.CustomerId, si.CustomerNameSnapshot", "Customer"),
            _ => ("sii.ProductId, sii.ProductCodeSnapshot AS ProductCode, sii.ProductNameSnapshot AS Product",
                "sii.ProductId, sii.ProductCodeSnapshot, sii.ProductNameSnapshot", "Product"),
        };
        var select = $@"SELECT {dim},
            COUNT(*) AS Lines,
            COUNT(DISTINCT si.SalesInvoiceId) AS Invoices,
            SUM(sii.Quantity) AS Quantity, SUM(sii.FreeQuantity) AS FreeQuantity,
            SUM(sii.GrossAmount) AS Gross, SUM(sii.DiscountAmount) AS Discount, SUM(sii.TaxableAmount) AS Taxable,
            SUM(sii.CGSTAmount) AS CGST, SUM(sii.SGSTAmount) AS SGST, SUM(sii.IGSTAmount) AS IGST,
            SUM(sii.CESSAmount) AS CESS, SUM(sii.LineTotal) AS LineTotal
            {join}{where} GROUP BY {gb}";
        var (rows, total) = await PageAsync(conn, select, ob, dp, f.Page, f.Size);
        result.Rows = rows;
        result.TotalCount = total;
        return result;
    }

    // ============================================================
    // 5. Customer-wise Sales — grouped by customer snapshot columns
    // ============================================================
    private async Task<SalesReportResult> CustomerAsync(IDbConnection conn, SalesReportFilter f, DynamicParameters dp)
    {
        var result = new SalesReportResult();
        var where = BuildWhere(f);

        var (dim, gb, ob) = f.GroupBy switch
        {
            "branch" => ("br.BranchName AS Branch", "br.BranchName", "Branch"),
            "warehouse" => ("w.WarehouseName AS Warehouse", "w.WarehouseName", "Warehouse"),
            "month" => ("FORMAT(si.InvoiceDate, 'yyyy-MM') AS Period", "FORMAT(si.InvoiceDate, 'yyyy-MM')", "Period"),
            "sales-type" => ("CONCAT('Type-', ISNULL(CAST(si.SalesTypeId AS nvarchar(10)), '-')) AS SalesType",
                "CONCAT('Type-', ISNULL(CAST(si.SalesTypeId AS nvarchar(10)), '-'))", "SalesType"),
            "status" => ("COALESCE(si.InvoiceStatus, 'Unknown') AS Status", "COALESCE(si.InvoiceStatus, 'Unknown')", "Status"),
            _ => ("si.CustomerId, si.CustomerNameSnapshot AS Customer", "si.CustomerId, si.CustomerNameSnapshot", "Customer"),
        };

        var select = $@"SELECT {dim},
            COUNT(DISTINCT si.SalesInvoiceId) AS Invoices,
            SUM(si.TotalGrossAmount) AS Gross, SUM(si.TotalDiscountAmount) AS Discount,
            SUM(si.TotalTaxableAmount) AS Taxable,
            SUM(si.TotalCGSTAmount) AS CGST, SUM(si.TotalSGSTAmount) AS SGST,
            SUM(si.TotalIGSTAmount) AS IGST, SUM(si.TotalCESSAmount) AS CESS,
            SUM(si.GrandTotal) AS GrandTotal, SUM(si.PaidAmount) AS Paid, SUM(si.BalanceAmount) AS Balance,
            AVG(si.GrandTotal) AS AvgInvoice, MAX(si.InvoiceDate) AS LastInvoiceDate
            FROM dbo.SalesInvoice si
            LEFT JOIN dbo.Branches br ON br.Id = si.BranchId
            LEFT JOIN dbo.Warehouses w ON w.Id = si.WarehouseId
            {where} GROUP BY {gb}";
        var (rows, total) = await PageAsync(conn, select, ob, dp, f.Page, f.Size);
        result.Rows = rows;
        result.TotalCount = total;
        return result;
    }

    // ============================================================
    // 6. Daily Sales — one row per invoice date
    // ============================================================
    private async Task<SalesReportResult> DailyAsync(IDbConnection conn, SalesReportFilter f, DynamicParameters dp)
    {
        var result = new SalesReportResult();
        var where = BuildWhere(f);
        var select = $@"SELECT CAST(si.InvoiceDate AS date) AS Date,
            COUNT(*) AS Invoices,
            COUNT(DISTINCT si.CustomerId) AS Customers,
            SUM(si.TotalGrossAmount) AS Gross, SUM(si.TotalDiscountAmount) AS Discount,
            SUM(si.TotalTaxableAmount) AS Taxable,
            SUM(si.TotalCGSTAmount) AS CGST, SUM(si.TotalSGSTAmount) AS SGST,
            SUM(si.TotalIGSTAmount) AS IGST, SUM(si.TotalCESSAmount) AS CESS,
            SUM(si.TotalRoundOff) AS RoundOff,
            SUM(si.GrandTotal) AS GrandTotal, SUM(si.PaidAmount) AS Paid, SUM(si.BalanceAmount) AS Balance
            FROM dbo.SalesInvoice si
            {where} GROUP BY CAST(si.InvoiceDate AS date)";
        var (rows, total) = await PageAsync(conn, select, "Date DESC", dp, f.Page, f.Size);
        result.Rows = rows;
        result.TotalCount = total;
        return result;
    }

    // ============================================================
    // 7. Monthly Sales — one row per calendar month
    // ============================================================
    private async Task<SalesReportResult> MonthlyAsync(IDbConnection conn, SalesReportFilter f, DynamicParameters dp)
    {
        var result = new SalesReportResult();
        var where = BuildWhere(f);
        var select = $@"SELECT FORMAT(si.InvoiceDate, 'yyyy-MM') AS Period,
            COUNT(*) AS Invoices,
            COUNT(DISTINCT si.CustomerId) AS Customers,
            SUM(si.TotalGrossAmount) AS Gross, SUM(si.TotalDiscountAmount) AS Discount,
            SUM(si.TotalTaxableAmount) AS Taxable,
            SUM(si.TotalCGSTAmount) AS CGST, SUM(si.TotalSGSTAmount) AS SGST,
            SUM(si.TotalIGSTAmount) AS IGST, SUM(si.TotalCESSAmount) AS CESS,
            SUM(si.TotalRoundOff) AS RoundOff,
            SUM(si.GrandTotal) AS GrandTotal, SUM(si.PaidAmount) AS Paid, SUM(si.BalanceAmount) AS Balance
            FROM dbo.SalesInvoice si
            {where} GROUP BY FORMAT(si.InvoiceDate, 'yyyy-MM')";
        var (rows, total) = await PageAsync(conn, select, "Period DESC", dp, f.Page, f.Size);
        result.Rows = rows;
        result.TotalCount = total;
        return result;
    }
}
