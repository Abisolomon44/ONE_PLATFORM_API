namespace ONEERP.ERP.API.DTOs;

public record CreateRoleRequest(string Name, string Code, string? Description, List<string>? Permissions = null);

public record UpdateRoleRequest(string Name, string? Description, bool IsActive, List<string>? Permissions = null);

public record SetRolePermissionsRequest(List<string> PermissionCodes);

public record CreateCompanyRequest(
    string CompanyCode,
    string CompanyName,
    string? ShortName,
    string? Abbreviation,
    int BusinessTypeId,
    int IndustryTypeId,
    int? GSTRegistrationTypeId,
    string? GSTNumber,
    string? PANNumber,
    string? TANNumber,
    string? CINNumber,
    string? RegistrationNumber,
    int CurrencyId,
    int LanguageId,
    int TimeZoneId);

public record UpdateCompanyRequest(
    string CompanyName,
    string? ShortName,
    string? Abbreviation,
    int BusinessTypeId,
    int IndustryTypeId,
    int? GSTRegistrationTypeId,
    string? GSTNumber,
    string? PANNumber,
    string? TANNumber,
    string? CINNumber,
    string? RegistrationNumber,
    int CurrencyId,
    int LanguageId,
    int TimeZoneId,
    bool IsActive,
    bool IsBlocked);

public record UpdateSettingsRequest(Dictionary<string, string> Settings);

public class PermissionDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}
