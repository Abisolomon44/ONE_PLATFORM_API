using System.Data.Common;
using Dapper;
using ONEERP.Platform.API.Data;
using ONEERP.Platform.API.Models;
using ONEERP.Shared.Constants;

namespace ONEERP.Platform.API.Services;

/// <summary>
/// Applies a single migration script to a tenant database.
/// <para>
/// The script and the schema-version bump share one transaction, so a
/// migration either applies completely or leaves no trace. That is only
/// honest because the shipped sql/erp_migration_*.sql scripts contain nothing
/// that SQL Server cannot roll back: no COMMIT, no BEGIN TRANSACTION, no
/// EXEC, no TRUNCATE and no DROP TABLE. This is asserted by
/// <see cref="MigrationScriptGuard"/> before a script is allowed to run.
/// </para>
/// </summary>
public interface ITenantMigrationExecutor
{
    Task ApplyAsync(
        DbConnection connection,
        MigrationDefinition definition,
        string script,
        string executedBy,
        CancellationToken ct = default);

    Task ReapplyAsync(DbConnection connection, string script, CancellationToken ct = default);
}

public class TenantMigrationExecutor : ITenantMigrationExecutor
{
    private readonly ITenantSchemaVersionStore _versionStore;
    private readonly ILogger<TenantMigrationExecutor> _logger;
    private readonly int _commandTimeoutSeconds;

    public TenantMigrationExecutor(
        ITenantSchemaVersionStore versionStore,
        IConfiguration configuration,
        ILogger<TenantMigrationExecutor> logger)
    {
        _versionStore = versionStore;
        _logger = logger;
        _commandTimeoutSeconds = configuration.GetValue("Provisioning:CommandTimeoutSeconds", 300);
    }

    public async Task ApplyAsync(
        DbConnection connection,
        MigrationDefinition definition,
        string script,
        string executedBy,
        CancellationToken ct = default)
    {
        DbTransaction? transaction = null;
        try
        {
            transaction = await connection.BeginTransactionAsync(ct);

            await connection.ExecuteAsync(new CommandDefinition(
                script,
                transaction: transaction,
                commandTimeout: _commandTimeoutSeconds,
                cancellationToken: ct));

            await _versionStore.SetVersionAsync(connection, transaction, definition.Version, executedBy, ct);

            await transaction.CommitAsync(ct);
        }
        catch
        {
            if (transaction is not null)
            {
                try
                {
                    await transaction.RollbackAsync(ct);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogWarning(rollbackEx,
                        "Rollback of migration {Code} failed; SQL Server discards the open transaction when the connection closes.",
                        definition.MigrationCode);
                }
            }

            throw;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    /// <summary>Re-executes an already applied, checksum-pinned migration without changing schema version.</summary>
    public async Task ReapplyAsync(DbConnection connection, string script, CancellationToken ct = default)
    {
        await using var transaction = await connection.BeginTransactionAsync(ct);
        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                script,
                transaction: transaction,
                commandTimeout: _commandTimeoutSeconds,
                cancellationToken: ct));
            await transaction.CommitAsync(ct);
        }
        catch
        {
            try { await transaction.RollbackAsync(ct); }
            catch (Exception rollbackEx)
            {
                _logger.LogWarning(rollbackEx,
                    "Rollback of migration re-run failed; SQL Server discards the open transaction when the connection closes.");
            }
            throw;
        }
    }
}

/// <summary>
/// Rejects scripts whose contents would make the per-migration transaction a
/// lie (an embedded COMMIT would end the transaction early, leaving the
/// version bump to run outside it). GO batch separators are also rejected
/// because Dapper sends the script as a single batch.
/// </summary>
public static class MigrationScriptGuard
{
    private static readonly string[] ForbiddenTokens =
    {
        "COMMIT", "BEGIN TRAN", "BEGIN DISTRIBUTED TRAN", "ROLLBACK", "TRUNCATE TABLE", "DROP DATABASE"
    };

    /// <summary>Returns the reasons a script cannot be executed safely (empty when safe).</summary>
    public static IReadOnlyList<string> Inspect(string script)
    {
        var problems = new List<string>();
        if (string.IsNullOrWhiteSpace(script))
        {
            problems.Add("Script is empty.");
            return problems;
        }

        foreach (var raw in script.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("--", StringComparison.Ordinal) || line.StartsWith("/*", StringComparison.Ordinal))
                continue;

            if (line.Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                problems.Add("Script contains a GO batch separator, which cannot be executed as a single transaction.");
                continue;
            }

            foreach (var token in ForbiddenTokens)
            {
                if (line.StartsWith(token, StringComparison.OrdinalIgnoreCase)
                    || line.Contains($" {token}", StringComparison.OrdinalIgnoreCase))
                {
                    problems.Add($"Script contains '{token}', which cannot be rolled back safely.");
                    break;
                }
            }
        }

        return problems.Distinct().ToList();
    }

    public static void EnsureSafe(MigrationDefinition definition, string script)
    {
        var problems = Inspect(script);
        if (problems.Count > 0)
            throw new InvalidOperationException(
                $"Migration '{definition.MigrationCode}' cannot be executed safely: {string.Join(" ", problems)}");
    }
}
