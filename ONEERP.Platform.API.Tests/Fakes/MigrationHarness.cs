using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;
using ONEERP.Platform.API.Data;
using ONEERP.Platform.API.Models;
using ONEERP.Platform.API.Repositories;
using ONEERP.Platform.API.Services;

namespace ONEERP.Platform.API.Tests.Fakes;

/// <summary>
/// In-memory stand-ins for every collaborator of <see cref="TenantMigrationService"/>.
/// Only the tenant database itself is not faked, and it is handed over as an
/// unopened SqlConnection that no fake ever touches, so the suite needs no
/// SQL Server instance.
/// </summary>
internal sealed class MigrationHarness
{
    public const int TenantId = 12;

    private readonly List<TenantMigrationHistory> _history = new();
    private long _nextHistoryId = 1;

    public List<MigrationDefinition> Definitions { get; } = new();
    public List<AuditEntry> Audits { get; } = new();
    public List<string> ExecutedScripts { get; } = new();
    public List<int> RecordedVersions { get; } = new();

    public int CurrentVersion { get; set; } = TenantSchemaVersionStore.BaselineVersion;
    public bool LockAvailable { get; set; } = true;
    public int? FailAtVersion { get; set; }
    public string? HardFailureMessage { get; set; }
    public Tenant Tenant { get; set; } = new()
    {
        TenantId = TenantId,
        TenantCode = "ABC",
        TenantName = "ABC Company",
        DatabaseName = "ERP_ABC",
        Status = "Active"
    };

    public IReadOnlyList<TenantMigrationHistory> History => _history;

    public TenantMigrationService BuildService()
    {
        var resolver = new FakeResolver(this);
        var migrationRepository = new FakeMigrationRepository(this);
        var historyRepository = new FakeHistoryRepository(this);
        var scriptProvider = new FakeScriptProvider(this);
        var versionStore = new FakeVersionStore(this);
        var executor = new FakeExecutor(this);
        var migrationLock = new FakeLock(this);
        var audit = new FakeAuditService(this);
        var currentUser = new FakeCurrentUser();

        return new TenantMigrationService(
            resolver,
            migrationRepository,
            historyRepository,
            scriptProvider,
            versionStore,
            executor,
            migrationLock,
            audit,
            currentUser,
            NullLogger<TenantMigrationService>.Instance);
    }

    /// <summary>
    /// A fresh registry row has no pinned checksum, which is the state the
    /// platform schema seeds it in; the engine pins it on first successful run.
    /// </summary>
    public static MigrationDefinition Definition(int version, string name, string? checksum = null)
        => new()
        {
            MigrationId = version,
            Version = version,
            MigrationCode = $"{version:000}_{name}",
            MigrationName = name,
            ScriptName = $"erp_migration_{version:000}_{name}.sql",
            Checksum = checksum,
            IsActive = true
        };

    private sealed class FakeResolver : ITenantDatabaseResolver
    {
        private readonly MigrationHarness _h;
        public FakeResolver(MigrationHarness h) => _h = h;

        public Task<Tenant> ResolveTenantAsync(int tenantId, CancellationToken ct = default)
            => Task.FromResult(tenantId == _h.Tenant.TenantId
                ? _h.Tenant
                : throw new ONEERP.Shared.Exceptions.NotFoundException($"Tenant {tenantId} was not found."));

        public Task<ResolvedTenantDatabase> ResolveAsync(int tenantId, CancellationToken ct = default)
        {
            var tenant = _h.Tenant;
            if (tenantId != tenant.TenantId)
                throw new ONEERP.Shared.Exceptions.NotFoundException($"Tenant {tenantId} was not found.");

            // Never opened: the fakes below never touch the connection.
            return Task.FromResult(new ResolvedTenantDatabase(
                tenant,
                tenant.DatabaseName,
                new SqlConnection("Server=(unused);Database=(unused);Integrated Security=True;")));
        }
    }

    private sealed class FakeMigrationRepository : IMigrationRepository
    {
        private readonly MigrationHarness _h;
        public FakeMigrationRepository(MigrationHarness h) => _h = h;

        public Task<IEnumerable<MigrationDefinition>> GetAllAsync(bool activeOnly = true)
            => Task.FromResult<IEnumerable<MigrationDefinition>>(_h.Definitions.OrderBy(d => d.Version).ToList());

        public Task<MigrationDefinition?> GetByIdAsync(int migrationId)
            => Task.FromResult(_h.Definitions.FirstOrDefault(d => d.MigrationId == migrationId));

        public Task SetChecksumIfMissingAsync(int migrationId, string checksum)
        {
            var definition = _h.Definitions.FirstOrDefault(d => d.MigrationId == migrationId);
            if (definition is not null && string.IsNullOrWhiteSpace(definition.Checksum))
                definition.Checksum = checksum;
            return Task.CompletedTask;
        }

        public Task UpdateChecksumAsync(int migrationId, string checksum) => Task.CompletedTask;
    }

    private sealed class FakeHistoryRepository : ITenantMigrationHistoryRepository
    {
        private readonly MigrationHarness _h;
        public FakeHistoryRepository(MigrationHarness h) => _h = h;

        public Task<long> InsertRunningAsync(TenantMigrationHistory history)
        {
            if (_h.History.Any(x => x.Status == ONEERP.Shared.Constants.MigrationStatus.Running))
                throw new InvalidOperationException("A migration is already running for this tenant.");

            var id = _h._nextHistoryId++;
            _h._history.Add(new TenantMigrationHistory
            {
                TenantMigrationHistoryId = id,
                TenantId = history.TenantId,
                MigrationId = history.MigrationId,
                ExecutionId = history.ExecutionId,
                Version = history.Version,
                MigrationCode = history.MigrationCode,
                MigrationName = history.MigrationName,
                Status = ONEERP.Shared.Constants.MigrationStatus.Running,
                Checksum = history.Checksum,
                StartedAt = history.StartedAt,
                ExecutedBy = history.ExecutedBy
            });
            return Task.FromResult(id);
        }

        public Task<bool> CompleteAsync(long historyId, string status, DateTime completedAt, int durationMs, string? errorMessage)
        {
            var row = _h._history.First(x => x.TenantMigrationHistoryId == historyId);
            row.Status = status;
            row.CompletedAt = completedAt;
            row.DurationMs = durationMs;
            row.ErrorMessage = errorMessage;
            return Task.FromResult(true);
        }

        public Task<IEnumerable<TenantMigrationHistory>> GetRecentAsync(int tenantId, int take)
            => Task.FromResult<IEnumerable<TenantMigrationHistory>>(_h._history.TakeLast(take).Reverse().ToList());

        public Task<TenantMigrationHistory?> GetLatestAsync(int tenantId)
            => Task.FromResult(_h._history.LastOrDefault());

        public Task<bool> HasRunningAsync(int tenantId)
            => Task.FromResult(_h._history.Any(x => x.Status == ONEERP.Shared.Constants.MigrationStatus.Running));

        public Task<int> FailOrphanedRunningAsync(int tenantId, string executedBy)
        {
            var orphaned = _h._history.Where(x => x.Status == ONEERP.Shared.Constants.MigrationStatus.Running).ToList();
            foreach (var row in orphaned)
            {
                row.Status = ONEERP.Shared.Constants.MigrationStatus.Failed;
                row.CompletedAt = DateTime.UtcNow;
                row.ErrorMessage = "Migration execution was interrupted before it completed.";
            }
            return Task.FromResult(orphaned.Count);
        }
    }

    private sealed class FakeScriptProvider : IMigrationScriptProvider
    {
        private readonly MigrationHarness _h;
        public FakeScriptProvider(MigrationHarness h) => _h = h;

        public string GetScript(string scriptName)
        {
            var definition = _h.Definitions.FirstOrDefault(d => d.ScriptName == scriptName)
                ?? throw new InvalidOperationException($"Embedded SQL script '{scriptName}' was not found.");
            return $"-- {definition.MigrationCode}\nCREATE TABLE dbo.Probe_{definition.Version} (Id INT);\n";
        }

        public string ComputeChecksum(string script)
        {
            var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(script));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        public IReadOnlyList<string> ListAvailableScripts()
            => _h.Definitions.Select(d => d.ScriptName).ToList();
    }

    private sealed class FakeVersionStore : ITenantSchemaVersionStore
    {
        private readonly MigrationHarness _h;
        public FakeVersionStore(MigrationHarness h) => _h = h;

        public Task<int> GetCurrentVersionAsync(DbConnection tenantConnection, CancellationToken ct = default)
            => Task.FromResult(_h.CurrentVersion);

        public Task SetVersionAsync(DbConnection tenantConnection, DbTransaction transaction, int version, string? appliedBy, CancellationToken ct = default)
        {
            _h.RecordedVersions.Add(version);
            _h.CurrentVersion = version;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeExecutor : ITenantMigrationExecutor
    {
        private readonly MigrationHarness _h;
        public FakeExecutor(MigrationHarness h) => _h = h;

        public Task ApplyAsync(DbConnection connection, MigrationDefinition definition, string script, string executedBy, CancellationToken ct = default)
        {
            // Mirrors TenantMigrationExecutor: the script and the version bump
            // share one transaction, so a failure leaves the version untouched.
            if (_h.HardFailureMessage is not null)
                throw new InvalidOperationException(_h.HardFailureMessage);

            if (_h.FailAtVersion == definition.Version)
                throw new InvalidOperationException("Invalid object name 'dbo.SalesReturn'.");

            _h.ExecutedScripts.Add(definition.MigrationCode);
            _h.RecordedVersions.Add(definition.Version);
            _h.CurrentVersion = definition.Version;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeLock : ITenantMigrationLock
    {
        private readonly MigrationHarness _h;
        public FakeLock(MigrationHarness h) => _h = h;

        public Task<IAsyncDisposable?> AcquireAsync(int tenantId, int timeoutMs, CancellationToken ct = default)
            => Task.FromResult<IAsyncDisposable?>(_h.LockAvailable ? new NoopHandle() : null);
    }

    private sealed class NoopHandle : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class FakeAuditService : IAuditService
    {
        private readonly MigrationHarness _h;
        public FakeAuditService(MigrationHarness h) => _h = h;

        public Task WriteAsync(string entityName, string? entityId, string action, string? performedBy,
            int? tenantId = null, string? oldValues = null, string? newValues = null)
        {
            _h.Audits.Add(new AuditEntry(entityName, entityId, action, performedBy, tenantId, newValues));
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public int UserId => 1;
        public string Username => "admin";
        public string? DisplayName => "Platform Administrator";
        public string? Role => ONEERP.Shared.Constants.RoleNames.PlatformAdmin;
        public bool IsPlatformAdmin => true;
        public IReadOnlyList<string> Permissions => new[] { ONEERP.Shared.Constants.MigrationPermissions.Wildcard };
        public bool HasPermission(string permission) => true;
    }
}

internal sealed record AuditEntry(
    string EntityName,
    string? EntityId,
    string Action,
    string? PerformedBy,
    int? TenantId,
    string? NewValues);
