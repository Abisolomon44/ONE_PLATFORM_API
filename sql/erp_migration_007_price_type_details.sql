/* =============================================================================
   ONE ERP - Migration 007: Add PriceTypeId to PriceListDetails
   Purpose: Enable multi-price-type support per Price List (horizontal columns design)
   ============================================================================= */

-- 1. Add PriceTypeId column (nullable first for existing data)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PriceListDetails') AND name = 'PriceTypeId')
BEGIN
    ALTER TABLE dbo.PriceListDetails 
    ADD PriceTypeId BIGINT NULL;
END
;

-- 2. Backfill from parent PriceList (existing single price type per list)
UPDATE d
SET d.PriceTypeId = p.PriceTypeId
FROM dbo.PriceListDetails d
JOIN dbo.PriceLists p ON d.PriceListId = p.PriceListId
WHERE d.PriceTypeId IS NULL
  AND p.PriceTypeId IS NOT NULL
;

-- 3. Make NOT NULL
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PriceListDetails') AND name = 'PriceTypeId' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.PriceListDetails ALTER COLUMN PriceTypeId BIGINT NOT NULL;
END
;

-- 4. Add FK to PriceTypes
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PriceListDetails_PriceType')
BEGIN
    ALTER TABLE dbo.PriceListDetails
    ADD CONSTRAINT FK_PriceListDetails_PriceType 
    FOREIGN KEY (PriceTypeId) REFERENCES dbo.PriceTypes (PriceTypeId);
END
;

-- 5. Update unique constraint for multi-price-type support
-- Drop old unique constraint
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_PriceListDetails_Product')
BEGIN
    ALTER TABLE dbo.PriceListDetails
    DROP CONSTRAINT UQ_PriceListDetails_Product;
END
;

-- Add new unique constraint including PriceTypeId
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_PriceListDetails_Product')
BEGIN
    ALTER TABLE dbo.PriceListDetails
    ADD CONSTRAINT UQ_PriceListDetails_Product 
    UNIQUE (PriceListId, ProductId, UnitId, PriceTypeId);
END
;

-- 6. Add index for PriceTypeId lookups
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PriceListDetails_PriceType' AND object_id = OBJECT_ID(N'dbo.PriceListDetails'))
BEGIN
    CREATE INDEX IX_PriceListDetails_PriceType 
    ON dbo.PriceListDetails (PriceTypeId);
END
;

-- 7. Optional: Add PriceTypeId index on PriceLists for backward compat queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PriceLists_PriceType' AND object_id = OBJECT_ID(N'dbo.PriceLists'))
BEGIN
    CREATE INDEX IX_PriceLists_PriceType ON dbo.PriceLists (PriceTypeId);
END
;

-- Verification queries (run after migration)
-- SELECT COUNT(*) as TotalDetails, COUNT(PriceTypeId) as WithPriceType FROM dbo.PriceListDetails;
-- SELECT PriceListId, COUNT(DISTINCT PriceTypeId) as PriceTypeCount FROM dbo.PriceListDetails GROUP BY PriceListId HAVING COUNT(DISTINCT PriceTypeId) > 1;