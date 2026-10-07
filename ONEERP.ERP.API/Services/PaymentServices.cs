using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface IPaymentTypeService
{
    Task<IEnumerable<PaymentTypeDto>> GetAllAsync(bool includeInactive = false);
    Task<PaymentTypeDto> GetByIdAsync(long id);
    Task<PaymentTypeDto> CreateAsync(CreatePaymentTypeRequest request);
    Task<PaymentTypeDto> UpdateAsync(long id, UpdatePaymentTypeRequest request);
    Task<bool> DeleteAsync(long id);
}

public class PaymentTypeService : IPaymentTypeService
{
    private readonly IPaymentTypeRepository _repo;

    public PaymentTypeService(IPaymentTypeRepository repo) => _repo = repo;

    public async Task<IEnumerable<PaymentTypeDto>> GetAllAsync(bool includeInactive = false)
        => (await _repo.GetAllAsync(includeInactive)).Select(Map).ToList();

    public async Task<PaymentTypeDto> GetByIdAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Payment type '{id}' was not found.");
        return Map(e);
    }

    public async Task<PaymentTypeDto> CreateAsync(CreatePaymentTypeRequest r)
    {
        ValidatePaymentType(r.Code, r.Name, r.DisplayOrder);
        var code = r.Code.Trim();
        if (await _repo.GetByCodeAsync(code) is not null)
            throw new DomainException($"A payment type with code '{code}' already exists.");
        var e = new PaymentType { Code = code, Name = r.Name.Trim(), DisplayOrder = r.DisplayOrder, IsActive = r.IsActive };
        e.PaymentTypeId = await _repo.InsertAsync(e);
        return Map(e);
    }

    public async Task<PaymentTypeDto> UpdateAsync(long id, UpdatePaymentTypeRequest r)
    {
        ValidatePaymentType(r.Code, r.Name, r.DisplayOrder);
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Payment type '{id}' was not found.");
        var code = r.Code.Trim();
        var dup = await _repo.GetByCodeAsync(code);
        if (dup is not null && dup.PaymentTypeId != id)
            throw new DomainException($"A payment type with code '{code}' already exists.");
        e.Code = code;
        e.Name = r.Name.Trim();
        e.DisplayOrder = r.DisplayOrder;
        e.IsActive = r.IsActive;
        await _repo.UpdateAsync(e);
        return Map(e);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Payment type '{id}' was not found.");
        return await _repo.DeleteAsync(id);
    }

    private static void ValidatePaymentType(string? code, string? name, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 30)
            throw new DomainException("Payment type code is required and must be at most 30 characters.", 400);
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            throw new DomainException("Payment type name is required and must be at most 100 characters.", 400);
        if (displayOrder < 0)
            throw new DomainException("Display order cannot be negative.", 400);
    }

    private static PaymentTypeDto Map(PaymentType e) => new()
    {
        PaymentTypeId = e.PaymentTypeId,
        Code = e.Code,
        Name = e.Name,
        IsActive = e.IsActive,
        DisplayOrder = e.DisplayOrder,
    };
}

public interface IPaymentMethodService
{
    Task<IEnumerable<PaymentMethodDto>> GetAllAsync(bool includeInactive = false);
    Task<PaymentMethodDto> GetByIdAsync(long id);
    Task<PaymentMethodDto> CreateAsync(CreatePaymentMethodRequest request);
    Task<PaymentMethodDto> UpdateAsync(long id, UpdatePaymentMethodRequest request);
    Task<bool> DeleteAsync(long id);
}

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IPaymentMethodRepository _repo;

    public PaymentMethodService(IPaymentMethodRepository repo) => _repo = repo;

    public async Task<IEnumerable<PaymentMethodDto>> GetAllAsync(bool includeInactive = false)
        => (await _repo.GetAllAsync(includeInactive)).Select(Map).ToList();

    public async Task<PaymentMethodDto> GetByIdAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Payment method '{id}' was not found.");
        return Map(e);
    }

    public async Task<PaymentMethodDto> CreateAsync(CreatePaymentMethodRequest r)
    {
        ValidatePaymentMethod(r.Code, r.Name, r.PaymentCategory, r.DisplayOrder);
        var code = r.Code.Trim();
        if (await _repo.GetByCodeAsync(code) is not null)
            throw new DomainException($"A payment method with code '{code}' already exists.");
        var e = new PaymentMethod
        {
            Code = code,
            Name = r.Name.Trim(),
            PaymentCategory = r.PaymentCategory,
            IsCash = r.IsCash,
            IsCredit = r.IsCredit,
            RequiresReferenceNo = r.RequiresReferenceNo,
            DisplayOrder = r.DisplayOrder,
            IsActive = r.IsActive,
        };
        e.PaymentMethodId = await _repo.InsertAsync(e);
        return Map(e);
    }

    public async Task<PaymentMethodDto> UpdateAsync(long id, UpdatePaymentMethodRequest r)
    {
        ValidatePaymentMethod(r.Code, r.Name, r.PaymentCategory, r.DisplayOrder);
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Payment method '{id}' was not found.");
        var code = r.Code.Trim();
        var dup = await _repo.GetByCodeAsync(code);
        if (dup is not null && dup.PaymentMethodId != id)
            throw new DomainException($"A payment method with code '{code}' already exists.");
        e.Code = code;
        e.Name = r.Name.Trim();
        e.PaymentCategory = r.PaymentCategory;
        e.IsCash = r.IsCash;
        e.IsCredit = r.IsCredit;
        e.RequiresReferenceNo = r.RequiresReferenceNo;
        e.DisplayOrder = r.DisplayOrder;
        e.IsActive = r.IsActive;
        await _repo.UpdateAsync(e);
        return Map(e);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Payment method '{id}' was not found.");
        return await _repo.DeleteAsync(id);
    }

    private static void ValidatePaymentMethod(string? code, string? name, string? category, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 30)
            throw new DomainException("Payment method code is required and must be at most 30 characters.", 400);
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            throw new DomainException("Payment method name is required and must be at most 100 characters.", 400);
        if (string.IsNullOrWhiteSpace(category) || category.Trim().Length > 50)
            throw new DomainException("Payment category is required and must be at most 50 characters.", 400);
        if (displayOrder < 0)
            throw new DomainException("Display order cannot be negative.", 400);
    }

    private static PaymentMethodDto Map(PaymentMethod e) => new()
    {
        PaymentMethodId = e.PaymentMethodId,
        Code = e.Code,
        Name = e.Name,
        PaymentCategory = e.PaymentCategory,
        IsCash = e.IsCash,
        IsCredit = e.IsCredit,
        RequiresReferenceNo = e.RequiresReferenceNo,
        IsActive = e.IsActive,
        DisplayOrder = e.DisplayOrder,
    };
}

public interface IPaymentMethodDetailService
{
    Task<IEnumerable<PaymentMethodDetailDto>> GetByPaymentMethodIdAsync(long paymentMethodId, bool includeInactive = false);
    Task<PaymentMethodDetailDto> GetByIdAsync(long id);
    Task<PaymentMethodDetailDto> CreateAsync(long userId, CreatePaymentMethodDetailRequest request);
    Task<PaymentMethodDetailDto> UpdateAsync(long id, long userId, UpdatePaymentMethodDetailRequest request);
    Task<bool> DeleteAsync(long id);
}

public class PaymentMethodDetailService : IPaymentMethodDetailService
{
    private readonly IPaymentMethodDetailRepository _repo;
    private readonly IPaymentMethodService _paymentMethodService;

    public PaymentMethodDetailService(IPaymentMethodDetailRepository repo, IPaymentMethodService paymentMethodService)
    {
        _repo = repo;
        _paymentMethodService = paymentMethodService;
    }

    public async Task<IEnumerable<PaymentMethodDetailDto>> GetByPaymentMethodIdAsync(long paymentMethodId, bool includeInactive = false)
    {
        var method = await _paymentMethodService.GetByIdAsync(paymentMethodId);
        var items = await _repo.GetByPaymentMethodIdAsync(paymentMethodId, includeInactive);
        return items.Select(e => Map(e, method)).ToList();
    }

    public async Task<PaymentMethodDetailDto> GetByIdAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Payment method detail '{id}' was not found.");
        var method = await _paymentMethodService.GetByIdAsync(e.PaymentMethodId);
        return Map(e, method);
    }

    public async Task<PaymentMethodDetailDto> CreateAsync(long userId, CreatePaymentMethodDetailRequest r)
    {
        var method = await _paymentMethodService.GetByIdAsync(r.PaymentMethodId)
            ?? throw new NotFoundException($"Payment method '{r.PaymentMethodId}' was not found.");

        var code = r.Code?.Trim() ?? string.Empty;
        ValidateMethodDetail(code, r.Name, r.DisplayOrder, r.DisplayName, r.UPIId, r.BankName, r.AccountNumber,
            r.IFSCCode, r.TerminalName, r.CashCounterName, r.ReferenceValue);
        if (await _repo.GetByCodeAsync(r.PaymentMethodId, code) is not null)
            throw new DomainException($"A payment method detail with code '{code}' already exists for this payment method.");

        if (r.IsDefault)
            await _repo.ClearDefaultAsync(r.PaymentMethodId, null);

        var e = new PaymentMethodDetail
        {
            PaymentMethodId = r.PaymentMethodId,
            Code = code,
            Name = r.Name.Trim(),
            DisplayName = r.DisplayName?.Trim(),
            UPIId = r.UPIId?.Trim(),
            BankName = r.BankName?.Trim(),
            AccountNumber = r.AccountNumber?.Trim(),
            IFSCCode = r.IFSCCode?.Trim(),
            TerminalName = r.TerminalName?.Trim(),
            CashCounterName = r.CashCounterName?.Trim(),
            ReferenceValue = r.ReferenceValue?.Trim(),
            IsDefault = r.IsDefault,
            DisplayOrder = r.DisplayOrder,
            IsActive = r.IsActive,
            CreatedByUserId = userId,
        };
        e.PaymentMethodDetailId = await _repo.InsertAsync(e);
        return Map(e, method);
    }

    public async Task<PaymentMethodDetailDto> UpdateAsync(long id, long userId, UpdatePaymentMethodDetailRequest r)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Payment method detail '{id}' was not found.");
        var method = await _paymentMethodService.GetByIdAsync(e.PaymentMethodId);

        var code = r.Code?.Trim() ?? string.Empty;
        ValidateMethodDetail(code, r.Name, r.DisplayOrder, r.DisplayName, r.UPIId, r.BankName, r.AccountNumber,
            r.IFSCCode, r.TerminalName, r.CashCounterName, r.ReferenceValue);
        var dup = await _repo.GetByCodeAsync(e.PaymentMethodId, code);
        if (dup is not null && dup.PaymentMethodDetailId != id)
            throw new DomainException($"A payment method detail with code '{code}' already exists for this payment method.");

        if (r.IsDefault)
            await _repo.ClearDefaultAsync(e.PaymentMethodId, id);

        e.Code = code;
        e.Name = r.Name.Trim();
        e.DisplayName = r.DisplayName?.Trim();
        e.UPIId = r.UPIId?.Trim();
        e.BankName = r.BankName?.Trim();
        e.AccountNumber = r.AccountNumber?.Trim();
        e.IFSCCode = r.IFSCCode?.Trim();
        e.TerminalName = r.TerminalName?.Trim();
        e.CashCounterName = r.CashCounterName?.Trim();
        e.ReferenceValue = r.ReferenceValue?.Trim();
        e.IsDefault = r.IsDefault;
        e.DisplayOrder = r.DisplayOrder;
        e.IsActive = r.IsActive;
        e.UpdatedByUserId = userId;
        await _repo.UpdateAsync(e);
        return Map(e, method);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Payment method detail '{id}' was not found.");
        return await _repo.DeleteAsync(id);
    }

    private static void ValidateMethodDetail(string? code, string? name, int displayOrder, string? displayName,
        string? upiId, string? bankName, string? accountNumber, string? ifscCode, string? terminalName,
        string? cashCounterName, string? referenceValue)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 50)
            throw new DomainException("Payment method detail code is required and must be at most 50 characters.", 400);
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            throw new DomainException("Payment method detail name is required and must be at most 100 characters.", 400);
        if (displayName?.Length > 150 || upiId?.Length > 150 || bankName?.Length > 150 || accountNumber?.Length > 100 ||
            ifscCode?.Length > 20 || terminalName?.Length > 100 || cashCounterName?.Length > 100 || referenceValue?.Length > 200)
            throw new DomainException("A payment method detail field exceeds its maximum length.", 400);
        if (displayOrder < 0)
            throw new DomainException("Display order cannot be negative.", 400);
    }

    private static PaymentMethodDetailDto Map(PaymentMethodDetail e, PaymentMethodDto method) => new()
    {
        PaymentMethodDetailId = e.PaymentMethodDetailId,
        PaymentMethodId = e.PaymentMethodId,
        PaymentMethodCode = method.Code,
        PaymentMethodName = method.Name,
        PaymentCategory = method.PaymentCategory,
        Code = e.Code,
        Name = e.Name,
        DisplayName = e.DisplayName,
        UPIId = e.UPIId,
        BankName = e.BankName,
        AccountNumber = e.AccountNumber,
        IFSCCode = e.IFSCCode,
        TerminalName = e.TerminalName,
        CashCounterName = e.CashCounterName,
        ReferenceValue = e.ReferenceValue,
        IsDefault = e.IsDefault,
        DisplayOrder = e.DisplayOrder,
        IsActive = e.IsActive,
        CreatedByUserId = e.CreatedByUserId,
        CreatedAt = e.CreatedAt,
        UpdatedByUserId = e.UpdatedByUserId,
        UpdatedAt = e.UpdatedAt,
    };
}

public interface IPaymentService
{
    Task<PaginatedResult<PaymentDto>> GetPagedAsync(long companyId, int page, int size, string search);
    Task<PaymentDto?> GetByIdAsync(long companyId, long id);
    Task<string> GetNextPaymentNoAsync(long companyId);
    Task<PaymentDto> CreateAsync(long companyId, long userId, CreatePaymentRequest request);
    Task<PaymentDto> UpdateAsync(long companyId, long id, long userId, UpdatePaymentRequest request);
    Task DeleteAsync(long companyId, long id);
    Task<PaymentLookupsDto> GetLookupsAsync(long companyId);
}

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repo;
    private readonly IPaymentTypeService _paymentTypeService;
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly IBusinessPartnerService _businessPartnerService;

    public PaymentService(
        IPaymentRepository repo,
        IPaymentTypeService paymentTypeService,
        IPaymentMethodService paymentMethodService,
        IBusinessPartnerService businessPartnerService)
    {
        _repo = repo;
        _paymentTypeService = paymentTypeService;
        _paymentMethodService = paymentMethodService;
        _businessPartnerService = businessPartnerService;
    }

    private static PaymentDto Map(Payment e) => new()
    {
        PaymentId = e.PaymentId,
        CompanyId = e.CompanyId,
        PaymentNo = e.PaymentNo,
        PaymentDate = e.PaymentDate,
        PaymentTypeID = e.PaymentTypeID,
        PaymentMethodID = e.PaymentMethodID,
        ReferenceType = e.ReferenceType,
        ReferenceId = e.ReferenceId,
        BusinessPartnerId = e.BusinessPartnerId,
        Amount = e.Amount,
        ReferenceNo = e.ReferenceNo,
        Remarks = e.Remarks,
        StatusID = e.StatusID,
        CreatedByUserID = e.CreatedByUserID,
        CreatedAt = e.CreatedAt,
        UpdatedByUserID = e.UpdatedByUserID,
        UpdatedAt = e.UpdatedAt,
    };

    public async Task<PaginatedResult<PaymentDto>> GetPagedAsync(long companyId, int page, int size, string search)
    {
        var (items, total) = await _repo.GetPagedAsync(companyId, page, size, search);
        return new PaginatedResult<PaymentDto>
        {
            Items = items.Select(Map).ToList(),
            TotalCount = total,
            PageNumber = page,
            PageSize = size,
        };
    }

    public async Task<PaymentDto?> GetByIdAsync(long companyId, long id)
    {
        var e = await _repo.GetByIdAsync(companyId, id);
        return e == null ? null : Map(e);
    }

    public Task<string> GetNextPaymentNoAsync(long companyId) => _repo.GetNextPaymentNoAsync(companyId);

    public async Task<PaymentDto> CreateAsync(long companyId, long userId, CreatePaymentRequest r)
    {
        Validate(r.PaymentNo, r.PaymentDate, r.PaymentTypeID, r.PaymentMethodID, r.ReferenceType, r.ReferenceId, r.Amount);
        var e = new Payment
        {
            CompanyId = companyId,
            PaymentNo = r.PaymentNo?.Trim() ?? string.Empty,
            PaymentDate = DateTime.Parse(r.PaymentDate),
            PaymentTypeID = r.PaymentTypeID,
            PaymentMethodID = r.PaymentMethodID,
            ReferenceType = r.ReferenceType?.Trim() ?? string.Empty,
            ReferenceId = r.ReferenceId,
            BusinessPartnerId = r.BusinessPartnerId,
            Amount = r.Amount,
            ReferenceNo = r.ReferenceNo?.Trim(),
            Remarks = r.Remarks?.Trim(),
            StatusID = 4,
            CreatedByUserID = userId,
        };
        e.PaymentId = await _repo.InsertAsync(e);
        return Map(e);
    }

    public async Task<PaymentDto> UpdateAsync(long companyId, long id, long userId, UpdatePaymentRequest r)
    {
        Validate(r.PaymentNo, r.PaymentDate, r.PaymentTypeID, r.PaymentMethodID, r.ReferenceType, r.ReferenceId, r.Amount);
        var e = await _repo.GetByIdAsync(companyId, id) ?? throw new NotFoundException($"Payment '{id}' was not found.");
        e.PaymentNo = r.PaymentNo?.Trim() ?? e.PaymentNo;
        e.PaymentDate = DateTime.Parse(r.PaymentDate);
        e.PaymentTypeID = r.PaymentTypeID;
        e.PaymentMethodID = r.PaymentMethodID;
        e.ReferenceType = r.ReferenceType?.Trim() ?? e.ReferenceType;
        e.ReferenceId = r.ReferenceId;
        e.BusinessPartnerId = r.BusinessPartnerId;
        e.Amount = r.Amount;
        e.ReferenceNo = r.ReferenceNo?.Trim();
        e.Remarks = r.Remarks?.Trim();
        e.StatusID = r.StatusID;
        e.UpdatedByUserID = userId;
        await _repo.UpdateAsync(e);
        return Map(e);
    }

    public async Task DeleteAsync(long companyId, long id)
    {
        var e = await _repo.GetByIdAsync(companyId, id) ?? throw new NotFoundException($"Payment '{id}' was not found.");
        await _repo.DeleteAsync(companyId, id);
    }

    private static void Validate(string? paymentNo, string? paymentDate, long paymentTypeId, long paymentMethodId,
        string? referenceType, long referenceId, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(paymentNo) || paymentNo.Trim().Length > 30)
            throw new DomainException("Payment number is required and must be at most 30 characters.", 400);
        if (!DateTime.TryParse(paymentDate, out _))
            throw new DomainException("Payment date is invalid.", 400);
        if (paymentTypeId <= 0 || paymentMethodId <= 0)
            throw new DomainException("Payment type and method are required.", 400);
        if (string.IsNullOrWhiteSpace(referenceType) || referenceType.Trim().Length > 30 || referenceId <= 0)
            throw new DomainException("A valid reference type and reference ID are required.", 400);
        if (amount <= 0)
            throw new DomainException("Payment amount must be greater than zero.", 400);
    }

    public async Task<PaymentLookupsDto> GetLookupsAsync(long companyId)
    {
        var dto = new PaymentLookupsDto();
        var types = await _paymentTypeService.GetAllAsync(true);
        dto.PaymentTypes = types.Select(t => new LookupItem { Id = t.PaymentTypeId, Code = t.Code, Name = t.Name }).ToList();
        var methods = await _paymentMethodService.GetAllAsync(true);
        dto.PaymentMethods = methods.Select(m => new LookupItem { Id = m.PaymentMethodId, Code = m.Code, Name = m.Name }).ToList();
        var partners = await _businessPartnerService.GetAllAsync();
        dto.BusinessPartners = partners.Select(p => new LookupItem { Id = p.Id, Code = p.PartnerCode, Name = p.PartnerName }).ToList();
        return dto;
    }
}
