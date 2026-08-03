using ONEERP.ERP.API.Data;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Constants;

namespace ONEERP.ERP.API.Services;

public interface IDashboardService
{
    Task<DashboardDto> GetAsync();
}

public class DashboardService : IDashboardService
{
    private readonly ICurrentUser _currentUser;
    private readonly ITenantConnectionResolver _resolver;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public DashboardService(
        ICurrentUser currentUser,
        ITenantConnectionResolver resolver,
        ICompanyRepository companyRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        _currentUser = currentUser;
        _resolver = resolver;
        _companyRepository = companyRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<DashboardDto> GetAsync()
    {
        var user = await _userRepository.GetByIdAsync(_currentUser.UserId);
        var company = await _companyRepository.GetByIdAsync(_currentUser.CompanyId);
        var roles = await _roleRepository.GetRolesForUserAsync(_currentUser.UserId);
        var permissions = await _roleRepository.GetPermissionsForUserAsync(_currentUser.UserId);

        var totalUsers = await _userRepository.CountAsync(string.Empty);
        var rolesList = await _roleRepository.GetAllAsync();
        var recentUsers = await _userRepository.GetPagedAsync(1, 5, string.Empty);

        TenantSession tenant;
        try
        {
            tenant = await _resolver.ResolveTenantAsync(_currentUser.TenantCode);
        }
        catch
        {
            tenant = new TenantSession { TenantCode = _currentUser.TenantCode };
        }

        return new DashboardDto
        {            Company = company is null ? new CompanyDto() : new CompanyDto
            {
                CompanyId = company.CompanyId,
                CompanyCode = company.CompanyCode,
                CompanyName = company.CompanyName,
                Address = company.Address,
                Email = company.Email,
                Phone = company.Phone,
                GST = company.GST,
                Currency = company.Currency,
                Status = company.Status
            },
            User = user is null ? new UserDto() : new UserDto
            {
                UserId = user.UserId,
                CompanyId = user.CompanyId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Mobile = user.Mobile,
                Status = user.Status,
                IsSuperAdmin = user.IsSuperAdmin,
                LastLoginDate = user.LastLoginDate,
                CreatedDate = user.CreatedDate
            },
            Roles = roles.Select(r => r.Name).ToList(),
            Permissions = permissions.ToList(),
            TenantCode = tenant.TenantCode,
            TenantName = tenant.TenantName,
            PlanCode = tenant.PlanCode,
            PlanName = tenant.PlanName,
            SubscriptionEnd = tenant.SubscriptionEnd,
            TotalUsers = totalUsers,
            ActiveUsers = await _userRepository.CountByStatusAsync(EntityStatus.Active),
            TotalRoles = rolesList.Count(),
            RecentUsers = recentUsers.Select(u => new RecentUserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Status = u.Status,
                CreatedDate = u.CreatedDate
            }).ToList()
        };
    }
}
