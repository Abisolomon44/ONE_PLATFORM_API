/* =============================================================================
   ADD OPERATORS AND COUNTER ASSIGNMENTS TO BUSINESS MASTER
   ============================================================================= */

-- Add Operators and Counter Assignments screens under SUB-ORGSETUP (Organization Setup)
-- alongside Stores, Counters, POS Sessions

INSERT INTO dbo.Screens (SubModuleId, ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
SELECT sm.Id, k.ScreenCode, k.ScreenName, k.ScreenType, k.RouteUrl, k.ComponentName, k.SortOrder, k.IsActive, k.CreatedBy
FROM (VALUES
    ('OPERATOR_MASTER',      'Operators',           'MASTER', '/operators',             'OperatorsPage',        10, 1, 'system'),
    ('COUNTER_ASSIGN_MASTER', 'Counter Assignments', 'MASTER', '/counter-assignments',    'CounterAssignmentsPage', 11, 1, 'system')
) AS k(ScreenCode, ScreenName, ScreenType, RouteUrl, ComponentName, SortOrder, IsActive, CreatedBy)
INNER JOIN dbo.SubModules sm ON sm.SubModuleCode = 'SUB-ORGSETUP'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Screens s WHERE s.SubModuleId = sm.Id AND s.ScreenCode = k.ScreenCode
);
;