using FluentValidation;

namespace ONEERP.Platform.API.Validators;

/// <summary>
/// Route-level validation for the tenant migration endpoints. The tenant id is
/// the only client-supplied value; no SQL, connection string or script is ever
/// accepted from the client.
/// </summary>
public class MigrationRequestValidator : AbstractValidator<int>
{
    public MigrationRequestValidator()
    {
        RuleFor(id => id)
            .GreaterThan(0)
            .WithMessage("Tenant id must be greater than zero.")
            .LessThanOrEqualTo(int.MaxValue)
            .WithMessage("Tenant id is out of range.");
    }
}
