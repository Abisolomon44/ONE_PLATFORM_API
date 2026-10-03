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

# TASK GROUP — T032–T036

## CREDIT NOTE → HOLD → RECALL → PAYMENT UPDATE → REPRINT

### Dependency Order

```text
T032 → T033 → T034 → T035 → T036

Credit Note follows the return/refund foundation. Hold must exist before
Recall. Payment Update follows the payment model. Reprint consumes the
completed invoice/document lifecycle without mutating the transaction.
```

---

# T032 --- Credit Note

**Dependency:** T028 Sales Return + T031 Cancellation +
payment/accounting boundary

### UI / Screen

/sales-credit-notes or existing Sales Return workspace if architecture
chooses one combined screen

### API

Target only after design decision: POST /api/sales/{id}/credit-note, GET
/api/credit-notes/{id}, GET /api/credit-notes. Do not add duplicate APIs
if return already owns the financial credit outcome.

### DB / Tables

SalesInvoice, SalesInvoiceItem, Payment, PaymentAllocation. Dedicated
CreditNote tables are not confirmed in current sales audit.

### QA / Acceptance Cases

-   Full/partial credit
-   original invoice
-   tax reversal
-   customer credit
-   duplicate prevention
-   status
-   audit
-   company scope.

### Code Assistant Prompt --- COPY/PASTE

``` text
TASK T032 — Credit Note

Implement Credit Note only after deciding whether it is a distinct document or the financial outcome of Sales Return. The current sales audit says Credit Note is not implemented and no dedicated table exists. Prefer reusing the approved return model if business requirements allow; otherwise create the smallest header/items model with migration. Keep posted financial history immutable. Add API/UI/security/reconciliation tests. Do not duplicate the Payment engine.

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

# T033 --- Hold Bill

**Dependency:** T035 POS architecture / existing SalesEntry tab model

### UI / Screen

/pos/hold --- persistent held bills; reuse SalesEntry cart model

### API

Target: POST /api/pos/holds, GET /api/pos/holds, GET
/api/pos/holds/{id}. Verify whether any existing hold API exists.

### DB / Tables

POSSessions plus HoldBill/HoldBillItem persistence only if absent. A
browser tab/draft is not a persistent hold.

### QA / Acceptance Cases

-   Hold cart
-   customer
-   timestamp
-   operator/session scope
-   no posted stock/payment
-   browser refresh
-   logout/login
-   session close
-   duplicate hold.

### Code Assistant Prompt --- COPY/PASTE

``` text
TASK T033 — Hold Bill

Implement persistent POS Hold Bill only after inspecting current SalesEntry tab state and POS session model. Hold must not create a posted SalesInvoice, stock movement or payment. Add the minimum persistence only if absent, scoped to tenant/company/store/counter/operator/session. Build /pos/hold only if absent. Test refresh, logout/login, closed session, wrong operator, duplicate hold and recall readiness. Preserve existing keyboard shortcuts.

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

# T034 --- Recall Bill

**Dependency:** T033 Hold Bill + POS Session

### UI / Screen

/pos/recall --- search/filter held bills and restore to existing
SalesEntry

### API

Target: GET /api/pos/holds, GET /api/pos/holds/{id}, POST
/api/pos/holds/{id}/recall. Verify current routes first.

### DB / Tables

HoldBill + HoldBillItem if approved; POSSessions.

### QA / Acceptance Cases

-   Search
-   current session/operator filter
-   recall once
-   already recalled
-   wrong session/operator
-   closed session
-   concurrent recall
-   permission.

### Code Assistant Prompt --- COPY/PASTE

``` text
TASK T034 — Recall Bill

Implement Recall against the T033 persistent hold contract. Reuse the existing SalesEntry component rather than building a second billing engine. Validate company/store/counter/operator/session on the server, restore the exact held cart values, and mark the hold recalled atomically. Test refresh, wrong session/operator, already recalled, closed session, concurrent recall and permission failure.

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

# T035 --- Sales Payment Update

**Dependency:** T024 Sales Core + Payment Allocation engine

### UI / Screen

Existing Sales Details payment area/dialog; no duplicate payment
workspace

### API

Existing: GET /api/sales/{id}/payments. Gap/target: PUT
/api/sales/{id}/payment (current audit lists this as missing).

### DB / Tables

SalesInvoice, Payment, PaymentAllocation.

### QA / Acceptance Cases

-   Add payment to unpaid invoice
-   partial
-   full settlement
-   overpayment blocked
-   allocation
-   balance recalculation
-   permission
-   company scope
-   duplicate request
-   rollback.

### Code Assistant Prompt --- COPY/PASTE

``` text
TASK T035 — Sales Payment Update

Implement adding/updating payment against an existing posted SalesInvoice. Inspect existing Payment/PaymentAllocation repository/service first and reuse it. Do not let Angular update SalesInvoice.PaidAmount directly. Server must create/update Payment and Allocation atomically and calculate authoritative paid/balance values. Add the endpoint only if absent. Test partial/full/overpayment/duplicate/wrong invoice/wrong company/permission/rollback.

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

# T036 --- Sales Invoice Reprint

**Dependency:** T024 Sales Details + T058 Document Design

### UI / Screen

Existing Sales Details/Invoice view; add Reprint action

### API

GET /api/sales/{id}; resolve/preview through existing document-design
APIs such as GET /api/document-settings/resolve and the existing
template preview contract.

### DB / Tables

SalesInvoice, SalesInvoiceItem, InvoiceTemplate,
InvoiceTemplateAssignment, InvoiceTemplateVersion.

### QA / Acceptance Cases

-   Reprint posted invoice
-   same invoice number
-   no new sale
-   template resolution
-   company template
-   A4/thermal where configured
-   permission
-   no stock/payment changes.

### Code Assistant Prompt --- COPY/PASTE

``` text
TASK T036 — Sales Invoice Reprint

Implement Sales Invoice Reprint using the existing Document Design engine. Do not create a second invoice renderer. Resolve the published template for invoice type/paper/company, load existing SalesInvoice data, render/preview/print without changing transaction data, and add the Reprint action. Verify repeated reprints do not alter stock/payment/totals and respect company/permission scope.

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
