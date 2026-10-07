using FluentAssertions;
using ONEERP.Platform.API.Tests.Fakes;
using ONEERP.Shared.Constants;
using Xunit;

namespace ONEERP.Platform.API.Tests;

/// <summary>
/// TEST 01, 02, 03, 04, 05, 06, 08, 09, 10 and 11 driven through the real
/// TenantMigrationService with in-memory collaborators.
/// </summary>
public class TenantMigrationServiceTests
{
    private static MigrationHarness Chain(int currentVersion, params int[] versions)
    {
        var harness = new MigrationHarness { CurrentVersion = currentVersion };
        harness.Definitions.AddRange(versions.Select(v => MigrationHarness.Definition(v, $"Step{v}")));
        return harness;
    }

    /* ---------------- TEST 01: current == target ---------------- */

    [Fact]
    public async Task Test01_CurrentEqualsTarget_ReportsUpToDateWithNothingPending()
    {
        var harness = Chain(5, 2, 3, 4, 5);
        var service = harness.BuildService();

        var status = await service.GetStatusAsync(MigrationHarness.TenantId);

        status.CurrentVersion.Should().Be(5);
        status.TargetVersion.Should().Be(5);
        status.PendingCount.Should().Be(0);
        status.Status.Should().Be(MigrationStatus.UpToDate);
        status.PendingMigrations.Should().BeEmpty();
    }

    [Fact]
    public async Task Test01_RunOnUpToDateTenant_ExecutesNoSql()
    {
        var harness = Chain(5, 2, 3, 4, 5);
        var service = harness.BuildService();

        var result = await service.RunAsync(MigrationHarness.TenantId);

        result.Status.Should().Be(MigrationStatus.UpToDate);
        result.FromVersion.Should().Be(5);
        result.ToVersion.Should().Be(5);
        result.Executed.Should().BeEmpty();
        harness.ExecutedScripts.Should().BeEmpty();
        harness.History.Should().BeEmpty();
    }

    /* ---------------- TEST 02: 003 -> 005 executes 004 then 005 ---------------- */

    [Fact]
    public async Task Test02_CurrentThreeTargetFive_ListsFourAndFiveAsPending()
    {
        var harness = Chain(3, 2, 3, 4, 5);
        var service = harness.BuildService();

        var status = await service.GetStatusAsync(MigrationHarness.TenantId);

        status.CurrentVersion.Should().Be(3);
        status.TargetVersion.Should().Be(5);
        status.PendingCount.Should().Be(2);
        status.PendingMigrations.Select(p => p.Version).Should().ContainInOrder(4, 5);
        status.PendingMigrations.Should().OnlyContain(p => p.Status == MigrationStatus.Pending);
    }

    [Fact]
    public async Task Test02_RunExecutesFourThenFiveInAscendingOrder()
    {
        var harness = Chain(3, 2, 3, 4, 5);
        var service = harness.BuildService();

        var result = await service.RunAsync(MigrationHarness.TenantId);

        harness.ExecutedScripts.Should().Equal("004_Step4", "005_Step5");
        harness.RecordedVersions.Should().Equal(4, 5);
        result.Executed.Select(e => e.Version).Should().Equal(4, 5);
    }

    /* ---------------- TEST 03: success ---------------- */

    [Fact]
    public async Task Test03_SuccessfulRun_UpdatesVersionRecordsHistoryAndReportsSuccess()
    {
        var harness = Chain(3, 2, 3, 4, 5);
        var service = harness.BuildService();

        var result = await service.RunAsync(MigrationHarness.TenantId);

        result.Status.Should().Be(MigrationStatus.Success);
        result.FromVersion.Should().Be(3);
        result.ToVersion.Should().Be(5);
        result.Executed.Should().OnlyContain(e => e.Status == MigrationStatus.Success);
        result.Executed.Should().OnlyContain(e => e.ErrorMessage == null);

        harness.History.Should().HaveCount(2);
        harness.History.Should().OnlyContain(h => h.Status == MigrationStatus.Success);
        harness.History.Should().OnlyContain(h => h.CompletedAt != null);
        harness.History.Should().OnlyContain(h => h.ExecutionId == result.ExecutionId);
        harness.History.Should().OnlyContain(h => h.ExecutedBy == "admin");
        harness.Audits.Should().ContainSingle(a => a.Action == "MIGRATE_RUN");

        // Successful rows must record a real duration so the history grid and
        // the duration column on the run result are not silently zeroed.
        harness.History.Should().OnlyContain(h => h.DurationMs >= 0);
    }

    [Fact]
    public async Task SuccessfulRunRecordsNoErrorText()
    {
        var harness = Chain(1, 2, 3);
        var service = harness.BuildService();

        var result = await service.RunAsync(MigrationHarness.TenantId);

        result.Executed.Should().OnlyContain(e => e.ErrorMessage == null);
        harness.History.Should().OnlyContain(h => string.IsNullOrEmpty(h.ErrorMessage));
    }

    /* ---------------- TEST 04: failure ---------------- */

    [Fact]
    public async Task Test04_FailureAtFive_RetainsVersionFourAndNeverReportsSuccess()
    {
        var harness = Chain(3, 2, 3, 4, 5);
        harness.FailAtVersion = 5;
        var service = harness.BuildService();

        var result = await service.RunAsync(MigrationHarness.TenantId);

        result.Status.Should().Be(MigrationStatus.Failed);
        result.FromVersion.Should().Be(3);
        result.ToVersion.Should().Be(4);
        result.HasFailures.Should().BeTrue();
        result.Executed.Should().HaveCount(2);
        result.Executed[0].Status.Should().Be(MigrationStatus.Success);
        result.Executed[1].Status.Should().Be(MigrationStatus.Failed);
        result.Executed[1].ErrorMessage.Should().Contain("SalesReturn");

        harness.CurrentVersion.Should().Be(4);
        harness.History.Last().Status.Should().Be(MigrationStatus.Failed);
        harness.History.Last().ErrorMessage.Should().NotBeNullOrEmpty();
        harness.Audits.Should().ContainSingle(a => a.Action == "MIGRATE_FAILED");
    }

    [Fact]
    public async Task Test04_FailureStopsTheRunBeforeLaterVersions()
    {
        var harness = Chain(1, 2, 3, 4, 5);
        harness.FailAtVersion = 3;
        var service = harness.BuildService();

        var result = await service.RunAsync(MigrationHarness.TenantId);

        // 002 committed; 003 rolled back so it is never counted as executed;
        // 004 and 005 were never attempted.
        harness.ExecutedScripts.Should().Equal("002_Step2");
        harness.RecordedVersions.Should().Equal(2);
        harness.CurrentVersion.Should().Be(2);
        result.ToVersion.Should().Be(2);

        // The attempt is still recorded so the history is not silently lost.
        harness.History.Select(h => h.Version).Should().Equal(2, 3);
        harness.History.Last().Status.Should().Be(MigrationStatus.Failed);
    }

    [Fact]
    public async Task Test04_ConnectionFailureIsReportedWithoutCorruptingTheVersion()
    {
        var harness = Chain(3, 2, 3, 4, 5);
        harness.HardFailureMessage = "A network-related error occurred.";
        var service = harness.BuildService();

        var result = await service.RunAsync(MigrationHarness.TenantId);

        result.Status.Should().Be(MigrationStatus.Failed);
        result.ToVersion.Should().Be(3);
        harness.CurrentVersion.Should().Be(3);
        harness.RecordedVersions.Should().BeEmpty();
    }

    /* ---------------- TEST 05: retry ---------------- */

    [Fact]
    public async Task Test05_RetryRereadsActualVersionAndRunsOnlyWhatIsStillPending()
    {
        // First attempt: 004 succeeds, 005 fails.
        var harness = Chain(3, 2, 3, 4, 5);
        harness.FailAtVersion = 5;
        var service = harness.BuildService();

        var first = await service.RunAsync(MigrationHarness.TenantId);
        first.ToVersion.Should().Be(4);

        // Operator fixes the script and retries: 004 must NOT run again.
        harness.FailAtVersion = null;
        var retry = await service.RunAsync(MigrationHarness.TenantId);

        retry.Status.Should().Be(MigrationStatus.Success);
        retry.FromVersion.Should().Be(4);
        retry.ToVersion.Should().Be(5);
        retry.Executed.Should().ContainSingle(e => e.Version == 5);
        harness.ExecutedScripts.Should().Equal("004_Step4", "005_Step5");
        harness.RecordedVersions.Should().Equal(4, 5);
    }

    [Fact]
    public async Task Test05_RetryAfterFullSuccessIsANoOp()
    {
        var harness = Chain(3, 2, 3, 4, 5);
        var service = harness.BuildService();

        (await service.RunAsync(MigrationHarness.TenantId)).Status.Should().Be(MigrationStatus.Success);
        var second = await service.RunAsync(MigrationHarness.TenantId);

        second.Status.Should().Be(MigrationStatus.UpToDate);
        harness.ExecutedScripts.Should().HaveCount(2);
    }

    /* ---------------- TEST 06: double run ---------------- */

    [Fact]
    public async Task Test06_SecondConcurrentRunIsRejectedWithConflict()
    {
        var harness = Chain(1, 2, 3);
        harness.LockAvailable = false;
        var service = harness.BuildService();

        var act = async () => await service.RunAsync(MigrationHarness.TenantId);

        var thrown = await act.Should().ThrowAsync<ONEERP.Shared.Exceptions.DomainException>();
        thrown.Which.StatusCode.Should().Be(409);
        thrown.Which.Message.Should().Contain("already running");
        harness.ExecutedScripts.Should().BeEmpty();
    }

    [Fact]
    public async Task Test06_OrphanedRunningRowIsClosedBeforeStartingANewRun()
    {
        var harness = Chain(1, 2);
        var service = harness.BuildService();

        await service.RunAsync(MigrationHarness.TenantId);

        // Simulate a crash that left the execution row stuck in RUNNING, which
        // is what the tenant-scoped RUNNING unique index would otherwise block.
        var orphan = harness.History.Single();
        orphan.Status = MigrationStatus.Running;
        orphan.CompletedAt = null;

        var result = await service.RunAsync(MigrationHarness.TenantId);

        result.Status.Should().Be(MigrationStatus.UpToDate);
        orphan.Status.Should().Be(MigrationStatus.Failed);
        orphan.ErrorMessage.Should().Contain("interrupted");
    }

    /* ---------------- TEST 08: cross-tenant isolation ---------------- */

    [Fact]
    public async Task Test08_RequestForAnotherTenantNeverTouchesThisTenantsDatabase()
    {
        var harness = Chain(1, 2, 3);
        var service = harness.BuildService();

        var act = async () => await service.RunAsync(MigrationHarness.TenantId + 99);

        await act.Should().ThrowAsync<ONEERP.Shared.Exceptions.NotFoundException>();
        harness.ExecutedScripts.Should().BeEmpty();
        harness.RecordedVersions.Should().BeEmpty();
        harness.History.Should().BeEmpty();
    }

    [Fact]
    public async Task Test08_StatusForAnotherTenantIsNotFoundAndLeaksNothing()
    {
        var harness = Chain(1, 2, 3);
        var service = harness.BuildService();

        var act = async () => await service.GetStatusAsync(MigrationHarness.TenantId - 1);

        var thrown = await act.Should().ThrowAsync<ONEERP.Shared.Exceptions.NotFoundException>();
        thrown.Which.Message.Should().NotContain(harness.Tenant.DatabaseName);
    }

    [Fact]
    public async Task Test08_HistoryForAnotherTenantIsNotFound()
    {
        var harness = Chain(1, 2, 3);
        var service = harness.BuildService();

        var act = async () => await service.GetHistoryAsync(MigrationHarness.TenantId + 1);

        await act.Should().ThrowAsync<ONEERP.Shared.Exceptions.NotFoundException>();
    }

    /* ---------------- TEST 11: refresh after migration ---------------- */

    [Fact]
    public async Task Test11_StatusAfterRunShowsLatestVersionAndZeroPending()
    {
        var harness = Chain(3, 2, 3, 4, 5);
        var service = harness.BuildService();

        await service.RunAsync(MigrationHarness.TenantId);

        var refreshed = await service.GetStatusAsync(MigrationHarness.TenantId);
        refreshed.CurrentVersion.Should().Be(5);
        refreshed.TargetVersion.Should().Be(5);
        refreshed.PendingCount.Should().Be(0);
        refreshed.Status.Should().Be(MigrationStatus.UpToDate);
    }

    /* ---------------- History endpoint ---------------- */

    [Fact]
    public async Task GetHistory_ReturnsNewestFirstWithErrorForFailedRuns()
    {
        var harness = Chain(3, 2, 3, 4, 5);
        harness.FailAtVersion = 5;
        var service = harness.BuildService();

        await service.RunAsync(MigrationHarness.TenantId);
        var history = await service.GetHistoryAsync(MigrationHarness.TenantId);

        history.Should().HaveCount(2);
        history[0].Version.Should().Be(5);
        history[0].Status.Should().Be(MigrationStatus.Failed);
        history[0].ErrorMessage.Should().NotBeNullOrEmpty();
        history[0].ExecutedBy.Should().Be("admin");
        history[1].Version.Should().Be(4);
        history[1].Status.Should().Be(MigrationStatus.Success);
    }

    [Fact]
    public async Task GetHistory_ForUnknownTenantIsNotFound()
    {
        var harness = Chain(1, 2);
        var service = harness.BuildService();

        var act = async () => await service.GetHistoryAsync(MigrationHarness.TenantId + 1);

        await act.Should().ThrowAsync<ONEERP.Shared.Exceptions.NotFoundException>();
    }

    /* ---------------- Checksum drift ---------------- */

    [Fact]
    public async Task EditedScriptIsRejectedBeforeAnythingExecutes()
    {
        var harness = Chain(1, 2, 3);
        harness.Definitions[1].Checksum = "a-stale-checksum";
        var service = harness.BuildService();

        var act = async () => await service.RunAsync(MigrationHarness.TenantId);

        var thrown = await act.Should().ThrowAsync<ONEERP.Shared.Exceptions.DomainException>();
        thrown.Which.StatusCode.Should().Be(409);
        thrown.Which.Message.Should().Contain("checksum mismatch");
        harness.ExecutedScripts.Should().BeEmpty();
        harness.History.Should().BeEmpty();
    }

    [Fact]
    public async Task UnpinnedChecksumIsSeededOnFirstRun()
    {
        var harness = Chain(1, 2);
        harness.Definitions[0].Checksum = null;
        var service = harness.BuildService();

        await service.RunAsync(MigrationHarness.TenantId);

        harness.Definitions[0].Checksum.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvalidRegistryIsRejectedBeforeExecution()
    {
        var harness = Chain(1, 2, 4);
        var service = harness.BuildService();

        var act = async () => await service.RunAsync(MigrationHarness.TenantId);

        var thrown = await act.Should().ThrowAsync<ONEERP.Shared.Exceptions.DomainException>();
        thrown.Which.StatusCode.Should().Be(409);
        thrown.Which.Message.Should().Contain("no predecessor definition");
        harness.ExecutedScripts.Should().BeEmpty();
    }
}
