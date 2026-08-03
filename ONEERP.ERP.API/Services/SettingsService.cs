using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;

namespace ONEERP.ERP.API.Services;

public interface ISettingsService
{
    Task<IEnumerable<ApplicationSetting>> GetAllAsync();
    Task<bool> UpdateAsync(UpdateSettingsRequest request, string currentUser);
}

public class SettingsService : ISettingsService
{
    private readonly IApplicationSettingRepository _repository;
    private readonly IAuditService _auditService;

    public SettingsService(IApplicationSettingRepository repository, IAuditService auditService)
    {
        _repository = repository;
        _auditService = auditService;
    }

    public async Task<IEnumerable<ApplicationSetting>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<bool> UpdateAsync(UpdateSettingsRequest request, string currentUser)
    {
        if (request.Settings is null || request.Settings.Count == 0)
            return false;

        foreach (var (key, value) in request.Settings)
            await _repository.UpsertAsync(key, value, currentUser);

        await _auditService.WriteAsync("ApplicationSetting", null, "Update", currentUser);
        return true;
    }
}
