using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IPaymentTypeRepository
{
    Task<IEnumerable<PaymentType>> GetAllAsync(bool includeInactive = false);
    Task<PaymentType?> GetByIdAsync(long id);
    Task<PaymentType?> GetByCodeAsync(string code);
    Task<long> InsertAsync(PaymentType entity);
    Task<bool> UpdateAsync(PaymentType entity);
    Task<bool> DeleteAsync(long id);
}

public class PaymentTypeRepository : TenantRepositoryBase, IPaymentTypeRepository
{
    public PaymentTypeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<PaymentType>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.PaymentType ORDER BY DisplayOrder, Name"
            : "SELECT * FROM dbo.PaymentType WHERE IsActive = 1 ORDER BY DisplayOrder, Name";
        return await Sql.QueryAsync<PaymentType>(connection, sql);
    }

    public async Task<PaymentType?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<PaymentType>(connection,
            "SELECT * FROM dbo.PaymentType WHERE PaymentTypeId = @id", new { id });
    }

    public async Task<PaymentType?> GetByCodeAsync(string code)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<PaymentType>(connection,
            "SELECT * FROM dbo.PaymentType WHERE Code = @code", new { code });
    }

    public async Task<long> InsertAsync(PaymentType entity)
    {
        using var connection = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.PaymentType (Code, Name, DisplayOrder, IsActive)
            VALUES (@Code, @Name, @DisplayOrder, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(connection, sql, entity);
    }

    public async Task<bool> UpdateAsync(PaymentType entity)
    {
        using var connection = OpenTenant();
        const string sql = @"
            UPDATE dbo.PaymentType SET
                Code = @Code,
                Name = @Name,
                DisplayOrder = @DisplayOrder,
                IsActive = @IsActive
            WHERE PaymentTypeId = @PaymentTypeId;";
        return await Sql.ExecuteAsync(connection, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteAsync(connection, "DELETE FROM dbo.PaymentType WHERE PaymentTypeId = @id", new { id }) > 0;
    }
}

public interface IPaymentMethodRepository
{
    Task<IEnumerable<PaymentMethod>> GetAllAsync(bool includeInactive = false);
    Task<PaymentMethod?> GetByIdAsync(long id);
    Task<PaymentMethod?> GetByCodeAsync(string code);
    Task<long> InsertAsync(PaymentMethod entity);
    Task<bool> UpdateAsync(PaymentMethod entity);
    Task<bool> DeleteAsync(long id);
}

public class PaymentMethodRepository : TenantRepositoryBase, IPaymentMethodRepository
{
    public PaymentMethodRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<PaymentMethod>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.PaymentMethod ORDER BY DisplayOrder, Name"
            : "SELECT * FROM dbo.PaymentMethod WHERE IsActive = 1 ORDER BY DisplayOrder, Name";
        return await Sql.QueryAsync<PaymentMethod>(connection, sql);
    }

    public async Task<PaymentMethod?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<PaymentMethod>(connection,
            "SELECT * FROM dbo.PaymentMethod WHERE PaymentMethodId = @id", new { id });
    }

    public async Task<PaymentMethod?> GetByCodeAsync(string code)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<PaymentMethod>(connection,
            "SELECT * FROM dbo.PaymentMethod WHERE Code = @code", new { code });
    }

    public async Task<long> InsertAsync(PaymentMethod entity)
    {
        using var connection = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.PaymentMethod (Code, Name, PaymentCategory, IsCash, IsCredit, RequiresReferenceNo, DisplayOrder, IsActive)
            VALUES (@Code, @Name, @PaymentCategory, @IsCash, @IsCredit, @RequiresReferenceNo, @DisplayOrder, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(connection, sql, entity);
    }

    public async Task<bool> UpdateAsync(PaymentMethod entity)
    {
        using var connection = OpenTenant();
        const string sql = @"
            UPDATE dbo.PaymentMethod SET
                Code = @Code,
                Name = @Name,
                PaymentCategory = @PaymentCategory,
                IsCash = @IsCash,
                IsCredit = @IsCredit,
                RequiresReferenceNo = @RequiresReferenceNo,
                DisplayOrder = @DisplayOrder,
                IsActive = @IsActive
            WHERE PaymentMethodId = @PaymentMethodId;";
        return await Sql.ExecuteAsync(connection, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteAsync(connection, "DELETE FROM dbo.PaymentMethod WHERE PaymentMethodId = @id", new { id }) > 0;
    }
}

public interface IPaymentRepository
{
    Task<(List<Payment> Items, int TotalCount)> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<Payment?> GetByIdAsync(long id);
    Task<string> GetNextPaymentNoAsync(long companyId);
    Task<long> InsertAsync(Payment entity);
    Task<bool> UpdateAsync(Payment entity);
    Task<bool> DeleteAsync(long id);
}

public class PaymentRepository : TenantRepositoryBase, IPaymentRepository
{
    public PaymentRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<(List<Payment> Items, int TotalCount)> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        using var connection = OpenTenant();
        var sp = string.IsNullOrWhiteSpace(search) ? "%" : $"%{search}%";
        var offset = (pageNumber - 1) * pageSize;
        var total = await Sql.QuerySingleOrDefaultAsync<int>(connection,
            "SELECT COUNT(*) FROM dbo.Payment WHERE CompanyId = @companyId AND (PaymentNo LIKE @sp OR ReferenceNo LIKE @sp OR ReferenceType LIKE @sp OR Remarks LIKE @sp)",
            new { companyId, sp });
        var items = await Sql.QueryAsync<Payment>(connection,
            @"SELECT * FROM (
                SELECT *, ROW_NUMBER() OVER (ORDER BY PaymentId DESC) AS _rn
                FROM dbo.Payment
                WHERE CompanyId = @companyId AND (PaymentNo LIKE @sp OR ReferenceNo LIKE @sp OR ReferenceType LIKE @sp OR Remarks LIKE @sp)
              ) t
              WHERE t._rn > @offset AND t._rn <= @offset + @pageSize",
            new { companyId, sp, offset, pageSize });
        return (items.ToList(), total);
    }

    public async Task<Payment?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Payment>(connection,
            "SELECT * FROM dbo.Payment WHERE PaymentId = @id", new { id });
    }

    public async Task<string> GetNextPaymentNoAsync(long companyId)
    {
        using var connection = OpenTenant();
        var count = await Sql.QuerySingleOrDefaultAsync<int>(connection,
            "SELECT COUNT(*) FROM dbo.Payment WHERE CompanyId = @companyId", new { companyId });
        return $"PAY-{DateTime.UtcNow:yyyy}-{(count + 1):D5}";
    }

    public async Task<long> InsertAsync(Payment entity)
    {
        using var connection = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.Payment
            (
                CompanyId, PaymentNo, PaymentDate, PaymentTypeID, PaymentMethodID, ReferenceType, ReferenceId,
                BusinessPartnerId, Amount, ReferenceNo, Remarks, StatusID, CreatedByUserID
            )
            VALUES
            (
                @CompanyId, @PaymentNo, @PaymentDate, @PaymentTypeID, @PaymentMethodID, @ReferenceType, @ReferenceId,
                @BusinessPartnerId, @Amount, @ReferenceNo, @Remarks, @StatusID, @CreatedByUserID
            );
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(connection, sql, entity);
    }

    public async Task<bool> UpdateAsync(Payment entity)
    {
        using var connection = OpenTenant();
        const string sql = @"
            UPDATE dbo.Payment SET
                PaymentNo = @PaymentNo,
                PaymentDate = @PaymentDate,
                PaymentTypeID = @PaymentTypeID,
                PaymentMethodID = @PaymentMethodID,
                ReferenceType = @ReferenceType,
                ReferenceId = @ReferenceId,
                BusinessPartnerId = @BusinessPartnerId,
                Amount = @Amount,
                ReferenceNo = @ReferenceNo,
                Remarks = @Remarks,
                StatusID = @StatusID,
                UpdatedByUserID = @UpdatedByUserID,
                UpdatedAt = SYSUTCDATETIME()
            WHERE PaymentId = @PaymentId;";
        return await Sql.ExecuteAsync(connection, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteAsync(connection, "DELETE FROM dbo.Payment WHERE PaymentId = @id", new { id }) > 0;
    }
}
