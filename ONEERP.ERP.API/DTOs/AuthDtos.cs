namespace ONEERP.ERP.API.DTOs;

public record LoginRequest(string Username, string Password);

public record RefreshTokenRequest(string Username, string RefreshToken);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public class CompanyDto
{
    public int Id { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Abbreviation { get; set; }
    public int BusinessTypeId { get; set; }
    public int IndustryTypeId { get; set; }
    public int? GSTRegistrationTypeId { get; set; }
    public string? GSTNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? TANNumber { get; set; }
    public string? CINNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public int CurrencyId { get; set; }
    public int LanguageId { get; set; }
    public int TimeZoneId { get; set; }
    public bool IsActive { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
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
