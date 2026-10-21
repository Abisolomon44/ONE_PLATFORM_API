using Dapper;
using Microsoft.Data.SqlClient;
using ONEERP.Platform.API.Data;
using ONEERP.Platform.API.Repositories;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;

namespace ONEERP.Platform.API.Services;

public interface IPurchaseReturnScreenSeedService
{
    Task<DemoSeedResultDto> SeedAsync(int tenantId, string executedBy, CancellationToken ct = default);
}

/// <summary>Executes the embedded, audited Purchase Return screen registry seed.</summary>
public sealed class PurchaseReturnScreenSeedService(
    ITenantDatabaseResolver resolver,
    IMigrationScriptProvider scriptProvider,
    ITenantMigrationLock migrationLock,
    ITenantMigrationHistoryRepository history,
    IAuditService audit) : IPurchaseReturnScreenSeedService
{
    private const string ScriptName = "erp_purchase_return_screen_seed.sql";

    public async Task<DemoSeedResultDto> SeedAsync(int tenantId, string executedBy, CancellationToken ct = default)
    {
        var resolved = await resolver.ResolveAsync(tenantId, ct);
        await using var connection = resolved.Connection;
        await using var lockHandle = await migrationLock.AcquireAsync(tenantId, 0, ct)
            ?? throw new DomainException("A migration is already running for this tenant.", 409);

        var script = scriptProvider.GetScript(ScriptName);
        var checksum = scriptProvider.ComputeChecksum(script);
        var startedAt = DateTime.UtcNow;
        var historyId = await history.InsertRunningAsync(new Models.TenantMigrationHistory
        {
            TenantId = tenantId,
            MigrationId = 0,
            ExecutionId = Guid.NewGuid(),
            Version = 0,
            MigrationCode = "ERP-DEMO-PURCHASE-RETURNS-SCREENS-001",
            MigrationName = "Purchase Return screen registry seed",
            Status = MigrationStatus.Running,
            Checksum = checksum,
            StartedAt = startedAt,
            ExecutedBy = executedBy
        });

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(script, commandTimeout: 300, cancellationToken: ct));
            var completedAt = DateTime.UtcNow;
            await history.CompleteAsync(historyId, MigrationStatus.Success, completedAt,
                (int)Math.Max(0, (completedAt - startedAt).TotalMilliseconds), null);
            await audit.WriteAsync("TenantPurchaseReturnScreenSeed", tenantId.ToString(), "SEED", executedBy,
                tenantId, $"Script={ScriptName}; Checksum={checksum}");

            return new DemoSeedResultDto
            {
                TenantId = tenantId,
                DatabaseName = resolved.DatabaseName,
                Status = "APPLIED",
                Message = "Purchase Return screen registry seed executed successfully.",
                Checksum = checksum
            };
        }
        catch (Exception ex)
        {
            var completedAt = DateTime.UtcNow;
            var error = ex is SqlException sql
                ? $"SQL error {sql.Number}: {FirstLine(sql.Message)}"
                : FirstLine(ex.Message);
            await history.CompleteAsync(historyId, MigrationStatus.Failed, completedAt,
                (int)Math.Max(0, (completedAt - startedAt).TotalMilliseconds), error);
            throw new DomainException($"Purchase Return screen seed failed: {error}", 500);
        }
    }

    private static string FirstLine(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return string.Empty;
        var newline = message.IndexOfAny(new[] { '\r', '\n' });
        return (newline >= 0 ? message[..newline] : message).Trim();
    }
}
