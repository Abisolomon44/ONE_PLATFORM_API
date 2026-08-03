using FluentValidation;

namespace ONEERP.Platform.API.Validators;

public class CreatePlanRequestValidator : AbstractValidator<DTOs.CreatePlanRequest>
{
    public CreatePlanRequestValidator()
    {
        RuleFor(x => x.PlanCode).NotEmpty().MaximumLength(50)
            .Matches("^[A-Za-z0-9_]+$").WithMessage("Plan code may only contain letters, digits and underscores.");
        RuleFor(x => x.PlanName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.CountryCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CountryName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CurrencyCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.MonthlyPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AnnualPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxUsers).GreaterThan(0);
        RuleFor(x => x.MaxCompanies).GreaterThan(0);
    }
}

public class UpdatePlanRequestValidator : AbstractValidator<DTOs.UpdatePlanRequest>
{
    public UpdatePlanRequestValidator()
    {
        RuleFor(x => x.PlanCode).NotEmpty().MaximumLength(50)
            .Matches("^[A-Za-z0-9_]+$").WithMessage("Plan code may only contain letters, digits and underscores.");
        RuleFor(x => x.PlanName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.CountryCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CountryName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CurrencyCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.MonthlyPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AnnualPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxUsers).GreaterThan(0);
        RuleFor(x => x.MaxCompanies).GreaterThan(0);
    }
}
