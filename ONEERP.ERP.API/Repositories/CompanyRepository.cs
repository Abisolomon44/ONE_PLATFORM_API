using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(int companyId);
    Task<Company?> GetByCodeAsync(string companyCode);
    Task<bool> CodeInUseAsync(string companyCode);
    Task<string> GetNextCodeAsync(string prefix = "COMP");
    Task<IEnumerable<Company>> GetAllAsync();
    Task<IEnumerable<Company>> GetPagedAsync(int pageNumber, int pageSize, string search);
    Task<int> CountAsync(string search);
    Task<int> InsertAsync(Company company, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Company company, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int companyId, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
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
            "SELECT * FROM dbo.Companies WHERE Id = @companyId AND IsDeleted = 0", new { companyId });
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

    public async Task<string> GetNextCodeAsync(string prefix = "COMP")
    {
        using var connection = OpenTenant();
        var next = await Sql.ExecuteScalarAsync<int>(connection,
            "SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(CompanyCode, LEN(@prefix) + 2, 10) AS INT)), 0) + 1 " +
            "FROM dbo.Companies WHERE CompanyCode LIKE @prefix + '-%'",
            new { prefix });
        return $"{prefix}-{next:D3}";
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
              AND (@search = '' OR CompanyName LIKE '%' + @search + '%' OR CompanyCode LIKE '%' + @search + '%' OR ShortName LIKE '%' + @search + '%' OR Abbreviation LIKE '%' + @search + '%')
            ORDER BY Id DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { search, offset, pageSize });
    }

    public async Task<int> CountAsync(string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Companies
            WHERE IsDeleted = 0
              AND (@search = '' OR CompanyName LIKE '%' + @search + '%' OR CompanyCode LIKE '%' + @search + '%' OR ShortName LIKE '%' + @search + '%' OR Abbreviation LIKE '%' + @search + '%')",
            new { search });
    }

    public async Task<int> InsertAsync(Company company, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Companies (CompanyCode, CompanyName, ShortName, Abbreviation, BusinessTypeId, IndustryTypeId,
                    GSTRegistrationTypeId, GSTNumber, PANNumber, TANNumber, CINNumber, RegistrationNumber,
                    CurrencyId, LanguageId, TimeZoneId, IsActive, IsBlocked, IsDeleted, LastLoginDate,
                    CompanyGroupId, BusinessUnitId, Website, Email, Phone, Mobile, LogoUrl, DefaultFinancialYearId,
                    MultiBranchEnabled, MultiWarehouseEnabled, MultiCurrencyEnabled, DateFormat, TimeFormat, NumberFormat,
                    DefaultWarehouseId, Theme, PrimaryColor, SecondaryColor, Remarks,
                    CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@CompanyCode, @CompanyName, @ShortName, @Abbreviation, @BusinessTypeId, @IndustryTypeId,
                    @GSTRegistrationTypeId, @GSTNumber, @PANNumber, @TANNumber, @CINNumber, @RegistrationNumber,
                    @CurrencyId, @LanguageId, @TimeZoneId, @IsActive, @IsBlocked, @IsDeleted, @LastLoginDate,
                    @CompanyGroupId, @BusinessUnitId, @Website, @Email, @Phone, @Mobile, @LogoUrl, @DefaultFinancialYearId,
                    @MultiBranchEnabled, @MultiWarehouseEnabled, @MultiCurrencyEnabled, @DateFormat, @TimeFormat, @NumberFormat,
                    @DefaultWarehouseId, @Theme, @PrimaryColor, @SecondaryColor, @Remarks,
                    @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
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
                SET CompanyCode = @CompanyCode,
                    CompanyName = @CompanyName,
                    ShortName = @ShortName,
                    Abbreviation = @Abbreviation,
                    BusinessTypeId = @BusinessTypeId,
                    IndustryTypeId = @IndustryTypeId,
                    GSTRegistrationTypeId = @GSTRegistrationTypeId,
                    GSTNumber = @GSTNumber,
                    PANNumber = @PANNumber,
                    TANNumber = @TANNumber,
                    CINNumber = @CINNumber,
                    RegistrationNumber = @RegistrationNumber,
                    CurrencyId = @CurrencyId,
                    LanguageId = @LanguageId,
                    TimeZoneId = @TimeZoneId,
                    IsActive = @IsActive,
                    IsBlocked = @IsBlocked,
                    CompanyGroupId = @CompanyGroupId,
                    BusinessUnitId = @BusinessUnitId,
                    Website = @Website,
                    Email = @Email,
                    Phone = @Phone,
                    Mobile = @Mobile,
                    LogoUrl = @LogoUrl,
                    DefaultFinancialYearId = @DefaultFinancialYearId,
                    MultiBranchEnabled = @MultiBranchEnabled,
                    MultiWarehouseEnabled = @MultiWarehouseEnabled,
                    MultiCurrencyEnabled = @MultiCurrencyEnabled,
                    DateFormat = @DateFormat,
                    TimeFormat = @TimeFormat,
                    NumberFormat = @NumberFormat,
                    DefaultWarehouseId = @DefaultWarehouseId,
                    Theme = @Theme,
                    PrimaryColor = @PrimaryColor,
                    SecondaryColor = @SecondaryColor,
                    Remarks = @Remarks,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE Id = @Id;";
            return await Sql.ExecuteAsync(conn, sql, company, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int companyId, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Companies SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE Id = @companyId;";
            return await Sql.ExecuteAsync(conn, sql, new { companyId, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
