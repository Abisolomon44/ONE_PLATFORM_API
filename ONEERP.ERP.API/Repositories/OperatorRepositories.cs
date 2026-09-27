using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IOperatorRepository
{
    Task<Operator?> GetByIdAsync(int id);
    Task<Operator?> GetByUserIdAsync(int userId);
    Task<Operator?> GetByCodeAsync(int companyId, string operatorCode);
    Task<bool> CodeInUseAsync(int companyId, string operatorCode);
    Task<string> GetNextCodeAsync(int companyId, string prefix = "OP");
    Task<IEnumerable<Operator>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(int companyId, string search);
    Task<IEnumerable<Operator>> GetAllAsync(bool includeInactive);
    Task<int> InsertAsync(Operator entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Operator entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<OperatorType?> GetOperatorTypeAsync(int operatorTypeId);
}

public class OperatorRepository : TenantRepositoryBase, IOperatorRepository
{
    public OperatorRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<Operator?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Operator>(connection,
            "SELECT * FROM dbo.Operators WHERE OperatorId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<Operator?> GetByUserIdAsync(int userId)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Operator>(connection,
            "SELECT * FROM dbo.Operators WHERE UserId = @userId AND IsDeleted = 0", new { userId });
    }

    public async Task<Operator?> GetByCodeAsync(int companyId, string operatorCode)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Operator>(connection,
            "SELECT * FROM dbo.Operators WHERE CompanyId = @companyId AND OperatorCode = @operatorCode AND IsDeleted = 0",
            new { companyId, operatorCode });
    }

    public async Task<bool> CodeInUseAsync(int companyId, string operatorCode)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Operators WHERE CompanyId = @companyId AND OperatorCode = @operatorCode) THEN 1 ELSE 0 END",
            new { companyId, operatorCode });
    }

    public async Task<string> GetNextCodeAsync(int companyId, string prefix = "OP")
    {
        using var connection = OpenTenant();
        const string sql = @"
            SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(OperatorCode, LEN(@prefix) + 2, 10) AS INT)), 0) + 1
            FROM dbo.Operators
            WHERE CompanyId = @companyId AND OperatorCode LIKE @prefix + '-%'";
        var next = await Sql.ExecuteScalarAsync<int>(connection, sql, new { companyId, prefix });
        return $"{prefix}-{next:D4}";
    }

    public async Task<IEnumerable<Operator>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<Operator>(connection, @"
            SELECT * FROM dbo.Operators
            WHERE CompanyId = @companyId AND IsDeleted = 0
              AND (@search = '' OR OperatorName LIKE '%' + @search + '%' OR OperatorCode LIKE '%' + @search + '%')
            ORDER BY OperatorId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(int companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Operators
            WHERE CompanyId = @companyId AND IsDeleted = 0
              AND (@search = '' OR OperatorName LIKE '%' + @search + '%' OR OperatorCode LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<IEnumerable<Operator>> GetAllAsync(bool includeInactive)
    {
        using var connection = OpenTenant();
        var sql = @"
            SELECT * FROM dbo.Operators
            WHERE IsDeleted = 0"
              + (includeInactive ? "" : " AND IsActive = 1")
              + @" ORDER BY OperatorName;";
        return await Sql.QueryAsync<Operator>(connection, sql);
    }

    public async Task<int> InsertAsync(Operator entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                DECLARE @newId INT;
                INSERT INTO dbo.Operators (Id, CompanyId, BranchId, UserId, OperatorTypeId, OperatorCode, OperatorName, IsActive, IsDeleted, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
                VALUES (0, @CompanyId, @BranchId, @UserId, @OperatorTypeId, @OperatorCode, @OperatorName, @IsActive, @IsDeleted, @CreatedBy, SYSUTCDATETIME(), @UpdatedBy, SYSUTCDATETIME());
                SET @newId = CAST(SCOPE_IDENTITY() AS int);
                UPDATE dbo.Operators SET Id = @newId WHERE OperatorId = @newId;
                SELECT @newId;";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Operator entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Operators
                SET BranchId = @BranchId, OperatorTypeId = @OperatorTypeId, OperatorCode = @OperatorCode,
                    OperatorName = @OperatorName, IsActive = @IsActive,
                    UpdatedBy = @UpdatedBy, UpdatedAt = SYSUTCDATETIME()
                WHERE OperatorId = @OperatorId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
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
            const string sql = "UPDATE dbo.Operators SET IsDeleted = 1, IsActive = 0, UpdatedBy = @modifiedBy, UpdatedAt = SYSUTCDATETIME() WHERE OperatorId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<OperatorType?> GetOperatorTypeAsync(int operatorTypeId)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<OperatorType>(connection,
            "SELECT * FROM dbo.OperatorTypes WHERE Id = @id AND IsDeleted = 0", new { id = operatorTypeId });
    }
}

public interface ICounterAssignmentRepository
{
    Task<CounterAssignment?> GetByIdAsync(int id);
    Task<IEnumerable<CounterAssignment>> GetByOperatorAsync(int operatorId);
    Task<IEnumerable<CounterAssignment>> GetActiveByCounterAsync(int counterId);
    Task<CounterAssignment?> GetActiveByCounterAndOperatorAsync(int counterId, int operatorId);
    Task<IEnumerable<CounterAssignment>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(int companyId, string search);
    Task<IEnumerable<CounterAssignment>> GetAllAsync(bool includeInactive);
    Task<int> InsertAsync(CounterAssignment assignment, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(CounterAssignment assignment, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class CounterAssignmentRepository : TenantRepositoryBase, ICounterAssignmentRepository
{
    public CounterAssignmentRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<CounterAssignment?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<CounterAssignment>(connection,
            "SELECT * FROM dbo.CounterAssignments WHERE AssignmentId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<IEnumerable<CounterAssignment>> GetByOperatorAsync(int operatorId)
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<CounterAssignment>(connection,
            "SELECT * FROM dbo.CounterAssignments WHERE OperatorId = @operatorId AND IsDeleted = 0 ORDER BY CreatedAt DESC",
            new { operatorId });
    }

    public async Task<CounterAssignment?> GetActiveByCounterAndOperatorAsync(int counterId, int operatorId)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<CounterAssignment>(connection, @"
            SELECT * FROM dbo.CounterAssignments 
            WHERE CounterId = @counterId AND OperatorId = @operatorId AND IsDeleted = 0 AND IsActive = 1
              AND (ValidFrom IS NULL OR ValidFrom <= SYSUTCDATETIME())
              AND (ValidTo IS NULL OR ValidTo >= SYSUTCDATETIME())
            ORDER BY IsPrimary DESC, CreatedAt DESC",
            new { counterId, operatorId });
    }

    /// <summary>Operators currently assigned to a counter, primary assignment first.</summary>
    public async Task<IEnumerable<CounterAssignment>> GetActiveByCounterAsync(int counterId)
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<CounterAssignment>(connection, @"
            SELECT ca.*, s.StoreName, c.CounterCode, c.CounterName, o.OperatorCode, o.OperatorName
            FROM dbo.CounterAssignments ca
            LEFT JOIN dbo.Stores s ON s.StoreId = ca.StoreId
            LEFT JOIN dbo.Counters c ON c.CounterId = ca.CounterId
            INNER JOIN dbo.Operators o ON o.OperatorId = ca.OperatorId
            WHERE ca.CounterId = @counterId AND ca.IsDeleted = 0 AND ca.IsActive = 1
              AND o.IsDeleted = 0 AND o.IsActive = 1
              AND (ca.ValidFrom IS NULL OR ca.ValidFrom <= SYSUTCDATETIME())
              AND (ca.ValidTo IS NULL OR ca.ValidTo >= SYSUTCDATETIME())
            ORDER BY ca.IsPrimary DESC, ca.AssignmentId",
            new { counterId });
    }

    public async Task<IEnumerable<CounterAssignment>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<CounterAssignment>(connection, @"
            SELECT ca.*, s.StoreName, c.CounterCode, c.CounterName, o.OperatorCode, o.OperatorName
            FROM dbo.CounterAssignments ca
            LEFT JOIN dbo.Stores s ON s.StoreId = ca.StoreId
            LEFT JOIN dbo.Counters c ON c.CounterId = ca.CounterId
            LEFT JOIN dbo.Operators o ON o.OperatorId = ca.OperatorId
            WHERE ca.CompanyId = @companyId AND ca.IsDeleted = 0
              AND (@search = '' OR s.StoreName LIKE '%' + @search + '%' OR c.CounterCode LIKE '%' + @search + '%' OR o.OperatorCode LIKE '%' + @search + '%' OR o.OperatorName LIKE '%' + @search + '%')
            ORDER BY ca.AssignmentId DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(int companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1)
            FROM dbo.CounterAssignments ca
            LEFT JOIN dbo.Stores s ON s.StoreId = ca.StoreId
            LEFT JOIN dbo.Counters c ON c.CounterId = ca.CounterId
            LEFT JOIN dbo.Operators o ON o.OperatorId = ca.OperatorId
            WHERE ca.CompanyId = @companyId AND ca.IsDeleted = 0
              AND (@search = '' OR s.StoreName LIKE '%' + @search + '%' OR c.CounterCode LIKE '%' + @search + '%' OR o.OperatorCode LIKE '%' + @search + '%' OR o.OperatorName LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<IEnumerable<CounterAssignment>> GetAllAsync(bool includeInactive)
    {
        using var connection = OpenTenant();
        var sql = @"
            SELECT * FROM dbo.CounterAssignments
            WHERE IsDeleted = 0"
              + (includeInactive ? "" : " AND IsActive = 1")
              + @" ORDER BY CreatedAt DESC;";
        return await Sql.QueryAsync<CounterAssignment>(connection, sql);
    }

    public async Task<int> InsertAsync(CounterAssignment assignment, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                DECLARE @newId INT;
                INSERT INTO dbo.CounterAssignments (Id, CompanyId, BranchId, StoreId, CounterId, OperatorId, IsPrimary, ValidFrom, ValidTo, IsActive, IsDeleted, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
                VALUES (0, @CompanyId, @BranchId, @StoreId, @CounterId, @OperatorId, @IsPrimary, @ValidFrom, @ValidTo, @IsActive, @IsDeleted, @CreatedBy, SYSUTCDATETIME(), @UpdatedBy, SYSUTCDATETIME());
                SET @newId = CAST(SCOPE_IDENTITY() AS int);
                UPDATE dbo.CounterAssignments SET Id = @newId WHERE AssignmentId = @newId;
                SELECT @newId;";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, assignment, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(CounterAssignment assignment, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.CounterAssignments
                SET BranchId = @BranchId, StoreId = @StoreId, CounterId = @CounterId, OperatorId = @OperatorId,
                    IsPrimary = @IsPrimary, ValidFrom = @ValidFrom, ValidTo = @ValidTo, IsActive = @IsActive,
                    UpdatedBy = @UpdatedBy, UpdatedAt = SYSUTCDATETIME()
                WHERE AssignmentId = @AssignmentId;";
            return await Sql.ExecuteAsync(conn, sql, assignment, transaction) > 0;
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
            const string sql = "UPDATE dbo.CounterAssignments SET IsDeleted = 1, IsActive = 0, UpdatedBy = @modifiedBy, UpdatedAt = SYSUTCDATETIME() WHERE AssignmentId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}