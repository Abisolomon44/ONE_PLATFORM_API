/* =============================================================================
   ONE ERP - ERP Migration: Purchase Table Extra Columns
   Purpose  : Adds forward-looking / optional columns to dbo.Purchase,
              dbo.PurchaseItem, dbo.PurchaseReturn and dbo.PurchaseReturnItem
              (audit cancel fields, order/receipt references, quantity tracking,
              tax/cess references, batch/serial and navigation FK columns).
              New columns are all NULL (no existing data touched).
   Target DB: ONE ERP tenant database (run per tenant, database-per-tenant).
   Safe     : Re-runnable - every column guarded with sys.columns existence
              check, every FK guarded with sys.objects NOT EXISTS + target
              table/column verification.
   How      : Run with the tenant connection, e.g. via sqlcmd:
                sqlcmd -S LAPTOP-BOR8IKB8\MSSQL2022 -d <TENANT_DB> -E -i erp_migration_003_purchase_columns.sql
   Notes    : FK datatypes verified against existing masters:
                dbo.Taxes.Id            BIGINT
                dbo.FinancialYear.FinancialYearId BIGINT
                dbo.PaymentType.PaymentTypeId     BIGINT
                dbo.PaymentMethod.PaymentMethodId BIGINT
              dbo.Currencies.Id is INT, so CurrencyId (BIGINT) FK is NOT
              created (datatype mismatch rule) - intended for future use.
              PurchaseType / PurchaseOrder / PurchaseOrderItem / GRN /
              PurchaseReturnType / ReturnReason / Cess master tables do NOT
              exist, so their FKs are skipped.
   ============================================================================= */

/* =============================================================================
   01. dbo.Purchase - optional column additions
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Purchase') AND name = N'SupplierPONumber')
BEGIN
    ALTER TABLE dbo.Purchase ADD SupplierPONumber NVARCHAR(50) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Purchase') AND name = N'ReferenceNumber')
BEGIN
    ALTER TABLE dbo.Purchase ADD ReferenceNumber NVARCHAR(50) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Purchase') AND name = N'CurrencyId')
BEGIN
    ALTER TABLE dbo.Purchase ADD CurrencyId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Purchase') AND name = N'PurchaseTypeId')
BEGIN
    ALTER TABLE dbo.Purchase ADD PurchaseTypeId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Purchase') AND name = N'AccountingYearId')
BEGIN
    ALTER TABLE dbo.Purchase ADD AccountingYearId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Purchase') AND name = N'TaxId')
BEGIN
    ALTER TABLE dbo.Purchase ADD TaxId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Purchase') AND name = N'IsGSTInclusive')
BEGIN
    ALTER TABLE dbo.Purchase ADD IsGSTInclusive BIT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Purchase') AND name = N'CancelledByUserID')
BEGIN
    ALTER TABLE dbo.Purchase ADD CancelledByUserID BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Purchase') AND name = N'CancelledAt')
BEGIN
    ALTER TABLE dbo.Purchase ADD CancelledAt DATETIME2 NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Purchase') AND name = N'CancellationReason')
BEGIN
    ALTER TABLE dbo.Purchase ADD CancellationReason NVARCHAR(500) NULL;
END;
GO

/* =============================================================================
   02. dbo.PurchaseItem - reference + quantity tracking columns
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseItem') AND name = N'TaxId')
BEGIN
    ALTER TABLE dbo.PurchaseItem ADD TaxId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseItem') AND name = N'CessId')
BEGIN
    ALTER TABLE dbo.PurchaseItem ADD CessId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseItem') AND name = N'OrderedQuantity')
BEGIN
    ALTER TABLE dbo.PurchaseItem ADD OrderedQuantity DECIMAL(18,3) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseItem') AND name = N'ReceivedQuantity')
BEGIN
    ALTER TABLE dbo.PurchaseItem ADD ReceivedQuantity DECIMAL(18,3) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseItem') AND name = N'ReturnedQuantity')
BEGIN
    ALTER TABLE dbo.PurchaseItem ADD ReturnedQuantity DECIMAL(18,3) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseItem') AND name = N'RemainingQuantity')
BEGIN
    ALTER TABLE dbo.PurchaseItem ADD RemainingQuantity DECIMAL(18,3) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseItem') AND name = N'PurchaseOrderId')
BEGIN
    ALTER TABLE dbo.PurchaseItem ADD PurchaseOrderId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseItem') AND name = N'PurchaseOrderItemId')
BEGIN
    ALTER TABLE dbo.PurchaseItem ADD PurchaseOrderItemId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseItem') AND name = N'GRNId')
BEGIN
    ALTER TABLE dbo.PurchaseItem ADD GRNId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseItem') AND name = N'BatchNumber')
BEGIN
    ALTER TABLE dbo.PurchaseItem ADD BatchNumber NVARCHAR(100) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseItem') AND name = N'SerialNumber')
BEGIN
    ALTER TABLE dbo.PurchaseItem ADD SerialNumber NVARCHAR(100) NULL;
END;
GO

/* =============================================================================
   03. dbo.PurchaseReturn - optional column additions
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturn') AND name = N'SupplierInvoiceNumber')
BEGIN
    ALTER TABLE dbo.PurchaseReturn ADD SupplierInvoiceNumber NVARCHAR(50) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturn') AND name = N'ReferenceNumber')
BEGIN
    ALTER TABLE dbo.PurchaseReturn ADD ReferenceNumber NVARCHAR(50) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturn') AND name = N'CurrencyId')
BEGIN
    ALTER TABLE dbo.PurchaseReturn ADD CurrencyId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturn') AND name = N'PurchaseReturnTypeId')
BEGIN
    ALTER TABLE dbo.PurchaseReturn ADD PurchaseReturnTypeId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturn') AND name = N'AccountingYearId')
BEGIN
    ALTER TABLE dbo.PurchaseReturn ADD AccountingYearId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturn') AND name = N'PaymentTypeId')
BEGIN
    ALTER TABLE dbo.PurchaseReturn ADD PaymentTypeId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturn') AND name = N'PaymentMethodId')
BEGIN
    ALTER TABLE dbo.PurchaseReturn ADD PaymentMethodId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturn') AND name = N'ReturnReasonId')
BEGIN
    ALTER TABLE dbo.PurchaseReturn ADD ReturnReasonId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturn') AND name = N'CancelledByUserID')
BEGIN
    ALTER TABLE dbo.PurchaseReturn ADD CancelledByUserID BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturn') AND name = N'CancelledAt')
BEGIN
    ALTER TABLE dbo.PurchaseReturn ADD CancelledAt DATETIME2 NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturn') AND name = N'CancellationReason')
BEGIN
    ALTER TABLE dbo.PurchaseReturn ADD CancellationReason NVARCHAR(500) NULL;
END;
GO

/* =============================================================================
   04. dbo.PurchaseReturnItem - reference + batch/serial columns
   ============================================================================= */
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturnItem') AND name = N'TaxId')
BEGIN
    ALTER TABLE dbo.PurchaseReturnItem ADD TaxId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturnItem') AND name = N'CessId')
BEGIN
    ALTER TABLE dbo.PurchaseReturnItem ADD CessId BIGINT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturnItem') AND name = N'BarcodeSnapshot')
BEGIN
    ALTER TABLE dbo.PurchaseReturnItem ADD BarcodeSnapshot NVARCHAR(100) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturnItem') AND name = N'BatchNumber')
BEGIN
    ALTER TABLE dbo.PurchaseReturnItem ADD BatchNumber NVARCHAR(100) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturnItem') AND name = N'SerialNumber')
BEGIN
    ALTER TABLE dbo.PurchaseReturnItem ADD SerialNumber NVARCHAR(100) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PurchaseReturnItem') AND name = N'ReasonId')
BEGIN
    ALTER TABLE dbo.PurchaseReturnItem ADD ReasonId BIGINT NULL;
END;
GO

/* =============================================================================
   05. Optional FOREIGN KEY constraints
       Created ONLY where the referenced master table + column definitely exist
       and the datatypes match. Each guarded so it is safe to re-run.
   ============================================================================= */

/* 05.1 Purchase.TaxId -> dbo.Taxes(Id) */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'FK_Purchase_Taxes') AND type = N'F')
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Taxes') AND name = N'Id')
BEGIN
    ALTER TABLE dbo.Purchase ADD CONSTRAINT FK_Purchase_Taxes FOREIGN KEY (TaxId) REFERENCES dbo.Taxes(Id);
END;
GO

/* 05.2 Purchase.AccountingYearId -> dbo.FinancialYear(FinancialYearId) */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'FK_Purchase_FinancialYear') AND type = N'F')
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.FinancialYear') AND name = N'FinancialYearId')
BEGIN
    ALTER TABLE dbo.Purchase ADD CONSTRAINT FK_Purchase_FinancialYear FOREIGN KEY (AccountingYearId) REFERENCES dbo.FinancialYear(FinancialYearId);
END;
GO

/* 05.3 PurchaseItem.TaxId -> dbo.Taxes(Id) */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'FK_PurchaseItem_Taxes') AND type = N'F')
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Taxes') AND name = N'Id')
BEGIN
    ALTER TABLE dbo.PurchaseItem ADD CONSTRAINT FK_PurchaseItem_Taxes FOREIGN KEY (TaxId) REFERENCES dbo.Taxes(Id);
END;
GO

/* 05.4 PurchaseReturn.AccountingYearId -> dbo.FinancialYear(FinancialYearId) */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'FK_PurchaseReturn_FinancialYear') AND type = N'F')
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.FinancialYear') AND name = N'FinancialYearId')
BEGIN
    ALTER TABLE dbo.PurchaseReturn ADD CONSTRAINT FK_PurchaseReturn_FinancialYear FOREIGN KEY (AccountingYearId) REFERENCES dbo.FinancialYear(FinancialYearId);
END;
GO

/* 05.5 PurchaseReturn.PaymentTypeId -> dbo.PaymentType(PaymentTypeId) */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'FK_PurchaseReturn_PaymentType') AND type = N'F')
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PaymentType') AND name = N'PaymentTypeId')
BEGIN
    ALTER TABLE dbo.PurchaseReturn ADD CONSTRAINT FK_PurchaseReturn_PaymentType FOREIGN KEY (PaymentTypeId) REFERENCES dbo.PaymentType(PaymentTypeId);
END;
GO

/* 05.6 PurchaseReturn.PaymentMethodId -> dbo.PaymentMethod(PaymentMethodId) */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'FK_PurchaseReturn_PaymentMethod') AND type = N'F')
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PaymentMethod') AND name = N'PaymentMethodId')
BEGIN
    ALTER TABLE dbo.PurchaseReturn ADD CONSTRAINT FK_PurchaseReturn_PaymentMethod FOREIGN KEY (PaymentMethodId) REFERENCES dbo.PaymentMethod(PaymentMethodId);
END;
GO

/* 05.7 PurchaseReturnItem.TaxId -> dbo.Taxes(Id) */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'FK_PurchaseReturnItem_Taxes') AND type = N'F')
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Taxes') AND name = N'Id')
BEGIN
    ALTER TABLE dbo.PurchaseReturnItem ADD CONSTRAINT FK_PurchaseReturnItem_Taxes FOREIGN KEY (TaxId) REFERENCES dbo.Taxes(Id);
END;
GO

/* =============================================================================
   06. Verification query (run AFTER the migration to confirm)
   ============================================================================= */
/*

SELECT
    TABLE_SCHEMA,
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION,
    NUMERIC_SCALE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME IN
  (
      'Purchase',
      'PurchaseItem',
      'PurchaseReturn',
      'PurchaseReturnItem'
  )
ORDER BY
    TABLE_NAME,
    ORDINAL_POSITION;
*/