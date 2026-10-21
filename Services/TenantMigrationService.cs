using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using ONEERP.Platform.API.Data;
using ONEERP.Platform.API.DTOs;
using ONEERP.Platform.API.Models;
using ONEERP.Platform.API.Repositories;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;

namespace ONEERP.Platform.API.Services;

public interface ITenantMigrationService
{
    Task<MigrationStatusDto> GetStatusAsync(int tenantId, CancellationToken ct = default);

    Task<MigrationRunResultDto> RunAsync(int tenantId, CancellationToken ct = default);

    Task<MigrationRunResultDto> RunSingleAsync(int tenantId, int migrationId, CancellationToken ct = default);

    Task<MigrationRunResultDto> RerunSingleAsync(int tenantId, int migrationId, CancellationToken ct = default);

    Task<IReadOnlyList<MigrationHistoryDto>> GetHistoryAsync(int tenantId, int take = 50, CancellationToken ct = default);
}

/// <summary>
/// Tenant database migration engine.
/// <para>
/// Angular only ever supplies a tenant id. Everything else is decided here:
/// the tenant database connection, the actual current version read from the
/// tenant database, the pending set, the SQL scripts, the execution order and
/// the transaction strategy.
/// </para>
/// <para>Run flow: lock -&gt; re-read version -&gt; recompute pending -&gt; execute
/// ascending -&gt; record history -&gt; update version -&gt; release lock.</para>
/// </summary>
public class TenantMigrationService : ITenantMigrationService
{
    private const int LockTimeoutMs = 0;
    private const int HistoryTake = 50;
    private const int MaxErrorLength = 2000;

    private readonly ITenantDatabaseResolver _resolver;
    private readonly IMigrationRepository _migrationRepository;
    private readonly ITenantMigrationHistoryRepository _historyRepository;
    private readonly IMigrationScriptProvider _scriptProvider;
    private readonly ITenantSchemaVersionStore _versionStore;
    private readonly ITenantMigrationExecutor _executor;
    private readonly ITenantMigrationLock _migrationLock;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<TenantMigrationService> _logger;

    public TenantMigrationService(
        ITenantDatabaseResolver resolver,
        IMigrationRepository migrationRepository,
        ITenantMigrationHistoryRepository historyRepository,
        IMigrationScriptProvider scriptProvider,
        ITenantSchemaVersionStore versionStore,
        ITenantMigrationExecutor executor,
        ITenantMigrationLock migrationLock,
        IAuditService auditService,
        ICurrentUser currentUser,
        ILogger<TenantMigrationService> logger)
    {
        _resolver = resolver;
        _migrationRepository = migrationRepository;
        _historyRepository = historyRepository;
        _scriptProvider = scriptProvider;
        _versionStore = versionStore;
        _executor = executor;
        _migrationLock = migrationLock;
        _auditService = auditService;
        _currentUser = currentUser;
        _logger = logger;
    }

    /* ------------------------------------------------------------------ */
    /* Status                                                              */
    /* ------------------------------------------------------------------ */

    public async Task<MigrationStatusDto> GetStatusAsync(int tenantId, CancellationToken ct = default)
    {
        var definitions = (await _migrationRepository.GetAllAsync()).ToList();
        var resolved = await _resolver.ResolveAsync(tenantId, ct);
        await using var tenantConnection = resolved.Connection;

        var currentVersion = await _versionStore.GetCurrentVersionAsync(tenantConnection, ct);
        var pending = MigrationPlanner.ResolvePending(currentVersion, definitions);
        var targetVersion = MigrationPlanner.ResolveTargetVersion(currentVersion, pending);

        var hasRunning = await _historyRepository.HasRunningAsync(tenantId);
        var latest = await _historyRepository.GetLatestAsync(tenantId);

        return new MigrationStatusDto
        {
            TenantId = tenantId,
            TenantName = resolved.Tenant.TenantName,
            DatabaseName = resolved.DatabaseName,
            CurrentVersion = currentVersion,
            TargetVersion = targetVersion,
            PendingCount = pending.Count,
            Status = MigrationPlanner.ResolveStatus(
                currentVersion,
                targetVersion,
                hasRunning,
                latest?.Status == MigrationStatus.Failed),
            LastError = latest?.Status == MigrationStatus.Failed ? latest.ErrorMessage : null,
            LastRunAt = latest?.StartedAt,
            PendingMigrations = pending.Select(d => new PendingMigrationDto
            {
                MigrationId = d.MigrationId,
                Version = d.Version,
                MigrationCode = d.MigrationCode,
                MigrationName = d.MigrationName,
                ScriptName = d.ScriptName,
                Status = MigrationStatus.Pending
            }).ToList(),
            Migrations = _scriptProvider.ListAvailableScripts()
                .Select(scriptName =>
                {
                    var definition = definitions.FirstOrDefault(d =>
                        !d.IsDeleted && d.IsActive && string.Equals(d.ScriptName, scriptName, StringComparison.OrdinalIgnoreCase));
                    return new PendingMigrationDto
                    {
                        MigrationId = definition?.MigrationId ?? 0,
                        Version = definition?.Version ?? 0,
                        MigrationCode = definition?.MigrationCode ?? string.Empty,
                        MigrationName = definition?.MigrationName ?? string.Empty,
                        ScriptName = scriptName,
                        Status = definition is not null
                            ? definition.Version <= currentVersion ? "APPLIED" : MigrationStatus.Pending
                            : scriptName.Equals("erp_full.sql", StringComparison.OrdinalIgnoreCase) ? "BASELINE"
                            : scriptName.Equals("erp_demo_data.sql", StringComparison.OrdinalIgnoreCase) || scriptName.Equals("erp_purchase_demo_seed.sql", StringComparison.OrdinalIgnoreCase) ? "DEMO DATA"
                            : scriptName.Equals("platform_schema.sql", StringComparison.OrdinalIgnoreCase) ? "PLATFORM"
                            : scriptName.StartsWith("erp_migration_", StringComparison.OrdinalIgnoreCase) ? "UNREGISTERED"
                            : "SQL FILE"
                    };
                })
                .OrderBy(m => m.Version == 0 ? int.MaxValue : m.Version)
                .ThenBy(m => m.ScriptName, StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }

    /* ------------------------------------------------------------------ */
    /* Run                                                                 */
    /* ------------------------------------------------------------------ */

    public async Task<MigrationRunResultDto> RunAsync(int tenantId, CancellationToken ct = default)
        => await RunCoreAsync(tenantId, null, ct);

    public async Task<MigrationRunResultDto> RunSingleAsync(int tenantId, int migrationId, CancellationToken ct = default)
        => await RunCoreAsync(tenantId, migrationId, ct);

    public async Task<MigrationRunResultDto> RerunSingleAsync(int tenantId, int migrationId, CancellationToken ct = default)
    {
        var executedBy = SafeUsername();
        var definitions = (await _migrationRepository.GetAllAsync()).ToList();
        var definition = definitions.FirstOrDefault(d => d.MigrationId == migrationId && d.IsActive && !d.IsDeleted)
            ?? throw new DomainException("The selected SQL is not a registered tenant migration.", 404);

        var resolved = await _resolver.ResolveAsync(tenantId, ct);
        await using var tenantConnection = resolved.Connection;
        await using var lockHandle = await _migrationLock.AcquireAsync(tenantId, LockTimeoutMs, ct);
        if (lockHandle is null)
            throw new DomainException($"A migration is already running for tenant '{resolved.Tenant.TenantName}'. Wait for it to finish.", 409);

        var currentVersion = await _versionStore.GetCurrentVersionAsync(tenantConnection, ct);
        if (definition.Version > currentVersion)
            throw new DomainException("This migration has not been applied yet. Use Run on the pending migration.", 409);

        var script = _scriptProvider.GetScript(definition.ScriptName);
        var checksum = _scriptProvider.ComputeChecksum(script);
        if (string.IsNullOrWhiteSpace(definition.Checksum) || MigrationPlanner.IsChecksumDrifted(definition, checksum))
            throw new DomainException($"Migration '{definition.MigrationCode}' is not checksum-pinned to the applied SQL and cannot be re-run.", 409);
        MigrationScriptGuard.EnsureSafe(definition, script);

        var executionId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow;
        var historyId = await _historyRepository.InsertRunningAsync(new TenantMigrationHistory
        {
            TenantId = tenantId,
            MigrationId = definition.MigrationId,
            ExecutionId = executionId,
            Version = definition.Version,
            MigrationCode = definition.MigrationCode,
            MigrationName = definition.MigrationName + " (re-run)",
            Status = MigrationStatus.Running,
            Checksum = checksum,
            StartedAt = startedAt,
            ExecutedBy = executedBy
        });

        try
        {
            await _executor.ReapplyAsync(tenantConnection, script, ct);
            var completedAt = DateTime.UtcNow;
            await _historyRepository.CompleteAsync(historyId, MigrationStatus.Success, completedAt,
                (int)Math.Max(0, (completedAt - startedAt).TotalMilliseconds), null);
            var result = new MigrationRunResultDto
            {
                ExecutionId = executionId,
                TenantId = tenantId,
                DatabaseName = resolved.DatabaseName,
                FromVersion = currentVersion,
                ToVersion = currentVersion,
                Status = MigrationStatus.Success,
                Message = $"Migration '{definition.MigrationCode}' re-ran successfully; tenant version remains {currentVersion}.",
                Executed = new List<MigrationExecutionDto>
                {
                    new()
                    {
                        HistoryId = historyId,
                        Version = definition.Version,
                        MigrationCode = definition.MigrationCode,
                        MigrationName = definition.MigrationName + " (re-run)",
                        StartedAt = startedAt,
                        CompletedAt = completedAt,
                        DurationMs = (int)Math.Max(0, (completedAt - startedAt).TotalMilliseconds),
                        Status = MigrationStatus.Success
                    }
                }
            };
            await AuditAsync(tenantId, "MIGRATE_RERUN", executedBy,
                $"ExecutionId={executionId}; Tenant={resolved.Tenant.TenantName}; Database={resolved.DatabaseName}; Migration={definition.MigrationCode}; checksum={checksum}");
            return result;
        }
        catch (Exception ex)
        {
            var completedAt = DateTime.UtcNow;
            var message = ex is SqlException sql
                ? $"SQL error {sql.Number}: {FirstLine(sql.Message)}"
                : FirstLine(ex.Message);
            await _historyRepository.CompleteAsync(historyId, MigrationStatus.Failed, completedAt,
                (int)Math.Max(0, (completedAt - startedAt).TotalMilliseconds), message);
            throw;
        }
    }

    private async Task<MigrationRunResultDto> RunCoreAsync(int tenantId, int? requestedMigrationId, CancellationToken ct)
    {
        var executedBy = SafeUsername();
        var definitions = (await _migrationRepository.GetAllAsync()).ToList();

        var resolved = await _resolver.ResolveAsync(tenantId, ct);
        await using var tenantConnection = resolved.Connection;

        // ---- 1. Acquire the tenant-scoped migration lock -----------------
        await using var lockHandle = await _migrationLock.AcquireAsync(tenantId, LockTimeoutMs, ct);
        if (lockHandle is null)
        {
            throw new DomainException(
                $"A migration is already running for tenant '{resolved.Tenant.TenantName}'. Wait for it to finish before starting another.",
                409);
        }

        // Close orphaned RUNNING rows only after the lock is held: the lock is
        // what proves no live execution exists, so reconciling earlier would
        // mark a genuinely running migration as failed and corrupt its history.
        await _historyRepository.FailOrphanedRunningAsync(tenantId, executedBy);

        // ---- 2. Re-read the authoritative version inside the lock ---------
        var currentVersion = await _versionStore.GetCurrentVersionAsync(tenantConnection, ct);

        var sequenceProblems = MigrationPlanner.ValidateSequence(
            currentVersion, definitions, TenantSchemaVersionStore.BaselineVersion);
        if (sequenceProblems.Count > 0)
            throw new DomainException("The migration registry is invalid: " + string.Join(" ", sequenceProblems), 409);

        // ---- 3. Recompute pending inside the lock ------------------------
        var pending = MigrationPlanner.ResolvePending(currentVersion, definitions);

        if (requestedMigrationId.HasValue)
        {
            var requested = pending.FirstOrDefault(d => d.MigrationId == requestedMigrationId.Value);
            if (requested is null)
                throw new DomainException("This migration is no longer pending for the tenant. Refresh the migration list and try again.", 409);

            var next = pending[0];
            if (requested.MigrationId != next.MigrationId)
                throw new DomainException(
                    $"Migration '{requested.MigrationCode}' cannot run yet. Run '{next.MigrationCode}' first to preserve version order.", 409);

            pending = new[] { requested };
        }

        if (pending.Count == 0)
        {
            _logger.LogInformation("Tenant {TenantId} is already at version {Version}; no SQL executed.", tenantId, currentVersion);
            await AuditAsync(tenantId, "MIGRATE_NOOP", executedBy,
                $"Tenant={resolved.Tenant.TenantName}; no pending migrations at version {currentVersion}.");

            return new MigrationRunResultDto
            {
                ExecutionId = Guid.NewGuid(),
                TenantId = tenantId,
                DatabaseName = resolved.DatabaseName,
                FromVersion = currentVersion,
                ToVersion = currentVersion,
                Status = MigrationStatus.UpToDate,
                Message = "Tenant is already up to date."
            };
        }

        // ---- 4. Verify every pending script before touching the database --
        var scripts = new Dictionary<MigrationDefinition, string>();
        foreach (var definition in pending)
        {
            var script = _scriptProvider.GetScript(definition.ScriptName);
            var checksum = _scriptProvider.ComputeChecksum(script);

            if (MigrationPlanner.IsChecksumDrifted(definition, checksum))
            {
                var hasFailedForTenant = await _historyRepository.HasFailedAsync(tenantId, definition.MigrationId);
                var hasSucceededForAnyTenant = await _historyRepository.HasSuccessfulForMigrationAsync(definition.MigrationId);
                if (!hasFailedForTenant || hasSucceededForAnyTenant)
                {
                    throw new DomainException(
                        $"Migration '{definition.MigrationCode}' has been modified since it was registered (checksum mismatch). " +
                        "A checksum can only be refreshed after a failed attempt when no tenant has successfully applied this migration.",
                        409);
                }

                // The tenant-level transaction rolled back the failed attempt. Repin only
                // when no tenant has applied this migration, so a repair cannot change the
                // checksum of an already-deployed schema version.
                await _migrationRepository.UpdateChecksumAsync(definition.MigrationId, checksum);
                await AuditAsync(tenantId, "MIGRATE_CHECKSUM_REFRESH", executedBy,
                    $"Migration={definition.MigrationCode}; checksum refreshed after a failed attempt; no tenant has a successful history row.");
                _logger.LogWarning(
                    "Refreshed checksum for migration {Code} after a failed attempt for tenant {TenantId}; no tenant has successfully applied it.",
                    definition.MigrationCode, tenantId);
            }

            scripts[definition] = script;
            MigrationScriptGuard.EnsureSafe(definition, script);
            await _migrationRepository.SetChecksumIfMissingAsync(definition.MigrationId, checksum);
        }

        // ---- 5. Execute sequentially, ascending version order -------------
        var executionId = Guid.NewGuid();
        var executed = new List<MigrationExecutionDto>();
        var appliedVersion = currentVersion;
        string finalStatus = MigrationStatus.Success;
        string? finalError = null;

        foreach (var definition in pending)
        {
            var startedAt = DateTime.UtcNow;
            long historyId;

            try
            {
                historyId = await _historyRepository.InsertRunningAsync(new TenantMigrationHistory
                {
                    TenantId = tenantId,
                    MigrationId = definition.MigrationId,
                    ExecutionId = executionId,
                    Version = definition.Version,
                    MigrationCode = definition.MigrationCode,
                    MigrationName = definition.MigrationName,
                    Status = MigrationStatus.Running,
                    Checksum = _scriptProvider.ComputeChecksum(scripts[definition]),
                    StartedAt = startedAt,
                    ExecutedBy = executedBy
                });
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                // UX_TenantMigrationHistory_Running rejected a concurrent run.
                throw new DomainException(
                    $"A migration is already running for tenant '{resolved.Tenant.TenantName}'.", 409);
            }

            try
            {
                await _executor.ApplyAsync(tenantConnection, definition, scripts[definition], executedBy, ct);

                var completedAt = DateTime.UtcNow;
                await _historyRepository.CompleteAsync(
                    historyId,
                    MigrationStatus.Success,
                    completedAt,
                    (int)Math.Max(0, (completedAt - startedAt).TotalMilliseconds),
                    null);

                appliedVersion = definition.Version;
                executed.Add(new MigrationExecutionDto
                {
                    HistoryId = historyId,
                    Version = definition.Version,
                    MigrationCode = definition.MigrationCode,
                    MigrationName = definition.MigrationName,
                    StartedAt = startedAt,
                    CompletedAt = completedAt,
                    Status = MigrationStatus.Success
                });

                _logger.LogInformation("Applied migration {Code} (v{Version}) to tenant {TenantId}.",
                    definition.MigrationCode, definition.Version, tenantId);
            }
            catch (Exception ex)
            {
                var completedAt = DateTime.UtcNow;
                var duration = (int)Math.Max(0, (completedAt - startedAt).TotalMilliseconds);
                var safeError = Sanitize(ex);

                await _historyRepository.CompleteAsync(historyId, MigrationStatus.Failed, completedAt, duration, safeError);

                executed.Add(new MigrationExecutionDto
                {
                    HistoryId = historyId,
                    Version = definition.Version,
                    MigrationCode = definition.MigrationCode,
                    MigrationName = definition.MigrationName,
                    StartedAt = startedAt,
                    CompletedAt = completedAt,
                    DurationMs = duration,
                    Status = MigrationStatus.Failed,
                    ErrorMessage = safeError
                });

                // Version stays at the last successfully applied migration and
                // the remaining migrations are not attempted.
                finalStatus = MigrationStatus.Failed;
                finalError = safeError;

                _logger.LogError(ex, "Migration {Code} (v{Version}) failed for tenant {TenantId} at version {Applied}.",
                    definition.MigrationCode, definition.Version, tenantId, appliedVersion);
                break;
            }
        }

        var message = finalStatus == MigrationStatus.Success
            ? $"Applied {executed.Count} migration(s): v{currentVersion} to v{appliedVersion}."
            : $"Migration failed at v{(executed.Count > 0 ? executed[^1].Version : currentVersion)}. Tenant remains at v{appliedVersion}.";

        await AuditAsync(
            tenantId,
            finalStatus == MigrationStatus.Success ? "MIGRATE_RUN" : "MIGRATE_FAILED",
            executedBy,
            $"ExecutionId={executionId}; Tenant={resolved.Tenant.TenantName}; " +
            $"Database={resolved.DatabaseName}; {currentVersion} -> {appliedVersion}; {message}");

        return new MigrationRunResultDto
        {
            ExecutionId = executionId,
            TenantId = tenantId,
            DatabaseName = resolved.DatabaseName,
            FromVersion = currentVersion,
            ToVersion = appliedVersion,
            Status = finalStatus,
            Message = message,
            Executed = executed
        };
    }

    /* ------------------------------------------------------------------ */
    /* History                                                             */
    /* ------------------------------------------------------------------ */

    public async Task<IReadOnlyList<MigrationHistoryDto>> GetHistoryAsync(int tenantId, int take = HistoryTake, CancellationToken ct = default)
    {
        await _resolver.ResolveTenantAsync(tenantId, ct);

        var rows = await _historyRepository.GetRecentAsync(tenantId, Math.Clamp(take, 1, 500));
        return rows.Select(r => new MigrationHistoryDto
        {
            HistoryId = r.TenantMigrationHistoryId,
            ExecutionId = r.ExecutionId,
            TenantId = r.TenantId,
            Version = r.Version,
            MigrationCode = r.MigrationCode,
            MigrationName = r.MigrationName,
            StartedAt = r.StartedAt,
            CompletedAt = r.CompletedAt,
            DurationMs = r.DurationMs,
            Status = r.Status,
            ExecutedBy = r.ExecutedBy,
            ErrorMessage = r.ErrorMessage
        }).ToList();
    }

    /* ------------------------------------------------------------------ */
    /* Internals                                                           */
    /* ------------------------------------------------------------------ */


    private Task AuditAsync(int tenantId, string action, string executedBy, string detail) =>
        _auditService.WriteAsync(
            entityName: "TenantMigration",
            entityId: tenantId.ToString(),
            action: action,
            performedBy: executedBy,
            tenantId: tenantId,
            newValues: detail);

    private string SafeUsername() => _currentUser.Username;

    /// <summary>
    /// Keeps raw SQL Server diagnostics (which include server, database and
    /// occasionally object names) out of the API response and the audit trail,
    /// while preserving the actionable part for the administrator.
    /// </summary>
    private static string Sanitize(Exception ex)
    {
        var message = ex is SqlException sql
            ? $"SQL error {sql.Number}: {FirstLine(sql.Message)}"
            : FirstLine(ex.Message);

        if (string.IsNullOrWhiteSpace(message))
            message = ex.GetType().Name;

        return message.Length > MaxErrorLength ? message[..MaxErrorLength] : message;
    }

    private static string FirstLine(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return string.Empty;

        var newline = message.IndexOfAny(new[] { '\r', '\n' });
        return (newline >= 0 ? message[..newline] : message).Trim();
    }
}
