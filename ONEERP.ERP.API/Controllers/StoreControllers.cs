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
[Route("api/stores")]
public class StoresController : BaseController
{
    private readonly IStoreService _service;
    private readonly IValidator<CreateStoreRequest> _createValidator;
    private readonly IValidator<UpdateStoreRequest> _updateValidator;

    public StoresController(
        IStoreService service,
        IValidator<CreateStoreRequest> createValidator,
        IValidator<UpdateStoreRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.StoresView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<StoreDto>>), 200)]
    public async Task<IActionResult> GetStores([FromQuery] int companyId, [FromQuery] int? branchId, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(companyId, branchId ?? 0, page, size, search);
        return Ok(ApiResponse<PaginatedResult<StoreDto>>.Ok(result));
    }

    [HttpGet("all")]
    [Permission(Permissions.StoresView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StoreDto>>), 200)]
    public async Task<IActionResult> GetAllStores([FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<StoreDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.StoresView)]
    [ProducesResponseType(typeof(ApiResponse<StoreDto>), 200)]
    public async Task<IActionResult> GetStore(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<StoreDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.StoresCreate)]
    [ProducesResponseType(typeof(ApiResponse<StoreDto>), 200)]
    public async Task<IActionResult> CreateStore([FromBody] CreateStoreRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<StoreDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<StoreDto>.Ok(result, "Store created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.StoresEdit)]
    [ProducesResponseType(typeof(ApiResponse<StoreDto>), 200)]
    public async Task<IActionResult> UpdateStore(int id, [FromBody] UpdateStoreRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<StoreDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<StoreDto>.Ok(result, "Store updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.StoresDelete)]
    public async Task<IActionResult> DeleteStore(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Store deleted successfully"));
    }
}

[Authorize]
[Route("api/counters")]
public class CountersController : BaseController
{
    private readonly ICounterService _service;
    private readonly IValidator<CreateCounterRequest> _createValidator;
    private readonly IValidator<UpdateCounterRequest> _updateValidator;

    public CountersController(
        ICounterService service,
        IValidator<CreateCounterRequest> createValidator,
        IValidator<UpdateCounterRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.CountersView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<CounterDto>>), 200)]
    public async Task<IActionResult> GetCounters([FromQuery] int? storeId, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(storeId ?? 0, page, size, search);
        return Ok(ApiResponse<PaginatedResult<CounterDto>>.Ok(result));
    }

    [HttpGet("all")]
    [Permission(Permissions.CountersView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CounterDto>>), 200)]
    public async Task<IActionResult> GetAllCounters([FromQuery] int? storeId, [FromQuery] bool includeInactive = false)
    {
        var result = storeId.HasValue && storeId.Value > 0
            ? await _service.GetByStoreAsync(storeId.Value, includeInactive)
            : await _service.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<CounterDto>>.Ok(result));
    }

    [HttpGet("by-store/{storeId:int}")]
    [Permission(Permissions.CountersView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CounterDto>>), 200)]
    public async Task<IActionResult> GetCountersByStore(int storeId, [FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetByStoreAsync(storeId, includeInactive);
        return Ok(ApiResponse<IEnumerable<CounterDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.CountersView)]
    [ProducesResponseType(typeof(ApiResponse<CounterDto>), 200)]
    public async Task<IActionResult> GetCounter(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<CounterDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.CountersCreate)]
    [ProducesResponseType(typeof(ApiResponse<CounterDto>), 200)]
    public async Task<IActionResult> CreateCounter([FromBody] CreateCounterRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CounterDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<CounterDto>.Ok(result, "Counter created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.CountersEdit)]
    [ProducesResponseType(typeof(ApiResponse<CounterDto>), 200)]
    public async Task<IActionResult> UpdateCounter(int id, [FromBody] UpdateCounterRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CounterDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<CounterDto>.Ok(result, "Counter updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.CountersDelete)]
    public async Task<IActionResult> DeleteCounter(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Counter deleted successfully"));
    }
}

[Authorize]
[Route("api/pos-sessions")]
public class POSSessionsController : BaseController
{
    private readonly IPOSSessionService _service;
    private readonly IValidator<CreatePOSSessionRequest> _createValidator;
    private readonly IValidator<UpdatePOSSessionRequest> _updateValidator;

    public POSSessionsController(
        IPOSSessionService service,
        IValidator<CreatePOSSessionRequest> createValidator,
        IValidator<UpdatePOSSessionRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.POSSessionView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<POSSessionDto>>), 200)]
    public async Task<IActionResult> GetSessions([FromQuery] int companyId, [FromQuery] int? branchId, [FromQuery] int? storeId, [FromQuery] int? status, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(companyId, branchId ?? 0, storeId ?? 0, status ?? 0, page, size, search);
        return Ok(ApiResponse<PaginatedResult<POSSessionDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Permission(Permissions.POSSessionView)]
    [ProducesResponseType(typeof(ApiResponse<POSSessionDto>), 200)]
    public async Task<IActionResult> GetSession(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<POSSessionDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.POSSessionCreate)]
    [ProducesResponseType(typeof(ApiResponse<POSSessionDto>), 200)]
    public async Task<IActionResult> CreateSession([FromBody] CreatePOSSessionRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<POSSessionDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<POSSessionDto>.Ok(result, "POS session opened successfully"));
    }

    [HttpPut("{id:long}")]
    [Permission(Permissions.POSSessionEdit)]
    [ProducesResponseType(typeof(ApiResponse<POSSessionDto>), 200)]
    public async Task<IActionResult> UpdateSession(long id, [FromBody] UpdatePOSSessionRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<POSSessionDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<POSSessionDto>.Ok(result, "POS session updated successfully"));
    }

    [HttpDelete("{id:long}")]
    [Permission(Permissions.POSSessionDelete)]
    public async Task<IActionResult> DeleteSession(long id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("POS session deleted successfully"));
    }
}