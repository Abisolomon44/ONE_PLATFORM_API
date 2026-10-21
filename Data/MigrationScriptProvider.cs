using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace ONEERP.Platform.API.Data;

/// <summary>
/// Resolves the SQL text for a registered migration definition from the
/// embedded SQL resources and computes its SHA-256
/// checksum. The client never supplies SQL; the platform decides which script
/// runs for a given version.
/// </summary>
public interface IMigrationScriptProvider
{
    /// <summary>Returns the script text for the given file name.</summary>
    string GetScript(string scriptName);

    /// <summary>Returns the lower-case hex SHA-256 of the given script text.</summary>
    string ComputeChecksum(string script);

    /// <summary>File names of every embedded SQL file, ordered by name.</summary>
    IReadOnlyList<string> ListAvailableScripts();
}

public class MigrationScriptProvider : IMigrationScriptProvider
{
    private const string ResourcePrefix = "ONEERP.Platform.API.SqlScripts.";

    private readonly Assembly _assembly = typeof(MigrationScriptProvider).Assembly;
    private readonly Dictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _gate = new();

    public string GetScript(string scriptName)
    {
        if (string.IsNullOrWhiteSpace(scriptName))
            throw new InvalidOperationException("Migration definition has no ScriptName.");

        var fileName = Path.GetFileName(scriptName.Trim());
        if (!string.Equals(fileName, scriptName.Trim(), StringComparison.Ordinal))
            throw new InvalidOperationException($"Invalid migration script name '{scriptName}'.");

        lock (_gate)
        {
            if (_cache.TryGetValue(fileName, out var cached))
                return cached;

            using var stream = _assembly.GetManifestResourceStream(ResourcePrefix + fileName)
                ?? throw new InvalidOperationException(
                    $"Embedded SQL script '{fileName}' was not found. Available: {string.Join(", ", ListAvailableScripts())}.");

            using var reader = new StreamReader(stream, Encoding.UTF8);
            var script = reader.ReadToEnd();
            _cache[fileName] = script;
            return script;
        }
    }

    public string ComputeChecksum(string script)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(script ?? string.Empty));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public IReadOnlyList<string> ListAvailableScripts() =>
        _assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(ResourcePrefix, StringComparison.Ordinal)
                        && n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .Select(n => n[ResourcePrefix.Length..])
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
}
