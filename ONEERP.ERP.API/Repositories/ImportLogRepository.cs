using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IImportLogRepository
{
    Task<long> InsertAsync(ImportLog log, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<IEnumerable<ImportLog>> GetAllAsync(int page = 1, int pageSize = 50);
}

public class ImportLogRepository : TenantRepositoryBase, IImportLogRepository
{
    public ImportLogRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<long> InsertAsync(ImportLog log, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.ImportLogs (
                    CompanyId, BranchId, ImportType, ModuleName, EntityName,
                    FileName, FileType, TotalRows, SuccessRows, FailedRows,
                    Status, ErrorMessage, ImportedBy, ImportedAt)
                VALUES (
                    @CompanyId, @BranchId, @ImportType, @ModuleName, @EntityName,
                    @FileName, @FileType, @TotalRows, @SuccessRows, @FailedRows,
                    @Status, @ErrorMessage, @ImportedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, log, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<IEnumerable<ImportLog>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        using var connection = OpenTenant();
        var offset = (page - 1) * pageSize;
        return await Sql.QueryAsync<ImportLog>(connection, @"
            SELECT * FROM dbo.ImportLogs
            ORDER BY ImportedAt DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { offset, pageSize });
    }
}
