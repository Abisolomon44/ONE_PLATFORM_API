using System.Data;
using Dapper;
using ONEERP.ERP.API.DTOs;

namespace ONEERP.ERP.API.Repositories;

public partial class SalesReportRepository
{
    // ============================================================
    // 8. Tax / GST Analysis — item-level tax lines or grouped
    // ============================================================
    private async Task<SalesReportResult> TaxAsync(IDbConnection conn, SalesReportFilter f, DynamicParameters dp)
    {
        var result = new SalesReportResult();
        var where = BuildWhere(f);
        var join = @"FROM dbo.SalesInvoiceItem sii
            INNER JOIN dbo.SalesInvoice si ON si.SalesInvoiceId = sii.SalesInvoiceId ";

        if (string.IsNullOrWhiteSpace(f.GroupBy) || f.GroupBy == "none")
        {
            var select = $@"SELECT si.SalesInvoiceId, si.SalesInvoiceNo, si.InvoiceDate, si.CustomerNameSnapshot AS Customer,
                sii.ProductNameSnapshot AS Product, sii.HSNCodeSnapshot AS HSN,
                sii.GSTPercent AS GstRate, sii.TaxableAmount AS Taxable,
                sii.CGSTAmount AS CGST, sii.SGSTAmount AS SGST, sii.IGSTAmount AS IGST, sii.CESSAmount AS CESS,
                sii.CGSTAmount + sii.SGSTAmount + sii.IGSTAmount + sii.CESSAmount AS TotalTax
                {join}{where}";
            var order = OrderBy(f, "si.InvoiceDate DESC, si.SalesInvoiceId DESC, sii.SalesInvoiceItemId", new Dictionary<string, string>
            {
                ["invoiceNo"] = "si.SalesInvoiceNo",
                ["invoiceDate"] = "si.InvoiceDate",
                ["gstRate"] = "sii.GSTPercent",
                ["taxable"] = "sii.TaxableAmount",
                ["totalTax"] = "sii.CGSTAmount + sii.SGSTAmount + sii.IGSTAmount + sii.CESSAmount",
            });
            var (rows, total) = await PageAsync(conn, select, order, dp, f.Page, f.Size);
            result.Rows = rows;
            result.TotalCount = total;
            return result;
        }

        var (dim, gb, ob) = f.GroupBy switch
        {
            "gst-rate" => ("CONCAT(CAST(sii.GSTPercent AS decimal(8,1)), '%') AS GstRate", "sii.GSTPercent", "GstRate"),
            "hsn" => ("COALESCE(sii.HSNCodeSnapshot, 'Unknown') AS HSN", "COALESCE(sii.HSNCodeSnapshot, 'Unknown')", "HSN"),
            "product" => ("sii.ProductId, sii.ProductNameSnapshot AS Product", "sii.ProductId, sii.ProductNameSnapshot", "Product"),
            "customer" => ("si.CustomerId, si.CustomerNameSnapshot AS Customer", "si.CustomerId, si.CustomerNameSnapshot", "Customer"),
            "month" => ("FORMAT(si.InvoiceDate, 'yyyy-MM') AS Period", "FORMAT(si.InvoiceDate, 'yyyy-MM')", "Period"),
            _ => ("CONCAT(CAST(sii.GSTPercent AS decimal(8,1)), '%') AS GstRate", "sii.GSTPercent", "GstRate"),
        };
        var sel = $@"SELECT {dim},
            COUNT(*) AS Lines,
            SUM(sii.TaxableAmount) AS Taxable,
            SUM(sii.CGSTAmount) AS CGST, SUM(sii.SGSTAmount) AS SGST,
            SUM(sii.IGSTAmount) AS IGST, SUM(sii.CESSAmount) AS CESS,
            SUM(sii.CGSTAmount + sii.SGSTAmount + sii.IGSTAmount + sii.CESSAmount) AS TotalTax
            {join}{where} GROUP BY {gb}";
        var (rows1, total1) = await PageAsync(conn, sel, ob, dp, f.Page, f.Size);
        result.Rows = rows1;
        result.TotalCount = total1;
        return result;
    }

    // ============================================================
    // 9. HSN/SAC Summary — grouped by HSN snapshot + GST rate
    // ============================================================
    private async Task<SalesReportResult> HsnAsync(IDbConnection conn, SalesReportFilter f, DynamicParameters dp)
    {
        var result = new SalesReportResult();
        var where = BuildWhere(f);
        var select = $@"SELECT COALESCE(sii.HSNCodeSnapshot, 'Unknown') AS HSN,
            CONCAT(CAST(sii.GSTPercent AS decimal(8,1)), '%') AS GstRate,
            COUNT(*) AS Lines,
            SUM(sii.Quantity) AS Quantity,
            SUM(sii.TaxableAmount) AS Taxable,
            SUM(sii.CGSTAmount) AS CGST, SUM(sii.SGSTAmount) AS SGST,
            SUM(sii.IGSTAmount) AS IGST, SUM(sii.CESSAmount) AS CESS,
            SUM(sii.CGSTAmount + sii.SGSTAmount + sii.IGSTAmount + sii.CESSAmount) AS TotalTax
            FROM dbo.SalesInvoiceItem sii
            INNER JOIN dbo.SalesInvoice si ON si.SalesInvoiceId = sii.SalesInvoiceId
            {where} GROUP BY COALESCE(sii.HSNCodeSnapshot, 'Unknown'), sii.GSTPercent";
        var (rows, total) = await PageAsync(conn, select, "HSN, GstRate", dp, f.Page, f.Size);
        result.Rows = rows;
        result.TotalCount = total;
        return result;
    }

    // ============================================================
    // 10. Payment Analysis — dbo.Payment rows with ReferenceType='SALES',
    //     reconciled against dbo.PaymentAllocation.
    //     ReferenceId is only joined to SalesInvoice when the allocation
    //     reference type is actually SALES — never assumed.
    // ============================================================
    private async Task<SalesReportResult> PaymentsAsync(IDbConnection conn, SalesReportFilter f, DynamicParameters dp)
    {
        var result = new SalesReportResult();
        var c = new List<string> { "pay.CompanyId = @companyId", "pay.ReferenceType = 'SALES'" };
        if (f.DateFrom.HasValue) c.Add("pay.PaymentDate >= @dateFrom");
        if (f.DateTo.HasValue) c.Add("pay.PaymentDate < DATEADD(DAY, 1, @dateTo)");
        if (f.PaymentTypeID.HasValue) c.Add("pay.PaymentTypeID = @paymentTypeID");
        if (f.PaymentMethodID.HasValue) c.Add("pay.PaymentMethodID = @paymentMethodID");
        if (!string.IsNullOrWhiteSpace(f.Search)) c.Add("(pay.PaymentNo LIKE @search OR si.SalesInvoiceNo LIKE @search OR si.CustomerNameSnapshot LIKE @search)");
        if (f.CustomerId.HasValue) c.Add("si.CustomerId = @customerId");
        if (f.BranchId.HasValue) c.Add("si.BranchId = @branchId");
        if (f.WarehouseId.HasValue) c.Add("si.WarehouseId = @warehouseId");
        if (f.ProductId.HasValue || f.HsnId.HasValue || f.GstRate.HasValue)
        {
            var item = new List<string> { "sii.SalesInvoiceId = si.SalesInvoiceId" };
            if (f.ProductId.HasValue) item.Add("sii.ProductId = @productId");
            if (f.HsnId.HasValue) item.Add("sii.HSNID = @hsnId");
            if (f.GstRate.HasValue) item.Add("sii.GSTPercent = @gstRate");
            c.Add($"EXISTS (SELECT 1 FROM dbo.SalesInvoiceItem sii WHERE {string.Join(" AND ", item)})");
        }
        var where = "WHERE " + string.Join(" AND ", c);

        var select = $@"SELECT pay.PaymentId, pay.PaymentNo, pay.PaymentDate,
            si.SalesInvoiceId, si.SalesInvoiceNo, si.CustomerNameSnapshot AS Customer,
            pt.Name AS PaymentType, pm.Name AS PaymentMethod, pay.ReferenceNo,
            pay.Amount,
            COALESCE(alloc.Allocated, 0) AS Allocated,
            pay.Amount - COALESCE(alloc.Allocated, 0) AS Unallocated
            FROM dbo.Payment pay
            INNER JOIN dbo.SalesInvoice si ON si.SalesInvoiceId = pay.ReferenceId
            LEFT JOIN dbo.PaymentType pt ON pt.PaymentTypeId = pay.PaymentTypeID
            LEFT JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId = pay.PaymentMethodID
            LEFT JOIN (SELECT PaymentId, SUM(AllocatedAmount) AS Allocated FROM dbo.PaymentAllocation GROUP BY PaymentId) alloc
                   ON alloc.PaymentId = pay.PaymentId
            {where}";
        var order = OrderBy(f, "pay.PaymentDate DESC, pay.PaymentId DESC", new Dictionary<string, string>
        {
            ["paymentNo"] = "pay.PaymentNo",
            ["paymentDate"] = "pay.PaymentDate",
            ["invoiceNo"] = "si.SalesInvoiceNo",
            ["amount"] = "pay.Amount",
        });
        var (rows, total) = await PageAsync(conn, select, order, dp, f.Page, f.Size);
        result.Rows = rows;
        result.TotalCount = total;
        return result;
    }

    // ============================================================
    // 11. Outstanding — invoices with a balance, reconciled against
    //     allocation-level data (PaymentAllocation, ReferenceType='SALES')
    // ============================================================
    private async Task<SalesReportResult> OutstandingAsync(IDbConnection conn, SalesReportFilter f, DynamicParameters dp)
    {
        var result = new SalesReportResult();
        var c = new List<string> { "si.CompanyId = @companyId", "si.BalanceAmount > 0.005" };
        if (f.DateFrom.HasValue) c.Add("si.InvoiceDate >= @dateFrom");
        if (f.DateTo.HasValue) c.Add("si.InvoiceDate < DATEADD(DAY, 1, @dateTo)");
        if (f.BranchId.HasValue) c.Add("si.BranchId = @branchId");
        if (f.WarehouseId.HasValue) c.Add("si.WarehouseId = @warehouseId");
        if (f.CustomerId.HasValue) c.Add("si.CustomerId = @customerId");
        if (!string.IsNullOrWhiteSpace(f.InvoiceStatus)) c.Add("si.InvoiceStatus = @invoiceStatus");
        if (!string.IsNullOrWhiteSpace(f.InvoiceNumber)) c.Add("si.SalesInvoiceNo LIKE @invoiceNumber");
        if (!string.IsNullOrWhiteSpace(f.Search)) c.Add("(si.SalesInvoiceNo LIKE @search OR si.CustomerNameSnapshot LIKE @search)");
        var where = "WHERE " + string.Join(" AND ", c);

        var select = $@"SELECT si.SalesInvoiceId, si.SalesInvoiceNo, si.InvoiceDate,
            si.CustomerNameSnapshot AS Customer, br.BranchName AS Branch,
            si.GrandTotal, si.PaidAmount AS Paid, si.BalanceAmount AS Balance,
            COALESCE(alloc.Allocated, 0) AS Allocated,
            si.BalanceAmount - COALESCE(alloc.Allocated, 0) AS Unallocated,
            si.InvoiceStatus,
            CASE WHEN si.PaidAmount > 0 THEN 'Partial' ELSE 'Unpaid' END AS PaymentStatus
            FROM dbo.SalesInvoice si
            LEFT JOIN dbo.Branches br ON br.Id = si.BranchId
            LEFT JOIN (SELECT ReferenceId, SUM(AllocatedAmount) AS Allocated
                       FROM dbo.PaymentAllocation WHERE ReferenceType = 'SALES' GROUP BY ReferenceId) alloc
                   ON alloc.ReferenceId = si.SalesInvoiceId
            {where}";
        var order = OrderBy(f, "si.InvoiceDate DESC, si.SalesInvoiceId DESC", new Dictionary<string, string>
        {
            ["invoiceNo"] = "si.SalesInvoiceNo",
            ["invoiceDate"] = "si.InvoiceDate",
            ["customer"] = "si.CustomerNameSnapshot",
            ["balance"] = "si.BalanceAmount",
        });
        var (rows, total) = await PageAsync(conn, select, order, dp, f.Page, f.Size);
        result.Rows = rows;
        result.TotalCount = total;
        return result;
    }

    // ============================================================
    // 12. POS / Source Analysis — SourceType + POSSessionId only
    //     (no invented POS columns)
    // ============================================================
    private async Task<SalesReportResult> PosAsync(IDbConnection conn, SalesReportFilter f, DynamicParameters dp)
    {
        var result = new SalesReportResult();
        var where = BuildWhere(f);

        if (string.IsNullOrWhiteSpace(f.GroupBy) || f.GroupBy == "none")
        {
            var select = $@"SELECT si.SalesInvoiceId, si.SalesInvoiceNo, si.InvoiceDate, si.SourceType,
                si.POSSessionId, si.CustomerNameSnapshot AS Customer,
                si.GrandTotal, si.PaidAmount AS Paid, si.BalanceAmount AS Balance, si.InvoiceStatus
                FROM dbo.SalesInvoice si
                {where}";
            var (rows, total) = await PageAsync(conn, select, "si.InvoiceDate DESC, si.SalesInvoiceId DESC", dp, f.Page, f.Size);
            result.Rows = rows;
            result.TotalCount = total;
            return result;
        }

        var (dim, gb, ob) = f.GroupBy switch
        {
            "session" => ("si.POSSessionId, COALESCE(si.SourceType, 'UNKNOWN') AS SourceType",
                "si.POSSessionId, COALESCE(si.SourceType, 'UNKNOWN')", "Source"),
            "month" => ("FORMAT(si.InvoiceDate, 'yyyy-MM') AS Period", "FORMAT(si.InvoiceDate, 'yyyy-MM')", "Period"),
            _ => ("COALESCE(si.SourceType, 'UNKNOWN') AS SourceType", "COALESCE(si.SourceType, 'UNKNOWN')", "Source"),
        };
        var sel = $@"SELECT {dim},
            COUNT(*) AS Invoices,
            SUM(si.TotalGrossAmount) AS Gross, SUM(si.TotalDiscountAmount) AS Discount,
            SUM(si.TotalTaxableAmount) AS Taxable,
            SUM(si.GrandTotal) AS GrandTotal, SUM(si.PaidAmount) AS Paid, SUM(si.BalanceAmount) AS Balance
            FROM dbo.SalesInvoice si
            {where} GROUP BY {gb}";
        var (rows1, total1) = await PageAsync(conn, sel, ob, dp, f.Page, f.Size);
        result.Rows = rows1;
        result.TotalCount = total1;
        return result;
    }

    // ============================================================
    // 13. Price List Analysis — grouped by SalesInvoice.PriceListId
    // ============================================================
    private async Task<SalesReportResult> PriceListAsync(IDbConnection conn, SalesReportFilter f, DynamicParameters dp)
    {
        var result = new SalesReportResult();
        var where = BuildWhere(f);

        var (dim, gb, ob) = f.GroupBy switch
        {
            "month" => ("FORMAT(si.InvoiceDate, 'yyyy-MM') AS Period", "FORMAT(si.InvoiceDate, 'yyyy-MM')", "Period"),
            "product" => ("si.PriceListId, COALESCE(pl.Name, CONCAT('Price List ', CAST(si.PriceListId AS nvarchar(20)))) AS PriceList, sii.ProductId, sii.ProductNameSnapshot AS Product",
                "si.PriceListId, COALESCE(pl.Name, CONCAT('Price List ', CAST(si.PriceListId AS nvarchar(20)))), sii.ProductId, sii.ProductNameSnapshot", "Product"),
            _ => ("si.PriceListId, COALESCE(pl.Name, CONCAT('Price List ', CAST(si.PriceListId AS nvarchar(20)))) AS PriceList",
                "si.PriceListId, COALESCE(pl.Name, CONCAT('Price List ', CAST(si.PriceListId AS nvarchar(20))))", "PriceList"),
        };

        string from, extraCols;
        if (f.GroupBy == "product")
        {
            extraCols = "COUNT(*) AS Lines, SUM(sii.Quantity) AS Quantity, AVG(sii.Rate) AS AvgRate, SUM(sii.LineTotal) AS LineTotal";
            from = @"FROM dbo.SalesInvoiceItem sii
                INNER JOIN dbo.SalesInvoice si ON si.SalesInvoiceId = sii.SalesInvoiceId
                LEFT JOIN dbo.PriceLists pl ON pl.PriceListId = si.PriceListId
                ";
        }
        else
        {
            extraCols = @"COUNT(DISTINCT si.SalesInvoiceId) AS Invoices,
                SUM(si.TotalGrossAmount) AS Gross, SUM(si.TotalDiscountAmount) AS Discount,
                SUM(si.TotalTaxableAmount) AS Taxable,
                SUM(si.GrandTotal) AS GrandTotal, SUM(si.PaidAmount) AS Paid, SUM(si.BalanceAmount) AS Balance";
            from = @"FROM dbo.SalesInvoice si
                LEFT JOIN dbo.PriceLists pl ON pl.PriceListId = si.PriceListId
                ";
        }

        var select = $@"SELECT {dim}, {extraCols} {from}{where} GROUP BY {gb}";
        var (rows, total) = await PageAsync(conn, select, ob, dp, f.Page, f.Size);
        result.Rows = rows;
        result.TotalCount = total;
        return result;
    }

    // ============================================================
    // 14. Payment Allocation — one row per allocation of a SALES payment,
    //     with the invoice position for reconciliation
    // ============================================================
    private async Task<SalesReportResult> AllocationAsync(IDbConnection conn, SalesReportFilter f, DynamicParameters dp)
    {
        var result = new SalesReportResult();
        var c = new List<string> { "pay.CompanyId = @companyId", "pa.ReferenceType = 'SALES'", "pay.ReferenceType = 'SALES'" };
        if (f.DateFrom.HasValue) c.Add("pay.PaymentDate >= @dateFrom");
        if (f.DateTo.HasValue) c.Add("pay.PaymentDate < DATEADD(DAY, 1, @dateTo)");
        if (f.PaymentMethodID.HasValue) c.Add("pay.PaymentMethodID = @paymentMethodID");
        if (f.CustomerId.HasValue) c.Add("si.CustomerId = @customerId");
        if (!string.IsNullOrWhiteSpace(f.Search)) c.Add("(pay.PaymentNo LIKE @search OR si.SalesInvoiceNo LIKE @search)");
        var where = "WHERE " + string.Join(" AND ", c);

        var select = $@"SELECT pa.PaymentAllocationId, pay.PaymentId, pay.PaymentNo, pay.PaymentDate,
            si.SalesInvoiceId, si.SalesInvoiceNo, si.CustomerNameSnapshot AS Customer,
            pay.Amount AS PaymentAmount, pa.AllocatedAmount,
            pay.Amount - alloc.TotalAllocated AS Unallocated,
            si.GrandTotal, si.PaidAmount AS Paid, si.BalanceAmount AS Balance
            FROM dbo.PaymentAllocation pa
            INNER JOIN dbo.Payment pay ON pay.PaymentId = pa.PaymentId
            INNER JOIN dbo.SalesInvoice si ON si.SalesInvoiceId = pa.ReferenceId
            LEFT JOIN (SELECT PaymentId, SUM(AllocatedAmount) AS TotalAllocated
                       FROM dbo.PaymentAllocation GROUP BY PaymentId) alloc
                   ON alloc.PaymentId = pay.PaymentId
            {where}";
        var order = OrderBy(f, "pay.PaymentDate DESC, pa.PaymentAllocationId DESC", new Dictionary<string, string>
        {
            ["paymentNo"] = "pay.PaymentNo",
            ["paymentDate"] = "pay.PaymentDate",
            ["invoiceNo"] = "si.SalesInvoiceNo",
            ["allocated"] = "pa.AllocatedAmount",
        });
        var (rows, total) = await PageAsync(conn, select, order, dp, f.Page, f.Size);
        result.Rows = rows;
        result.TotalCount = total;
        return result;
    }
}
