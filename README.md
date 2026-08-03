# ONE ERP — Multi-Tenant SaaS Platform

A production-style multi-tenant ERP stack built with:

- **Backend**: .NET 9 Web APIs (Platform + ERP), Dapper-only data access, SQL Server, JWT/refresh auth, BCrypt, FluentValidation, Serilog, Swagger.
- **Frontend**: Two Angular 21 (standalone, signal-based) apps — a Platform Console and a per-tenant ERP Workspace — with a CSS-variable theme engine and `lucide-angular` icons.

## Solution Layout

```
D:\AWS\ERP
├── ONEERP.sln
├── ONEERP.Shared               Shared constants, models, helpers, exceptions
├── ONEERP.Platform.API         Multi-tenant control plane (tenants, plans, subscriptions)
├── ONEERP.ERP.API              Tenant workspace API (users, roles, company, dashboard, settings)
├── ONEERP.Platform.UI          Angular app — Platform Console  (http://localhost:4200)
├── ONEERP.ERP.UI               Angular app — Tenant Workspace   (http://localhost:4300)
└── sql
    ├── platform_schema.sql     Platform DB schema + seed (run once via sqlcmd)
    ├── erp_schema.sql          Per-tenant schema (embedded resource, applied on provisioning)
    └── erp_seed.sql            Per-tenant seed data (embedded resource)
```

## Architecture

- **Database-per-tenant**: creating a tenant provisions a dedicated SQL database (schema + seed applied automatically via embedded `erp_*.sql` scripts).
- **JWT auth**: short-lived access tokens + refresh tokens (stored per tenant). ERP tokens carry `tenant_id`, `tenant_code`, `company_id`, role and `permission` claims; the `PermissionAuthorizationFilter` enforces fine-grained permissions on every endpoint.
- **Dapper only** — no EF Core or ORM magic; raw SQL with an `ISqlHelper` wrapper and tenant-aware connection resolution.
- **Angular UIs** share a design system (`styles.css`) and reusable Base components (buttons, inputs, dropdowns, dialogs, tables, pagination, toasts, permission directives). Both apps use lazy-loaded routes, auth/API interceptors, and a signal-based theme service (light/dark + 5 accent colors).

## Prerequisites

- .NET 9 SDK
- Node.js 22+
- SQL Server (Windows Auth or SQL Auth)

## Database Setup

1. Provision the platform database (creates `ONEERP_PLATFORM` with admin user, plans, settings):

   ```powershell
   sqlcmd -S "LAPTOP-BOR8IKB8\MSSQL2022" -E -i sql\platform_schema.sql
   ```

2. Point the APIs at your SQL Server. Both APIs read `ConnectionStrings:Master` (ERP also has `PlatformDatabase:Name`). Adjust in:

   - `ONEERP.Platform.API\appsettings.json`
   - `ONEERP.ERP.API\appsettings.json`

## Running the APIs

```powershell
# Platform API — http://localhost:5180
$env:ASPNETCORE_URLS = "http://localhost:5180"
dotnet run --project ONEERP.Platform.API

# ERP API — http://localhost:5181
$env:ASPNETCORE_URLS = "http://localhost:5181"
dotnet run --project ONEERP.ERP.API
```

Swagger: `http://localhost:5180/swagger` and `http://localhost:5181/swagger`.

## Running the UIs

```powershell
# Platform Console — http://localhost:4200  (proxies /api → 5180)
cd ONEERP.Platform.UI
npm install
npm start

# ERP Workspace — http://localhost:4300    (proxies /api → 5181)
cd ONEERP.ERP.UI
npm install
npm start
```

> Change the port in `proxy.conf.json` and the API CORS origins (`Cors:Origins` in each `appsettings.json`) to match.

## Seed Accounts

| App | URL | Username | Password |
| --- | --- | --- | --- |
| Platform Console | http://localhost:4200 | `admin` | `PlatformAdmin@123` |
| ERP Workspace (tenant `ACME`) | http://localhost:4300 | `admin` | `Admin@123` |

Creating a tenant from the Platform Console automatically provisions a new database and seeds the same `admin / Admin@123` account for that tenant.

## Production Builds

```powershell
cd ONEERP.Platform.UI; npx ng build   # dist\ONEERP.Platform.UI
cd ONEERP.ERP.UI;     npx ng build   # dist\ONEERP.ERP.UI
```
