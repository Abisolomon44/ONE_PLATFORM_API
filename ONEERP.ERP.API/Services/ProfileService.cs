using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Helpers;

namespace ONEERP.ERP.API.Services;

public interface IProfileService
{
    Task<ProfileDto> GetAsync();
    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
}

public class ProfileService : IProfileService
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditService _auditService;

    public ProfileService(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICompanyRepository companyRepository,
        IAuditService auditService)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _companyRepository = companyRepository;
        _auditService = auditService;
    }

    public async Task<ProfileDto> GetAsync()
    {
        var user = await _userRepository.GetByIdAsync(_currentUser.UserId)
            ?? throw new NotFoundException("User not found.");

        var roles = await _roleRepository.GetRolesForUserAsync(user.UserId);
        var permissions = await _roleRepository.GetPermissionsForUserAsync(user.UserId);
        var company = await _companyRepository.GetByIdAsync(user.CompanyId);

        return new ProfileDto
        {
            User = new UserDto
            {
                UserId = user.UserId,
                CompanyId = user.CompanyId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Mobile = user.Mobile,
                Status = user.Status,
                IsSuperAdmin = user.IsSuperAdmin,
                Roles = roles.Select(r => r.Name).ToList()
            },
            Roles = roles.Select(r => r.Name).ToList(),
            Permissions = permissions.ToList(),
            Company = company is null ? new CompanyDto() : new CompanyDto
            {
                Id = company.Id,
                CompanyCode = company.CompanyCode,
                CompanyName = company.CompanyName,
                ShortName = company.ShortName,
                Abbreviation = company.Abbreviation,
                BusinessTypeId = company.BusinessTypeId,
                IndustryTypeId = company.IndustryTypeId,
                GSTRegistrationTypeId = company.GSTRegistrationTypeId,
                GSTNumber = company.GSTNumber,
                PANNumber = company.PANNumber,
                TANNumber = company.TANNumber,
                CINNumber = company.CINNumber,
                RegistrationNumber = company.RegistrationNumber,
                CurrencyId = company.CurrencyId,
                LanguageId = company.LanguageId,
                TimeZoneId = company.TimeZoneId,
                IsActive = company.IsActive,
                IsBlocked = company.IsBlocked,
                LastLoginDate = company.LastLoginDate,
                CreatedBy = company.CreatedBy,
                CreatedDate = company.CreatedDate,
                ModifiedBy = company.ModifiedBy,
                ModifiedDate = company.ModifiedDate
            }
        };
    }

    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(_currentUser.UserId)
            ?? throw new NotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            throw new DomainException("Current password is incorrect.");

        var passwordErrors = PasswordPolicy.Validate(newPassword);
        if (passwordErrors.Count > 0)
            throw new DomainException(passwordErrors[0]);

        var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.UpdatePasswordAsync(user.UserId, newHash);
        await _auditService.WriteAsync("User", user.UserId.ToString(), "ChangePassword", _currentUser.Username);
        return true;
    }
}
