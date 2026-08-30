using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface ITenantConfigurationRepository
{
    Task<IEnumerable<TenantConfiguration>> GetByTenantAsync(long tenantId);
    Task<TenantConfiguration?> GetByIdAsync(long id);
    Task<long> InsertAsync(TenantConfiguration entity);
    Task<bool> UpdateAsync(TenantConfiguration entity);
    Task<bool> DeleteAsync(long id);
}

public class TenantConfigurationRepository : TenantRepositoryBase, ITenantConfigurationRepository
{
    public TenantConfigurationRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<TenantConfiguration>> GetByTenantAsync(long tenantId)
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<TenantConfiguration>(connection,
            @"SELECT * FROM dbo.TenantConfiguration
              WHERE TenantId = @tenantId AND IsActive = 1
              ORDER BY ApplicationType, TransactionType, FlowType, PageCode, SequenceNo, DisplayOrder",
            new { tenantId });
    }

    public async Task<TenantConfiguration?> GetByIdAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<TenantConfiguration>(connection,
            "SELECT * FROM dbo.TenantConfiguration WHERE Id = @id", new { id });
    }

    public async Task<long> InsertAsync(TenantConfiguration entity)
    {
        using var connection = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.TenantConfiguration
            (
                TenantId, ApplicationType, TransactionType, FlowType, PageCode, FieldCode,
                SequenceNo, IsPageEnabled, IsVisible, IsRequired, IsReadonly, DisplayOrder,
                DefaultValue, IsActive, CreatedBy, CreatedAt
            )
            VALUES
            (
                @TenantId, @ApplicationType, @TransactionType, @FlowType, @PageCode, @FieldCode,
                @SequenceNo, @IsPageEnabled, @IsVisible, @IsRequired, @IsReadonly, @DisplayOrder,
                @DefaultValue, @IsActive, @CreatedBy, SYSUTCDATETIME()
            );
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(connection, sql, entity);
    }

    public async Task<bool> UpdateAsync(TenantConfiguration entity)
    {
        using var connection = OpenTenant();
        const string sql = @"
            UPDATE dbo.TenantConfiguration SET
                ApplicationType = @ApplicationType,
                TransactionType = @TransactionType,
                FlowType = @FlowType,
                PageCode = @PageCode,
                FieldCode = @FieldCode,
                SequenceNo = @SequenceNo,
                IsPageEnabled = @IsPageEnabled,
                IsVisible = @IsVisible,
                IsRequired = @IsRequired,
                IsReadonly = @IsReadonly,
                DisplayOrder = @DisplayOrder,
                DefaultValue = @DefaultValue,
                IsActive = @IsActive,
                UpdatedBy = @UpdatedBy,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id;";
        return await Sql.ExecuteAsync(connection, sql, entity) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteAsync(connection,
            "DELETE FROM dbo.TenantConfiguration WHERE Id = @id", new { id }) > 0;
    }
}
