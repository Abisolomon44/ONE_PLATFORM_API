using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.ERP.API.Security;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface ISaleEntryContextService
{
    Task<SaleEntryContextResponse> GetContextAsync(int? companyId, int? branchId, int? storeId);
    Task<SaleEntryContextValidateResponse> ValidateContextAsync(SaleEntryContextValidateRequest request);
}

public class SaleEntryContextService : ISaleEntryContextService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly ICounterRepository _counterRepository;
    private readonly IOperatorRepository _operatorRepository;
    private readonly ICounterAssignmentRepository _counterAssignmentRepository;
    private readonly IPOSSessionRepository _posSessionRepository;
    private readonly ICurrentUser _currentUser;

    public SaleEntryContextService(
        ICompanyRepository companyRepository,
        IBranchRepository branchRepository,
        IStoreRepository storeRepository,
        ICounterRepository counterRepository,
        IOperatorRepository operatorRepository,
        ICounterAssignmentRepository counterAssignmentRepository,
        IPOSSessionRepository posSessionRepository,
        ICurrentUser currentUser)
    {
        _companyRepository = companyRepository;
        _branchRepository = branchRepository;
        _storeRepository = storeRepository;
        _counterRepository = counterRepository;
        _operatorRepository = operatorRepository;
        _counterAssignmentRepository = counterAssignmentRepository;
        _posSessionRepository = posSessionRepository;
        _currentUser = currentUser;
    }

    public async Task<SaleEntryContextResponse> GetContextAsync(int? companyId, int? branchId, int? storeId)
    {
        var response = new SaleEntryContextResponse();
        var userId = _currentUser.UserId;
        var userCompanyId = _currentUser.CompanyId;

        // 1. Load accessible companies
        var companies = await _companyRepository.GetAccessibleAsync(userId);
        response.Companies = companies.Select(c => new SaleEntryCompanyDto
        {
            Id = c.Id,
            Name = c.CompanyName,
            Code = c.CompanyCode
        }).ToList();

        // Determine selected company
        int resolvedCompanyId;
        if (companyId.HasValue && companyId.Value > 0)
        {
            resolvedCompanyId = companyId.Value;
            if (!companies.Any(c => c.Id == resolvedCompanyId))
            {
                response.IsValid = false;
                response.Message = "Selected company is not accessible";
                return response;
            }
        }
        else if (companies.Count() == 1)
        {
            resolvedCompanyId = companies.First().Id;
        }
        else
        {
            resolvedCompanyId = userCompanyId;
        }

        response.Company = response.Companies.FirstOrDefault(c => c.Id == resolvedCompanyId);

        // 2. Load accessible branches for the company
        var branches = await _branchRepository.GetAllForCompanyAsync(resolvedCompanyId);
        response.Branches = branches.Where(b => b.IsActive && !b.IsDeleted).Select(b => new SaleEntryBranchDto
        {
            Id = b.Id,
            CompanyId = b.CompanyId,
            Name = b.BranchName,
            Code = b.BranchCode
        }).ToList();

        // Determine selected branch
        int resolvedBranchId;
        if (branchId.HasValue && branchId.Value > 0)
        {
            resolvedBranchId = branchId.Value;
            if (!response.Branches.Any(b => b.Id == resolvedBranchId))
            {
                response.IsValid = false;
                response.Message = "Selected branch is not accessible";
                return response;
            }
        }
        else if (response.Branches.Count == 1)
        {
            resolvedBranchId = response.Branches[0].Id;
        }
        else
        {
            // Try to find head office or first active branch
            resolvedBranchId = response.Branches.FirstOrDefault(b => 
                branches.FirstOrDefault(br => br.Id == b.Id)?.IsHeadOffice == true)?.Id 
                ?? response.Branches.FirstOrDefault()?.Id ?? 0;
        }

        response.Branch = response.Branches.FirstOrDefault(b => b.Id == resolvedBranchId);

        // 3. Load accessible stores for the branch
        var stores = await _storeRepository.GetByBranchAsync(resolvedBranchId, false);
        response.Stores = stores.Where(s => s.IsActive && !s.IsDeleted).Select(s => new SaleEntryStoreDto
        {
            Id = s.StoreId,
            BranchId = s.BranchId ?? 0,
            Name = s.StoreName,
            Code = s.StoreCode
        }).ToList();

        // Determine selected store
        int resolvedStoreId;
        if (storeId.HasValue && storeId.Value > 0)
        {
            resolvedStoreId = storeId.Value;
            if (!response.Stores.Any(s => s.Id == resolvedStoreId))
            {
                response.IsValid = false;
                response.Message = "Selected store is not accessible";
                return response;
            }
        }
        else if (response.Stores.Count == 1)
        {
            resolvedStoreId = response.Stores[0].Id;
        }
        else
        {
            resolvedStoreId = response.Stores.FirstOrDefault()?.Id ?? 0;
        }

        response.Store = response.Stores.FirstOrDefault(s => s.Id == resolvedStoreId);

        // 4. Load default counter for the authenticated user/operator
        var counter = await GetDefaultCounterAsync(userId, resolvedStoreId);
        if (counter != null)
        {
            response.Counter = new SaleEntryCounterDto
            {
                Id = counter.CounterId,
                Code = counter.CounterCode,
                Name = counter.CounterName,
                Assignment = "Default Counter",
                StoreId = counter.StoreId,
                StoreName = response.Store?.Name
            };
        }

        // 5. Load operator for authenticated user
        var operatorDto = await GetOperatorAsync(userId);
        if (operatorDto != null)
        {
            response.Operator = operatorDto;
        }

        // 6. Load active POS session for the counter
        if (response.Counter != null)
        {
            var posSession = await GetActivePOSSessionAsync(response.Counter.Id, userId);
            if (posSession != null)
            {
                response.PosSession = posSession;
            }
        }

        response.IsValid = true;
        response.Message = null;
        return response;
    }

    public async Task<SaleEntryContextValidateResponse> ValidateContextAsync(SaleEntryContextValidateRequest request)
    {
        var response = new SaleEntryContextValidateResponse();

        try
        {
            // Validate Company
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            if (company == null || company.IsDeleted || !company.IsActive)
            {
                response.IsValid = false;
                response.Code = "INVALID_COMPANY";
                response.Message = "Company is invalid or inactive";
                return response;
            }

            // Validate Branch
            var branch = await _branchRepository.GetByIdAsync(request.BranchId);
            if (branch == null || branch.IsDeleted || !branch.IsActive || branch.CompanyId != request.CompanyId)
            {
                response.IsValid = false;
                response.Code = "INVALID_BRANCH";
                response.Message = "Branch is invalid, inactive, or does not belong to selected company";
                return response;
            }

            // Validate Store
            var store = await _storeRepository.GetByIdAsync(request.StoreId);
            if (store == null || store.IsDeleted || !store.IsActive || store.BranchId != request.BranchId)
            {
                response.IsValid = false;
                response.Code = "INVALID_STORE";
                response.Message = "Store is invalid, inactive, or does not belong to selected branch";
                return response;
            }

            // Validate Counter
            var counter = await _counterRepository.GetByIdAsync(request.CounterId);
            if (counter == null || counter.IsDeleted || !counter.IsActive || counter.StoreId != request.StoreId)
            {
                response.IsValid = false;
                response.Code = "INVALID_COUNTER";
                response.Message = "Counter is invalid, inactive, or does not belong to selected store";
                return response;
            }

            // Validate Counter Assignment for current user
            var assignment = await _counterAssignmentRepository.GetActiveByCounterAndOperatorAsync(request.CounterId, request.OperatorId);
            if (assignment == null)
            {
                response.IsValid = false;
                response.Code = "DEFAULT_COUNTER_NOT_ASSIGNED";
                response.Message = "No active counter assignment found for this operator";
                return response;
            }

            // Validate Operator
            var operatorEntity = await _operatorRepository.GetByIdAsync(request.OperatorId);
            if (operatorEntity == null || operatorEntity.IsDeleted || !operatorEntity.IsActive)
            {
                response.IsValid = false;
                response.Code = "INVALID_OPERATOR";
                response.Message = "Operator is invalid or inactive";
                return response;
            }

            // Verify operator belongs to the current user
            if (operatorEntity.UserId != _currentUser.UserId)
            {
                response.IsValid = false;
                response.Code = "UNAUTHORIZED_OPERATOR";
                response.Message = "Operator does not belong to authenticated user";
                return response;
            }

            // Validate POS Session
            var posSession = await _posSessionRepository.GetByIdAsync(request.PosSessionId);
            if (posSession == null)
            {
                response.IsValid = false;
                response.Code = "ACTIVE_SESSION_NOT_FOUND";
                response.Message = "POS session not found";
                return response;
            }

            if (posSession.Status != 1) // 1 = OPEN
            {
                response.IsValid = false;
                response.Code = "SESSION_CLOSED";
                var statusText = posSession.Status switch
                {
                    1 => "OPEN",
                    2 => "CLOSED",
                    3 => "SUSPENDED",
                    _ => "UNKNOWN"
                };
                response.Message = $"POS session is {statusText.ToLower()}. Please open a new session.";
                return response;
            }

            // Verify session belongs to the correct context
            if (posSession.CompanyId != request.CompanyId ||
                posSession.BranchId != request.BranchId ||
                posSession.StoreId != request.StoreId ||
                posSession.CounterId != request.CounterId)
            {
                response.IsValid = false;
                response.Code = "CONTEXT_MISMATCH";
                response.Message = "POS session does not match the selected business context";
                return response;
            }

            // All validations passed
            response.IsValid = true;
            response.Code = null;
            response.Message = "Sale entry context validated successfully";
            
            // Generate a temporary context token (in production, use a secure token store)
            var tokenData = $"{request.CompanyId}|{request.BranchId}|{request.StoreId}|{request.CounterId}|{request.OperatorId}|{request.PosSessionId}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            response.ContextToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(tokenData));

            return response;
        }
        catch (Exception ex)
        {
            response.IsValid = false;
            response.Code = "VALIDATION_ERROR";
            response.Message = $"Validation failed: {ex.Message}";
            return response;
        }
    }

    private async Task<Counter?> GetDefaultCounterAsync(int userId, int storeId)
    {
        // Get operator for this user
        var operatorEntity = await _operatorRepository.GetByUserIdAsync(userId);
        if (operatorEntity == null) return null;

        // Get active counter assignment for this operator
        var assignments = await _counterAssignmentRepository.GetByOperatorAsync(operatorEntity.OperatorId);
        var activeAssignment = assignments.FirstOrDefault(a => 
            a.IsActive && !a.IsDeleted && 
            (a.ValidFrom == null || a.ValidFrom <= DateTime.UtcNow) &&
            (a.ValidTo == null || a.ValidTo >= DateTime.UtcNow));

        if (activeAssignment == null) return null;

        // If storeId is specified, verify the assignment matches
        if (storeId > 0 && activeAssignment.StoreId != storeId) return null;

        return await _counterRepository.GetByIdAsync(activeAssignment.CounterId);
    }

    private async Task<SaleEntryOperatorDto?> GetOperatorAsync(int userId)
    {
        var operatorEntity = await _operatorRepository.GetByUserIdAsync(userId);
        if (operatorEntity == null || operatorEntity.IsDeleted || !operatorEntity.IsActive) return null;

        var operatorType = await _operatorRepository.GetOperatorTypeAsync(operatorEntity.OperatorTypeId ?? 0);

        return new SaleEntryOperatorDto
        {
            Id = operatorEntity.OperatorId,
            Code = operatorEntity.OperatorCode,
            Name = operatorEntity.OperatorName,
            Type = operatorType?.Name ?? "POS Operator"
        };
    }

    private async Task<SaleEntryPOSSessionDto?> GetActivePOSSessionAsync(int counterId, int userId)
    {
        // Get active session for this counter
        var sessions = await _posSessionRepository.GetActiveByCounterAsync(counterId);

        // Sessions are owned by an operator, so match the operator of this user.
        var sessionOperator = await _operatorRepository.GetByUserIdAsync(userId);
        var activeSession = sessionOperator == null
            ? null
            : sessions.FirstOrDefault(s => s.OperatorId == sessionOperator.OperatorId);

        if (activeSession == null)
        {
            // Fallback: any active session for this counter
            activeSession = sessions.FirstOrDefault(s => s.Status == 1);
        }

        if (activeSession == null) return null;

        return new SaleEntryPOSSessionDto
        {
            Id = activeSession.POSSessionId,
            SessionNumber = activeSession.SessionNumber,
            Status = activeSession.Status,
            OpeningCash = activeSession.OpeningCash,
            OpenedAt = activeSession.OpenedAt,
            OpenedBy = activeSession.OperatorNameSnapshot ?? "Unknown"
        };
    }
}