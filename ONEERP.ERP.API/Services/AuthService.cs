using ONEERP.ERP.API.Data;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.ERP.API.Security;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(string username, string password);
    Task<LoginResponse> RefreshAsync(string username, string refreshToken);
    Task<bool> LogoutAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly ITenantConnectionResolver _resolver;
    private readonly TenantAccessor _accessor;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly IConfiguration _configuration;
    private readonly IRolePermissionEntryRepository _rolePermRepo;
    private readonly IWorkspaceRepository _wsRepo;
    private readonly IDomainRepository _domRepo;
    private readonly IModuleRepository _modRepo;
    private readonly ISubModuleRepository _subModRepo;
    private readonly IScreenRepository _scrRepo;
    private readonly IActionRepository _actRepo;
    private readonly IUserPermissionOverrideRepository _userPermOverrideRepo;
    private readonly IDataScopeRepository _dataScopeRepo;
    private readonly IUserDataScopeOverrideRepository _userDataScopeOverrideRepo;

    public AuthService(
        ITenantConnectionResolver resolver,
        TenantAccessor accessor,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICompanyRepository companyRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IAuditService auditService,
        IConfiguration configuration,
        IRolePermissionEntryRepository rolePermRepo,
        IWorkspaceRepository wsRepo,
        IDomainRepository domRepo,
        IModuleRepository modRepo,
        ISubModuleRepository subModRepo,
        IScreenRepository scrRepo,
        IActionRepository actRepo,
        IUserPermissionOverrideRepository userPermOverrideRepo,
        IDataScopeRepository dataScopeRepo,
        IUserDataScopeOverrideRepository userDataScopeOverrideRepo)
    {
        _resolver = resolver;
        _accessor = accessor;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _companyRepository = companyRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _auditService = auditService;
        _configuration = configuration;
        _rolePermRepo = rolePermRepo;
        _wsRepo = wsRepo;
        _domRepo = domRepo;
        _modRepo = modRepo;
        _subModRepo = subModRepo;
        _scrRepo = scrRepo;
        _actRepo = actRepo;
        _userPermOverrideRepo = userPermOverrideRepo;
        _dataScopeRepo = dataScopeRepo;
        _userDataScopeOverrideRepo = userDataScopeOverrideRepo;
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        username = username.Trim();

        var tenantCode = await _resolver.ResolveTenantCodeByUsernameAsync(username)
            ?? throw new UnauthorizedAccess("Invalid username or password.");

        var tenant = await _resolver.ResolveTenantAsync(tenantCode);
        _accessor.TenantCode = tenant.TenantCode;
        _accessor.ConnectionString = tenant.ConnectionString;

        var user = await _userRepository.GetByUsernameAsync(username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccess("Invalid username or password.");

        if (user.Status != ONEERP.Shared.Constants.EntityStatus.Active)
            throw new UnauthorizedAccess("User account is not active.");

        var roles = await _roleRepository.GetRolesForUserAsync(user.UserId);
        var permissions = await ResolveAllPermissionsAsync(user.UserId);
        var company = await _companyRepository.GetByIdAsync(user.CompanyId)
            ?? throw new DomainException("Company not found for this user.");

        await _userRepository.UpdateLastLoginAsync(user.UserId);
        await _auditService.WriteAsync("User", user.UserId.ToString(), "Login", user.Username);

        return await BuildLoginResponseAsync(tenant, user, company, roles, permissions);
    }

    public async Task<LoginResponse> RefreshAsync(string username, string refreshToken)
    {
        var tenantCode = await _resolver.ResolveTenantCodeByUsernameAsync(username.Trim())
            ?? throw new UnauthorizedAccess("Invalid or expired refresh token.");

        var tenant = await _resolver.ResolveTenantAsync(tenantCode);
        _accessor.TenantCode = tenant.TenantCode;
        _accessor.ConnectionString = tenant.ConnectionString;

        var stored = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (stored is null || stored.IsRevoked || stored.ExpiryDate <= DateTime.UtcNow)
            throw new UnauthorizedAccess("Invalid or expired refresh token.");

        var user = await _userRepository.GetByIdAsync(stored.UserId);

        if (user is null || user.Status != ONEERP.Shared.Constants.EntityStatus.Active)
            throw new UnauthorizedAccess("User account is no longer active.");

        var refreshDays = int.Parse(_configuration["Jwt:RefreshTokenDays"] ?? "7");
        await _refreshTokenRepository.ExtendExpiryAsync(refreshToken, DateTime.UtcNow.AddDays(refreshDays));

        var roles = await _roleRepository.GetRolesForUserAsync(user.UserId);
        var permissions = await ResolveAllPermissionsAsync(user.UserId);
        var company = await _companyRepository.GetByIdAsync(user.CompanyId)
            ?? throw new DomainException("Company not found for this user.");

        var accessToken = _tokenService.GenerateAccessToken(user, tenant, roles.ToList(), permissions.ToList());

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = int.Parse(_configuration["Jwt:AccessTokenMinutes"] ?? "15") * 60,
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
            Company = ToCompanyDto(company),
            TenantCode = tenant.TenantCode,
            TenantName = tenant.TenantName
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken)
        => await _refreshTokenRepository.RevokeAsync(refreshToken);

    private async Task<IEnumerable<string>> ResolveAllPermissionsAsync(int userId)
    {
        var legacyPermissions = await _roleRepository.GetPermissionsForUserAsync(userId);
        var hierarchicalPermissions = await ResolveHierarchicalPermissionsAsync(userId);
        return legacyPermissions.Concat(hierarchicalPermissions).Distinct().ToList();
    }

    private async Task<IEnumerable<string>> ResolveHierarchicalPermissionsAsync(int userId)
    {
        var roleIds = await _roleRepository.GetRoleIdsForUserAsync(userId);
        var wsNames = (await _wsRepo.GetAllAsync(true)).ToDictionary(w => w.Id, w => w.WorkspaceCode);
        var domNames = (await _domRepo.GetAllAsync(true)).ToDictionary(d => d.Id, d => d.DomainCode);
        var modNames = (await _modRepo.GetAllAsync(true)).ToDictionary(m => m.Id, m => m.ModuleCode);
        var subModNames = (await _subModRepo.GetAllAsync(true)).ToDictionary(s => s.Id, s => s.SubModuleCode);
        var scrNames = (await _scrRepo.GetAllAsync(true)).ToDictionary(s => s.Id, s => s.ScreenCode);
        var actNames = (await _actRepo.GetAllAsync(true)).ToDictionary(a => a.Id, a => a.ActionCode);

        var permCodes = new List<string>();

        foreach (var roleId in roleIds)
        {
            var rolePerms = await _rolePermRepo.GetByRoleAsync(roleId);
            foreach (var rp in rolePerms.Where(p => p.Allow && p.IsActive))
            {
                if (wsNames.TryGetValue(rp.WorkspaceId, out var wc) &&
                    domNames.TryGetValue(rp.DomainId, out var dc) &&
                    modNames.TryGetValue(rp.ModuleId, out var mc) &&
                    subModNames.TryGetValue(rp.SubModuleId, out var smc) &&
                    scrNames.TryGetValue(rp.ScreenId, out var sc) &&
                    actNames.TryGetValue(rp.ActionId, out var ac))
                {
                    permCodes.Add($"{wc}.{dc}.{mc}.{smc}.{sc}.{ac}");
                    permCodes.Add($"{mc}.{smc}.{ac}");
                }
            }
        }

        var overrides = await _userPermOverrideRepo.GetByUserAsync(userId);
        foreach (var o in overrides.Where(o => o.IsActive && o.EffectiveFrom <= DateTime.UtcNow && (o.EffectiveTo is null || o.EffectiveTo > DateTime.UtcNow)))
        {
            if (wsNames.TryGetValue(o.WorkspaceId, out var wc) &&
                domNames.TryGetValue(o.DomainId, out var dc) &&
                modNames.TryGetValue(o.ModuleId, out var mc) &&
                subModNames.TryGetValue(o.SubModuleId, out var smc) &&
                scrNames.TryGetValue(o.ScreenId, out var sc) &&
                actNames.TryGetValue(o.ActionId, out var ac))
            {
                if (o.Allow)
                {
                    permCodes.Add($"{wc}.{dc}.{mc}.{smc}.{sc}.{ac}");
                    permCodes.Add($"{mc}.{smc}.{ac}");
                }
                else
                {
                    permCodes.Remove($"{wc}.{dc}.{mc}.{smc}.{sc}.{ac}");
                    permCodes.Remove($"{mc}.{smc}.{ac}");
                }
            }
        }

        return permCodes.Distinct();
    }

    /// <summary>
    /// Resolves data scopes for all roles assigned to a user.
    /// Returns combined RoleDataScopeEntry list with user overrides applied.
    /// </summary>
    public async Task<(List<RoleDataScopeEntry> DataScopes, List<UserDataScopeOverrideEntry> UserOverrides)> ResolveDataScopesAsync(int userId)
    {
        var roleIds = await _roleRepository.GetRoleIdsForUserAsync(userId);
        var allDataScopes = new List<RoleDataScopeEntry>();

        foreach (var roleId in roleIds)
        {
            var scopes = await _dataScopeRepo.GetByRoleAsync(roleId);
            allDataScopes.AddRange(scopes.Select(s => new RoleDataScopeEntry
            {
                RoleId = s.RoleId,
                ModuleId = s.ModuleId,
                ScreenId = s.ScreenId,
                BranchId = s.BranchId,
                DepartmentId = s.DepartmentId,
                WarehouseId = s.WarehouseId,
                CanView = s.CanView,
                CanCreate = s.CanCreate,
                CanEdit = s.CanEdit,
                CanDelete = s.CanDelete
            }));
        }

        var userOverrides = await _userDataScopeOverrideRepo.GetByUserAsync(userId);
        var userOverrideEntries = userOverrides
            .Where(o => o.IsActive && o.EffectiveFrom <= DateTime.UtcNow && (o.EffectiveTo is null || o.EffectiveTo > DateTime.UtcNow))
            .Select(o => new UserDataScopeOverrideEntry
            {
                UserId = o.UserId,
                ModuleId = o.ModuleId,
                ScreenId = o.ScreenId,
                ScopeType = o.ScopeType,
                ScopeValue = o.ScopeValue,
                PermissionType = o.PermissionType,
                Allow = o.Allow,
                EffectiveFrom = o.EffectiveFrom,
                EffectiveTo = o.EffectiveTo
            }).ToList();

        return (allDataScopes, userOverrideEntries);
    }

    private async Task<LoginResponse> BuildLoginResponseAsync(
        TenantSession tenant,
        User user,
        Company company,
        IEnumerable<Role> roles,
        IEnumerable<string> permissions)
    {
        var roleList = roles.ToList();
        var permissionList = permissions.ToList();

        var accessToken = _tokenService.GenerateAccessToken(user, tenant, roleList, permissionList);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshDays = int.Parse(_configuration["Jwt:RefreshTokenDays"] ?? "7");

        await _refreshTokenRepository.InsertAsync(new RefreshToken
        {
            UserId = user.UserId,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(refreshDays)
        });

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = int.Parse(_configuration["Jwt:AccessTokenMinutes"] ?? "15") * 60,
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
                Roles = roleList.Select(r => r.Name).ToList()
            },
            Roles = roleList.Select(r => r.Name).ToList(),
            Permissions = permissionList,
            Company = ToCompanyDto(company),
            TenantCode = tenant.TenantCode,
            TenantName = tenant.TenantName
        };
    }

    private static CompanyDto ToCompanyDto(Company c) => new()
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
