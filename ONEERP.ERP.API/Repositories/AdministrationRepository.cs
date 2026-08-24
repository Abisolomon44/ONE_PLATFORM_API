using System.Data;
using Microsoft.Data.SqlClient;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

/// <summary>
/// Dapper-based repository for the Currencies master table.
/// All SQL is parameterized; the repository contains only data-access logic.
/// </summary>
public class AdministrationRepository : TenantRepositoryBase, IAdministrationRepository
{
    public AdministrationRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

public async Task<IEnumerable<Currency>> GetAllAsync(bool includeInactive = false)
{
    using var connection = OpenTenant();
    var sql = includeInactive
        ? @"SELECT Id, CurrencyCode, CurrencyName, Symbol, ISOCode, 
                    ISNULL(DecimalPlaces, 2) AS DecimalPlaces, 
                    IsBaseCurrency, SortOrder, IsActive, 
                    CreatedBy, CreatedDate, ModifiedBy, ModifiedDate 
            FROM dbo.Currencies 
            ORDER BY SortOrder, CurrencyName"
        : @"SELECT Id, CurrencyCode, CurrencyName, Symbol, ISOCode, 
                    ISNULL(DecimalPlaces, 2) AS DecimalPlaces, 
                    IsBaseCurrency, SortOrder, IsActive, 
                    CreatedBy, CreatedDate, ModifiedBy, ModifiedDate 
            FROM dbo.Currencies 
            WHERE IsActive = 1 
            ORDER BY SortOrder, CurrencyName";
    return await Sql.QueryAsync<Currency>(connection, sql);
}

    public async Task<Currency?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Currency>(connection,
            "SELECT * FROM dbo.Currencies WHERE Id = @id", new { id });
    }

    public async Task<Currency?> GetByCodeAsync(string currencyCode)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Currency>(connection,
            "SELECT * FROM dbo.Currencies WHERE CurrencyCode = @currencyCode", new { currencyCode });
    }

    public async Task<int> InsertAsync(Currency entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Currencies (CurrencyCode, CurrencyName, Symbol, ISOCode, DecimalPlaces, IsBaseCurrency, SortOrder, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@CurrencyCode, @CurrencyName, @Symbol, @ISOCode, @DecimalPlaces, @IsBaseCurrency, @SortOrder, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Currency entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Currencies
                SET CurrencyCode = @CurrencyCode,
                    CurrencyName = @CurrencyName,
                    Symbol = @Symbol,
                    ISOCode = @ISOCode,
                    DecimalPlaces = @DecimalPlaces,
                    IsBaseCurrency = @IsBaseCurrency,
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

    public async Task<bool> SoftDeleteAsync(int id, int? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Currencies
                SET IsActive = 0,
                    ModifiedBy = @modifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE Id = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
