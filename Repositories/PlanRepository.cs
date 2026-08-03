using System.Data;
using ONEERP.Platform.API.Models;

namespace ONEERP.Platform.API.Repositories;

public interface IPlanRepository
{
    Task<IEnumerable<Plan>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<Plan>> GetPagedAsync(int pageNumber, int pageSize, string search);
    Task<int> CountAsync(string search);
    Task<int> CountActiveAsync();
    Task<Plan?> GetByIdAsync(int planId);
    Task<Plan?> GetByCodeAsync(string planCode);
    Task<int> InsertAsync(Plan plan, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Plan plan, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int planId, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class PlanRepository : BaseRepository, IPlanRepository
{
    public PlanRepository(IDbConnectionFactory factory, ISqlHelper sql) : base(factory, sql)
    {
    }

    public async Task<IEnumerable<Plan>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenPlatform();
        var sql = includeInactive
            ? "SELECT * FROM dbo.Plans WHERE IsDeleted = 0 ORDER BY PlanId DESC"
            : "SELECT * FROM dbo.Plans WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY PlanId DESC";
        return await Sql.QueryAsync<Plan>(connection, sql);
    }

    public async Task<IEnumerable<Plan>> GetPagedAsync(int pageNumber, int pageSize, string search)
    {
        using var connection = OpenPlatform();
        var offset = (pageNumber - 1) * pageSize;
        const string sql = @"
            SELECT * FROM dbo.Plans
            WHERE IsDeleted = 0
              AND (@search = '' OR PlanName LIKE '%' + @search + '%' OR PlanCode LIKE '%' + @search + '%')
            ORDER BY PlanId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";
        return await Sql.QueryAsync<Plan>(connection, sql, new { search, offset, pageSize });
    }

    public async Task<int> CountAsync(string search)
    {
        using var connection = OpenPlatform();
        const string sql = @"
            SELECT COUNT(1) FROM dbo.Plans
            WHERE IsDeleted = 0
              AND (@search = '' OR PlanName LIKE '%' + @search + '%' OR PlanCode LIKE '%' + @search + '%');";
        return await Sql.ExecuteScalarAsync<int>(connection, sql, new { search });
    }

    public async Task<int> CountActiveAsync()
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteScalarAsync<int>(connection, "SELECT COUNT(1) FROM dbo.Plans WHERE IsDeleted = 0 AND IsActive = 1");
    }

    public async Task<Plan?> GetByIdAsync(int planId)
    {
        using var connection = OpenPlatform();
        return await Sql.QuerySingleOrDefaultAsync<Plan>(
            connection, "SELECT * FROM dbo.Plans WHERE PlanId = @planId AND IsDeleted = 0", new { planId });
    }

    public async Task<Plan?> GetByCodeAsync(string planCode)
    {
        using var connection = OpenPlatform();
        return await Sql.QuerySingleOrDefaultAsync<Plan>(
            connection, "SELECT * FROM dbo.Plans WHERE PlanCode = @planCode AND IsDeleted = 0", new { planCode });
    }

    public async Task<int> InsertAsync(Plan plan, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenPlatform();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Plans (PlanCode, PlanName, [Description], CountryCode, CountryName, CurrencyCode, MonthlyPrice, AnnualPrice, MaxUsers, MaxCompanies, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@PlanCode, @PlanName, @Description, @CountryCode, @CountryName, @CurrencyCode, @MonthlyPrice, @AnnualPrice, @MaxUsers, @MaxCompanies, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, plan, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Plan plan, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenPlatform();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Plans
                SET PlanCode = @PlanCode,
                    PlanName = @PlanName,
                    [Description] = @Description,
                    CountryCode = @CountryCode,
                    CountryName = @CountryName,
                    CurrencyCode = @CurrencyCode,
                    MonthlyPrice = @MonthlyPrice,
                    AnnualPrice = @AnnualPrice,
                    MaxUsers = @MaxUsers,
                    MaxCompanies = @MaxCompanies,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE PlanId = @PlanId;";
            return await Sql.ExecuteAsync(conn, sql, plan, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int planId, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenPlatform();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Plans SET IsDeleted = 1, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE PlanId = @planId;";
            return await Sql.ExecuteAsync(conn, sql, new { planId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
