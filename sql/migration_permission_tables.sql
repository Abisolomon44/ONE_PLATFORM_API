/* =============================================================================
   ONE ERP - Permission Tables Migration (Standalone)
   Creates only the tables needed for the permission system.
   Run this BEFORE the icon migration.
   ============================================================================= */

/* 1. CompanyGroups (dependency of Companies FK) */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CompanyGroups]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.CompanyGroups (
        CompanyGroupId INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_CompanyGroups PRIMARY KEY,
        GroupCode      NVARCHAR(20)        NOT NULL CONSTRAINT UQ_CompanyGroups_GroupCode UNIQUE,
        GroupName      NVARCHAR(200)       NOT NULL,
        ShortName      NVARCHAR(100)       NULL,
        [Description]  NVARCHAR(500)       NULL,
        ParentGroupId  INT                 NULL,
        IsActive       BIT                 NOT NULL CONSTRAINT DF_CompanyGroups_IsActive DEFAULT 1,
        IsDeleted      BIT                 NOT NULL CONSTRAINT DF_CompanyGroups_IsDeleted DEFAULT 0,
        CreatedBy      NVARCHAR(100)       NULL,
        CreatedDate    DATETIME2           NOT NULL CONSTRAINT DF_CompanyGroups_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy     NVARCHAR(100)       NULL,
        ModifiedDate   DATETIME2           NOT NULL CONSTRAINT DF_CompaniesGroups_ModifiedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_CompanyGroups_Parent FOREIGN KEY (ParentGroupId) REFERENCES dbo.CompanyGroups (CompanyGroupId)
    );
    CREATE INDEX IX_CompanyGroups_ParentGroupId ON dbo.CompanyGroups (ParentGroupId);
    CREATE INDEX IX_CompanyGroups_IsActive ON dbo.CompanyGroups (IsActive);
END;

/* 2. Add CompanyGroups FK to Companies (if table exists but FK missing) */
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Companies]') AND type = N'U')
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Companies_CompanyGroup')
BEGIN
    ALTER TABLE dbo.Companies
        ADD CONSTRAINT FK_Companies_CompanyGroup FOREIGN KEY (CompanyGroupId) REFERENCES dbo.CompanyGroups (CompanyGroupId);
END;

/* 3. Users (dependency of UserRoles FK) */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Users (
        UserId        INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        CompanyId     INT                 NOT NULL,
        Username      NVARCHAR(100)       NOT NULL,
        Email         NVARCHAR(200)       NOT NULL,
        Mobile        NVARCHAR(50)        NULL,
        FullName      NVARCHAR(200)       NOT NULL,
        PasswordHash  NVARCHAR(255)       NOT NULL,
        Status        NVARCHAR(20)        NOT NULL CONSTRAINT DF_Users_Status DEFAULT 'Active',
        IsSuperAdmin  BIT                 NOT NULL CONSTRAINT DF_Users_IsSuperAdmin DEFAULT 0,
        LastLoginDate DATETIME2           NULL,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_Users_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_Users_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_Users_IsDeleted DEFAULT 0,
        CONSTRAINT FK_Users_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Companies (Id),
        CONSTRAINT UQ_Users_Company_Username UNIQUE (CompanyId, Username)
    );
    CREATE INDEX IX_Users_CompanyId_Username ON dbo.Users (CompanyId, Username);
    CREATE INDEX IX_Users_Status ON dbo.Users (Status);
END;

/* 4. Roles */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.Roles (
        RoleId        INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
        [Name]        NVARCHAR(100)       NOT NULL,
        Code          NVARCHAR(100)       NOT NULL CONSTRAINT UQ_Roles_Code UNIQUE,
        [Description] NVARCHAR(500)       NULL,
        IsSystem      BIT                 NOT NULL CONSTRAINT DF_Roles_IsSystem DEFAULT 0,
        IsActive      BIT                 NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT 1,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_Roles_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy    NVARCHAR(100)       NULL,
        ModifiedDate  DATETIME2           NOT NULL CONSTRAINT DF_Roles_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted     BIT                 NOT NULL CONSTRAINT DF_Roles_IsDeleted DEFAULT 0
    );
    CREATE INDEX IX_Roles_IsActive ON dbo.Roles (IsActive);
END;

/* 5. UserRoles */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserRoles]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.UserRoles (
        UserRoleId    INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_UserRoles PRIMARY KEY,
        UserId        INT                 NOT NULL,
        RoleId        INT                 NOT NULL,
        CreatedBy     NVARCHAR(100)       NULL,
        CreatedDate   DATETIME2           NOT NULL CONSTRAINT DF_UserRoles_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId),
        CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT UQ_UserRoles_User_Role UNIQUE (UserId, RoleId)
    );
    CREATE INDEX IX_UserRoles_UserId ON dbo.UserRoles (UserId);
    CREATE INDEX IX_UserRoles_RoleId ON dbo.UserRoles (RoleId);
END;

/* 6. RolePermissions */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.RolePermissions (
        RolePermissionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RolePermissions PRIMARY KEY,
        RoleId           INT               NOT NULL,
        PermissionCode   NVARCHAR(100)     NOT NULL,
        CreatedBy        NVARCHAR(100)     NULL,
        CreatedDate      DATETIME2         NOT NULL CONSTRAINT DF_RolePermissions_CreatedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT UQ_RolePermissions_Role_Permission UNIQUE (RoleId, PermissionCode)
    );
    CREATE INDEX IX_RolePermissions_RoleId ON dbo.RolePermissions (RoleId);
END;

/* 7. PermissionModules */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PermissionModules]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PermissionModules (
        Id               INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_PermissionModules PRIMARY KEY,
        Code             NVARCHAR(100)       NOT NULL CONSTRAINT UQ_PermissionModules_Code UNIQUE,
        [Name]           NVARCHAR(200)       NOT NULL,
        ParentId         INT                 NULL,
        Level            NVARCHAR(20)        NOT NULL,
        SortOrder        INT                 NOT NULL CONSTRAINT DF_PermissionModules_SortOrder DEFAULT 0,
        IsVisible        BIT                 NOT NULL CONSTRAINT DF_PermissionModules_IsVisible DEFAULT 1,
        Icon             NVARCHAR(100)       NULL,
        RoutePath        NVARCHAR(200)       NULL,
        CreatedBy        NVARCHAR(100)       NULL,
        CreatedDate      DATETIME2           NOT NULL CONSTRAINT DF_PermissionModules_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy       NVARCHAR(100)       NULL,
        ModifiedDate     DATETIME2           NOT NULL CONSTRAINT DF_PermissionModules_ModifiedDate DEFAULT SYSUTCDATETIME(),
        IsDeleted        BIT                 NOT NULL CONSTRAINT DF_PermissionModules_IsDeleted DEFAULT 0,
        CONSTRAINT FK_PermissionModules_Parent FOREIGN KEY (ParentId) REFERENCES dbo.PermissionModules (Id)
    );
    CREATE INDEX IX_PermissionModules_ParentId ON dbo.PermissionModules (ParentId);
    CREATE INDEX IX_PermissionModules_Level ON dbo.PermissionModules (Level);
    CREATE INDEX IX_PermissionModules_IsVisible ON dbo.PermissionModules (IsVisible);
    CREATE INDEX IX_PermissionModules_SortOrder ON dbo.PermissionModules (SortOrder);
END;

/* 8. PermissionActions */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PermissionActions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.PermissionActions (
        Id               INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_PermissionActions PRIMARY KEY,
        Code             NVARCHAR(50)        NOT NULL CONSTRAINT UQ_PermissionActions_Code UNIQUE,
        [Name]           NVARCHAR(100)       NOT NULL,
        SortOrder        INT                 NOT NULL CONSTRAINT DF_PermissionActions_SortOrder DEFAULT 0,
        IsActive         BIT                 NOT NULL CONSTRAINT DF_PermissionActions_IsActive DEFAULT 1,
        CreatedBy        NVARCHAR(100)       NULL,
        CreatedDate      DATETIME2           NOT NULL CONSTRAINT DF_PermissionActions_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy       NVARCHAR(100)       NULL,
        ModifiedDate     DATETIME2           NOT NULL CONSTRAINT DF_PermissionActions_ModifiedDate DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX IX_PermissionActions_IsActive ON dbo.PermissionActions (IsActive);
END;

/* 9. ModulePermissions */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ModulePermissions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.ModulePermissions (
        Id               INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_ModulePermissions PRIMARY KEY,
        RoleId           INT                 NOT NULL,
        PermissionModuleId INT               NOT NULL,
        PermissionActionId INT               NOT NULL,
        Scope            NVARCHAR(20)        NOT NULL,
        ScopeId          INT                 NULL,
        GrantedBy        NVARCHAR(100)       NULL,
        GrantedDate      DATETIME2           NOT NULL CONSTRAINT DF_ModulePermissions_GrantedDate DEFAULT SYSUTCDATETIME(),
        IsRevoked        BIT                 NOT NULL CONSTRAINT DF_ModulePermissions_IsRevoked DEFAULT 0,
        RevokedBy        NVARCHAR(100)       NULL,
        RevokedDate      DATETIME2           NULL,
        CONSTRAINT FK_ModulePermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT FK_ModulePermissions_Module FOREIGN KEY (PermissionModuleId) REFERENCES dbo.PermissionModules (Id),
        CONSTRAINT FK_ModulePermissions_Action FOREIGN KEY (PermissionActionId) REFERENCES dbo.PermissionActions (Id),
        CONSTRAINT UQ_ModulePermissions_Role_Module_Action_Scope UNIQUE (RoleId, PermissionModuleId, PermissionActionId, Scope, ScopeId)
    );
    CREATE INDEX IX_ModulePermissions_RoleId ON dbo.ModulePermissions (RoleId);
    CREATE INDEX IX_ModulePermissions_ModuleId ON dbo.ModulePermissions (PermissionModuleId);
    CREATE INDEX IX_ModulePermissions_ActionId ON dbo.ModulePermissions (PermissionActionId);
    CREATE INDEX IX_ModulePermissions_Scope ON dbo.ModulePermissions (Scope);
    CREATE INDEX IX_ModulePermissions_IsRevoked ON dbo.ModulePermissions (IsRevoked);
END;

/* 10. FieldPermissions */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FieldPermissions]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.FieldPermissions (
        Id               INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_FieldPermissions PRIMARY KEY,
        RoleId           INT                 NOT NULL,
        PermissionModuleId INT               NOT NULL,
        FieldName        NVARCHAR(100)       NOT NULL,
        CanView          BIT                 NOT NULL CONSTRAINT DF_FieldPermissions_CanView DEFAULT 1,
        CanEdit          BIT                 NOT NULL CONSTRAINT DF_FieldPermissions_CanEdit DEFAULT 1,
        IsMandatory      BIT                 NOT NULL CONSTRAINT DF_FieldPermissions_IsMandatory DEFAULT 0,
        IsHidden         BIT                 NOT NULL CONSTRAINT DF_FieldPermissions_IsHidden DEFAULT 0,
        Scope            NVARCHAR(20)        NOT NULL CONSTRAINT DF_FieldPermissions_Scope_DEFAULT DEFAULT 'Company',
        ScopeId          INT                 NULL,
        CreatedBy        NVARCHAR(100)       NULL,
        CreatedDate      DATETIME2           NOT NULL CONSTRAINT DF_FieldPermissions_CreatedDate DEFAULT SYSUTCDATETIME(),
        ModifiedBy       NVARCHAR(100)       NULL,
        ModifiedDate     DATETIME2           NOT NULL CONSTRAINT DF_FieldPermissions_ModifiedDate DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_FieldPermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId),
        CONSTRAINT FK_FieldPermissions_Module FOREIGN KEY (PermissionModuleId) REFERENCES dbo.PermissionModules (Id),
        CONSTRAINT UQ_FieldPermissions_Role_Module_Field UNIQUE (RoleId, PermissionModuleId, FieldName, Scope, ScopeId)
    );
    CREATE INDEX IX_FieldPermissions_RoleId ON dbo.FieldPermissions (RoleId);
    CREATE INDEX IX_FieldPermissions_ModuleId ON dbo.FieldPermissions (PermissionModuleId);
    CREATE INDEX IX_FieldPermissions_FieldName ON dbo.FieldPermissions (FieldName);
    CREATE INDEX IX_FieldPermissions_Scope ON dbo.FieldPermissions (Scope);
END;

/* 11. Seed: PermissionActions */
IF NOT EXISTS (SELECT 1 FROM dbo.PermissionActions)
BEGIN
    INSERT INTO dbo.PermissionActions (Code, [Name], SortOrder, IsActive)
    VALUES
        ('view',   'View',   1, 1),
        ('create', 'Create', 2, 1),
        ('edit',   'Edit',   3, 1),
        ('delete', 'Delete', 4, 1),
        ('export', 'Export', 5, 1),
        ('import', 'Import', 6, 1),
        ('approve','Approve',7, 1),
        ('reject', 'Reject', 8, 1);
END;

/* 12. Seed: PermissionModules (with valid Lucide icons) */
IF NOT EXISTS (SELECT 1 FROM dbo.PermissionModules)
BEGIN
    INSERT INTO dbo.PermissionModules (Code, [Name], ParentId, [Level], SortOrder, IsVisible, Icon, RoutePath)
    VALUES
        ('platform',     'Platform',     NULL, 'Platform',     0, 1, 'server',           '/platform'),
        ('tenant',       'Tenant',       NULL, 'Tenant',       1, 1, 'building-2',       '/tenant'),
        ('company',      'Company',      NULL, 'Company',      2, 1, 'home',             '/companies'),
        ('branch',       'Branch',       NULL, 'Branch',       3, 1, 'git-branch',       '/branches'),
        ('user',         'User',         NULL, 'User',         4, 1, 'user',             '/users'),
        ('roleprofile',  'Role Profile', NULL, 'RoleProfile',  5, 1, 'id-card',          '/role-profiles'),
        ('role',         'Role',         NULL, 'Role',         6, 1, 'shield',           '/roles'),
        ('workspace',    'Workspace',    NULL, 'Workspace',    7, 1, 'layers',           '/workspaces'),
        ('domain',       'Domain',       NULL, 'Domain',       8, 1, 'globe',            '/domains'),
        ('module',       'Module',       NULL, 'Module',       9, 1, 'box',              '/modules'),
        ('screen',       'Screen',       NULL, 'Screen',      10, 1, 'monitor',          '/screens'),
        ('action',       'Action',       NULL, 'Action',      11, 1, 'zap',              '/actions'),
        ('field',        'Field',        NULL, 'Field',       12, 1, 'text-cursor-input','/fields'),
        ('datascope',    'Data Scope',   NULL, 'DataScope',   13, 1, 'database',         '/data-scopes'),
        ('workflow',     'Workflow',     NULL, 'Workflow',    14, 1, 'workflow',         '/workflows'),
        ('approval',     'Approval',     NULL, 'Approval',    15, 1, 'check-double',     '/approvals'),
        ('audit',        'Audit',        NULL, 'Audit',       16, 1, 'clipboard-list',   '/audit');
END;
