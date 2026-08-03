using System.Data;
using Microsoft.Data.SqlClient;

namespace ONEERP.Platform.API.Data;

/// <summary>
/// Creates ADO.NET connections to the SQL Server instance (master, platform
/// database or an arbitrary tenant database) using Windows Authentication.
/// </summary>
public interface IDbConnectionFactory
{
    string MasterConnectionString { get; }
    IDbConnection CreateMasterConnection();
    IDbConnection CreatePlatformConnection();
    IDbConnection CreateDatabaseConnection(string databaseName);
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _masterConnectionString;
    private readonly string _platformDatabaseName;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _masterConnectionString = configuration.GetConnectionString("Master")
            ?? throw new InvalidOperationException("ConnectionStrings:Master is not configured.");
        _platformDatabaseName = configuration["PlatformDatabase:Name"] ?? "ONEERP_PLATFORM";
    }

    public string MasterConnectionString => _masterConnectionString;

    public IDbConnection CreateMasterConnection() => new SqlConnection(_masterConnectionString);

    public IDbConnection CreatePlatformConnection() => CreateDatabaseConnection(_platformDatabaseName);

    public IDbConnection CreateDatabaseConnection(string databaseName)
    {
        var csb = new SqlConnectionStringBuilder(_masterConnectionString)
        {
            InitialCatalog = databaseName,
            ConnectTimeout = 15
        };
        return new SqlConnection(csb.ConnectionString);
    }
}
