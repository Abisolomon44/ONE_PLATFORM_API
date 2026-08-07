using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(int id);
    Task<Branch?> GetByCodeAsync(int companyId, string branchCode);
    Task<bool> CodeInUseAsync(int companyId, string branchCode);
    Task<IEnumerable<Branch>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(int companyId, string search);
    Task<int> InsertAsync(Branch branch, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Branch branch, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(int id);
    Task<Department?> GetByCodeAsync(int companyId, int branchId, string departmentCode);
    Task<bool> CodeInUseAsync(int companyId, int branchId, string departmentCode);
    Task<IEnumerable<Department>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(int companyId, int branchId, string search);
    Task<int> InsertAsync(Department department, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Department department, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public interface IDesignationRepository
{
    Task<Designation?> GetByIdAsync(int id);
    Task<Designation?> GetByCodeAsync(int companyId, string designationCode);
    Task<bool> CodeInUseAsync(int companyId, string designationCode);
    Task<IEnumerable<Designation>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(int companyId, string search);
    Task<int> InsertAsync(Designation designation, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Designation designation, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByCodeAsync(int companyId, string employeeCode);
    Task<bool> CodeInUseAsync(int companyId, string employeeCode);
    Task<IEnumerable<Employee>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(int companyId, int branchId, string search);
    Task<int> InsertAsync(Employee employee, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Employee employee, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(int id);
    Task<Warehouse?> GetByCodeAsync(int companyId, int branchId, string warehouseCode);
    Task<bool> CodeInUseAsync(int companyId, int branchId, string warehouseCode);
    Task<IEnumerable<Warehouse>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search);
    Task<int> CountAsync(int companyId, int branchId, string search);
    Task<int> InsertAsync(Warehouse warehouse, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Warehouse warehouse, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, int modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<IEnumerable<Warehouse>> GetAllAsync(bool includeInactive);
}

public class BranchRepository : TenantRepositoryBase, IBranchRepository
{
    public BranchRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<Branch?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Branch>(connection,
            "SELECT * FROM dbo.Branches WHERE Id = @id AND IsDeleted = 0", new { id });
    }

    public async Task<Branch?> GetByCodeAsync(int companyId, string branchCode)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Branch>(connection,
            "SELECT * FROM dbo.Branches WHERE CompanyId = @companyId AND BranchCode = @branchCode AND IsDeleted = 0",
            new { companyId, branchCode });
    }

    public async Task<bool> CodeInUseAsync(int companyId, string branchCode)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Branches WHERE CompanyId = @companyId AND BranchCode = @branchCode) THEN 1 ELSE 0 END",
            new { companyId, branchCode });
    }

    public async Task<IEnumerable<Branch>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<Branch>(connection, @"
            SELECT * FROM dbo.Branches
            WHERE CompanyId = @companyId AND IsDeleted = 0
              AND (@search = '' OR BranchName LIKE '%' + @search + '%' OR BranchCode LIKE '%' + @search + '%' OR ShortName LIKE '%' + @search + '%')
            ORDER BY Id DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(int companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Branches
            WHERE CompanyId = @companyId AND IsDeleted = 0
              AND (@search = '' OR BranchName LIKE '%' + @search + '%' OR BranchCode LIKE '%' + @search + '%' OR ShortName LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<int> InsertAsync(Branch branch, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Branches (CompanyId, BranchCode, BranchName, ShortName, BranchTypeId, ParentBranchId, ManagerEmployeeId, DefaultWarehouseId, GSTNumber, RegistrationNumber, IsHeadOffice, IsSalesBranch, IsPurchaseBranch, IsServiceBranch, SortOrder, IsActive, IsBlocked, IsDeleted, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@CompanyId, @BranchCode, @BranchName, @ShortName, @BranchTypeId, @ParentBranchId, @ManagerEmployeeId, @DefaultWarehouseId, @GSTNumber, @RegistrationNumber, @IsHeadOffice, @IsSalesBranch, @IsPurchaseBranch, @IsServiceBranch, @SortOrder, @IsActive, @IsBlocked, @IsDeleted, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, branch, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Branch branch, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Branches
                SET BranchCode = @BranchCode, BranchName = @BranchName, ShortName = @ShortName, BranchTypeId = @BranchTypeId,
                    ParentBranchId = @ParentBranchId, ManagerEmployeeId = @ManagerEmployeeId, DefaultWarehouseId = @DefaultWarehouseId,
                    GSTNumber = @GSTNumber, RegistrationNumber = @RegistrationNumber, IsHeadOffice = @IsHeadOffice,
                    IsSalesBranch = @IsSalesBranch, IsPurchaseBranch = @IsPurchaseBranch, IsServiceBranch = @IsServiceBranch,
                    SortOrder = @SortOrder, IsActive = @IsActive, IsBlocked = @IsBlocked,
                    ModifiedBy = @ModifiedBy, ModifiedDate = SYSUTCDATETIME()
                WHERE Id = @Id;";
            return await Sql.ExecuteAsync(conn, sql, branch, transaction) > 0;
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
            const string sql = "UPDATE dbo.Branches SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE Id = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

public class DepartmentRepository : TenantRepositoryBase, IDepartmentRepository
{
    public DepartmentRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Department>(connection,
            "SELECT * FROM dbo.Departments WHERE Id = @id AND IsDeleted = 0", new { id });
    }

    public async Task<Department?> GetByCodeAsync(int companyId, int branchId, string departmentCode)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Department>(connection,
            "SELECT * FROM dbo.Departments WHERE CompanyId = @companyId AND BranchId = @branchId AND DepartmentCode = @departmentCode AND IsDeleted = 0",
            new { companyId, branchId, departmentCode });
    }

    public async Task<bool> CodeInUseAsync(int companyId, int branchId, string departmentCode)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Departments WHERE CompanyId = @companyId AND BranchId = @branchId AND DepartmentCode = @departmentCode) THEN 1 ELSE 0 END",
            new { companyId, branchId, departmentCode });
    }

    public async Task<IEnumerable<Department>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<Department>(connection, @"
            SELECT * FROM dbo.Departments
            WHERE CompanyId = @companyId AND BranchId = @branchId AND IsDeleted = 0
              AND (@search = '' OR DepartmentName LIKE '%' + @search + '%' OR DepartmentCode LIKE '%' + @search + '%' OR ShortName LIKE '%' + @search + '%')
            ORDER BY Id DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, branchId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(int companyId, int branchId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Departments
            WHERE CompanyId = @companyId AND BranchId = @branchId AND IsDeleted = 0
              AND (@search = '' OR DepartmentName LIKE '%' + @search + '%' OR DepartmentCode LIKE '%' + @search + '%' OR ShortName LIKE '%' + @search + '%')",
            new { companyId, branchId, search });
    }

    public async Task<int> InsertAsync(Department department, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Departments (CompanyId, BranchId, DepartmentCode, DepartmentName, ShortName, ParentDepartmentId, ManagerEmployeeId, SortOrder, IsActive, IsBlocked, IsDeleted, Remarks, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@CompanyId, @BranchId, @DepartmentCode, @DepartmentName, @ShortName, @ParentDepartmentId, @ManagerEmployeeId, @SortOrder, @IsActive, @IsBlocked, @IsDeleted, @Remarks, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, department, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Department department, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Departments
                SET DepartmentCode = @DepartmentCode, DepartmentName = @DepartmentName, ShortName = @ShortName,
                    ParentDepartmentId = @ParentDepartmentId, ManagerEmployeeId = @ManagerEmployeeId, SortOrder = @SortOrder,
                    IsActive = @IsActive, IsBlocked = @IsBlocked, Remarks = @Remarks,
                    ModifiedBy = @ModifiedBy, ModifiedDate = SYSUTCDATETIME()
                WHERE Id = @Id;";
            return await Sql.ExecuteAsync(conn, sql, department, transaction) > 0;
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
            const string sql = "UPDATE dbo.Departments SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE Id = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

public class DesignationRepository : TenantRepositoryBase, IDesignationRepository
{
    public DesignationRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<Designation?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Designation>(connection,
            "SELECT * FROM dbo.Designations WHERE Id = @id AND IsDeleted = 0", new { id });
    }

    public async Task<Designation?> GetByCodeAsync(int companyId, string designationCode)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Designation>(connection,
            "SELECT * FROM dbo.Designations WHERE CompanyId = @companyId AND DesignationCode = @designationCode AND IsDeleted = 0",
            new { companyId, designationCode });
    }

    public async Task<bool> CodeInUseAsync(int companyId, string designationCode)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Designations WHERE CompanyId = @companyId AND DesignationCode = @designationCode) THEN 1 ELSE 0 END",
            new { companyId, designationCode });
    }

    public async Task<IEnumerable<Designation>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<Designation>(connection, @"
            SELECT * FROM dbo.Designations
            WHERE CompanyId = @companyId AND IsDeleted = 0
              AND (@search = '' OR DesignationName LIKE '%' + @search + '%' OR DesignationCode LIKE '%' + @search + '%' OR ShortName LIKE '%' + @search + '%')
            ORDER BY Id DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(int companyId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Designations
            WHERE CompanyId = @companyId AND IsDeleted = 0
              AND (@search = '' OR DesignationName LIKE '%' + @search + '%' OR DesignationCode LIKE '%' + @search + '%' OR ShortName LIKE '%' + @search + '%')",
            new { companyId, search });
    }

    public async Task<int> InsertAsync(Designation designation, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Designations (CompanyId, DepartmentId, DesignationCode, DesignationName, ShortName, LevelNo, Grade, [Description], SortOrder, IsDefault, IsActive, IsBlocked, IsDeleted, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@CompanyId, @DepartmentId, @DesignationCode, @DesignationName, @ShortName, @LevelNo, @Grade, @Description, @SortOrder, @IsDefault, @IsActive, @IsBlocked, @IsDeleted, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, designation, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Designation designation, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Designations
                SET DesignationCode = @DesignationCode, DesignationName = @DesignationName, ShortName = @ShortName,
                    LevelNo = @LevelNo, Grade = @Grade, [Description] = @Description, SortOrder = @SortOrder,
                    IsDefault = @IsDefault, IsActive = @IsActive, IsBlocked = @IsBlocked,
                    ModifiedBy = @ModifiedBy, ModifiedDate = SYSUTCDATETIME()
                WHERE Id = @Id;";
            return await Sql.ExecuteAsync(conn, sql, designation, transaction) > 0;
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
            const string sql = "UPDATE dbo.Designations SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE Id = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

public class EmployeeRepository : TenantRepositoryBase, IEmployeeRepository
{
    public EmployeeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Employee>(connection,
            "SELECT * FROM dbo.Employees WHERE Id = @id AND IsDeleted = 0", new { id });
    }

    public async Task<Employee?> GetByCodeAsync(int companyId, string employeeCode)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Employee>(connection,
            "SELECT * FROM dbo.Employees WHERE CompanyId = @companyId AND EmployeeCode = @employeeCode AND IsDeleted = 0",
            new { companyId, employeeCode });
    }

    public async Task<bool> CodeInUseAsync(int companyId, string employeeCode)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Employees WHERE CompanyId = @companyId AND EmployeeCode = @employeeCode) THEN 1 ELSE 0 END",
            new { companyId, employeeCode });
    }

    public async Task<IEnumerable<Employee>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<Employee>(connection, @"
            SELECT * FROM dbo.Employees
            WHERE CompanyId = @companyId AND BranchId = @branchId AND IsDeleted = 0
              AND (@search = '' OR FirstName LIKE '%' + @search + '%' OR LastName LIKE '%' + @search + '%' OR EmployeeCode LIKE '%' + @search + '%' OR DisplayName LIKE '%' + @search + '%')
            ORDER BY Id DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, branchId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(int companyId, int branchId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Employees
            WHERE CompanyId = @companyId AND BranchId = @branchId AND IsDeleted = 0
              AND (@search = '' OR FirstName LIKE '%' + @search + '%' OR LastName LIKE '%' + @search + '%' OR EmployeeCode LIKE '%' + @search + '%' OR DisplayName LIKE '%' + @search + '%')",
            new { companyId, branchId, search });
    }

    public async Task<int> InsertAsync(Employee employee, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Employees (CompanyId, BranchId, DepartmentId, DesignationId, EmployeeCode, EmployeeNumber, FirstName, MiddleName, LastName, DisplayName, GenderId, MaritalStatusId, DateOfBirth, DateOfJoining, DateOfLeaving, OfficialEmail, PersonalEmail, MobileNo, AlternateMobileNo, ReportingManagerId, EmploymentTypeId, IsActive, IsBlocked, IsDeleted, Remarks, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@CompanyId, @BranchId, @DepartmentId, @DesignationId, @EmployeeCode, @EmployeeNumber, @FirstName, @MiddleName, @LastName, @DisplayName, @GenderId, @MaritalStatusId, @DateOfBirth, @DateOfJoining, @DateOfLeaving, @OfficialEmail, @PersonalEmail, @MobileNo, @AlternateMobileNo, @ReportingManagerId, @EmploymentTypeId, @IsActive, @IsBlocked, @IsDeleted, @Remarks, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, employee, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Employee employee, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Employees
                SET EmployeeCode = @EmployeeCode, EmployeeNumber = @EmployeeNumber, FirstName = @FirstName, MiddleName = @MiddleName, LastName = @LastName,
                    DisplayName = @DisplayName, GenderId = @GenderId, MaritalStatusId = @MaritalStatusId, DateOfBirth = @DateOfBirth,
                    DateOfJoining = @DateOfJoining, DateOfLeaving = @DateOfLeaving, OfficialEmail = @OfficialEmail, PersonalEmail = @PersonalEmail,
                    MobileNo = @MobileNo, AlternateMobileNo = @AlternateMobileNo, ReportingManagerId = @ReportingManagerId, EmploymentTypeId = @EmploymentTypeId,
                    IsActive = @IsActive, IsBlocked = @IsBlocked, Remarks = @Remarks,
                    ModifiedBy = @ModifiedBy, ModifiedDate = SYSUTCDATETIME()
                WHERE Id = @Id;";
            return await Sql.ExecuteAsync(conn, sql, employee, transaction) > 0;
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
            const string sql = "UPDATE dbo.Employees SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE Id = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

public class WarehouseRepository : TenantRepositoryBase, IWarehouseRepository
{
    public WarehouseRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<Warehouse?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Warehouse>(connection,
            "SELECT * FROM dbo.Warehouses WHERE Id = @id AND IsDeleted = 0", new { id });
    }

    public async Task<Warehouse?> GetByCodeAsync(int companyId, int branchId, string warehouseCode)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Warehouse>(connection,
            "SELECT * FROM dbo.Warehouses WHERE CompanyId = @companyId AND BranchId = @branchId AND WarehouseCode = @warehouseCode AND IsDeleted = 0",
            new { companyId, branchId, warehouseCode });
    }

    public async Task<bool> CodeInUseAsync(int companyId, int branchId, string warehouseCode)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Warehouses WHERE CompanyId = @companyId AND BranchId = @branchId AND WarehouseCode = @warehouseCode) THEN 1 ELSE 0 END",
            new { companyId, branchId, warehouseCode });
    }

    public async Task<IEnumerable<Warehouse>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var offset = (pageNumber - 1) * pageSize;
        return await Sql.QueryAsync<Warehouse>(connection, @"
            SELECT * FROM dbo.Warehouses
            WHERE CompanyId = @companyId AND BranchId = @branchId AND IsDeleted = 0
              AND (@search = '' OR WarehouseName LIKE '%' + @search + '%' OR WarehouseCode LIKE '%' + @search + '%' OR ShortName LIKE '%' + @search + '%')
            ORDER BY Id DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
            new { companyId, branchId, search, offset, pageSize });
    }

    public async Task<int> CountAsync(int companyId, int branchId, string search)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<int>(connection, @"
            SELECT COUNT(1) FROM dbo.Warehouses
            WHERE CompanyId = @companyId AND BranchId = @branchId AND IsDeleted = 0
              AND (@search = '' OR WarehouseName LIKE '%' + @search + '%' OR WarehouseCode LIKE '%' + @search + '%' OR ShortName LIKE '%' + @search + '%')",
            new { companyId, branchId, search });
    }

    public async Task<int> InsertAsync(Warehouse warehouse, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Warehouses (CompanyId, BranchId, WarehouseCode, WarehouseName, ShortName, WarehouseTypeId, ParentWarehouseId, ManagerEmployeeId, AllowNegativeStock, IsDefault, SortOrder, Remarks, IsActive, IsBlocked, IsDeleted, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@CompanyId, @BranchId, @WarehouseCode, @WarehouseName, @ShortName, @WarehouseTypeId, @ParentWarehouseId, @ManagerEmployeeId, @AllowNegativeStock, @IsDefault, @SortOrder, @Remarks, @IsActive, @IsBlocked, @IsDeleted, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, warehouse, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Warehouse warehouse, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Warehouses
                SET WarehouseCode = @WarehouseCode, WarehouseName = @WarehouseName, ShortName = @ShortName, WarehouseTypeId = @WarehouseTypeId,
                    ParentWarehouseId = @ParentWarehouseId, ManagerEmployeeId = @ManagerEmployeeId, AllowNegativeStock = @AllowNegativeStock,
                    IsDefault = @IsDefault, SortOrder = @SortOrder, IsActive = @IsActive, IsBlocked = @IsBlocked, Remarks = @Remarks,
                    ModifiedBy = @ModifiedBy, ModifiedDate = SYSUTCDATETIME()
                WHERE Id = @Id;";
            return await Sql.ExecuteAsync(conn, sql, warehouse, transaction) > 0;
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
            const string sql = "UPDATE dbo.Warehouses SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE Id = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync(bool includeInactive)
    {
        using var connection = OpenTenant();
        var sql = @"
            SELECT * FROM dbo.Warehouses
            WHERE IsDeleted = 0"
                  + (includeInactive ? "" : " AND IsActive = 1")
                  + @" ORDER BY SortOrder, Id;";
        return await Sql.QueryAsync<Warehouse>(connection, sql);
    }
}

public interface IBranchTypeRepository
{
    Task<IEnumerable<BranchType>> GetAllAsync(bool includeInactive);
}

public interface IWarehouseTypeRepository
{
    Task<IEnumerable<WarehouseType>> GetAllAsync(bool includeInactive);
}

public interface IEmploymentTypeRepository
{
    Task<IEnumerable<EmploymentType>> GetAllAsync(bool includeInactive);
}

public class BranchTypeRepository : TenantRepositoryBase, IBranchTypeRepository
{
    public BranchTypeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<BranchType>> GetAllAsync(bool includeInactive)
    {
        using var connection = OpenTenant();
        const string sql = "SELECT * FROM dbo.BranchTypes ORDER BY SortOrder;";
        return await Sql.QueryAsync<BranchType>(connection, sql);
    }
}

public class WarehouseTypeRepository : TenantRepositoryBase, IWarehouseTypeRepository
{
    public WarehouseTypeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<WarehouseType>> GetAllAsync(bool includeInactive)
    {
        using var connection = OpenTenant();
        const string sql = "SELECT * FROM dbo.WarehouseTypes ORDER BY SortOrder;";
        return await Sql.QueryAsync<WarehouseType>(connection, sql);
    }
}

public class EmploymentTypeRepository : TenantRepositoryBase, IEmploymentTypeRepository
{
    public EmploymentTypeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<EmploymentType>> GetAllAsync(bool includeInactive)
    {
        using var connection = OpenTenant();
        const string sql = "SELECT * FROM dbo.EmploymentTypes ORDER BY SortOrder;";
        return await Sql.QueryAsync<EmploymentType>(connection, sql);
    }
}