/* =============================================================================
   ONE ERP - Purchase Management Permission Seed
   Purpose  : Registers the Purchase Management screen list in the Enterprise
              Permission Engine, under the existing SETUP workspace:
                SETUP (workspace)
                  └─ PURCHASE (domain)
                       └─ PURCHASE (module)
                            └─ GENERAL (submodule)
                                 ├─ PURCHASE_ENTRY      (ENTRY)
                                 ├─ PURCHASE_LIST       (LIST)
                                 ├─ PURCHASE_STOCK      (LIST)
                                 ├─ PURCHASE_RETURNS    (TRANSACTION)
                                 └─ PURCHASE_REPORTS    (REPORT)

   Notes   : Idempotent (IF NOT EXISTS guards). Uses the SETUP workspace that
             is created in erp_full.sql. Matches the existing seed pattern
             (Workspaces > Domains > Modules > SubModules > Screens).
   ========================================================================== */

/* --- Domain: PURCHASE (under SETUP workspace) ---------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'PURCHASE')
BEGIN
    DECLARE @SetupWorkspaceId INT = (SELECT Id FROM dbo.Workspaces WHERE WorkspaceCode = 'SETUP');
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    VALUES (@SetupWorkspaceId, 'PURCHASE', 'Purchase Management', 'shopping-cart', 2, 1, 'system');
END
;

/* --- Module: PURCHASE (under PURCHASE domain) ---------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'PURCHASE')
BEGIN
    DECLARE @PurchaseDomainId INT = (SELECT Id FROM dbo.Domains WHERE DomainCode = 'PURCHASE');
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    VALUES (@PurchaseDomainId, 'PURCHASE', 'Purchase', 'shopping-cart', '/purchase', 1, 1, 'system');
END
;

/* --- SubModule: GENERAL (under PURCHASE module) -------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.SubModules WHERE SubModuleCode = 'PURCHASE_GENERAL')
BEGIN
    DECLARE @PurchaseModuleId INT = (SELECT Id FROM dbo.Modules WHERE ModuleCode = 'PURCHASE');
    INSERT INTO dbo.SubModules (ModuleId, SubModuleCode, SubModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    VALUES (@PurchaseModuleId, 'PURCHASE_GENERAL', 'Purchase (General)', 'shopping-cart', '/purchase', 0, 1, 'system');
END
;

/* --- Screens (under PURCHASE_GENERAL submodule) -------------------------- */
DECLARE @PurchaseSubModuleId INT = (SELECT Id FROM dbo.SubModules WHERE SubModuleCode = 'PURCHASE_GENERAL');

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'PURCHASE_ENTRY')
BEGIN
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    VALUES (@PurchaseSubModuleId, 'PURCHASE_ENTRY',   'Purchase Entry',   'ENTRY',       '/purchase-entry',   'PurchaseEntryPage',   1, 1, 'system');
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'PURCHASE_LIST')
BEGIN
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    VALUES (@PurchaseSubModuleId, 'PURCHASE_LIST',    'Purchases',       'LIST',        '/purchase?tab=list', 'PurchaseListPage',    2, 1, 'system');
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'PURCHASE_STOCK')
BEGIN
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    VALUES (@PurchaseSubModuleId, 'PURCHASE_STOCK',   'Stock',           'LIST',        '/purchase?tab=stock', 'StockPage',          3, 1, 'system');
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'PURCHASE_RETURNS')
BEGIN
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    VALUES (@PurchaseSubModuleId, 'PURCHASE_RETURNS', 'Purchase Returns', 'TRANSACTION', '/purchase?tab=returns', 'PurchaseReturnPage', 4, 1, 'system');
END
;

IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'PURCHASE_REPORTS')
BEGIN
    INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    VALUES (@PurchaseSubModuleId, 'PURCHASE_REPORTS', 'Purchase Reports', 'REPORT',     '/reports',           'ReportsWorkspace',    5, 1, 'system');
END
;
