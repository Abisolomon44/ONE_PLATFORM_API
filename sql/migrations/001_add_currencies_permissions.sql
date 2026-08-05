/* =============================================================================
   ONE ERP - Migration: Add Currencies permissions to existing roles
   Purpose  : Adds the new 'currencies.view' and 'currencies.manage' permission
              codes to the SuperAdmin and Administrator roles in databases
              that were provisioned before the Currencies module was introduced.
   Idempotent: Uses WHERE NOT EXISTS so it is safe to re-run.
   ============================================================================= */

INSERT INTO dbo.RolePermissions (RoleId, PermissionCode, CreatedBy)
SELECT r.RoleId, p.PermissionCode, 'system'
FROM (VALUES
    ('currencies.view'),
    ('currencies.manage')
) AS p(PermissionCode)
CROSS JOIN (SELECT RoleId FROM dbo.Roles WHERE Code IN ('SuperAdmin', 'Administrator')) r
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions rp
    WHERE rp.RoleId = r.RoleId AND rp.PermissionCode = p.PermissionCode
);
