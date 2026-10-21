using Dapper;
using Microsoft.Data.SqlClient;
using ONEERP.Platform.API.Data;
using ONEERP.Platform.API.Repositories;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;

namespace ONEERP.Platform.API.Services;

public sealed class TenantSqlFileExecutionResult
{
    public int TenantId { get; init; }
    public string DatabaseName { get; init; } = string.Empty;
    public string ScriptName { get; init; } = string.Empty;
    public string Status { get; init; } = MigrationStatus.Success;
    public string Message { get; init; } = string.Empty;
}

public interface ITenantSqlFileExecutionService
{
    Task<TenantSqlFileExecutionResult> RunAsync(int tenantId, string scriptName, string executedBy, CancellationToken ct = default);
}

/// <summary>Runs an embedded tenant SQL file selected by name; SQL text and connection data never come from the client.</summary>
public sealed class TenantSqlFileExecutionService(
    ITenantDatabaseResolver resolver,
    IMigrationScriptProvider scripts,
    ITenantMigrationLock migrationLock,
    ITenantMigrationHistoryRepository history,
    ITenantSchemaVersionStore versionStore,
    IAuditService audit,
    ILogger<TenantSqlFileExecutionService> logger) : ITenantSqlFileExecutionService
{
    public async Task<TenantSqlFileExecutionResult> RunAsync(int tenantId, string scriptName, string executedBy, CancellationToken ct = default)
    {
        var fileName = Path.GetFileName(scriptName ?? string.Empty).Trim();
        if (!string.Equals(fileName, scriptName?.Trim(), StringComparison.Ordinal)
            || !scripts.ListAvailableScripts().Contains(fileName, StringComparer.OrdinalIgnoreCase)
            || fileName.Equals("platform_schema.sql", StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Select an available tenant SQL file. Platform schema SQL cannot run against a tenant.", 400);

        var resolved = await resolver.ResolveAsync(tenantId, ct);
        await using var connection = resolved.Connection;
        await using var lockHandle = await migrationLock.AcquireAsync(tenantId, 0, ct)
            ?? throw new DomainException($"A migration is already running for tenant '{resolved.Tenant.TenantName}'.", 409);

        var script = scripts.GetScript(fileName);
        var checksum = scripts.ComputeChecksum(script);
        var currentVersion = await versionStore.GetCurrentVersionAsync(connection, ct);
        var startedAt = DateTime.UtcNow;
        var executionId = Guid.NewGuid();
        var historyId = await history.InsertRunningAsync(new Models.TenantMigrationHistory
        {
            TenantId = tenantId,
            MigrationId = 0,
            ExecutionId = executionId,
            Version = currentVersion,
            MigrationCode = "ERP-SQL-FILE",
            MigrationName = fileName,
            Status = MigrationStatus.Running,
            Checksum = checksum,
            StartedAt = startedAt,
            ExecutedBy = executedBy
        });

        try
        {
            object? parameters = null;
            if (fileName.Equals("erp_full.sql", StringComparison.OrdinalIgnoreCase))
            {
                var baselineParameters = new DynamicParameters();
                baselineParameters.Add("CompanyCode", resolved.Tenant.TenantCode);
                baselineParameters.Add("CompanyName", resolved.Tenant.CompanyName ?? resolved.Tenant.TenantName);
                baselineParameters.Add("CompanyEmail", resolved.Tenant.ContactEmail ?? string.Empty);
                baselineParameters.Add("CurrencyCode", "USD");
                baselineParameters.Add("AdminUsername", resolved.Tenant.AdminUsername ?? string.Empty);
                baselineParameters.Add("AdminPasswordHash", resolved.Tenant.AdminPassword ?? string.Empty);
                baselineParameters.Add("AdminFullName", resolved.Tenant.AdminUsername ?? "ERP Administrator");
                parameters = baselineParameters;
            }

            await connection.ExecuteAsync(new CommandDefinition(script, parameters, commandTimeout: 300, cancellationToken: ct));
            var completedAt = DateTime.UtcNow;
            await history.CompleteAsync(historyId, MigrationStatus.Success, completedAt,
                (int)Math.Max(0, (completedAt - startedAt).TotalMilliseconds), null);
            await audit.WriteAsync("TenantSqlFile", tenantId.ToString(), "RUN", executedBy, tenantId,
                $"Script={fileName}; Checksum={checksum}; ExecutionId={executionId}");

            return new TenantSqlFileExecutionResult
            {
                TenantId = tenantId,
                DatabaseName = resolved.DatabaseName,
                ScriptName = fileName,
                Message = $"SQL file '{fileName}' executed successfully."
            };
        }
        catch (Exception ex)
        {
            var completedAt = DateTime.UtcNow;
            var message = ex is SqlException sql ? $"SQL error {sql.Number}: {sql.Message.Split('\n')[0]}" : ex.Message.Split('\n')[0];
            logger.LogError(ex, "SQL file {ScriptName} failed for tenant {TenantId}.", fileName, tenantId);
            await history.CompleteAsync(historyId, MigrationStatus.Failed, completedAt,
                (int)Math.Max(0, (completedAt - startedAt).TotalMilliseconds), message);
            throw new DomainException($"SQL file '{fileName}' failed: {message}", 500);
        }
    }
}
