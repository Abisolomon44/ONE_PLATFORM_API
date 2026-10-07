using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ONEERP.Platform.API.Data;
using ONEERP.Platform.API.Models;
using ONEERP.Platform.API.Repositories;
using ONEERP.Platform.API.Services;
using ONEERP.Shared.Exceptions;
using Xunit;

namespace ONEERP.Platform.API.Tests;

/// <summary>
/// TEST 09 (tenant database availability) and the tenant-isolation guard,
/// exercised against the real <see cref="TenantDatabaseResolver"/>. Only the
/// tenant-lookup path is covered here because it is the part that decides
/// whether a request may proceed; opening SQL connections requires a live
/// server and is verified in the deployment smoke test.
/// </summary>
public class TenantDatabaseResolverTests
{
    private static TenantDatabaseResolver CreateResolver(
        TenantRow? tenant,
        TenantConnection? activeConnection = null)
    {
        var tenants = new Mock<ITenantRepository>();
        tenants.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
               .ReturnsAsync(tenant);

        var connections = new Mock<ITenantConnectionRepository>();
        connections.Setup(r => r.GetActiveByTenantIdAsync(It.IsAny<int>()))
                  .ReturnsAsync(activeConnection);

        var factory = new Mock<IDbConnectionFactory>();
        factory.SetupGet(f => f.MasterConnectionString)
               .Returns("Server=sql;Database=master;Integrated Security=True;TrustServerCertificate=True");

        return new TenantDatabaseResolver(
            tenants.Object,
            connections.Object,
            factory.Object,
            NullLogger<TenantDatabaseResolver>.Instance);
    }

    private static TenantRow Tenant(string databaseName = "ERP_ABC", bool deleted = false) => new()
    {
        TenantId = 12,
        TenantCode = "ABC",
        TenantName = "ABC Company",
        DatabaseName = databaseName,
        Status = "Active",
        IsDeleted = deleted
    };

    /* ---------------- TEST 09: availability preconditions ---------------- */

    [Fact]
    public async Task Test09_TenantWithoutDatabaseIsRejectedBeforeAnyConnectionAttempt()
    {
        var resolver = CreateResolver(Tenant(string.Empty));

        var act = async () => await resolver.ResolveTenantAsync(12);

        var thrown = await act.Should().ThrowAsync<DomainException>();
        thrown.Which.StatusCode.Should().Be(409);
        thrown.Which.Message.Should().Contain("no database configured");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Test09_WhitespaceDatabaseNameIsTreatedAsNotConfigured(string? databaseName)
    {
        var resolver = CreateResolver(Tenant(databaseName!));

        var act = async () => await resolver.ResolveTenantAsync(12);

        (await act.Should().ThrowAsync<DomainException>())
            .Which.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task UnknownTenantIsNotFound()
    {
        var resolver = CreateResolver(null);

        var act = async () => await resolver.ResolveTenantAsync(12);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SoftDeletedTenantIsNotFound()
    {
        var resolver = CreateResolver(Tenant(deleted: true));

        var act = async () => await resolver.ResolveTenantAsync(12);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task NonPositiveTenantIdIsABadRequest(int tenantId)
    {
        var resolver = CreateResolver(Tenant());

        var act = async () => await resolver.ResolveTenantAsync(tenantId);

        var thrown = await act.Should().ThrowAsync<DomainException>();
        thrown.Which.StatusCode.Should().Be(400);
    }

    /* ---------------- happy path for the lookup ---------------- */

    [Fact]
    public async Task ConfiguredTenantResolvesToItsOwnDatabase()
    {
        var resolver = CreateResolver(Tenant());

        var tenant = await resolver.ResolveTenantAsync(12);

        tenant.TenantId.Should().Be(12);
        tenant.DatabaseName.Should().Be("ERP_ABC");
    }

    /* ---------------- isolation guard on the stored connection ---------------- */

    [Fact]
    public void StoredConnectionPointingAtAnotherDatabaseIsRejected()
    {
        // A stored connection whose Initial Catalog disagrees with the platform
        // tenant record must be discarded, otherwise a request for tenant A
        // could run against tenant B's data.
        TenantDatabaseResolver.TryUseStoredConnection(
            "Server=sql;Database=ERP_OTHER;Integrated Security=True;",
            "ERP_ABC",
            out var connectionString)
            .Should().BeFalse();

        connectionString.Should().BeEmpty();
    }

    [Fact]
    public void StoredConnectionForTheSameDatabaseIsAdoptedAndHardened()
    {
        TenantDatabaseResolver.TryUseStoredConnection(
            "Server=sql;Database=ERP_ABC;Integrated Security=True;",
            "ERP_ABC",
            out var connectionString)
            .Should().BeTrue();

        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
        builder.InitialCatalog.Should().Be("ERP_ABC");
        builder.MultipleActiveResultSets.Should().BeTrue();
        builder.ConnectTimeout.Should().Be(15);
    }

    [Theory]
    [InlineData("erp_abc")]
    [InlineData("ERP_ABC")]
    public void CatalogComparisonIsCaseInsensitive(string catalog)
    {
        TenantDatabaseResolver.TryUseStoredConnection(
            $"Server=sql;Database={catalog};Integrated Security=True;",
            "ERP_ABC",
            out _)
            .Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void MissingStoredConnectionFallsBackToTheTenantRecord(string? stored)
    {
        TenantDatabaseResolver.TryUseStoredConnection(stored, "ERP_ABC", out var connectionString)
            .Should().BeFalse();

        connectionString.Should().BeEmpty();
    }

    [Fact]
    public void MalformedStoredConnectionIsRejectedRatherThanThrowing()
    {
        TenantDatabaseResolver.TryUseStoredConnection(
            "this is not a connection string", "ERP_ABC", out _)
            .Should().BeFalse();
    }
}