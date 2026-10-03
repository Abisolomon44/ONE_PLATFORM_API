
# ONE ERP — GLOBAL CODE ASSISTANT CONTRACT

Execute **ONE TASK-ID at a time**.

Before coding:
1. Inspect existing Angular screens/components/routes.
2. Inspect existing API/controller/service/repository/DTOs.
3. Inspect actual SQL schema, migrations and indexes.
4. Inspect existing permissions/data scopes.
5. Identify reusable implementation.
6. Identify missing/broken/duplicate implementation.
7. Do not invent tables, columns, endpoints or business rules.

Mandatory completion:
DB + Migration (if required)
+ Model/DTO
+ Repository
+ Service
+ API
+ UI
+ Permission
+ Validation
+ Integration
+ Business-flow QA
+ Security QA

Use the existing database-per-tenant architecture.

Backend is authoritative for:
Tenant → Company → Branch → Store/Warehouse → User/Role → Business transaction.

Do not bypass APIs with direct production DB changes during functional testing.
Do not create duplicate engines for Sales, Purchase, Stock, Payment, Tax,
Accounting or Document Design.

Do not mark a task complete because only the UI works.

# STAGE 6 — PAYMENTS & ACCOUNTING

## Task Range
`T089–T103`

## Dependency Order

```text
T089 Payment Workspace
  ↓
T090 Payment Types / Methods
  ↓
T091 Customer Receipt
  ↓
T092 Supplier Payment
  ↓
T093 Payment Allocation
  ↓
T094 Refund Accounting
  ↓
T095 Chart of Accounts
  ↓
T096 Journal Entry
  ↓
T097 Cash / Bank Book
  ↓
T098 Day Book
  ↓
T099 Party Ledger
  ↓
T100 Trial Balance
  ↓
T101 Profit & Loss
  ↓
T102 Balance Sheet
  ↓
T103 Accounting Security / Reconciliation QA
```

---

# T089 — Payment Workspace

## Objective

Complete payment entry/register using existing Payment and PaymentAllocation.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T089 — Payment Workspace.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
---

# T090 — Payment Types / Methods

## Objective

Complete payment master and detail flows; avoid duplicate payment masters.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T090 — Payment Types / Methods.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
---

# T091 — Customer Receipt

## Objective

Implement customer receipt allocation against sales/outstanding.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T091 — Customer Receipt.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
---

# T092 — Supplier Payment

## Objective

Implement supplier payment allocation against purchases/outstanding.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T092 — Supplier Payment.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
---

# T093 — Payment Allocation

## Objective

Complete multi-document allocation, partial allocation and unallocated balance handling.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T093 — Payment Allocation.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
---

# T094 — Refund Accounting

## Objective

Reconcile refunds with original transaction and payment allocation.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T094 — Refund Accounting.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
---

# T095 — Chart of Accounts

## Objective

Implement/complete COA structure only from actual accounting schema.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T095 — Chart of Accounts.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
---

# T096 — Journal Entry

## Objective

Implement balanced journal entry workflow with validation and audit.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T096 — Journal Entry.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
---

# T097 — Cash / Bank Book

## Objective

Complete cash and bank ledger views from actual transactions.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T097 — Cash / Bank Book.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
---

# T098 — Day Book

## Objective

Complete chronological accounting transaction view.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T098 — Day Book.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
---

# T099 — Party Ledger

## Objective

Customer/supplier ledger with opening, transactions, receipts/payments and closing.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T099 — Party Ledger.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
---

# T100 — Trial Balance

## Objective

Server-side debit/credit aggregation and balance validation.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T100 — Trial Balance.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
---

# T101 — Profit & Loss

## Objective

Implement P&L from supported accounting data; document unsupported mappings.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T101 — Profit & Loss.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
---

# T102 — Balance Sheet

## Objective

Implement balance-sheet reporting only from supported account classifications.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T102 — Balance Sheet.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
---

# T103 — Accounting Security / Reconciliation QA

## Objective

Double-entry validation, payment reconciliation, sales/purchase reconciliation and authorization.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T103 — Accounting Security / Reconciliation QA.**
>
> Inspect the existing ONE ERP codebase first. Find the existing screen,
> Angular component/service, API/controller, DTO/model, repository/service,
> SQL tables/columns, migrations, indexes and permission/data-scope rules.
>
> Reuse existing implementation wherever possible. Do not create duplicate
> screens, components, endpoints or tables.
>
> Verify the complete flow:
>
> `UI → API → Business Service → Repository → DB → Side Effects → Response`
>
> Verify tenant/company/branch/warehouse/store/user permission boundaries.
>
> Use server-side validation and authoritative calculations.
>
> If schema support is missing, identify the minimum required migration first.
> Do not invent fields merely to make the UI work.
>
> Implement the current task only.
>
> Test:
> - happy path
> - validation failure
> - authorization failure
> - tenant/company isolation
> - duplicate/retry where applicable
> - persistence
> - integration with dependent modules
> - regression of existing functionality
>
> Use the real DEV/integration runtime and production API path for verification.
>
> Before marking complete, report:
> 1. Existing files reused
> 2. Files changed
> 3. APIs used/changed
> 4. DB tables/columns used
> 5. Permissions enforced
> 6. QA cases executed
> 7. Remaining known gaps
>
## Completion Gate

```text
DB
+ API
+ Business Logic
+ UI
+ Permission
+ Validation
+ Integration
+ QA
= COMPLETE
```
