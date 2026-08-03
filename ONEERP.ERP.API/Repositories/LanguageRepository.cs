using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface ILanguageRepository
{
    Task<IEnumerable<Language>> GetAllAsync(bool includeInactive = false);
    Task<Language?> GetByIdAsync(int id);
    Task<Language?> GetByCodeAsync(string code);
    Task<int> InsertAsync(Language entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Language entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class LanguageRepository : TenantRepositoryBase, ILanguageRepository
{
    public LanguageRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<Language>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.Languages WHERE IsDeleted = 0 ORDER BY SortOrder, [Name]"
            : "SELECT * FROM dbo.Languages WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY SortOrder, [Name]";
        return await Sql.QueryAsync<Language>(connection, sql);
    }

    public async Task<Language?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Language>(connection,
            "SELECT * FROM dbo.Languages WHERE LanguageId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<Language?> GetByCodeAsync(string code)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Language>(connection,
            "SELECT * FROM dbo.Languages WHERE Code = @code AND IsDeleted = 0", new { code });
    }

    public async Task<int> InsertAsync(Language entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Languages ([Name], Code, CultureCode, IsRTL, IsDefault, SortOrder, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@Name, @Code, @CultureCode, @IsRTL, @IsDefault, @SortOrder, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Language entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Languages
                SET [Name] = @Name,
                    Code = @Code,
                    CultureCode = @CultureCode,
                    IsRTL = @IsRTL,
                    IsDefault = @IsDefault,
                    SortOrder = @SortOrder,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE LanguageId = @LanguageId;";
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
            const string sql = "UPDATE dbo.Languages SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE LanguageId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
