namespace ONEERP.ERP.API.Models;

/* ---------------- Product Categories ---------------- */

public class ProductCategory
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? ParentCategoryId { get; set; }
    public int? SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/* ---------------- Product Sub Categories ---------------- */

public class ProductSubCategory
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public long CategoryId { get; set; }
    public string SubCategoryCode { get; set; } = string.Empty;
    public string SubCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/* ---------------- Product Brands ---------------- */

public class ProductBrand
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string BrandCode { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/* ---------------- Product Units ---------------- */

public class ProductUnit
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public int DecimalPlaces { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/* ---------------- Products ---------------- */

public class Product
{
    public long Id { get; set; }
    public long? EntityId { get; set; }
    public long CompanyId { get; set; }
    public long? BranchId { get; set; }

    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    public long? CategoryId { get; set; }
    public long? SubCategoryId { get; set; }
    public long? BrandId { get; set; }
    public long UOMId { get; set; }

    public string? SKU { get; set; }
    public string? Barcode { get; set; }

    public decimal? MRP { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? SalesPrice { get; set; }

    public long? TaxId { get; set; }
    public long? HsnSacId { get; set; }

    public bool IsStockItem { get; set; } = true;
    public bool IsSaleable { get; set; } = true;
    public bool IsPurchaseable { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }

    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
