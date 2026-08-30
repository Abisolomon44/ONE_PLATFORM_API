CREATE TABLE dbo.Purchase
(
    PurchaseId BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Purchase PRIMARY KEY,

    CompanyId BIGINT NOT NULL,
    BranchId BIGINT NULL,
    WarehouseId BIGINT NOT NULL,
    SupplierId BIGINT NOT NULL,

    PurchaseNo VARCHAR(30) NOT NULL,
    SupplierInvoiceNo VARCHAR(50) NULL,

    PurchaseDate DATE NOT NULL,
    PaymentType VARCHAR(20) NOT NULL,

    SubTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_SubTotal DEFAULT 0,
    DiscountAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_Discount DEFAULT 0,
    TaxableAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_Taxable DEFAULT 0,
    TaxAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_Tax DEFAULT 0,
    RoundOff DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_RoundOff DEFAULT 0,
    GrandTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_GrandTotal DEFAULT 0,
    PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_Paid DEFAULT 0,
    BalanceAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Purchase_Balance DEFAULT 0,

    Status VARCHAR(20) NOT NULL CONSTRAINT DF_Purchase_Status DEFAULT 'DRAFT',

    Remarks VARCHAR(500) NULL,

    CreatedBy BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Purchase_CreatedAt DEFAULT GETDATE(),
    UpdatedBy BIGINT NULL,
    UpdatedAt DATETIME2 NULL
);
GO

CREATE UNIQUE INDEX UX_Purchase_Company_PurchaseNo
ON dbo.Purchase (CompanyId, PurchaseNo);

CREATE INDEX IX_Purchase_Company_Supplier
ON dbo.Purchase (CompanyId, SupplierId);

CREATE INDEX IX_Purchase_Company_Date
ON dbo.Purchase (CompanyId, PurchaseDate);

CREATE INDEX IX_Purchase_Company_Status
ON dbo.Purchase (CompanyId, Status);
GO

CREATE TABLE dbo.PurchaseItem
(
    PurchaseItemId BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_PurchaseItem PRIMARY KEY,

    PurchaseId BIGINT NOT NULL,

    ProductId BIGINT NOT NULL,
    UOMId BIGINT NOT NULL,

    Quantity DECIMAL(18,3) NOT NULL,
    PurchaseRate DECIMAL(18,2) NOT NULL,

    DiscountPercent DECIMAL(5,2) NOT NULL CONSTRAINT DF_PurchaseItem_DiscountPercent DEFAULT 0,
    DiscountAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItem_DiscountAmount DEFAULT 0,

    TaxId BIGINT NULL,

    TaxPercent DECIMAL(5,2) NOT NULL CONSTRAINT DF_PurchaseItem_TaxPercent DEFAULT 0,
    TaxAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItem_TaxAmount DEFAULT 0,

    LineTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_PurchaseItem_LineTotal DEFAULT 0,

    CONSTRAINT FK_PurchaseItem_Purchase
        FOREIGN KEY (PurchaseId)
        REFERENCES dbo.Purchase(PurchaseId)
);
GO

CREATE INDEX IX_PurchaseItem_Purchase
ON dbo.PurchaseItem (PurchaseId);
GO
