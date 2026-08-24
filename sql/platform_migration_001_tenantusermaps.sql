/* =============================================================================
   ONE ERP - Platform Migration: TenantUserMaps
   Purpose  : Enables username-only ERP login for EVERY tenant user (not just
              the tenant admin stored on dbo.Tenants.AdminUsername).
   Target DB: ONEERP_PLATFORM
   Safe     : Re-runnable (IF NOT EXISTS guards + idempotent backfill).
   How      : Run with Windows Authentication against ONEERP_PLATFORM, or
              execute via sqlcmd:
                sqlcmd -S LAPTOP-BOR8IKB8\MSSQL2022 -d ONEERP_PLATFORM -E -i platform_migration_001_tenantusermaps.sql
   ============================================================================= */

USE [ONEERP_PLATFORM];
GO

/* ---------------------------------------------------------------------------
   1. New table: global username -> tenant lookup
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TenantUserMaps]') AND type = N'U')
BEGIN
    CREATE TABLE dbo.TenantUserMaps (
        TenantUserId INT IDENTITY(1,1)   NOT NULL CONSTRAINT PK_TenantUserMaps PRIMARY KEY,
        Username     NVARCHAR(100)       NOT NULL,
        TenantCode   NVARCHAR(50)        NOT NULL,
        CreatedDate  DATETIME2           NOT NULL CONSTRAINT DF_TenantUserMaps_CreatedDate DEFAULT SYSUTCDATETIME()
    );

    CREATE UNIQUE INDEX UQ_TenantUserMaps_Username ON dbo.TenantUserMaps (Username);
END
GO

/* ---------------------------------------------------------------------------
   2. Backfill existing tenant users into the global map.
      Usernames must be globally unique across tenants for username-only login;
      the first tenant to claim a username wins the mapping (NOT EXISTS guard
      prevents duplicate-key errors when the same username exists in 2 tenants).
--------------------------------------------------------------------------- */
DECLARE @tc NVARCHAR(50), @db NVARCHAR(128), @sql NVARCHAR(MAX);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT TenantCode, DatabaseName FROM dbo.Tenants WHERE IsDeleted = 0;
OPEN cur;
FETCH NEXT FROM cur INTO @tc, @db;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF DB_ID(@db) IS NOT NULL
    BEGIN
        SET @sql = N'INSERT INTO dbo.TenantUserMaps (Username, TenantCode, CreatedDate)
                     SELECT u.Username, @tc, SYSUTCDATETIME()
                     FROM [' + @db + N'].dbo.Users u
                     WHERE u.IsDeleted = 0
                       AND NOT EXISTS (SELECT 1 FROM dbo.TenantUserMaps m WHERE m.Username = u.Username);';
        EXEC sp_executesql @sql, N'@tc NVARCHAR(50)', @tc;
    END
    FETCH NEXT FROM cur INTO @tc, @db;
END
CLOSE cur;
DEALLOCATE cur;
GO

/* ---------------------------------------------------------------------------
   3. Verification
--------------------------------------------------------------------------- */
SELECT COUNT(*) AS TenantUserMapsRows FROM dbo.TenantUserMaps;
GO

PRINT 'Migration 001 (TenantUserMaps) applied successfully.';
GO
