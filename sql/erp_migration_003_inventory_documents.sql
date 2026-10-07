/*
   ONE ERP — Tenant migration v3
   Creates the Stage 5 inventory document tables consumed by InventoryRepository:
   stock adjustments, warehouse transfers, and physical stock counts.
   The baseline schema may already contain these tables; every object is guarded
   so this migration also safely upgrades tenants provisioned before Stage 5.
*/
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.StockAdjustment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockAdjustment (
        StockAdjustmentId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockAdjustment PRIMARY KEY,
        AdjustmentNumber NVARCHAR(30) NOT NULL,
        CompanyId BIGINT NOT NULL,
        BranchId BIGINT NOT NULL,
        WarehouseId BIGINT NOT NULL,
        AdjustmentDate DATETIME2 NOT NULL,
        Reason NVARCHAR(500) NULL,
        Remarks NVARCHAR(500) NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_StockAdjustment_Status DEFAULT 'DRAFT',
        CreatedByUserID BIGINT NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_StockAdjustment_CreatedAt DEFAULT SYSUTCDATETIME(),
        PostedByUserID BIGINT NULL,
        PostedAt DATETIME2 NULL,
        CONSTRAINT UQ_StockAdjustment_No UNIQUE (CompanyId, AdjustmentNumber)
    );
END;

IF OBJECT_ID(N'dbo.StockAdjustmentItem', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockAdjustmentItem (
        StockAdjustmentItemId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockAdjustmentItem PRIMARY KEY,
        StockAdjustmentId BIGINT NOT NULL,
        ProductId BIGINT NOT NULL,
        UnitId BIGINT NOT NULL,
        QuantityDelta DECIMAL(18,3) NOT NULL,
        Rate DECIMAL(18,4) NOT NULL CONSTRAINT DF_StockAdjustmentItem_Rate DEFAULT 0,
        Reason NVARCHAR(300) NULL,
        CONSTRAINT FK_StockAdjustmentItem_Header FOREIGN KEY (StockAdjustmentId)
            REFERENCES dbo.StockAdjustment(StockAdjustmentId)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.StockAdjustment') AND name = N'IX_StockAdjustment_Company_Date')
    CREATE INDEX IX_StockAdjustment_Company_Date ON dbo.StockAdjustment (CompanyId, AdjustmentDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.StockAdjustmentItem') AND name = N'IX_StockAdjustmentItem_Header')
    CREATE INDEX IX_StockAdjustmentItem_Header ON dbo.StockAdjustmentItem (StockAdjustmentId);

IF OBJECT_ID(N'dbo.StockTransfer', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockTransfer (
        StockTransferId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockTransfer PRIMARY KEY,
        TransferNumber NVARCHAR(30) NOT NULL,
        CompanyId BIGINT NOT NULL,
        BranchId BIGINT NOT NULL,
        FromWarehouseId BIGINT NOT NULL,
        ToWarehouseId BIGINT NOT NULL,
        TransferDate DATETIME2 NOT NULL,
        Remarks NVARCHAR(500) NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_StockTransfer_Status DEFAULT 'DRAFT',
        CreatedByUserID BIGINT NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_StockTransfer_CreatedAt DEFAULT SYSUTCDATETIME(),
        PostedByUserID BIGINT NULL,
        PostedAt DATETIME2 NULL,
        CONSTRAINT UQ_StockTransfer_No UNIQUE (CompanyId, TransferNumber),
        CONSTRAINT CK_StockTransfer_Warehouses CHECK (FromWarehouseId <> ToWarehouseId)
    );
END;

IF OBJECT_ID(N'dbo.StockTransferItem', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockTransferItem (
        StockTransferItemId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockTransferItem PRIMARY KEY,
        StockTransferId BIGINT NOT NULL,
        ProductId BIGINT NOT NULL,
        UnitId BIGINT NOT NULL,
        Quantity DECIMAL(18,3) NOT NULL CONSTRAINT CK_StockTransferItem_Qty CHECK (Quantity > 0),
        Rate DECIMAL(18,4) NOT NULL CONSTRAINT DF_StockTransferItem_Rate DEFAULT 0,
        Remarks NVARCHAR(300) NULL,
        CONSTRAINT FK_StockTransferItem_Header FOREIGN KEY (StockTransferId)
            REFERENCES dbo.StockTransfer(StockTransferId)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.StockTransfer') AND name = N'IX_StockTransfer_Company_Date')
    CREATE INDEX IX_StockTransfer_Company_Date ON dbo.StockTransfer (CompanyId, TransferDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.StockTransferItem') AND name = N'IX_StockTransferItem_Header')
    CREATE INDEX IX_StockTransferItem_Header ON dbo.StockTransferItem (StockTransferId);

IF OBJECT_ID(N'dbo.StockCount', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockCount (
        StockCountId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockCount PRIMARY KEY,
        CountNumber NVARCHAR(30) NOT NULL,
        CompanyId BIGINT NOT NULL,
        BranchId BIGINT NOT NULL,
        WarehouseId BIGINT NOT NULL,
        CountDate DATETIME2 NOT NULL,
        Remarks NVARCHAR(500) NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_StockCount_Status DEFAULT 'DRAFT',
        CreatedByUserID BIGINT NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_StockCount_CreatedAt DEFAULT SYSUTCDATETIME(),
        PostedByUserID BIGINT NULL,
        PostedAt DATETIME2 NULL,
        CONSTRAINT UQ_StockCount_No UNIQUE (CompanyId, CountNumber)
    );
END;

IF OBJECT_ID(N'dbo.StockCountItem', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockCountItem (
        StockCountItemId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockCountItem PRIMARY KEY,
        StockCountId BIGINT NOT NULL,
        ProductId BIGINT NOT NULL,
        UnitId BIGINT NOT NULL,
        BookQuantity DECIMAL(18,3) NOT NULL CONSTRAINT DF_StockCountItem_Book DEFAULT 0,
        CountedQuantity DECIMAL(18,3) NOT NULL CONSTRAINT DF_StockCountItem_Counted DEFAULT 0,
        Variance DECIMAL(18,3) NOT NULL CONSTRAINT DF_StockCountItem_Variance DEFAULT 0,
        Rate DECIMAL(18,4) NOT NULL CONSTRAINT DF_StockCountItem_Rate DEFAULT 0,
        CONSTRAINT FK_StockCountItem_Header FOREIGN KEY (StockCountId)
            REFERENCES dbo.StockCount(StockCountId)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.StockCount') AND name = N'IX_StockCount_Company_Date')
    CREATE INDEX IX_StockCount_Company_Date ON dbo.StockCount (CompanyId, CountDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.StockCountItem') AND name = N'IX_StockCountItem_Header')
    CREATE INDEX IX_StockCountItem_Header ON dbo.StockCountItem (StockCountId);
