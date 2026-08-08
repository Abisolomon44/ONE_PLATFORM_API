/* =============================================================================
   ONE ERP - SubModule Hierarchy Migration
   Version : 006
   Date    : 2026-08-07
   Purpose : Adds SubModule level between Module and Screen.
             Safe migration: preserves all existing data.
   ============================================================================= */

/* ---------------------------------------------------------------------------
   1. Create SubModules table
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
   2. Create default SubModule for each existing Module
      and link existing Screens to it
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'DEFAULT')
BEGIN
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    SELECT m.Id, 'DEFAULT', m.ModuleName + ' (General)', m.Icon, m.RouteUrl, 0, 1, 'system'
    FROM dbo.Modules m
    WHERE m.IsActive = 1
      AND NOT EXISTS (
          SELECT 1 FROM dbo.SubModules sm WHERE sm.ModuleId = m.Id
      );
END
;

/* ---------------------------------------------------------------------------
   3. Add SubModuleId to Screens (nullable first)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Screens]') AND name = 'SubModuleId')
BEGIN
    ALTER TABLE dbo.Screens ADD SubModuleId INT NULL;
END
;

/* ---------------------------------------------------------------------------
   4. Populate Screens.SubModuleId from default SubModules
--------------------------------------------------------------------------- */
UPDATE s
SET s.SubModuleId = sm.Id
FROM dbo.Screens s
INNER JOIN dbo.SubModules sm ON sm.ModuleId = s.ModuleId AND sm.SubModuleCode = 'DEFAULT'
WHERE s.SubModuleId IS NULL;

/* ---------------------------------------------------------------------------
   5. Make SubModuleId NOT NULL after population
--------------------------------------------------------------------------- */
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Screens]') AND name = 'SubModuleId' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.Screens ALTER COLUMN SubModuleId INT NOT NULL;
END
;

/* ---------------------------------------------------------------------------
   6. Drop old Screens.ModuleId FK and unique constraint if they exist
--------------------------------------------------------------------------- */
DECLARE @fkName NVARCHAR(200);
SELECT @fkName = fk.name
FROM sys.foreign_keys fk
INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
WHERE t.name = 'Screens' AND fk.name LIKE '%Module%';

IF @fkName IS NOT NULL
    EXEC('ALTER TABLE dbo.Screens DROP CONSTRAINT [' + @fkName + ']');

DECLARE @uqName NVARCHAR(200);
SELECT @uqName = kc.name
FROM sys.key_constraints kc
INNER JOIN sys.tables t ON kc.parent_object_id = t.object_id
WHERE t.name = 'Screens' AND kc.type = 'UQ' AND kc.name LIKE '%Code%';

IF @uqName IS NOT NULL
    EXEC('ALTER TABLE dbo.Screens DROP CONSTRAINT [' + @uqName + ']');

/* ---------------------------------------------------------------------------
   7. Add FK and unique constraint for SubModuleId on Screens
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Screens_SubModule')
BEGIN
    ALTER TABLE dbo.Screens
        ADD CONSTRAINT FK_Screens_SubModule FOREIGN KEY (SubModuleId) REFERENCES dbo.SubModules (Id);
END
;

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Screens_SubModule_Code')
BEGIN
    ALTER TABLE dbo.Screens
        ADD CONSTRAINT UQ_Screens_SubModule_Code UNIQUE (SubModuleId, ScreenCode);
END
;

CREATE INDEX IX_Screens_SubModuleId ON dbo.Screens (SubModuleId);

/* ---------------------------------------------------------------------------
   8. Drop old Screens.ModuleId column (after data is migrated)
--------------------------------------------------------------------------- */
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Screens]') AND name = 'ModuleId')
BEGIN
    ALTER TABLE dbo.Screens DROP COLUMN ModuleId;
END
;

/* ---------------------------------------------------------------------------
   9. Add SubModuleId to RolePermissions (nullable first)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND name = 'SubModuleId')
BEGIN
    ALTER TABLE dbo.RolePermissions ADD SubModuleId INT NULL;
END
;

/* ---------------------------------------------------------------------------
   10. Populate RolePermissions.SubModuleId
       For existing permissions, set to the default SubModule of the Module
--------------------------------------------------------------------------- */
UPDATE rp
SET rp.SubModuleId = sm.Id
FROM dbo.RolePermissions rp
INNER JOIN dbo.SubModules sm ON sm.ModuleId = rp.ModuleId AND sm.SubModuleCode = 'DEFAULT'
WHERE rp.SubModuleId IS NULL;

/* ---------------------------------------------------------------------------
   11. Make SubModuleId NOT NULL
--------------------------------------------------------------------------- */
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND name = 'SubModuleId' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.RolePermissions ALTER COLUMN SubModuleId INT NOT NULL;
END
;

/* ---------------------------------------------------------------------------
   12. Add FK for SubModuleId on RolePermissions
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RolePermissions_SubModule')
BEGIN
    ALTER TABLE dbo.RolePermissions
        ADD CONSTRAINT FK_RolePermissions_SubModule FOREIGN KEY (SubModuleId) REFERENCES dbo.SubModules (Id);
END
;

/* ---------------------------------------------------------------------------
   13. Drop old unique constraint on RolePermissions and recreate with SubModuleId
--------------------------------------------------------------------------- */
DECLARE @rpUqName NVARCHAR(200);
SELECT @rpUqName = kc.name
FROM sys.key_constraints kc
INNER JOIN sys.tables t ON kc.parent_object_id = t.object_id
WHERE t.name = 'RolePermissions' AND kc.type = 'UQ' AND kc.name LIKE '%Matrix%';

IF @rpUqName IS NOT NULL
    EXEC('ALTER TABLE dbo.RolePermissions DROP CONSTRAINT [' + @rpUqName + ']');

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_RolePermissions_Matrix')
BEGIN
    ALTER TABLE dbo.RolePermissions
        ADD CONSTRAINT UQ_RolePermissions_Matrix UNIQUE (RoleId, WorkspaceId, DomainId, ModuleId, SubModuleId, ScreenId, ActionId);
END
;

CREATE INDEX IX_RolePermissions_SubModuleId ON dbo.RolePermissions (SubModuleId);

/* ---------------------------------------------------------------------------
   14. Add SubModuleId to UserPermissionOverrides (nullable first)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserPermissionOverrides]') AND name = 'SubModuleId')
BEGIN
    ALTER TABLE dbo.UserPermissionOverrides ADD SubModuleId INT NULL;
END
;

UPDATE upo
SET upo.SubModuleId = sm.Id
FROM dbo.UserPermissionOverrides upo
INNER JOIN dbo.SubModules sm ON sm.ModuleId = upo.ModuleId AND sm.SubModuleCode = 'DEFAULT'
WHERE upo.SubModuleId IS NULL;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserPermissionOverrides]') AND name = 'SubModuleId' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.UserPermissionOverrides ALTER COLUMN SubModuleId INT NOT NULL;
END
;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserPermissionOverrides_SubModule')
BEGIN
    ALTER TABLE dbo.UserPermissionOverrides
        ADD CONSTRAINT FK_UserPermissionOverrides_SubModule FOREIGN KEY (SubModuleId) REFERENCES dbo.SubModules (Id);
END
;

DECLARE @upoUqName NVARCHAR(200);
SELECT @upoUqName = kc.name
FROM sys.key_constraints kc
INNER JOIN sys.tables t ON kc.parent_object_id = t.object_id
WHERE t.name = 'UserPermissionOverrides' AND kc.type = 'UQ';

IF @upoUqName IS NOT NULL
    EXEC('ALTER TABLE dbo.UserPermissionOverrides DROP CONSTRAINT [' + @upoUqName + ']');

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_UserPermissionOverrides_Matrix')
BEGIN
    ALTER TABLE dbo.UserPermissionOverrides
        ADD CONSTRAINT UQ_UserPermissionOverrides_Matrix UNIQUE (UserId, WorkspaceId, DomainId, ModuleId, SubModuleId, ScreenId, ActionId);
END
;

/* ---------------------------------------------------------------------------
   15. Add SubModuleId to WorkflowPermissions
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[WorkflowPermissions]') AND name = 'SubModuleId')
BEGIN
    ALTER TABLE dbo.WorkflowPermissions ADD SubModuleId INT NULL;
END
;

UPDATE wp
SET wp.SubModuleId = sm.Id
FROM dbo.WorkflowPermissions wp
INNER JOIN dbo.SubModules sm ON sm.ModuleId = wp.ModuleId AND sm.SubModuleCode = 'DEFAULT'
WHERE wp.SubModuleId IS NULL;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[WorkflowPermissions]') AND name = 'SubModuleId' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.WorkflowPermissions ALTER COLUMN SubModuleId INT NOT NULL;
END
;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_WorkflowPermissions_SubModule')
BEGIN
    ALTER TABLE dbo.WorkflowPermissions
        ADD CONSTRAINT FK_WorkflowPermissions_SubModule FOREIGN KEY (SubModuleId) REFERENCES dbo.SubModules (Id);
END
;

DECLARE @wpUqName NVARCHAR(200);
SELECT @wpUqName = kc.name
FROM sys.key_constraints kc
INNER JOIN sys.tables t ON kc.parent_object_id = t.object_id
WHERE t.name = 'WorkflowPermissions' AND kc.type = 'UQ';

IF @wpUqName IS NOT NULL
    EXEC('ALTER TABLE dbo.WorkflowPermissions DROP CONSTRAINT [' + @wpUqName + ']');

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_WorkflowPermissions_Matrix')
BEGIN
    ALTER TABLE dbo.WorkflowPermissions
        ADD CONSTRAINT UQ_WorkflowPermissions_Matrix UNIQUE (RoleId, ModuleId, SubModuleId, ScreenId);
END
;

/* ---------------------------------------------------------------------------
   16. Verification
--------------------------------------------------------------------------- */
SELECT 'SubModules' AS TableName, COUNT(*) AS RecordCount FROM dbo.SubModules;
SELECT 'Screens' AS TableName, COUNT(*) AS Total, SUM(CASE WHEN SubModuleId IS NOT NULL THEN 1 ELSE 0 END) AS Migrated FROM dbo.Screens;
SELECT 'RolePermissions' AS TableName, COUNT(*) AS Total, SUM(CASE WHEN SubModuleId IS NOT NULL THEN 1 ELSE 0 END) AS Migrated FROM dbo.RolePermissions;
