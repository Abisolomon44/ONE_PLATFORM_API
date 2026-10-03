/* ============================================================================
   ONE ERP — Stage 3 POS Operations migration
   T049/T050: POSCashMovement (Cash In / Cash Out per OPEN session)
   T047/T048: POSHoldBill (persistent hold/recall; cart stored as JSON so no
              itemized structure is invented — smallest required schema)
   T054:      no schema change — POSSessions already has ExpectedClosingCash /
              ActualClosingCash / CashDifference.
   Idempotent: safe to run repeatedly.
   ============================================================================ */

/* ---------------------------------------------------------------------------
   POSCashMovement
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POSCashMovement]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.POSCashMovement (
        POSCashMovementId BIGINT IDENTITY(1,1) CONSTRAINT PK_POSCashMovement PRIMARY KEY,
        POSSessionId      BIGINT        NOT NULL,
        CompanyId         BIGINT        NOT NULL,
        BranchId          BIGINT        NOT NULL,
        Direction         NVARCHAR(10)  NOT NULL,           -- 'IN' | 'OUT'
        Amount            DECIMAL(18,2) NOT NULL,
        Reason            NVARCHAR(300) NULL,
        ReferenceNo       NVARCHAR(50)  NULL,
        MovementDate      DATETIME2     NOT NULL CONSTRAINT DF_POSCashMovement_Date DEFAULT SYSUTCDATETIME(),
        CreatedByUserID   BIGINT        NOT NULL,
        CreatedAt         DATETIME2     NOT NULL CONSTRAINT DF_POSCashMovement_CreatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT CK_POSCashMovement_Direction CHECK (Direction IN ('IN', 'OUT')),
        CONSTRAINT CK_POSCashMovement_Amount CHECK (Amount > 0)
    );

    CREATE INDEX IX_POSCashMovement_Session ON dbo.POSCashMovement (POSSessionId);
    CREATE INDEX IX_POSCashMovement_Company_Date ON dbo.POSCashMovement (CompanyId, MovementDate);
END
;

/* ---------------------------------------------------------------------------
   POSHoldBill — persistent held POS carts
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POSHoldBill]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.POSHoldBill (
        POSHoldBillId     BIGINT IDENTITY(1,1) CONSTRAINT PK_POSHoldBill PRIMARY KEY,
        CompanyId         BIGINT        NOT NULL,
        BranchId          BIGINT        NOT NULL,
        StoreId           BIGINT        NULL,
        CounterId         BIGINT        NULL,
        HoldNumber        NVARCHAR(30)  NOT NULL,
        HoldDate          DATETIME2     NOT NULL CONSTRAINT DF_POSHoldBill_Date DEFAULT SYSUTCDATETIME(),
        CustomerId        BIGINT        NULL,
        CustomerName      NVARCHAR(200) NULL,
        ItemCount         INT           NOT NULL CONSTRAINT DF_POSHoldBill_Items DEFAULT 0,
        TotalAmount       DECIMAL(18,2) NOT NULL CONSTRAINT DF_POSHoldBill_Total DEFAULT 0,
        CartJson          NVARCHAR(MAX) NOT NULL,
        Status            NVARCHAR(20)  NOT NULL CONSTRAINT DF_POSHoldBill_Status DEFAULT 'HELD', -- HELD | RECALLED | CANCELLED
        CreatedByUserID   BIGINT        NOT NULL,
        CreatedAt         DATETIME2     NOT NULL CONSTRAINT DF_POSHoldBill_CreatedAt DEFAULT SYSUTCDATETIME(),
        RecalledByUserID  BIGINT        NULL,
        RecalledAt        DATETIME2     NULL,
        CancelledByUserID BIGINT        NULL,
        CancelledAt       DATETIME2     NULL,

        CONSTRAINT UQ_POSHoldBill_No UNIQUE (CompanyId, HoldNumber)
    );

    CREATE INDEX IX_POSHoldBill_Scope ON dbo.POSHoldBill (CompanyId, BranchId, Status);
    CREATE INDEX IX_POSHoldBill_Counter ON dbo.POSHoldBill (CounterId, Status);
END
;
