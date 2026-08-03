namespace ONEERP.ERP.API.DTOs;

public record CreateUserRequest(
    string Username,
    string FullName,
    string Email,
    string? Mobile,
    string Password,
    List<int> RoleIds,
    string Status = "Active");

public record UpdateUserRequest(
    string FullName,
    string Email,
    string? Mobile,
    List<int> RoleIds,
    string Status);

public class UserWithRolesDto
{
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<int> RoleIds { get; set; } = new();
    public List<string> RoleNames { get; set; } = new();
}
