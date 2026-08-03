namespace ONEERP.ERP.API.DTOs;

public record CreateRoleRequest(string Name, string Code, string? Description, List<string>? Permissions = null);

public record UpdateRoleRequest(string Name, string? Description, bool IsActive, List<string>? Permissions = null);

public record SetRolePermissionsRequest(List<string> PermissionCodes);

public record CreateCompanyRequest(
    string CompanyCode,
    string CompanyName,
    string? Address,
    string? Email,
    string? Phone,
    string? GST,
    string Currency,
    string Status);

public record UpdateCompanyRequest(
    string CompanyName,
    string? Address,
    string? Email,
    string? Phone,
    string? GST,
    string Currency,
    string Status);

public record UpdateSettingsRequest(Dictionary<string, string> Settings);

public class PermissionDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}
