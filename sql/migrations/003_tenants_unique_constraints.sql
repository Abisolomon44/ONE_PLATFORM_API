/* =============================================================================
    ONE ERP - Migration: Make tenant unique constraints active-based (filtered)
    Purpose  : Convert UQ_Tenants_TenantCode and UQ_Tenants_DatabaseName from
               non-filtered unique constraints to filtered unique indexes that
               only apply to non-deleted rows (IsDeleted = 0).
               Also update UQ_Tenants_AdminUsername to exclude soft-deleted rows.
    This allows soft-deleted tenants to have their TenantCode, DatabaseName,
    and AdminUsername reused by new tenants.
    ============================================================================= */

/* ---------------------------------------------------------------------------
    TenantCode: drop non-filtered unique constraint, recreate as filtered index
    --------------------------------------------------------------------------- */
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tenants]') AND type = N'U')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Tenants_TenantCode' AND parent_object_id = OBJECT_ID('dbo.Tenants'))
    BEGIN
        ALTER TABLE dbo.Tenants DROP CONSTRAINT UQ_Tenants_TenantCode;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Tenants_TenantCode' AND object_id = OBJECT_ID('dbo.Tenants'))
    BEGIN
        CREATE UNIQUE INDEX UQ_Tenants_TenantCode ON dbo.Tenants (TenantCode)
            WHERE IsDeleted = 0;
    END
END
;

/* ---------------------------------------------------------------------------
    DatabaseName: drop non-filtered unique constraint, recreate as filtered index
    --------------------------------------------------------------------------- */
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tenants]') AND type = N'U')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Tenants_DatabaseName' AND parent_object_id = OBJECT_ID('dbo.Tenants'))
    BEGIN
        ALTER TABLE dbo.Tenants DROP CONSTRAINT UQ_Tenants_DatabaseName;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Tenants_DatabaseName' AND object_id = OBJECT_ID('dbo.Tenants'))
    BEGIN
        CREATE UNIQUE INDEX UQ_Tenants_DatabaseName ON dbo.Tenants (DatabaseName)
            WHERE IsDeleted = 0;
    END
END
;

/* ---------------------------------------------------------------------------
    AdminUsername: drop existing index, recreate as filtered unique index
    that also excludes soft-deleted rows
    --------------------------------------------------------------------------- */
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tenants]') AND type = N'U')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Tenants_AdminUsername' AND object_id = OBJECT_ID('dbo.Tenants'))
    BEGIN
        DROP INDEX UQ_Tenants_AdminUsername ON dbo.Tenants;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Tenants_AdminUsername' AND object_id = OBJECT_ID('dbo.Tenants'))
    BEGIN
        CREATE UNIQUE INDEX UQ_Tenants_AdminUsername ON dbo.Tenants (AdminUsername)
            WHERE AdminUsername IS NOT NULL AND AdminUsername <> '' AND IsDeleted = 0;
    END
END
;