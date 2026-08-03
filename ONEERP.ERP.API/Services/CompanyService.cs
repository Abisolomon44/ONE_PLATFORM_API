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
    Task<CompanyDto> CreateAsync(CreateCompanyRequest request, string currentUser);
    Task<CompanyDto> UpdateAsync(int companyId, UpdateCompanyRequest request, string currentUser);
    Task<bool> DeleteAsync(int companyId, string currentUser);
}

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditService _auditService;

    public CompanyService(ICompanyRepository companyRepository, IAuditService auditService)
    {
        _companyRepository = companyRepository;
        _auditService = auditService;
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

    public async Task<CompanyDto> CreateAsync(CreateCompanyRequest request, string currentUser)
    {
        var companyCode = request.CompanyCode.Trim();

        if (await _companyRepository.CodeInUseAsync(companyCode))
            throw new DomainException($"Company code '{companyCode}' is already in use.");

        var company = new Company
        {
            CompanyCode = companyCode,
            CompanyName = request.CompanyName.Trim(),
            Address = request.Address,
            Email = request.Email,
            Phone = request.Phone,
            GST = request.GST,
            Currency = request.Currency,
            Status = request.Status,
            CreatedBy = currentUser,
            ModifiedBy = currentUser
        };

        company.CompanyId = await _companyRepository.InsertAsync(company);
        await _auditService.WriteAsync("Company", company.CompanyId.ToString(), "Create", currentUser);

        return ToDto(company);
    }

    public async Task<CompanyDto> UpdateAsync(int companyId, UpdateCompanyRequest request, string currentUser)
    {
        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException($"Company '{companyId}' was not found.");

        company.CompanyName = request.CompanyName.Trim();
        company.Address = request.Address;
        company.Email = request.Email;
        company.Phone = request.Phone;
        company.GST = request.GST;
        company.Currency = request.Currency;
        company.Status = request.Status;
        company.ModifiedBy = currentUser;

        await _companyRepository.UpdateAsync(company);
        await _auditService.WriteAsync("Company", companyId.ToString(), "Update", currentUser);

        return ToDto(company);
    }

    public async Task<bool> DeleteAsync(int companyId, string currentUser)
    {
        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException($"Company '{companyId}' was not found.");

        await _companyRepository.SoftDeleteAsync(companyId, currentUser);
        await _auditService.WriteAsync("Company", companyId.ToString(), "Delete", currentUser);
        return true;
    }

    private static CompanyDto ToDto(Company c) => new()
    {
        CompanyId = c.CompanyId,
        CompanyCode = c.CompanyCode,
        CompanyName = c.CompanyName,
        Address = c.Address,
        Email = c.Email,
        Phone = c.Phone,
        GST = c.GST,
        Currency = c.Currency,
        Status = c.Status,
        CreatedBy = c.CreatedBy,
        CreatedDate = c.CreatedDate,
        ModifiedBy = c.ModifiedBy,
        ModifiedDate = c.ModifiedDate
    };
}
