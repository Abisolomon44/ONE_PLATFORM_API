namespace ONEERP.ERP.API.Requests;

/// <summary>
/// Combined request payload used for both creating and updating a currency.
/// </summary>
public class SaveCurrencyRequest
{
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string? ISOCode { get; set; }
    public byte DecimalPlaces { get; set; } = 2;
    public bool IsBaseCurrency { get; set; } = false;
    public int SortOrder { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}
