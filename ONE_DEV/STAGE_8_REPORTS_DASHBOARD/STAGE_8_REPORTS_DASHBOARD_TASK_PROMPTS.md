
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

# STAGE 8 — REPORTS & DASHBOARD

## Task Range
`T117–T129`

## Dependency Order

```text
T117 Report Framework Audit
  ↓
T118 Sales Reports
  ↓
T119 Purchase Reports
  ↓
T120 Inventory Reports
  ↓
T121 Payment Reports
  ↓
T122 Tax Reports
  ↓
T123 Customer / Supplier Reports
  ↓
T124 Profitability Reports
  ↓
T125 Dashboard KPIs
  ↓
T126 Dashboard Drill-down
  ↓
T127 Export / Print
  ↓
T128 Report Security / Performance
  ↓
T129 Reports Business QA
```

---

# T117 — Report Framework Audit

## Objective

Audit existing Reports workspace, APIs, components, filters and permissions; do not create duplicate workspace.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T117 — Report Framework Audit.**
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

# T118 — Sales Reports

## Objective

Complete schema-backed sales reports and reconciliation.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T118 — Sales Reports.**
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

# T119 — Purchase Reports

## Objective

Complete schema-backed purchase reports.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T119 — Purchase Reports.**
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

# T120 — Inventory Reports

## Objective

Stock ledger, valuation, movement, ageing/expiry where supported.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T120 — Inventory Reports.**
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

# T121 — Payment Reports

## Objective

Receipts, payments, allocations, outstanding and reconciliation.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T121 — Payment Reports.**
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

# T122 — Tax Reports

## Objective

GST/tax reports using actual tax transaction data.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T122 — Tax Reports.**
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

# T123 — Customer / Supplier Reports

## Objective

Party-wise sales, purchases, balances and transaction drill-down.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T123 — Customer / Supplier Reports.**
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

# T124 — Profitability Reports

## Objective

Product/customer/branch profitability only where cost/accounting data supports it.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T124 — Profitability Reports.**
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

# T125 — Dashboard KPIs

## Objective

Complete real-time/near-real-time KPIs from authoritative APIs.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T125 — Dashboard KPIs.**
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

# T126 — Dashboard Drill-down

## Objective

Dashboard cards/charts must drill into existing reports/details.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T126 — Dashboard Drill-down.**
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

# T127 — Export / Print

## Objective

Standardize Excel/CSV/PDF export using current filtered dataset.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T127 — Export / Print.**
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

# T128 — Report Security / Performance

## Objective

Server-side scope, pagination, aggregation, indexing and large-data testing.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T128 — Report Security / Performance.**
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

# T129 — Reports Business QA

## Objective

Cross-check reports against invoice, stock, payment, tax and accounting totals.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T129 — Reports Business QA.**
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
