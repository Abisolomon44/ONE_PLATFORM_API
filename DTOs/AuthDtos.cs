namespace ONEERP.Platform.API.DTOs;

public record LoginRequest(string Username, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record ChangePasswordRequest(string OldPassword, string NewPassword);

public class PlatformUserDto
{
    public int PlatformUserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public PlatformUserDto User { get; set; } = new();
}
