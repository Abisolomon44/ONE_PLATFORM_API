/* =============================================================================
   ONE ERP - ERP Combined Schema + Seed (single batch)
   Combines erp_schema.sql + erp_seed.sql into one provision file.
   Executed automatically by ONEERP.Platform.API when provisioning a new
   tenant database (database-per-tenant architecture).
   NO GO statements here: the whole batch is executed with Dapper.

   Parameterized by the provisioning service:
     @CompanyCode, @CompanyName, @CompanyEmail,
     @CurrencyCode, @AdminUsername, @AdminPasswordHash, @AdminFullName
   ============================================================================= */

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
    BusinessTypes (company business classification master)
    --------------------------------------------------------------------------- */
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
    BusinessPartnerRoles (business partner role classification master)
    --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BusinessPartnerRoles]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.BusinessPartnerRoles (
        BusinessPartnerRoleId INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_BusinessPartnerRoles PRIMARY KEY,
        [Code]        NVARCHAR(50)       NOT NULL CONSTRAINT UQ_BusinessPartnerRoles_Code UNIQUE,
        [Name]        NVARCHAR(100)      NOT NULL,
        [Description] NVARCHAR(250)      NULL,
        IsActive      BIT                NOT NULL CONSTRAINT DF_BusinessPartnerRoles_IsActive DEFAULT 1,
        CreatedBy     NVARCHAR(100)      NULL,
        CreatedDate   DATETIME2          NOT NULL CONSTRAINT DF_BusinessPartnerRoles_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)      NULL,
        ModifiedDate  DATETIME2          NOT NULL CONSTRAINT DF_BusinessPartnerRoles_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                NOT NULL CONSTRAINT DF_BusinessPartnerRoles_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_BusinessPartnerRoles_IsActive ON dbo.BusinessPartnerRoles (IsActive);
END
;

/* ---------------------------------------------------------------------------
    IndustryTypes (company industry classification master)
    --------------------------------------------------------------------------- */
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
    Currencies (system master: supported currencies with ISO codes and symbols)
    --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Currencies]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Currencies (
        Id            INT IDENTITY(1,1)      NOT NULL CONSTRAINT PK_Currencies PRIMARY KEY,
        CurrencyCode  NVARCHAR(10)           NOT NULL CONSTRAINT UQ_Currencies_CurrencyCode UNIQUE,
        CurrencyName  NVARCHAR(100)          NOT NULL,
        Symbol        NVARCHAR(10)           NOT NULL,
        ISOCode       NVARCHAR(10)           NULL,
        DecimalPlaces TINYINT                NOT NULL CONSTRAINT DF_Currencies_DecimalPlaces DEFAULT 2,
        IsBaseCurrency BIT                  NOT NULL CONSTRAINT DF_Currencies_IsBaseCurrency DEFAULT 0,
        SortOrder     INT                    NOT NULL CONSTRAINT DF_Currencies_SortOrder DEFAULT 1,
        IsActive      BIT                    NOT NULL CONSTRAINT DF_Currencies_IsActive DEFAULT 1,
        CreatedBy     INT                    NULL,
        CreatedDate   DATETIME2              NOT NULL CONSTRAINT DF_Currencies_CreatedDate DEFAULT GETDATE(),
        ModifiedBy    INT                    NULL,
        ModifiedDate  DATETIME2              NULL
    );

    CREATE INDEX IX_Currencies_CurrencyCode ON dbo.Currencies (CurrencyCode);
    CREATE INDEX IX_Currencies_IsBaseCurrency ON dbo.Currencies (IsBaseCurrency);
    CREATE INDEX IX_Currencies_SortOrder ON dbo.Currencies (SortOrder);
    CREATE INDEX IX_Currencies_IsActive ON dbo.Currencies (IsActive);
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
    Companies
    --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Companies]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Companies (
        Id                      INT IDENTITY(1,1)  NOT NULL CONSTRAINT PK_Companies PRIMARY KEY,
        CompanyCode             NVARCHAR(20)       NOT NULL CONSTRAINT UQ_Companies_CompanyCode UNIQUE,
        CompanyName             NVARCHAR(200)      NOT NULL,
        ShortName               NVARCHAR(100)      NULL,
        Abbreviation            NVARCHAR(20)       NULL,
        BusinessTypeId          INT                NOT NULL,
        IndustryTypeId          INT                NOT NULL,
        GSTRegistrationTypeId   INT                NULL,
        GSTNumber               NVARCHAR(20)       NULL,
        PANNumber               NVARCHAR(20)       NULL,
        TANNumber               NVARCHAR(20)       NULL,
        CINNumber               NVARCHAR(30)       NULL,
        RegistrationNumber      NVARCHAR(100)      NULL,
        CurrencyId              INT                NOT NULL,
        LanguageId              INT                NOT NULL,
        TimeZoneId              INT                NOT NULL,
        IsActive                BIT                NOT NULL CONSTRAINT DF_Companies_IsActive DEFAULT 1,
        IsBlocked               BIT                NOT NULL CONSTRAINT DF_Companies_IsBlocked DEFAULT 0,
        IsDeleted               BIT                NOT NULL CONSTRAINT DF_Companies_IsDeleted DEFAULT 0,
        LastLoginDate           DATETIME2          NULL,
        CreatedBy               INT                NOT NULL,
        CreatedDate             DATETIME2          NOT NULL CONSTRAINT DF_Companies_CreatedDate DEFAULT GETDATE(),
        ModifiedBy              INT                NULL,
        ModifiedDate            DATETIME2          NULL,
        CONSTRAINT FK_Companies_BusinessType FOREIGN KEY (BusinessTypeId) REFERENCES dbo.BusinessTypes (BusinessTypeId),
        CONSTRAINT FK_Companies_IndustryType FOREIGN KEY (IndustryTypeId) REFERENCES dbo.IndustryTypes (IndustryTypeId),
        CONSTRAINT FK_Companies_GSTRegistrationType FOREIGN KEY (GSTRegistrationTypeId) REFERENCES dbo.GSTRegistrationTypes (GSTRegistrationTypeId),
        CONSTRAINT FK_Companies_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.Currencies (Id),
        CONSTRAINT FK_Companies_Language FOREIGN KEY (LanguageId) REFERENCES dbo.Languages (LanguageId),
        CONSTRAINT FK_Companies_TimeZone FOREIGN KEY (TimeZoneId) REFERENCES dbo.TimeZones (TimeZoneId)
    );

    CREATE INDEX IX_Companies_IsActive ON dbo.Companies (IsActive);
    CREATE INDEX IX_Companies_BusinessTypeId ON dbo.Companies (BusinessTypeId);
    CREATE INDEX IX_Companies_IndustryTypeId ON dbo.Companies (IndustryTypeId);
    CREATE INDEX IX_Companies_CurrencyId ON dbo.Companies (CurrencyId);
END
;
;

/* ---------------------------------------------------------------------------
    Companies - Extended Columns (ALTER TABLE Migration)
    --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Companies') AND name = 'CompanyGroupId')
BEGIN
    ALTER TABLE dbo.Companies
    ADD
        CompanyGroupId INT NULL,
        BusinessUnitId INT NULL,
        Website NVARCHAR(200) NULL,
        Email NVARCHAR(150) NULL,
        Phone NVARCHAR(30) NULL,
        Mobile NVARCHAR(30) NULL,
        LogoUrl NVARCHAR(500) NULL,
        DefaultFinancialYearId INT NULL,
        MultiBranchEnabled BIT NOT NULL CONSTRAINT DF_Companies_MultiBranchEnabled DEFAULT 1,
        MultiWarehouseEnabled BIT NOT NULL CONSTRAINT DF_Companies_MultiWarehouseEnabled DEFAULT 1,
        MultiCurrencyEnabled BIT NOT NULL CONSTRAINT DF_Companies_MultiCurrencyEnabled DEFAULT 0,
        DateFormat NVARCHAR(20) NULL,
        TimeFormat NVARCHAR(20) NULL,
        NumberFormat NVARCHAR(20) NULL,
        DefaultWarehouseId INT NULL,
        Theme NVARCHAR(50) NULL,
        PrimaryColor NVARCHAR(20) NULL,
        SecondaryColor NVARCHAR(20) NULL,
        Remarks NVARCHAR(1000) NULL;
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
        CONSTRAINT FK_Users_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Companies (Id),
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
   RolePermissionsLegacy (legacy flat permission codes - read by RoleRepository)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissionsLegacy]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.RolePermissionsLegacy (
        RolePermissionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RolePermissionsLegacy PRIMARY KEY,
        RoleId           INT               NOT NULL,
        PermissionCode   NVARCHAR(100)     NOT NULL,
        CreatedBy        NVARCHAR(100)     NULL,
        CreatedDate      DATETIME2         NOT NULL CONSTRAINT DF_RolePermissionsLegacy_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_RolePermissionsLegacy_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT UQ_RolePermissionsLegacy_Role_Permission UNIQUE (RoleId, PermissionCode)
    );

    CREATE INDEX IX_RolePermissionsLegacy_RoleId ON dbo.RolePermissionsLegacy (RoleId);
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
    PermissionModules (hierarchical permission modules)
    --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PermissionModules]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PermissionModules (
        Id               INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_PermissionModules PRIMARY KEY,
        Code             NVARCHAR(100)       NOT NULL CONSTRAINT UQ_PermissionModules_Code UNIQUE,
        [Name]           NVARCHAR(200)       NOT NULL,
        ParentId         INT                 NULL,
        Level            NVARCHAR(20)        NOT NULL,
        SortOrder        INT                 NOT NULL CONSTRAINT DF_PermissionModules_SortOrder DEFAULT 0,
        IsVisible        BIT                 NOT NULL CONSTRAINT DF_PermissionModules_IsVisible DEFAULT 1,
        Icon             NVARCHAR(100)       NULL,
        RoutePath        NVARCHAR(200)       NULL,
        CreatedBy        NVARCHAR(100)       NULL,
        CreatedDate      DATETIME2           NOT NULL CONSTRAINT DF_PermissionModules_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy       NVARCHAR(100)       NULL,
        ModifiedDate     DATETIME2           NOT NULL CONSTRAINT DF_PermissionModules_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted        BIT                 NOT NULL CONSTRAINT DF_PermissionModules_IsDeleted DEFAULT 0,
        CONSTRAINT FK_PermissionModules_Parent FOREIGN KEY (ParentId) REFERENCES dbo.PermissionModules (Id)
    );

    CREATE INDEX IX_PermissionModules_ParentId ON dbo.PermissionModules (ParentId);
    CREATE INDEX IX_PermissionModules_Level ON dbo.PermissionModules (Level);
    CREATE INDEX IX_PermissionModules_IsVisible ON dbo.PermissionModules (IsVisible);
    CREATE INDEX IX_PermissionModules_SortOrder ON dbo.PermissionModules (SortOrder);
END
;

/* ---------------------------------------------------------------------------
    PermissionActions (available actions for each module)
    --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PermissionActions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PermissionActions (
        Id               INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_PermissionActions PRIMARY KEY,
        Code             NVARCHAR(50)        NOT NULL CONSTRAINT UQ_PermissionActions_Code UNIQUE,
        [Name]           NVARCHAR(100)       NOT NULL,
        SortOrder        INT                 NOT NULL CONSTRAINT DF_PermissionActions_SortOrder DEFAULT 0,
        IsActive         BIT                 NOT NULL CONSTRAINT DF_PermissionActions_IsActive DEFAULT 1,
        CreatedBy        NVARCHAR(100)       NULL,
        CreatedDate      DATETIME2           NOT NULL CONSTRAINT DF_PermissionActions_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy       NVARCHAR(100)       NULL,
        ModifiedDate     DATETIME2           NOT NULL CONSTRAINT DF_PermissionActions_ModifiedDate DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_PermissionActions_IsActive ON dbo.PermissionActions (IsActive);
END
;

/* ---------------------------------------------------------------------------
    ModulePermissions (role-module-action matrix)
    --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ModulePermissions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.ModulePermissions (
        Id               INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_ModulePermissions PRIMARY KEY,
        RoleId           INT                 NOT NULL,
        PermissionModuleId INT               NOT NULL,
        PermissionActionId INT               NOT NULL,
        Scope            NVARCHAR(20)        NOT NULL,
        ScopeId          INT                 NULL,
        GrantedBy        NVARCHAR(100)       NULL,
        GrantedDate      DATETIME2           NOT NULL CONSTRAINT DF_ModulePermissions_GrantedDate DEFAULT SYSUTCDATETIME(),
        IsRevoked        BIT                 NOT NULL CONSTRAINT DF_ModulePermissions_IsRevoked DEFAULT 0,
        RevokedBy        NVARCHAR(100)       NULL,
        RevokedDate      DATETIME2           NULL,
        CONSTRAINT FK_ModulePermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT FK_ModulePermissions_Module FOREIGN KEY (PermissionModuleId) REFERENCES dbo.PermissionModules (Id),
        CONSTRAINT FK_ModulePermissions_Action FOREIGN KEY (PermissionActionId) REFERENCES dbo.PermissionActions (Id),
        CONSTRAINT UQ_ModulePermissions_Role_Module_Action_Scope UNIQUE (RoleId, PermissionModuleId, PermissionActionId, Scope, ScopeId)
    );

    CREATE INDEX IX_ModulePermissions_RoleId ON dbo.ModulePermissions (RoleId);
    CREATE INDEX IX_ModulePermissions_ModuleId ON dbo.ModulePermissions (PermissionModuleId);
    CREATE INDEX IX_ModulePermissions_ActionId ON dbo.ModulePermissions (PermissionActionId);
    CREATE INDEX IX_ModulePermissions_Scope ON dbo.ModulePermissions (Scope);
    CREATE INDEX IX_ModulePermissions_IsRevoked ON dbo.ModulePermissions (IsRevoked);
END
;

/* ---------------------------------------------------------------------------
    FieldPermissions (field-level security per role-module)
    --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FieldPermissions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.FieldPermissions (
        Id               INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_FieldPermissions PRIMARY KEY,
        RoleId           INT                 NOT NULL,
        PermissionModuleId INT               NOT NULL,
        FieldName        NVARCHAR(100)       NOT NULL,
        CanView          BIT                 NOT NULL CONSTRAINT DF_FieldPermissions_CanView DEFAULT 1,
        CanEdit          BIT                 NOT NULL CONSTRAINT DF_FieldPermissions_CanEdit DEFAULT 1,
        IsMandatory      BIT                 NOT NULL CONSTRAINT DF_FieldPermissions_IsMandatory DEFAULT 0,
        IsHidden         BIT                 NOT NULL CONSTRAINT DF_FieldPermissions_IsHidden DEFAULT 0,
        Scope            NVARCHAR(20)        NOT NULL CONSTRAINT DF_FieldPermissions_Scope_DEFAULT DEFAULT 'Company',
        ScopeId          INT                 NULL,
        CreatedBy        NVARCHAR(100)       NULL,
        CreatedDate      DATETIME2           NOT NULL CONSTRAINT DF_FieldPermissions_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy       NVARCHAR(100)       NULL,
        ModifiedDate     DATETIME2           NOT NULL CONSTRAINT DF_FieldPermissions_ModifiedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_FieldPermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT FK_FieldPermissions_Module FOREIGN KEY (PermissionModuleId) REFERENCES dbo.PermissionModules (Id),
        CONSTRAINT UQ_FieldPermissions_Role_Module_Field UNIQUE (RoleId, PermissionModuleId, FieldName, Scope, ScopeId)
    );

    CREATE INDEX IX_FieldPermissions_RoleId ON dbo.FieldPermissions (RoleId);
    CREATE INDEX IX_FieldPermissions_ModuleId ON dbo.FieldPermissions (PermissionModuleId);
    CREATE INDEX IX_FieldPermissions_FieldName ON dbo.FieldPermissions (FieldName);
    CREATE INDEX IX_FieldPermissions_Scope ON dbo.FieldPermissions (Scope);
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

/* ---------------------------------------------------------------------------
   BranchTypes (organization: branch classification)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BranchTypes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.BranchTypes (
        BranchTypeId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BranchTypes PRIMARY KEY,
        [Name]        NVARCHAR(100)     NOT NULL CONSTRAINT UQ_BranchTypes_Name UNIQUE,
        Code          NVARCHAR(20)      NOT NULL CONSTRAINT UQ_BranchTypes_Code UNIQUE,
        [Description] NVARCHAR(250)     NULL,
        SortOrder     INT               NOT NULL CONSTRAINT DF_BranchTypes_SortOrder DEFAULT 1,
        IsActive      BIT               NOT NULL CONSTRAINT DF_BranchTypes_IsActive DEFAULT 1,
        CreatedBy     NVARCHAR(100)     NULL,
        CreatedDate   DATETIME2         NOT NULL CONSTRAINT DF_BranchTypes_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)     NULL,
        ModifiedDate  DATETIME2         NOT NULL CONSTRAINT DF_BranchTypes_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT               NOT NULL CONSTRAINT DF_BranchTypes_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_BranchTypes_SortOrder ON dbo.BranchTypes (SortOrder);
    CREATE INDEX IX_BranchTypes_IsActive ON dbo.BranchTypes (IsActive);
END
;

/* ---------------------------------------------------------------------------
   WarehouseTypes (inventory: warehouse classification)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WarehouseTypes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.WarehouseTypes (
        WarehouseTypeId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WarehouseTypes PRIMARY KEY,
        [Name]          NVARCHAR(100)     NOT NULL CONSTRAINT UQ_WarehouseTypes_Name UNIQUE,
        Code            NVARCHAR(20)      NOT NULL CONSTRAINT UQ_WarehouseTypes_Code UNIQUE,
        [Description]   NVARCHAR(250)     NULL,
        SortOrder       INT               NOT NULL CONSTRAINT DF_WarehouseTypes_SortOrder DEFAULT 1,
        IsActive        BIT               NOT NULL CONSTRAINT DF_WarehouseTypes_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)     NULL,
        CreatedDate     DATETIME2         NOT NULL CONSTRAINT DF_WarehouseTypes_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100)     NULL,
        ModifiedDate    DATETIME2         NOT NULL CONSTRAINT DF_WarehouseTypes_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted       BIT               NOT NULL CONSTRAINT DF_WarehouseTypes_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_WarehouseTypes_SortOrder ON dbo.WarehouseTypes (SortOrder);
    CREATE INDEX IX_WarehouseTypes_IsActive ON dbo.WarehouseTypes (IsActive);
END
;

/* ---------------------------------------------------------------------------
   EmploymentTypes (HR: employment classification)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmploymentTypes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.EmploymentTypes (
        EmploymentTypeId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmploymentTypes PRIMARY KEY,
        [Name]           NVARCHAR(100)     NOT NULL CONSTRAINT UQ_EmploymentTypes_Name UNIQUE,
        Code             NVARCHAR(20)      NOT NULL CONSTRAINT UQ_EmploymentTypes_Code UNIQUE,
        [Description]    NVARCHAR(250)     NULL,
        SortOrder        INT               NOT NULL CONSTRAINT DF_EmploymentTypes_SortOrder DEFAULT 1,
        IsActive         BIT               NOT NULL CONSTRAINT DF_EmploymentTypes_IsActive DEFAULT 1,
        CreatedBy        NVARCHAR(100)     NULL,
        CreatedDate      DATETIME2         NOT NULL CONSTRAINT DF_EmploymentTypes_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy       NVARCHAR(100)     NULL,
        ModifiedDate     DATETIME2         NOT NULL CONSTRAINT DF_EmploymentTypes_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted        BIT               NOT NULL CONSTRAINT DF_EmploymentTypes_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_EmploymentTypes_SortOrder ON dbo.EmploymentTypes (SortOrder);
    CREATE INDEX IX_EmploymentTypes_IsActive ON dbo.EmploymentTypes (IsActive);
END
;

/* ---------------------------------------------------------------------------
   Genders (HR: gender master)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Genders]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Genders (
        GenderId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Genders PRIMARY KEY,
        [Name]   NVARCHAR(50)      NOT NULL CONSTRAINT UQ_Genders_Name UNIQUE,
        Code     NVARCHAR(10)      NOT NULL CONSTRAINT UQ_Genders_Code UNIQUE,
        IsActive BIT               NOT NULL CONSTRAINT DF_Genders_IsActive DEFAULT 1,
        SortOrder INT              NOT NULL CONSTRAINT DF_Genders_SortOrder DEFAULT 1
    );
END
;

/* ---------------------------------------------------------------------------
   MaritalStatuses (HR: marital status master)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MaritalStatuses]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.MaritalStatuses (
        MaritalStatusId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MaritalStatuses PRIMARY KEY,
        [Name]          NVARCHAR(50)      NOT NULL CONSTRAINT UQ_MaritalStatuses_Name UNIQUE,
        Code            NVARCHAR(10)      NOT NULL CONSTRAINT UQ_MaritalStatuses_Code UNIQUE,
        IsActive        BIT               NOT NULL CONSTRAINT DF_MaritalStatuses_IsActive DEFAULT 1,
        SortOrder       INT               NOT NULL CONSTRAINT DF_MaritalStatuses_SortOrder DEFAULT 1
    );
END
;

/* ---------------------------------------------------------------------------
   Branches (organization: company branches)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Branches]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Branches (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        CompanyId           INT NOT NULL,
        BranchCode          NVARCHAR(20) NOT NULL,
        BranchName          NVARCHAR(200) NOT NULL,
        ShortName           NVARCHAR(100) NULL,
        BranchTypeId        INT NOT NULL,
        ParentBranchId      INT NULL,
        ManagerEmployeeId   INT NULL,
        DefaultWarehouseId  INT NULL,
        GSTNumber           NVARCHAR(20) NULL,
        RegistrationNumber  NVARCHAR(100) NULL,
        IsHeadOffice        BIT NOT NULL DEFAULT(0),
        IsSalesBranch       BIT NOT NULL DEFAULT(1),
        IsPurchaseBranch    BIT NOT NULL DEFAULT(1),
        IsServiceBranch     BIT NOT NULL DEFAULT(0),
        SortOrder           INT NOT NULL DEFAULT(1),
        IsActive            BIT NOT NULL DEFAULT(1),
        IsBlocked           BIT NOT NULL DEFAULT(0),
        IsDeleted           BIT NOT NULL DEFAULT(0),
        CreatedBy           INT NOT NULL,
        CreatedDate         DATETIME2 NOT NULL DEFAULT(GETDATE()),
        ModifiedBy          INT NULL,
        ModifiedDate        DATETIME2 NULL,
        CONSTRAINT UQ_Branches UNIQUE (CompanyId, BranchCode),
        CONSTRAINT FK_Branches_Company FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
        CONSTRAINT FK_Branches_BranchType FOREIGN KEY (BranchTypeId) REFERENCES BranchTypes(BranchTypeId),
        CONSTRAINT FK_Branches_Parent FOREIGN KEY (ParentBranchId) REFERENCES Branches(Id)
    );

    CREATE INDEX IX_Branches_Company ON dbo.Branches (CompanyId);
    CREATE INDEX IX_Branches_BranchType ON dbo.Branches (BranchTypeId);
    CREATE INDEX IX_Branches_Parent ON dbo.Branches (ParentBranchId);
END
;

/* ---------------------------------------------------------------------------
   Departments (organization: company departments)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Departments]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Departments (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        CompanyId           INT NOT NULL,
        BranchId            INT NOT NULL,
        DepartmentCode      NVARCHAR(20) NOT NULL,
        DepartmentName      NVARCHAR(150) NOT NULL,
        ShortName           NVARCHAR(50) NULL,
        ParentDepartmentId  INT NULL,
        ManagerEmployeeId   INT NULL,
        SortOrder           INT NOT NULL DEFAULT(1),
        IsActive            BIT NOT NULL DEFAULT(1),
        IsBlocked           BIT NOT NULL DEFAULT(0),
        IsDeleted           BIT NOT NULL DEFAULT(0),
        Remarks             NVARCHAR(500) NULL,
        CreatedBy           INT NOT NULL,
        CreatedDate         DATETIME2 NOT NULL DEFAULT(GETDATE()),
        ModifiedBy          INT NULL,
        ModifiedDate        DATETIME2 NULL,
        CONSTRAINT UQ_Departments UNIQUE (CompanyId, BranchId, DepartmentCode),
        CONSTRAINT FK_Departments_Company FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
        CONSTRAINT FK_Departments_Branch FOREIGN KEY (BranchId) REFERENCES Branches(Id),
        CONSTRAINT FK_Departments_Parent FOREIGN KEY (ParentDepartmentId) REFERENCES Departments(Id)
    );

    CREATE INDEX IX_Departments_Company ON dbo.Departments (CompanyId);
    CREATE INDEX IX_Departments_Branch ON dbo.Departments (BranchId);
    CREATE INDEX IX_Departments_Parent ON dbo.Departments (ParentDepartmentId);
END
;

/* ---------------------------------------------------------------------------
   Designations (organization: employee designations)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Designations]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Designations (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        CompanyId           INT NOT NULL,
        DepartmentId        INT NULL,
        DesignationCode     NVARCHAR(20) NOT NULL,
        DesignationName     NVARCHAR(150) NOT NULL,
        ShortName           NVARCHAR(50) NULL,
        LevelNo             INT NULL,
        Grade               NVARCHAR(20) NULL,
        [Description]       NVARCHAR(500) NULL,
        SortOrder           INT NOT NULL DEFAULT(1),
        IsDefault           BIT NOT NULL DEFAULT(0),
        IsActive            BIT NOT NULL DEFAULT(1),
        IsBlocked           BIT NOT NULL DEFAULT(0),
        IsDeleted           BIT NOT NULL DEFAULT(0),
        CreatedBy           INT NOT NULL,
        CreatedDate         DATETIME2 NOT NULL DEFAULT(GETDATE()),
        ModifiedBy          INT NULL,
        ModifiedDate        DATETIME2 NULL,
        CONSTRAINT UQ_Designations UNIQUE (CompanyId, DesignationCode),
        CONSTRAINT FK_Designations_Company FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
        CONSTRAINT FK_Designations_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
    );

    CREATE INDEX IX_Designations_Company ON dbo.Designations (CompanyId);
    CREATE INDEX IX_Designations_Department ON dbo.Designations (DepartmentId);
END
;

/* ---------------------------------------------------------------------------
   Employees (HR: employee master)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Employees (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        CompanyId           INT NOT NULL,
        BranchId            INT NOT NULL,
        DepartmentId        INT NOT NULL,
        DesignationId       INT NOT NULL,
        EmployeeCode        NVARCHAR(20) NOT NULL,
        EmployeeNumber      NVARCHAR(50) NULL,
        FirstName           NVARCHAR(100) NOT NULL,
        MiddleName          NVARCHAR(100) NULL,
        LastName            NVARCHAR(100) NOT NULL,
        DisplayName         NVARCHAR(250) NULL,
        GenderId            INT NULL,
        MaritalStatusId     INT NULL,
        DateOfBirth         DATE NULL,
        DateOfJoining       DATE NOT NULL,
        DateOfLeaving       DATE NULL,
        OfficialEmail       NVARCHAR(150) NULL,
        PersonalEmail       NVARCHAR(150) NULL,
        MobileNo            NVARCHAR(20) NULL,
        AlternateMobileNo   NVARCHAR(20) NULL,
        ReportingManagerId  INT NULL,
        EmploymentTypeId    INT NULL,
        IsActive            BIT NOT NULL DEFAULT(1),
        IsBlocked           BIT NOT NULL DEFAULT(0),
        IsDeleted           BIT NOT NULL DEFAULT(0),
        Remarks             NVARCHAR(500) NULL,
        CreatedBy           INT NOT NULL,
        CreatedDate         DATETIME2 NOT NULL DEFAULT(GETDATE()),
        ModifiedBy          INT NULL,
        ModifiedDate        DATETIME2 NULL,
        CONSTRAINT UQ_Employees UNIQUE (CompanyId, EmployeeCode),
        CONSTRAINT FK_Employees_Company FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
        CONSTRAINT FK_Employees_Branch FOREIGN KEY (BranchId) REFERENCES Branches(Id),
        CONSTRAINT FK_Employees_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
        CONSTRAINT FK_Employees_Designation FOREIGN KEY (DesignationId) REFERENCES Designations(Id),
        CONSTRAINT FK_Employees_Manager FOREIGN KEY (ReportingManagerId) REFERENCES Employees(Id)
    );

    CREATE INDEX IX_Employees_Company ON dbo.Employees (CompanyId);
    CREATE INDEX IX_Employees_Branch ON dbo.Employees (BranchId);
    CREATE INDEX IX_Employees_Department ON dbo.Employees (DepartmentId);
    CREATE INDEX IX_Employees_Designation ON dbo.Employees (DesignationId);
    CREATE INDEX IX_Employees_Manager ON dbo.Employees (ReportingManagerId);
END
;

/* ---------------------------------------------------------------------------
   Warehouses (inventory: warehouse master)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Warehouses]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Warehouses (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        CompanyId           INT NOT NULL,
        BranchId            INT NOT NULL,
        WarehouseCode       NVARCHAR(20) NOT NULL,
        WarehouseName       NVARCHAR(150) NOT NULL,
        ShortName           NVARCHAR(50) NULL,
        WarehouseTypeId     INT NOT NULL,
        ParentWarehouseId   INT NULL,
        ManagerEmployeeId   INT NULL,
        AllowNegativeStock  BIT NOT NULL DEFAULT(0),
        IsDefault           BIT NOT NULL DEFAULT(0),
        SortOrder           INT NOT NULL DEFAULT(1),
        Remarks             NVARCHAR(500) NULL,
        IsActive            BIT NOT NULL DEFAULT(1),
        IsBlocked           BIT NOT NULL DEFAULT(0),
        IsDeleted           BIT NOT NULL DEFAULT(0),
        CreatedBy           INT NOT NULL,
        CreatedDate         DATETIME2 NOT NULL DEFAULT(GETDATE()),
        ModifiedBy          INT NULL,
        ModifiedDate        DATETIME2 NULL,
        CONSTRAINT UQ_Warehouses UNIQUE (CompanyId, BranchId, WarehouseCode),
        CONSTRAINT FK_Warehouses_Company FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
        CONSTRAINT FK_Warehouses_Branch FOREIGN KEY (BranchId) REFERENCES Branches(Id),
        CONSTRAINT FK_Warehouses_Type FOREIGN KEY (WarehouseTypeId) REFERENCES WarehouseTypes(WarehouseTypeId),
        CONSTRAINT FK_Warehouses_Parent FOREIGN KEY (ParentWarehouseId) REFERENCES Warehouses(Id),
        CONSTRAINT FK_Warehouses_Manager FOREIGN KEY (ManagerEmployeeId) REFERENCES Employees(Id)
    );

    CREATE INDEX IX_Warehouses_Company ON dbo.Warehouses (CompanyId);
    CREATE INDEX IX_Warehouses_Branch ON dbo.Warehouses (BranchId);
    CREATE INDEX IX_Warehouses_Type ON dbo.Warehouses (WarehouseTypeId);
    CREATE INDEX IX_Warehouses_Parent ON dbo.Warehouses (ParentWarehouseId);
END
;

/* ---------------------------------------------------------------------------
   Stores (retail/POS: physical point-of-sale locations)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Stores]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Stores (
        StoreId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Stores PRIMARY KEY,
        CompanyId       INT NOT NULL,
        BranchId        INT NULL,
        StoreCode       NVARCHAR(20) NOT NULL,
        StoreName       NVARCHAR(200) NOT NULL,
        StoreType       NVARCHAR(50) NULL,
        Address         NVARCHAR(500) NULL,
        Phone           NVARCHAR(30) NULL,
        Email           NVARCHAR(100) NULL,
        IsActive        BIT NOT NULL CONSTRAINT DF_Stores_IsActive DEFAULT 1,
        IsDeleted       BIT NOT NULL CONSTRAINT DF_Stores_IsDeleted DEFAULT 0,
        CreatedBy       INT NULL,
        CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Stores_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedBy       INT NULL,
        UpdatedAt       DATETIME2 NULL,
        CONSTRAINT UQ_Stores UNIQUE (CompanyId, BranchId, StoreCode),
        CONSTRAINT FK_Stores_Company FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
        CONSTRAINT FK_Stores_Branch FOREIGN KEY (BranchId) REFERENCES Branches(Id)
    );

    CREATE INDEX IX_Stores_Company ON dbo.Stores (CompanyId);
    CREATE INDEX IX_Stores_Branch ON dbo.Stores (BranchId);
    CREATE INDEX IX_Stores_Code ON dbo.Stores (StoreCode);
END
;

/* ---------------------------------------------------------------------------
   Counters (POS: physical counters/workstations within a store)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Counters]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Counters (
        CounterId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Counters PRIMARY KEY,
        StoreId         INT NOT NULL,
        CounterCode     NVARCHAR(20) NOT NULL,
        CounterName     NVARCHAR(200) NOT NULL,
        IsActive        BIT NOT NULL CONSTRAINT DF_Counters_IsActive DEFAULT 1,
        IsDeleted       BIT NOT NULL CONSTRAINT DF_Counters_IsDeleted DEFAULT 0,
        CreatedBy       INT NULL,
        CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Counters_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedBy       INT NULL,
        UpdatedAt       DATETIME2 NULL,
        CONSTRAINT UQ_Counters UNIQUE (StoreId, CounterCode),
        CONSTRAINT FK_Counters_Store FOREIGN KEY (StoreId) REFERENCES Stores(StoreId)
    );

    CREATE INDEX IX_Counters_Store ON dbo.Counters (StoreId);
    CREATE INDEX IX_Counters_Code ON dbo.Counters (CounterCode);
END
;

/* ---------------------------------------------------------------------------
   POS Sessions (cashier session at a store/counter with opening/closing cash)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POSSessions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.POSSessions (
        POSSessionId    BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_POSSessions PRIMARY KEY,
        CompanyId       INT NOT NULL,
        CompanyName     NVARCHAR(200) NULL,
        BranchId        INT NULL,
        BranchName      NVARCHAR(200) NULL,
        StoreId         INT NULL,
        StoreName       NVARCHAR(200) NULL,
        CounterId       INT NULL,
        CounterName     NVARCHAR(200) NULL,
        CashierUserId   INT NULL,
        CashierUserName NVARCHAR(200) NULL,
        SessionNumber   NVARCHAR(50) NOT NULL,
        OpeningCash     DECIMAL(18,2) NOT NULL CONSTRAINT DF_POSSessions_OpeningCash DEFAULT 0,
        ClosingCash     DECIMAL(18,2) NULL,
        OpenedAt        DATETIME2 NOT NULL CONSTRAINT DF_POSSessions_OpenedAt DEFAULT SYSUTCDATETIME(),
        ClosedAt        DATETIME2 NULL,
        Status          TINYINT NOT NULL CONSTRAINT DF_POSSessions_Status DEFAULT 1,
        CreatedBy       INT NULL,
        CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_POSSessions_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedBy       INT NULL,
        UpdatedAt       DATETIME2 NULL,
        CONSTRAINT FK_POSSessions_Company FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
        CONSTRAINT FK_POSSessions_Store FOREIGN KEY (StoreId) REFERENCES Stores(StoreId),
        CONSTRAINT FK_POSSessions_Counter FOREIGN KEY (CounterId) REFERENCES Counters(CounterId)
    );

    CREATE INDEX IX_POSSessions_Company ON dbo.POSSessions (CompanyId);
    CREATE INDEX IX_POSSessions_Branch ON dbo.POSSessions (BranchId);
    CREATE INDEX IX_POSSessions_Store ON dbo.POSSessions (StoreId);
    CREATE INDEX IX_POSSessions_Counter ON dbo.POSSessions (CounterId);
    CREATE INDEX IX_POSSessions_Status ON dbo.POSSessions (Status);
    CREATE INDEX IX_POSSessions_SessionNumber ON dbo.POSSessions (SessionNumber);
END
;

/* ---------------------------------------------------------------------------
   Financial Year (company fiscal periods; referenced by Companies.DefaultFinancialYearId)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FinancialYear]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.FinancialYear
    (
        FinancialYearId BIGINT IDENTITY(1,1)
            CONSTRAINT PK_FinancialYear PRIMARY KEY,

        CompanyId INT NOT NULL,

        Code VARCHAR(30) NOT NULL,

        [Name] VARCHAR(100) NOT NULL,

        StartDate DATE NOT NULL,

        EndDate DATE NOT NULL,

        IsCurrent BIT NOT NULL CONSTRAINT DF_FinancialYear_IsCurrent DEFAULT 0,

        IsClosed BIT NOT NULL CONSTRAINT DF_FinancialYear_IsClosed DEFAULT 0,

        IsActive BIT NOT NULL CONSTRAINT DF_FinancialYear_IsActive DEFAULT 1,

        CreatedBy INT NULL,

        CreatedAt DATETIME NOT NULL CONSTRAINT DF_FinancialYear_CreatedAt DEFAULT GETDATE(),

        ModifiedBy INT NULL,

        ModifiedAt DATETIME NULL,

        CONSTRAINT UQ_FinancialYear_Company_Code
            UNIQUE (CompanyId, Code),

        CONSTRAINT CK_FinancialYear_Date
            CHECK (StartDate < EndDate),

        CONSTRAINT FK_FinancialYear_Company
            FOREIGN KEY (CompanyId) REFERENCES dbo.Companies (Id)
    );

    CREATE INDEX IX_FinancialYear_Company ON dbo.FinancialYear (CompanyId);
    CREATE INDEX IX_FinancialYear_IsCurrent ON dbo.FinancialYear (IsCurrent);
    CREATE INDEX IX_FinancialYear_IsActive ON dbo.FinancialYear (IsActive);
END
;

/* =============================================================================
   ENTERPRISE PERMISSION ENGINE (Workspace > Domain > Module > SubModule > Screen)
   Merged from migrations 002 / 006 / 007 / 008 into the fresh-provision schema.
   ============================================================================= */

/* ---------------------------------------------------------------------------
   Workspaces (top-level permission grouping)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Workspaces]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Workspaces (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Workspaces PRIMARY KEY,
        WorkspaceCode   NVARCHAR(50)        NOT NULL CONSTRAINT UQ_Workspaces_Code UNIQUE,
        WorkspaceName   NVARCHAR(200)       NOT NULL,
        Icon            NVARCHAR(100)       NULL,
        Route           NVARCHAR(200)       NULL,
        SortOrder       INT                 NOT NULL CONSTRAINT DF_Workspaces_SortOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Workspaces_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_Workspaces_CreatedDate DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX IX_Workspaces_IsActive ON dbo.Workspaces (IsActive);
    CREATE INDEX IX_Workspaces_SortOrder ON dbo.Workspaces (SortOrder);
END
;

/* ---------------------------------------------------------------------------
   Domains (workspace sub-grouping)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Domains]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Domains (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Domains PRIMARY KEY,
        WorkspaceId     INT                 NOT NULL,
        DomainCode      NVARCHAR(50)        NOT NULL,
        DomainName      NVARCHAR(200)       NOT NULL,
        Icon            NVARCHAR(100)       NULL,
        SortOrder       INT                 NOT NULL CONSTRAINT DF_Domains_SortOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Domains_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_Domains_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Domains_Workspace FOREIGN KEY (WorkspaceId) REFERENCES dbo.Workspaces (Id),
        CONSTRAINT UQ_Domains_Code UNIQUE (WorkspaceId, DomainCode)
    );
    CREATE INDEX IX_Domains_WorkspaceId ON dbo.Domains (WorkspaceId);
    CREATE INDEX IX_Domains_IsActive ON dbo.Domains (IsActive);
END
;

/* ---------------------------------------------------------------------------
   Modules (domain sub-grouping)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Modules]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Modules (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Modules PRIMARY KEY,
        DomainId        INT                 NOT NULL,
        ModuleCode      NVARCHAR(50)        NOT NULL,
        ModuleName      NVARCHAR(200)       NOT NULL,
        Icon            NVARCHAR(100)       NULL,
        RouteUrl        NVARCHAR(200)       NULL,
        SortOrder       INT                 NOT NULL CONSTRAINT DF_Modules_SortOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Modules_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_Modules_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Modules_Domain FOREIGN KEY (DomainId) REFERENCES dbo.Domains (Id),
        CONSTRAINT UQ_Modules_Code UNIQUE (DomainId, ModuleCode)
    );
    CREATE INDEX IX_Modules_DomainId ON dbo.Modules (DomainId);
    CREATE INDEX IX_Modules_IsActive ON dbo.Modules (IsActive);
END
;

/* ---------------------------------------------------------------------------
   SubModules (level between Module and Screen - migration 006)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SubModules]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.SubModules (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_SubModules PRIMARY KEY,
        ModuleId        INT                 NOT NULL,
        SubModuleCode   NVARCHAR(50)        NOT NULL,
        SubModuleName   NVARCHAR(200)       NOT NULL,
        Icon            NVARCHAR(100)       NULL,
        RouteUrl        NVARCHAR(200)       NULL,
        SortOrder       INT                 NOT NULL CONSTRAINT DF_SubModules_SortOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_SubModules_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_SubModules_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100)       NULL,
        ModifiedDate    DATETIME2           NULL,
        CONSTRAINT FK_SubModules_Module FOREIGN KEY (ModuleId) REFERENCES dbo.Modules (Id),
        CONSTRAINT UQ_SubModules_Code UNIQUE (ModuleId, SubModuleCode)
    );
    CREATE INDEX IX_SubModules_ModuleId ON dbo.SubModules (ModuleId);
    CREATE INDEX IX_SubModules_IsActive ON dbo.SubModules (IsActive);
END
;

/* ---------------------------------------------------------------------------
   Screens (final shape: SubModuleId + ScreenType - migrations 006 / 007)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Screens]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Screens (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Screens PRIMARY KEY,
        SubModuleId     INT                 NOT NULL,
        ScreenCode      NVARCHAR(50)        NOT NULL,
        ScreenName      NVARCHAR(200)       NOT NULL,
        PermissionCode  NVARCHAR(100)       NULL,
        ScreenType      NVARCHAR(20)        NOT NULL CONSTRAINT DF_Screens_ScreenType DEFAULT 'MASTER',
        RouteUrl        NVARCHAR(200)       NULL,
        ComponentName   NVARCHAR(200)       NULL,
        SortOrder       INT                 NOT NULL CONSTRAINT DF_Screens_SortOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Screens_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_Screens_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Screens_SubModule FOREIGN KEY (SubModuleId) REFERENCES dbo.SubModules (Id),
        CONSTRAINT UQ_Screens_SubModule_Code UNIQUE (SubModuleId, ScreenCode),
        CONSTRAINT CK_Screens_ScreenType CHECK (ScreenType IN ('MASTER','TRANSACTION','REPORT','DASHBOARD','LIST','ENTRY','SETTINGS'))
    );
    CREATE INDEX IX_Screens_SubModuleId ON dbo.Screens (SubModuleId);
    CREATE INDEX IX_Screens_IsActive ON dbo.Screens (IsActive);
END
;

/* Screens.PermissionCode was added later; retrofit existing databases. */
IF OBJECT_ID(N'[dbo].[Screens]', N'U') IS NOT NULL
   AND COL_LENGTH('dbo.Screens', 'PermissionCode') IS NULL
BEGIN
    ALTER TABLE dbo.Screens ADD PermissionCode NVARCHAR(100) NULL;
END
;

/* ---------------------------------------------------------------------------
   Fields (screen-level field definitions)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Fields]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Fields (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Fields PRIMARY KEY,
        ScreenId        INT                 NOT NULL,
        FieldCode       NVARCHAR(100)       NOT NULL,
        FieldName       NVARCHAR(200)       NOT NULL,
        DisplayName     NVARCHAR(200)       NOT NULL,
        DataType        NVARCHAR(50)        NOT NULL CONSTRAINT DF_Fields_DataType DEFAULT 'text',
        DisplayOrder    INT                 NOT NULL CONSTRAINT DF_Fields_DisplayOrder DEFAULT 0,
        DefaultValue    NVARCHAR(500)       NULL,
        IsSystemField   BIT                 NOT NULL CONSTRAINT DF_Fields_IsSystemField DEFAULT 0,
        IsRequired      BIT                 NOT NULL CONSTRAINT DF_Fields_IsRequired DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Fields_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_Fields_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Fields_Screen FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
        CONSTRAINT UQ_Fields_Code UNIQUE (ScreenId, FieldCode)
    );
    CREATE INDEX IX_Fields_ScreenId ON dbo.Fields (ScreenId);
    CREATE INDEX IX_Fields_IsActive ON dbo.Fields (IsActive);
END
;

/* ---------------------------------------------------------------------------
   Actions (hierarchical action catalogue)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Actions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Actions (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Actions PRIMARY KEY,
        ActionCode      NVARCHAR(50)        NOT NULL CONSTRAINT UQ_Actions_Code UNIQUE,
        ActionName      NVARCHAR(100)       NOT NULL,
        DisplayOrder    INT                 NOT NULL CONSTRAINT DF_Actions_DisplayOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Actions_IsActive DEFAULT 1
    );
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Actions WHERE ActionCode = 'view')
BEGIN
    INSERT INTO dbo.Actions (ActionCode, ActionName, DisplayOrder, IsActive) VALUES
    ('view',    'View',    1,  1),
    ('create',  'Create',  2,  1),
    ('edit',    'Edit',    3,  1),
    ('delete',  'Delete',  4,  1),
    ('approve', 'Approve', 5,  1),
    ('reject',  'Reject',  6,  1),
    ('submit',  'Submit',  7,  1),
    ('cancel',  'Cancel',  8,  1),
    ('print',   'Print',   9,  1),
    ('export',  'Export',  10, 1),
    ('import',  'Import',  11, 1),
    ('email',   'Email',   12, 1),
    ('share',   'Share',   13, 1),
    ('clone',   'Clone',   14, 1),
    ('copy',    'Copy',    15, 1),
    ('upload',  'Upload',  16, 1),
    ('download','Download',17, 1);
END
;

/* ---------------------------------------------------------------------------
   RolePermissions (NEW - hierarchical Workspace/Domain/Module/SubModule/Screen/Action)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.RolePermissions (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_RolePermissions PRIMARY KEY,
        RoleId          INT                 NOT NULL,
        WorkspaceId     INT                 NOT NULL,
        DomainId        INT                 NOT NULL,
        ModuleId        INT                 NOT NULL,
        SubModuleId     INT                 NOT NULL,
        ScreenId        INT                 NOT NULL,
        ActionId        INT                 NOT NULL,
        Allow           BIT                 NOT NULL CONSTRAINT DF_RolePermissions_Allow DEFAULT 1,
        DisplayOrder    INT                 NOT NULL CONSTRAINT DF_RolePermissions_DisplayOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_RolePermissions_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_RolePermissions_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100)       NULL,
        ModifiedDate    DATETIME2           NULL,
        CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT FK_RolePermissions_Workspace FOREIGN KEY (WorkspaceId) REFERENCES dbo.Workspaces (Id),
        CONSTRAINT FK_RolePermissions_Domain FOREIGN KEY (DomainId) REFERENCES dbo.Domains (Id),
        CONSTRAINT FK_RolePermissions_Module FOREIGN KEY (ModuleId) REFERENCES dbo.Modules (Id),
        CONSTRAINT FK_RolePermissions_SubModule FOREIGN KEY (SubModuleId) REFERENCES dbo.SubModules (Id),
        CONSTRAINT FK_RolePermissions_Screen FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
        CONSTRAINT FK_RolePermissions_Action FOREIGN KEY (ActionId) REFERENCES dbo.Actions (Id),
        CONSTRAINT UQ_RolePermissions_Matrix UNIQUE (RoleId, WorkspaceId, DomainId, ModuleId, SubModuleId, ScreenId, ActionId)
    );
    CREATE INDEX IX_RolePermissions_RoleId ON dbo.RolePermissions (RoleId);
    CREATE INDEX IX_RolePermissions_RoleId_ScreenId ON dbo.RolePermissions (RoleId, ScreenId);
    CREATE INDEX IX_RolePermissions_WorkspaceId ON dbo.RolePermissions (WorkspaceId);
    CREATE INDEX IX_RolePermissions_DomainId ON dbo.RolePermissions (DomainId);
    CREATE INDEX IX_RolePermissions_ModuleId ON dbo.RolePermissions (ModuleId);
    CREATE INDEX IX_RolePermissions_SubModuleId ON dbo.RolePermissions (SubModuleId);
    CREATE INDEX IX_RolePermissions_ScreenId ON dbo.RolePermissions (ScreenId);
    CREATE INDEX IX_RolePermissions_ActionId ON dbo.RolePermissions (ActionId);
    CREATE INDEX IX_RolePermissions_IsActive ON dbo.RolePermissions (IsActive);
END
;

/* ---------------------------------------------------------------------------
   UserPermissionOverrides (user-level grant/deny beyond role)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPermissionOverrides]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.UserPermissionOverrides (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_UserPermissionOverrides PRIMARY KEY,
        UserId          INT                 NOT NULL,
        WorkspaceId     INT                 NOT NULL,
        DomainId        INT                 NOT NULL,
        ModuleId        INT                 NOT NULL,
        SubModuleId     INT                 NOT NULL,
        ScreenId        INT                 NOT NULL,
        ActionId        INT                 NOT NULL,
        PermissionType  NVARCHAR(20)        NOT NULL CONSTRAINT DF_UserPermissionOverrides_PermissionType DEFAULT 'Grant',
        Allow           BIT                 NOT NULL CONSTRAINT DF_UserPermissionOverrides_Allow DEFAULT 1,
        EffectiveFrom   DATETIME2           NOT NULL CONSTRAINT DF_UserPermissionOverrides_EffectiveFrom DEFAULT SYSUTCDATETIME(),
        EffectiveTo     DATETIME2           NULL,
        Remarks         NVARCHAR(500)       NULL,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_UserPermissionOverrides_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_UserPermissionOverrides_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100)       NULL,
        ModifiedDate    DATETIME2           NULL,
        CONSTRAINT FK_UserPermissionOverrides_User FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId),
        CONSTRAINT FK_UserPermissionOverrides_Workspace FOREIGN KEY (WorkspaceId) REFERENCES dbo.Workspaces (Id),
        CONSTRAINT FK_UserPermissionOverrides_Domain FOREIGN KEY (DomainId) REFERENCES dbo.Domains (Id),
        CONSTRAINT FK_UserPermissionOverrides_Module FOREIGN KEY (ModuleId) REFERENCES dbo.Modules (Id),
        CONSTRAINT FK_UserPermissionOverrides_SubModule FOREIGN KEY (SubModuleId) REFERENCES dbo.SubModules (Id),
        CONSTRAINT FK_UserPermissionOverrides_Screen FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
        CONSTRAINT FK_UserPermissionOverrides_Action FOREIGN KEY (ActionId) REFERENCES dbo.Actions (Id),
        CONSTRAINT UQ_UserPermissionOverrides_Matrix UNIQUE (UserId, WorkspaceId, DomainId, ModuleId, SubModuleId, ScreenId, ActionId)
    );
    CREATE INDEX IX_UserPermissionOverrides_UserId ON dbo.UserPermissionOverrides (UserId);
    CREATE INDEX IX_UserPermissionOverrides_UserId_IsActive ON dbo.UserPermissionOverrides (UserId, IsActive);
    CREATE INDEX IX_UserPermissionOverrides_WorkspaceId ON dbo.UserPermissionOverrides (WorkspaceId);
    CREATE INDEX IX_UserPermissionOverrides_DomainId ON dbo.UserPermissionOverrides (DomainId);
    CREATE INDEX IX_UserPermissionOverrides_ModuleId ON dbo.UserPermissionOverrides (ModuleId);
    CREATE INDEX IX_UserPermissionOverrides_ScreenId ON dbo.UserPermissionOverrides (ScreenId);
    CREATE INDEX IX_UserPermissionOverrides_ActionId ON dbo.UserPermissionOverrides (ActionId);
END
;

/* ---------------------------------------------------------------------------
   RoleFieldPermissions (field-level security per role)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RoleFieldPermissions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.RoleFieldPermissions (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_RoleFieldPermissions PRIMARY KEY,
        RoleId          INT                 NOT NULL,
        ScreenId        INT                 NOT NULL,
        FieldId         INT                 NOT NULL,
        CanView         BIT                 NOT NULL CONSTRAINT DF_RoleFieldPermissions_CanView DEFAULT 1,
        CanEdit         BIT                 NOT NULL CONSTRAINT DF_RoleFieldPermissions_CanEdit DEFAULT 1,
        IsHidden        BIT                 NOT NULL CONSTRAINT DF_RoleFieldPermissions_IsHidden DEFAULT 0,
        IsReadOnly      BIT                 NOT NULL CONSTRAINT DF_RoleFieldPermissions_IsReadOnly DEFAULT 0,
        IsMandatory     BIT                 NOT NULL CONSTRAINT DF_RoleFieldPermissions_IsMandatory DEFAULT 0,
        DisplayOrder    INT                 NOT NULL CONSTRAINT DF_RoleFieldPermissions_DisplayOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_RoleFieldPermissions_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_RoleFieldPermissions_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100)       NULL,
        ModifiedDate    DATETIME2           NULL,
        CONSTRAINT FK_RoleFieldPermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT FK_RoleFieldPermissions_Screen FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
        CONSTRAINT FK_RoleFieldPermissions_Field FOREIGN KEY (FieldId) REFERENCES dbo.Fields (Id),
        CONSTRAINT UQ_RoleFieldPermissions_Matrix UNIQUE (RoleId, ScreenId, FieldId)
    );
    CREATE INDEX IX_RoleFieldPermissions_RoleId ON dbo.RoleFieldPermissions (RoleId);
    CREATE INDEX IX_RoleFieldPermissions_ScreenId ON dbo.RoleFieldPermissions (ScreenId);
    CREATE INDEX IX_RoleFieldPermissions_FieldId ON dbo.RoleFieldPermissions (FieldId);
    CREATE INDEX IX_RoleFieldPermissions_IsActive ON dbo.RoleFieldPermissions (IsActive);
END
;

/* ---------------------------------------------------------------------------
   UserFieldPermissions (user-level field overrides)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserFieldPermissions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.UserFieldPermissions (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_UserFieldPermissions PRIMARY KEY,
        UserId          INT                 NOT NULL,
        ScreenId        INT                 NOT NULL,
        FieldId         INT                 NOT NULL,
        CanView         BIT                 NOT NULL CONSTRAINT DF_UserFieldPermissions_CanView DEFAULT 1,
        CanEdit         BIT                 NOT NULL CONSTRAINT DF_UserFieldPermissions_CanEdit DEFAULT 1,
        IsHidden        BIT                 NOT NULL CONSTRAINT DF_UserFieldPermissions_IsHidden DEFAULT 0,
        IsReadOnly      BIT                 NOT NULL CONSTRAINT DF_UserFieldPermissions_IsReadOnly DEFAULT 0,
        IsMandatory     BIT                 NOT NULL CONSTRAINT DF_UserFieldPermissions_IsMandatory DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_UserFieldPermissions_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_UserFieldPermissions_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100)       NULL,
        ModifiedDate    DATETIME2           NULL,
        CONSTRAINT FK_UserFieldPermissions_User FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId),
        CONSTRAINT FK_UserFieldPermissions_Screen FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
        CONSTRAINT FK_UserFieldPermissions_Field FOREIGN KEY (FieldId) REFERENCES dbo.Fields (Id),
        CONSTRAINT UQ_UserFieldPermissions_Matrix UNIQUE (UserId, ScreenId, FieldId)
    );
    CREATE INDEX IX_UserFieldPermissions_UserId ON dbo.UserFieldPermissions (UserId);
    CREATE INDEX IX_UserFieldPermissions_ScreenId ON dbo.UserFieldPermissions (ScreenId);
    CREATE INDEX IX_UserFieldPermissions_FieldId ON dbo.UserFieldPermissions (FieldId);
    CREATE INDEX IX_UserFieldPermissions_IsActive ON dbo.UserFieldPermissions (IsActive);
END
;

/* ---------------------------------------------------------------------------
   RoleDataScopes (role-based data access restrictions - migration 008)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RoleDataScopes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.RoleDataScopes (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_RoleDataScopes PRIMARY KEY,
        RoleId          INT                 NOT NULL,
        ModuleId        INT                 NULL,
        ScreenId        INT                 NULL,
        CompanyId       INT                 NULL,
        BranchId        INT                 NULL,
        DepartmentId    INT                 NULL,
        WarehouseId     INT                 NULL,
        BusinessUnitId  INT                 NULL,
        CostCenterId    INT                 NULL,
        ProfitCenterId  INT                 NULL,
        CanView         BIT                 NOT NULL CONSTRAINT DF_RoleDataScopes_CanView DEFAULT 1,
        CanCreate       BIT                 NOT NULL CONSTRAINT DF_RoleDataScopes_CanCreate DEFAULT 0,
        CanEdit         BIT                 NOT NULL CONSTRAINT DF_RoleDataScopes_CanEdit DEFAULT 0,
        CanDelete       BIT                 NOT NULL CONSTRAINT DF_RoleDataScopes_CanDelete DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_RoleDataScopes_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_RoleDataScopes_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100)       NULL,
        ModifiedDate    DATETIME2           NULL,
        CONSTRAINT FK_RoleDataScopes_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId)
    );
    CREATE INDEX IX_RoleDataScopes_RoleId ON dbo.RoleDataScopes (RoleId);
    CREATE INDEX IX_RoleDataScopes_ModuleId ON dbo.RoleDataScopes (ModuleId);
    CREATE INDEX IX_RoleDataScopes_ScreenId ON dbo.RoleDataScopes (ScreenId);
    CREATE INDEX IX_RoleDataScopes_CompanyId ON dbo.RoleDataScopes (CompanyId);
    CREATE INDEX IX_RoleDataScopes_BranchId ON dbo.RoleDataScopes (BranchId);
    CREATE INDEX IX_RoleDataScopes_DepartmentId ON dbo.RoleDataScopes (DepartmentId);
    CREATE INDEX IX_RoleDataScopes_WarehouseId ON dbo.RoleDataScopes (WarehouseId);
END
;

/* ---------------------------------------------------------------------------
   UserDataScopeOverrides (user-level data scope overrides)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserDataScopeOverrides]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.UserDataScopeOverrides (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_UserDataScopeOverrides PRIMARY KEY,
        UserId          INT                 NOT NULL,
        ModuleId        INT                 NULL,
        ScreenId        INT                 NULL,
        ScopeType       NVARCHAR(50)        NOT NULL,
        ScopeValue      NVARCHAR(200)       NOT NULL,
        PermissionType  NVARCHAR(20)        NOT NULL,
        Allow           BIT                 NOT NULL,
        EffectiveFrom   DATETIME2           NOT NULL CONSTRAINT DF_UserDataScopeOverrides_EffectiveFrom DEFAULT SYSUTCDATETIME(),
        EffectiveTo     DATETIME2           NULL,
        Remarks         NVARCHAR(500)       NULL,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_UserDataScopeOverrides_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_UserDataScopeOverrides_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100)       NULL,
        ModifiedDate    DATETIME2           NULL,
        CONSTRAINT FK_UserDataScopeOverrides_User FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId)
    );
    CREATE INDEX IX_UserDataScopeOverrides_UserId ON dbo.UserDataScopeOverrides (UserId);
    CREATE INDEX IX_UserDataScopeOverrides_ModuleId ON dbo.UserDataScopeOverrides (ModuleId);
    CREATE INDEX IX_UserDataScopeOverrides_ScreenId ON dbo.UserDataScopeOverrides (ScreenId);
    CREATE INDEX IX_UserDataScopeOverrides_ScopeType ON dbo.UserDataScopeOverrides (ScopeType);
END
;

/* ---------------------------------------------------------------------------
   WorkflowPermissions (role-based workflow/approval permissions)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkflowPermissions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.WorkflowPermissions (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_WorkflowPermissions PRIMARY KEY,
        RoleId          INT                 NOT NULL,
        ModuleId        INT                 NOT NULL,
        SubModuleId     INT                 NOT NULL,
        ScreenId        INT                 NOT NULL,
        CanSubmit       BIT                 NOT NULL CONSTRAINT DF_WorkflowPermissions_CanSubmit DEFAULT 0,
        CanApprove      BIT                 NOT NULL CONSTRAINT DF_WorkflowPermissions_CanApprove DEFAULT 0,
        CanReject       BIT                 NOT NULL CONSTRAINT DF_WorkflowPermissions_CanReject DEFAULT 0,
        CanCancel       BIT                 NOT NULL CONSTRAINT DF_WorkflowPermissions_CanCancel DEFAULT 0,
        CanClose        BIT                 NOT NULL CONSTRAINT DF_WorkflowPermissions_CanClose DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_WorkflowPermissions_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_WorkflowPermissions_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_WorkflowPermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT FK_WorkflowPermissions_Module FOREIGN KEY (ModuleId) REFERENCES dbo.Modules (Id),
        CONSTRAINT FK_WorkflowPermissions_SubModule FOREIGN KEY (SubModuleId) REFERENCES dbo.SubModules (Id),
        CONSTRAINT FK_WorkflowPermissions_Screen FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
        CONSTRAINT UQ_WorkflowPermissions_Matrix UNIQUE (RoleId, ModuleId, SubModuleId, ScreenId)
    );
    CREATE INDEX IX_WorkflowPermissions_RoleId ON dbo.WorkflowPermissions (RoleId);
    CREATE INDEX IX_WorkflowPermissions_ModuleId ON dbo.WorkflowPermissions (ModuleId);
    CREATE INDEX IX_WorkflowPermissions_SubModuleId ON dbo.WorkflowPermissions (SubModuleId);
    CREATE INDEX IX_WorkflowPermissions_ScreenId ON dbo.WorkflowPermissions (ScreenId);
END
;

/* ---------------------------------------------------------------------------
   Default Workspace Structure (13 top-level workspaces)
   Order matches the ERP taxonomy: SETUP, PRODBILL, SALES, PURCHASE, INVENTORY,
   PAYMENTS, REPORTS, BUSINESS_PARTNERS, SECURITY, ENTERPRISE_PERMISSIONS,
   SETTINGS, LEGACY.
   Dapper-compatible: no GO, single batch. Idempotent per workspace code.
--------------------------------------------------------------------------- */
INSERT INTO dbo.Workspaces (WorkspaceCode, WorkspaceName, Icon, Route, SortOrder, IsActive, CreatedBy)
SELECT k.WorkspaceCode, k.WorkspaceName, k.Icon, k.Route, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('SETUP',                  'Setup & Configuration',    'settings',      '/setup',                 1,  1, 'system'),
    ('PRODBILL',               'Product & Billing',        'package',       '/product',               2,  1, 'system'),
    ('SALES',                  'Sales',                    'shopping-cart', '/sales',                 3,  1, 'system'),
    ('PURCHASE',               'Purchase',                 'shopping-cart', '/purchase',              4,  1, 'system'),
    ('INVENTORY',              'Inventory',                'box',           '/stock',                 5,  1, 'system'),
    ('PAYMENTS',               'Payments',                 'credit-card',   '/payment',               6,  1, 'system'),
    ('REPORTS',                'Reports',                  'file-text',     '/reports',               7,  1, 'system'),
    ('BUSINESS_PARTNERS',      'Business Partners',        'users',         '/business-partners',     8,  1, 'system'),
    ('SECURITY',               'Security & Access',        'shield',        '/security',              9,  1, 'system'),
    ('ENTERPRISE_PERMISSIONS', 'Enterprise Permissions',   'key-square',    '/enterprise-permissions', 10, 1, 'system'),
    ('SETTINGS',               'Settings',                 'settings',      '/settings',              11, 1, 'system')
) AS k(WorkspaceCode, WorkspaceName, Icon, Route, SortOrder, IsActive, CreatedBy)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Workspaces w WHERE w.WorkspaceCode = k.WorkspaceCode
);
;

/* Domain: Organization (SETUP) */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-ORG')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-ORG', 'Organization', 'building-2', 1, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'SETUP';
END
;

/* Domain: System (SETUP) */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-SYSTEM')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-SYSTEM', 'System', 'server', 2, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'SETUP';
END
;

/* Domain: Tenant (SETUP) */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-TENANT')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-TENANT', 'Tenant', 'building-2', 3, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'SETUP';
END
;

/* ---------------------------------------------------------------------------
    Seed Data: Permission Actions (standard CRUD actions)
    --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.PermissionActions)
BEGIN
    INSERT INTO dbo.PermissionActions (Code, [Name], SortOrder, IsActive)
    VALUES
        ('view',   'View',   1, 1),
        ('create', 'Create', 2, 1),
        ('edit',   'Edit',   3, 1),
        ('delete', 'Delete', 4, 1),
        ('manage', 'Manage', 5, 1),
        ('export', 'Export', 6, 1),
        ('import', 'Import', 7, 1),
        ('approve','Approve',8, 1),
        ('reject', 'Reject', 9, 1);
END
;

/* ---------------------------------------------------------------------------
    Seed Data: Permission Modules (hierarchical security scopes)
    --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.PermissionModules)
BEGIN
    INSERT INTO dbo.PermissionModules (Code, [Name], ParentId, [Level], SortOrder, IsVisible, Icon, RoutePath)
    VALUES
        ('platform',     'Platform',     NULL, 'Platform',     0, 1, 'server',           '/platform'),
        ('tenant',       'Tenant',       NULL, 'Tenant',       1, 1, 'building-2',       '/tenant'),
        ('company',      'Company',      NULL, 'Company',      2, 1, 'home',             '/companies'),
        ('branch',       'Branch',       NULL, 'Branch',       3, 1, 'git-branch',       '/branches'),
        ('user',         'User',         NULL, 'User',         4, 1, 'user',             '/users'),
        ('roleprofile',  'Role Profile', NULL, 'RoleProfile',  5, 1, 'id-card',          '/role-profiles'),
        ('role',         'Role',         NULL, 'Role',         6, 1, 'shield',           '/roles'),
        ('workspace',    'Workspace',    NULL, 'Workspace',    7, 1, 'layers',           '/workspaces'),
        ('domain',       'Domain',       NULL, 'Domain',       8, 1, 'globe',            '/domains'),
        ('module',       'Module',       NULL, 'Module',       9, 1, 'box',              '/modules'),
        ('screen',       'Screen',       NULL, 'Screen',      10, 1, 'monitor',          '/screens'),
        ('action',       'Action',       NULL, 'Action',      11, 1, 'zap',              '/actions'),
        ('field',        'Field',        NULL, 'Field',       12, 1, 'text-cursor-input','/fields'),
        ('datascope',    'Data Scope',   NULL, 'DataScope',   13, 1, 'database',         '/data-scopes'),
        ('workflow',     'Workflow',     NULL, 'Workflow',    14, 1, 'workflow',         '/workflows'),
        ('approval',     'Approval',     NULL, 'Approval',    15, 1, 'check-double',     '/approvals'),
        ('audit',        'Audit',        NULL, 'Audit',       16, 1, 'clipboard-list',   '/audit');
END
;


/* =============================================================================
   ONE ERP - ERP Database Seed Data
   Purpose  : Executed automatically by ONEERP.Platform.API when provisioning
              a new tenant database. NO GO statements (single Dapper batch).
              Parameterized by the provisioning service:
                @CompanyCode      - derived from the platform tenant code
                @CompanyName      - company name supplied at tenant creation
                @CompanyEmail     - admin/contact email for the tenant
                @CurrencyCode     - currency taken from the tenant plan
                @AdminUsername    - unique tenant admin username
                @AdminPasswordHash- BCrypt hash of the tenant admin password
                @AdminFullName    - display name of the tenant admin
   ============================================================================= */

DECLARE @CompanyId INT;
DECLARE @SuperAdminRoleId INT;
DECLARE @AdministratorRoleId INT;
DECLARE @SalesAdminRoleId INT;
DECLARE @AdminUserId INT;
DECLARE @CurrencyId INT;
DECLARE @LanguageId INT;
DECLARE @TimeZoneId INT;
DECLARE @BusinessTypeId INT;
DECLARE @IndustryTypeId INT;

/* ---------------------------------------------------------------------------
   Countries (location master hierarchy: Countries -> States -> Cities)
   --------------------------------------------------------------------------- */
INSERT INTO dbo.Countries ([Name], ISOCode2, ISOCode3, PhoneCode, CurrencyCode, Nationality, IsActive, CreatedBy)
SELECT c.[Name], c.ISOCode2, c.ISOCode3, c.PhoneCode, c.CurrencyCode, c.Nationality, 1, 'system'
FROM (VALUES
    ('India', 'IN', 'IND', '+91', 'INR', 'Indian'),
    ('United States', 'US', 'USA', '+1', 'USD', 'American'),
    ('United Arab Emirates', 'AE', 'ARE', '+971', 'AED', 'Emirati'),
    ('Singapore', 'SG', 'SGP', '+65', 'SGD', 'Singaporean'),
    ('Malaysia', 'MY', 'MYS', '+60', 'MYR', 'Malaysian')
) AS c([Name], ISOCode2, ISOCode3, PhoneCode, CurrencyCode, Nationality)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Countries t WHERE t.ISOCode2 = c.ISOCode2 OR t.ISOCode3 = c.ISOCode3
);

/* ---------------------------------------------------------------------------
   States (India)
   --------------------------------------------------------------------------- */
INSERT INTO dbo.States (CountryId, [Name], StateCode, GSTStateCode, IsActive, CreatedBy)
SELECT c.CountryId, s.[Name], s.StateCode, s.GSTStateCode, 1, 'system'
FROM (VALUES
    ('Tamil Nadu', 'TN', '33'),
    ('Karnataka', 'KA', '29'),
    ('Kerala', 'KL', '32'),
    ('Andhra Pradesh', 'AP', '37'),
    ('Telangana', 'TS', '36'),
    ('Maharashtra', 'MH', '27')
) AS s([Name], StateCode, GSTStateCode)
INNER JOIN dbo.Countries c ON c.ISOCode2 = 'IN'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.States t WHERE t.CountryId = c.CountryId AND t.StateCode = s.StateCode
);

/* ---------------------------------------------------------------------------
   Cities (India)
   --------------------------------------------------------------------------- */
INSERT INTO dbo.Cities (CountryId, StateId, [Name], IsActive, CreatedBy)
SELECT c.CountryId, st.StateId, ci.[Name], 1, 'system'
FROM (VALUES
    ('TN', 'Chennai'), ('TN', 'Madurai'), ('TN', 'Coimbatore'), ('TN', 'Salem'),
    ('KA', 'Bengaluru'), ('KA', 'Mysuru'),
    ('KL', 'Kochi'), ('KL', 'Thiruvananthapuram'),
    ('MH', 'Mumbai'), ('MH', 'Pune')
) AS ci(StateCode, [Name])
INNER JOIN dbo.Countries c ON c.ISOCode2 = 'IN'
INNER JOIN dbo.States st ON st.CountryId = c.CountryId AND st.StateCode = ci.StateCode
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Cities t WHERE t.StateId = st.StateId AND t.[Name] = ci.[Name]
);

/* ---------------------------------------------------------------------------
   System Master Data - Currencies
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Currencies)
BEGIN
    INSERT INTO dbo.Currencies (CurrencyCode, CurrencyName, Symbol, ISOCode, DecimalPlaces, IsBaseCurrency, SortOrder)
    VALUES
    ('INR', 'Indian Rupee',         'Ã¢â€šÂ¹',  '356', 2, 1, 1),
    ('USD', 'US Dollar',            '$',  '840', 2, 0, 2),
    ('EUR', 'Euro',                 'Ã¢â€šÂ¬',  '978', 2, 0, 3),
    ('GBP', 'Pound Sterling',        'Ã‚Â£',  '826', 2, 0, 4),
    ('JPY', 'Japanese Yen',         'Ã‚Â¥',  '392', 0, 0, 5),
    ('CNY', 'Yuan Renminbi',        'Ã‚Â¥',  '156', 2, 0, 6),
    ('CHF', 'Swiss Franc',          'CHF', '756', 2, 0, 7),
    ('AUD', 'Australian Dollar',    'A$', '036', 2, 0, 8),
    ('CAD', 'Canadian Dollar',      'C$', '124', 2, 0, 9),
    ('NZD', 'New Zealand Dollar',   'NZ$', '554', 2, 0, 10),
    ('KRW', 'South Korean Won',    'Ã¢â€šÂ©',  '410', 0, 0, 11),
    ('SGD', 'Singapore Dollar',    'S$', '702', 2, 0, 12),
    ('MYR', 'Malaysian Ringgit',  'RM', '458', 2, 0, 13),
    ('THB', 'Thai Baht',           'Ã Â¸Â¿',  '764', 2, 0, 14),
    ('IDR', 'Indonesian Rupiah',  'Rp', '360', 2, 0, 15),
    ('PHP', 'Philippine Peso',    'Ã¢â€šÂ±',  '608', 2, 0, 16),
    ('VND', 'Vietnamese Dong',    'Ã¢â€šÂ«',  '704', 2, 0, 17),
    ('MXN', 'Mexican Peso',       '$',  '484', 2, 0, 18),
    ('AED', 'UAE Dirham',         'Ã˜Â¯.Ã˜Â¥', '784', 2, 0, 19),
    ('SAR', 'Saudi Riyal',        'Ã¯Â·Â¼',  '682', 2, 0, 20),
    ('QAR', 'Qatari Riyal',       'Ã¯Â·Â¼',  '634', 2, 0, 21),
    ('KWD', 'Kuwaiti Dinar',      'Ã˜Â¯.Ã™Æ’', '414', 3, 0, 22),
    ('BHD', 'Bahraini Dinar',     '.Ã˜Â¯.Ã˜Â¨', '048', 3, 0, 23),
    ('OMR', 'Omani Rial',         'Ã¯Â·Â¼',  '512', 3, 0, 24),
    ('EGP', 'Egyptian Pound',     'Ã‚Â£',  '818', 2, 0, 25),
    ('ZAR', 'South African Rand', 'R',  '710', 2, 0, 26),
    ('NGN', 'Nigerian Naira',    'Ã¢â€šÂ¦',  '566', 2, 0, 27),
    ('KES', 'Kenyan Shilling',   'KSh', '400', 2, 0, 28),
    ('TRY', 'Turkish Lira',      'Ã¢â€šÂº',  '949', 2, 0, 29),
    ('RUB', 'Russian Ruble',     'Ã¢â€šÂ½',  '643', 2, 0, 30),
    ('BRL', 'Brazilian Real',    'R$', '986', 2, 0, 31),
    ('ARS', 'Argentine Peso',    '$',  '032', 2, 0, 32),
    ('BDT', 'Bangladeshi Taka', 'Ã Â§Â³',  '050', 2, 0, 33),
    ('LKR', 'Sri Lankan Rupee', 'Rs',  '144', 2, 0, 34),
    ('NPR', 'Nepalese Rupee',   'Ã¢â€šÂ¨',  '524', 2, 0, 35),
    ('PKR', 'Pakistani Rupee',  'Ã¢â€šÂ¨',  '586', 2, 0, 36);
END

/* ---------------------------------------------------------------------------
   System Master Data - Languages
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Languages)
BEGIN
    INSERT INTO dbo.Languages ([Name], Code, CultureCode, IsRTL, IsDefault, SortOrder)
    VALUES
    ('English', 'en', 'en-IN', 0, 1, 1),
    ('Tamil',   'ta', 'ta-IN', 0, 0, 2),
    ('Hindi',   'hi', 'hi-IN', 0, 0, 3),
    ('Arabic',  'ar', 'ar-SA', 1, 0, 4),
    ('French',  'fr', 'fr-FR', 0, 0, 5);
END

/* ---------------------------------------------------------------------------
   System Master Data - TimeZones
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.TimeZones)
BEGIN
    INSERT INTO dbo.TimeZones ([Name], TimeZoneName, UTCOffset)
    VALUES
    ('India Standard Time',      'Asia/Kolkata',       'UTC+05:30'),
    ('UTC',                      'UTC',                'UTC+00:00'),
    ('Singapore Standard Time',  'Asia/Singapore',     'UTC+08:00'),
    ('Arabian Standard Time',    'Asia/Dubai',         'UTC+04:00'),
    ('Eastern Standard Time',    'America/New_York',   'UTC-05:00');
END

/* ---------------------------------------------------------------------------
   System Master Data - GSTRegistrationTypes
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.GSTRegistrationTypes)
BEGIN
    INSERT INTO dbo.GSTRegistrationTypes ([Name], [Description])
    VALUES
    ('Regular',       'Regular GST Registration'),
    ('Composition',   'Composition Scheme'),
    ('SEZ',           'Special Economic Zone'),
    ('Unregistered',  'Not GST Registered'),
    ('Consumer',      'End Consumer');
END

/* ---------------------------------------------------------------------------
   Master Data - BusinessTypes
   -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.BusinessTypes)
BEGIN
    INSERT INTO dbo.BusinessTypes ([Name], [Description], SortOrder)
    VALUES
    ('Retail',          'Retail Business',          1),
    ('Wholesale',       'Wholesale Business',       2),
    ('Distribution',    'Distribution Business',    3),
    ('Trading',         'Trading Company',          4),
    ('Manufacturing',   'Manufacturing Company',    5),
    ('Service',         'Service Business',         6),
    ('E-Commerce',      'Online Business',          7),
    ('Construction',    'Construction Business',    8),
    ('Healthcare',      'Healthcare Organization',  9),
    ('Education',       'Educational Institution',  10),
    ('Hospitality',     'Hotel & Restaurant',       11),
    ('Logistics',       'Transport & Logistics',    12),
    ('Finance',         'Financial Services',       13),
    ('Agriculture',     'Agriculture Business',       14),
    ('Multi Business',  'Multiple Business Activities', 15);
END

/* ---------------------------------------------------------------------------
   Master Data - BusinessPartnerRoles
   -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.BusinessPartnerRoles)
BEGIN
    INSERT INTO dbo.BusinessPartnerRoles ([Code], [Name], [Description], IsActive)
    VALUES
    ('CUSTOMER',    'Customer',    'Customer of the organization',                1),
    ('VENDOR',      'Vendor',      'Supplier providing goods or services',        1),
    ('PROSPECT',    'Prospect',    'Potential customer not yet converted',        1),
    ('EMPLOYEE',    'Employee',    'Internal staff member',                       1),
    ('CONTRACTOR',  'Contractor',  'External contracted worker',                  1),
    ('PARTNER',     'Partner',     'Business alliance partner',                   1),
    ('AGENT',       'Agent',       'Sales or collection agent',                   1),
    ('DISTRIBUTOR', 'Distributor', 'Channel distributor',                        1),
    ('CONSIGNEE',   'Consignee',   'Goods consignee',                            1),
    ('CONSIGNOR',   'Consignor',   'Goods consignor',                            1),
    ('BANK',        'Bank',        'Banking or financial institution',           1),
    ('GOVERNMENT',  'Government',  'Government body or authority',               1),
    ('AUDITOR',     'Auditor',     'External auditor',                           1),
    ('BROKER',      'Broker',      'Broker or intermediary',                     1),
    ('TRANSPORTER', 'Transporter', 'Logistics or transport provider',            1),
    ('CONSULTANT',  'Consultant',  'Advisory consultant',                        1),
    ('FRANCHISEE',  'Franchisee',  'Franchise operator',                         1),
    ('AFFILIATE',   'Affiliate',   'Affiliate entity',                           1),
    ('OTHER',       'Other',       'Miscellaneous role',           1);
END

/* ---------------------------------------------------------------------------
   BusinessPartners (company business partners)
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BusinessPartners]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.BusinessPartners (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        CompanyId BIGINT NOT NULL,
        PartnerCode VARCHAR(30) NOT NULL,
        PartnerName VARCHAR(200) NOT NULL,
        PatnerRoleIds VARCHAR(100) NOT NULL,
        ContactPerson VARCHAR(150) NULL,
        MobileNo VARCHAR(20) NULL,
        Email VARCHAR(150) NULL,
        TaxRegistrationNo VARCHAR(50) NULL,
        CreditLimit DECIMAL(18,2) NOT NULL DEFAULT 0,
        CreditDays INT NOT NULL DEFAULT 0,
        PaymentTermId INT NULL,
        CurrencyId INT NULL,
        PriceListId INT NULL,
        Notes VARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedBy BIGINT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedBy BIGINT NULL,
        ModifiedAt DATETIME NULL
    );
END
;

/* ---------------------------------------------------------------------------
   Master Data - IndustryTypes
   -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.IndustryTypes)
BEGIN
    INSERT INTO dbo.IndustryTypes ([Name], [Description], SortOrder)
    VALUES
    ('Electronics',     'Mobile & Electronics',        1),
    ('Supermarket',     'Supermarket & Grocery',       2),
    ('Pharmacy',        'Medical & Pharmacy',          3),
    ('Textile',         'Garments & Textile',          4),
    ('Automobile',      'Automobile Industry',         5),
    ('Furniture',       'Furniture Industry',          6),
    ('Jewellery',       'Jewellery Business',          7),
    ('Food & Beverage', 'Food Industry',               8),
    ('IT',              'Software & IT Services',      9),
    ('Healthcare',      'Hospital & Medical',          10),
    ('Education',       'School & College',            11),
    ('Construction',    'Construction Industry',       12),
    ('Manufacturing',   'General Manufacturing',       13),
    ('Logistics',       'Transport & Logistics',       14),
    ('E-Commerce',      'Online Commerce',             15);
END

/* ---------------------------------------------------------------------------
   Master Data - CompanyGroups
   -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.CompanyGroups)
BEGIN
    INSERT INTO dbo.CompanyGroups (GroupCode, GroupName, ShortName, [Description])
    VALUES
    ('DEFAULT',      'Default Group',        'DEFAULT',     'Default Company Group'),
    ('RETAIL',       'Retail Group',         'RETAIL',      'Retail Business Group'),
    ('WHOLESALE',    'Wholesale Group',      'WHOLESALE',   'Wholesale Business Group'),
    ('DISTRIBUTION', 'Distribution Group',   'DIST',        'Distribution Companies'),
    ('MANUFACTURING','Manufacturing Group',  'MFG',         'Manufacturing Companies'),
    ('SERVICES',     'Services Group',       'SERV',        'Service Companies'),
    ('HEALTH',       'Healthcare Group',     'HEALTH',      'Healthcare Organizations'),
    ('EDU',          'Education Group',      'EDU',         'Educational Institutions');
END

/* ---------------------------------------------------------------------------
   System Master Data - AddressTypes
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.AddressTypes)
BEGIN
    INSERT INTO dbo.AddressTypes ([Name], [Description])
    VALUES
    ('Registered',  'Registered Office'),
    ('Billing',     'Billing Address'),
    ('Shipping',    'Shipping Address'),
    ('Office',      'Office Address'),
    ('Warehouse',   'Warehouse Address'),
    ('Factory',     'Factory Address');
END

/* ---------------------------------------------------------------------------
   System Master Data - ContactTypes
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.ContactTypes)
BEGIN
    INSERT INTO dbo.ContactTypes ([Name], [Description])
    VALUES
    ('Primary',   'Primary Contact'),
    ('Sales',     'Sales Contact'),
    ('Purchase',  'Purchase Contact'),
    ('Accounts',  'Accounts Contact'),
    ('HR',        'Human Resources'),
    ('Support',   'Customer Support');
END

/* ---------------------------------------------------------------------------
   System Master Data - DocumentTypes
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.DocumentTypes)
BEGIN
    INSERT INTO dbo.DocumentTypes ([Name], [Description])
    VALUES
    ('GST Certificate',     'GST Registration'),
    ('PAN Card',            'Permanent Account Number'),
    ('TAN Certificate',     'Tax Deduction Number'),
    ('CIN Certificate',     'Company Registration'),
    ('Trade License',       'Trade License'),
    ('MSME Certificate',    'MSME Registration'),
    ('Logo',                'Company Logo'),
    ('Digital Signature',   'Digital Signature');
END

/* ---------------------------------------------------------------------------
   System Master Data - OrganizationTypes
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.OrganizationTypes)
BEGIN
    INSERT INTO dbo.OrganizationTypes ([Name], Code, [Description], SortOrder)
    VALUES
    ('Head Office',    'HO',        'Corporate Head Office',  1),
    ('Branch',         'BRANCH',    'Company Branch',         2),
    ('Warehouse',      'WAREHOUSE', 'Warehouse',              3),
    ('Store',          'STORE',     'Retail Store',           4),
    ('Department',     'DEPARTMENT','Department',             5),
    ('Division',       'DIVISION',  'Business Division',      6),
    ('Business Unit',  'BU',        'Business Unit',          7),
    ('Cost Center',    'CC',        'Cost Center',            8),
    ('Profit Center',  'PC',        'Profit Center',          9),
    ('Factory',        'FACTORY',   'Manufacturing Unit',    10),
    ('Showroom',       'SHOWROOM',  'Sales Showroom',        11),
    ('Service Center', 'SERVICE',   'Service Center',        12);
END

/* ---------------------------------------------------------------------------
   Roles (must exist before Company for User assignment, and before permissions)
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Code = 'SuperAdmin')
BEGIN
    INSERT INTO dbo.Roles ([Name], Code, [Description], IsSystem, IsActive, CreatedBy)
    VALUES ('Super Admin', 'SuperAdmin', 'Full system access', 1, 1, 'system');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Code = 'Administrator')
BEGIN
    INSERT INTO dbo.Roles ([Name], Code, [Description], IsSystem, IsActive, CreatedBy)
    VALUES ('Administrator', 'Administrator', 'Administrative access', 1, 1, 'system');
END

SELECT @SuperAdminRoleId = RoleId FROM dbo.Roles WHERE Code = 'SuperAdmin';
SELECT @AdministratorRoleId = RoleId FROM dbo.Roles WHERE Code = 'Administrator';

/* ---------------------------------------------------------------------------
   Sales Admin role
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Code = 'SalesAdmin')
BEGIN
    INSERT INTO dbo.Roles ([Name], Code, [Description], IsSystem, IsActive, CreatedBy)
    VALUES ('Sales Admin', 'SalesAdmin', 'Sales management access', 0, 1, 'system');
END

SELECT @SalesAdminRoleId = RoleId FROM dbo.Roles WHERE Code = 'SalesAdmin';

INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode, CreatedBy)
SELECT @SalesAdminRoleId, p.PermissionCode, 'system'
FROM (VALUES
    ('dashboard.view'),
    /* SALES */
    ('sales.view'), ('sales.create'), ('sales.edit'), ('sales.delete'), ('sales.manage'),
    ('sales.pos.view'), ('sales.return.view'), ('sales.return.manage'),
    /* PRODBILL - product catalog lookup needed in sales */
    ('products.view'), ('product-categories.view'), ('product-subcategories.view'), ('brands.view'), ('units.view'),
    /* BUSINESS_PARTNERS - customers */
    ('business-partners.view'), ('business-partners.manage'),
    /* INVENTORY */
    ('stock.view'),
    /* PAYMENTS - collect/refund payments on sales */
    ('payment.view'), ('payment.create'), ('payment.edit'),
    /* REPORTS */
    ('reports.view'),
    /* PROFILE */
    ('profile.edit')
) AS p(PermissionCode)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissionsLegacy rp WHERE rp.RoleId = @SalesAdminRoleId AND rp.PermissionCode = p.PermissionCode
);

/* ---------------------------------------------------------------------------
   Business roles for the RolePermission Matrix (real role-based access).
   Each role's screen/action grants live in dbo.RolePermissions (seeded in the
   matrix block near the end of this file); the matrix->legacy bridge derives
   the flat API permission codes automatically. Here we only declare the roles
   and grant the shared dashboard access.
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Code = 'FinanceYearOfficer')
BEGIN
    INSERT INTO dbo.Roles ([Name], Code, [Description], IsSystem, IsActive, CreatedBy)
    VALUES ('Financial Year Officer', 'FinanceYearOfficer', 'Manages financial years and reporting', 0, 1, 'system');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Code = 'StoreManager')
BEGIN
    INSERT INTO dbo.Roles ([Name], Code, [Description], IsSystem, IsActive, CreatedBy)
    VALUES ('Store Manager', 'StoreManager', 'Manages stores, counters, POS sessions and stock', 0, 1, 'system');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Code = 'POSCashier')
BEGIN
    INSERT INTO dbo.Roles ([Name], Code, [Description], IsSystem, IsActive, CreatedBy)
    VALUES ('POS Cashier', 'POSCashier', 'Operates POS sales and payments', 0, 1, 'system');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Code = 'PurchaseAdmin')
BEGIN
    INSERT INTO dbo.Roles ([Name], Code, [Description], IsSystem, IsActive, CreatedBy)
    VALUES ('Purchase Admin', 'PurchaseAdmin', 'Manages purchase transactions and returns', 0, 1, 'system');
END

INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode, CreatedBy)
SELECT r.RoleId, 'dashboard.view', 'system'
FROM dbo.Roles r
WHERE r.Code IN ('FinanceYearOfficer', 'StoreManager', 'POSCashier', 'PurchaseAdmin')
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RolePermissionsLegacy l
      WHERE l.RoleId = r.RoleId AND l.PermissionCode = 'dashboard.view'
  );

/* ---------------------------------------------------------------------------
   Company (master data must exist for FK references)
   --------------------------------------------------------------------------- */
SELECT TOP 1 @CurrencyId = Id FROM dbo.Currencies WHERE CurrencyCode = @CurrencyCode;
SELECT TOP 1 @LanguageId = LanguageId FROM dbo.Languages WHERE IsDefault = 1;
SELECT TOP 1 @TimeZoneId = TimeZoneId FROM dbo.TimeZones ORDER BY TimeZoneId;
SELECT TOP 1 @BusinessTypeId = BusinessTypeId FROM dbo.BusinessTypes;
SELECT TOP 1 @IndustryTypeId = IndustryTypeId FROM dbo.IndustryTypes;

IF NOT EXISTS (SELECT 1 FROM dbo.Companies WHERE CompanyCode = @CompanyCode)
BEGIN
    INSERT INTO dbo.Companies (CompanyCode, CompanyName, ShortName, Abbreviation, BusinessTypeId, IndustryTypeId,
        GSTRegistrationTypeId, GSTNumber, PANNumber, TANNumber, CINNumber, RegistrationNumber,
        CurrencyId, LanguageId, TimeZoneId, IsActive, IsBlocked, IsDeleted, LastLoginDate, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
    VALUES (@CompanyCode, @CompanyName, NULL, @CompanyCode, @BusinessTypeId, @IndustryTypeId,
        NULL, NULL, NULL, NULL, NULL, @CompanyCode + '-10001',
        @CurrencyId, @LanguageId, @TimeZoneId, 1, 0, 0, NULL, 0, SYSUTCDATETIME(), NULL, NULL);
    SET @CompanyId = SCOPE_IDENTITY();
END
ELSE
    SELECT @CompanyId = Id FROM dbo.Companies WHERE CompanyCode = @CompanyCode;

/* ---------------------------------------------------------------------------
    Permissions (SuperAdmin + Administrator)
    --------------------------------------------------------------------------- */
INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode, CreatedBy)
SELECT @SuperAdminRoleId, p.PermissionCode, 'system'
FROM (VALUES
    ('dashboard.view'),
    ('companies.view'), ('companies.create'), ('companies.edit'),
    ('users.view'), ('users.create'), ('users.edit'), ('users.delete'),
    ('roles.view'), ('roles.manage'),
    ('locations.view'), ('locations.create'), ('locations.edit'), ('locations.delete'),
    ('business-types.view'), ('business-types.manage'),
    ('business-partners.view'), ('business-partners.manage'),
    ('industry-types.view'), ('industry-types.manage'),
    ('company-groups.view'), ('company-groups.manage'),
    ('settings.view'), ('settings.edit'),
    ('audit.view'),
    ('currencies.view'), ('currencies.manage'),
    ('financial-years.view'), ('financial-years.create'), ('financial-years.edit'), ('financial-years.delete'),
    ('profile.edit')
) AS p(PermissionCode)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissionsLegacy rp WHERE rp.RoleId = @SuperAdminRoleId AND rp.PermissionCode = p.PermissionCode
);

INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode, CreatedBy)
SELECT @AdministratorRoleId, p.PermissionCode, 'system'
FROM (VALUES
    ('dashboard.view'),
    ('companies.view'), ('companies.create'), ('companies.edit'),
    ('users.view'), ('users.create'), ('users.edit'), ('users.delete'),
    ('roles.view'),
    ('locations.view'), ('locations.create'), ('locations.edit'), ('locations.delete'),
    ('business-types.view'), ('business-types.manage'),
    ('business-partners.view'), ('business-partners.manage'),
    ('industry-types.view'), ('industry-types.manage'),
    ('company-groups.view'), ('company-groups.manage'),
    ('settings.view'), ('settings.edit'),
    ('currencies.view'), ('currencies.manage'),
    ('financial-years.view'), ('financial-years.create'), ('financial-years.edit'), ('financial-years.delete'),
    ('profile.edit')
) AS p(PermissionCode)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissionsLegacy rp WHERE rp.RoleId = @AdministratorRoleId AND rp.PermissionCode = p.PermissionCode
);

/* ---------------------------------------------------------------------------
    ERP Admin user (unique per tenant, BCrypt hashed)
    --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @AdminUsername)
BEGIN
    INSERT INTO dbo.Users (CompanyId, Username, Email, Mobile, FullName, PasswordHash, Status, IsSuperAdmin, CreatedBy)
    VALUES (@CompanyId, @AdminUsername, @CompanyEmail, '+1 000 000 0000', @AdminFullName, @AdminPasswordHash, 'Active', 1, 'system');
    SET @AdminUserId = SCOPE_IDENTITY();
    INSERT INTO dbo.UserRoles (UserId, RoleId, CreatedBy)
    VALUES (@AdminUserId, @SuperAdminRoleId, 'system');
END

/* ---------------------------------------------------------------------------
    Application Settings
    --------------------------------------------------------------------------- */
INSERT INTO dbo.ApplicationSettings (SettingKey, SettingValue, [Description], UpdatedBy)
SELECT k.SettingKey, k.SettingValue, k.[Description], 'system'
FROM (VALUES
    ('Company.Name', @CompanyName, 'Company display name'),
    ('Company.Address', '123 Business Avenue, City, Country', 'Company address'),
    ('Company.Email', @CompanyEmail, 'Company email'),
    ('Company.Phone', '+1 000 000 0000', 'Company phone'),
    ('Company.Currency', @CurrencyCode, 'Default currency'),
    ('Security.PasswordMinLength', '8', 'Minimum password length'),
    ('Security.PasswordRequireSpecial', 'True', 'Require special character in password')
) AS k(SettingKey, SettingValue, [Description])
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ApplicationSettings s WHERE s.SettingKey = k.SettingKey
);

/* ---------------------------------------------------------------------------
    System Master Permissions (both SuperAdmin and Administrator)
    --------------------------------------------------------------------------- */
INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode, CreatedBy)
SELECT r.RoleId, p.PermissionCode, 'system'
FROM (VALUES
    ('languages.view'), ('languages.manage'),
    ('timezones.view'), ('timezones.manage'),
    ('gst-registration-types.view'), ('gst-registration-types.manage'),
    ('address-types.view'), ('address-types.manage'),
    ('contact-types.view'), ('contact-types.manage'),
    ('document-types.view'), ('document-types.manage'),
    ('organization-types.view'), ('organization-types.manage'),
    ('business-partners.view'), ('business-partners.manage'),
    ('currencies.view'), ('currencies.manage')
) AS p(PermissionCode)
CROSS JOIN (SELECT RoleId FROM dbo.Roles WHERE Code IN ('SuperAdmin', 'Administrator')) r
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissionsLegacy rp WHERE rp.RoleId = r.RoleId AND rp.PermissionCode = p.PermissionCode
);

/* ---------------------------------------------------------------------------
    BranchTypes
    --------------------------------------------------------------------------- */
INSERT INTO dbo.BranchTypes ([Name], Code, [Description], SortOrder, IsActive, CreatedBy)
SELECT k.[Name], k.Code, k.[Description], k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('Head Office', 'HQ', 'Main headquarters branch', 1, 1, 'system'),
    ('Sales Branch', 'SAL', 'Sales and marketing branch', 2, 1, 'system'),
    ('Purchase Branch', 'PUR', 'Purchase and procurement branch', 3, 1, 'system'),
    ('Service Branch', 'SRV', 'Service and support branch', 4, 1, 'system'),
    ('Warehouse', 'WH', 'Warehouse and inventory branch', 5, 1, 'system')
) AS k([Name], Code, [Description], SortOrder, IsActive, CreatedBy)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.BranchTypes bt WHERE bt.Code = k.Code
);

/* ---------------------------------------------------------------------------
    WarehouseTypes
    --------------------------------------------------------------------------- */
INSERT INTO dbo.WarehouseTypes ([Name], Code, [Description], SortOrder, IsActive, CreatedBy)
SELECT k.[Name], k.Code, k.[Description], k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('Main Warehouse', 'MAIN', 'Primary warehouse', 1, 1, 'system'),
    ('Sales Warehouse', 'SAL', 'Sales warehouse', 2, 1, 'system'),
    ('Purchase Warehouse', 'PUR', 'Purchase warehouse', 3, 1, 'system'),
    ('Return Warehouse', 'RET', 'Returns and defective goods', 4, 1, 'system'),
    ('Raw Material', 'RAW', 'Raw material warehouse', 5, 1, 'system'),
    ('Finished Goods', 'FGD', 'Finished goods warehouse', 6, 1, 'system')
) AS k([Name], Code, [Description], SortOrder, IsActive, CreatedBy)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.WarehouseTypes wt WHERE wt.Code = k.Code
);

/* ---------------------------------------------------------------------------
    EmploymentTypes
    --------------------------------------------------------------------------- */
INSERT INTO dbo.EmploymentTypes ([Name], Code, [Description], SortOrder, IsActive, CreatedBy)
SELECT k.[Name], k.Code, k.[Description], k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('Full Time', 'FT', 'Full-time employee', 1, 1, 'system'),
    ('Part Time', 'PT', 'Part-time employee', 2, 1, 'system'),
    ('Contract', 'CT', 'Contract employee', 3, 1, 'system'),
    ('Intern', 'IN', 'Intern or trainee', 4, 1, 'system'),
    ('Temporary', 'TMP', 'Temporary employee', 5, 1, 'system')
) AS k([Name], Code, [Description], SortOrder, IsActive, CreatedBy)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.EmploymentTypes et WHERE et.Code = k.Code
);

/* ---------------------------------------------------------------------------
    Genders
    --------------------------------------------------------------------------- */
INSERT INTO dbo.Genders ([Name], Code, SortOrder)
SELECT k.[Name], k.Code, k.SortOrder
FROM (VALUES
    ('Male', 'M', 1),
    ('Female', 'F', 2),
    ('Other', 'O', 3)
) AS k([Name], Code, SortOrder)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Genders g WHERE g.Code = k.Code
);

/* ---------------------------------------------------------------------------
    MaritalStatuses
    --------------------------------------------------------------------------- */
INSERT INTO dbo.MaritalStatuses ([Name], Code, SortOrder)
SELECT k.[Name], k.Code, k.SortOrder
FROM (VALUES
    ('Single', 'S', 1),
    ('Married', 'M', 2),
    ('Divorced', 'D', 3),
    ('Widowed', 'W', 4)
) AS k([Name], Code, SortOrder)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.MaritalStatuses ms WHERE ms.Code = k.Code
);

/* =============================================================================
   Product / Billing Masters (merged from product_masters_init.sql, product_init.sql)
   ============================================================================= */

/* ---------------------------------------------------------------------------
   ProductSubCategories
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductSubCategories]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.ProductSubCategories (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        CompanyId BIGINT NOT NULL,
        CategoryId BIGINT NOT NULL,
        SubCategoryCode VARCHAR(30) NOT NULL,
        SubCategoryName VARCHAR(100) NOT NULL,
        Description VARCHAR(500) NULL,
        SortOrder INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedBy BIGINT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedBy BIGINT NULL,
        ModifiedAt DATETIME NULL
    );
END
;

/* ---------------------------------------------------------------------------
   ProductCategories
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductCategories]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.ProductCategories (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        CompanyId BIGINT NOT NULL,
        CategoryCode VARCHAR(30) NOT NULL,
        CategoryName VARCHAR(100) NOT NULL,
        Description VARCHAR(500) NULL,
        ParentCategoryId BIGINT NULL,
        SortOrder INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedBy BIGINT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedBy BIGINT NULL,
        ModifiedAt DATETIME NULL
    );
END
;

/* ---------------------------------------------------------------------------
   ProductBrands
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductBrands]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.ProductBrands (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        CompanyId BIGINT NOT NULL,
        BrandCode VARCHAR(30) NOT NULL,
        BrandName VARCHAR(100) NOT NULL,
        Description VARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedBy BIGINT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedBy BIGINT NULL,
        ModifiedAt DATETIME NULL
    );
END
;

/* ---------------------------------------------------------------------------
   Units
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Units]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Units (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        CompanyId BIGINT NOT NULL,
        UnitCode VARCHAR(20) NOT NULL,
        UnitName VARCHAR(50) NOT NULL,
        Symbol VARCHAR(20) NULL,
        DecimalPlaces INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedBy BIGINT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedBy BIGINT NULL,
        ModifiedAt DATETIME NULL
    );
END
;

/* ---------------------------------------------------------------------------
   Products
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Products (
        Id BIGINT IDENTITY(1,1) CONSTRAINT PK_Products PRIMARY KEY,
        CompanyId BIGINT NOT NULL,
        BranchId BIGINT NULL,
        ProductCode VARCHAR(30) NOT NULL,
        ProductName VARCHAR(200) NOT NULL,
        CategoryId BIGINT NULL,
        SubCategoryId BIGINT NULL,
        BrandId BIGINT NULL,
        UOMId BIGINT NOT NULL,
        SKU VARCHAR(50) NULL,
        Barcode VARCHAR(100) NULL,
        MRP DECIMAL(18,2) NULL,
        PurchasePrice DECIMAL(18,2) NULL,
        SalesPrice DECIMAL(18,2) NULL,
        TaxId BIGINT NULL,
        HsnSacId BIGINT NULL,
        IsStockItem BIT NOT NULL DEFAULT 1,
        IsSaleable BIT NOT NULL DEFAULT 1,
        IsPurchaseable BIT NOT NULL DEFAULT 1,
        IsActive BIT NOT NULL DEFAULT 1,
        Description VARCHAR(500) NULL,
        CreatedBy BIGINT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedBy BIGINT NULL,
        ModifiedAt DATETIME NULL
    );
END
;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'HsnSacId')
BEGIN
    ALTER TABLE dbo.Products ADD HsnSacId BIGINT NULL;
END
;

/* =============================================================================
   Tax Masters (merged from tax_init.sql, taxes_init.sql)
   ============================================================================= */

/* ---------------------------------------------------------------------------
   TaxTypeSystems
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaxTypeSystems]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.TaxTypeSystems (
        Id BIGINT IDENTITY(1,1) CONSTRAINT PK_TaxTypeSystems PRIMARY KEY,
        Code VARCHAR(30) NOT NULL,
        [Name] VARCHAR(100) NOT NULL,
        Description VARCHAR(300) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedBy BIGINT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
;

INSERT INTO dbo.TaxTypeSystems (Code, [Name], Description, IsActive)
SELECT k.Code, k.[Name], k.Description, k.IsActive
FROM (VALUES
    ('TAXTYPE-001', 'GST',          'Goods and Services Tax', 1),
    ('TAXTYPE-002', 'VAT',          'Value Added Tax',        1),
    ('TAXTYPE-003', 'Sales Tax',    'Sales Tax',              1),
    ('TAXTYPE-004', 'Service Tax',  'Service Tax',            1),
    ('TAXTYPE-005', 'Excise Tax',   'Excise Tax',             1),
    ('TAXTYPE-006', 'Customs Duty', 'Customs Duty',           1)
) AS k(Code, [Name], Description, IsActive)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TaxTypeSystems t WHERE t.Code = k.Code);
;

/* ---------------------------------------------------------------------------
   Taxes
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Taxes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Taxes (
        Id BIGINT IDENTITY(1,1) CONSTRAINT PK_Taxes PRIMARY KEY,
        CompanyId BIGINT NOT NULL,
        BranchId BIGINT NULL,
        TaxTypeSystemId BIGINT NOT NULL,
        TaxCode VARCHAR(30) NOT NULL,
        TaxName VARCHAR(100) NOT NULL,
        TaxRate DECIMAL(8,4) NOT NULL,
        IsInclusive BIT NOT NULL DEFAULT 0,
        EffectiveFrom DATE NULL,
        EffectiveTo DATE NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        Description VARCHAR(300) NULL,
        CreatedBy BIGINT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedBy BIGINT NULL,
        ModifiedAt DATETIME NULL,
        CONSTRAINT FK_Taxes_TaxTypeSystems FOREIGN KEY (TaxTypeSystemId) REFERENCES dbo.TaxTypeSystems(Id)
    );
END
;

/* =============================================================================
   Billing Masters (Unit Conversion, Barcode, HSN/SAC, Services, Price Lists)
   Adapted to the ERP's plural table + Id PK conventions.
   Company-scoped tables store a bare CompanyId (BIGINT) like Products/Units/Taxes
   (no FK to dbo.Companies). System masters (PriceTypes) carry no CompanyId.
   ============================================================================= */

/* ---------------------------------------------------------------------------
   PriceTypes (system master: pricing types for PriceLists). No CompanyId.
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PriceTypes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PriceTypes (
        PriceTypeId BIGINT IDENTITY(1,1) CONSTRAINT PK_PriceTypes PRIMARY KEY,
        Code        VARCHAR(30)  NOT NULL,
        [Name]      NVARCHAR(100) NOT NULL,
        Description NVARCHAR(300) NULL,
        IsActive    BIT          NOT NULL CONSTRAINT DF_PriceTypes_IsActive DEFAULT 1,
        DisplayOrder INT         NOT NULL CONSTRAINT DF_PriceTypes_DisplayOrder DEFAULT 0,
        CreatedBy   BIGINT       NULL,
        CreatedAt   DATETIME     NOT NULL CONSTRAINT DF_PriceTypes_CreatedAt DEFAULT GETDATE(),
        ModifiedBy  BIGINT       NULL,
        ModifiedAt  DATETIME     NULL,
        CONSTRAINT UQ_PriceTypes_Code UNIQUE (Code)
    );

    CREATE INDEX IX_PriceTypes_IsActive ON dbo.PriceTypes (IsActive);
END
;

INSERT INTO dbo.PriceTypes (Code, [Name], Description, IsActive, DisplayOrder)
SELECT k.Code, k.[Name], k.Description, k.IsActive, k.DisplayOrder
FROM (VALUES
    ('PRICETYPE-001', 'Sales Price',    'Default selling price',        1, 1),
    ('PRICETYPE-002', 'Purchase Price', 'Default purchase price',       1, 2),
    ('PRICETYPE-003', 'Wholesale Price','Wholesale quantity price',     1, 3),
    ('PRICETYPE-004', 'Retail Price',   'Retail shelf price',           1, 4),
    ('PRICETYPE-005', 'Distributor Price','Distributor pricing',        1, 5),
    ('PRICETYPE-006', 'Special Price',  'Promotional/special pricing',  1, 6)
) AS k(Code, [Name], Description, IsActive, DisplayOrder)
WHERE NOT EXISTS (SELECT 1 FROM dbo.PriceTypes p WHERE p.Code = k.Code);
;

/* ---------------------------------------------------------------------------
   UnitConversions (company-scoped). ProductId nullable (common or per-product).
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UnitConversions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.UnitConversions (
        UnitConversionId BIGINT IDENTITY(1,1) CONSTRAINT PK_UnitConversions PRIMARY KEY,
        CompanyId        BIGINT        NOT NULL,
        ProductId        BIGINT        NULL,
        FromUnitId       BIGINT        NOT NULL,
        ToUnitId         BIGINT        NOT NULL,
        ConversionFactor DECIMAL(18,6) NOT NULL,
        IsDefault        BIT           NOT NULL CONSTRAINT DF_UnitConversions_IsDefault DEFAULT 0,
        IsActive         BIT           NOT NULL CONSTRAINT DF_UnitConversions_IsActive DEFAULT 1,
        CreatedBy        BIGINT        NULL,
        CreatedAt        DATETIME      NOT NULL CONSTRAINT DF_UnitConversions_CreatedAt DEFAULT GETDATE(),
        ModifiedBy       BIGINT        NULL,
        ModifiedAt       DATETIME      NULL,
        CONSTRAINT CK_UnitConversions_Factor CHECK (ConversionFactor > 0),
        CONSTRAINT CK_UnitConversions_DifferentUnits CHECK (FromUnitId <> ToUnitId),
        CONSTRAINT FK_UnitConversions_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id),
        CONSTRAINT FK_UnitConversions_FromUnit FOREIGN KEY (FromUnitId) REFERENCES dbo.Units (Id),
        CONSTRAINT FK_UnitConversions_ToUnit FOREIGN KEY (ToUnitId) REFERENCES dbo.Units (Id)
    );

    CREATE INDEX IX_UnitConversions_Company ON dbo.UnitConversions (CompanyId);
    CREATE INDEX IX_UnitConversions_Product ON dbo.UnitConversions (ProductId);
END
;

/* ---------------------------------------------------------------------------
   Barcodes (company-scoped).
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Barcodes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Barcodes (
        BarcodeId   BIGINT IDENTITY(1,1) CONSTRAINT PK_Barcodes PRIMARY KEY,
        CompanyId   BIGINT       NOT NULL,
        ProductId   BIGINT       NOT NULL,
        UnitId      BIGINT       NULL,
        Barcode     VARCHAR(100) NOT NULL,
        BarcodeType VARCHAR(30)  NULL,
        IsPrimary   BIT          NOT NULL CONSTRAINT DF_Barcodes_IsPrimary DEFAULT 0,
        IsActive    BIT          NOT NULL CONSTRAINT DF_Barcodes_IsActive DEFAULT 1,
        CreatedBy   BIGINT       NULL,
        CreatedAt   DATETIME     NOT NULL CONSTRAINT DF_Barcodes_CreatedAt DEFAULT GETDATE(),
        ModifiedBy  BIGINT       NULL,
        ModifiedAt  DATETIME     NULL,
        CONSTRAINT UQ_Barcodes_Company_Barcode UNIQUE (CompanyId, Barcode),
        CONSTRAINT FK_Barcodes_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id),
        CONSTRAINT FK_Barcodes_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Units (Id)
    );

    CREATE INDEX IX_Barcodes_Company ON dbo.Barcodes (CompanyId);
    CREATE INDEX IX_Barcodes_Product ON dbo.Barcodes (ProductId);
END
;

/* ---------------------------------------------------------------------------
   HsnSacs (company-scoped GST/SAC codes).
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HsnSacs]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.HsnSacs (
        HsnSacId    BIGINT IDENTITY(1,1) CONSTRAINT PK_HsnSacs PRIMARY KEY,
        CompanyId   BIGINT       NOT NULL,
        Code        VARCHAR(20)  NOT NULL,
        [Name]      VARCHAR(200) NOT NULL,
        HsnSacType  VARCHAR(10)  NOT NULL,
        Description VARCHAR(500) NULL,
        TaxId       BIGINT       NULL,
        IsActive    BIT          NOT NULL CONSTRAINT DF_HsnSacs_IsActive DEFAULT 1,
        CreatedBy   BIGINT       NULL,
        CreatedAt   DATETIME     NOT NULL CONSTRAINT DF_HsnSacs_CreatedAt DEFAULT GETDATE(),
        ModifiedBy  BIGINT       NULL,
        ModifiedAt  DATETIME     NULL,
        CONSTRAINT UQ_HsnSacs_Company_Code UNIQUE (CompanyId, Code),
        CONSTRAINT CK_HsnSacs_Type CHECK (HsnSacType IN ('HSN', 'SAC')),
        CONSTRAINT FK_HsnSacs_Tax FOREIGN KEY (TaxId) REFERENCES dbo.Taxes (Id)
    );

    CREATE INDEX IX_HsnSacs_Company ON dbo.HsnSacs (CompanyId);
END
;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HsnSacs]') AND name = 'TaxId')
BEGIN
    ALTER TABLE dbo.HsnSacs ADD TaxId BIGINT NULL;
END
;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HsnSacs]') AND name = 'TaxId')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HsnSacs_Tax')
        ALTER TABLE dbo.HsnSacs ADD CONSTRAINT FK_HsnSacs_Tax FOREIGN KEY (TaxId) REFERENCES dbo.Taxes (Id);
END
;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Products_HsnSac')
BEGIN
    ALTER TABLE dbo.Products ADD CONSTRAINT FK_Products_HsnSac FOREIGN KEY (HsnSacId) REFERENCES dbo.HsnSacs (HsnSacId);
END
;

/* ---------------------------------------------------------------------------
   ServiceCategories (company-scoped).
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServiceCategories]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.ServiceCategories (
        ServiceCategoryId BIGINT IDENTITY(1,1) CONSTRAINT PK_ServiceCategories PRIMARY KEY,
        CompanyId         BIGINT       NOT NULL,
        Code              VARCHAR(30)  NOT NULL,
        [Name]            VARCHAR(100) NOT NULL,
        Description       VARCHAR(300) NULL,
        DisplayOrder      INT          NOT NULL CONSTRAINT DF_ServiceCategories_DisplayOrder DEFAULT 0,
        IsActive          BIT          NOT NULL CONSTRAINT DF_ServiceCategories_IsActive DEFAULT 1,
        CreatedBy         BIGINT       NULL,
        CreatedAt         DATETIME     NOT NULL CONSTRAINT DF_ServiceCategories_CreatedAt DEFAULT GETDATE(),
        ModifiedBy        BIGINT       NULL,
        ModifiedAt        DATETIME     NULL,
        CONSTRAINT UQ_ServiceCategories_Company_Code UNIQUE (CompanyId, Code)
    );

    CREATE INDEX IX_ServiceCategories_Company ON dbo.ServiceCategories (CompanyId);
END
;

/* ---------------------------------------------------------------------------
   Services (company-scoped).
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Services (
        ServiceId         BIGINT IDENTITY(1,1) CONSTRAINT PK_Services PRIMARY KEY,
        CompanyId         BIGINT        NOT NULL,
        Code              VARCHAR(30)   NOT NULL,
        [Name]            VARCHAR(200)  NOT NULL,
        ServiceCategoryId BIGINT        NULL,
        UnitId            BIGINT        NULL,
        HsnSacId          BIGINT        NULL,
        DefaultTaxId      BIGINT        NULL,
        StandardRate      DECIMAL(18,2) NOT NULL CONSTRAINT DF_Services_StandardRate DEFAULT 0,
        IsTaxInclusive    BIT           NOT NULL CONSTRAINT DF_Services_IsTaxInclusive DEFAULT 0,
        Description       VARCHAR(500)  NULL,
        IsActive          BIT           NOT NULL CONSTRAINT DF_Services_IsActive DEFAULT 1,
        CreatedBy         BIGINT        NULL,
        CreatedAt         DATETIME      NOT NULL CONSTRAINT DF_Services_CreatedAt DEFAULT GETDATE(),
        ModifiedBy        BIGINT        NULL,
        ModifiedAt        DATETIME      NULL,
        CONSTRAINT UQ_Services_Company_Code UNIQUE (CompanyId, Code),
        CONSTRAINT CK_Services_StandardRate CHECK (StandardRate >= 0),
        CONSTRAINT FK_Services_Category FOREIGN KEY (ServiceCategoryId) REFERENCES dbo.ServiceCategories (ServiceCategoryId),
        CONSTRAINT FK_Services_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Units (Id),
        CONSTRAINT FK_Services_HsnSac FOREIGN KEY (HsnSacId) REFERENCES dbo.HsnSacs (HsnSacId),
        CONSTRAINT FK_Services_Tax FOREIGN KEY (DefaultTaxId) REFERENCES dbo.Taxes (Id)
    );

    CREATE INDEX IX_Services_Company ON dbo.Services (CompanyId);
END
;

/* ---------------------------------------------------------------------------
   PriceLists (company-scoped).
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PriceLists]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PriceLists (
        PriceListId   BIGINT IDENTITY(1,1) CONSTRAINT PK_PriceLists PRIMARY KEY,
        CompanyId     BIGINT       NOT NULL,
        PriceTypeId   BIGINT       NOT NULL,
        CurrencyId    INT          NOT NULL,
        Code          VARCHAR(30)  NOT NULL,
        [Name]        VARCHAR(100) NOT NULL,
        Description   VARCHAR(300) NULL,
        EffectiveFrom DATE         NULL,
        EffectiveTo   DATE         NULL,
        IsDefault     BIT          NOT NULL CONSTRAINT DF_PriceLists_IsDefault DEFAULT 0,
        IsActive      BIT          NOT NULL CONSTRAINT DF_PriceLists_IsActive DEFAULT 1,
        CreatedBy     BIGINT       NULL,
        CreatedAt     DATETIME     NOT NULL CONSTRAINT DF_PriceLists_CreatedAt DEFAULT GETDATE(),
        ModifiedBy    BIGINT       NULL,
        ModifiedAt    DATETIME     NULL,
        CONSTRAINT UQ_PriceLists_Company_Code UNIQUE (CompanyId, Code),
        CONSTRAINT CK_PriceLists_Dates CHECK (EffectiveTo IS NULL OR EffectiveFrom IS NULL OR EffectiveFrom <= EffectiveTo),
        CONSTRAINT FK_PriceLists_PriceType FOREIGN KEY (PriceTypeId) REFERENCES dbo.PriceTypes (PriceTypeId),
        CONSTRAINT FK_PriceLists_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.Currencies (Id)
    );

    CREATE INDEX IX_PriceLists_Company ON dbo.PriceLists (CompanyId);
    CREATE INDEX IX_PriceLists_PriceType ON dbo.PriceLists (PriceTypeId);
END
;

/* ---------------------------------------------------------------------------
   PriceListDetails (child of PriceList).
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PriceListDetails]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PriceListDetails (
        PriceListDetailId BIGINT IDENTITY(1,1) CONSTRAINT PK_PriceListDetails PRIMARY KEY,
        PriceListId       BIGINT        NOT NULL,
        ProductId         BIGINT        NOT NULL,
        UnitId            BIGINT        NULL,
        Price             DECIMAL(18,4) NOT NULL,
        MinimumQuantity   DECIMAL(18,4) NOT NULL CONSTRAINT DF_PriceListDetails_MinQty DEFAULT 1,
        MaximumQuantity   DECIMAL(18,4) NULL,
        IsActive          BIT           NOT NULL CONSTRAINT DF_PriceListDetails_IsActive DEFAULT 1,
        CreatedBy         BIGINT        NULL,
        CreatedAt         DATETIME      NOT NULL CONSTRAINT DF_PriceListDetails_CreatedAt DEFAULT GETDATE(),
        ModifiedBy        BIGINT        NULL,
        ModifiedAt        DATETIME      NULL,
        CONSTRAINT UQ_PriceListDetails_Product UNIQUE (PriceListId, ProductId, UnitId),
        CONSTRAINT CK_PriceListDetails_Price CHECK (Price >= 0),
        CONSTRAINT CK_PriceListDetails_MinQty CHECK (MinimumQuantity > 0),
        CONSTRAINT CK_PriceListDetails_MaxQty CHECK (MaximumQuantity IS NULL OR MaximumQuantity >= MinimumQuantity),
        CONSTRAINT FK_PriceListDetails_PriceList FOREIGN KEY (PriceListId) REFERENCES dbo.PriceLists (PriceListId),
        CONSTRAINT FK_PriceListDetails_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id),
        CONSTRAINT FK_PriceListDetails_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Units (Id)
    );

    CREATE INDEX IX_PriceListDetails_PriceList ON dbo.PriceListDetails (PriceListId);
    CREATE INDEX IX_PriceListDetails_Product ON dbo.PriceListDetails (ProductId);
END
;

/* ---------------------------------------------------------------------------
   DiscountRules (company-scoped pricing rules: percentage or fixed amount).
   target a product, service, and/or a product category (all optional).
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DiscountRules]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.DiscountRules (
        DiscountRuleId     BIGINT IDENTITY(1,1) CONSTRAINT PK_DiscountRules PRIMARY KEY,
        CompanyId          BIGINT       NOT NULL,
        Code               VARCHAR(30)  NOT NULL,
        [Name]             VARCHAR(150) NOT NULL,
        ProductId          BIGINT       NULL,
        ServiceId          BIGINT       NULL,
        ProductCategoryId  BIGINT       NULL,
        PriceListId        BIGINT       NULL,
        DiscountType       VARCHAR(20)  NOT NULL,
        DiscountValue      DECIMAL(18,4) NOT NULL,
        MinimumQuantity    DECIMAL(18,4) NULL,
        MinimumAmount      DECIMAL(18,2) NULL,
        MaximumDiscount    DECIMAL(18,2) NULL,
        EffectiveFrom      DATE         NULL,
        EffectiveTo        DATE         NULL,
        IsActive           BIT          NOT NULL CONSTRAINT DF_DiscountRules_IsActive DEFAULT 1,
        CreatedBy          BIGINT       NULL,
        CreatedAt          DATETIME     NOT NULL CONSTRAINT DF_DiscountRules_CreatedAt DEFAULT GETDATE(),
        ModifiedBy         BIGINT       NULL,
        ModifiedAt         DATETIME     NULL,
        CONSTRAINT UQ_DiscountRules_Company_Code UNIQUE (CompanyId, Code),
        CONSTRAINT CK_DiscountRules_Type CHECK (DiscountType IN ('PERCENTAGE', 'AMOUNT')),
        CONSTRAINT CK_DiscountRules_Value CHECK (DiscountValue >= 0),
        CONSTRAINT FK_DiscountRules_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id),
        CONSTRAINT FK_DiscountRules_Service FOREIGN KEY (ServiceId) REFERENCES dbo.Services (ServiceId),
        CONSTRAINT FK_DiscountRules_Category FOREIGN KEY (ProductCategoryId) REFERENCES dbo.ProductCategories (Id),
        CONSTRAINT FK_DiscountRules_PriceList FOREIGN KEY (PriceListId) REFERENCES dbo.PriceLists (PriceListId)
    );

    CREATE INDEX IX_DiscountRules_Company ON dbo.DiscountRules (CompanyId);
    CREATE INDEX IX_DiscountRules_Product ON dbo.DiscountRules (ProductId);
    CREATE INDEX IX_DiscountRules_PriceList ON dbo.DiscountRules (PriceListId);
END
;

/* ---------------------------------------------------------------------------
   Offers (company-scoped promotions).
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Offers]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Offers (
        OfferId          BIGINT IDENTITY(1,1) CONSTRAINT PK_Offers PRIMARY KEY,
        CompanyId        BIGINT       NOT NULL,
        Code             VARCHAR(30)  NOT NULL,
        [Name]           VARCHAR(150) NOT NULL,
        OfferType        VARCHAR(30)  NOT NULL,
        DiscountType     VARCHAR(20)  NULL,
        DiscountValue    DECIMAL(18,4) NULL,
        MinimumQuantity  DECIMAL(18,4) NULL,
        MinimumAmount    DECIMAL(18,2) NULL,
        MaximumDiscount  DECIMAL(18,2) NULL,
        StartDate        DATE         NOT NULL,
        EndDate          DATE         NULL,
        IsActive         BIT          NOT NULL CONSTRAINT DF_Offers_IsActive DEFAULT 1,
        Description      VARCHAR(500) NULL,
        CreatedBy        BIGINT       NULL,
        CreatedAt        DATETIME     NOT NULL CONSTRAINT DF_Offers_CreatedAt DEFAULT GETDATE(),
        ModifiedBy       BIGINT       NULL,
        ModifiedAt       DATETIME     NULL,
        CONSTRAINT UQ_Offers_Company_Code UNIQUE (CompanyId, Code),
        CONSTRAINT CK_Offers_Dates CHECK (EndDate IS NULL OR EndDate >= StartDate),
        CONSTRAINT CK_Offers_DiscountType CHECK (DiscountType IS NULL OR DiscountType IN ('PERCENTAGE', 'AMOUNT'))
    );

    CREATE INDEX IX_Offers_Company ON dbo.Offers (CompanyId);
END
;

/* ---------------------------------------------------------------------------
   OfferDetails (child of Offer). Targets a product, service, or product category.
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OfferDetails]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.OfferDetails (
        OfferDetailId     BIGINT IDENTITY(1,1) CONSTRAINT PK_OfferDetails PRIMARY KEY,
        OfferId           BIGINT        NOT NULL,
        ProductId         BIGINT        NULL,
        ServiceId         BIGINT        NULL,
        ProductCategoryId BIGINT        NULL,
        MinimumQuantity   DECIMAL(18,4) NULL,
        FreeQuantity      DECIMAL(18,4) NULL,
        IsActive          BIT           NOT NULL CONSTRAINT DF_OfferDetails_IsActive DEFAULT 1,
        CreatedBy         BIGINT        NULL,
        CreatedAt         DATETIME      NOT NULL CONSTRAINT DF_OfferDetails_CreatedAt DEFAULT GETDATE(),
        ModifiedBy        BIGINT        NULL,
        ModifiedAt        DATETIME      NULL,
        CONSTRAINT CK_OfferDetails_Item CHECK (
            ProductId IS NOT NULL OR ServiceId IS NOT NULL OR ProductCategoryId IS NOT NULL),
        CONSTRAINT FK_OfferDetails_Offer FOREIGN KEY (OfferId) REFERENCES dbo.Offers (OfferId),
        CONSTRAINT FK_OfferDetails_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id),
        CONSTRAINT FK_OfferDetails_Service FOREIGN KEY (ServiceId) REFERENCES dbo.Services (ServiceId),
        CONSTRAINT FK_OfferDetails_Category FOREIGN KEY (ProductCategoryId) REFERENCES dbo.ProductCategories (Id)
    );

    CREATE INDEX IX_OfferDetails_Offer ON dbo.OfferDetails (OfferId);
    CREATE INDEX IX_OfferDetails_Product ON dbo.OfferDetails (ProductId);
    CREATE INDEX IX_OfferDetails_Service ON dbo.OfferDetails (ServiceId);
    CREATE INDEX IX_OfferDetails_Category ON dbo.OfferDetails (ProductCategoryId);
END
;

/* ---------------------------------------------------------------------------
   Coupons (company-scoped, linked to an offer).
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Coupons]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Coupons (
        CouponId          BIGINT IDENTITY(1,1) CONSTRAINT PK_Coupons PRIMARY KEY,
        CompanyId         BIGINT       NOT NULL,
        OfferId           BIGINT       NOT NULL,
        Code              VARCHAR(50)  NOT NULL,
        [Name]            VARCHAR(150) NULL,
        UsageLimit        INT          NULL,
        UsagePerCustomer  INT          NULL,
        UsedCount         INT          NOT NULL CONSTRAINT DF_Coupons_UsedCount DEFAULT 0,
        StartDate         DATE         NOT NULL,
        EndDate           DATE         NULL,
        IsActive          BIT          NOT NULL CONSTRAINT DF_Coupons_IsActive DEFAULT 1,
        CreatedBy         BIGINT       NULL,
        CreatedAt         DATETIME     NOT NULL CONSTRAINT DF_Coupons_CreatedAt DEFAULT GETDATE(),
        ModifiedBy        BIGINT       NULL,
        ModifiedAt        DATETIME     NULL,
        CONSTRAINT UQ_Coupons_Company_Code UNIQUE (CompanyId, Code),
        CONSTRAINT CK_Coupons_Dates CHECK (EndDate IS NULL OR EndDate >= StartDate),
        CONSTRAINT CK_Coupons_Usage CHECK (UsageLimit IS NULL OR UsageLimit >= 0),
        CONSTRAINT FK_Coupons_Offer FOREIGN KEY (OfferId) REFERENCES dbo.Offers (OfferId)
    );

    CREATE INDEX IX_Coupons_Company ON dbo.Coupons (CompanyId);
    CREATE INDEX IX_Coupons_Offer ON dbo.Coupons (OfferId);
END
;

/* =============================================================================
   Import Logs (merged from import_logs.sql)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ImportLogs]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.ImportLogs (
        Id BIGINT IDENTITY(1,1) CONSTRAINT PK_ImportLogs PRIMARY KEY,
        CompanyId BIGINT NOT NULL,
        BranchId BIGINT NULL,
        ImportType VARCHAR(30) NOT NULL DEFAULT 'MASTER',
        ModuleName VARCHAR(50) NOT NULL,
        EntityName VARCHAR(100) NOT NULL,
        FileName VARCHAR(255) NOT NULL,
        FileType VARCHAR(20) NOT NULL,
        TotalRows INT NOT NULL DEFAULT 0,
        SuccessRows INT NOT NULL DEFAULT 0,
        FailedRows INT NOT NULL DEFAULT 0,
        Status VARCHAR(30) NOT NULL,
        ErrorMessage VARCHAR(2000) NULL,
        ImportedBy BIGINT NOT NULL,
        ImportedAt DATETIME NOT NULL DEFAULT GETDATE()
    );

    CREATE INDEX IX_ImportLogs_Company_ImportedAt ON dbo.ImportLogs (CompanyId, ImportedAt DESC);
END
;

PRINT 'ERP full schema complete: product masters, taxes, import logs.';

/* =============================================================================
   Sales Module (merged from sql/sales_invoice.sql)
   Unified design: one SalesInvoice table serves POS and normal sales via SourceType.
   ============================================================================= */

/* ---------------------------------------------------------------------------
   SalesInvoice
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalesInvoice]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.SalesInvoice (
        SalesInvoiceId        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalesInvoice PRIMARY KEY,
        SalesInvoiceNo        NVARCHAR(30)         NOT NULL,
        InvoiceDate           DATETIME2            NOT NULL,
        SourceType            NVARCHAR(20)         NOT NULL CONSTRAINT DF_SalesInvoice_SourceType DEFAULT 'SALES',

        CompanyId             BIGINT               NOT NULL,
        CompanyNameSnapshot   NVARCHAR(200)        NULL,
        BranchId              BIGINT               NOT NULL,
        WarehouseId           BIGINT               NOT NULL,
        CustomerId            BIGINT               NOT NULL,
        CustomerNameSnapshot  NVARCHAR(200)        NULL,

        SalesTypeId           INT                  NULL,
        PriceListId           BIGINT               NULL,

        ReferenceNo           NVARCHAR(50)         NULL,
        ReferenceDate         DATE                 NULL,

        TotalGrossAmount      DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_Gross DEFAULT 0,
        TotalDiscountAmount   DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_Disc DEFAULT 0,
        TotalTaxableAmount    DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_Taxable DEFAULT 0,
        TotalCGSTAmount       DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_CGST DEFAULT 0,
        TotalSGSTAmount       DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_SGST DEFAULT 0,
        TotalIGSTAmount       DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_IGST DEFAULT 0,
        TotalCESSAmount       DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_CESS DEFAULT 0,
        TotalRoundOff         DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_RoundOff DEFAULT 0,
        GrandTotal            DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_Grand DEFAULT 0,
        PaidAmount            DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_Paid DEFAULT 0,
        BalanceAmount         DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_Balance DEFAULT 0,

        PaymentTypeID         BIGINT               NULL,
        PaymentMethodID       BIGINT               NULL,
        StatusID              BIGINT               NOT NULL CONSTRAINT DF_SalesInvoice_Status DEFAULT 1,
        InvoiceStatus         NVARCHAR(20)         NOT NULL CONSTRAINT DF_SalesInvoice_InvStatus DEFAULT 'POSTED',

        Remarks               NVARCHAR(500)        NULL,

        IsActive              BIT                  NOT NULL CONSTRAINT DF_SalesInvoice_IsActive DEFAULT 1,
        CreatedByUserID       BIGINT               NOT NULL,
        CreatedAt             DATETIME2            NOT NULL CONSTRAINT DF_SalesInvoice_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedByUserID       BIGINT               NULL,
        UpdatedAt             DATETIME2            NULL,

        CONSTRAINT UQ_SalesInvoice_No UNIQUE (CompanyId, SalesInvoiceNo)
    );

    CREATE INDEX IX_SalesInvoice_Company_Date ON dbo.SalesInvoice (CompanyId, InvoiceDate);
    CREATE INDEX IX_SalesInvoice_CustomerId ON dbo.SalesInvoice (CustomerId);
END
;

/* ---------------------------------------------------------------------------
   SalesInvoiceItem
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalesInvoiceItem]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.SalesInvoiceItem (
        SalesInvoiceItemId    BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalesInvoiceItem PRIMARY KEY,
        SalesInvoiceId        BIGINT               NOT NULL,

        ProductId             BIGINT               NOT NULL,
        ProductCodeSnapshot   NVARCHAR(100)        NULL,
        ProductNameSnapshot   NVARCHAR(200)        NULL,
        UnitID                BIGINT               NOT NULL,
        UnitNameSnapshot      NVARCHAR(100)        NULL,
        BatchId               BIGINT               NULL,
        HSNID                 BIGINT               NULL,
        HSNCodeSnapshot       NVARCHAR(100)        NULL,
        BarcodeSnapshot       NVARCHAR(100)        NULL,

        Quantity              DECIMAL(18,3)        NOT NULL,
        FreeQuantity          DECIMAL(18,3)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_Free DEFAULT 0,
        Rate                  DECIMAL(18,4)        NOT NULL,
        GrossAmount           DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_Gross DEFAULT 0,

        DiscountPercentage    DECIMAL(8,3)         NOT NULL CONSTRAINT DF_SalesInvoiceItem_DiscPct DEFAULT 0,
        DiscountAmount        DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_Disc DEFAULT 0,

        TaxableAmount         DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_Taxable DEFAULT 0,

        GSTPercent            DECIMAL(8,3)         NOT NULL CONSTRAINT DF_SalesInvoiceItem_GSTPct DEFAULT 0,
        CGSTPercent           DECIMAL(8,3)         NOT NULL CONSTRAINT DF_SalesInvoiceItem_CGSTPct DEFAULT 0,
        SGSTPercent           DECIMAL(8,3)         NOT NULL CONSTRAINT DF_SalesInvoiceItem_SGSTPct DEFAULT 0,
        IGSTPercent           DECIMAL(8,3)         NOT NULL CONSTRAINT DF_SalesInvoiceItem_IGSTPct DEFAULT 0,
        CESSPercent           DECIMAL(8,3)         NOT NULL CONSTRAINT DF_SalesInvoiceItem_CESSPct DEFAULT 0,

        CGSTAmount            DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_CGSTAmt DEFAULT 0,
        SGSTAmount            DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_SGSTAmt DEFAULT 0,
        IGSTAmount            DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_IGSTAmt DEFAULT 0,
        CESSAmount            DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_CESSAmt DEFAULT 0,

        LineTotal             DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_Line DEFAULT 0,
        Remarks               NVARCHAR(500)        NULL
    );

    CREATE INDEX IX_SalesInvoiceItem_InvoiceId ON dbo.SalesInvoiceItem (SalesInvoiceId);
    CREATE INDEX IX_SalesInvoiceItem_ProductId ON dbo.SalesInvoiceItem (ProductId);
END
;

PRINT 'ERP full schema complete: sales module added.';

/* =============================================================================
   Payment Masters (PaymentType, PaymentMethod, Payment, PaymentAllocation)
   ============================================================================= */

/* ---------------------------------------------------------------------------
   PaymentType
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentType]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PaymentType (
        PaymentTypeId BIGINT IDENTITY(1,1) CONSTRAINT PK_PaymentType PRIMARY KEY,
        Code          VARCHAR(30)  NOT NULL,
        [Name]        NVARCHAR(100) NOT NULL,
        IsActive      BIT          NOT NULL CONSTRAINT DF_PaymentType_IsActive DEFAULT 1,
        DisplayOrder  INT          NOT NULL CONSTRAINT DF_PaymentType_DisplayOrder DEFAULT 0,
        CreatedAt     DATETIME2    NOT NULL CONSTRAINT DF_PaymentType_CreatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_PaymentType_Code ON dbo.PaymentType (Code);
END
;

INSERT INTO dbo.PaymentType (Code, [Name], IsActive, DisplayOrder)
SELECT k.Code, k.[Name], k.IsActive, k.DisplayOrder
FROM (VALUES
    ('PAYTYPE-001', 'Cash',        1, 1),
    ('PAYTYPE-002', 'Credit',      1, 2),
    ('PAYTYPE-003', 'Advance',     1, 3),
    ('PAYTYPE-004', 'Refund',      1, 4),
    ('PAYTYPE-005', 'Settlement',  1, 5)
) AS k(Code, [Name], IsActive, DisplayOrder)
WHERE NOT EXISTS (SELECT 1 FROM dbo.PaymentType p WHERE p.Code = k.Code);
;

/* ---------------------------------------------------------------------------
   PaymentMethod
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentMethod]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PaymentMethod (
        PaymentMethodId     BIGINT IDENTITY(1,1) CONSTRAINT PK_PaymentMethod PRIMARY KEY,
        Code                VARCHAR(30)   NOT NULL,
        [Name]              NVARCHAR(100) NOT NULL,
        PaymentCategory     VARCHAR(50)   NULL,
        IsCash              BIT           NOT NULL CONSTRAINT DF_PaymentMethod_IsCash DEFAULT 0,
        IsCredit            BIT           NOT NULL CONSTRAINT DF_PaymentMethod_IsCredit DEFAULT 0,
        RequiresReferenceNo BIT           NOT NULL CONSTRAINT DF_PaymentMethod_ReqRef DEFAULT 0,
        DisplayOrder        INT           NOT NULL CONSTRAINT DF_PaymentMethod_DisplayOrder DEFAULT 0,
        IsActive            BIT           NOT NULL CONSTRAINT DF_PaymentMethod_IsActive DEFAULT 1,
        CreatedAt           DATETIME2     NOT NULL CONSTRAINT DF_PaymentMethod_CreatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_PaymentMethod_Code ON dbo.PaymentMethod (Code);
END
;

INSERT INTO dbo.PaymentMethod
    (Code, [Name], PaymentCategory, IsCash, IsCredit, RequiresReferenceNo, DisplayOrder, IsActive)
SELECT k.Code, k.[Name], k.PaymentCategory, k.IsCash, k.IsCredit, k.RequiresReferenceNo, k.DisplayOrder, k.IsActive
FROM (VALUES
    ('PAYMETHOD-001', 'Cash',           'CASH',    1, 0, 0, 1,  1),
    ('PAYMETHOD-002', 'UPI',            'DIGITAL', 0, 0, 1, 2,  1),
    ('PAYMETHOD-003', 'Credit Card',    'CARD',    0, 0, 1, 3,  1),
    ('PAYMETHOD-004', 'Debit Card',     'CARD',    0, 0, 1, 4,  1),
    ('PAYMETHOD-005', 'Bank Transfer',  'BANK',    0, 0, 1, 5,  1),
    ('PAYMETHOD-006', 'Cheque',         'BANK',    0, 0, 1, 6,  1),
    ('PAYMETHOD-007', 'Credit Account', 'CREDIT',  0, 1, 0, 7,  1),
    ('PAYMETHOD-008', 'Wallet',         'DIGITAL', 0, 0, 1, 8,  1),
    ('PAYMETHOD-009', 'Other',          'OTHER',   0, 0, 0, 99, 1)
) AS k(Code, [Name], PaymentCategory, IsCash, IsCredit, RequiresReferenceNo, DisplayOrder, IsActive)
WHERE NOT EXISTS (SELECT 1 FROM dbo.PaymentMethod p WHERE p.Code = k.Code);
;

/* ---------------------------------------------------------------------------
   Payment
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Payment]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Payment (
        PaymentId         BIGINT IDENTITY(1,1) CONSTRAINT PK_Payment PRIMARY KEY,
        CompanyId         BIGINT        NOT NULL,
        PaymentNo         NVARCHAR(30)  NOT NULL,
        PaymentDate       DATETIME2     NOT NULL,
        PaymentTypeID     BIGINT        NULL,
        PaymentMethodID   BIGINT        NULL,
        ReferenceType     NVARCHAR(30)  NOT NULL,
        ReferenceId       BIGINT        NOT NULL,
        BusinessPartnerId BIGINT        NULL,
        Amount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_Payment_Amount DEFAULT 0,
        ReferenceNo       NVARCHAR(50)  NULL,
        Remarks           NVARCHAR(500) NULL,
        StatusID          BIGINT        NOT NULL CONSTRAINT DF_Payment_Status DEFAULT 1,
        CreatedByUserID   BIGINT        NOT NULL,
        CreatedAt         DATETIME2     NOT NULL CONSTRAINT DF_Payment_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedByUserID   BIGINT        NULL,
        UpdatedAt         DATETIME2     NULL,

        CONSTRAINT UQ_Payment_No UNIQUE (CompanyId, PaymentNo)
    );

    CREATE INDEX IX_Payment_Company_Date ON dbo.Payment (CompanyId, PaymentDate);
    CREATE INDEX IX_Payment_Reference ON dbo.Payment (ReferenceType, ReferenceId);
END
;

/* ---------------------------------------------------------------------------
   PaymentAllocation
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentAllocation]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PaymentAllocation (
        PaymentAllocationId BIGINT IDENTITY(1,1) CONSTRAINT PK_PaymentAllocation PRIMARY KEY,
        PaymentId           BIGINT        NOT NULL,
        ReferenceType       NVARCHAR(30)  NOT NULL,
        ReferenceId         BIGINT        NOT NULL,
        AllocatedAmount     DECIMAL(18,2) NOT NULL CONSTRAINT DF_PaymentAllocation_Amt DEFAULT 0,
        CreatedAt           DATETIME2     NOT NULL CONSTRAINT DF_PaymentAllocation_CreatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_PaymentAllocation_Ref ON dbo.PaymentAllocation (ReferenceType, ReferenceId);
    CREATE INDEX IX_PaymentAllocation_Payment ON dbo.PaymentAllocation (PaymentId);
END
;

/* =============================================================================
   Purchase Module (Purchase, PurchaseItem)
   Snapshot design: PurchaseNumber/CompanyNameSnapshot/BranchNameSnapshot/
   SupplierNameSnapshot; PurchaseItem uses BrandID/CategoryID snapshots.
   ============================================================================= */

/* ---------------------------------------------------------------------------
   Purchase
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Purchase]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Purchase (
        PurchaseId            BIGINT IDENTITY(1,1) CONSTRAINT PK_Purchase PRIMARY KEY,
        CompanyId             BIGINT        NOT NULL,
        CompanyNameSnapshot   NVARCHAR(200) NULL,
        BranchId              BIGINT        NOT NULL,
        BranchNameSnapshot    NVARCHAR(200) NULL,
        WarehouseId           BIGINT        NOT NULL,
        SupplierId            BIGINT        NOT NULL,
        SupplierNameSnapshot  NVARCHAR(200) NULL,
        PurchaseNumber        NVARCHAR(30)  NOT NULL,
        PurchaseDate          DATETIME2     NOT NULL,
        SupplierInvoiceNumber NVARCHAR(50)  NULL,
        SupplierInvoiceDate   DATE          NULL,
        TotalGrossAmount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_Gross DEFAULT 0,
        TotalDiscountAmount   DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_Disc DEFAULT 0,
        TotalTaxableAmount    DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_Taxable DEFAULT 0,
        TotalTaxAmount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_Tax DEFAULT 0,
        TotalCessAmount       DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_CESS DEFAULT 0,
        TotalRoundOff         DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_RoundOff DEFAULT 0,
        GrandTotal            DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_Grand DEFAULT 0,
        PaidAmount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_Paid DEFAULT 0,
        BalanceAmount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_Balance DEFAULT 0,
        PaymentTypeID         BIGINT        NULL,
        PaymentMethodID       BIGINT        NULL,
        StatusID              BIGINT        NOT NULL CONSTRAINT DF_Purchase_Status DEFAULT 1,
        Remarks               NVARCHAR(500) NULL,
        IsActive              BIT           NOT NULL CONSTRAINT DF_Purchase_IsActive DEFAULT 1,
        CreatedByUserID       BIGINT        NOT NULL,
        CreatedAt             DATETIME2     NOT NULL CONSTRAINT DF_Purchase_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedByUserID       BIGINT        NULL,
        UpdatedAt             DATETIME2     NULL,
        SupplierPONumber      NVARCHAR(50)  NULL,
        ReferenceNumber       NVARCHAR(50)  NULL,
        CurrencyId            BIGINT        NULL,
        PurchaseTypeId        BIGINT        NULL,
        AccountingYearId      BIGINT        NULL,
        TaxId                 BIGINT        NULL,
        IsGSTInclusive        BIT           NULL,
        CancelledByUserID     BIGINT        NULL,
        CancelledAt           DATETIME2     NULL,
        CancellationReason    NVARCHAR(500) NULL,

        CONSTRAINT UQ_Purchase_No UNIQUE (CompanyId, PurchaseNumber),
        CONSTRAINT FK_Purchase_Taxes FOREIGN KEY (TaxId) REFERENCES dbo.Taxes(Id),
        CONSTRAINT FK_Purchase_FinancialYear FOREIGN KEY (AccountingYearId) REFERENCES dbo.FinancialYear(FinancialYearId)
    );

    CREATE INDEX IX_Purchase_Company_Date ON dbo.Purchase (CompanyId, PurchaseDate);
    CREATE INDEX IX_Purchase_Supplier ON dbo.Purchase (SupplierId);
    CREATE INDEX IX_Purchase_AccountingYearId ON dbo.Purchase (AccountingYearId);
    CREATE INDEX IX_Purchase_TaxId ON dbo.Purchase (TaxId);
END
;

/* ---------------------------------------------------------------------------
   PurchaseItem
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PurchaseItem]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PurchaseItem (
        PurchaseItemId     BIGINT IDENTITY(1,1) CONSTRAINT PK_PurchaseItem PRIMARY KEY,
        PurchaseId         BIGINT        NOT NULL,
        ProductId          BIGINT        NOT NULL,
        ProductCodeSnapshot NVARCHAR(100) NULL,
        ProductNameSnapshot NVARCHAR(200) NULL,
        BrandID            BIGINT        NULL,
        CategoryID         BIGINT        NULL,
        SubCategoryID      BIGINT        NULL,
        UnitID             BIGINT        NOT NULL,
        UnitNameSnapshot   NVARCHAR(100) NULL,
        HSNID              BIGINT        NULL,
        HSNCodeSnapshot    NVARCHAR(100) NULL,
        BarcodeSnapshot    NVARCHAR(100) NULL,
        Quantity           DECIMAL(18,3) NOT NULL CONSTRAINT DF_PurchaseItem_Qty DEFAULT 0,
        FreeQuantity       DECIMAL(18,3) NOT NULL CONSTRAINT DF_PurchaseItem_Free DEFAULT 0,
        PurchaseRate       DECIMAL(18,4) NOT NULL CONSTRAINT DF_PurchaseItem_Rate DEFAULT 0,
        MRP                DECIMAL(18,2) NULL,
        RetailPrice        DECIMAL(18,2) NULL,
        WholesalePrice     DECIMAL(18,2) NULL,
        SaleRate           DECIMAL(18,4) NULL,
        DiscountPercentage DECIMAL(8,3)  NOT NULL CONSTRAINT DF_PurchaseItem_DiscPct DEFAULT 0,
        DiscountAmount     DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItem_Disc DEFAULT 0,
        IsGSTInclusive     BIT           NOT NULL CONSTRAINT DF_PurchaseItem_GSTInc DEFAULT 0,
        TaxableValue       DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItem_Taxable DEFAULT 0,
        GSTRate            DECIMAL(8,3)  NOT NULL CONSTRAINT DF_PurchaseItem_GST DEFAULT 0,
        GSTAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItem_GSTAmt DEFAULT 0,
        CGSTRate           DECIMAL(8,3)  NOT NULL CONSTRAINT DF_PurchaseItem_CGSTR DEFAULT 0,
        CGSTAmount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItem_CGSTA DEFAULT 0,
        SGSTRate           DECIMAL(8,3)  NOT NULL CONSTRAINT DF_PurchaseItem_SGSTR DEFAULT 0,
        SGSTAmount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItem_SGSTA DEFAULT 0,
        IGSTRate           DECIMAL(8,3)  NOT NULL CONSTRAINT DF_PurchaseItem_IGSTR DEFAULT 0,
        IGSTAmount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItem_IGSTA DEFAULT 0,
        CESSRate           DECIMAL(8,3)  NOT NULL CONSTRAINT DF_PurchaseItem_CESSR DEFAULT 0,
        CESSAmount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItem_CESSA DEFAULT 0,
        LineTotal          DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItem_Line DEFAULT 0,
        ManufacturingDate  DATE          NULL,
        ExpiryDate         DATE          NULL,
        Remarks            NVARCHAR(500) NULL,
        TaxId              BIGINT        NULL,
        CessId             BIGINT        NULL,
        OrderedQuantity    DECIMAL(18,3) NULL,
        ReceivedQuantity   DECIMAL(18,3) NULL,
        ReturnedQuantity   DECIMAL(18,3) NULL,
        RemainingQuantity  DECIMAL(18,3) NULL,
        PurchaseOrderId    BIGINT        NULL,
        PurchaseOrderItemId BIGINT       NULL,
        GRNId              BIGINT        NULL,
        BatchNumber        NVARCHAR(100) NULL,
        SerialNumber       NVARCHAR(100) NULL,

        CONSTRAINT FK_PurchaseItem_Taxes FOREIGN KEY (TaxId) REFERENCES dbo.Taxes(Id)
    );

    CREATE INDEX IX_PurchaseItem_PurchaseId ON dbo.PurchaseItem (PurchaseId);
    CREATE INDEX IX_PurchaseItem_ProductId ON dbo.PurchaseItem (ProductId);
    CREATE INDEX IX_PurchaseItem_TaxId ON dbo.PurchaseItem (TaxId);
    CREATE INDEX IX_PurchaseItem_PurchaseOrderId ON dbo.PurchaseItem (PurchaseOrderId);
    CREATE INDEX IX_PurchaseItem_GRNId ON dbo.PurchaseItem (GRNId);
END
;

/* =============================================================================
   Purchase Return Module (PurchaseReturn, PurchaseReturnItem)
   ============================================================================= */

/* ---------------------------------------------------------------------------
   PurchaseReturn
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PurchaseReturn]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PurchaseReturn (
        PurchaseReturnId      BIGINT IDENTITY(1,1) CONSTRAINT PK_PurchaseReturn PRIMARY KEY,
        PurchaseId            BIGINT        NOT NULL,
        CompanyId             BIGINT        NOT NULL,
        CompanyNameSnapshot   NVARCHAR(200) NULL,
        BranchId              BIGINT        NOT NULL,
        BranchNameSnapshot    NVARCHAR(200) NULL,
        WarehouseId           BIGINT        NOT NULL,
        SupplierId            BIGINT        NOT NULL,
        SupplierNameSnapshot  NVARCHAR(200) NULL,
        ReturnNumber          NVARCHAR(30)  NOT NULL,
        ReturnDate            DATETIME2     NOT NULL,
        TotalGrossAmount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturn_Gross DEFAULT 0,
        TotalDiscountAmount   DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturn_Disc DEFAULT 0,
        TotalTaxableAmount    DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturn_Taxable DEFAULT 0,
        TotalTaxAmount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturn_Tax DEFAULT 0,
        TotalCessAmount       DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturn_CESS DEFAULT 0,
        TotalRoundOff         DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturn_RoundOff DEFAULT 0,
        GrandTotal            DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturn_Grand DEFAULT 0,
        StatusID              BIGINT        NOT NULL CONSTRAINT DF_PurchaseReturn_Status DEFAULT 1,
        Reason                NVARCHAR(500) NULL,
        Remarks               NVARCHAR(500) NULL,
        CreatedByUserID       BIGINT        NOT NULL,
        CreatedAt             DATETIME2     NOT NULL CONSTRAINT DF_PurchaseReturn_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedByUserID       BIGINT        NULL,
        UpdatedAt             DATETIME2     NULL,
        SupplierInvoiceNumber NVARCHAR(50)  NULL,
        ReferenceNumber       NVARCHAR(50)  NULL,
        CurrencyId            BIGINT        NULL,
        PurchaseReturnTypeId  BIGINT        NULL,
        AccountingYearId      BIGINT        NULL,
        PaymentTypeId         BIGINT        NULL,
        PaymentMethodId       BIGINT        NULL,
        ReturnReasonId        BIGINT        NULL,
        CancelledByUserID     BIGINT        NULL,
        CancelledAt           DATETIME2     NULL,
        CancellationReason    NVARCHAR(500) NULL,

        CONSTRAINT UQ_PurchaseReturn_No UNIQUE (CompanyId, ReturnNumber),
        CONSTRAINT FK_PurchaseReturn_FinancialYear FOREIGN KEY (AccountingYearId) REFERENCES dbo.FinancialYear(FinancialYearId),
        CONSTRAINT FK_PurchaseReturn_PaymentType FOREIGN KEY (PaymentTypeId) REFERENCES dbo.PaymentType(PaymentTypeId),
        CONSTRAINT FK_PurchaseReturn_PaymentMethod FOREIGN KEY (PaymentMethodId) REFERENCES dbo.PaymentMethod(PaymentMethodId)
    );

    CREATE INDEX IX_PurchaseReturn_Company_Date ON dbo.PurchaseReturn (CompanyId, ReturnDate);
    CREATE INDEX IX_PurchaseReturn_AccountingYearId ON dbo.PurchaseReturn (AccountingYearId);
    CREATE INDEX IX_PurchaseReturn_PaymentTypeId ON dbo.PurchaseReturn (PaymentTypeId);
    CREATE INDEX IX_PurchaseReturn_PaymentMethodId ON dbo.PurchaseReturn (PaymentMethodId);
END
;

/* ---------------------------------------------------------------------------
   PurchaseReturnItem
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PurchaseReturnItem]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PurchaseReturnItem (
        PurchaseReturnItemId BIGINT IDENTITY(1,1) CONSTRAINT PK_PurchaseReturnItem PRIMARY KEY,
        PurchaseReturnId     BIGINT        NOT NULL,
        PurchaseItemId       BIGINT        NOT NULL,
        ProductId            BIGINT        NOT NULL,
        ProductCodeSnapshot  NVARCHAR(100) NULL,
        ProductNameSnapshot  NVARCHAR(200) NULL,
        UnitId               BIGINT        NOT NULL,
        UnitNameSnapshot     NVARCHAR(100) NULL,
        HSNId                BIGINT        NULL,
        HSNCodeSnapshot      NVARCHAR(100) NULL,
        ReturnQuantity       DECIMAL(18,3) NOT NULL CONSTRAINT DF_PurchaseReturnItem_Qty DEFAULT 0,
        PurchaseRate         DECIMAL(18,4) NOT NULL CONSTRAINT DF_PurchaseReturnItem_Rate DEFAULT 0,
        DiscountAmount       DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturnItem_Disc DEFAULT 0,
        TaxableValue         DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturnItem_Taxable DEFAULT 0,
        GSTRate              DECIMAL(8,3)  NOT NULL CONSTRAINT DF_PurchaseReturnItem_GST DEFAULT 0,
        GSTAmount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturnItem_GSTAmt DEFAULT 0,
        CGSTRate             DECIMAL(8,3)  NOT NULL CONSTRAINT DF_PurchaseReturnItem_CGSTR DEFAULT 0,
        CGSTAmount           DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturnItem_CGSTA DEFAULT 0,
        SGSTRate             DECIMAL(8,3)  NOT NULL CONSTRAINT DF_PurchaseReturnItem_SGSTR DEFAULT 0,
        SGSTAmount           DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturnItem_SGSTA DEFAULT 0,
        IGSTRate             DECIMAL(8,3)  NOT NULL CONSTRAINT DF_PurchaseReturnItem_IGSTR DEFAULT 0,
        IGSTAmount           DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturnItem_IGSTA DEFAULT 0,
        CESSRate             DECIMAL(8,3)  NOT NULL CONSTRAINT DF_PurchaseReturnItem_CESSR DEFAULT 0,
        CESSAmount           DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturnItem_CESSA DEFAULT 0,
        LineTotal            DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturnItem_Line DEFAULT 0,
        TaxId                BIGINT        NULL,
        CessId               BIGINT        NULL,
        BarcodeSnapshot      NVARCHAR(100) NULL,
        BatchNumber          NVARCHAR(100) NULL,
        SerialNumber         NVARCHAR(100) NULL,
        ReasonId             BIGINT        NULL,

        CONSTRAINT FK_PurchaseReturnItem_Taxes FOREIGN KEY (TaxId) REFERENCES dbo.Taxes(Id)
    );

    CREATE INDEX IX_PurchaseReturnItem_ReturnId ON dbo.PurchaseReturnItem (PurchaseReturnId);
    CREATE INDEX IX_PurchaseReturnItem_ProductId ON dbo.PurchaseReturnItem (ProductId);
    CREATE INDEX IX_PurchaseReturnItem_TaxId ON dbo.PurchaseReturnItem (TaxId);
END
;

/* =============================================================================
   Stock Module (Stock, StockTransaction)
   ============================================================================= */

/* ---------------------------------------------------------------------------
   Stock
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Stock]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Stock (
        StockId           BIGINT IDENTITY(1,1) CONSTRAINT PK_Stock PRIMARY KEY,
        CompanyId         BIGINT        NOT NULL,
        BranchId          BIGINT        NOT NULL,
        WarehouseId       BIGINT        NOT NULL,
        ProductId         BIGINT        NOT NULL,
        UnitId            BIGINT        NOT NULL,
        Quantity          DECIMAL(18,3) NOT NULL CONSTRAINT DF_Stock_Qty DEFAULT 0,
        ReservedQuantity  DECIMAL(18,3) NOT NULL CONSTRAINT DF_Stock_Reserved DEFAULT 0,
        AvailableQuantity DECIMAL(18,3) NOT NULL CONSTRAINT DF_Stock_Available DEFAULT 0,
        AverageCost       DECIMAL(18,4) NOT NULL CONSTRAINT DF_Stock_AvgCost DEFAULT 0,
        LastPurchaseRate  DECIMAL(18,4) NOT NULL CONSTRAINT DF_Stock_LastRate DEFAULT 0,
        UpdatedAt         DATETIME2     NOT NULL CONSTRAINT DF_Stock_UpdatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UQ_Stock_Key UNIQUE (CompanyId, BranchId, WarehouseId, ProductId, UnitId)
    );

    CREATE INDEX IX_Stock_Company_Product ON dbo.Stock (CompanyId, ProductId);
    CREATE INDEX IX_Stock_Warehouse ON dbo.Stock (WarehouseId);
END
;

/* ---------------------------------------------------------------------------
   StockTransaction
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StockTransaction]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.StockTransaction (
        StockTransactionId BIGINT IDENTITY(1,1) CONSTRAINT PK_StockTransaction PRIMARY KEY,
        CompanyId          BIGINT        NOT NULL,
        BranchId           BIGINT        NOT NULL,
        WarehouseId        BIGINT        NOT NULL,
        ProductId          BIGINT        NOT NULL,
        UnitId             BIGINT        NOT NULL,
        TransactionType    VARCHAR(10)   NOT NULL,
        ReferenceType      VARCHAR(30)   NOT NULL,
        ReferenceId        BIGINT        NOT NULL,
        QuantityIn         DECIMAL(18,3) NOT NULL CONSTRAINT DF_StockTx_QtyIn DEFAULT 0,
        QuantityOut        DECIMAL(18,3) NOT NULL CONSTRAINT DF_StockTx_QtyOut DEFAULT 0,
        Rate               DECIMAL(18,4) NOT NULL CONSTRAINT DF_StockTx_Rate DEFAULT 0,
        BalanceQuantity    DECIMAL(18,3) NOT NULL CONSTRAINT DF_StockTx_Balance DEFAULT 0,
        TransactionDate    DATETIME2     NOT NULL,
        Remarks            NVARCHAR(500) NULL,
        CreatedByUserID    BIGINT        NOT NULL,
        CreatedAt          DATETIME2     NOT NULL CONSTRAINT DF_StockTx_CreatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_StockTx_Company_Product ON dbo.StockTransaction (CompanyId, ProductId);
    CREATE INDEX IX_StockTx_Reference ON dbo.StockTransaction (ReferenceType, ReferenceId);
END
;

/* =============================================================================
   TenantConfiguration (dynamic Purchase / Sale / Billing screen config)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TenantConfiguration]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.TenantConfiguration (
        Id              BIGINT IDENTITY(1,1) CONSTRAINT PK_TenantConfiguration PRIMARY KEY,
        TenantId        BIGINT        NOT NULL,
        ApplicationType NVARCHAR(50)  NOT NULL,
        TransactionType NVARCHAR(50)  NULL,
        FlowType        NVARCHAR(50)  NULL,
        PageCode        NVARCHAR(50)  NULL,
        FieldCode       NVARCHAR(50)  NULL,
        SequenceNo      INT           NULL,
        IsPageEnabled   BIT           NOT NULL CONSTRAINT DF_TC_IsPageEnabled DEFAULT 1,
        IsVisible       BIT           NOT NULL CONSTRAINT DF_TC_IsVisible DEFAULT 1,
        IsRequired      BIT           NOT NULL CONSTRAINT DF_TC_IsRequired DEFAULT 0,
        IsReadonly      BIT           NOT NULL CONSTRAINT DF_TC_IsReadonly DEFAULT 0,
        DisplayOrder    INT           NULL,
        DefaultValue    NVARCHAR(500) NULL,
        IsActive        BIT           NOT NULL CONSTRAINT DF_TC_IsActive DEFAULT 1,
        CreatedBy       BIGINT        NOT NULL,
        CreatedAt       DATETIME2     NOT NULL CONSTRAINT DF_TC_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedBy       BIGINT        NULL,
        UpdatedAt       DATETIME2     NULL
    );

    CREATE INDEX IX_TenantConfiguration_Tenant ON dbo.TenantConfiguration (TenantId, ApplicationType);
END
;

/* =============================================================================
   Status (document lifecycle lookup used by Sales / Purchase / Returns)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Status]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.[Status] (
        StatusId    BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Status PRIMARY KEY,
        Code        NVARCHAR(20)         NOT NULL CONSTRAINT UQ_Status_Code UNIQUE,
        [Name]      NVARCHAR(100)        NOT NULL,
        Module      NVARCHAR(50)         NULL,
        IsActive    BIT                  NOT NULL CONSTRAINT DF_Status_IsActive DEFAULT 1,
        SortOrder   INT                  NOT NULL CONSTRAINT DF_Status_SortOrder DEFAULT 0,
        CreatedBy   NVARCHAR(50)         NULL,
        CreatedAt   DATETIME2            NOT NULL CONSTRAINT DF_Status_CreatedAt DEFAULT SYSUTCDATETIME()
    );

    INSERT INTO dbo.[Status] ([Code], [Name], [Module], IsActive, SortOrder)
    VALUES
        ('DRAFT',            'Draft',             NULL,        1, 10),
        ('POSTED',           'Posted',            NULL,        1, 20),
        ('RETURNED',         'Returned',          'PURCHASE',  1, 30),
        ('PARTIALLY_RETURNED','Partially Returned', 'PURCHASE', 1, 40),
        ('APPROVED',         'Approved',          NULL,        1, 50),
        ('REJECTED',         'Rejected',          NULL,        1, 60),
        ('CANCELLED',        'Cancelled',         NULL,        1, 70),
        ('PARTIALLY_PAID',   'Partially Paid',    'PAYMENT',   1, 80),
        ('PAID',             'Paid',              'PAYMENT',   1, 90),
        ('UNPAID',           'Unpaid',            'PAYMENT',   1, 100),
        ('COMPLETED',        'Completed',         NULL,        1, 110),
        ('CLOSED',           'Closed',            NULL,        1, 120),
        ('PENDING',          'Pending',           NULL,        1, 130);
END
;

/* =============================================================================
   Module Configuration Seed Data
   Merged from: seed_purchase_management.sql, product_masters_init.sql,
                product_init.sql, tax_init.sql, taxes_init.sql,
                import_logs.sql, sql/sales_invoice.sql
   Builds Workspaces > Domains > Modules > SubModules > Screens + Role-2 perms.
   Dapper-compatible: no GO, parent IDs resolved via subqueries.
   ============================================================================= */

/* =============================================================================
   SETUP WORKSPACE (Organization / System / Tenant)
   ============================================================================= */

/* --- Organization > Business Master --- */
IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-BUSINESSMASTER')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-BUSINESSMASTER', 'Business Master', 'briefcase', '/business-master', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-ORG';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-ORGSETUP')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-ORGSETUP', 'Organization Setup', 'building-2', '/business-master', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-BUSINESSMASTER';
END
;

INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, k.ScreenCode, k.ScreenName, k.ScreenType, k.RouteUrl, k.ComponentName, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('COMPANY_MASTER',       'Company',         'MASTER', '/company',         'CompanyPage',        2, 1, 'system'),
    ('BRANCH_MASTER',        'Branch',          'MASTER', '/branch',          'BranchPage',         3, 1, 'system'),
    ('DEPARTMENT_MASTER',    'Department',      'MASTER', '/department',      'Department',         4, 1, 'system'),
    ('WAREHOUSE_MASTER',     'Warehouse',       'MASTER', '/warehouse',       'Warehouse',          5, 1, 'system'),
    ('DESIGNATION_MASTER',   'Designation',     'MASTER', '/designation',     'Designation',        6, 1, 'system'),
    ('STORE_MASTER',         'Stores',          'MASTER', '/stores',          'StoresPage',         7, 1, 'system'),
    ('COUNTER_MASTER',       'Counters',        'MASTER', '/counters',        'CountersPage',       8, 1, 'system'),
    ('POS_SESSION_MASTER',   'POS Sessions',    'MASTER', '/pos-sessions',    'PosSessionsPage',    9, 1, 'system')
) AS k(ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
INNER JOIN dbo.SubModules sm ON sm.SubModuleCode = 'SUB-ORGSETUP'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Screens s WHERE s.SubModuleId = sm.Id AND s.ScreenCode = k.ScreenCode
);
;

/* --- Organization > Financial Setup --- */
IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-FINANCE')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-FINANCE', 'Financial Setup', 'landmark', '/finance-year', 2, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-ORG';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-FINANCE')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-FINANCE', 'Finance', 'calculator', '/finance-year', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-FINANCE';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'FINANCE_YEAR')
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'FINANCE_YEAR', 'Financial Year', 'MASTER', '/finance-year', 'FinanceYear', 1, 1, 'system'
    FROM dbo.SubModules WHERE SubModuleCode = 'SUB-FINANCE';
;

/* --- System > System Master --- */
IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-SYSMASTER')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-SYSMASTER', 'System Master', 'server', '/system-master', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-SYSTEM';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-SYSCONFIG')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-SYSCONFIG', 'System Configuration', 'settings', '/system-master', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-SYSMASTER';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'SYSTEM_MASTER_HOME')
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SYSTEM_MASTER_HOME', 'System Master', 'MASTER', '/system-master', 'SystemMasterPage', 1, 1, 'system'
    FROM dbo.SubModules WHERE SubModuleCode = 'SUB-SYSCONFIG';
;

/* --- Tenant > Tenant Configuration --- */
IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-TENANTCONFIG')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-TENANTCONFIG', 'Tenant Configuration', 'building-2', '/tenant-configuration', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-TENANT';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-TENANTCONFIG')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-TENANTCONFIG', 'Configuration', 'sliders-horizontal', '/tenant-configuration', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-TENANTCONFIG';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'TENANT_CONFIGURATION')
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'TENANT_CONFIGURATION', 'Tenant Configuration', 'SETTINGS', '/tenant-configuration', 'TenantConfigurationPage', 1, 1, 'system'
    FROM dbo.SubModules WHERE SubModuleCode = 'SUB-TENANTCONFIG';
;

/* =============================================================================
   PRODBILL WORKSPACE (Product & Billing = Product Management + Billing/Tax)
   ============================================================================= */

/* --- Product > Product Management > Product Master --- */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-PRODUCT')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-PRODUCT', 'Product', 'package', 1, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'PRODBILL';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-PRODMASTERS')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-PRODMASTERS', 'Product Management', 'boxes', '/product', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-PRODUCT';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-PRODSETUP')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-PRODSETUP', 'Product Master', 'package', '/product', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-PRODMASTERS';
END
;

INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, k.ScreenCode, k.ScreenName, k.ScreenType, k.RouteUrl, k.ComponentName, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('PRODUCT-CATEGORIES',     'Product Categories',     'MASTER', '/product-categories',     'CategoryPage',     1, 1, 'system'),
    ('PRODUCT-SUBCATEGORIES',  'Product Sub Categories', 'MASTER', '/product-subcategories',  'SubCategoryPage',  2, 1, 'system'),
    ('BRANDS',                 'Brands',                 'MASTER', '/brands',                 'BrandPage',        3, 1, 'system'),
    ('UNITS',                  'Units',                  'MASTER', '/units',                  'UnitPage',         4, 1, 'system'),
    ('PRODUCTS',               'Products',               'MASTER', '/products',               'ProductPage',      5, 1, 'system'),
    ('IMPORT-LOGS',            'Import Logs',            'LIST',   '/import-logs',            'MasterImportPage', 6, 1, 'system')
) AS k(ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
INNER JOIN dbo.SubModules sm ON sm.SubModuleCode = 'SUB-PRODSETUP'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Screens s WHERE s.SubModuleId = sm.Id AND s.ScreenCode = k.ScreenCode
);
;

/* --- Product > Master Data > Import Management --- */
IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-MASTERDATA')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-MASTERDATA', 'Master Data', 'database', '/master-import', 2, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-PRODUCT';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-IMPORTMGMT')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-IMPORTMGMT', 'Import Management', 'upload', '/master-import', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-MASTERDATA';
END
;

INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, k.ScreenCode, k.ScreenName, k.ScreenType, k.RouteUrl, k.ComponentName, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('MASTER_IMPORT', 'Master Import', 'LIST', '/master-import', 'MasterImportPage', 1, 1, 'system')
) AS k(ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
INNER JOIN dbo.SubModules sm ON sm.SubModuleCode = 'SUB-IMPORTMGMT'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Screens s WHERE s.SubModuleId = sm.Id AND s.ScreenCode = k.ScreenCode
);
;

/* --- Billing > Tax Management > Tax Configuration --- */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-BILLING')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-BILLING', 'Billing', 'receipt', 2, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'PRODBILL';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-TAXMGMT')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-TAXMGMT', 'Tax Management', 'percent', '/taxes', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-BILLING';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-TAXCONFIG')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-TAXCONFIG', 'Tax Configuration', 'percent', '/taxes', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-TAXMGMT';
END
;

INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, k.ScreenCode, k.ScreenName, k.ScreenType, k.RouteUrl, k.ComponentName, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('TAXTYPES', 'Tax Type Systems', 'MASTER', '/tax-type-systems', 'TaxTypeSystemPage', 1, 1, 'system'),
    ('TAXES',    'Taxes',           'MASTER', '/taxes',            'TaxPage',           2, 1, 'system')
) AS k(ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
INNER JOIN dbo.SubModules sm ON sm.SubModuleCode = 'SUB-TAXCONFIG'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Screens s WHERE s.SubModuleId = sm.Id AND s.ScreenCode = k.ScreenCode
);
;

/* --- Billing > Billing Masters > Billing Setup --- */
IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-BILLMASTERS')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-BILLMASTERS', 'Billing Masters', 'book-open', '/billing-masters', 2, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-BILLING';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-BILLSETUP')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-BILLSETUP', 'Billing Setup', 'settings', '/billing-masters', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-BILLMASTERS';
END
;

INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, k.ScreenCode, k.ScreenName, k.ScreenType, k.RouteUrl, k.ComponentName, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('PRICE-TYPES',         'Price Types',          'MASTER', '/price-types',         'PriceTypesPage',        1, 1, 'system'),
    ('UNIT-CONVERSIONS',    'Unit Conversions',     'MASTER', '/unit-conversions',    'UnitConversionPage',    2, 1, 'system'),
    ('BARCODES',            'Barcodes',             'MASTER', '/barcodes',            'BarcodePage',           3, 1, 'system'),
    ('HSN-SACS',            'HSN / SAC',            'MASTER', '/hsn-sacs',            'HsnSacPage',            4, 1, 'system'),
    ('SERVICE-CATEGORIES',  'Service Categories',   'MASTER', '/service-categories',  'ServiceCategoryPage',   5, 1, 'system'),
    ('SERVICES',            'Services',             'MASTER', '/services',            'ServicePage',           6, 1, 'system'),
    ('PRICE-LISTS',         'Price Lists',          'MASTER', '/price-lists',         'PriceListPage',         7, 1, 'system'),
    ('DISCOUNT-RULES',      'Discount Rules',       'MASTER', '/discount-rules',      'DiscountRulePage',      8, 1, 'system'),
    ('OFFERS',              'Offers',               'MASTER', '/offers',              'OfferPage',             9, 1, 'system'),
    ('COUPONS',             'Coupons',              'MASTER', '/coupons',             'CouponPage',           10, 1, 'system')
) AS k(ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
INNER JOIN dbo.SubModules sm ON sm.SubModuleCode = 'SUB-BILLSETUP'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Screens s WHERE s.SubModuleId = sm.Id AND s.ScreenCode = k.ScreenCode
);
;

/* =============================================================================
   SALES WORKSPACE (Sales > Sales Management > Transactions + POS)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-SALES')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-SALES', 'Sales', 'shopping-cart', 1, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'SALES';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-SALESMGMT')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-SALESMGMT', 'Sales Management', 'shopping-cart', '/sales', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-SALES';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-SALESTXN')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-SALESTXN', 'Transactions', 'receipt', '/sales', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-SALESMGMT';
END
;

INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, k.ScreenCode, k.ScreenName, k.ScreenType, k.RouteUrl, k.ComponentName, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('SALES_ENTRY',  'Sales Entry', 'ENTRY',  '/sales-entry', 'SalesEntryPage', 1, 1, 'system')
) AS k(ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
INNER JOIN dbo.SubModules sm ON sm.SubModuleCode = 'SUB-SALESTXN'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Screens s WHERE s.SubModuleId = sm.Id AND s.ScreenCode = k.ScreenCode
);
;

-- Sales POS submodule + screen
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-POS')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-POS', 'POS', 'monitor', '/pos', 2, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-SALESMGMT';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'SALES_POS')
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SALES_POS', 'POS', 'ENTRY', '/pos', 'PosPage', 1, 1, 'system'
    FROM dbo.SubModules WHERE SubModuleCode = 'SUB-POS';
;

/* =============================================================================
   PURCHASE WORKSPACE (Purchase > Purchase Management > Transactions)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-PURCHASE')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-PURCHASE', 'Purchase', 'shopping-cart', 1, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'PURCHASE';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-PURCHASEMGMT')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-PURCHASEMGMT', 'Purchase Management', 'shopping-cart', '/purchase', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-PURCHASE';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-PURCHASETXN')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-PURCHASETXN', 'Transactions', 'receipt', '/purchase', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-PURCHASEMGMT';
END
;

INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, k.ScreenCode, k.ScreenName, k.ScreenType, k.RouteUrl, k.ComponentName, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('PURCHASE_ENTRY',       'Purchase Entry',   'ENTRY',       '/purchase-entry',         'PurchaseEntryPage',    1, 1, 'system'),
    ('PURCHASE_LIST',        'Purchases',        'LIST',        '/purchase?tab=list',      'PurchaseListPage',     2, 1, 'system'),
    ('PURCHASE_STOCK',       'Stock',            'LIST',        '/purchase?tab=stock',     'StockPage',            3, 1, 'system'),
    ('PURCHASE_RETURNS',     'Purchase Returns', 'TRANSACTION', '/purchase?tab=returns',   'PurchaseReturnPage',   4, 1, 'system'),
    ('PURCHASE_REPORTS',     'Purchase Reports', 'REPORT',      '/reports',                'ReportsWorkspace',     5, 1, 'system')
) AS k(ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
INNER JOIN dbo.SubModules sm ON sm.SubModuleCode = 'SUB-PURCHASETXN'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Screens s WHERE s.SubModuleId = sm.Id AND s.ScreenCode = k.ScreenCode
);
;

/* =============================================================================
   INVENTORY WORKSPACE (Inventory > Inventory Management > Stock Management)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-INVENTORY')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-INVENTORY', 'Inventory', 'box', 1, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'INVENTORY';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-INVMTMGMT')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-INVMTMGMT', 'Inventory Management', 'boxes', '/stock', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-INVENTORY';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-STOCKMGMT')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-STOCKMGMT', 'Stock Management', 'warehouse', '/stock', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-INVMTMGMT';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'STOCK_HOME')
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'STOCK_HOME', 'Stock', 'LIST', '/stock', 'StockPage', 1, 1, 'system'
    FROM dbo.SubModules WHERE SubModuleCode = 'SUB-STOCKMGMT';
;

/* =============================================================================
   PAYMENTS WORKSPACE (Payment > Payment Configuration + Payment Management)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-PAYMENT')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-PAYMENT', 'Payment', 'credit-card', 1, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'PAYMENTS';
END
;

/* Payment Management > Transactions */
IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-PAYMGMT')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-PAYMGMT', 'Payment Management', 'credit-card', '/payment', 2, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-PAYMENT';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-PAYTXN')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-PAYTXN', 'Transactions', 'receipt', '/payment', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-PAYMGMT';
END
;

INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, k.ScreenCode, k.ScreenName, k.ScreenType, k.RouteUrl, k.ComponentName, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('PAYMENT_HOME',  'Payment',       'LIST',  '/payment',       'PaymentWorkspace', 1, 1, 'system'),
    ('PAYMENT_ENTRY', 'Payment Entry', 'ENTRY', '/payment-entry', 'PaymentEntryPage', 2, 1, 'system')
) AS k(ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
INNER JOIN dbo.SubModules sm ON sm.SubModuleCode = 'SUB-PAYTXN'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Screens s WHERE s.SubModuleId = sm.Id AND s.ScreenCode = k.ScreenCode
);
;

/* =============================================================================
   REPORTS WORKSPACE (Reporting > Business Reports > Reports)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-REPORTING')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-REPORTING', 'Reporting', 'file-text', 1, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'REPORTS';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-BIZREPORTS')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-BIZREPORTS', 'Business Reports', 'chart-bar', '/reports', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-REPORTING';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-REPORTS')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-REPORTS', 'Reports', 'chart-line', '/reports', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-BIZREPORTS';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'REPORTS_HOME')
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'REPORTS_HOME', 'Reports', 'REPORT', '/reports', 'ReportsWorkspace', 1, 1, 'system'
    FROM dbo.SubModules WHERE SubModuleCode = 'SUB-REPORTS';
;

/* =============================================================================
   BUSINESS_PARTNERS WORKSPACE (Partners > Partner Management)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-PARTNERS')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-PARTNERS', 'Partners', 'users', 1, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'BUSINESS_PARTNERS';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-PARTNERMGMT')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-PARTNERMGMT', 'Partner Management', 'users', '/business-partners', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-PARTNERS';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-PARTNERMASTER')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-PARTNERMASTER', 'Partner Master', 'id-card', '/business-partners', 2, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-PARTNERMGMT';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'BUSINESS_PARTNERS')
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'BUSINESS_PARTNERS', 'Business Partners', 'MASTER', '/business-partners', 'BusinessPartnersPage', 1, 1, 'system'
    FROM dbo.SubModules WHERE SubModuleCode = 'SUB-PARTNERMASTER';
;

/* =============================================================================
   SECURITY WORKSPACE (Security > User Management + Role Management)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-SECURITY')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-SECURITY', 'Security', 'shield', 1, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'SECURITY';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-USERMGMT')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-USERMGMT', 'User Management', 'user', '/users', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-SECURITY';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-USERADMIN')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-USERADMIN', 'User Administration', 'user-cog', '/users', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-USERMGMT';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'USERS')
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'USERS', 'Users', 'MASTER', '/users', 'UsersPage', 1, 1, 'system'
    FROM dbo.SubModules WHERE SubModuleCode = 'SUB-USERADMIN';
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-ROLEMGMT')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-ROLEMGMT', 'Role Management', 'shield', '/roles', 2, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-SECURITY';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-ROLEADMIN')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-ROLEADMIN', 'Role Administration', 'id-card', '/roles', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-ROLEMGMT';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'ROLES')
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'ROLES', 'Roles', 'MASTER', '/roles', 'RolesPage', 1, 1, 'system'
    FROM dbo.SubModules WHERE SubModuleCode = 'SUB-ROLEADMIN';
;

/* =============================================================================
   ENTERPRISE_PERMISSIONS WORKSPACE
   (Permission Structure + Role Security + User Security + Data Security +
    Workflow Security + Legacy module-level matrix)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-PERMCONFIG')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-PERMCONFIG', 'Permission Configuration', 'key-square', 1, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'ENTERPRISE_PERMISSIONS';
END
;

/* Permission Structure > Workspace / Domain / Module / SubModule / Screen / Field / Action management */
IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-PERMSTRUCT')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-PERMSTRUCT', 'Permission Structure', 'network', '/enterprise-permissions', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-PERMCONFIG';
END
;

DECLARE @PermStructModuleId INT = (SELECT Id FROM dbo.Modules WHERE ModuleCode = 'MOD-PERMSTRUCT');

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-WSMGMT' AND ModuleId = @PermStructModuleId)
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    VALUES (@PermStructModuleId, 'SUB-WSMGMT', 'Workspace Management', 'layers', '/workspaces', 1, 1, 'system');
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-DOMMGMT' AND ModuleId = @PermStructModuleId)
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    VALUES (@PermStructModuleId, 'SUB-DOMMGMT', 'Domain Management', 'globe', '/domains', 2, 1, 'system');
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-MODMGMT' AND ModuleId = @PermStructModuleId)
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    VALUES (@PermStructModuleId, 'SUB-MODMGMT', 'Module Management', 'box', '/modules', 3, 1, 'system');
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-SUBMODMGMT' AND ModuleId = @PermStructModuleId)
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    VALUES (@PermStructModuleId, 'SUB-SUBMODMGMT', 'SubModule Management', 'box-open', '/submodules', 4, 1, 'system');
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-SCREENMGMT' AND ModuleId = @PermStructModuleId)
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    VALUES (@PermStructModuleId, 'SUB-SCREENMGMT', 'Screen Management', 'monitor', '/screens', 5, 1, 'system');
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-FIELDMGMT' AND ModuleId = @PermStructModuleId)
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    VALUES (@PermStructModuleId, 'SUB-FIELDMGMT', 'Field Management', 'text-cursor-input', '/fields', 6, 1, 'system');
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-ACTIONMGMT' AND ModuleId = @PermStructModuleId)
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    VALUES (@PermStructModuleId, 'SUB-ACTIONMGMT', 'Action Management', 'zap', '/permission-actions-list', 7, 1, 'system');

INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, k.ScreenCode, k.ScreenName, k.ScreenType, k.RouteUrl, k.ComponentName, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('WSMGMT',        'WORKS',       'Workspaces',   'SETTINGS', '/workspaces',             'WorkspacesPage',            1, 1, 'system'),
    ('DOMMGMT',       'DOMAINS',     'Domains',      'SETTINGS', '/domains',                'DomainsPage',               1, 1, 'system'),
    ('MODMGMT',       'MODULES',     'Modules',      'SETTINGS', '/modules',                'ModulesPage',               1, 1, 'system'),
    ('SUBMODMGMT',    'SUBMODULES',  'Sub Modules',  'SETTINGS', '/submodules',             'SubModulesPage',            1, 1, 'system'),
    ('SCREENMGMT',    'SCREENS',     'Screens',      'SETTINGS', '/screens',                'ScreensPage',              1, 1, 'system'),
    ('FIELDMGMT',     'FIELDS',      'Fields',       'SETTINGS', '/fields',                 'FieldsPage',               1, 1, 'system'),
    ('ACTIONMGMT',    'ACTIONS',     'Actions',      'SETTINGS', '/permission-actions-list', 'ActionsPage',             1, 1, 'system')
) AS k(SubModuleCode, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
INNER JOIN dbo.SubModules sm ON sm.SubModuleCode = k.SubModuleCode AND sm.ModuleId = @PermStructModuleId
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Screens s WHERE s.SubModuleId = sm.Id AND s.ScreenCode = k.ScreenCode
);
;

/* Role Security > Role Permissions */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-ROLESEC')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-ROLESEC', 'Role Security', 'shield', 2, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'ENTERPRISE_PERMISSIONS';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-ROLEFIELD')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-ROLEFIELD', 'Role Field Permissions', 'columns-3', '/role-field-permissions', 2, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-ROLESEC';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-PERMMATRIX')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-PERMMATRIX', 'Role Permission Matrix', 'grid-2x2', '/role-permission-matrix', 3, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-ROLESEC';
END
;

/* User Security > User Overrides */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-USERSEC')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-USERSEC', 'User Security', 'shield-user', 3, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'ENTERPRISE_PERMISSIONS';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-USEROVERRIDES')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-USEROVERRIDES', 'User Overrides', 'user-cog', '/user-permission-overrides', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-USERSEC';
END
;

/* Data Security > Data Scope */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-DATASEC')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-DATASEC', 'Data Security', 'database', 4, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'ENTERPRISE_PERMISSIONS';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-DATASCOPE')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-DATASCOPE', 'Data Scope', 'funnel', '/data-scopes', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-DATASEC';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-USERSCOPE')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-USERSCOPE', 'User Data Scope Overrides', 'user-lock', '/user-data-scope-overrides', 2, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-DATASEC';
END
;

/* Workflow Security > Workflow */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-WORKFLOW')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-WORKFLOW', 'Workflow Security', 'workflow', 5, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'ENTERPRISE_PERMISSIONS';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-WORKFLOW')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-WORKFLOW', 'Workflow Permissions', 'workflow', '/workflow-permissions', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-WORKFLOW';
END
;

/* Permission Management > Enterprise Permission hub */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-PERMMGMT')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-PERMMGMT', 'Permission Management', 'key', 6, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'ENTERPRISE_PERMISSIONS';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-ENTPERMS')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-ENTPERMS', 'Enterprise Permission', 'key-square', '/enterprise-permissions', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-PERMMGMT';
END
;

/* Add the permission management submodules + screens (attached to the
   ENTERPRISE_PERMISSIONS workspace domains/modules created above) */

/* Role Security > Role Field Permissions > Field Security */
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-ROLEFIELDSEC')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-ROLEFIELDSEC', 'Field Security', 'columns-3', '/role-field-permissions', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-ROLEFIELD';
END
;

/* Role Security > Role Permission Matrix > Matrix */
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-PERMMATRIX')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-PERMMATRIX', 'Permission Matrix', 'grid-2x2', '/role-permission-matrix', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-PERMMATRIX';
END
;

/* User Security > User Overrides > Permission Overrides */
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-PERMOVERRIDES')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-PERMOVERRIDES', 'Permission Overrides', 'user-cog', '/user-permission-overrides', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-USEROVERRIDES';
END
;

/* Data Security > Data Scope > Scope Management */
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-SCOPEMGMT')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-SCOPEMGMT', 'Scope Management', 'funnel', '/data-scopes', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-DATASCOPE';
END
;

/* Data Security > User Data Scope Overrides > User Scope Overrides */
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-USERSCOPE')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-USERSCOPE', 'User Scope Overrides', 'user-lock', '/user-data-scope-overrides', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-USERSCOPE';
END
;

/* Workflow Security > Workflow > Workflow Permissions */
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-WORKFLOWPERMS')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-WORKFLOWPERMS', 'Workflow Permissions', 'workflow', '/workflow-permissions', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-WORKFLOW';
END
;

/* Permission Management > Enterprise Permission > Matrix */
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-ENTPERMS')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-ENTPERMS', 'Enterprise Permission Matrix', 'key-square', '/enterprise-permissions', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-ENTPERMS';
END
;

/* Attach screens to their ENTERPRISE_PERMISSIONS submodules */
INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, k.ScreenCode, k.ScreenName, k.ScreenType, k.RouteUrl, k.ComponentName, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('SUB-ROLEFIELDSEC',  'ROLE_FIELD_PERMISSIONS',   'Role Field Permissions',   'SETTINGS', '/role-field-permissions',     'RoleFieldPermissionsPage',     1, 1, 'system'),
    ('SUB-PERMMATRIX',    'ROLE_PERMISSION_MATRIX',   'Role Permission Matrix',   'SETTINGS', '/role-permission-matrix',     'RolePermissionMatrixPage',     1, 1, 'system'),
    ('SUB-PERMOVERRIDES', 'USER_PERMISSION_OVERRIDES','User Permission Overrides','SETTINGS', '/user-permission-overrides',  'UserPermissionOverridesPage',  1, 1, 'system'),
    ('SUB-SCOPEMGMT',     'DATA_SCOPES',              'Data Scopes',              'SETTINGS', '/data-scopes',                'DataScopesPage',               1, 1, 'system'),
    ('SUB-USERSCOPE',     'USER_DATA_SCOPE_OVERRIDES','User Data Scope Overrides','SETTINGS', '/user-data-scope-overrides',  'UserDataScopeOverridesPage',   1, 1, 'system'),
    ('SUB-WORKFLOWPERMS', 'WORKFLOW_PERMISSIONS',     'Workflow Permissions',     'SETTINGS', '/workflow-permissions',       'WorkflowPermissionsPage',      1, 1, 'system'),
    ('SUB-ENTPERMS',      'ENTERPRISE_PERMISSIONS',   'Enterprise Permissions',   'SETTINGS', '/enterprise-permissions',     'EnterprisePermissionsPage',    1, 1, 'system')
) AS k(SubModuleCode, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
INNER JOIN dbo.SubModules sm ON sm.SubModuleCode = k.SubModuleCode
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Screens s WHERE s.SubModuleId = sm.Id AND s.ScreenCode = k.ScreenCode
);
;

/* =============================================================================
   SETTINGS WORKSPACE (System > Settings Management > System Settings)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'DOM-SETTINGS')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'DOM-SETTINGS', 'System', 'settings', 1, 1, 'system'
    FROM dbo.Workspaces WHERE WorkspaceCode = 'SETTINGS';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-SETTINGSMGMT')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-SETTINGSMGMT', 'Settings Management', 'settings', '/settings', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-SETTINGS';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-SYSETTINGS')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-SYSETTINGS', 'System Settings', 'sliders-horizontal', '/settings', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-SETTINGSMGMT';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'SETTINGS_HOME')
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SETTINGS_HOME', 'Settings', 'SETTINGS', '/settings', 'SettingsPage', 1, 1, 'system'
    FROM dbo.SubModules WHERE SubModuleCode = 'SUB-SYSETTINGS';
;

/* ---------------------------------------------------------------------------
   Legacy role permissions (SuperAdmin role 1 + Administrator role 2)
   Bounded to the ERP screens seeded above (flat codes bridged to the
   hierarchical RolePermissions matrix by the backend).
   --------------------------------------------------------------------------- */
INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode, CreatedBy)
SELECT r.RoleId, v.code, 'system'
FROM dbo.Roles r
CROSS JOIN (VALUES
    /* SETUP / Organization */
    ('companies.view'), ('companies.create'), ('companies.edit'), ('companies.delete'),
    ('branches.view'), ('branches.create'), ('branches.edit'), ('branches.delete'),
    ('departments.view'), ('departments.create'), ('departments.edit'), ('departments.delete'),
    ('warehouses.view'), ('warehouses.create'), ('warehouses.edit'), ('warehouses.delete'),
    ('stores.view'), ('stores.create'), ('stores.edit'), ('stores.delete'),
    ('counters.view'), ('counters.create'), ('counters.edit'), ('counters.delete'),
    ('pos-sessions.view'), ('pos-sessions.create'), ('pos-sessions.edit'), ('pos-sessions.delete'),
    ('designations.view'), ('designations.create'), ('designations.edit'), ('designations.delete'),
    ('financial-years.view'), ('financial-years.create'), ('financial-years.edit'), ('financial-years.delete'),
    ('system-master.view'), ('system-master.manage'),
    ('tenant-config.view'), ('tenant-config.manage'),
    /* LOOKUP / DROPDOWN MASTERS (no screen rows in the matrix; seed for admin) */
    ('business-types.view'), ('business-types.manage'),
    ('industry-types.view'), ('industry-types.manage'),
    ('company-groups.view'), ('company-groups.manage'),
    ('locations.view'), ('locations.create'), ('locations.edit'), ('locations.delete'),
    ('languages.view'), ('languages.manage'),
    ('timezones.view'), ('timezones.manage'),
    ('gst-registration-types.view'), ('gst-registration-types.manage'),
    ('address-types.view'), ('address-types.manage'),
    ('contact-types.view'), ('contact-types.manage'),
    ('document-types.view'), ('document-types.manage'),
    ('organization-types.view'), ('organization-types.manage'),
    ('branch-types.view'), ('branch-types.manage'),
    ('warehouse-types.view'), ('warehouse-types.manage'),
    ('employment-types.view'), ('employment-types.manage'),
    ('employees.view'), ('employees.create'), ('employees.edit'), ('employees.delete'),
    ('currencies.view'), ('currencies.manage'),
    ('payment-types.view'), ('payment-types.manage'),
    ('payment-methods.view'), ('payment-methods.manage'),
    ('payment-method-details.view'), ('payment-method-details.manage'),
    ('permission-modules.view'), ('permission-modules.manage'),
    ('permission-actions.view'), ('permission-actions.manage'),
    ('audit.view'), ('profile.edit'),
    /* PRODBILL / Product & Billing */
    ('product-categories.view'), ('product-categories.create'), ('product-categories.edit'), ('product-categories.delete'),
    ('product-subcategories.view'), ('product-subcategories.create'), ('product-subcategories.edit'), ('product-subcategories.delete'),
    ('brands.view'), ('brands.create'), ('brands.edit'), ('brands.delete'),
    ('units.view'), ('units.create'), ('units.edit'), ('units.delete'),
    ('products.view'), ('products.create'), ('products.edit'), ('products.delete'),
    ('tax-type-systems.view'), ('tax-type-systems.create'), ('tax-type-systems.edit'), ('tax-type-systems.delete'),
    ('taxes.view'), ('taxes.create'), ('taxes.edit'), ('taxes.delete'),
    ('master-import.view'), ('master-import.manage'), ('import-logs.view'),
    /* SALES */
    ('sales.view'), ('sales.manage'), ('sales.create'), ('sales.edit'), ('sales.delete'),
    ('sales.pos.view'), ('sales.return.view'), ('sales.return.manage'),
    /* PURCHASE */
    ('purchases.view'), ('purchases.create'), ('purchases.edit'), ('purchases.cancel'), ('purchases.delete'), ('purchases.manage'),
    ('purchases.return.view'), ('purchases.return.manage'),
    ('purchases-return.create'), ('purchases-return.view'), ('purchases-return.edit'), ('purchases-return.cancel'), ('purchases-return.delete'),
    /* INVENTORY */
    ('stock.view'), ('stock.manage'),
    /* PAYMENTS */
    ('payments.view'), ('payments.manage'),
    /* REPORTS */
    ('reports.view'),
    /* BUSINESS_PARTNERS */
    ('business-partners.view'), ('business-partners.manage'),
    /* SECURITY */
    ('users.view'), ('users.create'), ('users.edit'), ('users.delete'),
    ('roles.view'), ('roles.manage'),
    /* ENTERPRISE_PERMISSIONS */
    ('workspaces.view'), ('workspaces.manage'),
    ('domains.view'), ('domains.manage'),
    ('modules.view'), ('modules.manage'),
    ('submodules.view'), ('submodules.manage'),
    ('screens.view'), ('screens.manage'),
    ('fields.view'), ('fields.manage'),
    ('actions.view'), ('actions.manage'),
    ('role-field-permissions.view'), ('role-field-permissions.manage'),
    ('role-permission-matrix.view'), ('role-permission-matrix.manage'),
    ('user-permission-overrides.view'), ('user-permission-overrides.manage'),
    ('data-scopes.view'), ('data-scopes.manage'),
    ('user-data-scope-overrides.view'), ('user-data-scope-overrides.manage'),
    ('workflow-permissions.view'), ('workflow-permissions.manage'),
    ('enterprise-permissions.view'), ('enterprise-permissions.manage'),
    /* SETTINGS */
    ('settings.view'), ('settings.edit')
) AS v(code)
WHERE r.Code IN ('SuperAdmin', 'Administrator')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissionsLegacy rp WHERE rp.RoleId = r.RoleId AND rp.PermissionCode = v.code);

/* ---------------------------------------------------------------------------
   Hierarchical RolePermissions matrix (real role-based permission data).
   Grants each role only the Actions it needs per Screen (view/create/edit/
   delete), instead of a blanket allow-everything. The matrix is declarative:
   each row = (RoleCode, ScreenCode, ActionCode). NULL ScreenCode = every
   screen; NULL ActionCode = every action on the given screen. It is joined
   to the seeded Workspace->Domain->Module->SubModule->Screen and Action
   hierarchy, so it stays in sync with screens/actions above and drives both
   the permission-matrix UI and the derived flat API permission codes (bridge
   below). SuperAdmin retains full access; the business roles are restricted
   to the screens relevant to their function.
   --------------------------------------------------------------------------- */
INSERT INTO dbo.RolePermissions
    (RoleId, WorkspaceId, DomainId, ModuleId, SubModuleId, ScreenId, ActionId,
     Allow, DisplayOrder, IsActive, CreatedBy, CreatedDate)
SELECT r.RoleId,
       w.Id                 AS WorkspaceId,
       d.Id                 AS DomainId,
       m.Id                 AS ModuleId,
       sm.Id                AS SubModuleId,
       s.Id                 AS ScreenId,
       a.Id                 AS ActionId,
       1                    AS Allow,
       a.DisplayOrder AS DisplayOrder,
       1                    AS IsActive,
       'system'             AS CreatedBy,
       SYSUTCDATETIME()
FROM dbo.Roles r
INNER JOIN (VALUES
    /* ------------- SuperAdmin: every screen, every action ------------- */
    ('SuperAdmin',     NULL,                    NULL),

    /* ------------- Administrator: every screen EXCEPT security config -------------
       View + CRUD on masters/operational for all screens, but the 7
       permission/security configuration screens are SuperAdmin-only (they are
       excluded in the WHERE clause below via AdministrativeOnlyScreens). */
    ('Administrator',  NULL,                    NULL),

    /* ------------- Financial Year Officer ------------- */
    ('FinanceYearOfficer', 'FINANCE_YEAR',    NULL),
    ('FinanceYearOfficer', 'COMPANY_MASTER',  'view'),
    ('FinanceYearOfficer', 'COMPANY_MASTER',  'edit'),
    ('FinanceYearOfficer', 'REPORTS_HOME',    'view'),
    ('FinanceYearOfficer', 'SETTINGS_HOME',   'view'),
    ('FinanceYearOfficer', 'SYSTEM_MASTER_HOME', 'view'),

    /* ------------- Store Manager ------------- */
    ('StoreManager',   'STORE_MASTER',        NULL),
    ('StoreManager',   'COUNTER_MASTER',      NULL),
    ('StoreManager',   'POS_SESSION_MASTER',  NULL),
    ('StoreManager',   'PRODUCT-CATEGORIES',  NULL),
    ('StoreManager',   'PRODUCT-SUBCATEGORIES', NULL),
    ('StoreManager',   'BRANDS',              NULL),
    ('StoreManager',   'UNITS',               NULL),
    ('StoreManager',   'PRODUCTS',            NULL),
    ('StoreManager',   'STOCK_HOME',          NULL),
    ('StoreManager',   'BUSINESS_PARTNERS',   NULL),
    ('StoreManager',   'SALES_ENTRY',         'view'),
    ('StoreManager',   'SALES_ENTRY',         'create'),
    ('StoreManager',   'SALES_ENTRY',         'edit'),
    ('StoreManager',   'PURCHASE_ENTRY',      'view'),
    ('StoreManager',   'PURCHASE_LIST',       'view'),
    ('StoreManager',   'PURCHASE_STOCK',      'view'),
    ('StoreManager',   'PURCHASE_RETURNS',    'view'),
    ('StoreManager',   'REPORTS_HOME',        'view'),
    ('StoreManager',   'MASTER_IMPORT',       'view'),
    ('StoreManager',   'IMPORT-LOGS',         'view'),

    /* ------------- POS Cashier ------------- */
    ('POSCashier',     'SALES_POS',           NULL),
    ('POSCashier',     'SALES_ENTRY',         'view'),
    ('POSCashier',     'SALES_ENTRY',         'create'),
    ('POSCashier',     'PAYMENT_ENTRY',       'view'),
    ('POSCashier',     'PAYMENT_ENTRY',       'create'),
    ('POSCashier',     'PAYMENT_HOME',        'view'),
    ('POSCashier',     'COUNTER_MASTER',      'view'),
    ('POSCashier',     'USERS',               'view'),

    /* ------------- Purchase Admin ------------- */
    ('PurchaseAdmin',  'PURCHASE_ENTRY',      NULL),
    ('PurchaseAdmin',  'PURCHASE_LIST',       NULL),
    ('PurchaseAdmin',  'PURCHASE_STOCK',      'view'),
    ('PurchaseAdmin',  'PURCHASE_RETURNS',    NULL),
    ('PurchaseAdmin',  'PURCHASE_REPORTS',    NULL),
    ('PurchaseAdmin',  'PRODUCTS',            'view'),
    ('PurchaseAdmin',  'PRODUCT-CATEGORIES',  'view'),
    ('PurchaseAdmin',  'STOCK_HOME',          'view'),
    ('PurchaseAdmin',  'BUSINESS_PARTNERS',   'view'),
    ('PurchaseAdmin',  'REPORTS_HOME',        'view'),

    /* ------------- Sales Admin ------------- */
    ('SalesAdmin',     'SALES_ENTRY',         NULL),
    ('SalesAdmin',     'SALES_POS',           'view'),
    ('SalesAdmin',     'SALES_POS',           'create'),
    ('SalesAdmin',     'PRODUCTS',            'view'),
    ('SalesAdmin',     'PRODUCT-CATEGORIES',  'view'),
    ('SalesAdmin',     'PRODUCT-SUBCATEGORIES', 'view'),
    ('SalesAdmin',     'BRANDS',              'view'),
    ('SalesAdmin',     'UNITS',               'view'),
    ('SalesAdmin',     'BUSINESS_PARTNERS',   NULL),
    ('SalesAdmin',     'STOCK_HOME',          'view'),
    ('SalesAdmin',     'PAYMENT_ENTRY',       'view'),
    ('SalesAdmin',     'PAYMENT_ENTRY',       'create'),
    ('SalesAdmin',     'PAYMENT_ENTRY',       'edit'),
    ('SalesAdmin',     'REPORTS_HOME',        'view')
) v(RoleCode, ScreenCode, ActionCode)
INNER JOIN dbo.Screens s ON (v.ScreenCode IS NULL OR s.ScreenCode = v.ScreenCode)
    AND s.IsActive = 1
INNER JOIN dbo.SubModules sm ON sm.Id = s.SubModuleId
INNER JOIN dbo.Modules m ON m.Id = sm.ModuleId
INNER JOIN dbo.Domains d ON d.Id = m.DomainId
INNER JOIN dbo.Workspaces w ON w.Id = d.WorkspaceId
CROSS JOIN dbo.Actions a
    ON (v.ActionCode IS NULL OR a.ActionCode = v.ActionCode)
    AND a.IsActive = 1
WHERE r.Code = v.RoleCode
  AND NOT (r.Code = 'Administrator' AND v.ScreenCode IS NULL
           AND s.ScreenCode IN (
               'ROLE_FIELD_PERMISSIONS',
               'ROLE_PERMISSION_MATRIX',
               'USER_PERMISSION_OVERRIDES',
               'DATA_SCOPES',
               'USER_DATA_SCOPE_OVERRIDES',
               'WORKFLOW_PERMISSIONS',
               'ENTERPRISE_PERMISSIONS'))
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RolePermissions rp
      WHERE rp.RoleId = r.RoleId
        AND rp.WorkspaceId = w.Id
        AND rp.DomainId = d.Id
        AND rp.ModuleId = m.Id
        AND rp.SubModuleId = sm.Id
        AND rp.ScreenId = s.Id
        AND rp.ActionId = a.Id
  );

/* ---------------------------------------------------------------------------
   Screens.PermissionCode mapping (flat API codes used by [Permission] checks).
   Kept in sync with ONEERP.Shared/Constants/Permissions.cs.
   --------------------------------------------------------------------------- */
UPDATE s
SET s.PermissionCode = v.PermissionCode
FROM dbo.Screens s
INNER JOIN (VALUES
    ('COMPANY_MASTER',          'companies'),
    ('BRANCH_MASTER',           'branches'),
    ('DEPARTMENT_MASTER',       'departments'),
    ('WAREHOUSE_MASTER',        'warehouses'),
    ('DESIGNATION_MASTER',      'designations'),
    ('STORE_MASTER',            'stores'),
    ('COUNTER_MASTER',          'counters'),
    ('POS_SESSION_MASTER',      'pos-sessions'),
    ('FINANCE_YEAR',            'financial-years'),
    ('SYSTEM_MASTER_HOME',      'system-master'),
    ('TENANT_CONFIGURATION',    'tenant-config'),
    ('PRODUCT-CATEGORIES',      'product-categories'),
    ('PRODUCT-SUBCATEGORIES',   'product-subcategories'),
    ('BRANDS',                  'brands'),
    ('UNITS',                   'units'),
    ('PRODUCTS',                'products'),
    ('IMPORT-LOGS',             'import-logs'),
    ('MASTER_IMPORT',           'master-import'),
    ('TAXTYPES',                'tax-type-systems'),
    ('TAXES',                   'taxes'),
    ('PRICE-TYPES',             'price-types'),
    ('UNIT-CONVERSIONS',        'unit-conversions'),
    ('BARCODES',                'barcodes'),
    ('HSN-SACS',                'hsn-sacs'),
    ('SERVICE-CATEGORIES',      'service-categories'),
    ('SERVICES',                'services'),
    ('PRICE-LISTS',             'price-lists'),
    ('DISCOUNT-RULES',          'discount-rules'),
    ('OFFERS',                  'offers'),
    ('COUPONS',                 'coupons'),
    ('SALES_ENTRY',             'sales'),
    ('SALES_POS',               'sales.pos'),
    ('PURCHASE_ENTRY',          'purchases'),
    ('PURCHASE_LIST',           'purchases'),
    ('PURCHASE_STOCK',          'purchases'),
    ('PURCHASE_RETURNS',        'purchases.return'),
    ('PURCHASE_REPORTS',        'reports'),
    ('STOCK_HOME',              'stock'),
    ('PAYMENT_HOME',            'payments'),
    ('PAYMENT_ENTRY',           'payments'),
    ('REPORTS_HOME',            'reports'),
    ('BUSINESS_PARTNERS',       'business-partners'),
    ('USERS',                   'users'),
    ('ROLES',                   'roles'),
    ('WORKS',                   'workspaces'),
    ('DOMAINS',                 'domains'),
    ('MODULES',                 'modules'),
    ('SUBMODULES',              'submodules'),
    ('SCREENS',                 'screens'),
    ('FIELDS',                  'fields'),
    ('ACTIONS',                 'actions'),
    ('ROLE_FIELD_PERMISSIONS',  'role-field-permissions'),
    ('ROLE_PERMISSION_MATRIX',  'role-permission-matrix'),
    ('USER_PERMISSION_OVERRIDES','user-permission-overrides'),
    ('DATA_SCOPES',             'data-scopes'),
    ('USER_DATA_SCOPE_OVERRIDES','user-data-scope-overrides'),
    ('WORKFLOW_PERMISSIONS',    'workflow-permissions'),
    ('ENTERPRISE_PERMISSIONS',  'enterprise-permissions'),
    ('SETTINGS_HOME',           'settings')
) v(ScreenCode, PermissionCode)
    ON s.ScreenCode = v.ScreenCode
WHERE s.PermissionCode IS NULL OR s.PermissionCode <> v.PermissionCode;

/* ---------------------------------------------------------------------------
   Matrix -> Legacy bridge. Derive RolePermissionsLegacy flat codes for every
   matrix-managed role from the hierarchical RolePermissions grants (mirrors
   EnterprisePermissionServices.BulkAssignAsync) so roles granted ONLY through
   the Role Permission Matrix (e.g. a custom "Purchase Admin") get real API
   authorization. Roles without matrix rows (e.g. SalesAdmin) keep their
   manually seeded legacy codes untouched. The clean-up DELETE below only
   removes codes that the matrix CAN re-derive ({screen.PermissionCode}.
   {action.ActionCode} where the action exists in dbo.Actions, e.g.
   "stores.view"), preserving service/platform codes that do NOT exist as
   matrix actions (e.g. "roles.manage", "sales.manage").
   --------------------------------------------------------------------------- */
DELETE rp
FROM dbo.RolePermissionsLegacy rp
WHERE EXISTS (
    SELECT 1
    FROM dbo.Screens s
    INNER JOIN dbo.Actions a ON a.IsActive = 1
    WHERE s.PermissionCode IS NOT NULL AND s.PermissionCode <> ''
      AND rp.PermissionCode = CONCAT(s.PermissionCode, '.', a.ActionCode)
      AND EXISTS (
          SELECT 1 FROM dbo.RolePermissions r WHERE r.RoleId = rp.RoleId)
);

INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode, CreatedBy)
SELECT DISTINCT rp.RoleId,
                CONCAT(s.PermissionCode, '.', a.ActionCode),
                'system'
FROM dbo.RolePermissions rp
INNER JOIN dbo.Screens s ON s.Id = rp.ScreenId
    AND s.PermissionCode IS NOT NULL AND s.PermissionCode <> ''
INNER JOIN dbo.Actions a ON a.Id = rp.ActionId
WHERE rp.Allow = 1 AND rp.IsActive = 1
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RolePermissionsLegacy l
      WHERE l.RoleId = rp.RoleId
        AND l.PermissionCode = CONCAT(s.PermissionCode, '.', a.ActionCode)
  );

PRINT 'ERP module configuration seed complete.';

/* =============================================================================
   ONE ERP - Universal Entity layer (Address / Contact / File / Note / Tag)
   Purpose  : Adds the polymorphic Entity layer used by every master (Company,
              Branch, Warehouse, Store, Product, Service, Employee, Customer/
              Supplier via BusinessPartners). One Entity has many Addresses,
              Contacts, Files, Notes and Tags, connected through junction tables.
              Business masters reference the common Entity through an EntityId
              column. AddressTypeId / ContactTypeId reuse the existing
              AddressTypes / ContactTypes system masters.
   Safe     : Re-runnable (IF NOT EXISTS guards on every object / column / FK).
   ============================================================================= */

/* ---------------------------------------------------------------------------
   Entity - common identity (COMPANY, BRANCH, WAREHOUSE, STORE, PRODUCT,
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
   Address - reusable postal address.
   AddressTypeId reuses dbo.AddressTypes; CityId/StateId/CountryId reuse the
   location masters.
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
   EntityAddress - junction: entity <-> address
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
   Contact - reusable person/contact row.
   ContactTypeId reuses dbo.ContactTypes.
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
   EntityContact - junction: entity <-> contact
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
   Files - MinIO / S3 stored-object metadata only
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
   EntityFile - junction: entity <-> file (FileType: LOGO / DOCUMENT / IMAGE / ATTACHMENT)
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
   Note - common note row
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
   EntityNote - junction: entity <-> note
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
   Tag - common tag row
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
   EntityTag - junction: entity <-> tag
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
   Business Masters -> EntityId
   Adds a nullable FK column on each business master so the master keeps its
   own columns and simply points to the common Entity row that owns its
   Addresses / Contacts / Files / Notes / Tags.
--------------------------------------------------------------------------- */

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Companies]') AND name = N'EntityId')
    ALTER TABLE dbo.Companies ADD EntityId BIGINT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Companies_Entity')
    ALTER TABLE dbo.Companies ADD CONSTRAINT FK_Companies_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Branches]') AND name = N'EntityId')
    ALTER TABLE dbo.Branches ADD EntityId BIGINT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Branches_Entity')
    ALTER TABLE dbo.Branches ADD CONSTRAINT FK_Branches_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Warehouses]') AND name = N'EntityId')
    ALTER TABLE dbo.Warehouses ADD EntityId BIGINT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Warehouses_Entity')
    ALTER TABLE dbo.Warehouses ADD CONSTRAINT FK_Warehouses_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Stores]') AND name = N'EntityId')
    ALTER TABLE dbo.Stores ADD EntityId BIGINT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Stores_Entity')
    ALTER TABLE dbo.Stores ADD CONSTRAINT FK_Stores_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = N'EntityId')
    ALTER TABLE dbo.Products ADD EntityId BIGINT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Products_Entity')
    ALTER TABLE dbo.Products ADD CONSTRAINT FK_Products_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = N'EntityId')
    ALTER TABLE dbo.Services ADD EntityId BIGINT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Services_Entity')
    ALTER TABLE dbo.Services ADD CONSTRAINT FK_Services_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND name = N'EntityId')
    ALTER TABLE dbo.Employees ADD EntityId BIGINT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Employees_Entity')
    ALTER TABLE dbo.Employees ADD CONSTRAINT FK_Employees_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BusinessPartners]') AND name = N'EntityId')
    ALTER TABLE dbo.BusinessPartners ADD EntityId BIGINT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_BusinessPartners_Entity')
    ALTER TABLE dbo.BusinessPartners ADD CONSTRAINT FK_BusinessPartners_Entity FOREIGN KEY (EntityId) REFERENCES dbo.Entity (EntityId);

PRINT 'Universal Entity layer added.';

/* =============================================================================
   Invoice Template Design (merged from sql/erp_migration_002_invoice_templates.sql)
   Purpose  : Adds the Invoice Template Design module - a single common designer
              used for Retail, POS, Service, Wholesale, GST, Export and Purchase
              documents. One InvoiceType (what document), one PaperSize (physical
              size), one PrinterType (technology), one Template (how it looks),
              Assignment (which template), Version (lifecycle), Component
              (drag/drop block), Variable (data binding) and a Renderer that
              emits PDF / thermal print output from the published TemplateJson.
   Safe     : Re-runnable (IF NOT EXISTS guards on every object / column / FK;
              seeds insert row-by-row with existence checks).
   Notes    : CompanyId / IndustryTypeId are INT to match the existing
              dbo.Companies.Id (INT) and dbo.IndustryTypes.IndustryTypeId (INT)
              so FK constraints can be declared; every new table PK is BIGINT.
   ============================================================================= */

/* =============================================================================
   01. InvoiceType - WHAT document is being printed
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceType]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceType (
        InvoiceTypeId BIGINT IDENTITY(1,1)  NOT NULL CONSTRAINT PK_InvoiceType PRIMARY KEY,
        Code          VARCHAR(30)            NOT NULL CONSTRAINT UQ_InvoiceType_Code UNIQUE,
        Name          VARCHAR(100)           NOT NULL,
        Description   VARCHAR(300)           NULL,
        DisplayOrder  INT                    NOT NULL CONSTRAINT DF_InvoiceType_DisplayOrder DEFAULT 0,
        IsActive      BIT                    NOT NULL CONSTRAINT DF_InvoiceType_IsActive DEFAULT 1,
        CreatedBy     BIGINT                 NULL,
        CreatedAt     DATETIME               NOT NULL CONSTRAINT DF_InvoiceType_CreatedAt DEFAULT GETDATE(),
        ModifiedBy    BIGINT                 NULL,
        ModifiedAt    DATETIME               NULL
    );

    CREATE INDEX IX_InvoiceType_IsActive ON dbo.InvoiceType (IsActive);
    CREATE INDEX IX_InvoiceType_DisplayOrder ON dbo.InvoiceType (DisplayOrder);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.InvoiceType WHERE Code = 'SALES')
BEGIN
    INSERT INTO dbo.InvoiceType (Code, Name, Description, DisplayOrder) VALUES
        ('SALES',          'Sales Invoice',        'Standard sales invoice',                        1),
        ('POS',            'POS Receipt',          'Cash counter / walk-in receipts (thermal)',      2),
        ('SERVICE',        'Service Invoice',      'Service delivery / job-work invoice',            3),
        ('HYBRID',         'Hybrid Invoice',       'Goods + services combined invoice',              4),
        ('WHOLESALE',      'Wholesale Invoice',    'Bulk / distribution invoice',                    5),
        ('EXPORT',         'Export Invoice',       'Export / cross-border invoice',                  6),
        ('PROFORMA',       'Proforma Invoice',     'Quotation-style advance invoice',                7),
        ('CREDIT_NOTE',    'Credit Note',          'Customer credit / returns adjustment',           8),
        ('DEBIT_NOTE',     'Debit Note',           'Debit / additional-charge note',                 9),
        ('SALES_RETURN',   'Sales Return',         'Sales return document',                          10),
        ('PURCHASE',       'Purchase Invoice',     'Vendor / supplier purchase invoice',             11),
        ('PURCHASE_RETURN','Purchase Return',      'Vendor return document',                         12);
END
;

/* =============================================================================
   02. InvoicePaperSize - physical page / roll size
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoicePaperSize]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoicePaperSize (
        PaperSizeId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoicePaperSize PRIMARY KEY,
        Code        VARCHAR(30)           NOT NULL CONSTRAINT UQ_InvoicePaperSize_Code UNIQUE,
        Name        VARCHAR(100)          NOT NULL,
        Width       DECIMAL(10,2)         NOT NULL,
        Height      DECIMAL(10,2)         NULL,
        Unit        VARCHAR(10)           NOT NULL CONSTRAINT DF_InvoicePaperSize_Unit DEFAULT 'MM',
        IsThermal   BIT                   NOT NULL CONSTRAINT DF_InvoicePaperSize_IsThermal DEFAULT 0,
        IsCustom    BIT                   NOT NULL CONSTRAINT DF_InvoicePaperSize_IsCustom DEFAULT 0,
        IsActive    BIT                   NOT NULL CONSTRAINT DF_InvoicePaperSize_IsActive DEFAULT 1,
        CreatedBy   BIGINT                NULL,
        CreatedAt   DATETIME              NOT NULL CONSTRAINT DF_InvoicePaperSize_CreatedAt DEFAULT GETDATE(),
        ModifiedBy  BIGINT                NULL,
        ModifiedAt  DATETIME              NULL
    );

    CREATE INDEX IX_InvoicePaperSize_IsActive ON dbo.InvoicePaperSize (IsActive);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.InvoicePaperSize WHERE Code = 'A4')
BEGIN
    INSERT INTO dbo.InvoicePaperSize (Code, Name, Width, Height, Unit, IsThermal, IsCustom) VALUES
        ('A4',      'A4',                210.00, 297.00, 'MM',  0, 0),
        ('A5',      'A5',                148.00, 210.00, 'MM',  0, 0),
        ('A6',      'A6',                105.00, 148.00, 'MM',  0, 0),
        ('58MM',    '58mm Thermal',       58.00,  NULL,   'MM',  1, 0),
        ('80MM',    '80mm Thermal',       80.00,  NULL,   'MM',  1, 0),
        ('LETTER',  'Letter',            215.90, 279.40, 'MM',  0, 0),
        ('LEGAL',   'Legal',             215.90, 355.60, 'MM',  0, 0),
        ('CUSTOM',  'Custom',              0.00,   0.00, 'MM',  0, 1);
END
;
-- Safety UPDATE so the rows created above always carry correct physics on re-runs.
UPDATE dbo.InvoicePaperSize SET Width = 210.00, Height = 297.00, Unit = 'MM', IsThermal = 0 WHERE Code = 'A4'    AND IsCustom = 0;
UPDATE dbo.InvoicePaperSize SET Width = 148.00, Height = 210.00, Unit = 'MM', IsThermal = 0 WHERE Code = 'A5'    AND IsCustom = 0;
UPDATE dbo.InvoicePaperSize SET Width = 58.00,  Height = NULL,   Unit = 'MM', IsThermal = 1 WHERE Code = '58MM'  AND IsCustom = 0;
UPDATE dbo.InvoicePaperSize SET Width = 80.00,  Height = NULL,   Unit = 'MM', IsThermal = 1 WHERE Code = '80MM'  AND IsCustom = 0;

/* =============================================================================
   03. PrinterType - printing technology
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PrinterType]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PrinterType (
        PrinterTypeId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PrinterType PRIMARY KEY,
        Code          VARCHAR(30)           NOT NULL CONSTRAINT UQ_PrinterType_Code UNIQUE,
        Name          VARCHAR(100)          NOT NULL,
        Description   VARCHAR(300)          NULL,
        IsActive      BIT                   NOT NULL CONSTRAINT DF_PrinterType_IsActive DEFAULT 1,
        CreatedBy     BIGINT                NULL,
        CreatedAt     DATETIME              NOT NULL CONSTRAINT DF_PrinterType_CreatedAt DEFAULT GETDATE(),
        ModifiedBy    BIGINT                NULL,
        ModifiedAt    DATETIME              NULL
    );

    CREATE INDEX IX_PrinterType_IsActive ON dbo.PrinterType (IsActive);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.PrinterType WHERE Code = 'THERMAL')
BEGIN
    INSERT INTO dbo.PrinterType (Code, Name, Description) VALUES
        ('THERMAL',    'Thermal',     'Heat-based direct thermal printing (receipt printers)'),
        ('LASER',      'Laser',       'Laser / LED page printing'),
        ('INKJET',     'Inkjet',      'Inkjet page printing'),
        ('DOT_MATRIX', 'Dot Matrix',  'Impact / dot matrix printing'),
        ('PDF',        'PDF',         'Render to PDF file (no physical printer)');
END
;

/* =============================================================================
   04. PrinterModel - optional physical printer models
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PrinterModel]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PrinterModel (
        PrinterModelId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PrinterModel PRIMARY KEY,
        PrinterTypeId  BIGINT               NOT NULL,
        Code           VARCHAR(50)           NOT NULL CONSTRAINT UQ_PrinterModel_Code UNIQUE,
        Name           VARCHAR(150)          NOT NULL,
        Manufacturer   VARCHAR(100)          NULL,
        IsActive       BIT                   NOT NULL CONSTRAINT DF_PrinterModel_IsActive DEFAULT 1,
        CreatedBy      BIGINT                NULL,
        CreatedAt      DATETIME              NOT NULL CONSTRAINT DF_PrinterModel_CreatedAt DEFAULT GETDATE(),
        ModifiedBy     BIGINT                NULL,
        ModifiedAt     DATETIME              NULL,
        CONSTRAINT FK_PrinterModel_PrinterType FOREIGN KEY (PrinterTypeId) REFERENCES dbo.PrinterType (PrinterTypeId)
    );

    CREATE INDEX IX_PrinterModel_PrinterTypeId ON dbo.PrinterModel (PrinterTypeId);
    CREATE INDEX IX_PrinterModel_IsActive ON dbo.PrinterModel (IsActive);
END
;

/* =============================================================================
   05. InvoiceTemplateCategory - classification of the template itself
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateCategory]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateCategory (
        TemplateCategoryId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateCategory PRIMARY KEY,
        Code               VARCHAR(30)           NOT NULL CONSTRAINT UQ_InvoiceTemplateCategory_Code UNIQUE,
        Name               VARCHAR(100)          NOT NULL,
        Description        VARCHAR(300)          NULL,
        DisplayOrder       INT                   NOT NULL CONSTRAINT DF_InvoiceTemplateCategory_DisplayOrder DEFAULT 0,
        IsActive           BIT                   NOT NULL CONSTRAINT DF_InvoiceTemplateCategory_IsActive DEFAULT 1,
        CreatedBy          BIGINT                NULL,
        CreatedAt          DATETIME              NOT NULL CONSTRAINT DF_InvoiceTemplateCategory_CreatedAt DEFAULT GETDATE(),
        ModifiedBy         BIGINT                NULL,
        ModifiedAt         DATETIME              NULL
    );

    CREATE INDEX IX_InvoiceTemplateCategory_IsActive ON dbo.InvoiceTemplateCategory (IsActive);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.InvoiceTemplateCategory WHERE Code = 'GENERAL')
BEGIN
    INSERT INTO dbo.InvoiceTemplateCategory (Code, Name, Description, DisplayOrder) VALUES
        ('GENERAL',  'General',  'Generic layout usable across document types', 1),
        ('RETAIL',   'Retail',   'Retail store layout',                         2),
        ('WHOLESALE','Wholesale','Wholesale / distribution layout',             3),
        ('SERVICE',  'Service',  'Service delivery layout',                     4),
        ('POS',      'POS',      'Point-of-sale thermal layout',                5),
        ('EXPORT',   'Export',   'Export documentation layout',                 6),
        ('GST',      'GST',      'GST / statutory compliance layout',           7);
END
;

/* =============================================================================
   06. InvoiceTemplateComponent - drag/drop building blocks
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateComponent]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateComponent (
        ComponentId    BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateComponent PRIMARY KEY,
        Code           VARCHAR(50)           NOT NULL CONSTRAINT UQ_InvoiceTemplateComponent_Code UNIQUE,
        Name           VARCHAR(100)          NOT NULL,
        ComponentType  VARCHAR(50)           NOT NULL,
        Description    VARCHAR(300)          NULL,
        DisplayOrder   INT                   NOT NULL CONSTRAINT DF_InvoiceTemplateComponent_DisplayOrder DEFAULT 0,
        IsActive       BIT                   NOT NULL CONSTRAINT DF_InvoiceTemplateComponent_IsActive DEFAULT 1,
        CreatedBy      BIGINT                NULL,
        CreatedAt      DATETIME              NOT NULL CONSTRAINT DF_InvoiceTemplateComponent_CreatedAt DEFAULT GETDATE(),
        ModifiedBy     BIGINT                NULL,
        ModifiedAt     DATETIME              NULL
    );

    CREATE INDEX IX_InvoiceTemplateComponent_ComponentType ON dbo.InvoiceTemplateComponent (ComponentType);
    CREATE INDEX IX_InvoiceTemplateComponent_IsActive ON dbo.InvoiceTemplateComponent (IsActive);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.InvoiceTemplateComponent WHERE Code = 'LOGO')
BEGIN
    INSERT INTO dbo.InvoiceTemplateComponent (Code, Name, ComponentType, Description, DisplayOrder) VALUES
        ('LOGO',            'Logo',            'IMAGE',     'Company logo image',                  1),
        ('COMPANY_HEADER',  'Company Header',  'TEXT',      'Company letter-head header',          2),
        ('COMPANY_NAME',    'Company Name',    'TEXT',      'Company name line',                   3),
        ('COMPANY_ADDRESS', 'Company Address', 'TEXT',      'Company address block',               4),
        ('COMPANY_CONTACT', 'Company Contact', 'TEXT',      'Phone / email line',                  5),
        ('GSTIN',           'GSTIN',           'TEXT',      'Company GSTIN / PAN line',            6),
        ('INVOICE_INFO',    'Invoice Info',    'TEXT',      'Invoice number / date / due date',    7),
        ('CUSTOMER',        'Customer',        'TEXT',      'Customer info block',                 8),
        ('CUSTOMER_ADDRESS','Customer Address','TEXT',      'Customer address block',              9),
        ('BILLING_ADDRESS', 'Billing Address', 'TEXT',      'Billing address block',               10),
        ('SHIPPING_ADDRESS','Shipping Address','TEXT',      'Shipping address block',              11),
        ('ITEM_TABLE',      'Item Table',      'TABLE',     'Line-item detail table',              12),
        ('DISCOUNT',        'Discount',        'TEXT',      'Discount line(s)',                    13),
        ('TAX',             'Tax',             'TEXT',      'Tax details',                         14),
        ('TAX_SUMMARY',     'Tax Summary',     'TABLE',     'Tax rate-wise summary table',         15),
        ('SUBTOTAL',        'Subtotal',        'TEXT',      'Subtotal line',                       16),
        ('TOTAL',           'Total',           'TEXT',      'Grand total line',                    17),
        ('PAYMENT',         'Payment',         'TEXT',      'Payment details',                     18),
        ('BANK_DETAILS',    'Bank Details',    'TEXT',      'Company bank account details',        19),
        ('QR_CODE',         'QR Code',         'QRCODE',    'QR code image block',                 20),
        ('BARCODE',         'Barcode',         'BARCODE',   'Barcode image block',                 21),
        ('TERMS',           'Terms',           'TEXT',      'Terms & conditions text',             22),
        ('NOTES',           'Notes',           'TEXT',      'Free-form notes',                     23),
        ('SIGNATURE',       'Signature',       'IMAGE',     'Signature image / line',              24),
        ('FOOTER',          'Footer',          'TEXT',      'Page footer text',                    25),
        ('CUSTOM_TEXT',     'Custom Text',     'TEXT',      'Custom static text',                  26),
        ('CUSTOM_IMAGE',    'Custom Image',    'IMAGE',     'Custom image block',                  27),
        ('DIVIDER',         'Divider',         'DIVIDER',   'Horizontal rule',                     28);
END
;

/* =============================================================================
   07. InvoiceTemplateVariable - data binding path dictionary
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateVariable]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateVariable (
        VariableId   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateVariable PRIMARY KEY,
        Code         VARCHAR(100)          NOT NULL CONSTRAINT UQ_InvoiceTemplateVariable_Code UNIQUE,
        Name         VARCHAR(150)          NOT NULL,
        BindingPath  VARCHAR(300)          NOT NULL,
        DataType     VARCHAR(30)           NOT NULL,
        Category     VARCHAR(50)           NULL,
        Description  VARCHAR(300)          NULL,
        IsCollection BIT                   NOT NULL CONSTRAINT DF_InvoiceTemplateVariable_IsCollection DEFAULT 0,
        IsActive     BIT                   NOT NULL CONSTRAINT DF_InvoiceTemplateVariable_IsActive DEFAULT 1,
        CreatedBy    BIGINT                NULL,
        CreatedAt    DATETIME              NOT NULL CONSTRAINT DF_InvoiceTemplateVariable_CreatedAt DEFAULT GETDATE(),
        ModifiedBy   BIGINT                NULL,
        ModifiedAt   DATETIME              NULL
    );

    CREATE INDEX IX_InvoiceTemplateVariable_Category ON dbo.InvoiceTemplateVariable (Category);
    CREATE INDEX IX_InvoiceTemplateVariable_IsActive ON dbo.InvoiceTemplateVariable (IsActive);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.InvoiceTemplateVariable WHERE BindingPath = 'Company.Name')
BEGIN
    INSERT INTO dbo.InvoiceTemplateVariable (Code, Name, BindingPath, DataType, Category, Description, IsCollection) VALUES
        ('Company.Name',            'Company Name',            'Company.Name',            'STRING', 'Company', 'Legal display name of the company',               0),
        ('Company.LegalName',       'Company Legal Name',      'Company.LegalName',       'STRING', 'Company', 'Registered legal name',                                 0),
        ('Company.Address',         'Company Address',         'Company.Address',         'STRING', 'Company', 'Registered address line',                               0),
        ('Company.GSTIN',           'Company GSTIN',           'Company.GSTIN',           'STRING', 'Company', 'GST identification number',                            0),
        ('Company.PAN',             'Company PAN',             'Company.PAN',             'STRING', 'Company', 'Permanent account number',                              0),
        ('Invoice.Number',          'Invoice Number',          'Invoice.Number',          'STRING', 'Invoice', 'Document number',                                      0),
        ('Invoice.Date',            'Invoice Date',            'Invoice.Date',            'DATETIME','Invoice', 'Document date',                                         0),
        ('Invoice.DueDate',         'Due Date',                'Invoice.DueDate',         'DATETIME','Invoice', 'Payment due date',                                      0),
        ('Invoice.Type',            'Invoice Type',            'Invoice.Type',            'STRING', 'Invoice', 'Document type label',                                   0),
        ('Invoice.SubTotal',        'Subtotal',                'Invoice.SubTotal',        'DECIMAL', 'Invoice', 'Subtotal amount',                                       0),
        ('Invoice.Discount',        'Discount',                'Invoice.Discount',        'DECIMAL', 'Invoice', 'Total discount amount',                                 0),
        ('Invoice.Tax',             'Tax',                     'Invoice.Tax',             'DECIMAL', 'Invoice', 'Total tax amount',                                      0),
        ('Invoice.GrandTotal',      'Grand Total',             'Invoice.GrandTotal',      'DECIMAL', 'Invoice', 'Grand total (amount due)',                              0),
        ('Customer.Name',           'Customer Name',           'Customer.Name',           'STRING', 'Customer', 'Bill-to customer name',                                 0),
        ('Customer.Address',        'Customer Address',        'Customer.Address',        'STRING', 'Customer', 'Bill-to address',                                       0),
        ('Customer.GSTIN',          'Customer GSTIN',          'Customer.GSTIN',          'STRING', 'Customer', 'Customer GST identification number',                   0),
        ('Payment.Method',          'Payment Method',          'Payment.Method',          'STRING', 'Payment',  'Payment method label',                                    0),
        ('Payment.Amount',          'Payment Amount',          'Payment.Amount',          'DECIMAL', 'Payment',  'Amount paid',                                           0),
        ('Item.ProductName',        'Product Name',            'Item.ProductName',        'STRING', 'Item',     'Product / service description',                          0),
        ('Item.SKU',                'SKU',                     'Item.SKU',                'STRING', 'Item',     'Stock keeping unit code',                                0),
        ('Item.HSNSAC',             'HSN / SAC',               'Item.HSNSAC',             'STRING', 'Item',     'HSN / SAC code',                                        0),
        ('Item.Unit',               'Unit',                    'Item.Unit',               'STRING', 'Item',     'Unit of measure',                                       0),
        ('Item.Quantity',           'Quantity',                'Item.Quantity',           'DECIMAL', 'Item',     'Quantity',                                              0),
        ('Item.Rate',               'Rate',                    'Item.Rate',               'DECIMAL', 'Item',     'Unit rate',                                             0),
        ('Item.Discount',           'Item Discount',           'Item.Discount',           'DECIMAL', 'Item',     'Line discount amount',                                  0),
        ('Item.Tax',                'Item Tax',                'Item.Tax',                'DECIMAL', 'Item',     'Line tax amount',                                       0),
        ('Item.Amount',             'Item Amount',             'Item.Amount',             'DECIMAL', 'Item',     'Line amount',                                           0);
END
;

/* =============================================================================
   08. InvoiceFont - seeded font choices (FontFileId reserved for font files)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceFont]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceFont (
        FontId       BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceFont PRIMARY KEY,
        Code         VARCHAR(50)           NOT NULL CONSTRAINT UQ_InvoiceFont_Code UNIQUE,
        Name         VARCHAR(100)          NOT NULL,
        FontFamily   VARCHAR(100)          NOT NULL,
        FontFileId   BIGINT                NULL,
        IsActive     BIT                   NOT NULL CONSTRAINT DF_InvoiceFont_IsActive DEFAULT 1,
        CreatedBy    BIGINT                NULL,
        CreatedAt    DATETIME              NOT NULL CONSTRAINT DF_InvoiceFont_CreatedAt DEFAULT GETDATE(),
        ModifiedBy   BIGINT                NULL,
        ModifiedAt   DATETIME              NULL
    );

    CREATE INDEX IX_InvoiceFont_IsActive ON dbo.InvoiceFont (IsActive);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.InvoiceFont WHERE Code = 'ARIAL')
BEGIN
    INSERT INTO dbo.InvoiceFont (Code, Name, FontFamily) VALUES
        ('ARIAL',       'Arial',       'Arial'),
        ('ROBOTO',      'Roboto',      'Roboto'),
        ('INTER',       'Inter',       'Inter'),
        ('TAHOMA',      'Tahoma',      'Tahoma'),
        ('COURIER_NEW', 'Courier New', 'Courier New');
END
;

/* =============================================================================
   09. PrintOrientation
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PrintOrientation]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PrintOrientation (
        OrientationId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PrintOrientation PRIMARY KEY,
        Code          VARCHAR(20)           NOT NULL CONSTRAINT UQ_PrintOrientation_Code UNIQUE,
        Name          VARCHAR(50)           NOT NULL,
        IsActive      BIT                   NOT NULL CONSTRAINT DF_PrintOrientation_IsActive DEFAULT 1
    );
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.PrintOrientation WHERE Code = 'PORTRAIT')
BEGIN
    INSERT INTO dbo.PrintOrientation (Code, Name) VALUES
        ('PORTRAIT', 'Portrait'),
        ('LANDSCAPE','Landscape');
END
;

/* =============================================================================
   10. PrintUnit
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PrintUnit]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PrintUnit (
        UnitId   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PrintUnit PRIMARY KEY,
        Code     VARCHAR(20)           NOT NULL CONSTRAINT UQ_PrintUnit_Code UNIQUE,
        Name     VARCHAR(50)           NOT NULL,
        IsActive BIT                   NOT NULL CONSTRAINT DF_PrintUnit_IsActive DEFAULT 1
    );
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.PrintUnit WHERE Code = 'MM')
BEGIN
    INSERT INTO dbo.PrintUnit (Code, Name) VALUES
        ('MM',   'Millimeter'),
        ('PX',   'Pixel'),
        ('INCH', 'Inch');
END
;

/* =============================================================================
   11. InvoiceTemplate - the template header
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplate]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplate (
        InvoiceTemplateId   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplate PRIMARY KEY,
        CompanyId           INT                  NULL,
        TemplateCategoryId  BIGINT               NULL,
        InvoiceTypeId       BIGINT               NOT NULL,
        PaperSizeId         BIGINT               NOT NULL,
        OrientationId       BIGINT               NOT NULL,
        Code                VARCHAR(50)          NOT NULL,
        Name                VARCHAR(150)         NOT NULL,
        Description         VARCHAR(500)         NULL,
        Width               DECIMAL(10,2)        NULL,
        Height              DECIMAL(10,2)        NULL,
        IsDefault           BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplate_IsDefault DEFAULT 0,
        IsActive            BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplate_IsActive DEFAULT 1,
        CreatedBy           BIGINT               NULL,
        CreatedAt           DATETIME             NOT NULL CONSTRAINT DF_InvoiceTemplate_CreatedAt DEFAULT GETDATE(),
        ModifiedBy          BIGINT               NULL,
        ModifiedAt          DATETIME             NULL,
        CONSTRAINT FK_InvoiceTemplate_InvoiceType FOREIGN KEY (InvoiceTypeId) REFERENCES dbo.InvoiceType (InvoiceTypeId),
        CONSTRAINT FK_InvoiceTemplate_PaperSize FOREIGN KEY (PaperSizeId) REFERENCES dbo.InvoicePaperSize (PaperSizeId),
        CONSTRAINT FK_InvoiceTemplate_Orientation FOREIGN KEY (OrientationId) REFERENCES dbo.PrintOrientation (OrientationId),
        CONSTRAINT FK_InvoiceTemplate_Category FOREIGN KEY (TemplateCategoryId) REFERENCES dbo.InvoiceTemplateCategory (TemplateCategoryId),
        CONSTRAINT UQ_InvoiceTemplate_Company_Code UNIQUE (CompanyId, Code)
    );

    CREATE INDEX IX_InvoiceTemplate_CompanyId ON dbo.InvoiceTemplate (CompanyId);
    CREATE INDEX IX_InvoiceTemplate_InvoiceTypeId ON dbo.InvoiceTemplate (InvoiceTypeId);
    CREATE INDEX IX_InvoiceTemplate_PaperSizeId ON dbo.InvoiceTemplate (PaperSizeId);
    CREATE INDEX IX_InvoiceTemplate_IsActive ON dbo.InvoiceTemplate (IsActive);
END
;

/* =============================================================================
   12. InvoiceTemplateAssignment - decides which template is used at runtime
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateAssignment]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateAssignment (
        AssignmentId       BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateAssignment PRIMARY KEY,
        InvoiceTemplateId  BIGINT               NOT NULL,
        CompanyId          INT                  NULL,
        IndustryTypeId     INT                  NULL,
        InvoiceTypeId      BIGINT               NULL,
        PaperSizeId        BIGINT               NULL,
        IsDefault          BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateAssignment_IsDefault DEFAULT 0,
        IsActive           BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateAssignment_IsActive DEFAULT 1,
        CreatedBy          BIGINT               NULL,
        CreatedAt          DATETIME             NOT NULL CONSTRAINT DF_InvoiceTemplateAssignment_CreatedAt DEFAULT GETDATE(),
        ModifiedBy         BIGINT               NULL,
        ModifiedAt         DATETIME             NULL,
        CONSTRAINT FK_InvoiceTemplateAssignment_Template FOREIGN KEY (InvoiceTemplateId) REFERENCES dbo.InvoiceTemplate (InvoiceTemplateId),
        CONSTRAINT FK_InvoiceTemplateAssignment_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Companies (Id),
        CONSTRAINT FK_InvoiceTemplateAssignment_IndustryType FOREIGN KEY (IndustryTypeId) REFERENCES dbo.IndustryTypes (IndustryTypeId),
        CONSTRAINT FK_InvoiceTemplateAssignment_InvoiceType FOREIGN KEY (InvoiceTypeId) REFERENCES dbo.InvoiceType (InvoiceTypeId),
        CONSTRAINT FK_InvoiceTemplateAssignment_PaperSize FOREIGN KEY (PaperSizeId) REFERENCES dbo.InvoicePaperSize (PaperSizeId)
    );

    CREATE INDEX IX_InvoiceTemplateAssignment_TemplateId ON dbo.InvoiceTemplateAssignment (InvoiceTemplateId);
    CREATE INDEX IX_InvoiceTemplateAssignment_Lookup ON dbo.InvoiceTemplateAssignment (CompanyId, IndustryTypeId, InvoiceTypeId, PaperSizeId);
END
;

/* =============================================================================
   13. InvoiceTemplateVersion - template lifecycle (DRAFT -> PUBLISHED -> ARCHIVED)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateVersion]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateVersion (
        TemplateVersionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateVersion PRIMARY KEY,
        InvoiceTemplateId BIGINT               NOT NULL,
        VersionNumber     INT                  NOT NULL,
        TemplateJson      NVARCHAR(MAX)        NOT NULL,
        Status            VARCHAR(20)          NOT NULL CONSTRAINT DF_InvoiceTemplateVersion_Status DEFAULT 'DRAFT',
        IsPublished       BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateVersion_IsPublished DEFAULT 0,
        CreatedBy         BIGINT               NULL,
        CreatedAt         DATETIME             NOT NULL CONSTRAINT DF_InvoiceTemplateVersion_CreatedAt DEFAULT GETDATE(),
        PublishedBy       BIGINT               NULL,
        PublishedAt       DATETIME             NULL,
        CONSTRAINT FK_InvoiceTemplateVersion_Template FOREIGN KEY (InvoiceTemplateId) REFERENCES dbo.InvoiceTemplate (InvoiceTemplateId),
        CONSTRAINT UQ_InvoiceTemplateVersion_Template_Number UNIQUE (InvoiceTemplateId, VersionNumber)
    );

    CREATE INDEX IX_InvoiceTemplateVersion_TemplateId ON dbo.InvoiceTemplateVersion (InvoiceTemplateId);
    CREATE INDEX IX_InvoiceTemplateVersion_IsPublished ON dbo.InvoiceTemplateVersion (IsPublished);
END
;

/* =============================================================================
   14. InvoiceTemplateSection - region of the canvas inside a version
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateSection]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateSection (
        SectionId          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateSection PRIMARY KEY,
        TemplateVersionId  BIGINT               NOT NULL,
        SectionCode        VARCHAR(50)          NOT NULL,
        SectionName        VARCHAR(100)         NOT NULL,
        DisplayOrder       INT                  NOT NULL CONSTRAINT DF_InvoiceTemplateSection_DisplayOrder DEFAULT 0,
        X                  DECIMAL(10,2)        NULL,
        Y                  DECIMAL(10,2)        NULL,
        Width              DECIMAL(10,2)        NULL,
        Height             DECIMAL(10,2)        NULL,
        IsVisible          BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateSection_IsVisible DEFAULT 1,
        CONSTRAINT FK_InvoiceTemplateSection_Version FOREIGN KEY (TemplateVersionId) REFERENCES dbo.InvoiceTemplateVersion (TemplateVersionId)
    );

    CREATE INDEX IX_InvoiceTemplateSection_VersionId ON dbo.InvoiceTemplateSection (TemplateVersionId);
END
;

/* =============================================================================
   15. InvoiceTemplateElement - one drag/dropped component instance
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateElement]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateElement (
        ElementId      BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateElement PRIMARY KEY,
        SectionId      BIGINT               NOT NULL,
        ComponentId    BIGINT               NOT NULL,
        ElementType    VARCHAR(50)          NOT NULL,
        ElementName    VARCHAR(100)         NULL,
        X              DECIMAL(10,2)        NULL,
        Y              DECIMAL(10,2)        NULL,
        Width          DECIMAL(10,2)        NULL,
        Height         DECIMAL(10,2)        NULL,
        DisplayOrder   INT                  NOT NULL CONSTRAINT DF_InvoiceTemplateElement_DisplayOrder DEFAULT 0,
        IsVisible      BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateElement_IsVisible DEFAULT 1,
        CONSTRAINT FK_InvoiceTemplateElement_Section FOREIGN KEY (SectionId) REFERENCES dbo.InvoiceTemplateSection (SectionId),
        CONSTRAINT FK_InvoiceTemplateElement_Component FOREIGN KEY (ComponentId) REFERENCES dbo.InvoiceTemplateComponent (ComponentId)
    );

    CREATE INDEX IX_InvoiceTemplateElement_SectionId ON dbo.InvoiceTemplateElement (SectionId);
    CREATE INDEX IX_InvoiceTemplateElement_ComponentId ON dbo.InvoiceTemplateElement (ComponentId);
END
;

/* =============================================================================
   16. InvoiceTemplateField - data binding (element -> variable)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateField]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateField (
        TemplateFieldId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateField PRIMARY KEY,
        ElementId       BIGINT               NOT NULL,
        VariableId      BIGINT               NOT NULL,
        FieldName       VARCHAR(100)         NOT NULL,
        BindingPath     VARCHAR(300)         NOT NULL,
        Label           VARCHAR(100)         NULL,
        IsVisible       BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateField_IsVisible DEFAULT 1,
        CONSTRAINT FK_InvoiceTemplateField_Element FOREIGN KEY (ElementId) REFERENCES dbo.InvoiceTemplateElement (ElementId),
        CONSTRAINT FK_InvoiceTemplateField_Variable FOREIGN KEY (VariableId) REFERENCES dbo.InvoiceTemplateVariable (VariableId)
    );

    CREATE INDEX IX_InvoiceTemplateField_ElementId ON dbo.InvoiceTemplateField (ElementId);
    CREATE INDEX IX_InvoiceTemplateField_VariableId ON dbo.InvoiceTemplateField (VariableId);
END
;

/* =============================================================================
   17. InvoiceTemplateItemColumn - item table column definitions
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateItemColumn]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateItemColumn (
        ItemColumnId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateItemColumn PRIMARY KEY,
        ElementId    BIGINT               NOT NULL,
        FieldName    VARCHAR(100)         NOT NULL,
        HeaderText   VARCHAR(100)         NOT NULL,
        DisplayOrder INT                  NOT NULL,
        Width        DECIMAL(10,2)        NULL,
        Alignment    VARCHAR(20)          NOT NULL CONSTRAINT DF_InvoiceTemplateItemColumn_Alignment DEFAULT 'LEFT',
        IsVisible    BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateItemColumn_IsVisible DEFAULT 1,
        CONSTRAINT FK_InvoiceTemplateItemColumn_Element FOREIGN KEY (ElementId) REFERENCES dbo.InvoiceTemplateElement (ElementId)
    );

    CREATE INDEX IX_InvoiceTemplateItemColumn_ElementId ON dbo.InvoiceTemplateItemColumn (ElementId);
END
;

/* =============================================================================
   18. InvoiceTemplateStyle - visual style for one element
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateStyle]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateStyle (
        StyleId        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateStyle PRIMARY KEY,
        ElementId      BIGINT               NOT NULL,
        FontId         BIGINT               NULL,
        FontSize       DECIMAL(6,2)         NULL,
        FontWeight     VARCHAR(20)          NULL,
        TextAlign      VARCHAR(20)          NULL,
        VerticalAlign  VARCHAR(20)          NULL,
        PaddingTop     DECIMAL(8,2)         NULL,
        PaddingRight   DECIMAL(8,2)         NULL,
        PaddingBottom  DECIMAL(8,2)         NULL,
        PaddingLeft    DECIMAL(8,2)         NULL,
        BorderTop      BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateStyle_BorderTop DEFAULT 0,
        BorderRight    BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateStyle_BorderRight DEFAULT 0,
        BorderBottom   BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateStyle_BorderBottom DEFAULT 0,
        BorderLeft     BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateStyle_BorderLeft DEFAULT 0,
        CONSTRAINT FK_InvoiceTemplateStyle_Element FOREIGN KEY (ElementId) REFERENCES dbo.InvoiceTemplateElement (ElementId),
        CONSTRAINT FK_InvoiceTemplateStyle_Font FOREIGN KEY (FontId) REFERENCES dbo.InvoiceFont (FontId),
        CONSTRAINT UQ_InvoiceTemplateStyle_Element UNIQUE (ElementId)
    );

    CREATE INDEX IX_InvoiceTemplateStyle_FontId ON dbo.InvoiceTemplateStyle (FontId);
END
;

/* =============================================================================
   19. InvoiceTemplatePrinter - printer binding for a template
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplatePrinter]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplatePrinter (
        TemplatePrinterId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplatePrinter PRIMARY KEY,
        InvoiceTemplateId BIGINT               NOT NULL,
        PaperSizeId       BIGINT               NOT NULL,
        PrinterTypeId     BIGINT               NOT NULL,
        PrinterModelId    BIGINT               NULL,
        PrinterName       VARCHAR(200)         NULL,
        IsDefault         BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplatePrinter_IsDefault DEFAULT 0,
        IsActive          BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplatePrinter_IsActive DEFAULT 1,
        CreatedBy         BIGINT               NULL,
        CreatedAt         DATETIME             NOT NULL CONSTRAINT DF_InvoiceTemplatePrinter_CreatedAt DEFAULT GETDATE(),
        ModifiedBy        BIGINT               NULL,
        ModifiedAt        DATETIME             NULL,
        CONSTRAINT FK_InvoiceTemplatePrinter_Template FOREIGN KEY (InvoiceTemplateId) REFERENCES dbo.InvoiceTemplate (InvoiceTemplateId),
        CONSTRAINT FK_InvoiceTemplatePrinter_PaperSize FOREIGN KEY (PaperSizeId) REFERENCES dbo.InvoicePaperSize (PaperSizeId),
        CONSTRAINT FK_InvoiceTemplatePrinter_PrinterType FOREIGN KEY (PrinterTypeId) REFERENCES dbo.PrinterType (PrinterTypeId),
        CONSTRAINT FK_InvoiceTemplatePrinter_PrinterModel FOREIGN KEY (PrinterModelId) REFERENCES dbo.PrinterModel (PrinterModelId)
    );

    CREATE INDEX IX_InvoiceTemplatePrinter_TemplateId ON dbo.InvoiceTemplatePrinter (InvoiceTemplateId);
    CREATE INDEX IX_InvoiceTemplatePrinter_PaperSizeId ON dbo.InvoiceTemplatePrinter (PaperSizeId);
END
;

/* =============================================================================
   20. InvoiceTemplatePrintSetting - per-printer print defaults
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplatePrintSetting]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplatePrintSetting (
        PrintSettingId   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplatePrintSetting PRIMARY KEY,
        TemplatePrinterId BIGINT              NOT NULL,
        MarginTop        DECIMAL(10,2)        NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_MarginTop DEFAULT 0,
        MarginRight      DECIMAL(10,2)        NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_MarginRight DEFAULT 0,
        MarginBottom     DECIMAL(10,2)        NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_MarginBottom DEFAULT 0,
        MarginLeft       DECIMAL(10,2)        NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_MarginLeft DEFAULT 0,
        Scale            DECIMAL(6,2)         NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_Scale DEFAULT 100,
        Copies           INT                  NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_Copies DEFAULT 1,
        AutoFit          BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_AutoFit DEFAULT 1,
        CutPaper         BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_CutPaper DEFAULT 0,
        PrintHeader      BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_PrintHeader DEFAULT 1,
        PrintFooter      BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_PrintFooter DEFAULT 1,
        CreatedBy        BIGINT               NULL,
        CreatedAt        DATETIME             NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_CreatedAt DEFAULT GETDATE(),
        ModifiedBy       BIGINT               NULL,
        ModifiedAt       DATETIME             NULL,
        CONSTRAINT FK_InvoiceTemplatePrintSetting_Printer FOREIGN KEY (TemplatePrinterId) REFERENCES dbo.InvoiceTemplatePrinter (TemplatePrinterId)
    );

    CREATE INDEX IX_InvoiceTemplatePrintSetting_TemplatePrinterId ON dbo.InvoiceTemplatePrintSetting (TemplatePrinterId);
END
;

PRINT 'Invoice Template Design module added.';
