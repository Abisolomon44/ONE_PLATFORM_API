using FluentValidation;
using ONEERP.ERP.API.DTOs;

namespace ONEERP.ERP.API.Validators;

public class CreateEntityRequestValidator : AbstractValidator<CreateEntityRequest>
{
    public CreateEntityRequestValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required.")
            .MaximumLength(30).WithMessage("Entity type must be at most 30 characters.");

        RuleFor(x => x.EntityCode)
            .MaximumLength(50).When(x => x.EntityCode is not null)
            .WithMessage("Entity code must be at most 50 characters.");

        RuleFor(x => x.EntityName)
            .NotEmpty().WithMessage("Entity name is required.")
            .MaximumLength(200).WithMessage("Entity name must be at most 200 characters.");

        RuleForEach(x => x.Addresses).SetValidator(new CreateAddressRequestValidator());
        RuleForEach(x => x.Contacts).SetValidator(new CreateContactRequestValidator());
        RuleForEach(x => x.Files).SetValidator(new CreateFileRequestValidator());
        RuleForEach(x => x.Notes).SetValidator(new CreateNoteRequestValidator());
        RuleForEach(x => x.Tags).SetValidator(new CreateTagRequestValidator());
    }
}

public class CreateAddressRequestValidator : AbstractValidator<CreateAddressRequest>
{
    public CreateAddressRequestValidator()
    {
        RuleFor(x => x.AddressLine1)
            .MaximumLength(200).When(x => x.AddressLine1 is not null)
            .WithMessage("Address line 1 must be at most 200 characters.");

        RuleFor(x => x.AddressLine2)
            .MaximumLength(200).When(x => x.AddressLine2 is not null)
            .WithMessage("Address line 2 must be at most 200 characters.");

        RuleFor(x => x.AddressLine3)
            .MaximumLength(200).When(x => x.AddressLine3 is not null)
            .WithMessage("Address line 3 must be at most 200 characters.");

        RuleFor(x => x.AddressLine4)
            .MaximumLength(200).When(x => x.AddressLine4 is not null)
            .WithMessage("Address line 4 must be at most 200 characters.");

        RuleFor(x => x.AddressLine5)
            .MaximumLength(200).When(x => x.AddressLine5 is not null)
            .WithMessage("Address line 5 must be at most 200 characters.");

        RuleFor(x => x.Landmark)
            .MaximumLength(200).When(x => x.Landmark is not null)
            .WithMessage("Landmark must be at most 200 characters.");

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).When(x => x.PostalCode is not null)
            .WithMessage("Postal code must be at most 20 characters.");
    }
}

public class CreateContactRequestValidator : AbstractValidator<CreateContactRequest>
{
    public CreateContactRequestValidator()
    {
        RuleFor(x => x.ContactName)
            .MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.ContactName))
            .WithMessage("Contact name must be at most 200 characters.");

        RuleFor(x => x.Designation)
            .MaximumLength(100).When(x => x.Designation is not null)
            .WithMessage("Designation must be at most 100 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("A valid email address is required.")
            .MaximumLength(200).When(x => x.Email is not null)
            .WithMessage("Email must be at most 200 characters.");

        RuleFor(x => x.Mobile)
            .MaximumLength(30).When(x => x.Mobile is not null)
            .WithMessage("Mobile must be at most 30 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).When(x => x.Phone is not null)
            .WithMessage("Phone must be at most 30 characters.");

        RuleFor(x => x.Website)
            .MaximumLength(300).When(x => x.Website is not null)
            .WithMessage("Website must be at most 300 characters.");
    }
}

public class CreateFileRequestValidator : AbstractValidator<CreateFileRequest>
{
    public CreateFileRequestValidator()
    {
        RuleFor(x => x.FileName)
            .MaximumLength(255).When(x => x.FileName is not null)
            .WithMessage("File name must be at most 255 characters.");

        RuleFor(x => x.OriginalFileName)
            .MaximumLength(255).When(x => x.OriginalFileName is not null)
            .WithMessage("Original file name must be at most 255 characters.");

        RuleFor(x => x.BucketName)
            .MaximumLength(100).When(x => x.BucketName is not null)
            .WithMessage("Bucket name must be at most 100 characters.");

        RuleFor(x => x.ObjectKey)
            .MaximumLength(1000).When(x => x.ObjectKey is not null)
            .WithMessage("Object key must be at most 1000 characters.");

        RuleFor(x => x.FileSize)
            .GreaterThanOrEqualTo(0).WithMessage("File size cannot be negative.");

        RuleFor(x => x.FileType)
            .MaximumLength(30).When(x => x.FileType is not null)
            .WithMessage("File type must be at most 30 characters.");

        RuleFor(x => x.StorageProvider)
            .MaximumLength(30).When(x => x.StorageProvider is not null)
            .WithMessage("Storage provider must be at most 30 characters.");
    }
}

public class CreateNoteRequestValidator : AbstractValidator<CreateNoteRequest>
{
    public CreateNoteRequestValidator()
    {
        RuleFor(x => x.NoteText)
            .NotEmpty().WithMessage("Note text is required.")
            .MaximumLength(2000).WithMessage("Note text must be at most 2000 characters.");

        RuleFor(x => x.NoteType)
            .MaximumLength(30).When(x => x.NoteType is not null)
            .WithMessage("Note type must be at most 30 characters.");
    }
}

public class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagRequestValidator()
    {
        RuleFor(x => x.TagName)
            .MaximumLength(100).When(x => x.TagName is not null)
            .WithMessage("Tag name must be at most 100 characters.")
            .NotEmpty().When(x => x.TagId is null)
            .WithMessage("Tag name is required when TagId is not provided.");

        RuleFor(x => x.Color)
            .MaximumLength(20).When(x => x.Color is not null)
            .WithMessage("Tag color must be at most 20 characters.");
    }
}