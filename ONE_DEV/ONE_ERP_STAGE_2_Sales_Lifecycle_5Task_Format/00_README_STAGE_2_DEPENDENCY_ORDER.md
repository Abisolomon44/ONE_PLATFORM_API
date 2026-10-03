# ONE ERP — STAGE 2 COMPLETE SALES LIFECYCLE

## 3 dependency-based MD files

1. `01_T027-T031_Sales_Return_to_Cancellation.md`
   - T027 Sales Return
   - T028 Sales Return Entry
   - T029 Sales Exchange
   - T030 Refund
   - T031 Sales Cancellation

2. `02_T032-T036_Credit_Hold_Payment_Reprint.md`
   - T032 Credit Note
   - T033 Hold Bill
   - T034 Recall Bill
   - T035 Sales Payment Update
   - T036 Sales Invoice Reprint

3. `03_T037-T040_Print_Security_Idempotency_QA.md`
   - T037 Sales Print using Document Design
   - T038 Sales API Security Validation
   - T039 Sales Duplicate-Save / Idempotency
   - T040 Sales Business-Flow Testing / Release Gate

## Master dependency

T027
→ T028
→ T029
→ T030
→ T031
→ T032
→ T033
→ T034
→ T035
→ T036
→ T037
→ T038
→ T039
→ T040
→ SALES MODULE COMPLETE

## Execution rule

Run exactly one TASK-ID at a time. Do not give the entire Stage 2 pack to
the coding agent as one implementation request.

Every task must pass:
DB → API → Business Logic → Validation → Permission → UI → Integration → QA.

Stage 2 completion means business-complete Sales lifecycle, not UI-complete.
