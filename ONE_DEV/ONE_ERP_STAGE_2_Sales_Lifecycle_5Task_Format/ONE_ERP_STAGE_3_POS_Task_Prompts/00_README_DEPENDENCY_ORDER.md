# ONE ERP — STAGE 3 POS — Task Pack

## Files

1. `01_T041-T045_POS_Foundation.md`
   - T041 POS Dashboard
   - T042 POS Context Popup
   - T043 POS Session Open
   - T044 POS Session Validation
   - T045 POS Sale

2. `02_T046-T050_POS_Operations.md`
   - T046 Barcode Billing
   - T047 Hold
   - T048 Recall
   - T049 Cash In
   - T050 Cash Out

3. `03_T051-T055_POS_Money_Shift.md`
   - T051 POS Refund
   - T052 POS Exchange
   - T053 POS Session Close
   - T054 Cash Difference
   - T055 POS Shift Summary

4. `04_T056-T058_POS_Finalization.md`
   - T056 Thermal Print
   - T057 POS Permission/Security
   - T058 POS Performance Testing
   - Billing Software v1 exit criteria

## Master dependency

T041
→ T042
→ T043
→ T044
→ T045
→ T046
→ T047
→ T048
→ T049
→ T050
→ T051
→ T052
→ T053
→ T054
→ T055
→ T056
→ T057
→ T058
→ BILLING SOFTWARE v1

## Execution rule

Run **one task at a time**.

Do not give T041–T058 to the coding agent as one implementation request.

For every task:

Audit → Design/Gap Check → Implement → Build → API Test → DB Test → UI Test → Permission Test → Business Test → Regression → Mark Complete.

## Important schema rule

The task pack intentionally does not assume that every POS capability already has a dedicated table. Cash movement/hold structures must be verified against the actual ERP schema before migration.

Do not invent database columns or tables without a schema audit and smallest-required migration.
