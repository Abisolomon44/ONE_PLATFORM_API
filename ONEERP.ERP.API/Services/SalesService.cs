using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface ISalesService
{
    Task<PaginatedResult<SalesInvoiceDto>> GetPagedAsync(long companyId, int page, int size, string search);
    Task<SalesInvoiceDto?> GetByIdAsync(long id);
    Task<List<SalesItemDto>> GetItemsAsync(long id);
    Task<List<PaymentAllocationDto>> GetAllocationsAsync(long id);
    Task<List<StockTransaction>> GetStockTransactionsAsync(long id);
    Task<string> GetNextSalesNoAsync(long companyId);
    Task<List<ProductStockDto>> GetStockAvailabilityAsync(long companyId, long productId);
    Task<SalesInvoiceDto> CreateAsync(long companyId, long userId, CreateSalesRequest request);
    Task<SalesInvoiceDto> UpdateAsync(long id, long userId, UpdateSalesRequest request);
    Task DeleteAsync(long id);
}

public class SalesService : ISalesService
{
    private readonly ISalesRepository _repo;
    private readonly IStatusRepository _statusRepo;
    private readonly IProductService _productService;
    private readonly IProductUnitService _unitService;
    private readonly IBusinessPartnerService _businessPartnerService;
    private readonly ICompanyService _companyService;

    public SalesService(
        ISalesRepository repo,
        IStatusRepository statusRepo,
        IProductService productService,
        IProductUnitService unitService,
        IBusinessPartnerService businessPartnerService,
        ICompanyService companyService)
    {
        _repo = repo;
        _statusRepo = statusRepo;
        _productService = productService;
        _unitService = unitService;
        _businessPartnerService = businessPartnerService;
        _companyService = companyService;
    }

    public async Task<PaginatedResult<SalesInvoiceDto>> GetPagedAsync(long companyId, int page, int size, string search)
    {
        var (items, total) = await _repo.GetPagedAsync(companyId, page, size, search);
        return new PaginatedResult<SalesInvoiceDto>
        {
            Items = items.Select(MapHeader).ToList(),
            TotalCount = total,
            PageNumber = page,
            PageSize = size,
        };
    }

    public async Task<SalesInvoiceDto?> GetByIdAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e == null ? null : Map(e);
    }

    public async Task<List<SalesItemDto>> GetItemsAsync(long id)
        => (await _repo.GetItemsAsync(id)).Select(MapItem).ToList();

    public Task<List<PaymentAllocationDto>> GetAllocationsAsync(long id) => _repo.GetAllocationsAsync(id);

    public Task<List<StockTransaction>> GetStockTransactionsAsync(long id) => _repo.GetStockTransactionsAsync(id);

    public Task<string> GetNextSalesNoAsync(long companyId) => _repo.GetNextSalesNoAsync(companyId);

    public Task<List<ProductStockDto>> GetStockAvailabilityAsync(long companyId, long productId)
        => _repo.GetStockAvailabilityAsync(companyId, productId);

    public async Task<SalesInvoiceDto> CreateAsync(long companyId, long userId, CreateSalesRequest r)
    {
        var effectiveCompanyId = r.CompanyId > 0 ? r.CompanyId : companyId;
        var entity = await BuildAsync(effectiveCompanyId, userId, r.BranchId, r.WarehouseId, r.CustomerId,
            r.InvoiceNumber, r.InvoiceDate, r.SourceType, r.SalesTypeId, r.PriceListId,
            r.ReferenceNo, r.ReferenceDate, r.PaymentTypeID, r.PaymentMethodID, r.Remarks, r.Items, "POSTED");
        entity.CompanyNameSnapshot = await GetCompanyNameAsync(effectiveCompanyId);
        await _repo.InsertAsync(entity, r.Payment);
        return Map(await _repo.GetByIdAsync(entity.SalesInvoiceId) ?? entity);
    }

    public async Task<SalesInvoiceDto> UpdateAsync(long id, long userId, UpdateSalesRequest r)
    {
        var existing = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Sales invoice '{id}' was not found.");
        var effectiveCompanyId = r.CompanyId > 0 ? r.CompanyId : existing.CompanyId;
        var entity = await BuildAsync(effectiveCompanyId, userId, r.BranchId, r.WarehouseId, r.CustomerId,
            r.InvoiceNumber, r.InvoiceDate, existing.SourceType, r.SalesTypeId, r.PriceListId,
            r.ReferenceNo, r.ReferenceDate, r.PaymentTypeID, r.PaymentMethodID, r.Remarks, r.Items, "POSTED");
        entity.SalesInvoiceId = id;
        entity.CompanyId = effectiveCompanyId;
        entity.CompanyNameSnapshot = await GetCompanyNameAsync(effectiveCompanyId);
        entity.CreatedByUserID = existing.CreatedByUserID;
        entity.CreatedAt = existing.CreatedAt;
        entity.UpdatedByUserID = userId;
        await _repo.UpdateAsync(entity);
        return Map(await _repo.GetByIdAsync(id) ?? entity);
    }

    private async Task<string?> GetCompanyNameAsync(long companyId)
    {
        if (companyId <= 0) return null;
        try
        {
            var company = await _companyService.GetByIdAsync((int)companyId);
            return company?.CompanyName;
        }
        catch
        {
            return null;
        }
    }

    public async Task DeleteAsync(long id)
    {
        var existing = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Sales invoice '{id}' was not found.");
        await _repo.DeleteAsync(id);
    }

    private async Task<SalesInvoice> BuildAsync(
        long companyId, long userId, long branchId, long warehouseId, long customerId,
        string invoiceNumber, string invoiceDate, string sourceType, int? salesTypeId, long? priceListId,
        string? referenceNo, string? referenceDate, long? paymentTypeID, long? paymentMethodID, string? remarks,
        List<CreateSalesItemInput> items, string statusCode)
    {
        var products = (await _productService.GetPagedAsync(companyId, 1, 10000, "")).Items.ToDictionary(p => p.Id);
        var units = (await _unitService.GetAllAsync(companyId, true)).ToDictionary(u => u.Id);
        var partners = (await _businessPartnerService.GetAllAsync(true)).ToDictionary(p => p.Id);

        if (!products.Any())
            throw new DomainException("No products found for the company.");
        var customer = partners.GetValueOrDefault(customerId);

        var statusId = await _statusRepo.GetIdByCodeAsync(statusCode);
        if (statusId == 0) statusId = await _statusRepo.GetIdByCodeAsync("POSTED");

        var entity = new SalesInvoice
        {
            CompanyId = companyId,
            BranchId = branchId,
            WarehouseId = warehouseId,
            CustomerId = customerId,
            CustomerNameSnapshot = customer?.PartnerName,
            SalesInvoiceNo = invoiceNumber?.Trim() ?? string.Empty,
            InvoiceDate = DateTime.Parse(invoiceDate),
            SourceType = string.IsNullOrWhiteSpace(sourceType) ? "SALES" : sourceType,
            SalesTypeId = salesTypeId,
            PriceListId = priceListId,
            ReferenceNo = referenceNo?.Trim(),
            ReferenceDate = string.IsNullOrWhiteSpace(referenceDate) ? null : DateTime.Parse(referenceDate),
            PaymentTypeID = paymentTypeID,
            PaymentMethodID = paymentMethodID,
            StatusID = statusId == 0 ? 1 : statusId,
            InvoiceStatus = "POSTED",
            Remarks = remarks?.Trim(),
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow,
        };

        decimal gross = 0, disc = 0, taxable = 0, cgst = 0, sgst = 0, igst = 0, cess = 0;
        foreach (var input in items)
        {
            var product = products.GetValueOrDefault(input.ProductId)
                ?? throw new DomainException($"Product '{input.ProductId}' was not found.");
            units.TryGetValue(input.UnitID, out var unit);

            var lineBase = input.Quantity * input.Rate;
            var discountAmount = Math.Round(lineBase * (input.DiscountPercentage / 100m), 2);
            var taxableValue = Math.Round(lineBase - discountAmount, 2);
            var cgstAmt = Math.Round(taxableValue * (input.CGSTPercent / 100m), 2);
            var sgstAmt = Math.Round(taxableValue * (input.SGSTPercent / 100m), 2);
            var igstAmt = Math.Round(taxableValue * (input.IGSTPercent / 100m), 2);
            var cessAmt = Math.Round(taxableValue * (input.CESSPercent / 100m), 2);
            var lineTotal = Math.Round(taxableValue + cgstAmt + sgstAmt + igstAmt + cessAmt, 2);

            var item = new SalesInvoiceItem
            {
                ProductId = input.ProductId,
                ProductCodeSnapshot = product.ProductCode,
                ProductNameSnapshot = product.ProductName,
                UnitID = input.UnitID,
                UnitNameSnapshot = unit?.UnitName,
                BatchId = input.BatchId,
                HSNID = null,
                HSNCodeSnapshot = null,
                BarcodeSnapshot = product.Barcode,
                Quantity = input.Quantity,
                FreeQuantity = input.FreeQuantity,
                Rate = input.Rate,
                GrossAmount = lineBase,
                DiscountPercentage = input.DiscountPercentage,
                DiscountAmount = discountAmount,
                TaxableAmount = taxableValue,
                GSTPercent = input.GSTPercent,
                CGSTPercent = input.CGSTPercent,
                SGSTPercent = input.SGSTPercent,
                IGSTPercent = input.IGSTPercent,
                CESSPercent = input.CESSPercent,
                CGSTAmount = cgstAmt,
                SGSTAmount = sgstAmt,
                IGSTAmount = igstAmt,
                CESSAmount = cessAmt,
                LineTotal = lineTotal,
                Remarks = input.Remarks,
            };
            entity.Items.Add(item);

            gross += lineBase;
            disc += discountAmount;
            taxable += taxableValue;
            cgst += cgstAmt;
            sgst += sgstAmt;
            igst += igstAmt;
            cess += cessAmt;
        }

        entity.TotalGrossAmount = Math.Round(gross, 2);
        entity.TotalDiscountAmount = Math.Round(disc, 2);
        entity.TotalTaxableAmount = Math.Round(taxable, 2);
        entity.TotalCGSTAmount = Math.Round(cgst, 2);
        entity.TotalSGSTAmount = Math.Round(sgst, 2);
        entity.TotalIGSTAmount = Math.Round(igst, 2);
        entity.TotalCESSAmount = Math.Round(cess, 2);
        entity.TotalRoundOff = 0;
        entity.GrandTotal = Math.Round(taxable + cgst + sgst + igst + cess, 2);
        entity.PaidAmount = 0;
        entity.BalanceAmount = entity.GrandTotal;

        return entity;
    }

    private static SalesInvoiceDto MapHeader(SalesInvoice e) => new()
    {
        SalesInvoiceId = e.SalesInvoiceId,
        CompanyId = e.CompanyId,
        CompanyNameSnapshot = e.CompanyNameSnapshot,
        BranchId = e.BranchId,
        WarehouseId = e.WarehouseId,
        CustomerId = e.CustomerId,
        CustomerNameSnapshot = e.CustomerNameSnapshot,
        SalesInvoiceNo = e.SalesInvoiceNo,
        InvoiceDate = e.InvoiceDate,
        SourceType = e.SourceType,
        SalesTypeId = e.SalesTypeId,
        PriceListId = e.PriceListId,
        ReferenceNo = e.ReferenceNo,
        ReferenceDate = e.ReferenceDate,
        TotalGrossAmount = e.TotalGrossAmount,
        TotalDiscountAmount = e.TotalDiscountAmount,
        TotalTaxableAmount = e.TotalTaxableAmount,
        TotalCGSTAmount = e.TotalCGSTAmount,
        TotalSGSTAmount = e.TotalSGSTAmount,
        TotalIGSTAmount = e.TotalIGSTAmount,
        TotalCESSAmount = e.TotalCESSAmount,
        TotalRoundOff = e.TotalRoundOff,
        GrandTotal = e.GrandTotal,
        PaidAmount = e.PaidAmount,
        BalanceAmount = e.BalanceAmount,
        PaymentTypeID = e.PaymentTypeID,
        PaymentMethodID = e.PaymentMethodID,
        StatusID = e.StatusID,
        InvoiceStatus = e.InvoiceStatus,
        Remarks = e.Remarks,
        IsActive = e.IsActive,
        CreatedByUserID = e.CreatedByUserID,
        CreatedAt = e.CreatedAt,
    };

    private static SalesInvoiceDto Map(SalesInvoice e)
    {
        var dto = MapHeader(e);
        dto.Items = e.Items.Select(MapItem).ToList();
        return dto;
    }

    private static SalesItemDto MapItem(SalesInvoiceItem i) => new()
    {
        SalesInvoiceItemId = i.SalesInvoiceItemId,
        SalesInvoiceId = i.SalesInvoiceId,
        ProductId = i.ProductId,
        ProductCodeSnapshot = i.ProductCodeSnapshot,
        ProductNameSnapshot = i.ProductNameSnapshot,
        UnitID = i.UnitID,
        UnitNameSnapshot = i.UnitNameSnapshot,
        BatchId = i.BatchId,
        HSNID = i.HSNID,
        HSNCodeSnapshot = i.HSNCodeSnapshot,
        BarcodeSnapshot = i.BarcodeSnapshot,
        Quantity = i.Quantity,
        FreeQuantity = i.FreeQuantity,
        Rate = i.Rate,
        GrossAmount = i.GrossAmount,
        DiscountPercentage = i.DiscountPercentage,
        DiscountAmount = i.DiscountAmount,
        TaxableAmount = i.TaxableAmount,
        GSTPercent = i.GSTPercent,
        CGSTPercent = i.CGSTPercent,
        SGSTPercent = i.SGSTPercent,
        IGSTPercent = i.IGSTPercent,
        CESSPercent = i.CESSPercent,
        CGSTAmount = i.CGSTAmount,
        SGSTAmount = i.SGSTAmount,
        IGSTAmount = i.IGSTAmount,
        CESSAmount = i.CESSAmount,
        LineTotal = i.LineTotal,
        Remarks = i.Remarks,
    };
}
