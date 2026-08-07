/* =============================================================================
   ONE ERP - Enterprise Permission Engine Migration
   Version : 002
   Date    : 2026-08-07
   Purpose : Creates the full 16-table enterprise permission engine:
             Workspace -> Domain -> Module -> Screen -> Field hierarchy
             with Role Permissions, User Overrides, Field Permissions,
             Data Scopes, Workflow Permissions, and Audit Trail.
   ============================================================================= */

/* ---------------------------------------------------------------------------
   1. Workspaces (top-level permission grouping)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Workspaces]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Workspaces (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Workspaces PRIMARY KEY,
        WorkspaceCode   NVARCHAR(50)        NOT NULL CONSTRAINT UQ_Workspaces_Code UNIQUE,
        WorkspaceName   NVARCHAR(200)       NOT NULL,
        Icon            NVARCHAR(100)       NULL,
        Route           NVARCHAR(200)       NULL,
        SortOrder       INT                 NOT NULL CONSTRAINT DF_Workspaces_SortOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Workspaces_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_Workspaces_CreatedDate DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX IX_Workspaces_IsActive ON dbo.Workspaces (IsActive);
    CREATE INDEX IX_Workspaces_SortOrder ON dbo.Workspaces (SortOrder);
END
;

/* ---------------------------------------------------------------------------
   2. Domains (workspace sub-grouping)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Domains]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Domains (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Domains PRIMARY KEY,
        WorkspaceId     INT                 NOT NULL,
        DomainCode      NVARCHAR(50)        NOT NULL,
        DomainName      NVARCHAR(200)       NOT NULL,
        Icon            NVARCHAR(100)       NULL,
        SortOrder       INT                 NOT NULL CONSTRAINT DF_Domains_SortOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Domains_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_Domains_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Domains_Workspace FOREIGN KEY (WorkspaceId) REFERENCES dbo.Workspaces (Id),
        CONSTRAINT UQ_Domains_Code UNIQUE (WorkspaceId, DomainCode)
    );
    CREATE INDEX IX_Domains_WorkspaceId ON dbo.Domains (WorkspaceId);
    CREATE INDEX IX_Domains_IsActive ON dbo.Domains (IsActive);
END
;

/* ---------------------------------------------------------------------------
   3. Modules (domain sub-grouping)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Modules]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Modules (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Modules PRIMARY KEY,
        DomainId        INT                 NOT NULL,
        ModuleCode      NVARCHAR(50)        NOT NULL,
        ModuleName      NVARCHAR(200)       NOT NULL,
        Icon            NVARCHAR(100)       NULL,
        RouteUrl        NVARCHAR(200)       NULL,
        SortOrder       INT                 NOT NULL CONSTRAINT DF_Modules_SortOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Modules_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_Modules_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Modules_Domain FOREIGN KEY (DomainId) REFERENCES dbo.Domains (Id),
        CONSTRAINT UQ_Modules_Code UNIQUE (DomainId, ModuleCode)
    );
    CREATE INDEX IX_Modules_DomainId ON dbo.Modules (DomainId);
    CREATE INDEX IX_Modules_IsActive ON dbo.Modules (IsActive);
END
;

/* ---------------------------------------------------------------------------
   4. Screens (module sub-grouping)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Screens]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Screens (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Screens PRIMARY KEY,
        ModuleId        INT                 NOT NULL,
        ScreenCode      NVARCHAR(50)        NOT NULL,
        ScreenName      NVARCHAR(200)       NOT NULL,
        RouteUrl        NVARCHAR(200)       NULL,
        ComponentName   NVARCHAR(200)       NULL,
        SortOrder       INT                 NOT NULL CONSTRAINT DF_Screens_SortOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Screens_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_Screens_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Screens_Module FOREIGN KEY (ModuleId) REFERENCES dbo.Modules (Id),
        CONSTRAINT UQ_Screens_Code UNIQUE (ModuleId, ScreenCode)
    );
    CREATE INDEX IX_Screens_ModuleId ON dbo.Screens (ModuleId);
    CREATE INDEX IX_Screens_IsActive ON dbo.Screens (IsActive);
END
;

/* ---------------------------------------------------------------------------
   5. Fields (screen-level field definitions)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Fields]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Fields (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Fields PRIMARY KEY,
        ScreenId        INT                 NOT NULL,
        FieldCode       NVARCHAR(100)       NOT NULL,
        FieldName       NVARCHAR(200)       NOT NULL,
        DisplayName     NVARCHAR(200)       NOT NULL,
        DataType        NVARCHAR(50)        NOT NULL DEFAULT 'text',
        DisplayOrder    INT                 NOT NULL CONSTRAINT DF_Fields_DisplayOrder DEFAULT 0,
        DefaultValue    NVARCHAR(500)       NULL,
        IsSystemField   BIT                 NOT NULL CONSTRAINT DF_Fields_IsSystemField DEFAULT 0,
        IsRequired      BIT                 NOT NULL CONSTRAINT DF_Fields_IsRequired DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Fields_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_Fields_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Fields_Screen FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
        CONSTRAINT UQ_Fields_Code UNIQUE (ScreenId, FieldCode)
    );
    CREATE INDEX IX_Fields_ScreenId ON dbo.Fields (ScreenId);
    CREATE INDEX IX_Fields_IsActive ON dbo.Fields (IsActive);
END
;

/* ---------------------------------------------------------------------------
   6. Actions (seed data - 17 actions)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Actions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Actions (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Actions PRIMARY KEY,
        ActionCode      NVARCHAR(50)        NOT NULL CONSTRAINT UQ_Actions_Code UNIQUE,
        ActionName      NVARCHAR(100)       NOT NULL,
        DisplayOrder    INT                 NOT NULL CONSTRAINT DF_Actions_DisplayOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_Actions_IsActive DEFAULT 1
    );
END
;

-- Seed 17 actions
IF NOT EXISTS (SELECT 1 FROM dbo.Actions WHERE ActionCode = 'view')
BEGIN
    INSERT INTO dbo.Actions (ActionCode, ActionName, DisplayOrder, IsActive) VALUES
    ('view',    'View',    1,  1),
    ('create',  'Create',  2,  1),
    ('edit',    'Edit',    3,  1),
    ('delete',  'Delete',  4,  1),
    ('approve', 'Approve', 5,  1),
    ('reject',  'Reject',  6,  1),
    ('submit',  'Submit',  7,  1),
    ('cancel',  'Cancel',  8,  1),
    ('print',   'Print',   9,  1),
    ('export',  'Export',  10, 1),
    ('import',  'Import',  11, 1),
    ('email',   'Email',   12, 1),
    ('share',   'Share',   13, 1),
    ('clone',   'Clone',   14, 1),
    ('copy',    'Copy',    15, 1),
    ('upload',  'Upload',  16, 1),
    ('download','Download',17, 1);
END
;

/* ---------------------------------------------------------------------------
   7. RolePermissions (NEW - hierarchical Workspace/Domain/Module/Screen/Action)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.RolePermissions (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_RolePermissions PRIMARY KEY,
        RoleId          INT                 NOT NULL,
        WorkspaceId     INT                 NOT NULL,
        DomainId        INT                 NOT NULL,
        ModuleId        INT                 NOT NULL,
        ScreenId        INT                 NOT NULL,
        ActionId        INT                 NOT NULL,
        Allow           BIT                 NOT NULL CONSTRAINT DF_RolePermissions_Allow DEFAULT 1,
        DisplayOrder    INT                 NOT NULL CONSTRAINT DF_RolePermissions_DisplayOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_RolePermissions_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_RolePermissions_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100)       NULL,
        ModifiedDate    DATETIME2           NULL,
        CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT FK_RolePermissions_Workspace FOREIGN KEY (WorkspaceId) REFERENCES dbo.Workspaces (Id),
        CONSTRAINT FK_RolePermissions_Domain FOREIGN KEY (DomainId) REFERENCES dbo.Domains (Id),
        CONSTRAINT FK_RolePermissions_Module FOREIGN KEY (ModuleId) REFERENCES dbo.Modules (Id),
        CONSTRAINT FK_RolePermissions_Screen FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
        CONSTRAINT FK_RolePermissions_Action FOREIGN KEY (ActionId) REFERENCES dbo.Actions (Id),
        CONSTRAINT UQ_RolePermissions_Matrix UNIQUE (RoleId, WorkspaceId, DomainId, ModuleId, ScreenId, ActionId)
    );
    CREATE INDEX IX_RolePermissions_RoleId ON dbo.RolePermissions (RoleId);
    CREATE INDEX IX_RolePermissions_WorkspaceId ON dbo.RolePermissions (WorkspaceId);
    CREATE INDEX IX_RolePermissions_DomainId ON dbo.RolePermissions (DomainId);
    CREATE INDEX IX_RolePermissions_ModuleId ON dbo.RolePermissions (ModuleId);
    CREATE INDEX IX_RolePermissions_ScreenId ON dbo.RolePermissions (ScreenId);
    CREATE INDEX IX_RolePermissions_ActionId ON dbo.RolePermissions (ActionId);
    CREATE INDEX IX_RolePermissions_IsActive ON dbo.RolePermissions (IsActive);
END
;

/* ---------------------------------------------------------------------------
   8. UserPermissionOverrides (user-level grant/deny beyond role)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPermissionOverrides]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.UserPermissionOverrides (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_UserPermissionOverrides PRIMARY KEY,
        UserId          INT                 NOT NULL,
        WorkspaceId     INT                 NOT NULL,
        DomainId        INT                 NOT NULL,
        ModuleId        INT                 NOT NULL,
        ScreenId        INT                 NOT NULL,
        ActionId        INT                 NOT NULL,
        PermissionType  NVARCHAR(20)        NOT NULL CONSTRAINT DF_UserPermissionOverrides_Type DEFAULT 'Grant',
        Allow           BIT                 NOT NULL CONSTRAINT DF_UserPermissionOverrides_Allow DEFAULT 1,
        EffectiveFrom   DATETIME2           NOT NULL CONSTRAINT DF_UserPermissionOverrides_From DEFAULT SYSUTCDATETIME(),
        EffectiveTo     DATETIME2           NULL,
        Remarks         NVARCHAR(500)       NULL,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_UserPermissionOverrides_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_UserPermissionOverrides_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100)       NULL,
        ModifiedDate    DATETIME2           NULL,
        CONSTRAINT FK_UserPermissionOverrides_User FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId),
        CONSTRAINT FK_UserPermissionOverrides_Workspace FOREIGN KEY (WorkspaceId) REFERENCES dbo.Workspaces (Id),
        CONSTRAINT FK_UserPermissionOverrides_Domain FOREIGN KEY (DomainId) REFERENCES dbo.Domains (Id),
        CONSTRAINT FK_UserPermissionOverrides_Module FOREIGN KEY (ModuleId) REFERENCES dbo.Modules (Id),
        CONSTRAINT FK_UserPermissionOverrides_Screen FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
        CONSTRAINT FK_UserPermissionOverrides_Action FOREIGN KEY (ActionId) REFERENCES dbo.Actions (Id),
        CONSTRAINT UQ_UserPermissionOverrides_Matrix UNIQUE (UserId, WorkspaceId, DomainId, ModuleId, ScreenId, ActionId)
    );
    CREATE INDEX IX_UserPermissionOverrides_UserId ON dbo.UserPermissionOverrides (UserId);
    CREATE INDEX IX_UserPermissionOverrides_IsActive ON dbo.UserPermissionOverrides (IsActive);
END
;

/* ---------------------------------------------------------------------------
   9. RoleFieldPermissions (field-level security per role)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RoleFieldPermissions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.RoleFieldPermissions (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_RoleFieldPermissions PRIMARY KEY,
        RoleId          INT                 NOT NULL,
        ScreenId        INT                 NOT NULL,
        FieldId         INT                 NOT NULL,
        CanView         BIT                 NOT NULL CONSTRAINT DF_RoleFieldPermissions_CanView DEFAULT 1,
        CanEdit         BIT                 NOT NULL CONSTRAINT DF_RoleFieldPermissions_CanEdit DEFAULT 1,
        IsHidden        BIT                 NOT NULL CONSTRAINT DF_RoleFieldPermissions_IsHidden DEFAULT 0,
        IsReadOnly      BIT                 NOT NULL CONSTRAINT DF_RoleFieldPermissions_IsReadOnly DEFAULT 0,
        IsMandatory     BIT                 NOT NULL CONSTRAINT DF_RoleFieldPermissions_IsMandatory DEFAULT 0,
        DisplayOrder    INT                 NOT NULL CONSTRAINT DF_RoleFieldPermissions_DisplayOrder DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_RoleFieldPermissions_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_RoleFieldPermissions_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100)       NULL,
        ModifiedDate    DATETIME2           NULL,
        CONSTRAINT FK_RoleFieldPermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT FK_RoleFieldPermissions_Screen FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
        CONSTRAINT FK_RoleFieldPermissions_Field FOREIGN KEY (FieldId) REFERENCES dbo.Fields (Id),
        CONSTRAINT UQ_RoleFieldPermissions_Matrix UNIQUE (RoleId, ScreenId, FieldId)
    );
    CREATE INDEX IX_RoleFieldPermissions_RoleId ON dbo.RoleFieldPermissions (RoleId);
    CREATE INDEX IX_RoleFieldPermissions_ScreenId ON dbo.RoleFieldPermissions (ScreenId);
    CREATE INDEX IX_RoleFieldPermissions_FieldId ON dbo.RoleFieldPermissions (FieldId);
    CREATE INDEX IX_RoleFieldPermissions_IsActive ON dbo.RoleFieldPermissions (IsActive);
END
;

/* ---------------------------------------------------------------------------
   10. UserFieldPermissions (user-level field overrides)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserFieldPermissions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.UserFieldPermissions (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_UserFieldPermissions PRIMARY KEY,
        UserId          INT                 NOT NULL,
        ScreenId        INT                 NOT NULL,
        FieldId         INT                 NOT NULL,
        CanView         BIT                 NOT NULL CONSTRAINT DF_UserFieldPermissions_CanView DEFAULT 1,
        CanEdit         BIT                 NOT NULL CONSTRAINT DF_UserFieldPermissions_CanEdit DEFAULT 1,
        IsHidden        BIT                 NOT NULL CONSTRAINT DF_UserFieldPermissions_IsHidden DEFAULT 0,
        IsReadOnly      BIT                 NOT NULL CONSTRAINT DF_UserFieldPermissions_IsReadOnly DEFAULT 0,
        IsMandatory     BIT                 NOT NULL CONSTRAINT DF_UserFieldPermissions_IsMandatory DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_UserFieldPermissions_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_UserFieldPermissions_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100)       NULL,
        ModifiedDate    DATETIME2           NULL,
        CONSTRAINT FK_UserFieldPermissions_User FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId),
        CONSTRAINT FK_UserFieldPermissions_Screen FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
        CONSTRAINT FK_UserFieldPermissions_Field FOREIGN KEY (FieldId) REFERENCES dbo.Fields (Id),
        CONSTRAINT UQ_UserFieldPermissions_Matrix UNIQUE (UserId, ScreenId, FieldId)
    );
    CREATE INDEX IX_UserFieldPermissions_UserId ON dbo.UserFieldPermissions (UserId);
    CREATE INDEX IX_UserFieldPermissions_ScreenId ON dbo.UserFieldPermissions (ScreenId);
    CREATE INDEX IX_UserFieldPermissions_IsActive ON dbo.UserFieldPermissions (IsActive);
END
;

/* ---------------------------------------------------------------------------
   11. DataScopes (role-based data access restrictions)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataScopes]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.DataScopes (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_DataScopes PRIMARY KEY,
        RoleId          INT                 NOT NULL,
        CompanyId       INT                 NULL,
        BranchId        INT                 NULL,
        DepartmentId    INT                 NULL,
        WarehouseId     INT                 NULL,
        BusinessUnitId  INT                 NULL,
        CostCenterId    INT                 NULL,
        ProfitCenterId  INT                 NULL,
        CanViewAll      BIT                 NOT NULL CONSTRAINT DF_DataScopes_CanViewAll DEFAULT 0,
        CanEditAll      BIT                 NOT NULL CONSTRAINT DF_DataScopes_CanEditAll DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_DataScopes_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_DataScopes_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_DataScopes_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT UQ_DataScopes_Role UNIQUE (RoleId)
    );
    CREATE INDEX IX_DataScopes_RoleId ON dbo.DataScopes (RoleId);
    CREATE INDEX IX_DataScopes_CompanyId ON dbo.DataScopes (CompanyId);
    CREATE INDEX IX_DataScopes_BranchId ON dbo.DataScopes (BranchId);
END
;

/* ---------------------------------------------------------------------------
   12. WorkflowPermissions (role-based workflow/approval permissions)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkflowPermissions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.WorkflowPermissions (
        Id              INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_WorkflowPermissions PRIMARY KEY,
        RoleId          INT                 NOT NULL,
        ModuleId        INT                 NOT NULL,
        ScreenId        INT                 NOT NULL,
        CanSubmit       BIT                 NOT NULL CONSTRAINT DF_WorkflowPermissions_CanSubmit DEFAULT 0,
        CanApprove      BIT                 NOT NULL CONSTRAINT DF_WorkflowPermissions_CanApprove DEFAULT 0,
        CanReject       BIT                 NOT NULL CONSTRAINT DF_WorkflowPermissions_CanReject DEFAULT 0,
        CanCancel       BIT                 NOT NULL CONSTRAINT DF_WorkflowPermissions_CanCancel DEFAULT 0,
        CanClose        BIT                 NOT NULL CONSTRAINT DF_WorkflowPermissions_CanClose DEFAULT 0,
        IsActive        BIT                 NOT NULL CONSTRAINT DF_WorkflowPermissions_IsActive DEFAULT 1,
        CreatedBy       NVARCHAR(100)       NULL,
        CreatedDate     DATETIME2           NOT NULL CONSTRAINT DF_WorkflowPermissions_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_WorkflowPermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT FK_WorkflowPermissions_Module FOREIGN KEY (ModuleId) REFERENCES dbo.Modules (Id),
        CONSTRAINT FK_WorkflowPermissions_Screen FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
        CONSTRAINT UQ_WorkflowPermissions_Matrix UNIQUE (RoleId, ModuleId, ScreenId)
    );
    CREATE INDEX IX_WorkflowPermissions_RoleId ON dbo.WorkflowPermissions (RoleId);
    CREATE INDEX IX_WorkflowPermissions_ModuleId ON dbo.WorkflowPermissions (ModuleId);
    CREATE INDEX IX_WorkflowPermissions_ScreenId ON dbo.WorkflowPermissions (ScreenId);
END
;

/* ---------------------------------------------------------------------------
   13. AuditLogs (enhanced - replaces existing if needed)
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.AuditLogs (
        Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
        UserId          INT                  NULL,
        ModuleId        INT                  NULL,
        ScreenId        INT                  NULL,
        [Action]        NVARCHAR(50)         NOT NULL,
        ReferenceId     NVARCHAR(50)         NULL,
        OldValue        NVARCHAR(MAX)        NULL,
        NewValue        NVARCHAR(MAX)        NULL,
        IPAddress       NVARCHAR(50)         NULL,
        Browser         NVARCHAR(200)        NULL,
        CreatedDate     DATETIME2            NOT NULL CONSTRAINT DF_AuditLogs_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_AuditLogs_User FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId),
        CONSTRAINT FK_AuditLogs_Module FOREIGN KEY (ModuleId) REFERENCES dbo.Modules (Id),
        CONSTRAINT FK_AuditLogs_Screen FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id)
    );
    CREATE INDEX IX_AuditLogs_UserId ON dbo.AuditLogs (UserId);
    CREATE INDEX IX_AuditLogs_ModuleId ON dbo.AuditLogs (ModuleId);
    CREATE INDEX IX_AuditLogs_Action ON dbo.AuditLogs ([Action]);
    CREATE INDEX IX_AuditLogs_CreatedDate ON dbo.AuditLogs (CreatedDate);
END
;

/* =============================================================================
   SEED DATA - Default Workspace Structure
   ============================================================================= */

-- Default Workspace: Setup & Configuration
IF NOT EXISTS (SELECT 1 FROM dbo.Workspaces WHERE WorkspaceCode = 'SETUP')
BEGIN
    INSERT INTO dbo.Workspaces (WorkspaceCode, WorkspaceName, Icon, Route, SortOrder, IsActive, CreatedBy)
    VALUES ('SETUP', 'Setup & Configuration', 'settings', '/setup', 1, 1, 'system');
END
;

-- Default Domain: Master Data
IF NOT EXISTS (SELECT 1 FROM dbo.Domains WHERE DomainCode = 'MASTER')
BEGIN
    DECLARE @setupWsId INT = (SELECT Id FROM dbo.Workspaces WHERE WorkspaceCode = 'SETUP');
    INSERT INTO dbo.Domains (WorkspaceId, DomainCode, DomainName, Icon, SortOrder, IsActive, CreatedBy)
    VALUES (@setupWsId, 'MASTER', 'Master Data', 'database', 1, 1, 'system');
END
;

-- Default Module: Company
IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'COMPANY')
BEGIN
    DECLARE @masterDomId INT = (SELECT Id FROM dbo.Domains WHERE DomainCode = 'MASTER');
    INSERT INTO dbo.Modules (DomainId, ModuleCode, ModuleName, Icon, RouteUrl, SortOrder, IsActive, CreatedBy)
    VALUES (@masterDomId, 'COMPANY', 'Company', 'home', '/companies', 1, 1, 'system');
END
;

-- Default Screen: Company List
IF NOT EXISTS (SELECT 1 FROM dbo.Screens WHERE ScreenCode = 'COMPANY_LIST')
BEGIN
    DECLARE @companyModId INT = (SELECT Id FROM dbo.Modules WHERE ModuleCode = 'COMPANY');
    INSERT INTO dbo.Screens (ModuleId, ScreenCode, ScreenName, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
    VALUES (@companyModId, 'COMPANY_LIST', 'Company List', '/companies', 'CompanyListPage', 1, 1, 'system');
END
;

-- Default Fields for Company List Screen
IF NOT EXISTS (SELECT 1 FROM dbo.Fields WHERE FieldCode = 'company_code' AND ScreenId = (SELECT Id FROM dbo.Screens WHERE ScreenCode = 'COMPANY_LIST'))
BEGIN
    DECLARE @companyListScrId INT = (SELECT Id FROM dbo.Screens WHERE ScreenCode = 'COMPANY_LIST');
    INSERT INTO dbo.Fields (ScreenId, FieldCode, FieldName, DisplayName, DataType, DisplayOrder, IsSystemField, IsRequired, IsActive, CreatedBy)
    VALUES
    (@companyListScrId, 'company_code',   'CompanyCode',   'Company Code',   'text',     1, 1, 1, 1, 'system'),
    (@companyListScrId, 'company_name',   'CompanyName',   'Company Name',   'text',     2, 1, 1, 1, 'system'),
    (@companyListScrId, 'short_name',     'ShortName',     'Short Name',     'text',     3, 0, 0, 1, 'system'),
    (@companyListScrId, 'email',          'Email',         'Email',          'email',    4, 0, 0, 1, 'system'),
    (@companyListScrId, 'phone',          'Phone',         'Phone',          'text',     5, 0, 0, 1, 'system'),
    (@companyListScrId, 'is_active',      'IsActive',      'Active',         'boolean',  6, 1, 0, 1, 'system');
END
;
