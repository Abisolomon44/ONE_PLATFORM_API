using System.Reflection;
using System.Text;

namespace ONEERP.Platform.API.Data;

public interface IPurchaseDemoDataScriptProvider { string GetScript(); string ComputeChecksum(string script); }

public sealed class PurchaseDemoDataScriptProvider : IPurchaseDemoDataScriptProvider
{
    private const string ResourceName = "ONEERP.Platform.API.SqlScripts.erp_purchase_demo_seed.sql";
    private readonly Assembly _assembly = typeof(PurchaseDemoDataScriptProvider).Assembly;
    public string GetScript()
    {
        using var stream = _assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException("Embedded purchase demo seed was not found.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
    public string ComputeChecksum(string script) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(script ?? string.Empty))).ToLowerInvariant();
}
