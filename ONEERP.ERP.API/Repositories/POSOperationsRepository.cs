using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IPOSOperationsRepository
{
    Task<long> InsertCashMovementAsync(POSCashMovement m);
    Task<List<POSCashMovement>> GetCashMovementsAsync(long sessionId);
    Task<decimal> GetCashTotalAsync(long sessionId, string direction);
    Task<string> GetNextHoldNumberAsync(long companyId);
    Task<long> InsertHoldAsync(POSHoldBill h);
    Task<(List<POSHoldBill> Items, int Total)> GetHoldsAsync(long companyId, long branchId, string status, int page, int size);
    Task<POSHoldBill?> GetHoldAsync(long id);
    Task<bool> RecallHoldAsync(long id, long userId);
    Task<bool> CancelHoldAsync(long id, long userId);
    Task<DashboardAgg?> GetDashboardAggAsync(long companyId, long? branchId, long? sessionId);
    Task<ShiftAgg?> GetShiftAggAsync(long companyId, long sessionId);
}

public class DashboardAgg
{
    public decimal PosSalesToday { get; set; }
    public int InvoiceCountToday { get; set; }
}

public class ShiftAgg
{
    public int InvoiceCount { get; set; }
    public decimal Gross { get; set; }
    public decimal Discount { get; set; }
    public decimal Taxable { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal Paid { get; set; }
    public decimal Balance { get; set; }
    public int ReturnCount { get; set; }
    public decimal ReturnTotal { get; set; }
}

public class POSOperationsRepository : TenantRepositoryBase, IPOSOperationsRepository
{
    public POSOperationsRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<long> InsertCashMovementAsync(POSCashMovement m)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<long>(conn,
            @"INSERT INTO dbo.POSCashMovement
              (POSSessionId, CompanyId, BranchId, Direction, Amount, Reason, ReferenceNo, CreatedByUserID)
              VALUES (@POSSessionId, @CompanyId, @BranchId, @Direction, @Amount, @Reason, @ReferenceNo, @CreatedByUserID);
              SELECT CAST(SCOPE_IDENTITY() AS bigint);", m);
    }

    public async Task<List<POSCashMovement>> GetCashMovementsAsync(long sessionId)
    {
        using var conn = OpenTenant();
        var rows = await Sql.QueryAsync<POSCashMovement>(conn,
            "SELECT * FROM dbo.POSCashMovement WHERE POSSessionId = @sessionId ORDER BY POSCashMovementId",
            new { sessionId });
        return rows.ToList();
    }

    public async Task<decimal> GetCashTotalAsync(long sessionId, string direction)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<decimal>(conn,
            "SELECT COALESCE(SUM(Amount), 0) FROM dbo.POSCashMovement WHERE POSSessionId = @sessionId AND Direction = @direction",
            new { sessionId, direction });
    }

    public async Task<string> GetNextHoldNumberAsync(long companyId)
    {
        using var conn = OpenTenant();
        var count = await Sql.QuerySingleOrDefaultAsync<int>(conn,
            "SELECT COUNT(*) FROM dbo.POSHoldBill WHERE CompanyId = @companyId", new { companyId });
        return $"HLD-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D4}";
    }

    public async Task<long> InsertHoldAsync(POSHoldBill h)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<long>(conn,
            @"INSERT INTO dbo.POSHoldBill
              (CompanyId, BranchId, StoreId, CounterId, HoldNumber, CustomerId, CustomerName,
               ItemCount, TotalAmount, CartJson, Status, CreatedByUserID)
              VALUES (@CompanyId, @BranchId, @StoreId, @CounterId, @HoldNumber, @CustomerId, @CustomerName,
               @ItemCount, @TotalAmount, @CartJson, 'HELD', @CreatedByUserID);
              SELECT CAST(SCOPE_IDENTITY() AS bigint);", h);
    }

    public async Task<(List<POSHoldBill> Items, int Total)> GetHoldsAsync(long companyId, long branchId, string status, int page, int size)
    {
        using var conn = OpenTenant();
        var offset = (page - 1) * size;
        var where = "WHERE CompanyId = @companyId AND Status = @status";
        if (branchId > 0) where += " AND BranchId = @branchId";
        var total = await Sql.QuerySingleOrDefaultAsync<int>(conn,
            $"SELECT COUNT(*) FROM dbo.POSHoldBill {where}",
            new { companyId, branchId, status });
        var rows = await Sql.QueryAsync<POSHoldBill>(conn,
            $@"SELECT * FROM ({{
                SELECT *, ROW_NUMBER() OVER (ORDER BY POSHoldBillId DESC) AS _rn
                FROM dbo.POSHoldBill {where}
              }} t) WHERE t._rn > @offset AND t._rn <= @offset + @size",
            new { companyId, branchId, status, offset, size });
        return (rows.ToList(), total);
    }

    public async Task<POSHoldBill?> GetHoldAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<POSHoldBill>(conn,
            "SELECT * FROM dbo.POSHoldBill WHERE POSHoldBillId = @id", new { id });
    }

    public async Task<bool> RecallHoldAsync(long id, long userId)
    {
        using var conn = OpenTenant();
        var rows = await Sql.ExecuteAsync(conn,
            @"UPDATE dbo.POSHoldBill SET Status = 'RECALLED', RecalledByUserID = @userId, RecalledAt = SYSUTCDATETIME()
              WHERE POSHoldBillId = @id AND Status = 'HELD'",
            new { id, userId });
        return rows > 0;
    }

    public async Task<bool> CancelHoldAsync(long id, long userId)
    {
        using var conn = OpenTenant();
        var rows = await Sql.ExecuteAsync(conn,
            @"UPDATE dbo.POSHoldBill SET Status = 'CANCELLED', CancelledByUserID = @userId, CancelledAt = SYSUTCDATETIME()
              WHERE POSHoldBillId = @id AND Status = 'HELD'",
            new { id, userId });
        return rows > 0;
    }

    public async Task<DashboardAgg?> GetDashboardAggAsync(long companyId, long? branchId, long? sessionId)
    {
        using var conn = OpenTenant();
        if (!sessionId.HasValue) return null;
        var row = await Sql.QueryFirstOrDefaultAsync<dynamic>(conn,
            @"SELECT COUNT(*) AS InvoiceCount,
                     COALESCE(SUM(si.GrandTotal), 0) AS PosSales
              FROM dbo.SalesInvoice si
              WHERE si.CompanyId = @companyId AND si.SourceType = 'POS' AND si.POSSessionId = @sessionId
                AND si.InvoiceStatus <> 'CANCELLED'",
            new { companyId, sessionId });
        var d = (IDictionary<string, object>)row!;
        return new DashboardAgg
        {
            InvoiceCountToday = Convert.ToInt32(d["InvoiceCount"]),
            PosSalesToday = Convert.ToDecimal(d["PosSales"]),
        };
    }

    public async Task<ShiftAgg?> GetShiftAggAsync(long companyId, long sessionId)
    {
        using var conn = OpenTenant();
        var sales = await Sql.QueryFirstOrDefaultAsync<dynamic>(conn,
            @"SELECT COUNT(*) AS InvoiceCount,
                     COALESCE(SUM(si.TotalGrossAmount), 0) AS Gross,
                     COALESCE(SUM(si.TotalDiscountAmount), 0) AS Discount,
                     COALESCE(SUM(si.TotalTaxableAmount), 0) AS Taxable,
                     COALESCE(SUM(si.TotalCGSTAmount + si.TotalSGSTAmount + si.TotalIGSTAmount + si.TotalCESSAmount), 0) AS Tax,
                     COALESCE(SUM(si.GrandTotal), 0) AS GrandTotal,
                     COALESCE(SUM(si.PaidAmount), 0) AS Paid,
                     COALESCE(SUM(si.BalanceAmount), 0) AS Balance
              FROM dbo.SalesInvoice si
              WHERE si.CompanyId = @companyId AND si.SourceType = 'POS' AND si.POSSessionId = @sessionId
                AND si.InvoiceStatus <> 'CANCELLED'",
            new { companyId, sessionId });
        var s = (IDictionary<string, object>)sales!;
        var returns = await Sql.QueryFirstOrDefaultAsync<dynamic>(conn,
            @"SELECT COUNT(*) AS ReturnCount, COALESCE(SUM(r.GrandTotal), 0) AS ReturnTotal
              FROM dbo.SalesReturn r
              INNER JOIN dbo.SalesInvoice si ON si.SalesInvoiceId = r.SalesInvoiceId
              WHERE r.CompanyId = @companyId AND si.SourceType = 'POS' AND si.POSSessionId = @sessionId
                AND r.StatusID <> (SELECT StatusId FROM dbo.[Status] WHERE Code = 'CANCELLED')",
            new { companyId, sessionId });
        var r = (IDictionary<string, object>)returns!;
        return new ShiftAgg
        {
            InvoiceCount = Convert.ToInt32(s["InvoiceCount"]),
            Gross = Convert.ToDecimal(s["Gross"]),
            Discount = Convert.ToDecimal(s["Discount"]),
            Taxable = Convert.ToDecimal(s["Taxable"]),
            Tax = Convert.ToDecimal(s["Tax"]),
            GrandTotal = Convert.ToDecimal(s["GrandTotal"]),
            Paid = Convert.ToDecimal(s["Paid"]),
            Balance = Convert.ToDecimal(s["Balance"]),
            ReturnCount = Convert.ToInt32(r["ReturnCount"]),
            ReturnTotal = Convert.ToDecimal(r["ReturnTotal"]),
        };
    }
}
