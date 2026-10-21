using ONEERP.Platform.API.Models;
using ONEERP.Platform.API.Repositories;

namespace ONEERP.Platform.API.Services;

public interface IAuditService
{
    Task WriteAsync(
        string entityName,
        string? entityId,
        string action,
        string? performedBy,
        int? tenantId = null,
        string? oldValues = null,
        string? newValues = null);
}

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(IAuditLogRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task WriteAsync(
        string entityName,
        string? entityId,
        string action,
        string? performedBy,
        int? tenantId = null,
        string? oldValues = null,
        string? newValues = null)
    {
        var log = new AuditLog
        {
            TenantId = tenantId,
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            PerformedBy = performedBy,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
        };
        return _repository.InsertAsync(log);
    }
}
