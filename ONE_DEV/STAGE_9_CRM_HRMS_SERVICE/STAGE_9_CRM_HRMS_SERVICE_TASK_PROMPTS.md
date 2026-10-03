
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

# STAGE 9 — CRM / SERVICE / HRMS

## Task Range
`T130–T144`

## Dependency Order

```text
T130 CRM Foundation
  ↓
T131 Lead Management
  ↓
T132 Opportunity Management
  ↓
T133 CRM Activities
  ↓
T134 Service Management
  ↓
T135 Service Billing
  ↓
T136 Employee Master
  ↓
T137 Attendance
  ↓
T138 Leave Management
  ↓
T139 Payroll Foundation
  ↓
T140 Employee Expenses
  ↓
T141 HR Permissions
  ↓
T142 CRM/HR Reports
  ↓
T143 CRM/HR Notifications
  ↓
T144 CRM/Service/HR Business QA
```

---

# T130 — CRM Foundation

## Objective

Customers/leads/contacts and activity foundation using Business Partner where possible.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T130 — CRM Foundation.**
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

# T131 — Lead Management

## Objective

Lead creation, qualification, assignment and conversion.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T131 — Lead Management.**
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

# T132 — Opportunity Management

## Objective

Opportunity pipeline, stages, expected value and activity.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T132 — Opportunity Management.**
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

# T133 — CRM Activities

## Objective

Calls/tasks/follow-ups/reminders and audit.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T133 — CRM Activities.**
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

# T134 — Service Management

## Objective

Service request/ticket/customer service lifecycle.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T134 — Service Management.**
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

# T135 — Service Billing

## Objective

Service/labour/parts billing through existing Sales engine.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T135 — Service Billing.**
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

# T136 — Employee Master

## Objective

Employee/designation/department/branch relationships.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T136 — Employee Master.**
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

# T137 — Attendance

## Objective

Attendance/shift records using actual HR schema.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T137 — Attendance.**
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

# T138 — Leave Management

## Objective

Leave types, requests, approval and balances.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T138 — Leave Management.**
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

# T139 — Payroll Foundation

## Objective

Payroll setup/calculation only from supported HR/accounting schema.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T139 — Payroll Foundation.**
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

# T140 — Employee Expenses

## Objective

Employee expense claim and payment integration.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T140 — Employee Expenses.**
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

# T141 — HR Permissions

## Objective

Employee self-service/manager/admin data scope and authorization.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T141 — HR Permissions.**
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

# T142 — CRM/HR Reports

## Objective

Operational reports using actual transaction/master data.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T142 — CRM/HR Reports.**
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

# T143 — CRM/HR Notifications

## Objective

Follow-up, approval and operational notifications.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T143 — CRM/HR Notifications.**
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

# T144 — CRM/Service/HR Business QA

## Objective

End-to-end customer → service → billing and employee workflows.

## Dependency

Complete the immediately preceding task in this stage before starting this task,
unless the implementation audit proves the dependency is already complete.

## UI / API / DB / QA Prompt

> **Implement T144 — CRM/Service/HR Business QA.**
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
