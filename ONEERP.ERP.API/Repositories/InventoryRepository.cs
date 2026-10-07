using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Repositories;

public interface IInventoryRepository
{
    Task<InventoryDashboardDto> GetDashboardAsync(long companyId);
    Task PostOpeningStockAsync(long companyId, OpeningStockRequest request, long userId);

    Task<(List<StockAdjustment> Items, int Total)> GetAdjustmentsAsync(long companyId, int page, int size);
    Task<StockAdjustment?> GetAdjustmentAsync(long id);
    Task<string> GetNextNumberAsync(long companyId, string prefix, string table, string column);
    Task<long> InsertAdjustmentAsync(StockAdjustment e);
    Task<bool> PostAdjustmentAsync(long id, long userId);

    Task<(List<StockTransfer> Items, int Total)> GetTransfersAsync(long companyId, int page, int size);
    Task<StockTransfer?> GetTransferAsync(long id);
    Task<long> InsertTransferAsync(StockTransfer e);
    Task<bool> PostTransferAsync(long id, long userId);

    Task<(List<StockCount> Items, int Total)> GetCountsAsync(long companyId, int page, int size);
    Task<StockCount?> GetCountAsync(long id);
    Task<long> InsertCountAsync(StockCount e);
    Task<bool> PostCountAsync(long id, long userId);

    Task<(List<StockReconciliationRow> Rows, int Total)> GetReconciliationAsync(long companyId, long? warehouseId, long? productId, int page, int size);
    Task<(List<StockValuationRow> Rows, int Total, decimal TotalValue)> GetValuationAsync(long companyId, long? warehouseId, int page, int size);
    Task<(List<LowStockRow> Rows, int Total)> GetLowStockAsync(long companyId, long? warehouseId, int page, int size);
}

public class InventoryRepository : TenantRepositoryBase, IInventoryRepository
{
    public InventoryRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    // ============================================================
    // Shared ledger writer — the ONLY place Stage 5 mutates stock.
    // Upserts dbo.Stock and appends dbo.StockTransaction atomically.
    // ============================================================
    private async Task UpsertStockAndLedgerAsync(System.Data.IDbConnection conn, System.Data.IDbTransaction tx,
        long companyId, long branchId, long warehouseId, long productId, long unitId,
        decimal delta, decimal rate, string referenceType, long referenceId, DateTime date, string? remarks, long userId)
    {
        if (delta == 0) return;

        if (delta < 0)
        {
            var available = await Sql.QuerySingleOrDefaultAsync<decimal?>(conn,
                @"SELECT AvailableQuantity FROM dbo.Stock
                  WHERE CompanyId=@c AND BranchId=@b AND WarehouseId=@w AND ProductId=@p AND UnitId=@u",
                new { c = companyId, b = branchId, w = warehouseId, p = productId, u = unitId }, tx);
            if ((available ?? 0) + delta < -0.000001m)
                throw new DomainException($"Insufficient stock for product {productId} (available: {available ?? 0}, required: {-delta}).");
        }

        var updated = await Sql.ExecuteAsync(conn,
            @"UPDATE dbo.Stock SET
                Quantity = Quantity + @delta,
                AvailableQuantity = AvailableQuantity + @delta,
                UpdatedAt = SYSUTCDATETIME()
              WHERE CompanyId=@c AND BranchId=@b AND WarehouseId=@w AND ProductId=@p AND UnitId=@u",
            new { delta, c = companyId, b = branchId, w = warehouseId, p = productId, u = unitId }, tx);

        if (updated == 0)
        {
            // First movement of this key — create the row.
            if (delta < 0) throw new DomainException($"Insufficient stock for product {productId} (available: 0, required: {-delta}).");
            await Sql.ExecuteAsync(conn,
                @"INSERT INTO dbo.Stock (CompanyId, BranchId, WarehouseId, ProductId, UnitId, Quantity, ReservedQuantity, AvailableQuantity)
                  VALUES (@c, @b, @w, @p, @u, @delta, 0, @delta)",
                new { delta, c = companyId, b = branchId, w = warehouseId, p = productId, u = unitId }, tx);
        }

        await Sql.ExecuteAsync(conn,
            @"INSERT INTO dbo.StockTransaction
              (CompanyId, BranchId, WarehouseId, ProductId, UnitId, TransactionType, ReferenceType, ReferenceId,
               QuantityIn, QuantityOut, Rate, BalanceQuantity, TransactionDate, Remarks, CreatedByUserID)
              VALUES (@c, @b, @w, @p, @u, @tt, @rt, @rid, @qtyIn, @qtyOut, @rate,
               (SELECT ISNULL(Quantity,0) FROM dbo.Stock WHERE CompanyId=@c AND BranchId=@b AND WarehouseId=@w AND ProductId=@p AND UnitId=@u),
               @date, @remarks, @userId)",
            new
            {
                c = companyId, b = branchId, w = warehouseId, p = productId, u = unitId,
                tt = delta > 0 ? "IN" : "OUT",
                rt = referenceType, rid = referenceId,
                qtyIn = delta > 0 ? delta : 0m,
                qtyOut = delta < 0 ? -delta : 0m,
                rate, date, remarks, userId
            }, tx);
    }

    // ============================================================
    // T074 — dashboard KPIs (all server-side SQL)
    // ============================================================
    public async Task<InventoryDashboardDto> GetDashboardAsync(long companyId)
    {
        using var conn = OpenTenant();
        var row = await Sql.QueryFirstOrDefaultAsync<dynamic>(conn,
            @"SELECT
                (SELECT COUNT(*) FROM dbo.Stock WHERE CompanyId = @companyId) AS ProductsWithStock,
                (SELECT COUNT(*) FROM dbo.Stock WHERE CompanyId = @companyId AND AvailableQuantity <= 0) AS OutOfStockCount,
                (SELECT ISNULL(SUM(QuantityIn), 0) FROM dbo.StockTransaction
                  WHERE CompanyId = @companyId AND CAST(TransactionDate AS date) = CAST(SYSUTCDATETIME() AS date)) AS QtyInToday,
                (SELECT ISNULL(SUM(QuantityOut), 0) FROM dbo.StockTransaction
                  WHERE CompanyId = @companyId AND CAST(TransactionDate AS date) = CAST(SYSUTCDATETIME() AS date)) AS QtyOutToday,
                (SELECT COUNT(*) FROM dbo.StockTransaction
                  WHERE CompanyId = @companyId AND CAST(TransactionDate AS date) = CAST(SYSUTCDATETIME() AS date)) AS TransactionsToday",
            new { companyId });
        var d = (IDictionary<string, object>)row!;

        // ReorderLevel is introduced by T085. T074 must remain usable before
        // that later migration is applied, so only compile the column query
        // after checking the tenant schema.
        var hasReorderLevel = await Sql.QuerySingleOrDefaultAsync<bool>(conn,
            "SELECT CASE WHEN COL_LENGTH(N'dbo.Products', N'ReorderLevel') IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END");
        var lowStockCount = hasReorderLevel
            ? await Sql.QuerySingleOrDefaultAsync<int>(conn,
                @"SELECT COUNT(*) FROM dbo.Products p
                  WHERE p.CompanyId = @companyId AND p.IsActive = 1 AND p.ReorderLevel IS NOT NULL
                    AND ISNULL((SELECT SUM(s.AvailableQuantity) FROM dbo.Stock s
                                WHERE s.CompanyId = p.CompanyId AND s.ProductId = p.Id), 0) <= p.ReorderLevel",
                new { companyId })
            : 0;

        // T084 valuation cost basis reused for the dashboard value.
        var valuation = await GetValuationAsync(companyId, null, 1, 1);
        return new InventoryDashboardDto
        {
            StockValue = valuation.TotalValue,
            ProductsWithStock = Convert.ToInt32(d["ProductsWithStock"]),
            OutOfStockCount = Convert.ToInt32(d["OutOfStockCount"]),
            LowStockCount = lowStockCount,
            QtyInToday = Convert.ToDecimal(d["QtyInToday"]),
            QtyOutToday = Convert.ToDecimal(d["QtyOutToday"]),
            TransactionsToday = Convert.ToInt32(d["TransactionsToday"]),
        };
    }

    // ============================================================
    // T076 — opening stock (ReferenceType='OPENING', once per key)
    // ============================================================
    public async Task PostOpeningStockAsync(long companyId, OpeningStockRequest request, long userId)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new DomainException("Add at least one opening stock line.");
        var date = request.OpeningDate ?? DateTime.UtcNow;

        using var conn = OpenTenant();
        using var tx = conn.BeginTransaction();
        try
        {
            var referenceId = 0L;
            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                    throw new DomainException("Opening quantity must be greater than 0.");

                var already = await Sql.QuerySingleOrDefaultAsync<int>(conn,
                    @"SELECT COUNT(*) FROM dbo.StockTransaction
                      WHERE CompanyId=@c AND WarehouseId=@w AND ProductId=@p AND UnitId=@u AND ReferenceType='OPENING'",
                    new { c = companyId, w = item.WarehouseId, p = item.ProductId, u = item.UnitId }, tx);
                if (already > 0)
                    throw new DomainException($"Opening stock for product {item.ProductId} / warehouse {item.WarehouseId} was already recorded.");

                await UpsertStockAndLedgerAsync(conn, tx, companyId, request.BranchId, item.WarehouseId,
                    item.ProductId, item.UnitId, item.Quantity, item.Rate, "OPENING", referenceId, date,
                    request.Remarks ?? "Opening stock", userId);
                referenceId++;
            }
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    // ============================================================
    // T079 — Stock Adjustment
    // ============================================================
    public async Task<string> GetNextNumberAsync(long companyId, string prefix, string table, string column)
    {
        using var conn = OpenTenant();
        var count = await Sql.QuerySingleOrDefaultAsync<int>(conn,
            $"SELECT COUNT(*) FROM {table} WHERE CompanyId = @companyId", new { companyId });
        return $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D4}";
    }

    public async Task<(List<StockAdjustment> Items, int Total)> GetAdjustmentsAsync(long companyId, int page, int size)
    {
        using var conn = OpenTenant();
        var offset = (page - 1) * size;
        var total = await Sql.QuerySingleOrDefaultAsync<int>(conn,
            "SELECT COUNT(*) FROM dbo.StockAdjustment WHERE CompanyId = @companyId", new { companyId });
        var rows = await Sql.QueryAsync<StockAdjustment>(conn,
            @"SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY StockAdjustmentId DESC) AS _rn
               FROM dbo.StockAdjustment WHERE CompanyId = @companyId) t
              WHERE t._rn > @offset AND t._rn <= @offset + @size",
            new { companyId, offset, size });
        return (rows.ToList(), total);
    }

    public async Task<StockAdjustment?> GetAdjustmentAsync(long id)
    {
        using var conn = OpenTenant();
        var header = await Sql.QuerySingleOrDefaultAsync<StockAdjustment>(conn,
            "SELECT * FROM dbo.StockAdjustment WHERE StockAdjustmentId = @id", new { id });
        if (header == null) return null;
        header.Items = (await Sql.QueryAsync<StockAdjustmentItem>(conn,
            "SELECT * FROM dbo.StockAdjustmentItem WHERE StockAdjustmentId = @id", new { id })).ToList();
        return header;
    }

    public async Task<long> InsertAdjustmentAsync(StockAdjustment e)
    {
        using var conn = OpenTenant();
        using var tx = conn.BeginTransaction();
        try
        {
            var id = await Sql.QuerySingleOrDefaultAsync<long>(conn,
                @"INSERT INTO dbo.StockAdjustment
                  (AdjustmentNumber, CompanyId, BranchId, WarehouseId, AdjustmentDate, Reason, Remarks, Status, CreatedByUserID)
                  VALUES (@AdjustmentNumber, @CompanyId, @BranchId, @WarehouseId, @AdjustmentDate, @Reason, @Remarks, 'DRAFT', @CreatedByUserID);
                  SELECT CAST(SCOPE_IDENTITY() AS bigint);", e, tx);
            foreach (var item in e.Items)
            {
                item.StockAdjustmentId = id;
                await Sql.ExecuteAsync(conn,
                    @"INSERT INTO dbo.StockAdjustmentItem (StockAdjustmentId, ProductId, UnitId, QuantityDelta, Rate, Reason)
                      VALUES (@StockAdjustmentId, @ProductId, @UnitId, @QuantityDelta, @Rate, @Reason)", item, tx);
            }
            tx.Commit();
            return id;
        }
        catch { tx.Rollback(); throw; }
    }

    public async Task<bool> PostAdjustmentAsync(long id, long userId)
    {
        using var conn = OpenTenant();
        using var tx = conn.BeginTransaction();
        try
        {
            var header = await Sql.QuerySingleOrDefaultAsync<StockAdjustment>(conn,
                "SELECT * FROM dbo.StockAdjustment WHERE StockAdjustmentId = @id", new { id }, tx);
            if (header == null) return false;
            if (header.Status != "DRAFT") throw new DomainException($"Adjustment is {header.Status} and cannot be posted.");

            var items = (await Sql.QueryAsync<StockAdjustmentItem>(conn,
                "SELECT * FROM dbo.StockAdjustmentItem WHERE StockAdjustmentId = @id", new { id }, tx)).ToList();

            foreach (var item in items)
            {
                await UpsertStockAndLedgerAsync(conn, tx, header.CompanyId, header.BranchId, header.WarehouseId,
                    item.ProductId, item.UnitId, item.QuantityDelta, item.Rate, "ADJUSTMENT", id,
                    header.AdjustmentDate, $"Adjustment {header.AdjustmentNumber}", userId);
            }

            await Sql.ExecuteAsync(conn,
                @"UPDATE dbo.StockAdjustment SET Status='POSTED', PostedByUserID=@userId, PostedAt=SYSUTCDATETIME()
                  WHERE StockAdjustmentId=@id AND Status='DRAFT'",
                new { id, userId }, tx);
            tx.Commit();
            return true;
        }
        catch { tx.Rollback(); throw; }
    }

    // ============================================================
    // T080/T081 — Stock Transfer
    // ============================================================
    public async Task<(List<StockTransfer> Items, int Total)> GetTransfersAsync(long companyId, int page, int size)
    {
        using var conn = OpenTenant();
        var offset = (page - 1) * size;
        var total = await Sql.QuerySingleOrDefaultAsync<int>(conn,
            "SELECT COUNT(*) FROM dbo.StockTransfer WHERE CompanyId = @companyId", new { companyId });
        var rows = await Sql.QueryAsync<StockTransfer>(conn,
            @"SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY StockTransferId DESC) AS _rn
               FROM dbo.StockTransfer WHERE CompanyId = @companyId) t
              WHERE t._rn > @offset AND t._rn <= @offset + @size",
            new { companyId, offset, size });
        return (rows.ToList(), total);
    }

    public async Task<StockTransfer?> GetTransferAsync(long id)
    {
        using var conn = OpenTenant();
        var header = await Sql.QuerySingleOrDefaultAsync<StockTransfer>(conn,
            "SELECT * FROM dbo.StockTransfer WHERE StockTransferId = @id", new { id });
        if (header == null) return null;
        header.Items = (await Sql.QueryAsync<StockTransferItem>(conn,
            "SELECT * FROM dbo.StockTransferItem WHERE StockTransferId = @id", new { id })).ToList();
        return header;
    }

    public async Task<long> InsertTransferAsync(StockTransfer e)
    {
        using var conn = OpenTenant();
        using var tx = conn.BeginTransaction();
        try
        {
            var id = await Sql.QuerySingleOrDefaultAsync<long>(conn,
                @"INSERT INTO dbo.StockTransfer
                  (TransferNumber, CompanyId, BranchId, FromWarehouseId, ToWarehouseId, TransferDate, Remarks, Status, CreatedByUserID)
                  VALUES (@TransferNumber, @CompanyId, @BranchId, @FromWarehouseId, @ToWarehouseId, @TransferDate, @Remarks, 'DRAFT', @CreatedByUserID);
                  SELECT CAST(SCOPE_IDENTITY() AS bigint);", e, tx);
            foreach (var item in e.Items)
            {
                item.StockTransferId = id;
                await Sql.ExecuteAsync(conn,
                    @"INSERT INTO dbo.StockTransferItem (StockTransferId, ProductId, UnitId, Quantity, Rate, Remarks)
                      VALUES (@StockTransferId, @ProductId, @UnitId, @Quantity, @Rate, @Remarks)", item, tx);
            }
            tx.Commit();
            return id;
        }
        catch { tx.Rollback(); throw; }
    }

    public async Task<bool> PostTransferAsync(long id, long userId)
    {
        using var conn = OpenTenant();
        using var tx = conn.BeginTransaction();
        try
        {
            var header = await Sql.QuerySingleOrDefaultAsync<StockTransfer>(conn,
                "SELECT * FROM dbo.StockTransfer WHERE StockTransferId = @id", new { id }, tx);
            if (header == null) return false;
            if (header.Status != "DRAFT") throw new DomainException($"Transfer is {header.Status} and cannot be posted.");

            var items = (await Sql.QueryAsync<StockTransferItem>(conn,
                "SELECT * FROM dbo.StockTransferItem WHERE StockTransferId = @id", new { id }, tx)).ToList();

            foreach (var item in items)
            {
                // OUT from source (validates available qty first).
                await UpsertStockAndLedgerAsync(conn, tx, header.CompanyId, header.BranchId, header.FromWarehouseId,
                    item.ProductId, item.UnitId, -item.Quantity, item.Rate, "STOCK_TRANSFER", id,
                    header.TransferDate, $"Transfer {header.TransferNumber} OUT", userId);
                // IN to destination.
                await UpsertStockAndLedgerAsync(conn, tx, header.CompanyId, header.BranchId, header.ToWarehouseId,
                    item.ProductId, item.UnitId, item.Quantity, item.Rate, "STOCK_TRANSFER", id,
                    header.TransferDate, $"Transfer {header.TransferNumber} IN", userId);
            }

            await Sql.ExecuteAsync(conn,
                @"UPDATE dbo.StockTransfer SET Status='POSTED', PostedByUserID=@userId, PostedAt=SYSUTCDATETIME()
                  WHERE StockTransferId=@id AND Status='DRAFT'",
                new { id, userId }, tx);
            tx.Commit();
            return true;
        }
        catch { tx.Rollback(); throw; }
    }

    // ============================================================
    // T082 — Stock Count
    // ============================================================
    public async Task<(List<StockCount> Items, int Total)> GetCountsAsync(long companyId, int page, int size)
    {
        using var conn = OpenTenant();
        var offset = (page - 1) * size;
        var total = await Sql.QuerySingleOrDefaultAsync<int>(conn,
            "SELECT COUNT(*) FROM dbo.StockCount WHERE CompanyId = @companyId", new { companyId });
        var rows = await Sql.QueryAsync<StockCount>(conn,
            @"SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY StockCountId DESC) AS _rn
               FROM dbo.StockCount WHERE CompanyId = @companyId) t
              WHERE t._rn > @offset AND t._rn <= @offset + @size",
            new { companyId, offset, size });
        return (rows.ToList(), total);
    }

    public async Task<StockCount?> GetCountAsync(long id)
    {
        using var conn = OpenTenant();
        var header = await Sql.QuerySingleOrDefaultAsync<StockCount>(conn,
            "SELECT * FROM dbo.StockCount WHERE StockCountId = @id", new { id });
        if (header == null) return null;
        header.Items = (await Sql.QueryAsync<StockCountItem>(conn,
            "SELECT * FROM dbo.StockCountItem WHERE StockCountId = @id", new { id })).ToList();
        return header;
    }

    public async Task<long> InsertCountAsync(StockCount e)
    {
        using var conn = OpenTenant();
        using var tx = conn.BeginTransaction();
        try
        {
            var id = await Sql.QuerySingleOrDefaultAsync<long>(conn,
                @"INSERT INTO dbo.StockCount
                  (CountNumber, CompanyId, BranchId, WarehouseId, CountDate, Remarks, Status, CreatedByUserID)
                  VALUES (@CountNumber, @CompanyId, @BranchId, @WarehouseId, @CountDate, @Remarks, 'DRAFT', @CreatedByUserID);
                  SELECT CAST(SCOPE_IDENTITY() AS bigint);", e, tx);
            foreach (var item in e.Items)
            {
                item.StockCountId = id;
                item.Variance = item.CountedQuantity - item.BookQuantity;
                await Sql.ExecuteAsync(conn,
                    @"INSERT INTO dbo.StockCountItem (StockCountId, ProductId, UnitId, BookQuantity, CountedQuantity, Variance, Rate)
                      VALUES (@StockCountId, @ProductId, @UnitId, @BookQuantity, @CountedQuantity, @Variance, @Rate)", item, tx);
            }
            tx.Commit();
            return id;
        }
        catch { tx.Rollback(); throw; }
    }

    public async Task<bool> PostCountAsync(long id, long userId)
    {
        using var conn = OpenTenant();
        using var tx = conn.BeginTransaction();
        try
        {
            var header = await Sql.QuerySingleOrDefaultAsync<StockCount>(conn,
                "SELECT * FROM dbo.StockCount WHERE StockCountId = @id", new { id }, tx);
            if (header == null) return false;
            if (header.Status != "DRAFT") throw new DomainException($"Stock count is {header.Status} and cannot be posted.");

            var items = (await Sql.QueryAsync<StockCountItem>(conn,
                "SELECT * FROM dbo.StockCountItem WHERE StockCountId = @id", new { id }, tx)).ToList();

            foreach (var item in items)
            {
                if (item.Variance == 0) continue;
                // Book quantity recorded at entry time may be stale — recompute variance
                // against the live stock so posting correct reality, not a snapshot.
                var live = await Sql.QuerySingleOrDefaultAsync<decimal?>(conn,
                    @"SELECT Quantity FROM dbo.Stock
                      WHERE CompanyId=@c AND BranchId=@b AND WarehouseId=@w AND ProductId=@p AND UnitId=@u",
                    new { c = header.CompanyId, b = header.BranchId, w = header.WarehouseId, p = item.ProductId, u = item.UnitId }, tx);
                var delta = item.CountedQuantity - (live ?? 0);
                if (delta == 0) continue;

                await UpsertStockAndLedgerAsync(conn, tx, header.CompanyId, header.BranchId, header.WarehouseId,
                    item.ProductId, item.UnitId, delta, item.Rate, "STOCK_COUNT", id,
                    header.CountDate, $"Stock count {header.CountNumber}", userId);
            }

            await Sql.ExecuteAsync(conn,
                @"UPDATE dbo.StockCount SET Status='POSTED', PostedByUserID=@userId, PostedAt=SYSUTCDATETIME()
                  WHERE StockCountId=@id AND Status='DRAFT'",
                new { id, userId }, tx);
            tx.Commit();
            return true;
        }
        catch { tx.Rollback(); throw; }
    }

    // ============================================================
    // T083 — Reconciliation: Stock (book) vs StockTransaction (ledger)
    // ============================================================
    public async Task<(List<StockReconciliationRow> Rows, int Total)> GetReconciliationAsync(
        long companyId, long? warehouseId, long? productId, int page, int size)
    {
        using var conn = OpenTenant();
        var offset = (page - 1) * size;
        var where = "WHERE s.CompanyId = @companyId";
        if (warehouseId.HasValue) where += " AND s.WarehouseId = @warehouseId";
        if (productId.HasValue) where += " AND s.ProductId = @productId";

        var countSql = $@"SELECT COUNT(*) FROM dbo.Stock s {where}";
        var total = await Sql.QuerySingleOrDefaultAsync<int>(conn, countSql, new { companyId, warehouseId, productId });

        var selectSql = $@"SELECT s.ProductId, p.ProductCode, p.ProductName, s.WarehouseId,
                s.Quantity AS BookQuantity,
                ISNULL(l.LedgerQty, 0) AS LedgerQuantity,
                s.Quantity - ISNULL(l.LedgerQty, 0) AS Variance
            FROM dbo.Stock s
            LEFT JOIN dbo.Products p ON p.Id = s.ProductId
            LEFT JOIN (SELECT CompanyId, WarehouseId, ProductId, SUM(QuantityIn) - SUM(QuantityOut) AS LedgerQty
                       FROM dbo.StockTransaction GROUP BY CompanyId, WarehouseId, ProductId) l
                   ON l.CompanyId = s.CompanyId AND l.WarehouseId = s.WarehouseId AND l.ProductId = s.ProductId
            {where}";
        var rows = await Sql.QueryAsync<StockReconciliationRow>(conn,
            $@"SELECT * FROM ({selectSql}) t ORDER BY t.ProductId
               OFFSET @offset ROWS FETCH NEXT @size ROWS ONLY",
            new { companyId, warehouseId, productId, offset, size });
        return (rows.ToList(), total);
    }

    // ============================================================
    // T084 — Valuation: qty × cost basis (weighted-avg IN rate from the
    // ledger; falls back to Products.PurchasePrice for never-received stock)
    // ============================================================
    public async Task<(List<StockValuationRow> Rows, int Total, decimal TotalValue)> GetValuationAsync(
        long companyId, long? warehouseId, int page, int size)
    {
        using var conn = OpenTenant();
        var offset = (page - 1) * size;
        var where = "WHERE s.CompanyId = @companyId";
        if (warehouseId.HasValue) where += " AND s.WarehouseId = @warehouseId";

        var total = await Sql.QuerySingleOrDefaultAsync<int>(conn,
            $"SELECT COUNT(*) FROM dbo.Stock s {where}", new { companyId, warehouseId });

        var totalValue = await Sql.QuerySingleOrDefaultAsync<decimal>(conn,
            $@"SELECT ISNULL(SUM(s.Quantity * COALESCE(wa.AvgRate, p.PurchasePrice, 0)), 0)
               FROM dbo.Stock s
               LEFT JOIN dbo.Products p ON p.Id = s.ProductId
               OUTER APPLY (SELECT CASE WHEN SUM(t.QuantityIn) > 0 THEN SUM(t.QuantityIn * t.Rate) / SUM(t.QuantityIn) END AS AvgRate
                            FROM dbo.StockTransaction t
                            WHERE t.CompanyId = s.CompanyId AND t.ProductId = s.ProductId AND t.QuantityIn > 0) wa
               {where}",
            new { companyId, warehouseId });

        var rows = await Sql.QueryAsync<StockValuationRow>(conn,
            $@"SELECT * FROM (
                SELECT s.ProductId, p.ProductCode, p.ProductName, s.WarehouseId,
                       s.Quantity AS Quantity,
                       COALESCE(wa.AvgRate, p.PurchasePrice, 0) AS UnitCost,
                       s.Quantity * COALESCE(wa.AvgRate, p.PurchasePrice, 0) AS Value
                FROM dbo.Stock s
                LEFT JOIN dbo.Products p ON p.Id = s.ProductId
                OUTER APPLY (SELECT CASE WHEN SUM(t.QuantityIn) > 0 THEN SUM(t.QuantityIn * t.Rate) / SUM(t.QuantityIn) END AS AvgRate
                             FROM dbo.StockTransaction t
                             WHERE t.CompanyId = s.CompanyId AND t.ProductId = s.ProductId AND t.QuantityIn > 0) wa
                {where}
              ) t ORDER BY t.ProductId
              OFFSET @offset ROWS FETCH NEXT @size ROWS ONLY",
            new { companyId, warehouseId, offset, size });
        return (rows.ToList(), total, totalValue);
    }

    // ============================================================
    // T085 — Low stock: products whose available qty ≤ ReorderLevel
    // ============================================================
    public async Task<(List<LowStockRow> Rows, int Total)> GetLowStockAsync(long companyId, long? warehouseId, int page, int size)
    {
        using var conn = OpenTenant();
        var offset = (page - 1) * size;
        var scope = warehouseId.HasValue
            ? "AND s.WarehouseId = @warehouseId"
            : string.Empty;

        var total = await Sql.QuerySingleOrDefaultAsync<int>(conn,
            $@"SELECT COUNT(*) FROM dbo.Products p
               OUTER APPLY (SELECT SUM(s.AvailableQuantity) AS Qty FROM dbo.Stock s
                            WHERE s.CompanyId = p.CompanyId AND s.ProductId = p.Id {scope}) s
               WHERE p.CompanyId = @companyId AND p.IsActive = 1 AND p.ReorderLevel IS NOT NULL
                 AND ISNULL(s.Qty, 0) <= p.ReorderLevel",
            new { companyId, warehouseId });

        var rows = await Sql.QueryAsync<LowStockRow>(conn,
            $@"SELECT p.Id AS ProductId, p.ProductCode, p.ProductName, ISNULL(s.WarehouseId, 0) AS WarehouseId,
                      ISNULL(s.Qty, 0) AS AvailableQuantity, p.ReorderLevel
               FROM dbo.Products p
               OUTER APPLY (SELECT SUM(x.AvailableQuantity) AS Qty, MIN(x.WarehouseId) AS WarehouseId FROM dbo.Stock x
                            WHERE x.CompanyId = p.CompanyId AND x.ProductId = p.Id {scope}) s
               WHERE p.CompanyId = @companyId AND p.IsActive = 1 AND p.ReorderLevel IS NOT NULL
                 AND ISNULL(s.Qty, 0) <= p.ReorderLevel
               ORDER BY (ISNULL(s.Qty, 0) - p.ReorderLevel)
               OFFSET @offset ROWS FETCH NEXT @size ROWS ONLY",
            new { companyId, warehouseId, offset, size });
        return (rows.ToList(), total);
    }
}
