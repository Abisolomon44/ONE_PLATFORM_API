using FluentValidation;

namespace ONEERP.ERP.API.Validators;

public class CreateStoreRequestValidator : AbstractValidator<DTOs.CreateStoreRequest>
{
    public CreateStoreRequestValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThan(0);
        RuleFor(x => x.StoreCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Store code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.StoreName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StoreType).MaximumLength(50);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Email).MaximumLength(100);
    }
}

public class UpdateStoreRequestValidator : AbstractValidator<DTOs.UpdateStoreRequest>
{
    public UpdateStoreRequestValidator()
    {
        RuleFor(x => x.StoreCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Store code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.StoreName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StoreType).MaximumLength(50);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Email).MaximumLength(100);
    }
}

public class CreateCounterRequestValidator : AbstractValidator<DTOs.CreateCounterRequest>
{
    public CreateCounterRequestValidator()
    {
        RuleFor(x => x.StoreId).GreaterThan(0);
        RuleFor(x => x.CounterCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Counter code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.CounterName).NotEmpty().MaximumLength(200);
    }
}

public class UpdateCounterRequestValidator : AbstractValidator<DTOs.UpdateCounterRequest>
{
    public UpdateCounterRequestValidator()
    {
        RuleFor(x => x.CounterCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Counter code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.CounterName).NotEmpty().MaximumLength(200);
    }
}

public class CreatePOSSessionRequestValidator : AbstractValidator<DTOs.CreatePOSSessionRequest>
{
    public CreatePOSSessionRequestValidator()
    {
        RuleFor(x => x.StoreId).GreaterThan(0).WithMessage("Store is required.");
        RuleFor(x => x.CounterId).GreaterThan(0).WithMessage("Counter is required.");
        RuleFor(x => x.OperatorId).GreaterThan(0).WithMessage("Operator is required.");
        RuleFor(x => x.OpeningCash).GreaterThanOrEqualTo(0);
    }
}

public class UpdatePOSSessionRequestValidator : AbstractValidator<DTOs.UpdatePOSSessionRequest>
{
    public UpdatePOSSessionRequestValidator()
    {
        RuleFor(x => x.ActualClosingCash).GreaterThanOrEqualTo(0);
        RuleFor(x => (int)x.Status).InclusiveBetween(2, 3)
            .WithMessage("Status must be Closed (2) or Void (3).");
        RuleFor(x => x.ClosingRemarks).MaximumLength(500);
        RuleFor(x => x).Must(x => x.Status != 2 || x.ActualClosingCash.HasValue)
            .WithMessage("Actual closing cash is required to close a session.");
    }
}