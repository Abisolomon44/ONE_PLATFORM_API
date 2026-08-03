using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IOrganizationTypeRepository
{
    Task<IEnumerable<OrganizationType>> GetAllAsync(bool includeInactive = false);
    Task<OrganizationType?> GetByIdAsync(int id);
    Task<OrganizationType?> GetByNameAsync(string name);
    Task<OrganizationType?> GetByCodeAsync(string code);
    Task<int> InsertAsync(OrganizationType entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(OrganizationType entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class OrganizationTypeRepository : TenantRepositoryBase, IOrganizationTypeRepository
{
    public OrganizationTypeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<OrganizationType>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.OrganizationTypes WHERE IsDeleted = 0 ORDER BY SortOrder, [Name]"
            : "SELECT * FROM dbo.OrganizationTypes WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY SortOrder, [Name]";
        return await Sql.QueryAsync<OrganizationType>(connection, sql);
    }

    public async Task<OrganizationType?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<OrganizationType>(connection,
            "SELECT * FROM dbo.OrganizationTypes WHERE OrganizationTypeId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<OrganizationType?> GetByNameAsync(string name)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<OrganizationType>(connection,
            "SELECT * FROM dbo.OrganizationTypes WHERE [Name] = @name AND IsDeleted = 0", new { name });
    }

    public async Task<OrganizationType?> GetByCodeAsync(string code)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<OrganizationType>(connection,
            "SELECT * FROM dbo.OrganizationTypes WHERE Code = @code AND IsDeleted = 0", new { code });
    }

    public async Task<int> InsertAsync(OrganizationType entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.OrganizationTypes ([Name], Code, [Description], SortOrder, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@Name, @Code, @Description, @SortOrder, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(OrganizationType entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.OrganizationTypes
                SET [Name] = @Name,
                    Code = @Code,
                    [Description] = @Description,
                    SortOrder = @SortOrder,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE OrganizationTypeId = @OrganizationTypeId;";
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
            const string sql = "UPDATE dbo.OrganizationTypes SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE OrganizationTypeId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
