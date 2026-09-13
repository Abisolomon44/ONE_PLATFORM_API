using FluentValidation;

namespace ONEERP.ERP.API.Validators;

public class CreateFinancialYearRequestValidator : AbstractValidator<DTOs.CreateFinancialYearRequest>
{
    public CreateFinancialYearRequestValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Financial year code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");
    }
}

public class UpdateFinancialYearRequestValidator : AbstractValidator<DTOs.UpdateFinancialYearRequest>
{
    public UpdateFinancialYearRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Financial year code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");
    }
}