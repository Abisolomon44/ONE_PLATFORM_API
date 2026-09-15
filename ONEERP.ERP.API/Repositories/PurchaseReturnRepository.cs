using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IPurchaseReturnRepository
{
    Task<(List<PurchaseReturn> Items, int TotalCount)> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<PurchaseReturn?> GetByIdAsync(long id);
    Task<string> GetNextReturnNoAsync(long companyId);
    Task<long> InsertAsync(PurchaseReturn entity);
    Task<bool> UpdateAsync(PurchaseReturn entity);
    Task<bool> DeleteAsync(long id);
    Task<bool> CancelAsync(long id, long userId, string reason);
    Task<string?> GetStatusCodeAsync(long id);
    Task<Dictionary<long, decimal>> GetReturnedQtyByPurchaseAsync(long purchaseId, long excludeReturnId = 0);
    Task<int> CountStockTransactionsAsync(long id);
}

public class PurchaseReturnRepository : TenantRepositoryBase, IPurchaseReturnRepository
{
    public PurchaseReturnRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<(List<PurchaseReturn> Items, int TotalCount)> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var sp = string.IsNullOrWhiteSpace(search) ? "%" : $"%{search}%";
        var offset = (pageNumber - 1) * pageSize;
        var total = await Sql.QuerySingleOrDefaultAsync<int>(connection,
            "SELECT COUNT(*) FROM dbo.PurchaseReturn WHERE CompanyId = @companyId AND (ReturnNumber LIKE @sp OR Remarks LIKE @sp OR Reason LIKE @sp)",
            new { companyId, sp });
        var items = await Sql.QueryAsync<PurchaseReturn>(connection,
            @"SELECT * FROM (
                SELECT *, ROW_NUMBER() OVER (ORDER BY PurchaseReturnId DESC) AS _rn
                FROM dbo.PurchaseReturn
                WHERE CompanyId = @companyId AND (ReturnNumber LIKE @sp OR Remarks LIKE @sp OR Reason LIKE @sp)
              ) t
              WHERE t._rn > @offset AND t._rn <= @offset + @pageSize",
            new { companyId, sp, offset, pageSize });
        return (items.ToList(), total);
    }

    public async Task<PurchaseReturn?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        var header = await Sql.QuerySingleOrDefaultAsync<PurchaseReturn>(connection,
            "SELECT * FROM dbo.PurchaseReturn WHERE PurchaseReturnId = @id", new { id });
        if (header == null) return null;
        var items = await Sql.QueryAsync<PurchaseReturnItem>(connection,
            "SELECT * FROM dbo.PurchaseReturnItem WHERE PurchaseReturnId = @id ORDER BY PurchaseReturnItemId", new { id });
        header.Items = items.ToList();
        return header;
    }

    public async Task<string> GetNextReturnNoAsync(long companyId)
    {
        using var connection = OpenTenant();
        var count = await Sql.QuerySingleOrDefaultAsync<int>(connection,
            "SELECT COUNT(*) FROM dbo.PurchaseReturn WHERE CompanyId = @companyId", new { companyId });
        return $"PRT-{DateTime.UtcNow:yyyy}-{(count + 1):D5}";
    }

    public async Task<long> InsertAsync(PurchaseReturn entity)
    {
        using var connection = OpenTenant();
        using var tx = connection.BeginTransaction();
        try
        {
            const string sqlH = @"
                INSERT INTO dbo.PurchaseReturn
                (
                    PurchaseId, CompanyId, CompanyNameSnapshot, BranchId, BranchNameSnapshot, WarehouseId,
                    SupplierId, SupplierNameSnapshot, ReturnNumber, ReturnDate, TotalGrossAmount,
                    TotalDiscountAmount, TotalTaxableAmount, TotalTaxAmount, TotalCessAmount, TotalRoundOff,
                    GrandTotal, StatusID, Reason, Remarks, CreatedByUserID, CreatedAt
                )
                VALUES
                (
                    @PurchaseId, @CompanyId, @CompanyNameSnapshot, @BranchId, @BranchNameSnapshot, @WarehouseId,
                    @SupplierId, @SupplierNameSnapshot, @ReturnNumber, @ReturnDate, @TotalGrossAmount,
                    @TotalDiscountAmount, @TotalTaxableAmount, @TotalTaxAmount, @TotalCessAmount, @TotalRoundOff,
                    @GrandTotal, @StatusID, @Reason, @Remarks, @CreatedByUserID, SYSUTCDATETIME()
                );
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            var id = await Sql.QuerySingleOrDefaultAsync<long>(connection, sqlH, entity, tx);
            entity.PurchaseReturnId = id;

            foreach (var item in entity.Items)
            {
                item.PurchaseReturnId = id;
                const string sqlI = @"
                    INSERT INTO dbo.PurchaseReturnItem
                    (
                        PurchaseReturnId, PurchaseItemId, ProductId, ProductCodeSnapshot, ProductNameSnapshot,
                        UnitId, UnitNameSnapshot, HSNId, HSNCodeSnapshot, ReturnQuantity, PurchaseRate,
                        DiscountAmount, TaxableValue, GSTRate, GSTAmount, CGSTRate, CGSTAmount, SGSTRate,
                        SGSTAmount, IGSTRate, IGSTAmount, CESSRate, CESSAmount, LineTotal
                    )
                    VALUES
                    (
                        @PurchaseReturnId, @PurchaseItemId, @ProductId, @ProductCodeSnapshot, @ProductNameSnapshot,
                        @UnitId, @UnitNameSnapshot, @HSNId, @HSNCodeSnapshot, @ReturnQuantity, @PurchaseRate,
                        @DiscountAmount, @TaxableValue, @GSTRate, @GSTAmount, @CGSTRate, @CGSTAmount, @SGSTRate,
                        @SGSTAmount, @IGSTRate, @IGSTAmount, @CESSRate, @CESSAmount, @LineTotal
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS bigint);";
                await Sql.QuerySingleOrDefaultAsync<long>(connection, sqlI, item, tx);

                const string stockTx = @"
                    INSERT INTO dbo.StockTransaction
                    (
                        CompanyId, BranchId, WarehouseId, ProductId, UnitId, TransactionType, ReferenceType,
                        ReferenceId, QuantityIn, QuantityOut, Rate, BalanceQuantity, TransactionDate, Remarks, CreatedByUserID
                    )
                    VALUES
                    (
                        @CompanyId, @BranchId, @WarehouseId, @ProductId, @UnitId, 'OUT', 'PURCHASE_RETURN',
                        @ReferenceId, 0, @QuantityOut, @Rate,
                        (SELECT ISNULL(Quantity,0) FROM dbo.Stock WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId AND ProductId=@ProductId AND UnitId=@UnitId),
                        @TransactionDate, @Remarks, @CreatedByUserID
                    );";
                await Sql.ExecuteAsync(connection, stockTx, new
                {
                    entity.CompanyId,
                    entity.BranchId,
                    entity.WarehouseId,
                    item.ProductId,
                    item.UnitId,
                    ReferenceId = id,
                    QuantityOut = item.ReturnQuantity,
                    Rate = item.PurchaseRate,
                    TransactionDate = entity.ReturnDate,
                    Remarks = $"Purchase Return {entity.ReturnNumber}",
                    entity.CreatedByUserID
                }, tx);

                const string upd = @"
                    UPDATE dbo.Stock SET
                        Quantity = Quantity - @qty,
                        AvailableQuantity = AvailableQuantity - @qty,
                        UpdatedAt = SYSUTCDATETIME()
                    WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId AND ProductId=@ProductId AND UnitId=@UnitId;";
                await Sql.ExecuteAsync(connection, upd, new
                {
                    entity.CompanyId,
                    entity.BranchId,
                    entity.WarehouseId,
                    item.ProductId,
                    item.UnitId,
                    qty = item.ReturnQuantity
                }, tx);
            }

            tx.Commit();
            return id;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<string?> GetStatusCodeAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<string?>(connection,
            @"SELECT s.Code FROM dbo.PurchaseReturn r LEFT JOIN dbo.[Status] s ON s.StatusId = r.StatusID WHERE r.PurchaseReturnId = @id",
            new { id });
    }

    public async Task<Dictionary<long, decimal>> GetReturnedQtyByPurchaseAsync(long purchaseId, long excludeReturnId = 0)
    {
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long PurchaseItemId, decimal Qty)>(connection,
            @"SELECT i.PurchaseItemId, SUM(i.ReturnQuantity)
              FROM dbo.PurchaseReturnItem i INNER JOIN dbo.PurchaseReturn r ON r.PurchaseReturnId = i.PurchaseReturnId
              WHERE r.PurchaseId = @purchaseId AND r.PurchaseReturnId <> @excludeReturnId
              GROUP BY i.PurchaseItemId",
            new { purchaseId, excludeReturnId });
        return rows.ToDictionary(x => x.PurchaseItemId, x => (decimal)x.Qty);
    }

    public async Task<int> CountStockTransactionsAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<int>(connection,
            "SELECT COUNT(*) FROM dbo.StockTransaction WHERE ReferenceType = 'PURCHASE_RETURN' AND ReferenceId = @id",
            new { id });
    }

    public async Task<bool> UpdateAsync(PurchaseReturn entity)
    {
        using var connection = OpenTenant();
        using var tx = connection.BeginTransaction();
        try
        {
            var old = await GetByIdAsync(entity.PurchaseReturnId);
            // Reverse old stock postings (add quantities back).
            if (old != null)
            {
                foreach (var item in old.Items)
                {
                    await Sql.ExecuteAsync(connection,
                        @"UPDATE dbo.Stock SET Quantity = Quantity + @qty, AvailableQuantity = AvailableQuantity + @qty,
                          UpdatedAt = SYSUTCDATETIME()
                          WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId
                            AND ProductId=@ProductId AND UnitId=@UnitId",
                        new { old.CompanyId, old.BranchId, old.WarehouseId, item.ProductId, item.UnitId, qty = item.ReturnQuantity }, tx);
                }
                await Sql.ExecuteAsync(connection,
                    "DELETE FROM dbo.StockTransaction WHERE ReferenceType = 'PURCHASE_RETURN' AND ReferenceId = @id",
                    new { id = entity.PurchaseReturnId }, tx);
                await Sql.ExecuteAsync(connection,
                    "DELETE FROM dbo.PurchaseReturnItem WHERE PurchaseReturnId = @id",
                    new { id = entity.PurchaseReturnId }, tx);
            }

            await Sql.ExecuteAsync(connection,
                @"UPDATE dbo.PurchaseReturn SET ReturnDate = @ReturnDate, TotalGrossAmount = @TotalGrossAmount,
                  TotalDiscountAmount = @TotalDiscountAmount, TotalTaxableAmount = @TotalTaxableAmount,
                  TotalTaxAmount = @TotalTaxAmount, TotalCessAmount = @TotalCessAmount, TotalRoundOff = @TotalRoundOff,
                  GrandTotal = @GrandTotal, Reason = @Reason, Remarks = @Remarks,
                  UpdatedByUserID = @UpdatedByUserID, UpdatedAt = SYSUTCDATETIME()
                  WHERE PurchaseReturnId = @PurchaseReturnId",
                entity, tx);

            foreach (var item in entity.Items)
            {
                item.PurchaseReturnId = entity.PurchaseReturnId;
                const string sqlI = @"
                    INSERT INTO dbo.PurchaseReturnItem
                    (
                        PurchaseReturnId, PurchaseItemId, ProductId, ProductCodeSnapshot, ProductNameSnapshot,
                        UnitId, UnitNameSnapshot, HSNId, HSNCodeSnapshot, ReturnQuantity, PurchaseRate,
                        DiscountAmount, TaxableValue, GSTRate, GSTAmount, CGSTRate, CGSTAmount, SGSTRate,
                        SGSTAmount, IGSTRate, IGSTAmount, CESSRate, CESSAmount, LineTotal
                    )
                    VALUES
                    (
                        @PurchaseReturnId, @PurchaseItemId, @ProductId, @ProductCodeSnapshot, @ProductNameSnapshot,
                        @UnitId, @UnitNameSnapshot, @HSNId, @HSNCodeSnapshot, @ReturnQuantity, @PurchaseRate,
                        @DiscountAmount, @TaxableValue, @GSTRate, @GSTAmount, @CGSTRate, @CGSTAmount, @SGSTRate,
                        @SGSTAmount, @IGSTRate, @IGSTAmount, @CESSRate, @CESSAmount, @LineTotal
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS bigint);";
                await Sql.QuerySingleOrDefaultAsync<long>(connection, sqlI, item, tx);

                await Sql.ExecuteAsync(connection,
                    @"INSERT INTO dbo.StockTransaction
                      (CompanyId, BranchId, WarehouseId, ProductId, UnitId, TransactionType, ReferenceType,
                       ReferenceId, QuantityIn, QuantityOut, Rate, BalanceQuantity, TransactionDate, Remarks, CreatedByUserID)
                      VALUES (@CompanyId, @BranchId, @WarehouseId, @ProductId, @UnitId, 'OUT', 'PURCHASE_RETURN',
                       @ReferenceId, 0, @QuantityOut, @Rate,
                       (SELECT ISNULL(Quantity,0) FROM dbo.Stock WHERE CompanyId=@CompanyId AND BranchId=@BranchId
                         AND WarehouseId=@WarehouseId AND ProductId=@ProductId AND UnitId=@UnitId),
                       @TransactionDate, @Remarks, @CreatedByUserID)",
                    new
                    {
                        entity.CompanyId, entity.BranchId, entity.WarehouseId,
                        item.ProductId, item.UnitId,
                        ReferenceId = entity.PurchaseReturnId,
                        QuantityOut = item.ReturnQuantity,
                        Rate = item.PurchaseRate,
                        TransactionDate = entity.ReturnDate,
                        Remarks = $"Purchase Return {entity.ReturnNumber}",
                        CreatedByUserID = entity.UpdatedByUserID ?? entity.CreatedByUserID
                    }, tx);

                await Sql.ExecuteAsync(connection,
                    @"UPDATE dbo.Stock SET Quantity = Quantity - @qty, AvailableQuantity = AvailableQuantity - @qty,
                      UpdatedAt = SYSUTCDATETIME()
                      WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId
                        AND ProductId=@ProductId AND UnitId=@UnitId",
                    new { entity.CompanyId, entity.BranchId, entity.WarehouseId, item.ProductId, item.UnitId, qty = item.ReturnQuantity }, tx);
            }

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var connection = OpenTenant();
        using var tx = connection.BeginTransaction();
        try
        {
            var header = await Sql.QuerySingleOrDefaultAsync<PurchaseReturn>(connection,
                "SELECT * FROM dbo.PurchaseReturn WHERE PurchaseReturnId = @id", new { id }, tx);
            if (header == null) return false;
            var items = (await Sql.QueryAsync<PurchaseReturnItem>(connection,
                "SELECT * FROM dbo.PurchaseReturnItem WHERE PurchaseReturnId = @id", new { id }, tx)).ToList();
            // Reverse stock (goods come back).
            foreach (var item in items)
            {
                await Sql.ExecuteAsync(connection,
                    @"UPDATE dbo.Stock SET Quantity = Quantity + @qty, AvailableQuantity = AvailableQuantity + @qty,
                      UpdatedAt = SYSUTCDATETIME()
                      WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId
                        AND ProductId=@ProductId AND UnitId=@UnitId",
                    new { header.CompanyId, header.BranchId, header.WarehouseId, item.ProductId, item.UnitId, qty = item.ReturnQuantity }, tx);
            }
            await Sql.ExecuteAsync(connection,
                "DELETE FROM dbo.StockTransaction WHERE ReferenceType = 'PURCHASE_RETURN' AND ReferenceId = @id", new { id }, tx);
            await Sql.ExecuteAsync(connection,
                "DELETE FROM dbo.PurchaseReturnItem WHERE PurchaseReturnId = @id", new { id }, tx);
            var ok = await Sql.ExecuteAsync(connection,
                "DELETE FROM dbo.PurchaseReturn WHERE PurchaseReturnId = @id", new { id }, tx) > 0;
            tx.Commit();
            return ok;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<bool> CancelAsync(long id, long userId, string reason)
    {
        using var connection = OpenTenant();
        using var tx = connection.BeginTransaction();
        try
        {
            var header = await Sql.QuerySingleOrDefaultAsync<PurchaseReturn>(connection,
                "SELECT * FROM dbo.PurchaseReturn WHERE PurchaseReturnId = @id", new { id }, tx);
            if (header == null) return false;
            var items = (await Sql.QueryAsync<PurchaseReturnItem>(connection,
                "SELECT * FROM dbo.PurchaseReturnItem WHERE PurchaseReturnId = @id", new { id }, tx)).ToList();
            var cancelledId = await Sql.QuerySingleOrDefaultAsync<long>(connection,
                "SELECT ISNULL(StatusId, 0) FROM dbo.[Status] WHERE Code = 'CANCELLED'", null, tx);

            await Sql.ExecuteAsync(connection,
                @"UPDATE dbo.PurchaseReturn SET StatusID = @sid, CancelledByUserID = @uid,
                  CancelledAt = SYSUTCDATETIME(), CancellationReason = @reason,
                  UpdatedByUserID = @uid, UpdatedAt = SYSUTCDATETIME()
                  WHERE PurchaseReturnId = @id",
                new { sid = cancelledId, uid = userId, reason, id }, tx);

            // Reverse stock (goods come back) + reversal IN transaction.
            foreach (var item in items)
            {
                await Sql.ExecuteAsync(connection,
                    @"UPDATE dbo.Stock SET Quantity = Quantity + @qty, AvailableQuantity = AvailableQuantity + @qty,
                      UpdatedAt = SYSUTCDATETIME()
                      WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId
                        AND ProductId=@ProductId AND UnitId=@UnitId",
                    new { header.CompanyId, header.BranchId, header.WarehouseId, item.ProductId, item.UnitId, qty = item.ReturnQuantity }, tx);
                await Sql.ExecuteAsync(connection,
                    @"INSERT INTO dbo.StockTransaction
                      (CompanyId, BranchId, WarehouseId, ProductId, UnitId, TransactionType, ReferenceType,
                       ReferenceId, QuantityIn, QuantityOut, Rate, BalanceQuantity, TransactionDate, Remarks, CreatedByUserID)
                      VALUES (@CompanyId, @BranchId, @WarehouseId, @ProductId, @UnitId, 'IN', 'PURCHASE_RETURN',
                       @ReferenceId, @QuantityIn, 0, @Rate,
                       (SELECT ISNULL(Quantity,0) FROM dbo.Stock WHERE CompanyId=@CompanyId AND BranchId=@BranchId
                         AND WarehouseId=@WarehouseId AND ProductId=@ProductId AND UnitId=@UnitId),
                       SYSUTCDATETIME(), @Remarks, @CreatedByUserID)",
                    new
                    {
                        header.CompanyId, header.BranchId, header.WarehouseId,
                        item.ProductId, item.UnitId,
                        ReferenceId = id,
                        QuantityIn = item.ReturnQuantity,
                        Rate = item.PurchaseRate,
                        Remarks = $"Purchase return cancel {header.ReturnNumber}",
                        CreatedByUserID = userId
                    }, tx);
            }

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
