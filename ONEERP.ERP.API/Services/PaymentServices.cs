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
        var code = r.Code.Trim();
        if (await _repo.GetByCodeAsync(code) is not null)
            throw new DomainException($"A payment type with code '{code}' already exists.");
        var e = new PaymentType { Code = code, Name = r.Name.Trim(), DisplayOrder = r.DisplayOrder, IsActive = r.IsActive };
        e.PaymentTypeId = await _repo.InsertAsync(e);
        return Map(e);
    }

    public async Task<PaymentTypeDto> UpdateAsync(long id, UpdatePaymentTypeRequest r)
    {
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

public interface IPaymentService
{
    Task<PaginatedResult<PaymentDto>> GetPagedAsync(long companyId, int page, int size, string search);
    Task<PaymentDto?> GetByIdAsync(long id);
    Task<string> GetNextPaymentNoAsync(long companyId);
    Task<PaymentDto> CreateAsync(long companyId, long userId, CreatePaymentRequest request);
    Task<PaymentDto> UpdateAsync(long id, long userId, UpdatePaymentRequest request);
    Task DeleteAsync(long id);
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

    public async Task<PaymentDto?> GetByIdAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e == null ? null : Map(e);
    }

    public Task<string> GetNextPaymentNoAsync(long companyId) => _repo.GetNextPaymentNoAsync(companyId);

    public async Task<PaymentDto> CreateAsync(long companyId, long userId, CreatePaymentRequest r)
    {
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

    public async Task<PaymentDto> UpdateAsync(long id, long userId, UpdatePaymentRequest r)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Payment '{id}' was not found.");
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

    public async Task DeleteAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Payment '{id}' was not found.");
        await _repo.DeleteAsync(id);
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
