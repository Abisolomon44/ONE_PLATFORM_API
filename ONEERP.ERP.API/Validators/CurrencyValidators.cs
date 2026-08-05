using FluentValidation;
using ONEERP.ERP.API.Requests;

namespace ONEERP.ERP.API.Validators;

/// <summary>
/// Validation rules for SaveCurrencyRequest.
/// Uniqueness of CurrencyCode and base-currency rules are enforced
/// in the service layer where the database is accessible.
/// </summary>
public class SaveCurrencyRequestValidator : AbstractValidator<SaveCurrencyRequest>
{
    public SaveCurrencyRequestValidator()
    {
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required.")
            .MaximumLength(10).WithMessage("Currency code must be at most 10 characters.");

        RuleFor(x => x.CurrencyName)
            .NotEmpty().WithMessage("Currency name is required.")
            .MaximumLength(100).WithMessage("Currency name must be at most 100 characters.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol is required.")
            .MaximumLength(10).WithMessage("Symbol must be at most 10 characters.");

        RuleFor(x => x.ISOCode)
            .MaximumLength(10).When(x => x.ISOCode is not null)
            .WithMessage("ISO code must be at most 10 characters.");

        RuleFor(x => x.DecimalPlaces)
            .GreaterThanOrEqualTo((byte)0)
            .LessThanOrEqualTo((byte)4)
            .WithMessage("Decimal places must be between 0 and 4.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Sort order must be a non-negative number.");
    }
}
