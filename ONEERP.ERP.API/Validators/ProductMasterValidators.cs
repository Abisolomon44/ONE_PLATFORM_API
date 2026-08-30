using FluentValidation;
using ONEERP.ERP.API.DTOs;

namespace ONEERP.ERP.API.Validators;

/* ---------------- Product Categories ---------------- */

public class CreateProductCategoryRequestValidator : AbstractValidator<CreateProductCategoryRequest>
{
    public CreateProductCategoryRequestValidator()
    {
        RuleFor(x => x.CategoryCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(100);
        When(x => x.SortOrder.HasValue, () => RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0));
    }
}

public class UpdateProductCategoryRequestValidator : AbstractValidator<UpdateProductCategoryRequest>
{
    public UpdateProductCategoryRequestValidator()
    {
        RuleFor(x => x.CategoryCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(100);
        When(x => x.SortOrder.HasValue, () => RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0));
    }
}

/* ---------------- Product Sub Categories ---------------- */

public class CreateProductSubCategoryRequestValidator : AbstractValidator<CreateProductSubCategoryRequest>
{
    public CreateProductSubCategoryRequestValidator()
    {
        RuleFor(x => x.CategoryId).NotNull().GreaterThan(0);
        RuleFor(x => x.SubCategoryCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.SubCategoryName).NotEmpty().MaximumLength(100);
        When(x => x.SortOrder.HasValue, () => RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0));
    }
}

public class UpdateProductSubCategoryRequestValidator : AbstractValidator<UpdateProductSubCategoryRequest>
{
    public UpdateProductSubCategoryRequestValidator()
    {
        RuleFor(x => x.CategoryId).NotNull().GreaterThan(0);
        RuleFor(x => x.SubCategoryCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.SubCategoryName).NotEmpty().MaximumLength(100);
        When(x => x.SortOrder.HasValue, () => RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0));
    }
}

/* ---------------- Product Brands ---------------- */

public class CreateProductBrandRequestValidator : AbstractValidator<CreateProductBrandRequest>
{
    public CreateProductBrandRequestValidator()
    {
        RuleFor(x => x.BrandCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.BrandName).NotEmpty().MaximumLength(100);
    }
}

public class UpdateProductBrandRequestValidator : AbstractValidator<UpdateProductBrandRequest>
{
    public UpdateProductBrandRequestValidator()
    {
        RuleFor(x => x.BrandCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.BrandName).NotEmpty().MaximumLength(100);
    }
}

/* ---------------- Product Units ---------------- */

public class CreateProductUnitRequestValidator : AbstractValidator<CreateProductUnitRequest>
{
    public CreateProductUnitRequestValidator()
    {
        RuleFor(x => x.UnitCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.UnitName).NotEmpty().MaximumLength(50);
        When(x => x.DecimalPlaces.HasValue, () => RuleFor(x => x.DecimalPlaces).GreaterThanOrEqualTo(0).LessThanOrEqualTo(6));
    }
}

public class UpdateProductUnitRequestValidator : AbstractValidator<UpdateProductUnitRequest>
{
    public UpdateProductUnitRequestValidator()
    {
        RuleFor(x => x.UnitCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.UnitName).NotEmpty().MaximumLength(50);
        When(x => x.DecimalPlaces.HasValue, () => RuleFor(x => x.DecimalPlaces).GreaterThanOrEqualTo(0).LessThanOrEqualTo(6));
    }
}

/* ---------------- Products ---------------- */

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UOMId).GreaterThan(0);
        When(x => x.MRP.HasValue, () => RuleFor(x => x.MRP).GreaterThanOrEqualTo(0));
        When(x => x.PurchasePrice.HasValue, () => RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0));
        When(x => x.SalesPrice.HasValue, () => RuleFor(x => x.SalesPrice).GreaterThanOrEqualTo(0));
        When(x => x.Description != null, () => RuleFor(x => x.Description).MaximumLength(500));
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UOMId).GreaterThan(0);
        When(x => x.MRP.HasValue, () => RuleFor(x => x.MRP).GreaterThanOrEqualTo(0));
        When(x => x.PurchasePrice.HasValue, () => RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0));
        When(x => x.SalesPrice.HasValue, () => RuleFor(x => x.SalesPrice).GreaterThanOrEqualTo(0));
        When(x => x.Description != null, () => RuleFor(x => x.Description).MaximumLength(500));
    }
}
