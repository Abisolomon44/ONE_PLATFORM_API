/* =============================================================================
   ONE ERP - Migration: Add TenantCode to RefreshTokens
   Purpose  : Store the TenantCode in the RefreshTokens table so that
               token refresh can resolve the tenant directly without
               depending on AdminUsername matching the login username.
   ============================================================================= */

IF COL_LENGTH('dbo.RefreshTokens', 'TenantCode') IS NULL
    ALTER TABLE dbo.RefreshTokens ADD TenantCode NVARCHAR(50) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[RefreshTokens]') AND name = N'IX_RefreshTokens_TenantCode')
    CREATE INDEX IX_RefreshTokens_TenantCode ON dbo.RefreshTokens (TenantCode);