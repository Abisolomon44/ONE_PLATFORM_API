using FluentValidation;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Validators;

public class CreateOperatorRequestValidator : AbstractValidator<CreateOperatorRequest>
{
    public CreateOperatorRequestValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.OperatorCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.OperatorName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.OperatorTypeId).GreaterThan(0);
    }
}

public class UpdateOperatorRequestValidator : AbstractValidator<UpdateOperatorRequest>
{
    public UpdateOperatorRequestValidator()
    {
        RuleFor(x => x.OperatorCode).NotEmpty().MaximumLength(50).When(x => x.OperatorCode != null);
        RuleFor(x => x.OperatorName).NotEmpty().MaximumLength(100).When(x => x.OperatorName != null);
        RuleFor(x => x.OperatorTypeId).GreaterThan(0).When(x => x.OperatorTypeId != null);
    }
}

public class CreateCounterAssignmentRequestValidator : AbstractValidator<CreateCounterAssignmentRequest>
{
    public CreateCounterAssignmentRequestValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThan(0);
        RuleFor(x => x.StoreId).GreaterThan(0);
        RuleFor(x => x.CounterId).GreaterThan(0);
        RuleFor(x => x.OperatorId).GreaterThan(0);
    }
}

public class UpdateCounterAssignmentRequestValidator : AbstractValidator<UpdateCounterAssignmentRequest>
{
    public UpdateCounterAssignmentRequestValidator()
    {
        RuleFor(x => x.StoreId).GreaterThan(0).When(x => x.StoreId != null);
        RuleFor(x => x.CounterId).GreaterThan(0).When(x => x.CounterId != null);
        RuleFor(x => x.OperatorId).GreaterThan(0).When(x => x.OperatorId != null);
    }
}

public class SaleEntryContextValidateRequestValidator : AbstractValidator<SaleEntryContextValidateRequest>
{
    public SaleEntryContextValidateRequestValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThan(0);
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.StoreId).GreaterThan(0);
        RuleFor(x => x.CounterId).GreaterThan(0);
        RuleFor(x => x.OperatorId).GreaterThan(0);
        RuleFor(x => x.PosSessionId).GreaterThan(0);
    }
}