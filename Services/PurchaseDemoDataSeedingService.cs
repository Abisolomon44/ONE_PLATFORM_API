using Dapper;
using Microsoft.Data.SqlClient;
using ONEERP.Platform.API.Data;
using ONEERP.Platform.API.Repositories;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;

namespace ONEERP.Platform.API.Services;

public interface IPurchaseDemoDataSeedingService
{
    Task<DemoSeedResultDto> SeedAsync(int tenantId, string executedBy, CancellationToken ct = default);
}

public sealed class PurchaseDemoDataSeedingService(
    ITenantDatabaseResolver resolver,
    IPurchaseDemoDataScriptProvider scriptProvider,
    ITenantMigrationLock migrationLock,
    ITenantMigrationHistoryRepository history,
    IAuditService audit) : IPurchaseDemoDataSeedingService
{
    public async Task<DemoSeedResultDto> SeedAsync(int tenantId, string executedBy, CancellationToken ct = default)
    {
        var resolved = await resolver.ResolveAsync(tenantId, ct);
        await using var connection = resolved.Connection;
        await using var handle = await migrationLock.AcquireAsync(tenantId, 0, ct)
            ?? throw new DomainException("A migration is already running for this tenant.", 409);
        var script = scriptProvider.GetScript();
        var checksum = scriptProvider.ComputeChecksum(script);
        var started = DateTime.UtcNow;
        var historyId = await history.InsertRunningAsync(new Models.TenantMigrationHistory {
            TenantId = tenantId, MigrationId = 0, ExecutionId = Guid.NewGuid(), Version = 0,
            MigrationCode = "ERP-DEMO-PURCHASE-001", MigrationName = "Purchase demo data seed",
            Status = MigrationStatus.Running, Checksum = checksum, StartedAt = started, ExecutedBy = executedBy
        });
        try
        {
            await connection.ExecuteAsync(new CommandDefinition(script, commandTimeout: 300, cancellationToken: ct));
            var completed = DateTime.UtcNow;
            await history.CompleteAsync(historyId, MigrationStatus.Success, completed, (int)(completed-started).TotalMilliseconds, null);
            await audit.WriteAsync("TenantPurchaseDemoSeed", tenantId.ToString(), "SEED", executedBy, tenantId, $"Checksum={checksum}");
            return new DemoSeedResultDto { TenantId=tenantId, DatabaseName=resolved.DatabaseName, Status="APPLIED", Message="Purchase demo data seeded successfully.", Checksum=checksum };
        }
        catch (Exception ex)
        {
            var completed = DateTime.UtcNow;
            var message = ex is SqlException sql ? $"SQL error {sql.Number}: {sql.Message.Split('\n')[0]}" : ex.Message.Split('\n')[0];
            await history.CompleteAsync(historyId, MigrationStatus.Failed, completed, (int)(completed-started).TotalMilliseconds, message);
            throw new DomainException($"Purchase demo seeding failed: {message}", 500);
        }
    }
}
