using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;

namespace ONEERP.ERP.API.Services;

public interface IAuditService
{
    Task WriteAsync(string entityName, string? entityId, string action, string? performedBy);
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

    public Task WriteAsync(string entityName, string? entityId, string action, string? performedBy)
    {
        var log = new AuditLog
        {
            Action = action,
            ReferenceId = entityId,
            IPAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            Browser = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString()
        };
        return _repository.InsertAsync(log);
    }
}
