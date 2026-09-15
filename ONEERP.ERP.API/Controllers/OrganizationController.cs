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
[Route("api/organization")]
public class OrganizationController : BaseController
{
    private readonly IBranchService _branchService;
    private readonly IDepartmentService _departmentService;
    private readonly IDesignationService _designationService;
    private readonly IEmployeeService _employeeService;
    private readonly IWarehouseService _warehouseService;
    private readonly IBranchTypeService _branchTypeService;
    private readonly IWarehouseTypeService _warehouseTypeService;
    private readonly IEmploymentTypeService _employmentTypeService;
    private readonly IValidator<CreateBranchRequest> _createBranchValidator;
    private readonly IValidator<UpdateBranchRequest> _updateBranchValidator;
    private readonly IValidator<CreateDepartmentRequest> _createDepartmentValidator;
    private readonly IValidator<UpdateDepartmentRequest> _updateDepartmentValidator;
    private readonly IValidator<CreateDesignationRequest> _createDesignationValidator;
    private readonly IValidator<UpdateDesignationRequest> _updateDesignationValidator;
    private readonly IValidator<CreateEmployeeRequest> _createEmployeeValidator;
    private readonly IValidator<UpdateEmployeeRequest> _updateEmployeeValidator;
    private readonly IValidator<CreateWarehouseRequest> _createWarehouseValidator;
    private readonly IValidator<UpdateWarehouseRequest> _updateWarehouseValidator;

    public OrganizationController(
        IBranchService branchService,
        IDepartmentService departmentService,
        IDesignationService designationService,
        IEmployeeService employeeService,
        IWarehouseService warehouseService,
        IBranchTypeService branchTypeService,
        IWarehouseTypeService warehouseTypeService,
        IEmploymentTypeService employmentTypeService,
        IValidator<CreateBranchRequest> createBranchValidator,
        IValidator<UpdateBranchRequest> updateBranchValidator,
        IValidator<CreateDepartmentRequest> createDepartmentValidator,
        IValidator<UpdateDepartmentRequest> updateDepartmentValidator,
        IValidator<CreateDesignationRequest> createDesignationValidator,
        IValidator<UpdateDesignationRequest> updateDesignationValidator,
        IValidator<CreateEmployeeRequest> createEmployeeValidator,
        IValidator<UpdateEmployeeRequest> updateEmployeeValidator,
        IValidator<CreateWarehouseRequest> createWarehouseValidator,
        IValidator<UpdateWarehouseRequest> updateWarehouseValidator)
    {
        _branchService = branchService;
        _departmentService = departmentService;
        _designationService = designationService;
        _employeeService = employeeService;
        _warehouseService = warehouseService;
        _branchTypeService = branchTypeService;
        _warehouseTypeService = warehouseTypeService;
        _employmentTypeService = employmentTypeService;
        _createBranchValidator = createBranchValidator;
        _updateBranchValidator = updateBranchValidator;
        _createDepartmentValidator = createDepartmentValidator;
        _updateDepartmentValidator = updateDepartmentValidator;
        _createDesignationValidator = createDesignationValidator;
        _updateDesignationValidator = updateDesignationValidator;
        _createEmployeeValidator = createEmployeeValidator;
        _updateEmployeeValidator = updateEmployeeValidator;
        _createWarehouseValidator = createWarehouseValidator;
        _updateWarehouseValidator = updateWarehouseValidator;
    }

    /* ==================== Branch Types ==================== */
    // No [Permission] gate: simple lookup used to populate dropdowns app-wide.
    [HttpGet("branch-types")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BranchTypeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBranchTypes([FromQuery] bool includeInactive = false)
    {
        var result = await _branchTypeService.GetAllAsync(includeInactive);

        return Ok(ApiResponse<IEnumerable<BranchTypeDto>>.Ok(result));
    }
    /* ==================== Warehouse Types ==================== */

    // No [Permission] gate: simple lookup used to populate dropdowns app-wide.
    [HttpGet("warehouse-types")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WarehouseTypeDto>>), 200)]
    public async Task<IActionResult> GetWarehouseTypes([FromQuery] bool includeInactive = false)
    {
        var result = await _warehouseTypeService.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<WarehouseTypeDto>>.Ok(result));
    }

    /* ==================== Employment Types ==================== */

    // No [Permission] gate: simple lookup used to populate dropdowns app-wide.
    [HttpGet("employment-types")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmploymentTypeDto>>), 200)]
    public async Task<IActionResult> GetEmploymentTypes([FromQuery] bool includeInactive = false)
    {
        var result = await _employmentTypeService.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<EmploymentTypeDto>>.Ok(result));
    }

    /* ==================== Branches ==================== */

    [HttpGet("branches")]
    [Permission(Permissions.BranchesView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<BranchDto>>), 200)]
    public async Task<IActionResult> GetBranches([FromQuery] int companyId, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _branchService.GetPagedAsync(companyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<BranchDto>>.Ok(result));
    }

    [HttpGet("branches/{id:int}")]
    [Permission(Permissions.BranchesView)]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), 200)]
    public async Task<IActionResult> GetBranch(int id)
    {
        var result = await _branchService.GetByIdAsync(id);
        return Ok(ApiResponse<BranchDto>.Ok(result));
    }

    [HttpPost("branches")]
    [Permission(Permissions.BranchesCreate)]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), 200)]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request)
    {
        var errors = await ValidateAsync(_createBranchValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<BranchDto>.Fail("Validation failed", errors));

        var result = await _branchService.CreateAsync(request);
        return Ok(ApiResponse<BranchDto>.Ok(result, "Branch created successfully"));
    }

    [HttpPut("branches/{id:int}")]
    [Permission(Permissions.BranchesEdit)]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), 200)]
    public async Task<IActionResult> UpdateBranch(int id, [FromBody] UpdateBranchRequest request)
    {
        var errors = await ValidateAsync(_updateBranchValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<BranchDto>.Fail("Validation failed", errors));

        var result = await _branchService.UpdateAsync(id, request);
        return Ok(ApiResponse<BranchDto>.Ok(result, "Branch updated successfully"));
    }

    [HttpDelete("branches/{id:int}")]
    [Permission(Permissions.BranchesEdit)]
    public async Task<IActionResult> DeleteBranch(int id)
    {
        await _branchService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Branch deleted successfully"));
    }

    /* ==================== Departments ==================== */

    [HttpGet("departments")]
    [Permission(Permissions.DepartmentsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<DepartmentDto>>), 200)]
    public async Task<IActionResult> GetDepartments([FromQuery] int companyId, [FromQuery] int? branchId, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _departmentService.GetPagedAsync(companyId, branchId ?? 0, page, size, search);
        return Ok(ApiResponse<PaginatedResult<DepartmentDto>>.Ok(result));
    }

    [HttpGet("departments/{id:int}")]
    [Permission(Permissions.DepartmentsView)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), 200)]
    public async Task<IActionResult> GetDepartment(int id)
    {
        var result = await _departmentService.GetByIdAsync(id);
        return Ok(ApiResponse<DepartmentDto>.Ok(result));
    }

    [HttpPost("departments")]
    [Permission(Permissions.DepartmentsCreate)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), 200)]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        var errors = await ValidateAsync(_createDepartmentValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<DepartmentDto>.Fail("Validation failed", errors));

        var result = await _departmentService.CreateAsync(request);
        return Ok(ApiResponse<DepartmentDto>.Ok(result, "Department created successfully"));
    }

    [HttpPut("departments/{id:int}")]
    [Permission(Permissions.DepartmentsEdit)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), 200)]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentRequest request)
    {
        var errors = await ValidateAsync(_updateDepartmentValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<DepartmentDto>.Fail("Validation failed", errors));

        var result = await _departmentService.UpdateAsync(id, request);
        return Ok(ApiResponse<DepartmentDto>.Ok(result, "Department updated successfully"));
    }

    [HttpDelete("departments/{id:int}")]
    [Permission(Permissions.DepartmentsEdit)]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        await _departmentService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Department deleted successfully"));
    }

    /* ==================== Designations ==================== */

    [HttpGet("designations")]
    [Permission(Permissions.DesignationsView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<DesignationDto>>), 200)]
    public async Task<IActionResult> GetDesignations([FromQuery] int companyId, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _designationService.GetPagedAsync(companyId, page, size, search);
        return Ok(ApiResponse<PaginatedResult<DesignationDto>>.Ok(result));
    }

    [HttpGet("designations/{id:int}")]
    [Permission(Permissions.DesignationsView)]
    [ProducesResponseType(typeof(ApiResponse<DesignationDto>), 200)]
    public async Task<IActionResult> GetDesignation(int id)
    {
        var result = await _designationService.GetByIdAsync(id);
        return Ok(ApiResponse<DesignationDto>.Ok(result));
    }

    [HttpPost("designations")]
    [Permission(Permissions.DesignationsCreate)]
    [ProducesResponseType(typeof(ApiResponse<DesignationDto>), 200)]
    public async Task<IActionResult> CreateDesignation([FromBody] CreateDesignationRequest request)
    {
        var errors = await ValidateAsync(_createDesignationValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<DesignationDto>.Fail("Validation failed", errors));

        var result = await _designationService.CreateAsync(request);
        return Ok(ApiResponse<DesignationDto>.Ok(result, "Designation created successfully"));
    }

    [HttpPut("designations/{id:int}")]
    [Permission(Permissions.DesignationsEdit)]
    [ProducesResponseType(typeof(ApiResponse<DesignationDto>), 200)]
    public async Task<IActionResult> UpdateDesignation(int id, [FromBody] UpdateDesignationRequest request)
    {
        var errors = await ValidateAsync(_updateDesignationValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<DesignationDto>.Fail("Validation failed", errors));

        var result = await _designationService.UpdateAsync(id, request);
        return Ok(ApiResponse<DesignationDto>.Ok(result, "Designation updated successfully"));
    }

    [HttpDelete("designations/{id:int}")]
    [Permission(Permissions.DesignationsEdit)]
    public async Task<IActionResult> DeleteDesignation(int id)
    {
        await _designationService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Designation deleted successfully"));
    }

    /* ==================== Employees ==================== */

    [HttpGet("employees")]
    [Permission(Permissions.EmployeesView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<EmployeeDto>>), 200)]
    public async Task<IActionResult> GetEmployees([FromQuery] int companyId, [FromQuery] int? branchId, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _employeeService.GetPagedAsync(companyId, branchId ?? 0, page, size, search);
        return Ok(ApiResponse<PaginatedResult<EmployeeDto>>.Ok(result));
    }

    [HttpGet("employees/{id:int}")]
    [Permission(Permissions.EmployeesView)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), 200)]
    public async Task<IActionResult> GetEmployee(int id)
    {
        var result = await _employeeService.GetByIdAsync(id);
        return Ok(ApiResponse<EmployeeDto>.Ok(result));
    }

    [HttpPost("employees")]
    [Permission(Permissions.EmployeesCreate)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), 200)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
    {
        var errors = await ValidateAsync(_createEmployeeValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<EmployeeDto>.Fail("Validation failed", errors));

        var result = await _employeeService.CreateAsync(request);
        return Ok(ApiResponse<EmployeeDto>.Ok(result, "Employee created successfully"));
    }

    [HttpPut("employees/{id:int}")]
    [Permission(Permissions.EmployeesEdit)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), 200)]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
    {
        var errors = await ValidateAsync(_updateEmployeeValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<EmployeeDto>.Fail("Validation failed", errors));

        var result = await _employeeService.UpdateAsync(id, request);
        return Ok(ApiResponse<EmployeeDto>.Ok(result, "Employee updated successfully"));
    }

    [HttpDelete("employees/{id:int}")]
    [Permission(Permissions.EmployeesDelete)]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        await _employeeService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Employee deleted successfully"));
    }

    /* ==================== Warehouses ==================== */

    [HttpGet("warehouses")]
    [Permission(Permissions.WarehousesView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<WarehouseDto>>), 200)]
    public async Task<IActionResult> GetWarehouses([FromQuery] int companyId, [FromQuery] int? branchId, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string search = "")
    {
        var result = await _warehouseService.GetPagedAsync(companyId, branchId ?? 0, page, size, search);
        return Ok(ApiResponse<PaginatedResult<WarehouseDto>>.Ok(result));
    }

    [HttpGet("warehouses/all")]
    [Permission(Permissions.WarehousesView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WarehouseDto>>), 200)]
    public async Task<IActionResult> GetAllWarehouses([FromQuery] bool includeInactive = false)
    {
        var result = await _warehouseService.GetAllAsync(includeInactive);
        return Ok(ApiResponse<IEnumerable<WarehouseDto>>.Ok(result));
    }

    [HttpGet("warehouses/next-code")]
    [Permission(Permissions.WarehousesCreate)]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    public async Task<IActionResult> GetNextWarehouseCode([FromQuery] int companyId, [FromQuery] string? prefix = null)
        => Ok(ApiResponse<string>.Ok(await _warehouseService.GetNextCodeAsync(companyId, prefix ?? "WH")));

    [HttpGet("warehouses/{id:int}")]
    [Permission(Permissions.WarehousesView)]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), 200)]
    public async Task<IActionResult> GetWarehouse(int id)
    {
        var result = await _warehouseService.GetByIdAsync(id);
        return Ok(ApiResponse<WarehouseDto>.Ok(result));
    }

    [HttpPost("warehouses")]
    [Permission(Permissions.WarehousesCreate)]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), 200)]
    public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseRequest request)
    {
        var errors = await ValidateAsync(_createWarehouseValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<WarehouseDto>.Fail("Validation failed", errors));

        var result = await _warehouseService.CreateAsync(request);
        return Ok(ApiResponse<WarehouseDto>.Ok(result, "Warehouse created successfully"));
    }

    [HttpPut("warehouses/{id:int}")]
    [Permission(Permissions.WarehousesEdit)]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), 200)]
    public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] UpdateWarehouseRequest request)
    {
        var errors = await ValidateAsync(_updateWarehouseValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<WarehouseDto>.Fail("Validation failed", errors));

        var result = await _warehouseService.UpdateAsync(id, request);
        return Ok(ApiResponse<WarehouseDto>.Ok(result, "Warehouse updated successfully"));
    }

    [HttpDelete("warehouses/{id:int}")]
    [Permission(Permissions.WarehousesEdit)]
    public async Task<IActionResult> DeleteWarehouse(int id)
    {
        await _warehouseService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Warehouse deleted successfully"));
    }
}