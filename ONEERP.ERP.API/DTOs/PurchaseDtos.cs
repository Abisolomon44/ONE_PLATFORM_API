namespace ONEERP.ERP.API.DTOs;

public class LookupItem
{
    public long Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
}

public class ProductLookupItem : LookupItem
{
    public long? UomId { get; set; }
    public string? UomName { get; set; }
    public string? HsnCode { get; set; }
    public decimal? GstRate { get; set; }
    public string? Barcode { get; set; }
}

public class PurchaseLookupsDto
{
    public List<LookupItem> Companies { get; set; } = new();
    public List<LookupItem> Branches { get; set; } = new();
    public List<LookupItem> Warehouses { get; set; } = new();
    public List<LookupItem> Suppliers { get; set; } = new();
    public List<LookupItem> Products { get; set; } = new();
    public List<LookupItem> Units { get; set; } = new();
    public List<LookupItem> Taxes { get; set; } = new();
}
