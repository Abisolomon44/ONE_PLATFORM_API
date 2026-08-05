/* =============================================================================
   ONE ERP - Migration: Fix Foreign Key References for BranchTypes and WarehouseTypes
   Purpose  : FK_Branches_BranchType was referencing BranchTypes(Id) but the
               PK column is BranchTypeId.
               FK_Warehouses_Type was referencing WarehouseTypes(Id) but the
               PK column is WarehouseTypeId.
   ============================================================================= */

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Branches_BranchType')
BEGIN
    ALTER TABLE dbo.Branches DROP CONSTRAINT FK_Branches_BranchType;
END;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Warehouses_Type')
BEGIN
    ALTER TABLE dbo.Warehouses DROP CONSTRAINT FK_Warehouses_Type;
END;

ALTER TABLE dbo.Branches
    ADD CONSTRAINT FK_Branches_BranchType
    FOREIGN KEY (BranchTypeId) REFERENCES dbo.BranchTypes(BranchTypeId);

ALTER TABLE dbo.Warehouses
    ADD CONSTRAINT FK_Warehouses_Type
    FOREIGN KEY (WarehouseTypeId) REFERENCES dbo.WarehouseTypes(WarehouseTypeId);