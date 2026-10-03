# ONE ERP — STAGE 2: COMPLETE SALES LIFECYCLE

## Dependency-based Task Prompt Pack

**Execution rule:** Execute one task at a time. Complete the current task's
DB + API + business logic + validation + permission + UI + integration + QA
gates before starting the next task.

**Source basis:** This pack preserves the existing Stage 2 task content and
groups it into the same dependency-oriented 5-task MD format used for
Stage 3. Do not invent endpoints, tables, columns, permissions or business
rules; inspect the actual codebase/schema first.

---

## Global Code Assistant Contract

Read the existing ONE ERP codebase, routes, Angular components, services,
controllers, DTOs, repositories, SQL/schema, migrations, permissions and
existing APIs first.

Implement ONLY the current TASK-ID.

Before coding:
1. Find existing UI.
2. Find existing API.
3. Find existing DB tables/columns.
4. Find existing business logic.
5. Find existing permission/data-scope rules.
6. Find existing migrations.
7. Identify what is actually missing.
8. Do not create duplicate screens/APIs/components.

Implementation order:
DB/migration (only if actually required)
→ model/DTO
→ repository
→ application/service
→ controller/API
→ authorization/data scope
→ Angular service
→ Angular UI
→ QA/API/integration tests.

Use the existing architecture, naming and coding style.

Do not trust client totals for financial/stock operations.
Server recalculates authoritative values.

Do not physically delete posted financial history.
Use status/reversal/return/credit/cancellation workflows.

Do not create a second Sales engine, Payment engine, Stock engine,
POS engine or Document renderer.

Task is DONE only when:
DB + API + business logic + validation + permission +
UI + integration + QA are verified.

Do not implement future tasks.
Do not refactor unrelated modules.

---

# TASK GROUP — T037–T040

## DOCUMENT PRINT → SECURITY → IDEMPOTENCY → BUSINESS-FLOW QA

### Dependency Order

```text
T037 → T038 → T039 → T040

Document Design printing is finalized before cross-cutting security and
idempotency validation. Full business-flow testing is the final release gate.
```

---

# T037 --- Sales Print Using Document Design

**Dependency:** T036 + existing Document Design engine

### UI / Screen

Existing /document-design, /document-designer, preview and document
master screens; integrate Sales print into them

### API

GET /api/document-settings/resolve; existing document-template
preview/publish APIs; GET /api/sales/{id}

### DB / Tables

InvoiceTemplate, InvoiceTemplateAssignment, InvoiceTemplateVersion,
InvoiceTemplateSection, InvoiceTemplateElement, InvoiceTemplateField,
InvoiceTemplateItemColumn, InvoiceTemplateStyle, InvoiceTemplatePrinter,
InvoiceTemplatePrintSetting.

### QA / Acceptance Cases

-   Published version
-   Sales data binding
-   one item table
-   tax summary
-   totals
-   payment
-   A4
-   58/80mm where configured
-   preview=print
-   no duplicate item table
-   repeated print.

### Code Assistant Prompt --- COPY/PASTE

``` text
TASK T037 — Sales Print Using Document Design

Connect Sales Print to the existing Document Design engine. Inspect current template APIs and renderer before changing anything. Resolve template assignment and published version, map SalesInvoice/SalesInvoiceItem/Payment data to existing variables/fields, and render the configured document. Do not create new template tables or seed data. Fix only integration issues required for Sales printing. Test A4 and configured thermal formats, multiple items, tax totals, payment, repeated print and template version changes.

Required output before implementation:
1. Existing UI files/components found
2. Existing API/controller/service/repository found
3. Existing DB tables/columns found
4. Existing permission/data-scope rules found
5. Gap list
6. Files to modify
7. Migration required? YES/NO with reason
8. API contract to use/change
9. UI changes
10. QA test cases
11. Implementation
12. Verification result

Stop and report if a required business table/API does not exist
instead of silently inventing it.
```

### Task Completion Gate

-   [ ] DB/schema verified
-   [ ] API verified
-   [ ] Backend business rule verified
-   [ ] Authorization verified
-   [ ] Angular UI verified
-   [ ] Positive QA passed
-   [ ] Negative QA passed
-   [ ] Tenant/company scope passed
-   [ ] Branch/warehouse scope passed where applicable
-   [ ] Calculation/reconciliation passed
-   [ ] No duplicate API/component created
-   [ ] Integration test passed
-   [ ] Regression test passed

------------------------------------------------------------------------

---

# T038 --- Sales API Security Validation

**Dependency:** T027--T037

### UI / Screen

Existing Sales, Return, Exchange, Payment, POS Hold/Recall screens; no
new screen required

### API

All Sales lifecycle endpoints, including /api/sales\*,
return/exchange/refund/cancel/payment/hold/recall endpoints created in
prior tasks

### DB / Tables

All Sales lifecycle tables and approved new tables

### QA / Acceptance Cases

-   Tenant isolation
-   company
-   branch
-   warehouse
-   SalesView
-   SalesManage
-   SalesReturnView/Manage
-   PaymentsManage
-   cancellation permission
-   price override
-   discount override
-   direct API bypass
-   manipulated CompanyId/BranchId/WarehouseId.

### Code Assistant Prompt --- COPY/PASTE

``` text
TASK T038 — Sales API Security Validation

Perform a security audit of every Sales endpoint. Use the existing permission and data-scope system; do not add hard-coded role logic. Verify authorization at API/service/repository boundaries, not only Angular. Test cross-company, cross-branch, cross-warehouse, unauthorized actions, direct API calls, manipulated scope IDs, discount/price override, cancellation, return, exchange and payment permissions. Fix only verified gaps and add regression tests.

Required output before implementation:
1. Existing UI files/components found
2. Existing API/controller/service/repository found
3. Existing DB tables/columns found
4. Existing permission/data-scope rules found
5. Gap list
6. Files to modify
7. Migration required? YES/NO with reason
8. API contract to use/change
9. UI changes
10. QA test cases
11. Implementation
12. Verification result

Stop and report if a required business table/API does not exist
instead of silently inventing it.
```

### Task Completion Gate

-   [ ] DB/schema verified
-   [ ] API verified
-   [ ] Backend business rule verified
-   [ ] Authorization verified
-   [ ] Angular UI verified
-   [ ] Positive QA passed
-   [ ] Negative QA passed
-   [ ] Tenant/company scope passed
-   [ ] Branch/warehouse scope passed where applicable
-   [ ] Calculation/reconciliation passed
-   [ ] No duplicate API/component created
-   [ ] Integration test passed
-   [ ] Regression test passed

------------------------------------------------------------------------

---

# T039 --- Sales Duplicate Save / Idempotency

**Dependency:** T024 Sales Create + T029 Exchange + T030 Refund + T031
Cancel + T035 Payment

### UI / Screen

No new user-facing screen; optional diagnostic/administration visibility
only if existing architecture supports it

### API

Add/reuse idempotency key support for POST /api/sales and all
stock/money-changing lifecycle commands. Exact final route/header name
must follow existing application conventions.

### DB / Tables

SalesInvoice, Payment, PaymentAllocation, StockTransaction; shared
idempotency persistence only if no existing mechanism is found.

### QA / Acceptance Cases

-   Double click
-   network retry
-   browser refresh
-   same key same payload
-   same key different payload
-   concurrent duplicate
-   no double stock movement
-   no double payment
-   same response replay.

### Code Assistant Prompt --- COPY/PASTE

``` text
TASK T039 — Sales Duplicate Save / Idempotency

Implement server-side idempotency for Sales create and all money/stock-changing Sales lifecycle commands. First inspect whether a shared idempotency mechanism already exists; reuse it if present. If absent, propose the smallest tenant-scoped persistence mechanism and migration. Enforce request-key uniqueness and payload consistency, execute the business transaction once, and replay the original result safely. Test concurrent requests and network retries. Do not rely on Angular saving flags as the only protection.

Required output before implementation:
1. Existing UI files/components found
2. Existing API/controller/service/repository found
3. Existing DB tables/columns found
4. Existing permission/data-scope rules found
5. Gap list
6. Files to modify
7. Migration required? YES/NO with reason
8. API contract to use/change
9. UI changes
10. QA test cases
11. Implementation
12. Verification result

Stop and report if a required business table/API does not exist
instead of silently inventing it.
```

### Task Completion Gate

-   [ ] DB/schema verified
-   [ ] API verified
-   [ ] Backend business rule verified
-   [ ] Authorization verified
-   [ ] Angular UI verified
-   [ ] Positive QA passed
-   [ ] Negative QA passed
-   [ ] Tenant/company scope passed
-   [ ] Branch/warehouse scope passed where applicable
-   [ ] Calculation/reconciliation passed
-   [ ] No duplicate API/component created
-   [ ] Integration test passed
-   [ ] Regression test passed

------------------------------------------------------------------------

---

# T040 --- Sales Business Flow Testing / Release Gate

**Dependency:** T027--T039

### UI / Screen

Existing Sales, POS, Reports, Payment and Document Design screens

### API

All Sales lifecycle APIs

### DB / Tables

All Sales lifecycle tables and approved migrations

### QA / Acceptance Cases

-   Retail sale→return→refund
-   credit sale→payment update→outstanding
-   sale→exchange→difference payment/refund
-   sale→cancel→stock reversal
-   hold→recall→sale
-   reprint
-   duplicate request
-   permission denial
-   cross-company denial
-   tax/payment/stock reconciliation.

### Code Assistant Prompt --- COPY/PASTE

``` text
TASK T040 — Sales Business Flow Testing / Release Gate

Execute complete Sales lifecycle release qualification using the real API/runtime and production-like database. Do not bypass APIs or write directly to DB for business tests. Cover Retail, Credit/Wholesale, Service/Hybrid where supported, Return, Exchange, Refund, Cancellation, Hold/Recall, Payment Update, Reprint, Security and Idempotency. Verify GrandTotal = Paid + Balance; Payment = Allocated + Unallocated; stock movement reconciles; tax totals reconcile; no duplicate transaction. Produce a pass/fail matrix and block Sales completion if any critical flow fails.

Required output before implementation:
1. Existing UI files/components found
2. Existing API/controller/service/repository found
3. Existing DB tables/columns found
4. Existing permission/data-scope rules found
5. Gap list
6. Files to modify
7. Migration required? YES/NO with reason
8. API contract to use/change
9. UI changes
10. QA test cases
11. Implementation
12. Verification result

Stop and report if a required business table/API does not exist
instead of silently inventing it.
```

### Task Completion Gate

-   [ ] DB/schema verified
-   [ ] API verified
-   [ ] Backend business rule verified
-   [ ] Authorization verified
-   [ ] Angular UI verified
-   [ ] Positive QA passed
-   [ ] Negative QA passed
-   [ ] Tenant/company scope passed
-   [ ] Branch/warehouse scope passed where applicable
-   [ ] Calculation/reconciliation passed
-   [ ] No duplicate API/component created
-   [ ] Integration test passed
-   [ ] Regression test passed

------------------------------------------------------------------------

# T040 Release Gate --- Required Business Reconciliation

Before marking Stage 2 complete, verify:

``` text
Sales Invoice:
GrandTotal = PaidAmount + BalanceAmount

Payment:
Payment Amount = Allocated Amount + Unallocated Amount

Tax:
Taxable + CGST + SGST + IGST + CESS + RoundOff ≈ GrandTotal

Stock:
Opening + Purchase + Stock IN - Sales - Stock OUT ± Adjustment = Closing
```

For return/exchange/cancellation, verify the corresponding stock
reversal and financial effect is recorded exactly once.

## Final Stage-2 Exit Criteria

``` text
Sales Entry
   ↓
Posted Invoice
   ↓
Payment / Outstanding
   ↓
Return
   ↓
Refund / Credit
   ↓
Exchange
   ↓
Cancellation
   ↓
Hold / Recall
   ↓
Payment Update
   ↓
Reprint
   ↓
Document Design Print
   ↓
Security
   ↓
Idempotency
   ↓
Business UAT
   ↓
SALES MODULE COMPLETE
```

### Required UAT journeys

1.  Retail sale → cash/UPI payment → invoice → reprint.
2.  Credit sale → partial payment → payment update → outstanding.
3.  Full return → stock IN → refund.
4.  Partial return → tax reversal → customer credit.
5.  Exchange → old product IN → new product OUT → customer pays
    difference.
6.  Exchange → refund difference.
7.  Posted invoice → cancellation → stock/payment reversal.
8.  POS hold → recall → complete sale.
9.  Same request submitted twice → exactly one business transaction.
10. Unauthorized user/direct API → 403 and no data mutation.
11. Cross-company/branch/warehouse request → denied and no data
    mutation.
12. Document template change/version → reprint uses the resolved
    published version without changing the original transaction.

**Stage 2 is COMPLETE only after T040 passes.**

---
