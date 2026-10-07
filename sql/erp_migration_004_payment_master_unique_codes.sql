/* T090: make payment master code uniqueness race-safe for existing tenants. */
IF OBJECT_ID(N'dbo.PaymentType', N'U') IS NULL
    THROW 51040, 'Cannot add payment type code uniqueness: dbo.PaymentType is missing.', 1;

IF OBJECT_ID(N'dbo.PaymentMethod', N'U') IS NULL
    THROW 51041, 'Cannot add payment method code uniqueness: dbo.PaymentMethod is missing.', 1;

IF EXISTS (SELECT Code FROM dbo.PaymentType GROUP BY Code HAVING COUNT_BIG(*) > 1)
    THROW 51042, 'Duplicate payment type codes exist; resolve them before applying T090 migration.', 1;

IF EXISTS (SELECT Code FROM dbo.PaymentMethod GROUP BY Code HAVING COUNT_BIG(*) > 1)
    THROW 51043, 'Duplicate payment method codes exist; resolve them before applying T090 migration.', 1;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.PaymentType') AND name = N'UX_PaymentType_Code')
    CREATE UNIQUE INDEX UX_PaymentType_Code ON dbo.PaymentType (Code);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.PaymentMethod') AND name = N'UX_PaymentMethod_Code')
    CREATE UNIQUE INDEX UX_PaymentMethod_Code ON dbo.PaymentMethod (Code);
