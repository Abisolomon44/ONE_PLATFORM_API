using ONEERP.ERP.API.Data;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
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

    public AuthService(
        ITenantConnectionResolver resolver,
        TenantAccessor accessor,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICompanyRepository companyRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IAuditService auditService,
        IConfiguration configuration)
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
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        username = username.Trim();

        // 1. Resolve the owning tenant from the globally unique username
        var tenantCode = await _resolver.ResolveTenantCodeByUsernameAsync(username)
            ?? throw new UnauthorizedAccess("Invalid username or password.");

        var tenant = await _resolver.ResolveTenantAsync(tenantCode);
        _accessor.TenantCode = tenant.TenantCode;
        _accessor.ConnectionString = tenant.ConnectionString;

        // 2. Validate user credentials inside the tenant database
        var user = await _userRepository.GetByUsernameAsync(username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccess("Invalid username or password.");

        if (user.Status != ONEERP.Shared.Constants.EntityStatus.Active)
            throw new UnauthorizedAccess("User account is not active.");

        // 3. Load roles, permissions and company
        var roles = await _roleRepository.GetRolesForUserAsync(user.UserId);
        var permissions = await _roleRepository.GetPermissionsForUserAsync(user.UserId);
        var company = await _companyRepository.GetByIdAsync(user.CompanyId)
            ?? throw new DomainException("Company not found for this user.");

        await _userRepository.UpdateLastLoginAsync(user.UserId);
        await _auditService.WriteAsync("User", user.UserId.ToString(), "Login", user.Username);

        return await BuildLoginResponseAsync(tenant, user, company, roles, permissions);
    }

    public async Task<LoginResponse> RefreshAsync(string username, string refreshToken)
    {
        // Resolve the owning tenant from the globally unique username
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

        await _refreshTokenRepository.RevokeAsync(refreshToken);

        var roles = await _roleRepository.GetRolesForUserAsync(user.UserId);
        var permissions = await _roleRepository.GetPermissionsForUserAsync(user.UserId);
        var company = await _companyRepository.GetByIdAsync(user.CompanyId)
            ?? throw new DomainException("Company not found for this user.");

        return await BuildLoginResponseAsync(tenant, user, company, roles, permissions);
    }

    public async Task<bool> LogoutAsync(string refreshToken)
        => await _refreshTokenRepository.RevokeAsync(refreshToken);

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
