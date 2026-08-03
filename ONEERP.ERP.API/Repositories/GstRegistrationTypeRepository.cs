using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IGstRegistrationTypeRepository
{
    Task<IEnumerable<GstRegistrationType>> GetAllAsync(bool includeInactive = false);
    Task<GstRegistrationType?> GetByIdAsync(int id);
    Task<GstRegistrationType?> GetByNameAsync(string name);
    Task<int> InsertAsync(GstRegistrationType entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(GstRegistrationType entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class GstRegistrationTypeRepository : TenantRepositoryBase, IGstRegistrationTypeRepository
{
    public GstRegistrationTypeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<GstRegistrationType>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.GSTRegistrationTypes WHERE IsDeleted = 0 ORDER BY [Name]"
            : "SELECT * FROM dbo.GSTRegistrationTypes WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY [Name]";
        return await Sql.QueryAsync<GstRegistrationType>(connection, sql);
    }

    public async Task<GstRegistrationType?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<GstRegistrationType>(connection,
            "SELECT * FROM dbo.GSTRegistrationTypes WHERE GstRegistrationTypeId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<GstRegistrationType?> GetByNameAsync(string name)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<GstRegistrationType>(connection,
            "SELECT * FROM dbo.GSTRegistrationTypes WHERE [Name] = @name AND IsDeleted = 0", new { name });
    }

    public async Task<int> InsertAsync(GstRegistrationType entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.GSTRegistrationTypes ([Name], [Description], IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@Name, @Description, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(GstRegistrationType entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.GSTRegistrationTypes
                SET [Name] = @Name,
                    [Description] = @Description,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE GstRegistrationTypeId = @GstRegistrationTypeId;";
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
            const string sql = "UPDATE dbo.GSTRegistrationTypes SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE GstRegistrationTypeId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
