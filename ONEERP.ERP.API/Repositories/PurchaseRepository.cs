using System.Data;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IPurchaseRepository
{
    Task<(List<Purchase> Items, int TotalCount)> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<Purchase?> GetByIdAsync(long id);
    Task<List<PurchaseItem>> GetItemsAsync(long purchaseId);
    Task<List<PaymentAllocationDto>> GetAllocationsAsync(long purchaseId);
    Task<List<StockTransaction>> GetStockTransactionsAsync(long purchaseId);
    Task<string> GetNextPurchaseNoAsync(long companyId);
    Task<long> InsertAsync(Purchase entity, PurchasePaymentInput? payment);
    Task<bool> UpdateAsync(Purchase entity);
    Task<bool> DeleteAsync(long id);
}

public class PurchasePaymentInput
{
    public decimal Amount { get; set; }
    public long? PaymentTypeID { get; set; }
    public long? PaymentMethodID { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Remarks { get; set; }
}

public class PurchaseRepository : TenantRepositoryBase, IPurchaseRepository
{
    public PurchaseRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<(List<Purchase> Items, int TotalCount)> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var sp = string.IsNullOrWhiteSpace(search) ? "%" : $"%{search}%";
        var offset = (pageNumber - 1) * pageSize;
        var total = await Sql.QuerySingleOrDefaultAsync<int>(connection,
            "SELECT COUNT(*) FROM dbo.Purchase WHERE CompanyId = @companyId AND (PurchaseNumber LIKE @sp OR SupplierInvoiceNumber LIKE @sp OR Remarks LIKE @sp)",
            new { companyId, sp });
        var items = await Sql.QueryAsync<Purchase>(connection,
            @"SELECT * FROM (
                SELECT *, ROW_NUMBER() OVER (ORDER BY PurchaseId DESC) AS _rn
                FROM dbo.Purchase
                WHERE CompanyId = @companyId AND (PurchaseNumber LIKE @sp OR SupplierInvoiceNumber LIKE @sp OR Remarks LIKE @sp)
              ) t
              WHERE t._rn > @offset AND t._rn <= @offset + @pageSize",
            new { companyId, sp, offset, pageSize });
        return (items.ToList(), total);
    }

    public async Task<Purchase?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        var header = await Sql.QuerySingleOrDefaultAsync<Purchase>(connection,
            "SELECT * FROM dbo.Purchase WHERE PurchaseId = @id", new { id });
        if (header == null) return null;
        header.Items = (await GetItemsAsync(id)).ToList();
        return header;
    }

    public async Task<List<PurchaseItem>> GetItemsAsync(long purchaseId)
    {
        using var connection = OpenTenant();
        var items = await Sql.QueryAsync<PurchaseItem>(connection,
            "SELECT * FROM dbo.PurchaseItem WHERE PurchaseId = @purchaseId ORDER BY PurchaseItemId", new { purchaseId });
        return items.ToList();
    }

    public async Task<List<PaymentAllocationDto>> GetAllocationsAsync(long purchaseId)
    {
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<PaymentAllocationDto>(connection,
            @"SELECT pa.* FROM dbo.PaymentAllocation pa
              INNER JOIN dbo.Payment p ON p.PaymentId = pa.PaymentId
              WHERE pa.ReferenceType = 'PURCHASE' AND pa.ReferenceId = @purchaseId",
            new { purchaseId });
        return rows.ToList();
    }

    public async Task<List<StockTransaction>> GetStockTransactionsAsync(long purchaseId)
    {
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<StockTransaction>(connection,
            "SELECT * FROM dbo.StockTransaction WHERE ReferenceType = 'PURCHASE' AND ReferenceId = @purchaseId ORDER BY StockTransactionId",
            new { purchaseId });
        return rows.ToList();
    }

    public async Task<string> GetNextPurchaseNoAsync(long companyId)
    {
        using var connection = OpenTenant();
        var count = await Sql.QuerySingleOrDefaultAsync<int>(connection,
            "SELECT COUNT(*) FROM dbo.Purchase WHERE CompanyId = @companyId", new { companyId });
        return $"PUR-{DateTime.UtcNow:yyyy}-{(count + 1):D5}";
    }

    public async Task<long> InsertAsync(Purchase entity, PurchasePaymentInput? payment)
    {
        using var connection = OpenTenant();
        using var tx = connection.BeginTransaction();
        try
        {
            const string sqlH = @"
                INSERT INTO dbo.Purchase
                (
                    CompanyId, BranchId, WarehouseId, SupplierId, PurchaseNumber, PurchaseDate,
                    SupplierInvoiceNumber, SupplierInvoiceDate, TotalGrossAmount, TotalDiscountAmount,
                    TotalTaxableAmount, TotalTaxAmount, TotalCessAmount, TotalRoundOff, GrandTotal,
                    PaidAmount, BalanceAmount, PaymentTypeID, PaymentMethodID, StatusID, Remarks,
                    IsActive, CreatedByUserID, CreatedAt
                )
                VALUES
                (
                    @CompanyId, @BranchId, @WarehouseId, @SupplierId, @PurchaseNumber, @PurchaseDate,
                    @SupplierInvoiceNumber, @SupplierInvoiceDate, @TotalGrossAmount, @TotalDiscountAmount,
                    @TotalTaxableAmount, @TotalTaxAmount, @TotalCessAmount, @TotalRoundOff, @GrandTotal,
                    @PaidAmount, @BalanceAmount, @PaymentTypeID, @PaymentMethodID, @StatusID, @Remarks,
                    1, @CreatedByUserID, SYSUTCDATETIME()
                );
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            var id = await Sql.QuerySingleOrDefaultAsync<long>(connection, sqlH, entity, tx);
            entity.PurchaseId = id;

            foreach (var item in entity.Items)
            {
                item.PurchaseId = id;
                const string sqlI = @"
                    INSERT INTO dbo.PurchaseItem
                    (
                        PurchaseId, ProductId, ProductCodeSnapshot, ProductNameSnapshot, BrandID, CategoryID,
                        SubCategoryID, UnitID, UnitNameSnapshot, HSNID, HSNCodeSnapshot, BarcodeSnapshot,
                        Quantity, FreeQuantity, PurchaseRate, MRP, RetailPrice, WholesalePrice, SaleRate,
                        DiscountPercentage, DiscountAmount, IsGSTInclusive, TaxableValue, GSTRate, GSTAmount,
                        CGSTRate, CGSTAmount, SGSTRate, SGSTAmount, IGSTRate, IGSTAmount, CESSRate, CESSAmount,
                        LineTotal, ManufacturingDate, ExpiryDate, Remarks
                    )
                    VALUES
                    (
                        @PurchaseId, @ProductId, @ProductCodeSnapshot, @ProductNameSnapshot, @BrandID, @CategoryID,
                        @SubCategoryID, @UnitID, @UnitNameSnapshot, @HSNID, @HSNCodeSnapshot, @BarcodeSnapshot,
                        @Quantity, @FreeQuantity, @PurchaseRate, @MRP, @RetailPrice, @WholesalePrice, @SaleRate,
                        @DiscountPercentage, @DiscountAmount, @IsGSTInclusive, @TaxableValue, @GSTRate, @GSTAmount,
                        @CGSTRate, @CGSTAmount, @SGSTRate, @SGSTAmount, @IGSTRate, @IGSTAmount, @CESSRate, @CESSAmount,
                        @LineTotal, @ManufacturingDate, @ExpiryDate, @Remarks
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS bigint);";
                await Sql.QuerySingleOrDefaultAsync<long>(connection, sqlI, item, tx);

                await UpdateStockAsync(connection, tx, entity, item);
            }

            if (payment != null && payment.Amount > 0)
            {
                const string sqlP = @"
                    INSERT INTO dbo.Payment
                    (
                        CompanyId, PaymentNo, PaymentDate, PaymentTypeID, PaymentMethodID, ReferenceType,
                        ReferenceId, BusinessPartnerId, Amount, ReferenceNo, Remarks, StatusID, CreatedByUserID
                    )
                    VALUES
                    (
                        @CompanyId, @PaymentNo, @PaymentDate, @PaymentTypeID, @PaymentMethodID, 'PURCHASE',
                        @ReferenceId, @BusinessPartnerId, @Amount, @ReferenceNo, @Remarks, @StatusID, @CreatedByUserID
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS bigint);";
                var pay = new
                {
                    CompanyId = entity.CompanyId,
                    PaymentNo = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{id}",
                    PaymentDate = entity.PurchaseDate,
                    PaymentTypeID = payment.PaymentTypeID ?? entity.PaymentTypeID,
                    PaymentMethodID = payment.PaymentMethodID ?? entity.PaymentMethodID,
                    ReferenceId = id,
                    BusinessPartnerId = entity.SupplierId,
                    Amount = payment.Amount,
                    ReferenceNo = payment.ReferenceNo,
                    Remarks = payment.Remarks,
                    StatusID = 4L,
                    CreatedByUserID = entity.CreatedByUserID
                };
                var paymentId = await Sql.QuerySingleOrDefaultAsync<long>(connection, sqlP, pay, tx);

                const string sqlA = @"
                    INSERT INTO dbo.PaymentAllocation (PaymentId, ReferenceType, ReferenceId, AllocatedAmount, CreatedAt)
                    VALUES (@PaymentId, 'PURCHASE', @ReferenceId, @AllocatedAmount, SYSUTCDATETIME());";
                await Sql.ExecuteAsync(connection, sqlA, new { PaymentId = paymentId, ReferenceId = id, AllocatedAmount = payment.Amount }, tx);

                await Sql.ExecuteAsync(connection,
                    "UPDATE dbo.Purchase SET PaidAmount = @paid, BalanceAmount = @bal WHERE PurchaseId = @id",
                    new { paid = payment.Amount, bal = entity.GrandTotal - payment.Amount, id }, tx);
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

    private async Task UpdateStockAsync(System.Data.IDbConnection connection, System.Data.IDbTransaction tx, Purchase entity, PurchaseItem item)
    {
        const string merge = @"
            MERGE dbo.Stock AS target
            USING (SELECT @CompanyId AS CompanyId, @BranchId AS BranchId, @WarehouseId AS WarehouseId, @ProductId AS ProductId, @UnitId AS UnitId) AS src
            ON target.CompanyId = src.CompanyId AND target.BranchId = src.BranchId AND target.WarehouseId = src.WarehouseId AND target.ProductId = src.ProductId AND target.UnitId = src.UnitId
            WHEN MATCHED THEN
                UPDATE SET
                    Quantity = target.Quantity + @Qty,
                    AvailableQuantity = target.AvailableQuantity + @Qty,
                    LastPurchaseRate = @Rate,
                    AverageCost = (target.Quantity * target.AverageCost + @Qty * @Rate) / NULLIF(target.Quantity + @Qty, 0),
                    UpdatedAt = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (CompanyId, BranchId, WarehouseId, ProductId, UnitId, Quantity, ReservedQuantity, AvailableQuantity, AverageCost, LastPurchaseRate, UpdatedAt)
                VALUES (@CompanyId, @BranchId, @WarehouseId, @ProductId, @UnitId, @Qty, 0, @Qty, @Rate, @Rate, SYSUTCDATETIME());";
        await Sql.ExecuteAsync(connection, merge, new
        {
            entity.CompanyId,
            entity.BranchId,
            entity.WarehouseId,
            item.ProductId,
            item.UnitID,
            Qty = item.Quantity,
            Rate = item.PurchaseRate
        }, tx);

        const string stockTx = @"
            INSERT INTO dbo.StockTransaction
            (
                CompanyId, BranchId, WarehouseId, ProductId, UnitId, TransactionType, ReferenceType,
                ReferenceId, QuantityIn, QuantityOut, Rate, BalanceQuantity, TransactionDate, Remarks, CreatedByUserID
            )
            VALUES
            (
                @CompanyId, @BranchId, @WarehouseId, @ProductId, @UnitId, 'IN', 'PURCHASE',
                @ReferenceId, @QuantityIn, 0, @Rate,
                (SELECT ISNULL(Quantity,0) FROM dbo.Stock WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId AND ProductId=@ProductId AND UnitId=@UnitId),
                @TransactionDate, @Remarks, @CreatedByUserID
            );";
        await Sql.ExecuteAsync(connection, stockTx, new
        {
            entity.CompanyId,
            entity.BranchId,
            entity.WarehouseId,
            item.ProductId,
            item.UnitID,
            ReferenceId = entity.PurchaseId,
            QuantityIn = item.Quantity,
            Rate = item.PurchaseRate,
            TransactionDate = entity.PurchaseDate,
            Remarks = $"Purchase {entity.PurchaseNumber}",
            entity.CreatedByUserID
        }, tx);
    }

    public async Task<bool> UpdateAsync(Purchase entity)
    {
        using var connection = OpenTenant();
        using var tx = connection.BeginTransaction();
        try
        {
            const string sqlH = @"
                UPDATE dbo.Purchase SET
                    BranchId = @BranchId, WarehouseId = @WarehouseId, SupplierId = @SupplierId,
                    PurchaseNumber = @PurchaseNumber, PurchaseDate = @PurchaseDate,
                    SupplierInvoiceNumber = @SupplierInvoiceNumber, SupplierInvoiceDate = @SupplierInvoiceDate,
                    TotalGrossAmount = @TotalGrossAmount, TotalDiscountAmount = @TotalDiscountAmount,
                    TotalTaxableAmount = @TotalTaxableAmount, TotalTaxAmount = @TotalTaxAmount,
                    TotalCessAmount = @TotalCessAmount, TotalRoundOff = @TotalRoundOff,
                    GrandTotal = @GrandTotal, PaidAmount = @PaidAmount, BalanceAmount = @BalanceAmount,
                    PaymentTypeID = @PaymentTypeID, PaymentMethodID = @PaymentMethodID, StatusID = @StatusID,
                    Remarks = @Remarks, UpdatedByUserID = @UpdatedByUserID, UpdatedAt = SYSUTCDATETIME()
                WHERE PurchaseId = @PurchaseId;";
            await Sql.ExecuteAsync(connection, sqlH, entity, tx);

            await Sql.ExecuteAsync(connection, "DELETE FROM dbo.PurchaseItem WHERE PurchaseId = @PurchaseId", new { entity.PurchaseId }, tx);

            foreach (var item in entity.Items)
            {
                item.PurchaseId = entity.PurchaseId;
                const string sqlI = @"
                    INSERT INTO dbo.PurchaseItem
                    (
                        PurchaseId, ProductId, ProductCodeSnapshot, ProductNameSnapshot, BrandID, CategoryID,
                        SubCategoryID, UnitID, UnitNameSnapshot, HSNID, HSNCodeSnapshot, BarcodeSnapshot,
                        Quantity, FreeQuantity, PurchaseRate, MRP, RetailPrice, WholesalePrice, SaleRate,
                        DiscountPercentage, DiscountAmount, IsGSTInclusive, TaxableValue, GSTRate, GSTAmount,
                        CGSTRate, CGSTAmount, SGSTRate, SGSTAmount, IGSTRate, IGSTAmount, CESSRate, CESSAmount,
                        LineTotal, ManufacturingDate, ExpiryDate, Remarks
                    )
                    VALUES
                    (
                        @PurchaseId, @ProductId, @ProductCodeSnapshot, @ProductNameSnapshot, @BrandID, @CategoryID,
                        @SubCategoryID, @UnitID, @UnitNameSnapshot, @HSNID, @HSNCodeSnapshot, @BarcodeSnapshot,
                        @Quantity, @FreeQuantity, @PurchaseRate, @MRP, @RetailPrice, @WholesalePrice, @SaleRate,
                        @DiscountPercentage, @DiscountAmount, @IsGSTInclusive, @TaxableValue, @GSTRate, @GSTAmount,
                        @CGSTRate, @CGSTAmount, @SGSTRate, @SGSTAmount, @IGSTRate, @IGSTAmount, @CESSRate, @CESSAmount,
                        @LineTotal, @ManufacturingDate, @ExpiryDate, @Remarks
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS bigint);";
                await Sql.QuerySingleOrDefaultAsync<long>(connection, sqlI, item, tx);
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
            await Sql.ExecuteAsync(connection, "DELETE FROM dbo.PurchaseItem WHERE PurchaseId = @id", new { id }, tx);
            var ok = await Sql.ExecuteAsync(connection, "DELETE FROM dbo.Purchase WHERE PurchaseId = @id", new { id }, tx) > 0;
            tx.Commit();
            return ok;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
