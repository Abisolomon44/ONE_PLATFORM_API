using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.Platform.API.DTOs;
using ONEERP.Platform.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.Platform.API.Controllers;

/// <summary>
/// Tenant database migration management. Read-only endpoints require a
/// platform administrator; running migrations is additionally audited and is
/// the only endpoint that mutates a tenant database.
/// <para>
/// Authorization is enforced here and in <see cref="MigrationAuthorization"/>,
/// never by the Angular UI. Every path resolves the tenant from the platform
/// database, so a caller cannot address another tenant's database by crafting
/// a request.
/// </para>
/// </summary>
[Authorize]
[MigrationAuthorization(MigrationPermissions.View)]
[Route("api/tenants")]
public class TenantMigrationsController : BaseController
{
    private readonly ITenantMigrationService _migrationService;
    private readonly ITenantSqlFileExecutionService _sqlFileExecutionService;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<int> _tenantIdValidator;

    public TenantMigrationsController(
        ITenantMigrationService migrationService,
        ITenantSqlFileExecutionService sqlFileExecutionService,
        ICurrentUser currentUser,
        IValidator<int> tenantIdValidator)
    {
        _migrationService = migrationService;
        _sqlFileExecutionService = sqlFileExecutionService;
        _currentUser = currentUser;
        _tenantIdValidator = tenantIdValidator;
    }

    /// <summary>Current vs target version and the pending migration list.</summary>
    [HttpGet("{tenantId:int}/migration-status")]
    [ProducesResponseType(typeof(ApiResponse<MigrationStatusDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    [ProducesResponseType(typeof(ApiResponse<object>), 503)]
    public async Task<IActionResult> GetStatus(int tenantId)
    {
        var errors = await ValidateAsync(_tenantIdValidator, tenantId);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<object>.Fail("Validation failed", errors));

        var status = await _migrationService.GetStatusAsync(tenantId);
        return Ok(ApiResponse<MigrationStatusDto>.Ok(status));
    }

    /// <summary>
    /// Executes every pending migration for the tenant in ascending version
    /// order. Takes no body: the tenant id in the route is the only input.
    /// </summary>
    [HttpPost("{tenantId:int}/migration/run")]
    [MigrationAuthorization(MigrationPermissions.Run)]
    [ProducesResponseType(typeof(ApiResponse<MigrationRunResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    [ProducesResponseType(typeof(ApiResponse<object>), 503)]
    public async Task<IActionResult> Run(int tenantId)
    {
        var errors = await ValidateAsync(_tenantIdValidator, tenantId);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<object>.Fail("Validation failed", errors));

        var result = await _migrationService.RunAsync(tenantId);
        return Ok(ApiResponse<MigrationRunResultDto>.Ok(result, result.Message));
    }

    /// <summary>Executes one specific pending migration, only when it is the next version in sequence.</summary>
    [HttpPost("{tenantId:int}/migration/{migrationId:int}/run")]
    [MigrationAuthorization(MigrationPermissions.Run)]
    [ProducesResponseType(typeof(ApiResponse<MigrationRunResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    [ProducesResponseType(typeof(ApiResponse<object>), 503)]
    public async Task<IActionResult> RunSingle(int tenantId, int migrationId)
    {
        var errors = await ValidateAsync(_tenantIdValidator, tenantId);
        if (migrationId <= 0)
            errors.Add("Migration id must be greater than zero.");
        if (errors.Count > 0)
            return BadRequest(ApiResponse<object>.Fail("Validation failed", errors));

        var result = await _migrationService.RunSingleAsync(tenantId, migrationId);
        return Ok(ApiResponse<MigrationRunResultDto>.Ok(result, result.Message));
    }

    /// <summary>Re-runs an already applied, checksum-pinned tenant migration without changing schema version.</summary>
    [HttpPost("{tenantId:int}/migration/{migrationId:int}/rerun")]
    [MigrationAuthorization(MigrationPermissions.Run)]
    [ProducesResponseType(typeof(ApiResponse<MigrationRunResultDto>), 200)]
    public async Task<IActionResult> RerunSingle(int tenantId, int migrationId, CancellationToken ct)
    {
        var errors = await ValidateAsync(_tenantIdValidator, tenantId);
        if (migrationId <= 0)
            errors.Add("Migration id must be greater than zero.");
        if (errors.Count > 0)
            return BadRequest(ApiResponse<object>.Fail("Validation failed", errors));

        var result = await _migrationService.RerunSingleAsync(tenantId, migrationId, ct);
        return Ok(ApiResponse<MigrationRunResultDto>.Ok(result, result.Message));
    }

    /// <summary>Runs one embedded tenant SQL file by name; platform_schema.sql is excluded server-side.</summary>
    [HttpPost("{tenantId:int}/migration/sql-file/run")]
    [MigrationAuthorization(MigrationPermissions.Run)]
    [ProducesResponseType(typeof(ApiResponse<TenantSqlFileExecutionResult>), 200)]
    public async Task<IActionResult> RunSqlFile(int tenantId, [FromBody] RunSqlFileRequest request, CancellationToken ct)
    {
        var errors = await ValidateAsync(_tenantIdValidator, tenantId);
        if (string.IsNullOrWhiteSpace(request.ScriptName))
            errors.Add("Script name is required.");
        if (errors.Count > 0)
            return BadRequest(ApiResponse<object>.Fail("Validation failed", errors));

        var result = await _sqlFileExecutionService.RunAsync(tenantId, request.ScriptName, _currentUser.Username, ct);
        return Ok(ApiResponse<TenantSqlFileExecutionResult>.Ok(result, result.Message));
    }

    /// <summary>Recent execution log for the tenant.</summary>
    [HttpGet("{tenantId:int}/migration/history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MigrationHistoryDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetHistory(int tenantId, [FromQuery] int take = 50)
    {
        var errors = await ValidateAsync(_tenantIdValidator, tenantId);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<object>.Fail("Validation failed", errors));

        var history = await _migrationService.GetHistoryAsync(tenantId, take);
        return Ok(ApiResponse<IReadOnlyList<MigrationHistoryDto>>.Ok(history));
    }
}

public sealed class RunSqlFileRequest
{
    public string ScriptName { get; set; } = string.Empty;
}
