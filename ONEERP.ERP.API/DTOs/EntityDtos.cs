namespace ONEERP.ERP.API.DTOs;

/* ---------------- Create requests ---------------- */

/// <summary>
/// One address for an entity (inserted into dbo.Address + linked via dbo.EntityAddress).
/// </summary>
public record CreateAddressRequest(
    long? AddressTypeId,
    string? AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string? AddressLine4,
    string? AddressLine5,
    string? Landmark,
    long? CityId,
    long? StateId,
    long? CountryId,
    string? PostalCode,
    bool IsPrimary);

/// <summary>
/// One contact for an entity (inserted into dbo.Contact + linked via dbo.EntityContact).
/// </summary>
public record CreateContactRequest(
    long? ContactTypeId,
    string ContactName,
    string? Designation,
    string? Email,
    string? Mobile,
    string? Phone,
    string? Website,
    bool IsPrimary);

/// <summary>
/// One stored file for an entity (inserted into dbo.Files + linked via dbo.EntityFile).
/// </summary>
public record CreateFileRequest(
    string FileName,
    string OriginalFileName,
    string BucketName,
    string ObjectKey,
    string? ContentType,
    string? Extension,
    long FileSize,
    string? StorageProvider,
    string FileType,
    bool IsPrimary);

/// <summary>
/// One note for an entity (inserted into dbo.Note + linked via dbo.EntityNote).
/// </summary>
public record CreateNoteRequest(
    string NoteText,
    string? NoteType);

/// <summary>
/// One tag for an entity. When TagId is provided an existing tag is reused,
/// otherwise a new tag is created from TagName (inserted into dbo.Tag + linked
/// via dbo.EntityTag).
/// </summary>
public record CreateTagRequest(
    long? TagId,
    string? TagName,
    string? Color);

public record CreateEntityRequest(
    string EntityType,
    string? EntityCode,
    string EntityName,
    bool IsActive,
    List<CreateAddressRequest>? Addresses,
    List<CreateContactRequest>? Contacts,
    List<CreateFileRequest>? Files,
    List<CreateNoteRequest>? Notes,
    List<CreateTagRequest>? Tags);

/* ---------------- Result DTOs ---------------- */

public class AddressDto
{
    public long AddressId { get; set; }
    public long? AddressTypeId { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? AddressLine4 { get; set; }
    public string? AddressLine5 { get; set; }
    public string? Landmark { get; set; }
    public long? CityId { get; set; }
    public long? StateId { get; set; }
    public long? CountryId { get; set; }
    public string? PostalCode { get; set; }
    public bool IsPrimary { get; set; }
}

public class ContactDto
{
    public long ContactId { get; set; }
    public long? ContactTypeId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public bool IsPrimary { get; set; }
}

public class FileDto
{
    public long FileId { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string? Extension { get; set; }
    public long FileSize { get; set; }
    public string StorageProvider { get; set; } = "MINIO";
    public bool IsPrimary { get; set; }
}

public class NoteDto
{
    public long NoteId { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public string? NoteType { get; set; }
}

public class TagDto
{
    public long TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public string? Color { get; set; }
}

public class EntityDto
{
    public long EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityCode { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<AddressDto> Addresses { get; set; } = new();
    public List<ContactDto> Contacts { get; set; } = new();
    public List<FileDto> Files { get; set; } = new();
    public List<NoteDto> Notes { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();
}