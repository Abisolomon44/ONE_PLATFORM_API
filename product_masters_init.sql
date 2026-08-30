/* =========================================================
   ONE ERP — Product / Billing Masters
   Tables + Seed + Workspace navigation
   Tenant DB: ERPNEXT
   ========================================================= */

/* ---------------- 1. Tables ---------------- */

IF OBJECT_ID('dbo.ProductSubCategories', 'U') IS NULL
CREATE TABLE ProductSubCategories
(
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

IF OBJECT_ID('dbo.ProductCategories', 'U') IS NULL
CREATE TABLE ProductCategories
(
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
GO

IF OBJECT_ID('dbo.ProductBrands', 'U') IS NULL
CREATE TABLE ProductBrands
(
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
GO

IF OBJECT_ID('dbo.Units', 'U') IS NULL
CREATE TABLE Units
(
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
GO

/* ---------------- 2. Seed: Units ---------------- */

IF NOT EXISTS (SELECT 1 FROM dbo.Units WHERE CompanyId = 1)
INSERT INTO Units (CompanyId, UnitCode, UnitName, Symbol, DecimalPlaces, IsActive, CreatedBy)
VALUES
(1, 'PCS',   'Piece',       'PCS',  0, 1, 1),
(1, 'BOX',   'Box',         'BOX',  0, 1, 1),
(1, 'PACK',  'Pack',        'PACK', 0, 1, 1),
(1, 'SET',   'Set',         'SET',  0, 1, 1),
(1, 'DOZ',   'Dozen',       'DOZ',  0, 1, 1),
(1, 'KG',    'Kilogram',    'KG',   3, 1, 1),
(1, 'G',     'Gram',        'G',    3, 1, 1),
(1, 'MG',    'Milligram',   'MG',   3, 1, 1),
(1, 'LTR',   'Litre',       'LTR',  3, 1, 1),
(1, 'ML',    'Millilitre',  'ML',   3, 1, 1),
(1, 'MTR',   'Meter',       'M',    3, 1, 1),
(1, 'CM',    'Centimeter',  'CM',   3, 1, 1),
(1, 'SQFT',  'Square Feet', 'SQFT', 2, 1, 1),
(1, 'SQM',   'Square Meter','SQM',  2, 1, 1),
(1, 'PAIR',  'Pair',        'PAIR', 0, 1, 1),
(1, 'ROLL',  'Roll',        'ROLL', 0, 1, 1),
(1, 'BAG',   'Bag',         'BAG',  0, 1, 1),
(1, 'BOTTLE','Bottle',      'BTL',  0, 1, 1),
(1, 'CAN',   'Can',         'CAN',  0, 1, 1);
GO

/* ---------------- 3. Seed: example Categories / Brands ---------------- */

IF NOT EXISTS (SELECT 1 FROM dbo.ProductCategories WHERE CompanyId = 1 AND CategoryCode = 'CAT-CRK')
INSERT INTO ProductCategories (CompanyId, CategoryCode, CategoryName, Description, ParentCategoryId, SortOrder, IsActive, CreatedBy)
VALUES (1, 'CAT-CRK', 'Crackers', 'Cracker products', NULL, 1, 1, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ProductSubCategories WHERE CompanyId = 1 AND SubCategoryCode = 'SUBCAT-SPK')
INSERT INTO ProductSubCategories (CompanyId, CategoryId, SubCategoryCode, SubCategoryName, Description, SortOrder, IsActive, CreatedBy)
VALUES (1, (SELECT Id FROM dbo.ProductCategories WHERE CompanyId = 1 AND CategoryCode = 'CAT-CRK'),
        'SUBCAT-SPK', 'Sparklers', 'Sparkler items', 1, 1, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ProductBrands WHERE CompanyId = 1 AND BrandCode = 'BRD-STD')
INSERT INTO ProductBrands (CompanyId, BrandCode, BrandName, Description, IsActive, CreatedBy)
VALUES (1, 'BRD-STD', 'Standard', 'Default brand', 1, 1);
GO

/* ---------------- 4. Navigation: workspace screens ---------------- */

IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE WorkspaceId = 2 AND DomainCode = 'DOM-PRODUCT')
BEGIN
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedDate)
    VALUES (2, 'DOM-PRODUCT', 'Product & Billing', 'package', 6, 1, SYSUTCDATETIME());
    DECLARE @dom INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, SortOrder, IsActive, CreatedDate)
    VALUES (@dom, 'MOD-PRODMASTERS', 'Product Masters', 'boxes', 1, 1, SYSUTCDATETIME());
    DECLARE @mod INT = SCOPE_IDENTITY();

    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, SortOrder, IsActive, CreatedDate)
    VALUES (@mod, 'SUB-PRODSETUP', 'Product Setup', 'package', 1, 1, SYSUTCDATETIME());
    DECLARE @sub INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, PermissionCode, SortOrder, IsActive, CreatedDate)
    VALUES
        (@sub, 'PRODUCT-CATEGORIES',    'Product Categories',     'Master', '/product-categories',    'product-categories',    1, 1, SYSUTCDATETIME()),
        (@sub, 'PRODUCT-SUBCATEGORIES', 'Product Sub Categories', 'Master', '/product-subcategories', 'product-subcategories', 2, 1, SYSUTCDATETIME()),
        (@sub, 'BRANDS',               'Brands',                 'Master', '/brands',                 'brands',                3, 1, SYSUTCDATETIME()),
        (@sub, 'UNITS',                'Units',                 'Master', '/units',                  'units',                 4, 1, SYSUTCDATETIME());
END
GO

/* ---------------- 5. Legacy permissions for Administrator (RoleId = 2) ---------------- */

IF EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleId = 2)
BEGIN
    INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode)
    SELECT 2, v.code FROM (VALUES
        ('product-categories.view'), ('product-categories.create'), ('product-categories.edit'), ('product-categories.delete'),
        ('product-subcategories.view'), ('product-subcategories.create'), ('product-subcategories.edit'), ('product-subcategories.delete'),
        ('brands.view'), ('brands.create'), ('brands.edit'), ('brands.delete'),
        ('units.view'), ('units.create'), ('units.edit'), ('units.delete')
    ) AS v(code)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.RolePermissionsLegacy WHERE RoleId = 2 AND PermissionCode = v.code);
END
GO

PRINT 'Product masters schema + seed + navigation applied.';
