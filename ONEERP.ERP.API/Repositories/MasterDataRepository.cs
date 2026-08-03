using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IBusinessTypeRepository
{
    Task<IEnumerable<BusinessType>> GetAllAsync(bool includeInactive = false);
    Task<BusinessType?> GetByIdAsync(int id);
    Task<BusinessType?> GetByNameAsync(string name);
    Task<int> InsertAsync(BusinessType entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(BusinessType entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class BusinessTypeRepository : TenantRepositoryBase, IBusinessTypeRepository
{
    public BusinessTypeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<BusinessType>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.BusinessTypes WHERE IsDeleted = 0 ORDER BY SortOrder, [Name]"
            : "SELECT * FROM dbo.BusinessTypes WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY SortOrder, [Name]";
        return await Sql.QueryAsync<BusinessType>(connection, sql);
    }

    public async Task<BusinessType?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<BusinessType>(connection,
            "SELECT * FROM dbo.BusinessTypes WHERE BusinessTypeId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<BusinessType?> GetByNameAsync(string name)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<BusinessType>(connection,
            "SELECT * FROM dbo.BusinessTypes WHERE [Name] = @name AND IsDeleted = 0", new { name });
    }

    public async Task<int> InsertAsync(BusinessType entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.BusinessTypes ([Name], [Description], SortOrder, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@Name, @Description, @SortOrder, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(BusinessType entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.BusinessTypes
                SET [Name] = @Name,
                    [Description] = @Description,
                    SortOrder = @SortOrder,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE BusinessTypeId = @BusinessTypeId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.BusinessTypes SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE BusinessTypeId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

public interface IIndustryTypeRepository
{
    Task<IEnumerable<IndustryType>> GetAllAsync(bool includeInactive = false);
    Task<IndustryType?> GetByIdAsync(int id);
    Task<IndustryType?> GetByNameAsync(string name);
    Task<int> InsertAsync(IndustryType entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(IndustryType entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class IndustryTypeRepository : TenantRepositoryBase, IIndustryTypeRepository
{
    public IndustryTypeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<IndustryType>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.IndustryTypes WHERE IsDeleted = 0 ORDER BY SortOrder, [Name]"
            : "SELECT * FROM dbo.IndustryTypes WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY SortOrder, [Name]";
        return await Sql.QueryAsync<IndustryType>(connection, sql);
    }

    public async Task<IndustryType?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<IndustryType>(connection,
            "SELECT * FROM dbo.IndustryTypes WHERE IndustryTypeId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<IndustryType?> GetByNameAsync(string name)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<IndustryType>(connection,
            "SELECT * FROM dbo.IndustryTypes WHERE [Name] = @name AND IsDeleted = 0", new { name });
    }

    public async Task<int> InsertAsync(IndustryType entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.IndustryTypes ([Name], [Description], SortOrder, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@Name, @Description, @SortOrder, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(IndustryType entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.IndustryTypes
                SET [Name] = @Name,
                    [Description] = @Description,
                    SortOrder = @SortOrder,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE IndustryTypeId = @IndustryTypeId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.IndustryTypes SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE IndustryTypeId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

public interface ICompanyGroupRepository
{
    Task<IEnumerable<CompanyGroup>> GetAllAsync(bool includeInactive = false);
    Task<CompanyGroup?> GetByIdAsync(int id);
    Task<CompanyGroup?> GetByCodeAsync(string code);
    Task<IEnumerable<CompanyGroup>> GetByParentAsync(int parentGroupId);
    Task<int> InsertAsync(CompanyGroup entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(CompanyGroup entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class CompanyGroupRepository : TenantRepositoryBase, ICompanyGroupRepository
{
    public CompanyGroupRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<CompanyGroup>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.CompanyGroups WHERE IsDeleted = 0 ORDER BY GroupName"
            : "SELECT * FROM dbo.CompanyGroups WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY GroupName";
        return await Sql.QueryAsync<CompanyGroup>(connection, sql);
    }

    public async Task<CompanyGroup?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<CompanyGroup>(connection,
            "SELECT * FROM dbo.CompanyGroups WHERE CompanyGroupId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<CompanyGroup?> GetByCodeAsync(string code)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<CompanyGroup>(connection,
            "SELECT * FROM dbo.CompanyGroups WHERE GroupCode = @code AND IsDeleted = 0", new { code });
    }

    public async Task<IEnumerable<CompanyGroup>> GetByParentAsync(int parentGroupId)
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<CompanyGroup>(connection,
            "SELECT * FROM dbo.CompanyGroups WHERE ParentGroupId = @parentGroupId AND IsDeleted = 0", new { parentGroupId });
    }

    public async Task<int> InsertAsync(CompanyGroup entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.CompanyGroups (GroupCode, GroupName, ShortName, [Description], ParentGroupId, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@GroupCode, @GroupName, @ShortName, @Description, @ParentGroupId, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(CompanyGroup entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.CompanyGroups
                SET GroupCode = @GroupCode,
                    GroupName = @GroupName,
                    ShortName = @ShortName,
                    [Description] = @Description,
                    ParentGroupId = @ParentGroupId,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE CompanyGroupId = @CompanyGroupId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.CompanyGroups SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE CompanyGroupId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
