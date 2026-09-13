/* =============================================================================
   ONE ERP - ERP Migration: Invoice Template Design
   Purpose  : Adds the Invoice Template Design module - a single common designer
              used for Retail, POS, Service, Wholesale, GST, Export and Purchase
              documents. One InvoiceType (what document), one PaperSize (physical
              size), one PrinterType (technology), one Template (how it looks),
              Assignment (which template), Version (lifecycle), Component
              (drag/drop block), Variable (data binding) and a Renderer that
              emits PDF / thermal print output from the published TemplateJson.
   Target DB: ONE ERP tenant database (run per tenant, database-per-tenant).
   Safe     : Re-runnable (IF NOT EXISTS guards on every object / column / FK;
              seeds insert row-by-row with existence checks).
   How      : Run with the tenant connection, e.g. via sqlcmd:
                sqlcmd -S LAPTOP-BOR8IKB8\MSSQL2022 -d <TENANT_DB> -E -i erp_migration_002_invoice_templates.sql
   Notes    : CompanyId / IndustryTypeId are INT to match the existing
              dbo.Companies.Id (INT) and dbo.IndustryTypes.IndustryTypeId (INT)
              so FK constraints can be declared; every new table PK is BIGINT.
   ============================================================================= */

/* =============================================================================
   01. InvoiceType - WHAT document is being printed
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceType]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceType (
        InvoiceTypeId BIGINT IDENTITY(1,1)  NOT NULL CONSTRAINT PK_InvoiceType PRIMARY KEY,
        Code          VARCHAR(30)            NOT NULL CONSTRAINT UQ_InvoiceType_Code UNIQUE,
        Name          VARCHAR(100)           NOT NULL,
        Description   VARCHAR(300)           NULL,
        DisplayOrder  INT                    NOT NULL CONSTRAINT DF_InvoiceType_DisplayOrder DEFAULT 0,
        IsActive      BIT                    NOT NULL CONSTRAINT DF_InvoiceType_IsActive DEFAULT 1,
        CreatedBy     BIGINT                 NULL,
        CreatedAt     DATETIME               NOT NULL CONSTRAINT DF_InvoiceType_CreatedAt DEFAULT GETDATE(),
        ModifiedBy    BIGINT                 NULL,
        ModifiedAt    DATETIME               NULL
    );

    CREATE INDEX IX_InvoiceType_IsActive ON dbo.InvoiceType (IsActive);
    CREATE INDEX IX_InvoiceType_DisplayOrder ON dbo.InvoiceType (DisplayOrder);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.InvoiceType WHERE Code = 'SALES')
BEGIN
    INSERT INTO dbo.InvoiceType (Code, Name, Description, DisplayOrder) VALUES
        ('SALES',          'Sales Invoice',        'Standard sales invoice',                        1),
        ('POS',            'POS Receipt',          'Cash counter / walk-in receipts (thermal)',      2),
        ('SERVICE',        'Service Invoice',      'Service delivery / job-work invoice',            3),
        ('HYBRID',         'Hybrid Invoice',       'Goods + services combined invoice',              4),
        ('WHOLESALE',      'Wholesale Invoice',    'Bulk / distribution invoice',                    5),
        ('EXPORT',         'Export Invoice',       'Export / cross-border invoice',                  6),
        ('PROFORMA',       'Proforma Invoice',     'Quotation-style advance invoice',                7),
        ('CREDIT_NOTE',    'Credit Note',          'Customer credit / returns adjustment',           8),
        ('DEBIT_NOTE',     'Debit Note',           'Debit / additional-charge note',                 9),
        ('SALES_RETURN',   'Sales Return',         'Sales return document',                          10),
        ('PURCHASE',       'Purchase Invoice',     'Vendor / supplier purchase invoice',             11),
        ('PURCHASE_RETURN','Purchase Return',      'Vendor return document',                         12);
END
;

/* =============================================================================
   02. InvoicePaperSize - physical page / roll size
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoicePaperSize]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoicePaperSize (
        PaperSizeId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoicePaperSize PRIMARY KEY,
        Code        VARCHAR(30)           NOT NULL CONSTRAINT UQ_InvoicePaperSize_Code UNIQUE,
        Name        VARCHAR(100)          NOT NULL,
        Width       DECIMAL(10,2)         NOT NULL,
        Height      DECIMAL(10,2)         NULL,
        Unit        VARCHAR(10)           NOT NULL CONSTRAINT DF_InvoicePaperSize_Unit DEFAULT 'MM',
        IsThermal   BIT                   NOT NULL CONSTRAINT DF_InvoicePaperSize_IsThermal DEFAULT 0,
        IsCustom    BIT                   NOT NULL CONSTRAINT DF_InvoicePaperSize_IsCustom DEFAULT 0,
        IsActive    BIT                   NOT NULL CONSTRAINT DF_InvoicePaperSize_IsActive DEFAULT 1,
        CreatedBy   BIGINT                NULL,
        CreatedAt   DATETIME              NOT NULL CONSTRAINT DF_InvoicePaperSize_CreatedAt DEFAULT GETDATE(),
        ModifiedBy  BIGINT                NULL,
        ModifiedAt  DATETIME              NULL
    );

    CREATE INDEX IX_InvoicePaperSize_IsActive ON dbo.InvoicePaperSize (IsActive);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.InvoicePaperSize WHERE Code = 'A4')
BEGIN
    INSERT INTO dbo.InvoicePaperSize (Code, Name, Width, Height, Unit, IsThermal, IsCustom) VALUES
        ('A4',      'A4',                210.00, 297.00, 'MM',  0, 0),
        ('A5',      'A5',                148.00, 210.00, 'MM',  0, 0),
        ('A6',      'A6',                105.00, 148.00, 'MM',  0, 0),
        ('58MM',    '58mm Thermal',       58.00,  NULL,   'MM',  1, 0),
        ('80MM',    '80mm Thermal',       80.00,  NULL,   'MM',  1, 0),
        ('LETTER',  'Letter',            215.90, 279.40, 'MM',  0, 0),
        ('LEGAL',   'Legal',             215.90, 355.60, 'MM',  0, 0),
        ('CUSTOM',  'Custom',              0.00,   0.00, 'MM',  0, 1);
END
;
-- Safety UPDATE so the rows created above always carry correct physics on re-runs.
UPDATE dbo.InvoicePaperSize SET Width = 210.00, Height = 297.00, Unit = 'MM', IsThermal = 0 WHERE Code = 'A4'    AND IsCustom = 0;
UPDATE dbo.InvoicePaperSize SET Width = 148.00, Height = 210.00, Unit = 'MM', IsThermal = 0 WHERE Code = 'A5'    AND IsCustom = 0;
UPDATE dbo.InvoicePaperSize SET Width = 58.00,  Height = NULL,   Unit = 'MM', IsThermal = 1 WHERE Code = '58MM'  AND IsCustom = 0;
UPDATE dbo.InvoicePaperSize SET Width = 80.00,  Height = NULL,   Unit = 'MM', IsThermal = 1 WHERE Code = '80MM'  AND IsCustom = 0;

/* =============================================================================
   03. PrinterType - printing technology
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PrinterType]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PrinterType (
        PrinterTypeId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PrinterType PRIMARY KEY,
        Code          VARCHAR(30)           NOT NULL CONSTRAINT UQ_PrinterType_Code UNIQUE,
        Name          VARCHAR(100)          NOT NULL,
        Description   VARCHAR(300)          NULL,
        IsActive      BIT                   NOT NULL CONSTRAINT DF_PrinterType_IsActive DEFAULT 1,
        CreatedBy     BIGINT                NULL,
        CreatedAt     DATETIME              NOT NULL CONSTRAINT DF_PrinterType_CreatedAt DEFAULT GETDATE(),
        ModifiedBy    BIGINT                NULL,
        ModifiedAt    DATETIME              NULL
    );

    CREATE INDEX IX_PrinterType_IsActive ON dbo.PrinterType (IsActive);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.PrinterType WHERE Code = 'THERMAL')
BEGIN
    INSERT INTO dbo.PrinterType (Code, Name, Description) VALUES
        ('THERMAL',    'Thermal',     'Heat-based direct thermal printing (receipt printers)'),
        ('LASER',      'Laser',       'Laser / LED page printing'),
        ('INKJET',     'Inkjet',      'Inkjet page printing'),
        ('DOT_MATRIX', 'Dot Matrix',  'Impact / dot matrix printing'),
        ('PDF',        'PDF',         'Render to PDF file (no physical printer)');
END
;

/* =============================================================================
   04. PrinterModel - optional physical printer models
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PrinterModel]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PrinterModel (
        PrinterModelId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PrinterModel PRIMARY KEY,
        PrinterTypeId  BIGINT               NOT NULL,
        Code           VARCHAR(50)           NOT NULL CONSTRAINT UQ_PrinterModel_Code UNIQUE,
        Name           VARCHAR(150)          NOT NULL,
        Manufacturer   VARCHAR(100)          NULL,
        IsActive       BIT                   NOT NULL CONSTRAINT DF_PrinterModel_IsActive DEFAULT 1,
        CreatedBy      BIGINT                NULL,
        CreatedAt      DATETIME              NOT NULL CONSTRAINT DF_PrinterModel_CreatedAt DEFAULT GETDATE(),
        ModifiedBy     BIGINT                NULL,
        ModifiedAt     DATETIME              NULL,
        CONSTRAINT FK_PrinterModel_PrinterType FOREIGN KEY (PrinterTypeId) REFERENCES dbo.PrinterType (PrinterTypeId)
    );

    CREATE INDEX IX_PrinterModel_PrinterTypeId ON dbo.PrinterModel (PrinterTypeId);
    CREATE INDEX IX_PrinterModel_IsActive ON dbo.PrinterModel (IsActive);
END
;

/* =============================================================================
   05. InvoiceTemplateCategory - classification of the template itself
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateCategory]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateCategory (
        TemplateCategoryId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateCategory PRIMARY KEY,
        Code               VARCHAR(30)           NOT NULL CONSTRAINT UQ_InvoiceTemplateCategory_Code UNIQUE,
        Name               VARCHAR(100)          NOT NULL,
        Description        VARCHAR(300)          NULL,
        DisplayOrder       INT                   NOT NULL CONSTRAINT DF_InvoiceTemplateCategory_DisplayOrder DEFAULT 0,
        IsActive           BIT                   NOT NULL CONSTRAINT DF_InvoiceTemplateCategory_IsActive DEFAULT 1,
        CreatedBy          BIGINT                NULL,
        CreatedAt          DATETIME              NOT NULL CONSTRAINT DF_InvoiceTemplateCategory_CreatedAt DEFAULT GETDATE(),
        ModifiedBy         BIGINT                NULL,
        ModifiedAt         DATETIME              NULL
    );

    CREATE INDEX IX_InvoiceTemplateCategory_IsActive ON dbo.InvoiceTemplateCategory (IsActive);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.InvoiceTemplateCategory WHERE Code = 'GENERAL')
BEGIN
    INSERT INTO dbo.InvoiceTemplateCategory (Code, Name, Description, DisplayOrder) VALUES
        ('GENERAL',  'General',  'Generic layout usable across document types', 1),
        ('RETAIL',   'Retail',   'Retail store layout',                         2),
        ('WHOLESALE','Wholesale','Wholesale / distribution layout',             3),
        ('SERVICE',  'Service',  'Service delivery layout',                     4),
        ('POS',      'POS',      'Point-of-sale thermal layout',                5),
        ('EXPORT',   'Export',   'Export documentation layout',                 6),
        ('GST',      'GST',      'GST / statutory compliance layout',           7);
END
;

/* =============================================================================
   06. InvoiceTemplateComponent - drag/drop building blocks
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateComponent]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateComponent (
        ComponentId    BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateComponent PRIMARY KEY,
        Code           VARCHAR(50)           NOT NULL CONSTRAINT UQ_InvoiceTemplateComponent_Code UNIQUE,
        Name           VARCHAR(100)          NOT NULL,
        ComponentType  VARCHAR(50)           NOT NULL,
        Description    VARCHAR(300)          NULL,
        DisplayOrder   INT                   NOT NULL CONSTRAINT DF_InvoiceTemplateComponent_DisplayOrder DEFAULT 0,
        IsActive       BIT                   NOT NULL CONSTRAINT DF_InvoiceTemplateComponent_IsActive DEFAULT 1,
        CreatedBy      BIGINT                NULL,
        CreatedAt      DATETIME              NOT NULL CONSTRAINT DF_InvoiceTemplateComponent_CreatedAt DEFAULT GETDATE(),
        ModifiedBy     BIGINT                NULL,
        ModifiedAt     DATETIME              NULL
    );

    CREATE INDEX IX_InvoiceTemplateComponent_ComponentType ON dbo.InvoiceTemplateComponent (ComponentType);
    CREATE INDEX IX_InvoiceTemplateComponent_IsActive ON dbo.InvoiceTemplateComponent (IsActive);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.InvoiceTemplateComponent WHERE Code = 'LOGO')
BEGIN
    INSERT INTO dbo.InvoiceTemplateComponent (Code, Name, ComponentType, Description, DisplayOrder) VALUES
        ('LOGO',            'Logo',            'IMAGE',     'Company logo image',                  1),
        ('COMPANY_HEADER',  'Company Header',  'TEXT',      'Company letter-head header',          2),
        ('COMPANY_NAME',    'Company Name',    'TEXT',      'Company name line',                   3),
        ('COMPANY_ADDRESS', 'Company Address', 'TEXT',      'Company address block',               4),
        ('COMPANY_CONTACT', 'Company Contact', 'TEXT',      'Phone / email line',                  5),
        ('GSTIN',           'GSTIN',           'TEXT',      'Company GSTIN / PAN line',            6),
        ('INVOICE_INFO',    'Invoice Info',    'TEXT',      'Invoice number / date / due date',    7),
        ('CUSTOMER',        'Customer',        'TEXT',      'Customer info block',                 8),
        ('CUSTOMER_ADDRESS','Customer Address','TEXT',      'Customer address block',              9),
        ('BILLING_ADDRESS', 'Billing Address', 'TEXT',      'Billing address block',               10),
        ('SHIPPING_ADDRESS','Shipping Address','TEXT',      'Shipping address block',              11),
        ('ITEM_TABLE',      'Item Table',      'TABLE',     'Line-item detail table',              12),
        ('DISCOUNT',        'Discount',        'TEXT',      'Discount line(s)',                    13),
        ('TAX',             'Tax',             'TEXT',      'Tax details',                         14),
        ('TAX_SUMMARY',     'Tax Summary',     'TABLE',     'Tax rate-wise summary table',         15),
        ('SUBTOTAL',        'Subtotal',        'TEXT',      'Subtotal line',                       16),
        ('TOTAL',           'Total',           'TEXT',      'Grand total line',                    17),
        ('PAYMENT',         'Payment',         'TEXT',      'Payment details',                     18),
        ('BANK_DETAILS',    'Bank Details',    'TEXT',      'Company bank account details',        19),
        ('QR_CODE',         'QR Code',         'QRCODE',    'QR code image block',                 20),
        ('BARCODE',         'Barcode',         'BARCODE',   'Barcode image block',                 21),
        ('TERMS',           'Terms',           'TEXT',      'Terms & conditions text',             22),
        ('NOTES',           'Notes',           'TEXT',      'Free-form notes',                     23),
        ('SIGNATURE',       'Signature',       'IMAGE',     'Signature image / line',              24),
        ('FOOTER',          'Footer',          'TEXT',      'Page footer text',                    25),
        ('CUSTOM_TEXT',     'Custom Text',     'TEXT',      'Custom static text',                  26),
        ('CUSTOM_IMAGE',    'Custom Image',    'IMAGE',     'Custom image block',                  27),
        ('DIVIDER',         'Divider',         'DIVIDER',   'Horizontal rule',                     28);
END
;

/* =============================================================================
   07. InvoiceTemplateVariable - data binding path dictionary
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateVariable]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateVariable (
        VariableId   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateVariable PRIMARY KEY,
        Code         VARCHAR(100)          NOT NULL CONSTRAINT UQ_InvoiceTemplateVariable_Code UNIQUE,
        Name         VARCHAR(150)          NOT NULL,
        BindingPath  VARCHAR(300)          NOT NULL,
        DataType     VARCHAR(30)           NOT NULL,
        Category     VARCHAR(50)           NULL,
        Description  VARCHAR(300)          NULL,
        IsCollection BIT                   NOT NULL CONSTRAINT DF_InvoiceTemplateVariable_IsCollection DEFAULT 0,
        IsActive     BIT                   NOT NULL CONSTRAINT DF_InvoiceTemplateVariable_IsActive DEFAULT 1,
        CreatedBy    BIGINT                NULL,
        CreatedAt    DATETIME              NOT NULL CONSTRAINT DF_InvoiceTemplateVariable_CreatedAt DEFAULT GETDATE(),
        ModifiedBy   BIGINT                NULL,
        ModifiedAt   DATETIME              NULL
    );

    CREATE INDEX IX_InvoiceTemplateVariable_Category ON dbo.InvoiceTemplateVariable (Category);
    CREATE INDEX IX_InvoiceTemplateVariable_IsActive ON dbo.InvoiceTemplateVariable (IsActive);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.InvoiceTemplateVariable WHERE BindingPath = 'Company.Name')
BEGIN
    INSERT INTO dbo.InvoiceTemplateVariable (Code, Name, BindingPath, DataType, Category, Description, IsCollection) VALUES
        ('Company.Name',            'Company Name',            'Company.Name',            'STRING', 'Company', 'Legal display name of the company',               0),
        ('Company.LegalName',       'Company Legal Name',      'Company.LegalName',       'STRING', 'Company', 'Registered legal name',                                 0),
        ('Company.Address',         'Company Address',         'Company.Address',         'STRING', 'Company', 'Registered address line',                               0),
        ('Company.GSTIN',           'Company GSTIN',           'Company.GSTIN',           'STRING', 'Company', 'GST identification number',                            0),
        ('Company.PAN',             'Company PAN',             'Company.PAN',             'STRING', 'Company', 'Permanent account number',                              0),
        ('Invoice.Number',          'Invoice Number',          'Invoice.Number',          'STRING', 'Invoice', 'Document number',                                      0),
        ('Invoice.Date',            'Invoice Date',            'Invoice.Date',            'DATETIME','Invoice', 'Document date',                                         0),
        ('Invoice.DueDate',         'Due Date',                'Invoice.DueDate',         'DATETIME','Invoice', 'Payment due date',                                      0),
        ('Invoice.Type',            'Invoice Type',            'Invoice.Type',            'STRING', 'Invoice', 'Document type label',                                   0),
        ('Invoice.SubTotal',        'Subtotal',                'Invoice.SubTotal',        'DECIMAL', 'Invoice', 'Subtotal amount',                                       0),
        ('Invoice.Discount',        'Discount',                'Invoice.Discount',        'DECIMAL', 'Invoice', 'Total discount amount',                                 0),
        ('Invoice.Tax',             'Tax',                     'Invoice.Tax',             'DECIMAL', 'Invoice', 'Total tax amount',                                      0),
        ('Invoice.GrandTotal',      'Grand Total',             'Invoice.GrandTotal',      'DECIMAL', 'Invoice', 'Grand total (amount due)',                              0),
        ('Customer.Name',           'Customer Name',           'Customer.Name',           'STRING', 'Customer', 'Bill-to customer name',                                 0),
        ('Customer.Address',        'Customer Address',        'Customer.Address',        'STRING', 'Customer', 'Bill-to address',                                       0),
        ('Customer.GSTIN',          'Customer GSTIN',          'Customer.GSTIN',          'STRING', 'Customer', 'Customer GST identification number',                   0),
        ('Payment.Method',          'Payment Method',          'Payment.Method',          'STRING', 'Payment',  'Payment method label',                                    0),
        ('Payment.Amount',          'Payment Amount',          'Payment.Amount',          'DECIMAL', 'Payment',  'Amount paid',                                           0),
        ('Item.ProductName',        'Product Name',            'Item.ProductName',        'STRING', 'Item',     'Product / service description',                          0),
        ('Item.SKU',                'SKU',                     'Item.SKU',                'STRING', 'Item',     'Stock keeping unit code',                                0),
        ('Item.HSNSAC',             'HSN / SAC',               'Item.HSNSAC',             'STRING', 'Item',     'HSN / SAC code',                                        0),
        ('Item.Unit',               'Unit',                    'Item.Unit',               'STRING', 'Item',     'Unit of measure',                                       0),
        ('Item.Quantity',           'Quantity',                'Item.Quantity',           'DECIMAL', 'Item',     'Quantity',                                              0),
        ('Item.Rate',               'Rate',                    'Item.Rate',               'DECIMAL', 'Item',     'Unit rate',                                             0),
        ('Item.Discount',           'Item Discount',           'Item.Discount',           'DECIMAL', 'Item',     'Line discount amount',                                  0),
        ('Item.Tax',                'Item Tax',                'Item.Tax',                'DECIMAL', 'Item',     'Line tax amount',                                       0),
        ('Item.Amount',             'Item Amount',             'Item.Amount',             'DECIMAL', 'Item',     'Line amount',                                           0);
END
;

/* =============================================================================
   08. InvoiceFont - seeded font choices (FontFileId reserved for font files)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceFont]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceFont (
        FontId       BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceFont PRIMARY KEY,
        Code         VARCHAR(50)           NOT NULL CONSTRAINT UQ_InvoiceFont_Code UNIQUE,
        Name         VARCHAR(100)          NOT NULL,
        FontFamily   VARCHAR(100)          NOT NULL,
        FontFileId   BIGINT                NULL,
        IsActive     BIT                   NOT NULL CONSTRAINT DF_InvoiceFont_IsActive DEFAULT 1,
        CreatedBy    BIGINT                NULL,
        CreatedAt    DATETIME              NOT NULL CONSTRAINT DF_InvoiceFont_CreatedAt DEFAULT GETDATE(),
        ModifiedBy   BIGINT                NULL,
        ModifiedAt   DATETIME              NULL
    );

    CREATE INDEX IX_InvoiceFont_IsActive ON dbo.InvoiceFont (IsActive);
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.InvoiceFont WHERE Code = 'ARIAL')
BEGIN
    INSERT INTO dbo.InvoiceFont (Code, Name, FontFamily) VALUES
        ('ARIAL',       'Arial',       'Arial'),
        ('ROBOTO',      'Roboto',      'Roboto'),
        ('INTER',       'Inter',       'Inter'),
        ('TAHOMA',      'Tahoma',      'Tahoma'),
        ('COURIER_NEW', 'Courier New', 'Courier New');
END
;

/* =============================================================================
   09. PrintOrientation
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PrintOrientation]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PrintOrientation (
        OrientationId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PrintOrientation PRIMARY KEY,
        Code          VARCHAR(20)           NOT NULL CONSTRAINT UQ_PrintOrientation_Code UNIQUE,
        Name          VARCHAR(50)           NOT NULL,
        IsActive      BIT                   NOT NULL CONSTRAINT DF_PrintOrientation_IsActive DEFAULT 1
    );
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.PrintOrientation WHERE Code = 'PORTRAIT')
BEGIN
    INSERT INTO dbo.PrintOrientation (Code, Name) VALUES
        ('PORTRAIT', 'Portrait'),
        ('LANDSCAPE','Landscape');
END
;

/* =============================================================================
   10. PrintUnit
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PrintUnit]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PrintUnit (
        UnitId   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PrintUnit PRIMARY KEY,
        Code     VARCHAR(20)           NOT NULL CONSTRAINT UQ_PrintUnit_Code UNIQUE,
        Name     VARCHAR(50)           NOT NULL,
        IsActive BIT                   NOT NULL CONSTRAINT DF_PrintUnit_IsActive DEFAULT 1
    );
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.PrintUnit WHERE Code = 'MM')
BEGIN
    INSERT INTO dbo.PrintUnit (Code, Name) VALUES
        ('MM',   'Millimeter'),
        ('PX',   'Pixel'),
        ('INCH', 'Inch');
END
;

/* =============================================================================
   11. InvoiceTemplate - the template header
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplate]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplate (
        InvoiceTemplateId   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplate PRIMARY KEY,
        CompanyId           INT                  NULL,
        TemplateCategoryId  BIGINT               NULL,
        InvoiceTypeId       BIGINT               NOT NULL,
        PaperSizeId         BIGINT               NOT NULL,
        OrientationId       BIGINT               NOT NULL,
        Code                VARCHAR(50)          NOT NULL,
        Name                VARCHAR(150)         NOT NULL,
        Description         VARCHAR(500)         NULL,
        Width               DECIMAL(10,2)        NULL,
        Height              DECIMAL(10,2)        NULL,
        IsDefault           BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplate_IsDefault DEFAULT 0,
        IsActive            BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplate_IsActive DEFAULT 1,
        CreatedBy           BIGINT               NULL,
        CreatedAt           DATETIME             NOT NULL CONSTRAINT DF_InvoiceTemplate_CreatedAt DEFAULT GETDATE(),
        ModifiedBy          BIGINT               NULL,
        ModifiedAt          DATETIME             NULL,
        CONSTRAINT FK_InvoiceTemplate_InvoiceType FOREIGN KEY (InvoiceTypeId) REFERENCES dbo.InvoiceType (InvoiceTypeId),
        CONSTRAINT FK_InvoiceTemplate_PaperSize FOREIGN KEY (PaperSizeId) REFERENCES dbo.InvoicePaperSize (PaperSizeId),
        CONSTRAINT FK_InvoiceTemplate_Orientation FOREIGN KEY (OrientationId) REFERENCES dbo.PrintOrientation (OrientationId),
        CONSTRAINT FK_InvoiceTemplate_Category FOREIGN KEY (TemplateCategoryId) REFERENCES dbo.InvoiceTemplateCategory (TemplateCategoryId),
        CONSTRAINT UQ_InvoiceTemplate_Company_Code UNIQUE (CompanyId, Code)
    );

    CREATE INDEX IX_InvoiceTemplate_CompanyId ON dbo.InvoiceTemplate (CompanyId);
    CREATE INDEX IX_InvoiceTemplate_InvoiceTypeId ON dbo.InvoiceTemplate (InvoiceTypeId);
    CREATE INDEX IX_InvoiceTemplate_PaperSizeId ON dbo.InvoiceTemplate (PaperSizeId);
    CREATE INDEX IX_InvoiceTemplate_IsActive ON dbo.InvoiceTemplate (IsActive);
END
;

/* =============================================================================
   12. InvoiceTemplateAssignment - decides which template is used at runtime
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateAssignment]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateAssignment (
        AssignmentId       BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateAssignment PRIMARY KEY,
        InvoiceTemplateId  BIGINT               NOT NULL,
        CompanyId          INT                  NULL,
        IndustryTypeId     INT                  NULL,
        InvoiceTypeId      BIGINT               NULL,
        PaperSizeId        BIGINT               NULL,
        IsDefault          BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateAssignment_IsDefault DEFAULT 0,
        IsActive           BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateAssignment_IsActive DEFAULT 1,
        CreatedBy          BIGINT               NULL,
        CreatedAt          DATETIME             NOT NULL CONSTRAINT DF_InvoiceTemplateAssignment_CreatedAt DEFAULT GETDATE(),
        ModifiedBy         BIGINT               NULL,
        ModifiedAt         DATETIME             NULL,
        CONSTRAINT FK_InvoiceTemplateAssignment_Template FOREIGN KEY (InvoiceTemplateId) REFERENCES dbo.InvoiceTemplate (InvoiceTemplateId),
        CONSTRAINT FK_InvoiceTemplateAssignment_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Companies (Id),
        CONSTRAINT FK_InvoiceTemplateAssignment_IndustryType FOREIGN KEY (IndustryTypeId) REFERENCES dbo.IndustryTypes (IndustryTypeId),
        CONSTRAINT FK_InvoiceTemplateAssignment_InvoiceType FOREIGN KEY (InvoiceTypeId) REFERENCES dbo.InvoiceType (InvoiceTypeId),
        CONSTRAINT FK_InvoiceTemplateAssignment_PaperSize FOREIGN KEY (PaperSizeId) REFERENCES dbo.InvoicePaperSize (PaperSizeId)
    );

    CREATE INDEX IX_InvoiceTemplateAssignment_TemplateId ON dbo.InvoiceTemplateAssignment (InvoiceTemplateId);
    CREATE INDEX IX_InvoiceTemplateAssignment_Lookup ON dbo.InvoiceTemplateAssignment (CompanyId, IndustryTypeId, InvoiceTypeId, PaperSizeId);
END
;

/* =============================================================================
   13. InvoiceTemplateVersion - template lifecycle (DRAFT -> PUBLISHED -> ARCHIVED)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateVersion]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateVersion (
        TemplateVersionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateVersion PRIMARY KEY,
        InvoiceTemplateId BIGINT               NOT NULL,
        VersionNumber     INT                  NOT NULL,
        TemplateJson      NVARCHAR(MAX)        NOT NULL,
        Status            VARCHAR(20)          NOT NULL CONSTRAINT DF_InvoiceTemplateVersion_Status DEFAULT 'DRAFT',
        IsPublished       BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateVersion_IsPublished DEFAULT 0,
        CreatedBy         BIGINT               NULL,
        CreatedAt         DATETIME             NOT NULL CONSTRAINT DF_InvoiceTemplateVersion_CreatedAt DEFAULT GETDATE(),
        PublishedBy       BIGINT               NULL,
        PublishedAt       DATETIME             NULL,
        CONSTRAINT FK_InvoiceTemplateVersion_Template FOREIGN KEY (InvoiceTemplateId) REFERENCES dbo.InvoiceTemplate (InvoiceTemplateId),
        CONSTRAINT UQ_InvoiceTemplateVersion_Template_Number UNIQUE (InvoiceTemplateId, VersionNumber)
    );

    CREATE INDEX IX_InvoiceTemplateVersion_TemplateId ON dbo.InvoiceTemplateVersion (InvoiceTemplateId);
    CREATE INDEX IX_InvoiceTemplateVersion_IsPublished ON dbo.InvoiceTemplateVersion (IsPublished);
END
;

/* =============================================================================
   14. InvoiceTemplateSection - region of the canvas inside a version
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateSection]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateSection (
        SectionId          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateSection PRIMARY KEY,
        TemplateVersionId  BIGINT               NOT NULL,
        SectionCode        VARCHAR(50)          NOT NULL,
        SectionName        VARCHAR(100)         NOT NULL,
        DisplayOrder       INT                  NOT NULL CONSTRAINT DF_InvoiceTemplateSection_DisplayOrder DEFAULT 0,
        X                  DECIMAL(10,2)        NULL,
        Y                  DECIMAL(10,2)        NULL,
        Width              DECIMAL(10,2)        NULL,
        Height             DECIMAL(10,2)        NULL,
        IsVisible          BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateSection_IsVisible DEFAULT 1,
        CONSTRAINT FK_InvoiceTemplateSection_Version FOREIGN KEY (TemplateVersionId) REFERENCES dbo.InvoiceTemplateVersion (TemplateVersionId)
    );

    CREATE INDEX IX_InvoiceTemplateSection_VersionId ON dbo.InvoiceTemplateSection (TemplateVersionId);
END
;

/* =============================================================================
   15. InvoiceTemplateElement - one drag/dropped component instance
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateElement]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateElement (
        ElementId      BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateElement PRIMARY KEY,
        SectionId      BIGINT               NOT NULL,
        ComponentId    BIGINT               NOT NULL,
        ElementType    VARCHAR(50)          NOT NULL,
        ElementName    VARCHAR(100)         NULL,
        X              DECIMAL(10,2)        NULL,
        Y              DECIMAL(10,2)        NULL,
        Width          DECIMAL(10,2)        NULL,
        Height         DECIMAL(10,2)        NULL,
        DisplayOrder   INT                  NOT NULL CONSTRAINT DF_InvoiceTemplateElement_DisplayOrder DEFAULT 0,
        IsVisible      BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateElement_IsVisible DEFAULT 1,
        CONSTRAINT FK_InvoiceTemplateElement_Section FOREIGN KEY (SectionId) REFERENCES dbo.InvoiceTemplateSection (SectionId),
        CONSTRAINT FK_InvoiceTemplateElement_Component FOREIGN KEY (ComponentId) REFERENCES dbo.InvoiceTemplateComponent (ComponentId)
    );

    CREATE INDEX IX_InvoiceTemplateElement_SectionId ON dbo.InvoiceTemplateElement (SectionId);
    CREATE INDEX IX_InvoiceTemplateElement_ComponentId ON dbo.InvoiceTemplateElement (ComponentId);
END
;

/* =============================================================================
   16. InvoiceTemplateField - data binding (element -> variable)
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateField]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateField (
        TemplateFieldId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateField PRIMARY KEY,
        ElementId       BIGINT               NOT NULL,
        VariableId      BIGINT               NOT NULL,
        FieldName       VARCHAR(100)         NOT NULL,
        BindingPath     VARCHAR(300)         NOT NULL,
        Label           VARCHAR(100)         NULL,
        IsVisible       BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateField_IsVisible DEFAULT 1,
        CONSTRAINT FK_InvoiceTemplateField_Element FOREIGN KEY (ElementId) REFERENCES dbo.InvoiceTemplateElement (ElementId),
        CONSTRAINT FK_InvoiceTemplateField_Variable FOREIGN KEY (VariableId) REFERENCES dbo.InvoiceTemplateVariable (VariableId)
    );

    CREATE INDEX IX_InvoiceTemplateField_ElementId ON dbo.InvoiceTemplateField (ElementId);
    CREATE INDEX IX_InvoiceTemplateField_VariableId ON dbo.InvoiceTemplateField (VariableId);
END
;

/* =============================================================================
   17. InvoiceTemplateItemColumn - item table column definitions
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateItemColumn]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateItemColumn (
        ItemColumnId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateItemColumn PRIMARY KEY,
        ElementId    BIGINT               NOT NULL,
        FieldName    VARCHAR(100)         NOT NULL,
        HeaderText   VARCHAR(100)         NOT NULL,
        DisplayOrder INT                  NOT NULL,
        Width        DECIMAL(10,2)        NULL,
        Alignment    VARCHAR(20)          NOT NULL CONSTRAINT DF_InvoiceTemplateItemColumn_Alignment DEFAULT 'LEFT',
        IsVisible    BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateItemColumn_IsVisible DEFAULT 1,
        CONSTRAINT FK_InvoiceTemplateItemColumn_Element FOREIGN KEY (ElementId) REFERENCES dbo.InvoiceTemplateElement (ElementId)
    );

    CREATE INDEX IX_InvoiceTemplateItemColumn_ElementId ON dbo.InvoiceTemplateItemColumn (ElementId);
END
;

/* =============================================================================
   18. InvoiceTemplateStyle - visual style for one element
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplateStyle]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplateStyle (
        StyleId        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplateStyle PRIMARY KEY,
        ElementId      BIGINT               NOT NULL,
        FontId         BIGINT               NULL,
        FontSize       DECIMAL(6,2)         NULL,
        FontWeight     VARCHAR(20)          NULL,
        TextAlign      VARCHAR(20)          NULL,
        VerticalAlign  VARCHAR(20)          NULL,
        PaddingTop     DECIMAL(8,2)         NULL,
        PaddingRight   DECIMAL(8,2)         NULL,
        PaddingBottom  DECIMAL(8,2)         NULL,
        PaddingLeft    DECIMAL(8,2)         NULL,
        BorderTop      BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateStyle_BorderTop DEFAULT 0,
        BorderRight    BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateStyle_BorderRight DEFAULT 0,
        BorderBottom   BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateStyle_BorderBottom DEFAULT 0,
        BorderLeft     BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplateStyle_BorderLeft DEFAULT 0,
        CONSTRAINT FK_InvoiceTemplateStyle_Element FOREIGN KEY (ElementId) REFERENCES dbo.InvoiceTemplateElement (ElementId),
        CONSTRAINT FK_InvoiceTemplateStyle_Font FOREIGN KEY (FontId) REFERENCES dbo.InvoiceFont (FontId),
        CONSTRAINT UQ_InvoiceTemplateStyle_Element UNIQUE (ElementId)
    );

    CREATE INDEX IX_InvoiceTemplateStyle_FontId ON dbo.InvoiceTemplateStyle (FontId);
END
;

/* =============================================================================
   19. InvoiceTemplatePrinter - printer binding for a template
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplatePrinter]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplatePrinter (
        TemplatePrinterId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplatePrinter PRIMARY KEY,
        InvoiceTemplateId BIGINT               NOT NULL,
        PaperSizeId       BIGINT               NOT NULL,
        PrinterTypeId     BIGINT               NOT NULL,
        PrinterModelId    BIGINT               NULL,
        PrinterName       VARCHAR(200)         NULL,
        IsDefault         BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplatePrinter_IsDefault DEFAULT 0,
        IsActive          BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplatePrinter_IsActive DEFAULT 1,
        CreatedBy         BIGINT               NULL,
        CreatedAt         DATETIME             NOT NULL CONSTRAINT DF_InvoiceTemplatePrinter_CreatedAt DEFAULT GETDATE(),
        ModifiedBy        BIGINT               NULL,
        ModifiedAt        DATETIME             NULL,
        CONSTRAINT FK_InvoiceTemplatePrinter_Template FOREIGN KEY (InvoiceTemplateId) REFERENCES dbo.InvoiceTemplate (InvoiceTemplateId),
        CONSTRAINT FK_InvoiceTemplatePrinter_PaperSize FOREIGN KEY (PaperSizeId) REFERENCES dbo.InvoicePaperSize (PaperSizeId),
        CONSTRAINT FK_InvoiceTemplatePrinter_PrinterType FOREIGN KEY (PrinterTypeId) REFERENCES dbo.PrinterType (PrinterTypeId),
        CONSTRAINT FK_InvoiceTemplatePrinter_PrinterModel FOREIGN KEY (PrinterModelId) REFERENCES dbo.PrinterModel (PrinterModelId)
    );

    CREATE INDEX IX_InvoiceTemplatePrinter_TemplateId ON dbo.InvoiceTemplatePrinter (InvoiceTemplateId);
    CREATE INDEX IX_InvoiceTemplatePrinter_PaperSizeId ON dbo.InvoiceTemplatePrinter (PaperSizeId);
END
;

/* =============================================================================
   20. InvoiceTemplatePrintSetting - per-printer print defaults
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InvoiceTemplatePrintSetting]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.InvoiceTemplatePrintSetting (
        PrintSettingId   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplatePrintSetting PRIMARY KEY,
        TemplatePrinterId BIGINT              NOT NULL,
        MarginTop        DECIMAL(10,2)        NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_MarginTop DEFAULT 0,
        MarginRight      DECIMAL(10,2)        NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_MarginRight DEFAULT 0,
        MarginBottom     DECIMAL(10,2)        NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_MarginBottom DEFAULT 0,
        MarginLeft       DECIMAL(10,2)        NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_MarginLeft DEFAULT 0,
        Scale            DECIMAL(6,2)         NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_Scale DEFAULT 100,
        Copies           INT                  NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_Copies DEFAULT 1,
        AutoFit          BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_AutoFit DEFAULT 1,
        CutPaper         BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_CutPaper DEFAULT 0,
        PrintHeader      BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_PrintHeader DEFAULT 1,
        PrintFooter      BIT                  NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_PrintFooter DEFAULT 1,
        CreatedBy        BIGINT               NULL,
        CreatedAt        DATETIME             NOT NULL CONSTRAINT DF_InvoiceTemplatePrintSetting_CreatedAt DEFAULT GETDATE(),
        ModifiedBy       BIGINT               NULL,
        ModifiedAt       DATETIME             NULL,
        CONSTRAINT FK_InvoiceTemplatePrintSetting_Printer FOREIGN KEY (TemplatePrinterId) REFERENCES dbo.InvoiceTemplatePrinter (TemplatePrinterId)
    );

    CREATE INDEX IX_InvoiceTemplatePrintSetting_TemplatePrinterId ON dbo.InvoiceTemplatePrintSetting (TemplatePrinterId);
END
;

/* =============================================================================
   VERIFICATION
   ============================================================================= */
SELECT 'InvoiceType' AS MasterName, COUNT(*) AS Rows FROM dbo.InvoiceType
UNION ALL SELECT 'InvoicePaperSize', COUNT(*) FROM dbo.InvoicePaperSize
UNION ALL SELECT 'PrinterType', COUNT(*) FROM dbo.PrinterType
UNION ALL SELECT 'PrinterModel', COUNT(*) FROM dbo.PrinterModel
UNION ALL SELECT 'InvoiceTemplateCategory', COUNT(*) FROM dbo.InvoiceTemplateCategory
UNION ALL SELECT 'InvoiceTemplateComponent', COUNT(*) FROM dbo.InvoiceTemplateComponent
UNION ALL SELECT 'InvoiceTemplateVariable', COUNT(*) FROM dbo.InvoiceTemplateVariable
UNION ALL SELECT 'InvoiceFont', COUNT(*) FROM dbo.InvoiceFont
UNION ALL SELECT 'PrintOrientation', COUNT(*) FROM dbo.PrintOrientation
UNION ALL SELECT 'PrintUnit', COUNT(*) FROM dbo.PrintUnit;