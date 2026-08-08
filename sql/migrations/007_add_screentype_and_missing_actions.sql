-- 007_add_screentype_and_missing_actions.sql
-- Adds ScreenType column to Screens and seeds 5 missing actions.

-- 1. Add ScreenType column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Screens') AND name = 'ScreenType')
BEGIN
    ALTER TABLE dbo.Screens ADD ScreenType NVARCHAR(20) NOT NULL DEFAULT 'MASTER';
    ALTER TABLE dbo.Screens ADD CONSTRAINT UQ_Screens_ScreenType_Default CHECK (ScreenType IN ('MASTER','TRANSACTION','REPORT','DASHBOARD','LIST','ENTRY','SETTINGS'));
END;
GO

-- 2. Seed 5 missing actions
SET NOCOUNT ON;

MERGE dbo.PermissionActionsEntry AS target
USING (VALUES
    ('CLOSE', 'Close', 11, 1),
    ('SEARCH', 'Search', 18, 1),
    ('FILTER', 'Filter', 19, 1),
    ('SORT', 'Sort', 20, 1),
    ('VIEW_ALL', 'View All', 21, 1)
) AS source (ActionCode, ActionName, DisplayOrder, IsActive)
ON target.ActionCode = source.ActionCode
WHEN NOT MATCHED THEN
    INSERT (ActionCode, ActionName, DisplayOrder, IsActive)
    VALUES (source.ActionCode, source.ActionName, source.DisplayOrder, source.IsActive);
GO

PRINT 'Migration 007 complete: ScreenType column added, 5 missing actions seeded.';
GO
