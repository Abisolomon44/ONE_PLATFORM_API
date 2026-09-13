using ONEERP.ERP.API.Data;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface IEntityService
{
    Task<EntityDto> CreateAsync(CreateEntityRequest request);
    Task<EntityDto?> GetByIdAsync(long entityId);
    Task<EntityDto> ReplaceAddressesAsync(long entityId, IReadOnlyList<CreateAddressRequest> addresses);
    Task<EntityDto> ReplaceContactsAsync(long entityId, IReadOnlyList<CreateContactRequest> contacts);
    Task<EntityDto> ReplaceFilesAsync(long entityId, IReadOnlyList<CreateFileRequest> files);
    Task<EntityDto> ReplaceNotesAsync(long entityId, IReadOnlyList<CreateNoteRequest> notes);
    Task<EntityDto> ReplaceTagsAsync(long entityId, IReadOnlyList<CreateTagRequest> tags);
}

public class EntityService : IEntityService
{
    private readonly IEntityRepository _entityRepository;
    private readonly TenantAccessor _accessor;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public EntityService(
        IEntityRepository entityRepository,
        TenantAccessor accessor,
        IAuditService auditService,
        ICurrentUser currentUser)
    {
        _entityRepository = entityRepository;
        _accessor = accessor;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<EntityDto> CreateAsync(CreateEntityRequest request)
    {
        var entityType = request.EntityType.Trim().ToUpperInvariant();

        var entityCode = string.IsNullOrWhiteSpace(request.EntityCode)
            ? await _entityRepository.GetNextCodeAsync(entityType)
            : request.EntityCode.Trim().ToUpperInvariant();

        if (await _entityRepository.CodeExistsAsync(entityType, entityCode))
            throw new DomainException($"Entity code '{entityCode}' already exists for type '{entityType}'.");

        long? createdBy = _currentUser.UserId;

        var entity = new Entity
        {
            EntityType = entityType,
            EntityCode = entityCode,
            EntityName = request.EntityName.Trim(),
            IsActive = request.IsActive,
            CreatedBy = createdBy,
            ModifiedBy = createdBy
        };

        var dto = new EntityDto
        {
            EntityType = entityType,
            EntityCode = entityCode,
            EntityName = entity.EntityName,
            IsActive = entity.IsActive,
            Addresses = new List<AddressDto>(),
            Contacts = new List<ContactDto>(),
            Files = new List<FileDto>(),
            Notes = new List<NoteDto>(),
            Tags = new List<TagDto>()
        };

        using (var connection = _accessor.OpenTenantConnection())
        {
            using var transaction = connection.BeginTransaction();

            entity.EntityId = await _entityRepository.InsertEntityAsync(entity, connection, transaction);
            dto.EntityId = entity.EntityId;

            foreach (var addressReq in request.Addresses ?? new List<CreateAddressRequest>())
            {
                if (IsEmptyAddress(addressReq))
                    continue;

                var address = new Address
                {
                    AddressTypeId = addressReq.AddressTypeId,
                    AddressLine1 = addressReq.AddressLine1,
                    AddressLine2 = addressReq.AddressLine2,
                    AddressLine3 = addressReq.AddressLine3,
                    AddressLine4 = addressReq.AddressLine4,
                    AddressLine5 = addressReq.AddressLine5,
                    Landmark = addressReq.Landmark,
                    CityId = addressReq.CityId,
                    StateId = addressReq.StateId,
                    CountryId = addressReq.CountryId,
                    PostalCode = addressReq.PostalCode,
                    IsActive = true,
                    CreatedBy = createdBy,
                    ModifiedBy = createdBy
                };

                address.AddressId = await _entityRepository.InsertAddressAsync(address, connection, transaction);

                await _entityRepository.InsertEntityAddressAsync(new EntityAddress
                {
                    EntityId = entity.EntityId,
                    AddressId = address.AddressId,
                    IsPrimary = addressReq.IsPrimary,
                    IsActive = true,
                    CreatedBy = createdBy
                }, connection, transaction);

                dto.Addresses.Add(new AddressDto
                {
                    AddressId = address.AddressId,
                    AddressTypeId = address.AddressTypeId,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    AddressLine3 = address.AddressLine3,
                    AddressLine4 = address.AddressLine4,
                    AddressLine5 = address.AddressLine5,
                    Landmark = address.Landmark,
                    CityId = address.CityId,
                    StateId = address.StateId,
                    CountryId = address.CountryId,
                    PostalCode = address.PostalCode,
                    IsPrimary = addressReq.IsPrimary
                });
            }

            foreach (var contactReq in request.Contacts ?? new List<CreateContactRequest>())
            {
                if (IsEmptyContact(contactReq))
                    continue;

                var contact = new Contact
                {
                    ContactTypeId = contactReq.ContactTypeId,
                    ContactName = contactReq.ContactName?.Trim() ?? string.Empty,
                    Designation = contactReq.Designation,
                    Email = contactReq.Email,
                    Mobile = contactReq.Mobile,
                    Phone = contactReq.Phone,
                    Website = contactReq.Website,
                    IsActive = true,
                    CreatedBy = createdBy,
                    ModifiedBy = createdBy
                };

                contact.ContactId = await _entityRepository.InsertContactAsync(contact, connection, transaction);

                await _entityRepository.InsertEntityContactAsync(new EntityContact
                {
                    EntityId = entity.EntityId,
                    ContactId = contact.ContactId,
                    IsPrimary = contactReq.IsPrimary,
                    IsActive = true,
                    CreatedBy = createdBy
                }, connection, transaction);

                dto.Contacts.Add(new ContactDto
                {
                    ContactId = contact.ContactId,
                    ContactTypeId = contact.ContactTypeId,
                    ContactName = contact.ContactName,
                    Designation = contact.Designation,
                    Email = contact.Email,
                    Mobile = contact.Mobile,
                    Phone = contact.Phone,
                    Website = contact.Website,
                    IsPrimary = contactReq.IsPrimary
                });
            }

            foreach (var fileReq in request.Files ?? new List<CreateFileRequest>())
            {
                if (IsEmptyFile(fileReq))
                    continue;

                var file = new StoredFile
                {
                    FileName = fileReq.FileName?.Trim() ?? string.Empty,
                    OriginalFileName = fileReq.OriginalFileName?.Trim() ?? string.Empty,
                    BucketName = fileReq.BucketName?.Trim() ?? string.Empty,
                    ObjectKey = fileReq.ObjectKey?.Trim() ?? string.Empty,
                    ContentType = fileReq.ContentType,
                    Extension = fileReq.Extension,
                    FileSize = fileReq.FileSize,
                    StorageProvider = string.IsNullOrWhiteSpace(fileReq.StorageProvider) ? "MINIO" : fileReq.StorageProvider.Trim().ToUpperInvariant(),
                    IsActive = true,
                    CreatedBy = createdBy,
                    ModifiedBy = createdBy
                };

                var fileType = string.IsNullOrWhiteSpace(fileReq.FileType) ? "FILE" : fileReq.FileType.Trim().ToUpperInvariant();

                file.FileId = await _entityRepository.InsertFileAsync(file, connection, transaction);

                await _entityRepository.InsertEntityFileAsync(new EntityFile
                {
                    EntityId = entity.EntityId,
                    FileId = file.FileId,
                    FileType = fileType,
                    IsPrimary = fileReq.IsPrimary,
                    IsActive = true,
                    CreatedBy = createdBy
                }, connection, transaction);

                dto.Files.Add(new FileDto
                {
                    FileId = file.FileId,
                    FileType = fileType,
                    FileName = file.FileName,
                    OriginalFileName = file.OriginalFileName,
                    BucketName = file.BucketName,
                    ObjectKey = file.ObjectKey,
                    ContentType = file.ContentType,
                    Extension = file.Extension,
                    FileSize = file.FileSize,
                    StorageProvider = file.StorageProvider,
                    IsPrimary = fileReq.IsPrimary
                });
            }

            foreach (var noteReq in request.Notes ?? new List<CreateNoteRequest>())
            {
                if (string.IsNullOrWhiteSpace(noteReq.NoteText))
                    continue;

                var note = new Note
                {
                    NoteText = noteReq.NoteText?.Trim() ?? string.Empty,
                    NoteType = noteReq.NoteType?.Trim(),
                    IsActive = true,
                    CreatedBy = createdBy,
                    ModifiedBy = createdBy
                };

                note.NoteId = await _entityRepository.InsertNoteAsync(note, connection, transaction);

                await _entityRepository.InsertEntityNoteAsync(new EntityNote
                {
                    EntityId = entity.EntityId,
                    NoteId = note.NoteId,
                    IsActive = true,
                    CreatedBy = createdBy
                }, connection, transaction);

                dto.Notes.Add(new NoteDto
                {
                    NoteId = note.NoteId,
                    NoteText = note.NoteText,
                    NoteType = note.NoteType
                });
            }

            foreach (var tagReq in request.Tags ?? new List<CreateTagRequest>())
            {
                if (tagReq.TagId is null && string.IsNullOrWhiteSpace(tagReq.TagName))
                    continue;

                long tagId = tagReq.TagId ?? 0;
                var tagDto = new TagDto();

                if (tagReq.TagId is null)
                {
                    var tagName = tagReq.TagName?.Trim();
                    if (string.IsNullOrWhiteSpace(tagName))
                        throw new DomainException($"Tag name is required when TagId is not provided.");

                    var existing = await _entityRepository.GetTagByNameAsync(tagName, connection, transaction);
                    if (existing is not null)
                    {
                        tagId = existing.TagId;
                        tagDto.TagId = existing.TagId;
                        tagDto.TagName = existing.TagName;
                        tagDto.Color = existing.Color;
                    }
                    else
                    {
                        var tag = new Tag
                        {
                            TagName = tagName,
                            Color = tagReq.Color?.Trim(),
                            IsActive = true,
                            CreatedBy = createdBy,
                            ModifiedBy = createdBy
                        };
                        tag.TagId = await _entityRepository.InsertTagAsync(tag, connection, transaction);
                        tagDto.TagId = tag.TagId;
                        tagDto.TagName = tag.TagName;
                        tagDto.Color = tag.Color;
                        tagId = tag.TagId;
                    }
                }

                await _entityRepository.InsertEntityTagAsync(new EntityTag
                {
                    EntityId = entity.EntityId,
                    TagId = tagId,
                    IsActive = true,
                    CreatedBy = createdBy
                }, connection, transaction);

                dto.Tags.Add(tagDto);
            }

            transaction.Commit();
        }

        await _auditService.WriteAsync("Entity", entity.EntityId.ToString(), "Create", _currentUser.Username);

        return dto;
    }

    public async Task<EntityDto?> GetByIdAsync(long entityId)
    {
        var entity = await _entityRepository.GetByIdAsync(entityId);
        if (entity is null)
            return null;

        return new EntityDto
        {
            EntityId = entity.EntityId,
            EntityType = entity.EntityType,
            EntityCode = entity.EntityCode,
            EntityName = entity.EntityName,
            IsActive = entity.IsActive,
            Addresses = (await _entityRepository.GetAddressesAsync(entityId)).ToList(),
            Contacts = (await _entityRepository.GetContactsAsync(entityId)).ToList(),
            Files = (await _entityRepository.GetFilesAsync(entityId)).ToList(),
            Notes = (await _entityRepository.GetNotesAsync(entityId)).ToList(),
            Tags = (await _entityRepository.GetTagsAsync(entityId)).ToList()
        };
    }

    public async Task<EntityDto> ReplaceAddressesAsync(long entityId, IReadOnlyList<CreateAddressRequest> addresses)
    {
        var entity = await RequireEntityAsync(entityId);

        using (var connection = _accessor.OpenTenantConnection())
        {
            using var transaction = connection.BeginTransaction();

            await _entityRepository.DeleteAddressesForEntityAsync(entityId, connection, transaction);

            foreach (var addressReq in addresses ?? Array.Empty<CreateAddressRequest>())
            {
                if (IsEmptyAddress(addressReq))
                    continue;

                var address = new Address
                {
                    AddressTypeId = addressReq.AddressTypeId,
                    AddressLine1 = addressReq.AddressLine1,
                    AddressLine2 = addressReq.AddressLine2,
                    AddressLine3 = addressReq.AddressLine3,
                    AddressLine4 = addressReq.AddressLine4,
                    AddressLine5 = addressReq.AddressLine5,
                    Landmark = addressReq.Landmark,
                    CityId = addressReq.CityId,
                    StateId = addressReq.StateId,
                    CountryId = addressReq.CountryId,
                    PostalCode = addressReq.PostalCode,
                    IsActive = true,
                    CreatedBy = _currentUser.UserId,
                    ModifiedBy = _currentUser.UserId
                };

                address.AddressId = await _entityRepository.InsertAddressAsync(address, connection, transaction);
                await _entityRepository.InsertEntityAddressAsync(new EntityAddress
                {
                    EntityId = entityId,
                    AddressId = address.AddressId,
                    IsPrimary = addressReq.IsPrimary,
                    IsActive = true,
                    CreatedBy = _currentUser.UserId
                }, connection, transaction);
            }

            transaction.Commit();
        }

        return await ReloadAsync(entityId);
    }

    public async Task<EntityDto> ReplaceContactsAsync(long entityId, IReadOnlyList<CreateContactRequest> contacts)
    {
        var entity = await RequireEntityAsync(entityId);

        using (var connection = _accessor.OpenTenantConnection())
        {
            using var transaction = connection.BeginTransaction();

            await _entityRepository.DeleteContactsForEntityAsync(entityId, connection, transaction);

            foreach (var contactReq in contacts ?? Array.Empty<CreateContactRequest>())
            {
                if (IsEmptyContact(contactReq))
                    continue;

                var contact = new Contact
                {
                    ContactTypeId = contactReq.ContactTypeId,
                    ContactName = contactReq.ContactName?.Trim() ?? string.Empty,
                    Designation = contactReq.Designation,
                    Email = contactReq.Email,
                    Mobile = contactReq.Mobile,
                    Phone = contactReq.Phone,
                    Website = contactReq.Website,
                    IsActive = true,
                    CreatedBy = _currentUser.UserId,
                    ModifiedBy = _currentUser.UserId
                };

                contact.ContactId = await _entityRepository.InsertContactAsync(contact, connection, transaction);
                await _entityRepository.InsertEntityContactAsync(new EntityContact
                {
                    EntityId = entityId,
                    ContactId = contact.ContactId,
                    IsPrimary = contactReq.IsPrimary,
                    IsActive = true,
                    CreatedBy = _currentUser.UserId
                }, connection, transaction);
            }

            transaction.Commit();
        }

        return await ReloadAsync(entityId);
    }

    public async Task<EntityDto> ReplaceFilesAsync(long entityId, IReadOnlyList<CreateFileRequest> files)
    {
        var entity = await RequireEntityAsync(entityId);

        using (var connection = _accessor.OpenTenantConnection())
        {
            using var transaction = connection.BeginTransaction();

            await _entityRepository.DeleteFilesForEntityAsync(entityId, connection, transaction);

            foreach (var fileReq in files ?? Array.Empty<CreateFileRequest>())
            {
                if (IsEmptyFile(fileReq))
                    continue;

                var fileType = string.IsNullOrWhiteSpace(fileReq.FileType) ? "FILE" : fileReq.FileType.Trim().ToUpperInvariant();

                var file = new StoredFile
                {
                    FileName = fileReq.FileName?.Trim() ?? string.Empty,
                    OriginalFileName = fileReq.OriginalFileName?.Trim() ?? string.Empty,
                    BucketName = fileReq.BucketName?.Trim() ?? string.Empty,
                    ObjectKey = fileReq.ObjectKey?.Trim() ?? string.Empty,
                    ContentType = fileReq.ContentType,
                    Extension = fileReq.Extension,
                    FileSize = fileReq.FileSize,
                    StorageProvider = string.IsNullOrWhiteSpace(fileReq.StorageProvider) ? "MINIO" : fileReq.StorageProvider.Trim().ToUpperInvariant(),
                    IsActive = true,
                    CreatedBy = _currentUser.UserId,
                    ModifiedBy = _currentUser.UserId
                };

                file.FileId = await _entityRepository.InsertFileAsync(file, connection, transaction);
                await _entityRepository.InsertEntityFileAsync(new EntityFile
                {
                    EntityId = entityId,
                    FileId = file.FileId,
                    FileType = fileType,
                    IsPrimary = fileReq.IsPrimary,
                    IsActive = true,
                    CreatedBy = _currentUser.UserId
                }, connection, transaction);
            }

            transaction.Commit();
        }

        return await ReloadAsync(entityId);
    }

    public async Task<EntityDto> ReplaceNotesAsync(long entityId, IReadOnlyList<CreateNoteRequest> notes)
    {
        var entity = await RequireEntityAsync(entityId);

        using (var connection = _accessor.OpenTenantConnection())
        {
            using var transaction = connection.BeginTransaction();

            await _entityRepository.DeleteNotesForEntityAsync(entityId, connection, transaction);

            foreach (var noteReq in notes ?? Array.Empty<CreateNoteRequest>())
            {
                if (string.IsNullOrWhiteSpace(noteReq.NoteText))
                    continue;

                var note = new Note
                {
                    NoteText = noteReq.NoteText.Trim(),
                    NoteType = noteReq.NoteType?.Trim(),
                    IsActive = true,
                    CreatedBy = _currentUser.UserId,
                    ModifiedBy = _currentUser.UserId
                };

                note.NoteId = await _entityRepository.InsertNoteAsync(note, connection, transaction);
                await _entityRepository.InsertEntityNoteAsync(new EntityNote
                {
                    EntityId = entityId,
                    NoteId = note.NoteId,
                    IsActive = true,
                    CreatedBy = _currentUser.UserId
                }, connection, transaction);
            }

            transaction.Commit();
        }

        return await ReloadAsync(entityId);
    }

    public async Task<EntityDto> ReplaceTagsAsync(long entityId, IReadOnlyList<CreateTagRequest> tags)
    {
        var entity = await RequireEntityAsync(entityId);

        using (var connection = _accessor.OpenTenantConnection())
        {
            using var transaction = connection.BeginTransaction();

            await _entityRepository.DeleteTagsForEntityAsync(entityId, connection, transaction);

            foreach (var tagReq in tags ?? Array.Empty<CreateTagRequest>())
            {
                if (tagReq.TagId is null && string.IsNullOrWhiteSpace(tagReq.TagName))
                    continue;

                long tagId = tagReq.TagId ?? 0;

                if (tagReq.TagId is null)
                {
                    var tagName = tagReq.TagName?.Trim();
                    if (string.IsNullOrWhiteSpace(tagName))
                        throw new DomainException("Tag name is required when TagId is not provided.");

                    var existing = await _entityRepository.GetTagByNameAsync(tagName, connection, transaction);
                    if (existing is not null)
                    {
                        tagId = existing.TagId;
                    }
                    else
                    {
                        var tag = new Tag
                        {
                            TagName = tagName,
                            Color = tagReq.Color?.Trim(),
                            IsActive = true,
                            CreatedBy = _currentUser.UserId,
                            ModifiedBy = _currentUser.UserId
                        };
                        tag.TagId = await _entityRepository.InsertTagAsync(tag, connection, transaction);
                        tagId = tag.TagId;
                    }
                }

                await _entityRepository.InsertEntityTagAsync(new EntityTag
                {
                    EntityId = entityId,
                    TagId = tagId,
                    IsActive = true,
                    CreatedBy = _currentUser.UserId
                }, connection, transaction);
            }

            transaction.Commit();
        }

        return await ReloadAsync(entityId);
    }

    private async Task<Entity> RequireEntityAsync(long entityId)
    {
        return await _entityRepository.GetByIdAsync(entityId)
            ?? throw new NotFoundException($"Entity '{entityId}' was not found.");
    }

    private async Task<EntityDto> ReloadAsync(long entityId)
    {
        return await GetByIdAsync(entityId)
            ?? throw new NotFoundException($"Entity '{entityId}' was not found.");
    }

    private static bool IsEmptyAddress(CreateAddressRequest a) =>
        string.IsNullOrWhiteSpace(a.AddressLine1) &&
        string.IsNullOrWhiteSpace(a.AddressLine2) &&
        string.IsNullOrWhiteSpace(a.AddressLine3) &&
        string.IsNullOrWhiteSpace(a.AddressLine4) &&
        string.IsNullOrWhiteSpace(a.AddressLine5) &&
        string.IsNullOrWhiteSpace(a.Landmark) &&
        string.IsNullOrWhiteSpace(a.PostalCode) &&
        a.AddressTypeId is null &&
        a.CountryId is null &&
        a.StateId is null &&
        a.CityId is null;

    private static bool IsEmptyContact(CreateContactRequest c) =>
        string.IsNullOrWhiteSpace(c.ContactName) &&
        string.IsNullOrWhiteSpace(c.Designation) &&
        string.IsNullOrWhiteSpace(c.Email) &&
        string.IsNullOrWhiteSpace(c.Mobile) &&
        string.IsNullOrWhiteSpace(c.Phone) &&
        string.IsNullOrWhiteSpace(c.Website) &&
        c.ContactTypeId is null;

    private static bool IsEmptyFile(CreateFileRequest f) =>
        string.IsNullOrWhiteSpace(f.FileName) &&
        string.IsNullOrWhiteSpace(f.OriginalFileName) &&
        string.IsNullOrWhiteSpace(f.BucketName) &&
        string.IsNullOrWhiteSpace(f.ObjectKey) &&
        string.IsNullOrWhiteSpace(f.FileType);
}
