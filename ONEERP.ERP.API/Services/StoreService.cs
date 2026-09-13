using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface IStoreService
{
    Task<PaginatedResult<StoreDto>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search);
    Task<StoreDto> GetByIdAsync(int id);
    Task<IEnumerable<StoreDto>> GetAllAsync(bool includeInactive);
    Task<StoreDto> CreateAsync(CreateStoreRequest request);
    Task<StoreDto> UpdateAsync(int id, UpdateStoreRequest request);
    Task<bool> DeleteAsync(int id);
}

public class StoreService : IStoreService
{
    private readonly IStoreRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public StoreService(IStoreRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<StoreDto>> GetPagedAsync(int companyId, int branchId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var stores = await _repository.GetPagedAsync(companyId, branchId, normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(companyId, branchId, search);
        return new PaginatedResult<StoreDto>
        {
            Items = stores.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<StoreDto> GetByIdAsync(int id)
    {
        var store = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Store '{id}' was not found.");
        return ToDto(store);
    }

    public async Task<IEnumerable<StoreDto>> GetAllAsync(bool includeInactive)
    {
        var stores = await _repository.GetAllAsync(includeInactive);
        return stores.Select(ToDto);
    }

    public async Task<StoreDto> CreateAsync(CreateStoreRequest request)
    {
        var storeCode = request.StoreCode.Trim().ToUpperInvariant();
        if (await _repository.CodeInUseAsync(request.CompanyId, request.BranchId, storeCode))
            throw new DomainException($"Store code '{storeCode}' is already in use.");

        var store = new Store
        {
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            StoreCode = storeCode,
            StoreName = request.StoreName.Trim(),
            StoreType = request.StoreType?.Trim(),
            Address = request.Address?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            IsActive = request.IsActive,
            EntityId = request.EntityId,
            IsDeleted = false,
            CreatedBy = _currentUser.UserId,
            UpdatedBy = _currentUser.UserId
        };

        store.StoreId = await _repository.InsertAsync(store);
        await _auditService.WriteAsync("Store", store.StoreId.ToString(), "Create", _currentUser.Username);
        return ToDto(store);
    }

    public async Task<StoreDto> UpdateAsync(int id, UpdateStoreRequest request)
    {
        var store = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Store '{id}' was not found.");

        store.BranchId = request.BranchId;
        store.StoreCode = request.StoreCode.Trim().ToUpperInvariant();
        store.StoreName = request.StoreName.Trim();
        store.StoreType = request.StoreType?.Trim();
        store.Address = request.Address?.Trim();
        store.Phone = request.Phone?.Trim();
        store.Email = request.Email?.Trim();
        store.IsActive = request.IsActive;
        store.EntityId = request.EntityId;
        store.UpdatedBy = _currentUser.UserId;

        await _repository.UpdateAsync(store);
        await _auditService.WriteAsync("Store", id.ToString(), "Update", _currentUser.Username);
        return ToDto(store);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var store = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Store '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("Store", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static StoreDto ToDto(Store s) => new()
    {
        StoreId = s.StoreId, CompanyId = s.CompanyId, BranchId = s.BranchId, StoreCode = s.StoreCode,
        StoreName = s.StoreName, StoreType = s.StoreType, Address = s.Address, Phone = s.Phone,
        Email = s.Email, IsActive = s.IsActive, EntityId = s.EntityId, IsDeleted = s.IsDeleted,
        CreatedBy = s.CreatedBy, CreatedAt = s.CreatedAt, UpdatedBy = s.UpdatedBy, UpdatedAt = s.UpdatedAt
    };
}

public interface ICounterService
{
    Task<PaginatedResult<CounterDto>> GetPagedAsync(int storeId, int pageNumber, int pageSize, string search);
    Task<CounterDto> GetByIdAsync(int id);
    Task<IEnumerable<CounterDto>> GetByStoreAsync(int storeId, bool includeInactive);
    Task<IEnumerable<CounterDto>> GetAllAsync(bool includeInactive);
    Task<CounterDto> CreateAsync(CreateCounterRequest request);
    Task<CounterDto> UpdateAsync(int id, UpdateCounterRequest request);
    Task<bool> DeleteAsync(int id);
}

public class CounterService : ICounterService
{
    private readonly ICounterRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public CounterService(ICounterRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<CounterDto>> GetPagedAsync(int storeId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var counters = await _repository.GetPagedAsync(storeId, normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(storeId, search);
        return new PaginatedResult<CounterDto>
        {
            Items = counters.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<CounterDto> GetByIdAsync(int id)
    {
        var counter = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Counter '{id}' was not found.");
        return ToDto(counter);
    }

    public async Task<IEnumerable<CounterDto>> GetByStoreAsync(int storeId, bool includeInactive)
    {
        var counters = await _repository.GetByStoreAsync(storeId, includeInactive);
        return counters.Select(ToDto);
    }

    public async Task<IEnumerable<CounterDto>> GetAllAsync(bool includeInactive)
    {
        var counters = await _repository.GetAllAsync(includeInactive);
        return counters.Select(ToDto);
    }

    public async Task<CounterDto> CreateAsync(CreateCounterRequest request)
    {
        var counterCode = request.CounterCode.Trim().ToUpperInvariant();
        if (await _repository.CodeInUseAsync(request.StoreId, counterCode))
            throw new DomainException($"Counter code '{counterCode}' is already in use for this store.");

        var counter = new Counter
        {
            StoreId = request.StoreId,
            CounterCode = counterCode,
            CounterName = request.CounterName.Trim(),
            IsActive = request.IsActive,
            IsDeleted = false,
            CreatedBy = _currentUser.UserId,
            UpdatedBy = _currentUser.UserId
        };

        counter.CounterId = await _repository.InsertAsync(counter);
        await _auditService.WriteAsync("Counter", counter.CounterId.ToString(), "Create", _currentUser.Username);
        return ToDto(counter);
    }

    public async Task<CounterDto> UpdateAsync(int id, UpdateCounterRequest request)
    {
        var counter = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Counter '{id}' was not found.");

        counter.CounterCode = request.CounterCode.Trim().ToUpperInvariant();
        counter.CounterName = request.CounterName.Trim();
        counter.IsActive = request.IsActive;
        counter.UpdatedBy = _currentUser.UserId;

        await _repository.UpdateAsync(counter);
        await _auditService.WriteAsync("Counter", id.ToString(), "Update", _currentUser.Username);
        return ToDto(counter);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var counter = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Counter '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("Counter", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static CounterDto ToDto(Counter c) => new()
    {
        CounterId = c.CounterId, StoreId = c.StoreId, CounterCode = c.CounterCode, CounterName = c.CounterName,
        IsActive = c.IsActive, IsDeleted = c.IsDeleted,
        CreatedBy = c.CreatedBy, CreatedAt = c.CreatedAt, UpdatedBy = c.UpdatedBy, UpdatedAt = c.UpdatedAt
    };
}

public interface IPOSSessionService
{
    Task<PaginatedResult<POSSessionDto>> GetPagedAsync(int companyId, int branchId, int storeId, int status, int pageNumber, int pageSize, string search);
    Task<POSSessionDto> GetByIdAsync(long id);
    Task<POSSessionDto> CreateAsync(CreatePOSSessionRequest request);
    Task<POSSessionDto> UpdateAsync(long id, UpdatePOSSessionRequest request);
    Task<bool> DeleteAsync(long id);
}

public class POSSessionService : IPOSSessionService
{
    private readonly IPOSSessionRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public POSSessionService(IPOSSessionRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<POSSessionDto>> GetPagedAsync(int companyId, int branchId, int storeId, int status, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var sessions = await _repository.GetPagedAsync(companyId, branchId, storeId, status, normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(companyId, branchId, storeId, status, search);
        return new PaginatedResult<POSSessionDto>
        {
            Items = sessions.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<POSSessionDto> GetByIdAsync(long id)
    {
        var session = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"POS session '{id}' was not found.");
        return ToDto(session);
    }

    public async Task<POSSessionDto> CreateAsync(CreatePOSSessionRequest request)
    {
        var sessionNumber = GenerateSessionNumber();
        if (await _repository.GetByNumberAsync(sessionNumber) != null)
            sessionNumber = GenerateSessionNumber();

        var session = new POSSession
        {
            CompanyId = request.CompanyId,
            CompanyName = request.CompanyName?.Trim(),
            BranchId = request.BranchId,
            BranchName = request.BranchName?.Trim(),
            StoreId = request.StoreId,
            StoreName = request.StoreName?.Trim(),
            CounterId = request.CounterId,
            CounterName = request.CounterName?.Trim(),
            CashierUserId = request.CashierUserId,
            CashierUserName = request.CashierUserName?.Trim(),
            SessionNumber = sessionNumber,
            OpeningCash = request.OpeningCash,
            ClosingCash = null,
            OpenedAt = DateTime.UtcNow,
            ClosedAt = null,
            Status = 1,
            CreatedBy = _currentUser.UserId,
            UpdatedBy = _currentUser.UserId
        };

        session.POSSessionId = await _repository.InsertAsync(session);
        await _auditService.WriteAsync("POSSession", session.POSSessionId.ToString(), "Create", _currentUser.Username);
        return ToDto(session);
    }

    public async Task<POSSessionDto> UpdateAsync(long id, UpdatePOSSessionRequest request)
    {
        var session = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"POS session '{id}' was not found.");

        session.ClosingCash = request.ClosingCash;
        session.Status = request.Status;
        session.ClosedAt = request.Status == 2 ? DateTime.UtcNow : session.ClosedAt;
        session.UpdatedBy = _currentUser.UserId;

        await _repository.UpdateAsync(session);
        await _auditService.WriteAsync("POSSession", id.ToString(), "Update", _currentUser.Username);
        return ToDto(session);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var session = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"POS session '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("POSSession", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static string GenerateSessionNumber()
    {
        return $"POS-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
    }

    private static POSSessionDto ToDto(POSSession p) => new()
    {
        POSSessionId = p.POSSessionId, CompanyId = p.CompanyId, CompanyName = p.CompanyName,
        BranchId = p.BranchId, BranchName = p.BranchName, StoreId = p.StoreId, StoreName = p.StoreName,
        CounterId = p.CounterId, CounterName = p.CounterName, CashierUserId = p.CashierUserId,
        CashierUserName = p.CashierUserName, SessionNumber = p.SessionNumber, OpeningCash = p.OpeningCash,
        ClosingCash = p.ClosingCash, OpenedAt = p.OpenedAt, ClosedAt = p.ClosedAt, Status = p.Status,
        CreatedBy = p.CreatedBy, CreatedAt = p.CreatedAt, UpdatedBy = p.UpdatedBy, UpdatedAt = p.UpdatedAt
    };
}