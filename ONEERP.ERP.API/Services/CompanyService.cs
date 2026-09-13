using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface ICompanyService
{
    Task<PaginatedResult<CompanyDto>> GetPagedAsync(int pageNumber, int pageSize, string search);
    Task<CompanyDto> GetByIdAsync(int companyId);
    Task<string> GetNextCodeAsync();
    Task<CompanyDto> CreateAsync(CreateCompanyRequest request);
    Task<CompanyDto> UpdateAsync(int companyId, UpdateCompanyRequest request);
    Task<bool> DeleteAsync(int companyId);
}

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;
    private readonly IDataScopeResolver _dataScopeResolver;

    public CompanyService(ICompanyRepository companyRepository, IAuditService auditService, ICurrentUser currentUser, IDataScopeResolver dataScopeResolver)
    {
        _companyRepository = companyRepository;
        _auditService = auditService;
        _currentUser = currentUser;
        _dataScopeResolver = dataScopeResolver;
    }

    public async Task<PaginatedResult<CompanyDto>> GetPagedAsync(int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;

        var allowedCompanyIds = await _dataScopeResolver.GetAllowedCompanyIdsAsync();
        if (allowedCompanyIds is null)
        {
            var companies = await _companyRepository.GetPagedAsync(normalizedPage, normalizedSize, search);
            var total = await _companyRepository.CountAsync(search);
            return new PaginatedResult<CompanyDto>
            {
                Items = companies.Select(ToDto).ToList(),
                TotalCount = total,
                PageNumber = normalizedPage,
                PageSize = normalizedSize
            };
        }

        var scoped = (await _companyRepository.GetAllAsync())
            .Where(c => allowedCompanyIds.Contains(c.Id));
        if (!string.IsNullOrWhiteSpace(search))
        {
            scoped = scoped.Where(c =>
                (c.CompanyName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (c.CompanyCode?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (c.ShortName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (c.Abbreviation?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        var scopedList = scoped.OrderByDescending(c => c.Id).ToList();

        return new PaginatedResult<CompanyDto>
        {
            Items = scopedList.Skip((normalizedPage - 1) * normalizedSize).Take(normalizedSize).Select(ToDto).ToList(),
            TotalCount = scopedList.Count,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<CompanyDto> GetByIdAsync(int companyId)
    {
        if (!await _dataScopeResolver.CanAccessCompanyAsync(companyId))
            throw new NotFoundException($"Company '{companyId}' was not found.");
        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException($"Company '{companyId}' was not found.");
        return ToDto(company);
    }

    public async Task<string> GetNextCodeAsync()
        => await _companyRepository.GetNextCodeAsync();

    public async Task<CompanyDto> CreateAsync(CreateCompanyRequest request)
    {
        var companyCode = string.IsNullOrWhiteSpace(request.CompanyCode)
            ? await _companyRepository.GetNextCodeAsync()
            : request.CompanyCode.Trim();

        if (await _companyRepository.CodeInUseAsync(companyCode))
            throw new DomainException($"Company code '{companyCode}' is already in use.");

        var company = new Company
        {
            EntityId = request.EntityId,
            CompanyCode = companyCode,
            CompanyName = request.CompanyName.Trim(),
            ShortName = request.ShortName?.Trim(),
            Abbreviation = request.Abbreviation?.Trim(),
            BusinessTypeId = request.BusinessTypeId,
            IndustryTypeId = request.IndustryTypeId,
            GSTRegistrationTypeId = request.GSTRegistrationTypeId,
            GSTNumber = request.GSTNumber?.Trim(),
            PANNumber = request.PANNumber?.Trim(),
            TANNumber = request.TANNumber?.Trim(),
            CINNumber = request.CINNumber?.Trim(),
            RegistrationNumber = request.RegistrationNumber?.Trim(),
            CurrencyId = request.CurrencyId,
            LanguageId = request.LanguageId,
            TimeZoneId = request.TimeZoneId,
            CompanyGroupId = request.CompanyGroupId,
            BusinessUnitId = request.BusinessUnitId,
            Website = request.Website?.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            Mobile = request.Mobile?.Trim(),
            LogoUrl = request.LogoUrl,
            DefaultFinancialYearId = request.DefaultFinancialYearId,
            MultiBranchEnabled = request.MultiBranchEnabled,
            MultiWarehouseEnabled = request.MultiWarehouseEnabled,
            MultiCurrencyEnabled = request.MultiCurrencyEnabled,
            DateFormat = request.DateFormat,
            TimeFormat = request.TimeFormat,
            NumberFormat = request.NumberFormat,
            DefaultWarehouseId = request.DefaultWarehouseId,
            Theme = request.Theme,
            PrimaryColor = request.PrimaryColor,
            SecondaryColor = request.SecondaryColor,
            Remarks = request.Remarks,
            IsActive = true,
            IsBlocked = false,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        company.Id = await _companyRepository.InsertAsync(company);
        await _auditService.WriteAsync("Company", company.Id.ToString(), "Create", _currentUser.Username);

        return ToDto(company);
    }

    public async Task<CompanyDto> UpdateAsync(int companyId, UpdateCompanyRequest request)
    {
        if (!await _dataScopeResolver.CanAccessCompanyAsync(companyId))
            throw new NotFoundException($"Company '{companyId}' was not found.");
        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException($"Company '{companyId}' was not found.");

        company.CompanyName = request.CompanyName.Trim();
        company.ShortName = request.ShortName?.Trim();
        company.Abbreviation = request.Abbreviation?.Trim();
        company.BusinessTypeId = request.BusinessTypeId;
        company.IndustryTypeId = request.IndustryTypeId;
        company.GSTRegistrationTypeId = request.GSTRegistrationTypeId;
        company.GSTNumber = request.GSTNumber?.Trim();
        company.PANNumber = request.PANNumber?.Trim();
        company.TANNumber = request.TANNumber?.Trim();
        company.CINNumber = request.CINNumber?.Trim();
        company.RegistrationNumber = request.RegistrationNumber?.Trim();
        company.CurrencyId = request.CurrencyId;
        company.LanguageId = request.LanguageId;
        company.TimeZoneId = request.TimeZoneId;
        company.IsActive = request.IsActive;
        company.IsBlocked = request.IsBlocked;
        company.CompanyGroupId = request.CompanyGroupId;
        company.BusinessUnitId = request.BusinessUnitId;
        company.Website = request.Website?.Trim();
        company.Email = request.Email?.Trim();
        company.Phone = request.Phone?.Trim();
        company.Mobile = request.Mobile?.Trim();
        company.LogoUrl = request.LogoUrl;
        company.DefaultFinancialYearId = request.DefaultFinancialYearId;
        company.MultiBranchEnabled = request.MultiBranchEnabled ?? company.MultiBranchEnabled;
        company.MultiWarehouseEnabled = request.MultiWarehouseEnabled ?? company.MultiWarehouseEnabled;
        company.MultiCurrencyEnabled = request.MultiCurrencyEnabled ?? company.MultiCurrencyEnabled;
        company.DateFormat = request.DateFormat;
        company.TimeFormat = request.TimeFormat;
        company.NumberFormat = request.NumberFormat;
        company.DefaultWarehouseId = request.DefaultWarehouseId;
        company.Theme = request.Theme;
        company.PrimaryColor = request.PrimaryColor;
        company.SecondaryColor = request.SecondaryColor;
        company.Remarks = request.Remarks;
        company.ModifiedBy = _currentUser.UserId;

        await _companyRepository.UpdateAsync(company);
        await _auditService.WriteAsync("Company", companyId.ToString(), "Update", _currentUser.Username);

        return ToDto(company);
    }

    public async Task<bool> DeleteAsync(int companyId)
    {
        if (!await _dataScopeResolver.CanAccessCompanyAsync(companyId))
            throw new NotFoundException($"Company '{companyId}' was not found.");
        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException($"Company '{companyId}' was not found.");

        await _companyRepository.SoftDeleteAsync(companyId, _currentUser.UserId);
        await _auditService.WriteAsync("Company", companyId.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static CompanyDto ToDto(Company c) => new()
    {
        Id = c.Id,
        EntityId = c.EntityId,
        CompanyCode = c.CompanyCode,
        CompanyName = c.CompanyName,
        ShortName = c.ShortName,
        Abbreviation = c.Abbreviation,
        BusinessTypeId = c.BusinessTypeId,
        IndustryTypeId = c.IndustryTypeId,
        GSTRegistrationTypeId = c.GSTRegistrationTypeId,
        GSTNumber = c.GSTNumber,
        PANNumber = c.PANNumber,
        TANNumber = c.TANNumber,
        CINNumber = c.CINNumber,
        RegistrationNumber = c.RegistrationNumber,
        CurrencyId = c.CurrencyId,
        LanguageId = c.LanguageId,
        TimeZoneId = c.TimeZoneId,
        IsActive = c.IsActive,
        IsBlocked = c.IsBlocked,
        LastLoginDate = c.LastLoginDate,
        CreatedBy = c.CreatedBy,
        CreatedDate = c.CreatedDate,
        ModifiedBy = c.ModifiedBy,
        ModifiedDate = c.ModifiedDate,
        CompanyGroupId = c.CompanyGroupId,
        BusinessUnitId = c.BusinessUnitId,
        Website = c.Website,
        Email = c.Email,
        Phone = c.Phone,
        Mobile = c.Mobile,
        LogoUrl = c.LogoUrl,
        DefaultFinancialYearId = c.DefaultFinancialYearId,
        MultiBranchEnabled = c.MultiBranchEnabled,
        MultiWarehouseEnabled = c.MultiWarehouseEnabled,
        MultiCurrencyEnabled = c.MultiCurrencyEnabled,
        DateFormat = c.DateFormat,
        TimeFormat = c.TimeFormat,
        NumberFormat = c.NumberFormat,
        DefaultWarehouseId = c.DefaultWarehouseId,
        Theme = c.Theme,
        PrimaryColor = c.PrimaryColor,
        SecondaryColor = c.SecondaryColor,
        Remarks = c.Remarks
    };
}
