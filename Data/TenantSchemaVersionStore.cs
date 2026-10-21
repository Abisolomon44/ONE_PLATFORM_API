using System.Data.Common;
using Dapper;

namespace ONEERP.Platform.API.Data;

/// <summary>
/// Reads and writes the tenant database schema version from
/// dbo.SchemaVersions inside the tenant database. The tenant database is the
/// single source of truth for its own schema version; the platform DB only
/// keeps execution history.
/// </summary>
public interface ITenantSchemaVersionStore
{
    /// <summary>
    /// Returns the tenant database's current schema version, creating
    /// dbo.SchemaVersions when it does not exist yet. A freshly provisioned
    /// database has an empty version table, which means "baseline applied".
    /// </summary>
    Task<int> GetCurrentVersionAsync(DbConnection tenantConnection, CancellationToken ct = default);

    /// <summary>Records a successfully applied version inside the tenant DB transaction.</summary>
    Task SetVersionAsync(DbConnection tenantConnection, DbTransaction transaction, int version, string? appliedBy, CancellationToken ct = default);
}

public class TenantSchemaVersionStore : ITenantSchemaVersionStore
{
    /// <summary>
    /// Version 1 is sql/erp_full.sql, executed by DatabaseProvisioningService
    /// when the tenant database is created. Incremental migrations start at 2.
    /// </summary>
    public const int BaselineVersion = 1;

    private const string EnsureVersionTableSql = @"
IF OBJECT_ID('dbo.SchemaVersions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchemaVersions (
        SchemaVersionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SchemaVersions PRIMARY KEY,
        Version          INT           NOT NULL CONSTRAINT UQ_SchemaVersions_Version UNIQUE,
        UpdatedAt        DATETIME2     NOT NULL CONSTRAINT DF_SchemaVersions_UpdatedAt DEFAULT SYSUTCDATETIME(),
        AppliedBy        NVARCHAR(100) NULL
    );
END";

    private const string ReadVersionSql = @"
SELECT ISNULL(MAX(Version), @baseline) FROM dbo.SchemaVersions;
";

    private const string UpsertVersionSql = @"
IF EXISTS (SELECT 1 FROM dbo.SchemaVersions WHERE Version = @version)
    UPDATE dbo.SchemaVersions SET UpdatedAt = SYSUTCDATETIME(), AppliedBy = @appliedBy WHERE Version = @version;
ELSE
    INSERT INTO dbo.SchemaVersions (Version, UpdatedAt, AppliedBy) VALUES (@version, SYSUTCDATETIME(), @appliedBy);";

    public async Task<int> GetCurrentVersionAsync(DbConnection tenantConnection, CancellationToken ct = default)
    {
        await tenantConnection.ExecuteAsync(new CommandDefinition(EnsureVersionTableSql, cancellationToken: ct));

        var version = await tenantConnection.ExecuteScalarAsync<int?>(new CommandDefinition(
            ReadVersionSql, new { baseline = BaselineVersion }, cancellationToken: ct));

        var resolved = version ?? 0;
        return resolved > 0 ? resolved : BaselineVersion;
    }

    public Task SetVersionAsync(DbConnection tenantConnection, DbTransaction transaction, int version, string? appliedBy, CancellationToken ct = default)
        => tenantConnection.ExecuteAsync(new CommandDefinition(
            UpsertVersionSql, new { version, appliedBy }, transaction, cancellationToken: ct));
}
