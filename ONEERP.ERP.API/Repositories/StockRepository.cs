using Dapper;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IStockRepository
{
    Task<(List<Stock> Items, int TotalCount)> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search, long? warehouseId, long? productId);
    Task<(List<StockTransaction> Items, int TotalCount)> GetTransactionsAsync(long companyId, int pageNumber, int pageSize, long? productId, long? warehouseId);
    Task<Stock?> GetByKeyAsync(long companyId, long branchId, long warehouseId, long productId, long unitId);

    // Price Master: Get purchase products from stock (joined with product details)
    Task<(IEnumerable<Stock> Items, int TotalCount)> GetPriceMasterPurchaseProductsAsync(
        long companyId,
        long? branchId,
        long? warehouseId,
        string? search,
        long? categoryId,
        int pageNumber,
        int pageSize);
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

    public async Task<(List<StockTransaction> Items, int TotalCount)> GetTransactionsAsync(long companyId, int pageNumber, int pageSize, long? productId, long? warehouseId)
    {
        using var connection = OpenTenant();

        // Build WHERE clause
        var where = "WHERE CompanyId = @companyId";
        if (productId.HasValue) where += " AND ProductId = @productId";
        if (warehouseId.HasValue) where += " AND WarehouseId = @warehouseId";

        // Get total count
        var total = await Sql.QuerySingleOrDefaultAsync<int>(connection,
            $"SELECT COUNT(*) FROM dbo.StockTransaction {where}",
            new { companyId, productId, warehouseId });

        // Calculate running balance using window function (chronological ASC)
        // Then paginate and return in DESC order for display
        var sql = $@"
            WITH OrderedTx AS (
                SELECT
                    StockTransactionId,
                    CompanyId, BranchId, WarehouseId, ProductId, UnitId,
                    TransactionType, ReferenceType, ReferenceId,
                    QuantityIn, QuantityOut, Rate, TransactionDate, Remarks, CreatedByUserID,
                    -- Running balance: SUM(QuantityIn - QuantityOut) OVER (PARTITION BY key ORDER BY date, id)
                    SUM(QuantityIn - QuantityOut) OVER (
                        PARTITION BY CompanyId, BranchId, WarehouseId, ProductId, UnitId
                        ORDER BY TransactionDate, StockTransactionId
                        ROWS UNBOUNDED PRECEDING
                    ) AS RunningBalance
                FROM dbo.StockTransaction
                {where}
            ),
            Paged AS (
                SELECT *
                FROM (
                    SELECT *, ROW_NUMBER() OVER (ORDER BY TransactionDate DESC, StockTransactionId DESC) AS _rn
                    FROM OrderedTx
                ) t
                WHERE t._rn > @offset AND t._rn <= @offset + @pageSize
            )
            SELECT
                StockTransactionId, CompanyId, BranchId, WarehouseId, ProductId, UnitId,
                TransactionType, ReferenceType, ReferenceId,
                QuantityIn, QuantityOut, Rate,
                RunningBalance AS BalanceQuantity,
                TransactionDate, Remarks, CreatedByUserID
            FROM Paged
            ORDER BY TransactionDate DESC, StockTransactionId DESC;";

        var offset = (pageNumber - 1) * pageSize;
        var items = await Sql.QueryAsync<StockTransaction>(connection, sql,
            new { companyId, productId, warehouseId, offset, pageSize });

        return (items.ToList(), total);
    }

    public async Task<Stock?> GetByKeyAsync(long companyId, long branchId, long warehouseId, long productId, long unitId)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Stock>(connection,
            "SELECT * FROM dbo.Stock WHERE CompanyId=@companyId AND BranchId=@branchId AND WarehouseId=@warehouseId AND ProductId=@productId AND UnitId=@unitId",
            new { companyId, branchId, warehouseId, productId, unitId });
    }

    public async Task<(IEnumerable<Stock> Items, int TotalCount)> GetPriceMasterPurchaseProductsAsync(
        long companyId,
        long? branchId,
        long? warehouseId,
        string? search,
        long? categoryId,
        int pageNumber,
        int pageSize)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;

        var whereClause = new List<string> { "s.CompanyId = @companyId" };
        var parameters = new DynamicParameters();
        parameters.Add("companyId", companyId);
        parameters.Add("offset", (pageNumber - 1) * pageSize);
        parameters.Add("pageSize", pageSize);

        if (branchId.HasValue && branchId.Value > 0)
        {
            whereClause.Add("s.BranchId = @branchId");
            parameters.Add("branchId", branchId.Value);
        }

        if (warehouseId.HasValue && warehouseId.Value > 0)
        {
            whereClause.Add("s.WarehouseId = @warehouseId");
            parameters.Add("warehouseId", warehouseId.Value);
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            whereClause.Add("p.CategoryId = @categoryId");
            parameters.Add("categoryId", categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            whereClause.Add("(p.ProductName LIKE '%' + @search + '%' OR p.ProductCode LIKE '%' + @search + '%' OR p.SKU LIKE '%' + @search + '%' OR p.Barcode LIKE '%' + @search + '%')");
            parameters.Add("search", search);
        }

        var whereSql = string.Join(" AND ", whereClause);

        // Total count
        var countSql = $@"
            SELECT COUNT(1)
            FROM dbo.Stock s
            INNER JOIN dbo.Products p ON p.Id = s.ProductId
            WHERE {whereSql}";
        var total = await Sql.ExecuteScalarAsync<int>(connection, countSql, parameters);

        // Paged items with product details
        var sql = $@"
            SELECT
                s.StockId, s.CompanyId, s.BranchId, s.WarehouseId, s.ProductId, s.UnitId,
                s.Quantity, s.ReservedQuantity, s.AvailableQuantity, s.AverageCost, s.LastPurchaseRate, s.UpdatedAt,
                p.ProductCode, p.ProductName, p.CategoryId,
                u.UnitName
            FROM dbo.Stock s
            INNER JOIN dbo.Products p ON p.Id = s.ProductId
            LEFT JOIN dbo.ProductUnits u ON u.Id = s.UnitId
            WHERE {whereSql}
            ORDER BY s.StockId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

        parameters.Add("offset", (pageNumber - 1) * pageSize);
        parameters.Add("pageSize", pageSize);

        var items = await Sql.QueryAsync<Stock>(connection, sql, parameters);

        return (items, total);
    }
}