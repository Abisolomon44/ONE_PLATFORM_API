/* Tenant Purchase/Sale/Billing configuration settings (single-table design) */
CREATE TABLE dbo.TenantConfiguration
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,

    TenantId BIGINT NOT NULL,

    ApplicationType VARCHAR(30) NOT NULL,
    TransactionType VARCHAR(30) NULL,
    FlowType VARCHAR(50) NULL,

    PageCode VARCHAR(50) NULL,
    FieldCode VARCHAR(50) NULL,

    SequenceNo INT NULL,

    IsPageEnabled BIT NOT NULL DEFAULT 1,
    IsVisible BIT NOT NULL DEFAULT 1,
    IsRequired BIT NOT NULL DEFAULT 0,
    IsReadonly BIT NOT NULL DEFAULT 0,

    DisplayOrder INT NULL,

    DefaultValue VARCHAR(200) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    UpdatedBy BIGINT NULL,
    UpdatedAt DATETIME2 NULL
);
GO

CREATE INDEX IX_TenantConfiguration_Tenant
ON dbo.TenantConfiguration (TenantId);
GO

CREATE INDEX IX_TenantConfiguration_Flow
ON dbo.TenantConfiguration
(
    TenantId,
    ApplicationType,
    TransactionType,
    FlowType
);
GO

CREATE INDEX IX_TenantConfiguration_Page
ON dbo.TenantConfiguration
(
    TenantId,
    PageCode
);
GO

INSERT INTO dbo.TenantConfiguration
(
    TenantId,
    ApplicationType,
    TransactionType,
    FlowType,
    PageCode,
    FieldCode,
    SequenceNo,
    IsPageEnabled,
    IsVisible,
    IsRequired,
    IsReadonly,
    DisplayOrder,
    IsActive,
    CreatedBy
)
VALUES
(1001,'ERP_BILLING','PURCHASE','PO_GRN_INVOICE','PURCHASE_ORDER','SUPPLIER',1,1,1,1,0,1,1,1),
(1001,'ERP_BILLING','PURCHASE','PO_GRN_INVOICE','PURCHASE_ORDER','ORDER_DATE',1,1,1,1,0,2,1,1),
(1001,'ERP_BILLING','PURCHASE','PO_GRN_INVOICE','PURCHASE_ORDER','EXPECTED_DATE',1,1,1,0,0,3,1,1),
(1001,'ERP_BILLING','PURCHASE','PO_GRN_INVOICE','PURCHASE_ORDER','PRODUCT',1,1,1,1,0,4,1,1),
(1001,'ERP_BILLING','PURCHASE','PO_GRN_INVOICE','PURCHASE_ORDER','QUANTITY',1,1,1,1,0,5,1,1),
(1001,'ERP_BILLING','PURCHASE','PO_GRN_INVOICE','GRN','RECEIVED_QTY',2,1,1,1,0,1,1,1),
(1001,'ERP_BILLING','PURCHASE','PO_GRN_INVOICE','GRN','REJECTED_QTY',2,1,1,0,0,2,1,1),
(1001,'ERP_BILLING','PURCHASE','PO_GRN_INVOICE','PURCHASE_INVOICE','SUPPLIER_INVOICE_NO',3,1,1,1,0,1,1,1),
(1001,'ERP_BILLING','SALES','SO_DELIVERY_INVOICE','SALES_ORDER','CUSTOMER',1,1,1,1,0,1,1,1),
(1001,'ERP_BILLING','SALES','SO_DELIVERY_INVOICE','DELIVERY','DELIVERED_QTY',2,1,1,1,0,1,1,1),
(1001,'ERP_BILLING','SALES','SO_DELIVERY_INVOICE','SALES_INVOICE','PAYMENT_TERMS',3,1,1,0,0,1,1,1),
(1001,'ERP_BILLING','BILLING','POS','POS_BILLING','BARCODE',1,1,1,0,0,1,1,1),
(1001,'ERP_BILLING','BILLING','POS','POS_BILLING','PRODUCT',1,1,1,1,0,2,1,1),
(1001,'ERP_BILLING','BILLING','POS','POS_BILLING','QUANTITY',1,1,1,1,0,3,1,1),
(1001,'ERP_BILLING','BILLING','POS','POS_BILLING','PAYMENT_METHOD',2,1,1,1,0,1,1,1);
GO
