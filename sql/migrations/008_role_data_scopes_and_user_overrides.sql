-- 008_role_data_scopes_and_user_data_scope_overrides.sql
-- Transforms DataScopes into multi-row RoleDataScopes and creates UserDataScopeOverrides.

SET NOCOUNT ON;

-- 1. Create new RoleDataScopes table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RoleDataScopes')
BEGIN
    CREATE TABLE dbo.RoleDataScopes (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        RoleId          INT NOT NULL,
        ModuleId        INT NULL,
        ScreenId        INT NULL,
        CompanyId       INT NULL,
        BranchId        INT NULL,
        DepartmentId    INT NULL,
        WarehouseId     INT NULL,
        BusinessUnitId  INT NULL,
        CostCenterId    INT NULL,
        ProfitCenterId  INT NULL,
        CanView         BIT NOT NULL DEFAULT 1,
        CanCreate       BIT NOT NULL DEFAULT 0,
        CanEdit         BIT NOT NULL DEFAULT 0,
        CanDelete       BIT NOT NULL DEFAULT 0,
        IsActive        BIT NOT NULL DEFAULT 1,
        CreatedBy       NVARCHAR(100) NULL,
        CreatedDate     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100) NULL,
        ModifiedDate    DATETIME2 NULL
    );

    CREATE NONCLUSTERED INDEX IX_RoleDataScopes_RoleId ON dbo.RoleDataScopes(RoleId);
    CREATE NONCLUSTERED INDEX IX_RoleDataScopes_CompanyId ON dbo.RoleDataScopes(CompanyId);
    CREATE NONCLUSTERED INDEX IX_RoleDataScopes_BranchId ON dbo.RoleDataScopes(BranchId);
    CREATE NONCLUSTERED INDEX IX_RoleDataScopes_WarehouseId ON dbo.RoleDataScopes(WarehouseId);
    CREATE NONCLUSTERED INDEX IX_RoleDataScopes_ScreenId ON dbo.RoleDataScopes(ScreenId);
END;
GO

-- 2. Migrate existing DataScopes data into RoleDataScopes
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DataScopes')
BEGIN
    INSERT INTO dbo.RoleDataScopes (RoleId, CompanyId, BranchId, DepartmentId, WarehouseId,
        BusinessUnitId, CostCenterId, ProfitCenterId, CanView, CanCreate, CanEdit, CanDelete, IsActive, CreatedBy, CreatedDate)
    SELECT RoleId, CompanyId, BranchId, DepartmentId, WarehouseId,
        BusinessUnitId, CostCenterId, ProfitCenterId,
        CASE WHEN CanViewAll = 1 THEN 1 ELSE 0 END,
        0,
        CASE WHEN CanEditAll = 1 THEN 1 ELSE 0 END,
        0,
        IsActive, CreatedBy, CreatedDate
    FROM dbo.DataScopes
    WHERE IsActive = 1;
END;
GO

-- 3. Create UserDataScopeOverrides table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserDataScopeOverrides')
BEGIN
    CREATE TABLE dbo.UserDataScopeOverrides (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        UserId          INT NOT NULL,
        ModuleId        INT NULL,
        ScreenId        INT NULL,
        ScopeType       NVARCHAR(50) NOT NULL,
        ScopeValue      NVARCHAR(200) NOT NULL,
        PermissionType  NVARCHAR(20) NOT NULL,
        Allow           BIT NOT NULL,
        EffectiveFrom   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        EffectiveTo     DATETIME2 NULL,
        Remarks         NVARCHAR(500) NULL,
        IsActive        BIT NOT NULL DEFAULT 1,
        CreatedBy       NVARCHAR(100) NULL,
        CreatedDate     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ModifiedBy      NVARCHAR(100) NULL,
        ModifiedDate    DATETIME2 NULL
    );

    CREATE NONCLUSTERED INDEX IX_UserDataScopeOverrides_UserId ON dbo.UserDataScopeOverrides(UserId);
    CREATE NONCLUSTERED INDEX IX_UserDataScopeOverrides_ScopeType ON dbo.UserDataScopeOverrides(ScopeType);
END;
GO

-- 4. Add composite indexes for authorization queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RolePermissions_RoleId_ScreenId' AND object_id = OBJECT_ID('dbo.RolePermissions'))
    CREATE NONCLUSTERED INDEX IX_RolePermissions_RoleId_ScreenId ON dbo.RolePermissions(RoleId, ScreenId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserPermissionOverrides_UserId_IsActive' AND object_id = OBJECT_ID('dbo.UserPermissionOverrides'))
    CREATE NONCLUSTERED INDEX IX_UserPermissionOverrides_UserId_IsActive ON dbo.UserPermissionOverrides(UserId, IsActive);
GO

PRINT 'Migration 008 complete.';
GO
