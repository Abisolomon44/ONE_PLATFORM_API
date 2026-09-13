namespace ONEERP.ERP.API.Models;

public class Entity
{
    public long EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityCode { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class Address
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
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class EntityAddress
{
    public long EntityAddressId { get; set; }
    public long EntityId { get; set; }
    public long AddressId { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Contact
{
    public long ContactId { get; set; }
    public long? ContactTypeId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class EntityContact
{
    public long EntityContactId { get; set; }
    public long EntityId { get; set; }
    public long ContactId { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StoredFile
{
    public long FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string? Extension { get; set; }
    public long FileSize { get; set; }
    public string StorageProvider { get; set; } = "MINIO";
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class EntityFile
{
    public long EntityFileId { get; set; }
    public long EntityId { get; set; }
    public long FileId { get; set; }
    public string FileType { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Note
{
    public long NoteId { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public string? NoteType { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class EntityNote
{
    public long EntityNoteId { get; set; }
    public long EntityId { get; set; }
    public long NoteId { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Tag
{
    public long TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public string? Color { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class EntityTag
{
    public long EntityTagId { get; set; }
    public long EntityId { get; set; }
    public long TagId { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}