using FluentValidation;
using ONEERP.ERP.API.DTOs;

namespace ONEERP.ERP.API.Validators;

/* ---------------- Price Types ---------------- */

public class CreatePriceTypeRequestValidator : AbstractValidator<CreatePriceTypeRequest>
{
    public CreatePriceTypeRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("Name is required (max 100).");
        RuleFor(x => x.Description).MaximumLength(300);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdatePriceTypeRequestValidator : AbstractValidator<UpdatePriceTypeRequest>
{
    public UpdatePriceTypeRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("Name is required (max 100).");
        RuleFor(x => x.Description).MaximumLength(300);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

/* ---------------- Unit Conversions ---------------- */

public class CreateUnitConversionRequestValidator : AbstractValidator<CreateUnitConversionRequest>
{
    public CreateUnitConversionRequestValidator()
    {
        RuleFor(x => x.FromUnitId).GreaterThan(0).WithMessage("From unit is required.");
        RuleFor(x => x.ToUnitId).GreaterThan(0).WithMessage("To unit is required.");
        RuleFor(x => x.ConversionFactor).GreaterThan(0).WithMessage("Conversion factor must be greater than zero.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product is required.");
        RuleFor(x => x).Must(x => x.FromUnitId != x.ToUnitId).WithMessage("From unit and to unit must be different.");
    }
}

public class UpdateUnitConversionRequestValidator : AbstractValidator<UpdateUnitConversionRequest>
{
    public UpdateUnitConversionRequestValidator()
    {
        RuleFor(x => x.FromUnitId).GreaterThan(0).WithMessage("From unit is required.");
        RuleFor(x => x.ToUnitId).GreaterThan(0).WithMessage("To unit is required.");
        RuleFor(x => x.ConversionFactor).GreaterThan(0).WithMessage("Conversion factor must be greater than zero.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product is required.");
        RuleFor(x => x).Must(x => x.FromUnitId != x.ToUnitId).WithMessage("From unit and to unit must be different.");
    }
}

/* ---------------- Barcodes ---------------- */

public class CreateBarcodeRequestValidator : AbstractValidator<CreateBarcodeRequest>
{
    public CreateBarcodeRequestValidator()
    {
        RuleFor(x => x.Barcode).NotEmpty().MaximumLength(100).WithMessage("Barcode is required (max 100).");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product is required.");
        RuleFor(x => x.BarcodeType).MaximumLength(30);
    }
}

public class UpdateBarcodeRequestValidator : AbstractValidator<UpdateBarcodeRequest>
{
    public UpdateBarcodeRequestValidator()
    {
        RuleFor(x => x.Barcode).NotEmpty().MaximumLength(100).WithMessage("Barcode is required (max 100).");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product is required.");
        RuleFor(x => x.BarcodeType).MaximumLength(30);
    }
}

/* ---------------- HSN/SAC ---------------- */

public class CreateHsnSacRequestValidator : AbstractValidator<CreateHsnSacRequest>
{
    public CreateHsnSacRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Name is required (max 200).");
        RuleFor(x => x.HsnSacType).NotEmpty().Must(x => x is "HSN" or "SAC").WithMessage("Type must be HSN or SAC.");
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.TaxId).GreaterThan(0).When(x => x.TaxId.HasValue).WithMessage("Tax must be a valid tax.");
    }
}

public class UpdateHsnSacRequestValidator : AbstractValidator<UpdateHsnSacRequest>
{
    public UpdateHsnSacRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Name is required (max 200).");
        RuleFor(x => x.HsnSacType).NotEmpty().Must(x => x is "HSN" or "SAC").WithMessage("Type must be HSN or SAC.");
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.TaxId).GreaterThan(0).When(x => x.TaxId.HasValue).WithMessage("Tax must be a valid tax.");
    }
}

/* ---------------- Service Categories ---------------- */

public class CreateServiceCategoryRequestValidator : AbstractValidator<CreateServiceCategoryRequest>
{
    public CreateServiceCategoryRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("Name is required (max 100).");
        RuleFor(x => x.Description).MaximumLength(300);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateServiceCategoryRequestValidator : AbstractValidator<UpdateServiceCategoryRequest>
{
    public UpdateServiceCategoryRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("Name is required (max 100).");
        RuleFor(x => x.Description).MaximumLength(300);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

/* ---------------- Services ---------------- */

public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequest>
{
    public CreateServiceRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Name is required (max 200).");
        RuleFor(x => x.StandardRate).GreaterThanOrEqualTo(0).WithMessage("Standard rate must be zero or greater.");
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateServiceRequestValidator : AbstractValidator<UpdateServiceRequest>
{
    public UpdateServiceRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Name is required (max 200).");
        RuleFor(x => x.StandardRate).GreaterThanOrEqualTo(0).WithMessage("Standard rate must be zero or greater.");
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

/* ---------------- Price Lists ---------------- */

public class CreatePriceListRequestValidator : AbstractValidator<CreatePriceListRequest>
{
    public CreatePriceListRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("Name is required (max 100).");
        RuleFor(x => x.PriceTypeId).GreaterThan(0).WithMessage("Price type is required.");
        RuleFor(x => x.CurrencyId).GreaterThan(0).WithMessage("Currency is required.");
        RuleFor(x => x.Description).MaximumLength(300);
        RuleFor(x => x).Must(x => !x.EffectiveFrom.HasValue || !x.EffectiveTo.HasValue || x.EffectiveFrom <= x.EffectiveTo)
            .WithMessage("Effective from must be on or before effective to.");
    }
}

public class UpdatePriceListRequestValidator : AbstractValidator<UpdatePriceListRequest>
{
    public UpdatePriceListRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("Name is required (max 100).");
        RuleFor(x => x.PriceTypeId).GreaterThan(0).WithMessage("Price type is required.");
        RuleFor(x => x.CurrencyId).GreaterThan(0).WithMessage("Currency is required.");
        RuleFor(x => x.Description).MaximumLength(300);
        RuleFor(x => x).Must(x => !x.EffectiveFrom.HasValue || !x.EffectiveTo.HasValue || x.EffectiveFrom <= x.EffectiveTo)
            .WithMessage("Effective from must be on or before effective to.");
    }
}

/* ---------------- Price List Details ---------------- */

public class CreatePriceListDetailRequestValidator : AbstractValidator<CreatePriceListDetailRequest>
{
    public CreatePriceListDetailRequestValidator()
    {
        RuleFor(x => x.PriceListId).GreaterThan(0).WithMessage("Price list is required.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product is required.");
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price must be zero or greater.");
        RuleFor(x => x.MinimumQuantity).GreaterThan(0).WithMessage("Minimum quantity must be greater than zero.");
        RuleFor(x => x).Must(x => !x.MaximumQuantity.HasValue || x.MaximumQuantity >= x.MinimumQuantity)
            .WithMessage("Maximum quantity must be greater than or equal to minimum quantity.");
    }
}

public class UpdatePriceListDetailRequestValidator : AbstractValidator<UpdatePriceListDetailRequest>
{
    public UpdatePriceListDetailRequestValidator()
    {
        RuleFor(x => x.PriceListId).GreaterThan(0).WithMessage("Price list is required.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product is required.");
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price must be zero or greater.");
        RuleFor(x => x.MinimumQuantity).GreaterThan(0).WithMessage("Minimum quantity must be greater than zero.");
        RuleFor(x => x).Must(x => !x.MaximumQuantity.HasValue || x.MaximumQuantity >= x.MinimumQuantity)
            .WithMessage("Maximum quantity must be greater than or equal to minimum quantity.");
    }
}

public class PriceListDetailsReplaceRequestValidator : AbstractValidator<PriceListDetailsReplaceRequest>
{
    public PriceListDetailsReplaceRequestValidator()
    {
        RuleFor(x => x.Items).NotNull().WithMessage("Details are required.");
        RuleForEach(x => x.Items).SetValidator(new CreatePriceListDetailRequestValidator());
    }
}

/* ---------------- Discount Rules ---------------- */

public class CreateDiscountRuleRequestValidator : AbstractValidator<CreateDiscountRuleRequest>
{
    public CreateDiscountRuleRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150).WithMessage("Name is required (max 150).");
        RuleFor(x => x.DiscountType).NotEmpty().Must(x => x is "PERCENTAGE" or "AMOUNT").WithMessage("Discount type must be PERCENTAGE or AMOUNT.");
        RuleFor(x => x.DiscountValue).GreaterThanOrEqualTo(0).WithMessage("Discount value must be zero or greater.");
        RuleFor(x => x.MinimumQuantity).GreaterThanOrEqualTo(0).When(x => x.MinimumQuantity.HasValue);
        RuleFor(x => x.MinimumAmount).GreaterThanOrEqualTo(0).When(x => x.MinimumAmount.HasValue);
        RuleFor(x => x.MaximumDiscount).GreaterThanOrEqualTo(0).When(x => x.MaximumDiscount.HasValue);
        RuleFor(x => x).Must(x => !x.EffectiveFrom.HasValue || !x.EffectiveTo.HasValue || x.EffectiveFrom <= x.EffectiveTo)
            .WithMessage("Effective from must be on or before effective to.");
    }
}

public class UpdateDiscountRuleRequestValidator : AbstractValidator<UpdateDiscountRuleRequest>
{
    public UpdateDiscountRuleRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150).WithMessage("Name is required (max 150).");
        RuleFor(x => x.DiscountType).NotEmpty().Must(x => x is "PERCENTAGE" or "AMOUNT").WithMessage("Discount type must be PERCENTAGE or AMOUNT.");
        RuleFor(x => x.DiscountValue).GreaterThanOrEqualTo(0).WithMessage("Discount value must be zero or greater.");
        RuleFor(x => x.MinimumQuantity).GreaterThanOrEqualTo(0).When(x => x.MinimumQuantity.HasValue);
        RuleFor(x => x.MinimumAmount).GreaterThanOrEqualTo(0).When(x => x.MinimumAmount.HasValue);
        RuleFor(x => x.MaximumDiscount).GreaterThanOrEqualTo(0).When(x => x.MaximumDiscount.HasValue);
        RuleFor(x => x).Must(x => !x.EffectiveFrom.HasValue || !x.EffectiveTo.HasValue || x.EffectiveFrom <= x.EffectiveTo)
            .WithMessage("Effective from must be on or before effective to.");
    }
}

/* ---------------- Offers ---------------- */

public class CreateOfferRequestValidator : AbstractValidator<CreateOfferRequest>
{
    public CreateOfferRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150).WithMessage("Name is required (max 150).");
        RuleFor(x => x.OfferType).NotEmpty().MaximumLength(30).WithMessage("Offer type is required.");
        RuleFor(x => x.DiscountType).Must(x => x is null or "PERCENTAGE" or "AMOUNT").When(x => x.DiscountType is not null)
            .WithMessage("Discount type must be PERCENTAGE or AMOUNT.");
        RuleFor(x => x.DiscountValue).GreaterThanOrEqualTo(0).When(x => x.DiscountValue.HasValue)
            .WithMessage("Discount value must be zero or greater.");
        RuleFor(x => x.MinimumQuantity).GreaterThanOrEqualTo(0).When(x => x.MinimumQuantity.HasValue);
        RuleFor(x => x.MinimumAmount).GreaterThanOrEqualTo(0).When(x => x.MinimumAmount.HasValue);
        RuleFor(x => x.MaximumDiscount).GreaterThanOrEqualTo(0).When(x => x.MaximumDiscount.HasValue);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.StartDate).NotEmpty().WithMessage("Start date is required.");
        RuleFor(x => x).Must(x => !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("End date must be on or after start date.");
    }
}

public class UpdateOfferRequestValidator : AbstractValidator<UpdateOfferRequest>
{
    public UpdateOfferRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150).WithMessage("Name is required (max 150).");
        RuleFor(x => x.OfferType).NotEmpty().MaximumLength(30).WithMessage("Offer type is required.");
        RuleFor(x => x.DiscountType).Must(x => x is null or "PERCENTAGE" or "AMOUNT").When(x => x.DiscountType is not null)
            .WithMessage("Discount type must be PERCENTAGE or AMOUNT.");
        RuleFor(x => x.DiscountValue).GreaterThanOrEqualTo(0).When(x => x.DiscountValue.HasValue)
            .WithMessage("Discount value must be zero or greater.");
        RuleFor(x => x.MinimumQuantity).GreaterThanOrEqualTo(0).When(x => x.MinimumQuantity.HasValue);
        RuleFor(x => x.MinimumAmount).GreaterThanOrEqualTo(0).When(x => x.MinimumAmount.HasValue);
        RuleFor(x => x.MaximumDiscount).GreaterThanOrEqualTo(0).When(x => x.MaximumDiscount.HasValue);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.StartDate).NotEmpty().WithMessage("Start date is required.");
        RuleFor(x => x).Must(x => !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("End date must be on or after start date.");
    }
}

/* ---------------- Offer Details ---------------- */

public class CreateOfferDetailRequestValidator : AbstractValidator<CreateOfferDetailRequest>
{
    public CreateOfferDetailRequestValidator()
    {
        RuleFor(x => x.OfferId).GreaterThan(0).WithMessage("Offer is required.");
        RuleFor(x => x.MinimumQuantity).GreaterThanOrEqualTo(0).When(x => x.MinimumQuantity.HasValue);
        RuleFor(x => x.FreeQuantity).GreaterThanOrEqualTo(0).When(x => x.FreeQuantity.HasValue);
        RuleFor(x => x).Must(x => x.ProductId.HasValue || x.ServiceId.HasValue || x.ProductCategoryId.HasValue)
            .WithMessage("At least one of product, service, or product category is required.");
    }
}

public class UpdateOfferDetailRequestValidator : AbstractValidator<UpdateOfferDetailRequest>
{
    public UpdateOfferDetailRequestValidator()
    {
        RuleFor(x => x.OfferId).GreaterThan(0).WithMessage("Offer is required.");
        RuleFor(x => x.MinimumQuantity).GreaterThanOrEqualTo(0).When(x => x.MinimumQuantity.HasValue);
        RuleFor(x => x.FreeQuantity).GreaterThanOrEqualTo(0).When(x => x.FreeQuantity.HasValue);
        RuleFor(x => x).Must(x => x.ProductId.HasValue || x.ServiceId.HasValue || x.ProductCategoryId.HasValue)
            .WithMessage("At least one of product, service, or product category is required.");
    }
}

public class OfferDetailsReplaceRequestValidator : AbstractValidator<OfferDetailsReplaceRequest>
{
    public OfferDetailsReplaceRequestValidator()
    {
        RuleFor(x => x.Items).NotNull().WithMessage("Details are required.");
        RuleForEach(x => x.Items).SetValidator(new CreateOfferDetailRequestValidator());
    }
}

/* ---------------- Coupons ---------------- */

public class CreateCouponRequestValidator : AbstractValidator<CreateCouponRequest>
{
    public CreateCouponRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(50);
        RuleFor(x => x.OfferId).GreaterThan(0).WithMessage("Offer is required.");
        RuleFor(x => x.Name).MaximumLength(150);
        RuleFor(x => x.UsageLimit).GreaterThanOrEqualTo(0).When(x => x.UsageLimit.HasValue).WithMessage("Usage limit must be zero or greater.");
        RuleFor(x => x.UsagePerCustomer).GreaterThanOrEqualTo(0).When(x => x.UsagePerCustomer.HasValue).WithMessage("Usage per customer must be zero or greater.");
        RuleFor(x => x.StartDate).NotEmpty().WithMessage("Start date is required.");
        RuleFor(x => x).Must(x => !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("End date must be on or after start date.");
    }
}

public class UpdateCouponRequestValidator : AbstractValidator<UpdateCouponRequest>
{
    public UpdateCouponRequestValidator()
    {
        RuleFor(x => x.Code).MaximumLength(50);
        RuleFor(x => x.OfferId).GreaterThan(0).WithMessage("Offer is required.");
        RuleFor(x => x.Name).MaximumLength(150);
        RuleFor(x => x.UsageLimit).GreaterThanOrEqualTo(0).When(x => x.UsageLimit.HasValue).WithMessage("Usage limit must be zero or greater.");
        RuleFor(x => x.UsagePerCustomer).GreaterThanOrEqualTo(0).When(x => x.UsagePerCustomer.HasValue).WithMessage("Usage per customer must be zero or greater.");
        RuleFor(x => x.StartDate).NotEmpty().WithMessage("Start date is required.");
        RuleFor(x => x).Must(x => !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("End date must be on or after start date.");
    }
}