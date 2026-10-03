/* ============================================================================
   ONE ERP — Stage 2 Sales Lifecycle migration
   T027/T028: SalesReturn + SalesReturnItem (mirrors PurchaseReturn DDL)
   T039:      ApiIdempotency (duplicate-save guard; no existing mechanism found)
   Idempotent: safe to run repeatedly.
   ============================================================================ */

/* ---------------------------------------------------------------------------
   SalesReturn
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalesReturn]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.SalesReturn (
        SalesReturnId         BIGINT IDENTITY(1,1) CONSTRAINT PK_SalesReturn PRIMARY KEY,
        SalesInvoiceId        BIGINT        NOT NULL,
        CompanyId             BIGINT        NOT NULL,
        CompanyNameSnapshot   NVARCHAR(200) NULL,
        BranchId              BIGINT        NOT NULL,
        BranchNameSnapshot    NVARCHAR(200) NULL,
        WarehouseId           BIGINT        NOT NULL,
        CustomerId            BIGINT        NOT NULL,
        CustomerNameSnapshot  NVARCHAR(200) NULL,
        ReturnNumber          NVARCHAR(30)  NOT NULL,
        ReturnDate            DATETIME2     NOT NULL,
        TotalGrossAmount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturn_Gross DEFAULT 0,
        TotalDiscountAmount   DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturn_Disc DEFAULT 0,
        TotalTaxableAmount    DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturn_Taxable DEFAULT 0,
        TotalCGSTAmount       DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturn_CGST DEFAULT 0,
        TotalSGSTAmount       DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturn_SGST DEFAULT 0,
        TotalIGSTAmount       DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturn_IGST DEFAULT 0,
        TotalCESSAmount       DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturn_CESS DEFAULT 0,
        TotalRoundOff         DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturn_RoundOff DEFAULT 0,
        GrandTotal            DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturn_Grand DEFAULT 0,
        StatusID              BIGINT        NOT NULL CONSTRAINT DF_SalesReturn_Status DEFAULT 1,
        Reason                NVARCHAR(500) NULL,
        Remarks               NVARCHAR(500) NULL,
        PaymentTypeId         BIGINT        NULL,
        PaymentMethodId       BIGINT        NULL,
        RefundAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturn_Refund DEFAULT 0,
        CreatedByUserID       BIGINT        NOT NULL,
        CreatedAt             DATETIME2     NOT NULL CONSTRAINT DF_SalesReturn_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedByUserID       BIGINT        NULL,
        UpdatedAt             DATETIME2     NULL,
        CancelledByUserID     BIGINT        NULL,
        CancelledAt           DATETIME2     NULL,
        CancellationReason    NVARCHAR(500) NULL,

        CONSTRAINT UQ_SalesReturn_No UNIQUE (CompanyId, ReturnNumber)
    );

    CREATE INDEX IX_SalesReturn_Company_Date ON dbo.SalesReturn (CompanyId, ReturnDate);
    CREATE INDEX IX_SalesReturn_Invoice ON dbo.SalesReturn (SalesInvoiceId);
    CREATE INDEX IX_SalesReturn_Customer ON dbo.SalesReturn (CustomerId);
END
;

/* ---------------------------------------------------------------------------
   SalesReturnItem
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalesReturnItem]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.SalesReturnItem (
        SalesReturnItemId    BIGINT IDENTITY(1,1) CONSTRAINT PK_SalesReturnItem PRIMARY KEY,
        SalesReturnId        BIGINT        NOT NULL,
        SalesInvoiceItemId   BIGINT        NOT NULL,
        ProductId            BIGINT        NOT NULL,
        ProductCodeSnapshot  NVARCHAR(100) NULL,
        ProductNameSnapshot  NVARCHAR(200) NULL,
        UnitID               BIGINT        NOT NULL,
        UnitNameSnapshot     NVARCHAR(100) NULL,
        HSNID                BIGINT        NULL,
        HSNCodeSnapshot      NVARCHAR(100) NULL,
        BarcodeSnapshot      NVARCHAR(100) NULL,
        BatchId              BIGINT        NULL,
        ReturnQuantity       DECIMAL(18,3) NOT NULL CONSTRAINT DF_SalesReturnItem_Qty DEFAULT 0,
        FreeQuantity         DECIMAL(18,3) NOT NULL CONSTRAINT DF_SalesReturnItem_FreeQty DEFAULT 0,
        Rate                 DECIMAL(18,4) NOT NULL CONSTRAINT DF_SalesReturnItem_Rate DEFAULT 0,
        DiscountAmount       DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturnItem_Disc DEFAULT 0,
        TaxableAmount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturnItem_Taxable DEFAULT 0,
        GSTPercent           DECIMAL(8,3)  NOT NULL CONSTRAINT DF_SalesReturnItem_GST DEFAULT 0,
        CGSTPercent          DECIMAL(8,3)  NOT NULL CONSTRAINT DF_SalesReturnItem_CGSTP DEFAULT 0,
        SGSTPercent          DECIMAL(8,3)  NOT NULL CONSTRAINT DF_SalesReturnItem_SGSTP DEFAULT 0,
        IGSTPercent          DECIMAL(8,3)  NOT NULL CONSTRAINT DF_SalesReturnItem_IGSTP DEFAULT 0,
        CESSPercent          DECIMAL(8,3)  NOT NULL CONSTRAINT DF_SalesReturnItem_CESSP DEFAULT 0,
        CGSTAmount           DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturnItem_CGSTA DEFAULT 0,
        SGSTAmount           DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturnItem_SGSTA DEFAULT 0,
        IGSTAmount           DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturnItem_IGSTA DEFAULT 0,
        CESSAmount           DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturnItem_CESSA DEFAULT 0,
        LineTotal            DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturnItem_Line DEFAULT 0,

        CONSTRAINT FK_SalesReturnItem_Return FOREIGN KEY (SalesReturnId) REFERENCES dbo.SalesReturn(SalesReturnId)
    );

    CREATE INDEX IX_SalesReturnItem_ReturnId ON dbo.SalesReturnItem (SalesReturnId);
    CREATE INDEX IX_SalesReturnItem_ProductId ON dbo.SalesReturnItem (ProductId);
END
;

/* ---------------------------------------------------------------------------
   ApiIdempotency (T039) — duplicate-save guard for money/stock-changing POSTs.
   Stores the created resource id so a retried POST can replay the result.
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApiIdempotency]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.ApiIdempotency (
        IdempotencyKey   NVARCHAR(128) NOT NULL,
        CompanyId        BIGINT        NOT NULL,
        Endpoint         NVARCHAR(200) NOT NULL,
        ReferenceId      BIGINT        NULL,
        ResponseJson     NVARCHAR(MAX) NULL,
        CreatedAt        DATETIME2     NOT NULL CONSTRAINT DF_ApiIdempotency_CreatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_ApiIdempotency PRIMARY KEY (CompanyId, Endpoint, IdempotencyKey)
    );
END
;
