/* ===========================================================================
   erp_migration_009_sources.sql
   Adds the Sources master (transaction channels) for existing tenant DBs.

   Safe to re-run: every statement is idempotent.
   =========================================================================== */

/* ---------------------------------------------------------------------------
   Table
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Sources]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Sources (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Sources PRIMARY KEY,
        Code            NVARCHAR(50)        NOT NULL CONSTRAINT UQ_Sources_Code UNIQUE,
        Name            NVARCHAR(100)       NOT NULL,
        Description     NVARCHAR(250)       NULL,
        SortOrder       INT                 NOT NULL CONSTRAINT DF_Sources_SortOrder DEFAULT 1,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Sources_IsActive DEFAULT 1,
        IsDeleted       BIT                 NOT NULL CONSTRAINT DF_Sources_IsDeleted DEFAULT 0,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_Sources_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100)       NULL,
        ModifiedDate    DATETIME2           NOT NULL CONSTRAINT DF_Sources_ModifiedDate DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_Sources_IsActive ON dbo.Sources (IsActive);
    CREATE INDEX IX_Sources_SortOrder ON dbo.Sources (SortOrder);
END
;

/* ---------------------------------------------------------------------------
   Seed data
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Sources)
BEGIN
    INSERT INTO dbo.Sources (Code, Name, Description, SortOrder, IsActive)
    VALUES
    ('SALES',   'Sales Entry',       'Normal sales entry transaction',                10, 1),
    ('POS',     'POS Billing',       'Point of sale billing transaction',             20, 1),
    ('ONLINE',  'Online',             'Online sales transaction',                      30, 1),
    ('IMPORT',  'Import',             'Imported transaction',                          40, 1),
    ('API',     'API',                'Transaction created through API integration',   50, 1),
    ('MOBILE',  'Mobile',             'Transaction created through mobile application', 60, 1);
END
;

/* ---------------------------------------------------------------------------
   Menu screen (Organization > Organization Setup > Sources)
   --------------------------------------------------------------------------- */
INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, k.ScreenCode, k.ScreenName, k.ScreenType, k.RouteUrl, k.ComponentName, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('SOURCE_MASTER', 'Sources', 'MASTER', '/sources', 'SourcesPage', 14, 1, 'system')
) AS k(ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
INNER JOIN dbo.SubModules sm ON sm.SubModuleCode = 'SUB-ORGSETUP'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Screens s WHERE s.SubModuleId = sm.Id AND s.ScreenCode = k.ScreenCode
);
;

/* ---------------------------------------------------------------------------
   Permissions
   --------------------------------------------------------------------------- */
INSERT INTO dbo.Permissions (Code)
SELECT k.Code
FROM (VALUES
    ('sources.view'), ('sources.manage')
) AS k(Code)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions p WHERE p.Code = k.Code);
;