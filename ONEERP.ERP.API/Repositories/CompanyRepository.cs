using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(int companyId);
    Task<Company?> GetByCodeAsync(string companyCode);
    Task<bool> CodeInUseAsync(string companyCode);
    Task<IEnumerable<Company>> GetAllAsync();
    Task<IEnumerable<Company>> GetPagedAsync(int pageNumber, int pageSize, string search);
    Task<int> CountAsync(string search);
    Task<int> InsertAsync(Company company, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Company company, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int companyId, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class CompanyRepository : TenantRepositoryBase, ICompanyRepository
{
    public CompanyRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<Company?> GetByIdAsync(int companyId)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Company>(connection,
            "SELECT * FROM dbo.Companies WHERE CompanyId = @companyId AND IsDeleted = 0", new { companyId });
    }

    public async Task<Company?> GetByCodeAsync(string companyCode)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Company>(connection,
            "SELECT * FROM dbo.Companies WHERE CompanyCode = @companyCode AND IsDeleted = 0", new { companyCode });
    }

    public async Task<bool> CodeInUseAsync(string companyCode)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Companies WHERE CompanyCode = @companyCode) THEN 1 ELSE 0 END", new { companyCode });
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<Company>(connection, "SELECT * FROM dbo.Companies WHERE IsDeleted = 0 ORDER BY CompanyName");
    }

    public async Task<IEnumerable<Company>> GetPagedAsync(int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<Company>(connection, @"
            SELECT * FROM dbo.Companies
            WHERE IsDeleted = 0
              AND (@search = '' OR CompanyName LIKE '%' + @search + '%' OR CompanyCode LIKE '%' + @search + '%')
            ORDER BY CompanyId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { search, offset, pageSize });
    }

    public async Task<int> CountAsync(string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Companies
            WHERE IsDeleted = 0
              AND (@search = '' OR CompanyName LIKE '%' + @search + '%' OR CompanyCode LIKE '%' + @search + '%')",
            new { search });
    }

    public async Task<int> InsertAsync(Company company, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Companies (CompanyCode, CompanyName, [Address], Email, Phone, GST, Currency, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@CompanyCode, @CompanyName, @Address, @Email, @Phone, @GST, @Currency, @Status, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, company, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Company company, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Companies
                SET CompanyName = @CompanyName,
                    [Address] = @Address,
                    Email = @Email,
                    Phone = @Phone,
                    GST = @GST,
                    Currency = @Currency,
                    Status = @Status,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE CompanyId = @CompanyId;";
            return await Sql.ExecuteAsync(conn, sql, company, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int companyId, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Companies SET IsDeleted = 1, Status = 'Inactive', ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE CompanyId = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
