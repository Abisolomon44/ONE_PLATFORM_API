namespace ONEERP.Platform.API.Models;

/// <summary>
/// One migration execution attempt (dbo.TenantMigrationHistory). A row is
/// written as RUNNING before the SQL is executed and is then updated to
/// SUCCESS or FAILED. ExecutionId groups every row produced by a single
/// "Run Migration" request so the UI can render a coherent execution log.
/// </summary>
public class TenantMigrationHistory
{
    public long TenantMigrationHistoryId { get; set; }
    public int TenantId { get; set; }
    public int? MigrationId { get; set; }
    public Guid ExecutionId { get; set; }
    public int Version { get; set; }
    public string MigrationCode { get; set; } = string.Empty;
    public string MigrationName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Checksum { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationMs { get; set; }
    public string? ExecutedBy { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedDate { get; set; }
}
