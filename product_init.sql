/* =========================================================
   ONE ERP — Product master
   ========================================================= */

IF OBJECT_ID('dbo.Products', 'U') IS NULL
CREATE TABLE Products(
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
GO

/* Navigation: add a "Products" screen under the existing Product Setup submodule */
IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'PRODUCTS')
BEGIN
    DECLARE @sub INT = (SELECT TOP 1 Id FROM dbo.SubModules WHERE SubModuleCode = 'SUB-PRODSETUP');
    IF @sub IS NOT NULL
    BEGIN
        INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, PermissionCode, SortOrder, IsActive, CreatedDate)
        VALUES (@sub, 'PRODUCTS', 'Products', 'Master', '/products', 'products', 5, 1, SYSUTCDATETIME());
    END
END
GO

/* Legacy permissions for Administrator (RoleId = 2) */
IF EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleId = 2)
BEGIN
    INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode)
    SELECT 2, v.code FROM (VALUES
        ('products.view'), ('products.create'), ('products.edit'), ('products.delete')
    ) AS v(code)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.RolePermissionsLegacy WHERE RoleId = 2 AND PermissionCode = v.code);
END
GO

PRINT 'Product master schema + navigation + permissions applied.';
