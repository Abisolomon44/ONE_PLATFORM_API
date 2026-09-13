using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IStoreRepository
{
    Task<Store?> GetByIdAsync(int id);
    Task<Store?> GetByCodeAsync(int companyId, int? branchId, string storeCode);
    Task<bool> CodeInUseAsync(int companyId, int? branchId, string storeCode);
    Task<IEnumerable<Store>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(int companyId, int branchId, string search);
    Task<IEnumerable<Store>> GetAllAsync(bool includeInactive);
    Task<int> InsertAsync(Store store, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Store store, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class StoreRepository : TenantRepositoryBase, IStoreRepository
{
    public StoreRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<Store?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Store>(connection,
            "SELECT * FROM dbo.Stores WHERE StoreId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<Store?> GetByCodeAsync(int companyId, int? branchId, string storeCode)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Store>(connection,
            "SELECT * FROM dbo.Stores WHERE CompanyId = @companyId AND (@branchId IS NULL OR BranchId = @branchId) AND StoreCode = @storeCode AND IsDeleted = 0",
            new { companyId, branchId, storeCode });
    }

    public async Task<bool> CodeInUseAsync(int companyId, int? branchId, string storeCode)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Stores WHERE CompanyId = @companyId AND (@branchId IS NULL OR BranchId = @branchId) AND StoreCode = @storeCode) THEN 1 ELSE 0 END",
            new { companyId, branchId, storeCode });
    }

    public async Task<IEnumerable<Store>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<Store>(connection, @"
            SELECT * FROM dbo.Stores
            WHERE CompanyId = @companyId AND IsDeleted = 0
              AND (@branchId = 0 OR BranchId = @branchId)
              AND (@search = '' OR StoreName LIKE '%' + @search + '%' OR StoreCode LIKE '%' + @search + '%')
            ORDER BY StoreId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, branchId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(int companyId, int branchId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Stores
            WHERE CompanyId = @companyId AND IsDeleted = 0
              AND (@branchId = 0 OR BranchId = @branchId)
              AND (@search = '' OR StoreName LIKE '%' + @search + '%' OR StoreCode LIKE '%' + @search + '%')",
            new { companyId, branchId, search });
    }

    public async Task<IEnumerable<Store>> GetAllAsync(bool includeInactive)
    {
        using var connection = OpenTenant();
        var sql = @"
            SELECT * FROM dbo.Stores
            WHERE IsDeleted = 0"
                  + (includeInactive ? "" : " AND IsActive = 1")
                  + @" ORDER BY StoreName;";
        return await Sql.QueryAsync<Store>(connection, sql);
    }

    public async Task<int> InsertAsync(Store store, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Stores (CompanyId, BranchId, StoreCode, StoreName, StoreType, Address, Phone, Email, IsActive, IsDeleted, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, EntityId)
                VALUES (@CompanyId, @BranchId, @StoreCode, @StoreName, @StoreType, @Address, @Phone, @Email, @IsActive, @IsDeleted, @CreatedBy, SYSUTCDATETIME(), @UpdatedBy, SYSUTCDATETIME(), @EntityId);
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, store, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Store store, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Stores
                SET BranchId = @BranchId, StoreCode = @StoreCode, StoreName = @StoreName, StoreType = @StoreType,
                    Address = @Address, Phone = @Phone, Email = @Email, IsActive = @IsActive, EntityId = @EntityId,
                    UpdatedBy = @UpdatedBy, UpdatedAt = SYSUTCDATETIME()
                WHERE StoreId = @StoreId;";
            return await Sql.ExecuteAsync(conn, sql, store, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int id, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Stores SET IsDeleted = 1, IsActive = 0, UpdatedBy = @modifiedBy, UpdatedAt = SYSUTCDATETIME() WHERE StoreId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

public interface ICounterRepository
{
    Task<Counter?> GetByIdAsync(int id);
    Task<Counter?> GetByCodeAsync(int storeId, string counterCode);
    Task<bool> CodeInUseAsync(int storeId, string counterCode);
    Task<IEnumerable<Counter>> GetByStoreAsync(int storeId, bool includeInactive);
    Task<IEnumerable<Counter>> GetPagedAsync(int storeId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(int storeId, string search);
    Task<IEnumerable<Counter>> GetAllAsync(bool includeInactive);
    Task<int> InsertAsync(Counter counter, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Counter counter, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class CounterRepository : TenantRepositoryBase, ICounterRepository
{
    public CounterRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<Counter?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Counter>(connection,
            "SELECT * FROM dbo.Counters WHERE CounterId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<Counter?> GetByCodeAsync(int storeId, string counterCode)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Counter>(connection,
            "SELECT * FROM dbo.Counters WHERE StoreId = @storeId AND CounterCode = @counterCode AND IsDeleted = 0",
            new { storeId, counterCode });
    }

    public async Task<bool> CodeInUseAsync(int storeId, string counterCode)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Counters WHERE StoreId = @storeId AND CounterCode = @counterCode) THEN 1 ELSE 0 END",
            new { storeId, counterCode });
    }

    public async Task<IEnumerable<Counter>> GetByStoreAsync(int storeId, bool includeInactive)
    {
        using var connection = OpenTenant();
        var sql = @"
            SELECT * FROM dbo.Counters
            WHERE StoreId = @storeId AND IsDeleted = 0"
                  + (includeInactive ? "" : " AND IsActive = 1")
                  + @" ORDER BY CounterName;";
        return await Sql.QueryAsync<Counter>(connection, sql, new { storeId });
    }

    public async Task<IEnumerable<Counter>> GetPagedAsync(int storeId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<Counter>(connection, @"
            SELECT * FROM dbo.Counters
            WHERE IsDeleted = 0
              AND (@storeId = 0 OR StoreId = @storeId)
              AND (@search = '' OR CounterName LIKE '%' + @search + '%' OR CounterCode LIKE '%' + @search + '%')
            ORDER BY CounterId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { storeId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(int storeId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Counters
            WHERE IsDeleted = 0
              AND (@storeId = 0 OR StoreId = @storeId)
              AND (@search = '' OR CounterName LIKE '%' + @search + '%' OR CounterCode LIKE '%' + @search + '%')",
            new { storeId, search });
    }

    public async Task<IEnumerable<Counter>> GetAllAsync(bool includeInactive)
    {
        using var connection = OpenTenant();
        var sql = @"
            SELECT * FROM dbo.Counters
            WHERE IsDeleted = 0"
                  + (includeInactive ? "" : " AND IsActive = 1")
                  + @" ORDER BY CounterName;";
        return await Sql.QueryAsync<Counter>(connection, sql);
    }

    public async Task<int> InsertAsync(Counter counter, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Counters (StoreId, CounterCode, CounterName, IsActive, IsDeleted, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
                VALUES (@StoreId, @CounterCode, @CounterName, @IsActive, @IsDeleted, @CreatedBy, SYSUTCDATETIME(), @UpdatedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, counter, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Counter counter, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Counters
                SET CounterCode = @CounterCode, CounterName = @CounterName, IsActive = @IsActive,
                    UpdatedBy = @UpdatedBy, UpdatedAt = SYSUTCDATETIME()
                WHERE CounterId = @CounterId;";
            return await Sql.ExecuteAsync(conn, sql, counter, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int id, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Counters SET IsDeleted = 1, IsActive = 0, UpdatedBy = @modifiedBy, UpdatedAt = SYSUTCDATETIME() WHERE CounterId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

public interface IPOSSessionRepository
{
    Task<POSSession?> GetByIdAsync(long id);
    Task<POSSession?> GetByNumberAsync(string sessionNumber);
    Task<IEnumerable<POSSession>> GetPagedAsync(int companyId, int branchId, int storeId, int status, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(int companyId, int branchId, int storeId, int status, string search);
    Task<long> InsertAsync(POSSession session, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(POSSession session, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(long id, int updatedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class POSSessionRepository : TenantRepositoryBase, IPOSSessionRepository
{
    public POSSessionRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<POSSession?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<POSSession>(connection,
            "SELECT * FROM dbo.POSSessions WHERE POSSessionId = @id", new { id });
    }

    public async Task<POSSession?> GetByNumberAsync(string sessionNumber)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<POSSession>(connection,
            "SELECT * FROM dbo.POSSessions WHERE SessionNumber = @sessionNumber", new { sessionNumber });
    }

    public async Task<IEnumerable<POSSession>> GetPagedAsync(int companyId, int branchId, int storeId, int status, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<POSSession>(connection, @"
            SELECT * FROM dbo.POSSessions
            WHERE CompanyId = @companyId
              AND (@branchId = 0 OR BranchId = @branchId)
              AND (@storeId = 0 OR StoreId = @storeId)
              AND (@status = 0 OR Status = @status)
              AND (@search = '' OR SessionNumber LIKE '%' + @search + '%' OR StoreName LIKE '%' + @search + '%' OR CashierUserName LIKE '%' + @search + '%')
            ORDER BY POSSessionId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, branchId, storeId, status, search, offset, pageSize });
    }

    public async Task<int> CountAsync(int companyId, int branchId, int storeId, int status, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.POSSessions
            WHERE CompanyId = @companyId
              AND (@branchId = 0 OR BranchId = @branchId)
              AND (@storeId = 0 OR StoreId = @storeId)
              AND (@status = 0 OR Status = @status)
              AND (@search = '' OR SessionNumber LIKE '%' + @search + '%' OR StoreName LIKE '%' + @search + '%' OR CashierUserName LIKE '%' + @search + '%')",
            new { companyId, branchId, storeId, status, search });
    }

    public async Task<long> InsertAsync(POSSession session, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.POSSessions (CompanyId, CompanyName, BranchId, BranchName, StoreId, StoreName, CounterId, CounterName, CashierUserId, CashierUserName, SessionNumber, OpeningCash, ClosingCash, OpenedAt, ClosedAt, Status, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
                VALUES (@CompanyId, @CompanyName, @BranchId, @BranchName, @StoreId, @StoreName, @CounterId, @CounterName, @CashierUserId, @CashierUserName, @SessionNumber, @OpeningCash, @ClosingCash, @OpenedAt, @ClosedAt, @Status, @CreatedBy, SYSUTCDATETIME(), @UpdatedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, session, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(POSSession session, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.POSSessions
                SET ClosingCash = @ClosingCash, ClosedAt = @ClosedAt, Status = @Status,
                    UpdatedBy = @UpdatedBy, UpdatedAt = SYSUTCDATETIME()
                WHERE POSSessionId = @POSSessionId;";
            return await Sql.ExecuteAsync(conn, sql, session, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(long id, int updatedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.POSSessions SET Status = 3, UpdatedBy = @updatedBy, UpdatedAt = SYSUTCDATETIME() WHERE POSSessionId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, updatedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}