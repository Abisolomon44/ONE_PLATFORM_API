/* ============================================================================
   ONE ERP — Stage 5 Inventory migration (T074–T085)
   T079: StockAdjustment + StockAdjustmentItem
   T080/T081: StockTransfer + StockTransferItem
   T082: StockCount + StockCountItem
   T085: Products.ReorderLevel (real schema gap — no threshold column existed)
   T076/T077/T078/T083/T084 need NO new tables (Stock / StockTransaction
   already exist and are reused as the ledger + valuation source).
   Idempotent: safe to run repeatedly.
   ============================================================================ */

/* ---------------------------------------------------------------------------
   T079 — StockAdjustment
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StockAdjustment]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.StockAdjustment (
        StockAdjustmentId BIGINT IDENTITY(1,1) CONSTRAINT PK_StockAdjustment PRIMARY KEY,
        AdjustmentNumber  NVARCHAR(30)  NOT NULL,
        CompanyId         BIGINT        NOT NULL,
        BranchId          BIGINT        NOT NULL,
        WarehouseId       BIGINT        NOT NULL,
        AdjustmentDate    DATETIME2     NOT NULL,
        Reason            NVARCHAR(500) NULL,
        Remarks           NVARCHAR(500) NULL,
        Status            NVARCHAR(20)  NOT NULL CONSTRAINT DF_StockAdjustment_Status DEFAULT 'DRAFT', -- DRAFT | POSTED | CANCELLED
        CreatedByUserID   BIGINT        NOT NULL,
        CreatedAt         DATETIME2     NOT NULL CONSTRAINT DF_StockAdjustment_CreatedAt DEFAULT SYSUTCDATETIME(),
        PostedByUserID    BIGINT        NULL,
        PostedAt          DATETIME2     NULL,

        CONSTRAINT UQ_StockAdjustment_No UNIQUE (CompanyId, AdjustmentNumber)
    );

    CREATE INDEX IX_StockAdjustment_Company_Date ON dbo.StockAdjustment (CompanyId, AdjustmentDate);

    CREATE TABLE dbo.StockAdjustmentItem (
        StockAdjustmentItemId BIGINT IDENTITY(1,1) CONSTRAINT PK_StockAdjustmentItem PRIMARY KEY,
        StockAdjustmentId     BIGINT        NOT NULL,
        ProductId             BIGINT        NOT NULL,
        UnitId                BIGINT        NOT NULL,
        QuantityDelta         DECIMAL(18,3) NOT NULL,  -- + increase / - decrease
        Rate                  DECIMAL(18,4) NOT NULL CONSTRAINT DF_StockAdjustmentItem_Rate DEFAULT 0,
        Reason                NVARCHAR(300) NULL,

        CONSTRAINT FK_StockAdjustmentItem_Header FOREIGN KEY (StockAdjustmentId) REFERENCES dbo.StockAdjustment(StockAdjustmentId)
    );

    CREATE INDEX IX_StockAdjustmentItem_Header ON dbo.StockAdjustmentItem (StockAdjustmentId);
END
;

/* ---------------------------------------------------------------------------
   T080/T081 — StockTransfer
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StockTransfer]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.StockTransfer (
        StockTransferId    BIGINT IDENTITY(1,1) CONSTRAINT PK_StockTransfer PRIMARY KEY,
        TransferNumber     NVARCHAR(30)  NOT NULL,
        CompanyId          BIGINT        NOT NULL,
        BranchId           BIGINT        NOT NULL,
        FromWarehouseId    BIGINT        NOT NULL,
        ToWarehouseId      BIGINT        NOT NULL,
        TransferDate       DATETIME2     NOT NULL,
        Remarks            NVARCHAR(500) NULL,
        Status             NVARCHAR(20)  NOT NULL CONSTRAINT DF_StockTransfer_Status DEFAULT 'DRAFT', -- DRAFT | POSTED | CANCELLED
        CreatedByUserID    BIGINT        NOT NULL,
        CreatedAt          DATETIME2     NOT NULL CONSTRAINT DF_StockTransfer_CreatedAt DEFAULT SYSUTCDATETIME(),
        PostedByUserID     BIGINT        NULL,
        PostedAt           DATETIME2     NULL,

        CONSTRAINT UQ_StockTransfer_No UNIQUE (CompanyId, TransferNumber),
        CONSTRAINT CK_StockTransfer_Warehouses CHECK (FromWarehouseId <> ToWarehouseId)
    );

    CREATE INDEX IX_StockTransfer_Company_Date ON dbo.StockTransfer (CompanyId, TransferDate);

    CREATE TABLE dbo.StockTransferItem (
        StockTransferItemId BIGINT IDENTITY(1,1) CONSTRAINT PK_StockTransferItem PRIMARY KEY,
        StockTransferId     BIGINT        NOT NULL,
        ProductId           BIGINT        NOT NULL,
        UnitId              BIGINT        NOT NULL,
        Quantity            DECIMAL(18,3) NOT NULL CONSTRAINT DF_StockTransferItem_Qty CHECK (Quantity > 0),
        Rate                DECIMAL(18,4) NOT NULL CONSTRAINT DF_StockTransferItem_Rate DEFAULT 0,
        Remarks             NVARCHAR(300) NULL,

        CONSTRAINT FK_StockTransferItem_Header FOREIGN KEY (StockTransferId) REFERENCES dbo.StockTransfer(StockTransferId)
    );

    CREATE INDEX IX_StockTransferItem_Header ON dbo.StockTransferItem (StockTransferId);
END
;

/* ---------------------------------------------------------------------------
   T082 — StockCount
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StockCount]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.StockCount (
        StockCountId     BIGINT IDENTITY(1,1) CONSTRAINT PK_StockCount PRIMARY KEY,
        CountNumber      NVARCHAR(30)  NOT NULL,
        CompanyId        BIGINT        NOT NULL,
        BranchId         BIGINT        NOT NULL,
        WarehouseId      BIGINT        NOT NULL,
        CountDate        DATETIME2     NOT NULL,
        Remarks          NVARCHAR(500) NULL,
        Status           NVARCHAR(20)  NOT NULL CONSTRAINT DF_StockCount_Status DEFAULT 'DRAFT', -- DRAFT | POSTED | CANCELLED
        CreatedByUserID  BIGINT        NOT NULL,
        CreatedAt        DATETIME2     NOT NULL CONSTRAINT DF_StockCount_CreatedAt DEFAULT SYSUTCDATETIME(),
        PostedByUserID   BIGINT        NULL,
        PostedAt         DATETIME2     NULL,

        CONSTRAINT UQ_StockCount_No UNIQUE (CompanyId, CountNumber)
    );

    CREATE INDEX IX_StockCount_Company_Date ON dbo.StockCount (CompanyId, CountDate);

    CREATE TABLE dbo.StockCountItem (
        StockCountItemId BIGINT IDENTITY(1,1) CONSTRAINT PK_StockCountItem PRIMARY KEY,
        StockCountId     BIGINT        NOT NULL,
        ProductId        BIGINT        NOT NULL,
        UnitId           BIGINT        NOT NULL,
        BookQuantity     DECIMAL(18,3) NOT NULL CONSTRAINT DF_StockCountItem_Book DEFAULT 0,  -- book qty at count time
        CountedQuantity  DECIMAL(18,3) NOT NULL CONSTRAINT DF_StockCountItem_Counted DEFAULT 0,
        Variance         DECIMAL(18,3) NOT NULL CONSTRAINT DF_StockCountItem_Variance DEFAULT 0, -- counted - book
        Rate             DECIMAL(18,4) NOT NULL CONSTRAINT DF_StockCountItem_Rate DEFAULT 0,

        CONSTRAINT FK_StockCountItem_Header FOREIGN KEY (StockCountId) REFERENCES dbo.StockCount(StockCountId)
    );

    CREATE INDEX IX_StockCountItem_Header ON dbo.StockCountItem (StockCountId);
END
;

/* ---------------------------------------------------------------------------
   T085 — Products.ReorderLevel: low-stock threshold. The column does not
   exist anywhere in the schema; the feature has no threshold without it.
   NULL = product is not managed by reorder level.
   --------------------------------------------------------------------------- */
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'ReorderLevel'
)
BEGIN
    ALTER TABLE dbo.Products ADD ReorderLevel DECIMAL(18,3) NULL;
END
;
