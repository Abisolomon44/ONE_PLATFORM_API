using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IAddressTypeRepository
{
    Task<IEnumerable<AddressType>> GetAllAsync(bool includeInactive = false);
    Task<AddressType?> GetByIdAsync(int id);
    Task<AddressType?> GetByNameAsync(string name);
    Task<int> InsertAsync(AddressType entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(AddressType entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class AddressTypeRepository : TenantRepositoryBase, IAddressTypeRepository
{
    public AddressTypeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<AddressType>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.AddressTypes WHERE IsDeleted = 0 ORDER BY [Name]"
            : "SELECT * FROM dbo.AddressTypes WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY [Name]";
        return await Sql.QueryAsync<AddressType>(connection, sql);
    }

    public async Task<AddressType?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<AddressType>(connection,
            "SELECT * FROM dbo.AddressTypes WHERE AddressTypeId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<AddressType?> GetByNameAsync(string name)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<AddressType>(connection,
            "SELECT * FROM dbo.AddressTypes WHERE [Name] = @name AND IsDeleted = 0", new { name });
    }

    public async Task<int> InsertAsync(AddressType entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.AddressTypes ([Name], [Description], IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@Name, @Description, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(AddressType entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.AddressTypes
                SET [Name] = @Name,
                    [Description] = @Description,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE AddressTypeId = @AddressTypeId;";
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
            const string sql = "UPDATE dbo.AddressTypes SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE AddressTypeId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
