/* =============================================================================
   Migration 005 - Final Purchase + Purchase Return transaction management
   - Grants granular purchase permissions (view/create/edit/cancel/delete) and
     hyphen-form purchase-return permissions to SuperAdmin + Administrator.
   - Idempotent: NOT EXISTS guards on every insert.
   ============================================================================= */
INSERT INTO dbo.RolePermissionsLegacy (RoleId, PermissionCode, CreatedBy)
SELECT r.RoleId, v.code, 'system'
FROM dbo.Roles r
CROSS JOIN (VALUES
    ('purchases.view'), ('purchases.create'), ('purchases.edit'), ('purchases.cancel'), ('purchases.delete'), ('purchases.manage'),
    ('purchases.return.view'), ('purchases.return.manage'),
    ('purchases-return.create'), ('purchases-return.view'), ('purchases-return.edit'), ('purchases-return.cancel'), ('purchases-return.delete')
) AS v(code)
WHERE r.Code IN ('SuperAdmin', 'Administrator')
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissionsLegacy rp WHERE rp.RoleId = r.RoleId AND rp.PermissionCode = v.code);
