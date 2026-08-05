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
    Task<CompanyDto> CreateAsync(CreateCompanyRequest request);
    Task<CompanyDto> UpdateAsync(int companyId, UpdateCompanyRequest request);
    Task<bool> DeleteAsync(int companyId);
}

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public CompanyService(ICompanyRepository companyRepository, IAuditService auditService, ICurrentUser currentUser)
    {
        _companyRepository = companyRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<CompanyDto>> GetPagedAsync(int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;

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

    public async Task<CompanyDto> GetByIdAsync(int companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException($"Company '{companyId}' was not found.");
        return ToDto(company);
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyRequest request)
    {
        var companyCode = request.CompanyCode.Trim();

        if (await _companyRepository.CodeInUseAsync(companyCode))
            throw new DomainException($"Company code '{companyCode}' is already in use.");

        var company = new Company
        {
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
        company.ModifiedBy = _currentUser.UserId;

        await _companyRepository.UpdateAsync(company);
        await _auditService.WriteAsync("Company", companyId.ToString(), "Update", _currentUser.Username);

        return ToDto(company);
    }

    public async Task<bool> DeleteAsync(int companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException($"Company '{companyId}' was not found.");

        await _companyRepository.SoftDeleteAsync(companyId, _currentUser.UserId);
        await _auditService.WriteAsync("Company", companyId.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static CompanyDto ToDto(Company c) => new()
    {
        Id = c.Id,
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
        ModifiedDate = c.ModifiedDate
    };
}
