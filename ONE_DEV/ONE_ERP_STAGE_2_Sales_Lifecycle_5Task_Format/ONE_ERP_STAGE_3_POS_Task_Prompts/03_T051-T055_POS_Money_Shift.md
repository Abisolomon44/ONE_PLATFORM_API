# ONE ERP — STAGE 3 POS

## Global Code Assistant Contract

You are modifying the existing ONE ERP codebase. Work **one task at a time** and preserve the existing architecture.

### Mandatory rules

1. Inspect the existing Angular UI, API, DTO/model, service/repository, SQL schema, permissions and tests before coding.
2. Reuse existing screens, components, services, endpoints and tables wherever they already exist.
3. Do not create duplicate POS screens, APIs, components or tables.
4. Do not invent database columns. If a required capability is not supported by the current schema, identify the gap first and add the smallest production-safe migration only when required.
5. Keep the existing database-per-tenant architecture and tenant/company isolation.
6. Backend must be authoritative for CompanyId, BranchId, StoreId, CounterId, OperatorId and POSSessionId.
7. UI permission checks are not sufficient; API authorization is mandatory.
8. POS transactions must be atomic. A failed stock/payment/session operation must not leave partial data.
9. Do not update financial totals directly from the frontend. Recalculate authoritative values on the server.
10. Preserve existing Sales lifecycle work from Stage 2.
11. Use the existing Document Design engine for thermal printing. Do not create a second print engine.
12. Do not mark a task complete merely because the UI renders. Completion requires DB + API + UI + Permission + Validation + Integration + QA.
13. Use real production endpoints/runtime for verification. No fake in-memory production substitute.
14. Do not bypass the API by directly changing production business data from tests.
15. Remove temporary debug code before completion.

## Existing POS/Sales architecture to respect

Expected hierarchy:

Company
↓
Branch
↓
Store
↓
Counter
↓
Counter Assignment
↓
Operator
↓
POS Session
↓
POS Sale / Return / Exchange / Refund / Cash Movement

### Important business rules

- POS Sale requires an OPEN POS Session.
- Normal ERP Sales do not require a POS Session.
- POS Return and POS Cash In/Out are session-based.
- Backend derives/validates POS context from the session.
- One counter must not have more than one OPEN session.
- Company/Branch/Store/Counter/Operator relationships must be validated server-side.
- A POS sale must be linked to the correct POSSessionId.
- Stock, tax, payment and invoice totals must remain consistent with the Sales lifecycle.


# TASK GROUP 03 — T051–T055
## Refund → Exchange → Session Close → Cash Difference → Shift Summary

### Dependency order

```text
T050 Cash Out
   ↓
T051 POS Refund
   ↓
T052 POS Exchange
   ↓
T053 POS Session Close
   ↓
T054 Cash Difference
   ↓
T055 POS Shift Summary
```

---

# T051 — POS Refund

## Dependency

T050 + completed Sales Return/Refund lifecycle from Stage 2

## Objective

Connect POS refund to the existing Sales Return/Refund architecture.

## UI

Existing POS Refund screen/action.

Do not create a separate refund calculation engine.

Flow:

```text
Original POS Sale
↓
Select item/qty
↓
Validate returnable qty
↓
Return stock where applicable
↓
Calculate refund/credit
↓
Process payment/refund
↓
Link to original transaction
```

## API

Reuse Stage 2 Sales Return/Refund APIs.

Potential existing target patterns:

```http
POST /api/sales/{id}/return
POST /api/payments/refunds
```

Verify actual implementation.

## DB

Use existing:

- SalesInvoice
- SalesInvoiceItem
- Payment
- PaymentAllocation
- POSSession
- stock transaction structures
- Stage 2 return/refund structures if implemented

## Rules

- OPEN session required where business flow requires POS refund.
- Original transaction must belong to authorized company.
- Refund quantity cannot exceed eligible quantity.
- Refund cannot exceed eligible financial amount.
- Stock/payment operation must be atomic.
- Session cash impact must be recorded correctly.

## QA

- Full refund.
- Partial refund.
- Multiple refunds against one invoice.
- Over-refund rejected.
- Wrong company rejected.
- Closed session rejected.
- Stock restored.
- Refund payment recorded.
- Cash drawer/session totals updated.
- Duplicate request safe.

## Prompt

> Implement T051 POS Refund by integrating the completed Stage 2 Sales Return and Refund services.
>
> Do not create duplicate refund logic.
>
> Inspect actual APIs, DTOs, return schema, Payment/PaymentAllocation and stock transaction implementation.
>
> POS refund must validate the current OPEN session where required and link the refund to the original POS sale.
>
> Enforce return quantity and financial limits.
>
> Keep stock, refund and session cash movement atomic.
>
> Add integration and reconciliation tests.

## Gate

POS refund must reconcile original sale, returned quantity, stock and money.

---

# T052 — POS Exchange

## Dependency

T051 + Stage 2 Sales Exchange

## Objective

Integrate exchange into POS.

## Business flow

```text
Original POS Sale
       ↓
Old Item Return
       ↓
Stock IN
       ↓
New Item Selection
       ↓
Stock OUT
       ↓
Tax/Discount Recalculation
       ↓
Difference
   ┌───┴────┐
   ↓        ↓
Customer Pays   Customer Refund
```

## API

Reuse Stage 2 Exchange API.

Possible target:

```http
POST /api/sales-exchanges
```

Verify actual API before implementation.

## DB

Use existing sales, return, stock and payment structures.

Do not create duplicate POS-only exchange tables if the Stage 2 exchange model can represent POS origin.

## Rules

- OPEN session.
- Original sale belongs to authorized context.
- Old quantity cannot exceed eligible quantity.
- New stock must be validated.
- Difference must be calculated server-side.
- Payment/refund must reconcile.
- Operation must be atomic.

## QA

- Equal-value exchange.
- Customer pays difference.
- Customer receives refund.
- Different tax rates.
- Discounted original item.
- Partial quantity.
- Insufficient stock.
- Duplicate request.
- Wrong company/session.
- Stock reconciliation.

## Prompt

> Implement T052 POS Exchange by reusing the Stage 2 Sales Exchange business service.
>
> POS should provide the UI/context/session integration only.
>
> Do not duplicate exchange calculations.
>
> Validate old-item return eligibility, new-item stock, GST/tax, discount, price difference and payment/refund.
>
> Ensure one atomic transaction and correct POSSession linkage.
>
> Add tests for pay-difference, refund-difference, tax differences, stock and duplicate submissions.

## Gate

Exchange must reconcile both inventory and money.

---

# T053 — POS Session Close

## Dependency

T052

## Objective

Close an OPEN POS session safely.

## UI

Existing POS Session Close screen.

Display:

- Session number
- Opening cash
- Cash In
- Cash Out
- Cash sales
- Cash refunds
- Expected cash
- Actual cash input
- Difference
- Closing remarks
- Close Session

## API

Reuse/create only one authoritative close command.

Possible pattern:

```http
POST /api/pos/sessions/{id}/close
```

## DB

POSSessions plus actual cash movement/payment/sales structures.

## Rules

- Only OPEN session can close.
- Correct user/permission.
- Session cannot be closed for another counter.
- Actual cash is recorded.
- Expected cash is server-calculated.
- Closing is atomic.
- Session status becomes Closed.
- Future POS operations cannot use the closed session.

## QA

- Normal close.
- Close with zero sales.
- Close with cash in/out.
- Close with refund.
- Wrong session.
- Already closed.
- Unauthorized user.
- Concurrent close.
- Retry.

## Prompt

> Implement T053 POS Session Close.
>
> Inspect the current POSSession close implementation and all transaction sources contributing to expected cash.
>
> Calculate expected cash server-side.
>
> Accept actual counted cash as user input.
>
> Persist ActualClosingCash, CashDifference and closing metadata only according to the existing schema.
>
> Transition status atomically from OPEN to CLOSED.
>
> Prevent duplicate/concurrent close.
>
> After close, POS sale/cash movement/refund/exchange operations must reject that session.

## Gate

OPEN → CLOSED must be atomic and irreversible unless an explicitly supported administrative reopen feature exists.

---

# T054 — Cash Difference

## Dependency

T053

## Objective

Calculate and expose the actual-vs-expected cash difference.

## Formula

```text
Cash Difference
= Actual Closing Cash
- Expected Closing Cash
```

Use the exact sign convention already used by the current codebase.

## UI

Show:

- Expected Cash
- Actual Cash
- Difference
- Difference status if existing business rules define one

Do not invent approval rules unless already defined.

## API

Reuse session detail/close response where possible.

## DB

POSSessions:

- ExpectedClosingCash
- ActualClosingCash
- CashDifference

Verify exact names.

## QA

- Exact match = zero difference.
- Actual lower than expected.
- Actual higher than expected.
- Decimal/currency precision.
- Recalculation consistency.
- Cannot alter closed-session figures without authorized process.

## Prompt

> Implement T054 Cash Difference using the existing POSSession fields and session close logic.
>
> Do not create a second cash-difference calculation.
>
> Verify decimal precision and sign convention.
>
> Reconcile ExpectedClosingCash, ActualClosingCash and CashDifference against the underlying sales, refund, cash-in and cash-out transactions.
>
> Add tests for zero, shortage and excess scenarios.

## Gate

The stored difference must be reproducible from underlying session transactions.

---

# T055 — POS Shift Summary

## Dependency

T054

## Objective

Provide the cashier/manager shift summary from real transactional data.

## UI

Existing POS Shift Summary/dashboard/report area.

Suggested sections, only where schema supports them:

- Session information
- Opening Cash
- POS Sales
- Cash Sales
- Digital Payments
- Refunds
- Cash In
- Cash Out
- Expected Cash
- Actual Cash
- Difference
- Invoice count
- Return count
- Exchange count

## API

Reuse session/report APIs.

Possible pattern:

```http
GET /api/pos/sessions/{id}/summary
```

Use existing endpoint if available.

## DB

Aggregate from:

- POSSessions
- SalesInvoice
- SalesInvoiceItem
- Payment
- PaymentAllocation
- POS cash movement structure
- return/exchange structures from Stage 2

## Rules

- Server-side aggregation.
- Session-scoped.
- Company/branch/store/counter security.
- Totals must reconcile.
- No client-side authoritative totals.

## QA

```text
Sales totals
= POS invoice totals

Payments
= allocated + unallocated where applicable

Expected cash
= opening + cash-in - cash-out
  + cash sales - cash refunds
  ± supported movements
```

Also test large sessions and multiple payment methods.

## Prompt

> Implement T055 POS Shift Summary using existing session, sales, payment, refund, exchange and cash-movement data.
>
> Reuse existing reports/services where possible.
>
> Do not duplicate Sales Reports logic unnecessarily.
>
> Ensure all aggregation is server-side and session-scoped.
>
> Verify the summary against actual transaction records.
>
> Add reconciliation tests and performance checks for a large session.

## Gate

Shift Summary must reconcile to underlying transactions, not merely display cached frontend totals.
