namespace ONEERP.ERP.API.DTOs;

/* ---------------- Product Categories ---------------- */

public record CreateProductCategoryRequest(
    string CategoryCode,
    string CategoryName,
    string? Description,
    long? ParentCategoryId,
    int? SortOrder = null);

public record UpdateProductCategoryRequest(
    string CategoryCode,
    string CategoryName,
    string? Description,
    long? ParentCategoryId,
    int? SortOrder,
    bool IsActive);

public class ProductCategoryDto
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int? SortOrder { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- Product Sub Categories ---------------- */

public record CreateProductSubCategoryRequest(
    long? CategoryId,
    string SubCategoryCode,
    string SubCategoryName,
    string? Description,
    int? SortOrder = null);

public record UpdateProductSubCategoryRequest(
    long? CategoryId,
    string SubCategoryCode,
    string SubCategoryName,
    string? Description,
    int? SortOrder,
    bool IsActive);

public class ProductSubCategoryDto
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string SubCategoryCode { get; set; } = string.Empty;
    public string SubCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? SortOrder { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- Product Brands ---------------- */

public record CreateProductBrandRequest(
    string BrandCode,
    string BrandName,
    string? Description);

public record UpdateProductBrandRequest(
    string BrandCode,
    string BrandName,
    string? Description,
    bool IsActive);

public class ProductBrandDto
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string BrandCode { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- Product Units ---------------- */

public record CreateProductUnitRequest(
    string UnitCode,
    string UnitName,
    string? Symbol,
    int? DecimalPlaces = null);

public record UpdateProductUnitRequest(
    string UnitCode,
    string UnitName,
    string? Symbol,
    int? DecimalPlaces,
    bool IsActive);

public class ProductUnitDto
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public int DecimalPlaces { get; set; }
    public bool IsActive { get; set; }
}

/* ---------------- Products ---------------- */

public record CreateProductRequest(
    string ProductCode,
    string ProductName,
    long CompanyId,
    long? CategoryId = null,
    long? SubCategoryId = null,
    long? BrandId = null,
    long UOMId = 0,
    long? BranchId = null,
    string? SKU = null,
    string? Barcode = null,
    decimal? MRP = null,
    decimal? PurchasePrice = null,
    decimal? SalesPrice = null,
    long? TaxId = null,
    bool IsStockItem = true,
    bool IsSaleable = true,
    bool IsPurchaseable = true,
    string? Description = null);

public record UpdateProductRequest(
    string ProductCode,
    string ProductName,
    long CompanyId,
    long? CategoryId,
    long? SubCategoryId,
    long? BrandId,
    long UOMId,
    long? BranchId,
    string? SKU,
    string? Barcode,
    decimal? MRP,
    decimal? PurchasePrice,
    decimal? SalesPrice,
    long? TaxId,
    bool IsStockItem,
    bool IsSaleable,
    bool IsPurchaseable,
    string? Description,
    bool IsActive);

public class ProductDto
{
    public long Id { get; set; }
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

    public bool IsStockItem { get; set; }
    public bool IsSaleable { get; set; }
    public bool IsPurchaseable { get; set; }
    public bool IsActive { get; set; }

    public string? Description { get; set; }

    public string? CategoryName { get; set; }
    public string? SubCategoryName { get; set; }
    public string? BrandName { get; set; }
    public string? UOMName { get; set; }
    public string? BranchName { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
