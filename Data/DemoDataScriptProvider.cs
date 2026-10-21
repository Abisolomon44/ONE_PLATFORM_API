using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace ONEERP.Platform.API.Data;

/// <summary>
/// Resolves the embedded demo-data seed script (sql/erp_demo_data.sql) and
/// computes its checksum, reusing the same conventions as the tenant
/// migration script provider so the demo seed can be version-checked and
/// recorded exactly like a migration.
/// </summary>
public interface IDemoDataScriptProvider
{
    string GetScript();
    string ComputeChecksum(string script);
}

public class DemoDataScriptProvider : IDemoDataScriptProvider
{
    private const string ResourceName = "ONEERP.Platform.API.SqlScripts.erp_demo_data.sql";

    private readonly Assembly _assembly = typeof(DemoDataScriptProvider).Assembly;

    public string GetScript()
    {
        using var stream = _assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded demo-data script '{ResourceName}' was not found. " +
                "Ensure the csproj embeds sql/erp_demo_data.sql with the matching logical name.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public string ComputeChecksum(string script)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(script ?? string.Empty));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
