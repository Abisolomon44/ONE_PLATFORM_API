using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IDocumentTypeRepository
{
    Task<IEnumerable<DocumentType>> GetAllAsync(bool includeInactive = false);
    Task<DocumentType?> GetByIdAsync(int id);
    Task<DocumentType?> GetByNameAsync(string name);
    Task<int> InsertAsync(DocumentType entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(DocumentType entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class DocumentTypeRepository : TenantRepositoryBase, IDocumentTypeRepository
{
    public DocumentTypeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<DocumentType>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.DocumentTypes WHERE IsDeleted = 0 ORDER BY [Name]"
            : "SELECT * FROM dbo.DocumentTypes WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY [Name]";
        return await Sql.QueryAsync<DocumentType>(connection, sql);
    }

    public async Task<DocumentType?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<DocumentType>(connection,
            "SELECT * FROM dbo.DocumentTypes WHERE DocumentTypeId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<DocumentType?> GetByNameAsync(string name)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<DocumentType>(connection,
            "SELECT * FROM dbo.DocumentTypes WHERE [Name] = @name AND IsDeleted = 0", new { name });
    }

    public async Task<int> InsertAsync(DocumentType entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.DocumentTypes ([Name], [Description], IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@Name, @Description, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(DocumentType entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.DocumentTypes
                SET [Name] = @Name,
                    [Description] = @Description,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE DocumentTypeId = @DocumentTypeId;";
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
            const string sql = "UPDATE dbo.DocumentTypes SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE DocumentTypeId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
