using FluentValidation;

namespace ONEERP.ERP.API.Validators;

public class LoginRequestValidator : AbstractValidator<DTOs.LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class RefreshTokenRequestValidator : AbstractValidator<DTOs.RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class CreateUserRequestValidator : AbstractValidator<DTOs.CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Mobile).MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.Status).Must(s => s is "Active" or "Inactive").WithMessage("Status must be Active or Inactive.");
    }
}

public class UpdateUserRequestValidator : AbstractValidator<DTOs.UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Mobile).MaximumLength(50);
        RuleFor(x => x.Status).Must(s => s is "Active" or "Inactive").WithMessage("Status must be Active or Inactive.");
    }
}

public class ResetUserPasswordRequestValidator : AbstractValidator<DTOs.ResetUserPasswordRequest>
{
    public ResetUserPasswordRequestValidator()
    {
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.");
    }
}

public class CreateRoleRequestValidator : AbstractValidator<DTOs.CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100)
            .Matches("^[A-Za-z0-9_]+$").WithMessage("Role code may only contain letters, digits and underscores.");
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateRoleRequestValidator : AbstractValidator<DTOs.UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateCompanyRequestValidator : AbstractValidator<DTOs.UpdateCompanyRequest>
{
    public UpdateCompanyRequestValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ShortName).MaximumLength(100);
        RuleFor(x => x.Abbreviation).MaximumLength(20);
        RuleFor(x => x.BusinessTypeId).GreaterThan(0);
        RuleFor(x => x.IndustryTypeId).GreaterThan(0);
        RuleFor(x => x.GSTNumber).MaximumLength(20);
        RuleFor(x => x.PANNumber).MaximumLength(20);
        RuleFor(x => x.TANNumber).MaximumLength(20);
        RuleFor(x => x.CINNumber).MaximumLength(30);
        RuleFor(x => x.RegistrationNumber).MaximumLength(100);
        RuleFor(x => x.CurrencyId).GreaterThan(0);
        RuleFor(x => x.LanguageId).GreaterThan(0);
        RuleFor(x => x.TimeZoneId).GreaterThan(0);
        RuleFor(x => x.Website).MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(150).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Mobile).MaximumLength(30);
        RuleFor(x => x.LogoUrl).MaximumLength(500);
        RuleFor(x => x.DefaultFinancialYearId).GreaterThan(0).When(x => x.DefaultFinancialYearId.HasValue);
        RuleFor(x => x.DateFormat).MaximumLength(20);
        RuleFor(x => x.TimeFormat).MaximumLength(20);
        RuleFor(x => x.NumberFormat).MaximumLength(20);
        RuleFor(x => x.Theme).MaximumLength(50);
        RuleFor(x => x.PrimaryColor).MaximumLength(20);
        RuleFor(x => x.SecondaryColor).MaximumLength(20);
        RuleFor(x => x.Remarks).MaximumLength(1000);
    }
}

public class CreateCompanyRequestValidator : AbstractValidator<DTOs.CreateCompanyRequest>
{
    public CreateCompanyRequestValidator()
    {
        RuleFor(x => x.CompanyCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Company code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityId).GreaterThan(0).When(x => x.EntityId.HasValue);
        RuleFor(x => x.ShortName).MaximumLength(100);
        RuleFor(x => x.Abbreviation).MaximumLength(20);
        RuleFor(x => x.BusinessTypeId).GreaterThan(0);
        RuleFor(x => x.IndustryTypeId).GreaterThan(0);
        RuleFor(x => x.GSTNumber).MaximumLength(20);
        RuleFor(x => x.PANNumber).MaximumLength(20);
        RuleFor(x => x.TANNumber).MaximumLength(20);
        RuleFor(x => x.CINNumber).MaximumLength(30);
        RuleFor(x => x.RegistrationNumber).MaximumLength(100);
        RuleFor(x => x.CurrencyId).GreaterThan(0);
        RuleFor(x => x.LanguageId).GreaterThan(0);
        RuleFor(x => x.TimeZoneId).GreaterThan(0);
        RuleFor(x => x.Website).MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(150).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Mobile).MaximumLength(30);
        RuleFor(x => x.LogoUrl).MaximumLength(500);
        RuleFor(x => x.DefaultFinancialYearId).GreaterThan(0).When(x => x.DefaultFinancialYearId.HasValue);
        RuleFor(x => x.DateFormat).MaximumLength(20);
        RuleFor(x => x.TimeFormat).MaximumLength(20);
        RuleFor(x => x.NumberFormat).MaximumLength(20);
        RuleFor(x => x.Theme).MaximumLength(50);
        RuleFor(x => x.PrimaryColor).MaximumLength(20);
        RuleFor(x => x.SecondaryColor).MaximumLength(20);
        RuleFor(x => x.Remarks).MaximumLength(1000);
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<DTOs.ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}

public class UpdateSettingsRequestValidator : AbstractValidator<DTOs.UpdateSettingsRequest>
{
    public UpdateSettingsRequestValidator()
    {
        RuleFor(x => x.Settings).NotNull().WithMessage("Settings payload is required.");
        RuleFor(x => x.Settings).Must(d => d.Count > 0).When(x => x.Settings is not null)
            .WithMessage("At least one setting must be provided.");
    }
}

public class CreateBusinessTypeRequestValidator : AbstractValidator<DTOs.CreateBusinessTypeRequest>
{
    public CreateBusinessTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

    public class UpdateBusinessTypeRequestValidator : AbstractValidator<DTOs.UpdateBusinessTypeRequest>
    {
        public UpdateBusinessTypeRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(250);
            RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        }
    }

    public class CreateBusinessPartnerRoleRequestValidator : AbstractValidator<DTOs.CreateBusinessPartnerRoleRequest>
    {
        public CreateBusinessPartnerRoleRequestValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(250);
        }
    }

    public class UpdateBusinessPartnerRoleRequestValidator : AbstractValidator<DTOs.UpdateBusinessPartnerRoleRequest>
    {
        public UpdateBusinessPartnerRoleRequestValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(250);
        }
    }

    public class CreateBusinessPartnerRequestValidator : AbstractValidator<DTOs.CreateBusinessPartnerRequest>
    {
        public CreateBusinessPartnerRequestValidator()
        {
            RuleFor(x => x.PartnerCode).NotEmpty().MaximumLength(30);
            RuleFor(x => x.PartnerName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.PatnerRoleIds).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
            RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CreditDays).GreaterThanOrEqualTo(0);
        }
    }

    public class UpdateBusinessPartnerRequestValidator : AbstractValidator<DTOs.UpdateBusinessPartnerRequest>
    {
        public UpdateBusinessPartnerRequestValidator()
        {
            RuleFor(x => x.PartnerCode).NotEmpty().MaximumLength(30);
            RuleFor(x => x.PartnerName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.PatnerRoleIds).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
            RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CreditDays).GreaterThanOrEqualTo(0);
        }
    }

    public class CreateIndustryTypeRequestValidator : AbstractValidator<DTOs.CreateIndustryTypeRequest>
{
    public CreateIndustryTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateIndustryTypeRequestValidator : AbstractValidator<DTOs.UpdateIndustryTypeRequest>
{
    public UpdateIndustryTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateCompanyGroupRequestValidator : AbstractValidator<DTOs.CreateCompanyGroupRequest>
{
    public CreateCompanyGroupRequestValidator()
    {
        RuleFor(x => x.GroupCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Group code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.GroupName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ShortName).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateCompanyGroupRequestValidator : AbstractValidator<DTOs.UpdateCompanyGroupRequest>
{
    public UpdateCompanyGroupRequestValidator()
    {
        RuleFor(x => x.GroupCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Group code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.GroupName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ShortName).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class CreateCountryRequestValidator : AbstractValidator<DTOs.CreateCountryRequest>
{
    public CreateCountryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ISOCode2).NotEmpty().Length(2)
            .Matches("^[A-Za-z]{2}$").WithMessage("ISO-2 code must be exactly two letters.");
        RuleFor(x => x.ISOCode3).NotEmpty().Length(3)
            .Matches("^[A-Za-z]{3}$").WithMessage("ISO-3 code must be exactly three letters.");
        RuleFor(x => x.PhoneCode).MaximumLength(10);
        RuleFor(x => x.CurrencyCode).MaximumLength(10);
        RuleFor(x => x.Nationality).MaximumLength(100);
    }
}

public class UpdateCountryRequestValidator : AbstractValidator<DTOs.UpdateCountryRequest>
{
    public UpdateCountryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ISOCode2).NotEmpty().Length(2)
            .Matches("^[A-Za-z]{2}$").WithMessage("ISO-2 code must be exactly two letters.");
        RuleFor(x => x.ISOCode3).NotEmpty().Length(3)
            .Matches("^[A-Za-z]{3}$").WithMessage("ISO-3 code must be exactly three letters.");
        RuleFor(x => x.PhoneCode).MaximumLength(10);
        RuleFor(x => x.CurrencyCode).MaximumLength(10);
        RuleFor(x => x.Nationality).MaximumLength(100);
    }
}

public class CreateStateRequestValidator : AbstractValidator<DTOs.CreateStateRequest>
{
    public CreateStateRequestValidator()
    {
        RuleFor(x => x.CountryId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StateCode).NotEmpty().MaximumLength(10)
            .Matches("^[A-Za-z0-9]+$").WithMessage("State code may only contain letters and digits.");
        RuleFor(x => x.GSTStateCode).MaximumLength(5);
    }
}

public class UpdateStateRequestValidator : AbstractValidator<DTOs.UpdateStateRequest>
{
    public UpdateStateRequestValidator()
    {
        RuleFor(x => x.CountryId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StateCode).NotEmpty().MaximumLength(10)
            .Matches("^[A-Za-z0-9]+$").WithMessage("State code may only contain letters and digits.");
        RuleFor(x => x.GSTStateCode).MaximumLength(5);
    }
}

public class CreateCityRequestValidator : AbstractValidator<DTOs.CreateCityRequest>
{
    public CreateCityRequestValidator()
    {
        RuleFor(x => x.CountryId).GreaterThan(0);
        RuleFor(x => x.StateId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).MaximumLength(10);
        RuleFor(x => x.Latitude).GreaterThanOrEqualTo((decimal)-90).LessThanOrEqualTo((decimal)90);
        RuleFor(x => x.Longitude).GreaterThanOrEqualTo((decimal)-180).LessThanOrEqualTo((decimal)180);
    }
}

public class UpdateCityRequestValidator : AbstractValidator<DTOs.UpdateCityRequest>
{
    public UpdateCityRequestValidator()
    {
        RuleFor(x => x.CountryId).GreaterThan(0);
        RuleFor(x => x.StateId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).MaximumLength(10);
        RuleFor(x => x.Latitude).GreaterThanOrEqualTo((decimal)-90).LessThanOrEqualTo((decimal)90);
        RuleFor(x => x.Longitude).GreaterThanOrEqualTo((decimal)-180).LessThanOrEqualTo((decimal)180);
    }
}

public class CreateLanguageRequestValidator : AbstractValidator<DTOs.CreateLanguageRequest>
{
    public CreateLanguageRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10)
            .Matches("^[A-Za-z]{2,3}$").WithMessage("Language code must be a valid ISO-639 code (2-3 letters).");
        RuleFor(x => x.CultureCode).MaximumLength(20);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateLanguageRequestValidator : AbstractValidator<DTOs.UpdateLanguageRequest>
{
    public UpdateLanguageRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10)
            .Matches("^[A-Za-z]{2,3}$").WithMessage("Language code must be a valid ISO-639 code (2-3 letters).");
        RuleFor(x => x.CultureCode).MaximumLength(20);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateTimeZoneRequestValidator : AbstractValidator<DTOs.CreateTimeZoneRequest>
{
    public CreateTimeZoneRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TimeZoneName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.UTCOffset).MaximumLength(20);
    }
}

public class UpdateTimeZoneRequestValidator : AbstractValidator<DTOs.UpdateTimeZoneRequest>
{
    public UpdateTimeZoneRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TimeZoneName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.UTCOffset).MaximumLength(20);
    }
}

public class CreateGstRegistrationTypeRequestValidator : AbstractValidator<DTOs.CreateGstRegistrationTypeRequest>
{
    public CreateGstRegistrationTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public class UpdateGstRegistrationTypeRequestValidator : AbstractValidator<DTOs.UpdateGstRegistrationTypeRequest>
{
    public UpdateGstRegistrationTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public class CreateAddressTypeRequestValidator : AbstractValidator<DTOs.CreateAddressTypeRequest>
{
    public CreateAddressTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public class UpdateAddressTypeRequestValidator : AbstractValidator<DTOs.UpdateAddressTypeRequest>
{
    public UpdateAddressTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public class CreateContactTypeRequestValidator : AbstractValidator<DTOs.CreateContactTypeRequest>
{
    public CreateContactTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public class UpdateContactTypeRequestValidator : AbstractValidator<DTOs.UpdateContactTypeRequest>
{
    public UpdateContactTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public class CreateDocumentTypeRequestValidator : AbstractValidator<DTOs.CreateDocumentTypeRequest>
{
    public CreateDocumentTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public class UpdateDocumentTypeRequestValidator : AbstractValidator<DTOs.UpdateDocumentTypeRequest>
{
    public UpdateDocumentTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public class CreateOrganizationTypeRequestValidator : AbstractValidator<DTOs.CreateOrganizationTypeRequest>
{
    public CreateOrganizationTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Organization type code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.Description).MaximumLength(250);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateOrganizationTypeRequestValidator : AbstractValidator<DTOs.UpdateOrganizationTypeRequest>
{
    public UpdateOrganizationTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Organization type code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.Description).MaximumLength(250);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateBranchRequestValidator : AbstractValidator<DTOs.CreateBranchRequest>
{
    public CreateBranchRequestValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Branch code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BranchTypeId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GSTNumber).MaximumLength(15);
        RuleFor(x => x.RegistrationNumber).MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateBranchRequestValidator : AbstractValidator<DTOs.UpdateBranchRequest>
{
    public UpdateBranchRequestValidator()
    {
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Branch code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BranchTypeId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GSTNumber).MaximumLength(15);
        RuleFor(x => x.RegistrationNumber).MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateDepartmentRequestValidator : AbstractValidator<DTOs.CreateDepartmentRequest>
{
    public CreateDepartmentRequestValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThan(0);
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.DepartmentCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Department code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.DepartmentName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateDepartmentRequestValidator : AbstractValidator<DTOs.UpdateDepartmentRequest>
{
    public UpdateDepartmentRequestValidator()
    {
        RuleFor(x => x.DepartmentCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Department code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.DepartmentName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateDesignationRequestValidator : AbstractValidator<DTOs.CreateDesignationRequest>
{
    public CreateDesignationRequestValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThan(0);
        RuleFor(x => x.DesignationCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Designation code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.DesignationName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateDesignationRequestValidator : AbstractValidator<DTOs.UpdateDesignationRequest>
{
    public UpdateDesignationRequestValidator()
    {
        RuleFor(x => x.DesignationCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Designation code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.DesignationName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateEmployeeRequestValidator : AbstractValidator<DTOs.CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThan(0);
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfJoining).LessThanOrEqualTo(DateTime.UtcNow);
    }
}

public class UpdateEmployeeRequestValidator : AbstractValidator<DTOs.UpdateEmployeeRequest>
{
    public UpdateEmployeeRequestValidator()
    {
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}

public class CreateWarehouseRequestValidator : AbstractValidator<DTOs.CreateWarehouseRequest>
{
    public CreateWarehouseRequestValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThan(0);
        RuleFor(x => x.WarehouseCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Warehouse code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.WarehouseName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateWarehouseRequestValidator : AbstractValidator<DTOs.UpdateWarehouseRequest>
{
    public UpdateWarehouseRequestValidator()
    {
        RuleFor(x => x.WarehouseCode).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Warehouse code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.WarehouseName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreatePermissionModuleRequestValidator : AbstractValidator<DTOs.CreatePermissionModuleRequest>
{
    public CreatePermissionModuleRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100)
            .Matches("^[A-Za-z0-9._-]+$").WithMessage("Code may only contain letters, digits, dots, hyphens and underscores.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Level).NotEmpty().MaximumLength(20);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Icon).MaximumLength(100);
        RuleFor(x => x.RoutePath).MaximumLength(200);
    }
}

public class UpdatePermissionModuleRequestValidator : AbstractValidator<DTOs.UpdatePermissionModuleRequest>
{
    public UpdatePermissionModuleRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100)
            .Matches("^[A-Za-z0-9._-]+$").WithMessage("Code may only contain letters, digits, dots, hyphens and underscores.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Level).NotEmpty().MaximumLength(20);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Icon).MaximumLength(100);
        RuleFor(x => x.RoutePath).MaximumLength(200);
    }
}

public class CreatePermissionActionRequestValidator : AbstractValidator<DTOs.CreatePermissionActionRequest>
{
    public CreatePermissionActionRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdatePermissionActionRequestValidator : AbstractValidator<DTOs.UpdatePermissionActionRequest>
{
    public UpdatePermissionActionRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Code may only contain letters, digits, hyphens and underscores.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
