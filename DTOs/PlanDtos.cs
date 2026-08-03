namespace ONEERP.Platform.API.DTOs;

public class PlanDto
{
    public int PlanId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public int MaxUsers { get; set; }
    public int MaxCompanies { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public record CreatePlanRequest(
    string PlanCode,
    string PlanName,
    string? Description,
    string CountryCode,
    string CountryName,
    string CurrencyCode,
    decimal MonthlyPrice,
    decimal AnnualPrice,
    int MaxUsers,
    int MaxCompanies,
    bool IsActive = true);

public record UpdatePlanRequest(
    string PlanCode,
    string PlanName,
    string? Description,
    string CountryCode,
    string CountryName,
    string CurrencyCode,
    decimal MonthlyPrice,
    decimal AnnualPrice,
    int MaxUsers,
    int MaxCompanies,
    bool IsActive);

public static class PlanMapper
{
    public static PlanDto ToDto(Models.Plan p) => new()
    {
        PlanId = p.PlanId,
        PlanCode = p.PlanCode,
        PlanName = p.PlanName,
        Description = p.Description,
        CountryCode = p.CountryCode,
        CountryName = p.CountryName,
        CurrencyCode = p.CurrencyCode,
        MonthlyPrice = p.MonthlyPrice,
        AnnualPrice = p.AnnualPrice,
        MaxUsers = p.MaxUsers,
        MaxCompanies = p.MaxCompanies,
        IsActive = p.IsActive,
        CreatedDate = p.CreatedDate
    };
}
