using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface ISalesReturnService
{
    Task<PaginatedResult<SalesReturnDto>> GetPagedAsync(long companyId, int page, int size, string search);
    Task<SalesReturnDto?> GetByIdAsync(long id);
    Task<string> GetNextReturnNoAsync(long companyId);
    Task<SalesReturnDto> CreateAsync(long companyId, long userId, CreateSalesReturnRequest r);
    Task<SalesReturnDto> UpdateAsync(long id, long userId, UpdateSalesReturnRequest r);
    Task<SalesReturnDto> CancelAsync(long id, long userId, string reason);
    Task<RefundDto> CreateRefundAsync(long companyId, long userId, CreateRefundRequest r);
}

public class SalesReturnService : ISalesReturnService
{
    private readonly ISalesReturnRepository _repo;
    private readonly ISalesRepository _salesRepo;
    private readonly IStatusRepository _statusRepo;

    public SalesReturnService(ISalesReturnRepository repo, ISalesRepository salesRepo, IStatusRepository statusRepo)
    {
        _repo = repo;
        _salesRepo = salesRepo;
        _statusRepo = statusRepo;
    }

    public async Task<PaginatedResult<SalesReturnDto>> GetPagedAsync(long companyId, int page, int size, string search)
    {
        var (items, total) = await _repo.GetPagedAsync(companyId, page, size, search);
        return new PaginatedResult<SalesReturnDto>
        {
            Items = items.Select(MapHeader).ToList(),
            TotalCount = total,
            PageNumber = page,
            PageSize = size,
        };
    }

    public async Task<SalesReturnDto?> GetByIdAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e == null ? null : await MapAsync(e);
    }

    public Task<string> GetNextReturnNoAsync(long companyId) => _repo.GetNextReturnNoAsync(companyId);

    public async Task<SalesReturnDto> CreateAsync(long companyId, long userId, CreateSalesReturnRequest r)
    {
        var entity = await BuildAsync(companyId, userId, r.SalesInvoiceId, r.ReturnDate, r.Items, r.Reason, r.Remarks, r.Refund, excludeReturnId: 0);
        await _repo.InsertAsync(entity);

        // Optional immediate refund (T030) — a negative-flow Payment against the return.
        if (r.Refund is { Amount: > 0 })
            await InsertRefundPaymentAsync(entity, userId);

        return MapHeader(await _repo.GetByIdAsync(entity.SalesReturnId) ?? entity);
    }

    public async Task<SalesReturnDto> UpdateAsync(long id, long userId, UpdateSalesReturnRequest r)
    {
        var existing = await _repo.GetByIdAsync(id)
            ?? throw new DomainException($"Sales return '{id}' was not found.");
        var status = await _repo.GetStatusCodeAsync(id) ?? string.Empty;
        if (string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Cancelled sales returns cannot be edited.");
        var entity = await BuildAsync(existing.CompanyId, userId, existing.SalesInvoiceId, r.ReturnDate, r.Items, r.Reason, r.Remarks, r.Refund, excludeReturnId: id);
        entity.SalesReturnId = id;
        entity.ReturnNumber = existing.ReturnNumber;
        entity.CreatedByUserID = existing.CreatedByUserID;
        entity.UpdatedByUserID = userId;
        await _repo.UpdateAsync(entity);
        return MapHeader(await _repo.GetByIdAsync(id) ?? entity);
    }

    public async Task<SalesReturnDto> CancelAsync(long id, long userId, string reason)
    {
        _ = await _repo.GetByIdAsync(id) ?? throw new DomainException($"Sales return '{id}' was not found.");
        var status = await _repo.GetStatusCodeAsync(id) ?? string.Empty;
        if (string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Sales return is already cancelled.");

        // Cancelling a return puts the goods back out of the warehouse (repo reverses stock).
        await _repo.CancelAsync(id, userId, reason);
        return MapHeader((await _repo.GetByIdAsync(id))!);
    }

    public async Task<RefundDto> CreateRefundAsync(long companyId, long userId, CreateRefundRequest r)
    {
        var ret = await _repo.GetByIdAsync(r.SalesReturnId)
            ?? throw new DomainException($"Sales return '{r.SalesReturnId}' was not found.");
        if (ret.CompanyId != companyId)
            throw new DomainException("Sales return does not belong to your company.");
        if (string.Equals(await _repo.GetStatusCodeAsync(r.SalesReturnId), "CANCELLED", StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Cannot refund a cancelled sales return.");
        if (r.Amount <= 0)
            throw new DomainException("Refund amount must be greater than 0.");
        if (r.Amount > ret.GrandTotal)
            throw new DomainException($"Refund amount {r.Amount:N2} exceeds the return total {ret.GrandTotal:N2}.");

        var paymentId = await InsertRefundPaymentAsync(ret, userId, r.PaymentDate, r.PaymentTypeId, r.PaymentMethodId, r.Amount, r.ReferenceNo, r.Remarks);
        var payment = await _salesRepo.GetPaymentByIdAsync(paymentId)
            ?? throw new DomainException("Refund payment could not be loaded.");
        return new RefundDto
        {
            PaymentId = payment.PaymentId,
            PaymentNo = payment.PaymentNo,
            PaymentDate = payment.PaymentDate,
            ReferenceType = payment.ReferenceType,
            ReferenceId = payment.ReferenceId,
            BusinessPartnerId = payment.BusinessPartnerId,
            Amount = payment.Amount,
            ReferenceNo = payment.ReferenceNo,
            Remarks = payment.Remarks,
        };
    }

    // ============================================================
    // Build + validation
    // ============================================================
    private async Task<SalesReturn> BuildAsync(long companyId, long userId, long salesInvoiceId,
        string returnDate, List<CreateSalesReturnItemInput> inputs, string? reason, string? remarks,
        SalesReturnRefundInput? refund, long excludeReturnId)
    {
        var invoice = await _salesRepo.GetByIdAsync(salesInvoiceId)
            ?? throw new DomainException($"Sales invoice '{salesInvoiceId}' was not found.");
        if (invoice.CompanyId != companyId)
            throw new DomainException("Sales invoice does not belong to your company.");
        if (!string.Equals(invoice.InvoiceStatus, "POSTED", StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Only posted sales invoices can be returned.");
        if (inputs == null || inputs.Count == 0)
            throw new DomainException("Add at least one return item.");

        var statusId = await _statusRepo.GetIdByCodeAsync("RETURNED");
        if (statusId == 0) statusId = 1;

        var soldByItem = invoice.Items.ToDictionary(i => i.SalesInvoiceItemId);
        var alreadyReturned = await _repo.GetReturnedQtyByInvoiceAsync(salesInvoiceId, excludeReturnId);

        var entity = new SalesReturn
        {
            SalesInvoiceId = invoice.SalesInvoiceId,
            CompanyId = companyId,
            CompanyNameSnapshot = invoice.CompanyNameSnapshot,
            BranchId = invoice.BranchId,
            WarehouseId = invoice.WarehouseId,
            CustomerId = invoice.CustomerId,
            CustomerNameSnapshot = invoice.CustomerNameSnapshot,
            ReturnNumber = excludeReturnId == 0 ? await _repo.GetNextReturnNoAsync(companyId) : string.Empty,
            ReturnDate = DateTime.Parse(returnDate),
            StatusID = statusId,
            Reason = reason,
            Remarks = remarks,
            PaymentTypeId = refund?.PaymentTypeId,
            PaymentMethodId = refund?.PaymentMethodId,
            RefundAmount = refund?.Amount ?? 0,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow,
        };

        decimal gross = 0, disc = 0, taxable = 0, cgst = 0, sgst = 0, igst = 0, cess = 0, grand = 0;
        foreach (var input in inputs)
        {
            if (!(input.ReturnQuantity > 0))
                throw new DomainException("Return quantity must be greater than 0.");
            if (!soldByItem.TryGetValue(input.SalesInvoiceItemId, out var src))
                throw new DomainException($"Sales item '{input.SalesInvoiceItemId}' does not belong to invoice '{salesInvoiceId}'.");
            alreadyReturned.TryGetValue(input.SalesInvoiceItemId, out var prev);
            var available = src.Quantity - prev;
            if (input.ReturnQuantity - available > 0.000001m)
                throw new DomainException($"Return quantity {input.ReturnQuantity} exceeds available {available} for '{src.ProductNameSnapshot ?? src.ProductId.ToString()}'.");

            var lineGross = input.Rate * input.ReturnQuantity;
            var lineTaxable = lineGross - input.DiscountAmount;
            var lineCgst = lineTaxable * input.CGSTPercent / 100m;
            var lineSgst = lineTaxable * input.SGSTPercent / 100m;
            var lineIgst = lineTaxable * input.IGSTPercent / 100m;
            var lineCess = lineTaxable * input.CESSPercent / 100m;
            var lineTotal = lineTaxable + lineCgst + lineSgst + lineIgst + lineCess;

            gross += lineGross;
            disc += input.DiscountAmount;
            taxable += lineTaxable;
            cgst += lineCgst;
            sgst += lineSgst;
            igst += lineIgst;
            cess += lineCess;
            grand += lineTotal;

            entity.Items.Add(new SalesReturnItem
            {
                SalesInvoiceItemId = input.SalesInvoiceItemId,
                ProductId = input.ProductId,
                ProductCodeSnapshot = src.ProductCodeSnapshot,
                ProductNameSnapshot = src.ProductNameSnapshot,
                UnitID = input.UnitID,
                UnitNameSnapshot = src.UnitNameSnapshot,
                HSNID = src.HSNID,
                HSNCodeSnapshot = src.HSNCodeSnapshot,
                BarcodeSnapshot = src.BarcodeSnapshot,
                BatchId = src.BatchId,
                ReturnQuantity = input.ReturnQuantity,
                FreeQuantity = input.FreeQuantity,
                Rate = input.Rate,
                DiscountAmount = input.DiscountAmount,
                TaxableAmount = Math.Round(lineTaxable, 2),
                GSTPercent = input.GSTPercent,
                CGSTPercent = input.CGSTPercent,
                SGSTPercent = input.SGSTPercent,
                IGSTPercent = input.IGSTPercent,
                CESSPercent = input.CESSPercent,
                CGSTAmount = Math.Round(lineCgst, 2),
                SGSTAmount = Math.Round(lineSgst, 2),
                IGSTAmount = Math.Round(lineIgst, 2),
                CESSAmount = Math.Round(lineCess, 2),
                LineTotal = Math.Round(lineTotal, 2),
            });
        }

        entity.TotalGrossAmount = Math.Round(gross, 2);
        entity.TotalDiscountAmount = Math.Round(disc, 2);
        entity.TotalTaxableAmount = Math.Round(taxable, 2);
        entity.TotalCGSTAmount = Math.Round(cgst, 2);
        entity.TotalSGSTAmount = Math.Round(sgst, 2);
        entity.TotalIGSTAmount = Math.Round(igst, 2);
        entity.TotalCESSAmount = Math.Round(cess, 2);
        entity.TotalRoundOff = 0;
        entity.GrandTotal = Math.Round(grand, 2);

        if (refund is { Amount: > 0 } && refund.Amount > entity.GrandTotal)
            throw new DomainException($"Refund amount {refund.Amount:N2} exceeds the return total {entity.GrandTotal:N2}.");

        return entity;
    }

    private async Task<long> InsertRefundPaymentAsync(SalesReturn entity, long userId,
        string? paymentDate = null, long? paymentTypeId = null, long? paymentMethodId = null,
        decimal? amount = null, string? referenceNo = null, string? remarks = null)
    {
        var postedStatusId = await _statusRepo.GetIdByCodeAsync("POSTED");
        if (postedStatusId == 0) postedStatusId = 4;

        var paymentId = await _salesRepo.InsertRefundPaymentAsync(new Payment
        {
            CompanyId = entity.CompanyId,
            PaymentNo = $"SREF-{DateTime.UtcNow:yyyyMMdd}-{entity.SalesReturnId}",
            PaymentDate = DateTime.TryParse(paymentDate, out var pd) ? pd : entity.ReturnDate,
            PaymentTypeID = paymentTypeId ?? entity.PaymentTypeId ?? 0,
            PaymentMethodID = paymentMethodId ?? entity.PaymentMethodId ?? 0,
            ReferenceType = "SALES_RETURN",
            ReferenceId = entity.SalesReturnId,
            BusinessPartnerId = entity.CustomerId,
            Amount = amount ?? entity.RefundAmount,
            ReferenceNo = referenceNo,
            Remarks = remarks ?? $"Refund for sales return {entity.ReturnNumber}",
            StatusID = postedStatusId,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow,
        });
        return paymentId;
    }

    private static SalesReturnDto MapHeader(SalesReturn e) => new()
    {
        SalesReturnId = e.SalesReturnId,
        SalesInvoiceId = e.SalesInvoiceId,
        CompanyId = e.CompanyId,
        BranchId = e.BranchId,
        WarehouseId = e.WarehouseId,
        CustomerId = e.CustomerId,
        CustomerNameSnapshot = e.CustomerNameSnapshot,
        ReturnNumber = e.ReturnNumber,
        ReturnDate = e.ReturnDate,
        TotalGrossAmount = e.TotalGrossAmount,
        TotalDiscountAmount = e.TotalDiscountAmount,
        TotalTaxableAmount = e.TotalTaxableAmount,
        TotalCGSTAmount = e.TotalCGSTAmount,
        TotalSGSTAmount = e.TotalSGSTAmount,
        TotalIGSTAmount = e.TotalIGSTAmount,
        TotalCESSAmount = e.TotalCESSAmount,
        TotalRoundOff = e.TotalRoundOff,
        GrandTotal = e.GrandTotal,
        RefundAmount = e.RefundAmount,
        Reason = e.Reason,
        Remarks = e.Remarks,
        CancellationReason = e.CancellationReason,
    };

    private async Task<SalesReturnDto> MapAsync(SalesReturn e)
    {
        var dto = MapHeader(e);
        dto.SalesInvoiceNo = (await _salesRepo.GetByIdAsync(e.SalesInvoiceId))?.SalesInvoiceNo ?? string.Empty;
        dto.Items = e.Items.Select(i => new SalesReturnItemDto
        {
            SalesReturnItemId = i.SalesReturnItemId,
            SalesReturnId = i.SalesReturnId,
            SalesInvoiceItemId = i.SalesInvoiceItemId,
            ProductId = i.ProductId,
            ProductCodeSnapshot = i.ProductCodeSnapshot,
            ProductNameSnapshot = i.ProductNameSnapshot,
            UnitID = i.UnitID,
            UnitNameSnapshot = i.UnitNameSnapshot,
            HSNCodeSnapshot = i.HSNCodeSnapshot,
            ReturnQuantity = i.ReturnQuantity,
            FreeQuantity = i.FreeQuantity,
            Rate = i.Rate,
            DiscountAmount = i.DiscountAmount,
            TaxableAmount = i.TaxableAmount,
            GSTPercent = i.GSTPercent,
            CGSTAmount = i.CGSTAmount,
            SGSTAmount = i.SGSTAmount,
            IGSTAmount = i.IGSTAmount,
            CESSAmount = i.CESSAmount,
            LineTotal = i.LineTotal,
        }).ToList();
        return dto;
    }
}
