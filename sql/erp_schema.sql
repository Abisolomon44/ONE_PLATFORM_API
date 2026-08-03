/* =============================================================================
   ONE ERP - ERP Database Schema
   Purpose  : Executed automatically by ONEERP.Platform.API when provisioning
              a new tenant database (database-per-tenant architecture).
              NO GO statements here: the whole batch is executed with Dapper.
   ============================================================================= */

/* ---------------------------------------------------------------------------
   Countries (location master hierarchy: Countries -> States -> Cities)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Countries]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Countries (
        CountryId     INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Countries PRIMARY KEY,
        [Name]        NVARCHAR(100)       NOT NULL,
        ISOCode2      CHAR(2)             NOT NULL CONSTRAINT UQ_Countries_ISOCode2 UNIQUE,
        ISOCode3      CHAR(3)             NOT NULL CONSTRAINT UQ_Countries_ISOCode3 UNIQUE,
        PhoneCode     NVARCHAR(10)        NULL,
        CurrencyCode  NVARCHAR(10)        NULL,
        Nationality   NVARCHAR(100)       NULL,
        IsActive      BIT                 NOT NULL CONSTRAINT DF_Countries_IsActive DEFAULT 1,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_Countries_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_Countries_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_Countries_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_Countries_IsActive ON dbo.Countries (IsActive);
END
;

/* ---------------------------------------------------------------------------
   States
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[States]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.States (
        StateId       INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_States PRIMARY KEY,
        CountryId     INT                 NOT NULL,
        [Name]        NVARCHAR(100)       NOT NULL,
        StateCode     NVARCHAR(10)        NOT NULL,
        GSTStateCode  NVARCHAR(5)         NULL,
        IsActive      BIT                 NOT NULL CONSTRAINT DF_States_IsActive DEFAULT 1,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_States_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_States_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_States_IsDeleted DEFAULT 0,
        CONSTRAINT FK_States_Country FOREIGN KEY (CountryId) REFERENCES dbo.Countries (CountryId),
        CONSTRAINT UQ_States_Country_Code UNIQUE (CountryId, StateCode)
    );

    CREATE INDEX IX_States_CountryId ON dbo.States (CountryId);
    CREATE INDEX IX_States_IsActive ON dbo.States (IsActive);
END
;

/* ---------------------------------------------------------------------------
   Cities
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cities]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Cities (
        CityId        INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Cities PRIMARY KEY,
        CountryId     INT                 NOT NULL,
        StateId       INT                 NOT NULL,
        [Name]        NVARCHAR(100)       NOT NULL,
        PostalCode    NVARCHAR(10)        NULL,
        Latitude      DECIMAL(10,7)       NULL,
        Longitude     DECIMAL(10,7)       NULL,
        IsActive      BIT                 NOT NULL CONSTRAINT DF_Cities_IsActive DEFAULT 1,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_Cities_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_Cities_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_Cities_IsDeleted DEFAULT 0,
        CONSTRAINT FK_Cities_Country FOREIGN KEY (CountryId) REFERENCES dbo.Countries (CountryId),
        CONSTRAINT FK_Cities_State FOREIGN KEY (StateId) REFERENCES dbo.States (StateId),
        CONSTRAINT UQ_Cities_State_Name UNIQUE (StateId, [Name])
    );

    CREATE INDEX IX_Cities_StateId ON dbo.Cities (StateId);
    CREATE INDEX IX_Cities_CountryId ON dbo.Cities (CountryId);
    CREATE INDEX IX_Cities_IsActive ON dbo.Cities (IsActive);
END
;

/* ---------------------------------------------------------------------------
   Companies
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Companies]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Companies (
        CompanyId     INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Companies PRIMARY KEY,
        CompanyCode   NVARCHAR(50)        NOT NULL CONSTRAINT UQ_Companies_CompanyCode UNIQUE,
        CompanyName   NVARCHAR(200)       NOT NULL,
        [Address]     NVARCHAR(500)       NULL,
        Email         NVARCHAR(200)       NULL,
        Phone         NVARCHAR(50)        NULL,
        GST           NVARCHAR(50)        NULL,
        Currency      NVARCHAR(10)        NOT NULL CONSTRAINT DF_Companies_Currency DEFAULT 'USD',
        Status        NVARCHAR(20)        NOT NULL CONSTRAINT DF_Companies_Status DEFAULT 'Active',
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_Companies_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_Companies_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_Companies_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_Companies_Status ON dbo.Companies (Status);
END
;

/* ---------------------------------------------------------------------------
   Users
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Users (
        UserId        INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        CompanyId     INT                 NOT NULL,
        Username      NVARCHAR(100)       NOT NULL,
        Email         NVARCHAR(200)       NOT NULL,
        Mobile        NVARCHAR(50)        NULL,
        FullName      NVARCHAR(200)       NOT NULL,
        PasswordHash  NVARCHAR(255)       NOT NULL,
        Status        NVARCHAR(20)        NOT NULL CONSTRAINT DF_Users_Status DEFAULT 'Active',
        IsSuperAdmin  BIT                 NOT NULL CONSTRAINT DF_Users_IsSuperAdmin DEFAULT 0,
        LastLoginDate DATETIME2           NULL,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_Users_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_Users_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_Users_IsDeleted DEFAULT 0,
        CONSTRAINT FK_Users_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Companies (CompanyId),
        CONSTRAINT UQ_Users_Company_Username UNIQUE (CompanyId, Username)
    );

    CREATE INDEX IX_Users_CompanyId_Username ON dbo.Users (CompanyId, Username);
    CREATE INDEX IX_Users_Status ON dbo.Users (Status);
END
;

/* ---------------------------------------------------------------------------
   Roles
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Roles (
        RoleId        INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
        [Name]        NVARCHAR(100)       NOT NULL,
        Code          NVARCHAR(100)       NOT NULL CONSTRAINT UQ_Roles_Code UNIQUE,
        [Description] NVARCHAR(500)       NULL,
        IsSystem      BIT                 NOT NULL CONSTRAINT DF_Roles_IsSystem DEFAULT 0,
        IsActive      BIT                 NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT 1,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_Roles_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_Roles_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_Roles_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_Roles_IsActive ON dbo.Roles (IsActive);
END
;

/* ---------------------------------------------------------------------------
   UserRoles
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserRoles]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.UserRoles (
        UserRoleId    INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_UserRoles PRIMARY KEY,
        UserId        INT                 NOT NULL,
        RoleId        INT                 NOT NULL,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_UserRoles_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId),
        CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT UQ_UserRoles_User_Role UNIQUE (UserId, RoleId)
    );

    CREATE INDEX IX_UserRoles_UserId ON dbo.UserRoles (UserId);
    CREATE INDEX IX_UserRoles_RoleId ON dbo.UserRoles (RoleId);
END
;

/* ---------------------------------------------------------------------------
   RolePermissions (permission-based authorization)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.RolePermissions (
        RolePermissionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RolePermissions PRIMARY KEY,
        RoleId           INT               NOT NULL,
        PermissionCode   NVARCHAR(100)     NOT NULL,
        CreatedBy        NVARCHAR(100)     NULL,
        CreatedDate      DATETIME2         NOT NULL CONSTRAINT DF_RolePermissions_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT UQ_RolePermissions_Role_Permission UNIQUE (RoleId, PermissionCode)
    );

    CREATE INDEX IX_RolePermissions_RoleId ON dbo.RolePermissions (RoleId);
END
;

/* ---------------------------------------------------------------------------
   ApplicationSettings
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApplicationSettings]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.ApplicationSettings (
        SettingId     INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_ApplicationSettings PRIMARY KEY,
        SettingKey    NVARCHAR(100)       NOT NULL CONSTRAINT UQ_ApplicationSettings_Key UNIQUE,
        SettingValue  NVARCHAR(500)       NULL,
        [Description] NVARCHAR(500)       NULL,
        UpdatedBy     NVARCHAR(100)       NULL,
        UpdatedDate   DATETIME2           NOT NULL CONSTRAINT DF_ApplicationSettings_UpdatedDate DEFAULT SYSUTCDATETIME()
    );
END
;

/* ---------------------------------------------------------------------------
   RefreshTokens (ERP sessions)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RefreshTokens]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.RefreshTokens (
        RefreshTokenId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
        UserId         INT                  NOT NULL,
        Token          NVARCHAR(500)        NOT NULL,
        ExpiryDate     DATETIME2            NOT NULL,
        IsRevoked      BIT                  NOT NULL CONSTRAINT DF_RefreshTokens_IsRevoked DEFAULT 0,
        CreatedDate    DATETIME2            NOT NULL CONSTRAINT DF_RefreshTokens_CreatedDate DEFAULT SYSUTCDATETIME(),
        RevokedDate    DATETIME2            NULL,
        CONSTRAINT FK_RefreshTokens_User FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId)
    );

    CREATE INDEX IX_RefreshTokens_Token ON dbo.RefreshTokens (Token);
    CREATE INDEX IX_RefreshTokens_UserId ON dbo.RefreshTokens (UserId);
END
;

/* ---------------------------------------------------------------------------
   BusinessTypes (company business classification master)
-------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BusinessTypes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.BusinessTypes (
        BusinessTypeId INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_BusinessTypes PRIMARY KEY,
        [Name]         NVARCHAR(100)       NOT NULL CONSTRAINT UQ_BusinessTypes_Name UNIQUE,
        [Description]  NVARCHAR(250)       NULL,
        SortOrder      INT                 NOT NULL CONSTRAINT DF_BusinessTypes_SortOrder DEFAULT 1,
        IsActive       BIT                 NOT NULL CONSTRAINT DF_BusinessTypes_IsActive DEFAULT 1,
        CreatedBy      NVARCHAR(100)       NULL,
        CreatedDate    DATETIME2           NOT NULL CONSTRAINT DF_BusinessTypes_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy     NVARCHAR(100)       NULL,
        ModifiedDate   DATETIME2           NOT NULL CONSTRAINT DF_BusinessTypes_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted      BIT                 NOT NULL CONSTRAINT DF_BusinessTypes_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_BusinessTypes_SortOrder ON dbo.BusinessTypes (SortOrder);
    CREATE INDEX IX_BusinessTypes_IsActive ON dbo.BusinessTypes (IsActive);
END
;

/* ---------------------------------------------------------------------------
   IndustryTypes (company industry classification master)
-------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IndustryTypes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.IndustryTypes (
        IndustryTypeId INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_IndustryTypes PRIMARY KEY,
        [Name]         NVARCHAR(100)       NOT NULL CONSTRAINT UQ_IndustryTypes_Name UNIQUE,
        [Description]  NVARCHAR(250)       NULL,
        SortOrder      INT                 NOT NULL CONSTRAINT DF_IndustryTypes_SortOrder DEFAULT 1,
        IsActive       BIT                 NOT NULL CONSTRAINT DF_IndustryTypes_IsActive DEFAULT 1,
        CreatedBy      NVARCHAR(100)       NULL,
        CreatedDate    DATETIME2           NOT NULL CONSTRAINT DF_IndustryTypes_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy     NVARCHAR(100)       NULL,
        ModifiedDate   DATETIME2           NOT NULL CONSTRAINT DF_IndustryTypes_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted      BIT                 NOT NULL CONSTRAINT DF_IndustryTypes_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_IndustryTypes_SortOrder ON dbo.IndustryTypes (SortOrder);
    CREATE INDEX IX_IndustryTypes_IsActive ON dbo.IndustryTypes (IsActive);
END
;

/* ---------------------------------------------------------------------------
   CompanyGroups (corporate ownership / company grouping master)
-------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CompanyGroups]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.CompanyGroups (
        CompanyGroupId INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_CompanyGroups PRIMARY KEY,
        GroupCode      NVARCHAR(20)        NOT NULL CONSTRAINT UQ_CompanyGroups_GroupCode UNIQUE,
        GroupName      NVARCHAR(200)       NOT NULL,
        ShortName      NVARCHAR(100)       NULL,
        [Description]  NVARCHAR(500)       NULL,
        ParentGroupId  INT                 NULL,
        IsActive       BIT                 NOT NULL CONSTRAINT DF_CompanyGroups_IsActive DEFAULT 1,
        IsDeleted      BIT                 NOT NULL CONSTRAINT DF_CompanyGroups_IsDeleted DEFAULT 0,
        CreatedBy      NVARCHAR(100)       NULL,
        CreatedDate    DATETIME2           NOT NULL CONSTRAINT DF_CompanyGroups_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy     NVARCHAR(100)       NULL,
        ModifiedDate   DATETIME2           NOT NULL CONSTRAINT DF_CompanyGroups_ModifiedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_CompanyGroups_Parent
            FOREIGN KEY (ParentGroupId) REFERENCES dbo.CompanyGroups (CompanyGroupId)
    );

    CREATE INDEX IX_CompanyGroups_ParentGroupId ON dbo.CompanyGroups (ParentGroupId);
    CREATE INDEX IX_CompanyGroups_IsActive ON dbo.CompanyGroups (IsActive);
END
;

/* ---------------------------------------------------------------------------
   AuditLogs (ERP)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.AuditLogs (
        AuditLogId    BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
        EntityName    NVARCHAR(100)        NOT NULL,
        EntityId      NVARCHAR(50)         NULL,
        [Action]      NVARCHAR(50)         NOT NULL,
        PerformedBy   NVARCHAR(100)        NULL,
        PerformedDate DATETIME2            NOT NULL CONSTRAINT DF_AuditLogs_PerformedDate DEFAULT SYSUTCDATETIME(),
        OldValues     NVARCHAR(MAX)        NULL,
        NewValues     NVARCHAR(MAX)        NULL,
        IpAddress     NVARCHAR(50)         NULL
    );

    CREATE INDEX IX_AuditLogs_EntityDate ON dbo.AuditLogs (EntityName, PerformedDate);
END
;

/* ---------------------------------------------------------------------------
   Languages (system master: supported UI/localization languages)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Languages]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Languages (
        LanguageId    INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Languages PRIMARY KEY,
        [Name]        NVARCHAR(100)       NOT NULL,
        Code          NVARCHAR(10)        NOT NULL CONSTRAINT UQ_Languages_Code UNIQUE,
        CultureCode   NVARCHAR(20)        NULL,
        IsRTL         BIT                 NOT NULL CONSTRAINT DF_Languages_IsRTL DEFAULT 0,
        IsDefault     BIT                 NOT NULL CONSTRAINT DF_Languages_IsDefault DEFAULT 0,
        SortOrder     INT                 NOT NULL CONSTRAINT DF_Languages_SortOrder DEFAULT 1,
        IsActive      BIT                 NOT NULL CONSTRAINT DF_Languages_IsActive DEFAULT 1,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_Languages_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_Languages_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_Languages_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_Languages_SortOrder ON dbo.Languages (SortOrder);
    CREATE INDEX IX_Languages_IsActive ON dbo.Languages (IsActive);
END
;

/* ---------------------------------------------------------------------------
   TimeZones (system master: supported time zones)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TimeZones]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.TimeZones (
        TimeZoneId    INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_TimeZones PRIMARY KEY,
        [Name]        NVARCHAR(100)       NOT NULL,
        TimeZoneName  NVARCHAR(100)       NOT NULL CONSTRAINT UQ_TimeZones_TimeZoneName UNIQUE,
        UTCOffset     NVARCHAR(20)        NULL,
        IsActive      BIT                 NOT NULL CONSTRAINT DF_TimeZones_IsActive DEFAULT 1,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_TimeZones_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_TimeZones_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_TimeZones_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_TimeZones_IsActive ON dbo.TimeZones (IsActive);
END
;

/* ---------------------------------------------------------------------------
   GSTRegistrationTypes (system master: GST registration classification)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GSTRegistrationTypes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.GSTRegistrationTypes (
        GSTRegistrationTypeId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_GSTRegistrationTypes PRIMARY KEY,
        [Name]        NVARCHAR(100)       NOT NULL CONSTRAINT UQ_GSTRegistrationTypes_Name UNIQUE,
        [Description] NVARCHAR(250)       NULL,
        IsActive      BIT                 NOT NULL CONSTRAINT DF_GSTRegistrationTypes_IsActive DEFAULT 1,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_GSTRegistrationTypes_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_GSTRegistrationTypes_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_GSTRegistrationTypes_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_GSTRegistrationTypes_IsActive ON dbo.GSTRegistrationTypes (IsActive);
END
;

/* ---------------------------------------------------------------------------
   AddressTypes (system master: address classifications)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AddressTypes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.AddressTypes (
        AddressTypeId INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_AddressTypes PRIMARY KEY,
        [Name]        NVARCHAR(100)       NOT NULL CONSTRAINT UQ_AddressTypes_Name UNIQUE,
        [Description] NVARCHAR(250)       NULL,
        IsActive      BIT                 NOT NULL CONSTRAINT DF_AddressTypes_IsActive DEFAULT 1,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_AddressTypes_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_AddressTypes_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_AddressTypes_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_AddressTypes_IsActive ON dbo.AddressTypes (IsActive);
END
;

/* ---------------------------------------------------------------------------
   ContactTypes (system master: contact classifications)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ContactTypes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.ContactTypes (
        ContactTypeId INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_ContactTypes PRIMARY KEY,
        [Name]        NVARCHAR(100)       NOT NULL CONSTRAINT UQ_ContactTypes_Name UNIQUE,
        [Description] NVARCHAR(250)       NULL,
        IsActive      BIT                 NOT NULL CONSTRAINT DF_ContactTypes_IsActive DEFAULT 1,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_ContactTypes_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_ContactTypes_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_ContactTypes_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_ContactTypes_IsActive ON dbo.ContactTypes (IsActive);
END
;

/* ---------------------------------------------------------------------------
   DocumentTypes (system master: document classifications)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocumentTypes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.DocumentTypes (
        DocumentTypeId INT IDENTITY(1,1)  NOT NULL CONSTRAINT PK_DocumentTypes PRIMARY KEY,
        [Name]        NVARCHAR(100)       NOT NULL CONSTRAINT UQ_DocumentTypes_Name UNIQUE,
        [Description] NVARCHAR(250)       NULL,
        IsActive      BIT                 NOT NULL CONSTRAINT DF_DocumentTypes_IsActive DEFAULT 1,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_DocumentTypes_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_DocumentTypes_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_DocumentTypes_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_DocumentTypes_IsActive ON dbo.DocumentTypes (IsActive);
END
;

/* ---------------------------------------------------------------------------
   OrganizationTypes (system master: organization structure classification)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrganizationTypes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.OrganizationTypes (
        OrganizationTypeId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrganizationTypes PRIMARY KEY,
        [Name]        NVARCHAR(100)       NOT NULL CONSTRAINT UQ_OrganizationTypes_Name UNIQUE,
        Code          NVARCHAR(30)        NOT NULL CONSTRAINT UQ_OrganizationTypes_Code UNIQUE,
        [Description] NVARCHAR(250)       NULL,
        SortOrder     INT                 NOT NULL CONSTRAINT DF_OrganizationTypes_SortOrder DEFAULT 1,
        IsActive      BIT                 NOT NULL CONSTRAINT DF_OrganizationTypes_IsActive DEFAULT 1,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_OrganizationTypes_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_OrganizationTypes_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_OrganizationTypes_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_OrganizationTypes_SortOrder ON dbo.OrganizationTypes (SortOrder);
    CREATE INDEX IX_OrganizationTypes_IsActive ON dbo.OrganizationTypes (IsActive);
END
;
