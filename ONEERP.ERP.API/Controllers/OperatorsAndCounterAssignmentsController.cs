using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Authorize]
[Route("api/operators")]
public class OperatorsController : BaseController
{
    private readonly IOperatorService _service;
    private readonly IValidator<CreateOperatorRequest> _createValidator;
    private readonly IValidator<UpdateOperatorRequest> _updateValidator;

    public OperatorsController(
        IOperatorService service,
        IValidator<CreateOperatorRequest> createValidator,
        IValidator<UpdateOperatorRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.OperatorsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<OperatorDto>>), 200)]
    public async Task<IActionResult> GetOperators(
        [FromQuery] int companyId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(companyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<OperatorDto>>.Ok(result));
    }

    [HttpGet("me")]
    [Permission(Permissions.OperatorsView)]
    [ProducesResponseType(typeof(ApiResponse<OperatorDto>), 200)]
    public async Task<IActionResult> GetMyOperator()
    {
        var operatorEntity = await _service.GetByUserIdAsync();
        if (operatorEntity == null)
            return NotFound(ApiResponse<OperatorDto>.Fail("Operator not found for current user"));
        return Ok(ApiResponse<OperatorDto>.Ok(operatorEntity));
    }

    [HttpGet("all")]
    [Permission(Permissions.OperatorsView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OperatorDto>>), 200)]
    public async Task<IActionResult> GetAllOperators(
        [FromQuery] int companyId,
        [FromQuery] bool includeInactive = false)
    {
        var result = await _service.GetAllAsync(companyId, includeInactive);
        return Ok(ApiResponse<IEnumerable<OperatorDto>>.Ok(result));
    }

    [HttpGet("next-code")]
    [Permission(Permissions.OperatorsView)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> GetNextCode([FromQuery] int companyId)
    {
        var code = await _service.GetNextCodeAsync(companyId);
        return Ok(ApiResponse<string>.Ok(code));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.OperatorsView)]
    [ProducesResponseType(typeof(ApiResponse<OperatorDto>), 200)]
    public async Task<IActionResult> GetOperator(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<OperatorDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.OperatorsCreate)]
    [ProducesResponseType(typeof(ApiResponse<OperatorDto>), 200)]
    public async Task<IActionResult> CreateOperator([FromBody] CreateOperatorRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<OperatorDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<OperatorDto>.Ok(result, "Operator created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.OperatorsEdit)]
    [ProducesResponseType(typeof(ApiResponse<OperatorDto>), 200)]
    public async Task<IActionResult> UpdateOperator(int id, [FromBody] UpdateOperatorRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<OperatorDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<OperatorDto>.Ok(result, "Operator updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.OperatorsDelete)]
    public async Task<IActionResult> DeleteOperator(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Operator deleted successfully"));
    }
}

[Authorize]
[Route("api/counter-assignments")]
public class CounterAssignmentsController : BaseController
{
    private readonly ICounterAssignmentService _service;
    private readonly IValidator<CreateCounterAssignmentRequest> _createValidator;
    private readonly IValidator<UpdateCounterAssignmentRequest> _updateValidator;

    public CounterAssignmentsController(
        ICounterAssignmentService service,
        IValidator<CreateCounterAssignmentRequest> createValidator,
        IValidator<UpdateCounterAssignmentRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Permission(Permissions.CounterAssignmentsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<CounterOperatorAssignmentDto>>), 200)]
    public async Task<IActionResult> GetAssignments(
        [FromQuery] int companyId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string search = "")
    {
        var result = await _service.GetPagedAsync(companyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<CounterOperatorAssignmentDto>>.Ok(result));
    }

    [HttpGet("my-default-counter")]
    [Permission(Permissions.CounterAssignmentsView)]
    [ProducesResponseType(typeof(ApiResponse<CounterOperatorAssignmentDto>), 200)]
    public async Task<IActionResult> GetMyDefaultCounter()
    {
        var result = await _service.GetMyDefaultCounterAsync();
        if (result == null)
            return NotFound(ApiResponse<CounterOperatorAssignmentDto>.Fail("No default counter assigned"));
        return Ok(ApiResponse<CounterOperatorAssignmentDto>.Ok(result));
    }

    [HttpGet("my-assignments")]
    [Permission(Permissions.CounterAssignmentsView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CounterOperatorAssignmentDto>>), 200)]
    public async Task<IActionResult> GetMyAssignments()
    {
        var result = await _service.GetMyAssignmentsAsync();
        return Ok(ApiResponse<IEnumerable<CounterOperatorAssignmentDto>>.Ok(result));
    }

    [HttpGet("by-counter/{counterId:int}")]
    [Permission(Permissions.CounterAssignmentsView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CounterOperatorAssignmentDto>>), 200)]
    public async Task<IActionResult> GetOperatorsByCounter(int counterId)
    {
        var result = await _service.GetOperatorsByCounterAsync(counterId);
        return Ok(ApiResponse<IEnumerable<CounterOperatorAssignmentDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Permission(Permissions.CounterAssignmentsView)]
    [ProducesResponseType(typeof(ApiResponse<CounterOperatorAssignmentDto>), 200)]
    public async Task<IActionResult> GetAssignment(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<CounterOperatorAssignmentDto>.Ok(result));
    }

    [HttpPost]
    [Permission(Permissions.CounterAssignmentsCreate)]
    [ProducesResponseType(typeof(ApiResponse<CounterOperatorAssignmentDto>), 200)]
    public async Task<IActionResult> CreateAssignment([FromBody] CreateCounterAssignmentRequest request)
    {
        var errors = await ValidateAsync(_createValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CounterOperatorAssignmentDto>.Fail("Validation failed", errors));

        var result = await _service.CreateAsync(request);
        return Ok(ApiResponse<CounterOperatorAssignmentDto>.Ok(result, "Counter assignment created successfully"));
    }

    [HttpPut("{id:int}")]
    [Permission(Permissions.CounterAssignmentsEdit)]
    [ProducesResponseType(typeof(ApiResponse<CounterOperatorAssignmentDto>), 200)]
    public async Task<IActionResult> UpdateAssignment(int id, [FromBody] UpdateCounterAssignmentRequest request)
    {
        var errors = await ValidateAsync(_updateValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<CounterOperatorAssignmentDto>.Fail("Validation failed", errors));

        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<CounterOperatorAssignmentDto>.Ok(result, "Counter assignment updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Permission(Permissions.CounterAssignmentsDelete)]
    public async Task<IActionResult> DeleteAssignment(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Counter assignment deleted successfully"));
    }
}