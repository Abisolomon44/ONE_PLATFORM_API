using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/countries")]
public class CountriesController : BaseController
{
    private readonly ICountryService _service;
    private readonly IValidator<CreateCountryRequest> _createValidator;
    private readonly IValidator<UpdateCountryRequest> _updateValidator;

    public CountriesController(
        ICountryService service,
        IValidator<CreateCountryRequest> createValidator,
        IValidator<UpdateCountryRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.LocationsView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CountryDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<CountryDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.LocationsView)]
    [ProducesResponseType(typeof(ApiResponse<CountryDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<CountryDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.LocationsCreate)]
    [ProducesResponseType(typeof(ApiResponse<CountryDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateCountryRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CountryDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<CountryDto>.Ok(result, "Country created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.LocationsEdit)]
    [ProducesResponseType(typeof(ApiResponse<CountryDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCountryRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CountryDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<CountryDto>.Ok(result, "Country updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.LocationsDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Country deleted successfully"));
    }
}

[Authorize]
[Route("api/states")]
public class StatesController : BaseController
{
    private readonly IStateService _service;
    private readonly IValidator<CreateStateRequest> _createValidator;
    private readonly IValidator<UpdateStateRequest> _updateValidator;

    public StatesController(
        IStateService service,
        IValidator<CreateStateRequest> createValidator,
        IValidator<UpdateStateRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.LocationsView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StateDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int? countryId = null, [FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(countryId, includeInactive);
        return Ok(ApiResponse<IEnumerable<StateDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.LocationsView)]
    [ProducesResponseType(typeof(ApiResponse<StateDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<StateDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.LocationsCreate)]
    [ProducesResponseType(typeof(ApiResponse<StateDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateStateRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<StateDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<StateDto>.Ok(result, "State created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.LocationsEdit)]
    [ProducesResponseType(typeof(ApiResponse<StateDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStateRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<StateDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<StateDto>.Ok(result, "State updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.LocationsDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("State deleted successfully"));
    }
}

[Authorize]
[Route("api/cities")]
public class CitiesController : BaseController
{
    private readonly ICityService _service;
    private readonly IValidator<CreateCityRequest> _createValidator;
    private readonly IValidator<UpdateCityRequest> _updateValidator;

    public CitiesController(
        ICityService service,
        IValidator<CreateCityRequest> createValidator,
        IValidator<UpdateCityRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.LocationsView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CityDto>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? countryId = null,
        [FromQuery] int? stateId = null,
        [FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(countryId, stateId, includeInactive);
        return Ok(ApiResponse<IEnumerable<CityDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.LocationsView)]
    [ProducesResponseType(typeof(ApiResponse<CityDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<CityDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.LocationsCreate)]
    [ProducesResponseType(typeof(ApiResponse<CityDto>), 200)]
    public async Task<IActionResult> Create([FromBody] CreateCityRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CityDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<CityDto>.Ok(result, "City created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.LocationsEdit)]
    [ProducesResponseType(typeof(ApiResponse<CityDto>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCityRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CityDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<CityDto>.Ok(result, "City updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.LocationsDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("City deleted successfully"));
    }
}
