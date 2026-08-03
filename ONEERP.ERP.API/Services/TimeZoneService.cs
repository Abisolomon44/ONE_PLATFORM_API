using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface ITimeZoneService
{
    Task<IEnumerable<TimeZoneDto>> GetAllAsync(bool includeInactive = false);
    Task<TimeZoneDto> GetByIdAsync(int id);
    Task<TimeZoneDto> CreateAsync(CreateTimeZoneRequest request);
    Task<TimeZoneDto> UpdateAsync(int id, UpdateTimeZoneRequest request);
    Task<bool> DeleteAsync(int id);
}

public class TimeZoneService : ITimeZoneService
{
    private readonly ITimeZoneRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public TimeZoneService(ITimeZoneRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<TimeZoneDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(Map).ToList();
    }

    public async Task<TimeZoneDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Time zone '{id}' was not found.");
        return Map(item);
    }

    public async Task<TimeZoneDto> CreateAsync(CreateTimeZoneRequest request)
    {
        var timeZoneName = request.TimeZoneName.Trim();
        if (await _repository.GetByTimeZoneNameAsync(timeZoneName) is not null)
            throw new DomainException($"A time zone with id '{timeZoneName}' already exists.");

        var entity = new Timezone
        {
            Name = request.Name.Trim(),
            TimeZoneName = timeZoneName,
            UTCOffset = request.UTCOffset,
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        entity.TimezoneId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("TimeZone", entity.TimezoneId.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<TimeZoneDto> UpdateAsync(int id, UpdateTimeZoneRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Time zone '{id}' was not found.");

        var timeZoneName = request.TimeZoneName.Trim();
        var duplicate = await _repository.GetByTimeZoneNameAsync(timeZoneName);
        if (duplicate is not null && duplicate.TimezoneId != id)
            throw new DomainException($"A time zone with id '{timeZoneName}' already exists.");

        entity.Name = request.Name.Trim();
        entity.TimeZoneName = timeZoneName;
        entity.UTCOffset = request.UTCOffset;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("TimeZone", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Time zone '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("TimeZone", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static TimeZoneDto Map(Timezone entity) => new()
    {
        TimeZoneId = entity.TimezoneId,
        Name = entity.Name,
        TimeZoneName = entity.TimeZoneName,
        UTCOffset = entity.UTCOffset,
        IsActive = entity.IsActive
    };
}
