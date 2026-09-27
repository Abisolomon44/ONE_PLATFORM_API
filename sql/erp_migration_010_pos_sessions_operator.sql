/* ===========================================================================
   erp_migration_010_pos_sessions_operator.sql
   Aligns dbo.POSSessions with the final POS Session hierarchy
   (Store > Counter > Counter Assignment > Operator).

   - Replaces the Cashier terminology with Operator (operatorId +
     operatorNameSnapshot) and backfills existing rows.
   - Stores the CounterAssignmentId that authorised the session.
   - Adds the session lifecycle fields (openedBy/closedBy/closingRemarks)
     and the system-controlled cash reconciliation fields
     (expectedClosingCash / actualClosingCash / cashDifference).
   - Adds a rowversion Version column for optimistic concurrency.

   CompanyId/BranchId are retained for tenant isolation and reporting.
   They are resolved by the backend from the selected Store.

   Safe to re-run: every statement is idempotent.
   =========================================================================== */

/* ---------------------------------------------------------------------------
   1. New columns
   --------------------------------------------------------------------------- */
IF COL_LENGTH('dbo.POSSessions', 'CounterAssignmentId') IS NULL
    ALTER TABLE dbo.POSSessions ADD CounterAssignmentId INT NULL;
GO

IF COL_LENGTH('dbo.POSSessions', 'OperatorId') IS NULL
    ALTER TABLE dbo.POSSessions ADD OperatorId INT NULL;
GO

IF COL_LENGTH('dbo.POSSessions', 'OperatorNameSnapshot') IS NULL
    ALTER TABLE dbo.POSSessions ADD OperatorNameSnapshot NVARCHAR(200) NULL;
GO

IF COL_LENGTH('dbo.POSSessions', 'OpenedBy') IS NULL
    ALTER TABLE dbo.POSSessions ADD OpenedBy INT NULL;
GO

IF COL_LENGTH('dbo.POSSessions', 'ClosedBy') IS NULL
    ALTER TABLE dbo.POSSessions ADD ClosedBy INT NULL;
GO

IF COL_LENGTH('dbo.POSSessions', 'ClosingRemarks') IS NULL
    ALTER TABLE dbo.POSSessions ADD ClosingRemarks NVARCHAR(500) NULL;
GO

IF COL_LENGTH('dbo.POSSessions', 'ExpectedClosingCash') IS NULL
    ALTER TABLE dbo.POSSessions ADD ExpectedClosingCash DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_POSSessions_ExpectedClosingCash DEFAULT 0;
GO

/* ClosingCash becomes ActualClosingCash: the counted cash in the drawer. */
IF COL_LENGTH('dbo.POSSessions', 'ActualClosingCash') IS NULL
   AND COL_LENGTH('dbo.POSSessions', 'ClosingCash') IS NOT NULL
BEGIN
    EXEC sp_rename 'dbo.POSSessions.ClosingCash', 'ActualClosingCash', 'COLUMN';
END
GO

IF COL_LENGTH('dbo.POSSessions', 'CashDifference') IS NULL
    ALTER TABLE dbo.POSSessions ADD CashDifference DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_POSSessions_CashDifference DEFAULT 0;
GO

/* ---------------------------------------------------------------------------
   2. Optimistic concurrency token
   --------------------------------------------------------------------------- */
IF COL_LENGTH('dbo.POSSessions', 'Version') IS NULL
    ALTER TABLE dbo.POSSessions ADD Version ROWVERSION NOT NULL;
GO

/* ---------------------------------------------------------------------------
   3. Backfill from the legacy Cashier columns
      CashierUserId was a UserId; OperatorId is an OperatorId, so the
      historical cashier is resolved through Operators.UserId.
      Guarded on the column still existing, because section 4 drops it.
   --------------------------------------------------------------------------- */
IF COL_LENGTH('dbo.POSSessions', 'CashierUserId') IS NOT NULL
BEGIN
    UPDATE ps
    SET ps.OperatorId = o.OperatorId
    FROM dbo.POSSessions ps
    INNER JOIN dbo.Operators o
            ON o.UserId = ps.CashierUserId
           AND o.IsDeleted = 0
    WHERE ps.OperatorId IS NULL
      AND ps.CashierUserId IS NOT NULL;

    UPDATE ps
    SET ps.OperatorNameSnapshot = ps.CashierUserName
    FROM dbo.POSSessions ps
    WHERE ps.OperatorNameSnapshot IS NULL
      AND ps.CashierUserName IS NOT NULL;
END
GO

UPDATE dbo.POSSessions
SET OpenedBy = COALESCE(CreatedBy, UpdatedBy)
WHERE OpenedBy IS NULL;
GO

/* Sessions already closed keep their counted cash; open sessions have none. */
UPDATE dbo.POSSessions
SET ActualClosingCash = 0
WHERE ActualClosingCash IS NULL;
GO

UPDATE dbo.POSSessions
SET ExpectedClosingCash = OpeningCash,
    CashDifference = 0
WHERE Status IN (2, 3);
GO

/* ---------------------------------------------------------------------------
   4. Drop the legacy Cashier columns
   --------------------------------------------------------------------------- */
IF COL_LENGTH('dbo.POSSessions', 'CashierUserId') IS NOT NULL
    ALTER TABLE dbo.POSSessions DROP COLUMN CashierUserId;
GO

IF COL_LENGTH('dbo.POSSessions', 'CashierUserName') IS NOT NULL
    ALTER TABLE dbo.POSSessions DROP COLUMN CashierUserName;
GO

/* ---------------------------------------------------------------------------
   5. Foreign keys and indexes on POSSessions
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_POSSessions_Operator')
    ALTER TABLE dbo.POSSessions ADD CONSTRAINT FK_POSSessions_Operator
        FOREIGN KEY (OperatorId) REFERENCES dbo.Operators(OperatorId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_POSSessions_CounterAssignment')
    ALTER TABLE dbo.POSSessions ADD CONSTRAINT FK_POSSessions_CounterAssignment
        FOREIGN KEY (CounterAssignmentId) REFERENCES dbo.CounterAssignments(AssignmentId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_POSSessions_Operator' AND object_id = OBJECT_ID(N'dbo.POSSessions'))
    CREATE INDEX IX_POSSessions_Operator ON dbo.POSSessions (OperatorId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_POSSessions_CounterAssignment' AND object_id = OBJECT_ID(N'dbo.POSSessions'))
    CREATE INDEX IX_POSSessions_CounterAssignment ON dbo.POSSessions (CounterAssignmentId);
GO

/* One session per counter at a time: a counter can only have one OPEN session.
   Skipped when legacy data already violates it, so the migration cannot fail. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'UX_POSSessions_OpenCounter' AND object_id = OBJECT_ID(N'dbo.POSSessions'))
   AND NOT EXISTS (SELECT 1 FROM dbo.POSSessions
                   WHERE Status = 1 GROUP BY CounterId HAVING COUNT(1) > 1)
    CREATE UNIQUE INDEX UX_POSSessions_OpenCounter
        ON dbo.POSSessions (CounterId) WHERE Status = 1;
GO

/* SessionNumber is generated by the backend, so it must stay unique. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'UX_POSSessions_SessionNumber' AND object_id = OBJECT_ID(N'dbo.POSSessions'))
   AND NOT EXISTS (SELECT 1 FROM dbo.POSSessions
                   GROUP BY SessionNumber HAVING COUNT(1) > 1)
     CREATE UNIQUE INDEX UX_POSSessions_SessionNumber ON dbo.POSSessions (SessionNumber);
GO

/* ---------------------------------------------------------------------------
   6. Link POS sales invoices to the drawer session
      This is the exact link used to reconcile expected closing cash, so the
      session total is never a date-window guess.
   --------------------------------------------------------------------------- */
IF COL_LENGTH('dbo.SalesInvoice', 'POSSessionId') IS NULL
    ALTER TABLE dbo.SalesInvoice ADD POSSessionId BIGINT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SalesInvoice_POSSession')
    ALTER TABLE dbo.SalesInvoice ADD CONSTRAINT FK_SalesInvoice_POSSession
        FOREIGN KEY (POSSessionId) REFERENCES dbo.POSSessions(POSSessionId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_SalesInvoice_POSSession' AND object_id = OBJECT_ID(N'dbo.SalesInvoice'))
    CREATE INDEX IX_SalesInvoice_POSSession ON dbo.SalesInvoice (POSSessionId);
GO
