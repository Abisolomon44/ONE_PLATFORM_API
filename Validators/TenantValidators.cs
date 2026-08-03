using FluentValidation;

namespace ONEERP.Platform.API.Validators;

public class CreateTenantRequestValidator : AbstractValidator<DTOs.CreateTenantRequest>
{
    private static readonly string[] ReservedDatabases = { "master", "model", "msdb", "tempdb", "oneerp_platform" };

    public CreateTenantRequestValidator()
    {
        RuleFor(x => x.TenantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.TenantCode).NotEmpty().MaximumLength(50)
            .Matches("^[A-Za-z0-9_]+$").WithMessage("Tenant code may only contain letters, digits and underscores.");
        RuleFor(x => x.DatabaseName).NotEmpty().MaximumLength(128)
            .Matches("^[A-Za-z0-9_]+$").WithMessage("Database name may only contain letters, digits and underscores.")
            .Must(name => !ReservedDatabases.Contains(name.ToLowerInvariant()))
            .WithMessage("Database name is a reserved system database.");
        RuleFor(x => x.PlanId).GreaterThan(0).WithMessage("A plan must be selected.");
        RuleFor(x => x.AdminUsername).NotEmpty().MaximumLength(100)
            .Matches("^[A-Za-z0-9_]+$").WithMessage("Admin username may only contain letters, digits and underscores.");
        RuleFor(x => x.AdminPassword).NotEmpty().MinimumLength(8).MaximumLength(128)
            .WithMessage("Admin password must be at least 8 characters.");
        RuleFor(x => x.SubscriptionEnd).GreaterThan(x => x.SubscriptionStart)
            .WithMessage("Subscription end date must be after the start date.");
        RuleFor(x => x.SubscriptionEnd).GreaterThan(DateTime.UtcNow)
            .WithMessage("Subscription end date must be in the future.");
        RuleFor(x => x.Status).Must(BeValidStatus).WithMessage("Status must be Active or Suspended.");
    }

    private static bool BeValidStatus(string? status)
        => string.IsNullOrWhiteSpace(status) || status is "Active" or "Suspended";
}

public class UpdateTenantRequestValidator : AbstractValidator<DTOs.UpdateTenantRequest>
{
    public UpdateTenantRequestValidator()
    {
        RuleFor(x => x.TenantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.Status).Must(BeValidStatus).WithMessage("Status must be Active or Suspended.");
    }

    private static bool BeValidStatus(string? status)
        => string.IsNullOrWhiteSpace(status) || status is "Active" or "Suspended";
}

public class CreateSubscriptionRequestValidator : AbstractValidator<DTOs.CreateSubscriptionRequest>
{
    public CreateSubscriptionRequestValidator()
    {
        RuleFor(x => x.TenantId).GreaterThan(0);
        RuleFor(x => x.PlanId).GreaterThan(0);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage("End date must be after the start date.");
        RuleFor(x => x.EndDate).GreaterThan(DateTime.UtcNow).WithMessage("End date must be in the future.");
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(20);
    }
}
