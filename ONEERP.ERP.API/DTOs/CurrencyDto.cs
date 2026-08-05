namespace ONEERP.ERP.API.DTOs;

/// <summary>
/// Read-only representation of a currency returned to the client.
/// </summary>
public class CurrencyDto
{
    public int Id { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string? ISOCode { get; set; }
    public byte DecimalPlaces { get; set; }
    public bool IsBaseCurrency { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
