using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface ITimeZoneRepository
{
    Task<IEnumerable<Timezone>> GetAllAsync(bool includeInactive = false);
    Task<Timezone?> GetByIdAsync(int id);
    Task<Timezone?> GetByTimeZoneNameAsync(string timeZoneName);
    Task<int> InsertAsync(Timezone entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Timezone entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class TimeZoneRepository : TenantRepositoryBase, ITimeZoneRepository
{
    public TimeZoneRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<Timezone>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.TimeZones WHERE IsDeleted = 0 ORDER BY [Name]"
            : "SELECT * FROM dbo.TimeZones WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY [Name]";
        return await Sql.QueryAsync<Timezone>(connection, sql);
    }

    public async Task<Timezone?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Timezone>(connection,
            "SELECT * FROM dbo.TimeZones WHERE TimeZoneId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<Timezone?> GetByTimeZoneNameAsync(string timeZoneName)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Timezone>(connection,
            "SELECT * FROM dbo.TimeZones WHERE TimeZoneName = @timeZoneName AND IsDeleted = 0", new { timeZoneName });
    }

    public async Task<int> InsertAsync(Timezone entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.TimeZones ([Name], TimeZoneName, UTCOffset, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@Name, @TimeZoneName, @UTCOffset, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Timezone entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.TimeZones
                SET [Name] = @Name,
                    TimeZoneName = @TimeZoneName,
                    UTCOffset = @UTCOffset,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE TimeZoneId = @TimezoneId;";
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
            const string sql = "UPDATE dbo.TimeZones SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE TimeZoneId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
