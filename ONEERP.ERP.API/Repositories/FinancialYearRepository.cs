using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IFinancialYearRepository
{
    Task<FinancialYear?> GetByIdAsync(long id);
    Task<FinancialYear?> GetByCodeAsync(int companyId, string code);
    Task<bool> CodeInUseAsync(int companyId, string code);
    Task<IEnumerable<FinancialYear>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(int companyId, string search);
    Task<IEnumerable<FinancialYear>> GetAllAsync(bool includeInactive);
    Task<long> InsertAsync(FinancialYear fy, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(FinancialYear fy, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class FinancialYearRepository : TenantRepositoryBase, IFinancialYearRepository
{
    public FinancialYearRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<FinancialYear?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<FinancialYear>(connection,
            "SELECT * FROM dbo.FinancialYear WHERE FinancialYearId = @id", new { id });
    }

    public async Task<FinancialYear?> GetByCodeAsync(int companyId, string code)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<FinancialYear>(connection,
            "SELECT * FROM dbo.FinancialYear WHERE CompanyId = @companyId AND Code = @code",
            new { companyId, code });
    }

    public async Task<bool> CodeInUseAsync(int companyId, string code)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.FinancialYear WHERE CompanyId = @companyId AND Code = @code) THEN 1 ELSE 0 END",
            new { companyId, code });
    }

    public async Task<IEnumerable<FinancialYear>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<FinancialYear>(connection, @"
            SELECT * FROM dbo.FinancialYear
            WHERE CompanyId = @companyId
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR [Name] LIKE '%' + @search + '%')
            ORDER BY FinancialYearId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(int companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.FinancialYear
            WHERE CompanyId = @companyId
              AND (@search = '' OR Code LIKE '%' + @search + '%' OR [Name] LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<IEnumerable<FinancialYear>> GetAllAsync(bool includeInactive)
    {
        using var connection = OpenTenant();
        var sql = "SELECT * FROM dbo.FinancialYear"
                  + (includeInactive ? "" : " WHERE IsActive = 1")
                  + " ORDER BY [Name];";
        return await Sql.QueryAsync<FinancialYear>(connection, sql);
    }

    public async Task<long> InsertAsync(FinancialYear fy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.FinancialYear (CompanyId, Code, [Name], StartDate, EndDate, IsCurrent, IsClosed, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@CompanyId, @Code, @Name, @StartDate, @EndDate, @IsCurrent, @IsClosed, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, fy, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(FinancialYear fy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.FinancialYear
                SET Code = @Code, [Name] = @Name, StartDate = @StartDate, EndDate = @EndDate,
                    IsCurrent = @IsCurrent, IsClosed = @IsClosed, IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy, ModifiedAt = SYSUTCDATETIME()
                WHERE FinancialYearId = @FinancialYearId;";
            return await Sql.ExecuteAsync(conn, sql, fy, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(long id, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.FinancialYear SET IsActive = 0, ModifiedBy = @modifiedBy, ModifiedAt = SYSUTCDATETIME() WHERE FinancialYearId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}