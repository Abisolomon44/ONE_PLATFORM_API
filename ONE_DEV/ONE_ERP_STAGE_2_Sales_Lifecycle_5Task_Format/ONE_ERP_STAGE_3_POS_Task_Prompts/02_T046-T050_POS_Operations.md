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


# TASK GROUP 02 — T046–T050
## Barcode → Hold → Recall → Cash In → Cash Out

### Dependency order

```text
T045 POS Sale
   ↓
T046 Barcode Billing
   ↓
T047 Hold
   ↓
T048 Recall
   ↓
T049 Cash In
   ↓
T050 Cash Out
```

---

# T046 — Barcode Billing

## Dependency

T045

## Objective

Make barcode billing production-ready using the existing Product/Barcode and Sales infrastructure.

## UI

Existing POS/Sales Entry:

```text
Barcode Scan
↓
Product Lookup
↓
Add/Increase Cart Line
↓
Price
↓
Tax
↓
Stock
↓
Totals
```

Support scanner keyboard input where the existing UI already follows this pattern.

## API

Audit existing product/barcode endpoints.

Possible existing resources include product lookup and sales product lookup. Do not create duplicates without checking.

## DB

Relevant existing tables:

- Products
- ProductBarcodes / Barcodes
- Units
- PriceList / PriceListDetails
- Taxes / tax-related masters
- Stock
- SalesInvoiceItem

Verify actual names and relationships.

## QA

- Valid barcode.
- Unknown barcode.
- Duplicate barcode.
- Same product scanned multiple times.
- Unit/barcode mapping.
- Price list rate.
- GST.
- Out-of-stock behavior.
- Inactive product.
- Fast repeated scans.
- Large product master.
- Wrong company barcode isolation.

## Prompt

> Implement T046 Barcode Billing.
>
> Inspect existing barcode/product lookup implementation, ProductBarcodes/Barcodes schema, price lookup, stock lookup and POS cart code.
>
> Reuse existing APIs and product pricing/tax logic.
>
> A barcode scan must resolve the correct product and unit for the current company/tenant and current POS context.
>
> Do not trust client-supplied price or tax.
>
> Support repeated scans efficiently without creating duplicate API calls where the existing architecture can avoid them.
>
> Test unknown barcode, inactive product, duplicate scans, price list, GST, stock and company isolation.
>
> Keep the billing path fast.

## Gate

Barcode → Cart → Price → Tax → Stock must be correct.

---

# T047 — Hold

## Dependency

T046

## Objective

Persist an unposted POS bill as a Hold transaction.

## Rule

Hold is NOT:

- Posted SalesInvoice
- Stock deduction
- Final Payment

## UI

Existing POS Hold action.

Capture enough data to reconstruct the POS cart.

## API

Audit existing persistent hold implementation.

Possible command pattern:

```http
POST /api/pos/holds
GET  /api/pos/holds
```

Reuse equivalent existing APIs.

## DB

First inspect whether a hold/cart table already exists.

Do not store hold state in SalesInvoice unless the existing architecture explicitly supports it.

If missing, propose the smallest schema needed for persistent POS holds.

## Scope

Hold must be scoped by:

- Tenant
- Company
- Branch
- Store
- Counter
- Operator
- POS Session

according to existing business rules.

## QA

- Hold new cart.
- Hold with customer.
- Hold with discount/tax.
- Multiple holds.
- Hold does not affect stock.
- Hold does not affect payment.
- Closed session cannot create hold.
- Cross-company access rejected.
- Duplicate hold request safe.

## Prompt

> Implement T047 POS Hold.
>
> Inspect existing POS hold/cart model, Angular state, API and SQL schema first.
>
> If persistent hold storage exists, reuse it.
>
> If it does not exist, identify the minimum migration required; do not misuse SalesInvoice as a posted transaction.
>
> Hold must preserve enough data to reconstruct the cart and must not post stock/payment.
>
> Enforce current tenant/company/branch/store/counter/operator/session scope.
>
> Add tests for multiple holds, invalid session, cross-company access and retry behavior.

## Gate

Hold can be recalled without creating a sale until the user explicitly posts it.

---

# T048 — Recall

## Dependency

T047

## Objective

Recall a held POS transaction safely into the existing POS billing screen.

## UI

Existing Recall screen/dialog.

Show:

- Hold number
- Date/time
- Customer if present
- Operator
- Amount
- Status

Then:

```text
Recall
↓
Restore cart
↓
Validate current POS session/context
↓
Continue/Edit
↓
Post sale
```

## API

Reuse existing hold API if available.

## Business rules

- Only valid held records can be recalled.
- Scope must match authorized context.
- Recalled record must not silently post.
- Once consumed/cancelled, it must not be recalled again unless business rules explicitly allow it.
- Current price/stock rules must be handled according to documented hold behavior; do not silently invent repricing rules.

## QA

- Recall valid hold.
- Recall from wrong company.
- Recall from wrong counter/session.
- Recall already consumed hold.
- Recall after session close.
- Edit recalled cart.
- Post recalled cart.
- Duplicate recall.

## Prompt

> Implement T048 Recall using the existing persistent Hold implementation.
>
> Inspect actual hold status/state model and reuse it.
>
> Validate current POS session/context before recall.
>
> Do not automatically post the invoice.
>
> Prevent double consumption of a hold.
>
> Preserve customer/cart information and integrate the recalled cart into the existing POS Sale flow.
>
> Test scope, session, duplicate recall and post-after-recall behavior.

## Gate

Hold → Recall → Edit → Sale must be a controlled lifecycle.

---

# T049 — Cash In

## Dependency

T048

## Objective

Record cash added to the POS drawer during an OPEN session.

## Important

Do not assume the database already has a CashMovement table.

First inspect the schema.

Existing Payment/POSSession tables may or may not be sufficient. A dedicated cash movement table may be required.

## UI

Existing POS Cash In screen/action:

- Amount
- Reason
- Reference/remarks

## API

Reuse existing cash movement API if present.

Otherwise define the smallest command endpoint after schema audit.

## DB

Audit:

- POSSessions
- Payment
- PaymentAllocation
- any CashTransaction/CashMovement table
- status/master tables

Do not incorrectly create a customer payment for internal cash-in unless the existing accounting design explicitly defines it that way.

## Business rules

- OPEN session required.
- Positive amount.
- Correct counter/session.
- Authorized permission.
- Atomic save.
- Included in expected closing cash.

## QA

- Valid cash in.
- Zero/negative amount.
- Closed session.
- Wrong session.
- Unauthorized user.
- Duplicate request.
- Expected cash calculation.

## Prompt

> Implement T049 Cash In.
>
> First inspect the existing schema for a dedicated cash movement/cash transaction structure.
>
> Reuse an existing implementation if available.
>
> If no suitable structure exists, propose the smallest production-safe migration rather than misusing SalesInvoice or customer Payment.
>
> Cash In must require an OPEN POS session and be linked to the correct session/context.
>
> Include the transaction in session expected cash/shift summary according to the existing accounting model.
>
> Add authorization, validation, idempotency and integration tests.

## Gate

Cash In affects session cash calculation, but does not create a sales invoice.

---

# T050 — Cash Out

## Dependency

T049

## Objective

Record cash removed from the POS drawer during an OPEN session.

## UI

Existing Cash Out action:

- Amount
- Reason
- Reference/remarks

## API

Reuse existing cash movement endpoint/pattern.

## Business rules

- OPEN session required.
- Positive amount.
- Cannot exceed permitted available cash if the existing business rule requires that restriction.
- Correct session/context.
- Permission required.
- Atomic operation.
- Included in expected closing cash.

## QA

- Valid cash out.
- Invalid amount.
- Closed session.
- Wrong session.
- Unauthorized user.
- Concurrent cash-out.
- Expected cash calculation.
- Duplicate retry.

## Prompt

> Implement T050 Cash Out using the same authoritative cash movement model established for T049.
>
> Do not duplicate the cash movement architecture.
>
> Validate OPEN session, user permission, company/branch/store/counter/session ownership and amount rules.
>
> Recalculate expected session cash server-side.
>
> Test concurrency, duplicate submission, invalid session and closing-summary impact.
>
> Keep Cash Out separate from customer refund unless the existing business model explicitly treats them as the same transaction.

## Gate

```text
Opening Cash
+ Cash In
- Cash Out
+ Cash Sales
- Cash Refunds
± other supported drawer movements
= Expected Cash
```

The exact formula must use the existing schema/business definitions.
