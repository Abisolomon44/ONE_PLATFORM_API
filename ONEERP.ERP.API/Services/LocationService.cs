using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface ICountryService
{
    Task<IEnumerable<CountryDto>> GetAllAsync(bool includeInactive = false);
    Task<CountryDto> GetByIdAsync(int id);
    Task<CountryDto> CreateAsync(CreateCountryRequest request);
    Task<CountryDto> UpdateAsync(int id, UpdateCountryRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IStateService
{
    Task<IEnumerable<StateDto>> GetAllAsync(int? countryId, bool includeInactive = false);
    Task<StateDto> GetByIdAsync(int id);
    Task<StateDto> CreateAsync(CreateStateRequest request);
    Task<StateDto> UpdateAsync(int id, UpdateStateRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface ICityService
{
    Task<IEnumerable<CityDto>> GetAllAsync(int? countryId, int? stateId, bool includeInactive = false);
    Task<CityDto> GetByIdAsync(int id);
    Task<CityDto> CreateAsync(CreateCityRequest request);
    Task<CityDto> UpdateAsync(int id, UpdateCityRequest request);
    Task<bool> DeleteAsync(int id);
}

public class CountryService : ICountryService
{
    private readonly ICountryRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public CountryService(ICountryRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<CountryDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(Map).ToList();
    }

    public async Task<CountryDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Country '{id}' was not found.");
        return Map(item);
    }

    public async Task<CountryDto> CreateAsync(CreateCountryRequest request)
    {
        var name = request.Name.Trim();
        var iso2 = request.ISOCode2.Trim().ToUpperInvariant();
        var iso3 = request.ISOCode3.Trim().ToUpperInvariant();

        if (await _repository.GetByISOCode2Async(iso2) is not null)
            throw new DomainException($"A country with ISO-2 code '{iso2}' already exists.");
        if (await _repository.GetByISOCode3Async(iso3) is not null)
            throw new DomainException($"A country with ISO-3 code '{iso3}' already exists.");

        var entity = new Country
        {
            Name = name,
            ISOCode2 = iso2,
            ISOCode3 = iso3,
            PhoneCode = request.PhoneCode?.Trim(),
            CurrencyCode = request.CurrencyCode?.Trim().ToUpperInvariant(),
            Nationality = request.Nationality?.Trim(),
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        entity.CountryId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("Country", entity.CountryId.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<CountryDto> UpdateAsync(int id, UpdateCountryRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Country '{id}' was not found.");

        var name = request.Name.Trim();
        var iso2 = request.ISOCode2.Trim().ToUpperInvariant();
        var iso3 = request.ISOCode3.Trim().ToUpperInvariant();

        var duplicate2 = await _repository.GetByISOCode2Async(iso2);
        if (duplicate2 is not null && duplicate2.CountryId != id)
            throw new DomainException($"A country with ISO-2 code '{iso2}' already exists.");

        var duplicate3 = await _repository.GetByISOCode3Async(iso3);
        if (duplicate3 is not null && duplicate3.CountryId != id)
            throw new DomainException($"A country with ISO-3 code '{iso3}' already exists.");

        entity.Name = name;
        entity.ISOCode2 = iso2;
        entity.ISOCode3 = iso3;
        entity.PhoneCode = request.PhoneCode?.Trim();
        entity.CurrencyCode = request.CurrencyCode?.Trim().ToUpperInvariant();
        entity.Nationality = request.Nationality?.Trim();
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("Country", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Country '{id}' was not found.");

        if (await _repository.HasStatesAsync(id))
            throw new DomainException("Countries with states cannot be deleted.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("Country", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static CountryDto Map(Country entity) => new()
    {
        CountryId = entity.CountryId,
        Name = entity.Name,
        ISOCode2 = entity.ISOCode2,
        ISOCode3 = entity.ISOCode3,
        PhoneCode = entity.PhoneCode,
        CurrencyCode = entity.CurrencyCode,
        Nationality = entity.Nationality,
        IsActive = entity.IsActive,
        CreatedBy = entity.CreatedBy,
        CreatedDate = entity.CreatedDate,
        ModifiedBy = entity.ModifiedBy,
        ModifiedDate = entity.ModifiedDate
    };
}

public class StateService : IStateService
{
    private readonly IStateRepository _repository;
    private readonly ICountryRepository _countryRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public StateService(IStateRepository repository, ICountryRepository countryRepository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _countryRepository = countryRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<StateDto>> GetAllAsync(int? countryId, bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(countryId, includeInactive);
        return items.Select(Map).ToList();
    }

    public async Task<StateDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"State '{id}' was not found.");
        return Map(item);
    }

    public async Task<StateDto> CreateAsync(CreateStateRequest request)
    {
        await EnsureCountryAsync(request.CountryId);

        var code = request.StateCode.Trim().ToUpperInvariant();
        if (await _repository.GetByCodeAsync(request.CountryId, code) is not null)
            throw new DomainException($"A state with code '{code}' already exists in this country.");

        var entity = new State
        {
            CountryId = request.CountryId,
            Name = request.Name.Trim(),
            StateCode = code,
            GSTStateCode = request.GSTStateCode?.Trim(),
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        entity.StateId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("State", entity.StateId.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<StateDto> UpdateAsync(int id, UpdateStateRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"State '{id}' was not found.");

        await EnsureCountryAsync(request.CountryId);

        var code = request.StateCode.Trim().ToUpperInvariant();
        var duplicate = await _repository.GetByCodeAsync(request.CountryId, code);
        if (duplicate is not null && duplicate.StateId != id)
            throw new DomainException($"A state with code '{code}' already exists in this country.");

        entity.CountryId = request.CountryId;
        entity.Name = request.Name.Trim();
        entity.StateCode = code;
        entity.GSTStateCode = request.GSTStateCode?.Trim();
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("State", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"State '{id}' was not found.");

        if (await _repository.HasCitiesAsync(id))
            throw new DomainException("States with cities cannot be deleted.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("State", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private async Task EnsureCountryAsync(int countryId)
    {
        var country = await _countryRepository.GetByIdAsync(countryId)
            ?? throw new DomainException($"Country '{countryId}' was not found.");
        if (!country.IsActive)
            throw new DomainException($"Country '{country.Name}' is inactive.");
    }

    private static StateDto Map(State entity) => new()
    {
        StateId = entity.StateId,
        CountryId = entity.CountryId,
        CountryName = entity.CountryName,
        Name = entity.Name,
        StateCode = entity.StateCode,
        GSTStateCode = entity.GSTStateCode,
        IsActive = entity.IsActive,
        CreatedBy = entity.CreatedBy,
        CreatedDate = entity.CreatedDate,
        ModifiedBy = entity.ModifiedBy,
        ModifiedDate = entity.ModifiedDate
    };
}

public class CityService : ICityService
{
    private readonly ICityRepository _repository;
    private readonly ICountryRepository _countryRepository;
    private readonly IStateRepository _stateRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public CityService(
        ICityRepository repository,
        ICountryRepository countryRepository,
        IStateRepository stateRepository,
        IAuditService auditService,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _countryRepository = countryRepository;
        _stateRepository = stateRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<CityDto>> GetAllAsync(int? countryId, int? stateId, bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(countryId, stateId, includeInactive);
        return items.Select(Map).ToList();
    }

    public async Task<CityDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"City '{id}' was not found.");
        return Map(item);
    }

    public async Task<CityDto> CreateAsync(CreateCityRequest request)
    {
        await EnsureStateAsync(request.CountryId, request.StateId);

        var name = request.Name.Trim();
        if (await _repository.GetByNameAsync(request.StateId, name) is not null)
            throw new DomainException($"A city with name '{name}' already exists in this state.");

        var entity = new City
        {
            CountryId = request.CountryId,
            StateId = request.StateId,
            Name = name,
            PostalCode = request.PostalCode?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        entity.CityId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("City", entity.CityId.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<CityDto> UpdateAsync(int id, UpdateCityRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"City '{id}' was not found.");

        await EnsureStateAsync(request.CountryId, request.StateId);

        var name = request.Name.Trim();
        var duplicate = await _repository.GetByNameAsync(request.StateId, name);
        if (duplicate is not null && duplicate.CityId != id)
            throw new DomainException($"A city with name '{name}' already exists in this state.");

        entity.CountryId = request.CountryId;
        entity.StateId = request.StateId;
        entity.Name = name;
        entity.PostalCode = request.PostalCode?.Trim();
        entity.Latitude = request.Latitude;
        entity.Longitude = request.Longitude;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("City", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"City '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("City", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private async Task EnsureStateAsync(int countryId, int stateId)
    {
        var country = await _countryRepository.GetByIdAsync(countryId)
            ?? throw new DomainException($"Country '{countryId}' was not found.");
        if (!country.IsActive)
            throw new DomainException($"Country '{country.Name}' is inactive.");

        var state = await _stateRepository.GetByIdAsync(stateId)
            ?? throw new DomainException($"State '{stateId}' was not found.");
        if (!state.IsActive)
            throw new DomainException($"State '{state.Name}' is inactive.");
        if (state.CountryId != countryId)
            throw new DomainException($"State '{state.Name}' does not belong to country '{country.Name}'.");
    }

    private static CityDto Map(City entity) => new()
    {
        CityId = entity.CityId,
        CountryId = entity.CountryId,
        CountryName = entity.CountryName,
        StateId = entity.StateId,
        StateName = entity.StateName,
        Name = entity.Name,
        PostalCode = entity.PostalCode,
        Latitude = entity.Latitude,
        Longitude = entity.Longitude,
        IsActive = entity.IsActive,
        CreatedBy = entity.CreatedBy,
        CreatedDate = entity.CreatedDate,
        ModifiedBy = entity.ModifiedBy,
        ModifiedDate = entity.ModifiedDate
    };
}
