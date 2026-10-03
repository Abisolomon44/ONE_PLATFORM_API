using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface ISalesReturnRepository
{
    Task<(List<SalesReturn> Items, int TotalCount)> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<SalesReturn?> GetByIdAsync(long id);
    Task<string> GetNextReturnNoAsync(long companyId);
    Task<long> InsertAsync(SalesReturn entity);
    Task<bool> UpdateAsync(SalesReturn entity);
    Task<bool> CancelAsync(long id, long userId, string reason);
    Task<string?> GetStatusCodeAsync(long id);
    Task<Dictionary<long, decimal>> GetReturnedQtyByInvoiceAsync(long salesInvoiceId, long excludeReturnId = 0);
    Task<int> CountStockTransactionsAsync(long id);
    Task<SalesReturn?> GetByRefundPaymentAsync(long paymentId);
}

public class SalesReturnRepository : TenantRepositoryBase, ISalesReturnRepository
{
    public SalesReturnRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<(List<SalesReturn> Items, int TotalCount)> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var sp = string.IsNullOrWhiteSpace(search) ? "%" : $"%{search}%";
        var offset = (pageNumber - 1) * pageSize;
        var total = await Sql.QuerySingleOrDefaultAsync<int>(connection,
            "SELECT COUNT(*) FROM dbo.SalesReturn WHERE CompanyId = @companyId AND (ReturnNumber LIKE @sp OR Remarks LIKE @sp OR Reason LIKE @sp OR CustomerNameSnapshot LIKE @sp)",
            new { companyId, sp });
        var items = await Sql.QueryAsync<SalesReturn>(connection,
            @"SELECT * FROM (
                SELECT *, ROW_NUMBER() OVER (ORDER BY SalesReturnId DESC) AS _rn
                FROM dbo.SalesReturn
                WHERE CompanyId = @companyId AND (ReturnNumber LIKE @sp OR Remarks LIKE @sp OR Reason LIKE @sp OR CustomerNameSnapshot LIKE @sp)
              ) t
              WHERE t._rn > @offset AND t._rn <= @offset + @pageSize",
            new { companyId, sp, offset, pageSize });
        return (items.ToList(), total);
    }

    public async Task<SalesReturn?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        var header = await Sql.QuerySingleOrDefaultAsync<SalesReturn>(connection,
            "SELECT * FROM dbo.SalesReturn WHERE SalesReturnId = @id", new { id });
        if (header == null) return null;
        var items = await Sql.QueryAsync<SalesReturnItem>(connection,
            "SELECT * FROM dbo.SalesReturnItem WHERE SalesReturnId = @id ORDER BY SalesReturnItemId", new { id });
        header.Items = items.ToList();
        return header;
    }

    public async Task<string> GetNextReturnNoAsync(long companyId)
    {
        using var connection = OpenTenant();
        var count = await Sql.QuerySingleOrDefaultAsync<int>(connection,
            "SELECT COUNT(*) FROM dbo.SalesReturn WHERE CompanyId = @companyId", new { companyId });
        return $"SRT-{DateTime.UtcNow:yyyy}-{(count + 1):D5}";
    }

    public async Task<long> InsertAsync(SalesReturn entity)
    {
        using var connection = OpenTenant();
        using var tx = connection.BeginTransaction();
        try
        {
            const string sqlH = @"
                INSERT INTO dbo.SalesReturn
                (
                    SalesInvoiceId, CompanyId, CompanyNameSnapshot, BranchId, BranchNameSnapshot, WarehouseId,
                    CustomerId, CustomerNameSnapshot, ReturnNumber, ReturnDate, TotalGrossAmount,
                    TotalDiscountAmount, TotalTaxableAmount, TotalCGSTAmount, TotalSGSTAmount, TotalIGSTAmount,
                    TotalCESSAmount, TotalRoundOff, GrandTotal, StatusID, Reason, Remarks,
                    PaymentTypeId, PaymentMethodId, RefundAmount, CreatedByUserID
                )
                VALUES
                (
                    @SalesInvoiceId, @CompanyId, @CompanyNameSnapshot, @BranchId, @BranchNameSnapshot, @WarehouseId,
                    @CustomerId, @CustomerNameSnapshot, @ReturnNumber, @ReturnDate, @TotalGrossAmount,
                    @TotalDiscountAmount, @TotalTaxableAmount, @TotalCGSTAmount, @TotalSGSTAmount, @TotalIGSTAmount,
                    @TotalCESSAmount, @TotalRoundOff, @GrandTotal, @StatusID, @Reason, @Remarks,
                    @PaymentTypeId, @PaymentMethodId, @RefundAmount, @CreatedByUserID
                );
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            var id = await Sql.QuerySingleOrDefaultAsync<long>(connection, sqlH, entity, tx);
            entity.SalesReturnId = id;

            foreach (var item in entity.Items)
            {
                item.SalesReturnId = id;
                const string sqlI = @"
                    INSERT INTO dbo.SalesReturnItem
                    (
                        SalesReturnId, SalesInvoiceItemId, ProductId, ProductCodeSnapshot, ProductNameSnapshot,
                        UnitID, UnitNameSnapshot, HSNID, HSNCodeSnapshot, BarcodeSnapshot, BatchId,
                        ReturnQuantity, FreeQuantity, Rate, DiscountAmount, TaxableAmount,
                        GSTPercent, CGSTPercent, SGSTPercent, IGSTPercent, CESSPercent,
                        CGSTAmount, SGSTAmount, IGSTAmount, CESSAmount, LineTotal
                    )
                    VALUES
                    (
                        @SalesReturnId, @SalesInvoiceItemId, @ProductId, @ProductCodeSnapshot, @ProductNameSnapshot,
                        @UnitID, @UnitNameSnapshot, @HSNID, @HSNCodeSnapshot, @BarcodeSnapshot, @BatchId,
                        @ReturnQuantity, @FreeQuantity, @Rate, @DiscountAmount, @TaxableAmount,
                        @GSTPercent, @CGSTPercent, @SGSTPercent, @IGSTPercent, @CESSPercent,
                        @CGSTAmount, @SGSTAmount, @IGSTAmount, @CESSAmount, @LineTotal
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS bigint);";
                await Sql.QuerySingleOrDefaultAsync<long>(connection, sqlI, item, tx);

                // Sales return: goods come back into the warehouse → stock IN.
                const string stockTx = @"
                    INSERT INTO dbo.StockTransaction
                    (
                        CompanyId, BranchId, WarehouseId, ProductId, UnitId, TransactionType, ReferenceType,
                        ReferenceId, QuantityIn, QuantityOut, Rate, BalanceQuantity, TransactionDate, Remarks, CreatedByUserID
                    )
                    VALUES
                    (
                        @CompanyId, @BranchId, @WarehouseId, @ProductId, @UnitID, 'IN', 'SALES_RETURN',
                        @ReferenceId, @QuantityIn, 0, @Rate,
                        (SELECT ISNULL(Quantity,0) FROM dbo.Stock WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId AND ProductId=@ProductId AND UnitId=@UnitID),
                        @TransactionDate, @Remarks, @CreatedByUserID
                    );";
                await Sql.ExecuteAsync(connection, stockTx, new
                {
                    entity.CompanyId,
                    entity.BranchId,
                    entity.WarehouseId,
                    item.ProductId,
                    item.UnitID,
                    ReferenceId = id,
                    QuantityIn = item.ReturnQuantity,
                    Rate = item.Rate,
                    TransactionDate = entity.ReturnDate,
                    Remarks = $"Sales Return {entity.ReturnNumber}",
                    entity.CreatedByUserID
                }, tx);

                const string upd = @"
                    UPDATE dbo.Stock SET
                        Quantity = Quantity + @qty,
                        AvailableQuantity = AvailableQuantity + @qty,
                        UpdatedAt = SYSUTCDATETIME()
                    WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId AND ProductId=@ProductId AND UnitId=@UnitID;";
                await Sql.ExecuteAsync(connection, upd, new
                {
                    entity.CompanyId,
                    entity.BranchId,
                    entity.WarehouseId,
                    item.ProductId,
                    item.UnitID,
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

    public async Task<bool> UpdateAsync(SalesReturn entity)
    {
        using var connection = OpenTenant();
        using var tx = connection.BeginTransaction();
        try
        {
            var old = await GetByIdAsync(entity.SalesReturnId);
            // Reverse old stock postings (quantities go back out).
            if (old != null)
            {
                foreach (var item in old.Items)
                {
                    await Sql.ExecuteAsync(connection,
                        @"UPDATE dbo.Stock SET Quantity = Quantity - @qty, AvailableQuantity = AvailableQuantity - @qty,
                          UpdatedAt = SYSUTCDATETIME()
                          WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId
                            AND ProductId=@ProductId AND UnitId=@UnitID",
                        new { old.CompanyId, old.BranchId, old.WarehouseId, item.ProductId, item.UnitID, qty = item.ReturnQuantity }, tx);
                }
                await Sql.ExecuteAsync(connection,
                    "DELETE FROM dbo.StockTransaction WHERE ReferenceType = 'SALES_RETURN' AND ReferenceId = @id",
                    new { id = entity.SalesReturnId }, tx);
                await Sql.ExecuteAsync(connection,
                    "DELETE FROM dbo.SalesReturnItem WHERE SalesReturnId = @id",
                    new { id = entity.SalesReturnId }, tx);
            }

            await Sql.ExecuteAsync(connection,
                @"UPDATE dbo.SalesReturn SET ReturnDate = @ReturnDate,
                  TotalGrossAmount = @TotalGrossAmount, TotalDiscountAmount = @TotalDiscountAmount,
                  TotalTaxableAmount = @TotalTaxableAmount, TotalCGSTAmount = @TotalCGSTAmount,
                  TotalSGSTAmount = @TotalSGSTAmount, TotalIGSTAmount = @TotalIGSTAmount,
                  TotalCESSAmount = @TotalCESSAmount, TotalRoundOff = @TotalRoundOff,
                  GrandTotal = @GrandTotal, Reason = @Reason, Remarks = @Remarks,
                  PaymentTypeId = @PaymentTypeId, PaymentMethodId = @PaymentMethodId, RefundAmount = @RefundAmount,
                  UpdatedByUserID = @UpdatedByUserID, UpdatedAt = SYSUTCDATETIME()
                  WHERE SalesReturnId = @SalesReturnId",
                entity, tx);

            foreach (var item in entity.Items)
            {
                item.SalesReturnId = entity.SalesReturnId;
                const string sqlI = @"
                    INSERT INTO dbo.SalesReturnItem
                    (
                        SalesReturnId, SalesInvoiceItemId, ProductId, ProductCodeSnapshot, ProductNameSnapshot,
                        UnitID, UnitNameSnapshot, HSNID, HSNCodeSnapshot, BarcodeSnapshot, BatchId,
                        ReturnQuantity, FreeQuantity, Rate, DiscountAmount, TaxableAmount,
                        GSTPercent, CGSTPercent, SGSTPercent, IGSTPercent, CESSPercent,
                        CGSTAmount, SGSTAmount, IGSTAmount, CESSAmount, LineTotal
                    )
                    VALUES
                    (
                        @SalesReturnId, @SalesInvoiceItemId, @ProductId, @ProductCodeSnapshot, @ProductNameSnapshot,
                        @UnitID, @UnitNameSnapshot, @HSNID, @HSNCodeSnapshot, @BarcodeSnapshot, @BatchId,
                        @ReturnQuantity, @FreeQuantity, @Rate, @DiscountAmount, @TaxableAmount,
                        @GSTPercent, @CGSTPercent, @SGSTPercent, @IGSTPercent, @CESSPercent,
                        @CGSTAmount, @SGSTAmount, @IGSTAmount, @CESSAmount, @LineTotal
                    );";
                await Sql.ExecuteAsync(connection, sqlI, item, tx);

                await Sql.ExecuteAsync(connection,
                    @"INSERT INTO dbo.StockTransaction
                      (CompanyId, BranchId, WarehouseId, ProductId, UnitId, TransactionType, ReferenceType,
                       ReferenceId, QuantityIn, QuantityOut, Rate, BalanceQuantity, TransactionDate, Remarks, CreatedByUserID)
                      VALUES (@CompanyId, @BranchId, @WarehouseId, @ProductId, @UnitID, 'IN', 'SALES_RETURN',
                       @ReferenceId, @QuantityIn, 0, @Rate,
                       (SELECT ISNULL(Quantity,0) FROM dbo.Stock WHERE CompanyId=@CompanyId AND BranchId=@BranchId
                         AND WarehouseId=@WarehouseId AND ProductId=@ProductId AND UnitId=@UnitID),
                       @TransactionDate, @Remarks, @CreatedByUserID)",
                    new
                    {
                        entity.CompanyId,
                        entity.BranchId,
                        entity.WarehouseId,
                        item.ProductId,
                        item.UnitID,
                        ReferenceId = entity.SalesReturnId,
                        QuantityIn = item.ReturnQuantity,
                        Rate = item.Rate,
                        TransactionDate = entity.ReturnDate,
                        Remarks = $"Sales Return {entity.ReturnNumber}",
                        entity.CreatedByUserID
                    }, tx);

                await Sql.ExecuteAsync(connection,
                    @"UPDATE dbo.Stock SET Quantity = Quantity + @qty, AvailableQuantity = AvailableQuantity + @qty,
                      UpdatedAt = SYSUTCDATETIME()
                      WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId
                        AND ProductId=@ProductId AND UnitId=@UnitID",
                    new { entity.CompanyId, entity.BranchId, entity.WarehouseId, item.ProductId, item.UnitID, qty = item.ReturnQuantity }, tx);
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

    public async Task<bool> CancelAsync(long id, long userId, string reason)
    {
        using var connection = OpenTenant();
        using var tx = connection.BeginTransaction();
        try
        {
            var header = await Sql.QuerySingleOrDefaultAsync<SalesReturn>(
                connection, "SELECT * FROM dbo.SalesReturn WHERE SalesReturnId = @id", new { id }, tx);
            if (header == null) return false;
            var items = (await Sql.QueryAsync<SalesReturnItem>(connection,
                "SELECT * FROM dbo.SalesReturnItem WHERE SalesReturnId = @id", new { id }, tx)).ToList();

            // Cancelling a return sends the goods back out of the warehouse.
            foreach (var item in items)
            {
                await Sql.ExecuteAsync(connection,
                    @"INSERT INTO dbo.StockTransaction
                      (CompanyId, BranchId, WarehouseId, ProductId, UnitId, TransactionType, ReferenceType,
                       ReferenceId, QuantityIn, QuantityOut, Rate, BalanceQuantity, TransactionDate, Remarks, CreatedByUserID)
                      VALUES (@CompanyId, @BranchId, @WarehouseId, @ProductId, @UnitID, 'OUT', 'SALES_RETURN_CANCEL',
                       @ReferenceId, 0, @QuantityOut, @Rate,
                       (SELECT ISNULL(Quantity,0) FROM dbo.Stock WHERE CompanyId=@CompanyId AND BranchId=@BranchId
                         AND WarehouseId=@WarehouseId AND ProductId=@ProductId AND UnitId=@UnitID),
                       SYSUTCDATETIME(), @Remarks, @userId)",
                    new
                    {
                        header.CompanyId,
                        header.BranchId,
                        header.WarehouseId,
                        item.ProductId,
                        item.UnitID,
                        ReferenceId = id,
                        QuantityOut = item.ReturnQuantity,
                        Rate = item.Rate,
                        Remarks = $"Sales Return cancel {header.ReturnNumber}",
                        userId
                    }, tx);

                await Sql.ExecuteAsync(connection,
                    @"UPDATE dbo.Stock SET Quantity = Quantity - @qty, AvailableQuantity = AvailableQuantity - @qty,
                      UpdatedAt = SYSUTCDATETIME()
                      WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId
                        AND ProductId=@ProductId AND UnitId=@UnitID",
                    new { header.CompanyId, header.BranchId, header.WarehouseId, item.ProductId, item.UnitID, qty = item.ReturnQuantity }, tx);
            }

            await Sql.ExecuteAsync(connection,
                @"UPDATE dbo.SalesReturn
                  SET StatusID = (SELECT StatusId FROM dbo.[Status] WHERE Code = 'CANCELLED'),
                      CancelledByUserID = @userId, CancelledAt = SYSUTCDATETIME(), CancellationReason = @reason,
                      UpdatedByUserID = @userId, UpdatedAt = SYSUTCDATETIME()
                  WHERE SalesReturnId = @id",
                new { id, userId, reason }, tx);

            tx.Commit();
            return true;
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
            @"SELECT s.Code FROM dbo.SalesReturn r LEFT JOIN dbo.[Status] s ON s.StatusId = r.StatusID WHERE r.SalesReturnId = @id",
            new { id });
    }

    public async Task<Dictionary<long, decimal>> GetReturnedQtyByInvoiceAsync(long salesInvoiceId, long excludeReturnId = 0)
    {
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long SalesInvoiceItemId, decimal Qty)>(connection,
            @"SELECT i.SalesInvoiceItemId, SUM(i.ReturnQuantity)
              FROM dbo.SalesReturnItem i INNER JOIN dbo.SalesReturn r ON r.SalesReturnId = i.SalesReturnId
              WHERE r.SalesInvoiceId = @salesInvoiceId AND r.SalesReturnId <> @excludeReturnId
                AND r.StatusID <> (SELECT StatusId FROM dbo.[Status] WHERE Code = 'CANCELLED')
              GROUP BY i.SalesInvoiceItemId",
            new { salesInvoiceId, excludeReturnId });
        return rows.ToDictionary(x => x.SalesInvoiceItemId, x => (decimal)x.Qty);
    }

    public async Task<int> CountStockTransactionsAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<int>(connection,
            "SELECT COUNT(*) FROM dbo.StockTransaction WHERE ReferenceType = 'SALES_RETURN' AND ReferenceId = @id",
            new { id });
    }

    public async Task<SalesReturn?> GetByRefundPaymentAsync(long paymentId)
    {
        using var connection = OpenTenant();
        var payment = await Sql.QuerySingleOrDefaultAsync<Payment>(connection,
            "SELECT * FROM dbo.Payment WHERE PaymentId = @paymentId AND ReferenceType = 'SALES_RETURN'",
            new { paymentId });
        if (payment == null) return null;
        return await GetByIdAsync(payment.ReferenceId);
    }
}
