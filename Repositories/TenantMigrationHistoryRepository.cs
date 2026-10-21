using ONEERP.Platform.API.Models;
using ONEERP.Shared.Constants;

namespace ONEERP.Platform.API.Repositories;

public interface ITenantMigrationHistoryRepository
{
    Task<long> InsertRunningAsync(TenantMigrationHistory history);

    Task<bool> CompleteAsync(long historyId, string status, DateTime completedAt, int durationMs, string? errorMessage);

    Task<IEnumerable<TenantMigrationHistory>> GetRecentAsync(int tenantId, int take);

    Task<TenantMigrationHistory?> GetLatestAsync(int tenantId);

    Task<bool> HasFailedAsync(int tenantId, int migrationId);

    Task<bool> HasSuccessfulForMigrationAsync(int migrationId);

    Task<bool> HasRunningAsync(int tenantId);

    /// <summary>
    /// Marks RUNNING rows left behind by a crashed process as FAILED so the
    /// tenant-scoped RUNNING unique index does not block future runs.
    /// </summary>
    Task<int> FailOrphanedRunningAsync(int tenantId, string executedBy);
}

public class TenantMigrationHistoryRepository : BaseRepository, ITenantMigrationHistoryRepository
{
    private const string SelectColumns = @"
    SELECT TenantMigrationHistoryId, TenantId, MigrationId, ExecutionId, Version,
           MigrationCode, MigrationName, Status, Checksum, StartedAt, CompletedAt,
           DurationMs, ExecutedBy, ErrorMessage, CreatedDate
    FROM dbo.TenantMigrationHistory";

    private const string SelectRecentSql = @"
    SELECT TOP (@take)
           h.TenantMigrationHistoryId, h.TenantId, h.MigrationId, h.ExecutionId, h.Version,
           h.MigrationCode, h.MigrationName, h.Status, h.Checksum, h.StartedAt, h.CompletedAt,
           h.DurationMs, h.ExecutedBy, h.ErrorMessage, h.CreatedDate
    FROM dbo.TenantMigrationHistory h
    WHERE h.TenantId = @tenantId
    ORDER BY h.TenantMigrationHistoryId DESC";

    public TenantMigrationHistoryRepository(IDbConnectionFactory factory, ISqlHelper sql) : base(factory, sql)
    {
    }

    public async Task<long> InsertRunningAsync(TenantMigrationHistory history)
    {
        using var connection = OpenPlatform();
        const string sql = @"
            INSERT INTO dbo.TenantMigrationHistory
                (TenantId, MigrationId, ExecutionId, Version, MigrationCode, MigrationName,
                 Status, Checksum, StartedAt, ExecutedBy, ErrorMessage, CreatedDate)
            VALUES
                (@TenantId, @MigrationId, @ExecutionId, @Version, @MigrationCode, @MigrationName,
                 @Status, @Checksum, @StartedAt, @ExecutedBy, NULL, SYSUTCDATETIME());
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(connection, sql, history);
    }

    public async Task<bool> CompleteAsync(long historyId, string status, DateTime completedAt, int durationMs, string? errorMessage)
    {
        using var connection = OpenPlatform();
        const string sql = @"
            UPDATE dbo.TenantMigrationHistory
            SET Status = @status, CompletedAt = @completedAt, DurationMs = @durationMs, ErrorMessage = @errorMessage
            WHERE TenantMigrationHistoryId = @historyId AND Status = 'RUNNING';";
        return await Sql.ExecuteAsync(connection, sql, new { historyId, status, completedAt, durationMs, errorMessage }) > 0;
    }

    public async Task<IEnumerable<TenantMigrationHistory>> GetRecentAsync(int tenantId, int take)
    {
        using var connection = OpenPlatform();
        return await Sql.QueryAsync<TenantMigrationHistory>(
            connection,
            SelectRecentSql,
            new { tenantId, take },
            commandTimeout: 30);
    }

    public async Task<TenantMigrationHistory?> GetLatestAsync(int tenantId)
    {
        using var connection = OpenPlatform();
        return await Sql.QueryFirstOrDefaultAsync<TenantMigrationHistory>(
            connection,
            SelectColumns + " WHERE TenantId = @tenantId ORDER BY TenantMigrationHistoryId DESC",
            new { tenantId });
    }

    public async Task<bool> HasFailedAsync(int tenantId, int migrationId)
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteScalarAsync<int>(
            connection,
            "SELECT COUNT(1) FROM dbo.TenantMigrationHistory WHERE TenantId = @tenantId AND MigrationId = @migrationId AND Status = 'FAILED';",
            new { tenantId, migrationId }) > 0;
    }

    public async Task<bool> HasSuccessfulForMigrationAsync(int migrationId)
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteScalarAsync<int>(
            connection,
            "SELECT COUNT(1) FROM dbo.TenantMigrationHistory WHERE MigrationId = @migrationId AND Status = 'SUCCESS';",
            new { migrationId }) > 0;
    }

    public async Task<bool> HasRunningAsync(int tenantId)
    {
        using var connection = OpenPlatform();
        return await Sql.ExecuteScalarAsync<int>(
            connection,
            "SELECT COUNT(1) FROM dbo.TenantMigrationHistory WHERE TenantId = @tenantId AND Status = 'RUNNING';",
            new { tenantId }) > 0;
    }

    public async Task<int> FailOrphanedRunningAsync(int tenantId, string executedBy)
    {
        using var connection = OpenPlatform();
        const string sql = @"
            UPDATE dbo.TenantMigrationHistory
            SET Status = @failed,
                CompletedAt = SYSUTCDATETIME(),
                ErrorMessage = COALESCE(ErrorMessage, @message)
            WHERE TenantId = @tenantId AND Status = @running;";
        return await Sql.ExecuteAsync(connection, sql, new
        {
            tenantId,
            failed = MigrationStatus.Failed,
            running = MigrationStatus.Running,
            message = "Migration execution was interrupted before it completed. Re-check the tenant database before retrying."
        });
    }
}
