namespace ONEERP.ERP.API.DTOs;

public record LoginRequest(string Username, string Password);

public record RefreshTokenRequest(string Username, string RefreshToken);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public class CompanyDto
{
    public int CompanyId { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? GST { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
}

public class UserDto
{
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime? LastLoginDate { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class RoleDto
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public UserDto User { get; set; } = new();
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public CompanyDto Company { get; set; } = new();
    public string TenantCode { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
}
