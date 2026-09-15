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

public interface IBusinessPartnerRoleRepository
{
    Task<IEnumerable<BusinessPartnerRole>> GetAllAsync(bool includeInactive = false);
    Task<BusinessPartnerRole?> GetByIdAsync(int id);
    Task<BusinessPartnerRole?> GetByCodeAsync(string code);
    Task<int> InsertAsync(BusinessPartnerRole entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(BusinessPartnerRole entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class BusinessPartnerRoleRepository : TenantRepositoryBase, IBusinessPartnerRoleRepository
{
    public BusinessPartnerRoleRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<BusinessPartnerRole>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.BusinessPartnerRoles WHERE IsDeleted = 0 ORDER BY [Code]"
            : "SELECT * FROM dbo.BusinessPartnerRoles WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY [Code]";
        return await Sql.QueryAsync<BusinessPartnerRole>(connection, sql);
    }

    public async Task<BusinessPartnerRole?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<BusinessPartnerRole>(connection,
            "SELECT * FROM dbo.BusinessPartnerRoles WHERE BusinessPartnerRoleId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<BusinessPartnerRole?> GetByCodeAsync(string code)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<BusinessPartnerRole>(connection,
            "SELECT * FROM dbo.BusinessPartnerRoles WHERE [Code] = @code AND IsDeleted = 0", new { code });
    }

    public async Task<int> InsertAsync(BusinessPartnerRole entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.BusinessPartnerRoles ([Code], [Name], [Description], IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@Code, @Name, @Description, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(BusinessPartnerRole entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.BusinessPartnerRoles
                SET [Code] = @Code,
                    [Name] = @Name,
                    [Description] = @Description,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE BusinessPartnerRoleId = @BusinessPartnerRoleId;";
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
            const string sql = "UPDATE dbo.BusinessPartnerRoles SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE BusinessPartnerRoleId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

public interface IBusinessPartnerRepository
{
    Task<IEnumerable<BusinessPartner>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<BusinessPartner?> GetByIdAsync(long id);
    Task<BusinessPartner?> GetByCodeAsync(long companyId, string code);
    Task<string> GetNextCodeAsync(long companyId, string prefix = "BP");
    Task<long> InsertAsync(BusinessPartner entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(BusinessPartner entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> DeleteAsync(long id, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class BusinessPartnerRepository : TenantRepositoryBase, IBusinessPartnerRepository
{
    public BusinessPartnerRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<BusinessPartner>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = "SELECT * FROM dbo.BusinessPartners WHERE CompanyId = @companyId AND (IsActive = 1 OR @includeInactive = 1) ORDER BY PartnerName";
        return await Sql.QueryAsync<BusinessPartner>(connection, sql, new { companyId, includeInactive });
    }

    public async Task<BusinessPartner?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<BusinessPartner>(connection,
            "SELECT * FROM dbo.BusinessPartners WHERE Id = @id", new { id });
    }

    public async Task<BusinessPartner?> GetByCodeAsync(long companyId, string code)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<BusinessPartner>(connection,
            "SELECT * FROM dbo.BusinessPartners WHERE CompanyId = @companyId AND PartnerCode = @code", new { companyId, code });
    }

    public async Task<string> GetNextCodeAsync(long companyId, string prefix = "BP")
    {
        using var connection = OpenTenant();
        const string sql = @"
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(PartnerCode, LEN(@prefix) + 2, 10) AS INT)), 0) + 1
            FROM dbo.BusinessPartners WHERE CompanyId = @companyId AND PartnerCode LIKE @prefix + '-%'";
        var next = await Sql.ExecuteScalarAsync<int>(connection, sql, new { companyId, prefix });
        return $"{prefix}-{next:D3}";
    }

    public async Task<long> InsertAsync(BusinessPartner entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.BusinessPartners (CompanyId, PartnerCode, PartnerName, PatnerRoleIds, ContactPerson, MobileNo, Email, TaxRegistrationNo, CreditLimit, CreditDays, PaymentTermId, CurrencyId, PriceListId, Notes, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@CompanyId, @PartnerCode, @PartnerName, @PatnerRoleIds, @ContactPerson, @MobileNo, @Email, @TaxRegistrationNo, @CreditLimit, @CreditDays, @PaymentTermId, @CurrencyId, @PriceListId, @Notes, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(BusinessPartner entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.BusinessPartners
                SET PartnerCode = @PartnerCode,
                    PartnerName = @PartnerName,
                    PatnerRoleIds = @PatnerRoleIds,
                    ContactPerson = @ContactPerson,
                    MobileNo = @MobileNo,
                    Email = @Email,
                    TaxRegistrationNo = @TaxRegistrationNo,
                    CreditLimit = @CreditLimit,
                    CreditDays = @CreditDays,
                    PaymentTermId = @PaymentTermId,
                    CurrencyId = @CurrencyId,
                    PriceListId = @PriceListId,
                    Notes = @Notes,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedAt = SYSUTCDATETIME()
                WHERE Id = @Id;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> DeleteAsync(long id, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "DELETE FROM dbo.BusinessPartners WHERE Id = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id }, transaction) > 0;
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

public interface IPermissionModuleRepository
{
    Task<IEnumerable<PermissionModule>> GetAllAsync(bool includeInactive = false, bool includeDeleted = false);
    Task<PermissionModule?> GetByIdAsync(int id);
    Task<PermissionModule?> GetByCodeAsync(string code);
    Task<IEnumerable<PermissionModule>> GetByLevelAsync(string level);
    Task<int> InsertAsync(PermissionModule entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(PermissionModule entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class PermissionModuleRepository : TenantRepositoryBase, IPermissionModuleRepository
{
    public PermissionModuleRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<PermissionModule>> GetAllAsync(bool includeInactive = false, bool includeDeleted = false)
    {
        using var connection = OpenTenant();
        var sql = @"SELECT * FROM dbo.PermissionModules WHERE 1=1";

        if (!includeDeleted)
        {
            sql += " AND IsDeleted = 0";
        }
        if (!includeInactive)
        {
            sql += " AND IsVisible = 1";
        }
        sql += " ORDER BY SortOrder";

        return await Sql.QueryAsync<PermissionModule>(connection, sql);
    }

    public async Task<PermissionModule?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<PermissionModule>(connection,
            "SELECT * FROM dbo.PermissionModules WHERE Id = @id AND IsDeleted = 0", new { id });
    }

    public async Task<PermissionModule?> GetByCodeAsync(string code)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<PermissionModule>(connection,
            "SELECT * FROM dbo.PermissionModules WHERE Code = @code AND IsDeleted = 0", new { code });
    }

    public async Task<IEnumerable<PermissionModule>> GetByLevelAsync(string level)
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<PermissionModule>(connection,
            "SELECT * FROM dbo.PermissionModules WHERE Level = @level AND IsDeleted = 0 ORDER BY SortOrder", new { level });
    }

    public async Task<int> InsertAsync(PermissionModule entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.PermissionModules (Code, [Name], ParentId, [Level], SortOrder, IsVisible, Icon, RoutePath, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@Code, @Name, @ParentId, @Level, @SortOrder, @IsVisible, @Icon, @RoutePath, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(PermissionModule entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.PermissionModules
                SET Code = @Code,
                    [Name] = @Name,
                    ParentId = @ParentId,
                    [Level] = @Level,
                    SortOrder = @SortOrder,
                    IsVisible = @IsVisible,
                    Icon = @Icon,
                    RoutePath = @RoutePath,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE Id = @Id AND IsDeleted = 0;";
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
            const string sql = @"
                UPDATE dbo.PermissionModules 
                SET IsDeleted = 1, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() 
                WHERE Id = @id AND IsDeleted = 0;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

public interface IPermissionActionRepository
{
    Task<IEnumerable<PermissionAction>> GetAllAsync(bool includeInactive = false);
    Task<PermissionAction?> GetByIdAsync(int id);
    Task<PermissionAction?> GetByCodeAsync(string code);
    Task<int> InsertAsync(PermissionAction entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(PermissionAction entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class PermissionActionRepository : TenantRepositoryBase, IPermissionActionRepository
{
    public PermissionActionRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<PermissionAction>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.PermissionActions ORDER BY SortOrder"
            : "SELECT * FROM dbo.PermissionActions WHERE IsActive = 1 ORDER BY SortOrder";
        return await Sql.QueryAsync<PermissionAction>(connection, sql);
    }

    public async Task<PermissionAction?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<PermissionAction>(connection,
            "SELECT * FROM dbo.PermissionActions WHERE Id = @id", new { id });
    }

    public async Task<PermissionAction?> GetByCodeAsync(string code)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<PermissionAction>(connection,
            "SELECT * FROM dbo.PermissionActions WHERE Code = @code", new { code });
    }

    public async Task<int> InsertAsync(PermissionAction entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.PermissionActions (Code, [Name], SortOrder, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@Code, @Name, @SortOrder, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(PermissionAction entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.PermissionActions
                SET Code = @Code,
                    [Name] = @Name,
                    SortOrder = @SortOrder,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE Id = @Id;";
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
            const string sql = @"
                UPDATE dbo.PermissionActions 
                SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() 
                WHERE Id = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

public interface IModulePermissionRepository
{
    Task<IEnumerable<ModulePermission>> GetByRoleAsync(int roleId);
    Task<IEnumerable<ModulePermission>> GetByModuleAsync(int moduleId);
    Task<IEnumerable<ModulePermission>> GetByUserAsync(int userId);
    Task<IEnumerable<ModulePermission>> GetScopedAsync(string scope, int? scopeId = null);
    Task<bool> AssignAsync(int roleId, int moduleId, int actionId, string scope, int? scopeId, string grantedBy);
    Task<bool> RevokeAsync(int roleId, int moduleId, int actionId, string scope, int? scopeId, string revokedBy);
    Task<bool> RevokeAllForRoleAsync(int roleId);
}

public class ModulePermissionRepository : TenantRepositoryBase, IModulePermissionRepository
{
    public ModulePermissionRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<ModulePermission>> GetByRoleAsync(int roleId)
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<ModulePermission>(connection,
            "SELECT * FROM dbo.ModulePermissions WHERE RoleId = @roleId AND IsRevoked = 0", new { roleId });
    }

    public async Task<IEnumerable<ModulePermission>> GetByModuleAsync(int moduleId)
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<ModulePermission>(connection,
            "SELECT * FROM dbo.ModulePermissions WHERE PermissionModuleId = @moduleId AND IsRevoked = 0", new { moduleId });
    }

    public async Task<IEnumerable<ModulePermission>> GetByUserAsync(int userId)
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<ModulePermission>(connection, @"
            SELECT mp.*
            FROM dbo.ModulePermissions mp
            INNER JOIN dbo.UserRoles ur ON ur.RoleId = mp.RoleId
            WHERE ur.UserId = @userId AND mp.IsRevoked = 0
            ORDER BY mp.PermissionModuleId, mp.PermissionActionId", new { userId });
    }

    public async Task<IEnumerable<ModulePermission>> GetScopedAsync(string scope, int? scopeId = null)
    {
        using var connection = OpenTenant();
        if (scopeId.HasValue)
        {
            return await Sql.QueryAsync<ModulePermission>(connection,
                "SELECT * FROM dbo.ModulePermissions WHERE Scope = @scope AND ScopeId = @scopeId AND IsRevoked = 0",
                new { scope, scopeId });
        }
        return await Sql.QueryAsync<ModulePermission>(connection,
            "SELECT * FROM dbo.ModulePermissions WHERE Scope = @scope AND IsRevoked = 0", new { scope });
    }

    public async Task<bool> AssignAsync(int roleId, int moduleId, int actionId, string scope, int? scopeId, string grantedBy)
    {
        using var connection = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.ModulePermissions (RoleId, PermissionModuleId, PermissionActionId, Scope, ScopeId, GrantedBy, GrantedDate, IsRevoked)
            VALUES (@roleId, @moduleId, @actionId, @scope, @scopeId, @grantedBy, SYSUTCDATETIME(), 0);";
        return await Sql.ExecuteAsync(connection, sql, new { roleId, moduleId, actionId, scope, scopeId, grantedBy }) > 0;
    }

    public async Task<bool> RevokeAsync(int roleId, int moduleId, int actionId, string scope, int? scopeId, string revokedBy)
    {
        using var connection = OpenTenant();
        const string sql = @"
            UPDATE dbo.ModulePermissions 
            SET IsRevoked = 1, RevokedBy = @revokedBy, RevokedDate = SYSUTCDATETIME()
            WHERE RoleId = @roleId AND PermissionModuleId = @moduleId 
              AND PermissionActionId = @actionId AND Scope = @scope 
              AND (@scopeId IS NULL OR ScopeId = @scopeId) 
              AND IsRevoked = 0;";
        return await Sql.ExecuteAsync(connection, sql, new { roleId, moduleId, actionId, scope, scopeId, revokedBy }) > 0;
    }

    public async Task<bool> RevokeAllForRoleAsync(int roleId)
    {
        using var connection = OpenTenant();
        const string sql = @"
            UPDATE dbo.ModulePermissions 
            SET IsRevoked = 1, RevokedDate = SYSUTCDATETIME()
            WHERE RoleId = @roleId AND IsRevoked = 0;";
        return await Sql.ExecuteAsync(connection, sql, new { roleId }) > 0;
    }
}

public interface IFieldPermissionRepository
{
    Task<IEnumerable<FieldPermission>> GetByRoleModuleAsync(int roleId, int moduleId);
    Task<FieldPermission?> GetByRoleModuleFieldAsync(int roleId, int moduleId, string fieldName, string scope, int? scopeId);
    Task<int> InsertAsync(FieldPermission entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(FieldPermission entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> DeleteAsync(int id, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class FieldPermissionRepository : TenantRepositoryBase, IFieldPermissionRepository
{
    public FieldPermissionRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<FieldPermission>> GetByRoleModuleAsync(int roleId, int moduleId)
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<FieldPermission>(connection,
            @"SELECT * FROM dbo.FieldPermissions 
              WHERE RoleId = @roleId AND PermissionModuleId = @moduleId 
              ORDER BY FieldName", new { roleId, moduleId });
    }

    public async Task<FieldPermission?> GetByRoleModuleFieldAsync(int roleId, int moduleId, string fieldName, string scope, int? scopeId)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<FieldPermission>(connection,
            @"SELECT * FROM dbo.FieldPermissions 
              WHERE RoleId = @roleId AND PermissionModuleId = @moduleId 
                AND FieldName = @fieldName AND Scope = @scope AND ScopeId = @scopeId",
            new { roleId, moduleId, fieldName, scope, scopeId });
    }

    public async Task<int> InsertAsync(FieldPermission entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.FieldPermissions (RoleId, PermissionModuleId, FieldName, CanView, CanEdit, IsMandatory, IsHidden, Scope, ScopeId, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@RoleId, @PermissionModuleId, @FieldName, @CanView, @CanEdit, @IsMandatory, @IsHidden, @Scope, @ScopeId, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(FieldPermission entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.FieldPermissions SET
                    CanView = @CanView, CanEdit = @CanEdit, IsMandatory = @IsMandatory,
                    IsHidden = @IsHidden, ModifiedBy = @ModifiedBy, ModifiedDate = SYSUTCDATETIME()
                WHERE Id = @Id;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> DeleteAsync(int id, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "DELETE FROM dbo.FieldPermissions WHERE Id = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
