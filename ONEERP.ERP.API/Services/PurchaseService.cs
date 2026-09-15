using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface IPurchaseService
{
    Task<PaginatedResult<PurchaseDto>> GetPagedAsync(long companyId, int page, int size, string search);
    Task<PurchaseDto?> GetByIdAsync(long id);
    Task<List<PurchaseItemDto>> GetItemsAsync(long id);
    Task<List<PaymentAllocationDto>> GetAllocationsAsync(long id);
    Task<List<StockTransaction>> GetStockTransactionsAsync(long id);
    Task<string> GetNextPurchaseNoAsync(long companyId);
    Task<PurchaseDto> CreateAsync(long companyId, long userId, CreatePurchaseRequest request);
    Task<PurchaseDto> UpdateAsync(long id, long userId, UpdatePurchaseRequest request);
    Task DeleteAsync(long id);
    Task<PurchaseDto> CancelAsync(long id, long userId, string reason);
    Task<DeleteCheckDto> GetDeleteCheckAsync(long id);
}

public class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _repo;
    private readonly IStatusRepository _statusRepo;
    private readonly IProductService _productService;
    private readonly IProductUnitService _unitService;
    private readonly IBusinessPartnerService _businessPartnerService;
    private readonly ICompanyService _companyService;
    private readonly IPaymentTypeService _paymentTypeService;

    public PurchaseService(
        IPurchaseRepository repo,
        IStatusRepository statusRepo,
        IProductService productService,
        IProductUnitService unitService,
        IBusinessPartnerService businessPartnerService,
        ICompanyService companyService,
        IPaymentTypeService paymentTypeService)
    {
        _repo = repo;
        _statusRepo = statusRepo;
        _productService = productService;
        _unitService = unitService;
        _businessPartnerService = businessPartnerService;
        _companyService = companyService;
        _paymentTypeService = paymentTypeService;
    }

    public async Task<PaginatedResult<PurchaseDto>> GetPagedAsync(long companyId, int page, int size, string search)
    {
        var (items, total) = await _repo.GetPagedAsync(companyId, page, size, search);
        return new PaginatedResult<PurchaseDto>
        {
            Items = items.Select(MapHeader).ToList(),
            TotalCount = total,
            PageNumber = page,
            PageSize = size,
        };
    }

    public async Task<PurchaseDto?> GetByIdAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e == null ? null : Map(e);
    }

    public async Task<List<PurchaseItemDto>> GetItemsAsync(long id)
        => (await _repo.GetItemsAsync(id)).Select(MapItem).ToList();

    public Task<List<PaymentAllocationDto>> GetAllocationsAsync(long id) => _repo.GetAllocationsAsync(id);

    public Task<List<StockTransaction>> GetStockTransactionsAsync(long id) => _repo.GetStockTransactionsAsync(id);

    public Task<string> GetNextPurchaseNoAsync(long companyId) => _repo.GetNextPurchaseNoAsync(companyId);

    public async Task<PurchaseDto> CreateAsync(long companyId, long userId, CreatePurchaseRequest r)
    {
        var effectiveCompanyId = r.CompanyId > 0 ? r.CompanyId : companyId;
         var (entity, payment) = await BuildAsync(effectiveCompanyId, userId, r.BranchId, r.WarehouseId, r.SupplierId,
             r.PurchaseNumber, r.PurchaseDate, r.SupplierInvoiceNumber, r.SupplierInvoiceDate,
             r.SupplierPONumber, r.ReferenceNumber, r.CurrencyId, r.PurchaseTypeId,
             r.AccountingYearId, r.TaxId, r.IsGSTInclusive,
             r.PaymentTypeID, r.PaymentMethodID, r.Remarks,
             r.Items, "POSTED", r.Payment, r.PaidAmount, r.BalanceAmount);
        entity.CompanyNameSnapshot = await GetCompanyNameAsync(effectiveCompanyId);
        await _repo.InsertAsync(entity, payment);
        return Map(await _repo.GetByIdAsync(entity.PurchaseId) ?? entity);
    }

    public async Task<DeleteCheckDto> GetDeleteCheckAsync(long id)
    {
        var existing = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Purchase '{id}' was not found.");
        var status = await _repo.GetStatusCodeAsync(id) ?? string.Empty;
        var payments = await _repo.CountPaymentsAsync(id);
        var stock = await _repo.CountStockTransactionsAsync(id);
        var returns = await _repo.CountReturnsAsync(id);
        var reasons = new List<string>();
        if (string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            reasons.Add("Purchase is already cancelled.");
        if (payments > 0)
            reasons.Add($"Cannot delete: {payments} payment allocation(s) reference this purchase.");
        if (returns > 0)
            reasons.Add($"Cannot delete: {returns} purchase return(s) reference this purchase.");
        return new DeleteCheckDto
        {
            Allowed = reasons.Count == 0,
            Reasons = reasons,
            StatusCode = status,
            PaymentCount = payments,
            StockTransactionCount = stock,
            ReturnCount = returns,
        };
    }

    public async Task<PurchaseDto> CancelAsync(long id, long userId, string reason)
    {
        var existing = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Purchase '{id}' was not found.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Cancellation reason is required.");
        var status = await _repo.GetStatusCodeAsync(id) ?? string.Empty;
        if (string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Purchase is already cancelled.");
        if (await _repo.CountPaymentsAsync(id) > 0)
            throw new DomainException("Cannot cancel: payments are allocated to this purchase.");
        if (await _repo.CountReturnsAsync(id) > 0)
            throw new DomainException("Cannot cancel: purchase returns reference this purchase.");
        await _repo.CancelAsync(id, userId, reason.Trim());
        return Map(await _repo.GetByIdAsync(id) ?? existing);
    }

    public async Task<PurchaseDto> UpdateAsync(long id, long userId, UpdatePurchaseRequest r)
    {
        var existing = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Purchase '{id}' was not found.");
        var status = await _repo.GetStatusCodeAsync(id) ?? string.Empty;
        if (string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Cancelled purchases cannot be edited.");
        if (await _repo.CountPaymentsAsync(id) > 0)
            throw new DomainException("Cannot edit: payments are allocated to this purchase.");
        if (await _repo.CountReturnsAsync(id) > 0)
            throw new DomainException("Cannot edit: purchase returns reference this purchase.");
        var effectiveCompanyId = r.CompanyId > 0 ? r.CompanyId : existing.CompanyId;
         var (entity, _) = await BuildAsync(effectiveCompanyId, userId, r.BranchId, r.WarehouseId, r.SupplierId,
             r.PurchaseNumber, r.PurchaseDate, r.SupplierInvoiceNumber, r.SupplierInvoiceDate,
             r.SupplierPONumber, r.ReferenceNumber, r.CurrencyId, r.PurchaseTypeId,
             r.AccountingYearId, r.TaxId, r.IsGSTInclusive,
             r.PaymentTypeID, r.PaymentMethodID, r.Remarks,
             r.Items, "POSTED", null, r.PaidAmount, r.BalanceAmount);
        entity.PurchaseId = id;
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
        var existing = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Purchase '{id}' was not found.");
        var check = await GetDeleteCheckAsync(id);
        if (!check.Allowed)
            throw new DomainException(string.Join(" ", check.Reasons));
        await _repo.DeleteAsync(id);
    }

     private async Task<(Purchase Entity, PurchasePaymentInput? Payment)> BuildAsync(
         long companyId, long userId, long branchId, long warehouseId, long supplierId,
         string purchaseNumber, string purchaseDate, string? supplierInvoiceNumber, string? supplierInvoiceDate,
         string? supplierPoNumber, string? referenceNumber, long? currencyId, long? purchaseTypeId,
         long? accountingYearId, long? taxId, bool? isGstInclusive,
         long? paymentTypeID, long? paymentMethodID, string? remarks,
         List<CreatePurchaseItemInput> items, string statusCode,
         CreatePurchasePaymentInput? payment = null, decimal paidAmount = 0, decimal balanceAmount = 0)
    {
        var products = (await _productService.GetPagedAsync(companyId, 1, 10000, "")).Items.ToDictionary(p => p.Id);
        var units = (await _unitService.GetAllAsync(companyId, true)).ToDictionary(u => u.Id);
        var partners = (await _businessPartnerService.GetAllAsync(true)).ToDictionary(p => p.Id);

        if (!products.Any())
            throw new DomainException("No products found for the company.");
        if (string.IsNullOrWhiteSpace(supplierInvoiceNumber))
            throw new DomainException("Supplier Invoice Number is required.");
        if (string.IsNullOrWhiteSpace(supplierInvoiceDate))
            throw new DomainException("Supplier Invoice Date is required.");
        if (paymentTypeID == null || paymentTypeID <= 0)
            throw new DomainException("Payment Type is required.");
        // Cash / partial pay needs a method; pure credit (paid 0, no payment obj) does not.
        var hasPay = paidAmount > 0 || (payment != null && payment.Amount > 0);
        if (hasPay && (paymentMethodID == null || paymentMethodID <= 0)
            && (payment?.PaymentMethodID == null || payment?.PaymentMethodID <= 0))
            throw new DomainException("Payment Method is required.");
        // CASH only: paid amount must be > 0.
        var paymentTypeCode = string.Empty;
        try
        {
            var pt = await _paymentTypeService.GetByIdAsync(paymentTypeID.Value);
            paymentTypeCode = (pt.Code ?? string.Empty).Trim().ToUpperInvariant();
        }
        catch (NotFoundException)
        {
            throw new DomainException("Payment Type is invalid.");
        }
        if (paymentTypeCode == "CASH")
        {
            var effectivePaid = (payment != null && payment.Amount > 0) ? payment.Amount : paidAmount;
            if (!(effectivePaid > 0))
                throw new DomainException("Paid Amount must be greater than 0 for Cash payment.");
        }
        var supplier = partners.GetValueOrDefault(supplierId);

        var statusId = await _statusRepo.GetIdByCodeAsync(statusCode);
        if (statusId == 0) statusId = await _statusRepo.GetIdByCodeAsync("POSTED");

        var entity = new Purchase
        {
            CompanyId = companyId,
            BranchId = branchId,
            WarehouseId = warehouseId,
            SupplierId = supplierId,
            SupplierNameSnapshot = supplier?.PartnerName,
            PurchaseNumber = purchaseNumber?.Trim() ?? string.Empty,
            PurchaseDate = DateTime.Parse(purchaseDate),
            SupplierInvoiceNumber = supplierInvoiceNumber?.Trim(),
            SupplierInvoiceDate = string.IsNullOrWhiteSpace(supplierInvoiceDate) ? null : DateTime.Parse(supplierInvoiceDate),
            SupplierPONumber = supplierPoNumber?.Trim(),
            ReferenceNumber = referenceNumber?.Trim(),
            CurrencyId = currencyId,
            PurchaseTypeId = purchaseTypeId,
            AccountingYearId = accountingYearId,
            TaxId = taxId,
            IsGSTInclusive = isGstInclusive,
            PaymentTypeID = paymentTypeID,
            PaymentMethodID = paymentMethodID,
            StatusID = statusId == 0 ? 1 : statusId,
            Remarks = remarks?.Trim(),
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow,
        };

        decimal gross = 0, disc = 0, taxable = 0, tax = 0, cess = 0;
        if (items == null || items.Count == 0)
            throw new DomainException("Add at least one item.");
        foreach (var input in items)
        {
            if (input.ProductId <= 0)
                throw new DomainException("Product is required on all rows.");
            if (!(input.Quantity > 0))
                throw new DomainException($"Quantity must be greater than 0 for product '{input.ProductId}'.");
            if (!(input.PurchaseRate > 0))
                throw new DomainException($"P.Rate must be greater than 0 for product '{input.ProductId}'.");
            var product = products.GetValueOrDefault(input.ProductId)
                ?? throw new DomainException($"Product '{input.ProductId}' was not found.");
            units.TryGetValue(input.UnitID, out var unit);

            var lineBase = input.Quantity * input.PurchaseRate;
            var discountAmount = Math.Round(lineBase * (input.DiscountPercentage / 100m), 2);
            decimal taxableValue, gst, cgst, sgst, igst, cessAmt, lineTotal;

            var isItemInclusive = input.IsGSTInclusive ?? isGstInclusive ?? false;
            if (isItemInclusive)
            {
                // GST Inclusive: PurchaseRate includes GST
                taxableValue = Math.Round(lineBase / (1 + (input.GSTRate / 100m)), 2);
                gst = Math.Round(lineBase - taxableValue, 2);
                cgst = Math.Round(taxableValue * (input.CGSTRate / 100m), 2);
                sgst = Math.Round(taxableValue * (input.SGSTRate / 100m), 2);
                igst = Math.Round(taxableValue * (input.IGSTRate / 100m), 2);
                cessAmt = Math.Round(taxableValue * (input.CESSRate / 100m), 2);
                lineTotal = Math.Round(lineBase - discountAmount, 2);
            }
            else
            {
                // GST Exclusive: PurchaseRate is the base taxable rate
                taxableValue = Math.Round(lineBase - discountAmount, 2);
                gst = Math.Round(taxableValue * (input.GSTRate / 100m), 2);
                cgst = Math.Round(taxableValue * (input.CGSTRate / 100m), 2);
                sgst = Math.Round(taxableValue * (input.SGSTRate / 100m), 2);
                igst = Math.Round(taxableValue * (input.IGSTRate / 100m), 2);
                cessAmt = Math.Round(taxableValue * (input.CESSRate / 100m), 2);
                lineTotal = Math.Round(taxableValue + gst + cessAmt, 2);
            }

            var item = new PurchaseItem
            {
                ProductId = input.ProductId,
                ProductCodeSnapshot = product.ProductCode,
                ProductNameSnapshot = product.ProductName,
                BrandID = input.BrandID ?? product.BrandId,
                CategoryID = input.CategoryID ?? product.CategoryId,
                SubCategoryID = input.SubCategoryID ?? product.SubCategoryId,
                UnitID = input.UnitID,
                UnitNameSnapshot = unit?.UnitName,
                HSNID = input.HSNID,
                HSNCodeSnapshot = input.HSNCode,
                BarcodeSnapshot = product.Barcode,
                Quantity = input.Quantity,
                FreeQuantity = input.FreeQuantity,
                PurchaseRate = input.PurchaseRate,
                MRP = input.MRP ?? product.MRP,
                RetailPrice = input.RetailPrice,
                WholesalePrice = input.WholesalePrice,
                SaleRate = input.SaleRate,
                DiscountPercentage = input.DiscountPercentage,
                DiscountAmount = discountAmount,
                IsGSTInclusive = isItemInclusive,
                TaxableValue = taxableValue,
                GSTRate = input.GSTRate,
                GSTAmount = gst,
                CGSTRate = input.CGSTRate,
                CGSTAmount = cgst,
                SGSTRate = input.SGSTRate,
                SGSTAmount = sgst,
                IGSTRate = input.IGSTRate,
                IGSTAmount = igst,
                CESSRate = input.CESSRate,
                CESSAmount = cessAmt,
                LineTotal = lineTotal,
                ManufacturingDate = input.ManufacturingDate,
                ExpiryDate = input.ExpiryDate,
                Remarks = input.Remarks,
                TaxId = input.TaxId,
                CessId = input.CessId,
                OrderedQuantity = input.OrderedQuantity,
                ReceivedQuantity = input.ReceivedQuantity,
                ReturnedQuantity = input.ReturnedQuantity,
                RemainingQuantity = input.RemainingQuantity,
                PurchaseOrderId = input.PurchaseOrderId,
                PurchaseOrderItemId = input.PurchaseOrderItemId,
                GRNId = input.GRNId,
                BatchNumber = input.BatchNumber?.Trim(),
                SerialNumber = input.SerialNumber?.Trim(),
            };
            entity.Items.Add(item);

            gross += lineBase;
            disc += discountAmount;
            taxable += taxableValue;
            tax += gst;
            cess += cessAmt;
        }

        entity.TotalGrossAmount = Math.Round(gross, 2);
        entity.TotalDiscountAmount = Math.Round(disc, 2);
        entity.TotalTaxableAmount = Math.Round(taxable, 2);
        entity.TotalTaxAmount = Math.Round(tax, 2);
        entity.TotalCessAmount = Math.Round(cess, 2);
        entity.TotalRoundOff = 0;
        entity.GrandTotal = Math.Round(taxable + tax + cess, 2);
         entity.PaidAmount = Math.Round(paidAmount, 2);
         entity.BalanceAmount = Math.Round(entity.GrandTotal - paidAmount, 2);

        PurchasePaymentInput? paymentInput = null;
        if (payment != null && payment.Amount > 0)
        {
            paymentInput = new PurchasePaymentInput
            {
                Amount = payment.Amount,
                PaymentTypeID = payment.PaymentTypeID ?? paymentTypeID,
                PaymentMethodID = payment.PaymentMethodID ?? paymentMethodID,
                ReferenceNo = payment.ReferenceNo,
                Remarks = payment.Remarks,
            };
            entity.PaidAmount = payment.Amount;
            entity.BalanceAmount = Math.Round(entity.GrandTotal - payment.Amount, 2);
        }

        return (entity, paymentInput);
    }

    private static PurchaseDto MapHeader(Purchase e) => new()
    {
        PurchaseId = e.PurchaseId,
        CompanyId = e.CompanyId,
        BranchId = e.BranchId,
        WarehouseId = e.WarehouseId,
        SupplierId = e.SupplierId,
        SupplierNameSnapshot = e.SupplierNameSnapshot,
        PurchaseNumber = e.PurchaseNumber,
        PurchaseDate = e.PurchaseDate,
        SupplierInvoiceNumber = e.SupplierInvoiceNumber,
        SupplierInvoiceDate = e.SupplierInvoiceDate,
        TotalGrossAmount = e.TotalGrossAmount,
        TotalDiscountAmount = e.TotalDiscountAmount,
        TotalTaxableAmount = e.TotalTaxableAmount,
        TotalTaxAmount = e.TotalTaxAmount,
        TotalCessAmount = e.TotalCessAmount,
        TotalRoundOff = e.TotalRoundOff,
        GrandTotal = e.GrandTotal,
        PaidAmount = e.PaidAmount,
        BalanceAmount = e.BalanceAmount,
        PaymentTypeID = e.PaymentTypeID,
        PaymentMethodID = e.PaymentMethodID,
        StatusID = e.StatusID,
        Remarks = e.Remarks,
        IsActive = e.IsActive,
        CreatedByUserID = e.CreatedByUserID,
        CreatedAt = e.CreatedAt,
        SupplierPONumber = e.SupplierPONumber,
        ReferenceNumber = e.ReferenceNumber,
        CurrencyId = e.CurrencyId,
        PurchaseTypeId = e.PurchaseTypeId,
        AccountingYearId = e.AccountingYearId,
        TaxId = e.TaxId,
        IsGSTInclusive = e.IsGSTInclusive,
        CancelledByUserID = e.CancelledByUserID,
        CancelledAt = e.CancelledAt,
        CancellationReason = e.CancellationReason,
    };

    private static PurchaseDto Map(Purchase e)
    {
        var dto = MapHeader(e);
        dto.Items = e.Items.Select(MapItem).ToList();
        return dto;
    }

    private static PurchaseItemDto MapItem(PurchaseItem i) => new()
    {
        PurchaseItemId = i.PurchaseItemId,
        PurchaseId = i.PurchaseId,
        ProductId = i.ProductId,
        ProductCodeSnapshot = i.ProductCodeSnapshot,
        ProductNameSnapshot = i.ProductNameSnapshot,
        BrandID = i.BrandID,
        CategoryID = i.CategoryID,
        SubCategoryID = i.SubCategoryID,
        UnitID = i.UnitID,
        UnitNameSnapshot = i.UnitNameSnapshot,
        HSNID = i.HSNID,
        HSNCodeSnapshot = i.HSNCodeSnapshot,
        Quantity = i.Quantity,
        FreeQuantity = i.FreeQuantity,
        PurchaseRate = i.PurchaseRate,
        MRP = i.MRP,
        RetailPrice = i.RetailPrice,
        WholesalePrice = i.WholesalePrice,
        SaleRate = i.SaleRate,
        DiscountPercentage = i.DiscountPercentage,
        DiscountAmount = i.DiscountAmount,
        IsGSTInclusive = i.IsGSTInclusive,
        TaxableValue = i.TaxableValue,
        GSTRate = i.GSTRate,
        GSTAmount = i.GSTAmount,
        CGSTRate = i.CGSTRate,
        CGSTAmount = i.CGSTAmount,
        SGSTRate = i.SGSTRate,
        SGSTAmount = i.SGSTAmount,
        IGSTRate = i.IGSTRate,
        IGSTAmount = i.IGSTAmount,
        CESSRate = i.CESSRate,
        CESSAmount = i.CESSAmount,
        LineTotal = i.LineTotal,
        ManufacturingDate = i.ManufacturingDate,
        ExpiryDate = i.ExpiryDate,
        Remarks = i.Remarks,
        TaxId = i.TaxId,
        CessId = i.CessId,
        OrderedQuantity = i.OrderedQuantity,
        ReceivedQuantity = i.ReceivedQuantity,
        ReturnedQuantity = i.ReturnedQuantity,
        RemainingQuantity = i.RemainingQuantity,
        PurchaseOrderId = i.PurchaseOrderId,
        PurchaseOrderItemId = i.PurchaseOrderItemId,
        GRNId = i.GRNId,
        BatchNumber = i.BatchNumber,
        SerialNumber = i.SerialNumber,
    };
}
