using System.Data;
using Dapper;
using ONEERP.ERP.API.DTOs;

namespace ONEERP.ERP.API.Repositories;

public partial class PurchaseReportRepository
{
    // ============================================================
    // 9. Purchase Return Analysis
    // ============================================================
    private async Task<PurchaseReportResult> ReturnsAsync(IDbConnection conn, PurchaseReportFilter f, DynamicParameters dp)
    {
        var result = new PurchaseReportResult();
        var where = BuildReturnWhere(f);

        if (string.IsNullOrWhiteSpace(f.GroupBy) || f.GroupBy == "none")
        {
            var select = $@"SELECT pr.PurchaseReturnId, pr.PurchaseId, pr.ReturnNumber, pr.ReturnDate,
                ph.PurchaseNumber, pr.SupplierNameSnapshot AS Supplier, pr.BranchNameSnapshot AS Branch,
                COALESCE(w.WarehouseName, '') AS Warehouse, pr.Reason, st.Name AS Status,
                (SELECT COALESCE(SUM(pri.ReturnQuantity),0) FROM dbo.PurchaseReturnItem pri
                 WHERE pri.PurchaseReturnId = pr.PurchaseReturnId) AS Quantity,
                pr.TotalTaxableAmount AS Taxable, pr.TotalTaxAmount AS GST, pr.TotalCessAmount AS CESS, pr.GrandTotal
                FROM dbo.PurchaseReturn pr
                LEFT JOIN dbo.Purchase ph ON ph.PurchaseId = pr.PurchaseId
                LEFT JOIN dbo.Warehouses w ON w.Id = pr.WarehouseId
                LEFT JOIN dbo.[Status] st ON st.StatusId = pr.StatusID
                {where}";
            var (rows, total) = await PageAsync(conn, select, "pr.ReturnDate DESC, pr.PurchaseReturnId DESC", dp, f.Page, f.Size);
            result.Rows = rows;
            result.TotalCount = total;
            return result;
        }

        var (dim, gb, ob) = f.GroupBy switch
        {
            "reason" => ("COALESCE(NULLIF(pr.Reason, ''), 'Not specified') AS Reason", "COALESCE(NULLIF(pr.Reason, ''), 'Not specified')", "Reason"),
            "supplier" => ("pr.SupplierId, pr.SupplierNameSnapshot AS Supplier", "pr.SupplierId, pr.SupplierNameSnapshot", "Supplier"),
            "status" => ("COALESCE(st.Name, 'Unknown') AS Status", "COALESCE(st.Name, 'Unknown')", "Status"),
            "branch" => ("COALESCE(pr.BranchNameSnapshot, 'Unknown') AS Branch", "COALESCE(pr.BranchNameSnapshot, 'Unknown')", "Branch"),
            "warehouse" => ("COALESCE(w.WarehouseName, '') AS Warehouse", "COALESCE(w.WarehouseName, '')", "Warehouse"),
            "purchase-type" => ("CONCAT('Type-', ISNULL(CAST(pr.PurchaseReturnTypeId AS nvarchar(10)), '-')) AS PurchaseType", "CONCAT('Type-', ISNULL(CAST(pr.PurchaseReturnTypeId AS nvarchar(10)), '-'))", "PurchaseType"),
            "month" => ("FORMAT(pr.ReturnDate, 'yyyy-MM') AS Period", "FORMAT(pr.ReturnDate, 'yyyy-MM')", "Period"),
            "quarter" => ("CONCAT(YEAR(pr.ReturnDate), '-Q', DATEPART(QUARTER, pr.ReturnDate)) AS Period", "CONCAT(YEAR(pr.ReturnDate), '-Q', DATEPART(QUARTER, pr.ReturnDate))", "Period"),
            "year" => ("YEAR(pr.ReturnDate) AS Period", "YEAR(pr.ReturnDate)", "Period"),
            "product" => ("pri.ProductId, pri.ProductNameSnapshot AS Product", "pri.ProductId, pri.ProductNameSnapshot", "Product"),
            _ => ("pr.SupplierId, pr.SupplierNameSnapshot AS Supplier", "pr.SupplierId, pr.SupplierNameSnapshot", "Supplier"),
        };

        if (f.GroupBy == "product")
        {
            var select = $@"SELECT {dim},
                COUNT(DISTINCT pr.PurchaseReturnId) AS Returns,
                SUM(pri.ReturnQuantity) AS Quantity, SUM(pri.TaxableValue) AS Taxable,
                SUM(pri.GSTAmount) AS GST, SUM(pri.CESSAmount) AS CESS, SUM(pri.LineTotal) AS LineTotal
                FROM dbo.PurchaseReturnItem pri
                INNER JOIN dbo.PurchaseReturn pr ON pr.PurchaseReturnId = pri.PurchaseReturnId
                LEFT JOIN dbo.[Status] st ON st.StatusId = pr.StatusID
                LEFT JOIN dbo.Warehouses w ON w.Id = pr.WarehouseId
                {where} GROUP BY {gb}";
            var (rows, total) = await PageAsync(conn, select, ob, dp, f.Page, f.Size);
            result.Rows = rows;
            result.TotalCount = total;
            return result;
        }

        var selHeader = $@"SELECT {dim},
            COUNT(DISTINCT pr.PurchaseReturnId) AS Returns,
            COALESCE(SUM(ri.ReturnItemQty),0) AS Quantity,
            SUM(pr.TotalTaxableAmount) AS Taxable, SUM(pr.TotalTaxAmount) AS GST,
            SUM(pr.TotalCessAmount) AS CESS, SUM(pr.GrandTotal) AS GrandTotal
            FROM dbo.PurchaseReturn pr
            LEFT JOIN (
                SELECT pri.PurchaseReturnId, SUM(pri.ReturnQuantity) AS ReturnItemQty
                FROM dbo.PurchaseReturnItem pri GROUP BY pri.PurchaseReturnId
            ) ri ON ri.PurchaseReturnId = pr.PurchaseReturnId
            LEFT JOIN dbo.Purchase ph ON ph.PurchaseId = pr.PurchaseId
            LEFT JOIN dbo.Warehouses w ON w.Id = pr.WarehouseId
            LEFT JOIN dbo.[Status] st ON st.StatusId = pr.StatusID
            {where} GROUP BY {gb}";
        var (rows1, total1) = await PageAsync(conn, selHeader, ob, dp, f.Page, f.Size);
        result.Rows = rows1;
        result.TotalCount = total1;
        return result;
    }

    // ============================================================
    // 10. Purchase Reconciliation — purchases vs returns vs payments
    // ============================================================
    private async Task<PurchaseReportResult> ReconciliationAsync(IDbConnection conn, PurchaseReportFilter f, DynamicParameters dp)
    {
        var result = new PurchaseReportResult();
        var mode = string.IsNullOrWhiteSpace(f.Mode) ? "amount" : f.Mode;
        var group = string.IsNullOrWhiteSpace(f.GroupBy) || f.GroupBy == "none" ? "none" : f.GroupBy;

        var itemDim = group switch
        {
            "product" => "pi.ProductId, pi.ProductNameSnapshot AS Product",
            "category" => "pi.CategoryID, COALESCE(c.CategoryName, 'Uncategorised') AS Category",
            "brand" => "pi.BrandID, COALESCE(b.BrandName, 'Unknown') AS Brand",
            _ => "",
        };
        var itemGb = group switch
        {
            "product" => "pi.ProductId, pi.ProductNameSnapshot",
            "category" => "pi.CategoryID, COALESCE(c.CategoryName, 'Uncategorised')",
            "brand" => "pi.BrandID, COALESCE(b.BrandName, 'Unknown')",
            _ => "",
        };
        var itemJoin = @"FROM dbo.PurchaseItem pi
            INNER JOIN dbo.Purchase p ON p.PurchaseId = pi.PurchaseId
            LEFT JOIN dbo.ProductCategories c ON c.Id = pi.CategoryID
            LEFT JOIN dbo.ProductBrands b ON b.Id = pi.BrandID ";

        // --- quantity mode: purchase qty vs returned qty vs net ---
        if (mode == "quantity")
        {
            var qtyRt = @"LEFT JOIN (
                    SELECT pr.PurchaseId, pri.ProductId,
                        COALESCE(SUM(pri.ReturnQuantity),0) AS ReturnQty
                    FROM dbo.PurchaseReturnItem pri
                    INNER JOIN dbo.PurchaseReturn pr ON pr.PurchaseReturnId = pri.PurchaseReturnId
                    WHERE pr.CompanyId = @companyId
                    GROUP BY pr.PurchaseId, pri.ProductId
                ) rt ON rt.PurchaseId = pi.PurchaseId AND rt.ProductId = pi.ProductId";
            if (itemDim != "")
            {
                var select = $@"SELECT {itemDim},
                    SUM(pi.Quantity) AS PurchaseQty, COALESCE(SUM(rt.ReturnQty),0) AS ReturnQty,
                    SUM(pi.Quantity) - COALESCE(SUM(rt.ReturnQty),0) AS NetQty
                    {itemJoin}{qtyRt}{whereAlias(f)} GROUP BY {itemGb}";
                var (rows, total) = await PageAsync(conn, select, "Product", dp, f.Page, f.Size);
                result.Rows = rows;
                result.TotalCount = total;
                return result;
            }
            var selectAll = $@"SELECT COALESCE(SUM(t.Quantity),0) AS PurchaseQty,
                    COALESCE(SUM(rt.ReturnQty),0) AS ReturnQty,
                    COALESCE(SUM(t.Quantity),0) - COALESCE(SUM(rt.ReturnQty),0) AS NetQty
                    FROM (
                        SELECT pi.PurchaseId, pi.ProductId, pi.Quantity
                        FROM dbo.PurchaseItem pi INNER JOIN dbo.Purchase p ON p.PurchaseId = pi.PurchaseId
                        {whereAlias(f)}
                    ) t
                    LEFT JOIN (
                        SELECT pr.PurchaseId, pri.ProductId, COALESCE(SUM(pri.ReturnQuantity),0) AS ReturnQty
                        FROM dbo.PurchaseReturnItem pri
                        INNER JOIN dbo.PurchaseReturn pr ON pr.PurchaseReturnId = pri.PurchaseReturnId
                        WHERE pr.CompanyId = @companyId
                        GROUP BY pr.PurchaseId, pri.ProductId
                    ) rt ON rt.PurchaseId = t.PurchaseId AND rt.ProductId = t.ProductId";
            result.Rows = (await Sql.QueryAsync<dynamic>(conn, selectAll, dp)).Select(ToRow).ToList();
            result.TotalCount = result.Rows.Count;
            return result;
        }

        // --- tax mode: purchase GST vs return GST vs net ---
        if (mode == "tax")
        {
            var taxRt = @"LEFT JOIN (
                    SELECT pr.PurchaseId, pri.ProductId,
                        COALESCE(SUM(pri.TaxableValue),0) AS ReturnTaxable,
                        COALESCE(SUM(pri.GSTAmount),0) AS ReturnGST,
                        COALESCE(SUM(pri.CESSAmount),0) AS ReturnCESS
                    FROM dbo.PurchaseReturnItem pri
                    INNER JOIN dbo.PurchaseReturn pr ON pr.PurchaseReturnId = pri.PurchaseReturnId
                    WHERE pr.CompanyId = @companyId
                    GROUP BY pr.PurchaseId, pri.ProductId
                ) rt ON rt.PurchaseId = pi.PurchaseId AND rt.ProductId = pi.ProductId";
            if (itemDim != "")
            {
                var select = $@"SELECT {itemDim},
                    SUM(pi.TaxableValue) AS PurchaseTaxable, SUM(pi.GSTAmount) AS PurchaseGST, SUM(pi.CESSAmount) AS PurchaseCESS,
                    COALESCE(SUM(rt.ReturnTaxable),0) AS ReturnTaxable, COALESCE(SUM(rt.ReturnGST),0) AS ReturnGST,
                    COALESCE(SUM(rt.ReturnCESS),0) AS ReturnCESS,
                    SUM(pi.GSTAmount) - COALESCE(SUM(rt.ReturnGST),0) AS NetGST
                    {itemJoin}{taxRt}{whereAlias(f)} GROUP BY {itemGb}";
                var (rows, total) = await PageAsync(conn, select, "Product", dp, f.Page, f.Size);
                result.Rows = rows;
                result.TotalCount = total;
                return result;
            }
            var selectAll = $@"SELECT COALESCE(SUM(t.TaxableValue),0) AS PurchaseTaxable,
                    COALESCE(SUM(t.GSTAmount),0) AS PurchaseGST, COALESCE(SUM(t.CESSAmount),0) AS PurchaseCESS,
                    COALESCE(SUM(rt.ReturnTaxable),0) AS ReturnTaxable, COALESCE(SUM(rt.ReturnGST),0) AS ReturnGST,
                    COALESCE(SUM(rt.ReturnCESS),0) AS ReturnCESS,
                    COALESCE(SUM(t.GSTAmount),0) - COALESCE(SUM(rt.ReturnGST),0) AS NetGST
                    FROM (
                        SELECT pi.PurchaseId, pi.ProductId, pi.TaxableValue, pi.GSTAmount, pi.CESSAmount
                        FROM dbo.PurchaseItem pi INNER JOIN dbo.Purchase p ON p.PurchaseId = pi.PurchaseId
                        {whereAlias(f)}
                    ) t
                    LEFT JOIN (
                        SELECT pr.PurchaseId, pri.ProductId,
                            COALESCE(SUM(pri.TaxableValue),0) AS ReturnTaxable,
                            COALESCE(SUM(pri.GSTAmount),0) AS ReturnGST,
                            COALESCE(SUM(pri.CESSAmount),0) AS ReturnCESS
                        FROM dbo.PurchaseReturnItem pri
                        INNER JOIN dbo.PurchaseReturn pr ON pr.PurchaseReturnId = pri.PurchaseReturnId
                        WHERE pr.CompanyId = @companyId
                        GROUP BY pr.PurchaseId, pri.ProductId
                    ) rt ON rt.PurchaseId = t.PurchaseId AND rt.ProductId = t.ProductId";
            result.Rows = (await Sql.QueryAsync<dynamic>(conn, selectAll, dp)).Select(ToRow).ToList();
            result.TotalCount = result.Rows.Count;
            return result;
        }

        // --- amount / payment / return modes — header level purchase vs return vs paid ---
        var (dim, gb, ob) = group switch
        {
            "month" => ("FORMAT(p.PurchaseDate, 'yyyy-MM') AS Period", "FORMAT(p.PurchaseDate, 'yyyy-MM')", "Period"),
            "quarter" => ("CONCAT(YEAR(p.PurchaseDate), '-Q', DATEPART(QUARTER, p.PurchaseDate)) AS Period", "CONCAT(YEAR(p.PurchaseDate), '-Q', DATEPART(QUARTER, p.PurchaseDate))", "Period"),
            "year" => ("YEAR(p.PurchaseDate) AS Period", "YEAR(p.PurchaseDate)", "Period"),
            "supplier" => ("p.SupplierId, p.SupplierNameSnapshot AS Supplier", "p.SupplierId, p.SupplierNameSnapshot", "Supplier"),
            "branch" => ("COALESCE(p.BranchNameSnapshot, 'Unknown') AS Branch", "COALESCE(p.BranchNameSnapshot, 'Unknown')", "Branch"),
            "warehouse" => ("COALESCE(w.WarehouseName, '') AS Warehouse", "COALESCE(w.WarehouseName, '')", "Warehouse"),
            "status" => ("COALESCE(st.Name, 'Unknown') AS Status", "COALESCE(st.Name, 'Unknown')", "Status"),
            "purchase-type" => ("CONCAT('Type-', ISNULL(CAST(p.PurchaseTypeId AS nvarchar(10)), '-')) AS PurchaseType", "CONCAT('Type-', ISNULL(CAST(p.PurchaseTypeId AS nvarchar(10)), '-'))", "PurchaseType"),
            _ => ("", "", "Period"),
        };

        var headWhere = BuildWhere(f, "PurchaseDate", "PurchaseNumber");
        var headDim = string.IsNullOrEmpty(dim) ? "" : dim + ",";
        var selectHead = $@"SELECT {headDim}
                COUNT(DISTINCT p.PurchaseId) AS Purchases,
                SUM(p.TotalGrossAmount) AS PurchaseGross, SUM(p.TotalDiscountAmount) AS PurchaseDiscount,
                SUM(p.TotalTaxableAmount) AS PurchaseTaxable, SUM(p.TotalTaxAmount) AS PurchaseGST, SUM(p.TotalCessAmount) AS PurchaseCESS,
                SUM(p.GrandTotal) AS PurchaseGrand, SUM(p.PaidAmount) AS Paid, SUM(p.BalanceAmount) AS Outstanding,
                COALESCE(SUM(rt.ReturnCount),0) AS ReturnCount, COALESCE(SUM(rt.ReturnGrand),0) AS ReturnGrand,
                SUM(p.GrandTotal) - COALESCE(SUM(rt.ReturnGrand),0) AS NetPurchase
                FROM dbo.Purchase p
                LEFT JOIN (
                    SELECT pr.PurchaseId, COUNT(*) AS ReturnCount, SUM(pr.GrandTotal) AS ReturnGrand
                    FROM dbo.PurchaseReturn pr WHERE pr.CompanyId = @companyId
                    GROUP BY pr.PurchaseId
                ) rt ON rt.PurchaseId = p.PurchaseId
                LEFT JOIN dbo.Warehouses w ON w.Id = p.WarehouseId
                LEFT JOIN dbo.[Status] st ON st.StatusId = p.StatusID
                {headWhere}";
        if (gb == "")
        {
            result.Rows = (await Sql.QueryAsync<dynamic>(conn, selectHead, dp)).Select(ToRow).ToList();
            result.TotalCount = result.Rows.Count;
            return result;
        }
        var (rows1, total1) = await PageAsync(conn, selectHead + " GROUP BY " + gb, ob, dp, f.Page, f.Size);
        result.Rows = rows1;
        result.TotalCount = total1;
        return result;
    }

    private static string whereAlias(PurchaseReportFilter f) => BuildWhere(f, "PurchaseDate", "PurchaseNumber");
}