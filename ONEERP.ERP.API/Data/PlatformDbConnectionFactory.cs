using System.Data;
using Microsoft.Data.SqlClient;

namespace ONEERP.ERP.API.Data;

/// <summary>
/// Creates connections to the ONE ERP platform database (used to resolve
/// tenant connection strings and read plan/subscription data).
/// </summary>
public interface IPlatformDbConnectionFactory
{
    string MasterConnectionString { get; }
    IDbConnection CreatePlatformConnection();
    IDbConnection CreateDatabaseConnection(string databaseName);
}

public class PlatformDbConnectionFactory : IPlatformDbConnectionFactory
{
    private readonly string _masterConnectionString;
    private readonly string _platformDatabaseName;

    public PlatformDbConnectionFactory(IConfiguration configuration)
    {
        _masterConnectionString = configuration.GetConnectionString("Master")
            ?? throw new InvalidOperationException("ConnectionStrings:Master is not configured.");
        _platformDatabaseName = configuration["PlatformDatabase:Name"] ?? "ONEERP_PLATFORM";
    }

    public string MasterConnectionString => _masterConnectionString;

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
