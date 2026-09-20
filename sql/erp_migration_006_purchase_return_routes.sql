/* =============================================================================
   Migration 006 - Fix Purchase Return screen routes (final architecture)
   - PURCHASE_RETURNS register: /purchase?tab=returns -> /purchase-returns
   - New PURCHASE_RETURN_ENTRY screen: /purchase-returns/new
   - Management detail /purchase-returns/:id needs no screen row (guard
     prefix-allows it under the register screen; backend enforces permissions).
   - PermissionCode for both return screens -> purchases-return (hyphen form).
   - Mirrors PURCHASE_RETURNS role grants onto the new entry screen.
   - Idempotent: guarded by IF NOT EXISTS / NOT EXISTS checks.
   ============================================================================= */

-- 1. Fix the wrong register route.
UPDATE dbo.Screens
SET RouteUrl = '/purchase-returns',
    ScreenName = 'Purchase Returns',
    ComponentName = 'PurchaseReturnPage'
WHERE ScreenCode = 'PURCHASE_RETURNS'
  AND RouteUrl <> '/purchase-returns';

-- 2. Add the Return Entry screen.
INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, 'PURCHASE_RETURN_ENTRY', 'Purchase Return Entry', 'ENTRY', '/purchase-returns/new', 'PurchaseReturnEntryPage', 5, 1, 'system'
FROM dbo.SubModules sm
WHERE sm.SubModuleCode = 'SUB-PURCHASETXN'
  AND NOT EXISTS (SELECT 1 FROM dbo.Screens s WHERE s.SubModuleId = sm.Id AND s.ScreenCode = 'PURCHASE_RETURN_ENTRY');

-- 3. Re-sequence the reports screen after the inserted entry screen.
UPDATE dbo.Screens SET SortOrder = 6 WHERE ScreenCode = 'PURCHASE_REPORTS';

-- 4. PermissionCode mapping (hyphen form used by the new APIs).
UPDATE s SET s.PermissionCode = 'purchases-return'
FROM dbo.Screens s
WHERE s.ScreenCode IN ('PURCHASE_RETURNS', 'PURCHASE_RETURN_ENTRY')
  AND (s.PermissionCode IS NULL OR s.PermissionCode <> 'purchases-return');

-- 5. Mirror role grants from PURCHASE_RETURNS onto PURCHASE_RETURN_ENTRY.
INSERT INTO dbo.RolePermissions
    (RoleId, WorkspaceId, DomainId, ModuleId, SubModuleId, ScreenId, ActionId,
     Allow, DisplayOrder, IsActive, CreatedBy, CreatedDate)
SELECT rp.RoleId, rp.WorkspaceId, rp.DomainId, rp.ModuleId, rp.SubModuleId,
       entry.Id AS ScreenId, rp.ActionId,
       rp.Allow, rp.DisplayOrder, rp.IsActive, 'system', SYSUTCDATETIME()
FROM dbo.RolePermissions rp
INNER JOIN dbo.Screens src ON src.Id = rp.ScreenId AND src.ScreenCode = 'PURCHASE_RETURNS'
INNER JOIN dbo.Screens entry ON entry.ScreenCode = 'PURCHASE_RETURN_ENTRY'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions x
    WHERE x.RoleId = rp.RoleId AND x.ScreenId = entry.Id AND x.ActionId = rp.ActionId
);
