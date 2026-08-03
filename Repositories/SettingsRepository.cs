using ONEERP.Platform.API.Models;

namespace ONEERP.Platform.API.Repositories;

public interface ISettingsRepository
{
    Task<IEnumerable<Settings>> GetAllAsync();
    Task<Settings?> GetByKeyAsync(string settingKey);
}

public class SettingsRepository : BaseRepository, ISettingsRepository
{
    public SettingsRepository(IDbConnectionFactory factory, ISqlHelper sql) : base(factory, sql)
    {
    }

    public async Task<IEnumerable<Settings>> GetAllAsync()
    {
        using var connection = OpenPlatform();
        return await Sql.QueryAsync<Settings>(connection, "SELECT * FROM dbo.Settings ORDER BY SettingKey");
    }

    public async Task<Settings?> GetByKeyAsync(string settingKey)
    {
        using var connection = OpenPlatform();
        return await Sql.QuerySingleOrDefaultAsync<Settings>(
            connection, "SELECT * FROM dbo.Settings WHERE SettingKey = @settingKey", new { settingKey });
    }
}
