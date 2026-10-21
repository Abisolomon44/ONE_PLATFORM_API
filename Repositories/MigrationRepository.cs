using ONEERP.Platform.API.Models;

namespace ONEERP.Platform.API.Repositories;

public interface IMigrationRepository
{
    /// <summary>All non-deleted migration definitions in ascending version order.</summary>
    Task<IEnumerable<MigrationDefinition>> GetAllAsync(bool activeOnly = true);

    Task<MigrationDefinition?> GetByIdAsync(int migrationId);

    /// <summary>Pins the checksum the first time a script is seen so drift is detectable later.</summary>
    Task SetChecksumIfMissingAsync(int migrationId, string checksum);

    /// <summary>Renews the stored checksum after an intentional script edit.</summary>
    Task UpdateChecksumAsync(int migrationId, string checksum);
}

public class MigrationRepository : BaseRepository, IMigrationRepository
{
    private const string SelectColumns = @"
    SELECT MigrationId, Version, MigrationCode, MigrationName, ScriptName,
           Checksum, [Description], IsActive, CreatedBy, CreatedDate,
           ModifiedBy, ModifiedDate, IsDeleted
    FROM dbo.Migrations
    WHERE IsDeleted = 0";

    public MigrationRepository(IDbConnectionFactory factory, ISqlHelper sql) : base(factory, sql)
    {
    }

    public async Task<IEnumerable<MigrationDefinition>> GetAllAsync(bool activeOnly = true)
    {
        using var connection = OpenPlatform();
        var sql = SelectColumns + (activeOnly ? " AND IsActive = 1" : string.Empty) + " ORDER BY Version ASC";
        return await Sql.QueryAsync<MigrationDefinition>(connection, sql);
    }

    public async Task<MigrationDefinition?> GetByIdAsync(int migrationId)
    {
        using var connection = OpenPlatform();
        return await Sql.QuerySingleOrDefaultAsync<MigrationDefinition>(
            connection, SelectColumns + " AND MigrationId = @migrationId", new { migrationId });
    }

    public Task SetChecksumIfMissingAsync(int migrationId, string checksum) => ExecuteAsync(
        "UPDATE dbo.Migrations SET Checksum = @checksum, ModifiedDate = SYSUTCDATETIME() WHERE MigrationId = @migrationId AND (Checksum IS NULL OR Checksum = '')",
        new { migrationId, checksum });

    public Task UpdateChecksumAsync(int migrationId, string checksum) => ExecuteAsync(
        "UPDATE dbo.Migrations SET Checksum = @checksum, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE MigrationId = @migrationId",
        new { migrationId, checksum, modifiedBy = "system" });

    private async Task ExecuteAsync(string sql, object parameters)
    {
        using var connection = OpenPlatform();
        await Sql.ExecuteAsync(connection, sql, parameters);
    }
}
