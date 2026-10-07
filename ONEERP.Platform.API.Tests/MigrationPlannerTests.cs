using FluentAssertions;
using ONEERP.Platform.API.Models;
using ONEERP.Platform.API.Services;
using ONEERP.Shared.Constants;
using Xunit;

namespace ONEERP.Platform.API.Tests;

/// <summary>
/// TEST 01, 02, 10 and the section 8 ordering rule: migrations run in strictly
/// ascending version order and a later version never runs before an earlier one.
/// </summary>
public class MigrationPlannerTests
{
    private const int Baseline = 1;

    private static List<MigrationDefinition> Chain(params int[] versions) =>
        versions.Select(v => new MigrationDefinition
        {
            MigrationId = v,
            Version = v,
            MigrationCode = $"{v:000}_Step{v}",
            MigrationName = $"Step{v}",
            ScriptName = $"erp_migration_{v:000}_Step{v}.sql",
            IsActive = true
        }).ToList();

    [Fact]
    public void ResolvePending_WhenCurrentEqualsTarget_ReturnsNothing()
    {
        var pending = MigrationPlanner.ResolvePending(5, Chain(2, 3, 4, 5));

        pending.Should().BeEmpty();
        MigrationPlanner.ResolveTargetVersion(5, pending).Should().Be(5);
        MigrationPlanner.ResolveStatus(5, 5, hasRunningExecution: false, lastAttemptFailed: false)
            .Should().Be(MigrationStatus.UpToDate);
    }

    [Fact]
    public void ResolvePending_WhenCurrentIsThree_ReturnsFourThenFive()
    {
        var pending = MigrationPlanner.ResolvePending(3, Chain(2, 3, 4, 5));

        pending.Select(p => p.Version).Should().ContainInOrder(4, 5);
        MigrationPlanner.ResolveTargetVersion(3, pending).Should().Be(5);
        MigrationPlanner.ResolveStatus(3, 5, hasRunningExecution: false, lastAttemptFailed: false)
            .Should().Be(MigrationStatus.Pending);
    }

    [Fact]
    public void ResolvePending_NeverSkipsAnIntermediateVersion()
    {
        var pending = MigrationPlanner.ResolvePending(1, Chain(2, 3, 4, 5));

        pending.Select(p => p.Version).Should().Equal(2, 3, 4, 5);
        pending.Should().BeInAscendingOrder(p => p.Version);
    }

    [Fact]
    public void ResolvePending_SortsUnorderedInputAscending()
    {
        var pending = MigrationPlanner.ResolvePending(1, Chain(5, 2, 4, 3));

        pending.Select(p => p.Version).Should().Equal(2, 3, 4, 5);
    }

    [Fact]
    public void ResolvePending_IgnoresInactiveAndDeletedDefinitions()
    {
        var definitions = Chain(2, 3, 4);
        definitions[1].IsActive = false;
        definitions[2].IsDeleted = true;

        MigrationPlanner.ResolvePending(1, definitions).Select(p => p.Version).Should().Equal(2);
    }

    [Fact]
    public void ResolveStatus_RunningWinsOverEverything()
    {
        MigrationPlanner.ResolveStatus(1, 5, hasRunningExecution: true, lastAttemptFailed: true)
            .Should().Be(MigrationStatus.Running);
    }

    [Fact]
    public void ResolveStatus_ReportsFailedWhenLastAttemptBrokeAndWorkRemains()
    {
        MigrationPlanner.ResolveStatus(3, 5, hasRunningExecution: false, lastAttemptFailed: true)
            .Should().Be(MigrationStatus.Failed);
    }

    [Fact]
    public void ResolveStatus_UpToDateWinsWhenNothingPendingEvenAfterFailure()
    {
        MigrationPlanner.ResolveStatus(5, 5, hasRunningExecution: false, lastAttemptFailed: true)
            .Should().Be(MigrationStatus.UpToDate);
    }

    [Fact]
    public void IsChecksumDrifted_DetectsEditedScriptAndAllowsUnpinned()
    {
        var definition = Chain(2)[0];
        definition.Checksum = "abc123";

        MigrationPlanner.IsChecksumDrifted(definition, "abc123").Should().BeFalse();
        MigrationPlanner.IsChecksumDrifted(definition, "ABC123").Should().BeFalse();
        MigrationPlanner.IsChecksumDrifted(definition, "different").Should().BeTrue();

        definition.Checksum = null;
        MigrationPlanner.IsChecksumDrifted(definition, "anything").Should().BeFalse();
    }

    [Fact]
    public void ValidateSequence_RejectsDuplicateVersions()
    {
        var definitions = Chain(2, 2, 3);

        MigrationPlanner.ValidateSequence(1, definitions, Baseline)
            .Should().Contain(p => p.Contains("Duplicate migration version"));
    }

    [Fact]
    public void ValidateSequence_RejectsVersionsInsideTheReservedBaselineRange()
    {
        var definitions = Chain(1, 2);

        MigrationPlanner.ValidateSequence(1, definitions, Baseline)
            .Should().Contain(p => p.Contains("reserved for the baseline"));
    }

    [Fact]
    public void ValidateSequence_RejectsGaps()
    {
        var definitions = Chain(2, 4);

        MigrationPlanner.ValidateSequence(1, definitions, Baseline)
            .Should().Contain(p => p.Contains("no predecessor definition"));
    }

    [Fact]
    public void ValidateSequence_RejectsEmptyRegistry()
    {
        MigrationPlanner.ValidateSequence(1, new List<MigrationDefinition>(), Baseline)
            .Should().ContainSingle(p => p.Contains("No active migration definitions"));
    }

    [Fact]
    public void ValidateSequence_RejectsVersionBelowBaseline()
    {
        MigrationPlanner.ValidateSequence(0, Chain(2), Baseline)
            .Should().Contain(p => p.Contains("below the baseline version"));
    }

    [Fact]
    public void ValidateSequence_AcceptsAContiguousRegistry()
    {
        MigrationPlanner.ValidateSequence(1, Chain(2, 3, 4, 5), Baseline).Should().BeEmpty();
    }
}
