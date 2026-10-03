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


# TASK GROUP 01 — T041–T045
## POS Foundation → Context → Session → Validation → POS Sale

### Dependency order

```text
T041 POS Dashboard
   ↓
T042 POS Context Popup
   ↓
T043 POS Session Open
   ↓
T044 POS Session Validation
   ↓
T045 POS Sale
```

T041 establishes the POS workspace.
T042 establishes Company/Branch/Store/Counter context.
T043 creates the session.
T044 makes session state authoritative.
T045 connects POS billing to Sales.

---

# T041 — POS Dashboard

## Objective

Complete the existing POS Dashboard screen as the operational entry point for a cashier/operator.

## UI / Screen

Use the existing POS route/component.

Dashboard should expose only data/actions supported by the current backend, such as:

- Current Company
- Branch
- Store
- Counter
- Operator
- Current POS Session
- Session status
- Opening cash
- Current/expected cash where available
- Today's POS sales summary
- Quick actions:
  - Start Session
  - Open POS Sale
  - Hold
  - Recall
  - Cash In
  - Cash Out
  - Refund
  - Exchange
  - Close Session

Do not create fake dashboard totals.

## API

Audit and reuse existing APIs first.

Expected backend capabilities:

```http
GET /api/pos/dashboard
GET /api/pos/sessions/current
```

If equivalent APIs already exist, reuse them.

## DB

Inspect actual schema before coding.

Relevant existing tables may include:

- POSSessions
- SalesInvoice
- SalesInvoiceItem
- Payment
- PaymentAllocation

Do not add dashboard tables.

## QA

- Dashboard loads for authorized POS user.
- Unauthorized user cannot access POS.
- Company isolation works.
- Current session is shown correctly.
- Closed/no session state is clear.
- Sales summary matches actual Sales data.
- No cross-company data appears.
- API failure shows controlled error state.
- Refresh does not create/modify transactions.

## Code Assistant Prompt

> Inspect the existing ONE ERP POS Dashboard UI, Angular routes/components/services, backend POS APIs, DTOs, repositories/services, SQL schema and permission system.
>
> Implement T041 only.
>
> Reuse the existing POS Dashboard screen if present. Do not create duplicate components or APIs.
>
> Build the dashboard from real backend data. Verify the Company → Branch → Store → Counter → Operator → POS Session context and current-session state.
>
> Reuse existing SalesInvoice, SalesInvoiceItem, Payment and POSSession data where applicable. Do not invent columns.
>
> Enforce tenant/company authorization in the API.
>
> Before coding, provide:
> 1. Existing POS dashboard files
> 2. Existing APIs
> 3. Existing DB tables/columns
> 4. Existing permission checks
> 5. Missing/broken items
>
> Then implement only the required changes.
>
> Test UI → API → DB with real runtime endpoints.
>
> Completion requires UI + API + DB mapping + authorization + error handling + QA evidence.

## Completion Gate

```text
T041 = COMPLETE
only when
Dashboard UI
+ real API
+ real DB data
+ permission
+ tenant/company isolation
+ error handling
+ QA
```

---

# T042 — POS Context Popup

## Dependency

T041

## Objective

Complete the POS context selection/confirmation popup before session creation or POS billing.

## UI

Existing POS Context Popup should resolve:

```text
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
```

Do not expose unnecessary internal IDs.

Show:

- selected company
- branch
- store
- counter
- operator
- current session status
- Continue

## API

Reuse existing APIs where available:

```http
GET /api/companies
GET /api/organization/branches?companyId={companyId}
GET /api/stores?companyId={companyId}&branchId={branchId}
GET /api/counter-assignments/my-default-counter
```

Audit the exact existing route contracts before changing anything.

## DB

Validate against existing:

- Companies
- Branches
- Stores
- Counters
- CounterAssignments
- Operators
- POSSessions

## QA

- Company changes reload dependent Branch data.
- Branch changes reload Store.
- Store changes resolve Counter.
- Operator/counter assignment is authorized.
- Disabled/inactive context cannot continue.
- Existing open session is detected.
- Cross-company IDs are rejected.
- Direct API manipulation cannot bypass context validation.

## Code Assistant Prompt

> Implement T042 POS Context Popup using the existing ONE ERP context-popup implementation.
>
> First inspect existing Angular popup/component/service code and all related APIs and SQL tables.
>
> Reuse:
> GET /api/companies
> GET /api/organization/branches?companyId={companyId}
> GET /api/stores?companyId={companyId}&branchId={branchId}
> GET /api/counter-assignments/my-default-counter
>
> Verify actual API contracts instead of assuming them.
>
> Enforce Company → Branch → Store → Counter → Assignment → Operator consistency on the backend.
>
> Do not trust frontend-supplied CompanyId/BranchId/StoreId/CounterId.
>
> Detect an existing OPEN POS session before Continue.
>
> Do not create session data in T042 unless the existing architecture explicitly requires it.
>
> Produce an audit first, then implement only missing/broken pieces.
>
> Add QA for valid context, invalid hierarchy, inactive records, unauthorized records, cross-company access and existing-open-session handling.

## Completion Gate

Context is valid only when all hierarchy relationships are server-verified.

---

# T043 — POS Session Open

## Dependency

T042

## Objective

Implement production-safe POS session opening.

## UI

Existing POS Session Open UI.

Inputs should be limited to business data such as:

- Opening Cash
- Optional opening remarks

Context comes from the validated POS context.

## API

Reuse existing session API if present. Otherwise target an existing POS session command pattern, for example:

```http
POST /api/pos/sessions/open
GET  /api/pos/sessions/current
```

Do not create duplicate endpoints if equivalents exist.

## DB

Existing POSSession fields known from the current architecture include:

- POSSessionId
- SessionNumber
- CompanyId
- BranchId
- StoreId
- CounterId
- CounterAssignmentId
- OperatorId
- OperatorNameSnapshot
- OpenedAt
- OpenedBy
- OpeningCash
- ExpectedClosingCash
- ActualClosingCash
- CashDifference
- ClosedAt
- ClosedBy
- ClosingRemarks
- Status
- Version
- CreatedAt
- CreatedBy
- UpdatedAt
- UpdatedBy

Status:
- 1 Open
- 2 Closed
- 3 Void

Verify actual database names before coding.

## Business rules

- Context must be valid.
- Counter cannot already have an OPEN session.
- Operator must be authorized.
- Opening cash must be valid.
- Session number must be server-generated.
- Opening must be atomic.
- Concurrent open attempts must not create two sessions.

## QA

- Valid open.
- Invalid context.
- Duplicate open.
- Concurrent open.
- Negative/invalid cash.
- Unauthorized operator.
- Closed session followed by new session.
- Cross-company manipulation.
- Refresh/retry does not create duplicate session.

## Code Assistant Prompt

> Implement T043 POS Session Open.
>
> Inspect existing POSSession entity/table, migration, API, DTO, service, repository and Angular session-open UI.
>
> Reuse existing implementation wherever possible.
>
> Enforce:
> - Company/Branch/Store/Counter/Operator authorization
> - one OPEN session per counter
> - server-generated session number
> - valid opening cash
> - atomic transaction
> - concurrency protection
> - retry safety
>
> Do not accept frontend values as authoritative for tenant/company/session ownership.
>
> If a unique constraint/index is required to guarantee one OPEN session per counter, propose the smallest safe migration and implement it only after verifying the current schema.
>
> Add integration tests for duplicate and concurrent session opening.
>
> Completion is not UI-only.

## Completion Gate

```text
One Counter
→ maximum one OPEN Session
```

---

# T044 — POS Session Validation

## Dependency

T043

## Objective

Make session validation authoritative for every POS operation.

## API

Audit all POS endpoints.

Session validation must verify:

```text
JWT/User
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
Operator/Assignment
↓
POSSession
↓
OPEN status
```

## DB

Use POSSession plus related master tables.

Do not duplicate session state in frontend/localStorage as the authority.

## Validation cases

- Session missing.
- Session CLOSED.
- Session VOID.
- Session belongs to another company.
- Session belongs to another branch.
- Session belongs to another store.
- Session belongs to another counter.
- Session belongs to another operator where operator restriction applies.
- Session ID does not match the current context.
- Session was closed while another client was open.

## Code Assistant Prompt

> Implement T044 as the shared server-side POS Session Validation layer.
>
> Inspect every existing POS endpoint and determine where session validation is missing or duplicated.
>
> Create/reuse one authoritative validation/service pattern rather than copying business logic into every controller.
>
> Validate authenticated user, tenant/company, branch, store, counter, operator assignment and POSSession status.
>
> POS Sale, Hold/Recall, Cash In/Out, Refund, Exchange and Session Close must use this validation where applicable.
>
> Do not trust localStorage or frontend context.
>
> Return correct HTTP status codes for unauthorized, forbidden, invalid session and business conflicts.
>
> Add integration tests for cross-context and closed-session access.

## Completion Gate

No POS command may execute against an invalid/closed/wrong-context session.

---

# T045 — POS Sale

## Dependency

T044

## Objective

Connect the POS billing flow to the existing Sales lifecycle.

## UI

Reuse existing Sales Entry/POS screen.

POS should support:

- Product search
- Customer optional/required according to existing rules
- Barcode-ready item entry
- Quantity
- Discount
- GST
- Totals
- Payment
- Save/Post

Do not duplicate Sales Entry business calculations.

## API

Reuse existing Sales API.

The POS sale must ultimately create the existing SalesInvoice/SalesInvoiceItem transaction with:

```text
SourceType = POS
POSSessionId = current OPEN session
```

Verify actual SourceType values in the current code/schema before coding.

## DB

Primary tables:

- SalesInvoice
- SalesInvoiceItem
- POSSessions
- Payment
- PaymentAllocation
- stock transaction tables used by the existing Sales service

## Business rules

- OPEN POS session required.
- Company/Branch/Store/Counter must match session.
- Stock validation must use existing sales logic.
- Tax must use existing sales logic.
- Payment must use existing payment logic.
- Invoice totals must be server-authoritative.
- POSSessionId must be persisted.
- Duplicate submission must not create duplicate invoice.

## QA

- Cash sale.
- UPI/card/other supported method.
- Partial payment if supported.
- GST.
- Discount.
- Stock deduction.
- Invoice number.
- POSSessionId link.
- Closed session rejection.
- Cross-company rejection.
- Duplicate/retry protection.
- Payment reconciliation.
- Stock reconciliation.

## Code Assistant Prompt

> Implement T045 POS Sale by integrating the existing Sales lifecycle into the existing POS screen.
>
> Do not create a second Sales calculation engine.
>
> Inspect the current Sales Entry/POS Angular code and existing POST /api/sales implementation, DTOs, service, stock logic, payment logic and SalesInvoice/SalesInvoiceItem schema.
>
> POS sale must require an OPEN POSSession and persist the correct POSSessionId.
>
> Backend must derive/validate CompanyId, BranchId, StoreId, CounterId and Operator from the authorized session.
>
> Reuse existing pricing, GST, discount, stock and payment calculations.
>
> Ensure the operation is atomic and retry-safe.
>
> Test cash, supported digital payment, GST, discount, stock deduction, session mismatch, closed session and duplicate submission.
>
> Do not change unrelated Sales behavior.

## Completion Gate

T045 is complete only when:

```text
POS Session
   ↓
POS Sale
   ↓
SalesInvoice
   ↓
SalesInvoiceItem
   ↓
Stock
   ↓
Payment
   ↓
Reconciliation
```

all succeed consistently.
