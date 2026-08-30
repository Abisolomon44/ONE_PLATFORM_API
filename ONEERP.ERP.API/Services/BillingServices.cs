using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface IStockService
{
    Task<PaginatedResult<StockDto>> GetPagedAsync(long companyId, int page, int size, string search, long? warehouseId, long? productId);
    Task<PaginatedResult<StockTransactionDto>> GetTransactionsAsync(long companyId, int page, int size, long? productId, long? warehouseId);
}

public class StockService : IStockService
{
    private readonly IStockRepository _repo;

    public StockService(IStockRepository repo) => _repo = repo;

    public async Task<PaginatedResult<StockDto>> GetPagedAsync(long companyId, int page, int size, string search, long? warehouseId, long? productId)
    {
        var (items, total) = await _repo.GetPagedAsync(companyId, page, size, search, warehouseId, productId);
        return new PaginatedResult<StockDto>
        {
            Items = items.Select(e => new StockDto
            {
                StockId = e.StockId,
                CompanyId = e.CompanyId,
                BranchId = e.BranchId,
                WarehouseId = e.WarehouseId,
                ProductId = e.ProductId,
                UnitId = e.UnitId,
                Quantity = e.Quantity,
                ReservedQuantity = e.ReservedQuantity,
                AvailableQuantity = e.AvailableQuantity,
                AverageCost = e.AverageCost,
                LastPurchaseRate = e.LastPurchaseRate,
                UpdatedAt = e.UpdatedAt,
            }).ToList(),
            TotalCount = total,
            PageNumber = page,
            PageSize = size,
        };
    }

    public async Task<PaginatedResult<StockTransactionDto>> GetTransactionsAsync(long companyId, int page, int size, long? productId, long? warehouseId)
    {
        var items = await _repo.GetTransactionsAsync(companyId, page, size, productId, warehouseId);
        return new PaginatedResult<StockTransactionDto>
        {
            Items = items.Select(e => new StockTransactionDto
            {
                StockTransactionId = e.StockTransactionId,
                CompanyId = e.CompanyId,
                BranchId = e.BranchId,
                WarehouseId = e.WarehouseId,
                ProductId = e.ProductId,
                UnitId = e.UnitId,
                TransactionType = e.TransactionType,
                ReferenceType = e.ReferenceType,
                ReferenceId = e.ReferenceId,
                QuantityIn = e.QuantityIn,
                QuantityOut = e.QuantityOut,
                Rate = e.Rate,
                BalanceQuantity = e.BalanceQuantity,
                TransactionDate = e.TransactionDate,
                Remarks = e.Remarks,
            }).ToList(),
            TotalCount = items.Count,
            PageNumber = page,
            PageSize = size,
        };
    }
}

public interface IPurchaseReturnService
{
    Task<PaginatedResult<PurchaseReturnDto>> GetPagedAsync(long companyId, int page, int size, string search);
    Task<PurchaseReturnDto?> GetByIdAsync(long id);
    Task<string> GetNextReturnNoAsync(long companyId);
    Task<PurchaseReturnDto> CreateAsync(long companyId, long userId, CreatePurchaseReturnRequest request);
}

public class PurchaseReturnService : IPurchaseReturnService
{
    private readonly IPurchaseReturnRepository _repo;
    private readonly IPurchaseRepository _purchaseRepo;
    private readonly IStatusRepository _statusRepo;
    private readonly IProductService _productService;
    private readonly IProductUnitService _unitService;
    private readonly IBusinessPartnerService _businessPartnerService;

    public PurchaseReturnService(
        IPurchaseReturnRepository repo,
        IPurchaseRepository purchaseRepo,
        IStatusRepository statusRepo,
        IProductService productService,
        IProductUnitService unitService,
        IBusinessPartnerService businessPartnerService)
    {
        _repo = repo;
        _purchaseRepo = purchaseRepo;
        _statusRepo = statusRepo;
        _productService = productService;
        _unitService = unitService;
        _businessPartnerService = businessPartnerService;
    }

    public async Task<PaginatedResult<PurchaseReturnDto>> GetPagedAsync(long companyId, int page, int size, string search)
    {
        var (items, total) = await _repo.GetPagedAsync(companyId, page, size, search);
        return new PaginatedResult<PurchaseReturnDto>
        {
            Items = items.Select(MapHeader).ToList(),
            TotalCount = total,
            PageNumber = page,
            PageSize = size,
        };
    }

    public async Task<PurchaseReturnDto?> GetByIdAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e == null ? null : Map(e);
    }

    public Task<string> GetNextReturnNoAsync(long companyId) => _repo.GetNextReturnNoAsync(companyId);

    public async Task<PurchaseReturnDto> CreateAsync(long companyId, long userId, CreatePurchaseReturnRequest r)
    {
        var purchase = await _purchaseRepo.GetByIdAsync(r.PurchaseId)
            ?? throw new DomainException($"Purchase '{r.PurchaseId}' was not found.");

        var products = (await _productService.GetPagedAsync(companyId, 1, 10000, "")).Items.ToDictionary(p => p.Id);
        var units = (await _unitService.GetAllAsync(companyId, true)).ToDictionary(u => u.Id);
        var partners = (await _businessPartnerService.GetAllAsync(true)).ToDictionary(p => p.Id);
        var supplier = partners.GetValueOrDefault(purchase.SupplierId);
        var statusId = await _statusRepo.GetIdByCodeAsync("RETURNED");
        if (statusId == 0) statusId = 1;

        var entity = new PurchaseReturn
        {
            PurchaseId = purchase.PurchaseId,
            CompanyId = companyId,
            BranchId = purchase.BranchId,
            WarehouseId = purchase.WarehouseId,
            SupplierId = purchase.SupplierId,
            SupplierNameSnapshot = supplier?.PartnerName,
            ReturnNumber = await _repo.GetNextReturnNoAsync(companyId),
            ReturnDate = DateTime.Parse(r.ReturnDate),
            StatusID = statusId,
            Reason = r.Reason,
            Remarks = r.Remarks,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow,
        };

        decimal gross = 0, disc = 0, taxable = 0, tax = 0, cess = 0;
        foreach (var input in r.Items)
        {
            var product = products.GetValueOrDefault(input.ProductId)
                ?? throw new DomainException($"Product '{input.ProductId}' was not found.");
            units.TryGetValue(input.UnitId, out var unit);

            var item = new PurchaseReturnItem
            {
                PurchaseItemId = input.PurchaseItemId,
                ProductId = input.ProductId,
                ProductCodeSnapshot = product.ProductCode,
                ProductNameSnapshot = product.ProductName,
                UnitId = input.UnitId,
                UnitNameSnapshot = unit?.UnitName,
                ReturnQuantity = input.ReturnQuantity,
                PurchaseRate = input.PurchaseRate,
                DiscountAmount = input.DiscountAmount,
                TaxableValue = input.TaxableValue,
                GSTRate = input.GSTRate,
                GSTAmount = input.GSTAmount,
                CGSTRate = input.CGSTRate,
                CGSTAmount = input.CGSTAmount,
                SGSTRate = input.SGSTRate,
                SGSTAmount = input.SGSTAmount,
                IGSTRate = input.IGSTRate,
                IGSTAmount = input.IGSTAmount,
                CESSRate = input.CESSRate,
                CESSAmount = input.CESSAmount,
                LineTotal = Math.Round(input.TaxableValue + input.GSTAmount + input.CESSAmount, 2),
            };
            entity.Items.Add(item);

            gross += input.ReturnQuantity * input.PurchaseRate;
            disc += input.DiscountAmount;
            taxable += input.TaxableValue;
            tax += input.GSTAmount;
            cess += input.CESSAmount;
        }

        entity.TotalGrossAmount = Math.Round(gross, 2);
        entity.TotalDiscountAmount = Math.Round(disc, 2);
        entity.TotalTaxableAmount = Math.Round(taxable, 2);
        entity.TotalTaxAmount = Math.Round(tax, 2);
        entity.TotalCessAmount = Math.Round(cess, 2);
        entity.TotalRoundOff = 0;
        entity.GrandTotal = Math.Round(taxable + tax + cess, 2);

        await _repo.InsertAsync(entity);
        return Map(await _repo.GetByIdAsync(entity.PurchaseReturnId) ?? entity);
    }

    private static PurchaseReturnDto MapHeader(PurchaseReturn e) => new()
    {
        PurchaseReturnId = e.PurchaseReturnId,
        PurchaseId = e.PurchaseId,
        CompanyId = e.CompanyId,
        BranchId = e.BranchId,
        WarehouseId = e.WarehouseId,
        SupplierId = e.SupplierId,
        SupplierNameSnapshot = e.SupplierNameSnapshot,
        ReturnNumber = e.ReturnNumber,
        ReturnDate = e.ReturnDate,
        TotalGrossAmount = e.TotalGrossAmount,
        TotalDiscountAmount = e.TotalDiscountAmount,
        TotalTaxableAmount = e.TotalTaxableAmount,
        TotalTaxAmount = e.TotalTaxAmount,
        TotalCessAmount = e.TotalCessAmount,
        TotalRoundOff = e.TotalRoundOff,
        GrandTotal = e.GrandTotal,
        StatusID = e.StatusID,
        Reason = e.Reason,
        Remarks = e.Remarks,
    };

    private static PurchaseReturnDto Map(PurchaseReturn e)
    {
        var dto = MapHeader(e);
        dto.Items = e.Items.Select(i => new PurchaseReturnItemDto
        {
            PurchaseReturnItemId = i.PurchaseReturnItemId,
            PurchaseReturnId = i.PurchaseReturnId,
            PurchaseItemId = i.PurchaseItemId,
            ProductId = i.ProductId,
            ProductCodeSnapshot = i.ProductCodeSnapshot,
            ProductNameSnapshot = i.ProductNameSnapshot,
            UnitId = i.UnitId,
            UnitNameSnapshot = i.UnitNameSnapshot,
            ReturnQuantity = i.ReturnQuantity,
            PurchaseRate = i.PurchaseRate,
            DiscountAmount = i.DiscountAmount,
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
        }).ToList();
        return dto;
    }
}
