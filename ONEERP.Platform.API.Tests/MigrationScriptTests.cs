using FluentAssertions;
using ONEERP.Platform.API.Data;
using ONEERP.Platform.API.Models;
using ONEERP.Platform.API.Services;
using Xunit;

namespace ONEERP.Platform.API.Tests;

/// <summary>
/// The four sql/erp_migration_*.sql scripts that already exist in the repo must
/// be loadable, checksummed and safe to run inside a single transaction.
/// </summary>
public class MigrationScriptTests
{
    private readonly MigrationScriptProvider _provider = new();

    [Fact]
    public void AllShippedMigrationScriptsAreEmbedded()
    {
        var available = _provider.ListAvailableScripts();

        available.Should().BeEquivalentTo(new[]
        {
            "erp_migration_007b_price_type_csv.sql",
            "erp_migration_008_sales_lifecycle.sql",
            "erp_migration_009_pos_operations.sql",
            "erp_migration_010_inventory.sql"
        });
    }

    [Fact]
    public void BaselineScriptIsNotOfferedAsAMigration()
    {
        _provider.ListAvailableScripts().Should().NotContain("erp_full.sql");
    }

    [Theory]
    [InlineData("erp_migration_007b_price_type_csv.sql")]
    [InlineData("erp_migration_008_sales_lifecycle.sql")]
    [InlineData("erp_migration_009_pos_operations.sql")]
    [InlineData("erp_migration_010_inventory.sql")]
    public void ShippedScriptsLoadAndAreTransactionSafe(string scriptName)
    {
        var script = _provider.GetScript(scriptName);

        script.Should().NotBeNullOrWhiteSpace();
        _provider.ComputeChecksum(script).Should().HaveLength(64);

        MigrationScriptGuard.Inspect(script).Should().BeEmpty(
            $"'{scriptName}' must be safe to execute inside the per-migration transaction");
    }

    [Fact]
    public void ChecksumIsStableAndContentSensitive()
    {
        var a = _provider.ComputeChecksum("SELECT 1");
        var b = _provider.ComputeChecksum("SELECT 1");
        var c = _provider.ComputeChecksum("SELECT 2");

        a.Should().Be(b);
        a.Should().NotBe(c);
        a.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void UnknownScriptFailsWithAnActionableMessage()
    {
        var act = () => _provider.GetScript("erp_migration_999_nope.sql");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*was not found*Available:*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyScriptNameIsRejected(string scriptName)
    {
        var act = () => _provider.GetScript(scriptName);

        act.Should().Throw<InvalidOperationException>().WithMessage("*no ScriptName*");
    }

    [Theory]
    [InlineData("../appsettings.json")]
    [InlineData("..\\erp_full.sql")]
    [InlineData("sub/dir/erp_migration_008_sales_lifecycle.sql")]
    public void PathTraversalInScriptNameIsRejected(string scriptName)
    {
        var act = () => _provider.GetScript(scriptName);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Invalid migration script name*");
    }

    [Fact]
    public void GuardRejectsEmbeddedCommit()
    {
        MigrationScriptGuard.Inspect("COMMIT TRANSACTION;")
            .Should().Contain(p => p.Contains("COMMIT"));
    }

    [Fact]
    public void GuardRejectsGoBatchSeparator()
    {
        MigrationScriptGuard.Inspect("SELECT 1\nGO\nSELECT 2")
            .Should().Contain(p => p.Contains("GO batch separator"));
    }

    [Fact]
    public void GuardRejectsEmptyScript()
    {
        MigrationScriptGuard.Inspect("   ").Should().ContainSingle();
    }

    [Fact]
    public void GuardAllowsDdlDmlAndComments()
    {
        const string script = """
            -- add the price type csv column
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PriceLists') AND name = 'PriceTypeIds')
            BEGIN
                ALTER TABLE dbo.PriceLists ADD PriceTypeIds VARCHAR(500) NULL;
            END;

            /* backfill */
            UPDATE dbo.PriceLists SET PriceTypeIds = CAST(PriceTypeId AS VARCHAR(10)) WHERE PriceTypeIds IS NULL;
            """;

        MigrationScriptGuard.Inspect(script).Should().BeEmpty();
    }

    [Fact]
    public void GuardRejectsUnsafeScriptThroughEnsureSafe()
    {
        var definition = new MigrationDefinition
        {
            MigrationId = 9,
            Version = 9,
            MigrationCode = "009_Bad",
            MigrationName = "Bad",
            ScriptName = "erp_migration_009_Bad.sql"
        };

        var act = () => MigrationScriptGuard.EnsureSafe(definition, "COMMIT;");

        act.Should().Throw<InvalidOperationException>().WithMessage("*009_Bad*");
    }
}
