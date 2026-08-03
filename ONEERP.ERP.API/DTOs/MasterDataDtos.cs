namespace ONEERP.ERP.API.DTOs;

/* ---------------- BusinessTypes ---------------- */

public record CreateBusinessTypeRequest(string Name, string? Description, int SortOrder = 1);

public record UpdateBusinessTypeRequest(string Name, string? Description, int SortOrder, bool IsActive);

public class BusinessTypeDto
{
    public int BusinessTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- IndustryTypes ---------------- */

public record CreateIndustryTypeRequest(string Name, string? Description, int SortOrder = 1);

public record UpdateIndustryTypeRequest(string Name, string? Description, int SortOrder, bool IsActive);

public class IndustryTypeDto
{
    public int IndustryTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- CompanyGroups ---------------- */

public record CreateCompanyGroupRequest(
    string GroupCode,
    string GroupName,
    string? ShortName,
    string? Description,
    int? ParentGroupId = null);

public record UpdateCompanyGroupRequest(
    string GroupCode,
    string GroupName,
    string? ShortName,
    string? Description,
    int? ParentGroupId,
    bool IsActive);

public class CompanyGroupDto
{
    public int CompanyGroupId { get; set; }
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public int? ParentGroupId { get; set; }
    public string? ParentGroupName { get; set; }
    public bool IsActive { get; set; }
}
