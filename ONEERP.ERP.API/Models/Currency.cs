namespace ONEERP.ERP.API.Models;

/// <summary>
/// Currency master entity stored in the tenant database.
/// </summary>
public class Currency
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
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
