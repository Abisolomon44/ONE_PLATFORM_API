namespace ONEERP.ERP.API.DTOs;

/* ---------------- Countries ---------------- */

public record CreateCountryRequest(
    string Name,
    string ISOCode2,
    string ISOCode3,
    string? PhoneCode,
    string? CurrencyCode,
    string? Nationality);

public record UpdateCountryRequest(
    string Name,
    string ISOCode2,
    string ISOCode3,
    string? PhoneCode,
    string? CurrencyCode,
    string? Nationality,
    bool IsActive);

public class CountryDto
{
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ISOCode2 { get; set; } = string.Empty;
    public string ISOCode3 { get; set; } = string.Empty;
    public string? PhoneCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Nationality { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
}

/* ---------------- States ---------------- */

public record CreateStateRequest(int CountryId, string Name, string StateCode, string? GSTStateCode);

public record UpdateStateRequest(int CountryId, string Name, string StateCode, string? GSTStateCode, bool IsActive);

public class StateDto
{
    public int StateId { get; set; }
    public int CountryId { get; set; }
    public string? CountryName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string? GSTStateCode { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
}

/* ---------------- Cities ---------------- */

public record CreateCityRequest(
    int CountryId,
    int StateId,
    string Name,
    string? PostalCode,
    decimal? Latitude,
    decimal? Longitude);

public record UpdateCityRequest(
    int CountryId,
    int StateId,
    string Name,
    string? PostalCode,
    decimal? Latitude,
    decimal? Longitude,
    bool IsActive);

public class CityDto
{
    public int CityId { get; set; }
    public int CountryId { get; set; }
    public string? CountryName { get; set; }
    public int StateId { get; set; }
    public string? StateName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
}
