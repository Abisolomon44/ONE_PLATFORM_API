/* =============================================================================
   ONE ERP - Migration 007b: Add PriceTypeIds CSV to PriceLists
   Alternative approach: Store multiple price types as comma-separated VARCHAR
   ============================================================================= */

-- 1. Add PriceTypeIds column (VARCHAR for comma-separated IDs)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.PriceLists') AND name = 'PriceTypeIds')
BEGIN
    ALTER TABLE dbo.PriceLists 
    ADD PriceTypeIds VARCHAR(500) NULL;
END
;

-- 2. Backfill from existing PriceTypeId (single value as CSV)
UPDATE dbo.PriceLists
SET PriceTypeIds = CAST(PriceTypeId AS VARCHAR(10))
WHERE PriceTypeIds IS NULL
  AND PriceTypeId IS NOT NULL
;

-- 3. Optional: Keep PriceTypeId as primary/first price type for backward compat
-- PriceTypeId remains NOT NULL FK to PriceTypes
-- PriceTypeIds stores ALL price types as CSV (e.g., '1,2,3,4')

-- 4. Add index for searching (optional, limited utility for CSV)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PriceLists_PriceTypeIds' AND object_id = OBJECT_ID(N'dbo.PriceLists'))
BEGIN
    CREATE INDEX IX_PriceLists_PriceTypeIds ON dbo.PriceLists (PriceTypeIds);
END
;

-- Helper function to split CSV (for querying)
-- Usage: SELECT * FROM dbo.PriceLists WHERE ',' + PriceTypeIds + ',' LIKE '%,' + CAST(@PriceTypeId AS VARCHAR) + ',%'
-- Or use STRING_SPLIT (SQL Server 2016+): SELECT * FROM dbo.PriceLists CROSS APPLY STRING_SPLIT(PriceTypeIds, ',') WHERE value = @PriceTypeId

-- Example queries:
-- Find price lists containing price type 3:
-- SELECT * FROM dbo.PriceLists WHERE ',' + PriceTypeIds + ',' LIKE '%,3,%'

-- Get all price types for a price list:
-- SELECT pt.* FROM dbo.PriceTypes pt
-- JOIN dbo.PriceLists pl ON ',' + pl.PriceTypeIds + ',' LIKE '%,' + CAST(pt.PriceTypeId AS VARCHAR) + ',%'
-- WHERE pl.PriceListId = @PriceListId