/* =============================================================================
   ONE ERP - Sales Module (Stage 1: Sales Invoice core)
   -----------------------------------------------------------------------------
   Creates the Sales invoice transaction tables. Reuses the existing generic
   Payment / PaymentAllocation tables (ReferenceType = 'SALES') and the
   Stock / StockTransaction tables (ReferenceType = 'SALES', OUT).

   Unified design: one SalesInvoice table serves both POS and normal sales via
   the SourceType column ('SALES' / 'POS').
   ========================================================================== */

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalesInvoice]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.SalesInvoice (
        SalesInvoiceId        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalesInvoice PRIMARY KEY,
        SalesInvoiceNo        NVARCHAR(30)         NOT NULL,
        InvoiceDate           DATETIME2            NOT NULL,
        SourceType            NVARCHAR(20)         NOT NULL CONSTRAINT DF_SalesInvoice_SourceType DEFAULT 'SALES',

        CompanyId             BIGINT               NOT NULL,
        CompanyNameSnapshot   NVARCHAR(200)        NULL,
        BranchId              BIGINT               NOT NULL,
        WarehouseId           BIGINT               NOT NULL,
        CustomerId            BIGINT               NOT NULL,
        CustomerNameSnapshot  NVARCHAR(200)        NULL,

        SalesTypeId           INT                  NULL,
        PriceListId           BIGINT               NULL,

        ReferenceNo           NVARCHAR(50)         NULL,
        ReferenceDate         DATE                 NULL,

        TotalGrossAmount      DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_Gross DEFAULT 0,
        TotalDiscountAmount   DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_Disc DEFAULT 0,
        TotalTaxableAmount    DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_Taxable DEFAULT 0,
        TotalCGSTAmount       DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_CGST DEFAULT 0,
        TotalSGSTAmount       DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_SGST DEFAULT 0,
        TotalIGSTAmount       DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_IGST DEFAULT 0,
        TotalCESSAmount       DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_CESS DEFAULT 0,
        TotalRoundOff         DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_RoundOff DEFAULT 0,
        GrandTotal            DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_Grand DEFAULT 0,
        PaidAmount            DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_Paid DEFAULT 0,
        BalanceAmount         DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoice_Balance DEFAULT 0,

        PaymentTypeID         BIGINT               NULL,
        PaymentMethodID       BIGINT               NULL,
        StatusID              BIGINT               NOT NULL CONSTRAINT DF_SalesInvoice_Status DEFAULT 1,
        InvoiceStatus         NVARCHAR(20)         NOT NULL CONSTRAINT DF_SalesInvoice_InvStatus DEFAULT 'POSTED',

        Remarks               NVARCHAR(500)        NULL,

        IsActive              BIT                  NOT NULL CONSTRAINT DF_SalesInvoice_IsActive DEFAULT 1,
        CreatedByUserID       BIGINT               NOT NULL,
        CreatedAt             DATETIME2            NOT NULL CONSTRAINT DF_SalesInvoice_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedByUserID       BIGINT               NULL,
        UpdatedAt             DATETIME2           NULL,

        CONSTRAINT UQ_SalesInvoice_No UNIQUE (CompanyId, SalesInvoiceNo)
    );

    CREATE INDEX IX_SalesInvoice_Company_Date ON dbo.SalesInvoice (CompanyId, InvoiceDate);
    CREATE INDEX IX_SalesInvoice_CustomerId ON dbo.SalesInvoice (CustomerId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalesInvoiceItem]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.SalesInvoiceItem (
        SalesInvoiceItemId    BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalesInvoiceItem PRIMARY KEY,
        SalesInvoiceId        BIGINT               NOT NULL,

        ProductId             BIGINT               NOT NULL,
        ProductCodeSnapshot   NVARCHAR(100)        NULL,
        ProductNameSnapshot   NVARCHAR(200)        NULL,
        UnitID                BIGINT               NOT NULL,
        UnitNameSnapshot      NVARCHAR(100)        NULL,
        BatchId               BIGINT               NULL,
        HSNID                 BIGINT               NULL,
        HSNCodeSnapshot       NVARCHAR(100)        NULL,
        BarcodeSnapshot       NVARCHAR(100)        NULL,

        Quantity              DECIMAL(18,3)        NOT NULL,
        FreeQuantity          DECIMAL(18,3)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_Free DEFAULT 0,
        Rate                  DECIMAL(18,4)        NOT NULL,
        GrossAmount           DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_Gross DEFAULT 0,

        DiscountPercentage    DECIMAL(8,3)         NOT NULL CONSTRAINT DF_SalesInvoiceItem_DiscPct DEFAULT 0,
        DiscountAmount        DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_Disc DEFAULT 0,

        TaxableAmount         DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_Taxable DEFAULT 0,

        GSTPercent            DECIMAL(8,3)         NOT NULL CONSTRAINT DF_SalesInvoiceItem_GSTPct DEFAULT 0,
        CGSTPercent           DECIMAL(8,3)         NOT NULL CONSTRAINT DF_SalesInvoiceItem_CGSTPct DEFAULT 0,
        SGSTPercent           DECIMAL(8,3)         NOT NULL CONSTRAINT DF_SalesInvoiceItem_SGSTPct DEFAULT 0,
        IGSTPercent           DECIMAL(8,3)         NOT NULL CONSTRAINT DF_SalesInvoiceItem_IGSTPct DEFAULT 0,
        CESSPercent           DECIMAL(8,3)         NOT NULL CONSTRAINT DF_SalesInvoiceItem_CESSPct DEFAULT 0,

        CGSTAmount            DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_CGSTAmt DEFAULT 0,
        SGSTAmount            DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_SGSTAmt DEFAULT 0,
        IGSTAmount            DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_IGSTAmt DEFAULT 0,
        CESSAmount            DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_CESSAmt DEFAULT 0,

        LineTotal             DECIMAL(18,2)        NOT NULL CONSTRAINT DF_SalesInvoiceItem_Line DEFAULT 0,
        Remarks               NVARCHAR(500)        NULL
    );

    CREATE INDEX IX_SalesInvoiceItem_InvoiceId ON dbo.SalesInvoiceItem (SalesInvoiceId);
    CREATE INDEX IX_SalesInvoiceItem_ProductId ON dbo.SalesInvoiceItem (ProductId);
END
GO

/* ---------------------------------------------------------------------------
   Optional seed: Permission screens for the Sales module (idempotent).
   Registers under the same purchase management structure if present, otherwise
   creates a minimal SALES module/domain/workspace on demand.
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Workspaces WHERE WorkspaceCode = 'SALES')
BEGIN
    INSERT INTO dbo.Workspaces (WorkspaceCode, WorkspaceName, Icon, Route, SortOrder, IsActive, CreatedBy)
    VALUES ('SALES', 'Sales', 'shopping-cart', '/sales', 2, 1, 'system');
END
GO
