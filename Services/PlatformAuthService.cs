using ONEERP.Platform.API.DTOs;
using ONEERP.Platform.API.Models;
using ONEERP.Platform.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.Platform.API.Services;

public interface IPlatformAuthService
{
    Task<LoginResponse> LoginAsync(string username, string password);
    Task<LoginResponse> RefreshAsync(string refreshToken);
    Task<bool> LogoutAsync(string refreshToken);
}

public class PlatformAuthService : IPlatformAuthService
{
    private readonly IPlatformUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly IConfiguration _configuration;

    public PlatformAuthService(
        IPlatformUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IAuditService auditService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _auditService = auditService;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username.Trim());

        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccess("Invalid username or password.");

        await _userRepository.UpdateLastLoginAsync(user.PlatformUserId);
        await _auditService.WriteAsync("PlatformUser", user.PlatformUserId.ToString(), "Login", user.Username);

        return await BuildLoginResponseAsync(user);
    }

    public async Task<LoginResponse> RefreshAsync(string refreshToken)
    {
        var stored = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (stored is null || stored.IsRevoked || stored.ExpiryDate <= DateTime.UtcNow)
            throw new UnauthorizedAccess("Invalid or expired refresh token.");

        var user = await _userRepository.GetByIdAsync(stored.PlatformUserId);

        if (user is null || !user.IsActive)
            throw new UnauthorizedAccess("User account is no longer active.");

        await _refreshTokenRepository.RevokeAsync(refreshToken);
        return await BuildLoginResponseAsync(user);
    }

    public async Task<bool> LogoutAsync(string refreshToken)
        => await _refreshTokenRepository.RevokeAsync(refreshToken);

    private async Task<LoginResponse> BuildLoginResponseAsync(PlatformUser user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshDays = int.Parse(_configuration["Jwt:RefreshTokenDays"] ?? "7");

        await _refreshTokenRepository.InsertAsync(new RefreshToken
        {
            PlatformUserId = user.PlatformUserId,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(refreshDays)
        });

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = int.Parse(_configuration["Jwt:AccessTokenMinutes"] ?? "15") * 60,
            User = new PlatformUserDto
            {
                PlatformUserId = user.PlatformUserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            }
        };
    }
}
