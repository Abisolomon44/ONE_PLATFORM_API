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

/* Domain: MASTER (SETUP) - retained for backwards compatibility */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'MASTER')
BEGIN
    DECLARE @DefaultWorkspaceId INT = (SELECT Id FROM dbo.Workspaces WHERE WorkspaceCode = 'SETUP');
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    VALUES (@DefaultWorkspaceId, 'MASTER', 'Master Data', 'database', 1, 1, 'system');
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'COMPANY')
BEGIN
    DECLARE @DefaultDomainId INT = (SELECT Id FROM dbo.Domains WHERE DomainCode = 'MASTER');
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    VALUES (@DefaultDomainId, 'COMPANY', 'Company', 'home', '/companies', 1, 1, 'system');
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'DEFAULT')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT m.Id, 'DEFAULT', m.ModuleName + ' (General)', m.Icon, m.RouteUrl, 0, 1, 'system'
    FROM dbo.Modules m
    WHERE m.IsActive = 1
      AND NOT EXISTS (SELECT 1 FROM dbo.SubModules sm WHERE sm.ModuleId = m.Id);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'COMPANY_LIST')
BEGIN
    DECLARE @DefaultSubModuleId INT = (SELECT Id FROM dbo.SubModules WHERE SubModuleCode = 'DEFAULT' AND ModuleId = (SELECT Id FROM dbo.Modules WHERE ModuleCode = 'COMPANY'));
    IF @DefaultSubModuleId IS NOT NULL
    BEGIN
        INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
        VALUES (@DefaultSubModuleId, 'COMPANY_LIST', 'Company List', 'MASTER', '/companies', 'CompanyListPage', 1, 1, 'system');
    END
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Fields WHERE FieldCode = 'company_code' AND ScreenId = (SELECT Id FROM dbo.Screens WHERE ScreenCode = 'COMPANY_LIST'))
BEGIN
    DECLARE @DefaultCompanyListScreenId INT = (SELECT Id FROM dbo.Screens WHERE ScreenCode = 'COMPANY_LIST');
    IF @DefaultCompanyListScreenId IS NOT NULL
    BEGIN
        INSERT INTO dbo.Fields (ScreenId, FieldCode, FieldName, DisplayName, DataType, DisplayOrder, IsSystemField, IsRequired, IsActive, CreatedBy)
        VALUES
        (@DefaultCompanyListScreenId, 'company_code', 'CompanyCode', 'Company Code', 'text', 1, 1, 1, 1, 'system'),
        (@DefaultCompanyListScreenId, 'company_name', 'CompanyName', 'Company Name', 'text', 2, 1, 1, 1, 'system'),
        (@DefaultCompanyListScreenId, 'short_name',   'ShortName',   'Short Name',   'text', 3, 0, 0, 1, 'system'),
        (@DefaultCompanyListScreenId, 'email',        'Email',       'Email',        'email', 4, 0, 0, 1, 'system'),
        (@DefaultCompanyListScreenId, 'phone',        'Phone',       'Phone',        'text',  5, 0, 0, 1, 'system'),
        (@DefaultCompanyListScreenId, 'is_active',    'IsActive',    'Active',       'boolean', 6, 1, 0, 1, 'system');
    END
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
    ('INR', 'Indian Rupee',         'â‚¹',  '356', 2, 1, 1),
    ('USD', 'US Dollar',            '$',  '840', 2, 0, 2),
    ('EUR', 'Euro',                 'â‚¬',  '978', 2, 0, 3),
    ('GBP', 'Pound Sterling',        'Â£',  '826', 2, 0, 4),
    ('JPY', 'Japanese Yen',         'Â¥',  '392', 0, 0, 5),
    ('CNY', 'Yuan Renminbi',        'Â¥',  '156', 2, 0, 6),
    ('CHF', 'Swiss Franc',          'CHF', '756', 2, 0, 7),
    ('AUD', 'Australian Dollar',    'A$', '036', 2, 0, 8),
    ('CAD', 'Canadian Dollar',      'C$', '124', 2, 0, 9),
    ('NZD', 'New Zealand Dollar',   'NZ$', '554', 2, 0, 10),
    ('KRW', 'South Korean Won',    'â‚©',  '410', 0, 0, 11),
    ('SGD', 'Singapore Dollar',    'S$', '702', 2, 0, 12),
    ('MYR', 'Malaysian Ringgit',  'RM', '458', 2, 0, 13),
    ('THB', 'Thai Baht',           'à¸¿',  '764', 2, 0, 14),
    ('IDR', 'Indonesian Rupiah',  'Rp', '360', 2, 0, 15),
    ('PHP', 'Philippine Peso',    'â‚±',  '608', 2, 0, 16),
    ('VND', 'Vietnamese Dong',    'â‚«',  '704', 2, 0, 17),
    ('MXN', 'Mexican Peso',       '$',  '484', 2, 0, 18),
    ('AED', 'UAE Dirham',         'Ø¯.Ø¥', '784', 2, 0, 19),
    ('SAR', 'Saudi Riyal',        'ï·¼',  '682', 2, 0, 20),
    ('QAR', 'Qatari Riyal',       'ï·¼',  '634', 2, 0, 21),
    ('KWD', 'Kuwaiti Dinar',      'Ø¯.Ùƒ', '414', 3, 0, 22),
    ('BHD', 'Bahraini Dinar',     '.Ø¯.Ø¨', '048', 3, 0, 23),
    ('OMR', 'Omani Rial',         'ï·¼',  '512', 3, 0, 24),
    ('EGP', 'Egyptian Pound',     'Â£',  '818', 2, 0, 25),
    ('ZAR', 'South African Rand', 'R',  '710', 2, 0, 26),
    ('NGN', 'Nigerian Naira',    'â‚¦',  '566', 2, 0, 27),
    ('KES', 'Kenyan Shilling',   'KSh', '400', 2, 0, 28),
    ('TRY', 'Turkish Lira',      'â‚º',  '949', 2, 0, 29),
    ('RUB', 'Russian Ruble',     'â‚½',  '643', 2, 0, 30),
    ('BRL', 'Brazilian Real',    'R$', '986', 2, 0, 31),
    ('ARS', 'Argentine Peso',    '$',  '032', 2, 0, 32),
    ('BDT', 'Bangladeshi Taka', 'à§³',  '050', 2, 0, 33),
    ('LKR', 'Sri Lankan Rupee', 'Rs',  '144', 2, 0, 34),
    ('NPR', 'Nepalese Rupee',   'â‚¨',  '524', 2, 0, 35),
    ('PKR', 'Pakistani Rupee',  'â‚¨',  '586', 2, 0, 36);
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
    ('business-partner-roles.view'), ('business-partner-roles.manage'),
    ('business-partners.view'), ('business-partners.manage'),
    ('industry-types.view'), ('industry-types.manage'),
    ('company-groups.view'), ('company-groups.manage'),
    ('settings.view'), ('settings.edit'),
    ('audit.view'),
    ('currencies.view'), ('currencies.manage'),
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
    ('business-partner-roles.view'), ('business-partner-roles.manage'),
    ('business-partners.view'), ('business-partners.manage'),
    ('industry-types.view'), ('industry-types.manage'),
    ('company-groups.view'), ('company-groups.manage'),
    ('settings.view'), ('settings.edit'),
    ('currencies.view'), ('currencies.manage'),
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
    ('business-partner-roles.view'), ('business-partner-roles.manage'),
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

        CONSTRAINT UQ_Purchase_No UNIQUE (CompanyId, PurchaseNumber)
    );

    CREATE INDEX IX_Purchase_Company_Date ON dbo.Purchase (CompanyId, PurchaseDate);
    CREATE INDEX IX_Purchase_Supplier ON dbo.Purchase (SupplierId);
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
        Remarks            NVARCHAR(500) NULL
    );

    CREATE INDEX IX_PurchaseItem_PurchaseId ON dbo.PurchaseItem (PurchaseId);
    CREATE INDEX IX_PurchaseItem_ProductId ON dbo.PurchaseItem (ProductId);
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

        CONSTRAINT UQ_PurchaseReturn_No UNIQUE (CompanyId, ReturnNumber)
    );

    CREATE INDEX IX_PurchaseReturn_Company_Date ON dbo.PurchaseReturn (CompanyId, ReturnDate);
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
        LineTotal            DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseReturnItem_Line DEFAULT 0
    );

    CREATE INDEX IX_PurchaseReturnItem_ReturnId ON dbo.PurchaseReturnItem (PurchaseReturnId);
    CREATE INDEX IX_PurchaseReturnItem_ProductId ON dbo.PurchaseReturnItem (ProductId);
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
    ('BUSINESS_MASTER_HOME', 'Business Master', 'MASTER', '/business-master', 'BusinessMasterPage', 1, 1, 'system'),
    ('COMPANY_MASTER',       'Company',         'MASTER', '/company',         'CompanyPage',        2, 1, 'system'),
    ('BRANCH_MASTER',        'Branch',          'MASTER', '/branch',          'BranchPage',         3, 1, 'system'),
    ('DEPARTMENT_MASTER',    'Department',      'MASTER', '/department',      'Department',         4, 1, 'system'),
    ('WAREHOUSE_MASTER',     'Warehouse',       'MASTER', '/warehouse',       'Warehouse',          5, 1, 'system'),
    ('DESIGNATION_MASTER',   'Designation',     'MASTER', '/designation',     'Designation',        6, 1, 'system')
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
    ('SALES_HOME',   'Sales',       'LIST',   '/sales',       'SalesWorkspace', 1, 1, 'system'),
    ('SALES_ENTRY',  'Sales Entry', 'ENTRY',  '/sales-entry', 'SalesEntryPage', 2, 1, 'system')
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
    ('PURCHASE_HOME',        'Purchase',         'LIST',        '/purchase',               'PurchaseWorkspace',    1, 1, 'system'),
    ('PURCHASE_ENTRY',       'Purchase Entry',   'ENTRY',       '/purchase-entry',         'PurchaseEntryPage',    2, 1, 'system'),
    ('PURCHASE_LIST',        'Purchases',        'LIST',        '/purchase?tab=list',      'PurchaseListPage',     3, 1, 'system'),
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

/* Payment Configuration > Payment Master */
IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-PAYCONFIG')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-PAYCONFIG', 'Payment Configuration', 'settings', '/payment', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-PAYMENT';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-PAYMASTER')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-PAYMASTER', 'Payment Master', 'sliders-horizontal', '/payment', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-PAYCONFIG';
END
;

INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, k.ScreenCode, k.ScreenName, k.ScreenType, k.RouteUrl, k.ComponentName, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('PAYMENT_TYPES',   'Payment Types',   'MASTER', '/payment-type',   'PaymentTypePage',   1, 1, 'system'),
    ('PAYMENT_METHODS', 'Payment Methods', 'MASTER', '/payment-method', 'PaymentMethodPage', 2, 1, 'system')
) AS k(ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
INNER JOIN dbo.SubModules sm ON sm.SubModuleCode = 'SUB-PAYMASTER'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Screens s WHERE s.SubModuleId = sm.Id AND s.ScreenCode = k.ScreenCode
);
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

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-PARTNERCONFIG')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-PARTNERCONFIG', 'Partner Configuration', 'sliders-horizontal', '/business-partner-roles', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-PARTNERMGMT';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'BUSINESS_PARTNER_ROLES')
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'BUSINESS_PARTNER_ROLES', 'Business Partner Roles', 'MASTER', '/business-partner-roles', 'BusinessPartnerRolesPage', 1, 1, 'system'
    FROM dbo.SubModules WHERE SubModuleCode = 'SUB-PARTNERCONFIG';
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

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-ROLEPERMS')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-ROLEPERMS', 'Role Permissions', 'shield-check', '/role-permissions', 1, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-ROLESEC';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-ROLEFIELD')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-ROLEFIELD', 'Role Field Permissions', 'field-has-legend', '/role-field-permissions', 2, 1, 'system'
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
    SELECT Id, 'DOM-USERSEC', 'User Security', 'user-shield', 3, 1, 'system'
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
    SELECT Id, 'MOD-DATASCOPE', 'Data Scope', 'filter', '/data-scopes', 1, 1, 'system'
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

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-LEGACYPERMS')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-LEGACYPERMS', 'Permission Modules', 'layers', '/permission-modules', 2, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-PERMMGMT';
END
;

/* Add the permission management submodules + screens (attached to the
   ENTERPRISE_PERMISSIONS workspace domains/modules created above) */

/* Role Security > Role Permissions > Role Permission Management */
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-ROLEPERMMGMT')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-ROLEPERMMGMT', 'Role Permission Management', 'shield-check', '/role-permissions', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-ROLEPERMS';
END
;

/* Role Security > Role Field Permissions > Field Security */
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-ROLEFIELDSEC')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-ROLEFIELDSEC', 'Field Security', 'field-has-legend', '/role-field-permissions', 1, 1, 'system'
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
    SELECT Id, 'SUB-SCOPEMGMT', 'Scope Management', 'filter', '/data-scopes', 1, 1, 'system'
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

/* Permission Management > Legacy Permission Modules */
IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'MOD-LEGACYACTIONS')
BEGIN
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'MOD-LEGACYACTIONS', 'Permission Actions', 'zap', '/permission-actions', 3, 1, 'system'
    FROM dbo.Domains WHERE DomainCode = 'DOM-PERMMGMT';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-LEGACYPERMS')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-LEGACYPERMS', 'Permission Modules', 'layers', '/permission-modules', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-LEGACYPERMS';
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-LEGACYACTIONS')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT Id, 'SUB-LEGACYACTIONS', 'Permission Actions', 'zap', '/permission-actions', 1, 1, 'system'
    FROM dbo.Modules WHERE ModuleCode = 'MOD-LEGACYACTIONS';
END
;

/* Attach screens to their ENTERPRISE_PERMISSIONS submodules */
INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, k.ScreenCode, k.ScreenName, k.ScreenType, k.RouteUrl, k.ComponentName, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('SUB-ROLEPERMMGMT',  'ROLE_PERMISSIONS',         'Role Permissions',         'SETTINGS', '/role-permissions',           'RolePermissionsPage',          1, 1, 'system'),
    ('SUB-ROLEFIELDSEC',  'ROLE_FIELD_PERMISSIONS',   'Role Field Permissions',   'SETTINGS', '/role-field-permissions',     'RoleFieldPermissionsPage',     1, 1, 'system'),
    ('SUB-PERMMATRIX',    'ROLE_PERMISSION_MATRIX',   'Role Permission Matrix',   'SETTINGS', '/role-permission-matrix',     'RolePermissionMatrixPage',     1, 1, 'system'),
    ('SUB-PERMOVERRIDES', 'USER_PERMISSION_OVERRIDES','User Permission Overrides','SETTINGS', '/user-permission-overrides',  'UserPermissionOverridesPage',  1, 1, 'system'),
    ('SUB-SCOPEMGMT',     'DATA_SCOPES',              'Data Scopes',              'SETTINGS', '/data-scopes',                'DataScopesPage',               1, 1, 'system'),
    ('SUB-USERSCOPE',     'USER_DATA_SCOPE_OVERRIDES','User Data Scope Overrides','SETTINGS', '/user-data-scope-overrides',  'UserDataScopeOverridesPage',   1, 1, 'system'),
    ('SUB-WORKFLOWPERMS', 'WORKFLOW_PERMISSIONS',     'Workflow Permissions',     'SETTINGS', '/workflow-permissions',       'WorkflowPermissionsPage',      1, 1, 'system'),
    ('SUB-ENTPERMS',      'ENTERPRISE_PERMISSIONS',   'Enterprise Permissions',   'SETTINGS', '/enterprise-permissions',     'EnterprisePermissionsPage',    1, 1, 'system'),
    ('SUB-LEGACYPERMS',   'PERMISSION_MODULES',       'Permission Modules',       'SETTINGS', '/permission-modules',         'PermissionModulesPage',        1, 1, 'system'),
    ('SUB-LEGACYACTIONS', 'PERMISSION_ACTIONS',       'Permission Actions',       'SETTINGS', '/permission-actions',         'PermissionActionsPage',        1, 1, 'system')
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
    ('business-master.view'), ('business-master.manage'),
    ('company.view'), ('company.create'), ('company.edit'), ('company.delete'),
    ('branch.view'), ('branch.create'), ('branch.edit'), ('branch.delete'),
    ('department.view'), ('department.create'), ('department.edit'),
    ('warehouse.view'), ('warehouse.create'), ('warehouse.edit'),
    ('designation.view'), ('designation.create'), ('designation.edit'),
    ('financial-year.view'), ('financial-year.manage'),
    ('system-master.view'), ('system-master.manage'),
    ('tenant-configuration.view'), ('tenant-configuration.manage'),
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
    ('purchase.view'), ('purchase.manage'), ('purchase.create'), ('purchase.edit'), ('purchase.delete'),
    ('purchase-returns.view'), ('purchase-returns.manage'),
    /* INVENTORY */
    ('stock.view'), ('stock.manage'),
    /* PAYMENTS */
    ('payment-types.view'), ('payment-types.manage'),
    ('payment-methods.view'), ('payment-methods.manage'),
    ('payment.view'), ('payment.create'), ('payment.edit'), ('payment.delete'),
    /* REPORTS */
    ('reports.view'),
    /* BUSINESS_PARTNERS */
    ('business-partner-roles.view'), ('business-partner-roles.manage'),
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
    ('role-permissions.view'), ('role-permissions.manage'),
    ('role-field-permissions.view'), ('role-field-permissions.manage'),
    ('role-permission-matrix.view'), ('role-permission-matrix.manage'),
    ('user-permission-overrides.view'), ('user-permission-overrides.manage'),
    ('data-scopes.view'), ('data-scopes.manage'),
    ('user-data-scope-overrides.view'), ('user-data-scope-overrides.manage'),
    ('workflow-permissions.view'), ('workflow-permissions.manage'),
    ('enterprise-permissions.view'), ('enterprise-permissions.manage'),
    ('permission-modules.view'), ('permission-modules.manage'),
    ('permission-actions.view'), ('permission-actions.manage'),
    /* SETTINGS */
    ('settings.view'), ('settings.edit')
) AS v(code)
WHERE r.Code IN ('SuperAdmin', 'Administrator')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissionsLegacy rp WHERE rp.RoleId = r.RoleId AND rp.PermissionCode = v.code);

PRINT 'ERP module configuration seed complete.';
