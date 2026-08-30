-- ============================================================
-- Taxes (actual tax rates configured per company)
-- ============================================================
IF OBJECT_ID('dbo.Taxes', 'U') IS NULL
CREATE TABLE dbo.Taxes
(
    Id              BIGINT IDENTITY(1,1) CONSTRAINT PK_Taxes PRIMARY KEY,
    CompanyId       BIGINT NOT NULL,
    BranchId        BIGINT NULL,
    TaxTypeSystemId BIGINT NOT NULL,
    TaxCode         VARCHAR(30)  NOT NULL,
    TaxName         VARCHAR(100) NOT NULL,
    TaxRate         DECIMAL(8,4) NOT NULL,
    IsInclusive     BIT NOT NULL DEFAULT 0,
    EffectiveFrom   DATE NULL,
    EffectiveTo     DATE NULL,
    IsActive        BIT NOT NULL DEFAULT 1,
    Description     VARCHAR(300) NULL,
    CreatedBy       BIGINT NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiedBy      BIGINT NULL,
    ModifiedAt      DATETIME NULL,
    CONSTRAINT FK_Taxes_TaxTypeSystems FOREIGN KEY (TaxTypeSystemId) REFERENCES dbo.TaxTypeSystems(Id)
);
GO

-- Seed GST rate slabs for the default company (CompanyId = 1, GST TaxTypeSystem = 1)
IF NOT EXISTS (SELECT 1 FROM dbo.Taxes)
INSERT INTO dbo.Taxes (CompanyId, BranchId, TaxTypeSystemId, TaxCode, TaxName, TaxRate, IsInclusive, IsActive, CreatedBy)
VALUES
    (1, NULL, 1, 'GST0',  'GST 0%',  0.00,  0, 1, 1),
    (1, NULL, 1, 'GST5',  'GST 5%',  5.00,  0, 1, 1),
    (1, NULL, 1, 'GST12', 'GST 12%', 12.00, 0, 1, 1),
    (1, NULL, 1, 'GST18', 'GST 18%', 18.00, 0, 1, 1),
    (1, NULL, 1, 'GST28', 'GST 28%', 28.00, 0, 1, 1);
GO

-- Navigation: Screen "Taxes" under the "Tax Masters" submodule
DECLARE @smId BIGINT = (SELECT Id FROM dbo.SubModules WHERE SubModuleCode = 'SUB-TAX');

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'TAXES')
INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, PermissionCode, SortOrder, IsActive, CreatedDate)
VALUES (@smId, 'TAXES', 'Taxes', 'Master', '/taxes', 'taxes', 2, 1, SYSUTCDATETIME());

-- Legacy perms for Role 2
IF EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleId = 2)
BEGIN
    INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode)
    SELECT 2, v.code FROM (VALUES
        ('taxes.view'), ('taxes.create'), ('taxes.edit'), ('taxes.delete')
    ) AS v(code)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.RolePermissionsLegacy WHERE RoleId = 2 AND PermissionCode = v.code);
END
GO
