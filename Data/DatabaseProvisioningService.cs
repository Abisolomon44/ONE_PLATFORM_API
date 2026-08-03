using System.Reflection;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ONEERP.Platform.API.Data;

/// <summary>
/// Provisioning options supplied by the platform when seeding a new tenant
/// database (company details, plan currency and the unique tenant admin).
/// </summary>
public class TenantProvisioningOptions
{
    public string CompanyCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "USD";
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPasswordHash { get; set; } = string.Empty;
    public string AdminFullName { get; set; } = "ERP Administrator";
}

/// <summary>
/// Provisions a new tenant database: checks existence, creates the database,
/// executes the embedded ERP schema + seed scripts and returns the connection
/// string used to reach the new tenant database.
/// </summary>
public interface IDatabaseProvisioningService
{
    Task<bool> DatabaseExistsAsync(string databaseName);
    Task CreateDatabaseAsync(string databaseName);
    Task ExecuteScriptsAsync(string databaseName, TenantProvisioningOptions options);
    Task DropDatabaseAsync(string databaseName);
    string BuildConnectionString(string databaseName);
}

public class DatabaseProvisioningService : IDatabaseProvisioningService
{
    private const string SchemaResource = "ONEERP.Platform.API.SqlScripts.erp_schema.sql";
    private const string SeedResource = "ONEERP.Platform.API.SqlScripts.erp_seed.sql";

    private static readonly Regex DatabaseNameRegex = new(@"^[A-Za-z0-9_]{1,128}$", RegexOptions.Compiled);

    private readonly IDbConnectionFactory _factory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseProvisioningService> _logger;
    private readonly int _commandTimeoutSeconds;

    public DatabaseProvisioningService(IDbConnectionFactory factory, IConfiguration configuration, ILogger<DatabaseProvisioningService> logger)
    {
        _factory = factory;
        _configuration = configuration;
        _logger = logger;
        _commandTimeoutSeconds = configuration.GetValue("Provisioning:CommandTimeoutSeconds", 300);
    }

    public async Task<bool> DatabaseExistsAsync(string databaseName)
    {
        EnsureValidName(databaseName);
        using var connection = _factory.CreateMasterConnection();
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM sys.databases WHERE name = @name",
            new { name = databaseName }) > 0;
    }

    public async Task CreateDatabaseAsync(string databaseName)
    {
        EnsureValidName(databaseName);
        using var connection = _factory.CreateMasterConnection();
        await connection.ExecuteAsync($"CREATE DATABASE [{databaseName}]", commandTimeout: _commandTimeoutSeconds);
        _logger.LogInformation("Created tenant database {Database}", databaseName);
    }

    public async Task ExecuteScriptsAsync(string databaseName, TenantProvisioningOptions options)
    {
        EnsureValidName(databaseName);

        var schemaScript = ReadEmbeddedScript(SchemaResource);
        var seedScript = ReadEmbeddedScript(SeedResource);

        using var connection = _factory.CreateDatabaseConnection(databaseName);
        await connection.ExecuteAsync(schemaScript, commandTimeout: _commandTimeoutSeconds);
        await connection.ExecuteAsync(seedScript, new
        {
            options.CompanyCode,
            options.CompanyName,
            options.CompanyEmail,
            options.CurrencyCode,
            options.AdminUsername,
            options.AdminPasswordHash,
            options.AdminFullName
        }, commandTimeout: _commandTimeoutSeconds);

        _logger.LogInformation("Provisioned tenant schema and seed data for {Database}", databaseName);
    }

    public async Task DropDatabaseAsync(string databaseName)
    {
        EnsureValidName(databaseName);
        using var connection = _factory.CreateMasterConnection();
        await connection.ExecuteAsync(
            $"IF DB_ID(@name) IS NOT NULL BEGIN ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}]; END",
            new { name = databaseName },
            commandTimeout: _commandTimeoutSeconds);
        _logger.LogWarning("Dropped tenant database {Database} (cleanup after failed provisioning)", databaseName);
    }

    public string BuildConnectionString(string databaseName)
    {
        EnsureValidName(databaseName);
        var master = _factory.MasterConnectionString;
        var csb = new SqlConnectionStringBuilder(master) { InitialCatalog = databaseName };
        return csb.ConnectionString;
    }

    private static string ReadEmbeddedScript(string resourceName)
    {
        var assembly = typeof(DatabaseProvisioningService).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded SQL script '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static void EnsureValidName(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName) || !DatabaseNameRegex.IsMatch(databaseName))
            throw new InvalidOperationException($"Invalid database name '{databaseName}'. Only letters, digits and underscores are allowed.");
    }
}
