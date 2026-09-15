/* =============================================================================
   ONE ERP - ERP Migration: PaymentMethodDetails (child master of PaymentMethod)
   Purpose  : Creates dbo.PaymentMethodDetails, the child master that stores the
              concrete bank/UPI/card/cash-counter instances under a parent
              dbo.PaymentMethod, plus the permission codes for the detail screen.
              One PaymentMethod -> many PaymentMethodDetails.
   Target DB: ONE ERP tenant database (run per tenant, database-per-tenant).
   Safe     : Re-runnable - table creation guarded with sys.objects, permission
              seeds guarded with NOT EXISTS.
   How      : Run with the tenant connection, e.g. via sqlcmd:
                sqlcmd -S LAPTOP-BOR8IKB8\MSSQL2022 -d <TENANT_DB> -E -i erp_migration_004_payment_method_details.sql
   ============================================================================= */

/* ---------------------------------------------------------------------------
   01. dbo.PaymentMethodDetails
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentMethodDetails]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PaymentMethodDetails (
        PaymentMethodDetailId BIGINT IDENTITY(1,1)
            CONSTRAINT PK_PaymentMethodDetails PRIMARY KEY,

        PaymentMethodId BIGINT NOT NULL,

        Code            NVARCHAR(50)  NOT NULL,
        [Name]          NVARCHAR(100) NOT NULL,
        DisplayName     NVARCHAR(150) NULL,

        UPIId           NVARCHAR(150) NULL,

        BankName        NVARCHAR(150) NULL,
        AccountNumber   NVARCHAR(100) NULL,
        IFSCCode        NVARCHAR(20)  NULL,

        TerminalName    NVARCHAR(100) NULL,

        CashCounterName NVARCHAR(100) NULL,

        ReferenceValue  NVARCHAR(200) NULL,

        IsDefault       BIT           NOT NULL CONSTRAINT DF_PaymentMethodDetails_IsDefault DEFAULT 0,
        DisplayOrder    INT           NOT NULL CONSTRAINT DF_PaymentMethodDetails_DisplayOrder DEFAULT 0,
        IsActive        BIT           NOT NULL CONSTRAINT DF_PaymentMethodDetails_IsActive DEFAULT 1,

        CreatedByUserId BIGINT        NOT NULL,
        CreatedAt       DATETIME2     NOT NULL CONSTRAINT DF_PaymentMethodDetails_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedByUserId BIGINT        NULL,
        UpdatedAt       DATETIME2     NULL,

        CONSTRAINT FK_PaymentMethodDetails_PaymentMethod
            FOREIGN KEY (PaymentMethodId) REFERENCES dbo.PaymentMethod(PaymentMethodId),

        CONSTRAINT UQ_PaymentMethodDetails_Code UNIQUE (PaymentMethodId, Code)
    );

    CREATE INDEX IX_PaymentMethodDetails_PaymentMethodId ON dbo.PaymentMethodDetails (PaymentMethodId);
    CREATE INDEX IX_PaymentMethodDetails_IsActive ON dbo.PaymentMethodDetails (IsActive);
    CREATE INDEX IX_PaymentMethodDetails_IsDefault ON dbo.PaymentMethodDetails (PaymentMethodId, IsDefault) WHERE IsDefault = 1;
END;
GO

/* ---------------------------------------------------------------------------
   02. Permission codes for PaymentMethodDetails screen
       Seeded for SuperAdmin + Administrator, matching the other ERP masters.
   --------------------------------------------------------------------------- */
INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode, CreatedBy)
SELECT r.RoleId, v.code, 'system'
FROM dbo.Roles r
CROSS JOIN (VALUES
    ('payment-method-details.view'),
    ('payment-method-details.manage')
) AS v(code)
WHERE r.Code IN ('SuperAdmin', 'Administrator')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissionsLegacy rp WHERE rp.RoleId = r.RoleId AND rp.PermissionCode = v.code);
GO

PRINT 'PaymentMethodDetails migration complete.';
GO