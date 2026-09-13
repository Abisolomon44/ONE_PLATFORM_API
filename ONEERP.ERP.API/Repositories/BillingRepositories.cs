using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

/* ---------------- Price Types (system master) ---------------- */

public interface IPriceTypeRepository
{
    Task<PriceType?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(string code, long? excludeId = null);
    Task<string> GetNextCodeAsync(string prefix = "PRICETYPE");
    Task<IEnumerable<PriceType>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<PriceType>> GetPagedAsync(int pageNumber, int pageSize, string search);
    Task<int> CountAsync(string search);
    Task<Dictionary<long, string>> GetNamesAsync(IEnumerable<long> ids);
    Task<long> InsertAsync(PriceType entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(PriceType entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class PriceTypeRepository : TenantRepositoryBase, IPriceTypeRepository
{
    public PriceTypeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<PriceType?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<PriceType>(connection,
            "SELECT * FROM dbo.PriceTypes WHERE PriceTypeId = @id", new { id });
    }

    public async Task<bool> CodeInUseAsync(string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.PriceTypes
                WHERE Code = @code AND (@excludeId IS NULL OR PriceTypeId <> @excludeId)) THEN 1 ELSE 0 END",
            new { code, excludeId });
    }

    public async Task<string> GetNextCodeAsync(string prefix = "PRICETYPE")
    {
        using var connection = OpenTenant();
        const string sql = @"
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(Code, LEN(@prefix) + 2, 10) AS INT)), 0) + 1
            FROM dbo.PriceTypes WHERE Code LIKE @prefix + '-%'";
        var next = await Sql.ExecuteScalarAsync<int>(connection, sql, new { prefix });
        return $"{prefix}-{next:D3}";
    }

    public async Task<IEnumerable<PriceType>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.PriceTypes ORDER BY DisplayOrder, Name"
            : "SELECT * FROM dbo.PriceTypes WHERE IsActive = 1 ORDER BY DisplayOrder, Name";
        return await Sql.QueryAsync<PriceType>(connection, sql);
    }

    public async Task<IEnumerable<PriceType>> GetPagedAsync(int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<PriceType>(connection, @"
            SELECT * FROM dbo.PriceTypes
            WHERE (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')
            ORDER BY DisplayOrder, Name
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { search, offset, pageSize });
    }

    public async Task<int> CountAsync(string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.PriceTypes
            WHERE (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')",
            new { search });
    }

    public async Task<Dictionary<long, string>> GetNamesAsync(IEnumerable<long> ids)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT PriceTypeId, Name FROM dbo.PriceTypes WHERE PriceTypeId IN @ids", new { ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<long> InsertAsync(PriceType entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.PriceTypes (Code, Name, Description, IsActive, DisplayOrder, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@Code, @Name, @Description, @IsActive, @DisplayOrder, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(PriceType entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.PriceTypes
                SET Code = @Code, Name = @Name, Description = @Description, IsActive = @IsActive,
                    DisplayOrder = @DisplayOrder, ModifiedBy = @ModifiedBy, ModifiedAt = SYSUTCDATETIME()
                WHERE PriceTypeId = @PriceTypeId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(long id, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.PriceTypes SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE PriceTypeId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

/* ---------------- Unit Conversions (company) ---------------- */

public interface IUnitConversionRepository
{
    Task<UnitConversion?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(long companyId, long fromUnitId, long toUnitId, long? productId = null, long? excludeId = null);
    Task<IEnumerable<UnitConversion>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<IEnumerable<UnitConversion>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<Dictionary<long, string>> GetProductNamesAsync(long companyId, IEnumerable<long> ids);
    Task<Dictionary<long, string>> GetUnitNamesAsync(long companyId, IEnumerable<long> ids);
    Task<long> InsertAsync(UnitConversion entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(UnitConversion entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class UnitConversionRepository : TenantRepositoryBase, IUnitConversionRepository
{
    public UnitConversionRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<UnitConversion?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<UnitConversion>(connection,
            "SELECT * FROM dbo.UnitConversions WHERE UnitConversionId = @id", new { id });
    }

    public async Task<bool> CodeInUseAsync(long companyId, long fromUnitId, long toUnitId, long? productId = null, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.UnitConversions
                WHERE CompanyId = @companyId AND FromUnitId = @fromUnitId AND ToUnitId = @toUnitId
                  AND ISNULL(ProductId, 0) = ISNULL(@productId, 0)
                  AND (@excludeId IS NULL OR UnitConversionId <> @excludeId)) THEN 1 ELSE 0 END",
            new { companyId, fromUnitId, toUnitId, productId, excludeId });
    }

    public async Task<IEnumerable<UnitConversion>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.UnitConversions WHERE CompanyId = @companyId ORDER BY IsDefault DESC, UnitConversionId DESC"
            : "SELECT * FROM dbo.UnitConversions WHERE CompanyId = @companyId AND IsActive = 1 ORDER BY IsDefault DESC, UnitConversionId DESC";
        return await Sql.QueryAsync<UnitConversion>(connection, sql, new { companyId });
    }

    public async Task<IEnumerable<UnitConversion>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<UnitConversion>(connection, @"
            SELECT * FROM dbo.UnitConversions
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR CAST(FromUnitId AS VARCHAR) LIKE '%' + @search + '%' OR CAST(ToUnitId AS VARCHAR) LIKE '%' + @search + '%' OR CAST(ConversionFactor AS VARCHAR) LIKE '%' + @search + '%')
            ORDER BY UnitConversionId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.UnitConversions
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR CAST(FromUnitId AS VARCHAR) LIKE '%' + @search + '%' OR CAST(ToUnitId AS VARCHAR) LIKE '%' + @search + '%' OR CAST(ConversionFactor AS VARCHAR) LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<Dictionary<long, string>> GetProductNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Where(i => i > 0).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, ProductName FROM dbo.Products WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<Dictionary<long, string>> GetUnitNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, UnitName FROM dbo.Units WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<long> InsertAsync(UnitConversion entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.UnitConversions (CompanyId, ProductId, FromUnitId, ToUnitId, ConversionFactor, IsDefault, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@CompanyId, @ProductId, @FromUnitId, @ToUnitId, @ConversionFactor, @IsDefault, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(UnitConversion entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.UnitConversions
                SET ProductId = @ProductId, FromUnitId = @FromUnitId, ToUnitId = @ToUnitId,
                    ConversionFactor = @ConversionFactor, IsDefault = @IsDefault, IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy, ModifiedAt = SYSUTCDATETIME()
                WHERE UnitConversionId = @UnitConversionId AND CompanyId = @CompanyId;";
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
            const string sql = "UPDATE dbo.UnitConversions SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE UnitConversionId = @id AND CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { id, companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

/* ---------------- Barcodes (company) ---------------- */

public interface IBarcodeRepository
{
    Task<Barcode?> GetByIdAsync(long id);
    Task<bool> ValueInUseAsync(long companyId, string barcode, long? excludeId = null);
    Task<IEnumerable<Barcode>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<IEnumerable<Barcode>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<Dictionary<long, string>> GetProductNamesAsync(long companyId, IEnumerable<long> ids);
    Task<Dictionary<long, string>> GetUnitNamesAsync(long companyId, IEnumerable<long> ids);
    Task<long> InsertAsync(Barcode entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Barcode entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class BarcodeRepository : TenantRepositoryBase, IBarcodeRepository
{
    public BarcodeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<Barcode?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Barcode>(connection,
            "SELECT * FROM dbo.Barcodes WHERE BarcodeId = @id", new { id });
    }

    public async Task<bool> ValueInUseAsync(long companyId, string barcode, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.Barcodes
                WHERE CompanyId = @companyId AND Barcode = @barcode AND (@excludeId IS NULL OR BarcodeId <> @excludeId)) THEN 1 ELSE 0 END",
            new { companyId, barcode, excludeId });
    }

    public async Task<IEnumerable<Barcode>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.Barcodes WHERE CompanyId = @companyId ORDER BY IsPrimary DESC, BarcodeId DESC"
            : "SELECT * FROM dbo.Barcodes WHERE CompanyId = @companyId AND IsActive = 1 ORDER BY IsPrimary DESC, BarcodeId DESC";
        return await Sql.QueryAsync<Barcode>(connection, sql, new { companyId });
    }

    public async Task<IEnumerable<Barcode>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<Barcode>(connection, @"
            SELECT * FROM dbo.Barcodes
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Barcode LIKE '%' + @search + '%' OR BarcodeType LIKE '%' + @search + '%' OR CAST(ProductId AS VARCHAR) LIKE '%' + @search + '%')
            ORDER BY BarcodeId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Barcodes
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Barcode LIKE '%' + @search + '%' OR BarcodeType LIKE '%' + @search + '%' OR CAST(ProductId AS VARCHAR) LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<Dictionary<long, string>> GetProductNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, ProductName FROM dbo.Products WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<Dictionary<long, string>> GetUnitNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Where(i => i > 0).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, UnitName FROM dbo.Units WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<long> InsertAsync(Barcode entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Barcodes (CompanyId, ProductId, UnitId, Barcode, BarcodeType, IsPrimary, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@CompanyId, @ProductId, @UnitId, @Barcode, @BarcodeType, @IsPrimary, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Barcode entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Barcodes
                SET ProductId = @ProductId, UnitId = @UnitId, Barcode = @Barcode, BarcodeType = @BarcodeType,
                    IsPrimary = @IsPrimary, IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedAt = SYSUTCDATETIME()
                WHERE BarcodeId = @BarcodeId AND CompanyId = @CompanyId;";
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
            const string sql = "UPDATE dbo.Barcodes SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE BarcodeId = @id AND CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { id, companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

/* ---------------- HSN/SAC (company) ---------------- */

public interface IHsnSacRepository
{
    Task<HsnSac?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null);
    Task<string> GetNextCodeAsync(long companyId, string prefix = "HSN");
    Task<IEnumerable<HsnSac>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<IEnumerable<HsnSac>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids);
    Task<long> InsertAsync(HsnSac entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(HsnSac entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class HsnSacRepository : TenantRepositoryBase, IHsnSacRepository
{
    public HsnSacRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<HsnSac?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<HsnSac>(connection,
            "SELECT * FROM dbo.HsnSacs WHERE HsnSacId = @id", new { id });
    }

    public async Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.HsnSacs
                WHERE CompanyId = @companyId AND Code = @code AND (@excludeId IS NULL OR HsnSacId <> @excludeId)) THEN 1 ELSE 0 END",
            new { companyId, code, excludeId });
    }

    public async Task<string> GetNextCodeAsync(long companyId, string prefix = "HSN")
    {
        using var connection = OpenTenant();
        const string sql = @"
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(Code, LEN(@prefix) + 2, 10) AS INT)), 0) + 1
            FROM dbo.HsnSacs WHERE CompanyId = @companyId AND Code LIKE @prefix + '-%'";
        var next = await Sql.ExecuteScalarAsync<int>(connection, sql, new { companyId, prefix });
        return $"{prefix}-{next:D4}";
    }

    public async Task<IEnumerable<HsnSac>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.HsnSacs WHERE CompanyId = @companyId ORDER BY HsnSacType, Code"
            : "SELECT * FROM dbo.HsnSacs WHERE CompanyId = @companyId AND IsActive = 1 ORDER BY HsnSacType, Code";
        return await Sql.QueryAsync<HsnSac>(connection, sql, new { companyId });
    }

    public async Task<IEnumerable<HsnSac>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<HsnSac>(connection, @"
            SELECT * FROM dbo.HsnSacs
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%' OR HsnSacType LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')
            ORDER BY HsnSacId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.HsnSacs
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%' OR HsnSacType LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Where(i => i > 0).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT HsnSacId, Code AS Name FROM dbo.HsnSacs WHERE CompanyId = @companyId AND HsnSacId IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<long> InsertAsync(HsnSac entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.HsnSacs (CompanyId, Code, Name, HsnSacType, Description, TaxId, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@CompanyId, @Code, @Name, @HsnSacType, @Description, @TaxId, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(HsnSac entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.HsnSacs
                SET Code = @Code, Name = @Name, HsnSacType = @HsnSacType, Description = @Description, TaxId = @TaxId,
                    IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedAt = SYSUTCDATETIME()
                WHERE HsnSacId = @HsnSacId AND CompanyId = @CompanyId;";
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
            const string sql = "UPDATE dbo.HsnSacs SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE HsnSacId = @id AND CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { id, companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

/* ---------------- Service Categories (company) ---------------- */

public interface IServiceCategoryRepository
{
    Task<ServiceCategory?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null);
    Task<string> GetNextCodeAsync(long companyId, string prefix = "SVCAT");
    Task<IEnumerable<ServiceCategory>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<IEnumerable<ServiceCategory>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids);
    Task<long> InsertAsync(ServiceCategory entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(ServiceCategory entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class ServiceCategoryRepository : TenantRepositoryBase, IServiceCategoryRepository
{
    public ServiceCategoryRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<ServiceCategory?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<ServiceCategory>(connection,
            "SELECT * FROM dbo.ServiceCategories WHERE ServiceCategoryId = @id", new { id });
    }

    public async Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.ServiceCategories
                WHERE CompanyId = @companyId AND Code = @code AND (@excludeId IS NULL OR ServiceCategoryId <> @excludeId)) THEN 1 ELSE 0 END",
            new { companyId, code, excludeId });
    }

    public async Task<string> GetNextCodeAsync(long companyId, string prefix = "SVCAT")
    {
        using var connection = OpenTenant();
        const string sql = @"
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(Code, LEN(@prefix) + 2, 10) AS INT)), 0) + 1
            FROM dbo.ServiceCategories WHERE CompanyId = @companyId AND Code LIKE @prefix + '-%'";
        var next = await Sql.ExecuteScalarAsync<int>(connection, sql, new { companyId, prefix });
        return $"{prefix}-{next:D3}";
    }

    public async Task<IEnumerable<ServiceCategory>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.ServiceCategories WHERE CompanyId = @companyId ORDER BY DisplayOrder, Name"
            : "SELECT * FROM dbo.ServiceCategories WHERE CompanyId = @companyId AND IsActive = 1 ORDER BY DisplayOrder, Name";
        return await Sql.QueryAsync<ServiceCategory>(connection, sql, new { companyId });
    }

    public async Task<IEnumerable<ServiceCategory>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<ServiceCategory>(connection, @"
            SELECT * FROM dbo.ServiceCategories
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')
            ORDER BY ServiceCategoryId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.ServiceCategories
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT ServiceCategoryId, Name FROM dbo.ServiceCategories WHERE CompanyId = @companyId AND ServiceCategoryId IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<long> InsertAsync(ServiceCategory entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.ServiceCategories (CompanyId, Code, Name, Description, DisplayOrder, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@CompanyId, @Code, @Name, @Description, @DisplayOrder, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(ServiceCategory entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.ServiceCategories
                SET Code = @Code, Name = @Name, Description = @Description, DisplayOrder = @DisplayOrder,
                    IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedAt = SYSUTCDATETIME()
                WHERE ServiceCategoryId = @ServiceCategoryId AND CompanyId = @CompanyId;";
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
            const string sql = "UPDATE dbo.ServiceCategories SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE ServiceCategoryId = @id AND CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { id, companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

/* ---------------- Services (company) ---------------- */

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null);
    Task<string> GetNextCodeAsync(long companyId, string prefix = "SRV");
    Task<IEnumerable<Service>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<IEnumerable<Service>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids);
    Task<long> InsertAsync(Service entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Service entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class ServiceRepository : TenantRepositoryBase, IServiceRepository
{
    public ServiceRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<Service?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Service>(connection,
            "SELECT * FROM dbo.Services WHERE ServiceId = @id", new { id });
    }

    public async Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.Services
                WHERE CompanyId = @companyId AND Code = @code AND (@excludeId IS NULL OR ServiceId <> @excludeId)) THEN 1 ELSE 0 END",
            new { companyId, code, excludeId });
    }

    public async Task<string> GetNextCodeAsync(long companyId, string prefix = "SRV")
    {
        using var connection = OpenTenant();
        const string sql = @"
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(Code, LEN(@prefix) + 2, 10) AS INT)), 0) + 1
            FROM dbo.Services WHERE CompanyId = @companyId AND Code LIKE @prefix + '-%'";
        var next = await Sql.ExecuteScalarAsync<int>(connection, sql, new { companyId, prefix });
        return $"{prefix}-{next:D3}";
    }

    public async Task<IEnumerable<Service>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.Services WHERE CompanyId = @companyId ORDER BY Name"
            : "SELECT * FROM dbo.Services WHERE CompanyId = @companyId AND IsActive = 1 ORDER BY Name";
        return await Sql.QueryAsync<Service>(connection, sql, new { companyId });
    }

    public async Task<IEnumerable<Service>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<Service>(connection, @"
            SELECT * FROM dbo.Services
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')
            ORDER BY ServiceId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Services
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT ServiceId, Name FROM dbo.Services WHERE CompanyId = @companyId AND ServiceId IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<long> InsertAsync(Service entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Services (CompanyId, Code, Name, ServiceCategoryId, UnitId, HsnSacId, DefaultTaxId, StandardRate, IsTaxInclusive, Description, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt, EntityId)
                VALUES (@CompanyId, @Code, @Name, @ServiceCategoryId, @UnitId, @HsnSacId, @DefaultTaxId, @StandardRate, @IsTaxInclusive, @Description, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME(), @EntityId);
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Service entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Services
                SET Code = @Code, Name = @Name, ServiceCategoryId = @ServiceCategoryId, UnitId = @UnitId,
                    HsnSacId = @HsnSacId, DefaultTaxId = @DefaultTaxId, StandardRate = @StandardRate,
                    IsTaxInclusive = @IsTaxInclusive, Description = @Description, IsActive = @IsActive,
                    EntityId = @EntityId,
                    ModifiedBy = @ModifiedBy, ModifiedAt = SYSUTCDATETIME()
                WHERE ServiceId = @ServiceId AND CompanyId = @CompanyId;";
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
            const string sql = "UPDATE dbo.Services SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE ServiceId = @id AND CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { id, companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

/* ---------------- Price Lists (company) ---------------- */

public interface IPriceListRepository
{
    Task<PriceList?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null);
    Task<string> GetNextCodeAsync(long companyId, string prefix = "PL");
    Task<IEnumerable<PriceList>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<IEnumerable<PriceList>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<Dictionary<long, string>> GetPriceTypeNamesAsync(IEnumerable<long> ids);
    Task<Dictionary<int, string>> GetCurrencyNamesAsync(IEnumerable<int> ids);
    Task<long> InsertAsync(PriceList entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(PriceList entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class PriceListRepository : TenantRepositoryBase, IPriceListRepository
{
    public PriceListRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<PriceList?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<PriceList>(connection,
            "SELECT * FROM dbo.PriceLists WHERE PriceListId = @id", new { id });
    }

    public async Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.PriceLists
                WHERE CompanyId = @companyId AND Code = @code AND (@excludeId IS NULL OR PriceListId <> @excludeId)) THEN 1 ELSE 0 END",
            new { companyId, code, excludeId });
    }

    public async Task<string> GetNextCodeAsync(long companyId, string prefix = "PL")
    {
        using var connection = OpenTenant();
        const string sql = @"
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(Code, LEN(@prefix) + 2, 10) AS INT)), 0) + 1
            FROM dbo.PriceLists WHERE CompanyId = @companyId AND Code LIKE @prefix + '-%'";
        var next = await Sql.ExecuteScalarAsync<int>(connection, sql, new { companyId, prefix });
        return $"{prefix}-{next:D3}";
    }

    public async Task<IEnumerable<PriceList>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.PriceLists WHERE CompanyId = @companyId ORDER BY IsDefault DESC, Name"
            : "SELECT * FROM dbo.PriceLists WHERE CompanyId = @companyId AND IsActive = 1 ORDER BY IsDefault DESC, Name";
        return await Sql.QueryAsync<PriceList>(connection, sql, new { companyId });
    }

    public async Task<IEnumerable<PriceList>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<PriceList>(connection, @"
            SELECT * FROM dbo.PriceLists
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')
            ORDER BY PriceListId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.PriceLists
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<Dictionary<long, string>> GetPriceTypeNamesAsync(IEnumerable<long> ids)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT PriceTypeId, Name FROM dbo.PriceTypes WHERE PriceTypeId IN @ids", new { ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<Dictionary<int, string>> GetCurrencyNamesAsync(IEnumerable<int> ids)
    {
        var list = ids.Where(i => i > 0).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<int, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(int Id, string Name)>(connection,
            "SELECT Id, CurrencyName FROM dbo.Currencies WHERE Id IN @ids", new { ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<long> InsertAsync(PriceList entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.PriceLists (CompanyId, PriceTypeId, CurrencyId, Code, Name, Description, EffectiveFrom, EffectiveTo, IsDefault, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@CompanyId, @PriceTypeId, @CurrencyId, @Code, @Name, @Description, @EffectiveFrom, @EffectiveTo, @IsDefault, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(PriceList entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.PriceLists
                SET PriceTypeId = @PriceTypeId, CurrencyId = @CurrencyId, Code = @Code, Name = @Name,
                    Description = @Description, EffectiveFrom = @EffectiveFrom, EffectiveTo = @EffectiveTo,
                    IsDefault = @IsDefault, IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedAt = SYSUTCDATETIME()
                WHERE PriceListId = @PriceListId AND CompanyId = @CompanyId;";
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
            const string sql = "UPDATE dbo.PriceLists SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE PriceListId = @id AND CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { id, companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

/* ---------------- Price List Details (child of PriceList) ---------------- */

public interface IPriceListDetailRepository
{
    Task<PriceListDetail?> GetByIdAsync(long id);
    Task<IEnumerable<PriceListDetail>> GetByPriceListAsync(long priceListId, bool includeInactive = false);
    Task<bool> ProductInUseAsync(long priceListId, long productId, long? unitId = null, long? excludeId = null);
    Task<long> InsertAsync(PriceListDetail entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(PriceListDetail entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> DeleteByPriceListAsync(long priceListId, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<Dictionary<long, string>> GetProductNamesAsync(long companyId, IEnumerable<long> ids);
    Task<Dictionary<long, string>> GetUnitNamesAsync(long companyId, IEnumerable<long> ids);
}

public class PriceListDetailRepository : TenantRepositoryBase, IPriceListDetailRepository
{
    public PriceListDetailRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<PriceListDetail?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<PriceListDetail>(connection,
            "SELECT * FROM dbo.PriceListDetails WHERE PriceListDetailId = @id", new { id });
    }

    public async Task<IEnumerable<PriceListDetail>> GetByPriceListAsync(long priceListId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.PriceListDetails WHERE PriceListId = @priceListId ORDER BY PriceListDetailId"
            : "SELECT * FROM dbo.PriceListDetails WHERE PriceListId = @priceListId AND IsActive = 1 ORDER BY PriceListDetailId";
        return await Sql.QueryAsync<PriceListDetail>(connection, sql, new { priceListId });
    }

    public async Task<bool> ProductInUseAsync(long priceListId, long productId, long? unitId = null, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.PriceListDetails
                WHERE PriceListId = @priceListId AND ProductId = @productId
                  AND ISNULL(UnitId, 0) = ISNULL(@unitId, 0)
                  AND (@excludeId IS NULL OR PriceListDetailId <> @excludeId)) THEN 1 ELSE 0 END",
            new { priceListId, productId, unitId, excludeId });
    }

    public async Task<long> InsertAsync(PriceListDetail entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.PriceListDetails (PriceListId, ProductId, UnitId, Price, MinimumQuantity, MaximumQuantity, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@PriceListId, @ProductId, @UnitId, @Price, @MinimumQuantity, @MaximumQuantity, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(PriceListDetail entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.PriceListDetails
                SET ProductId = @ProductId, UnitId = @UnitId, Price = @Price, MinimumQuantity = @MinimumQuantity,
                    MaximumQuantity = @MaximumQuantity, IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedAt = SYSUTCDATETIME()
                WHERE PriceListDetailId = @PriceListDetailId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(long id, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.PriceListDetails SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE PriceListDetailId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> DeleteByPriceListAsync(long priceListId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "DELETE FROM dbo.PriceListDetails WHERE PriceListId = @priceListId;";
            return await Sql.ExecuteAsync(conn, sql, new { priceListId }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<Dictionary<long, string>> GetProductNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, ProductName FROM dbo.Products WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<Dictionary<long, string>> GetUnitNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Where(i => i > 0).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, UnitName FROM dbo.Units WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }
}

/* ---------------- Discount Rules (company) ---------------- */

public interface IDiscountRuleRepository
{
    Task<DiscountRule?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null);
    Task<string> GetNextCodeAsync(long companyId, string prefix = "DR");
    Task<IEnumerable<DiscountRule>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<Dictionary<long, string>> GetProductNamesAsync(long companyId, IEnumerable<long> ids);
    Task<Dictionary<long, string>> GetServiceNamesAsync(long companyId, IEnumerable<long> ids);
    Task<Dictionary<long, string>> GetCategoryNamesAsync(long companyId, IEnumerable<long> ids);
    Task<Dictionary<long, string>> GetPriceListNamesAsync(long companyId, IEnumerable<long> ids);
    Task<long> InsertAsync(DiscountRule entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(DiscountRule entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class DiscountRuleRepository : TenantRepositoryBase, IDiscountRuleRepository
{
    public DiscountRuleRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<DiscountRule?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<DiscountRule>(connection,
            "SELECT * FROM dbo.DiscountRules WHERE DiscountRuleId = @id", new { id });
    }

    public async Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.DiscountRules
                WHERE CompanyId = @companyId AND Code = @code AND (@excludeId IS NULL OR DiscountRuleId <> @excludeId)) THEN 1 ELSE 0 END",
            new { companyId, code, excludeId });
    }

    public async Task<string> GetNextCodeAsync(long companyId, string prefix = "DR")
    {
        using var connection = OpenTenant();
        const string sql = @"
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(Code, LEN(@prefix) + 2, 10) AS INT)), 0) + 1
            FROM dbo.DiscountRules WHERE CompanyId = @companyId AND Code LIKE @prefix + '-%'";
        var next = await Sql.ExecuteScalarAsync<int>(connection, sql, new { companyId, prefix });
        return $"{prefix}-{next:D3}";
    }

    public async Task<IEnumerable<DiscountRule>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<DiscountRule>(connection, @"
            SELECT * FROM dbo.DiscountRules
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%')
            ORDER BY DiscountRuleId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.DiscountRules
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<Dictionary<long, string>> GetProductNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Where(i => i > 0).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, ProductName FROM dbo.Products WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<Dictionary<long, string>> GetServiceNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Where(i => i > 0).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT ServiceId, Name FROM dbo.Services WHERE CompanyId = @companyId AND ServiceId IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<Dictionary<long, string>> GetCategoryNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Where(i => i > 0).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, CategoryName FROM dbo.ProductCategories WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<Dictionary<long, string>> GetPriceListNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Where(i => i > 0).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT PriceListId, Name FROM dbo.PriceLists WHERE CompanyId = @companyId AND PriceListId IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<long> InsertAsync(DiscountRule entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.DiscountRules (CompanyId, Code, Name, ProductId, ServiceId, ProductCategoryId, PriceListId, DiscountType, DiscountValue, MinimumQuantity, MinimumAmount, MaximumDiscount, EffectiveFrom, EffectiveTo, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@CompanyId, @Code, @Name, @ProductId, @ServiceId, @ProductCategoryId, @PriceListId, @DiscountType, @DiscountValue, @MinimumQuantity, @MinimumAmount, @MaximumDiscount, @EffectiveFrom, @EffectiveTo, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(DiscountRule entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.DiscountRules
                SET Code = @Code, Name = @Name, ProductId = @ProductId, ServiceId = @ServiceId,
                    ProductCategoryId = @ProductCategoryId, PriceListId = @PriceListId, DiscountType = @DiscountType,
                    DiscountValue = @DiscountValue, MinimumQuantity = @MinimumQuantity, MinimumAmount = @MinimumAmount,
                    MaximumDiscount = @MaximumDiscount, EffectiveFrom = @EffectiveFrom, EffectiveTo = @EffectiveTo,
                    IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedAt = SYSUTCDATETIME()
                WHERE DiscountRuleId = @DiscountRuleId AND CompanyId = @CompanyId;";
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
            const string sql = "UPDATE dbo.DiscountRules SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE DiscountRuleId = @id AND CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { id, companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

/* ---------------- Offers (company) ---------------- */

public interface IOfferRepository
{
    Task<Offer?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null);
    Task<string> GetNextCodeAsync(long companyId, string prefix = "OFFER");
    Task<IEnumerable<Offer>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<Dictionary<long, string>> GetOfferNamesAsync(long companyId, IEnumerable<long> ids);
    Task<long> InsertAsync(Offer entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Offer entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class OfferRepository : TenantRepositoryBase, IOfferRepository
{
    public OfferRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<Offer?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Offer>(connection,
            "SELECT * FROM dbo.Offers WHERE OfferId = @id", new { id });
    }

    public async Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.Offers
                WHERE CompanyId = @companyId AND Code = @code AND (@excludeId IS NULL OR OfferId <> @excludeId)) THEN 1 ELSE 0 END",
            new { companyId, code, excludeId });
    }

    public async Task<string> GetNextCodeAsync(long companyId, string prefix = "OFFER")
    {
        using var connection = OpenTenant();
        const string sql = @"
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(Code, LEN(@prefix) + 2, 10) AS INT)), 0) + 1
            FROM dbo.Offers WHERE CompanyId = @companyId AND Code LIKE @prefix + '-%'";
        var next = await Sql.ExecuteScalarAsync<int>(connection, sql, new { companyId, prefix });
        return $"{prefix}-{next:D3}";
    }

    public async Task<IEnumerable<Offer>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<Offer>(connection, @"
            SELECT * FROM dbo.Offers
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')
            ORDER BY OfferId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Offers
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%' OR Description LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<Dictionary<long, string>> GetOfferNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Where(i => i > 0).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT OfferId, Name FROM dbo.Offers WHERE CompanyId = @companyId AND OfferId IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<long> InsertAsync(Offer entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Offers (CompanyId, Code, Name, OfferType, DiscountType, DiscountValue, MinimumQuantity, MinimumAmount, MaximumDiscount, StartDate, EndDate, IsActive, Description, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@CompanyId, @Code, @Name, @OfferType, @DiscountType, @DiscountValue, @MinimumQuantity, @MinimumAmount, @MaximumDiscount, @StartDate, @EndDate, @IsActive, @Description, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Offer entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Offers
                SET Code = @Code, Name = @Name, OfferType = @OfferType, DiscountType = @DiscountType,
                    DiscountValue = @DiscountValue, MinimumQuantity = @MinimumQuantity, MinimumAmount = @MinimumAmount,
                    MaximumDiscount = @MaximumDiscount, StartDate = @StartDate, EndDate = @EndDate,
                    IsActive = @IsActive, Description = @Description, ModifiedBy = @ModifiedBy, ModifiedAt = SYSUTCDATETIME()
                WHERE OfferId = @OfferId AND CompanyId = @CompanyId;";
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
            const string sql = "UPDATE dbo.Offers SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE OfferId = @id AND CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { id, companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

/* ---------------- Offer Details (child of Offer) ---------------- */

public interface IOfferDetailRepository
{
    Task<OfferDetail?> GetByIdAsync(long id);
    Task<IEnumerable<OfferDetail>> GetByOfferAsync(long offerId, bool includeInactive = false);
    Task<bool> TargetInUseAsync(long offerId, long? productId, long? serviceId, long? productCategoryId, long? excludeId = null);
    Task<long> InsertAsync(OfferDetail entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(OfferDetail entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> DeleteByOfferAsync(long offerId, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<Dictionary<long, string>> GetProductNamesAsync(long companyId, IEnumerable<long> ids);
    Task<Dictionary<long, string>> GetServiceNamesAsync(long companyId, IEnumerable<long> ids);
    Task<Dictionary<long, string>> GetCategoryNamesAsync(long companyId, IEnumerable<long> ids);
}

public class OfferDetailRepository : TenantRepositoryBase, IOfferDetailRepository
{
    public OfferDetailRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<OfferDetail?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<OfferDetail>(connection,
            "SELECT * FROM dbo.OfferDetails WHERE OfferDetailId = @id", new { id });
    }

    public async Task<IEnumerable<OfferDetail>> GetByOfferAsync(long offerId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.OfferDetails WHERE OfferId = @offerId ORDER BY OfferDetailId"
            : "SELECT * FROM dbo.OfferDetails WHERE OfferId = @offerId AND IsActive = 1 ORDER BY OfferDetailId";
        return await Sql.QueryAsync<OfferDetail>(connection, sql, new { offerId });
    }

    public async Task<bool> TargetInUseAsync(long offerId, long? productId, long? serviceId, long? productCategoryId, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.OfferDetails
                WHERE OfferId = @offerId
                  AND ISNULL(ProductId, 0) = ISNULL(@productId, 0)
                  AND ISNULL(ServiceId, 0) = ISNULL(@serviceId, 0)
                  AND ISNULL(ProductCategoryId, 0) = ISNULL(@productCategoryId, 0)
                  AND (@excludeId IS NULL OR OfferDetailId <> @excludeId)) THEN 1 ELSE 0 END",
            new { offerId, productId, serviceId, productCategoryId, excludeId });
    }

    public async Task<long> InsertAsync(OfferDetail entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.OfferDetails (OfferId, ProductId, ServiceId, ProductCategoryId, MinimumQuantity, FreeQuantity, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@OfferId, @ProductId, @ServiceId, @ProductCategoryId, @MinimumQuantity, @FreeQuantity, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(OfferDetail entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.OfferDetails
                SET ProductId = @ProductId, ServiceId = @ServiceId, ProductCategoryId = @ProductCategoryId,
                    MinimumQuantity = @MinimumQuantity, FreeQuantity = @FreeQuantity, IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy, ModifiedAt = SYSUTCDATETIME()
                WHERE OfferDetailId = @OfferDetailId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(long id, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.OfferDetails SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE OfferDetailId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> DeleteByOfferAsync(long offerId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "DELETE FROM dbo.OfferDetails WHERE OfferId = @offerId;";
            return await Sql.ExecuteAsync(conn, sql, new { offerId }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<Dictionary<long, string>> GetProductNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Where(i => i > 0).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, ProductName FROM dbo.Products WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<Dictionary<long, string>> GetServiceNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Where(i => i > 0).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT ServiceId, Name FROM dbo.Services WHERE CompanyId = @companyId AND ServiceId IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<Dictionary<long, string>> GetCategoryNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Where(i => i > 0).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, CategoryName FROM dbo.ProductCategories WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }
}

/* ---------------- Coupons (company) ---------------- */

public interface ICouponRepository
{
    Task<Coupon?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null);
    Task<string> GetNextCodeAsync(long companyId, string prefix = "COUPON");
    Task<IEnumerable<Coupon>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<Dictionary<long, string>> GetOfferNamesAsync(long companyId, IEnumerable<long> ids);
    Task<long> InsertAsync(Coupon entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Coupon entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class CouponRepository : TenantRepositoryBase, ICouponRepository
{
    public CouponRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<Coupon?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Coupon>(connection,
            "SELECT * FROM dbo.Coupons WHERE CouponId = @id", new { id });
    }

    public async Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.Coupons
                WHERE CompanyId = @companyId AND Code = @code AND (@excludeId IS NULL OR CouponId <> @excludeId)) THEN 1 ELSE 0 END",
            new { companyId, code, excludeId });
    }

    public async Task<string> GetNextCodeAsync(long companyId, string prefix = "COUPON")
    {
        using var connection = OpenTenant();
        const string sql = @"
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(Code, LEN(@prefix) + 2, 10) AS INT)), 0) + 1
            FROM dbo.Coupons WHERE CompanyId = @companyId AND Code LIKE @prefix + '-%'";
        var next = await Sql.ExecuteScalarAsync<int>(connection, sql, new { companyId, prefix });
        return $"{prefix}-{next:D3}";
    }

    public async Task<IEnumerable<Coupon>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<Coupon>(connection, @"
            SELECT * FROM dbo.Coupons
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%')
            ORDER BY CouponId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Coupons
            WHERE CompanyId = @companyId AND IsActive = 1
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR Name LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<Dictionary<long, string>> GetOfferNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Where(i => i > 0).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT OfferId, Name FROM dbo.Offers WHERE CompanyId = @companyId AND OfferId IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }

    public async Task<long> InsertAsync(Coupon entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Coupons (CompanyId, OfferId, Code, Name, UsageLimit, UsagePerCustomer, UsedCount, StartDate, EndDate, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@CompanyId, @OfferId, @Code, @Name, @UsageLimit, @UsagePerCustomer, @UsedCount, @StartDate, @EndDate, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Coupon entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Coupons
                SET OfferId = @OfferId, Code = @Code, Name = @Name, UsageLimit = @UsageLimit, UsagePerCustomer = @UsagePerCustomer,
                    UsedCount = @UsedCount, StartDate = @StartDate, EndDate = @EndDate, IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy, ModifiedAt = SYSUTCDATETIME()
                WHERE CouponId = @CouponId AND CompanyId = @CompanyId;";
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
            const string sql = "UPDATE dbo.Coupons SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE CouponId = @id AND CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { id, companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}