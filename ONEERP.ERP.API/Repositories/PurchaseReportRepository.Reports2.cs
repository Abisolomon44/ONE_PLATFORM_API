using System.Data;
using Dapper;
using ONEERP.ERP.API.DTOs;

namespace ONEERP.ERP.API.Repositories;

public partial class PurchaseReportRepository
{
    // ============================================================
    // 5. Product / Price Analysis — history | rate | supplier
    // ============================================================
    private async Task<PurchaseReportResult> ProductPriceAsync(IDbConnection conn, PurchaseReportFilter f, DynamicParameters dp)
    {
        var result = new PurchaseReportResult();
        var mode = string.IsNullOrWhiteSpace(f.Mode) ? "history" : f.Mode;
        var where = BuildWhere(f, "PurchaseDate", "PurchaseNumber");

        if (mode == "history")
        {
            var inner = $@"SELECT t.*,
                    CASE WHEN t.PrevRate IS NULL OR t.PrevRate = 0 THEN NULL
                         ELSE (t.PurchaseRate - t.PrevRate) * 100.0 / t.PrevRate END AS ChangePct
                FROM (
                    SELECT p.PurchaseId, p.PurchaseDate, p.PurchaseNumber, p.SupplierNameSnapshot AS Supplier,
                        pi.ProductId, pi.ProductNameSnapshot AS ProductName, pi.ProductCodeSnapshot AS ProductCode,
                        pi.UnitNameSnapshot AS Unit, pi.PurchaseRate, pi.Quantity,
                        LAG(pi.PurchaseRate, 1) OVER (PARTITION BY pi.ProductId ORDER BY p.PurchaseDate, p.PurchaseId, pi.PurchaseItemId) AS PrevRate,
                        ROW_NUMBER() OVER (ORDER BY p.PurchaseDate DESC, p.PurchaseId DESC, pi.PurchaseItemId DESC) AS _rn
                    FROM dbo.PurchaseItem pi
                    INNER JOIN dbo.Purchase p ON p.PurchaseId = pi.PurchaseId
                    {where}
                ) t";
            var (rows, total) = await WindowedPageAsync(conn, inner, dp, f.Page, f.Size);
            // change % vs previous rate is computed client-side-friendly as raw columns
            result.Rows = rows;
            result.TotalCount = total;
            return result;
        }

        if (mode == "supplier")
        {
            var select = $@"SELECT t.ProductId, MIN(t.ProductName) AS ProductName, MIN(t.ProductCode) AS ProductCode,
                    MIN(t.Unit) AS Unit, MIN(t.SupplierName) AS Supplier,
                    MIN(t.PurchaseRate) AS MinRate, MAX(t.PurchaseRate) AS MaxRate, AVG(t.PurchaseRate) AS AvgRate,
                    MIN(t.LastRate) AS LastRate, MIN(t.LastDate) AS LastDate, SUM(t.Quantity) AS Quantity, COUNT(*) AS Lines,
                    CASE WHEN AVG(t.PurchaseRate) = 0 THEN NULL ELSE (MIN(t.LastRate) - AVG(t.PurchaseRate)) * 100.0 / AVG(t.PurchaseRate) END AS ChangePct
                FROM (
                    SELECT pi.ProductId, pi.ProductNameSnapshot AS ProductName, pi.ProductCodeSnapshot AS ProductCode,
                        pi.UnitNameSnapshot AS Unit, p.SupplierId, p.SupplierNameSnapshot AS SupplierName,
                        pi.PurchaseRate, pi.Quantity, p.PurchaseDate,
                        FIRST_VALUE(pi.PurchaseRate) OVER (PARTITION BY pi.ProductId, p.SupplierId ORDER BY p.PurchaseDate DESC, p.PurchaseId DESC, pi.PurchaseItemId DESC) AS LastRate,
                        FIRST_VALUE(p.PurchaseDate) OVER (PARTITION BY pi.ProductId, p.SupplierId ORDER BY p.PurchaseDate DESC, p.PurchaseId DESC, pi.PurchaseItemId DESC) AS LastDate,
                        ROW_NUMBER() OVER (PARTITION BY pi.ProductId, p.SupplierId ORDER BY p.PurchaseDate DESC, p.PurchaseId DESC, pi.PurchaseItemId DESC) AS _rn
                    FROM dbo.PurchaseItem pi
                    INNER JOIN dbo.Purchase p ON p.PurchaseId = pi.PurchaseId
                    {where}
                ) t
                WHERE t._rn = 1
                GROUP BY t.ProductId";
            var (rows, total) = await PageAsync(conn, select, "ProductName", dp, f.Page, f.Size);
            result.Rows = rows;
            result.TotalCount = total;
            return result;
        }

        // rate mode — per product price summary
        var lastCond = BuildConditions(f, "PurchaseDate", "PurchaseNumber", "p2", "pi2");
        var sel = $@"SELECT pi.ProductId, pi.ProductNameSnapshot AS ProductName, pi.ProductCodeSnapshot AS ProductCode,
                pi.UnitNameSnapshot AS Unit,
                MIN(pi.PurchaseRate) AS MinRate, MAX(pi.PurchaseRate) AS MaxRate, AVG(pi.PurchaseRate) AS AvgRate,
                COUNT(DISTINCT p.PurchaseId) AS PurchaseCount, COALESCE(SUM(pi.Quantity),0) AS Quantity,
                (SELECT TOP 1 pi2.PurchaseRate FROM dbo.PurchaseItem pi2
                    INNER JOIN dbo.Purchase p2 ON p2.PurchaseId = pi2.PurchaseId
                    WHERE pi2.ProductId = pi.ProductId AND {lastCond}
                    ORDER BY p2.PurchaseDate DESC, p2.PurchaseId DESC, pi2.PurchaseItemId DESC) AS LastRate
            FROM dbo.PurchaseItem pi
            INNER JOIN dbo.Purchase p ON p.PurchaseId = pi.PurchaseId
            {where}
            GROUP BY pi.ProductId, pi.ProductNameSnapshot, pi.ProductCodeSnapshot, pi.UnitNameSnapshot";
        var (rows2, total2) = await PageAsync(conn, sel, "ProductName", dp, f.Page, f.Size);
        result.Rows = rows2;
        result.TotalCount = total2;
        return result;
    }

    // ============================================================
    // 6. Quantity / PO / GRN Analysis
    // ============================================================
    private async Task<PurchaseReportResult> QuantityAsync(IDbConnection conn, PurchaseReportFilter f, DynamicParameters dp)
    {
        var result = new PurchaseReportResult();
        var where = BuildWhere(f, "PurchaseDate", "PurchaseNumber");
        var join = @"FROM dbo.PurchaseItem pi
            INNER JOIN dbo.Purchase p ON p.PurchaseId = pi.PurchaseId
            LEFT JOIN dbo.ProductCategories c ON c.Id = pi.CategoryID
            LEFT JOIN dbo.ProductBrands b ON b.Id = pi.BrandID ";

        if (string.IsNullOrWhiteSpace(f.GroupBy) || f.GroupBy == "none")
        {
            var select = $@"SELECT pi.PurchaseId, p.PurchaseDate, p.PurchaseNumber,
                p.SupplierNameSnapshot AS Supplier,
                p.SupplierPONumber AS PONumber, pi.GRNId,
                pi.ProductId, pi.ProductNameSnapshot AS Product, pi.UnitNameSnapshot AS Unit, pi.PurchaseRate,
                pi.OrderedQuantity, pi.Quantity AS ReceivedQuantity, pi.ReturnedQuantity, pi.RemainingQuantity
                {join}{where}";
            var (rows, total) = await PageAsync(conn, select, "p.PurchaseDate DESC, p.PurchaseId DESC", dp, f.Page, f.Size);
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
            "supplier" => ("p.SupplierId, p.SupplierNameSnapshot AS Supplier", "p.SupplierId, p.SupplierNameSnapshot", "Supplier"),
            "po" => ("COALESCE(p.SupplierPONumber, '') AS PONumber", "COALESCE(p.SupplierPONumber, '')", "PONumber"),
            "grn" => ("COALESCE(CAST(pi.GRNId AS nvarchar(20)), '') AS GRN", "COALESCE(CAST(pi.GRNId AS nvarchar(20)), '')", "GRN"),
            "month" => ("FORMAT(p.PurchaseDate, 'yyyy-MM') AS Period", "FORMAT(p.PurchaseDate, 'yyyy-MM')", "Period"),
            _ => ("pi.ProductNameSnapshot AS Product", "pi.ProductNameSnapshot", "Product"),
        };
        var grouped = $@"SELECT {dim},
            SUM(pi.OrderedQuantity) AS OrderedQuantity, SUM(pi.Quantity) AS ReceivedQuantity,
            SUM(pi.ReturnedQuantity) AS ReturnedQuantity, SUM(pi.RemainingQuantity) AS RemainingQuantity,
            COALESCE(MAX(pi.UnitNameSnapshot), '') AS Unit, AVG(pi.PurchaseRate) AS AvgRate
            {join}{where} GROUP BY {gb}";
        var (rows1, total1) = await PageAsync(conn, grouped, ob, dp, f.Page, f.Size);
        result.Rows = rows1;
        result.TotalCount = total1;
        return result;
    }

    // ============================================================
    // 7. Tax Analysis
    // ============================================================
    private async Task<PurchaseReportResult> TaxAsync(IDbConnection conn, PurchaseReportFilter f, DynamicParameters dp)
    {
        var result = new PurchaseReportResult();
        var where = BuildWhere(f, "PurchaseDate", "PurchaseNumber");
        var join = @"FROM dbo.PurchaseItem pi
            INNER JOIN dbo.Purchase p ON p.PurchaseId = pi.PurchaseId
            LEFT JOIN dbo.ProductCategories c ON c.Id = pi.CategoryID
            LEFT JOIN dbo.ProductBrands b ON b.Id = pi.BrandID ";

        if (string.IsNullOrWhiteSpace(f.GroupBy) || f.GroupBy == "none")
        {
            var select = $@"SELECT pi.PurchaseId, p.PurchaseDate, p.PurchaseNumber,
                p.SupplierNameSnapshot AS Supplier,
                pi.ProductId, pi.ProductNameSnapshot AS Product, pi.HSNCodeSnapshot AS HSN,
                pi.GSTRate AS TaxRate, pi.TaxableValue,
                pi.CGSTAmount AS CGST, pi.SGSTAmount AS SGST, pi.IGSTAmount AS IGST,
                pi.CESSAmount AS CESS, pi.GSTAmount AS TotalTax
                {join}{where}";
            var (rows, total) = await PageAsync(conn, select, "p.PurchaseDate DESC, p.PurchaseId DESC", dp, f.Page, f.Size);
            result.Rows = rows;
            result.TotalCount = total;
            return result;
        }

        var (dim, gb, ob) = f.GroupBy switch
        {
            "gst-rate" => ("pi.GSTRate AS TaxRate", "pi.GSTRate", "TaxRate"),
            "hsn" => ("COALESCE(pi.HSNCodeSnapshot, 'Unknown') AS HSN", "COALESCE(pi.HSNCodeSnapshot, 'Unknown')", "HSN"),
            "product" => ("pi.ProductId, pi.ProductNameSnapshot AS Product", "pi.ProductId, pi.ProductNameSnapshot", "Product"),
            "category" => ("pi.CategoryID, COALESCE(c.CategoryName, 'Uncategorised') AS Category", "pi.CategoryID, COALESCE(c.CategoryName, 'Uncategorised')", "Category"),
            "brand" => ("pi.BrandID, COALESCE(b.BrandName, 'Unknown') AS Brand", "pi.BrandID, COALESCE(b.BrandName, 'Unknown')", "Brand"),
            "supplier" => ("p.SupplierId, p.SupplierNameSnapshot AS Supplier", "p.SupplierId, p.SupplierNameSnapshot", "Supplier"),
            "month" => ("FORMAT(p.PurchaseDate, 'yyyy-MM') AS Period", "FORMAT(p.PurchaseDate, 'yyyy-MM')", "Period"),
            "year" => ("YEAR(p.PurchaseDate) AS Period", "YEAR(p.PurchaseDate)", "Period"),
            _ => ("pi.GSTRate AS TaxRate", "pi.GSTRate", "TaxRate"),
        };
        var grouped = $@"SELECT {dim},
            COUNT(*) AS Lines, COALESCE(SUM(pi.Quantity),0) AS Quantity, SUM(pi.TaxableValue) AS Taxable,
            SUM(pi.CGSTAmount) AS CGST, SUM(pi.SGSTAmount) AS SGST, SUM(pi.IGSTAmount) AS IGST,
            SUM(pi.CESSAmount) AS CESS, SUM(pi.GSTAmount) AS TotalTax
            {join}{where} GROUP BY {gb}";
        var (rows1, total1) = await PageAsync(conn, grouped, ob, dp, f.Page, f.Size);
        result.Rows = rows1;
        result.TotalCount = total1;
        return result;
    }

    // ============================================================
    // 8. Payment / Payable Analysis
    // ============================================================
    private async Task<PurchaseReportResult> PaymentAsync(IDbConnection conn, PurchaseReportFilter f, DynamicParameters dp)
    {
        var result = new PurchaseReportResult();
        var where = BuildWhere(f, "PurchaseDate", "PurchaseNumber");

        var tot = await Sql.QueryFirstOrDefaultAsync<dynamic>(conn,
            $@"SELECT COALESCE(SUM(p.GrandTotal),0) AS Invoice, COALESCE(SUM(p.PaidAmount),0) AS Paid,
                COALESCE(SUM(p.BalanceAmount),0) AS Outstanding, COUNT(*) AS Count
               FROM dbo.Purchase p {where}", dp);
        var tk = (IDictionary<string, object>)tot!;
        result.Kpis = new List<PurchaseReportKpi>
        {
            Kpi("invoices", "Invoices", null, IVal(tk, "Count").ToString()),
            Kpi("invoiceTotal", "Invoice Total", Rnd(DVal(tk, "Invoice"))),
            Kpi("paid", "Paid", Rnd(DVal(tk, "Paid"))),
            Kpi("outstanding", "Outstanding", Rnd(DVal(tk, "Outstanding"))),
        };

        if (string.IsNullOrWhiteSpace(f.GroupBy) || f.GroupBy == "none")
        {
            var select = $@"SELECT p.PurchaseId, p.PurchaseNumber, p.PurchaseDate, p.SupplierNameSnapshot AS Supplier,
                pt.Name AS PaymentType, pm.Name AS PaymentMethod, st.Name AS Status,
                p.GrandTotal AS Invoice, p.PaidAmount AS Paid, p.BalanceAmount AS Balance,
                (SELECT COALESCE(SUM(pa.AllocatedAmount),0) FROM dbo.PaymentAllocation pa
                 WHERE pa.ReferenceType = 'PURCHASE' AND pa.ReferenceId = p.PurchaseId) AS Allocated,
                CASE WHEN p.BalanceAmount <= 0.005 THEN 'Paid' WHEN p.PaidAmount > 0 THEN 'Partial' ELSE 'Unpaid' END AS PaymentStatus
                FROM dbo.Purchase p
                LEFT JOIN dbo.PaymentType pt ON pt.PaymentTypeId = p.PaymentTypeID
                LEFT JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId = p.PaymentMethodID
                LEFT JOIN dbo.[Status] st ON st.StatusId = p.StatusID
                {where}";
            var (rows, total) = await PageAsync(conn, select, "p.PurchaseDate DESC, p.PurchaseId DESC", dp, f.Page, f.Size);
            result.Rows = rows;
            result.TotalCount = total;
            return result;
        }

        var (dim, gb, ob, extra) = f.GroupBy switch
        {
            "payment-type" => ("COALESCE(pt.Name, 'Unknown') AS PaymentType", "COALESCE(pt.Name, 'Unknown')", "PaymentType", ""),
            "payment-method" => ("COALESCE(pm.Name, 'Unknown') AS PaymentMethod", "COALESCE(pm.Name, 'Unknown')", "PaymentMethod", ""),
            "supplier" => ("p.SupplierId, p.SupplierNameSnapshot AS Supplier", "p.SupplierId, p.SupplierNameSnapshot", "Supplier", ""),
            "status" => ("COALESCE(st.Name, 'Unknown') AS Status", "COALESCE(st.Name, 'Unknown')", "Status", ""),
            "payment-status" => ("CASE WHEN p.BalanceAmount <= 0.005 THEN 'Paid' WHEN p.PaidAmount > 0 THEN 'Partial' ELSE 'Unpaid' END AS PaymentStatus", "CASE WHEN p.BalanceAmount <= 0.005 THEN 'Paid' WHEN p.PaidAmount > 0 THEN 'Partial' ELSE 'Unpaid' END", "PaymentStatus", ""),
            "month" => ("FORMAT(p.PurchaseDate, 'yyyy-MM') AS Period", "FORMAT(p.PurchaseDate, 'yyyy-MM')", "Period", ""),
            "year" => ("YEAR(p.PurchaseDate) AS Period", "YEAR(p.PurchaseDate)", "Period", ""),
            _ => ("COALESCE(pt.Name, 'Unknown') AS PaymentType", "COALESCE(pt.Name, 'Unknown')", "PaymentType", ""),
        };
        var grouped = $@"SELECT {dim},
            COUNT(*) AS Invoices, SUM(p.GrandTotal) AS Invoice, SUM(p.PaidAmount) AS Paid, SUM(p.BalanceAmount) AS Balance
            FROM dbo.Purchase p
            LEFT JOIN dbo.PaymentType pt ON pt.PaymentTypeId = p.PaymentTypeID
            LEFT JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId = p.PaymentMethodID
            LEFT JOIN dbo.[Status] st ON st.StatusId = p.StatusID
            {where} GROUP BY {gb}";
        var (rows1, total1) = await PageAsync(conn, grouped, ob, dp, f.Page, f.Size);
        result.Rows = rows1;
        result.TotalCount = total1;
        return result;
    }
}