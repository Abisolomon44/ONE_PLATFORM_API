
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

# STAGE 4 — PURCHASE

## Task Range
`T059–T073`

## Dependency Order

```text
T059 Purchase Dashboard / Workspace
  ↓
T060 Purchase Context & Supplier
  ↓
T061 Purchase Entry
  ↓
T062 Purchase Pricing & Tax
  ↓
T063 Purchase Payment
  ↓
T064 Purchase Posting
  ↓
T065 Purchase Register
  ↓
T066 Purchase Detail / Edit
  ↓
T067 Purchase Cancellation
  ↓
T068 Purchase Return
  ↓
T069 Purchase Return Entry
  ↓
T070 Debit Note
  ↓
T071 Purchase Print
  ↓
T072 Purchase Security / Idempotency
  ↓
T073 Purchase Business-flow Testing
```

---

# T059 — Purchase Dashboard / Workspace

## Objective

Audit existing Purchase workspace, register, filters, permissions and APIs; complete the operational entry point without duplicating existing screens.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T059 — Purchase Dashboard / Workspace.**
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

# T060 — Purchase Context & Supplier

## Objective

Complete Company/Branch/Warehouse/Supplier/Price List/Tax context using real APIs and server-side scope validation.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T060 — Purchase Context & Supplier.**
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

# T061 — Purchase Entry

## Objective

Complete purchase invoice/entry with Product/Service where supported, quantity, rate, discount, GST, totals and supplier details.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T061 — Purchase Entry.**
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

# T062 — Purchase Pricing & Tax

## Objective

Integrate purchase price lists, tax lookup, HSN/SAC and supplier-specific pricing using existing masters.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T062 — Purchase Pricing & Tax.**
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

# T063 — Purchase Payment

## Objective

Integrate supplier payment/credit/partial payment with existing Payment and PaymentAllocation architecture.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T063 — Purchase Payment.**
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

# T064 — Purchase Posting

## Objective

Post purchase atomically and update stock, tax and payable balances through backend services.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T064 — Purchase Posting.**
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

# T065 — Purchase Register

## Objective

Complete server-side filtering, pagination, sorting, drill-down and export using actual Purchase tables.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T065 — Purchase Register.**
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

# T066 — Purchase Detail / Edit

## Objective

Complete purchase detail and permitted edit workflow with audit/history protection.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T066 — Purchase Detail / Edit.**
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

# T067 — Purchase Cancellation

## Objective

Implement controlled cancellation/reversal; never physically delete posted financial history.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T067 — Purchase Cancellation.**
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

# T068 — Purchase Return

## Objective

Implement return of purchased goods with stock reversal and supplier financial adjustment.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T068 — Purchase Return.**
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

# T069 — Purchase Return Entry

## Objective

Complete return UI/API validation for partial/full returns and duplicate protection.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T069 — Purchase Return Entry.**
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

# T070 — Debit Note

## Objective

Implement debit-note workflow only after verifying schema/business support.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T070 — Debit Note.**
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

# T071 — Purchase Print

## Objective

Use existing Document Design engine for purchase invoice/return/debit-note printing.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T071 — Purchase Print.**
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

# T072 — Purchase Security / Idempotency

## Objective

Audit API authorization, tenant/company/branch/warehouse scope and duplicate-save protection.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T072 — Purchase Security / Idempotency.**
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

# T073 — Purchase Business-flow Testing

## Objective

Retail/wholesale purchase, credit purchase, return, cancellation, payment, tax, stock and print reconciliation.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T073 — Purchase Business-flow Testing.**
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
