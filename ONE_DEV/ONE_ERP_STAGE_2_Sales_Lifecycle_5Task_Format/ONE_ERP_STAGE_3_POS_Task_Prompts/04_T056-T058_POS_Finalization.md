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


# TASK GROUP 04 — T056–T058
## Thermal Print → Security → Performance → Billing Software v1 Exit

### Dependency order

```text
T055 POS Shift Summary
   ↓
T056 Thermal Print
   ↓
T057 POS Permission/Security
   ↓
T058 POS Performance Testing
   ↓
STAGE 3 MILESTONE
Billing Software v1
```

---

# T056 — Thermal Print

## Dependency

T055 + Stage 2 Document Design / Sales Print

## Objective

Make POS thermal printing use the existing Document Design engine.

## Supported target

Existing seeded document design includes thermal paper sizes such as:

- 58MM
- 80MM

Use the actual existing Document Design configuration.

## UI

Existing POS Print action.

Do not use a separate hard-coded POS receipt renderer if the Document Design engine can render the receipt.

## API

Reuse existing Document Design APIs, including the current resolve/preview/print flow.

Known architecture:

```text
Sales Transaction
↓
Document Type
↓
Template Assignment
↓
Published Template Version
↓
Content Mapping
↓
Renderer
↓
Preview / PDF / Print
```

Audit actual endpoints before coding.

## DB

Reuse existing Document Design tables:

- InvoiceType
- InvoicePaperSize
- PrinterType
- PrinterModel
- InvoiceTemplate
- InvoiceTemplateAssignment
- InvoiceTemplateVersion
- InvoiceTemplateSection
- InvoiceTemplateElement
- InvoiceTemplateField
- InvoiceTemplateItemColumn
- InvoiceTemplateStyle
- InvoiceTemplatePrinter
- InvoiceTemplatePrintSetting

Do not add duplicate POS print tables.

## QA

- 58MM.
- 80MM.
- One-page receipt.
- Multiple items.
- GST.
- Discount.
- Payment.
- Customer.
- Invoice number.
- Barcode/QR where configured.
- Printer mapping.
- Preview vs printed output.
- Reprint does not mutate sale.
- Cancelled invoice print behavior follows business rule.

## Prompt

> Implement T056 POS Thermal Print by reusing the existing Document Design engine completed in Stage 2.
>
> Inspect the existing document master, template, assignment, version, renderer and printer configuration APIs.
>
> Resolve the correct POS document type/template and 58MM/80MM paper configuration.
>
> Do not create a second POS receipt engine.
>
> Map SalesInvoice, SalesInvoiceItem and Payment data through the existing document variable/field system.
>
> Verify published template version and printer settings server-side.
>
> Test 58MM, 80MM, multi-item, GST, discount, payment and reprint.
>
> Printing/reprinting must never change financial or stock data.

## Gate

POS print must be configuration-driven by Document Design.

---

# T057 — POS Permission / Security

## Dependency

T056

## Objective

Complete POS authorization at API level.

## Security areas

Audit existing permission system for:

- POS dashboard access
- Session open
- Session validation
- Sale
- Hold
- Recall
- Cash In
- Cash Out
- Refund
- Exchange
- Session close
- Shift summary
- Thermal print/reprint

Also verify existing:

- JWT authentication
- role permissions
- company scope
- branch scope
- warehouse/store/counter scope where supported
- operator/counter assignment

## Critical rules

UI hiding is not authorization.

Every sensitive endpoint must validate:

```text
Authentication
↓
Permission
↓
Tenant
↓
Company
↓
Branch
↓
Store
↓
Counter
↓
Session
↓
Business Rule
```

## Abuse cases

Test direct API calls for:

- another company
- another branch
- another store
- another counter
- another operator/session
- closed session
- missing permission
- manipulated IDs
- duplicate commands
- invalid JWT/session
- expired token
- SQL injection
- XSS payloads in allowed text fields

## Prompt

> Implement T057 POS Permission/Security.
>
> Audit every POS endpoint and command introduced or reused in Stage 3.
>
> Map each operation to the existing ONE ERP permission system.
>
> Enforce authorization in backend APIs, not only Angular.
>
> Validate tenant/company/branch/store/counter/operator/session ownership.
>
> Test direct API access with manipulated IDs.
>
> Verify JWT/session validation, permission denial, company isolation, branch isolation and POS session isolation.
>
> Do not invent a parallel permission framework.
>
> Fix only the missing/broken security controls.
>
> Add automated integration tests for important allow/deny cases.

## Gate

A user must not gain POS access by bypassing the Angular UI.

---

# T058 — POS Performance Testing

## Dependency

T057

## Objective

Prove that POS is fast enough for real-time billing under realistic data and transaction load.

## Test environment

Use the actual DEV/integration runtime and production-like API path.

Do not use fake in-memory production substitutes.

## Critical user journey

```text
Barcode
 ↓
Product Lookup
 ↓
Cart Add
 ↓
Price
 ↓
Tax
 ↓
Totals
 ↓
Payment
 ↓
Save
 ↓
Invoice
 ↓
Print
```

## Test areas

### 1. Barcode speed

- Repeated barcode scans
- Unknown barcode
- Large product master
- Duplicate scans

### 2. Product search

- Small product master
- Large product master
- Search by code/name/barcode
- Concurrent users

### 3. Cart calculation

- 1 item
- 10 items
- Large cart
- Discount
- GST
- multiple payment types

### 4. Save invoice

- Normal POS sale
- Payment
- Stock
- concurrent billing
- retry

### 5. Print

- 58MM
- 80MM
- multi-item receipt
- repeated reprint

### 6. Session

- Open
- concurrent open attempts
- close
- shift summary

### 7. Security overhead

Ensure authorization checks do not cause unacceptable latency.

## Required evidence

Capture:

- request latency
- error rate
- throughput
- DB query behavior
- slow queries
- API response size
- concurrency behavior
- failed transaction count

Do not invent a performance pass threshold. Establish targets from the existing project requirements/environment and document them.

## Prompt

> Implement T058 POS Performance Testing.
>
> Inspect the existing POS APIs, Angular request flow, database indexes and runtime configuration.
>
> Test the real production path:
> barcode → product → cart → pricing → GST → payment → save → invoice → print.
>
> Include realistic product master size, transaction history, large carts and concurrent billing.
>
> Measure API latency, database performance, failures and throughput.
>
> Investigate slow SQL queries and unnecessary API calls.
>
> Do not replace production services with fake in-memory implementations.
>
> Do not declare a target threshold without project evidence; document the measured baseline and agreed acceptance target.
>
> Fix only confirmed performance bottlenecks and retest.

## Gate

POS performance is accepted only with documented real-runtime test evidence.

---

# STAGE 3 EXIT — BILLING SOFTWARE v1

After T058, verify the complete milestone:

```text
Billing Software v1
│
├── Product
├── Service
├── Customer
├── Pricing
├── GST
├── Sales
├── Sales Return
├── Sales Exchange
├── Refund
├── POS
├── Payment
├── Invoice
└── Reports
```

## Final business-flow test

### Retail

```text
Open POS Session
→ Barcode Billing
→ Discount
→ GST
→ Cash/UPI Payment
→ Invoice
→ Thermal Print
→ Shift Summary
→ Session Close
```

### Return

```text
Original POS Sale
→ POS Refund/Return
→ Stock IN
→ Refund
→ Session Reconciliation
```

### Exchange

```text
Original Sale
→ Old Product Return
→ New Product
→ Difference
→ Pay/Refund
→ Stock Reconciliation
```

### Control

```text
Unauthorized API
→ DENIED

Wrong Company
→ DENIED

Wrong Branch/Store/Counter
→ DENIED

Closed Session
→ DENIED

Duplicate Save
→ No duplicate transaction
```

## Final reconciliation

```text
Sales:
GrandTotal = PaidAmount + BalanceAmount

Tax:
Taxable + CGST + SGST + IGST + CESS + RoundOff ≈ GrandTotal

Payment:
Payment Amount = Allocated + Unallocated

Stock:
Opening
+ Purchase
+ Stock IN
- Sales
- Stock OUT
± Adjustment
= Closing

POS Cash:
Opening Cash
+ Cash In
- Cash Out
+ Cash Sales
- Cash Refunds
± supported movements
= Expected Closing Cash

Cash Difference:
Actual Closing Cash
- Expected Closing Cash
= Cash Difference
```

## Billing Software v1 Definition of Done

```text
DB
+ Migration
+ Model
+ Repository
+ Service
+ API
+ UI
+ Permission
+ Validation
+ Stock
+ Payment
+ Tax
+ Printing
+ Integration
+ Security
+ Idempotency
+ Performance
+ Business-flow QA
= BILLING SOFTWARE v1
```

**Important:** UI-only completion is NOT Stage 3 completion.
