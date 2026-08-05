/* =============================================================================
   ONE ERP - Migration: Update Companies table to enriched schema
   Purpose  : Evolves the Companies table from the legacy flat schema
              (CompanyId, Address, Email, Phone, GST, Currency, Status)
              to the new enriched schema (Id, ShortName, Abbreviation,
              BusinessTypeId, IndustryTypeId, GSTRegistrationTypeId, GSTNumber,
              PANNumber, TANNumber, CINNumber, RegistrationNumber, CurrencyId,
              LanguageId, TimeZoneId, IsActive, IsBlocked, LastLoginDate,
              CreatedBy INT, ModifiedBy INT).
   Idempotent: Uses IF COL_LENGTH / IF OBJECT_ID checks so it is safe to re-run.
   Single-batch safe: All UPDATE/EXEC statements that reference dynamically-
              created columns use EXEC() so SQL Server resolves column metadata
              at execution time, not compile time.
   ============================================================================= */

-- 1. Rename PK column CompanyId -> Id (if not already done)
IF COL_LENGTH('dbo.Companies', 'Id') IS NULL AND COL_LENGTH('dbo.Companies', 'CompanyId') IS NOT NULL
BEGIN
    -- Drop FK from Users that references Companies(CompanyId) before touching PK
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Company' AND parent_object_id = OBJECT_ID('dbo.Users'))
        ALTER TABLE dbo.Users DROP CONSTRAINT FK_Users_Company;

    -- Rename PK column CompanyId -> Id
    EXEC sp_rename 'dbo.Companies.CompanyId', 'Id';
    EXEC sp_rename 'PK_Companies', 'PK_Companies_Old';
    ALTER TABLE dbo.Companies DROP CONSTRAINT PK_Companies_Old;
    ALTER TABLE dbo.Companies ADD CONSTRAINT PK_Companies PRIMARY KEY (Id);
END

-- 2. Add new columns (only if they do not already exist)
IF COL_LENGTH('dbo.Companies', 'ShortName') IS NULL
    ALTER TABLE dbo.Companies ADD ShortName NVARCHAR(100) NULL;

IF COL_LENGTH('dbo.Companies', 'Abbreviation') IS NULL
    ALTER TABLE dbo.Companies ADD Abbreviation NVARCHAR(20) NULL;

IF COL_LENGTH('dbo.Companies', 'BusinessTypeId') IS NULL
    ALTER TABLE dbo.Companies ADD BusinessTypeId INT NOT NULL CONSTRAINT DF_Companies_BusinessTypeId DEFAULT 1;

IF COL_LENGTH('dbo.Companies', 'IndustryTypeId') IS NULL
    ALTER TABLE dbo.Companies ADD IndustryTypeId INT NOT NULL CONSTRAINT DF_Companies_IndustryTypeId DEFAULT 1;

IF COL_LENGTH('dbo.Companies', 'GSTRegistrationTypeId') IS NULL
    ALTER TABLE dbo.Companies ADD GSTRegistrationTypeId INT NULL;

IF COL_LENGTH('dbo.Companies', 'GSTNumber') IS NULL
    ALTER TABLE dbo.Companies ADD GSTNumber NVARCHAR(20) NULL;

IF COL_LENGTH('dbo.Companies', 'PANNumber') IS NULL
    ALTER TABLE dbo.Companies ADD PANNumber NVARCHAR(20) NULL;

IF COL_LENGTH('dbo.Companies', 'TANNumber') IS NULL
    ALTER TABLE dbo.Companies ADD TANNumber NVARCHAR(20) NULL;

IF COL_LENGTH('dbo.Companies', 'CINNumber') IS NULL
    ALTER TABLE dbo.Companies ADD CINNumber NVARCHAR(30) NULL;

IF COL_LENGTH('dbo.Companies', 'RegistrationNumber') IS NULL
    ALTER TABLE dbo.Companies ADD RegistrationNumber NVARCHAR(100) NULL;

IF COL_LENGTH('dbo.Companies', 'CurrencyId') IS NULL
    ALTER TABLE dbo.Companies ADD CurrencyId INT NOT NULL CONSTRAINT DF_Companies_CurrencyId DEFAULT 1;

IF COL_LENGTH('dbo.Companies', 'LanguageId') IS NULL
    ALTER TABLE dbo.Companies ADD LanguageId INT NOT NULL CONSTRAINT DF_Companies_LanguageId DEFAULT 1;

IF COL_LENGTH('dbo.Companies', 'TimeZoneId') IS NULL
    ALTER TABLE dbo.Companies ADD TimeZoneId INT NOT NULL CONSTRAINT DF_Companies_TimeZoneId DEFAULT 1;

IF COL_LENGTH('dbo.Companies', 'IsActive') IS NULL
    ALTER TABLE dbo.Companies ADD IsActive BIT NOT NULL CONSTRAINT DF_Companies_IsActive DEFAULT 1;

IF COL_LENGTH('dbo.Companies', 'IsBlocked') IS NULL
    ALTER TABLE dbo.Companies ADD IsBlocked BIT NOT NULL CONSTRAINT DF_Companies_IsBlocked DEFAULT 0;

IF COL_LENGTH('dbo.Companies', 'LastLoginDate') IS NULL
    ALTER TABLE dbo.Companies ADD LastLoginDate DATETIME2 NULL;

-- 3. Data migration (wrapped in EXEC for single-batch compatibility)
--    Each UPDATE checks that the referenced master table exists.

--    Set IsActive based on old Status (only if both columns exist)
IF COL_LENGTH('dbo.Companies', 'Status') IS NOT NULL AND COL_LENGTH('dbo.Companies', 'IsActive') IS NOT NULL
    EXEC('UPDATE dbo.Companies SET IsActive = CASE WHEN Status = ''Active'' THEN 1 ELSE 0 END WHERE IsActive = 0');

--    Map old Currency code string -> Currencies.Id (only if Currencies table exists)
IF COL_LENGTH('dbo.Companies', 'Currency') IS NOT NULL AND COL_LENGTH('dbo.Companies', 'CurrencyId') IS NOT NULL
    AND OBJECT_ID('dbo.Currencies') IS NOT NULL
    EXEC('UPDATE c SET CurrencyId = cur.Id FROM dbo.Companies c INNER JOIN dbo.Currencies cur ON cur.CurrencyCode = c.Currency WHERE c.CurrencyId = 1 AND c.Currency IS NOT NULL');

--    Set default LanguageId (only if Languages table exists)
IF COL_LENGTH('dbo.Companies', 'LanguageId') IS NOT NULL AND OBJECT_ID('dbo.Languages') IS NOT NULL
    EXEC('UPDATE c SET LanguageId = lang.LanguageId FROM dbo.Companies c CROSS JOIN (SELECT TOP 1 LanguageId FROM dbo.Languages WHERE IsDefault = 1) lang WHERE c.LanguageId = 1');

--    Set default TimeZoneId (only if TimeZones table exists)
IF COL_LENGTH('dbo.Companies', 'TimeZoneId') IS NOT NULL AND OBJECT_ID('dbo.TimeZones') IS NOT NULL
    EXEC('UPDATE c SET TimeZoneId = tz.TimeZoneId FROM dbo.Companies c CROSS JOIN (SELECT TOP 1 TimeZoneId FROM dbo.TimeZones ORDER BY TimeZoneId) tz WHERE c.TimeZoneId = 1');

--    Set default BusinessTypeId from master table (only if BusinessTypes table exists)
IF COL_LENGTH('dbo.Companies', 'BusinessTypeId') IS NOT NULL AND OBJECT_ID('dbo.BusinessTypes') IS NOT NULL
    EXEC('UPDATE c SET BusinessTypeId = bt.BusinessTypeId FROM dbo.Companies c CROSS JOIN (SELECT TOP 1 BusinessTypeId FROM dbo.BusinessTypes ORDER BY BusinessTypeId) bt');

--    Set default IndustryTypeId from master table (only if IndustryTypes table exists)
IF COL_LENGTH('dbo.Companies', 'IndustryTypeId') IS NOT NULL AND OBJECT_ID('dbo.IndustryTypes') IS NOT NULL
    EXEC('UPDATE c SET IndustryTypeId = it.IndustryTypeId FROM dbo.Companies c CROSS JOIN (SELECT TOP 1 IndustryTypeId FROM dbo.IndustryTypes ORDER BY IndustryTypeId) it');

--    Map old GST -> GSTNumber (both columns on Companies)
IF COL_LENGTH('dbo.Companies', 'GST') IS NOT NULL AND COL_LENGTH('dbo.Companies', 'GSTNumber') IS NOT NULL
    EXEC('UPDATE dbo.Companies SET GSTNumber = GST WHERE GSTNumber IS NULL AND GST IS NOT NULL');

-- 4. Drop DEFAULT constraints and legacy columns (only if they still exist)
DECLARE @sql NVARCHAR(MAX);

IF COL_LENGTH('dbo.Companies', 'Address') IS NOT NULL
BEGIN
    SET @sql = 'ALTER TABLE dbo.Companies DROP CONSTRAINT IF EXISTS DF_Companies_Address; ALTER TABLE dbo.Companies DROP COLUMN [Address]';
    EXEC(@sql);
END

IF COL_LENGTH('dbo.Companies', 'Email') IS NOT NULL
BEGIN
    SET @sql = 'ALTER TABLE dbo.Companies DROP CONSTRAINT IF EXISTS DF_Companies_Email; ALTER TABLE dbo.Companies DROP COLUMN Email';
    EXEC(@sql);
END

IF COL_LENGTH('dbo.Companies', 'Phone') IS NOT NULL
BEGIN
    SET @sql = 'ALTER TABLE dbo.Companies DROP CONSTRAINT IF EXISTS DF_Companies_Phone; ALTER TABLE dbo.Companies DROP COLUMN Phone';
    EXEC(@sql);
END

IF COL_LENGTH('dbo.Companies', 'GST') IS NOT NULL
BEGIN
    SET @sql = 'ALTER TABLE dbo.Companies DROP CONSTRAINT IF EXISTS DF_Companies_GST; ALTER TABLE dbo.Companies DROP COLUMN GST';
    EXEC(@sql);
END

IF COL_LENGTH('dbo.Companies', 'Currency') IS NOT NULL
BEGIN
    SET @sql = 'ALTER TABLE dbo.Companies DROP CONSTRAINT IF EXISTS DF_Companies_Currency; ALTER TABLE dbo.Companies DROP COLUMN Currency';
    EXEC(@sql);
END

IF COL_LENGTH('dbo.Companies', 'Status') IS NOT NULL
BEGIN
    -- Drop index that depends on Status before dropping the column
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Companies_Status' AND object_id = OBJECT_ID('dbo.Companies'))
        DROP INDEX IX_Companies_Status ON dbo.Companies;
    SET @sql = 'ALTER TABLE dbo.Companies DROP CONSTRAINT IF EXISTS DF_Companies_Status; ALTER TABLE dbo.Companies DROP COLUMN Status';
    EXEC(@sql);
END

-- 5. Convert CreatedBy from NVARCHAR(100) to INT
IF COL_LENGTH('dbo.Companies', 'CreatedBy') IS NOT NULL AND COL_LENGTH('dbo.Companies', 'CreatedByOld') IS NULL
BEGIN
    EXEC sp_rename 'dbo.Companies.CreatedBy', 'CreatedByOld';
    ALTER TABLE dbo.Companies ADD CreatedBy INT NOT NULL CONSTRAINT DF_Companies_CreatedBy DEFAULT 0;
    EXEC('UPDATE dbo.Companies SET CreatedBy = 0 WHERE CreatedByOld IS NOT NULL');
    ALTER TABLE dbo.Companies DROP COLUMN CreatedByOld;
END

-- 6. Convert ModifiedBy from NVARCHAR(100) to INT (nullable)
IF COL_LENGTH('dbo.Companies', 'ModifiedBy') IS NOT NULL AND COL_LENGTH('dbo.Companies', 'ModifiedByOld') IS NULL
BEGIN
    EXEC sp_rename 'dbo.Companies.ModifiedBy', 'ModifiedByOld';
    ALTER TABLE dbo.Companies ADD ModifiedBy INT NULL;
    ALTER TABLE dbo.Companies DROP COLUMN ModifiedByOld;
END

-- 7. Drop old index on Status (column no longer exists)
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Companies_Status' AND object_id = OBJECT_ID('dbo.Companies'))
    DROP INDEX IX_Companies_Status ON dbo.Companies;

-- 8. Add indexes on new columns
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Companies_IsActive' AND object_id = OBJECT_ID('dbo.Companies'))
    CREATE INDEX IX_Companies_IsActive ON dbo.Companies (IsActive);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Companies_BusinessTypeId' AND object_id = OBJECT_ID('dbo.Companies'))
    CREATE INDEX IX_Companies_BusinessTypeId ON dbo.Companies (BusinessTypeId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Companies_IndustryTypeId' AND object_id = OBJECT_ID('dbo.Companies'))
    CREATE INDEX IX_Companies_IndustryTypeId ON dbo.Companies (IndustryTypeId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Companies_CurrencyId' AND object_id = OBJECT_ID('dbo.Companies'))
    CREATE INDEX IX_Companies_CurrencyId ON dbo.Companies (CurrencyId);

-- 9. Add FK constraints (only if the referenced master table exists AND has data)
IF OBJECT_ID('dbo.BusinessTypes') IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.BusinessTypes)
    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Companies_BusinessType' AND parent_object_id = OBJECT_ID('dbo.Companies'))
    EXEC('ALTER TABLE dbo.Companies ADD CONSTRAINT FK_Companies_BusinessType FOREIGN KEY (BusinessTypeId) REFERENCES dbo.BusinessTypes (BusinessTypeId)');

IF OBJECT_ID('dbo.IndustryTypes') IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.IndustryTypes)
    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Companies_IndustryType' AND parent_object_id = OBJECT_ID('dbo.Companies'))
    EXEC('ALTER TABLE dbo.Companies ADD CONSTRAINT FK_Companies_IndustryType FOREIGN KEY (IndustryTypeId) REFERENCES dbo.IndustryTypes (IndustryTypeId)');

IF OBJECT_ID('dbo.GSTRegistrationTypes') IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.GSTRegistrationTypes)
    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Companies_GSTRegistrationType' AND parent_object_id = OBJECT_ID('dbo.Companies'))
    EXEC('ALTER TABLE dbo.Companies ADD CONSTRAINT FK_Companies_GSTRegistrationType FOREIGN KEY (GSTRegistrationTypeId) REFERENCES dbo.GSTRegistrationTypes (GSTRegistrationTypeId)');

IF OBJECT_ID('dbo.Currencies') IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.Currencies)
    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Companies_Currency' AND parent_object_id = OBJECT_ID('dbo.Companies'))
    EXEC('ALTER TABLE dbo.Companies ADD CONSTRAINT FK_Companies_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.Currencies (Id)');

IF OBJECT_ID('dbo.Languages') IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.Languages)
    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Companies_Language' AND parent_object_id = OBJECT_ID('dbo.Companies'))
    EXEC('ALTER TABLE dbo.Companies ADD CONSTRAINT FK_Companies_Language FOREIGN KEY (LanguageId) REFERENCES dbo.Languages (LanguageId)');

IF OBJECT_ID('dbo.TimeZones') IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.TimeZones)
    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Companies_TimeZone' AND parent_object_id = OBJECT_ID('dbo.Companies'))
    EXEC('ALTER TABLE dbo.Companies ADD CONSTRAINT FK_Companies_TimeZone FOREIGN KEY (TimeZoneId) REFERENCES dbo.TimeZones (TimeZoneId)');

-- 10. Update Users FK to reference Companies(Id) instead of Companies(CompanyId)
IF COL_LENGTH('dbo.Companies', 'Id') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Company' AND parent_object_id = OBJECT_ID('dbo.Users'))
    BEGIN
        ALTER TABLE dbo.Users DROP CONSTRAINT FK_Users_Company;
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Company' AND parent_object_id = OBJECT_ID('dbo.Users'))
        ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Companies (Id);
END
;