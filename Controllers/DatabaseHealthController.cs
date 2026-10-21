using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.Platform.API.DTOs;
using ONEERP.Platform.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.Platform.API.Controllers;

/// <summary>
/// Tenant database health and demo-data management (Database &amp; Demo Manager).
/// Read endpoints require migrations.view; demo seeding requires migrations.run.
/// No SQL or connection strings are accepted from the client.
/// </summary>
[Authorize]
[Route("api/db-health")]
public class DatabaseHealthController : BaseController
{
    private readonly ITenantDatabaseHealthService _healthService;
    private readonly IDemoDataSeedingService _demoSeedingService;
    private readonly IPurchaseDemoDataSeedingService _purchaseDemoSeedingService;
    private readonly IPurchaseScreenSeedService _purchaseScreenSeedService;
    private readonly IPurchaseReturnScreenSeedService _purchaseReturnScreenSeedService;
    private readonly ICurrentUser _currentUser;

    public DatabaseHealthController(
        ITenantDatabaseHealthService healthService,
        IDemoDataSeedingService demoSeedingService,
        IPurchaseDemoDataSeedingService purchaseDemoSeedingService,
        IPurchaseScreenSeedService purchaseScreenSeedService,
        IPurchaseReturnScreenSeedService purchaseReturnScreenSeedService,
        ICurrentUser currentUser)
    {
        _healthService = healthService;
        _demoSeedingService = demoSeedingService;
        _purchaseDemoSeedingService = purchaseDemoSeedingService;
        _purchaseScreenSeedService = purchaseScreenSeedService;
        _purchaseReturnScreenSeedService = purchaseReturnScreenSeedService;
        _currentUser = currentUser;
    }

    /// <summary>Health row for every tenant (schema, migrations, demo, overall).</summary>
    [HttpGet("tenants")]
    [MigrationAuthorization(MigrationPermissions.View)]
    [ProducesResponseType(typeof(ApiResponse<List<TenantDbHealthDto>>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(ApiResponse<List<TenantDbHealthDto>>.Ok(await _healthService.GetAllHealthAsync(ct)));

    /// <summary>Dashboard aggregates (total/healthy/pending/failed/drifted/missing demo).</summary>
    [HttpGet("summary")]
    [MigrationAuthorization(MigrationPermissions.View)]
    [ProducesResponseType(typeof(ApiResponse<TenantDbHealthSummaryDto>), 200)]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
        => Ok(ApiResponse<TenantDbHealthSummaryDto>.Ok(await _healthService.GetSummaryAsync(ct)));

    /// <summary>Health for one tenant, including the missing-table list.</summary>
    [HttpGet("tenants/{tenantId:int}")]
    [MigrationAuthorization(MigrationPermissions.View)]
    [ProducesResponseType(typeof(ApiResponse<TenantDbHealthDto>), 200)]
    public async Task<IActionResult> GetOne(int tenantId, CancellationToken ct)
        => Ok(ApiResponse<TenantDbHealthDto>.Ok(await _healthService.GetHealthAsync(tenantId, ct)));

    /// <summary>Seeds the idempotent demo dataset into the tenant database.</summary>
    [HttpPost("tenants/{tenantId:int}/seed-demo")]
    [MigrationAuthorization(MigrationPermissions.Run)]
    [ProducesResponseType(typeof(ApiResponse<DemoSeedResultDto>), 200)]
    public async Task<IActionResult> SeedDemo(int tenantId, CancellationToken ct)
        => Ok(ApiResponse<DemoSeedResultDto>.Ok(
            await _demoSeedingService.SeedAsync(tenantId, _currentUser.Username ?? "system", ct),
            "Demo data seed executed"));

    [HttpPost("tenants/{tenantId:int}/seed-purchase-demo")]
    [MigrationAuthorization(MigrationPermissions.Run)]
    public async Task<IActionResult> SeedPurchaseDemo(int tenantId, CancellationToken ct)
        => Ok(ApiResponse<DemoSeedResultDto>.Ok(
            await _purchaseDemoSeedingService.SeedAsync(tenantId, _currentUser.Username ?? "system", ct),
            "Purchase demo data seed executed"));

    [HttpPost("tenants/{tenantId:int}/seed-purchase-screens")]
    [MigrationAuthorization(MigrationPermissions.Run)]
    public async Task<IActionResult> SeedPurchaseScreens(int tenantId, CancellationToken ct)
        => Ok(ApiResponse<DemoSeedResultDto>.Ok(
            await _purchaseScreenSeedService.SeedAsync(tenantId, _currentUser.Username ?? "system", ct),
            "Purchase screen registry seed executed"));

    [HttpPost("tenants/{tenantId:int}/seed-purchase-return-screens")]
    [MigrationAuthorization(MigrationPermissions.Run)]
    public async Task<IActionResult> SeedPurchaseReturnScreens(int tenantId, CancellationToken ct)
        => Ok(ApiResponse<DemoSeedResultDto>.Ok(
            await _purchaseReturnScreenSeedService.SeedAsync(tenantId, _currentUser.Username ?? "system", ct),
            "Purchase Return screen registry seed executed"));
}
