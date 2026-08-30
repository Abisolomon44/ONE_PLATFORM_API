using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IStockRepository
{
    Task<(List<Stock> Items, int TotalCount)> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search, long? warehouseId, long? productId);
    Task<List<StockTransaction>> GetTransactionsAsync(long companyId, int pageNumber, int pageSize, long? productId, long? warehouseId);
    Task<Stock?> GetByKeyAsync(long companyId, long branchId, long warehouseId, long productId, long unitId);
}

public class StockRepository : TenantRepositoryBase, IStockRepository
{
    public StockRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<(List<Stock> Items, int TotalCount)> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search, long? warehouseId, long? productId)
    {
        using var connection = OpenTenant();
        var sp = string.IsNullOrWhiteSpace(search) ? "%" : $"%{search}%";
        var offset = (pageNumber - 1) * pageSize;
        var total = await Sql.QuerySingleOrDefaultAsync<int>(connection,
            "SELECT COUNT(*) FROM dbo.Stock WHERE CompanyId = @companyId AND (@warehouseId IS NULL OR WarehouseId = @warehouseId) AND (@productId IS NULL OR ProductId = @productId)",
            new { companyId, warehouseId, productId });
        var items = await Sql.QueryAsync<Stock>(connection,
            @"SELECT * FROM (
                SELECT *, ROW_NUMBER() OVER (ORDER BY StockId DESC) AS _rn
                FROM dbo.Stock
                WHERE CompanyId = @companyId AND (@warehouseId IS NULL OR WarehouseId = @warehouseId) AND (@productId IS NULL OR ProductId = @productId)
              ) t
              WHERE t._rn > @offset AND t._rn <= @offset + @pageSize",
            new { companyId, warehouseId, productId, offset, pageSize });
        return (items.ToList(), total);
    }

    public async Task<List<StockTransaction>> GetTransactionsAsync(long companyId, int pageNumber, int pageSize, long? productId, long? warehouseId)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        var items = await Sql.QueryAsync<StockTransaction>(connection,
            @"SELECT * FROM (
                SELECT *, ROW_NUMBER() OVER (ORDER BY StockTransactionId DESC) AS _rn
                FROM dbo.StockTransaction
                WHERE CompanyId = @companyId AND (@productId IS NULL OR ProductId = @productId) AND (@warehouseId IS NULL OR WarehouseId = @warehouseId)
              ) t
              WHERE t._rn > @offset AND t._rn <= @offset + @pageSize",
            new { companyId, productId, warehouseId, offset, pageSize });
        return items.ToList();
    }

    public async Task<Stock?> GetByKeyAsync(long companyId, long branchId, long warehouseId, long productId, long unitId)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Stock>(connection,
            "SELECT * FROM dbo.Stock WHERE CompanyId=@companyId AND BranchId=@branchId AND WarehouseId=@warehouseId AND ProductId=@productId AND UnitId=@unitId",
            new { companyId, branchId, warehouseId, productId, unitId });
    }
}
