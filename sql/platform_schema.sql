/* =============================================================================
   ONE ERP - Platform Database Schema
   Server   : LAPTOP-BOR8IKB8\MSSQL2022
   Database : ONEERP_PLATFORM
   Purpose  : Multi-tenant platform catalog (tenants, plans, subscriptions, etc.)
   Run once during setup with Windows Authentication.
   ============================================================================= */

IF DB_ID('ONEERP_PLATFORM') IS NULL
BEGIN
    CREATE DATABASE [ONEERP_PLATFORM];
END
GO

USE [ONEERP_PLATFORM]
GO

/* ---------------------------------------------------------------------------
   Plans
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plans]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Plans (
        PlanId         INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Plans PRIMARY KEY,
        PlanCode       NVARCHAR(50)        NOT NULL CONSTRAINT UQ_Plans_PlanCode UNIQUE,
        PlanName       NVARCHAR(100)       NOT NULL,
        [Description]  NVARCHAR(500)       NULL,
        CountryCode    NVARCHAR(50)        NOT NULL CONSTRAINT DF_Plans_CountryCode DEFAULT 'US',
        CountryName    NVARCHAR(100)       NOT NULL CONSTRAINT DF_Plans_CountryName DEFAULT 'United States',
        CurrencyCode   NVARCHAR(10)        NOT NULL CONSTRAINT DF_Plans_CurrencyCode DEFAULT 'USD',
        MonthlyPrice   DECIMAL(18,2)       NOT NULL CONSTRAINT DF_Plans_MonthlyPrice DEFAULT 0,
        AnnualPrice    DECIMAL(18,2)       NOT NULL CONSTRAINT DF_Plans_AnnualPrice DEFAULT 0,
        MaxUsers       INT                 NOT NULL CONSTRAINT DF_Plans_MaxUsers DEFAULT 5,
        MaxCompanies   INT                 NOT NULL CONSTRAINT DF_Plans_MaxCompanies DEFAULT 1,
        IsActive       BIT                 NOT NULL CONSTRAINT DF_Plans_IsActive DEFAULT 1,
        CreatedBy      NVARCHAR(100)       NULL,
        CreatedDate    DATETIME2           NOT NULL CONSTRAINT DF_Plans_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy     NVARCHAR(100)       NULL,
        ModifiedDate   DATETIME2           NOT NULL CONSTRAINT DF_Plans_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted      BIT                 NOT NULL CONSTRAINT DF_Plans_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_Plans_IsActive ON dbo.Plans (IsActive) INCLUDE (PlanName, MonthlyPrice);
END
GO

/* ---------------------------------------------------------------------------
   Plans migration (country/currency + MaxCompanies, drop MaxStorageGB)
   Safe to re-run against an already-provisioned database.
---------------------------------------------------------------------------- */
IF COL_LENGTH('dbo.Plans', 'CountryCode') IS NULL
    ALTER TABLE dbo.Plans ADD CountryCode NVARCHAR(50) NOT NULL CONSTRAINT DF_Plans_CountryCode DEFAULT 'US';
GO
IF COL_LENGTH('dbo.Plans', 'CountryName') IS NULL
    ALTER TABLE dbo.Plans ADD CountryName NVARCHAR(100) NOT NULL CONSTRAINT DF_Plans_CountryName DEFAULT 'United States';
GO
IF COL_LENGTH('dbo.Plans', 'CurrencyCode') IS NULL
    ALTER TABLE dbo.Plans ADD CurrencyCode NVARCHAR(10) NOT NULL CONSTRAINT DF_Plans_CurrencyCode DEFAULT 'USD';
GO
IF COL_LENGTH('dbo.Plans', 'MaxCompanies') IS NULL
    ALTER TABLE dbo.Plans ADD MaxCompanies INT NOT NULL CONSTRAINT DF_Plans_MaxCompanies DEFAULT 1;
GO
IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Plans_MaxStorageGB' AND parent_object_id = OBJECT_ID('dbo.Plans'))
    ALTER TABLE dbo.Plans DROP CONSTRAINT DF_Plans_MaxStorageGB;
GO
IF COL_LENGTH('dbo.Plans', 'MaxStorageGB') IS NOT NULL
    ALTER TABLE dbo.Plans DROP COLUMN MaxStorageGB;
GO

/* ---------------------------------------------------------------------------
   Tenants
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tenants]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Tenants (
        TenantId       INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Tenants PRIMARY KEY,
        TenantCode     NVARCHAR(50)        NOT NULL CONSTRAINT UQ_Tenants_TenantCode UNIQUE,
        TenantName     NVARCHAR(200)       NOT NULL,
        CompanyName    NVARCHAR(200)       NULL,
        DatabaseName   NVARCHAR(128)       NOT NULL CONSTRAINT UQ_Tenants_DatabaseName UNIQUE,
        PlanId         INT                 NULL,
        ContactEmail   NVARCHAR(200)       NULL,
        AdminUsername  NVARCHAR(100)       NULL,
        AdminPassword  NVARCHAR(200)       NULL,
        Status         NVARCHAR(20)        NOT NULL CONSTRAINT DF_Tenants_Status DEFAULT 'Active',
        CreatedBy      NVARCHAR(100)       NULL,
        CreatedDate    DATETIME2           NOT NULL CONSTRAINT DF_Tenants_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy     NVARCHAR(100)       NULL,
        ModifiedDate   DATETIME2           NOT NULL CONSTRAINT DF_Tenants_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted      BIT                 NOT NULL CONSTRAINT DF_Tenants_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_Tenants_Status ON dbo.Tenants (Status);
END
GO

/* ---------------------------------------------------------------------------
   Tenants migration (CompanyName + AdminUsername)
   Safe to re-run against an already-provisioned database.
   Existing tenant admin users are renamed to 'admin_<lowercasetenantcode>'
   so every tenant admin username is globally unique (needed for username-only
   ERP login). Backfilled username lookup happens via the tenant database Users
   table only when the tenant database still exists.
---------------------------------------------------------------------------- */
IF COL_LENGTH('dbo.Tenants', 'CompanyName') IS NULL
    ALTER TABLE dbo.Tenants ADD CompanyName NVARCHAR(200) NULL;
GO
IF COL_LENGTH('dbo.Tenants', 'AdminUsername') IS NULL
    ALTER TABLE dbo.Tenants ADD AdminUsername NVARCHAR(100) NULL;
GO
IF COL_LENGTH('dbo.Tenants', 'AdminPassword') IS NULL
    ALTER TABLE dbo.Tenants ADD AdminPassword NVARCHAR(200) NULL;
GO

UPDATE dbo.Tenants SET CompanyName = TenantName WHERE CompanyName IS NULL AND IsDeleted = 0;
GO

DECLARE @tc NVARCHAR(50), @db NVARCHAR(128), @nu NVARCHAR(100), @sql NVARCHAR(MAX);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT TenantCode, DatabaseName FROM dbo.Tenants
    WHERE IsDeleted = 0 AND (AdminUsername IS NULL OR AdminUsername = '');
OPEN cur;
FETCH NEXT FROM cur INTO @tc, @db;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @nu = 'admin_' + LOWER(REPLACE(REPLACE(@tc, '-', '_'), '.', '_'));
    UPDATE dbo.Tenants SET AdminUsername = @nu WHERE TenantCode = @tc;
    IF DB_ID(@db) IS NOT NULL
    BEGIN
        SET @sql = 'IF OBJECT_ID(''[' + @db + '].dbo.Users'', ''U'') IS NOT NULL
                        UPDATE [' + @db + '].dbo.Users SET Username = ''' + @nu + '''
                        WHERE Username = ''admin'' AND IsDeleted = 0;';
        EXEC sp_executesql @sql;
    END
    FETCH NEXT FROM cur INTO @tc, @db;
END
CLOSE cur;
DEALLOCATE cur;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Tenants_AdminUsername' AND object_id = OBJECT_ID('dbo.Tenants'))
    CREATE UNIQUE INDEX UQ_Tenants_AdminUsername ON dbo.Tenants (AdminUsername)
        WHERE AdminUsername IS NOT NULL AND AdminUsername <> '';
GO

/* ---------------------------------------------------------------------------
   Subscriptions
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Subscriptions (
        SubscriptionId INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Subscriptions PRIMARY KEY,
        TenantId       INT                 NOT NULL,
        PlanId         INT                 NOT NULL,
        StartDate      DATETIME2           NOT NULL,
        EndDate        DATETIME2           NOT NULL,
        Amount         DECIMAL(18,2)       NOT NULL CONSTRAINT DF_Subscriptions_Amount DEFAULT 0,
        Status         NVARCHAR(20)        NOT NULL CONSTRAINT DF_Subscriptions_Status DEFAULT 'Active',
        CreatedBy      NVARCHAR(100)       NULL,
        CreatedDate    DATETIME2           NOT NULL CONSTRAINT DF_Subscriptions_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy     NVARCHAR(100)       NULL,
        ModifiedDate   DATETIME2           NOT NULL CONSTRAINT DF_Subscriptions_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted      BIT                 NOT NULL CONSTRAINT DF_Subscriptions_IsDeleted DEFAULT 0,
        CONSTRAINT FK_Subscriptions_Tenant FOREIGN KEY (TenantId) REFERENCES dbo.Tenants (TenantId),
        CONSTRAINT FK_Subscriptions_Plan   FOREIGN KEY (PlanId)   REFERENCES dbo.Plans (PlanId)
    );

    CREATE INDEX IX_Subscriptions_TenantId ON dbo.Subscriptions (TenantId) INCLUDE (PlanId, EndDate, Status);
    CREATE INDEX IX_Subscriptions_Status   ON dbo.Subscriptions (Status) INCLUDE (EndDate);
END
GO

/* ---------------------------------------------------------------------------
   PlatformUsers
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PlatformUsers]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PlatformUsers (
        PlatformUserId INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_PlatformUsers PRIMARY KEY,
        Username       NVARCHAR(100)       NOT NULL CONSTRAINT UQ_PlatformUsers_Username UNIQUE,
        Email          NVARCHAR(200)       NULL,
        FullName       NVARCHAR(200)       NOT NULL,
        PasswordHash   NVARCHAR(255)       NOT NULL,
        Role           NVARCHAR(50)        NOT NULL CONSTRAINT DF_PlatformUsers_Role DEFAULT 'PlatformAdmin',
        IsActive       BIT                 NOT NULL CONSTRAINT DF_PlatformUsers_IsActive DEFAULT 1,
        LastLoginDate  DATETIME2           NULL,
        CreatedBy      NVARCHAR(100)       NULL,
        CreatedDate    DATETIME2           NOT NULL CONSTRAINT DF_PlatformUsers_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy     NVARCHAR(100)       NULL,
        ModifiedDate   DATETIME2           NOT NULL CONSTRAINT DF_PlatformUsers_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted      BIT                 NOT NULL CONSTRAINT DF_PlatformUsers_IsDeleted DEFAULT 0
    );

    CREATE INDEX IX_PlatformUsers_IsActive ON dbo.PlatformUsers (IsActive);
END
GO

/* ---------------------------------------------------------------------------
   TenantConnections
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TenantConnections]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.TenantConnections (
        ConnectionId     INT IDENTITY(1,1)  NOT NULL CONSTRAINT PK_TenantConnections PRIMARY KEY,
        TenantId         INT                NOT NULL,
        ServerName       NVARCHAR(200)      NOT NULL,
        DatabaseName     NVARCHAR(128)      NOT NULL,
        ConnectionString NVARCHAR(1000)     NOT NULL,
        IsActive         BIT                NOT NULL CONSTRAINT DF_TenantConnections_IsActive DEFAULT 1,
        CreatedBy        NVARCHAR(100)      NULL,
        CreatedDate      DATETIME2          NOT NULL CONSTRAINT DF_TenantConnections_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_TenantConnections_Tenant FOREIGN KEY (TenantId) REFERENCES dbo.Tenants (TenantId)
    );

    CREATE INDEX IX_TenantConnections_TenantId ON dbo.TenantConnections (TenantId) INCLUDE (ConnectionString, IsActive);
END
GO

/* ---------------------------------------------------------------------------
   RefreshTokens (platform sessions)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RefreshTokens]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.RefreshTokens (
        RefreshTokenId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Platform_RefreshTokens PRIMARY KEY,
        PlatformUserId INT                  NOT NULL,
        Token          NVARCHAR(500)        NOT NULL,
        ExpiryDate     DATETIME2            NOT NULL,
        IsRevoked      BIT                  NOT NULL CONSTRAINT DF_Platform_RefreshTokens_IsRevoked DEFAULT 0,
        CreatedDate    DATETIME2            NOT NULL CONSTRAINT DF_Platform_RefreshTokens_CreatedDate DEFAULT SYSUTCDATETIME(),
        RevokedDate    DATETIME2            NULL,
        CONSTRAINT FK_RefreshTokens_PlatformUser FOREIGN KEY (PlatformUserId) REFERENCES dbo.PlatformUsers (PlatformUserId)
    );

    CREATE INDEX IX_Platform_RefreshTokens_Token ON dbo.RefreshTokens (Token);
    CREATE INDEX IX_Platform_RefreshTokens_UserId ON dbo.RefreshTokens (PlatformUserId);
END
GO

/* ---------------------------------------------------------------------------
   AuditLogs
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.AuditLogs (
        AuditLogId   BIGINT IDENTITY(1,1)  NOT NULL CONSTRAINT PK_Platform_AuditLogs PRIMARY KEY,
        TenantId     INT                   NULL,
        EntityName   NVARCHAR(100)         NOT NULL,
        EntityId     NVARCHAR(50)          NULL,
        [Action]     NVARCHAR(50)          NOT NULL,
        PerformedBy  NVARCHAR(100)         NULL,
        PerformedDate DATETIME2            NOT NULL CONSTRAINT DF_Platform_AuditLogs_PerformedDate DEFAULT SYSUTCDATETIME(),
        OldValues    NVARCHAR(MAX)         NULL,
        NewValues    NVARCHAR(MAX)         NULL,
        IpAddress    NVARCHAR(50)          NULL
    );

    CREATE INDEX IX_Platform_AuditLogs_TenantDate ON dbo.AuditLogs (TenantId, PerformedDate);
END
GO

/* ---------------------------------------------------------------------------
   Settings
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Settings]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Settings (
        SettingId    INT IDENTITY(1,1)    NOT NULL CONSTRAINT PK_Platform_Settings PRIMARY KEY,
        SettingKey   NVARCHAR(100)        NOT NULL CONSTRAINT UQ_Platform_Settings_Key UNIQUE,
        SettingValue NVARCHAR(500)        NULL,
        [Description] NVARCHAR(500)       NULL,
        UpdatedBy    NVARCHAR(100)        NULL,
        UpdatedDate  DATETIME2            NOT NULL CONSTRAINT DF_Platform_Settings_UpdatedDate DEFAULT SYSUTCDATETIME()
    );
END
GO

/* ---------------------------------------------------------------------------
   Seed Data
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.PlatformUsers WHERE Username = 'admin')
BEGIN
    INSERT INTO dbo.PlatformUsers (Username, Email, FullName, PasswordHash, Role, IsActive, CreatedBy)
    VALUES (
        'admin',
        'platform.admin@oneerp.com',
        'Platform Administrator',
        '$2a$11$D2emzGOJoJ4F7Bo4vJNcoOBLf9oORxLoTJQyVdo8jcAT0K5oMDGvy', -- PlatformAdmin@123
        'PlatformAdmin',
        1,
        'system'
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Plans WHERE PlanCode = 'BASIC')
BEGIN
    INSERT INTO dbo.Plans (PlanCode, PlanName, [Description], CountryCode, CountryName, CurrencyCode, MonthlyPrice, AnnualPrice, MaxUsers, MaxCompanies, IsActive, CreatedBy)
    VALUES ('BASIC', 'Basic', 'Entry level plan', 'US', 'United States', 'USD', 49.00, 490.00, 5, 1, 1, 'system');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Plans WHERE PlanCode = 'STANDARD')
BEGIN
    INSERT INTO dbo.Plans (PlanCode, PlanName, [Description], CountryCode, CountryName, CurrencyCode, MonthlyPrice, AnnualPrice, MaxUsers, MaxCompanies, IsActive, CreatedBy)
    VALUES ('STANDARD', 'Standard', 'Mid tier plan for growing businesses', 'US', 'United States', 'USD', 99.00, 990.00, 20, 3, 1, 'system');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Plans WHERE PlanCode = 'PREMIUM')
BEGIN
    INSERT INTO dbo.Plans (PlanCode, PlanName, [Description], CountryCode, CountryName, CurrencyCode, MonthlyPrice, AnnualPrice, MaxUsers, MaxCompanies, IsActive, CreatedBy)
    VALUES ('PREMIUM', 'Premium', 'Enterprise plan with unlimited scale', 'US', 'United States', 'USD', 199.00, 1990.00, 100, 10, 1, 'system');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Settings WHERE SettingKey = 'Platform.Name')
BEGIN
    INSERT INTO dbo.Settings (SettingKey, SettingValue, [Description], UpdatedBy)
    VALUES
        ('Platform.Name', 'ONE ERP', 'Platform display name', 'system'),
        ('Platform.Email', 'support@oneerp.com', 'Platform support email', 'system'),
        ('Platform.Currency', 'USD', 'Default platform currency', 'system');
END
GO

PRINT 'ONEERP_PLATFORM schema and seed data created successfully.'
GO
