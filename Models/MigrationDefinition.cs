namespace ONEERP.Platform.API.Models;

/// <summary>
/// A tenant database migration definition row (dbo.Migrations). Definitions are
/// registered by the platform DBA and mapped 1:1 to an embedded
/// sql/erp_migration_*.sql script. Version 1 is reserved for sql/erp_full.sql
/// (the baseline applied when the tenant database is provisioned), so the
/// first incremental migration starts at Version 2.
/// </summary>
public class MigrationDefinition
{
    public int MigrationId { get; set; }
    public int Version { get; set; }
    public string MigrationCode { get; set; } = string.Empty;
    public string MigrationName { get; set; } = string.Empty;
    public string ScriptName { get; set; } = string.Empty;
    public string? Checksum { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}
