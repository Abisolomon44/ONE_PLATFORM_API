-- ============================================================
-- Master Import common log table
-- ============================================================
IF OBJECT_ID('dbo.ImportLogs', 'U') IS NULL
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

        Status VARCHAR(30) NOT NULL,            -- PROCESSING / COMPLETED / PARTIAL / FAILED

        ErrorMessage VARCHAR(2000) NULL,

        ImportedBy BIGINT NOT NULL,
        ImportedAt DATETIME NOT NULL DEFAULT GETDATE()
    );

    CREATE INDEX IX_ImportLogs_Company_ImportedAt ON dbo.ImportLogs (CompanyId, ImportedAt DESC);
END
GO

-- Seed permission codes for the admin role (RoleId = 2) so the
-- Master Import page is accessible. Safe to re-run (ignores duplicates).
IF NOT EXISTS (SELECT 1 FROM dbo.RolePermissionsLegacy WHERE RoleId = 2 AND PermissionCode = 'master-import.view')
    INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode) VALUES (2, 'master-import.view');
IF NOT EXISTS (SELECT 1 FROM dbo.RolePermissionsLegacy WHERE RoleId = 2 AND PermissionCode = 'master-import.manage')
    INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode) VALUES (2, 'master-import.manage');
IF NOT EXISTS (SELECT 1 FROM dbo.RolePermissionsLegacy WHERE RoleId = 2 AND PermissionCode = 'import-logs.view')
    INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode) VALUES (2, 'import-logs.view');
GO
