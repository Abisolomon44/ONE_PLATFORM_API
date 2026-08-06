using System.Text.Json;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Middleware;

/// <summary>
/// Catches all unhandled exceptions, logs them via Serilog and returns a
/// consistent JSON error response without leaking internal details.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain error {StatusCode}: {Message}", ex.StatusCode, ex.Message);
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";
            var domainResponse = ApiResponse.Fail(ex.Message);
            await context.Response.WriteAsync(JsonSerializer.Serialize(domainResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
                throw;

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                Success = false,
                Message = ex.Message,
                Exception = ex.ToString()
            }));
        }
    }
}

/// <summary>
/// Populates the tenant context for each request from the X-Tenant-Code header
/// (or the tenant_code claim of the authenticated user).
/// </summary>
public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantAccessor accessor, ITenantConnectionResolver resolver)
    {
        var tenantCode = context.Request.Headers["X-Tenant-Code"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tenantCode) && context.User.Identity?.IsAuthenticated == true)
            tenantCode = context.User.FindFirst(ONEERP.Shared.Constants.ClaimTypes.TenantCode)?.Value;

        if (!string.IsNullOrWhiteSpace(tenantCode))
        {
            accessor.TenantCode = tenantCode.Trim();
            accessor.ConnectionString = await resolver.GetConnectionStringAsync(tenantCode);
        }

        await _next(context);
    }
}
