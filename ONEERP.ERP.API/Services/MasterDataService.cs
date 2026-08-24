using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface IBusinessTypeService
{
    Task<IEnumerable<BusinessTypeDto>> GetAllAsync(bool includeInactive = false);
    Task<BusinessTypeDto> GetByIdAsync(int id);
    Task<BusinessTypeDto> CreateAsync(CreateBusinessTypeRequest request);
    Task<BusinessTypeDto> UpdateAsync(int id, UpdateBusinessTypeRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IIndustryTypeService
{
    Task<IEnumerable<IndustryTypeDto>> GetAllAsync(bool includeInactive = false);
    Task<IndustryTypeDto> GetByIdAsync(int id);
    Task<IndustryTypeDto> CreateAsync(CreateIndustryTypeRequest request);
    Task<IndustryTypeDto> UpdateAsync(int id, UpdateIndustryTypeRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface ICompanyGroupService
{
    Task<IEnumerable<CompanyGroupDto>> GetAllAsync(bool includeInactive = false);
    Task<CompanyGroupDto> GetByIdAsync(int id);
    Task<CompanyGroupDto> CreateAsync(CreateCompanyGroupRequest request);
    Task<CompanyGroupDto> UpdateAsync(int id, UpdateCompanyGroupRequest request);
    Task<bool> DeleteAsync(int id);
}

public class BusinessTypeService : IBusinessTypeService
{
    private readonly IBusinessTypeRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public BusinessTypeService(IBusinessTypeRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<BusinessTypeDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(Map).ToList();
    }

    public async Task<BusinessTypeDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Business type '{id}' was not found.");
        return Map(item);
    }

    public async Task<BusinessTypeDto> CreateAsync(CreateBusinessTypeRequest request)
    {
        var name = request.Name.Trim();
        if (await _repository.GetByNameAsync(name) is not null)
            throw new DomainException($"A business type with name '{name}' already exists.");

        var entity = new BusinessType
        {
            Name = name,
            Description = request.Description,
            SortOrder = request.SortOrder,
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        entity.BusinessTypeId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("BusinessType", entity.BusinessTypeId.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<BusinessTypeDto> UpdateAsync(int id, UpdateBusinessTypeRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Business type '{id}' was not found.");

        var name = request.Name.Trim();
        var duplicate = await _repository.GetByNameAsync(name);
        if (duplicate is not null && duplicate.BusinessTypeId != id)
            throw new DomainException($"A business type with name '{name}' already exists.");

        entity.Name = name;
        entity.Description = request.Description;
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("BusinessType", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Business type '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("BusinessType", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static BusinessTypeDto Map(BusinessType entity) => new()
    {
        BusinessTypeId = entity.BusinessTypeId,
        Name = entity.Name,
        Description = entity.Description,
        SortOrder = entity.SortOrder,
        IsActive = entity.IsActive
    };
}

public interface IBusinessPartnerRoleService
{
    Task<IEnumerable<BusinessPartnerRoleDto>> GetAllAsync(bool includeInactive = false);
    Task<BusinessPartnerRoleDto> GetByIdAsync(int id);
    Task<BusinessPartnerRoleDto> CreateAsync(CreateBusinessPartnerRoleRequest request);
    Task<BusinessPartnerRoleDto> UpdateAsync(int id, UpdateBusinessPartnerRoleRequest request);
    Task<bool> DeleteAsync(int id);
}

public class BusinessPartnerRoleService : IBusinessPartnerRoleService
{
    private readonly IBusinessPartnerRoleRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public BusinessPartnerRoleService(IBusinessPartnerRoleRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<BusinessPartnerRoleDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(Map);
    }

    public async Task<BusinessPartnerRoleDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Business partner role '{id}' was not found.");
        return Map(item);
    }

    public async Task<BusinessPartnerRoleDto> CreateAsync(CreateBusinessPartnerRoleRequest request)
    {
        var code = request.Code.Trim();
        var name = request.Name.Trim();
        if (await _repository.GetByCodeAsync(code) is not null)
            throw new DomainException($"A business partner role with code '{code}' already exists.");

        var entity = new BusinessPartnerRole
        {
            Code = code,
            Name = name,
            Description = request.Description,
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        entity.BusinessPartnerRoleId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("BusinessPartnerRole", entity.BusinessPartnerRoleId.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<BusinessPartnerRoleDto> UpdateAsync(int id, UpdateBusinessPartnerRoleRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Business partner role '{id}' was not found.");

        var code = request.Code.Trim();
        var name = request.Name.Trim();
        var duplicate = await _repository.GetByCodeAsync(code);
        if (duplicate is not null && duplicate.BusinessPartnerRoleId != id)
            throw new DomainException($"A business partner role with code '{code}' already exists.");

        entity.Code = code;
        entity.Name = name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("BusinessPartnerRole", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Business partner role '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("BusinessPartnerRole", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static BusinessPartnerRoleDto Map(BusinessPartnerRole entity) => new()
    {
        BusinessPartnerRoleId = entity.BusinessPartnerRoleId,
        Code = entity.Code,
        Name = entity.Name,
        Description = entity.Description,
        IsActive = entity.IsActive
    };
}

public interface IBusinessPartnerService
{
    Task<IEnumerable<BusinessPartnerDto>> GetAllAsync(bool includeInactive = false);
    Task<BusinessPartnerDto> GetByIdAsync(long id);
    Task<BusinessPartnerDto> CreateAsync(CreateBusinessPartnerRequest request);
    Task<BusinessPartnerDto> UpdateAsync(long id, UpdateBusinessPartnerRequest request);
    Task<bool> DeleteAsync(long id);
}

public class BusinessPartnerService : IBusinessPartnerService
{
    private readonly IBusinessPartnerRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public BusinessPartnerService(IBusinessPartnerRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<BusinessPartnerDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(_currentUser.CompanyId, includeInactive);
        return items.Select(Map);
    }

    public async Task<BusinessPartnerDto> GetByIdAsync(long id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Business partner '{id}' was not found.");
        return Map(item);
    }

    public async Task<BusinessPartnerDto> CreateAsync(CreateBusinessPartnerRequest request)
    {
        var code = request.PartnerCode.Trim();
        if (await _repository.GetByCodeAsync(_currentUser.CompanyId, code) is not null)
            throw new DomainException($"A business partner with code '{code}' already exists.");

        var entity = new BusinessPartner
        {
            CompanyId = _currentUser.CompanyId,
            PartnerCode = code,
            PartnerName = request.PartnerName.Trim(),
            PatnerRoleIds = request.PatnerRoleIds,
            ContactPerson = request.ContactPerson,
            MobileNo = request.MobileNo,
            Email = request.Email,
            TaxRegistrationNo = request.TaxRegistrationNo,
            CreditLimit = request.CreditLimit,
            CreditDays = request.CreditDays,
            PaymentTermId = request.PaymentTermId,
            CurrencyId = request.CurrencyId,
            PriceListId = request.PriceListId,
            Notes = request.Notes,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.Id = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("BusinessPartner", entity.Id.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<BusinessPartnerDto> UpdateAsync(long id, UpdateBusinessPartnerRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Business partner '{id}' was not found.");

        var code = request.PartnerCode.Trim();
        var duplicate = await _repository.GetByCodeAsync(_currentUser.CompanyId, code);
        if (duplicate is not null && duplicate.Id != id)
            throw new DomainException($"A business partner with code '{code}' already exists.");

        entity.PartnerCode = code;
        entity.PartnerName = request.PartnerName.Trim();
        entity.PatnerRoleIds = request.PatnerRoleIds;
        entity.ContactPerson = request.ContactPerson;
        entity.MobileNo = request.MobileNo;
        entity.Email = request.Email;
        entity.TaxRegistrationNo = request.TaxRegistrationNo;
        entity.CreditLimit = request.CreditLimit;
        entity.CreditDays = request.CreditDays;
        entity.PaymentTermId = request.PaymentTermId;
        entity.CurrencyId = request.CurrencyId;
        entity.PriceListId = request.PriceListId;
        entity.Notes = request.Notes;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("BusinessPartner", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Business partner '{id}' was not found.");

        await _repository.DeleteAsync(id);
        await _auditService.WriteAsync("BusinessPartner", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static BusinessPartnerDto Map(BusinessPartner entity) => new()
    {
        Id = entity.Id,
        CompanyId = entity.CompanyId,
        PartnerCode = entity.PartnerCode,
        PartnerName = entity.PartnerName,
        PatnerRoleIds = entity.PatnerRoleIds,
        ContactPerson = entity.ContactPerson,
        MobileNo = entity.MobileNo,
        Email = entity.Email,
        TaxRegistrationNo = entity.TaxRegistrationNo,
        CreditLimit = entity.CreditLimit,
        CreditDays = entity.CreditDays,
        PaymentTermId = entity.PaymentTermId,
        CurrencyId = entity.CurrencyId,
        PriceListId = entity.PriceListId,
        Notes = entity.Notes,
        IsActive = entity.IsActive,
        CreatedBy = entity.CreatedBy,
        CreatedAt = entity.CreatedAt,
        ModifiedBy = entity.ModifiedBy,
        ModifiedAt = entity.ModifiedAt
    };
}

public class IndustryTypeService : IIndustryTypeService
{
    private readonly IIndustryTypeRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public IndustryTypeService(IIndustryTypeRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<IndustryTypeDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(Map).ToList();
    }

    public async Task<IndustryTypeDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Industry type '{id}' was not found.");
        return Map(item);
    }

    public async Task<IndustryTypeDto> CreateAsync(CreateIndustryTypeRequest request)
    {
        var name = request.Name.Trim();
        if (await _repository.GetByNameAsync(name) is not null)
            throw new DomainException($"An industry type with name '{name}' already exists.");

        var entity = new IndustryType
        {
            Name = name,
            Description = request.Description,
            SortOrder = request.SortOrder,
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        entity.IndustryTypeId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("IndustryType", entity.IndustryTypeId.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<IndustryTypeDto> UpdateAsync(int id, UpdateIndustryTypeRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Industry type '{id}' was not found.");

        var name = request.Name.Trim();
        var duplicate = await _repository.GetByNameAsync(name);
        if (duplicate is not null && duplicate.IndustryTypeId != id)
            throw new DomainException($"An industry type with name '{name}' already exists.");

        entity.Name = name;
        entity.Description = request.Description;
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("IndustryType", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Industry type '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("IndustryType", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static IndustryTypeDto Map(IndustryType entity) => new()
    {
        IndustryTypeId = entity.IndustryTypeId,
        Name = entity.Name,
        Description = entity.Description,
        SortOrder = entity.SortOrder,
        IsActive = entity.IsActive
    };
}

public class CompanyGroupService : ICompanyGroupService
{
    private readonly ICompanyGroupRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public CompanyGroupService(ICompanyGroupRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<CompanyGroupDto>> GetAllAsync(bool includeInactive = false)
    {
        var groups = await _repository.GetAllAsync(includeInactive);
        var parentNames = groups
            .Where(g => g.ParentGroupId is not null)
            .ToDictionary(g => g.CompanyGroupId, g => g.GroupName);
        return groups.Select(g => Map(g, parentNames)).ToList();
    }

    public async Task<CompanyGroupDto> GetByIdAsync(int id)
    {
        var group = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Company group '{id}' was not found.");
        return Map(group);
    }

    public async Task<CompanyGroupDto> CreateAsync(CreateCompanyGroupRequest request)
    {
        var code = request.GroupCode.Trim().ToUpperInvariant();
        if (await _repository.GetByCodeAsync(code) is not null)
            throw new DomainException($"A company group with code '{code}' already exists.");

        if (request.ParentGroupId is int parentId)
        {
            var parent = await _repository.GetByIdAsync(parentId)
                ?? throw new DomainException($"Parent group '{parentId}' was not found.");
            if (!parent.IsActive)
                throw new DomainException($"Parent group '{parent.GroupName}' is inactive.");
        }

        var entity = new CompanyGroup
        {
            GroupCode = code,
            GroupName = request.GroupName.Trim(),
            ShortName = request.ShortName?.Trim(),
            Description = request.Description,
            ParentGroupId = request.ParentGroupId,
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        entity.CompanyGroupId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("CompanyGroup", entity.CompanyGroupId.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<CompanyGroupDto> UpdateAsync(int id, UpdateCompanyGroupRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Company group '{id}' was not found.");

        var code = request.GroupCode.Trim().ToUpperInvariant();
        var duplicate = await _repository.GetByCodeAsync(code);
        if (duplicate is not null && duplicate.CompanyGroupId != id)
            throw new DomainException($"A company group with code '{code}' already exists.");

        if (request.ParentGroupId is int parentId)
        {
            if (parentId == id)
                throw new DomainException("A company group cannot be its own parent.");

            var parent = await _repository.GetByIdAsync(parentId)
                ?? throw new DomainException($"Parent group '{parentId}' was not found.");
            if (!parent.IsActive)
                throw new DomainException($"Parent group '{parent.GroupName}' is inactive.");
        }

        entity.GroupCode = code;
        entity.GroupName = request.GroupName.Trim();
        entity.ShortName = request.ShortName?.Trim();
        entity.Description = request.Description;
        entity.ParentGroupId = request.ParentGroupId;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("CompanyGroup", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Company group '{id}' was not found.");

        if ((await _repository.GetByParentAsync(id)).Any())
            throw new DomainException("Company groups with child groups cannot be deleted.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("CompanyGroup", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static CompanyGroupDto Map(CompanyGroup entity, IReadOnlyDictionary<int, string>? parentNames = null) => new()
    {
        CompanyGroupId = entity.CompanyGroupId,
        GroupCode = entity.GroupCode,
        GroupName = entity.GroupName,
        ShortName = entity.ShortName,
        Description = entity.Description,
        ParentGroupId = entity.ParentGroupId,
        ParentGroupName = entity.ParentGroupId is int pid && parentNames is not null
            ? parentNames.TryGetValue(pid, out var name) ? name : null
            : null,
        IsActive = entity.IsActive
    };
}
