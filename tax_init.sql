-- ============================================================
-- TaxTypeSystems (System Master)
-- ============================================================
IF OBJECT_ID('dbo.TaxTypeSystems', 'U') IS NULL
CREATE TABLE dbo.TaxTypeSystems
(
    Id          BIGINT IDENTITY(1,1) CONSTRAINT PK_TaxTypeSystems PRIMARY KEY,
    Code        VARCHAR(30)  NOT NULL,
    Name        VARCHAR(100) NOT NULL,
    Description VARCHAR(300) NULL,
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedBy   BIGINT NULL,
    CreatedAt   DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TaxTypeSystems)
INSERT INTO dbo.TaxTypeSystems (Code, Name, Description, IsActive, CreatedBy)
VALUES
    ('GST',        'Goods and Services Tax', 'GST based taxation system',            1, 1),
    ('VAT',        'Value Added Tax',        'VAT based taxation system',            1, 1),
    ('SALES_TAX',  'Sales Tax',              'Sales tax based system',               1, 1),
    ('HST',        'Harmonized Sales Tax',   'HST based taxation system',            1, 1),
    ('PST',        'Provincial Sales Tax',   'PST based taxation system',            1, 1),
    ('QST',        'Quebec Sales Tax',       'QST based taxation system',            1, 1),
    ('CESS',       'Cess',                   'Additional cess taxation',             1, 1),
    ('OTHER',      'Other Tax',              'Custom taxation system',               1, 1);
GO

-- Navigation: SubModule "Tax Masters" under Product & Billing / Product Masters
DECLARE @wsId BIGINT = (SELECT Id FROM dbo.WorkSpaces WHERE WorkSpaceCode = 'WS-PRODBILL');
DECLARE @mId  BIGINT = (SELECT Id FROM dbo.Modules    WHERE ModuleCode  = 'MOD-PRODMASTERS');

IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'SUB-TAX')
INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy, CreatedDate)
VALUES (@mId, 'SUB-TAX', 'Tax Masters', 'Percent', NULL, 3, 1, '1', GETDATE());

DECLARE @smId BIGINT = (SELECT Id FROM dbo.SubModules WHERE SubModuleCode = 'SUB-TAX');

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'TAXTYPES')
INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, PermissionCode, SortOrder, IsActive, CreatedDate)
VALUES (@smId, 'TAXTYPES', 'Tax Type Systems', 'Master', '/tax-type-systems', 'tax-type-systems', 1, 1, SYSUTCDATETIME());

-- Legacy perms for Role 2
IF EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleId = 2)
BEGIN
    INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode)
    SELECT 2, v.code FROM (VALUES
        ('tax-type-systems.view'), ('tax-type-systems.create'), ('tax-type-systems.edit'), ('tax-type-systems.delete')
    ) AS v(code)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.RolePermissionsLegacy WHERE RoleId = 2 AND PermissionCode = v.code);
END
GO
