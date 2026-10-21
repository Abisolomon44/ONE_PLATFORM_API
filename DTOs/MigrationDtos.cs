namespace ONEERP.Platform.API.DTOs;

/// <summary>
/// Response for GET /api/tenants/{tenantId}/migration-status.
/// CurrentVersion is read from the tenant database (the authoritative source),
/// never from the platform DB or the client.
/// </summary>
public class MigrationStatusDto
{
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public int CurrentVersion { get; set; }
    public int TargetVersion { get; set; }
    public int PendingCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? LastError { get; set; }
    public DateTime? LastRunAt { get; set; }
    public List<PendingMigrationDto> PendingMigrations { get; set; } = new();
    public List<PendingMigrationDto> Migrations { get; set; } = new();
}

public class PendingMigrationDto
{
    public int MigrationId { get; set; }
    public int Version { get; set; }
    public string MigrationCode { get; set; } = string.Empty;
    public string MigrationName { get; set; } = string.Empty;
    public string ScriptName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Response for POST /api/tenants/{tenantId}/migration/run. No SQL is ever
/// accepted from the client; the tenant id is the only input.
/// </summary>
public class MigrationRunResultDto
{
    public Guid ExecutionId { get; set; }
    public int TenantId { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public int FromVersion { get; set; }
    public int ToVersion { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool HasFailures => Status == ONEERP.Shared.Constants.MigrationStatus.Failed;
    public List<MigrationExecutionDto> Executed { get; set; } = new();
}

public class MigrationExecutionDto
{
    public long? HistoryId { get; set; }
    public int Version { get; set; }
    public string MigrationCode { get; set; } = string.Empty;
    public string MigrationName { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationMs { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

/// <summary>Response for GET /api/tenants/{tenantId}/migration/history.</summary>
public class MigrationHistoryDto
{
    public long HistoryId { get; set; }
    public Guid ExecutionId { get; set; }
    public int TenantId { get; set; }
    public int Version { get; set; }
    public string MigrationCode { get; set; } = string.Empty;
    public string MigrationName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationMs { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ExecutedBy { get; set; }
    public string? ErrorMessage { get; set; }
}
