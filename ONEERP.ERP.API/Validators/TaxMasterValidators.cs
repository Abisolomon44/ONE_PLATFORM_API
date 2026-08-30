using FluentValidation;
using ONEERP.ERP.API.DTOs;

namespace ONEERP.ERP.API.Validators;

public class CreateTaxTypeSystemRequestValidator : AbstractValidator<CreateTaxTypeSystemRequest>
{
    public CreateTaxTypeSystemRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(300);
    }
}

public class UpdateTaxTypeSystemRequestValidator : AbstractValidator<UpdateTaxTypeSystemRequest>
{
    public UpdateTaxTypeSystemRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(300);
    }
}

public class CreateTaxRequestValidator : AbstractValidator<CreateTaxRequest>
{
    public CreateTaxRequestValidator()
    {
        RuleFor(x => x.TaxTypeSystemId).GreaterThan(0).WithMessage("Tax type system is required.");
        RuleFor(x => x.TaxCode).NotEmpty().MaximumLength(30).WithMessage("Tax code is required (max 30).");
        RuleFor(x => x.TaxName).NotEmpty().MaximumLength(100).WithMessage("Tax name is required (max 100).");
        RuleFor(x => x.TaxRate).GreaterThanOrEqualTo(0).WithMessage("Tax rate must be zero or greater.");
        RuleFor(x => x.Description).MaximumLength(300);
    }
}

public class UpdateTaxRequestValidator : AbstractValidator<UpdateTaxRequest>
{
    public UpdateTaxRequestValidator()
    {
        RuleFor(x => x.TaxTypeSystemId).GreaterThan(0).WithMessage("Tax type system is required.");
        RuleFor(x => x.TaxCode).NotEmpty().MaximumLength(30).WithMessage("Tax code is required (max 30).");
        RuleFor(x => x.TaxName).NotEmpty().MaximumLength(100).WithMessage("Tax name is required (max 100).");
        RuleFor(x => x.TaxRate).GreaterThanOrEqualTo(0).WithMessage("Tax rate must be zero or greater.");
        RuleFor(x => x.Description).MaximumLength(300);
    }
}
