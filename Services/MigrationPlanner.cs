using ONEERP.Platform.API.Models;
using ONEERP.Shared.Constants;

namespace ONEERP.Platform.API.Services;

/// <summary>
/// Pure migration-set arithmetic, kept free of any I/O so the ordering and
/// versioning rules can be unit tested without a database.
/// </summary>
public static class MigrationPlanner
{
    /// <summary>
    /// Migrations that must still be applied, in strictly ascending version
    /// order. A migration is never skipped: 005 cannot run before 004.
    /// </summary>
    public static IReadOnlyList<MigrationDefinition> ResolvePending(
        int currentVersion,
        IEnumerable<MigrationDefinition> definitions) =>
        definitions
            .Where(d => !d.IsDeleted && d.IsActive)
            .Where(d => d.Version > currentVersion)
            .OrderBy(d => d.Version)
            .ToList();

    /// <summary>
    /// Highest version the tenant will reach once every pending migration has
    /// been applied. Equal to the current version when nothing is pending.
    /// </summary>
    public static int ResolveTargetVersion(int currentVersion, IReadOnlyList<MigrationDefinition> pending) =>
        pending.Count == 0 ? currentVersion : pending[^1].Version;

    /// <summary>
    /// Overall status shown to the administrator.
    /// RUNNING wins over everything, then FAILED when the last attempt broke,
    /// then UP_TO_DATE when nothing is pending.
    /// </summary>
    public static string ResolveStatus(
        int currentVersion,
        int targetVersion,
        bool hasRunningExecution,
        bool lastAttemptFailed)
    {
        if (hasRunningExecution)
            return MigrationStatus.Running;
        if (currentVersion >= targetVersion)
            return MigrationStatus.UpToDate;
        return lastAttemptFailed ? MigrationStatus.Failed : MigrationStatus.Pending;
    }

    /// <summary>
    /// True when a pinned checksum no longer matches the embedded script,
    /// which means a migration that may already have run was edited.
    /// </summary>
    public static bool IsChecksumDrifted(MigrationDefinition definition, string actualChecksum) =>
        !string.IsNullOrWhiteSpace(definition.Checksum)
        && !string.Equals(definition.Checksum, actualChecksum, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Rejects a definition set that cannot be executed safely: duplicate
    /// versions, versions inside the reserved baseline range, or gaps that
    /// would let a later migration run before an earlier one.
    /// </summary>
    public static IReadOnlyList<string> ValidateSequence(
        int currentVersion,
        IEnumerable<MigrationDefinition> definitions,
        int baselineVersion)
    {
        var problems = new List<string>();
        var ordered = definitions
            .Where(d => !d.IsDeleted && d.IsActive)
            .OrderBy(d => d.Version)
            .ToList();

        if (ordered.Count == 0)
        {
            problems.Add("No active migration definitions are registered.");
            return problems;
        }

        var duplicates = ordered.GroupBy(d => d.Version).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Count > 0)
            problems.Add($"Duplicate migration version(s): {string.Join(", ", duplicates)}.");

        foreach (var definition in ordered)
        {
            if (definition.Version <= baselineVersion)
                problems.Add($"Version {definition.Version} is reserved for the baseline schema (valid range starts at {baselineVersion + 1}).");
        }

        foreach (var definition in ordered)
        {
            if (definition.Version > baselineVersion + 1 && !ordered.Any(o => o.Version == definition.Version - 1))
                problems.Add($"Version {definition.Version} has no predecessor definition; migrations must form a contiguous sequence.");
        }

        if (currentVersion < baselineVersion)
            problems.Add($"Tenant reports version {currentVersion}, which is below the baseline version {baselineVersion}.");

        return problems;
    }
}
