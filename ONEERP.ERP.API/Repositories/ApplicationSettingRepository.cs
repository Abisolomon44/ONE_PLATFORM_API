using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IApplicationSettingRepository
{
    Task<IEnumerable<ApplicationSetting>> GetAllAsync();
    Task<ApplicationSetting?> GetByKeyAsync(string settingKey);
    Task<bool> UpsertAsync(string settingKey, string? settingValue, string? updatedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class ApplicationSettingRepository : TenantRepositoryBase, IApplicationSettingRepository
{
    public ApplicationSettingRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<ApplicationSetting>> GetAllAsync()
    {
        using var connection = OpenTenant();
        return await Sql.QueryAsync<ApplicationSetting>(connection, "SELECT * FROM dbo.ApplicationSettings ORDER BY SettingKey");
    }

    public async Task<ApplicationSetting?> GetByKeyAsync(string settingKey)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<ApplicationSetting>(connection,
            "SELECT * FROM dbo.ApplicationSettings WHERE SettingKey = @settingKey", new { settingKey });
    }

    public async Task<bool> UpsertAsync(string settingKey, string? settingValue, string? updatedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                IF EXISTS (SELECT 1 FROM dbo.ApplicationSettings WHERE SettingKey = @settingKey)
                    UPDATE dbo.ApplicationSettings SET SettingValue = @settingValue, UpdatedBy = @updatedBy, UpdatedDate = SYSUTCDATETIME() WHERE SettingKey = @settingKey;
                ELSE
                    INSERT INTO dbo.ApplicationSettings (SettingKey, SettingValue, UpdatedBy, UpdatedDate) VALUES (@settingKey, @settingValue, @updatedBy, SYSUTCDATETIME());";
            return await Sql.ExecuteAsync(conn, sql, new { settingKey, settingValue, updatedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
