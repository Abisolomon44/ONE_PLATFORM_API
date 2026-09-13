using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

/* ---------------- Product Categories ---------------- */

public interface IProductCategoryRepository
{
    Task<ProductCategory?> GetByIdAsync(long id);
    Task<ProductCategory?> GetByCodeAsync(long companyId, string code);
    Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null);
    Task<string> GetNextCodeAsync(long companyId, string prefix = "CAT");
    Task<IEnumerable<ProductCategory>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<IEnumerable<ProductCategory>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids);
    Task<long> InsertAsync(ProductCategory entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(ProductCategory entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class ProductCategoryRepository : TenantRepositoryBase, IProductCategoryRepository
{
    public ProductCategoryRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<ProductCategory?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<ProductCategory>(connection,
            "SELECT * FROM dbo.ProductCategories WHERE Id = @id AND IsActive = 1", new { id });
    }

    public async Task<ProductCategory?> GetByCodeAsync(long companyId, string code)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<ProductCategory>(connection,
            "SELECT * FROM dbo.ProductCategories WHERE CompanyId = @companyId AND CategoryCode = @code AND IsActive = 1",
            new { companyId, code });
    }

    public async Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.ProductCategories
                WHERE CompanyId = @companyId AND CategoryCode = @code
                  AND (@excludeId IS NULL OR Id <> @excludeId)) THEN 1 ELSE 0 END",
            new { companyId, code, excludeId });
    }

    public async Task<string> GetNextCodeAsync(long companyId, string prefix = "CAT")
    {
        using var connection = OpenTenant();
        const string sql = @"
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(CategoryCode, LEN(@prefix) + 2, 10) AS INT)), 0) + 1
            FROM dbo.ProductCategories
            WHERE CompanyId = @companyId AND CategoryCode LIKE @prefix + '-%'";
        var next = await Sql.ExecuteScalarAsync<int>(connection, sql, new { companyId, prefix });
        return $"{prefix}-{next:D4}";
    }

    public async Task<IEnumerable<ProductCategory>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.ProductCategories WHERE CompanyId = @companyId ORDER BY SortOrder, CategoryName"
            : "SELECT * FROM dbo.ProductCategories WHERE CompanyId = @companyId AND IsActive = 1 ORDER BY SortOrder, CategoryName";
        return await Sql.QueryAsync<ProductCategory>(connection, sql, new { companyId });
    }

    public async Task<IEnumerable<ProductCategory>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<ProductCategory>(connection, @"
            SELECT * FROM dbo.ProductCategories
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR CategoryName LIKE '%' + @search + '%' OR CategoryCode LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')
            ORDER BY Id DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.ProductCategories
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR CategoryName LIKE '%' + @search + '%' OR CategoryCode LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, CategoryName FROM dbo.ProductCategories WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<long> InsertAsync(ProductCategory entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.ProductCategories (CompanyId, CategoryCode, CategoryName, Description, ParentCategoryId, SortOrder, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@CompanyId, @CategoryCode, @CategoryName, @Description, @ParentCategoryId, @SortOrder, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(ProductCategory entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.ProductCategories
                SET CategoryCode = @CategoryCode,
                    CategoryName = @CategoryName,
                    Description = @Description,
                    ParentCategoryId = @ParentCategoryId,
                    SortOrder = @SortOrder,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedAt = SYSUTCDATETIME()
                WHERE Id = @Id AND CompanyId = @CompanyId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.ProductCategories SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE Id = @id AND CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { id, companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

/* ---------------- Product Sub Categories ---------------- */

public interface IProductSubCategoryRepository
{
    Task<ProductSubCategory?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null);
    Task<string> GetNextCodeAsync(long companyId, string prefix = "SUB");
    Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids);
    Task<IEnumerable<ProductSubCategory>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<IEnumerable<ProductSubCategory>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<long> InsertAsync(ProductSubCategory entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(ProductSubCategory entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class ProductSubCategoryRepository : TenantRepositoryBase, IProductSubCategoryRepository
{
    public ProductSubCategoryRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<ProductSubCategory?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<ProductSubCategory>(connection,
            "SELECT * FROM dbo.ProductSubCategories WHERE Id = @id AND IsActive = 1", new { id });
    }

    public async Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.ProductSubCategories
                WHERE CompanyId = @companyId AND SubCategoryCode = @code
                  AND (@excludeId IS NULL OR Id <> @excludeId)) THEN 1 ELSE 0 END",
            new { companyId, code, excludeId });
    }

    public async Task<string> GetNextCodeAsync(long companyId, string prefix = "SUB")
    {
        using var connection = OpenTenant();
        const string sql = @"
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(SubCategoryCode, LEN(@prefix) + 2, 10) AS INT)), 0) + 1
            FROM dbo.ProductSubCategories
            WHERE CompanyId = @companyId AND SubCategoryCode LIKE @prefix + '-%'";
        var next = await Sql.ExecuteScalarAsync<int>(connection, sql,             new { companyId, prefix });
        return $"{prefix}-{next:D4}";
    }

    public async Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, SubCategoryName FROM dbo.ProductSubCategories WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<IEnumerable<ProductSubCategory>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.ProductSubCategories WHERE CompanyId = @companyId ORDER BY SortOrder, SubCategoryName"
            : "SELECT * FROM dbo.ProductSubCategories WHERE CompanyId = @companyId AND IsActive = 1 ORDER BY SortOrder, SubCategoryName";
        return await Sql.QueryAsync<ProductSubCategory>(connection, sql, new { companyId });
    }

    public async Task<IEnumerable<ProductSubCategory>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<ProductSubCategory>(connection, @"
            SELECT * FROM dbo.ProductSubCategories
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR SubCategoryName LIKE '%' + @search + '%' OR SubCategoryCode LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')
            ORDER BY Id DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.ProductSubCategories
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR SubCategoryName LIKE '%' + @search + '%' OR SubCategoryCode LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<long> InsertAsync(ProductSubCategory entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.ProductSubCategories (CompanyId, CategoryId, SubCategoryCode, SubCategoryName, Description, SortOrder, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@CompanyId, @CategoryId, @SubCategoryCode, @SubCategoryName, @Description, @SortOrder, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(ProductSubCategory entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.ProductSubCategories
                SET CategoryId = @CategoryId,
                    SubCategoryCode = @SubCategoryCode,
                    SubCategoryName = @SubCategoryName,
                    Description = @Description,
                    SortOrder = @SortOrder,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedAt = SYSUTCDATETIME()
                WHERE Id = @Id AND CompanyId = @CompanyId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.ProductSubCategories SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE Id = @id AND CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { id, companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

/* ---------------- Product Brands ---------------- */

public interface IProductBrandRepository
{
    Task<ProductBrand?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null);
    Task<string> GetNextCodeAsync(long companyId, string prefix = "BRD");
    Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids);
    Task<IEnumerable<ProductBrand>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<IEnumerable<ProductBrand>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<long> InsertAsync(ProductBrand entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(ProductBrand entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class ProductBrandRepository : TenantRepositoryBase, IProductBrandRepository
{
    public ProductBrandRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<ProductBrand?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<ProductBrand>(connection,
            "SELECT * FROM dbo.ProductBrands WHERE Id = @id AND IsActive = 1", new { id });
    }

    public async Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.ProductBrands
                WHERE CompanyId = @companyId AND BrandCode = @code
                  AND (@excludeId IS NULL OR Id <> @excludeId)) THEN 1 ELSE 0 END",
            new { companyId, code, excludeId });
    }

    public async Task<string> GetNextCodeAsync(long companyId, string prefix = "BRD")
    {
        using var connection = OpenTenant();
        const string sql = @"
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(BrandCode, LEN(@prefix) + 2, 10) AS INT)), 0) + 1
            FROM dbo.ProductBrands
            WHERE CompanyId = @companyId AND BrandCode LIKE @prefix + '-%'";
        var next = await Sql.ExecuteScalarAsync<int>(connection, sql,             new { companyId, prefix });
        return $"{prefix}-{next:D4}";
    }

    public async Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, BrandName FROM dbo.ProductBrands WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<IEnumerable<ProductBrand>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.ProductBrands WHERE CompanyId = @companyId ORDER BY BrandName"
            : "SELECT * FROM dbo.ProductBrands WHERE CompanyId = @companyId AND IsActive = 1 ORDER BY BrandName";
        return await Sql.QueryAsync<ProductBrand>(connection, sql, new { companyId });
    }

    public async Task<IEnumerable<ProductBrand>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<ProductBrand>(connection, @"
            SELECT * FROM dbo.ProductBrands
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR BrandName LIKE '%' + @search + '%' OR BrandCode LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')
            ORDER BY Id DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.ProductBrands
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR BrandName LIKE '%' + @search + '%' OR BrandCode LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<long> InsertAsync(ProductBrand entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.ProductBrands (CompanyId, BrandCode, BrandName, Description, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@CompanyId, @BrandCode, @BrandName, @Description, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(ProductBrand entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.ProductBrands
                SET BrandCode = @BrandCode,
                    BrandName = @BrandName,
                    Description = @Description,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedAt = SYSUTCDATETIME()
                WHERE Id = @Id AND CompanyId = @CompanyId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.ProductBrands SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE Id = @id AND CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { id, companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

/* ---------------- Product Units ---------------- */

public interface IProductUnitRepository
{
    Task<ProductUnit?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null);
    Task<string> GetNextCodeAsync(long companyId, string prefix = "UNT");
    Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids);
    Task<IEnumerable<ProductUnit>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<IEnumerable<ProductUnit>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<long> InsertAsync(ProductUnit entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(ProductUnit entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class ProductUnitRepository : TenantRepositoryBase, IProductUnitRepository
{
    public ProductUnitRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<ProductUnit?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<ProductUnit>(connection,
            "SELECT * FROM dbo.Units WHERE Id = @id AND IsActive = 1", new { id });
    }

    public async Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.Units
                WHERE CompanyId = @companyId AND UnitCode = @code
                  AND (@excludeId IS NULL OR Id <> @excludeId)) THEN 1 ELSE 0 END",
            new { companyId, code, excludeId });
    }

    public async Task<string> GetNextCodeAsync(long companyId, string prefix = "UNT")
    {
        using var connection = OpenTenant();
        const string sql = @"
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(UnitCode, LEN(@prefix) + 2, 10) AS INT)), 0) + 1
            FROM dbo.Units
            WHERE CompanyId = @companyId AND UnitCode LIKE @prefix + '-%'";
        var next = await Sql.ExecuteScalarAsync<int>(connection, sql,             new { companyId, prefix });
        return $"{prefix}-{next:D4}";
    }

    public async Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, UnitName FROM dbo.Units WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<IEnumerable<ProductUnit>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.Units WHERE CompanyId = @companyId ORDER BY UnitName"
            : "SELECT * FROM dbo.Units WHERE CompanyId = @companyId AND IsActive = 1 ORDER BY UnitName";
        return await Sql.QueryAsync<ProductUnit>(connection, sql, new { companyId });
    }

    public async Task<IEnumerable<ProductUnit>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<ProductUnit>(connection, @"
            SELECT * FROM dbo.Units
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR UnitName LIKE '%' + @search + '%' OR UnitCode LIKE '%' + @search + '%' OR Symbol LIKE '%' + @search + '%')
            ORDER BY Id DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Units
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR UnitName LIKE '%' + @search + '%' OR UnitCode LIKE '%' + @search + '%' OR Symbol LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<long> InsertAsync(ProductUnit entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Units (CompanyId, UnitCode, UnitName, Symbol, DecimalPlaces, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@CompanyId, @UnitCode, @UnitName, @Symbol, @DecimalPlaces, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(ProductUnit entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Units
                SET UnitCode = @UnitCode,
                    UnitName = @UnitName,
                    Symbol = @Symbol,
                    DecimalPlaces = @DecimalPlaces,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedAt = SYSUTCDATETIME()
                WHERE Id = @Id AND CompanyId = @CompanyId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Units SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE Id = @id AND CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { id, companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

/* ============================================================
   Products
   ============================================================ */

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null);
    Task<IEnumerable<Product>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<long> InsertAsync(Product entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Product entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<string> GetNextCodeAsync(long companyId, string prefix = "PRC");
}

public class ProductRepository : TenantRepositoryBase, IProductRepository
{
    public ProductRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<Product?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Product>(connection,
            "SELECT * FROM dbo.Products WHERE Id = @id AND IsActive = 1", new { id });
    }

    public async Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.Products
                WHERE CompanyId = @companyId AND ProductCode = @code
                  AND (@excludeId IS NULL OR Id <> @excludeId)) THEN 1 ELSE 0 END",
            new { companyId, code, excludeId });
    }

    public async Task<IEnumerable<Product>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<Product>(connection, @"
            SELECT * FROM dbo.Products
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR ProductName LIKE '%' + @search + '%' OR ProductCode LIKE '%' + @search + '%' OR SKU LIKE '%' + @search + '%' OR Barcode LIKE '%' + @search + '%')
            ORDER BY Id DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Products
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR ProductName LIKE '%' + @search + '%' OR ProductCode LIKE '%' + @search + '%' OR SKU LIKE '%' + @search + '%' OR Barcode LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<long> InsertAsync(Product entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Products (
                    CompanyId, BranchId, ProductCode, ProductName, CategoryId, SubCategoryId, BrandId, UOMId,
                    SKU, Barcode, MRP, PurchasePrice, SalesPrice, TaxId, HsnSacId,
                    IsStockItem, IsSaleable, IsPurchaseable, IsActive, Description,
                    CreatedBy, CreatedAt, ModifiedBy, ModifiedAt, EntityId)
                VALUES (
                    @CompanyId, @BranchId, @ProductCode, @ProductName, @CategoryId, @SubCategoryId, @BrandId, @UOMId,
                    @SKU, @Barcode, @MRP, @PurchasePrice, @SalesPrice, @TaxId, @HsnSacId,
                    @IsStockItem, @IsSaleable, @IsPurchaseable, @IsActive, @Description,
                    @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME(), @EntityId);
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Product entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Products
                SET BranchId = @BranchId,
                    ProductCode = @ProductCode,
                    ProductName = @ProductName,
                    CategoryId = @CategoryId,
                    SubCategoryId = @SubCategoryId,
                    BrandId = @BrandId,
                    UOMId = @UOMId,
                    SKU = @SKU,
                    Barcode = @Barcode,
                    MRP = @MRP,
                    PurchasePrice = @PurchasePrice,
                    SalesPrice = @SalesPrice,
                    TaxId = @TaxId,
                    HsnSacId = @HsnSacId,
                    IsStockItem = @IsStockItem,
                    IsSaleable = @IsSaleable,
                    IsPurchaseable = @IsPurchaseable,
                    IsActive = @IsActive,
                    Description = @Description,
                    EntityId = @EntityId,
                    ModifiedBy = @ModifiedBy,
                    ModifiedAt = SYSUTCDATETIME()
                WHERE Id = @Id AND CompanyId = @CompanyId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Products SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE Id = @id AND CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { id, companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<string> GetNextCodeAsync(long companyId, string prefix = "PRC")
    {
        using var connection = OpenTenant();
        var next = await Sql.ExecuteScalarAsync<int>(connection,
            "SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(ProductCode, LEN(@prefix) + 2, 10) AS INT)), 0) + 1 " +
            "FROM dbo.Products WHERE CompanyId = @companyId AND ProductCode LIKE @prefix + '-%'",
            new { companyId, prefix });
        return $"{prefix}-{next:D3}";
    }
}
