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
    Task<long> InsertAsync(SalesInvoice entity, List<CreateSalesPaymentInput>? payments);
    Task<bool> UpdateAsync(SalesInvoice entity);
    Task<bool> DeleteAsync(long id);
    Task<bool> CancelAsync(long id, long userId, string reason);
    Task<bool> ReplacePaymentsAsync(long id, List<CreateSalesPaymentInput> payments, decimal grandTotal, long userId);
    Task<long> InsertRefundPaymentAsync(Payment payment);
    Task<Payment?> GetPaymentByIdAsync(long id);
    Task<List<ProductStockDto>> GetStockAvailabilityAsync(long companyId, long productId);
    Task<List<SalesProductSearchDto>> SearchProductsAsync(long companyId, long branchId, long warehouseId, long? priceListId, string search, int size, bool includeOutOfStock);
    Task<SalesInvoicePrintDto?> GetPrintDataAsync(long salesInvoiceId);
}

public class SalesRepository : TenantRepositoryBase, ISalesRepository
{
    /// <summary>
    /// Read-model for document designer preview / print. One query joins the
    /// invoice header, customer (BusinessPartners), company, branch, warehouse
    /// and the first payment line; items come from SalesInvoiceItem.
    /// </summary>
    public async Task<SalesInvoicePrintDto?> GetPrintDataAsync(long salesInvoiceId)
    {
        using var connection = OpenTenant();
        var header = (await Sql.QueryAsync<SalesInvoicePrintDto>(connection, @"
            SELECT TOP 1
                   si.SalesInvoiceId, si.SalesInvoiceNo, si.InvoiceDate,
                   si.CompanyId,
                   c.CompanyName, c.GSTNumber AS CompanyGstin,
                   ISNULL(a1.AddressLine1, '') AS CompanyAddress, c.Phone AS CompanyPhone, c.Email AS CompanyEmail,
                   c.LogoUrl AS CompanyLogoUrl,
                   si.CustomerId, bp.PartnerName AS CustomerName, bp.TaxRegistrationNo AS CustomerGstin,
                   ISNULL(a2.AddressLine1, '') AS CustomerAddress, bp.MobileNo AS CustomerPhone,
                   si.BranchId, br.BranchName, si.WarehouseId, w.WarehouseName,
                   si.TotalGrossAmount, si.TotalDiscountAmount, si.TotalTaxableAmount,
                   si.TotalCGSTAmount, si.TotalSGSTAmount, si.TotalIGSTAmount, si.TotalCESSAmount,
                   si.TotalRoundOff, si.GrandTotal, si.PaidAmount, si.BalanceAmount,
                   pm.[Name] AS PaymentMode, si.Remarks
            FROM dbo.SalesInvoice si
            LEFT JOIN dbo.Companies c ON c.Id = si.CompanyId
            LEFT JOIN dbo.BusinessPartners bp ON bp.Id = si.CustomerId
            LEFT JOIN dbo.Branches br ON br.Id = si.BranchId
            LEFT JOIN dbo.Warehouses w ON w.Id = si.WarehouseId
            LEFT JOIN dbo.PaymentMethod pm ON pm.PaymentMethodId = si.PaymentMethodID
            LEFT JOIN dbo.EntityAddress ea1 ON ea1.EntityId = c.EntityId AND ea1.IsPrimary = 1 AND ea1.IsActive = 1
            LEFT JOIN dbo.Address a1 ON a1.AddressId = ea1.AddressId
            LEFT JOIN dbo.EntityAddress ea2 ON ea2.EntityId = bp.EntityId AND ea2.IsPrimary = 1 AND ea2.IsActive = 1
            LEFT JOIN dbo.Address a2 ON a2.AddressId = ea2.AddressId
            WHERE si.SalesInvoiceId = @salesInvoiceId", new { salesInvoiceId })).FirstOrDefault();
        if (header == null) return null;

        header.Items = (await Sql.QueryAsync<SalesInvoicePrintItemDto>(connection, @"
            SELECT sii.SalesInvoiceItemId,
                   ROW_NUMBER() OVER (ORDER BY sii.SalesInvoiceItemId) AS SlNo,
                   sii.ProductCodeSnapshot AS ProductCode, sii.ProductNameSnapshot AS ProductName,
                   sii.HSNCodeSnapshot AS HsnCode, sii.UnitNameSnapshot AS UnitName,
                   sii.Quantity, sii.Rate, sii.DiscountAmount, sii.TaxableAmount,
                   CASE WHEN siig.IsInterstate = 1 THEN sii.IGSTPercent ELSE sii.CGSTPercent + sii.SGSTPercent END AS TaxPercent,
                   CASE WHEN siig.IsInterstate = 1 THEN sii.IGSTAmount ELSE sii.CGSTAmount + sii.SGSTAmount END AS TaxAmount,
                   sii.LineTotal
            FROM dbo.SalesInvoiceItem sii
            OUTER APPLY (SELECT CASE WHEN sii.IGSTPercent > 0 THEN 1 ELSE 0 END AS IsInterstate) siig
            WHERE sii.SalesInvoiceId = @salesInvoiceId
            ORDER BY sii.SalesInvoiceItemId", new { salesInvoiceId })).ToList();
        return header;
    }

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

    public async Task<long> InsertAsync(SalesInvoice entity, List<CreateSalesPaymentInput>? payments)
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
                    FinancialYearId, POSSessionId, ReferenceNo, ReferenceDate, TotalGrossAmount, TotalDiscountAmount, TotalTaxableAmount,
                    TotalCGSTAmount, TotalSGSTAmount, TotalIGSTAmount, TotalCESSAmount, TotalRoundOff,
                    GrandTotal, PaidAmount, BalanceAmount, PaymentTypeID, PaymentMethodID, StatusID,
                    InvoiceStatus, Remarks, IsActive, CreatedByUserID, CreatedAt
                )
                VALUES
                (
                    @SalesInvoiceNo, @InvoiceDate, @SourceType, @CompanyId, @CompanyNameSnapshot,
                    @BranchId, @WarehouseId, @CustomerId, @CustomerNameSnapshot, @SalesTypeId, @PriceListId,
                    @FinancialYearId, @POSSessionId, @ReferenceNo, @ReferenceDate, @TotalGrossAmount, @TotalDiscountAmount, @TotalTaxableAmount,
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

            var appliedPayments = payments?.Where(p => p.Amount > 0).ToList() ?? new List<CreateSalesPaymentInput>();
            if (appliedPayments.Count > 0)
            {
                var totalPaid = appliedPayments.Sum(p => p.Amount);
                if (totalPaid > entity.GrandTotal)
                    throw new DomainException(
                        $"Payment total {totalPaid:N2} exceeds the invoice total {entity.GrandTotal:N2}.");

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

                const string sqlA = @"
                    INSERT INTO dbo.PaymentAllocation (PaymentId, ReferenceType, ReferenceId, AllocatedAmount, CreatedAt)
                    VALUES (@PaymentId, 'SALES', @ReferenceId, @AllocatedAmount, SYSUTCDATETIME());";

                var seq = 0;
                foreach (var payment in appliedPayments)
                {
                    seq++;
                    var pay = new
                    {
                        CompanyId = entity.CompanyId,
                        PaymentNo = $"SPAY-{DateTime.UtcNow:yyyyMMdd}-{id}-{seq}",
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
                    await Sql.ExecuteAsync(connection, sqlA,
                        new { PaymentId = paymentId, ReferenceId = id, AllocatedAmount = payment.Amount }, tx);
                }

                await Sql.ExecuteAsync(connection,
                    "UPDATE dbo.SalesInvoice SET PaidAmount = @paid, BalanceAmount = @bal WHERE SalesInvoiceId = @id",
                    new { paid = totalPaid, bal = entity.GrandTotal - totalPaid, id }, tx);
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

    private async Task ReverseStockAsync(IDbConnection connection, IDbTransaction tx, SalesInvoice entity, string reason)
    {
        const string netRows = @"
            SELECT CompanyId, BranchId, WarehouseId, ProductId, UnitId,
                   SUM(ISNULL(QuantityOut, 0) - ISNULL(QuantityIn, 0)) AS NetOut
            FROM dbo.StockTransaction
            WHERE ReferenceType = 'SALES' AND ReferenceId = @id
            GROUP BY CompanyId, BranchId, WarehouseId, ProductId, UnitId";

        const string restore = @"
            UPDATE st
            SET st.Quantity = ISNULL(st.Quantity, 0) + n.NetOut,
                st.AvailableQuantity = ISNULL(st.AvailableQuantity, 0) + n.NetOut,
                st.UpdatedAt = SYSUTCDATETIME()
            FROM dbo.Stock AS st
            INNER JOIN (" + netRows + @") AS n
                   ON n.CompanyId = st.CompanyId AND n.BranchId = st.BranchId
                  AND n.WarehouseId = st.WarehouseId AND n.ProductId = st.ProductId
                  AND n.UnitId = st.UnitId
            WHERE n.NetOut <> 0;";
        await Sql.ExecuteAsync(connection, restore, new { id = entity.SalesInvoiceId }, tx);

        const string ledger = @"
            INSERT INTO dbo.StockTransaction
            (
                CompanyId, BranchId, WarehouseId, ProductId, UnitId, TransactionType, ReferenceType,
                ReferenceId, QuantityIn, QuantityOut, Rate, BalanceQuantity, TransactionDate, Remarks, CreatedByUserID
            )
            SELECT n.CompanyId, n.BranchId, n.WarehouseId, n.ProductId, n.UnitId, 'IN', 'SALES',
                   @id, n.NetOut, 0, 0,
                   (SELECT ISNULL(s.AvailableQuantity, 0) FROM dbo.Stock AS s
                     WHERE s.CompanyId = n.CompanyId AND s.BranchId = n.BranchId
                       AND s.WarehouseId = n.WarehouseId AND s.ProductId = n.ProductId
                       AND s.UnitId = n.UnitId),
                   @TransactionDate, @Remarks, @CreatedByUserID
            FROM (" + netRows + @") AS n
            WHERE n.NetOut <> 0;";
        await Sql.ExecuteAsync(connection, ledger, new
        {
            id = entity.SalesInvoiceId,
            TransactionDate = DateTime.UtcNow,
            Remarks = reason,
            CreatedByUserID = entity.CreatedByUserID
        }, tx);
    }

    private async Task SyncPaidAmountAsync(IDbConnection connection, IDbTransaction tx, SalesInvoice entity)
    {
        const string sql = @"
            UPDATE dbo.SalesInvoice
            SET PaidAmount = ISNULL(a.Allocated, 0),
                BalanceAmount = @grand - ISNULL(a.Allocated, 0)
            FROM dbo.SalesInvoice AS si
            OUTER APPLY (
                SELECT SUM(AllocatedAmount) AS Allocated
                FROM dbo.PaymentAllocation
                WHERE ReferenceType = 'SALES' AND ReferenceId = si.SalesInvoiceId
            ) AS a
            WHERE si.SalesInvoiceId = @id;";
        await Sql.ExecuteAsync(connection, sql, new { id = entity.SalesInvoiceId, grand = entity.GrandTotal }, tx);
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
                    FinancialYearId = @FinancialYearId,
                    POSSessionId = @POSSessionId,
                    TotalDiscountAmount = @TotalDiscountAmount, TotalTaxableAmount = @TotalTaxableAmount,
                    TotalCGSTAmount = @TotalCGSTAmount, TotalSGSTAmount = @TotalSGSTAmount,
                    TotalIGSTAmount = @TotalIGSTAmount, TotalCESSAmount = @TotalCESSAmount,
                    TotalRoundOff = @TotalRoundOff, GrandTotal = @GrandTotal, PaidAmount = @PaidAmount,
                    BalanceAmount = @BalanceAmount, PaymentTypeID = @PaymentTypeID,
                    PaymentMethodID = @PaymentMethodID, StatusID = @StatusID, InvoiceStatus = @InvoiceStatus,
                    Remarks = @Remarks, UpdatedByUserID = @UpdatedByUserID, UpdatedAt = SYSUTCDATETIME()
                WHERE SalesInvoiceId = @SalesInvoiceId;";
            await Sql.ExecuteAsync(connection, sqlH, entity, tx);

            await ReverseStockAsync(connection, tx, entity, $"Stock reversal for edited sales {entity.SalesInvoiceNo}");

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

                await UpdateStockAsync(connection, tx, entity, item);
            }

            await SyncPaidAmountAsync(connection, tx, entity);

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
            var header = await Sql.QuerySingleOrDefaultAsync<SalesInvoice>(
                connection, "SELECT * FROM dbo.SalesInvoice WHERE SalesInvoiceId = @id", new { id }, tx);
            if (header == null) return false;

            await ReverseStockAsync(connection, tx, header, $"Stock reversal for deleted sales {header.SalesInvoiceNo}");

            await Sql.ExecuteAsync(connection,
                "DELETE FROM dbo.PaymentAllocation WHERE ReferenceType = 'SALES' AND ReferenceId = @id", new { id }, tx);
            await Sql.ExecuteAsync(connection,
                "DELETE FROM dbo.Payment WHERE ReferenceType = 'SALES' AND ReferenceId = @id", new { id }, tx);

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

    public async Task<List<ProductStockDto>> GetStockAvailabilityAsync(long companyId, long productId)
    {
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<ProductStockDto>(connection,
            @"SELECT s.UnitId, u.UnitName, s.Quantity, s.AvailableQuantity, s.BranchId, s.WarehouseId
              FROM dbo.Stock AS s
              LEFT JOIN dbo.Units AS u ON u.Id = s.UnitId
              WHERE s.CompanyId = @companyId AND s.ProductId = @productId
              ORDER BY s.BranchId, s.WarehouseId, s.UnitId",
            new { companyId, productId });
        return rows.ToList();
    }

    public async Task<List<SalesProductSearchDto>> SearchProductsAsync(
        long companyId, long branchId, long warehouseId, long? priceListId, string search, int size, bool includeOutOfStock)
    {
        using var connection = OpenTenant();
        var sp = string.IsNullOrWhiteSpace(search) ? "%" : $"%{search.Trim()}%";
        var rows = await Sql.QueryAsync<SalesProductSearchDto>(connection,
            @"SELECT TOP (@size)
                p.Id AS ProductId,
                p.ProductCode,
                p.ProductName,
                p.Barcode,
                p.UOMId AS UnitId,
                u.UnitName,
                p.HsnSacId AS HSNID,
                h.GovernmentCode AS HSNCode,
                p.TaxId,
                ISNULL(t.TaxRate, 0) AS TaxRate,
                ISNULL(pld.Price, ISNULL(p.SalesPrice, ISNULL(p.MRP, 0))) AS Price,
                ISNULL(sel.AvailableQuantity, 0) AS AvailableQuantity,
                CASE WHEN ISNULL(sel.AvailableQuantity, 0) > 0 THEN 1 ELSE 0 END AS InStock,
                p.IsStockItem
              FROM dbo.Products AS p
              LEFT JOIN dbo.Units AS u ON u.Id = p.UOMId
              LEFT JOIN dbo.HsnSacs AS h ON h.HsnSacId = p.HsnSacId
              LEFT JOIN dbo.Taxes AS t ON t.Id = p.TaxId
              OUTER APPLY (
                  SELECT SUM(s.AvailableQuantity) AS AvailableQuantity
                  FROM dbo.Stock AS s
                  WHERE s.CompanyId = p.CompanyId
                    AND s.ProductId = p.Id
                    AND (@branchId = 0 OR s.BranchId = @branchId)
                    AND (@warehouseId = 0 OR s.WarehouseId = @warehouseId)
              ) AS sel
              OUTER APPLY (
                  SELECT TOP 1 d.Price
                  FROM dbo.PriceListDetails AS d
                  WHERE @priceListId IS NOT NULL
                    AND d.PriceListId = @priceListId
                    AND d.ProductId = p.Id
                    AND d.IsActive = 1
                    AND (d.UnitId IS NULL OR d.UnitId = p.UOMId)
                  ORDER BY CASE WHEN d.UnitId = p.UOMId THEN 0 ELSE 1 END, d.MinimumQuantity
              ) AS pld
              WHERE p.CompanyId = @companyId
                AND p.IsActive = 1
                AND p.IsSaleable = 1
                AND (@branchId = 0 OR p.BranchId IS NULL OR p.BranchId = @branchId)
                AND (@warehouseId = 0 OR p.WarehouseId IS NULL OR p.WarehouseId = @warehouseId)
                AND (@search IS NULL OR @search = '' OR p.ProductCode LIKE @sp OR p.ProductName LIKE @sp
                     OR p.Barcode LIKE @sp OR h.GovernmentCode LIKE @sp OR p.SKU LIKE @sp)
                AND (@includeOutOfStock = 1 OR ISNULL(sel.AvailableQuantity, 0) > 0)
              ORDER BY p.ProductName",
            new { companyId, branchId, warehouseId, priceListId, search, sp, size, includeOutOfStock });
        return rows.ToList();
    }

    /// <summary>
    /// T031 — cancel a posted invoice: marks InvoiceStatus='CANCELLED' and
    /// reverses stock with IN transactions (ReferenceType='SALES_CANCEL').
    /// Financial history is never physically deleted.
    /// </summary>
    public async Task<bool> CancelAsync(long id, long userId, string reason)
    {
        using var connection = OpenTenant();
        using var tx = connection.BeginTransaction();
        try
        {
            var header = await Sql.QuerySingleOrDefaultAsync<SalesInvoice>(
                connection, "SELECT * FROM dbo.SalesInvoice WHERE SalesInvoiceId = @id", new { id }, tx);
            if (header == null) return false;
            if (!string.Equals(header.InvoiceStatus, "POSTED", StringComparison.OrdinalIgnoreCase))
                throw new DomainException($"Only posted invoices can be cancelled (current status: {header.InvoiceStatus}).");

            var items = await GetItemsAsync(id);
            foreach (var item in items)
            {
                await Sql.ExecuteAsync(connection,
                    @"INSERT INTO dbo.StockTransaction
                      (CompanyId, BranchId, WarehouseId, ProductId, UnitId, TransactionType, ReferenceType,
                       ReferenceId, QuantityIn, QuantityOut, Rate, BalanceQuantity, TransactionDate, Remarks, CreatedByUserID)
                      VALUES (@CompanyId, @BranchId, @WarehouseId, @ProductId, @UnitID, 'IN', 'SALES_CANCEL',
                       @ReferenceId, @QuantityIn, 0, @Rate,
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
                        QuantityIn = item.Quantity,
                        Rate = item.Rate,
                        Remarks = $"Sales cancel {header.SalesInvoiceNo}",
                        userId
                    }, tx);

                await Sql.ExecuteAsync(connection,
                    @"UPDATE dbo.Stock SET Quantity = Quantity + @qty, AvailableQuantity = AvailableQuantity + @qty,
                      UpdatedAt = SYSUTCDATETIME()
                      WHERE CompanyId=@CompanyId AND BranchId=@BranchId AND WarehouseId=@WarehouseId
                        AND ProductId=@ProductId AND UnitId=@UnitID",
                    new { header.CompanyId, header.BranchId, header.WarehouseId, item.ProductId, item.UnitID, qty = item.Quantity }, tx);
            }

            await Sql.ExecuteAsync(connection,
                @"UPDATE dbo.SalesInvoice SET InvoiceStatus = 'CANCELLED', Remarks = COALESCE(NULLIF(@reason, ''), Remarks),
                  UpdatedByUserID = @userId, UpdatedAt = SYSUTCDATETIME()
                  WHERE SalesInvoiceId = @id",
                new { id, reason, userId }, tx);

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <summary>
    /// T035 — replace the invoice's SALES payments atomically: existing
    /// PaymentAllocation + Payment rows (ReferenceType='SALES') are removed and
    /// the new set inserted, then PaidAmount/BalanceAmount are re-derived from
    /// GrandTotal so the invoice reconciliation always holds.
    /// </summary>
    public async Task<bool> ReplacePaymentsAsync(long id, List<CreateSalesPaymentInput> payments, decimal grandTotal, long userId)
    {
        using var connection = OpenTenant();
        using var tx = connection.BeginTransaction();
        try
        {
            var applied = payments.Where(p => p.Amount > 0).ToList();
            var totalPaid = applied.Sum(p => p.Amount);
            if (totalPaid > grandTotal)
                throw new DomainException($"Payment total {totalPaid:N2} exceeds the invoice total {grandTotal:N2}.");

            var header = await Sql.QuerySingleOrDefaultAsync<SalesInvoice>(
                connection, "SELECT * FROM dbo.SalesInvoice WHERE SalesInvoiceId = @id", new { id }, tx)
                ?? throw new DomainException($"Sales invoice '{id}' was not found.");

            await Sql.ExecuteAsync(connection,
                "DELETE FROM dbo.PaymentAllocation WHERE ReferenceType = 'SALES' AND ReferenceId = @id",
                new { id }, tx);
            await Sql.ExecuteAsync(connection,
                "DELETE FROM dbo.Payment WHERE ReferenceType = 'SALES' AND ReferenceId = @id",
                new { id }, tx);

            var seq = 0;
            foreach (var payment in applied)
            {
                seq++;
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
                const string sqlA = @"
                    INSERT INTO dbo.PaymentAllocation (PaymentId, ReferenceType, ReferenceId, AllocatedAmount, CreatedAt)
                    VALUES (@PaymentId, 'SALES', @ReferenceId, @AllocatedAmount, SYSUTCDATETIME());";

                var pay = new
                {
                    header.CompanyId,
                    PaymentNo = $"SPAY-{DateTime.UtcNow:yyyyMMdd}-{id}-{seq}",
                    PaymentDate = DateTime.UtcNow,
                    payment.PaymentTypeID,
                    payment.PaymentMethodID,
                    ReferenceId = id,
                    BusinessPartnerId = header.CustomerId,
                    payment.Amount,
                    payment.ReferenceNo,
                    payment.Remarks,
                    StatusID = 4L,
                    CreatedByUserID = userId
                };
                var paymentId = await Sql.QuerySingleOrDefaultAsync<long>(connection, sqlP, pay, tx);
                await Sql.ExecuteAsync(connection, sqlA,
                    new { PaymentId = paymentId, ReferenceId = id, AllocatedAmount = payment.Amount }, tx);
            }

            await Sql.ExecuteAsync(connection,
                "UPDATE dbo.SalesInvoice SET PaidAmount = @paid, BalanceAmount = @bal, UpdatedByUserID = @userId, UpdatedAt = SYSUTCDATETIME() WHERE SalesInvoiceId = @id",
                new { paid = totalPaid, bal = grandTotal - totalPaid, userId, id }, tx);

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <summary>T030 — refund payment against a sales return (ReferenceType='SALES_RETURN').</summary>
    public async Task<long> InsertRefundPaymentAsync(Payment payment)
    {
        using var connection = OpenTenant();
        using var tx = connection.BeginTransaction();
        try
        {
            const string sqlP = @"
                INSERT INTO dbo.Payment
                (
                    CompanyId, PaymentNo, PaymentDate, PaymentTypeID, PaymentMethodID, ReferenceType,
                    ReferenceId, BusinessPartnerId, Amount, ReferenceNo, Remarks, StatusID, CreatedByUserID
                )
                VALUES
                (
                    @CompanyId, @PaymentNo, @PaymentDate, @PaymentTypeID, @PaymentMethodID, @ReferenceType,
                    @ReferenceId, @BusinessPartnerId, @Amount, @ReferenceNo, @Remarks, @StatusID, @CreatedByUserID
                );
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            var paymentId = await Sql.QuerySingleOrDefaultAsync<long>(connection, sqlP, payment, tx);

            const string sqlA = @"
                INSERT INTO dbo.PaymentAllocation (PaymentId, ReferenceType, ReferenceId, AllocatedAmount, CreatedAt)
                VALUES (@PaymentId, @ReferenceType, @ReferenceId, @AllocatedAmount, SYSUTCDATETIME());";
            await Sql.ExecuteAsync(connection, sqlA, new
            {
                PaymentId = paymentId,
                payment.ReferenceType,
                payment.ReferenceId,
                AllocatedAmount = payment.Amount
            }, tx);

            tx.Commit();
            return paymentId;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<Payment?> GetPaymentByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Payment>(connection,
            "SELECT * FROM dbo.Payment WHERE PaymentId = @id", new { id });
    }
}
