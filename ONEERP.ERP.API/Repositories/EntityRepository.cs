using System.Data;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IEntityRepository
{
    Task<bool> CodeExistsAsync(string entityType, string entityCode, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<string> GetNextCodeAsync(string entityType, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<long> InsertEntityAsync(Entity entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<long> InsertAddressAsync(Address address, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<long> InsertContactAsync(Contact contact, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<long> InsertFileAsync(StoredFile file, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<long> InsertNoteAsync(Note note, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<long> InsertTagAsync(Tag tag, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<Tag?> GetTagByNameAsync(string tagName, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<Entity?> GetByIdAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<IEnumerable<AddressDto>> GetAddressesAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<IEnumerable<ContactDto>> GetContactsAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<IEnumerable<FileDto>> GetFilesAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<IEnumerable<NoteDto>> GetNotesAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<IEnumerable<TagDto>> GetTagsAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task DeleteAddressesForEntityAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task DeleteContactsForEntityAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task DeleteFilesForEntityAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task DeleteNotesForEntityAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task DeleteTagsForEntityAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<long> InsertEntityAddressAsync(EntityAddress link, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<long> InsertEntityContactAsync(EntityContact link, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<long> InsertEntityFileAsync(EntityFile link, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<long> InsertEntityNoteAsync(EntityNote link, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<long> InsertEntityTagAsync(EntityTag link, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class EntityRepository : TenantRepositoryBase, IEntityRepository
{
    public EntityRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<bool> CodeExistsAsync(string entityType, string entityCode, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            return await Sql.ExecuteScalarAsync<bool>(conn,
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Entity WHERE EntityType = @entityType AND EntityCode = @entityCode) THEN 1 ELSE 0 END",
                new { entityType, entityCode }, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<string> GetNextCodeAsync(string entityType, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            var next = await Sql.ExecuteScalarAsync<int>(conn,
                "SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(EntityCode, LEN(@entityType) + 2, 10) AS INT)), 0) + 1 " +
                "FROM dbo.Entity WHERE EntityType = @entityType AND EntityCode LIKE @entityType + '-%'",
                new { entityType }, transaction);
            return $"{entityType}-{next:D3}";
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<long> InsertEntityAsync(Entity entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Entity (EntityType, EntityCode, EntityName, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@EntityType, @EntityCode, @EntityName, @IsActive, @CreatedBy, GETDATE(), @ModifiedBy, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<long> InsertAddressAsync(Address address, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Address (AddressTypeId, AddressLine1, AddressLine2, AddressLine3, AddressLine4, AddressLine5,
                    Landmark, CityId, StateId, CountryId, PostalCode, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@AddressTypeId, @AddressLine1, @AddressLine2, @AddressLine3, @AddressLine4, @AddressLine5,
                    @Landmark, @CityId, @StateId, @CountryId, @PostalCode, @IsActive, @CreatedBy, GETDATE(), @ModifiedBy, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, address, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<long> InsertContactAsync(Contact contact, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Contact (ContactTypeId, ContactName, Designation, Email, Mobile, Phone, Website,
                    IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@ContactTypeId, @ContactName, @Designation, @Email, @Mobile, @Phone, @Website,
                    @IsActive, @CreatedBy, GETDATE(), @ModifiedBy, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, contact, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<long> InsertFileAsync(StoredFile file, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.[Files] (FileName, OriginalFileName, BucketName, ObjectKey, ContentType, Extension,
                    FileSize, StorageProvider, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@FileName, @OriginalFileName, @BucketName, @ObjectKey, @ContentType, @Extension,
                    @FileSize, @StorageProvider, @IsActive, @CreatedBy, GETDATE(), @ModifiedBy, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, file, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<long> InsertNoteAsync(Note note, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Note (NoteText, NoteType, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@NoteText, @NoteType, @IsActive, @CreatedBy, GETDATE(), @ModifiedBy, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, note, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<long> InsertTagAsync(Tag tag, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Tag (TagName, Color, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
                VALUES (@TagName, @Color, @IsActive, @CreatedBy, GETDATE(), @ModifiedBy, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, tag, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<Tag?> GetTagByNameAsync(string tagName, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            return await Sql.QuerySingleOrDefaultAsync<Tag>(conn,
                "SELECT * FROM dbo.Tag WHERE TagName = @tagName", new { tagName }, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<Entity?> GetByIdAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            return await Sql.QuerySingleOrDefaultAsync<Entity>(conn,
                "SELECT * FROM dbo.Entity WHERE EntityId = @entityId AND IsActive = 1",
                new { entityId }, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<IEnumerable<AddressDto>> GetAddressesAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            return await Sql.QueryAsync<AddressDto>(conn, @"
                SELECT a.AddressId, a.AddressTypeId, a.AddressLine1, a.AddressLine2, a.AddressLine3, a.AddressLine4, a.AddressLine5,
                       a.Landmark, a.CityId, a.StateId, a.CountryId, a.PostalCode, ea.IsPrimary
                FROM dbo.Address a
                INNER JOIN dbo.EntityAddress ea ON ea.AddressId = a.AddressId
                WHERE ea.EntityId = @entityId AND a.IsActive = 1 AND ea.IsActive = 1",
                new { entityId }, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<IEnumerable<ContactDto>> GetContactsAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            return await Sql.QueryAsync<ContactDto>(conn, @"
                SELECT c.ContactId, c.ContactTypeId, c.ContactName, c.Designation, c.Email, c.Mobile, c.Phone, c.Website, ec.IsPrimary
                FROM dbo.Contact c
                INNER JOIN dbo.EntityContact ec ON ec.ContactId = c.ContactId
                WHERE ec.EntityId = @entityId AND c.IsActive = 1 AND ec.IsActive = 1",
                new { entityId }, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<IEnumerable<FileDto>> GetFilesAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            return await Sql.QueryAsync<FileDto>(conn, @"
                SELECT f.FileId, ef.FileType, f.FileName, f.OriginalFileName, f.BucketName, f.ObjectKey, f.ContentType,
                       f.Extension, f.FileSize, f.StorageProvider, ef.IsPrimary
                FROM dbo.[Files] f
                INNER JOIN dbo.EntityFile ef ON ef.FileId = f.FileId
                WHERE ef.EntityId = @entityId AND f.IsActive = 1 AND ef.IsActive = 1",
                new { entityId }, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<IEnumerable<NoteDto>> GetNotesAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            return await Sql.QueryAsync<NoteDto>(conn, @"
                SELECT n.NoteId, n.NoteText, n.NoteType
                FROM dbo.Note n
                INNER JOIN dbo.EntityNote en ON en.NoteId = n.NoteId
                WHERE en.EntityId = @entityId AND n.IsActive = 1 AND en.IsActive = 1",
                new { entityId }, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<IEnumerable<TagDto>> GetTagsAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            return await Sql.QueryAsync<TagDto>(conn, @"
                SELECT t.TagId, t.TagName, t.Color
                FROM dbo.Tag t
                INNER JOIN dbo.EntityTag et ON et.TagId = t.TagId
                WHERE et.EntityId = @entityId AND t.IsActive = 1 AND et.IsActive = 1",
                new { entityId }, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task DeleteAddressesForEntityAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            await Sql.ExecuteAsync(conn, @"
                DELETE FROM dbo.EntityAddress WHERE EntityId = @entityId;
                DELETE FROM dbo.Address WHERE AddressId NOT IN (SELECT AddressId FROM dbo.EntityAddress);",
                new { entityId }, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task DeleteContactsForEntityAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            await Sql.ExecuteAsync(conn, @"
                DELETE FROM dbo.EntityContact WHERE EntityId = @entityId;
                DELETE FROM dbo.Contact WHERE ContactId NOT IN (SELECT ContactId FROM dbo.EntityContact);",
                new { entityId }, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task DeleteFilesForEntityAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            await Sql.ExecuteAsync(conn, @"
                DELETE FROM dbo.EntityFile WHERE EntityId = @entityId;
                DELETE FROM dbo.[Files] WHERE FileId NOT IN (SELECT FileId FROM dbo.EntityFile);",
                new { entityId }, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task DeleteNotesForEntityAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            await Sql.ExecuteAsync(conn, @"
                DELETE FROM dbo.EntityNote WHERE EntityId = @entityId;
                DELETE FROM dbo.Note WHERE NoteId NOT IN (SELECT NoteId FROM dbo.EntityNote);",
                new { entityId }, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task DeleteTagsForEntityAsync(long entityId, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            await Sql.ExecuteAsync(conn, @"
                DELETE FROM dbo.EntityTag WHERE EntityId = @entityId;
                DELETE FROM dbo.Tag WHERE TagId NOT IN (SELECT TagId FROM dbo.EntityTag);",
                new { entityId }, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<long> InsertEntityAddressAsync(EntityAddress link, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.EntityAddress (EntityId, AddressId, IsPrimary, IsActive, CreatedBy, CreatedAt)
                VALUES (@EntityId, @AddressId, @IsPrimary, @IsActive, @CreatedBy, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, link, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<long> InsertEntityContactAsync(EntityContact link, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.EntityContact (EntityId, ContactId, IsPrimary, IsActive, CreatedBy, CreatedAt)
                VALUES (@EntityId, @ContactId, @IsPrimary, @IsActive, @CreatedBy, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, link, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<long> InsertEntityFileAsync(EntityFile link, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.EntityFile (EntityId, FileId, FileType, IsPrimary, IsActive, CreatedBy, CreatedAt)
                VALUES (@EntityId, @FileId, @FileType, @IsPrimary, @IsActive, @CreatedBy, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, link, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<long> InsertEntityNoteAsync(EntityNote link, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.EntityNote (EntityId, NoteId, IsActive, CreatedBy, CreatedAt)
                VALUES (@EntityId, @NoteId, @IsActive, @CreatedBy, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, link, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<long> InsertEntityTagAsync(EntityTag link, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.EntityTag (EntityId, TagId, IsActive, CreatedBy, CreatedAt)
                VALUES (@EntityId, @TagId, @IsActive, @CreatedBy, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, link, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}