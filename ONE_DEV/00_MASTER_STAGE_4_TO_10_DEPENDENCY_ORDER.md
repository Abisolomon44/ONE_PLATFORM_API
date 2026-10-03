# ONE ERP — STAGE 4 TO 10 MASTER DEPENDENCY ROADMAP

| Stage | Range | Target |
|---|---:|---|
| Stage 4 | T059–T073 | Purchase |
| Stage 5 | T074–T088 | Inventory / Stock |
| Stage 6 | T089–T103 | Payments & Accounting |
| Stage 7 | T104–T116 | GST / Tax & Compliance |
| Stage 8 | T117–T129 | Reports & Dashboard |
| Stage 9 | T130–T144 | CRM / Service / HRMS |
| Stage 10 | T145–T160 | Manufacturing / Industry / SaaS |

## MASTER DEPENDENCY

```text
STAGE 2 — SALES COMPLETE
        ↓
STAGE 3 — POS COMPLETE
        ↓
STAGE 4 — PURCHASE
        ↓
STAGE 5 — INVENTORY
        ↓
STAGE 6 — PAYMENTS + ACCOUNTING
        ↓
STAGE 7 — GST + TAX
        ↓
STAGE 8 — REPORTS + DASHBOARD
        ↓
STAGE 9 — CRM + SERVICE + HRMS
        ↓
STAGE 10 — MANUFACTURING + INDUSTRY + SAAS
        ↓
FULL ONE ERP
```

## Stage completion rule

A stage is not complete when its screens are finished.

It is complete only after:

```text
DB
→ API
→ Business Logic
→ UI
→ Permission
→ Validation
→ Integration
→ Security
→ Performance where applicable
→ Business-flow QA
→ Regression
→ Production-readiness
```

## Important

These Stage 4–10 task definitions are the development roadmap based on the
existing ONE ERP architecture and previously established module order. For
each task, the coding agent must audit the actual current schema/code before
adding or changing implementation.
