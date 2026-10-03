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

# TASK GROUP — T027–T031

## SALES RETURN → ENTRY → EXCHANGE → REFUND → CANCELLATION

### Dependency Order

```text
T027 → T028 → T029 → T030 → T031

Foundation/contract first, then return entry. Exchange and refund depend
on the return lifecycle. Cancellation is handled after the core reversal
paths are defined.
```

---

# T027 --- Sales Return Foundation / Contract

**Dependency:** T026 Sales Detail + existing
SalesInvoice/SalesInvoiceItem + stock/payment read APIs

### UI / Screen

/sales-return --- create only if absent; reuse existing Sales Details
for source invoice selection

### API

Existing: GET /api/sales/{id}, GET /api/sales/{id}/items, GET
/api/sales/{id}/payments, GET /api/sales/{id}/stock. Gap/target: POST
/api/sales/{id}/return (current audit says missing).

### DB / Tables

Existing: SalesInvoice, SalesInvoiceItem, Payment, PaymentAllocation,
StockTransaction. Missing in current sales audit: SalesReturn/ReturnItem
persistence.

### QA / Acceptance Cases

-   Full return
-   partial return
-   return qty \> sold/returnable qty blocked
-   cancelled invoice blocked
-   original invoice required
-   company/branch/warehouse scope
-   tax reversal
-   atomic rollback
-   duplicate return blocked.

### Code Assistant Prompt --- COPY/PASTE

``` text
TASK T027 — Sales Return Foundation / Contract

Create the Sales Return domain contract and implementation skeleton. First inspect the existing codebase, SalesController, SalesService, SalesRepository, Angular SalesEntry/SalesDetails, permissions, migrations and schema. Reuse SalesInvoice/SalesInvoiceItem, Payment/PaymentAllocation and StockTransaction patterns. The current audit says Sales Return has no table, API or UI, although SalesReturnView/SalesReturnManage permissions exist. Do not invent existing tables or pretend an API exists. Determine the minimum migration needed for a return header/items and original-invoice reference. Server must re-read the posted invoice and calculate/validate returnable quantity; never trust client totals. Enforce tenant/company/branch/warehouse authorization. Make stock/payment/business effects atomic. Add API tests and UI QA. Do not implement Exchange, Refund or Credit Note in this task.

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

# T028 --- Sales Return Entry

**Dependency:** T027

### UI / Screen

/sales-return --- list, create, source-invoice selection, item/qty grid,
reason, tax, refund/credit outcome, details

### API

Reuse GET /api/sales/{id}, GET /api/sales/{id}/items. Use the approved
T027 return command endpoint.

### DB / Tables

Approved T027 SalesReturn + SalesReturnItem if migration is required;
SalesInvoice/SalesInvoiceItem; StockTransaction;
Payment/PaymentAllocation where applicable.

### QA / Acceptance Cases

-   Source invoice load
-   partial/full return
-   tax
-   stock IN
-   refund/credit outcome
-   duplicate save
-   wrong company/branch/warehouse
-   permission
-   rollback.

### Code Assistant Prompt --- COPY/PASTE

``` text
TASK T028 — Sales Return Entry

Implement Sales Return Entry end-to-end using the approved T027 contract. Inspect existing Angular patterns before creating components. Build the return UI with original invoice selection, item/quantity selection, reason, tax totals and refund/credit outcome. Backend must re-read the posted invoice and calculate/validate returnable quantity. Never trust client totals. Perform return + stock movement + payment/credit side effect atomically. Add positive, negative, permission, tenant-scope, duplicate and reconciliation tests. Do not implement Exchange.

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

# T029 --- Sales Exchange

**Dependency:** T028 + existing Sales + Stock + Payment engines

### UI / Screen

/sales-exchange --- old item, new item, difference, payment/refund
summary

### API

Target: POST /api/sales-exchanges, GET /api/sales-exchanges/{id}. Reuse
GET /api/sales/{id}. Verify whether any existing exchange endpoint
already exists before adding.

### DB / Tables

Existing SalesInvoice/SalesInvoiceItem, StockTransaction, Payment,
PaymentAllocation. SalesExchange persistence is not confirmed in current
schema/audit; add only after schema inspection.

### QA / Acceptance Cases

-   Equal price
-   customer pays difference
-   customer receives refund
-   partial exchange
-   tax difference
-   insufficient stock
-   invalid original invoice
-   duplicate submission
-   atomic rollback
-   permission/scope.

### Code Assistant Prompt --- COPY/PASTE

``` text
TASK T029 — Sales Exchange

Implement Sales Exchange as one controlled business transaction. Inspect the existing return, sales, stock and payment services first. Do not implement exchange as two unrelated Angular calls. Define the minimum exchange persistence only if absent, link the original sale, calculate old-return and new-sale totals server-side, and commit stock/payment changes atomically. Reuse the existing Sales/Payment/Stock engines. Add Angular UI and API/UI/integration tests. Do not duplicate return, payment or invoice engines.

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

# T030 --- Refund

**Dependency:** T028 + Payment engine

### UI / Screen

Existing payment area / refund dialog or page; do not create a duplicate
payment workspace

### API

Gap/target: POST /api/payments/refunds, GET /api/payments/refunds/{id}.
Reuse GET /api/sales/{id}/payments and existing Payment APIs.

### DB / Tables

Payment, PaymentAllocation, SalesInvoice; approved SalesReturn/Exchange
tables if created. PaymentType already contains a Refund seed in the
schema.

### QA / Acceptance Cases

-   Full refund
-   partial refund
-   over-refund blocked
-   original invoice/return reference
-   method
-   duplicate refund
-   wrong company
-   permission
-   rollback.

### Code Assistant Prompt --- COPY/PASTE

``` text
TASK T030 — Refund

Implement Refund using the existing Payment/PaymentAllocation model where possible. Inspect how PaymentType=Refund is seeded and how payments are persisted. Do not create a duplicate refund table unless schema analysis proves it is necessary. Calculate refundable amount from server-side allocations/returns/exchanges, enforce limits and permissions, and execute atomically. Add UI only where absent. Test full/partial/over-refund, duplicate, wrong invoice/company, permission and rollback.

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

# T031 --- Sales Cancellation

**Dependency:** T024 Sales Core + stock/payment reversal capability

### UI / Screen

Existing Sales Details action + confirmation/reason dialog; no separate
page unless current UX requires it

### API

Gap/target: POST /api/sales/{id}/cancel. Existing DELETE /api/sales/{id}
must be audited so posted financial history is not physically deleted.

### DB / Tables

SalesInvoice, SalesInvoiceItem, StockTransaction, Payment,
PaymentAllocation.

### QA / Acceptance Cases

-   Posted cancellation
-   mandatory reason
-   permission
-   stock reversal
-   payment treatment
-   cannot cancel twice
-   invalid status
-   audit
-   atomic rollback.

### Code Assistant Prompt --- COPY/PASTE

``` text
TASK T031 — Sales Cancellation

Replace financial deletion semantics with an explicit Sales Cancellation workflow. Inspect the existing DELETE implementation and status handling first. Add POST /api/sales/{id}/cancel only if absent. Verify company/branch/warehouse scope, invoice state, permission, existing returns/refunds/allocations. Preserve posted financial history; do not physically delete a posted invoice. Perform required reversal in one transaction. Add Angular confirmation/reason dialog and API/UI/integration tests.

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
