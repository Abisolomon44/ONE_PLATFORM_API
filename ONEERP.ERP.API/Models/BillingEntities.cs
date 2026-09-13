using System;

namespace ONEERP.ERP.API.Models;

/// <summary>System master defining pricing types (Sales, Wholesale, Retail, ...). Not company-scoped.</summary>
public class PriceType
{
    public long PriceTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>Company-scoped unit conversions (common or product-specific).</summary>
public class UnitConversion
{
    public long UnitConversionId { get; set; }
    public long CompanyId { get; set; }
    public long? ProductId { get; set; }
    public long FromUnitId { get; set; }
    public long ToUnitId { get; set; }
    public decimal ConversionFactor { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>Company-scoped barcodes for products.</summary>
public class Barcode
{
    public long BarcodeId { get; set; }
    public long CompanyId { get; set; }
    public long ProductId { get; set; }
    public long? UnitId { get; set; }
    public string BarcodeValue { get; set; } = string.Empty;
    public string? BarcodeType { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>Company-scoped GST/HSN/SAC codes.</summary>
public class HsnSac
{
    public long HsnSacId { get; set; }
    public long CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HsnSacType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? TaxId { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>Company-scoped service categories.</summary>
public class ServiceCategory
{
    public long ServiceCategoryId { get; set; }
    public long CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>Company-scoped services.</summary>
public class Service
{
    public long ServiceId { get; set; }
    public long? EntityId { get; set; }
    public long CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long? ServiceCategoryId { get; set; }
    public long? UnitId { get; set; }
    public long? HsnSacId { get; set; }
    public long? DefaultTaxId { get; set; }
    public decimal StandardRate { get; set; }
    public bool IsTaxInclusive { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>Company-scoped price lists.</summary>
public class PriceList
{
    public long PriceListId { get; set; }
    public long CompanyId { get; set; }
    public long PriceTypeId { get; set; }
    public int CurrencyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>Price list line item (child of PriceList).</summary>
public class PriceListDetail
{
    public long PriceListDetailId { get; set; }
    public long PriceListId { get; set; }
    public long ProductId { get; set; }
    public long? UnitId { get; set; }
    public decimal Price { get; set; }
    public decimal MinimumQuantity { get; set; } = 1;
    public decimal? MaximumQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>Company-scoped discount rules (percentage or amount) with product/service/category/price-list targeting.</summary>
public class DiscountRule
{
    public long DiscountRuleId { get; set; }
    public long CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long? ProductId { get; set; }
    public long? ServiceId { get; set; }
    public long? ProductCategoryId { get; set; }
    public long? PriceListId { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal? MinimumQuantity { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumDiscount { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>Company-scoped offers (promotions).</summary>
public class Offer
{
    public long OfferId { get; set; }
    public long CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OfferType { get; set; } = string.Empty;
    public string? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? MinimumQuantity { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumDiscount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>Offer line item (child of Offer). Targets a product, service, or product category.</summary>
public class OfferDetail
{
    public long OfferDetailId { get; set; }
    public long OfferId { get; set; }
    public long? ProductId { get; set; }
    public long? ServiceId { get; set; }
    public long? ProductCategoryId { get; set; }
    public decimal? MinimumQuantity { get; set; }
    public decimal? FreeQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>Company-scoped coupons linked to an offer.</summary>
public class Coupon
{
    public long CouponId { get; set; }
    public long CompanyId { get; set; }
    public long OfferId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int? UsageLimit { get; set; }
    public int? UsagePerCustomer { get; set; }
    public int UsedCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}