using System;
using System.Collections.Generic;

namespace ONEERP.ERP.API.DTOs;

/* ---------------- Price Types (system master) ---------------- */

public record CreatePriceTypeRequest(string Code, string Name, string? Description, int? DisplayOrder = null);

public record UpdatePriceTypeRequest(string Code, string Name, string? Description, int DisplayOrder, bool IsActive);

public class PriceTypeDto
{
    public long PriceTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/* ---------------- Unit Conversions (company) ---------------- */

public record CreateUnitConversionRequest(long? ProductId, long FromUnitId, long ToUnitId, decimal ConversionFactor, bool IsDefault = false);

public record UpdateUnitConversionRequest(long? ProductId, long FromUnitId, long ToUnitId, decimal ConversionFactor, bool IsDefault, bool IsActive);

public class UnitConversionDto
{
    public long UnitConversionId { get; set; }
    public long CompanyId { get; set; }
    public long? ProductId { get; set; }
    public string? ProductName { get; set; }
    public long FromUnitId { get; set; }
    public string? FromUnitName { get; set; }
    public long ToUnitId { get; set; }
    public string? ToUnitName { get; set; }
    public decimal ConversionFactor { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/* ---------------- Barcodes (company) ---------------- */

public record CreateBarcodeRequest(string Barcode, long ProductId, long? UnitId = null, string? BarcodeType = null, bool IsPrimary = false);

public record UpdateBarcodeRequest(string Barcode, long ProductId, long? UnitId, string? BarcodeType, bool IsPrimary, bool IsActive);

public class BarcodeDto
{
    public long BarcodeId { get; set; }
    public long CompanyId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public long? UnitId { get; set; }
    public string? UnitName { get; set; }
    public string? BarcodeType { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/* ---------------- HSN/SAC (company) ---------------- */

public record CreateHsnSacRequest(string Code, string Name, string HsnSacType, long? TaxId = null, string? Description = null);

public record UpdateHsnSacRequest(string Code, string Name, string HsnSacType, long? TaxId, string? Description, bool IsActive);

public class HsnSacDto
{
    public long HsnSacId { get; set; }
    public long CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HsnSacType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? TaxId { get; set; }
    public string? TaxName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/* ---------------- Service Categories (company) ---------------- */

public record CreateServiceCategoryRequest(string Code, string Name, string? Description = null, int? DisplayOrder = null);

public record UpdateServiceCategoryRequest(string Code, string Name, string? Description, int DisplayOrder, bool IsActive);

public class ServiceCategoryDto
{
    public long ServiceCategoryId { get; set; }
    public long CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/* ---------------- Services (company) ---------------- */

public record CreateServiceRequest(string Code, string Name, long? ServiceCategoryId = null, long? UnitId = null, long? HsnSacId = null, long? DefaultTaxId = null, decimal StandardRate = 0, bool IsTaxInclusive = false, string? Description = null, long? EntityId = null);

public record UpdateServiceRequest(string Code, string Name, long? ServiceCategoryId, long? UnitId, long? HsnSacId, long? DefaultTaxId, decimal StandardRate, bool IsTaxInclusive, string? Description, bool IsActive, long? EntityId = null);

public class ServiceDto
{
    public long ServiceId { get; set; }
    public long? EntityId { get; set; }
    public long CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long? ServiceCategoryId { get; set; }
    public string? ServiceCategoryName { get; set; }
    public long? UnitId { get; set; }
    public string? UnitName { get; set; }
    public long? HsnSacId { get; set; }
    public string? HsnSacCode { get; set; }
    public long? DefaultTaxId { get; set; }
    public string? DefaultTaxName { get; set; }
    public decimal StandardRate { get; set; }
    public bool IsTaxInclusive { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/* ---------------- Price Lists (company) ---------------- */

public record CreatePriceListRequest(string Code, string Name, long PriceTypeId, int CurrencyId, string? Description = null, DateTime? EffectiveFrom = null, DateTime? EffectiveTo = null, bool IsDefault = false);

public record UpdatePriceListRequest(string Code, string Name, long PriceTypeId, int CurrencyId, string? Description, DateTime? EffectiveFrom, DateTime? EffectiveTo, bool IsDefault, bool IsActive);

public class PriceListDto
{
    public long PriceListId { get; set; }
    public long CompanyId { get; set; }
    public long PriceTypeId { get; set; }
    public string? PriceTypeName { get; set; }
    public int CurrencyId { get; set; }
    public string? CurrencyName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/* ---------------- Price List Details (child of PriceList) ---------------- */

public record CreatePriceListDetailRequest(long PriceListId, long ProductId, long? UnitId, decimal Price, decimal MinimumQuantity = 1, decimal? MaximumQuantity = null);

public record UpdatePriceListDetailRequest(long PriceListId, long ProductId, long? UnitId, decimal Price, decimal MinimumQuantity, decimal? MaximumQuantity, bool IsActive);

public class PriceListDetailDto
{
    public long PriceListDetailId { get; set; }
    public long PriceListId { get; set; }
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public long? UnitId { get; set; }
    public string? UnitName { get; set; }
    public decimal Price { get; set; }
    public decimal MinimumQuantity { get; set; }
    public decimal? MaximumQuantity { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PriceListDetailsReplaceRequest
{
    public List<CreatePriceListDetailRequest> Items { get; set; } = new();
}

/* ---------------- Discount Rules (company) ---------------- */

public record CreateDiscountRuleRequest(string Code, string Name, long? ProductId = null, long? ServiceId = null, long? ProductCategoryId = null, long? PriceListId = null, string DiscountType = "PERCENTAGE", decimal DiscountValue = 0, decimal? MinimumQuantity = null, decimal? MinimumAmount = null, decimal? MaximumDiscount = null, DateTime? EffectiveFrom = null, DateTime? EffectiveTo = null);

public record UpdateDiscountRuleRequest(string Code, string Name, long? ProductId, long? ServiceId, long? ProductCategoryId, long? PriceListId, string DiscountType, decimal DiscountValue, decimal? MinimumQuantity, decimal? MinimumAmount, decimal? MaximumDiscount, DateTime? EffectiveFrom, DateTime? EffectiveTo, bool IsActive);

public class DiscountRuleDto
{
    public long DiscountRuleId { get; set; }
    public long CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long? ProductId { get; set; }
    public string? ProductName { get; set; }
    public long? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public long? ProductCategoryId { get; set; }
    public string? ProductCategoryName { get; set; }
    public long? PriceListId { get; set; }
    public string? PriceListName { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal? MinimumQuantity { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumDiscount { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/* ---------------- Offers (company, with OfferDetails child) ---------------- */

public record CreateOfferRequest(string Code, string Name, string OfferType, DateTime StartDate, string? DiscountType = null, decimal? DiscountValue = null, decimal? MinimumQuantity = null, decimal? MinimumAmount = null, decimal? MaximumDiscount = null, DateTime? EndDate = null, string? Description = null);

public record UpdateOfferRequest(string Code, string Name, string OfferType, string? DiscountType, decimal? DiscountValue, decimal? MinimumQuantity, decimal? MinimumAmount, decimal? MaximumDiscount, DateTime StartDate, DateTime? EndDate, string? Description, bool IsActive);

public class OfferDto
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
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

/* ---------------- Offer Details (child of Offer) ---------------- */

public record CreateOfferDetailRequest(long OfferId, long? ProductId = null, long? ServiceId = null, long? ProductCategoryId = null, decimal? MinimumQuantity = null, decimal? FreeQuantity = null);

public record UpdateOfferDetailRequest(long OfferId, long? ProductId, long? ServiceId, long? ProductCategoryId, decimal? MinimumQuantity, decimal? FreeQuantity, bool IsActive);

public class OfferDetailDto
{
    public long OfferDetailId { get; set; }
    public long OfferId { get; set; }
    public long? ProductId { get; set; }
    public string? ProductName { get; set; }
    public long? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public long? ProductCategoryId { get; set; }
    public string? ProductCategoryName { get; set; }
    public decimal? MinimumQuantity { get; set; }
    public decimal? FreeQuantity { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OfferDetailsReplaceRequest
{
    public List<CreateOfferDetailRequest> Items { get; set; } = new();
}

/* ---------------- Coupons (company) ---------------- */

public record CreateCouponRequest(string Code, long OfferId, DateTime StartDate, string? Name = null, int? UsageLimit = null, int? UsagePerCustomer = null, DateTime? EndDate = null);

public record UpdateCouponRequest(string Code, long OfferId, string? Name, int? UsageLimit, int? UsagePerCustomer, DateTime StartDate, DateTime? EndDate, bool IsActive);

public class CouponDto
{
    public long CouponId { get; set; }
    public long CompanyId { get; set; }
    public long OfferId { get; set; }
    public string? OfferName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int? UsageLimit { get; set; }
    public int? UsagePerCustomer { get; set; }
    public int UsedCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}