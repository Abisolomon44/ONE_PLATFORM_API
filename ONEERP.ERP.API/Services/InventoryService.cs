using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface IInventoryService
{
    Task<InventoryDashboardDto> GetDashboardAsync(long companyId);
    Task PostOpeningStockAsync(long companyId, long userId, OpeningStockRequest request);

    Task<PaginatedResult<StockAdjustmentDto>> GetAdjustmentsAsync(long companyId, int page, int size);
    Task<StockAdjustmentDto> CreateAdjustmentAsync(long companyId, long userId, CreateStockAdjustmentRequest r);
    Task<StockAdjustmentDto> PostAdjustmentAsync(long companyId, long id, long userId);

    Task<PaginatedResult<StockTransferDto>> GetTransfersAsync(long companyId, int page, int size);
    Task<StockTransferDto> CreateTransferAsync(long companyId, long userId, CreateStockTransferRequest r);
    Task<StockTransferDto> PostTransferAsync(long companyId, long id, long userId);

    Task<PaginatedResult<StockCountDto>> GetCountsAsync(long companyId, int page, int size);
    Task<StockCountDto> CreateCountAsync(long companyId, long userId, CreateStockCountRequest r);
    Task<StockCountDto> PostCountAsync(long companyId, long id, long userId);

    Task<PaginatedResult<StockReconciliationRow>> GetReconciliationAsync(long companyId, long? warehouseId, long? productId, int page, int size);
    Task<(PaginatedResult<StockValuationRow> Rows, decimal TotalValue)> GetValuationAsync(long companyId, long? warehouseId, int page, int size);
    Task<PaginatedResult<LowStockRow>> GetLowStockAsync(long companyId, long? warehouseId, int page, int size);
}

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repo;
    private readonly IStockRepository _stockRepo;

    public InventoryService(IInventoryRepository repo, IStockRepository stockRepo)
    {
        _repo = repo;
        _stockRepo = stockRepo;
    }

    private static int ClampPage(int page) => Math.Max(1, page);
    private static int ClampSize(int size) => Math.Clamp(size, 1, 500);

    public Task<InventoryDashboardDto> GetDashboardAsync(long companyId) => _repo.GetDashboardAsync(companyId);

    public Task PostOpeningStockAsync(long companyId, long userId, OpeningStockRequest request)
    {
        if (request.Items.Any(i => i.WarehouseId <= 0))
            throw new DomainException("Each opening line needs a warehouse.");
        return _repo.PostOpeningStockAsync(companyId, request, userId);
    }

    // ============================================================
    // T079 — Adjustment
    // ============================================================
    public async Task<PaginatedResult<StockAdjustmentDto>> GetAdjustmentsAsync(long companyId, int page, int size)
    {
        var (items, total) = await _repo.GetAdjustmentsAsync(companyId, ClampPage(page), ClampSize(size));
        return new PaginatedResult<StockAdjustmentDto>
        {
            Items = items.Select(MapAdjustment).ToList(),
            TotalCount = total,
            PageNumber = page,
            PageSize = size,
        };
    }

    public async Task<StockAdjustmentDto> CreateAdjustmentAsync(long companyId, long userId, CreateStockAdjustmentRequest r)
    {
        ValidateDeltas(r.Items);
        var entity = new StockAdjustment
        {
            AdjustmentNumber = await _repo.GetNextNumberAsync(companyId, "ADJ", "dbo.StockAdjustment", nameof(StockAdjustment.AdjustmentNumber)),
            CompanyId = companyId,
            BranchId = r.BranchId,
            WarehouseId = r.WarehouseId,
            AdjustmentDate = DateTime.Parse(r.AdjustmentDate),
            Reason = r.Reason,
            Remarks = r.Remarks,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow,
            Items = r.Items.Select(i => new StockAdjustmentItem
            {
                ProductId = i.ProductId,
                UnitId = i.UnitId,
                QuantityDelta = i.QuantityDelta,
                Rate = i.Rate,
                Reason = i.Reason,
            }).ToList(),
        };
        entity.StockAdjustmentId = await _repo.InsertAdjustmentAsync(entity);
        return MapAdjustment((await _repo.GetAdjustmentAsync(entity.StockAdjustmentId))!);
    }

    public async Task<StockAdjustmentDto> PostAdjustmentAsync(long companyId, long id, long userId)
    {
        var existing = await _repo.GetAdjustmentAsync(id)
            ?? throw new DomainException($"Adjustment '{id}' was not found.");
        if (existing.CompanyId != companyId) throw new DomainException("Adjustment does not belong to your company.", 403);
        await _repo.PostAdjustmentAsync(id, userId);
        return MapAdjustment((await _repo.GetAdjustmentAsync(id))!);
    }

    // ============================================================
    // T080/T081 — Transfer
    // ============================================================
    public async Task<PaginatedResult<StockTransferDto>> GetTransfersAsync(long companyId, int page, int size)
    {
        var (items, total) = await _repo.GetTransfersAsync(companyId, ClampPage(page), ClampSize(size));
        return new PaginatedResult<StockTransferDto>
        {
            Items = items.Select(MapTransfer).ToList(),
            TotalCount = total,
            PageNumber = page,
            PageSize = size,
        };
    }

    public async Task<StockTransferDto> CreateTransferAsync(long companyId, long userId, CreateStockTransferRequest r)
    {
        if (r.FromWarehouseId == r.ToWarehouseId)
            throw new DomainException("Source and destination warehouses must differ.");
        if (r.Items == null || r.Items.Count == 0)
            throw new DomainException("Add at least one transfer line.");
        foreach (var i in r.Items)
            if (!(i.Quantity > 0)) throw new DomainException("Transfer quantity must be greater than 0.");

        var entity = new StockTransfer
        {
            TransferNumber = await _repo.GetNextNumberAsync(companyId, "TRF", "dbo.StockTransfer", nameof(StockTransfer.TransferNumber)),
            CompanyId = companyId,
            BranchId = r.BranchId,
            FromWarehouseId = r.FromWarehouseId,
            ToWarehouseId = r.ToWarehouseId,
            TransferDate = DateTime.Parse(r.TransferDate),
            Remarks = r.Remarks,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow,
            Items = r.Items.Select(i => new StockTransferItem
            {
                ProductId = i.ProductId,
                UnitId = i.UnitId,
                Quantity = i.Quantity,
                Rate = i.Rate,
                Remarks = i.Remarks,
            }).ToList(),
        };
        entity.StockTransferId = await _repo.InsertTransferAsync(entity);
        return MapTransfer((await _repo.GetTransferAsync(entity.StockTransferId))!);
    }

    public async Task<StockTransferDto> PostTransferAsync(long companyId, long id, long userId)
    {
        var existing = await _repo.GetTransferAsync(id)
            ?? throw new DomainException($"Transfer '{id}' was not found.");
        if (existing.CompanyId != companyId) throw new DomainException("Transfer does not belong to your company.", 403);
        await _repo.PostTransferAsync(id, userId);
        return MapTransfer((await _repo.GetTransferAsync(id))!);
    }

    // ============================================================
    // T082 — Count
    // ============================================================
    public async Task<PaginatedResult<StockCountDto>> GetCountsAsync(long companyId, int page, int size)
    {
        var (items, total) = await _repo.GetCountsAsync(companyId, ClampPage(page), ClampSize(size));
        return new PaginatedResult<StockCountDto>
        {
            Items = items.Select(MapCount).ToList(),
            TotalCount = total,
            PageNumber = page,
            PageSize = size,
        };
    }

    public async Task<StockCountDto> CreateCountAsync(long companyId, long userId, CreateStockCountRequest r)
    {
        if (r.Items == null || r.Items.Count == 0)
            throw new DomainException("Add at least one counted line.");
        foreach (var i in r.Items)
            if (i.CountedQuantity < 0) throw new DomainException("Counted quantity cannot be negative.");

        var entity = new StockCount
        {
            CountNumber = await _repo.GetNextNumberAsync(companyId, "CNT", "dbo.StockCount", nameof(StockCount.CountNumber)),
            CompanyId = companyId,
            BranchId = r.BranchId,
            WarehouseId = r.WarehouseId,
            CountDate = DateTime.Parse(r.CountDate),
            Remarks = r.Remarks,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow,
            Items = r.Items.Select(i => new StockCountItem
            {
                ProductId = i.ProductId,
                UnitId = i.UnitId,
                CountedQuantity = i.CountedQuantity,
                Rate = i.Rate,
            }).ToList(),
        };

        // Snapshot book qty at entry so the variance column is meaningful.
        foreach (var item in entity.Items)
        {
            var book = await _stockRepo.GetByKeyAsync(companyId, entity.BranchId, entity.WarehouseId, item.ProductId, item.UnitId);
            item.BookQuantity = book?.Quantity ?? 0;
        }

        entity.StockCountId = await _repo.InsertCountAsync(entity);
        return MapCount((await _repo.GetCountAsync(entity.StockCountId))!);
    }

    public async Task<StockCountDto> PostCountAsync(long companyId, long id, long userId)
    {
        var existing = await _repo.GetCountAsync(id)
            ?? throw new DomainException($"Stock count '{id}' was not found.");
        if (existing.CompanyId != companyId) throw new DomainException("Stock count does not belong to your company.", 403);
        await _repo.PostCountAsync(id, userId);
        return MapCount((await _repo.GetCountAsync(id))!);
    }

    // ============================================================
    // T083 / T084 / T085 — read models
    // ============================================================
    public async Task<PaginatedResult<StockReconciliationRow>> GetReconciliationAsync(long companyId, long? warehouseId, long? productId, int page, int size)
    {
        var (rows, total) = await _repo.GetReconciliationAsync(companyId, warehouseId, productId, ClampPage(page), ClampSize(size));
        return new PaginatedResult<StockReconciliationRow> { Items = rows, TotalCount = total, PageNumber = page, PageSize = size };
    }

    public async Task<(PaginatedResult<StockValuationRow> Rows, decimal TotalValue)> GetValuationAsync(long companyId, long? warehouseId, int page, int size)
    {
        var (rows, total, value) = await _repo.GetValuationAsync(companyId, warehouseId, ClampPage(page), ClampSize(size));
        return (new PaginatedResult<StockValuationRow> { Items = rows, TotalCount = total, PageNumber = page, PageSize = size }, value);
    }

    public async Task<PaginatedResult<LowStockRow>> GetLowStockAsync(long companyId, long? warehouseId, int page, int size)
    {
        var (rows, total) = await _repo.GetLowStockAsync(companyId, warehouseId, ClampPage(page), ClampSize(size));
        return new PaginatedResult<LowStockRow> { Items = rows, TotalCount = total, PageNumber = page, PageSize = size };
    }

    // ============================================================
    // Helpers
    // ============================================================
    private static void ValidateDeltas(List<StockAdjustmentItemInput>? items)
    {
        if (items == null || items.Count == 0)
            throw new DomainException("Add at least one adjustment line.");
        if (items.All(i => i.QuantityDelta == 0))
            throw new DomainException("At least one line must have a non-zero quantity change.");
    }

    private static StockAdjustmentDto MapAdjustment(StockAdjustment e) => new()
    {
        StockAdjustmentId = e.StockAdjustmentId,
        AdjustmentNumber = e.AdjustmentNumber,
        WarehouseId = e.WarehouseId,
        AdjustmentDate = e.AdjustmentDate,
        Reason = e.Reason,
        Remarks = e.Remarks,
        Status = e.Status,
        Items = e.Items.Select(i => new StockAdjustmentItemInput
        {
            ProductId = i.ProductId,
            UnitId = i.UnitId,
            QuantityDelta = i.QuantityDelta,
            Rate = i.Rate,
            Reason = i.Reason,
        }).ToList(),
    };

    private static StockTransferDto MapTransfer(StockTransfer e) => new()
    {
        StockTransferId = e.StockTransferId,
        TransferNumber = e.TransferNumber,
        FromWarehouseId = e.FromWarehouseId,
        ToWarehouseId = e.ToWarehouseId,
        TransferDate = e.TransferDate,
        Remarks = e.Remarks,
        Status = e.Status,
        Items = e.Items.Select(i => new StockTransferItemInput
        {
            ProductId = i.ProductId,
            UnitId = i.UnitId,
            Quantity = i.Quantity,
            Rate = i.Rate,
            Remarks = i.Remarks,
        }).ToList(),
    };

    private static StockCountDto MapCount(StockCount e) => new()
    {
        StockCountId = e.StockCountId,
        CountNumber = e.CountNumber,
        WarehouseId = e.WarehouseId,
        CountDate = e.CountDate,
        Remarks = e.Remarks,
        Status = e.Status,
        Items = e.Items.Select(i => new StockCountItemDto
        {
            ProductId = i.ProductId,
            UnitId = i.UnitId,
            BookQuantity = i.BookQuantity,
            CountedQuantity = i.CountedQuantity,
            Variance = i.Variance,
            Rate = i.Rate,
        }).ToList(),
    };
}
