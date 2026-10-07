/*
   ONE ERP — Tenant migration v2
   Adds the nullable low-stock threshold used by InventoryRepository.
   Safe to run repeatedly; existing product rows remain unmanaged (NULL).
*/
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
    THROW 51011, 'Cannot add ReorderLevel because dbo.Products does not exist in this database.', 1;

IF COL_LENGTH(N'dbo.Products', N'ReorderLevel') IS NULL
BEGIN
    ALTER TABLE dbo.Products ADD ReorderLevel DECIMAL(18,3) NULL;
END;
