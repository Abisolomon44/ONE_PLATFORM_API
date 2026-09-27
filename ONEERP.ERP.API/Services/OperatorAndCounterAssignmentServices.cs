using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface IOperatorService
{
    Task<PaginatedResult<OperatorDto>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search);
    Task<string> GetNextCodeAsync(int companyId);
    Task<OperatorDto> GetByIdAsync(int id);
    Task<OperatorDto?> GetByUserIdAsync();
    Task<IEnumerable<OperatorDto>> GetAllAsync(int companyId, bool includeInactive);
    Task<OperatorDto> CreateAsync(CreateOperatorRequest request);
    Task<OperatorDto> UpdateAsync(int id, UpdateOperatorRequest request);
    Task<bool> DeleteAsync(int id);
}

public class OperatorService : IOperatorService
{
    private readonly IOperatorRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public OperatorService(IOperatorRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<OperatorDto>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var operators = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(companyId, search);
        return new PaginatedResult<OperatorDto>
        {
            Items = operators.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<OperatorDto> GetByIdAsync(int id)
    {
        var operatorEntity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Operator '{id}' was not found.");
        return ToDto(operatorEntity);
    }

    public async Task<string> GetNextCodeAsync(int companyId)
        => await _repository.GetNextCodeAsync(companyId);

    public async Task<OperatorDto?> GetByUserIdAsync()
    {
        var operatorEntity = await _repository.GetByUserIdAsync(_currentUser.UserId);
        return operatorEntity != null ? ToDto(operatorEntity) : null;
    }

    public async Task<IEnumerable<OperatorDto>> GetAllAsync(int companyId, bool includeInactive)
    {
        var operators = await _repository.GetAllAsync(includeInactive);
        return operators.Where(o => o.CompanyId == companyId).Select(ToDto);
    }

    public async Task<OperatorDto> CreateAsync(CreateOperatorRequest request)
    {
        var operatorCode = request.OperatorCode.Trim().ToUpperInvariant();
        if (await _repository.CodeInUseAsync(request.CompanyId, operatorCode))
            throw new DomainException($"Operator code '{operatorCode}' is already in use.");

        var operatorEntity = new Operator
        {
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            UserId = request.UserId,
            OperatorTypeId = request.OperatorTypeId,
            OperatorCode = operatorCode,
            OperatorName = request.OperatorName.Trim(),
            IsActive = request.IsActive,
            IsDeleted = false,
            CreatedBy = _currentUser.UserId,
            UpdatedBy = _currentUser.UserId
        };

        operatorEntity.OperatorId = await _repository.InsertAsync(operatorEntity);
        await _auditService.WriteAsync("Operator", operatorEntity.OperatorId.ToString(), "Create", _currentUser.Username);
        return ToDto(operatorEntity);
    }

    public async Task<OperatorDto> UpdateAsync(int id, UpdateOperatorRequest request)
    {
        var operatorEntity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Operator '{id}' was not found.");

        if (request.BranchId is not null) operatorEntity.BranchId = request.BranchId;
        if (request.OperatorTypeId is not null) operatorEntity.OperatorTypeId = request.OperatorTypeId;
        if (request.OperatorCode is not null) operatorEntity.OperatorCode = request.OperatorCode.Trim().ToUpperInvariant();
        if (request.OperatorName is not null) operatorEntity.OperatorName = request.OperatorName.Trim();
        operatorEntity.IsActive = request.IsActive;
        operatorEntity.UpdatedBy = _currentUser.UserId;

        await _repository.UpdateAsync(operatorEntity);
        await _auditService.WriteAsync("Operator", id.ToString(), "Update", _currentUser.Username);
        return ToDto(operatorEntity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var operatorEntity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Operator '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("Operator", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static OperatorDto ToDto(Operator o) => new()
    {
        OperatorId = o.OperatorId,
        CompanyId = o.CompanyId,
        BranchId = o.BranchId,
        UserId = o.UserId,
        UserName = null,
        OperatorTypeId = o.OperatorTypeId,
        OperatorTypeCode = null,
        OperatorTypeName = null,
        OperatorCode = o.OperatorCode,
        OperatorName = o.OperatorName,
        IsActive = o.IsActive,
        IsDeleted = o.IsDeleted,
        CreatedAt = o.CreatedAt,
        UpdatedAt = o.UpdatedAt
    };
}

public interface ICounterAssignmentService
{
    Task<PaginatedResult<CounterOperatorAssignmentDto>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search);
    Task<CounterOperatorAssignmentDto> GetByIdAsync(int id);
    Task<CounterOperatorAssignmentDto?> GetMyDefaultCounterAsync();
    Task<IEnumerable<CounterOperatorAssignmentDto>> GetMyAssignmentsAsync();
    Task<IEnumerable<CounterOperatorAssignmentDto>> GetOperatorsByCounterAsync(int counterId);
    Task<CounterOperatorAssignmentDto> CreateAsync(CreateCounterAssignmentRequest request);
    Task<CounterOperatorAssignmentDto> UpdateAsync(int id, UpdateCounterAssignmentRequest request);
    Task<bool> DeleteAsync(int id);
}

public class CounterAssignmentService : ICounterAssignmentService
{
    private readonly ICounterAssignmentRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;
    private readonly IOperatorRepository _operatorRepository;

    public CounterAssignmentService(
        ICounterAssignmentRepository repository,
        IAuditService auditService,
        ICurrentUser currentUser,
        IOperatorRepository operatorRepository)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
        _operatorRepository = operatorRepository;
    }

    public async Task<PaginatedResult<CounterOperatorAssignmentDto>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var assignments = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(companyId, search);
        return new PaginatedResult<CounterOperatorAssignmentDto>
        {
            Items = assignments.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<CounterOperatorAssignmentDto> GetByIdAsync(int id)
    {
        var assignment = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Counter assignment '{id}' was not found.");
        return ToDto(assignment);
    }

    public async Task<CounterOperatorAssignmentDto?> GetMyDefaultCounterAsync()
    {
        var operatorEntity = await _operatorRepository.GetByUserIdAsync(_currentUser.UserId);
        if (operatorEntity == null) return null;

        var assignments = await _repository.GetByOperatorAsync(operatorEntity.OperatorId);
        var activeAssignment = assignments.FirstOrDefault(a =>
            a.IsActive && !a.IsDeleted &&
            (a.ValidFrom == null || a.ValidFrom <= DateTime.UtcNow) &&
            (a.ValidTo == null || a.ValidTo >= DateTime.UtcNow));

        return activeAssignment != null ? ToDto(activeAssignment) : null;
    }

    public async Task<IEnumerable<CounterOperatorAssignmentDto>> GetMyAssignmentsAsync()
    {
        var operatorEntity = await _operatorRepository.GetByUserIdAsync(_currentUser.UserId);
        if (operatorEntity == null) return [];

        var assignments = await _repository.GetByOperatorAsync(operatorEntity.OperatorId);
        return assignments.Where(a => a.IsActive && !a.IsDeleted).Select(ToDto);
    }

    public async Task<IEnumerable<CounterOperatorAssignmentDto>> GetOperatorsByCounterAsync(int counterId)
    {
        if (counterId <= 0) return [];
        var assignments = await _repository.GetActiveByCounterAsync(counterId);
        return assignments.Select(ToDto);
    }

    public async Task<CounterOperatorAssignmentDto> CreateAsync(CreateCounterAssignmentRequest request)
    {
        var assignment = new CounterAssignment
        {
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            StoreId = request.StoreId,
            CounterId = request.CounterId,
            OperatorId = request.OperatorId,
            IsPrimary = request.IsPrimary,
            ValidFrom = string.IsNullOrEmpty(request.ValidFrom) ? null : DateTime.Parse(request.ValidFrom),
            ValidTo = string.IsNullOrEmpty(request.ValidTo) ? null : DateTime.Parse(request.ValidTo),
            IsActive = request.IsActive,
            IsDeleted = false,
            CreatedBy = _currentUser.UserId,
            UpdatedBy = _currentUser.UserId
        };

        assignment.AssignmentId = await _repository.InsertAsync(assignment);
        await _auditService.WriteAsync("CounterAssignment", assignment.AssignmentId.ToString(), "Create", _currentUser.Username);
        return ToDto(assignment);
    }

    public async Task<CounterOperatorAssignmentDto> UpdateAsync(int id, UpdateCounterAssignmentRequest request)
    {
        var assignment = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Counter assignment '{id}' was not found.");

        if (request.BranchId is not null) assignment.BranchId = request.BranchId;
        if (request.StoreId is not null) assignment.StoreId = request.StoreId.Value;
        if (request.CounterId is not null) assignment.CounterId = request.CounterId.Value;
        if (request.OperatorId is not null) assignment.OperatorId = request.OperatorId.Value;
        assignment.IsPrimary = request.IsPrimary;
        assignment.ValidFrom = string.IsNullOrEmpty(request.ValidFrom) ? null : DateTime.Parse(request.ValidFrom);
        assignment.ValidTo = string.IsNullOrEmpty(request.ValidTo) ? null : DateTime.Parse(request.ValidTo);
        assignment.IsActive = request.IsActive;
        assignment.UpdatedBy = _currentUser.UserId;

        await _repository.UpdateAsync(assignment);
        await _auditService.WriteAsync("CounterAssignment", id.ToString(), "Update", _currentUser.Username);
        return ToDto(assignment);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var assignment = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Counter assignment '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("CounterAssignment", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static CounterOperatorAssignmentDto ToDto(CounterAssignment a) => new()
    {
        AssignmentId = a.AssignmentId,
        CompanyId = a.CompanyId,
        BranchId = a.BranchId,
        StoreId = a.StoreId,
        StoreName = a.StoreName,
        CounterId = a.CounterId,
        CounterCode = a.CounterCode,
        CounterName = a.CounterName,
        OperatorId = a.OperatorId,
        OperatorCode = a.OperatorCode,
        OperatorName = a.OperatorName,
        IsPrimary = a.IsPrimary,
        ValidFrom = a.ValidFrom?.ToString("yyyy-MM-dd"),
        ValidTo = a.ValidTo?.ToString("yyyy-MM-dd"),
        IsActive = a.IsActive,
        IsDeleted = a.IsDeleted,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}