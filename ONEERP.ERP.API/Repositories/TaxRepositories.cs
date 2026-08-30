using System.Data;
using ONEERP.ERP.API.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface ITaxTypeSystemRepository
{
    Task<(IEnumerable<TaxTypeSystem> Items, int Total)> GetPagedAsync(int page, int size, string search);
    Task<TaxTypeSystem?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(string code, long? excludeId = null);
    Task<long> InsertAsync(TaxTypeSystem entity);
    Task<int> UpdateAsync(TaxTypeSystem entity);
    Task<int> SoftDeleteAsync(long id);
    Task<string> GetNextCodeAsync(string prefix = "TAXTYPE");
    Task<Dictionary<long, string>> GetNamesAsync(IEnumerable<long> ids);
}

public class TaxTypeSystemRepository : TenantRepositoryBase, ITaxTypeSystemRepository
{
    public TaxTypeSystemRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<(IEnumerable<TaxTypeSystem> Items, int Total)> GetPagedAsync(int page, int size, string search)
    {
        var offset = (page - 1) * size;
        var like = $"%{search ?? string.Empty}%";
        using var connection = OpenTenant();
        var items = await Sql.QueryAsync<TaxTypeSystem>(connection,
            "SELECT Id, Code, Name, Description, IsActive, CreatedBy, CreatedAt FROM dbo.TaxTypeSystems " +
            "WHERE (@search = '' OR Code LIKE @search OR Name LIKE @search) " +
            "ORDER BY Id OFFSET @offset ROWS FETCH NEXT @size ROWS ONLY",
            new { search = like, offset, size });
        var total = await Sql.ExecuteScalarAsync<int>(connection,
            "SELECT COUNT(1) FROM dbo.TaxTypeSystems WHERE (@search = '' OR Code LIKE @search OR Name LIKE @search)",
            new { search = like });
        return (items, total);
    }

    public async Task<TaxTypeSystem?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<TaxTypeSystem>(connection,
            "SELECT Id, Code, Name, Description, IsActive, CreatedBy, CreatedAt FROM dbo.TaxTypeSystems WHERE Id = @id",
            new { id });
    }

    public async Task<bool> CodeInUseAsync(string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.TaxTypeSystems WHERE Code = @code AND (@excl IS NULL OR Id <> @excl)) THEN 1 ELSE 0 END",
            new { code, excl = excludeId });
    }

    public async Task<long> InsertAsync(TaxTypeSystem entity)
    {
        using var connection = OpenTenant();
        entity.Id = await Sql.ExecuteScalarAsync<long>(connection,
            "INSERT INTO dbo.TaxTypeSystems (Code, Name, Description, IsActive, CreatedBy, CreatedAt) " +
            "VALUES (@Code, @Name, @Description, @IsActive, @CreatedBy, GETDATE()); " +
            "SELECT CAST(SCOPE_IDENTITY() AS BIGINT);",
            entity);
        return entity.Id;
    }

    public async Task<int> UpdateAsync(TaxTypeSystem entity)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteAsync(connection,
            "UPDATE dbo.TaxTypeSystems SET Code = @Code, Name = @Name, Description = @Description, IsActive = @IsActive WHERE Id = @Id",
            entity);
    }

    public async Task<int> SoftDeleteAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteAsync(connection, "UPDATE dbo.TaxTypeSystems SET IsActive = 0 WHERE Id = @id", new { id });
    }

    public async Task<string> GetNextCodeAsync(string prefix = "TAXTYPE")
    {
        using var connection = OpenTenant();
        var next = await Sql.ExecuteScalarAsync<int>(connection,
            "SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(Code, LEN(@prefix) + 2, 10) AS INT)), 0) + 1 " +
            "FROM dbo.TaxTypeSystems WHERE Code LIKE @prefix + '-%'",
            new { prefix });
        return $"{prefix}-{next:D3}";
    }

    public async Task<Dictionary<long, string>> GetNamesAsync(IEnumerable<long> ids)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, Name FROM dbo.TaxTypeSystems WHERE Id IN @ids", new { ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }
}

/* ---------------- Taxes (rates) ---------------- */

public interface ITaxRepository
{
    Task<IEnumerable<Tax>> GetPagedAsync(long companyId, int page, int size, string search);
    Task<int> CountAsync(long companyId, string search);
    Task<Tax?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null);
    Task<string> GetNextCodeAsync(long companyId, string prefix = "TAX");
    Task<long> InsertAsync(Tax entity);
    Task<bool> UpdateAsync(Tax entity);
    Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy);
    Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids);
}

public class TaxRepository : TenantRepositoryBase, ITaxRepository
{
    public TaxRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<Tax>> GetPagedAsync(long companyId, int page, int size, string search)
    {
        var offset = (page - 1) * size;
        var like = $"%{search ?? string.Empty}%";
        using var connection = OpenTenant();
        return await Sql.QueryAsync<Tax>(connection,
            "SELECT Id, CompanyId, BranchId, TaxTypeSystemId, TaxCode, TaxName, TaxRate, IsInclusive, " +
            "EffectiveFrom, EffectiveTo, IsActive, Description, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt " +
            "FROM dbo.Taxes " +
            "WHERE CompanyId = @companyId AND (@search = '' OR TaxCode LIKE @search OR TaxName LIKE @search) " +
            "ORDER BY Id OFFSET @offset ROWS FETCH NEXT @size ROWS ONLY",
            new { companyId, search = like, offset, size });
    }

    public async Task<int> CountAsync(long companyId, string search)
    {
        var like = $"%{search ?? string.Empty}%";
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection,
            "SELECT COUNT(1) FROM dbo.Taxes WHERE CompanyId = @companyId AND (@search = '' OR TaxCode LIKE @search OR TaxName LIKE @search)",
            new { companyId, search = like });
    }

    public async Task<Tax?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Tax>(connection,
            "SELECT Id, CompanyId, BranchId, TaxTypeSystemId, TaxCode, TaxName, TaxRate, IsInclusive, " +
            "EffectiveFrom, EffectiveTo, IsActive, Description, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt " +
            "FROM dbo.Taxes WHERE Id = @id", new { id });
    }

    public async Task<bool> CodeInUseAsync(long companyId, string code, long? excludeId = null)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Taxes WHERE CompanyId = @companyId AND TaxCode = @code AND (@excl IS NULL OR Id <> @excl)) THEN 1 ELSE 0 END",
            new { companyId, code, excl = excludeId });
    }

    public async Task<string> GetNextCodeAsync(long companyId, string prefix = "TAX")
    {
        using var connection = OpenTenant();
        var next = await Sql.ExecuteScalarAsync<int>(connection,
            "SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(TaxCode, LEN(@prefix) + 2, 10) AS INT)), 0) + 1 " +
            "FROM dbo.Taxes WHERE CompanyId = @companyId AND TaxCode LIKE @prefix + '-%'",
            new { companyId, prefix });
        return $"{prefix}-{next:D4}";
    }

    public async Task<long> InsertAsync(Tax entity)
    {
        using var connection = OpenTenant();
        entity.Id = await Sql.ExecuteScalarAsync<long>(connection,
            "INSERT INTO dbo.Taxes (CompanyId, BranchId, TaxTypeSystemId, TaxCode, TaxName, TaxRate, IsInclusive, " +
            "EffectiveFrom, EffectiveTo, IsActive, Description, CreatedBy, CreatedAt) " +
            "VALUES (@CompanyId, @BranchId, @TaxTypeSystemId, @TaxCode, @TaxName, @TaxRate, @IsInclusive, " +
            "@EffectiveFrom, @EffectiveTo, @IsActive, @Description, @CreatedBy, GETDATE()); " +
            "SELECT CAST(SCOPE_IDENTITY() AS BIGINT);",
            entity);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(Tax entity)
    {
        using var connection = OpenTenant();
        var rows = await Sql.ExecuteAsync(connection,
            "UPDATE dbo.Taxes SET BranchId = @BranchId, TaxTypeSystemId = @TaxTypeSystemId, TaxCode = @TaxCode, " +
            "TaxName = @TaxName, TaxRate = @TaxRate, IsInclusive = @IsInclusive, EffectiveFrom = @EffectiveFrom, " +
            "EffectiveTo = @EffectiveTo, IsActive = @IsActive, Description = @Description, ModifiedBy = @ModifiedBy, " +
            "ModifiedAt = GETDATE() WHERE Id = @Id",
            entity);
        return rows > 0;
    }

    public async Task<bool> SoftDeleteAsync(long id, long companyId, long modifiedBy)
    {
        using var connection = OpenTenant();
        var rows = await Sql.ExecuteAsync(connection,
            "UPDATE dbo.Taxes SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = GETDATE() " +
            "WHERE Id = @id AND CompanyId = @companyId", new { id, companyId, modifiedBy });
        return rows > 0;
    }

    public async Task<Dictionary<long, string>> GetNamesAsync(long companyId, IEnumerable<long> ids)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return new Dictionary<long, string>();
        using var connection = OpenTenant();
        var rows = await Sql.QueryAsync<(long Id, string Name)>(connection,
            "SELECT Id, TaxName FROM dbo.Taxes WHERE CompanyId = @companyId AND Id IN @ids",
            new { companyId, ids = list });
        return rows.ToDictionary(r => r.Id, r => r.Name);
    }
}
