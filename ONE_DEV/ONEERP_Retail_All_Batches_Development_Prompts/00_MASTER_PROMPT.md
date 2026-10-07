# ONEERP / UNITY BUSINESS — RETAIL DEVELOPMENT MASTER PROMPT

This is an EXISTING ONEERP application. Do not build a new ERP from scratch.

For every task:
SOURCE → AUDIT → WORKSPACE → DEPENDENCY TABLES → INTEGRATION TABLES → APIs → UI SCREENS → PERMISSIONS → IMPLEMENTATION → TESTING → VERIFICATION

## Source of Truth
Inspect actual Angular routes/components/services, ASP.NET Core controllers/DTOs/services/repositories/queries, SQL tables/columns/PK/FK/indexes, migrations/seeds, permissions and data scopes.

Never invent existing tables, APIs, routes, components, IDs, or business rules.

## Classification
COMPLETE / PARTIAL / MISSING / DUPLICATE / BLOCKED / NOT REQUIRED

Reuse existing implementations before creating new ones.

## Mandatory task format

### 1. Workspace / Module
| Screen | Workspace | Module | Route | Component | Status | Action |

### 2. Dependency Table List
| Table | Purpose | Relationship | Why Required | Status |

### 3. Integration Table List
| Table | Integration Purpose | Relationship | Usage | Status |

### 4. API List
| Method | Endpoint | Purpose | Status | Controller | Service |

### 5. UI Screen List
| Screen | Route | Component | API | DB Tables | Status | Action |

### 6. Permission
| Permission | Action | Scope | Status |

### 7. Business Flow
Show actual dependency and downstream integration.

### 8. Implementation
DB → Migration → Model/Entity → DTO → Repository/Query → Service → Controller → Angular Service → UI → Permission → Integration

### 9. Testing
DB, API, UI, authorization, tenant/company/branch isolation, business flow, reconciliation and runtime.

### 10. Final Status
COMPLETE / PARTIAL / BLOCKED with remaining gaps.

## Workspace rule
Do not create a new workspace if an existing workspace can correctly own the feature. Inspect the actual workspace/menu/permission configuration first.

Initial candidates only:
- BUSINESS MASTER: product, category, subcategory, brand, unit, variant, model, specification, UOM, barcode
- INVENTORY: serial/IMEI, batch, expiry, stock tracking
- SERVICE or existing appropriate workspace: warranty

## Database rule
If existing table is sufficient: reuse it.
If incomplete: extend through migration.
If genuinely missing: create migration.
Never create runtime tables, duplicate masters, or destructive migrations without explicit justification.

## API rule
Reuse existing APIs where possible. No fake/mock production APIs, hardcoded IDs, direct DB access from Angular, or authorization bypass.

## Security
Enforce backend:
Tenant → Company → Branch → Warehouse/Store → User → Role → Permission → Data Scope.

## UI
Reuse existing ONEERP UI system, shared components, dark/light themes, tables, forms, dialogs, alerts, loading/empty/error states.

## Completion rule
Compilation alone is NOT completion. Complete only after applicable DB + API + UI + permission + validation + business integration + security + runtime verification.

## Execution control
Work only on the requested batch/feature. Do not jump ahead. Complete, verify, then STOP and wait for NEXT.
