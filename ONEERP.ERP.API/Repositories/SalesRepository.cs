using System.Data;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Repositories;

public interface ISalesRepository
{
    Task<(List<SalesInvoice> Items, int TotalCount)> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<SalesInvoice?> GetByIdAsync(long id);
    Task<List<SalesInvoiceItem>> GetItemsAsync(long salesInvoiceId);
    Task<List<PaymentAllocationDto>> GetAllocationsAsync(long salesInvoiceId);
    Task<List<StockTransaction>> GetStockTransactionsAsync(long salesInvoiceId);
    Task<string> GetNextSalesNoAsync(long companyId);
    Task<long> InsertAsync(SalesInvoice entity, CreateSalesPaymentInput? payment);
    Task<bool> UpdateAsync(SalesInvoice entity);
    Task<bool> DeleteAsync(long id);
    Task<List<ProductStockDto>> GetStockAvailabilityAsync(long companyId, long productId);
}

public class SalesRepository : TenantRepositoryBase, ISalesRepository
{
    public SalesRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<(List<SalesInvoice> Items, int TotalCount)> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var sp = string.IsNullOrWhiteSpace(search) ? "%" : $"%{search}%";
        var offset = (pageNumber - 1) * pageSize;
        var total = await Sql.QuerySingleOrDefaultAsync<int>(connection,
            "SELECT COUNT(*) FROM dbo.SalesInvoice WHERE CompanyId = @companyId AND (SalesInvoiceNo LIKE @sp OR CustomerNameSnapshot LIKE @sp OR Remarks LIKE @sp)",
            new { companyId, sp });
        var items = await Sql.QueryAsync<SalesInvoice>(connection,
            @"SELECT * FROM (
                SELECT *, ROW_NUMBER() OVER (ORDER BY SalesInvoiceId DESC) AS _rn
                FROM dbo.SalesInvoice
                WHERE CompanyId = @companyId AND (SalesInvoiceNo LIKE @sp OR CustomerNameSnapshot LIKE @sp OR Remarks LIKE @sp)
              ) t
              WHERE t._rn > @offset AND t._rn <= @offset + @pageSize",
            new { companyId, sp, offset, pageSize });
        return (items.ToList(), total);
    }

    public async Task<SalesInvoice?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        var header = await Sql.QuerySingleOrDefaultAsync<SalesInvoice>(connection,
            "SELECT * FROM dbo.SalesInvoice WHERE SalesInvoiceId = @id", new { id });
        if (header == null) return null;
        header.Items = (await GetItemsAsync(id)).ToList();
        return header;
    }

    public async Task<List<SalesInvoiceItem>> GetItemsAsync(long salesInvoiceId)
    {
        using var connection = OpenTenant();
        var items = await Sql.QueryAsync<SalesInvoiceItem>(connection,
            "SELECT * FROM dbo.SalesInvoiceItem WHERE SalesInvoiceId = @salesInvoiceId ORDER BY SalesInvoiceItemId", new { salesInvoiceId });
        return items.ToList();
    }

    public async Task<List<PaymentAllocationDto>> GetAllocationsAsync(long salesInvoiceId)
    {
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<PaymentAllocationDto>(connection,
            @"SELECT pa.* FROM dbo.PaymentAllocation pa
              INNER JOIN dbo.Payment p ON p.PaymentId = pa.PaymentId
              WHERE pa.ReferenceType = 'SALES' AND pa.ReferenceId = @salesInvoiceId",
            new { salesInvoiceId });
        return rows.ToList();
    }

    public async Task<List<StockTransaction>> GetStockTransactionsAsync(long salesInvoiceId)
    {
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<StockTransaction>(connection,
            "SELECT * FROM dbo.StockTransaction WHERE ReferenceType = 'SALES' AND ReferenceId = @salesInvoiceId ORDER BY StockTransactionId",
            new { salesInvoiceId });
        return rows.ToList();
    }

    public async Task<string> GetNextSalesNoAsync(long companyId)
    {
        using var connection = OpenTenant();
        var count = await Sql.QuerySingleOrDefaultAsync<int>(connection,
            "SELECT COUNT(*) FROM dbo.SalesInvoice WHERE CompanyId = @companyId", new { companyId });
        return $"SIN-{DateTime.UtcNow:yyyy}-{(count + 1):D5}";
    }

    public async Task<long> InsertAsync(SalesInvoice entity, CreateSalesPaymentInput? payment)
    {
        using var connection = OpenTenant();
        using var tx = connection.BeginTransaction();
        try
        {
            const string sqlH = @"
                INSERT INTO dbo.SalesInvoice
                (
                    SalesInvoiceNo, InvoiceDate, SourceType, CompanyId, CompanyNameSnapshot,
                    BranchId, WarehouseId, CustomerId, CustomerNameSnapshot, SalesTypeId, PriceListId,
                    ReferenceNo, ReferenceDate, TotalGrossAmount, TotalDiscountAmount, TotalTaxableAmount,
                    TotalCGSTAmount, TotalSGSTAmount, TotalIGSTAmount, TotalCESSAmount, TotalRoundOff,
                    GrandTotal, PaidAmount, BalanceAmount, PaymentTypeID, PaymentMethodID, StatusID,
                    InvoiceStatus, Remarks, IsActive, CreatedByUserID, CreatedAt
                )
                VALUES
                (
                    @SalesInvoiceNo, @InvoiceDate, @SourceType, @CompanyId, @CompanyNameSnapshot,
                    @BranchId, @WarehouseId, @CustomerId, @CustomerNameSnapshot, @SalesTypeId, @PriceListId,
                    @ReferenceNo, @ReferenceDate, @TotalGrossAmount, @TotalDiscountAmount, @TotalTaxableAmount,
                    @TotalCGSTAmount, @TotalSGSTAmount, @TotalIGSTAmount, @TotalCESSAmount, @TotalRoundOff,
                    @GrandTotal, @PaidAmount, @BalanceAmount, @PaymentTypeID, @PaymentMethodID, @StatusID,
                    @InvoiceStatus, @Remarks, 1, @CreatedByUserID, SYSUTCDATETIME()
                );
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            var id = await Sql.QuerySingleOrDefaultAsync<long>(connection, sqlH, entity, tx);
            entity.SalesInvoiceId = id;

            foreach (var item in entity.Items)
            {
                item.SalesInvoiceId = id;
                const string sqlI = @"
                    INSERT INTO dbo.SalesInvoiceItem
                    (
                        SalesInvoiceId, ProductId, ProductCodeSnapshot, ProductNameSnapshot, UnitID,
                        UnitNameSnapshot, BatchId, HSNID, HSNCodeSnapshot, BarcodeSnapshot,
                        Quantity, FreeQuantity, Rate, GrossAmount, DiscountPercentage, DiscountAmount,
                        TaxableAmount, GSTPercent, CGSTPercent, SGSTPercent, IGSTPercent, CESSPercent,
                        CGSTAmount, SGSTAmount, IGSTAmount, CESSAmount, LineTotal, Remarks
                    )
                    VALUES
                    (
                        @SalesInvoiceId, @ProductId, @ProductCodeSnapshot, @ProductNameSnapshot, @UnitID,
                        @UnitNameSnapshot, @BatchId, @HSNID, @HSNCodeSnapshot, @BarcodeSnapshot,
                        @Quantity, @FreeQuantity, @Rate, @GrossAmount, @DiscountPercentage, @DiscountAmount,
                        @TaxableAmount, @GSTPercent, @CGSTPercent, @SGSTPercent, @IGSTPercent, @CESSPercent,
                        @CGSTAmount, @SGSTAmount, @IGSTAmount, @CESSAmount, @LineTotal, @Remarks
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
                        @CompanyId, @PaymentNo, @PaymentDate, @PaymentTypeID, @PaymentMethodID, 'SALES',
                        @ReferenceId, @BusinessPartnerId, @Amount, @ReferenceNo, @Remarks, @StatusID, @CreatedByUserID
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS bigint);";
                var pay = new
                {
                    CompanyId = entity.CompanyId,
                    PaymentNo = $"SPAY-{DateTime.UtcNow:yyyyMMdd}-{id}",
                    PaymentDate = entity.InvoiceDate,
                    PaymentTypeID = payment.PaymentTypeID ?? entity.PaymentTypeID,
                    PaymentMethodID = payment.PaymentMethodID ?? entity.PaymentMethodID,
                    ReferenceId = id,
                    BusinessPartnerId = entity.CustomerId,
                    Amount = payment.Amount,
                    ReferenceNo = payment.ReferenceNo,
                    Remarks = payment.Remarks,
                    StatusID = 4L,
                    CreatedByUserID = entity.CreatedByUserID
                };
                var paymentId = await Sql.QuerySingleOrDefaultAsync<long>(connection, sqlP, pay, tx);

                const string sqlA = @"
                    INSERT INTO dbo.PaymentAllocation (PaymentId, ReferenceType, ReferenceId, AllocatedAmount, CreatedAt)
                    VALUES (@PaymentId, 'SALES', @ReferenceId, @AllocatedAmount, SYSUTCDATETIME());";
                await Sql.ExecuteAsync(connection, sqlA, new { PaymentId = paymentId, ReferenceId = id, AllocatedAmount = payment.Amount }, tx);

                await Sql.ExecuteAsync(connection,
                    "UPDATE dbo.SalesInvoice SET PaidAmount = @paid, BalanceAmount = @bal WHERE SalesInvoiceId = @id",
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

    private async Task UpdateStockAsync(IDbConnection connection, IDbTransaction tx, SalesInvoice entity, SalesInvoiceItem item)
    {
        var stock = await Sql.QuerySingleOrDefaultAsync<decimal?>(connection,
            @"SELECT AvailableQuantity FROM dbo.Stock
              WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId AND ProductId=@ProductId AND UnitId=@UnitId",
            new { entity.CompanyId, entity.BranchId, entity.WarehouseId, item.ProductId, item.UnitID }, tx);

        if ((stock ?? 0) < item.Quantity)
            throw new DomainException($"Insufficient stock for product '{item.ProductNameSnapshot ?? item.ProductId.ToString()}' (available: {stock ?? 0}, demanded: {item.Quantity}).");

        const string upd = @"
            UPDATE dbo.Stock
            SET Quantity = Quantity - @Qty,
                AvailableQuantity = AvailableQuantity - @Qty,
                UpdatedAt = SYSUTCDATETIME()
            WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId
              AND ProductId=@ProductId AND UnitId=@UnitId;";
        await Sql.ExecuteAsync(connection, upd, new
        {
            entity.CompanyId,
            entity.BranchId,
            entity.WarehouseId,
            item.ProductId,
            item.UnitID,
            Qty = item.Quantity
        }, tx);

        const string stockTx = @"
            INSERT INTO dbo.StockTransaction
            (
                CompanyId, BranchId, WarehouseId, ProductId, UnitId, TransactionType, ReferenceType,
                ReferenceId, QuantityIn, QuantityOut, Rate, BalanceQuantity, TransactionDate, Remarks, CreatedByUserID
            )
            VALUES
            (
                @CompanyId, @BranchId, @WarehouseId, @ProductId, @UnitId, 'OUT', 'SALES',
                @ReferenceId, 0, @QuantityOut, @Rate,
                (SELECT ISNULL(AvailableQuantity,0) FROM dbo.Stock WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId AND ProductId=@ProductId AND UnitId=@UnitId),
                @TransactionDate, @Remarks, @CreatedByUserID
            );";
        await Sql.ExecuteAsync(connection, stockTx, new
        {
            entity.CompanyId,
            entity.BranchId,
            entity.WarehouseId,
            item.ProductId,
            item.UnitID,
            ReferenceId = entity.SalesInvoiceId,
            QuantityOut = item.Quantity,
            Rate = item.Rate,
            TransactionDate = entity.InvoiceDate,
            Remarks = $"Sales {entity.SalesInvoiceNo}",
            entity.CreatedByUserID
        }, tx);
    }

    public async Task<bool> UpdateAsync(SalesInvoice entity)
    {
        using var connection = OpenTenant();
        using var tx = connection.BeginTransaction();
        try
        {
            const string sqlH = @"
                UPDATE dbo.SalesInvoice SET
                    BranchId = @BranchId, WarehouseId = @WarehouseId, CustomerId = @CustomerId,
                    SalesInvoiceNo = @SalesInvoiceNo, InvoiceDate = @InvoiceDate, SourceType = @SourceType,
                    SalesTypeId = @SalesTypeId, PriceListId = @PriceListId, ReferenceNo = @ReferenceNo,
                    ReferenceDate = @ReferenceDate, TotalGrossAmount = @TotalGrossAmount,
                    TotalDiscountAmount = @TotalDiscountAmount, TotalTaxableAmount = @TotalTaxableAmount,
                    TotalCGSTAmount = @TotalCGSTAmount, TotalSGSTAmount = @TotalSGSTAmount,
                    TotalIGSTAmount = @TotalIGSTAmount, TotalCESSAmount = @TotalCESSAmount,
                    TotalRoundOff = @TotalRoundOff, GrandTotal = @GrandTotal, PaidAmount = @PaidAmount,
                    BalanceAmount = @BalanceAmount, PaymentTypeID = @PaymentTypeID,
                    PaymentMethodID = @PaymentMethodID, StatusID = @StatusID, InvoiceStatus = @InvoiceStatus,
                    Remarks = @Remarks, UpdatedByUserID = @UpdatedByUserID, UpdatedAt = SYSUTCDATETIME()
                WHERE SalesInvoiceId = @SalesInvoiceId;";
            await Sql.ExecuteAsync(connection, sqlH, entity, tx);

            await Sql.ExecuteAsync(connection, "DELETE FROM dbo.SalesInvoiceItem WHERE SalesInvoiceId = @SalesInvoiceId", new { entity.SalesInvoiceId }, tx);

            foreach (var item in entity.Items)
            {
                item.SalesInvoiceId = entity.SalesInvoiceId;
                const string sqlI = @"
                    INSERT INTO dbo.SalesInvoiceItem
                    (
                        SalesInvoiceId, ProductId, ProductCodeSnapshot, ProductNameSnapshot, UnitID,
                        UnitNameSnapshot, BatchId, HSNID, HSNCodeSnapshot, BarcodeSnapshot,
                        Quantity, FreeQuantity, Rate, GrossAmount, DiscountPercentage, DiscountAmount,
                        TaxableAmount, GSTPercent, CGSTPercent, SGSTPercent, IGSTPercent, CESSPercent,
                        CGSTAmount, SGSTAmount, IGSTAmount, CESSAmount, LineTotal, Remarks
                    )
                    VALUES
                    (
                        @SalesInvoiceId, @ProductId, @ProductCodeSnapshot, @ProductNameSnapshot, @UnitID,
                        @UnitNameSnapshot, @BatchId, @HSNID, @HSNCodeSnapshot, @BarcodeSnapshot,
                        @Quantity, @FreeQuantity, @Rate, @GrossAmount, @DiscountPercentage, @DiscountAmount,
                        @TaxableAmount, @GSTPercent, @CGSTPercent, @SGSTPercent, @IGSTPercent, @CESSPercent,
                        @CGSTAmount, @SGSTAmount, @IGSTAmount, @CESSAmount, @LineTotal, @Remarks
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
            await Sql.ExecuteAsync(connection, "DELETE FROM dbo.SalesInvoiceItem WHERE SalesInvoiceId = @id", new { id }, tx);
            var ok = await Sql.ExecuteAsync(connection, "DELETE FROM dbo.SalesInvoice WHERE SalesInvoiceId = @id", new { id }, tx) > 0;
            tx.Commit();
            return ok;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public Task<List<ProductStockDto>> GetStockAvailabilityAsync(long companyId, long productId)
    {
        throw new NotImplementedException();
    }
}
