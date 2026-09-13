using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface IBranchService
{
    Task<PaginatedResult<BranchDto>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search);
    Task<BranchDto> GetByIdAsync(int id);
    Task<BranchDto> CreateAsync(CreateBranchRequest request);
    Task<BranchDto> UpdateAsync(int id, UpdateBranchRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IDepartmentService
{
    Task<PaginatedResult<DepartmentDto>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search);
    Task<DepartmentDto> GetByIdAsync(int id);
    Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request);
    Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IDesignationService
{
    Task<PaginatedResult<DesignationDto>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search);
    Task<DesignationDto> GetByIdAsync(int id);
    Task<DesignationDto> CreateAsync(CreateDesignationRequest request);
    Task<DesignationDto> UpdateAsync(int id, UpdateDesignationRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IEmployeeService
{
    Task<PaginatedResult<EmployeeDto>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search);
    Task<EmployeeDto> GetByIdAsync(int id);
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request);
    Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IWarehouseService
{
    Task<PaginatedResult<WarehouseDto>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search);
    Task<WarehouseDto> GetByIdAsync(int id);
    Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request);
    Task<WarehouseDto> UpdateAsync(int id, UpdateWarehouseRequest request);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<WarehouseDto>> GetAllAsync(bool includeInactive);
}

public interface IBranchTypeService
{
    Task<IEnumerable<BranchTypeDto>> GetAllAsync(bool includeInactive);
}

public interface IWarehouseTypeService
{
    Task<IEnumerable<WarehouseTypeDto>> GetAllAsync(bool includeInactive);
}

public interface IEmploymentTypeService
{
    Task<IEnumerable<EmploymentTypeDto>> GetAllAsync(bool includeInactive);
}

public class BranchService : IBranchService
{
    private readonly IBranchRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;
    private readonly IDataScopeResolver _dataScopeResolver;

    public BranchService(IBranchRepository repository, IAuditService auditService, ICurrentUser currentUser, IDataScopeResolver dataScopeResolver)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
        _dataScopeResolver = dataScopeResolver;
    }

    public async Task<PaginatedResult<BranchDto>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;

        if (!await _dataScopeResolver.CanAccessCompanyAsync(companyId))
            return new PaginatedResult<BranchDto> { Items = new List<BranchDto>(), TotalCount = 0, PageNumber = normalizedPage, PageSize = normalizedSize };

        var allowedBranchIds = await _dataScopeResolver.GetAllowedBranchIdsAsync();
        if (allowedBranchIds is null)
        {
            var branches = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search);
            var total = await _repository.CountAsync(companyId, search);
            return new PaginatedResult<BranchDto>
            {
                Items = branches.Select(ToDto).ToList(),
                TotalCount = total,
                PageNumber = normalizedPage,
                PageSize = normalizedSize
            };
        }

        var scoped = (await _repository.GetAllForCompanyAsync(companyId)).Where(b => allowedBranchIds.Contains(b.Id));
        if (!string.IsNullOrWhiteSpace(search))
        {
            scoped = scoped.Where(b =>
                (b.BranchName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (b.BranchCode?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (b.ShortName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        var scopedList = scoped.OrderByDescending(b => b.Id).ToList();

        return new PaginatedResult<BranchDto>
        {
            Items = scopedList.Skip((normalizedPage - 1) * normalizedSize).Take(normalizedSize).Select(ToDto).ToList(),
            TotalCount = scopedList.Count,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<BranchDto> GetByIdAsync(int id)
    {
        if (!await _dataScopeResolver.CanAccessBranchAsync(id))
            throw new NotFoundException($"Branch '{id}' was not found.");
        var branch = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Branch '{id}' was not found.");
        return ToDto(branch);
    }

    public async Task<BranchDto> CreateAsync(CreateBranchRequest request)
    {
        if (!await _dataScopeResolver.CanAccessCompanyAsync(request.CompanyId))
            throw new UnauthorizedAccess("You do not have access to the selected company.");

        var branchCode = request.BranchCode.Trim();
        if (await _repository.CodeInUseAsync(request.CompanyId, branchCode))
            throw new DomainException($"Branch code '{branchCode}' is already in use.");

        var branch = new Branch
        {
            CompanyId = request.CompanyId,
            EntityId = request.EntityId,
            BranchCode = branchCode,
            BranchName = request.BranchName.Trim(),
            ShortName = request.ShortName?.Trim(),
            BranchTypeId = request.BranchTypeId,
            ParentBranchId = request.ParentBranchId,
            ManagerEmployeeId = request.ManagerEmployeeId,
            DefaultWarehouseId = request.DefaultWarehouseId,
            GSTNumber = request.GSTNumber?.Trim(),
            RegistrationNumber = request.RegistrationNumber?.Trim(),
            IsHeadOffice = request.IsHeadOffice,
            IsSalesBranch = request.IsSalesBranch,
            IsPurchaseBranch = request.IsPurchaseBranch,
            IsServiceBranch = request.IsServiceBranch,
            SortOrder = request.SortOrder,
            IsActive = true,
            IsBlocked = false,
            IsDeleted = false,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        branch.Id = await _repository.InsertAsync(branch);
        await _auditService.WriteAsync("Branch", branch.Id.ToString(), "Create", _currentUser.Username);
        return ToDto(branch);
    }

    public async Task<BranchDto> UpdateAsync(int id, UpdateBranchRequest request)
    {
        if (!await _dataScopeResolver.CanAccessBranchAsync(id))
            throw new NotFoundException($"Branch '{id}' was not found.");
        var branch = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Branch '{id}' was not found.");

        branch.EntityId = request.EntityId;
        branch.BranchCode = request.BranchCode.Trim();
        branch.BranchName = request.BranchName.Trim();
        branch.ShortName = request.ShortName?.Trim();
        branch.BranchTypeId = request.BranchTypeId ?? branch.BranchTypeId;
        branch.ParentBranchId = request.ParentBranchId;
        branch.ManagerEmployeeId = request.ManagerEmployeeId;
        branch.DefaultWarehouseId = request.DefaultWarehouseId;
        branch.GSTNumber = request.GSTNumber?.Trim();
        branch.RegistrationNumber = request.RegistrationNumber?.Trim();
        branch.IsHeadOffice = request.IsHeadOffice;
        branch.IsSalesBranch = request.IsSalesBranch;
        branch.IsPurchaseBranch = request.IsPurchaseBranch;
        branch.IsServiceBranch = request.IsServiceBranch;
        branch.SortOrder = request.SortOrder;
        branch.IsActive = request.IsActive;
        branch.IsBlocked = request.IsBlocked;
        branch.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(branch);
        await _auditService.WriteAsync("Branch", id.ToString(), "Update", _currentUser.Username);
        return ToDto(branch);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (!await _dataScopeResolver.CanAccessBranchAsync(id))
            throw new NotFoundException($"Branch '{id}' was not found.");
        var branch = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Branch '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("Branch", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static BranchDto ToDto(Branch b) => new()
    {
        Id = b.Id, CompanyId = b.CompanyId, EntityId = b.EntityId, BranchCode = b.BranchCode, BranchName = b.BranchName,
        ShortName = b.ShortName, BranchTypeId = b.BranchTypeId, ParentBranchId = b.ParentBranchId,
        ManagerEmployeeId = b.ManagerEmployeeId, DefaultWarehouseId = b.DefaultWarehouseId,
        GSTNumber = b.GSTNumber, RegistrationNumber = b.RegistrationNumber,
        IsHeadOffice = b.IsHeadOffice, IsSalesBranch = b.IsSalesBranch,
        IsPurchaseBranch = b.IsPurchaseBranch, IsServiceBranch = b.IsServiceBranch,
        SortOrder = b.SortOrder, IsActive = b.IsActive, IsBlocked = b.IsBlocked,
        IsDeleted = b.IsDeleted, CreatedBy = b.CreatedBy, CreatedDate = b.CreatedDate,
        ModifiedBy = b.ModifiedBy, ModifiedDate = b.ModifiedDate
    };
}

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;
    private readonly IDataScopeResolver _dataScopeResolver;

    public DepartmentService(IDepartmentRepository repository, IAuditService auditService, ICurrentUser currentUser, IDataScopeResolver dataScopeResolver)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
        _dataScopeResolver = dataScopeResolver;
    }

    public async Task<PaginatedResult<DepartmentDto>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;

        if (!await _dataScopeResolver.CanAccessCompanyAsync(companyId)
            || (branchId > 0 && !await _dataScopeResolver.CanAccessBranchAsync(branchId)))
            return new PaginatedResult<DepartmentDto> { Items = new List<DepartmentDto>(), TotalCount = 0, PageNumber = normalizedPage, PageSize = normalizedSize };

        var departments = await _repository.GetPagedAsync(companyId, branchId, normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(companyId, branchId, search);
        return new PaginatedResult<DepartmentDto>
        {
            Items = departments.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<DepartmentDto> GetByIdAsync(int id)
    {
        var department = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Department '{id}' was not found.");
        if (!await _dataScopeResolver.CanAccessCompanyAsync(department.CompanyId))
            throw new NotFoundException($"Department '{id}' was not found.");
        return ToDto(department);
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request)
    {
        if (!await _dataScopeResolver.CanAccessCompanyAsync(request.CompanyId)
            || (request.BranchId > 0 && !await _dataScopeResolver.CanAccessBranchAsync(request.BranchId)))
            throw new UnauthorizedAccess("You do not have access to the selected company or branch.");

        var departmentCode = request.DepartmentCode.Trim();
        if (await _repository.CodeInUseAsync(request.CompanyId, request.BranchId, departmentCode))
            throw new DomainException($"Department code '{departmentCode}' is already in use.");

        var department = new Department
        {
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            DepartmentCode = departmentCode,
            DepartmentName = request.DepartmentName.Trim(),
            ShortName = request.ShortName?.Trim(),
            ParentDepartmentId = request.ParentDepartmentId,
            ManagerEmployeeId = request.ManagerEmployeeId,
            SortOrder = request.SortOrder,
            IsActive = true,
            IsBlocked = false,
            IsDeleted = false,
            Remarks = request.Remarks?.Trim(),
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        department.Id = await _repository.InsertAsync(department);
        await _auditService.WriteAsync("Department", department.Id.ToString(), "Create", _currentUser.Username);
        return ToDto(department);
    }

    public async Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentRequest request)
    {
        var department = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Department '{id}' was not found.");
        if (!await _dataScopeResolver.CanAccessCompanyAsync(department.CompanyId))
            throw new NotFoundException($"Department '{id}' was not found.");

        department.DepartmentCode = request.DepartmentCode.Trim();
        department.DepartmentName = request.DepartmentName.Trim();
        department.ShortName = request.ShortName?.Trim();
        department.ParentDepartmentId = request.ParentDepartmentId;
        department.ManagerEmployeeId = request.ManagerEmployeeId;
        department.SortOrder = request.SortOrder;
        department.IsActive = request.IsActive;
        department.IsBlocked = request.IsBlocked;
        department.Remarks = request.Remarks?.Trim();
        department.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(department);
        await _auditService.WriteAsync("Department", id.ToString(), "Update", _currentUser.Username);
        return ToDto(department);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Department '{id}' was not found.");
        if (!await _dataScopeResolver.CanAccessCompanyAsync(department.CompanyId))
            throw new NotFoundException($"Department '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("Department", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static DepartmentDto ToDto(Department d) => new()
    {
        Id = d.Id, CompanyId = d.CompanyId, BranchId = d.BranchId, DepartmentCode = d.DepartmentCode,
        DepartmentName = d.DepartmentName, ShortName = d.ShortName, ParentDepartmentId = d.ParentDepartmentId,
        ManagerEmployeeId = d.ManagerEmployeeId, SortOrder = d.SortOrder, IsActive = d.IsActive,
        IsBlocked = d.IsBlocked, IsDeleted = d.IsDeleted, Remarks = d.Remarks,
        CreatedBy = d.CreatedBy, CreatedDate = d.CreatedDate, ModifiedBy = d.ModifiedBy, ModifiedDate = d.ModifiedDate
    };
}

public class DesignationService : IDesignationService
{
    private readonly IDesignationRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;
    private readonly IDataScopeResolver _dataScopeResolver;

    public DesignationService(IDesignationRepository repository, IAuditService auditService, ICurrentUser currentUser, IDataScopeResolver dataScopeResolver)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
        _dataScopeResolver = dataScopeResolver;
    }

    public async Task<PaginatedResult<DesignationDto>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;

        if (!await _dataScopeResolver.CanAccessCompanyAsync(companyId))
            return new PaginatedResult<DesignationDto> { Items = new List<DesignationDto>(), TotalCount = 0, PageNumber = normalizedPage, PageSize = normalizedSize };

        var designations = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(companyId, search);
        return new PaginatedResult<DesignationDto>
        {
            Items = designations.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<DesignationDto> GetByIdAsync(int id)
    {
        var designation = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Designation '{id}' was not found.");
        if (!await _dataScopeResolver.CanAccessCompanyAsync(designation.CompanyId))
            throw new NotFoundException($"Designation '{id}' was not found.");
        return ToDto(designation);
    }

    public async Task<DesignationDto> CreateAsync(CreateDesignationRequest request)
    {
        if (!await _dataScopeResolver.CanAccessCompanyAsync(request.CompanyId))
            throw new UnauthorizedAccess("You do not have access to the selected company.");

        var designationCode = request.DesignationCode.Trim();
        if (await _repository.CodeInUseAsync(request.CompanyId, designationCode))
            throw new DomainException($"Designation code '{designationCode}' is already in use.");

        var designation = new Designation
        {
            CompanyId = request.CompanyId,
            DepartmentId = request.DepartmentId,
            DesignationCode = designationCode,
            DesignationName = request.DesignationName.Trim(),
            ShortName = request.ShortName?.Trim(),
            LevelNo = request.LevelNo,
            Grade = request.Grade?.Trim(),
            Description = request.Description?.Trim(),
            SortOrder = request.SortOrder,
            IsDefault = request.IsDefault,
            IsActive = true,
            IsBlocked = false,
            IsDeleted = false,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        designation.Id = await _repository.InsertAsync(designation);
        await _auditService.WriteAsync("Designation", designation.Id.ToString(), "Create", _currentUser.Username);
        return ToDto(designation);
    }

    public async Task<DesignationDto> UpdateAsync(int id, UpdateDesignationRequest request)
    {
        var designation = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Designation '{id}' was not found.");
        if (!await _dataScopeResolver.CanAccessCompanyAsync(designation.CompanyId))
            throw new NotFoundException($"Designation '{id}' was not found.");

        designation.DesignationCode = request.DesignationCode.Trim();
        designation.DesignationName = request.DesignationName.Trim();
        designation.ShortName = request.ShortName?.Trim();
        designation.LevelNo = request.LevelNo;
        designation.Grade = request.Grade?.Trim();
        designation.Description = request.Description?.Trim();
        designation.SortOrder = request.SortOrder;
        designation.IsDefault = request.IsDefault;
        designation.IsActive = request.IsActive;
        designation.IsBlocked = request.IsBlocked;
        designation.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(designation);
        await _auditService.WriteAsync("Designation", id.ToString(), "Update", _currentUser.Username);
        return ToDto(designation);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var designation = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Designation '{id}' was not found.");
        if (!await _dataScopeResolver.CanAccessCompanyAsync(designation.CompanyId))
            throw new NotFoundException($"Designation '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("Designation", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static DesignationDto ToDto(Designation d) => new()
    {
        Id = d.Id, CompanyId = d.CompanyId, DepartmentId = d.DepartmentId,
        DesignationCode = d.DesignationCode, DesignationName = d.DesignationName,
        ShortName = d.ShortName, LevelNo = d.LevelNo, Grade = d.Grade,
        Description = d.Description, SortOrder = d.SortOrder, IsDefault = d.IsDefault,
        IsActive = d.IsActive, IsBlocked = d.IsBlocked, IsDeleted = d.IsDeleted,
        CreatedBy = d.CreatedBy, CreatedDate = d.CreatedDate, ModifiedBy = d.ModifiedBy, ModifiedDate = d.ModifiedDate
    };
}

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;
    private readonly IDataScopeResolver _dataScopeResolver;

    public EmployeeService(IEmployeeRepository repository, IAuditService auditService, ICurrentUser currentUser, IDataScopeResolver dataScopeResolver)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
        _dataScopeResolver = dataScopeResolver;
    }

    public async Task<PaginatedResult<EmployeeDto>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;

        if (!await _dataScopeResolver.CanAccessCompanyAsync(companyId)
            || (branchId > 0 && !await _dataScopeResolver.CanAccessBranchAsync(branchId)))
            return new PaginatedResult<EmployeeDto> { Items = new List<EmployeeDto>(), TotalCount = 0, PageNumber = normalizedPage, PageSize = normalizedSize };

        var employees = await _repository.GetPagedAsync(companyId, branchId, normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(companyId, branchId, search);
        return new PaginatedResult<EmployeeDto>
        {
            Items = employees.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<EmployeeDto> GetByIdAsync(int id)
    {
        var employee = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Employee '{id}' was not found.");
        if (!await _dataScopeResolver.CanAccessCompanyAsync(employee.CompanyId))
            throw new NotFoundException($"Employee '{id}' was not found.");
        return ToDto(employee);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request)
    {
        if (!await _dataScopeResolver.CanAccessCompanyAsync(request.CompanyId)
            || (request.BranchId is > 0 && !await _dataScopeResolver.CanAccessBranchAsync(request.BranchId.Value)))
            throw new UnauthorizedAccess("You do not have access to the selected company or branch.");

        var employeeCode = request.EmployeeCode.Trim();
        if (await _repository.CodeInUseAsync(request.CompanyId, employeeCode))
            throw new DomainException($"Employee code '{employeeCode}' is already in use.");

        var employee = new Employee
        {
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            DepartmentId = request.DepartmentId,
            DesignationId = request.DesignationId,
            EmployeeCode = employeeCode,
            EmployeeNumber = request.EmployeeNumber?.Trim(),
            FirstName = request.FirstName.Trim(),
            MiddleName = request.MiddleName?.Trim(),
            LastName = request.LastName.Trim(),
            DisplayName = request.DisplayName?.Trim(),
            GenderId = request.GenderId,
            MaritalStatusId = request.MaritalStatusId,
            DateOfBirth = request.DateOfBirth,
            DateOfJoining = request.DateOfJoining,
            DateOfLeaving = request.DateOfLeaving,
            OfficialEmail = request.OfficialEmail?.Trim(),
            PersonalEmail = request.PersonalEmail?.Trim(),
            MobileNo = request.MobileNo?.Trim(),
            AlternateMobileNo = request.AlternateMobileNo?.Trim(),
            ReportingManagerId = request.ReportingManagerId,
            EmploymentTypeId = request.EmploymentTypeId,
            IsActive = true,
            IsBlocked = false,
            IsDeleted = false,
            Remarks = request.Remarks?.Trim(),
            EntityId = request.EntityId,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        employee.Id = await _repository.InsertAsync(employee);
        await _auditService.WriteAsync("Employee", employee.Id.ToString(), "Create", _currentUser.Username);
        return ToDto(employee);
    }

    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeRequest request)
    {
        var employee = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Employee '{id}' was not found.");
        if (!await _dataScopeResolver.CanAccessCompanyAsync(employee.CompanyId))
            throw new NotFoundException($"Employee '{id}' was not found.");

        employee.EmployeeCode = request.EmployeeCode.Trim();
        employee.EmployeeNumber = request.EmployeeNumber?.Trim();
        employee.FirstName = request.FirstName.Trim();
        employee.MiddleName = request.MiddleName?.Trim();
        employee.LastName = request.LastName.Trim();
        employee.DisplayName = request.DisplayName?.Trim();
        employee.GenderId = request.GenderId;
        employee.MaritalStatusId = request.MaritalStatusId;
        employee.DateOfBirth = request.DateOfBirth;
        employee.DateOfJoining = request.DateOfJoining;
        employee.DateOfLeaving = request.DateOfLeaving;
        employee.OfficialEmail = request.OfficialEmail?.Trim();
        employee.PersonalEmail = request.PersonalEmail?.Trim();
        employee.MobileNo = request.MobileNo?.Trim();
        employee.AlternateMobileNo = request.AlternateMobileNo?.Trim();
        employee.ReportingManagerId = request.ReportingManagerId;
        employee.EmploymentTypeId = request.EmploymentTypeId;
        employee.IsActive = request.IsActive;
        employee.IsBlocked = request.IsBlocked;
        employee.Remarks = request.Remarks?.Trim();
        employee.EntityId = request.EntityId;
        employee.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(employee);
        await _auditService.WriteAsync("Employee", id.ToString(), "Update", _currentUser.Username);
        return ToDto(employee);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Employee '{id}' was not found.");
        if (!await _dataScopeResolver.CanAccessCompanyAsync(employee.CompanyId))
            throw new NotFoundException($"Employee '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("Employee", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static EmployeeDto ToDto(Employee e) => new()
    {
        Id = e.Id, EntityId = e.EntityId, CompanyId = e.CompanyId, BranchId = e.BranchId, DepartmentId = e.DepartmentId,
        DesignationId = e.DesignationId, EmployeeCode = e.EmployeeCode, EmployeeNumber = e.EmployeeNumber,
        FirstName = e.FirstName, MiddleName = e.MiddleName, LastName = e.LastName,
        DisplayName = e.DisplayName, GenderId = e.GenderId, MaritalStatusId = e.MaritalStatusId,
        DateOfBirth = e.DateOfBirth, DateOfJoining = e.DateOfJoining, DateOfLeaving = e.DateOfLeaving,
        OfficialEmail = e.OfficialEmail, PersonalEmail = e.PersonalEmail, MobileNo = e.MobileNo,
        AlternateMobileNo = e.AlternateMobileNo, ReportingManagerId = e.ReportingManagerId,
        EmploymentTypeId = e.EmploymentTypeId, IsActive = e.IsActive, IsBlocked = e.IsBlocked,
        IsDeleted = e.IsDeleted, Remarks = e.Remarks, CreatedBy = e.CreatedBy,
        CreatedDate = e.CreatedDate, ModifiedBy = e.ModifiedBy, ModifiedDate = e.ModifiedDate
    };
}

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;
    private readonly IDataScopeResolver _dataScopeResolver;

    public WarehouseService(IWarehouseRepository repository, IAuditService auditService, ICurrentUser currentUser, IDataScopeResolver dataScopeResolver)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
        _dataScopeResolver = dataScopeResolver;
    }

    public async Task<PaginatedResult<WarehouseDto>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;

        if (!await _dataScopeResolver.CanAccessCompanyAsync(companyId)
            || (branchId > 0 && !await _dataScopeResolver.CanAccessBranchAsync(branchId)))
            return new PaginatedResult<WarehouseDto> { Items = new List<WarehouseDto>(), TotalCount = 0, PageNumber = normalizedPage, PageSize = normalizedSize };

        var allowedWarehouseIds = await _dataScopeResolver.GetAllowedWarehouseIdsAsync();
        if (allowedWarehouseIds is null)
        {
            var warehouses = await _repository.GetPagedAsync(companyId, branchId, normalizedPage, normalizedSize, search);
            var total = await _repository.CountAsync(companyId, branchId, search);
            return new PaginatedResult<WarehouseDto>
            {
                Items = warehouses.Select(ToDto).ToList(),
                TotalCount = total,
                PageNumber = normalizedPage,
                PageSize = normalizedSize
            };
        }

        var scoped = (await _repository.GetAllForCompanyAsync(companyId)).Where(w => allowedWarehouseIds.Contains(w.Id));
        if (branchId > 0)
            scoped = scoped.Where(w => w.BranchId == branchId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            scoped = scoped.Where(w =>
                (w.WarehouseName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (w.WarehouseCode?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (w.ShortName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        var scopedList = scoped.OrderByDescending(w => w.Id).ToList();

        return new PaginatedResult<WarehouseDto>
        {
            Items = scopedList.Skip((normalizedPage - 1) * normalizedSize).Take(normalizedSize).Select(ToDto).ToList(),
            TotalCount = scopedList.Count,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<WarehouseDto> GetByIdAsync(int id)
    {
        if (!await _dataScopeResolver.CanAccessWarehouseAsync(id))
            throw new NotFoundException($"Warehouse '{id}' was not found.");
        var warehouse = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Warehouse '{id}' was not found.");
        return ToDto(warehouse);
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllAsync(bool includeInactive)
    {
        var warehouses = await _repository.GetAllAsync(includeInactive);

        var allowedWarehouseIds = await _dataScopeResolver.GetAllowedWarehouseIdsAsync();
        if (allowedWarehouseIds is not null)
            warehouses = warehouses.Where(w => allowedWarehouseIds.Contains(w.Id));

        var allowedCompanyIds = await _dataScopeResolver.GetAllowedCompanyIdsAsync();
        if (allowedCompanyIds is not null)
            warehouses = warehouses.Where(w => allowedCompanyIds.Contains(w.CompanyId));

        return warehouses.Select(ToDto);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request)
    {
        if (!await _dataScopeResolver.CanAccessCompanyAsync(request.CompanyId)
            || (request.BranchId.HasValue && request.BranchId > 0 && !await _dataScopeResolver.CanAccessBranchAsync(request.BranchId.Value)))
            throw new UnauthorizedAccess("You do not have access to the selected company or branch.");

        var warehouseCode = request.WarehouseCode.Trim();
        if (await _repository.CodeInUseAsync(request.CompanyId, request.BranchId ?? 0, warehouseCode))
            throw new DomainException($"Warehouse code '{warehouseCode}' is already in use.");

        var warehouse = new Warehouse
        {
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            EntityId = request.EntityId,
            WarehouseCode = warehouseCode,
            WarehouseName = request.WarehouseName.Trim(),
            ShortName = request.ShortName?.Trim(),
            WarehouseTypeId = request.WarehouseTypeId,
            ParentWarehouseId = request.ParentWarehouseId,
            ManagerEmployeeId = request.ManagerEmployeeId,
            AllowNegativeStock = request.AllowNegativeStock,
            IsDefault = request.IsDefault,
            SortOrder = request.SortOrder,
            Remarks = request.Remarks?.Trim(),
            IsActive = true,
            IsBlocked = false,
            IsDeleted = false,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        warehouse.Id = await _repository.InsertAsync(warehouse);
        await _auditService.WriteAsync("Warehouse", warehouse.Id.ToString(), "Create", _currentUser.Username);
        return ToDto(warehouse);
    }

    public async Task<WarehouseDto> UpdateAsync(int id, UpdateWarehouseRequest request)
    {
        if (!await _dataScopeResolver.CanAccessWarehouseAsync(id))
            throw new NotFoundException($"Warehouse '{id}' was not found.");
        var warehouse = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Warehouse '{id}' was not found.");

        warehouse.EntityId = request.EntityId;
        warehouse.WarehouseCode = request.WarehouseCode.Trim();
        warehouse.WarehouseName = request.WarehouseName.Trim();
        warehouse.ShortName = request.ShortName?.Trim();
        warehouse.WarehouseTypeId = request.WarehouseTypeId;
        warehouse.ParentWarehouseId = request.ParentWarehouseId;
        warehouse.ManagerEmployeeId = request.ManagerEmployeeId;
        warehouse.AllowNegativeStock = request.AllowNegativeStock;
        warehouse.IsDefault = request.IsDefault;
        warehouse.SortOrder = request.SortOrder;
        warehouse.IsActive = request.IsActive;
        warehouse.IsBlocked = request.IsBlocked;
        warehouse.Remarks = request.Remarks?.Trim();
        warehouse.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(warehouse);
        await _auditService.WriteAsync("Warehouse", id.ToString(), "Update", _currentUser.Username);
        return ToDto(warehouse);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (!await _dataScopeResolver.CanAccessWarehouseAsync(id))
            throw new NotFoundException($"Warehouse '{id}' was not found.");
        var warehouse = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Warehouse '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("Warehouse", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static WarehouseDto ToDto(Warehouse w) => new()
    {
        Id = w.Id, CompanyId = w.CompanyId, BranchId = w.BranchId, EntityId = w.EntityId, WarehouseCode = w.WarehouseCode,
        WarehouseName = w.WarehouseName, ShortName = w.ShortName, WarehouseTypeId = w.WarehouseTypeId,
        ParentWarehouseId = w.ParentWarehouseId, ManagerEmployeeId = w.ManagerEmployeeId,
        AllowNegativeStock = w.AllowNegativeStock, IsDefault = w.IsDefault, SortOrder = w.SortOrder,
        Remarks = w.Remarks, IsActive = w.IsActive, IsBlocked = w.IsBlocked, IsDeleted = w.IsDeleted,
        CreatedBy = w.CreatedBy, CreatedDate = w.CreatedDate, ModifiedBy = w.ModifiedBy, ModifiedDate = w.ModifiedDate
    };
}

public class BranchTypeService : IBranchTypeService
{
    private readonly IBranchTypeRepository _repository;

    public BranchTypeService(IBranchTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<BranchTypeDto>> GetAllAsync(bool includeInactive)
    {
        var types = await _repository.GetAllAsync(includeInactive);
        return types.Select(t => new BranchTypeDto
        {
            BranchTypeId = t.BranchTypeId,
            Name = t.Name,
            Code = t.Code,
            Description = t.Description,
            SortOrder = t.SortOrder,
            IsActive = t.IsActive,
        });
    }
}

public class WarehouseTypeService : IWarehouseTypeService
{
    private readonly IWarehouseTypeRepository _repository;

    public WarehouseTypeService(IWarehouseTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<WarehouseTypeDto>> GetAllAsync(bool includeInactive)
    {
        var types = await _repository.GetAllAsync(includeInactive);
        return types.Select(t => new WarehouseTypeDto
        {
            WarehouseTypeId = t.WarehouseTypeId,
            Name = t.Name,
            Code = t.Code,
            Description = t.Description,
            SortOrder = t.SortOrder,
            IsActive = t.IsActive,
        });
    }
}

public class EmploymentTypeService : IEmploymentTypeService
{
    private readonly IEmploymentTypeRepository _repository;

    public EmploymentTypeService(IEmploymentTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EmploymentTypeDto>> GetAllAsync(bool includeInactive)
    {
        var types = await _repository.GetAllAsync(includeInactive);
        return types.Select(t => new EmploymentTypeDto
        {
            EmploymentTypeId = t.EmploymentTypeId,
            Name = t.Name,
            Code = t.Code,
            Description = t.Description,
            SortOrder = t.SortOrder,
            IsActive = t.IsActive,
        });
    }
}