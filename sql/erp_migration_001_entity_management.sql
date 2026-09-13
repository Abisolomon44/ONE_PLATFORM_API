/* =============================================================================
   ONE ERP - ERP Migration: Universal Entity (Address / Contact / File / Note / Tag)
   Purpose  : Adds the polymorphic Entity layer used by every master (Company,
              Branch, Warehouse, Store, Product, Service, Employee, Customer/
              Supplier via BusinessPartners). One Entity has many Addresses,
              Contacts, Files, Notes and Tags, connected through junction tables.
              Business masters reference the common Entity through an EntityId
              column (added by ALTER at the end). AddressTypeId / ContactTypeId
              reuse the existing AddressTypes / ContactTypes system masters.
   Target DB: ONE ERP tenant database (run per tenant, database-per-tenant).
   Safe     : Re-runnable (IF NOT EXISTS guards on every object / column / FK).
   How      : Run with the tenant connection, e.g. via sqlcmd:
                sqlcmd -S LAPTOP-BOR8IKB8\MSSQL2022 -d <TENANT_DB> -E -i erp_migration_001_entity_management.sql
   ============================================================================= */

/* ---------------------------------------------------------------------------
   1. Entity - common identity (COMPANY, BRANCH, WAREHOUSE, STORE, PRODUCT,
      SERVICE, EMPLOYEE, CUSTOMER, SUPPLIER ...)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Entity]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Entity (
        EntityId   BIGINT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Entity PRIMARY KEY,
        EntityType VARCHAR(30)            NOT NULL,
        EntityCode VARCHAR(50)            NOT NULL,
        EntityName VARCHAR(200)           NOT NULL,
        IsActive   BIT                    NOT NULL CONSTRAINT DF_Entity_IsActive DEFAULT 1,
        CreatedBy  BIGINT                 NULL,
        CreatedAt  DATETIME               NOT NULL CONSTRAINT DF_Entity_CreatedAt DEFAULT GETDATE(),
        ModifiedBy BIGINT                 NULL,
        ModifiedAt DATETIME               NULL,
        CONSTRAINT UQ_Entity_Type_Code UNIQUE (EntityType, EntityCode)
    );

    CREATE INDEX IX_Entity_EntityType ON dbo.Entity (EntityType);
    CREATE INDEX IX_Entity_IsActive ON dbo.Entity (IsActive);
END
;

/* ---------------------------------------------------------------------------
   2. Address - reusable postal address.
      AddressTypeId reuses the existing dbo.AddressTypes system master.
      CityId / StateId / CountryId reuses the existing location masters.
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Address]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Address (
        AddressId    BIGINT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Address PRIMARY KEY,
        AddressTypeId BIGINT                NULL,
        AddressLine1 VARCHAR(200)           NULL,
        AddressLine2 VARCHAR(200)           NULL,
        AddressLine3 VARCHAR(200)           NULL,
        AddressLine4 VARCHAR(200)           NULL,
        AddressLine5 VARCHAR(200)           NULL,
        Landmark     VARCHAR(200)           NULL,
        CityId       BIGINT                 NULL,
        StateId      BIGINT                 NULL,
        CountryId    BIGINT                 NULL,
        PostalCode   VARCHAR(20)            NULL,
        IsActive     BIT                    NOT NULL CONSTRAINT DF_Address_IsActive DEFAULT 1,
        CreatedBy    BIGINT                 NULL,
        CreatedAt    DATETIME               NOT NULL CONSTRAINT DF_Address_CreatedAt DEFAULT GETDATE(),
        ModifiedBy   BIGINT                 NULL,
        ModifiedAt   DATETIME               NULL
    );

    CREATE INDEX IX_Address_CityId ON dbo.Address (CityId);
    CREATE INDEX IX_Address_StateId ON dbo.Address (StateId);
    CREATE INDEX IX_Address_CountryId ON dbo.Address (CountryId);
    CREATE INDEX IX_Address_IsActive ON dbo.Address (IsActive);
END
;

/* ---------------------------------------------------------------------------
   3. EntityAddress - junction: entity ↔ address
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EntityAddress]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.EntityAddress (
        EntityAddressId BIGINT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_EntityAddress PRIMARY KEY,
        EntityId        BIGINT                 NOT NULL,
        AddressId       BIGINT                 NOT NULL,
        IsPrimary       BIT                    NOT NULL CONSTRAINT DF_EntityAddress_IsPrimary DEFAULT 0,
        IsActive        BIT                    NOT NULL CONSTRAINT DF_EntityAddress_IsActive DEFAULT 1,
        CreatedBy       BIGINT                 NULL,
        CreatedAt       DATETIME               NOT NULL CONSTRAINT DF_EntityAddress_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_EntityAddress_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId),
        CONSTRAINT FK_EntityAddress_Address FOREIGN KEY (AddressId) REFERENCES dbo.Address (AddressId),
        CONSTRAINT UQ_EntityAddress UNIQUE (EntityId, AddressId)
    );

    CREATE INDEX IX_EntityAddress_EntityId ON dbo.EntityAddress (EntityId);
    CREATE INDEX IX_EntityAddress_AddressId ON dbo.EntityAddress (AddressId);
END
;

/* ---------------------------------------------------------------------------
   4. Contact - reusable person/contact row.
      ContactTypeId reuses the existing dbo.ContactTypes system master.
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Contact]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Contact (
        ContactId    BIGINT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Contact PRIMARY KEY,
        ContactTypeId BIGINT                NULL,
        ContactName  VARCHAR(200)           NOT NULL,
        Designation  VARCHAR(100)           NULL,
        Email        VARCHAR(200)           NULL,
        Mobile       VARCHAR(30)            NULL,
        Phone        VARCHAR(30)            NULL,
        Website      VARCHAR(300)           NULL,
        IsActive     BIT                    NOT NULL CONSTRAINT DF_Contact_IsActive DEFAULT 1,
        CreatedBy    BIGINT                 NULL,
        CreatedAt    DATETIME               NOT NULL CONSTRAINT DF_Contact_CreatedAt DEFAULT GETDATE(),
        ModifiedBy   BIGINT                 NULL,
        ModifiedAt   DATETIME               NULL
    );

    CREATE INDEX IX_Contact_ContactTypeId ON dbo.Contact (ContactTypeId);
    CREATE INDEX IX_Contact_IsActive ON dbo.Contact (IsActive);
END
;

/* ---------------------------------------------------------------------------
   5. EntityContact - junction: entity ↔ contact
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EntityContact]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.EntityContact (
        EntityContactId BIGINT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_EntityContact PRIMARY KEY,
        EntityId        BIGINT                 NOT NULL,
        ContactId       BIGINT                 NOT NULL,
        IsPrimary       BIT                    NOT NULL CONSTRAINT DF_EntityContact_IsPrimary DEFAULT 0,
        IsActive        BIT                    NOT NULL CONSTRAINT DF_EntityContact_IsActive DEFAULT 1,
        CreatedBy       BIGINT                 NULL,
        CreatedAt       DATETIME               NOT NULL CONSTRAINT DF_EntityContact_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_EntityContact_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId),
        CONSTRAINT FK_EntityContact_Contact FOREIGN KEY (ContactId) REFERENCES dbo.Contact (ContactId),
        CONSTRAINT UQ_EntityContact UNIQUE (EntityId, ContactId)
    );

    CREATE INDEX IX_EntityContact_EntityId ON dbo.EntityContact (EntityId);
    CREATE INDEX IX_EntityContact_ContactId ON dbo.EntityContact (ContactId);
END
;

/* ---------------------------------------------------------------------------
   6. Files - MinIO / S3 stored-object metadata only (binary bytes live in the
      object store; the DB row is just metadata).
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Files]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.[Files] (
        FileId           BIGINT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Files PRIMARY KEY,
        FileName         VARCHAR(255)           NOT NULL,
        OriginalFileName VARCHAR(255)           NOT NULL,
        BucketName       VARCHAR(100)           NOT NULL,
        ObjectKey        VARCHAR(1000)          NOT NULL,
        ContentType      VARCHAR(150)           NULL,
        Extension        VARCHAR(20)            NULL,
        FileSize         BIGINT                 NOT NULL,
        StorageProvider  VARCHAR(30)            NOT NULL CONSTRAINT DF_Files_StorageProvider DEFAULT 'MINIO',
        IsActive         BIT                    NOT NULL CONSTRAINT DF_Files_IsActive DEFAULT 1,
        CreatedBy        BIGINT                 NULL,
        CreatedAt        DATETIME               NOT NULL CONSTRAINT DF_Files_CreatedAt DEFAULT GETDATE(),
        ModifiedBy       BIGINT                 NULL,
        ModifiedAt       DATETIME               NULL
    );

    CREATE INDEX IX_Files_BucketName ON dbo.[Files] (BucketName);
    CREATE INDEX IX_Files_IsActive ON dbo.[Files] (IsActive);
END
;

/* ---------------------------------------------------------------------------
   7. EntityFile - junction: entity ↔ file (FileType: LOGO / DOCUMENT / IMAGE / ATTACHMENT)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EntityFile]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.EntityFile (
        EntityFileId BIGINT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_EntityFile PRIMARY KEY,
        EntityId     BIGINT                 NOT NULL,
        FileId       BIGINT                 NOT NULL,
        FileType     VARCHAR(30)            NOT NULL,
        IsPrimary    BIT                    NOT NULL CONSTRAINT DF_EntityFile_IsPrimary DEFAULT 0,
        IsActive     BIT                    NOT NULL CONSTRAINT DF_EntityFile_IsActive DEFAULT 1,
        CreatedBy    BIGINT                 NULL,
        CreatedAt    DATETIME               NOT NULL CONSTRAINT DF_EntityFile_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_EntityFile_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId),
        CONSTRAINT FK_EntityFile_File FOREIGN KEY (FileId) REFERENCES dbo.[Files] (FileId)
    );

    CREATE INDEX IX_EntityFile_EntityId ON dbo.EntityFile (EntityId);
    CREATE INDEX IX_EntityFile_FileId ON dbo.EntityFile (FileId);
END
;

/* ---------------------------------------------------------------------------
   8. Note - common note row
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Note]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Note (
        NoteId     BIGINT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Note PRIMARY KEY,
        NoteText   VARCHAR(2000)          NOT NULL,
        NoteType   VARCHAR(30)            NULL,
        IsActive   BIT                    NOT NULL CONSTRAINT DF_Note_IsActive DEFAULT 1,
        CreatedBy  BIGINT                 NULL,
        CreatedAt  DATETIME               NOT NULL CONSTRAINT DF_Note_CreatedAt DEFAULT GETDATE(),
        ModifiedBy BIGINT                 NULL,
        ModifiedAt DATETIME               NULL
    );

    CREATE INDEX IX_Note_IsActive ON dbo.Note (IsActive);
END
;

/* ---------------------------------------------------------------------------
   9. EntityNote - junction: entity ↔ note
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EntityNote]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.EntityNote (
        EntityNoteId BIGINT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_EntityNote PRIMARY KEY,
        EntityId     BIGINT                 NOT NULL,
        NoteId       BIGINT                 NOT NULL,
        IsActive     BIT                    NOT NULL CONSTRAINT DF_EntityNote_IsActive DEFAULT 1,
        CreatedBy    BIGINT                 NULL,
        CreatedAt    DATETIME               NOT NULL CONSTRAINT DF_EntityNote_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_EntityNote_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId),
        CONSTRAINT FK_EntityNote_Note FOREIGN KEY (NoteId) REFERENCES dbo.Note (NoteId),
        CONSTRAINT UQ_EntityNote UNIQUE (EntityId, NoteId)
    );

    CREATE INDEX IX_EntityNote_EntityId ON dbo.EntityNote (EntityId);
    CREATE INDEX IX_EntityNote_NoteId ON dbo.EntityNote (NoteId);
END
;

/* ---------------------------------------------------------------------------
   10. Tag - common tag row
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tag]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Tag (
        TagId      BIGINT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Tag PRIMARY KEY,
        TagName    VARCHAR(100)           NOT NULL,
        Color      VARCHAR(20)            NULL,
        IsActive   BIT                    NOT NULL CONSTRAINT DF_Tag_IsActive DEFAULT 1,
        CreatedBy  BIGINT                 NULL,
        CreatedAt  DATETIME               NOT NULL CONSTRAINT DF_Tag_CreatedAt DEFAULT GETDATE(),
        ModifiedBy BIGINT                 NULL,
        ModifiedAt DATETIME               NULL,
        CONSTRAINT UQ_Tag_Name UNIQUE (TagName)
    );

    CREATE INDEX IX_Tag_IsActive ON dbo.Tag (IsActive);
END
;

/* ---------------------------------------------------------------------------
   11. EntityTag - junction: entity ↔ tag
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EntityTag]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.EntityTag (
        EntityTagId BIGINT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_EntityTag PRIMARY KEY,
        EntityId    BIGINT                 NOT NULL,
        TagId       BIGINT                 NOT NULL,
        IsActive    BIT                    NOT NULL CONSTRAINT DF_EntityTag_IsActive DEFAULT 1,
        CreatedBy   BIGINT                 NULL,
        CreatedAt   DATETIME               NOT NULL CONSTRAINT DF_EntityTag_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_EntityTag_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId),
        CONSTRAINT FK_EntityTag_Tag FOREIGN KEY (TagId) REFERENCES dbo.Tag (TagId),
        CONSTRAINT UQ_EntityTag UNIQUE (EntityId, TagId)
    );

    CREATE INDEX IX_EntityTag_EntityId ON dbo.EntityTag (EntityId);
    CREATE INDEX IX_EntityTag_TagId ON dbo.EntityTag (TagId);
END
;

/* ---------------------------------------------------------------------------
   12. Business Masters → EntityId
       Adds a nullable FK column on each business master so the master keeps
       its own columns and simply points to the common Entity row that owns its
       Addresses / Contacts / Files / Notes / Tags.
       Customers & Suppliers live in dbo.BusinessPartners (no separate
       Customer/Supplier tables in this schema), Employees in dbo.Employees.
--------------------------------------------------------------------------- */

-- Companies
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Companies]') AND name = N'EntityId')
    ALTER TABLE dbo.Companies ADD EntityId BIGINT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Companies_Entity')
    ALTER TABLE dbo.Companies ADD CONSTRAINT FK_Companies_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

-- Branches
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Branches]') AND name = N'EntityId')
    ALTER TABLE dbo.Branches ADD EntityId BIGINT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Branches_Entity')
    ALTER TABLE dbo.Branches ADD CONSTRAINT FK_Branches_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

-- Warehouses
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Warehouses]') AND name = N'EntityId')
    ALTER TABLE dbo.Warehouses ADD EntityId BIGINT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Warehouses_Entity')
    ALTER TABLE dbo.Warehouses ADD CONSTRAINT FK_Warehouses_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

-- Stores
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Stores]') AND name = N'EntityId')
    ALTER TABLE dbo.Stores ADD EntityId BIGINT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Stores_Entity')
    ALTER TABLE dbo.Stores ADD CONSTRAINT FK_Stores_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

-- Products
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = N'EntityId')
    ALTER TABLE dbo.Products ADD EntityId BIGINT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Products_Entity')
    ALTER TABLE dbo.Products ADD CONSTRAINT FK_Products_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

-- Services
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = N'EntityId')
    ALTER TABLE dbo.Services ADD EntityId BIGINT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Services_Entity')
    ALTER TABLE dbo.Services ADD CONSTRAINT FK_Services_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

-- Employees (user referenced dbo.Employee; schema uses dbo.Employees)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND name = N'EntityId')
    ALTER TABLE dbo.Employees ADD EntityId BIGINT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Employees_Entity')
    ALTER TABLE dbo.Employees ADD CONSTRAINT FK_Employees_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

-- BusinessPartners (customers & suppliers; no separate Customer/Supplier tables)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessPartners]') AND name = N'EntityId')
    ALTER TABLE dbo.BusinessPartners ADD EntityId BIGINT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BusinessPartners_Entity')
    ALTER TABLE dbo.BusinessPartners ADD CONSTRAINT FK_BusinessPartners_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);