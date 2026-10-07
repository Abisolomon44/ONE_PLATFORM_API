# ONEERP MASTER APPLICATION INVENTORY

_Static source audit: 2026-10-07, Asia/Calcutta. No tenant or platform DB connection was queried._

> **Evidence boundary:** This document inventories current source files and SQL declarations. Runtime endpoint health, actual tenant schema/data, migration application state, claims, scope enforcement under a real identity, and performance are UNKNOWN unless explicitly tested. Repeated full-install/migration table declarations are consolidated by name; possible differences are called out.

The working tree contains pre-existing changes and untracked files. This audit does not attribute those changes to this documentation task.

## 1. Project Overview

| Project | Role | Evidence |
|---|---|---|
| `ONEERP.ERP.UI` | Angular tenant ERP UI | `ONEERP.ERP.UI/package.json`, `src/app/app.routes.ts` |
| `ONEERP.ERP.API` | ASP.NET Core tenant API | `ONEERP.ERP.API/Program.cs` |
| `ONEERP.Platform.UI` | Angular platform console | `ONEERP.Platform.UI/package.json`, `src/app/app.routes.ts` |
| `ONEERP.Platform.API` | ASP.NET Core tenant/platform administration and migration API | `ONEERP.Platform.API/Program.cs` |
| `ONEERP.Shared` | Shared .NET models, permission constants, exceptions and contracts | `ONEERP.Shared/` |
| `sql/` | Tenant full schema, platform schema and tenant migration scripts | `sql/*.sql` |
| `ONEERP.Platform.API.Tests` | Platform API test project | `ONEERP.Platform.API.Tests/*.csproj` |

## 2. Project Structure

- Tenant UI: `src/app/core`, `layout`, `pages`, `shared`; lazy routes in `ONEERP.ERP.UI/src/app/app.routes.ts`.
- Platform UI: `src/app/core`, `layout`, `pages`, `shared`; routes in `ONEERP.Platform.UI/src/app/app.routes.ts`.
- Tenant API: `Controllers`, `DTOs`, `Data`, `Middleware`, `Models`, `Repositories`, `Security`, `Services`.
- Platform API: same backend layers plus migration catalog/execution code.
- Shared project: constants, auth claims, permission codes, API responses, exceptions and domain models.
- SQL source: `erp_full.sql` for tenant installation, `platform_schema.sql` for platform DB, and `erp_migration_*.sql` for registered tenant migrations.

## 3. Module / Feature List

Feature names are derived from UI page-folder and route names; they are not completion claims.

- `access-denied`
- `actions`
- `address-types`
- `adminitration`
- `app-shell.ts`
- `barcode`
- `base-button.ts`
- `base-controls.ts`
- `base-data.ts`
- `base-feedback.ts`
- `branch`
- `brand`
- `business-master`
- `business-partner-roles`
- `business-partners`
- `business-types`
- `category`
- `company`
- `company-groups`
- `contact-administrator`
- `contact-types`
- `coupon`
- `currencies`
- `dashboard`
- `data-scopes`
- `discount-rule`
- `document-design`
- `document-types`
- `domains`
- `enterprise-permissions`
- `fields`
- `gst-registration-types`
- `hsn-sac`
- `industry-types`
- `inventory`
- `invoice-template`
- `languages`
- `locations`
- `login`
- `master-import`
- `modules`
- `offer`
- `organization-types`
- `payment`
- `payment-entry`
- `payment-list`
- `payment-method`
- `payment-method-detail`
- `payment-type`
- `pos`
- `price-list`
- `price-master`
- `price-type`
- `product`
- `purchase`
- `purchase-cancel`
- `purchase-delete`
- `purchase-edit`
- `purchase-entry`
- `purchase-list`
- `purchase-management`
- `purchase-register`
- `purchase-return`
- `purchase-return-entry`
- `purchase-return-management`
- `purchase-view`
- `reports`
- `role-field-permissions`
- `role-permission-matrix`
- `roles`
- `sales`
- `sales-entry`
- `sales-list`
- `sales-return`
- `screens`
- `service`
- `service-category`
- `settings`
- `shared`
- `stock`
- `subcategory`
- `submodules`
- `system-master`
- `tax`
- `tax-type-system`
- `tenant-configuration`
- `time-zones`
- `unit`
- `unit-conversion`
- `user-data-scope-overrides`
- `user-permission-overrides`
- `users`
- `workflow-permissions`
- `workspace`
- `workspace-template`
- `workspaces`
- Platform console: login, dashboard, tenants, plans, subscriptions, migrations and settings.

## 4. Screen / Route Master List

Tenant child routes inherit the root shell `authGuard` and `navigationChildGuard`. Route-level guard column shows what was parsed near each route.

### Tenant UI

| # | Route | Title | Component | Redirect | Guard / data | Source |
|---:|---|---|---|---|---|---|
| 1 | `/login` | Login | `LoginPage` | — | canActivate: guestGuard | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 2 | `/contact-administrator` | Contact Administrator | `ContactAdministratorPage` | — | canActivate: authGuard | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 3 | `/access-denied` | Access Denied | `AccessDeniedPage` | — | canActivate: authGuard | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 4 | `/` | Root | `AppShell` | — | canActivate: authGuard; canActivateChild: navigationChildGuard | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 5 | `/` | Root | — | `dashboard` | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 6 | `/dashboard` | Dashboard | `DashboardPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 7 | `/workspace/:id` | Workspace | `WorkspacePage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 8 | `/users` | Users | `UsersPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 9 | `/roles` | Roles | `RolesPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 10 | `/administration` | Administration | — | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 11 | `/business-master` | Business Master | `BusinessMasterPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 12 | `/company` | Company | `CompanyPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 13 | `/branch` | Branch | `BranchPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 14 | `/product-categories` | Product Categories | `CategoryPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 15 | `/product-subcategories` | Product Sub Categories | `SubCategoryPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 16 | `/brands` | Brands | `BrandPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 17 | `/units` | Units | `UnitPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 18 | `/products` | Products | `ProductPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 19 | `/tax-type-systems` | Tax Type Systems | `TaxTypeSystemPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 20 | `/taxes` | Taxes | `TaxPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 21 | `/price-types` | Price Types | `PriceTypePage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 22 | `/unit-conversions` | Unit Conversions | `UnitConversionPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 23 | `/barcodes` | Barcodes | `BarcodePage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 24 | `/hsn-sacs` | HSN / SAC | `HsnSacPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 25 | `/service-categories` | Service Categories | `ServiceCategoryPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 26 | `/services` | Services | `ServicePage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 27 | `/price-lists` | Price Lists | `PriceListPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 28 | `/price-master` | Price Master | `PriceMasterPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 29 | `/discount-rules` | Discount Rules | `DiscountRulePage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 30 | `/offers` | Offers | `OfferPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 31 | `/coupons` | Coupons | `CouponPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 32 | `/master-import` | Master Import | `MasterImportPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 33 | `/import-logs` | Import Logs | `MasterImportPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 34 | `/tenant-configuration` | Tenant Configuration | `TenantConfigurationPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 35 | `/purchase-entry` | Purchase Entry | `PurchaseEntryPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 36 | `/purchase-register` | Purchase Register | `PurchaseRegisterPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 37 | `/purchase-view/:id` | Purchase View | `PurchaseViewPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 38 | `/purchase-edit/:id` | Purchase Edit | `PurchaseEditPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 39 | `/purchase-cancel/:id` | Purchase Cancel | `PurchaseCancelPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 40 | `/purchase-delete/:id` | Purchase Delete | `PurchaseDeletePage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 41 | `/sales-entry` | Sales Entry | `SalesEntryPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 42 | `/sales` | Sales | `SalesWorkspace` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 43 | `/pos` | POS | `PosPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 44 | `/purchase` | Purchase | `PurchaseWorkspace` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 45 | `/purchases/:id` | Purchase Management | `PurchaseManagementPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 46 | `/sales-return` | Sales Returns | `SalesReturnPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 47 | `/inventory` | Inventory | `InventoryPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 48 | `/purchase-returns` | Purchase Returns | `PurchaseReturnPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 49 | `/purchase-returns/new` | Purchase Return Entry | `PurchaseReturnEntryPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 50 | `/purchase-returns/:id` | Purchase Return Management | `PurchaseReturnManagementPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 51 | `/stock` | Stock | `StockPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 52 | `/reports` | Reports | `ReportsWorkspace` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 53 | `/payment-type` | Payment Types | `PaymentTypePage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 54 | `/payment-method` | Payment Methods | `PaymentMethodPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 55 | `/payment-method-detail/:paymentMethodId` | Payment Method Details | `PaymentMethodDetailPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 56 | `/payment-entry` | Payment Entry | `PaymentEntryPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 57 | `/payment` | Payment | `PaymentWorkspace` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 58 | `/department` | Department | — | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 59 | `/warehouse` | Warehouse | — | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 60 | `/employee` | Employees | — | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 61 | `/stores` | Stores | — | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 62 | `/counters` | Counters | — | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 63 | `/operators` | Operators | — | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 64 | `/counter-assignments` | Counter Assignments | — | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 65 | `/operator-types` | Operator Types | — | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 66 | `/store-types` | Store Types | — | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 67 | `/sources` | Sources | — | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 68 | `/pos-sessions` | POS Sessions | — | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 69 | `/designation` | Designation | — | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 70 | `/finance-year` | Financial Year | `FinanceYear` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 71 | `/system-master` | System Master | `SystemMasterPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 72 | `/settings` | Settings | `SettingsPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 73 | `/role-permission-matrix` | Role Permission Matrix | `RolePermissionMatrixPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 74 | `/enterprise-permissions` | Enterprise Permissions | `EnterprisePermissionsPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 75 | `/workspaces` | Workspaces | `WorkspacesPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 76 | `/domains` | Domains | `DomainsPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 77 | `/modules` | Modules | `ModulesPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 78 | `/submodules` | Sub Modules | `SubModulesPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 79 | `/screens` | Screens | `ScreensPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 80 | `/fields` | Fields | `FieldsPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 81 | `/permission-actions-list` | Actions | `ActionsPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 82 | `/user-permission-overrides` | User Permission Overrides | `UserPermissionOverridesPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 83 | `/role-field-permissions` | Role Field Permissions | `RoleFieldPermissionsPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 84 | `/data-scopes` | Data Scopes | `DataScopesPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 85 | `/user-data-scope-overrides` | User Data Scope Overrides | `UserDataScopeOverridesPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 86 | `/workflow-permissions` | Workflow Permissions | `WorkflowPermissionsPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 87 | `/business-partner-roles` | Business Partner Roles | `BusinessPartnerRolesPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 88 | `/business-partners` | Business Partners | `BusinessPartnersPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 89 | `/invoice-templates` | Invoice Design | `InvoiceTemplatePage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 90 | `/document-design` | Document Design | `DocumentDesignPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 91 | `/document-designer` | Document Designer | `DocumentDesignerPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 92 | `/document-design/preview` | Document Preview | `DocumentPreviewPage` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 93 | `/document-design/master/template-components` | Template Components | `TemplateComponentsMasterComponent` | — | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 94 | `/invoice-types` | Invoice Types | `DocumentMasterPageComponent` | — | Inherited/none; master: 'types' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 95 | `/document-design/master/types` | Document Types | `DocumentMasterPageComponent` | — | Inherited/none; master: 'types' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 96 | `/invoice-paper-sizes` | Paper Sizes | `DocumentMasterPageComponent` | — | Inherited/none; master: 'paper-sizes' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 97 | `/document-design/master/paper-sizes` | Paper Sizes | `DocumentMasterPageComponent` | — | Inherited/none; master: 'paper-sizes' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 98 | `/invoice-template-categories` | Template Categories | `DocumentMasterPageComponent` | — | Inherited/none; master: 'categories' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 99 | `/document-design/master/categories` | Template Categories | `DocumentMasterPageComponent` | — | Inherited/none; master: 'categories' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 100 | `/invoice-template-variables` | Template Variables | `DocumentMasterPageComponent` | — | Inherited/none; master: 'variables' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 101 | `/document-design/master/variables` | Template Variables | `DocumentMasterPageComponent` | — | Inherited/none; master: 'variables' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 102 | `/invoice-fonts` | Fonts | `DocumentMasterPageComponent` | — | Inherited/none; master: 'fonts' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 103 | `/document-design/master/fonts` | Fonts | `DocumentMasterPageComponent` | — | Inherited/none; master: 'fonts' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 104 | `/print-orientations` | Print Orientations | `DocumentMasterPageComponent` | — | Inherited/none; master: 'orientations' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 105 | `/document-design/master/orientations` | Print Orientations | `DocumentMasterPageComponent` | — | Inherited/none; master: 'orientations' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 106 | `/document-design/master/units` | Print Units | `DocumentMasterPageComponent` | — | Inherited/none; master: 'units' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 107 | `/printer-types` | Printer Types | `DocumentMasterPageComponent` | — | Inherited/none; master: 'printer-types' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 108 | `/document-design/master/printer-types` | Printer Types | `DocumentMasterPageComponent` | — | Inherited/none; master: 'printer-types' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 109 | `/printer-models` | Printer Models | `DocumentMasterPageComponent` | — | Inherited/none; master: 'printer-models' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 110 | `/document-design/master/printer-models` | Printer Models | `DocumentMasterPageComponent` | — | Inherited/none; master: 'printer-models' | `ONEERP.ERP.UI/src/app/app.routes.ts` |
| 111 | `/**` | ** | — | `dashboard` | Inherited/none | `ONEERP.ERP.UI/src/app/app.routes.ts` |

### Platform UI

| # | Route | Title | Component | Redirect | Guard / data | Source |
|---:|---|---|---|---|---|---|
| 1 | `/login` | login | `LoginPage` | — | canActivate: guestGuard | `ONEERP.Platform.UI/src/app/app.routes.ts` |
| 2 | `/` | Root | `AppShell` | — | canActivate: authGuard | `ONEERP.Platform.UI/src/app/app.routes.ts` |
| 3 | `/` | Root | — | `dashboard` | Inherited/none | `ONEERP.Platform.UI/src/app/app.routes.ts` |
| 4 | `/dashboard` | dashboard | `DashboardPage` | — | Inherited/none | `ONEERP.Platform.UI/src/app/app.routes.ts` |
| 5 | `/tenants` | tenants | `TenantsPage` | — | Inherited/none | `ONEERP.Platform.UI/src/app/app.routes.ts` |
| 6 | `/plans` | plans | `PlansPage` | — | Inherited/none | `ONEERP.Platform.UI/src/app/app.routes.ts` |
| 7 | `/subscriptions` | subscriptions | `SubscriptionsPage` | — | Inherited/none | `ONEERP.Platform.UI/src/app/app.routes.ts` |
| 8 | `/migrations` | migrations | `MigrationsPage` | — | Inherited/none | `ONEERP.Platform.UI/src/app/app.routes.ts` |
| 9 | `/settings` | settings | `SettingsPage` | — | Inherited/none | `ONEERP.Platform.UI/src/app/app.routes.ts` |
| 10 | `/**` | ** | — | — | Inherited/none | `ONEERP.Platform.UI/src/app/app.routes.ts` |

## 5. Angular Component Master List

All `@Component` classes in the two Angular source trees. Unrouted components may be embedded children; route absence is not proof of dead code.

| App | Component | Selector | File | Direct route(s) |
|---|---|---|---|---|
| Platform UI | `App` | `app-root` | `ONEERP.Platform.UI/src/app/app.ts` | No direct route match |
| Platform UI | `AppShell` | `app-shell` | `ONEERP.Platform.UI/src/app/layout/app-shell.ts` | `/`, `/` |
| Platform UI | `BaseButton` | `base-button` | `ONEERP.Platform.UI/src/app/shared/base-button.ts` | No direct route match |
| Platform UI | `BaseCard` | `base-card` | `ONEERP.Platform.UI/src/app/shared/base-data.ts` | No direct route match |
| Platform UI | `BaseDialog` | `base-dialog` | `ONEERP.Platform.UI/src/app/shared/base-feedback.ts` | No direct route match |
| Platform UI | `BaseInput` | `base-input` | `ONEERP.Platform.UI/src/app/shared/base-controls.ts` | No direct route match |
| Platform UI | `DashboardPage` | `app-dashboard` | `ONEERP.Platform.UI/src/app/pages/dashboard/dashboard.ts` | `/dashboard`, `/dashboard` |
| Platform UI | `LoginPage` | `app-login` | `ONEERP.Platform.UI/src/app/pages/login/login.ts` | `/login`, `/login` |
| Platform UI | `MigrationsPage` | `app-migrations` | `ONEERP.Platform.UI/src/app/pages/migrations/migrations.ts` | `/migrations` |
| Platform UI | `PlansPage` | `app-plans` | `ONEERP.Platform.UI/src/app/pages/plans/plans.ts` | `/plans` |
| Platform UI | `SettingsPage` | `app-settings` | `ONEERP.Platform.UI/src/app/pages/settings/settings.ts` | `/settings`, `/settings` |
| Platform UI | `SubscriptionsPage` | `app-subscriptions` | `ONEERP.Platform.UI/src/app/pages/subscriptions/subscriptions.ts` | `/subscriptions` |
| Platform UI | `TenantsPage` | `app-tenants` | `ONEERP.Platform.UI/src/app/pages/tenants/tenants.ts` | `/tenants` |
| Tenant UI | `AccessDeniedPage` | `app-access-denied` | `ONEERP.ERP.UI/src/app/pages/access-denied/access-denied.ts` | `/access-denied` |
| Tenant UI | `ActionsPage` | `app-actions` | `ONEERP.ERP.UI/src/app/pages/actions/actions.ts` | `/permission-actions-list` |
| Tenant UI | `AddressTypesPage` | `app-address-types` | `ONEERP.ERP.UI/src/app/pages/address-types/address-types.ts` | No direct route match |
| Tenant UI | `AdministrationWorkspace` | `app-administration-workspace` | `ONEERP.ERP.UI/src/app/pages/adminitration/administration-workspace/administration-workspace.ts` | No direct route match |
| Tenant UI | `App` | `app-root` | `ONEERP.ERP.UI/src/app/app.ts` | No direct route match |
| Tenant UI | `AppShell` | `app-shell` | `ONEERP.ERP.UI/src/app/layout/app-shell.ts` | `/`, `/` |
| Tenant UI | `BarcodePage` | `app-barcode` | `ONEERP.ERP.UI/src/app/pages/barcode/barcode.ts` | `/barcodes` |
| Tenant UI | `BaseButton` | `base-button` | `ONEERP.ERP.UI/src/app/shared/base-button.ts` | No direct route match |
| Tenant UI | `BaseCard` | `base-card` | `ONEERP.ERP.UI/src/app/shared/base-data.ts` | No direct route match |
| Tenant UI | `BaseDialog` | `base-dialog` | `ONEERP.ERP.UI/src/app/shared/base-feedback.ts` | No direct route match |
| Tenant UI | `BaseInput` | `base-input` | `ONEERP.ERP.UI/src/app/shared/base-controls.ts` | No direct route match |
| Tenant UI | `BranchPage` | `app-branch` | `ONEERP.ERP.UI/src/app/pages/branch/branch.ts` | `/branch` |
| Tenant UI | `BrandPage` | `app-brand` | `ONEERP.ERP.UI/src/app/pages/brand/brand.ts` | `/brands` |
| Tenant UI | `BusinessMasterPage` | `app-business-master` | `ONEERP.ERP.UI/src/app/pages/business-master/business-master.ts` | `/business-master` |
| Tenant UI | `BusinessPartnerRolesPage` | `app-business-partner-roles` | `ONEERP.ERP.UI/src/app/pages/business-partner-roles/business-partner-roles.ts` | `/business-partner-roles` |
| Tenant UI | `BusinessPartnersPage` | `app-business-partners` | `ONEERP.ERP.UI/src/app/pages/business-partners/business-partners.ts` | `/business-partners` |
| Tenant UI | `BusinessTypesPage` | `app-business-types` | `ONEERP.ERP.UI/src/app/pages/business-types/business-types.ts` | No direct route match |
| Tenant UI | `CategoryPage` | `app-category` | `ONEERP.ERP.UI/src/app/pages/category/category.ts` | `/product-categories` |
| Tenant UI | `CompanyGroupsPage` | `app-company-groups` | `ONEERP.ERP.UI/src/app/pages/company-groups/company-groups.ts` | No direct route match |
| Tenant UI | `CompanyPage` | `app-company` | `ONEERP.ERP.UI/src/app/pages/company/company.ts` | `/company` |
| Tenant UI | `ContactAdministratorPage` | `app-contact-administrator` | `ONEERP.ERP.UI/src/app/pages/contact-administrator/contact-administrator.ts` | `/contact-administrator` |
| Tenant UI | `ContactTypesPage` | `app-contact-types` | `ONEERP.ERP.UI/src/app/pages/contact-types/contact-types.ts` | No direct route match |
| Tenant UI | `CounterAssignmentsPage` | `app-counter-assignments` | `ONEERP.ERP.UI/src/app/pages/adminitration/business-master/counter-assignments/counter-assignments.ts` | No direct route match |
| Tenant UI | `CountersPage` | `app-counters` | `ONEERP.ERP.UI/src/app/pages/adminitration/business-master/counters/counters.ts` | No direct route match |
| Tenant UI | `CouponPage` | `app-coupon` | `ONEERP.ERP.UI/src/app/pages/coupon/coupon.ts` | `/coupons` |
| Tenant UI | `CurrenciesPage` | `app-currencies` | `ONEERP.ERP.UI/src/app/pages/currencies/currencies.ts` | No direct route match |
| Tenant UI | `DashboardPage` | `app-dashboard` | `ONEERP.ERP.UI/src/app/pages/dashboard/dashboard.ts` | `/dashboard`, `/dashboard` |
| Tenant UI | `DataScopesPage` | `app-data-scopes` | `ONEERP.ERP.UI/src/app/pages/data-scopes/data-scopes.ts` | `/data-scopes` |
| Tenant UI | `Department` | `app-department` | `ONEERP.ERP.UI/src/app/pages/adminitration/business-master/department/department.ts` | No direct route match |
| Tenant UI | `Designation` | `app-designation` | `ONEERP.ERP.UI/src/app/pages/adminitration/business-master/designation/designation.ts` | No direct route match |
| Tenant UI | `DiscountRulePage` | `app-discount-rule` | `ONEERP.ERP.UI/src/app/pages/discount-rule/discount-rule.ts` | `/discount-rules` |
| Tenant UI | `DocumentDesignerPage` | `app-document-designer` | `ONEERP.ERP.UI/src/app/pages/document-design/document-designer.ts` | `/document-designer` |
| Tenant UI | `DocumentDesignPage` | `app-document-design` | `ONEERP.ERP.UI/src/app/pages/document-design/document-design.ts` | `/document-design` |
| Tenant UI | `DocumentMasterPageComponent` | `app-document-master-page` | `ONEERP.ERP.UI/src/app/pages/document-design/master/document-master-page/document-master-page.component.ts` | `/invoice-types`, `/document-design/master/types`, `/invoice-paper-sizes`, `/document-design/master/paper-sizes`, `/invoice-template-categories`, `/document-design/master/categories`, `/invoice-template-variables`, `/document-design/master/variables`, `/invoice-fonts`, `/document-design/master/fonts`, `/print-orientations`, `/document-design/master/orientations`, `/document-design/master/units`, `/printer-types`, `/document-design/master/printer-types`, `/printer-models`, `/document-design/master/printer-models` |
| Tenant UI | `DocumentPreviewPage` | `app-document-preview` | `ONEERP.ERP.UI/src/app/pages/document-design/document-preview.ts` | `/document-design/preview` |
| Tenant UI | `DocumentTypesPage` | `app-document-types` | `ONEERP.ERP.UI/src/app/pages/document-types/document-types.ts` | No direct route match |
| Tenant UI | `DomainsPage` | `app-domains` | `ONEERP.ERP.UI/src/app/pages/domains/domains.ts` | `/domains` |
| Tenant UI | `Employee` | `app-employee` | `ONEERP.ERP.UI/src/app/pages/adminitration/business-master/employee/employee.ts` | No direct route match |
| Tenant UI | `EnterprisePermissionsPage` | `app-enterprise-permissions` | `ONEERP.ERP.UI/src/app/pages/enterprise-permissions/enterprise-permissions.ts` | `/enterprise-permissions` |
| Tenant UI | `EntityEditorComponent` | `app-entity-editor` | `ONEERP.ERP.UI/src/app/pages/shared/entity-editor/entity-editor.ts` | No direct route match |
| Tenant UI | `FieldsPage` | `app-fields` | `ONEERP.ERP.UI/src/app/pages/fields/fields.ts` | `/fields` |
| Tenant UI | `FinanceYear` | `app-finance-year` | `ONEERP.ERP.UI/src/app/pages/adminitration/business-master/finance-year/finance-year.ts` | `/finance-year` |
| Tenant UI | `GstPurchaseReport` | `app-gst-purchase-report` | `ONEERP.ERP.UI/src/app/pages/reports/gst-purchase-report/gst-purchase-report.ts` | No direct route match |
| Tenant UI | `GstRegistrationTypesPage` | `app-gst-registration-types` | `ONEERP.ERP.UI/src/app/pages/gst-registration-types/gst-registration-types.ts` | No direct route match |
| Tenant UI | `HsnSacPage` | `app-hsn-sac` | `ONEERP.ERP.UI/src/app/pages/hsn-sac/hsn-sac.ts` | `/hsn-sacs` |
| Tenant UI | `IndustryTypesPage` | `app-industry-types` | `ONEERP.ERP.UI/src/app/pages/industry-types/industry-types.ts` | No direct route match |
| Tenant UI | `InventoryPage` | `app-inventory` | `ONEERP.ERP.UI/src/app/pages/inventory/inventory.ts` | `/inventory` |
| Tenant UI | `InvoiceTemplatePage` | `app-invoice-template` | `ONEERP.ERP.UI/src/app/pages/invoice-template/invoice-template.ts` | `/invoice-templates` |
| Tenant UI | `LanguagesPage` | `app-languages` | `ONEERP.ERP.UI/src/app/pages/languages/languages.ts` | No direct route match |
| Tenant UI | `LocationsPage` | `app-locations` | `ONEERP.ERP.UI/src/app/pages/locations/locations.ts` | No direct route match |
| Tenant UI | `LoginPage` | `app-login` | `ONEERP.ERP.UI/src/app/pages/login/login.ts` | `/login`, `/login` |
| Tenant UI | `MasterImportPage` | `app-master-import` | `ONEERP.ERP.UI/src/app/pages/master-import/master-import.ts` | `/master-import`, `/import-logs` |
| Tenant UI | `MasterPage` | `app-master-page` | `ONEERP.ERP.UI/src/app/pages/shared/master-page/master-page.ts` | No direct route match |
| Tenant UI | `ModulesPage` | `app-modules` | `ONEERP.ERP.UI/src/app/pages/modules/modules.ts` | `/modules` |
| Tenant UI | `MultiSelectComponent` | `app-multi-select` | `ONEERP.ERP.UI/src/app/pages/shared/multi-select/multi-select.ts` | No direct route match |
| Tenant UI | `OfferPage` | `app-offer` | `ONEERP.ERP.UI/src/app/pages/offer/offer.ts` | `/offers` |
| Tenant UI | `OperatorsPage` | `app-operators` | `ONEERP.ERP.UI/src/app/pages/adminitration/business-master/operators/operators.ts` | No direct route match |
| Tenant UI | `OperatorTypesPage` | `app-operator-types` | `ONEERP.ERP.UI/src/app/pages/adminitration/business-master/operator-types/operator-types.ts` | No direct route match |
| Tenant UI | `OrganizationTypesPage` | `app-organization-types` | `ONEERP.ERP.UI/src/app/pages/organization-types/organization-types.ts` | No direct route match |
| Tenant UI | `PaymentEntryPage` | `app-payment-entry` | `ONEERP.ERP.UI/src/app/pages/payment-entry/payment-entry.ts` | `/payment-entry` |
| Tenant UI | `PaymentListPage` | `app-payment-list` | `ONEERP.ERP.UI/src/app/pages/payment-list/payment-list.ts` | No direct route match |
| Tenant UI | `PaymentMethodDetailPage` | `app-payment-method-detail` | `ONEERP.ERP.UI/src/app/pages/payment-method-detail/payment-method-detail.ts` | `/payment-method-detail/:paymentMethodId` |
| Tenant UI | `PaymentMethodPage` | `app-payment-method` | `ONEERP.ERP.UI/src/app/pages/payment-method/payment-method.ts` | `/payment-method` |
| Tenant UI | `PaymentTypePage` | `app-payment-type` | `ONEERP.ERP.UI/src/app/pages/payment-type/payment-type.ts` | `/payment-type` |
| Tenant UI | `PaymentWorkspace` | `app-payment` | `ONEERP.ERP.UI/src/app/pages/payment/payment.ts` | `/payment` |
| Tenant UI | `PosPage` | `app-pos` | `ONEERP.ERP.UI/src/app/pages/pos/pos.ts` | `/pos` |
| Tenant UI | `PosSessionsPage` | `app-pos-sessions` | `ONEERP.ERP.UI/src/app/pages/adminitration/business-master/pos-sessions/pos-sessions.ts` | No direct route match |
| Tenant UI | `PriceListPage` | `app-price-list` | `ONEERP.ERP.UI/src/app/pages/price-list/price-list.ts` | `/price-lists` |
| Tenant UI | `PriceMasterPage` | `app-price-master` | `ONEERP.ERP.UI/src/app/pages/price-master/price-master.ts` | `/price-master` |
| Tenant UI | `PriceTypePage` | `app-price-type` | `ONEERP.ERP.UI/src/app/pages/price-type/price-type.ts` | `/price-types` |
| Tenant UI | `ProductPage` | `app-product` | `ONEERP.ERP.UI/src/app/pages/product/product.ts` | `/products` |
| Tenant UI | `PrsBars` | `prs-bars` | `ONEERP.ERP.UI/src/app/pages/reports/purchase-reports/prs-bars.ts` | No direct route match |
| Tenant UI | `PurchaseCancelPage` | `app-purchase-cancel` | `ONEERP.ERP.UI/src/app/pages/purchase-cancel/purchase-cancel.ts` | `/purchase-cancel/:id` |
| Tenant UI | `PurchaseDeletePage` | `app-purchase-delete` | `ONEERP.ERP.UI/src/app/pages/purchase-delete/purchase-delete.ts` | `/purchase-delete/:id` |
| Tenant UI | `PurchaseEditPage` | `app-purchase-edit` | `ONEERP.ERP.UI/src/app/pages/purchase-edit/purchase-edit.ts` | `/purchase-edit/:id` |
| Tenant UI | `PurchaseEntryPage` | `app-purchase-entry` | `ONEERP.ERP.UI/src/app/pages/purchase-entry/purchase-entry.ts` | `/purchase-entry` |
| Tenant UI | `PurchaseListPage` | `app-purchase-list` | `ONEERP.ERP.UI/src/app/pages/purchase-list/purchase-list.ts` | No direct route match |
| Tenant UI | `PurchaseManagementPage` | `app-purchase-management` | `ONEERP.ERP.UI/src/app/pages/purchase-management/purchase-management.ts` | `/purchases/:id` |
| Tenant UI | `PurchaseOutstandingReport` | `app-purchase-outstanding-report` | `ONEERP.ERP.UI/src/app/pages/reports/purchase-outstanding/purchase-outstanding.ts` | No direct route match |
| Tenant UI | `PurchaseRegisterPage` | `app-purchase-register` | `ONEERP.ERP.UI/src/app/pages/purchase-register/purchase-register.ts` | `/purchase-register` |
| Tenant UI | `PurchaseRegisterReport` | `app-purchase-register-report` | `ONEERP.ERP.UI/src/app/pages/reports/purchase-register/purchase-register.ts` | No direct route match |
| Tenant UI | `PurchaseRegisterSummary` | `app-purchase-register-summary` | `ONEERP.ERP.UI/src/app/pages/purchase-register/purchase-register-summary.ts` | No direct route match |
| Tenant UI | `PurchaseRegisterTable` | `app-purchase-register-table` | `ONEERP.ERP.UI/src/app/pages/purchase-register/purchase-register-table.ts` | No direct route match |
| Tenant UI | `PurchaseRegisterToolbar` | `app-purchase-register-toolbar` | `ONEERP.ERP.UI/src/app/pages/purchase-register/purchase-register-toolbar.ts` | No direct route match |
| Tenant UI | `PurchaseReportScreen` | `app-purchase-report-screen` | `ONEERP.ERP.UI/src/app/pages/reports/purchase-reports/purchase-report-screen.ts` | No direct route match |
| Tenant UI | `PurchaseReportsWorkspace` | `app-purchase-reports-workspace` | `ONEERP.ERP.UI/src/app/pages/reports/purchase-reports/purchase-reports-workspace.ts` | No direct route match |
| Tenant UI | `PurchaseReturnEntryPage` | `app-purchase-return-entry` | `ONEERP.ERP.UI/src/app/pages/purchase-return-entry/purchase-return-entry.ts` | `/purchase-returns/new` |
| Tenant UI | `PurchaseReturnManagementPage` | `app-purchase-return-management` | `ONEERP.ERP.UI/src/app/pages/purchase-return-management/purchase-return-management.ts` | `/purchase-returns/:id` |
| Tenant UI | `PurchaseReturnPage` | `app-purchase-return` | `ONEERP.ERP.UI/src/app/pages/purchase-return/purchase-return.ts` | `/purchase-returns` |
| Tenant UI | `PurchaseViewPage` | `app-purchase-view` | `ONEERP.ERP.UI/src/app/pages/purchase-view/purchase-view.ts` | `/purchase-view/:id` |
| Tenant UI | `PurchaseWorkspace` | `app-purchase` | `ONEERP.ERP.UI/src/app/pages/purchase/purchase.ts` | `/purchase` |
| Tenant UI | `ReportsWorkspace` | `app-reports-workspace` | `ONEERP.ERP.UI/src/app/pages/reports/reports-workspace.ts` | `/reports` |
| Tenant UI | `RoleFieldPermissionsPage` | `app-role-field-permissions` | `ONEERP.ERP.UI/src/app/pages/role-field-permissions/role-field-permissions.ts` | `/role-field-permissions` |
| Tenant UI | `RolePermissionMatrixPage` | `app-role-permission-matrix` | `ONEERP.ERP.UI/src/app/pages/role-permission-matrix/role-permission-matrix.ts` | `/role-permission-matrix` |
| Tenant UI | `RolesPage` | `app-roles` | `ONEERP.ERP.UI/src/app/pages/roles/roles.ts` | `/roles` |
| Tenant UI | `SaleEntryContextPopupComponent` | `app-sale-entry-context-popup` | `ONEERP.ERP.UI/src/app/pages/sales-entry/sale-entry-context-popup.component.ts` | No direct route match |
| Tenant UI | `SalesEntryPage` | `app-sales-entry` | `ONEERP.ERP.UI/src/app/pages/sales-entry/sales-entry.ts` | `/sales-entry` |
| Tenant UI | `SalesListPage` | `app-sales-list` | `ONEERP.ERP.UI/src/app/pages/sales-list/sales-list.ts` | No direct route match |
| Tenant UI | `SalesReportScreen` | `app-sales-report-screen` | `ONEERP.ERP.UI/src/app/pages/reports/sales-reports/sales-report-screen.ts` | No direct route match |
| Tenant UI | `SalesReportsWorkspace` | `app-sales-reports-workspace` | `ONEERP.ERP.UI/src/app/pages/reports/sales-reports/sales-reports-workspace.ts` | No direct route match |
| Tenant UI | `SalesReturnPage` | `app-sales-return` | `ONEERP.ERP.UI/src/app/pages/sales-return/sales-return.ts` | `/sales-return` |
| Tenant UI | `SalesWorkspace` | `app-sales` | `ONEERP.ERP.UI/src/app/pages/sales/sales.ts` | `/sales` |
| Tenant UI | `ScreensPage` | `app-screens` | `ONEERP.ERP.UI/src/app/pages/screens/screens.ts` | `/screens` |
| Tenant UI | `ServiceCategoryPage` | `app-service-category` | `ONEERP.ERP.UI/src/app/pages/service-category/service-category.ts` | `/service-categories` |
| Tenant UI | `ServicePage` | `app-service` | `ONEERP.ERP.UI/src/app/pages/service/service.ts` | `/services` |
| Tenant UI | `SettingsPage` | `app-settings` | `ONEERP.ERP.UI/src/app/pages/settings/settings.ts` | `/settings`, `/settings` |
| Tenant UI | `SourcesPage` | `app-sources` | `ONEERP.ERP.UI/src/app/pages/adminitration/business-master/sources/sources.ts` | No direct route match |
| Tenant UI | `StockLedgerReport` | `app-stock-ledger-report` | `ONEERP.ERP.UI/src/app/pages/reports/stock-ledger/stock-ledger.ts` | No direct route match |
| Tenant UI | `StockPage` | `app-stock` | `ONEERP.ERP.UI/src/app/pages/stock/stock.ts` | `/stock` |
| Tenant UI | `StoresPage` | `app-stores` | `ONEERP.ERP.UI/src/app/pages/adminitration/business-master/stores/stores.ts` | No direct route match |
| Tenant UI | `StoreTypesPage` | `app-store-types` | `ONEERP.ERP.UI/src/app/pages/adminitration/business-master/storetypes/store-types.ts` | No direct route match |
| Tenant UI | `SubCategoryPage` | `app-subcategory` | `ONEERP.ERP.UI/src/app/pages/subcategory/subcategory.ts` | `/product-subcategories` |
| Tenant UI | `SubModulesPage` | `app-submodules` | `ONEERP.ERP.UI/src/app/pages/submodules/submodules.ts` | `/submodules` |
| Tenant UI | `SupplierLedgerReport` | `app-supplier-ledger-report` | `ONEERP.ERP.UI/src/app/pages/reports/supplier-ledger/supplier-ledger.ts` | No direct route match |
| Tenant UI | `SystemMasterPage` | `app-system-master` | `ONEERP.ERP.UI/src/app/pages/system-master/system-master.ts` | `/system-master` |
| Tenant UI | `TaxPage` | `app-tax` | `ONEERP.ERP.UI/src/app/pages/tax/tax.ts` | `/taxes` |
| Tenant UI | `TaxTypeSystemPage` | `app-tax-type-system` | `ONEERP.ERP.UI/src/app/pages/tax-type-system/tax-type-system.ts` | `/tax-type-systems` |
| Tenant UI | `TemplateComponentsMasterComponent` | `app-template-components-master` | `ONEERP.ERP.UI/src/app/pages/document-design/master/template-components/template-components-master.component.ts` | `/document-design/master/template-components` |
| Tenant UI | `TenantConfigurationPage` | `app-tenant-configuration` | `ONEERP.ERP.UI/src/app/pages/tenant-configuration/tenant-configuration.ts` | `/tenant-configuration` |
| Tenant UI | `TimeZonesPage` | `app-time-zones` | `ONEERP.ERP.UI/src/app/pages/time-zones/time-zones.ts` | No direct route match |
| Tenant UI | `UnitConversionPage` | `app-unit-conversion` | `ONEERP.ERP.UI/src/app/pages/unit-conversion/unit-conversion.ts` | `/unit-conversions` |
| Tenant UI | `UnitPage` | `app-unit` | `ONEERP.ERP.UI/src/app/pages/unit/unit.ts` | `/units` |
| Tenant UI | `UserDataScopeOverridesPage` | `app-user-data-scope-overrides` | `ONEERP.ERP.UI/src/app/pages/user-data-scope-overrides/user-data-scope-overrides.ts` | `/user-data-scope-overrides` |
| Tenant UI | `UserPermissionOverridesPage` | `app-user-permission-overrides` | `ONEERP.ERP.UI/src/app/pages/user-permission-overrides/user-permission-overrides.ts` | `/user-permission-overrides` |
| Tenant UI | `UsersPage` | `app-users` | `ONEERP.ERP.UI/src/app/pages/users/users.ts` | `/users` |
| Tenant UI | `Warehouse` | `app-warehouse` | `ONEERP.ERP.UI/src/app/pages/adminitration/business-master/warehouse/warehouse.ts` | No direct route match |
| Tenant UI | `WorkflowPermissionsPage` | `app-workflow-permissions` | `ONEERP.ERP.UI/src/app/pages/workflow-permissions/workflow-permissions.ts` | `/workflow-permissions` |
| Tenant UI | `WorkspacePage` | `app-workspace` | `ONEERP.ERP.UI/src/app/pages/workspace/workspace.ts` | `/workspace/:id` |
| Tenant UI | `WorkspacesPage` | `app-workspaces` | `ONEERP.ERP.UI/src/app/pages/workspaces/workspaces.ts` | `/workspaces` |
| Tenant UI | `WorkspaceTemplate` | `app-workspace-template` | `ONEERP.ERP.UI/src/app/pages/workspace-template/workspace-template.ts` | No direct route match |

## 6. Angular Service Master List

`@Injectable` classes and literal `/api` HTTP calls found in the class body. Dynamic URL builders and indirect calls can be absent from this list.

| App | Service | File | HTTP calls |
|---|---|---|---|
| Tenant UI | `KeyboardShortcutService` | `ONEERP.ERP.UI/src/app/core/keyboard/keyboard-shortcut.service.ts` | None extracted |
| Tenant UI | `PurchaseHubService` | `ONEERP.ERP.UI/src/app/core/purchase-hub.service.ts` | None extracted |
| Tenant UI | `SalesHubService` | `ONEERP.ERP.UI/src/app/core/sales-hub.service.ts` | None extracted |
| Tenant UI | `AuthService` | `ONEERP.ERP.UI/src/app/core/services/auth.service.ts` | POST `/api/auth/login`<br>POST `/api/auth/refresh`<br>POST `/api/auth/logout`<br>GET `/api/permission/user-permissions` |
| Tenant UI | `DocumentDesignService` | `ONEERP.ERP.UI/src/app/core/services/document-design.service.ts` | None extracted |
| Tenant UI | `DocumentPrintService` | `ONEERP.ERP.UI/src/app/core/services/document-print.service.ts` | None extracted |
| Tenant UI | `InventoryService` | `ONEERP.ERP.UI/src/app/core/services/inventory.service.ts` | GET `/api/inventory/dashboard`<br>POST `/api/inventory/opening`<br>GET `/api/inventory/adjustments?page=${page}&size=${size}`<br>POST `/api/inventory/adjustments`<br>POST `/api/inventory/adjustments/${id}/post`<br>GET `/api/inventory/transfers?page=${page}&size=${size}`<br>POST `/api/inventory/transfers`<br>POST `/api/inventory/transfers/${id}/post`<br>GET `/api/inventory/counts?page=${page}&size=${size}`<br>POST `/api/inventory/counts`<br>POST `/api/inventory/counts/${id}/post`<br>GET `/api/inventory/reconciliation`<br>GET `/api/inventory/valuation`<br>GET `/api/inventory/low-stock` |
| Tenant UI | `InvoiceTemplateService` | `ONEERP.ERP.UI/src/app/core/services/invoice-template.service.ts` | None extracted |
| Tenant UI | `AdministrationService` | `ONEERP.ERP.UI/src/app/core/services/master_service.ts` | None extracted |
| Tenant UI | `BillingMasterService` | `ONEERP.ERP.UI/src/app/core/services/master_service.ts` | None extracted |
| Tenant UI | `MasterImportService` | `ONEERP.ERP.UI/src/app/core/services/master_service.ts` | GET `/api/import/masters`<br>GET `/api/import/${encodeURIComponent(entityName)}/template`<br>POST `/api/import/preview`<br>POST `/api/import/confirm`<br>GET `/api/import/${name}/export/meta`<br>POST `/api/import/${name}/export/options`<br>POST `/api/import/${name}/export/preview`<br>POST `/api/import/${name}/export` |
| Tenant UI | `ImportLogService` | `ONEERP.ERP.UI/src/app/core/services/master_service.ts` | GET `/api/import-logs` |
| Tenant UI | `TenantConfigurationService` | `ONEERP.ERP.UI/src/app/core/services/master_service.ts` | GET `/api/tenant-configuration`<br>GET `/api/tenant-configuration/grouped`<br>GET `/api/tenant-configuration/${id}`<br>POST `/api/tenant-configuration`<br>PUT `/api/tenant-configuration/${id}`<br>DELETE `/api/tenant-configuration/${id}` |
| Tenant UI | `PurchaseService` | `ONEERP.ERP.UI/src/app/core/services/master_service.ts` | GET `/api/purchases`<br>GET `/api/purchases/lookups`<br>GET `/api/purchases/next-number`<br>GET `/api/purchases/${id}`<br>GET `/api/purchases/${id}/items`<br>GET `/api/purchases/${id}/payments`<br>GET `/api/purchases/${id}/stock`<br>POST `/api/purchases`<br>PUT `/api/purchases/${id}`<br>POST `/api/purchases/${id}/cancel`<br>GET `/api/purchases/${id}/delete-check`<br>DELETE `/api/purchases/${id}` |
| Tenant UI | `StockService` | `ONEERP.ERP.UI/src/app/core/services/master_service.ts` | GET `/api/stock`<br>GET `/api/stock/transactions`<br>GET `/api/stock/price-master-products` |
| Tenant UI | `PurchaseReturnService` | `ONEERP.ERP.UI/src/app/core/services/master_service.ts` | GET `/api/purchase-returns`<br>GET `/api/purchase-returns/next-number`<br>GET `/api/purchase-returns/${id}`<br>POST `/api/purchase-returns`<br>PUT `/api/purchase-returns/${id}`<br>POST `/api/purchase-returns/${id}/cancel`<br>GET `/api/purchase-returns/${id}/delete-check`<br>DELETE `/api/purchase-returns/${id}` |
| Tenant UI | `PaymentTypeService` | `ONEERP.ERP.UI/src/app/core/services/master_service.ts` | GET `/api/payment-types`<br>GET `/api/payment-types/${id}`<br>POST `/api/payment-types`<br>PUT `/api/payment-types/${id}`<br>DELETE `/api/payment-types/${id}` |
| Tenant UI | `PaymentMethodService` | `ONEERP.ERP.UI/src/app/core/services/master_service.ts` | GET `/api/payment-methods`<br>GET `/api/payment-methods/${id}`<br>POST `/api/payment-methods`<br>PUT `/api/payment-methods/${id}`<br>DELETE `/api/payment-methods/${id}` |
| Tenant UI | `PaymentMethodDetailService` | `ONEERP.ERP.UI/src/app/core/services/master_service.ts` | GET `/api/payment-method-details`<br>GET `/api/payment-method-details/${id}`<br>POST `/api/payment-method-details`<br>PUT `/api/payment-method-details/${id}`<br>DELETE `/api/payment-method-details/${id}` |
| Tenant UI | `PaymentService` | `ONEERP.ERP.UI/src/app/core/services/master_service.ts` | GET `/api/payments`<br>GET `/api/payments/lookups`<br>GET `/api/payments/next-number`<br>GET `/api/payments/${id}`<br>POST `/api/payments`<br>PUT `/api/payments/${id}`<br>DELETE `/api/payments/${id}` |
| Tenant UI | `NavigationStoreService` | `ONEERP.ERP.UI/src/app/core/services/navigation-store.service.ts` | GET `/api/permissions/my-navigation` |
| Tenant UI | `OrganizationService` | `ONEERP.ERP.UI/src/app/core/services/organization_service.ts` | None extracted |
| Tenant UI | `PermissionService` | `ONEERP.ERP.UI/src/app/core/services/permission.service.ts` | GET `/api/data-scopes/my`<br>GET `/api/roles/my`<br>GET `/api/data-scopes/role/${roleId}`<br>GET `/api/user-data-scope-overrides/user/${userId}` |
| Tenant UI | `PosService` | `ONEERP.ERP.UI/src/app/core/services/pos_service.ts` | GET `/api/counter-assignments/by-counter/${counterId}`<br>GET `/api/pos/dashboard`<br>GET `/api/pos/sessions/current`<br>POST `/api/pos/sessions/${id}/close`<br>POST `/api/pos/cash-in`<br>POST `/api/pos/cash-out`<br>POST `/api/pos/holds`<br>GET `/api/pos/holds`<br>POST `/api/pos/holds/${id}/recall`<br>POST `/api/pos/holds/${id}/cancel`<br>GET `/api/pos/sessions/${sessionId}/summary` |
| Tenant UI | `PurchaseReportUiStore` | `ONEERP.ERP.UI/src/app/core/services/purchase-report-ui.store.ts` | None extracted |
| Tenant UI | `PurchaseReportService` | `ONEERP.ERP.UI/src/app/core/services/purchase-report.service.ts` | GET `/api/purchase-reports` |
| Tenant UI | `SaleEntryContextService` | `ONEERP.ERP.UI/src/app/core/services/sale-entry-context.service.ts` | None extracted |
| Tenant UI | `SalesReportUiStore` | `ONEERP.ERP.UI/src/app/core/services/sales-report-ui.store.ts` | None extracted |
| Tenant UI | `SalesReportService` | `ONEERP.ERP.UI/src/app/core/services/sales-report.service.ts` | GET `/api/sales-reports` |
| Tenant UI | `SalesReturnService` | `ONEERP.ERP.UI/src/app/core/services/sales-return.service.ts` | GET `/api/sales-returns`<br>GET `/api/sales-returns/${id}`<br>GET `/api/sales-returns/next-number`<br>POST `/api/sales-returns`<br>POST `/api/sales-returns/${id}/cancel` |
| Tenant UI | `SalesService` | `ONEERP.ERP.UI/src/app/core/services/sales.service.ts` | GET `/api/sales`<br>GET `/api/sales/lookups`<br>GET `/api/sales/next-number`<br>GET `/api/sales/${id}`<br>GET `/api/sales/${id}/items`<br>GET `/api/sales/${id}/payments`<br>GET `/api/sales/${id}/stock`<br>GET `/api/sales/products/${productId}/stock`<br>POST `/api/sales`<br>PUT `/api/sales/${id}`<br>DELETE `/api/sales/${id}` |
| Tenant UI | `ThemeService` | `ONEERP.ERP.UI/src/app/core/services/theme.service.ts` | None extracted |
| Tenant UI | `ToastService` | `ONEERP.ERP.UI/src/app/core/services/toast.service.ts` | None extracted |
| Platform UI | `AuthService` | `ONEERP.Platform.UI/src/app/core/auth.service.ts` | POST `/api/auth/login`<br>POST `/api/auth/refresh`<br>POST `/api/auth/logout` |
| Platform UI | `MigrationService` | `ONEERP.Platform.UI/src/app/core/migration.service.ts` | GET `/api/tenants/${tenantId}/migration-status`<br>POST `/api/tenants/${tenantId}/migration/run`<br>GET `/api/tenants/${tenantId}/migration/history?take=${take}` |
| Platform UI | `PermissionService` | `ONEERP.Platform.UI/src/app/core/permission.service.ts` | None extracted |
| Platform UI | `ThemeService` | `ONEERP.Platform.UI/src/app/core/theme.service.ts` | None extracted |
| Platform UI | `ToastService` | `ONEERP.Platform.UI/src/app/core/toast.service.ts` | None extracted |

## 7. API Master List

Routes are parsed from controller `[Route]` and action `[Http*]` attributes. Permission column reports the nearest parsed `[Permission(...)]` identifier. `None` means no explicit action permission was found; global filter source says such actions require authentication only. The list does not prove runtime availability.

| # | App | Method | Route | Controller.Action | Permission | Class auth | Source |
|---:|---|---|---|---|---|---|---|
| 1 | Tenant | GET | `/` | `CompaniesController.GetPaged` | Permissions.CompaniesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/CompaniesController.cs` |
| 2 | Tenant | GET | `/` | `DashboardController.Get` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/DashboardController.cs` |
| 3 | Tenant | GET | `/` | `RolesController.GetAll` | Permissions.RolesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/RolesController.cs` |
| 4 | Tenant | GET | `/` | `SettingsController.GetAll` | Permissions.SettingsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/SettingsController.cs` |
| 5 | Tenant | GET | `/` | `UsersController.GetPaged` | Permissions.UsersView | [Authorize] on class | `ONEERP.ERP.API/Controllers/UsersController.cs` |
| 6 | Tenant | POST | `/` | `CompaniesController.Create` | Permissions.CompaniesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/CompaniesController.cs` |
| 7 | Tenant | POST | `/` | `EntitiesController.Create` | Permissions.EntitiesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/EntitiesController.cs` |
| 8 | Tenant | POST | `/` | `RolesController.Create` | Permissions.RolesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/RolesController.cs` |
| 9 | Tenant | POST | `/` | `UsersController.Create` | Permissions.UsersCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/UsersController.cs` |
| 10 | Tenant | PUT | `/` | `SettingsController.Update` | Permissions.SettingsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/SettingsController.cs` |
| 11 | Tenant | DELETE | `/{id:int}` | `CompaniesController.Delete` | Permissions.CompaniesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/CompaniesController.cs` |
| 12 | Tenant | DELETE | `/{id:int}` | `RolesController.Delete` | Permissions.RolesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/RolesController.cs` |
| 13 | Tenant | DELETE | `/{id:int}` | `UsersController.Delete` | Permissions.UsersDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/UsersController.cs` |
| 14 | Tenant | GET | `/{id:int}` | `CompaniesController.GetById` | Permissions.CompaniesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/CompaniesController.cs` |
| 15 | Tenant | GET | `/{id:int}` | `RolesController.GetById` | Permissions.RolesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/RolesController.cs` |
| 16 | Tenant | GET | `/{id:int}` | `UsersController.GetById` | Permissions.UsersView | [Authorize] on class | `ONEERP.ERP.API/Controllers/UsersController.cs` |
| 17 | Tenant | PUT | `/{id:int}` | `CompaniesController.Update` | Permissions.CompaniesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/CompaniesController.cs` |
| 18 | Tenant | PUT | `/{id:int}` | `RolesController.Update` | Permissions.RolesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/RolesController.cs` |
| 19 | Tenant | PUT | `/{id:int}` | `UsersController.Update` | Permissions.UsersEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/UsersController.cs` |
| 20 | Tenant | PUT | `/{id:int}/password` | `UsersController.ResetPassword` | Permissions.UsersEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/UsersController.cs` |
| 21 | Tenant | PUT | `/{id:int}/permissions` | `RolesController.SetPermissions` | Permissions.RolesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/RolesController.cs` |
| 22 | Tenant | GET | `/{id:long}` | `EntitiesController.GetById` | Permissions.EntitiesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/EntitiesController.cs` |
| 23 | Tenant | PUT | `/{id:long}/addresses` | `EntitiesController.ReplaceAddresses` | Permissions.EntitiesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/EntitiesController.cs` |
| 24 | Tenant | PUT | `/{id:long}/contacts` | `EntitiesController.ReplaceContacts` | Permissions.EntitiesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/EntitiesController.cs` |
| 25 | Tenant | PUT | `/{id:long}/files` | `EntitiesController.ReplaceFiles` | Permissions.EntitiesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/EntitiesController.cs` |
| 26 | Tenant | PUT | `/{id:long}/notes` | `EntitiesController.ReplaceNotes` | Permissions.EntitiesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/EntitiesController.cs` |
| 27 | Tenant | PUT | `/{id:long}/tags` | `EntitiesController.ReplaceTags` | Permissions.EntitiesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/EntitiesController.cs` |
| 28 | Tenant | GET | `/api/[controller]` | `WorkspacesController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 29 | Tenant | GET | `/api/[controller]` | `DomainsController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 30 | Tenant | GET | `/api/[controller]` | `ModulesController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 31 | Tenant | GET | `/api/[controller]` | `SubModulesController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 32 | Tenant | GET | `/api/[controller]` | `ScreensController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 33 | Tenant | GET | `/api/[controller]` | `FieldsController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 34 | Tenant | GET | `/api/[controller]` | `NavigationController.Get` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 35 | Tenant | GET | `/api/[controller]` | `PermissionModulesController.GetAll` | "permission-modules.view" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 36 | Tenant | GET | `/api/[controller]` | `PermissionActionsController.GetAll` | "permission-actions.view" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 37 | Tenant | POST | `/api/[controller]` | `WorkspacesController.Create` | "workspaces.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 38 | Tenant | POST | `/api/[controller]` | `DomainsController.Create` | "domains.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 39 | Tenant | POST | `/api/[controller]` | `ModulesController.Create` | "modules.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 40 | Tenant | POST | `/api/[controller]` | `SubModulesController.Create` | "submodules.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 41 | Tenant | POST | `/api/[controller]` | `ScreensController.Create` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 42 | Tenant | POST | `/api/[controller]` | `FieldsController.Create` | "fields.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 43 | Tenant | POST | `/api/[controller]` | `PermissionModulesController.Create` | "permission-modules.manage" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 44 | Tenant | POST | `/api/[controller]` | `PermissionActionsController.Create` | "permission-actions.manage" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 45 | Tenant | POST | `/api/[controller]` | `FieldPermissionsController.Set` | "roles.manage" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 46 | Tenant | DELETE | `/api/[controller]/{id:int}` | `WorkspacesController.Delete` | "workspaces.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 47 | Tenant | DELETE | `/api/[controller]/{id:int}` | `DomainsController.Delete` | "domains.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 48 | Tenant | DELETE | `/api/[controller]/{id:int}` | `ModulesController.Delete` | "modules.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 49 | Tenant | DELETE | `/api/[controller]/{id:int}` | `SubModulesController.Delete` | "submodules.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 50 | Tenant | DELETE | `/api/[controller]/{id:int}` | `ScreensController.Delete` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 51 | Tenant | DELETE | `/api/[controller]/{id:int}` | `FieldsController.Delete` | "fields.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 52 | Tenant | DELETE | `/api/[controller]/{id:int}` | `PermissionModulesController.Delete` | "permission-modules.manage" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 53 | Tenant | DELETE | `/api/[controller]/{id:int}` | `PermissionActionsController.Delete` | "permission-actions.manage" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 54 | Tenant | DELETE | `/api/[controller]/{id:int}` | `FieldPermissionsController.Delete` | "roles.manage" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 55 | Tenant | GET | `/api/[controller]/{id:int}` | `WorkspacesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 56 | Tenant | GET | `/api/[controller]/{id:int}` | `DomainsController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 57 | Tenant | GET | `/api/[controller]/{id:int}` | `ModulesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 58 | Tenant | GET | `/api/[controller]/{id:int}` | `SubModulesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 59 | Tenant | GET | `/api/[controller]/{id:int}` | `ScreensController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 60 | Tenant | GET | `/api/[controller]/{id:int}` | `FieldsController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 61 | Tenant | GET | `/api/[controller]/{id:int}` | `PermissionModulesController.GetById` | "permission-modules.view" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 62 | Tenant | GET | `/api/[controller]/{id:int}` | `PermissionActionsController.GetById` | "permission-actions.view" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 63 | Tenant | PUT | `/api/[controller]/{id:int}` | `WorkspacesController.Update` | "workspaces.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 64 | Tenant | PUT | `/api/[controller]/{id:int}` | `DomainsController.Update` | "domains.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 65 | Tenant | PUT | `/api/[controller]/{id:int}` | `ModulesController.Update` | "modules.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 66 | Tenant | PUT | `/api/[controller]/{id:int}` | `SubModulesController.Update` | "submodules.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 67 | Tenant | PUT | `/api/[controller]/{id:int}` | `ScreensController.Update` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 68 | Tenant | PUT | `/api/[controller]/{id:int}` | `FieldsController.Update` | "fields.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 69 | Tenant | PUT | `/api/[controller]/{id:int}` | `PermissionModulesController.Update` | "permission-modules.manage" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 70 | Tenant | PUT | `/api/[controller]/{id:int}` | `PermissionActionsController.Update` | "permission-actions.manage" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 71 | Tenant | POST | `/api/[controller]/assign` | `ModulePermissionsController.Assign` | "roles.manage" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 72 | Tenant | GET | `/api/[controller]/code/{code}` | `PermissionModulesController.GetByCode` | "permission-modules.view" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 73 | Tenant | GET | `/api/[controller]/domain/{domainId:int}` | `ModulesController.GetByDomain` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 74 | Tenant | GET | `/api/[controller]/module/{moduleId:int}` | `SubModulesController.GetByModule` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 75 | Tenant | GET | `/api/[controller]/module/{moduleId:int}` | `ModulePermissionsController.GetByModule` | "permission-modules.view" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 76 | Tenant | POST | `/api/[controller]/revoke` | `ModulePermissionsController.Revoke` | "roles.manage" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 77 | Tenant | GET | `/api/[controller]/role/{roleId:int}` | `ModulePermissionsController.GetByRole` | "roles.view" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 78 | Tenant | DELETE | `/api/[controller]/role/{roleId:int}/all` | `ModulePermissionsController.RevokeAllForRole` | "roles.manage" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 79 | Tenant | GET | `/api/[controller]/role/{roleId:int}/module/{moduleId:int}` | `FieldPermissionsController.GetByRoleModule` | "roles.view" | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 80 | Tenant | GET | `/api/[controller]/screen/{screenId:int}` | `FieldsController.GetByScreen` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 81 | Tenant | GET | `/api/[controller]/submodule/{subModuleId:int}` | `ScreensController.GetBySubModule` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 82 | Tenant | GET | `/api/[controller]/workspace/{workspaceId:int}` | `DomainsController.GetByWorkspace` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 83 | Tenant | GET | `/api/actions` | `ActionsController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 84 | Tenant | POST | `/api/actions` | `ActionsController.Create` | "actions.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 85 | Tenant | DELETE | `/api/actions/{id:int}` | `ActionsController.Delete` | "actions.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 86 | Tenant | GET | `/api/actions/{id:int}` | `ActionsController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 87 | Tenant | PUT | `/api/actions/{id:int}` | `ActionsController.Update` | "actions.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 88 | Tenant | GET | `/api/address-types` | `AddressTypesController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/AddressTypesController.cs` |
| 89 | Tenant | POST | `/api/address-types` | `AddressTypesController.Create` | Permissions.AddressTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/AddressTypesController.cs` |
| 90 | Tenant | DELETE | `/api/address-types/{id:int}` | `AddressTypesController.Delete` | Permissions.AddressTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/AddressTypesController.cs` |
| 91 | Tenant | GET | `/api/address-types/{id:int}` | `AddressTypesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/AddressTypesController.cs` |
| 92 | Tenant | PUT | `/api/address-types/{id:int}` | `AddressTypesController.Update` | Permissions.AddressTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/AddressTypesController.cs` |
| 93 | Tenant | GET | `/api/Administration/currencies` | `AdministrationController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/AdministrationController.cs` |
| 94 | Tenant | POST | `/api/Administration/currency` | `AdministrationController.Create` | Permissions.CurrenciesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/AdministrationController.cs` |
| 95 | Tenant | DELETE | `/api/Administration/currency/{id:int}` | `AdministrationController.Delete` | Permissions.CurrenciesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/AdministrationController.cs` |
| 96 | Tenant | GET | `/api/Administration/currency/{id:int}` | `AdministrationController.GetCurrency` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/AdministrationController.cs` |
| 97 | Tenant | PUT | `/api/Administration/currency/{id:int}` | `AdministrationController.Update` | Permissions.CurrenciesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/AdministrationController.cs` |
| 98 | Tenant | POST | `/api/auth/login` | `AuthController.Login` | None (authentication only per global filter) | Not found on class | `ONEERP.ERP.API/Controllers/AuthController.cs` |
| 99 | Tenant | POST | `/api/auth/logout` | `AuthController.Logout` | None (authentication only per global filter) | Not found on class | `ONEERP.ERP.API/Controllers/AuthController.cs` |
| 100 | Tenant | POST | `/api/auth/refresh` | `AuthController.Refresh` | None (authentication only per global filter) | Not found on class | `ONEERP.ERP.API/Controllers/AuthController.cs` |
| 101 | Tenant | GET | `/api/barcodes` | `BarcodesController.GetPaged` | Permissions.BarcodesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 102 | Tenant | POST | `/api/barcodes` | `BarcodesController.Create` | Permissions.BarcodesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 103 | Tenant | DELETE | `/api/barcodes/{id:long}` | `BarcodesController.Delete` | Permissions.BarcodesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 104 | Tenant | GET | `/api/barcodes/{id:long}` | `BarcodesController.GetById` | Permissions.BarcodesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 105 | Tenant | PUT | `/api/barcodes/{id:long}` | `BarcodesController.Update` | Permissions.BarcodesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 106 | Tenant | GET | `/api/brands` | `ProductBrandsController.GetPaged` | Permissions.BrandsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 107 | Tenant | POST | `/api/brands` | `ProductBrandsController.Create` | Permissions.BrandsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 108 | Tenant | DELETE | `/api/brands/{id:int}` | `ProductBrandsController.Delete` | Permissions.BrandsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 109 | Tenant | GET | `/api/brands/{id:int}` | `ProductBrandsController.GetById` | Permissions.BrandsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 110 | Tenant | PUT | `/api/brands/{id:int}` | `ProductBrandsController.Update` | Permissions.BrandsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 111 | Tenant | GET | `/api/brands/next-code` | `ProductBrandsController.GetNextCode` | Permissions.BrandsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 112 | Tenant | GET | `/api/business-partner-roles` | `BusinessPartnerRolesController.GetAll` | Permissions.BusinessPartnerRolesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 113 | Tenant | POST | `/api/business-partner-roles` | `BusinessPartnerRolesController.Create` | Permissions.BusinessPartnerRolesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 114 | Tenant | DELETE | `/api/business-partner-roles/{id:int}` | `BusinessPartnerRolesController.Delete` | Permissions.BusinessPartnerRolesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 115 | Tenant | GET | `/api/business-partner-roles/{id:int}` | `BusinessPartnerRolesController.GetById` | Permissions.BusinessPartnerRolesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 116 | Tenant | PUT | `/api/business-partner-roles/{id:int}` | `BusinessPartnerRolesController.Update` | Permissions.BusinessPartnerRolesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 117 | Tenant | GET | `/api/business-partners` | `BusinessPartnersController.GetAll` | Permissions.BusinessPartnersView | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 118 | Tenant | POST | `/api/business-partners` | `BusinessPartnersController.Create` | Permissions.BusinessPartnersManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 119 | Tenant | DELETE | `/api/business-partners/{id:long}` | `BusinessPartnersController.Delete` | Permissions.BusinessPartnersManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 120 | Tenant | GET | `/api/business-partners/{id:long}` | `BusinessPartnersController.GetById` | Permissions.BusinessPartnersView | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 121 | Tenant | PUT | `/api/business-partners/{id:long}` | `BusinessPartnersController.Update` | Permissions.BusinessPartnersManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 122 | Tenant | GET | `/api/business-partners/next-code` | `BusinessPartnersController.GetNextCode` | Permissions.BusinessPartnersManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 123 | Tenant | GET | `/api/business-types` | `BusinessTypesController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 124 | Tenant | POST | `/api/business-types` | `BusinessTypesController.Create` | Permissions.BusinessTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 125 | Tenant | DELETE | `/api/business-types/{id:int}` | `BusinessTypesController.Delete` | Permissions.BusinessTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 126 | Tenant | GET | `/api/business-types/{id:int}` | `BusinessTypesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 127 | Tenant | PUT | `/api/business-types/{id:int}` | `BusinessTypesController.Update` | Permissions.BusinessTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 128 | Tenant | GET | `/api/cities` | `CitiesController.GetAll` | Permissions.LocationsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 129 | Tenant | POST | `/api/cities` | `CitiesController.Create` | Permissions.LocationsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 130 | Tenant | DELETE | `/api/cities/{id:int}` | `CitiesController.Delete` | Permissions.LocationsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 131 | Tenant | GET | `/api/cities/{id:int}` | `CitiesController.GetById` | Permissions.LocationsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 132 | Tenant | PUT | `/api/cities/{id:int}` | `CitiesController.Update` | Permissions.LocationsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 133 | Tenant | GET | `/api/company-groups` | `CompanyGroupsController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 134 | Tenant | POST | `/api/company-groups` | `CompanyGroupsController.Create` | Permissions.CompanyGroupsManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 135 | Tenant | DELETE | `/api/company-groups/{id:int}` | `CompanyGroupsController.Delete` | Permissions.CompanyGroupsManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 136 | Tenant | GET | `/api/company-groups/{id:int}` | `CompanyGroupsController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 137 | Tenant | PUT | `/api/company-groups/{id:int}` | `CompanyGroupsController.Update` | Permissions.CompanyGroupsManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 138 | Tenant | GET | `/api/contact-types` | `ContactTypesController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/ContactTypesController.cs` |
| 139 | Tenant | POST | `/api/contact-types` | `ContactTypesController.Create` | Permissions.ContactTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/ContactTypesController.cs` |
| 140 | Tenant | DELETE | `/api/contact-types/{id:int}` | `ContactTypesController.Delete` | Permissions.ContactTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/ContactTypesController.cs` |
| 141 | Tenant | GET | `/api/contact-types/{id:int}` | `ContactTypesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/ContactTypesController.cs` |
| 142 | Tenant | PUT | `/api/contact-types/{id:int}` | `ContactTypesController.Update` | Permissions.ContactTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/ContactTypesController.cs` |
| 143 | Tenant | GET | `/api/counter-assignments` | `CounterAssignmentsController.GetAssignments` | Permissions.CounterAssignmentsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 144 | Tenant | POST | `/api/counter-assignments` | `CounterAssignmentsController.CreateAssignment` | Permissions.CounterAssignmentsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 145 | Tenant | DELETE | `/api/counter-assignments/{id:int}` | `CounterAssignmentsController.DeleteAssignment` | Permissions.CounterAssignmentsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 146 | Tenant | GET | `/api/counter-assignments/{id:int}` | `CounterAssignmentsController.GetAssignment` | Permissions.CounterAssignmentsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 147 | Tenant | PUT | `/api/counter-assignments/{id:int}` | `CounterAssignmentsController.UpdateAssignment` | Permissions.CounterAssignmentsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 148 | Tenant | GET | `/api/counter-assignments/by-counter/{counterId:int}` | `CounterAssignmentsController.GetOperatorsByCounter` | Permissions.CounterAssignmentsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 149 | Tenant | GET | `/api/counter-assignments/my-assignments` | `CounterAssignmentsController.GetMyAssignments` | Permissions.CounterAssignmentsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 150 | Tenant | GET | `/api/counter-assignments/my-default-counter` | `CounterAssignmentsController.GetMyDefaultCounter` | Permissions.CounterAssignmentsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 151 | Tenant | GET | `/api/counters` | `CountersController.GetCounters` | Permissions.CountersView | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 152 | Tenant | POST | `/api/counters` | `CountersController.CreateCounter` | Permissions.CountersCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 153 | Tenant | DELETE | `/api/counters/{id:int}` | `CountersController.DeleteCounter` | Permissions.CountersDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 154 | Tenant | GET | `/api/counters/{id:int}` | `CountersController.GetCounter` | Permissions.CountersView | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 155 | Tenant | PUT | `/api/counters/{id:int}` | `CountersController.UpdateCounter` | Permissions.CountersEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 156 | Tenant | GET | `/api/counters/all` | `CountersController.GetAllCounters` | Permissions.CountersView | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 157 | Tenant | GET | `/api/counters/by-store/{storeId:int}` | `CountersController.GetCountersByStore` | Permissions.CountersView | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 158 | Tenant | GET | `/api/counters/next-code` | `CountersController.GetNextCounterCode` | Permissions.CountersView | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 159 | Tenant | GET | `/api/countries` | `CountriesController.GetAll` | Permissions.LocationsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 160 | Tenant | POST | `/api/countries` | `CountriesController.Create` | Permissions.LocationsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 161 | Tenant | DELETE | `/api/countries/{id:int}` | `CountriesController.Delete` | Permissions.LocationsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 162 | Tenant | GET | `/api/countries/{id:int}` | `CountriesController.GetById` | Permissions.LocationsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 163 | Tenant | PUT | `/api/countries/{id:int}` | `CountriesController.Update` | Permissions.LocationsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 164 | Tenant | GET | `/api/coupons` | `CouponsController.GetPaged` | Permissions.CouponsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 165 | Tenant | POST | `/api/coupons` | `CouponsController.Create` | Permissions.CouponsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 166 | Tenant | DELETE | `/api/coupons/{id:long}` | `CouponsController.Delete` | Permissions.CouponsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 167 | Tenant | GET | `/api/coupons/{id:long}` | `CouponsController.GetById` | Permissions.CouponsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 168 | Tenant | PUT | `/api/coupons/{id:long}` | `CouponsController.Update` | Permissions.CouponsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 169 | Tenant | GET | `/api/coupons/next-code` | `CouponsController.GetNextCode` | Permissions.CouponsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 170 | Tenant | POST | `/api/data-scopes` | `DataScopesController.Create` | "data-scopes.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 171 | Tenant | DELETE | `/api/data-scopes/{id:int}` | `DataScopesController.Delete` | "data-scopes.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 172 | Tenant | GET | `/api/data-scopes/{id:int}` | `DataScopesController.GetById` | "data-scopes.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 173 | Tenant | PUT | `/api/data-scopes/{id:int}` | `DataScopesController.Update` | "data-scopes.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 174 | Tenant | GET | `/api/data-scopes/my` | `DataScopesController.GetMy` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 175 | Tenant | GET | `/api/data-scopes/role/{roleId:int}` | `DataScopesController.GetByRole` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 176 | Tenant | POST | `/api/data-scopes/role/{roleId:int}/replace` | `DataScopesController.Replace` | "data-scopes.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 177 | Tenant | GET | `/api/data-scopes/role/{roleId:int}/selection` | `DataScopesController.GetSelection` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 178 | Tenant | GET | `/api/discount-rules` | `DiscountRulesController.GetPaged` | Permissions.DiscountRulesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 179 | Tenant | POST | `/api/discount-rules` | `DiscountRulesController.Create` | Permissions.DiscountRulesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 180 | Tenant | DELETE | `/api/discount-rules/{id:long}` | `DiscountRulesController.Delete` | Permissions.DiscountRulesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 181 | Tenant | GET | `/api/discount-rules/{id:long}` | `DiscountRulesController.GetById` | Permissions.DiscountRulesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 182 | Tenant | PUT | `/api/discount-rules/{id:long}` | `DiscountRulesController.Update` | Permissions.DiscountRulesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 183 | Tenant | GET | `/api/discount-rules/next-code` | `DiscountRulesController.GetNextCode` | Permissions.DiscountRulesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 184 | Tenant | GET | `/api/document-lookups/categories` | `DocumentLookupsController.Categories` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 185 | Tenant | GET | `/api/document-lookups/components` | `DocumentLookupsController.Components` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 186 | Tenant | GET | `/api/document-lookups/fonts` | `DocumentLookupsController.Fonts` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 187 | Tenant | GET | `/api/document-lookups/invoice-types` | `DocumentLookupsController.InvoiceTypes` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 188 | Tenant | GET | `/api/document-lookups/orientations` | `DocumentLookupsController.Orientations` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 189 | Tenant | GET | `/api/document-lookups/paper-sizes` | `DocumentLookupsController.PaperSizes` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 190 | Tenant | GET | `/api/document-lookups/printer-models` | `DocumentLookupsController.PrinterModels` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 191 | Tenant | GET | `/api/document-lookups/printer-types` | `DocumentLookupsController.PrinterTypes` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 192 | Tenant | GET | `/api/document-lookups/units` | `DocumentLookupsController.Units` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 193 | Tenant | GET | `/api/document-lookups/variables` | `DocumentLookupsController.Variables` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 194 | Tenant | GET | `/api/document-settings/assignments` | `DocumentSettingsController.GetAssignments` | Permissions.InvoiceTemplatesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 195 | Tenant | POST | `/api/document-settings/assignments` | `DocumentSettingsController.CreateAssignment` | Permissions.InvoiceTemplatesAssign | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 196 | Tenant | DELETE | `/api/document-settings/assignments/{assignmentId:long}` | `DocumentSettingsController.DeleteAssignment` | Permissions.InvoiceTemplatesAssign | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 197 | Tenant | GET | `/api/document-settings/resolve` | `DocumentSettingsController.Resolve` | Permissions.InvoiceTemplatesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 198 | Tenant | GET | `/api/document-templates` | `DocumentTemplatesController.GetPaged` | Permissions.InvoiceTemplatesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 199 | Tenant | POST | `/api/document-templates` | `DocumentTemplatesController.Create` | Permissions.InvoiceTemplatesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 200 | Tenant | DELETE | `/api/document-templates/{id:long}` | `DocumentTemplatesController.Delete` | Permissions.InvoiceTemplatesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 201 | Tenant | GET | `/api/document-templates/{id:long}` | `DocumentTemplatesController.GetById` | Permissions.InvoiceTemplatesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 202 | Tenant | PUT | `/api/document-templates/{id:long}` | `DocumentTemplatesController.Update` | Permissions.InvoiceTemplatesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 203 | Tenant | POST | `/api/document-templates/{id:long}/set-default` | `DocumentTemplatesController.SetDefault` | Permissions.InvoiceTemplatesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 204 | Tenant | GET | `/api/document-templates/{id:long}/versions` | `DocumentTemplatesController.GetVersions` | Permissions.InvoiceTemplatesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 205 | Tenant | POST | `/api/document-templates/{id:long}/versions` | `DocumentTemplatesController.CreateVersion` | Permissions.InvoiceTemplatesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 206 | Tenant | GET | `/api/document-templates/{templateId:long}/assignments` | `DocumentTemplatesController.GetAssignments` | Permissions.InvoiceTemplatesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 207 | Tenant | POST | `/api/document-templates/{templateId:long}/assignments` | `DocumentTemplatesController.CreateAssignment` | Permissions.InvoiceTemplatesAssign | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 208 | Tenant | DELETE | `/api/document-templates/assignments/{assignmentId:long}` | `DocumentTemplatesController.DeleteAssignment` | Permissions.InvoiceTemplatesAssign | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 209 | Tenant | GET | `/api/document-templates/by-code/{code}` | `DocumentTemplatesController.GetByCode` | Permissions.InvoiceTemplatesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 210 | Tenant | GET | `/api/document-templates/resolve` | `DocumentTemplatesController.Resolve` | Permissions.InvoiceTemplatesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 211 | Tenant | GET | `/api/document-templates/templates/{templateId:long}/printers` | `DocumentTemplatesController.GetPrinters` | Permissions.InvoiceTemplatesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 212 | Tenant | POST | `/api/document-templates/templates/{templateId:long}/printers` | `DocumentTemplatesController.SavePrinter` | Permissions.InvoiceTemplatesAssign | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 213 | Tenant | DELETE | `/api/document-templates/templates/{templateId:long}/printers/{printerId:long}` | `DocumentTemplatesController.DeletePrinter` | Permissions.InvoiceTemplatesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 214 | Tenant | PUT | `/api/document-templates/templates/{templateId:long}/printers/{printerId:long}` | `DocumentTemplatesController.UpdatePrinter` | Permissions.InvoiceTemplatesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 215 | Tenant | PUT | `/api/document-templates/templates/{templateId:long}/printers/{printerId:long}/print-setting` | `DocumentTemplatesController.UpdatePrintSetting` | Permissions.InvoiceTemplatesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 216 | Tenant | POST | `/api/document-templates/versions/{versionId:long}/clone` | `DocumentTemplatesController.CloneToDraft` | Permissions.InvoiceTemplatesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 217 | Tenant | GET | `/api/document-templates/versions/{versionId:long}/designer` | `DocumentTemplatesController.GetDesigner` | Permissions.InvoiceTemplatesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 218 | Tenant | PUT | `/api/document-templates/versions/{versionId:long}/designer` | `DocumentTemplatesController.SaveDesigner` | Permissions.InvoiceTemplatesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 219 | Tenant | GET | `/api/document-templates/versions/{versionId:long}/preview` | `DocumentTemplatesController.Preview` | Permissions.InvoiceTemplatesPreview | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 220 | Tenant | POST | `/api/document-templates/versions/{versionId:long}/publish` | `DocumentTemplatesController.Publish` | Permissions.InvoiceTemplatesPublish | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| 221 | Tenant | GET | `/api/document-types` | `DocumentTypesController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentTypesController.cs` |
| 222 | Tenant | POST | `/api/document-types` | `DocumentTypesController.Create` | Permissions.DocumentTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentTypesController.cs` |
| 223 | Tenant | DELETE | `/api/document-types/{id:int}` | `DocumentTypesController.Delete` | Permissions.DocumentTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentTypesController.cs` |
| 224 | Tenant | GET | `/api/document-types/{id:int}` | `DocumentTypesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentTypesController.cs` |
| 225 | Tenant | PUT | `/api/document-types/{id:int}` | `DocumentTypesController.Update` | Permissions.DocumentTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/DocumentTypesController.cs` |
| 226 | Tenant | GET | `/api/financial-years` | `FinancialYearsController.GetFinancialYears` | Permissions.FinancialYearsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/FinancialYearController.cs` |
| 227 | Tenant | POST | `/api/financial-years` | `FinancialYearsController.CreateFinancialYear` | Permissions.FinancialYearsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/FinancialYearController.cs` |
| 228 | Tenant | DELETE | `/api/financial-years/{id:long}` | `FinancialYearsController.DeleteFinancialYear` | Permissions.FinancialYearsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/FinancialYearController.cs` |
| 229 | Tenant | GET | `/api/financial-years/{id:long}` | `FinancialYearsController.GetFinancialYear` | Permissions.FinancialYearsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/FinancialYearController.cs` |
| 230 | Tenant | PUT | `/api/financial-years/{id:long}` | `FinancialYearsController.UpdateFinancialYear` | Permissions.FinancialYearsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/FinancialYearController.cs` |
| 231 | Tenant | GET | `/api/financial-years/all` | `FinancialYearsController.GetAllFinancialYears` | Permissions.FinancialYearsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/FinancialYearController.cs` |
| 232 | Tenant | GET | `/api/financial-years/next-code` | `FinancialYearsController.GetNextFYCode` | Permissions.FinancialYearsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/FinancialYearController.cs` |
| 233 | Tenant | GET | `/api/gst-registration-types` | `GstRegistrationTypesController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/GstRegistrationTypesController.cs` |
| 234 | Tenant | POST | `/api/gst-registration-types` | `GstRegistrationTypesController.Create` | Permissions.GstRegistrationTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/GstRegistrationTypesController.cs` |
| 235 | Tenant | DELETE | `/api/gst-registration-types/{id:int}` | `GstRegistrationTypesController.Delete` | Permissions.GstRegistrationTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/GstRegistrationTypesController.cs` |
| 236 | Tenant | GET | `/api/gst-registration-types/{id:int}` | `GstRegistrationTypesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/GstRegistrationTypesController.cs` |
| 237 | Tenant | PUT | `/api/gst-registration-types/{id:int}` | `GstRegistrationTypesController.Update` | Permissions.GstRegistrationTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/GstRegistrationTypesController.cs` |
| 238 | Tenant | GET | `/api/hsn-sacs` | `HsnSacsController.GetPaged` | Permissions.HsnSacsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 239 | Tenant | POST | `/api/hsn-sacs` | `HsnSacsController.Create` | Permissions.HsnSacsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 240 | Tenant | DELETE | `/api/hsn-sacs/{id:long}` | `HsnSacsController.Delete` | Permissions.HsnSacsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 241 | Tenant | GET | `/api/hsn-sacs/{id:long}` | `HsnSacsController.GetById` | Permissions.HsnSacsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 242 | Tenant | PUT | `/api/hsn-sacs/{id:long}` | `HsnSacsController.Update` | Permissions.HsnSacsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 243 | Tenant | GET | `/api/hsn-sacs/next-code` | `HsnSacsController.GetNextCode` | Permissions.HsnSacsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 244 | Tenant | GET | `/api/import-logs` | `ImportLogsController.GetAll` | Permissions.ImportLogsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ImportLogsController.cs` |
| 245 | Tenant | POST | `/api/import/{entityName}/export` | `MasterImportController.Export` | Permissions.MasterImportView | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterImportController.cs` |
| 246 | Tenant | GET | `/api/import/{entityName}/export/meta` | `MasterImportController.GetExportMeta` | Permissions.MasterImportView | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterImportController.cs` |
| 247 | Tenant | POST | `/api/import/{entityName}/export/options` | `MasterImportController.GetExportOptions` | Permissions.MasterImportView | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterImportController.cs` |
| 248 | Tenant | POST | `/api/import/{entityName}/export/preview` | `MasterImportController.ExportPreview` | Permissions.MasterImportView | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterImportController.cs` |
| 249 | Tenant | GET | `/api/import/{entityName}/template` | `MasterImportController.GetTemplate` | Permissions.MasterImportView | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterImportController.cs` |
| 250 | Tenant | POST | `/api/import/confirm` | `MasterImportController.Confirm` | Permissions.MasterImportManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterImportController.cs` |
| 251 | Tenant | GET | `/api/import/masters` | `MasterImportController.GetMasters` | Permissions.MasterImportView | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterImportController.cs` |
| 252 | Tenant | POST | `/api/import/preview` | `MasterImportController.Preview` | Permissions.MasterImportView | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterImportController.cs` |
| 253 | Tenant | GET | `/api/industry-types` | `IndustryTypesController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 254 | Tenant | POST | `/api/industry-types` | `IndustryTypesController.Create` | Permissions.IndustryTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 255 | Tenant | DELETE | `/api/industry-types/{id:int}` | `IndustryTypesController.Delete` | Permissions.IndustryTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 256 | Tenant | GET | `/api/industry-types/{id:int}` | `IndustryTypesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 257 | Tenant | PUT | `/api/industry-types/{id:int}` | `IndustryTypesController.Update` | Permissions.IndustryTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 258 | Tenant | GET | `/api/inventory/adjustments` | `InventoryController.GetAdjustments` | Permissions.StockView | [Authorize] on class | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| 259 | Tenant | POST | `/api/inventory/adjustments` | `InventoryController.CreateAdjustment` | Permissions.StockManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| 260 | Tenant | POST | `/api/inventory/adjustments/{id:long}/post` | `InventoryController.PostAdjustment` | Permissions.StockManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| 261 | Tenant | GET | `/api/inventory/counts` | `InventoryController.GetCounts` | Permissions.StockView | [Authorize] on class | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| 262 | Tenant | POST | `/api/inventory/counts` | `InventoryController.CreateCount` | Permissions.StockManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| 263 | Tenant | POST | `/api/inventory/counts/{id:long}/post` | `InventoryController.PostCount` | Permissions.StockManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| 264 | Tenant | GET | `/api/inventory/dashboard` | `InventoryController.Dashboard` | Permissions.StockView | [Authorize] on class | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| 265 | Tenant | GET | `/api/inventory/low-stock` | `InventoryController.LowStock` | Permissions.StockView | [Authorize] on class | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| 266 | Tenant | POST | `/api/inventory/opening` | `InventoryController.Opening` | Permissions.StockManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| 267 | Tenant | GET | `/api/inventory/reconciliation` | `InventoryController.Reconciliation` | Permissions.StockView | [Authorize] on class | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| 268 | Tenant | GET | `/api/inventory/transfers` | `InventoryController.GetTransfers` | Permissions.StockView | [Authorize] on class | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| 269 | Tenant | POST | `/api/inventory/transfers` | `InventoryController.CreateTransfer` | Permissions.StockManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| 270 | Tenant | POST | `/api/inventory/transfers/{id:long}/post` | `InventoryController.PostTransfer` | Permissions.StockManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| 271 | Tenant | GET | `/api/inventory/valuation` | `InventoryController.Valuation` | Permissions.StockView | [Authorize] on class | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| 272 | Tenant | GET | `/api/languages` | `LanguagesController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/LanguagesController.cs` |
| 273 | Tenant | POST | `/api/languages` | `LanguagesController.Create` | Permissions.LanguagesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/LanguagesController.cs` |
| 274 | Tenant | DELETE | `/api/languages/{id:int}` | `LanguagesController.Delete` | Permissions.LanguagesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/LanguagesController.cs` |
| 275 | Tenant | GET | `/api/languages/{id:int}` | `LanguagesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/LanguagesController.cs` |
| 276 | Tenant | PUT | `/api/languages/{id:int}` | `LanguagesController.Update` | Permissions.LanguagesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/LanguagesController.cs` |
| 277 | Tenant | POST | `/api/offer-details` | `OfferDetailsController.Create` | Permissions.OffersCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 278 | Tenant | DELETE | `/api/offer-details/{id:long}` | `OfferDetailsController.Delete` | Permissions.OffersDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 279 | Tenant | GET | `/api/offer-details/{id:long}` | `OfferDetailsController.GetById` | Permissions.OffersView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 280 | Tenant | PUT | `/api/offer-details/{id:long}` | `OfferDetailsController.Update` | Permissions.OffersEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 281 | Tenant | GET | `/api/offers` | `OffersController.GetPaged` | Permissions.OffersView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 282 | Tenant | POST | `/api/offers` | `OffersController.Create` | Permissions.OffersCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 283 | Tenant | DELETE | `/api/offers/{id:long}` | `OffersController.Delete` | Permissions.OffersDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 284 | Tenant | GET | `/api/offers/{id:long}` | `OffersController.GetById` | Permissions.OffersView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 285 | Tenant | PUT | `/api/offers/{id:long}` | `OffersController.Update` | Permissions.OffersEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 286 | Tenant | GET | `/api/offers/{id:long}/details` | `OffersController.GetDetails` | Permissions.OffersView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 287 | Tenant | POST | `/api/offers/{id:long}/details/replace` | `OffersController.ReplaceDetails` | Permissions.OffersEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 288 | Tenant | GET | `/api/offers/next-code` | `OffersController.GetNextCode` | Permissions.OffersView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 289 | Tenant | GET | `/api/operator-types` | `OperatorTypesController.GetPaged` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 290 | Tenant | POST | `/api/operator-types` | `OperatorTypesController.Create` | Permissions.OperatorTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 291 | Tenant | DELETE | `/api/operator-types/{id:int}` | `OperatorTypesController.Delete` | Permissions.OperatorTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 292 | Tenant | GET | `/api/operator-types/{id:int}` | `OperatorTypesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 293 | Tenant | PUT | `/api/operator-types/{id:int}` | `OperatorTypesController.Update` | Permissions.OperatorTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 294 | Tenant | GET | `/api/operators` | `OperatorsController.GetOperators` | Permissions.OperatorsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 295 | Tenant | POST | `/api/operators` | `OperatorsController.CreateOperator` | Permissions.OperatorsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 296 | Tenant | DELETE | `/api/operators/{id:int}` | `OperatorsController.DeleteOperator` | Permissions.OperatorsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 297 | Tenant | GET | `/api/operators/{id:int}` | `OperatorsController.GetOperator` | Permissions.OperatorsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 298 | Tenant | PUT | `/api/operators/{id:int}` | `OperatorsController.UpdateOperator` | Permissions.OperatorsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 299 | Tenant | GET | `/api/operators/all` | `OperatorsController.GetAllOperators` | Permissions.OperatorsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 300 | Tenant | GET | `/api/operators/me` | `OperatorsController.GetMyOperator` | Permissions.OperatorsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 301 | Tenant | GET | `/api/operators/next-code` | `OperatorsController.GetNextCode` | Permissions.OperatorsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| 302 | Tenant | GET | `/api/organization-types` | `OrganizationTypesController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationTypesController.cs` |
| 303 | Tenant | POST | `/api/organization-types` | `OrganizationTypesController.Create` | Permissions.OrganizationTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationTypesController.cs` |
| 304 | Tenant | DELETE | `/api/organization-types/{id:int}` | `OrganizationTypesController.Delete` | Permissions.OrganizationTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationTypesController.cs` |
| 305 | Tenant | GET | `/api/organization-types/{id:int}` | `OrganizationTypesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationTypesController.cs` |
| 306 | Tenant | PUT | `/api/organization-types/{id:int}` | `OrganizationTypesController.Update` | Permissions.OrganizationTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationTypesController.cs` |
| 307 | Tenant | GET | `/api/organization/branch-types` | `OrganizationController.GetBranchTypes` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 308 | Tenant | GET | `/api/organization/branches` | `OrganizationController.GetBranches` | Permissions.BranchesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 309 | Tenant | POST | `/api/organization/branches` | `OrganizationController.CreateBranch` | Permissions.BranchesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 310 | Tenant | DELETE | `/api/organization/branches/{id:int}` | `OrganizationController.DeleteBranch` | Permissions.BranchesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 311 | Tenant | GET | `/api/organization/branches/{id:int}` | `OrganizationController.GetBranch` | Permissions.BranchesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 312 | Tenant | PUT | `/api/organization/branches/{id:int}` | `OrganizationController.UpdateBranch` | Permissions.BranchesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 313 | Tenant | GET | `/api/organization/branches/next-code` | `OrganizationController.GetNextBranchCode` | Permissions.BranchesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 314 | Tenant | GET | `/api/organization/departments` | `OrganizationController.GetDepartments` | Permissions.DepartmentsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 315 | Tenant | POST | `/api/organization/departments` | `OrganizationController.CreateDepartment` | Permissions.DepartmentsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 316 | Tenant | DELETE | `/api/organization/departments/{id:int}` | `OrganizationController.DeleteDepartment` | Permissions.DepartmentsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 317 | Tenant | GET | `/api/organization/departments/{id:int}` | `OrganizationController.GetDepartment` | Permissions.DepartmentsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 318 | Tenant | PUT | `/api/organization/departments/{id:int}` | `OrganizationController.UpdateDepartment` | Permissions.DepartmentsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 319 | Tenant | GET | `/api/organization/departments/next-code` | `OrganizationController.GetNextDepartmentCode` | Permissions.DepartmentsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 320 | Tenant | GET | `/api/organization/designations` | `OrganizationController.GetDesignations` | Permissions.DesignationsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 321 | Tenant | POST | `/api/organization/designations` | `OrganizationController.CreateDesignation` | Permissions.DesignationsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 322 | Tenant | DELETE | `/api/organization/designations/{id:int}` | `OrganizationController.DeleteDesignation` | Permissions.DesignationsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 323 | Tenant | GET | `/api/organization/designations/{id:int}` | `OrganizationController.GetDesignation` | Permissions.DesignationsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 324 | Tenant | PUT | `/api/organization/designations/{id:int}` | `OrganizationController.UpdateDesignation` | Permissions.DesignationsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 325 | Tenant | GET | `/api/organization/designations/next-code` | `OrganizationController.GetNextDesignationCode` | Permissions.DesignationsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 326 | Tenant | GET | `/api/organization/employees` | `OrganizationController.GetEmployees` | Permissions.EmployeesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 327 | Tenant | POST | `/api/organization/employees` | `OrganizationController.CreateEmployee` | Permissions.EmployeesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 328 | Tenant | DELETE | `/api/organization/employees/{id:int}` | `OrganizationController.DeleteEmployee` | Permissions.EmployeesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 329 | Tenant | GET | `/api/organization/employees/{id:int}` | `OrganizationController.GetEmployee` | Permissions.EmployeesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 330 | Tenant | PUT | `/api/organization/employees/{id:int}` | `OrganizationController.UpdateEmployee` | Permissions.EmployeesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 331 | Tenant | GET | `/api/organization/employment-types` | `OrganizationController.GetEmploymentTypes` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 332 | Tenant | GET | `/api/organization/warehouse-types` | `OrganizationController.GetWarehouseTypes` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 333 | Tenant | GET | `/api/organization/warehouses` | `OrganizationController.GetWarehouses` | Permissions.WarehousesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 334 | Tenant | POST | `/api/organization/warehouses` | `OrganizationController.CreateWarehouse` | Permissions.WarehousesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 335 | Tenant | DELETE | `/api/organization/warehouses/{id:int}` | `OrganizationController.DeleteWarehouse` | Permissions.WarehousesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 336 | Tenant | GET | `/api/organization/warehouses/{id:int}` | `OrganizationController.GetWarehouse` | Permissions.WarehousesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 337 | Tenant | PUT | `/api/organization/warehouses/{id:int}` | `OrganizationController.UpdateWarehouse` | Permissions.WarehousesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 338 | Tenant | GET | `/api/organization/warehouses/all` | `OrganizationController.GetAllWarehouses` | Permissions.WarehousesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 339 | Tenant | GET | `/api/organization/warehouses/next-code` | `OrganizationController.GetNextWarehouseCode` | Permissions.WarehousesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| 340 | Tenant | GET | `/api/payment-method-details` | `PaymentMethodDetailsController.GetByPaymentMethod` | Permissions.PaymentMethodDetailsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 341 | Tenant | POST | `/api/payment-method-details` | `PaymentMethodDetailsController.Create` | Permissions.PaymentMethodDetailsManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 342 | Tenant | DELETE | `/api/payment-method-details/{id:long}` | `PaymentMethodDetailsController.Delete` | Permissions.PaymentMethodDetailsManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 343 | Tenant | GET | `/api/payment-method-details/{id:long}` | `PaymentMethodDetailsController.GetById` | Permissions.PaymentMethodDetailsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 344 | Tenant | PUT | `/api/payment-method-details/{id:long}` | `PaymentMethodDetailsController.Update` | Permissions.PaymentMethodDetailsManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 345 | Tenant | GET | `/api/payment-methods` | `PaymentMethodsController.GetAll` | Permissions.PaymentMethodsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 346 | Tenant | POST | `/api/payment-methods` | `PaymentMethodsController.Create` | Permissions.PaymentMethodsManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 347 | Tenant | DELETE | `/api/payment-methods/{id:long}` | `PaymentMethodsController.Delete` | Permissions.PaymentMethodsManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 348 | Tenant | GET | `/api/payment-methods/{id:long}` | `PaymentMethodsController.GetById` | Permissions.PaymentMethodsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 349 | Tenant | PUT | `/api/payment-methods/{id:long}` | `PaymentMethodsController.Update` | Permissions.PaymentMethodsManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 350 | Tenant | GET | `/api/payment-types` | `PaymentTypesController.GetAll` | Permissions.PaymentTypesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 351 | Tenant | POST | `/api/payment-types` | `PaymentTypesController.Create` | Permissions.PaymentTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 352 | Tenant | DELETE | `/api/payment-types/{id:long}` | `PaymentTypesController.Delete` | Permissions.PaymentTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 353 | Tenant | GET | `/api/payment-types/{id:long}` | `PaymentTypesController.GetById` | Permissions.PaymentTypesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 354 | Tenant | PUT | `/api/payment-types/{id:long}` | `PaymentTypesController.Update` | Permissions.PaymentTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| 355 | Tenant | GET | `/api/payments` | `PaymentsController.GetAll` | Permissions.PaymentsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentsController.cs` |
| 356 | Tenant | POST | `/api/payments` | `PaymentsController.Create` | Permissions.PaymentsManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentsController.cs` |
| 357 | Tenant | DELETE | `/api/payments/{id:long}` | `PaymentsController.Delete` | Permissions.PaymentsManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentsController.cs` |
| 358 | Tenant | GET | `/api/payments/{id:long}` | `PaymentsController.GetById` | Permissions.PaymentsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentsController.cs` |
| 359 | Tenant | PUT | `/api/payments/{id:long}` | `PaymentsController.Update` | Permissions.PaymentsManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentsController.cs` |
| 360 | Tenant | GET | `/api/payments/lookups` | `PaymentsController.Lookups` | Permissions.PaymentsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentsController.cs` |
| 361 | Tenant | GET | `/api/payments/next-number` | `PaymentsController.NextNumber` | Permissions.PaymentsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/PaymentsController.cs` |
| 362 | Tenant | POST | `/api/payments/refunds` | `RefundsController.CreateRefund` | Permissions.SalesReturnManage, Permissions.PaymentsManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesReturnsController.cs` |
| 363 | Tenant | GET | `/api/payments/refunds/{id:long}` | `RefundsController.GetRefund` | Permissions.SalesReturnView, Permissions.PaymentsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesReturnsController.cs` |
| 364 | Tenant | GET | `/api/permission/user-permissions` | `UserPermissionsController.GetCurrentUserPermissions` | None (authentication only per global filter) | Not found on class | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| 365 | Tenant | GET | `/api/permissions/my-navigation` | `PermissionsController.MyNavigation` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/PermissionsController.cs` |
| 366 | Tenant | GET | `/api/pos-sessions` | `POSSessionsController.GetSessions` | Permissions.POSSessionView | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 367 | Tenant | POST | `/api/pos-sessions` | `POSSessionsController.CreateSession` | Permissions.POSSessionCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 368 | Tenant | DELETE | `/api/pos-sessions/{id:long}` | `POSSessionsController.DeleteSession` | Permissions.POSSessionDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 369 | Tenant | GET | `/api/pos-sessions/{id:long}` | `POSSessionsController.GetSession` | Permissions.POSSessionView | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 370 | Tenant | PUT | `/api/pos-sessions/{id:long}` | `POSSessionsController.UpdateSession` | Permissions.POSSessionEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 371 | Tenant | POST | `/api/pos/cash-in` | `POSOperationsController.CashIn` | Permissions.SalesPOSView, Permissions.SalesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/POSOperationsController.cs` |
| 372 | Tenant | POST | `/api/pos/cash-out` | `POSOperationsController.CashOut` | Permissions.SalesPOSView, Permissions.SalesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/POSOperationsController.cs` |
| 373 | Tenant | GET | `/api/pos/dashboard` | `POSOperationsController.Dashboard` | Permissions.SalesPOSView, Permissions.POSSessionView | [Authorize] on class | `ONEERP.ERP.API/Controllers/POSOperationsController.cs` |
| 374 | Tenant | GET | `/api/pos/holds` | `POSOperationsController.GetHolds` | Permissions.SalesPOSView, Permissions.SalesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/POSOperationsController.cs` |
| 375 | Tenant | POST | `/api/pos/holds` | `POSOperationsController.Hold` | Permissions.SalesPOSView, Permissions.SalesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/POSOperationsController.cs` |
| 376 | Tenant | POST | `/api/pos/holds/{id:long}/cancel` | `POSOperationsController.CancelHold` | Permissions.SalesPOSView, Permissions.SalesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/POSOperationsController.cs` |
| 377 | Tenant | POST | `/api/pos/holds/{id:long}/recall` | `POSOperationsController.Recall` | Permissions.SalesPOSView, Permissions.SalesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/POSOperationsController.cs` |
| 378 | Tenant | POST | `/api/pos/sessions/{id:long}/close` | `POSOperationsController.CloseSession` | Permissions.POSSessionEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/POSOperationsController.cs` |
| 379 | Tenant | GET | `/api/pos/sessions/{id:long}/summary` | `POSOperationsController.ShiftSummary` | Permissions.SalesPOSView, Permissions.POSSessionView | [Authorize] on class | `ONEERP.ERP.API/Controllers/POSOperationsController.cs` |
| 380 | Tenant | GET | `/api/pos/sessions/current` | `POSOperationsController.CurrentSession` | Permissions.SalesPOSView, Permissions.POSSessionView | [Authorize] on class | `ONEERP.ERP.API/Controllers/POSOperationsController.cs` |
| 381 | Tenant | POST | `/api/price-list-details` | `PriceListDetailsController.Create` | Permissions.PriceListsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 382 | Tenant | DELETE | `/api/price-list-details/{id:long}` | `PriceListDetailsController.Delete` | Permissions.PriceListsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 383 | Tenant | GET | `/api/price-list-details/{id:long}` | `PriceListDetailsController.GetById` | Permissions.PriceListsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 384 | Tenant | PUT | `/api/price-list-details/{id:long}` | `PriceListDetailsController.Update` | Permissions.PriceListsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 385 | Tenant | GET | `/api/price-lists` | `PriceListsController.GetPaged` | Permissions.PriceListsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 386 | Tenant | POST | `/api/price-lists` | `PriceListsController.Create` | Permissions.PriceListsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 387 | Tenant | DELETE | `/api/price-lists/{id:long}` | `PriceListsController.Delete` | Permissions.PriceListsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 388 | Tenant | GET | `/api/price-lists/{id:long}` | `PriceListsController.GetById` | Permissions.PriceListsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 389 | Tenant | PUT | `/api/price-lists/{id:long}` | `PriceListsController.Update` | Permissions.PriceListsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 390 | Tenant | GET | `/api/price-lists/{id:long}/details` | `PriceListsController.GetDetails` | Permissions.PriceListsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 391 | Tenant | POST | `/api/price-lists/{id:long}/details/replace` | `PriceListsController.ReplaceDetails` | Permissions.PriceListsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 392 | Tenant | GET | `/api/price-lists/{priceListId:long}/price-types` | `PriceListPriceTypesController.GetByPriceList` | Permissions.PriceListsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 393 | Tenant | POST | `/api/price-lists/{priceListId:long}/price-types` | `PriceListPriceTypesController.Create` | Permissions.PriceListsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 394 | Tenant | DELETE | `/api/price-lists/{priceListId:long}/price-types/{priceTypeId:long}` | `PriceListPriceTypesController.Delete` | Permissions.PriceListsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 395 | Tenant | PUT | `/api/price-lists/{priceListId:long}/price-types/{priceTypeId:long}` | `PriceListPriceTypesController.Update` | Permissions.PriceListsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 396 | Tenant | GET | `/api/price-lists/next-code` | `PriceListsController.GetNextCode` | Permissions.PriceListsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 397 | Tenant | GET | `/api/price-types` | `PriceTypesController.GetAll` | Permissions.PriceTypesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 398 | Tenant | POST | `/api/price-types` | `PriceTypesController.Create` | Permissions.PriceTypesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 399 | Tenant | DELETE | `/api/price-types/{id:long}` | `PriceTypesController.Delete` | Permissions.PriceTypesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 400 | Tenant | GET | `/api/price-types/{id:long}` | `PriceTypesController.GetById` | Permissions.PriceTypesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 401 | Tenant | PUT | `/api/price-types/{id:long}` | `PriceTypesController.Update` | Permissions.PriceTypesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 402 | Tenant | GET | `/api/price-types/next-code` | `PriceTypesController.GetNextCode` | Permissions.PriceTypesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 403 | Tenant | GET | `/api/product-categories` | `ProductCategoriesController.GetPaged` | Permissions.ProductCategoriesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 404 | Tenant | POST | `/api/product-categories` | `ProductCategoriesController.Create` | Permissions.ProductCategoriesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 405 | Tenant | DELETE | `/api/product-categories/{id:int}` | `ProductCategoriesController.Delete` | Permissions.ProductCategoriesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 406 | Tenant | GET | `/api/product-categories/{id:int}` | `ProductCategoriesController.GetById` | Permissions.ProductCategoriesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 407 | Tenant | PUT | `/api/product-categories/{id:int}` | `ProductCategoriesController.Update` | Permissions.ProductCategoriesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 408 | Tenant | GET | `/api/product-categories/next-code` | `ProductCategoriesController.GetNextCode` | Permissions.ProductCategoriesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 409 | Tenant | GET | `/api/product-subcategories` | `ProductSubCategoriesController.GetPaged` | Permissions.ProductSubCategoriesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 410 | Tenant | POST | `/api/product-subcategories` | `ProductSubCategoriesController.Create` | Permissions.ProductSubCategoriesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 411 | Tenant | DELETE | `/api/product-subcategories/{id:int}` | `ProductSubCategoriesController.Delete` | Permissions.ProductSubCategoriesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 412 | Tenant | GET | `/api/product-subcategories/{id:int}` | `ProductSubCategoriesController.GetById` | Permissions.ProductSubCategoriesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 413 | Tenant | PUT | `/api/product-subcategories/{id:int}` | `ProductSubCategoriesController.Update` | Permissions.ProductSubCategoriesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 414 | Tenant | GET | `/api/product-subcategories/next-code` | `ProductSubCategoriesController.GetNextCode` | Permissions.ProductSubCategoriesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 415 | Tenant | GET | `/api/products` | `ProductsController.GetPaged` | Permissions.ProductsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 416 | Tenant | POST | `/api/products` | `ProductsController.Create` | Permissions.ProductsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 417 | Tenant | DELETE | `/api/products/{id:int}` | `ProductsController.Delete` | Permissions.ProductsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 418 | Tenant | GET | `/api/products/{id:int}` | `ProductsController.GetById` | Permissions.ProductsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 419 | Tenant | PUT | `/api/products/{id:int}` | `ProductsController.Update` | Permissions.ProductsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 420 | Tenant | GET | `/api/products/next-code` | `ProductsController.GetNextCode` | Permissions.ProductsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 421 | Tenant | GET | `/api/products/price-master-products` | `ProductsController.GetPriceMasterProducts` | Permissions.ProductsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 422 | Tenant | GET | `/api/profile` | `ProfileController.Get` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProfileController.cs` |
| 423 | Tenant | PUT | `/api/profile/password` | `ProfileController.ChangePassword` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProfileController.cs` |
| 424 | Tenant | GET | `/api/purchase-reports` | `PurchaseReportsController.Run` | Permissions.PurchasesView, Permissions.PurchasesReturnView, Permissions.PurchaseReturnView | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchaseReportsController.cs` |
| 425 | Tenant | GET | `/api/purchase-returns` | `PurchaseReturnsController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchaseReturnsController.cs` |
| 426 | Tenant | POST | `/api/purchase-returns` | `PurchaseReturnsController.Create` | Permissions.PurchasesReturnManage, Permissions.PurchaseReturnCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchaseReturnsController.cs` |
| 427 | Tenant | DELETE | `/api/purchase-returns/{id:long}` | `PurchaseReturnsController.Delete` | Permissions.PurchasesReturnManage, Permissions.PurchaseReturnDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchaseReturnsController.cs` |
| 428 | Tenant | GET | `/api/purchase-returns/{id:long}` | `PurchaseReturnsController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchaseReturnsController.cs` |
| 429 | Tenant | PUT | `/api/purchase-returns/{id:long}` | `PurchaseReturnsController.Update` | Permissions.PurchasesReturnManage, Permissions.PurchaseReturnEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchaseReturnsController.cs` |
| 430 | Tenant | POST | `/api/purchase-returns/{id:long}/cancel` | `PurchaseReturnsController.Cancel` | Permissions.PurchasesReturnManage, Permissions.PurchaseReturnCancel | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchaseReturnsController.cs` |
| 431 | Tenant | GET | `/api/purchase-returns/{id:long}/delete-check` | `PurchaseReturnsController.DeleteCheck` | Permissions.PurchasesReturnView, Permissions.PurchasesReturnManage, Permissions.PurchaseReturnView, Permissions.PurchaseReturnDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchaseReturnsController.cs` |
| 432 | Tenant | GET | `/api/purchase-returns/next-number` | `PurchaseReturnsController.NextNumber` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchaseReturnsController.cs` |
| 433 | Tenant | GET | `/api/purchases` | `PurchasesController.GetAll` | Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchasesController.cs` |
| 434 | Tenant | POST | `/api/purchases` | `PurchasesController.Create` | Permissions.PurchasesManage, Permissions.PurchasesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchasesController.cs` |
| 435 | Tenant | DELETE | `/api/purchases/{id:long}` | `PurchasesController.Delete` | Permissions.PurchasesManage, Permissions.PurchasesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchasesController.cs` |
| 436 | Tenant | GET | `/api/purchases/{id:long}` | `PurchasesController.GetById` | Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchasesController.cs` |
| 437 | Tenant | PUT | `/api/purchases/{id:long}` | `PurchasesController.Update` | Permissions.PurchasesManage, Permissions.PurchasesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchasesController.cs` |
| 438 | Tenant | POST | `/api/purchases/{id:long}/cancel` | `PurchasesController.Cancel` | Permissions.PurchasesManage, Permissions.PurchasesCancel | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchasesController.cs` |
| 439 | Tenant | GET | `/api/purchases/{id:long}/delete-check` | `PurchasesController.DeleteCheck` | Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchasesController.cs` |
| 440 | Tenant | GET | `/api/purchases/{id:long}/items` | `PurchasesController.GetItems` | Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchasesController.cs` |
| 441 | Tenant | GET | `/api/purchases/{id:long}/payments` | `PurchasesController.GetPayments` | Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchasesController.cs` |
| 442 | Tenant | GET | `/api/purchases/{id:long}/stock` | `PurchasesController.GetStock` | Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchasesController.cs` |
| 443 | Tenant | GET | `/api/purchases/lookups` | `PurchasesController.Lookups` | Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchasesController.cs` |
| 444 | Tenant | GET | `/api/purchases/next-number` | `PurchasesController.NextNumber` | Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/PurchasesController.cs` |
| 445 | Tenant | POST | `/api/role-field-permissions` | `RoleFieldPermissionsController.Set` | "role-field-permissions.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 446 | Tenant | DELETE | `/api/role-field-permissions/{id:int}` | `RoleFieldPermissionsController.Delete` | "role-field-permissions.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 447 | Tenant | POST | `/api/role-field-permissions/bulk` | `RoleFieldPermissionsController.BulkSet` | "role-field-permissions.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 448 | Tenant | GET | `/api/role-field-permissions/role/{roleId:int}` | `RoleFieldPermissionsController.GetByRole` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 449 | Tenant | GET | `/api/role-field-permissions/role/{roleId:int}/screen/{screenId:int}` | `RoleFieldPermissionsController.GetByRoleScreen` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 450 | Tenant | POST | `/api/role-permissions` | `RolePermissionsController.Assign` | "role-permissions.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 451 | Tenant | DELETE | `/api/role-permissions/{id:int}` | `RolePermissionsController.Delete` | "role-permissions.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 452 | Tenant | POST | `/api/role-permissions/bulk` | `RolePermissionsController.BulkAssign` | "role-permissions.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 453 | Tenant | DELETE | `/api/role-permissions/role/{roleId:int}` | `RolePermissionsController.DeleteAllForRole` | "role-permissions.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 454 | Tenant | GET | `/api/role-permissions/role/{roleId:int}` | `RolePermissionsController.GetByRole` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 455 | Tenant | GET | `/api/sales` | `SalesController.GetAll` | Permissions.SalesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 456 | Tenant | POST | `/api/sales` | `SalesController.Create` | Permissions.SalesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 457 | Tenant | GET | `/api/sales-reports` | `SalesReportsController.Run` | Permissions.SalesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesReportsController.cs` |
| 458 | Tenant | GET | `/api/sales-returns` | `SalesReturnsController.GetPaged` | Permissions.SalesReturnView, Permissions.SalesReturnManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesReturnsController.cs` |
| 459 | Tenant | POST | `/api/sales-returns` | `SalesReturnsController.Create` | Permissions.SalesReturnManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesReturnsController.cs` |
| 460 | Tenant | GET | `/api/sales-returns/{id:long}` | `SalesReturnsController.GetById` | Permissions.SalesReturnView, Permissions.SalesReturnManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesReturnsController.cs` |
| 461 | Tenant | PUT | `/api/sales-returns/{id:long}` | `SalesReturnsController.Update` | Permissions.SalesReturnManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesReturnsController.cs` |
| 462 | Tenant | POST | `/api/sales-returns/{id:long}/cancel` | `SalesReturnsController.Cancel` | Permissions.SalesReturnManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesReturnsController.cs` |
| 463 | Tenant | GET | `/api/sales-returns/next-number` | `SalesReturnsController.NextNumber` | Permissions.SalesReturnView, Permissions.SalesReturnManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesReturnsController.cs` |
| 464 | Tenant | DELETE | `/api/sales/{id:long}` | `SalesController.Delete` | Permissions.SalesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 465 | Tenant | GET | `/api/sales/{id:long}` | `SalesController.GetById` | Permissions.SalesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 466 | Tenant | PUT | `/api/sales/{id:long}` | `SalesController.Update` | Permissions.SalesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 467 | Tenant | POST | `/api/sales/{id:long}/cancel` | `SalesController.Cancel` | Permissions.SalesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 468 | Tenant | GET | `/api/sales/{id:long}/items` | `SalesController.GetItems` | Permissions.SalesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 469 | Tenant | PUT | `/api/sales/{id:long}/payment` | `SalesController.UpdatePayment` | Permissions.SalesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 470 | Tenant | GET | `/api/sales/{id:long}/payments` | `SalesController.GetPayments` | Permissions.SalesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 471 | Tenant | GET | `/api/sales/{id:long}/print-data` | `SalesController.GetPrintData` | Permissions.SalesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 472 | Tenant | GET | `/api/sales/{id:long}/stock` | `SalesController.GetStock` | Permissions.SalesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 473 | Tenant | POST | `/api/sales/{salesInvoiceId:long}/return` | `SalesController.CreateReturn` | Permissions.SalesReturnManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 474 | Tenant | GET | `/api/sales/lookups` | `SalesController.Lookups` | Permissions.SalesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 475 | Tenant | GET | `/api/sales/next-number` | `SalesController.NextNumber` | Permissions.SalesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 476 | Tenant | GET | `/api/sales/products/{productId:long}/stock` | `SalesController.GetProductStock` | Permissions.SalesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 477 | Tenant | GET | `/api/sales/products/search` | `SalesController.SearchProducts` | Permissions.SalesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| 478 | Tenant | GET | `/api/sales/sale-entry-context` | `SaleEntryContextController.GetSaleEntryContext` | Permissions.SalesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/SaleEntryContextController.cs` |
| 479 | Tenant | POST | `/api/sales/sale-entry-context/validate` | `SaleEntryContextController.ValidateSaleEntryContext` | Permissions.SalesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/SaleEntryContextController.cs` |
| 480 | Tenant | GET | `/api/service-categories` | `ServiceCategoriesController.GetPaged` | Permissions.ServiceCategoriesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 481 | Tenant | POST | `/api/service-categories` | `ServiceCategoriesController.Create` | Permissions.ServiceCategoriesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 482 | Tenant | DELETE | `/api/service-categories/{id:long}` | `ServiceCategoriesController.Delete` | Permissions.ServiceCategoriesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 483 | Tenant | GET | `/api/service-categories/{id:long}` | `ServiceCategoriesController.GetById` | Permissions.ServiceCategoriesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 484 | Tenant | PUT | `/api/service-categories/{id:long}` | `ServiceCategoriesController.Update` | Permissions.ServiceCategoriesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 485 | Tenant | GET | `/api/service-categories/next-code` | `ServiceCategoriesController.GetNextCode` | Permissions.ServiceCategoriesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 486 | Tenant | GET | `/api/services` | `ServicesController.GetPaged` | Permissions.ServicesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 487 | Tenant | POST | `/api/services` | `ServicesController.Create` | Permissions.ServicesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 488 | Tenant | DELETE | `/api/services/{id:long}` | `ServicesController.Delete` | Permissions.ServicesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 489 | Tenant | GET | `/api/services/{id:long}` | `ServicesController.GetById` | Permissions.ServicesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 490 | Tenant | PUT | `/api/services/{id:long}` | `ServicesController.Update` | Permissions.ServicesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 491 | Tenant | GET | `/api/services/next-code` | `ServicesController.GetNextCode` | Permissions.ServicesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 492 | Tenant | GET | `/api/sources` | `SourcesController.GetPaged` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 493 | Tenant | POST | `/api/sources` | `SourcesController.Create` | Permissions.SourcesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 494 | Tenant | DELETE | `/api/sources/{id:int}` | `SourcesController.Delete` | Permissions.SourcesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 495 | Tenant | GET | `/api/sources/{id:int}` | `SourcesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 496 | Tenant | PUT | `/api/sources/{id:int}` | `SourcesController.Update` | Permissions.SourcesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 497 | Tenant | GET | `/api/states` | `StatesController.GetAll` | Permissions.LocationsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 498 | Tenant | POST | `/api/states` | `StatesController.Create` | Permissions.LocationsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 499 | Tenant | DELETE | `/api/states/{id:int}` | `StatesController.Delete` | Permissions.LocationsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 500 | Tenant | GET | `/api/states/{id:int}` | `StatesController.GetById` | Permissions.LocationsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 501 | Tenant | PUT | `/api/states/{id:int}` | `StatesController.Update` | Permissions.LocationsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| 502 | Tenant | GET | `/api/stock` | `StockController.GetAll` | Permissions.StockView | [Authorize] on class | `ONEERP.ERP.API/Controllers/StockController.cs` |
| 503 | Tenant | GET | `/api/stock/price-master-products` | `StockController.GetPriceMasterProducts` | Permissions.StockView | [Authorize] on class | `ONEERP.ERP.API/Controllers/StockController.cs` |
| 504 | Tenant | GET | `/api/stock/transactions` | `StockController.Transactions` | Permissions.StockView | [Authorize] on class | `ONEERP.ERP.API/Controllers/StockController.cs` |
| 505 | Tenant | GET | `/api/store-types` | `StoreTypesController.GetPaged` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 506 | Tenant | POST | `/api/store-types` | `StoreTypesController.Create` | Permissions.StoreTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 507 | Tenant | DELETE | `/api/store-types/{id:int}` | `StoreTypesController.Delete` | Permissions.StoreTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 508 | Tenant | GET | `/api/store-types/{id:int}` | `StoreTypesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 509 | Tenant | PUT | `/api/store-types/{id:int}` | `StoreTypesController.Update` | Permissions.StoreTypesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| 510 | Tenant | GET | `/api/stores` | `StoresController.GetStores` | Permissions.StoresView | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 511 | Tenant | POST | `/api/stores` | `StoresController.CreateStore` | Permissions.StoresCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 512 | Tenant | DELETE | `/api/stores/{id:int}` | `StoresController.DeleteStore` | Permissions.StoresDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 513 | Tenant | GET | `/api/stores/{id:int}` | `StoresController.GetStore` | Permissions.StoresView | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 514 | Tenant | PUT | `/api/stores/{id:int}` | `StoresController.UpdateStore` | Permissions.StoresEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 515 | Tenant | GET | `/api/stores/all` | `StoresController.GetAllStores` | Permissions.StoresView | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 516 | Tenant | GET | `/api/stores/next-code` | `StoresController.GetNextStoreCode` | Permissions.StoresView | [Authorize] on class | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| 517 | Tenant | GET | `/api/tax-type-systems` | `TaxTypeSystemsController.GetPaged` | Permissions.TaxTypeSystemsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/TaxMasterControllers.cs` |
| 518 | Tenant | POST | `/api/tax-type-systems` | `TaxTypeSystemsController.Create` | Permissions.TaxTypeSystemsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/TaxMasterControllers.cs` |
| 519 | Tenant | DELETE | `/api/tax-type-systems/{id:long}` | `TaxTypeSystemsController.Delete` | Permissions.TaxTypeSystemsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/TaxMasterControllers.cs` |
| 520 | Tenant | GET | `/api/tax-type-systems/{id:long}` | `TaxTypeSystemsController.GetById` | Permissions.TaxTypeSystemsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/TaxMasterControllers.cs` |
| 521 | Tenant | PUT | `/api/tax-type-systems/{id:long}` | `TaxTypeSystemsController.Update` | Permissions.TaxTypeSystemsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/TaxMasterControllers.cs` |
| 522 | Tenant | GET | `/api/tax-type-systems/next-code` | `TaxTypeSystemsController.GetNextCode` | Permissions.TaxTypeSystemsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/TaxMasterControllers.cs` |
| 523 | Tenant | GET | `/api/taxes` | `TaxesController.GetPaged` | Permissions.TaxesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/TaxMasterControllers.cs` |
| 524 | Tenant | POST | `/api/taxes` | `TaxesController.Create` | Permissions.TaxesCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/TaxMasterControllers.cs` |
| 525 | Tenant | DELETE | `/api/taxes/{id:long}` | `TaxesController.Delete` | Permissions.TaxesDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/TaxMasterControllers.cs` |
| 526 | Tenant | GET | `/api/taxes/{id:long}` | `TaxesController.GetById` | Permissions.TaxesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/TaxMasterControllers.cs` |
| 527 | Tenant | PUT | `/api/taxes/{id:long}` | `TaxesController.Update` | Permissions.TaxesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/TaxMasterControllers.cs` |
| 528 | Tenant | GET | `/api/taxes/next-code` | `TaxesController.GetNextCode` | Permissions.TaxesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/TaxMasterControllers.cs` |
| 529 | Tenant | GET | `/api/tenant-configuration` | `TenantConfigurationController.GetAll` | Permissions.TenantConfigView | [Authorize] on class | `ONEERP.ERP.API/Controllers/TenantConfigurationController.cs` |
| 530 | Tenant | POST | `/api/tenant-configuration` | `TenantConfigurationController.Create` | Permissions.TenantConfigManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/TenantConfigurationController.cs` |
| 531 | Tenant | DELETE | `/api/tenant-configuration/{id:long}` | `TenantConfigurationController.Delete` | Permissions.TenantConfigManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/TenantConfigurationController.cs` |
| 532 | Tenant | GET | `/api/tenant-configuration/{id:long}` | `TenantConfigurationController.GetById` | Permissions.TenantConfigView | [Authorize] on class | `ONEERP.ERP.API/Controllers/TenantConfigurationController.cs` |
| 533 | Tenant | PUT | `/api/tenant-configuration/{id:long}` | `TenantConfigurationController.Update` | Permissions.TenantConfigManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/TenantConfigurationController.cs` |
| 534 | Tenant | GET | `/api/tenant-configuration/grouped` | `TenantConfigurationController.GetGrouped` | Permissions.TenantConfigView | [Authorize] on class | `ONEERP.ERP.API/Controllers/TenantConfigurationController.cs` |
| 535 | Tenant | GET | `/api/timezones` | `TimeZonesController.GetAll` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/TimeZonesController.cs` |
| 536 | Tenant | POST | `/api/timezones` | `TimeZonesController.Create` | Permissions.TimeZonesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/TimeZonesController.cs` |
| 537 | Tenant | DELETE | `/api/timezones/{id:int}` | `TimeZonesController.Delete` | Permissions.TimeZonesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/TimeZonesController.cs` |
| 538 | Tenant | GET | `/api/timezones/{id:int}` | `TimeZonesController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/TimeZonesController.cs` |
| 539 | Tenant | PUT | `/api/timezones/{id:int}` | `TimeZonesController.Update` | Permissions.TimeZonesManage | [Authorize] on class | `ONEERP.ERP.API/Controllers/TimeZonesController.cs` |
| 540 | Tenant | GET | `/api/unit-conversions` | `UnitConversionsController.GetPaged` | Permissions.UnitConversionsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 541 | Tenant | POST | `/api/unit-conversions` | `UnitConversionsController.Create` | Permissions.UnitConversionsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 542 | Tenant | DELETE | `/api/unit-conversions/{id:long}` | `UnitConversionsController.Delete` | Permissions.UnitConversionsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 543 | Tenant | GET | `/api/unit-conversions/{id:long}` | `UnitConversionsController.GetById` | Permissions.UnitConversionsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 544 | Tenant | PUT | `/api/unit-conversions/{id:long}` | `UnitConversionsController.Update` | Permissions.UnitConversionsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| 545 | Tenant | GET | `/api/units` | `ProductUnitsController.GetPaged` | Permissions.UnitsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 546 | Tenant | POST | `/api/units` | `ProductUnitsController.Create` | Permissions.UnitsCreate | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 547 | Tenant | DELETE | `/api/units/{id:int}` | `ProductUnitsController.Delete` | Permissions.UnitsDelete | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 548 | Tenant | GET | `/api/units/{id:int}` | `ProductUnitsController.GetById` | Permissions.UnitsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 549 | Tenant | PUT | `/api/units/{id:int}` | `ProductUnitsController.Update` | Permissions.UnitsEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 550 | Tenant | GET | `/api/units/next-code` | `ProductUnitsController.GetNextCode` | Permissions.UnitsView | [Authorize] on class | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| 551 | Tenant | POST | `/api/user-data-scope-overrides` | `UserDataScopeOverridesController.Create` | "user-data-scope-overrides.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 552 | Tenant | DELETE | `/api/user-data-scope-overrides/{id:int}` | `UserDataScopeOverridesController.Delete` | "user-data-scope-overrides.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 553 | Tenant | GET | `/api/user-data-scope-overrides/{id:int}` | `UserDataScopeOverridesController.GetById` | "user-data-scope-overrides.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 554 | Tenant | PUT | `/api/user-data-scope-overrides/{id:int}` | `UserDataScopeOverridesController.Update` | "user-data-scope-overrides.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 555 | Tenant | GET | `/api/user-data-scope-overrides/user/{userId:int}` | `UserDataScopeOverridesController.GetByUser` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 556 | Tenant | POST | `/api/user-data-scope-overrides/user/{userId:int}/replace` | `UserDataScopeOverridesController.Replace` | "user-data-scope-overrides.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 557 | Tenant | GET | `/api/user-data-scope-overrides/user/{userId:int}/selection` | `UserDataScopeOverridesController.GetSelection` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 558 | Tenant | POST | `/api/user-field-permissions` | `UserFieldPermissionsController.Set` | "user-field-permissions.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 559 | Tenant | DELETE | `/api/user-field-permissions/{id:int}` | `UserFieldPermissionsController.Delete` | "user-field-permissions.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 560 | Tenant | GET | `/api/user-field-permissions/user/{userId:int}` | `UserFieldPermissionsController.GetByUser` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 561 | Tenant | GET | `/api/user-field-permissions/user/{userId:int}/screen/{screenId:int}` | `UserFieldPermissionsController.GetByUserScreen` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 562 | Tenant | POST | `/api/user-permission-overrides` | `UserPermissionOverridesController.Create` | "user-permission-overrides.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 563 | Tenant | DELETE | `/api/user-permission-overrides/{id:int}` | `UserPermissionOverridesController.Delete` | "user-permission-overrides.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 564 | Tenant | PUT | `/api/user-permission-overrides/{id:int}` | `UserPermissionOverridesController.Update` | "user-permission-overrides.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 565 | Tenant | GET | `/api/user-permission-overrides/user/{userId:int}` | `UserPermissionOverridesController.GetByUser` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 566 | Tenant | POST | `/api/workflow-permissions` | `WorkflowPermissionsController.Set` | "workflow-permissions.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 567 | Tenant | DELETE | `/api/workflow-permissions/{id:int}` | `WorkflowPermissionsController.Delete` | "workflow-permissions.manage" | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 568 | Tenant | GET | `/api/workflow-permissions/role/{roleId:int}` | `WorkflowPermissionsController.GetByRole` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| 569 | Tenant | GET | `/current` | `CompaniesController.GetCurrent` | Permissions.CompaniesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/CompaniesController.cs` |
| 570 | Tenant | PUT | `/current` | `CompaniesController.UpdateCurrent` | Permissions.CompaniesEdit | [Authorize] on class | `ONEERP.ERP.API/Controllers/CompaniesController.cs` |
| 571 | Tenant | GET | `/my` | `RolesController.GetMyRoles` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.ERP.API/Controllers/RolesController.cs` |
| 572 | Tenant | GET | `/next-code` | `CompaniesController.GetNextCode` | Permissions.CompaniesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/CompaniesController.cs` |
| 573 | Tenant | GET | `/permissions` | `RolesController.GetAllPermissions` | Permissions.RolesView | [Authorize] on class | `ONEERP.ERP.API/Controllers/RolesController.cs` |
| 574 | Platform | GET | `/` | `DashboardController.GetStats` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/DashboardController.cs` |
| 575 | Platform | GET | `/` | `PlansController.GetPaged` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/PlansController.cs` |
| 576 | Platform | GET | `/` | `SubscriptionsController.GetPaged` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/SubscriptionsController.cs` |
| 577 | Platform | GET | `/` | `TenantsController.GetPaged` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/TenantsController.cs` |
| 578 | Platform | POST | `/` | `PlansController.Create` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/PlansController.cs` |
| 579 | Platform | POST | `/` | `SubscriptionsController.Create` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/SubscriptionsController.cs` |
| 580 | Platform | POST | `/` | `TenantsController.Create` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/TenantsController.cs` |
| 581 | Platform | DELETE | `/{id:int}` | `PlansController.Delete` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/PlansController.cs` |
| 582 | Platform | DELETE | `/{id:int}` | `SubscriptionsController.Delete` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/SubscriptionsController.cs` |
| 583 | Platform | DELETE | `/{id:int}` | `TenantsController.Delete` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/TenantsController.cs` |
| 584 | Platform | GET | `/{id:int}` | `PlansController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/PlansController.cs` |
| 585 | Platform | GET | `/{id:int}` | `SubscriptionsController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/SubscriptionsController.cs` |
| 586 | Platform | GET | `/{id:int}` | `TenantsController.GetById` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/TenantsController.cs` |
| 587 | Platform | PUT | `/{id:int}` | `PlansController.Update` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/PlansController.cs` |
| 588 | Platform | PUT | `/{id:int}` | `SubscriptionsController.Update` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/SubscriptionsController.cs` |
| 589 | Platform | PUT | `/{id:int}` | `TenantsController.Update` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/TenantsController.cs` |
| 590 | Platform | GET | `/{id:int}/connections` | `TenantsController.GetConnections` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/TenantsController.cs` |
| 591 | Platform | PUT | `/{id:int}/status` | `TenantsController.UpdateStatus` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/TenantsController.cs` |
| 592 | Platform | GET | `/active` | `PlansController.GetActive` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/PlansController.cs` |
| 593 | Platform | GET | `/active` | `TenantsController.GetActive` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/TenantsController.cs` |
| 594 | Platform | POST | `/api/auth/login` | `AuthController.Login` | None (authentication only per global filter) | Not found on class | `ONEERP.Platform.API/Controllers/AuthController.cs` |
| 595 | Platform | POST | `/api/auth/logout` | `AuthController.Logout` | None (authentication only per global filter) | Not found on class | `ONEERP.Platform.API/Controllers/AuthController.cs` |
| 596 | Platform | POST | `/api/auth/refresh` | `AuthController.Refresh` | None (authentication only per global filter) | Not found on class | `ONEERP.Platform.API/Controllers/AuthController.cs` |
| 597 | Platform | GET | `/api/tenants/{tenantId:int}/migration-status` | `TenantMigrationsController.GetStatus` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/TenantMigrationsController.cs` |
| 598 | Platform | GET | `/api/tenants/{tenantId:int}/migration/history` | `TenantMigrationsController.GetHistory` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/TenantMigrationsController.cs` |
| 599 | Platform | POST | `/api/tenants/{tenantId:int}/migration/run` | `TenantMigrationsController.Run` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/TenantMigrationsController.cs` |
| 600 | Platform | GET | `/tenant/{tenantId:int}` | `SubscriptionsController.GetByTenant` | None (authentication only per global filter) | [Authorize] on class | `ONEERP.Platform.API/Controllers/SubscriptionsController.cs` |

## 8. Backend Controller List

| App | Controller | Route | Class authorization | Endpoint count | File |
|---|---|---|---|---:|---|
| Tenant | `ActionsController` | `api/actions` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Tenant | `AddressTypesController` | `api/address-types` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/AddressTypesController.cs` |
| Tenant | `AdministrationController` | `api/Administration` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/AdministrationController.cs` |
| Tenant | `AuthController` | `api/auth` | Not found | 3 | `ONEERP.ERP.API/Controllers/AuthController.cs` |
| Tenant | `BarcodesController` | `api/barcodes` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| Tenant | `BaseController` | `api/[controller]` | Not found | 0 | `ONEERP.ERP.API/Controllers/BaseController.cs` |
| Tenant | `BusinessPartnerRolesController` | `api/business-partner-roles` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| Tenant | `BusinessPartnersController` | `api/business-partners` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| Tenant | `BusinessTypesController` | `api/business-types` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| Tenant | `CitiesController` | `api/cities` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| Tenant | `CompaniesController` | `UNKNOWN` | [Authorize] | 8 | `ONEERP.ERP.API/Controllers/CompaniesController.cs` |
| Tenant | `CompanyGroupsController` | `api/company-groups` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| Tenant | `ContactTypesController` | `api/contact-types` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/ContactTypesController.cs` |
| Tenant | `CounterAssignmentsController` | `api/counter-assignments` | [Authorize] | 8 | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| Tenant | `CountersController` | `api/counters` | [Authorize] | 8 | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| Tenant | `CountriesController` | `api/countries` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| Tenant | `CouponsController` | `api/coupons` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| Tenant | `DashboardController` | `UNKNOWN` | [Authorize] | 1 | `ONEERP.ERP.API/Controllers/DashboardController.cs` |
| Tenant | `DataScopesController` | `api/data-scopes` | [Authorize] | 8 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Tenant | `DiscountRulesController` | `api/discount-rules` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| Tenant | `DocumentCategoriesController` | `api/document/master/categories` | [Authorize] | 0 | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| Tenant | `DocumentComponentsController` | `api/document/master/components` | [Authorize] | 0 | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| Tenant | `DocumentFontsController` | `api/document/master/fonts` | [Authorize] | 0 | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| Tenant | `DocumentLookupsController` | `api/document-lookups` | [Authorize] | 10 | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| Tenant | `DocumentMasterTypesController` | `api/document/master/types` | [Authorize] | 0 | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| Tenant | `DocumentOrientationsController` | `api/document/master/orientations` | [Authorize] | 0 | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| Tenant | `DocumentPaperSizesController` | `api/document/master/paper-sizes` | [Authorize] | 0 | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| Tenant | `DocumentPrinterModelsController` | `api/document/master/printer-models` | [Authorize] | 0 | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| Tenant | `DocumentPrinterTypesController` | `api/document/master/printer-types` | [Authorize] | 0 | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| Tenant | `DocumentSettingsController` | `api/document-settings` | [Authorize] | 4 | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| Tenant | `DocumentTemplatesController` | `api/document-templates` | [Authorize] | 23 | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| Tenant | `DocumentTypesController` | `api/document-types` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/DocumentTypesController.cs` |
| Tenant | `DocumentUnitsController` | `api/document/master/units` | [Authorize] | 0 | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| Tenant | `DocumentVariablesController` | `api/document/master/variables` | [Authorize] | 0 | `ONEERP.ERP.API/Controllers/DocumentControllers.cs` |
| Tenant | `DomainsController` | `api/[controller]` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Tenant | `EntitiesController` | `UNKNOWN` | [Authorize] | 7 | `ONEERP.ERP.API/Controllers/EntitiesController.cs` |
| Tenant | `FieldPermissionsController` | `api/[controller]` | Not found | 3 | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| Tenant | `FieldsController` | `api/[controller]` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Tenant | `FinancialYearsController` | `api/financial-years` | [Authorize] | 7 | `ONEERP.ERP.API/Controllers/FinancialYearController.cs` |
| Tenant | `GstRegistrationTypesController` | `api/gst-registration-types` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/GstRegistrationTypesController.cs` |
| Tenant | `HsnSacsController` | `api/hsn-sacs` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| Tenant | `ImportLogsController` | `api/import-logs` | [Authorize] | 1 | `ONEERP.ERP.API/Controllers/ImportLogsController.cs` |
| Tenant | `IndustryTypesController` | `api/industry-types` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| Tenant | `InventoryController` | `api/inventory` | [Authorize] | 14 | `ONEERP.ERP.API/Controllers/InventoryController.cs` |
| Tenant | `LanguagesController` | `api/languages` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/LanguagesController.cs` |
| Tenant | `MasterImportController` | `api/import` | [Authorize] | 8 | `ONEERP.ERP.API/Controllers/MasterImportController.cs` |
| Tenant | `ModulePermissionsController` | `api/[controller]` | Not found | 5 | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| Tenant | `ModulesController` | `api/[controller]` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Tenant | `NavigationController` | `api/[controller]` | [Authorize] | 1 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Tenant | `OfferDetailsController` | `api/offer-details` | [Authorize] | 4 | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| Tenant | `OffersController` | `api/offers` | [Authorize] | 8 | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| Tenant | `OperatorsController` | `api/operators` | [Authorize] | 8 | `ONEERP.ERP.API/Controllers/OperatorsAndCounterAssignmentsController.cs` |
| Tenant | `OperatorTypesController` | `api/operator-types` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| Tenant | `OrganizationController` | `api/organization` | [Authorize] | 33 | `ONEERP.ERP.API/Controllers/OrganizationController.cs` |
| Tenant | `OrganizationTypesController` | `api/organization-types` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/OrganizationTypesController.cs` |
| Tenant | `PaymentMethodDetailsController` | `api/payment-method-details` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| Tenant | `PaymentMethodsController` | `api/payment-methods` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| Tenant | `PaymentsController` | `api/payments` | [Authorize] | 7 | `ONEERP.ERP.API/Controllers/PaymentsController.cs` |
| Tenant | `PaymentTypesController` | `api/payment-types` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/PaymentTypesController.cs` |
| Tenant | `PermissionActionsController` | `api/[controller]` | Not found | 5 | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| Tenant | `PermissionModulesController` | `api/[controller]` | Not found | 6 | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| Tenant | `PermissionsController` | `api/permissions` | [Authorize] | 1 | `ONEERP.ERP.API/Controllers/PermissionsController.cs` |
| Tenant | `POSOperationsController` | `api/pos` | [Authorize] | 10 | `ONEERP.ERP.API/Controllers/POSOperationsController.cs` |
| Tenant | `POSSessionsController` | `api/pos-sessions` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| Tenant | `PriceListDetailsController` | `api/price-list-details` | [Authorize] | 4 | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| Tenant | `PriceListPriceTypesController` | `api/price-lists/{priceListId:long}/price-types` | [Authorize] | 4 | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| Tenant | `PriceListsController` | `api/price-lists` | [Authorize] | 8 | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| Tenant | `PriceTypesController` | `api/price-types` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| Tenant | `ProductBrandsController` | `api/brands` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| Tenant | `ProductCategoriesController` | `api/product-categories` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| Tenant | `ProductsController` | `api/products` | [Authorize] | 7 | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| Tenant | `ProductSubCategoriesController` | `api/product-subcategories` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| Tenant | `ProductUnitsController` | `api/units` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/ProductMasterControllers.cs` |
| Tenant | `ProfileController` | `api/profile` | [Authorize] | 2 | `ONEERP.ERP.API/Controllers/ProfileController.cs` |
| Tenant | `PurchaseReportsController` | `api/purchase-reports` | [Authorize] | 1 | `ONEERP.ERP.API/Controllers/PurchaseReportsController.cs` |
| Tenant | `PurchaseReturnsController` | `api/purchase-returns` | [Authorize] | 8 | `ONEERP.ERP.API/Controllers/PurchaseReturnsController.cs` |
| Tenant | `PurchasesController` | `api/purchases` | [Authorize] | 12 | `ONEERP.ERP.API/Controllers/PurchasesController.cs` |
| Tenant | `RefundsController` | `api/payments/refunds` | [Authorize] | 2 | `ONEERP.ERP.API/Controllers/SalesReturnsController.cs` |
| Tenant | `RoleFieldPermissionsController` | `api/role-field-permissions` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Tenant | `RolePermissionsController` | `api/role-permissions` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Tenant | `RolesController` | `UNKNOWN` | [Authorize] | 8 | `ONEERP.ERP.API/Controllers/RolesController.cs` |
| Tenant | `SaleEntryContextController` | `api/sales` | [Authorize] | 2 | `ONEERP.ERP.API/Controllers/SaleEntryContextController.cs` |
| Tenant | `SalesController` | `api/sales` | [Authorize] | 16 | `ONEERP.ERP.API/Controllers/SalesController.cs` |
| Tenant | `SalesReportsController` | `api/sales-reports` | [Authorize] | 1 | `ONEERP.ERP.API/Controllers/SalesReportsController.cs` |
| Tenant | `SalesReturnsController` | `api/sales-returns` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/SalesReturnsController.cs` |
| Tenant | `ScreensController` | `api/[controller]` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Tenant | `ServiceCategoriesController` | `api/service-categories` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| Tenant | `ServicesController` | `api/services` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| Tenant | `SettingsController` | `UNKNOWN` | [Authorize] | 2 | `ONEERP.ERP.API/Controllers/SettingsController.cs` |
| Tenant | `SourcesController` | `api/sources` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| Tenant | `StatesController` | `api/states` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/LocationsController.cs` |
| Tenant | `StockController` | `api/stock` | [Authorize] | 3 | `ONEERP.ERP.API/Controllers/StockController.cs` |
| Tenant | `StoresController` | `api/stores` | [Authorize] | 7 | `ONEERP.ERP.API/Controllers/StoreControllers.cs` |
| Tenant | `StoreTypesController` | `api/store-types` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/MasterDataController.cs` |
| Tenant | `SubModulesController` | `api/[controller]` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Tenant | `TaxesController` | `api/taxes` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/TaxMasterControllers.cs` |
| Tenant | `TaxTypeSystemsController` | `api/tax-type-systems` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/TaxMasterControllers.cs` |
| Tenant | `TenantConfigurationController` | `api/tenant-configuration` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/TenantConfigurationController.cs` |
| Tenant | `TimeZonesController` | `api/timezones` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/TimeZonesController.cs` |
| Tenant | `UnitConversionsController` | `api/unit-conversions` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/BillingMasterControllers.cs` |
| Tenant | `UserDataScopeOverridesController` | `api/user-data-scope-overrides` | [Authorize] | 7 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Tenant | `UserFieldPermissionsController` | `api/user-field-permissions` | [Authorize] | 4 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Tenant | `UserPermissionOverridesController` | `api/user-permission-overrides` | [Authorize] | 4 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Tenant | `UserPermissionsController` | `api/permission` | Not found | 1 | `ONEERP.ERP.API/Controllers/PermissionController.cs` |
| Tenant | `UsersController` | `UNKNOWN` | [Authorize] | 6 | `ONEERP.ERP.API/Controllers/UsersController.cs` |
| Tenant | `WorkflowPermissionsController` | `api/workflow-permissions` | [Authorize] | 3 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Tenant | `WorkspacesController` | `api/[controller]` | [Authorize] | 5 | `ONEERP.ERP.API/Controllers/EnterprisePermissionControllers.cs` |
| Platform | `AuthController` | `api/auth` | Not found | 3 | `ONEERP.Platform.API/Controllers/AuthController.cs` |
| Platform | `BaseController` | `api/[controller]` | Not found | 0 | `ONEERP.Platform.API/Controllers/BaseController.cs` |
| Platform | `DashboardController` | `UNKNOWN` | [Authorize] | 1 | `ONEERP.Platform.API/Controllers/DashboardController.cs` |
| Platform | `PlansController` | `UNKNOWN` | [Authorize] | 6 | `ONEERP.Platform.API/Controllers/PlansController.cs` |
| Platform | `SubscriptionsController` | `UNKNOWN` | [Authorize] | 6 | `ONEERP.Platform.API/Controllers/SubscriptionsController.cs` |
| Platform | `TenantMigrationsController` | `api/tenants` | [Authorize] | 3 | `ONEERP.Platform.API/Controllers/TenantMigrationsController.cs` |
| Platform | `TenantsController` | `UNKNOWN` | [Authorize] | 8 | `ONEERP.Platform.API/Controllers/TenantsController.cs` |

## 9. Backend Service List

| App | Service class | File |
|---|---|---|
| Tenant API | `AddressTypeService` | `ONEERP.ERP.API/Services/AddressTypeService.cs` |
| Tenant API | `AdministrationService` | `ONEERP.ERP.API/Services/AdministrationService.cs` |
| Tenant API | `AuditService` | `ONEERP.ERP.API/Services/AuditService.cs` |
| Tenant API | `AuthService` | `ONEERP.ERP.API/Services/AuthService.cs` |
| Tenant API | `BarcodeService` | `ONEERP.ERP.API/Services/BillingMasterServices.cs` |
| Tenant API | `CouponService` | `ONEERP.ERP.API/Services/BillingMasterServices.cs` |
| Tenant API | `DiscountRuleService` | `ONEERP.ERP.API/Services/BillingMasterServices.cs` |
| Tenant API | `HsnSacService` | `ONEERP.ERP.API/Services/BillingMasterServices.cs` |
| Tenant API | `OfferDetailService` | `ONEERP.ERP.API/Services/BillingMasterServices.cs` |
| Tenant API | `OfferService` | `ONEERP.ERP.API/Services/BillingMasterServices.cs` |
| Tenant API | `PriceListDetailService` | `ONEERP.ERP.API/Services/BillingMasterServices.cs` |
| Tenant API | `PriceListPriceTypeService` | `ONEERP.ERP.API/Services/BillingMasterServices.cs` |
| Tenant API | `PriceListService` | `ONEERP.ERP.API/Services/BillingMasterServices.cs` |
| Tenant API | `PriceTypeService` | `ONEERP.ERP.API/Services/BillingMasterServices.cs` |
| Tenant API | `ServiceCategoryService` | `ONEERP.ERP.API/Services/BillingMasterServices.cs` |
| Tenant API | `ServiceService` | `ONEERP.ERP.API/Services/BillingMasterServices.cs` |
| Tenant API | `UnitConversionService` | `ONEERP.ERP.API/Services/BillingMasterServices.cs` |
| Tenant API | `PurchaseReturnService` | `ONEERP.ERP.API/Services/BillingServices.cs` |
| Tenant API | `StockService` | `ONEERP.ERP.API/Services/BillingServices.cs` |
| Tenant API | `CompanyService` | `ONEERP.ERP.API/Services/CompanyService.cs` |
| Tenant API | `ContactTypeService` | `ONEERP.ERP.API/Services/ContactTypeService.cs` |
| Tenant API | `CurrentUser` | `ONEERP.ERP.API/Services/CurrentUser.cs` |
| Tenant API | `DashboardService` | `ONEERP.ERP.API/Services/DashboardService.cs` |
| Tenant API | `DataScopeResolver` | `ONEERP.ERP.API/Services/DataScopeResolver.cs` |
| Tenant API | `DocumentDesignService` | `ONEERP.ERP.API/Services/DocumentDesignService.cs` |
| Tenant API | `DocumentMasterService` | `ONEERP.ERP.API/Services/DocumentMasterService.cs` |
| Tenant API | `StringExt` | `ONEERP.ERP.API/Services/DocumentMasterService.cs` |
| Tenant API | `DocumentPreviewHtml` | `ONEERP.ERP.API/Services/DocumentPreviewHtml.cs` |
| Tenant API | `DocumentSettingService` | `ONEERP.ERP.API/Services/DocumentSettingService.cs` |
| Tenant API | `DocumentTypeService` | `ONEERP.ERP.API/Services/DocumentTypeService.cs` |
| Tenant API | `ActionService` | `ONEERP.ERP.API/Services/EnterprisePermissionServices.cs` |
| Tenant API | `DataScopeService` | `ONEERP.ERP.API/Services/EnterprisePermissionServices.cs` |
| Tenant API | `DomainService` | `ONEERP.ERP.API/Services/EnterprisePermissionServices.cs` |
| Tenant API | `FieldService` | `ONEERP.ERP.API/Services/EnterprisePermissionServices.cs` |
| Tenant API | `ModuleService` | `ONEERP.ERP.API/Services/EnterprisePermissionServices.cs` |
| Tenant API | `RoleFieldPermissionEntryService` | `ONEERP.ERP.API/Services/EnterprisePermissionServices.cs` |
| Tenant API | `RolePermissionEntryService` | `ONEERP.ERP.API/Services/EnterprisePermissionServices.cs` |
| Tenant API | `ScreenService` | `ONEERP.ERP.API/Services/EnterprisePermissionServices.cs` |
| Tenant API | `SubModuleService` | `ONEERP.ERP.API/Services/EnterprisePermissionServices.cs` |
| Tenant API | `UserDataScopeOverrideService` | `ONEERP.ERP.API/Services/EnterprisePermissionServices.cs` |
| Tenant API | `UserFieldPermissionEntryService` | `ONEERP.ERP.API/Services/EnterprisePermissionServices.cs` |
| Tenant API | `UserPermissionOverrideService` | `ONEERP.ERP.API/Services/EnterprisePermissionServices.cs` |
| Tenant API | `WorkflowPermissionEntryService` | `ONEERP.ERP.API/Services/EnterprisePermissionServices.cs` |
| Tenant API | `WorkspaceService` | `ONEERP.ERP.API/Services/EnterprisePermissionServices.cs` |
| Tenant API | `EntityService` | `ONEERP.ERP.API/Services/EntityService.cs` |
| Tenant API | `FinancialYearService` | `ONEERP.ERP.API/Services/FinancialYearService.cs` |
| Tenant API | `GstRegistrationTypeService` | `ONEERP.ERP.API/Services/GstRegistrationTypeService.cs` |
| Tenant API | `IdempotencyService` | `ONEERP.ERP.API/Services/IdempotencyService.cs` |
| Tenant API | `ImportLogService` | `ONEERP.ERP.API/Services/ImportLogService.cs` |
| Tenant API | `InventoryService` | `ONEERP.ERP.API/Services/InventoryService.cs` |
| Tenant API | `LanguageService` | `ONEERP.ERP.API/Services/LanguageService.cs` |
| Tenant API | `CityService` | `ONEERP.ERP.API/Services/LocationService.cs` |
| Tenant API | `CountryService` | `ONEERP.ERP.API/Services/LocationService.cs` |
| Tenant API | `StateService` | `ONEERP.ERP.API/Services/LocationService.cs` |
| Tenant API | `BusinessPartnerRoleService` | `ONEERP.ERP.API/Services/MasterDataService.cs` |
| Tenant API | `BusinessPartnerService` | `ONEERP.ERP.API/Services/MasterDataService.cs` |
| Tenant API | `BusinessTypeService` | `ONEERP.ERP.API/Services/MasterDataService.cs` |
| Tenant API | `CompanyGroupService` | `ONEERP.ERP.API/Services/MasterDataService.cs` |
| Tenant API | `IndustryTypeService` | `ONEERP.ERP.API/Services/MasterDataService.cs` |
| Tenant API | `OperatorTypeService` | `ONEERP.ERP.API/Services/MasterDataService.cs` |
| Tenant API | `SourceService` | `ONEERP.ERP.API/Services/MasterDataService.cs` |
| Tenant API | `StoreTypeService` | `ONEERP.ERP.API/Services/MasterDataService.cs` |
| Tenant API | `MasterExportService` | `ONEERP.ERP.API/Services/MasterExportService.cs` |
| Tenant API | `MasterImportService` | `ONEERP.ERP.API/Services/MasterImportService.cs` |
| Tenant API | `MasterReferenceCatalog` | `ONEERP.ERP.API/Services/MasterReferenceCatalog.cs` |
| Tenant API | `RefLookup` | `ONEERP.ERP.API/Services/MasterReferenceCatalog.cs` |
| Tenant API | `NavigationService` | `ONEERP.ERP.API/Services/NavigationService.cs` |
| Tenant API | `CounterAssignmentService` | `ONEERP.ERP.API/Services/OperatorAndCounterAssignmentServices.cs` |
| Tenant API | `OperatorService` | `ONEERP.ERP.API/Services/OperatorAndCounterAssignmentServices.cs` |
| Tenant API | `BranchService` | `ONEERP.ERP.API/Services/OrganizationService.cs` |
| Tenant API | `BranchTypeService` | `ONEERP.ERP.API/Services/OrganizationService.cs` |
| Tenant API | `DepartmentService` | `ONEERP.ERP.API/Services/OrganizationService.cs` |
| Tenant API | `DesignationService` | `ONEERP.ERP.API/Services/OrganizationService.cs` |
| Tenant API | `EmployeeService` | `ONEERP.ERP.API/Services/OrganizationService.cs` |
| Tenant API | `EmploymentTypeService` | `ONEERP.ERP.API/Services/OrganizationService.cs` |
| Tenant API | `WarehouseService` | `ONEERP.ERP.API/Services/OrganizationService.cs` |
| Tenant API | `WarehouseTypeService` | `ONEERP.ERP.API/Services/OrganizationService.cs` |
| Tenant API | `OrganizationTypeService` | `ONEERP.ERP.API/Services/OrganizationTypeService.cs` |
| Tenant API | `PaymentMethodDetailService` | `ONEERP.ERP.API/Services/PaymentServices.cs` |
| Tenant API | `PaymentMethodService` | `ONEERP.ERP.API/Services/PaymentServices.cs` |
| Tenant API | `PaymentService` | `ONEERP.ERP.API/Services/PaymentServices.cs` |
| Tenant API | `PaymentTypeService` | `ONEERP.ERP.API/Services/PaymentServices.cs` |
| Tenant API | `FieldPermissionService` | `ONEERP.ERP.API/Services/PermissionService.cs` |
| Tenant API | `ModulePermissionService` | `ONEERP.ERP.API/Services/PermissionService.cs` |
| Tenant API | `PermissionActionService` | `ONEERP.ERP.API/Services/PermissionService.cs` |
| Tenant API | `PermissionModuleService` | `ONEERP.ERP.API/Services/PermissionService.cs` |
| Tenant API | `POSOperationsService` | `ONEERP.ERP.API/Services/POSOperationsService.cs` |
| Tenant API | `ProductBrandService` | `ONEERP.ERP.API/Services/ProductMasterServices.cs` |
| Tenant API | `ProductCategoryService` | `ONEERP.ERP.API/Services/ProductMasterServices.cs` |
| Tenant API | `ProductService` | `ONEERP.ERP.API/Services/ProductMasterServices.cs` |
| Tenant API | `ProductSubCategoryService` | `ONEERP.ERP.API/Services/ProductMasterServices.cs` |
| Tenant API | `ProductUnitService` | `ONEERP.ERP.API/Services/ProductMasterServices.cs` |
| Tenant API | `ProfileService` | `ONEERP.ERP.API/Services/ProfileService.cs` |
| Tenant API | `PurchaseReportService` | `ONEERP.ERP.API/Services/PurchaseReportService.cs` |
| Tenant API | `PurchaseService` | `ONEERP.ERP.API/Services/PurchaseService.cs` |
| Tenant API | `RoleService` | `ONEERP.ERP.API/Services/RoleService.cs` |
| Tenant API | `SaleEntryContextService` | `ONEERP.ERP.API/Services/SaleEntryContextService.cs` |
| Tenant API | `SalesReportService` | `ONEERP.ERP.API/Services/SalesReportService.cs` |
| Tenant API | `SalesReturnService` | `ONEERP.ERP.API/Services/SalesReturnService.cs` |
| Tenant API | `SalesService` | `ONEERP.ERP.API/Services/SalesService.cs` |
| Tenant API | `SettingsService` | `ONEERP.ERP.API/Services/SettingsService.cs` |
| Tenant API | `CounterService` | `ONEERP.ERP.API/Services/StoreService.cs` |
| Tenant API | `POSSessionService` | `ONEERP.ERP.API/Services/StoreService.cs` |
| Tenant API | `StoreService` | `ONEERP.ERP.API/Services/StoreService.cs` |
| Tenant API | `TaxService` | `ONEERP.ERP.API/Services/TaxMasterServices.cs` |
| Tenant API | `TaxTypeSystemService` | `ONEERP.ERP.API/Services/TaxMasterServices.cs` |
| Tenant API | `TenantConfigurationService` | `ONEERP.ERP.API/Services/TenantConfigurationService.cs` |
| Tenant API | `TimeZoneService` | `ONEERP.ERP.API/Services/TimeZoneService.cs` |
| Tenant API | `TokenService` | `ONEERP.ERP.API/Services/TokenService.cs` |
| Tenant API | `UserService` | `ONEERP.ERP.API/Services/UserService.cs` |
| Platform API | `AuditService` | `ONEERP.Platform.API/Services/AuditService.cs` |
| Platform API | `CurrentUser` | `ONEERP.Platform.API/Services/CurrentUser.cs` |
| Platform API | `DashboardService` | `ONEERP.Platform.API/Services/DashboardService.cs` |
| Platform API | `MigrationPlanner` | `ONEERP.Platform.API/Services/MigrationPlanner.cs` |
| Platform API | `PlanService` | `ONEERP.Platform.API/Services/PlanService.cs` |
| Platform API | `PlatformAuthService` | `ONEERP.Platform.API/Services/PlatformAuthService.cs` |
| Platform API | `PlatformPermissionResolver` | `ONEERP.Platform.API/Services/PlatformPermissionResolver.cs` |
| Platform API | `SubscriptionService` | `ONEERP.Platform.API/Services/SubscriptionService.cs` |
| Platform API | `TenantDatabaseResolver` | `ONEERP.Platform.API/Services/TenantDatabaseResolver.cs` |
| Platform API | `MigrationScriptGuard` | `ONEERP.Platform.API/Services/TenantMigrationExecutor.cs` |
| Platform API | `TenantMigrationExecutor` | `ONEERP.Platform.API/Services/TenantMigrationExecutor.cs` |
| Platform API | `TenantMigrationService` | `ONEERP.Platform.API/Services/TenantMigrationService.cs` |
| Platform API | `TenantService` | `ONEERP.Platform.API/Services/TenantService.cs` |
| Platform API | `TokenService` | `ONEERP.Platform.API/Services/TokenService.cs` |

## 10. Repository / Query List

Table names are lexical `dbo.*` references per repository source file; they do not prove each method accesses every listed table.

| App | Repository class | File | SQL table names mentioned |
|---|---|---|---|
| Tenant API | `AddressTypeRepository` | `ONEERP.ERP.API/Repositories/AddressTypeRepository.cs` | `AddressTypes` |
| Tenant API | `AdministrationRepository` | `ONEERP.ERP.API/Repositories/AdministrationRepository.cs` | `Currencies` |
| Tenant API | `ApplicationSettingRepository` | `ONEERP.ERP.API/Repositories/ApplicationSettingRepository.cs` | `ApplicationSettings` |
| Tenant API | `BarcodeRepository` | `ONEERP.ERP.API/Repositories/BillingRepositories.cs` | `Barcodes`, `Coupons`, `Currencies`, `DiscountRules`, `HsnSacs`, `OfferDetails`, `Offers`, `PriceListDetails`, `PriceListPriceTypes`, `PriceLists`, `PriceTypes`, `ProductCategories`, `Products`, `ServiceCategories`, `Services`, `UnitConversions`, `Units` |
| Tenant API | `CouponRepository` | `ONEERP.ERP.API/Repositories/BillingRepositories.cs` | `Barcodes`, `Coupons`, `Currencies`, `DiscountRules`, `HsnSacs`, `OfferDetails`, `Offers`, `PriceListDetails`, `PriceListPriceTypes`, `PriceLists`, `PriceTypes`, `ProductCategories`, `Products`, `ServiceCategories`, `Services`, `UnitConversions`, `Units` |
| Tenant API | `DiscountRuleRepository` | `ONEERP.ERP.API/Repositories/BillingRepositories.cs` | `Barcodes`, `Coupons`, `Currencies`, `DiscountRules`, `HsnSacs`, `OfferDetails`, `Offers`, `PriceListDetails`, `PriceListPriceTypes`, `PriceLists`, `PriceTypes`, `ProductCategories`, `Products`, `ServiceCategories`, `Services`, `UnitConversions`, `Units` |
| Tenant API | `HsnSacRepository` | `ONEERP.ERP.API/Repositories/BillingRepositories.cs` | `Barcodes`, `Coupons`, `Currencies`, `DiscountRules`, `HsnSacs`, `OfferDetails`, `Offers`, `PriceListDetails`, `PriceListPriceTypes`, `PriceLists`, `PriceTypes`, `ProductCategories`, `Products`, `ServiceCategories`, `Services`, `UnitConversions`, `Units` |
| Tenant API | `OfferDetailRepository` | `ONEERP.ERP.API/Repositories/BillingRepositories.cs` | `Barcodes`, `Coupons`, `Currencies`, `DiscountRules`, `HsnSacs`, `OfferDetails`, `Offers`, `PriceListDetails`, `PriceListPriceTypes`, `PriceLists`, `PriceTypes`, `ProductCategories`, `Products`, `ServiceCategories`, `Services`, `UnitConversions`, `Units` |
| Tenant API | `OfferRepository` | `ONEERP.ERP.API/Repositories/BillingRepositories.cs` | `Barcodes`, `Coupons`, `Currencies`, `DiscountRules`, `HsnSacs`, `OfferDetails`, `Offers`, `PriceListDetails`, `PriceListPriceTypes`, `PriceLists`, `PriceTypes`, `ProductCategories`, `Products`, `ServiceCategories`, `Services`, `UnitConversions`, `Units` |
| Tenant API | `PriceListDetailRepository` | `ONEERP.ERP.API/Repositories/BillingRepositories.cs` | `Barcodes`, `Coupons`, `Currencies`, `DiscountRules`, `HsnSacs`, `OfferDetails`, `Offers`, `PriceListDetails`, `PriceListPriceTypes`, `PriceLists`, `PriceTypes`, `ProductCategories`, `Products`, `ServiceCategories`, `Services`, `UnitConversions`, `Units` |
| Tenant API | `PriceListPriceTypeRepository` | `ONEERP.ERP.API/Repositories/BillingRepositories.cs` | `Barcodes`, `Coupons`, `Currencies`, `DiscountRules`, `HsnSacs`, `OfferDetails`, `Offers`, `PriceListDetails`, `PriceListPriceTypes`, `PriceLists`, `PriceTypes`, `ProductCategories`, `Products`, `ServiceCategories`, `Services`, `UnitConversions`, `Units` |
| Tenant API | `PriceListRepository` | `ONEERP.ERP.API/Repositories/BillingRepositories.cs` | `Barcodes`, `Coupons`, `Currencies`, `DiscountRules`, `HsnSacs`, `OfferDetails`, `Offers`, `PriceListDetails`, `PriceListPriceTypes`, `PriceLists`, `PriceTypes`, `ProductCategories`, `Products`, `ServiceCategories`, `Services`, `UnitConversions`, `Units` |
| Tenant API | `PriceTypeRepository` | `ONEERP.ERP.API/Repositories/BillingRepositories.cs` | `Barcodes`, `Coupons`, `Currencies`, `DiscountRules`, `HsnSacs`, `OfferDetails`, `Offers`, `PriceListDetails`, `PriceListPriceTypes`, `PriceLists`, `PriceTypes`, `ProductCategories`, `Products`, `ServiceCategories`, `Services`, `UnitConversions`, `Units` |
| Tenant API | `ServiceCategoryRepository` | `ONEERP.ERP.API/Repositories/BillingRepositories.cs` | `Barcodes`, `Coupons`, `Currencies`, `DiscountRules`, `HsnSacs`, `OfferDetails`, `Offers`, `PriceListDetails`, `PriceListPriceTypes`, `PriceLists`, `PriceTypes`, `ProductCategories`, `Products`, `ServiceCategories`, `Services`, `UnitConversions`, `Units` |
| Tenant API | `ServiceRepository` | `ONEERP.ERP.API/Repositories/BillingRepositories.cs` | `Barcodes`, `Coupons`, `Currencies`, `DiscountRules`, `HsnSacs`, `OfferDetails`, `Offers`, `PriceListDetails`, `PriceListPriceTypes`, `PriceLists`, `PriceTypes`, `ProductCategories`, `Products`, `ServiceCategories`, `Services`, `UnitConversions`, `Units` |
| Tenant API | `UnitConversionRepository` | `ONEERP.ERP.API/Repositories/BillingRepositories.cs` | `Barcodes`, `Coupons`, `Currencies`, `DiscountRules`, `HsnSacs`, `OfferDetails`, `Offers`, `PriceListDetails`, `PriceListPriceTypes`, `PriceLists`, `PriceTypes`, `ProductCategories`, `Products`, `ServiceCategories`, `Services`, `UnitConversions`, `Units` |
| Tenant API | `CompanyRepository` | `ONEERP.ERP.API/Repositories/CompanyRepository.cs` | `Companies` |
| Tenant API | `ContactTypeRepository` | `ONEERP.ERP.API/Repositories/ContactTypeRepository.cs` | `ContactTypes` |
| Tenant API | `DocumentMasterGuardRepository` | `ONEERP.ERP.API/Repositories/DocumentMasterRepositories.cs` | `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplatePrinter`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `DocumentMasterRepositoryBase` | `ONEERP.ERP.API/Repositories/DocumentMasterRepositories.cs` | `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplatePrinter`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `DocumentPaperSizeRepository` | `ONEERP.ERP.API/Repositories/DocumentMasterRepositories.cs` | `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplatePrinter`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `DocumentPrinterModelRepository` | `ONEERP.ERP.API/Repositories/DocumentMasterRepositories.cs` | `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplatePrinter`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `DocumentPrinterTypeRepository` | `ONEERP.ERP.API/Repositories/DocumentMasterRepositories.cs` | `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplatePrinter`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `InvoiceFontRepository` | `ONEERP.ERP.API/Repositories/DocumentMasterRepositories.cs` | `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplatePrinter`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `InvoiceTypeRepository` | `ONEERP.ERP.API/Repositories/DocumentMasterRepositories.cs` | `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplatePrinter`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `PrintOrientationRepository` | `ONEERP.ERP.API/Repositories/DocumentMasterRepositories.cs` | `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplatePrinter`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `PrintUnitRepository` | `ONEERP.ERP.API/Repositories/DocumentMasterRepositories.cs` | `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplatePrinter`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `TemplateCategoryRepository` | `ONEERP.ERP.API/Repositories/DocumentMasterRepositories.cs` | `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplatePrinter`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `TemplateComponentRepository` | `ONEERP.ERP.API/Repositories/DocumentMasterRepositories.cs` | `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplatePrinter`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `TemplateVariableRepository` | `ONEERP.ERP.API/Repositories/DocumentMasterRepositories.cs` | `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplatePrinter`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `DocumentQueryRepository` | `ONEERP.ERP.API/Repositories/DocumentQueryRepository.cs` | `Companies`, `IndustryTypes`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplatePrintSetting`, `InvoiceTemplatePrinter`, `InvoiceTemplateVersion`, `InvoiceType`, `PrintOrientation`, `PrinterModel`, `PrinterType` |
| Tenant API | `DocumentTypeRepository` | `ONEERP.ERP.API/Repositories/DocumentTypeRepository.cs` | `DocumentTypes` |
| Tenant API | `ActionRepository` | `ONEERP.ERP.API/Repositories/EnterprisePermissionRepositories.cs` | `Actions`, `Domains`, `Fields`, `Modules`, `RoleDataScopes`, `RoleFieldPermissions`, `RolePermissions`, `Screens`, `SubModules`, `UserDataScopeOverrides`, `UserFieldPermissions`, `UserPermissionOverrides`, `WorkflowPermissions`, `Workspaces` |
| Tenant API | `DataScopeRepository` | `ONEERP.ERP.API/Repositories/EnterprisePermissionRepositories.cs` | `Actions`, `Domains`, `Fields`, `Modules`, `RoleDataScopes`, `RoleFieldPermissions`, `RolePermissions`, `Screens`, `SubModules`, `UserDataScopeOverrides`, `UserFieldPermissions`, `UserPermissionOverrides`, `WorkflowPermissions`, `Workspaces` |
| Tenant API | `DomainRepository` | `ONEERP.ERP.API/Repositories/EnterprisePermissionRepositories.cs` | `Actions`, `Domains`, `Fields`, `Modules`, `RoleDataScopes`, `RoleFieldPermissions`, `RolePermissions`, `Screens`, `SubModules`, `UserDataScopeOverrides`, `UserFieldPermissions`, `UserPermissionOverrides`, `WorkflowPermissions`, `Workspaces` |
| Tenant API | `FieldRepository` | `ONEERP.ERP.API/Repositories/EnterprisePermissionRepositories.cs` | `Actions`, `Domains`, `Fields`, `Modules`, `RoleDataScopes`, `RoleFieldPermissions`, `RolePermissions`, `Screens`, `SubModules`, `UserDataScopeOverrides`, `UserFieldPermissions`, `UserPermissionOverrides`, `WorkflowPermissions`, `Workspaces` |
| Tenant API | `ModuleRepository` | `ONEERP.ERP.API/Repositories/EnterprisePermissionRepositories.cs` | `Actions`, `Domains`, `Fields`, `Modules`, `RoleDataScopes`, `RoleFieldPermissions`, `RolePermissions`, `Screens`, `SubModules`, `UserDataScopeOverrides`, `UserFieldPermissions`, `UserPermissionOverrides`, `WorkflowPermissions`, `Workspaces` |
| Tenant API | `RoleFieldPermissionEntryRepository` | `ONEERP.ERP.API/Repositories/EnterprisePermissionRepositories.cs` | `Actions`, `Domains`, `Fields`, `Modules`, `RoleDataScopes`, `RoleFieldPermissions`, `RolePermissions`, `Screens`, `SubModules`, `UserDataScopeOverrides`, `UserFieldPermissions`, `UserPermissionOverrides`, `WorkflowPermissions`, `Workspaces` |
| Tenant API | `RolePermissionEntryRepository` | `ONEERP.ERP.API/Repositories/EnterprisePermissionRepositories.cs` | `Actions`, `Domains`, `Fields`, `Modules`, `RoleDataScopes`, `RoleFieldPermissions`, `RolePermissions`, `Screens`, `SubModules`, `UserDataScopeOverrides`, `UserFieldPermissions`, `UserPermissionOverrides`, `WorkflowPermissions`, `Workspaces` |
| Tenant API | `ScreenRepository` | `ONEERP.ERP.API/Repositories/EnterprisePermissionRepositories.cs` | `Actions`, `Domains`, `Fields`, `Modules`, `RoleDataScopes`, `RoleFieldPermissions`, `RolePermissions`, `Screens`, `SubModules`, `UserDataScopeOverrides`, `UserFieldPermissions`, `UserPermissionOverrides`, `WorkflowPermissions`, `Workspaces` |
| Tenant API | `SubModuleRepository` | `ONEERP.ERP.API/Repositories/EnterprisePermissionRepositories.cs` | `Actions`, `Domains`, `Fields`, `Modules`, `RoleDataScopes`, `RoleFieldPermissions`, `RolePermissions`, `Screens`, `SubModules`, `UserDataScopeOverrides`, `UserFieldPermissions`, `UserPermissionOverrides`, `WorkflowPermissions`, `Workspaces` |
| Tenant API | `UserDataScopeOverrideRepository` | `ONEERP.ERP.API/Repositories/EnterprisePermissionRepositories.cs` | `Actions`, `Domains`, `Fields`, `Modules`, `RoleDataScopes`, `RoleFieldPermissions`, `RolePermissions`, `Screens`, `SubModules`, `UserDataScopeOverrides`, `UserFieldPermissions`, `UserPermissionOverrides`, `WorkflowPermissions`, `Workspaces` |
| Tenant API | `UserFieldPermissionEntryRepository` | `ONEERP.ERP.API/Repositories/EnterprisePermissionRepositories.cs` | `Actions`, `Domains`, `Fields`, `Modules`, `RoleDataScopes`, `RoleFieldPermissions`, `RolePermissions`, `Screens`, `SubModules`, `UserDataScopeOverrides`, `UserFieldPermissions`, `UserPermissionOverrides`, `WorkflowPermissions`, `Workspaces` |
| Tenant API | `UserPermissionOverrideRepository` | `ONEERP.ERP.API/Repositories/EnterprisePermissionRepositories.cs` | `Actions`, `Domains`, `Fields`, `Modules`, `RoleDataScopes`, `RoleFieldPermissions`, `RolePermissions`, `Screens`, `SubModules`, `UserDataScopeOverrides`, `UserFieldPermissions`, `UserPermissionOverrides`, `WorkflowPermissions`, `Workspaces` |
| Tenant API | `WorkflowPermissionEntryRepository` | `ONEERP.ERP.API/Repositories/EnterprisePermissionRepositories.cs` | `Actions`, `Domains`, `Fields`, `Modules`, `RoleDataScopes`, `RoleFieldPermissions`, `RolePermissions`, `Screens`, `SubModules`, `UserDataScopeOverrides`, `UserFieldPermissions`, `UserPermissionOverrides`, `WorkflowPermissions`, `Workspaces` |
| Tenant API | `WorkspaceRepository` | `ONEERP.ERP.API/Repositories/EnterprisePermissionRepositories.cs` | `Actions`, `Domains`, `Fields`, `Modules`, `RoleDataScopes`, `RoleFieldPermissions`, `RolePermissions`, `Screens`, `SubModules`, `UserDataScopeOverrides`, `UserFieldPermissions`, `UserPermissionOverrides`, `WorkflowPermissions`, `Workspaces` |
| Tenant API | `EntityRepository` | `ONEERP.ERP.API/Repositories/EntityRepository.cs` | `Address`, `Contact`, `Entity`, `EntityAddress`, `EntityContact`, `EntityFile`, `EntityNote`, `EntityTag`, `Files`, `Note`, `Tag` |
| Tenant API | `FinancialYearRepository` | `ONEERP.ERP.API/Repositories/FinancialYearRepository.cs` | `FinancialYear` |
| Tenant API | `GstRegistrationTypeRepository` | `ONEERP.ERP.API/Repositories/GstRegistrationTypeRepository.cs` | `GSTRegistrationTypes` |
| Tenant API | `ImportLogRepository` | `ONEERP.ERP.API/Repositories/ImportLogRepository.cs` | `ImportLogs` |
| Tenant API | `InventoryRepository` | `ONEERP.ERP.API/Repositories/InventoryRepository.cs` | `Products`, `Stock`, `StockAdjustment`, `StockAdjustmentItem`, `StockCount`, `StockCountItem`, `StockTransaction`, `StockTransfer`, `StockTransferItem` |
| Tenant API | `InvoiceTemplateAssignmentRepository` | `ONEERP.ERP.API/Repositories/InvoiceTemplateRepositories.cs` | `IndustryTypes`, `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplateItemColumn`, `InvoiceTemplatePrintSetting`, `InvoiceTemplatePrinter`, `InvoiceTemplateSection`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceTemplateVersion`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `InvoiceTemplateLookupRepository` | `ONEERP.ERP.API/Repositories/InvoiceTemplateRepositories.cs` | `IndustryTypes`, `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplateItemColumn`, `InvoiceTemplatePrintSetting`, `InvoiceTemplatePrinter`, `InvoiceTemplateSection`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceTemplateVersion`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `InvoiceTemplatePrinterRepository` | `ONEERP.ERP.API/Repositories/InvoiceTemplateRepositories.cs` | `IndustryTypes`, `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplateItemColumn`, `InvoiceTemplatePrintSetting`, `InvoiceTemplatePrinter`, `InvoiceTemplateSection`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceTemplateVersion`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `InvoiceTemplateRepository` | `ONEERP.ERP.API/Repositories/InvoiceTemplateRepositories.cs` | `IndustryTypes`, `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplateItemColumn`, `InvoiceTemplatePrintSetting`, `InvoiceTemplatePrinter`, `InvoiceTemplateSection`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceTemplateVersion`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `InvoiceTemplateVersionRepository` | `ONEERP.ERP.API/Repositories/InvoiceTemplateRepositories.cs` | `IndustryTypes`, `InvoiceFont`, `InvoicePaperSize`, `InvoiceTemplate`, `InvoiceTemplateAssignment`, `InvoiceTemplateCategory`, `InvoiceTemplateComponent`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplateItemColumn`, `InvoiceTemplatePrintSetting`, `InvoiceTemplatePrinter`, `InvoiceTemplateSection`, `InvoiceTemplateStyle`, `InvoiceTemplateVariable`, `InvoiceTemplateVersion`, `InvoiceType`, `PrintOrientation`, `PrintUnit`, `PrinterModel`, `PrinterType` |
| Tenant API | `LanguageRepository` | `ONEERP.ERP.API/Repositories/LanguageRepository.cs` | `Languages` |
| Tenant API | `CityRepository` | `ONEERP.ERP.API/Repositories/LocationRepository.cs` | `Cities`, `Countries`, `States` |
| Tenant API | `CountryRepository` | `ONEERP.ERP.API/Repositories/LocationRepository.cs` | `Cities`, `Countries`, `States` |
| Tenant API | `StateRepository` | `ONEERP.ERP.API/Repositories/LocationRepository.cs` | `Cities`, `Countries`, `States` |
| Tenant API | `BusinessPartnerRepository` | `ONEERP.ERP.API/Repositories/MasterDataRepository.cs` | `BusinessPartnerRoles`, `BusinessPartners`, `BusinessTypes`, `CompanyGroups`, `FieldPermissions`, `IndustryTypes`, `ModulePermissions`, `OperatorTypes`, `PermissionActions`, `PermissionModules`, `Sources`, `StoreTypes`, `UserRoles` |
| Tenant API | `BusinessPartnerRoleRepository` | `ONEERP.ERP.API/Repositories/MasterDataRepository.cs` | `BusinessPartnerRoles`, `BusinessPartners`, `BusinessTypes`, `CompanyGroups`, `FieldPermissions`, `IndustryTypes`, `ModulePermissions`, `OperatorTypes`, `PermissionActions`, `PermissionModules`, `Sources`, `StoreTypes`, `UserRoles` |
| Tenant API | `BusinessTypeRepository` | `ONEERP.ERP.API/Repositories/MasterDataRepository.cs` | `BusinessPartnerRoles`, `BusinessPartners`, `BusinessTypes`, `CompanyGroups`, `FieldPermissions`, `IndustryTypes`, `ModulePermissions`, `OperatorTypes`, `PermissionActions`, `PermissionModules`, `Sources`, `StoreTypes`, `UserRoles` |
| Tenant API | `CompanyGroupRepository` | `ONEERP.ERP.API/Repositories/MasterDataRepository.cs` | `BusinessPartnerRoles`, `BusinessPartners`, `BusinessTypes`, `CompanyGroups`, `FieldPermissions`, `IndustryTypes`, `ModulePermissions`, `OperatorTypes`, `PermissionActions`, `PermissionModules`, `Sources`, `StoreTypes`, `UserRoles` |
| Tenant API | `FieldPermissionRepository` | `ONEERP.ERP.API/Repositories/MasterDataRepository.cs` | `BusinessPartnerRoles`, `BusinessPartners`, `BusinessTypes`, `CompanyGroups`, `FieldPermissions`, `IndustryTypes`, `ModulePermissions`, `OperatorTypes`, `PermissionActions`, `PermissionModules`, `Sources`, `StoreTypes`, `UserRoles` |
| Tenant API | `IndustryTypeRepository` | `ONEERP.ERP.API/Repositories/MasterDataRepository.cs` | `BusinessPartnerRoles`, `BusinessPartners`, `BusinessTypes`, `CompanyGroups`, `FieldPermissions`, `IndustryTypes`, `ModulePermissions`, `OperatorTypes`, `PermissionActions`, `PermissionModules`, `Sources`, `StoreTypes`, `UserRoles` |
| Tenant API | `ModulePermissionRepository` | `ONEERP.ERP.API/Repositories/MasterDataRepository.cs` | `BusinessPartnerRoles`, `BusinessPartners`, `BusinessTypes`, `CompanyGroups`, `FieldPermissions`, `IndustryTypes`, `ModulePermissions`, `OperatorTypes`, `PermissionActions`, `PermissionModules`, `Sources`, `StoreTypes`, `UserRoles` |
| Tenant API | `OperatorTypeRepository` | `ONEERP.ERP.API/Repositories/MasterDataRepository.cs` | `BusinessPartnerRoles`, `BusinessPartners`, `BusinessTypes`, `CompanyGroups`, `FieldPermissions`, `IndustryTypes`, `ModulePermissions`, `OperatorTypes`, `PermissionActions`, `PermissionModules`, `Sources`, `StoreTypes`, `UserRoles` |
| Tenant API | `PermissionActionRepository` | `ONEERP.ERP.API/Repositories/MasterDataRepository.cs` | `BusinessPartnerRoles`, `BusinessPartners`, `BusinessTypes`, `CompanyGroups`, `FieldPermissions`, `IndustryTypes`, `ModulePermissions`, `OperatorTypes`, `PermissionActions`, `PermissionModules`, `Sources`, `StoreTypes`, `UserRoles` |
| Tenant API | `PermissionModuleRepository` | `ONEERP.ERP.API/Repositories/MasterDataRepository.cs` | `BusinessPartnerRoles`, `BusinessPartners`, `BusinessTypes`, `CompanyGroups`, `FieldPermissions`, `IndustryTypes`, `ModulePermissions`, `OperatorTypes`, `PermissionActions`, `PermissionModules`, `Sources`, `StoreTypes`, `UserRoles` |
| Tenant API | `SourceRepository` | `ONEERP.ERP.API/Repositories/MasterDataRepository.cs` | `BusinessPartnerRoles`, `BusinessPartners`, `BusinessTypes`, `CompanyGroups`, `FieldPermissions`, `IndustryTypes`, `ModulePermissions`, `OperatorTypes`, `PermissionActions`, `PermissionModules`, `Sources`, `StoreTypes`, `UserRoles` |
| Tenant API | `StoreTypeRepository` | `ONEERP.ERP.API/Repositories/MasterDataRepository.cs` | `BusinessPartnerRoles`, `BusinessPartners`, `BusinessTypes`, `CompanyGroups`, `FieldPermissions`, `IndustryTypes`, `ModulePermissions`, `OperatorTypes`, `PermissionActions`, `PermissionModules`, `Sources`, `StoreTypes`, `UserRoles` |
| Tenant API | `CounterAssignmentRepository` | `ONEERP.ERP.API/Repositories/OperatorRepositories.cs` | `CounterAssignments`, `Counters`, `OperatorTypes`, `Operators`, `Stores` |
| Tenant API | `OperatorRepository` | `ONEERP.ERP.API/Repositories/OperatorRepositories.cs` | `CounterAssignments`, `Counters`, `OperatorTypes`, `Operators`, `Stores` |
| Tenant API | `BranchRepository` | `ONEERP.ERP.API/Repositories/OrganizationRepository.cs` | `BranchTypes`, `Branches`, `Departments`, `Designations`, `Employees`, `EmploymentTypes`, `WarehouseTypes`, `Warehouses` |
| Tenant API | `BranchTypeRepository` | `ONEERP.ERP.API/Repositories/OrganizationRepository.cs` | `BranchTypes`, `Branches`, `Departments`, `Designations`, `Employees`, `EmploymentTypes`, `WarehouseTypes`, `Warehouses` |
| Tenant API | `DepartmentRepository` | `ONEERP.ERP.API/Repositories/OrganizationRepository.cs` | `BranchTypes`, `Branches`, `Departments`, `Designations`, `Employees`, `EmploymentTypes`, `WarehouseTypes`, `Warehouses` |
| Tenant API | `DesignationRepository` | `ONEERP.ERP.API/Repositories/OrganizationRepository.cs` | `BranchTypes`, `Branches`, `Departments`, `Designations`, `Employees`, `EmploymentTypes`, `WarehouseTypes`, `Warehouses` |
| Tenant API | `EmployeeRepository` | `ONEERP.ERP.API/Repositories/OrganizationRepository.cs` | `BranchTypes`, `Branches`, `Departments`, `Designations`, `Employees`, `EmploymentTypes`, `WarehouseTypes`, `Warehouses` |
| Tenant API | `EmploymentTypeRepository` | `ONEERP.ERP.API/Repositories/OrganizationRepository.cs` | `BranchTypes`, `Branches`, `Departments`, `Designations`, `Employees`, `EmploymentTypes`, `WarehouseTypes`, `Warehouses` |
| Tenant API | `WarehouseRepository` | `ONEERP.ERP.API/Repositories/OrganizationRepository.cs` | `BranchTypes`, `Branches`, `Departments`, `Designations`, `Employees`, `EmploymentTypes`, `WarehouseTypes`, `Warehouses` |
| Tenant API | `WarehouseTypeRepository` | `ONEERP.ERP.API/Repositories/OrganizationRepository.cs` | `BranchTypes`, `Branches`, `Departments`, `Designations`, `Employees`, `EmploymentTypes`, `WarehouseTypes`, `Warehouses` |
| Tenant API | `OrganizationTypeRepository` | `ONEERP.ERP.API/Repositories/OrganizationTypeRepository.cs` | `OrganizationTypes` |
| Tenant API | `PaymentMethodDetailRepository` | `ONEERP.ERP.API/Repositories/PaymentRepositories.cs` | `Payment`, `PaymentAllocation`, `PaymentMethod`, `PaymentMethodDetails`, `PaymentType` |
| Tenant API | `PaymentMethodRepository` | `ONEERP.ERP.API/Repositories/PaymentRepositories.cs` | `Payment`, `PaymentAllocation`, `PaymentMethod`, `PaymentMethodDetails`, `PaymentType` |
| Tenant API | `PaymentRepository` | `ONEERP.ERP.API/Repositories/PaymentRepositories.cs` | `Payment`, `PaymentAllocation`, `PaymentMethod`, `PaymentMethodDetails`, `PaymentType` |
| Tenant API | `PaymentTypeRepository` | `ONEERP.ERP.API/Repositories/PaymentRepositories.cs` | `Payment`, `PaymentAllocation`, `PaymentMethod`, `PaymentMethodDetails`, `PaymentType` |
| Tenant API | `DashboardAgg` | `ONEERP.ERP.API/Repositories/POSOperationsRepository.cs` | `POSCashMovement`, `POSHoldBill`, `SalesInvoice`, `SalesReturn`, `Status` |
| Tenant API | `POSOperationsRepository` | `ONEERP.ERP.API/Repositories/POSOperationsRepository.cs` | `POSCashMovement`, `POSHoldBill`, `SalesInvoice`, `SalesReturn`, `Status` |
| Tenant API | `ShiftAgg` | `ONEERP.ERP.API/Repositories/POSOperationsRepository.cs` | `POSCashMovement`, `POSHoldBill`, `SalesInvoice`, `SalesReturn`, `Status` |
| Tenant API | `ProductBrandRepository` | `ONEERP.ERP.API/Repositories/ProductRepositories.cs` | `ProductBrands`, `ProductCategories`, `ProductSubCategories`, `Products`, `Units` |
| Tenant API | `ProductCategoryRepository` | `ONEERP.ERP.API/Repositories/ProductRepositories.cs` | `ProductBrands`, `ProductCategories`, `ProductSubCategories`, `Products`, `Units` |
| Tenant API | `ProductRepository` | `ONEERP.ERP.API/Repositories/ProductRepositories.cs` | `ProductBrands`, `ProductCategories`, `ProductSubCategories`, `Products`, `Units` |
| Tenant API | `ProductSubCategoryRepository` | `ONEERP.ERP.API/Repositories/ProductRepositories.cs` | `ProductBrands`, `ProductCategories`, `ProductSubCategories`, `Products`, `Units` |
| Tenant API | `ProductUnitRepository` | `ONEERP.ERP.API/Repositories/ProductRepositories.cs` | `ProductBrands`, `ProductCategories`, `ProductSubCategories`, `Products`, `Units` |
| Tenant API | `PurchaseReportRepository` | `ONEERP.ERP.API/Repositories/PurchaseReportRepository.cs` | `Products`, `PurchaseItem`, `PurchaseReturnItem` |
| Tenant API | `PurchaseReportRepository` | `ONEERP.ERP.API/Repositories/PurchaseReportRepository.Reports1.cs` | `PaymentMethod`, `PaymentType`, `ProductBrands`, `ProductCategories`, `ProductSubCategories`, `Purchase`, `PurchaseItem`, `PurchaseReturn`, `Status`, `Warehouses` |
| Tenant API | `PurchaseReportRepository` | `ONEERP.ERP.API/Repositories/PurchaseReportRepository.Reports2.cs` | `PaymentAllocation`, `PaymentMethod`, `PaymentType`, `ProductBrands`, `ProductCategories`, `Purchase`, `PurchaseItem`, `Status` |
| Tenant API | `PurchaseReportRepository` | `ONEERP.ERP.API/Repositories/PurchaseReportRepository.Reports3.cs` | `ProductBrands`, `ProductCategories`, `Purchase`, `PurchaseItem`, `PurchaseReturn`, `PurchaseReturnItem`, `Status`, `Warehouses` |
| Tenant API | `PurchasePaymentInput` | `ONEERP.ERP.API/Repositories/PurchaseRepository.cs` | `Address`, `Branches`, `BusinessPartners`, `Companies`, `EntityAddress`, `Payment`, `PaymentAllocation`, `PaymentMethod`, `Purchase`, `PurchaseItem`, `PurchaseReturn`, `Status`, `Stock`, `StockTransaction`, `Warehouses` |
| Tenant API | `PurchaseRepository` | `ONEERP.ERP.API/Repositories/PurchaseRepository.cs` | `Address`, `Branches`, `BusinessPartners`, `Companies`, `EntityAddress`, `Payment`, `PaymentAllocation`, `PaymentMethod`, `Purchase`, `PurchaseItem`, `PurchaseReturn`, `Status`, `Stock`, `StockTransaction`, `Warehouses` |
| Tenant API | `PurchaseReturnRepository` | `ONEERP.ERP.API/Repositories/PurchaseReturnRepository.cs` | `Address`, `Branches`, `BusinessPartners`, `Companies`, `EntityAddress`, `PurchaseReturn`, `PurchaseReturnItem`, `Status`, `Stock`, `StockTransaction`, `Warehouses` |
| Tenant API | `AuditLogRepository` | `ONEERP.ERP.API/Repositories/RefreshTokenRepository.cs` | `AuditLogs`, `RefreshTokens` |
| Tenant API | `RefreshTokenRepository` | `ONEERP.ERP.API/Repositories/RefreshTokenRepository.cs` | `AuditLogs`, `RefreshTokens` |
| Tenant API | `RoleRepository` | `ONEERP.ERP.API/Repositories/RoleRepository.cs` | `Actions`, `ModulePermissions`, `PermissionActions`, `PermissionModules`, `RolePermissions`, `RolePermissionsLegacy`, `Roles`, `Screens`, `UserPermissionOverrides`, `UserRoles` |
| Tenant API | `UserRoleAssignment` | `ONEERP.ERP.API/Repositories/RoleRepository.cs` | `Actions`, `ModulePermissions`, `PermissionActions`, `PermissionModules`, `RolePermissions`, `RolePermissionsLegacy`, `Roles`, `Screens`, `UserPermissionOverrides`, `UserRoles` |
| Tenant API | `SalesReportRepository` | `ONEERP.ERP.API/Repositories/SalesReportRepository.cs` | `Payment`, `PaymentAllocation`, `SalesInvoice`, `SalesInvoiceItem` |
| Tenant API | `SalesReportRepository` | `ONEERP.ERP.API/Repositories/SalesReportRepository.Reports1.cs` | `Branches`, `PaymentMethod`, `PaymentType`, `SalesInvoice`, `SalesInvoiceItem`, `Warehouses` |
| Tenant API | `SalesReportRepository` | `ONEERP.ERP.API/Repositories/SalesReportRepository.Reports2.cs` | `Branches`, `Payment`, `PaymentAllocation`, `PaymentMethod`, `PaymentType`, `PriceLists`, `SalesInvoice`, `SalesInvoiceItem` |
| Tenant API | `SalesRepository` | `ONEERP.ERP.API/Repositories/SalesRepository.cs` | `Address`, `Branches`, `BusinessPartners`, `Companies`, `EntityAddress`, `HsnSacs`, `Payment`, `PaymentAllocation`, `PaymentMethod`, `PriceListDetails`, `Products`, `SalesInvoice`, `SalesInvoiceItem`, `Stock`, `StockTransaction`, `Taxes`, `Units`, `Warehouses` |
| Tenant API | `SalesReturnRepository` | `ONEERP.ERP.API/Repositories/SalesReturnRepository.cs` | `Payment`, `SalesReturn`, `SalesReturnItem`, `Status`, `Stock`, `StockTransaction` |
| Tenant API | `StatusRepository` | `ONEERP.ERP.API/Repositories/StatusRepository.cs` | `Status` |
| Tenant API | `StockRepository` | `ONEERP.ERP.API/Repositories/StockRepository.cs` | `ProductUnits`, `Products`, `Stock`, `StockTransaction` |
| Tenant API | `CounterRepository` | `ONEERP.ERP.API/Repositories/StoreRepositories.cs` | `Counters`, `POSSessions`, `Payment`, `PaymentMethod`, `PaymentType`, `SalesInvoice`, `Stores` |
| Tenant API | `POSSessionRepository` | `ONEERP.ERP.API/Repositories/StoreRepositories.cs` | `Counters`, `POSSessions`, `Payment`, `PaymentMethod`, `PaymentType`, `SalesInvoice`, `Stores` |
| Tenant API | `StoreRepository` | `ONEERP.ERP.API/Repositories/StoreRepositories.cs` | `Counters`, `POSSessions`, `Payment`, `PaymentMethod`, `PaymentType`, `SalesInvoice`, `Stores` |
| Tenant API | `TaxRepository` | `ONEERP.ERP.API/Repositories/TaxRepositories.cs` | `TaxTypeSystems`, `Taxes` |
| Tenant API | `TaxTypeSystemRepository` | `ONEERP.ERP.API/Repositories/TaxRepositories.cs` | `TaxTypeSystems`, `Taxes` |
| Tenant API | `TenantConfigurationRepository` | `ONEERP.ERP.API/Repositories/TenantConfigurationRepository.cs` | `TenantConfiguration` |
| Tenant API | `TenantRepositoryBase` | `ONEERP.ERP.API/Repositories/TenantRepositoryBase.cs` | No literal dbo reference |
| Tenant API | `TimeZoneRepository` | `ONEERP.ERP.API/Repositories/TimeZoneRepository.cs` | `TimeZones` |
| Tenant API | `UserRepository` | `ONEERP.ERP.API/Repositories/UserRepository.cs` | `UserRoles`, `Users` |
| Platform API | `AuditLogRepository` | `ONEERP.Platform.API/Repositories/AuditLogRepository.cs` | `AuditLogs` |
| Platform API | `BaseRepository` | `ONEERP.Platform.API/Repositories/BaseRepository.cs` | No literal dbo reference |
| Platform API | `MigrationRepository` | `ONEERP.Platform.API/Repositories/MigrationRepository.cs` | `Migrations` |
| Platform API | `PlanRepository` | `ONEERP.Platform.API/Repositories/PlanRepository.cs` | `Plans` |
| Platform API | `PlatformUserRepository` | `ONEERP.Platform.API/Repositories/PlatformUserRepository.cs` | `PlatformUsers` |
| Platform API | `RefreshTokenRepository` | `ONEERP.Platform.API/Repositories/RefreshTokenRepository.cs` | `RefreshTokens` |
| Platform API | `SettingsRepository` | `ONEERP.Platform.API/Repositories/SettingsRepository.cs` | `Settings` |
| Platform API | `SubscriptionRepository` | `ONEERP.Platform.API/Repositories/SubscriptionRepository.cs` | `Plans`, `Subscriptions`, `Tenants` |
| Platform API | `SubscriptionRow` | `ONEERP.Platform.API/Repositories/SubscriptionRepository.cs` | `Plans`, `Subscriptions`, `Tenants` |
| Platform API | `TenantConnectionRepository` | `ONEERP.Platform.API/Repositories/TenantConnectionRepository.cs` | `TenantConnections` |
| Platform API | `TenantMigrationHistoryRepository` | `ONEERP.Platform.API/Repositories/TenantMigrationHistoryRepository.cs` | `TenantMigrationHistory` |
| Platform API | `TenantRepository` | `ONEERP.Platform.API/Repositories/TenantRepository.cs` | `Plans`, `Subscriptions`, `TenantConnections`, `Tenants` |
| Platform API | `TenantRow` | `ONEERP.Platform.API/Repositories/TenantRepository.cs` | `Plans`, `Subscriptions`, `TenantConnections`, `Tenants` |

## 11. Database Table Master List

Source definitions: `sql/erp_full.sql` tenant full-install schema, `sql/platform_schema.sql` platform schema, plus tenant migrations. The list consolidates table names across repeated script definitions. Live tables are UNKNOWN.

| # | Table | PK columns | FK count | Inline unique constraints | Indexes parsed | Definition source | Column count |
|---:|---|---|---:|---|---|---|---:|
| 1 | `dbo.Actions` | `Id` | 0 | None found inline | None found | `sql/erp_full.sql` | 5 |
| 2 | `dbo.Address` | `AddressId` | 0 | None found inline | `IX_Address_CityId`, `IX_Address_StateId`, `IX_Address_CountryId`, `IX_Address_IsActive` | `sql/erp_full.sql` | 17 |
| 3 | `dbo.AddressTypes` | `AddressTypeId` | 0 | None found inline | `IX_AddressTypes_IsActive` | `sql/erp_full.sql` | 9 |
| 4 | `dbo.ApiIdempotency` | `CompanyId`, `Endpoint`, `IdempotencyKey` | 0 | None found inline | None found | `sql/erp_full.sql` | 6 |
| 5 | `dbo.ApplicationSettings` | `SettingId` | 0 | None found inline | None found | `sql/erp_full.sql` | 6 |
| 6 | `dbo.AuditLogs` | `AuditLogId` | 0 | None found inline | `IX_AuditLogs_EntityDate`, `IX_Platform_AuditLogs_TenantDate` | `sql/platform_schema.sql` | 10 |
| 7 | `dbo.Barcodes` | `BarcodeId` | 2 | `CompanyId`+`Barcode` | `IX_Barcodes_Company`, `IX_Barcodes_Product` | `sql/erp_full.sql` | 12 |
| 8 | `dbo.Branches` | `Id` | 3 | `CompanyId`+`BranchCode` | `IX_Branches_Company`, `IX_Branches_BranchType`, `IX_Branches_Parent` | `sql/erp_full.sql` | 23 |
| 9 | `dbo.BranchTypes` | `BranchTypeId` | 0 | None found inline | `IX_BranchTypes_SortOrder`, `IX_BranchTypes_IsActive` | `sql/erp_full.sql` | 11 |
| 10 | `dbo.BusinessPartnerRoles` | `BusinessPartnerRoleId` | 0 | None found inline | `IX_BusinessPartnerRoles_IsActive` | `sql/erp_full.sql` | 10 |
| 11 | `dbo.BusinessPartners` | `Id` | 0 | None found inline | None found | `sql/erp_full.sql` | 20 |
| 12 | `dbo.BusinessTypes` | `BusinessTypeId` | 0 | None found inline | `IX_BusinessTypes_SortOrder`, `IX_BusinessTypes_IsActive` | `sql/erp_full.sql` | 10 |
| 13 | `dbo.Cities` | `CityId` | 2 | `StateId`+`Name` | `IX_Cities_StateId`, `IX_Cities_CountryId`, `IX_Cities_IsActive` | `sql/erp_full.sql` | 13 |
| 14 | `dbo.Companies` | `Id` | 6 | None found inline | `IX_Companies_IsActive`, `IX_Companies_BusinessTypeId`, `IX_Companies_IndustryTypeId`, `IX_Companies_CurrencyId` | `sql/erp_full.sql` | 24 |
| 15 | `dbo.CompanyGroups` | `CompanyGroupId` | 1 | None found inline | `IX_CompanyGroups_ParentGroupId`, `IX_CompanyGroups_IsActive` | `sql/erp_full.sql` | 12 |
| 16 | `dbo.Contact` | `ContactId` | 0 | None found inline | `IX_Contact_ContactTypeId`, `IX_Contact_IsActive` | `sql/erp_full.sql` | 13 |
| 17 | `dbo.ContactTypes` | `ContactTypeId` | 0 | None found inline | `IX_ContactTypes_IsActive` | `sql/erp_full.sql` | 9 |
| 18 | `dbo.CounterAssignments` | `AssignmentId` | 5 | None found inline | `IX_CounterAssignments_CompanyId`, `IX_CounterAssignments_StoreId`, `IX_CounterAssignments_CounterId`, `IX_CounterAssignments_OperatorId`, `IX_CounterAssignments_IsActive` | `sql/erp_full.sql` | 16 |
| 19 | `dbo.Counters` | `CounterId` | 1 | `StoreId`+`CounterCode` | `IX_Counters_Store`, `IX_Counters_Code` | `sql/erp_full.sql` | 10 |
| 20 | `dbo.Countries` | `CountryId` | 0 | None found inline | `IX_Countries_IsActive` | `sql/erp_full.sql` | 13 |
| 21 | `dbo.Coupons` | `CouponId` | 1 | `CompanyId`+`Code` | `IX_Coupons_Company`, `IX_Coupons_Offer` | `sql/erp_full.sql` | 15 |
| 22 | `dbo.Currencies` | `Id` | 0 | None found inline | `IX_Currencies_CurrencyCode`, `IX_Currencies_IsBaseCurrency`, `IX_Currencies_SortOrder`, `IX_Currencies_IsActive` | `sql/erp_full.sql` | 13 |
| 23 | `dbo.Departments` | `Id` | 3 | `CompanyId`+`BranchId`+`DepartmentCode` | `IX_Departments_Company`, `IX_Departments_Branch`, `IX_Departments_Parent` | `sql/erp_full.sql` | 17 |
| 24 | `dbo.Designations` | `Id` | 2 | `CompanyId`+`DesignationCode` | `IX_Designations_Company`, `IX_Designations_Department` | `sql/erp_full.sql` | 18 |
| 25 | `dbo.DiscountRules` | `DiscountRuleId` | 4 | `CompanyId`+`Code` | `IX_DiscountRules_Company`, `IX_DiscountRules_Product`, `IX_DiscountRules_PriceList` | `sql/erp_full.sql` | 20 |
| 26 | `dbo.DocumentTypes` | `DocumentTypeId` | 0 | None found inline | `IX_DocumentTypes_IsActive` | `sql/erp_full.sql` | 9 |
| 27 | `dbo.Domains` | `Id` | 1 | `WorkspaceId`+`DomainCode` | `IX_Domains_WorkspaceId`, `IX_Domains_IsActive` | `sql/erp_full.sql` | 9 |
| 28 | `dbo.Employees` | `Id` | 5 | `CompanyId`+`EmployeeCode` | `IX_Employees_Company`, `IX_Employees_Branch`, `IX_Employees_Department`, `IX_Employees_Designation`, `IX_Employees_Manager` | `sql/erp_full.sql` | 30 |
| 29 | `dbo.EmploymentTypes` | `EmploymentTypeId` | 0 | None found inline | `IX_EmploymentTypes_SortOrder`, `IX_EmploymentTypes_IsActive` | `sql/erp_full.sql` | 11 |
| 30 | `dbo.Entity` | `EntityId` | 0 | `EntityType`+`EntityCode` | `IX_Entity_EntityType`, `IX_Entity_IsActive` | `sql/erp_full.sql` | 9 |
| 31 | `dbo.EntityAddress` | `EntityAddressId` | 2 | `EntityId`+`AddressId` | `IX_EntityAddress_EntityId`, `IX_EntityAddress_AddressId` | `sql/erp_full.sql` | 7 |
| 32 | `dbo.EntityContact` | `EntityContactId` | 2 | `EntityId`+`ContactId` | `IX_EntityContact_EntityId`, `IX_EntityContact_ContactId` | `sql/erp_full.sql` | 7 |
| 33 | `dbo.EntityFile` | `EntityFileId` | 2 | None found inline | `IX_EntityFile_EntityId`, `IX_EntityFile_FileId` | `sql/erp_full.sql` | 8 |
| 34 | `dbo.EntityNote` | `EntityNoteId` | 2 | `EntityId`+`NoteId` | `IX_EntityNote_EntityId`, `IX_EntityNote_NoteId` | `sql/erp_full.sql` | 6 |
| 35 | `dbo.EntityTag` | `EntityTagId` | 2 | `EntityId`+`TagId` | `IX_EntityTag_EntityId`, `IX_EntityTag_TagId` | `sql/erp_full.sql` | 6 |
| 36 | `dbo.FieldPermissions` | `Id` | 2 | `RoleId`+`PermissionModuleId`+`FieldName`+`Scope`+`ScopeId` | `IX_FieldPermissions_RoleId`, `IX_FieldPermissions_ModuleId`, `IX_FieldPermissions_FieldName`, `IX_FieldPermissions_Scope` | `sql/erp_full.sql` | 14 |
| 37 | `dbo.Fields` | `Id` | 1 | `ScreenId`+`FieldCode` | `IX_Fields_ScreenId`, `IX_Fields_IsActive` | `sql/erp_full.sql` | 13 |
| 38 | `dbo.Files` | `FileId` | 0 | None found inline | `IX_Files_BucketName`, `IX_Files_IsActive` | `sql/erp_full.sql` | 14 |
| 39 | `dbo.FinancialYear` | UNKNOWN | 1 | `CompanyId`+`Code` | `IX_FinancialYear_Company`, `IX_FinancialYear_IsCurrent`, `IX_FinancialYear_IsActive` | `sql/erp_full.sql` | 13 |
| 40 | `dbo.Genders` | `GenderId` | 0 | None found inline | None found | `sql/erp_full.sql` | 5 |
| 41 | `dbo.GSTRegistrationTypes` | `GSTRegistrationTypeId` | 0 | None found inline | `IX_GSTRegistrationTypes_IsActive` | `sql/erp_full.sql` | 9 |
| 42 | `dbo.HsnSacs` | `HsnSacId` | 1 | `CompanyId`+`Code`; `CompanyId`+`GovernmentCode` | `IX_HsnSacs_Company` | `sql/erp_full.sql` | 13 |
| 43 | `dbo.ImportLogs` | `Id` | 0 | None found inline | `IX_ImportLogs_Company_ImportedAt` | `sql/erp_full.sql` | 15 |
| 44 | `dbo.IndustryTypes` | `IndustryTypeId` | 0 | None found inline | `IX_IndustryTypes_SortOrder`, `IX_IndustryTypes_IsActive` | `sql/erp_full.sql` | 10 |
| 45 | `dbo.InvoiceFont` | `FontId` | 0 | None found inline | `IX_InvoiceFont_IsActive` | `sql/erp_full.sql` | 10 |
| 46 | `dbo.InvoicePaperSize` | `PaperSizeId` | 0 | None found inline | `IX_InvoicePaperSize_IsActive` | `sql/erp_full.sql` | 13 |
| 47 | `dbo.InvoiceTemplate` | `InvoiceTemplateId` | 4 | `CompanyId`+`Code` | `IX_InvoiceTemplate_CompanyId`, `IX_InvoiceTemplate_InvoiceTypeId`, `IX_InvoiceTemplate_PaperSizeId`, `IX_InvoiceTemplate_IsActive` | `sql/erp_full.sql` | 17 |
| 48 | `dbo.InvoiceTemplateAssignment` | `AssignmentId` | 5 | None found inline | `IX_InvoiceTemplateAssignment_TemplateId`, `IX_InvoiceTemplateAssignment_Lookup` | `sql/erp_full.sql` | 12 |
| 49 | `dbo.InvoiceTemplateCategory` | `TemplateCategoryId` | 0 | None found inline | `IX_InvoiceTemplateCategory_IsActive` | `sql/erp_full.sql` | 10 |
| 50 | `dbo.InvoiceTemplateComponent` | `ComponentId` | 0 | None found inline | `IX_InvoiceTemplateComponent_ComponentType`, `IX_InvoiceTemplateComponent_IsActive` | `sql/erp_full.sql` | 11 |
| 51 | `dbo.InvoiceTemplateElement` | `ElementId` | 2 | None found inline | `IX_InvoiceTemplateElement_SectionId`, `IX_InvoiceTemplateElement_ComponentId` | `sql/erp_full.sql` | 11 |
| 52 | `dbo.InvoiceTemplateField` | `TemplateFieldId` | 2 | None found inline | `IX_InvoiceTemplateField_ElementId`, `IX_InvoiceTemplateField_VariableId` | `sql/erp_full.sql` | 7 |
| 53 | `dbo.InvoiceTemplateItemColumn` | `ItemColumnId` | 1 | None found inline | `IX_InvoiceTemplateItemColumn_ElementId` | `sql/erp_full.sql` | 8 |
| 54 | `dbo.InvoiceTemplatePrinter` | `TemplatePrinterId` | 4 | None found inline | `IX_InvoiceTemplatePrinter_TemplateId`, `IX_InvoiceTemplatePrinter_PaperSizeId` | `sql/erp_full.sql` | 12 |
| 55 | `dbo.InvoiceTemplatePrintSetting` | `PrintSettingId` | 1 | None found inline | `IX_InvoiceTemplatePrintSetting_TemplatePrinterId` | `sql/erp_full.sql` | 16 |
| 56 | `dbo.InvoiceTemplateSection` | `SectionId` | 1 | None found inline | `IX_InvoiceTemplateSection_VersionId` | `sql/erp_full.sql` | 10 |
| 57 | `dbo.InvoiceTemplateStyle` | `StyleId` | 2 | `ElementId` | `IX_InvoiceTemplateStyle_FontId` | `sql/erp_full.sql` | 19 |
| 58 | `dbo.InvoiceTemplateVariable` | `VariableId` | 0 | None found inline | `IX_InvoiceTemplateVariable_Category`, `IX_InvoiceTemplateVariable_IsActive` | `sql/erp_full.sql` | 13 |
| 59 | `dbo.InvoiceTemplateVersion` | `TemplateVersionId` | 1 | `InvoiceTemplateId`+`VersionNumber` | `IX_InvoiceTemplateVersion_TemplateId`, `IX_InvoiceTemplateVersion_IsPublished` | `sql/erp_full.sql` | 10 |
| 60 | `dbo.InvoiceType` | `InvoiceTypeId` | 0 | None found inline | `IX_InvoiceType_IsActive`, `IX_InvoiceType_DisplayOrder` | `sql/erp_full.sql` | 10 |
| 61 | `dbo.Languages` | `LanguageId` | 0 | None found inline | `IX_Languages_SortOrder`, `IX_Languages_IsActive` | `sql/erp_full.sql` | 13 |
| 62 | `dbo.MaritalStatuses` | `MaritalStatusId` | 0 | None found inline | None found | `sql/erp_full.sql` | 5 |
| 63 | `dbo.Migrations` | `MigrationId` | 0 | None found inline | `IX_Migrations_IsActive` | `sql/platform_schema.sql` | 13 |
| 64 | `dbo.ModulePermissions` | `Id` | 3 | `RoleId`+`PermissionModuleId`+`PermissionActionId`+`Scope`+`ScopeId` | `IX_ModulePermissions_RoleId`, `IX_ModulePermissions_ModuleId`, `IX_ModulePermissions_ActionId`, `IX_ModulePermissions_Scope`, `IX_ModulePermissions_IsRevoked` | `sql/erp_full.sql` | 11 |
| 65 | `dbo.Modules` | `Id` | 1 | `DomainId`+`ModuleCode` | `IX_Modules_DomainId`, `IX_Modules_IsActive` | `sql/erp_full.sql` | 10 |
| 66 | `dbo.Note` | `NoteId` | 0 | None found inline | `IX_Note_IsActive` | `sql/erp_full.sql` | 8 |
| 67 | `dbo.OfferDetails` | `OfferDetailId` | 4 | None found inline | `IX_OfferDetails_Offer`, `IX_OfferDetails_Product`, `IX_OfferDetails_Service`, `IX_OfferDetails_Category` | `sql/erp_full.sql` | 13 |
| 68 | `dbo.Offers` | `OfferId` | 0 | `CompanyId`+`Code` | `IX_Offers_Company` | `sql/erp_full.sql` | 18 |
| 69 | `dbo.Operators` | `OperatorId` | 4 | `CompanyId`+`OperatorCode` | `IX_Operators_CompanyId`, `IX_Operators_BranchId`, `IX_Operators_UserId`, `IX_Operators_OperatorTypeId`, `IX_Operators_IsActive` | `sql/erp_full.sql` | 14 |
| 70 | `dbo.OperatorTypes` | `Id` | 0 | None found inline | `IX_OperatorTypes_IsActive`, `IX_OperatorTypes_SortOrder` | `sql/erp_full.sql` | 11 |
| 71 | `dbo.OrganizationTypes` | `OrganizationTypeId` | 0 | None found inline | `IX_OrganizationTypes_SortOrder`, `IX_OrganizationTypes_IsActive` | `sql/erp_full.sql` | 11 |
| 72 | `dbo.Payment` | `PaymentId` | 0 | `CompanyId`+`PaymentNo` | `IX_Payment_Company_Date`, `IX_Payment_Reference` | `sql/erp_full.sql` | 17 |
| 73 | `dbo.PaymentAllocation` | `PaymentAllocationId` | 0 | None found inline | `IX_PaymentAllocation_Ref`, `IX_PaymentAllocation_Payment` | `sql/erp_full.sql` | 6 |
| 74 | `dbo.PaymentMethod` | `PaymentMethodId` | 0 | None found inline | `IX_PaymentMethod_Code`, `UX_PaymentMethod_Code` | `sql/erp_full.sql` | 10 |
| 75 | `dbo.PaymentMethodDetails` | UNKNOWN | 1 | `PaymentMethodId`+`Code` | `IX_PaymentMethodDetails_PaymentMethodId`, `IX_PaymentMethodDetails_IsActive`, `IX_PaymentMethodDetails_IsDefault` | `sql/erp_full.sql` | 19 |
| 76 | `dbo.PaymentType` | `PaymentTypeId` | 0 | None found inline | `IX_PaymentType_Code`, `UX_PaymentType_Code` | `sql/erp_full.sql` | 6 |
| 77 | `dbo.PermissionActions` | `Id` | 0 | None found inline | `IX_PermissionActions_IsActive` | `sql/erp_full.sql` | 9 |
| 78 | `dbo.PermissionModules` | `Id` | 1 | None found inline | `IX_PermissionModules_ParentId`, `IX_PermissionModules_Level`, `IX_PermissionModules_IsVisible`, `IX_PermissionModules_SortOrder` | `sql/erp_full.sql` | 14 |
| 79 | `dbo.Plans` | `PlanId` | 0 | None found inline | `IX_Plans_IsActive` | `sql/platform_schema.sql` | 17 |
| 80 | `dbo.PlatformUsers` | `PlatformUserId` | 0 | None found inline | `IX_PlatformUsers_IsActive` | `sql/platform_schema.sql` | 13 |
| 81 | `dbo.POSCashMovement` | `POSCashMovementId` | 0 | None found inline | `IX_POSCashMovement_Session`, `IX_POSCashMovement_Company_Date` | `sql/erp_full.sql` | 11 |
| 82 | `dbo.POSHoldBill` | `POSHoldBillId` | 0 | `CompanyId`+`HoldNumber` | `IX_POSHoldBill_Scope`, `IX_POSHoldBill_Counter` | `sql/erp_full.sql` | 19 |
| 83 | `dbo.POSSessions` | `POSSessionId` | 5 | None found inline | `IX_POSSessions_Company`, `IX_POSSessions_Branch`, `IX_POSSessions_Store`, `IX_POSSessions_Counter`, `IX_POSSessions_Operator`, `IX_POSSessions_CounterAssignment`, `IX_POSSessions_Status`, `UX_POSSessions_SessionNumber`, `UX_POSSessions_OpenCounter` | `sql/erp_full.sql` | 28 |
| 84 | `dbo.PriceListDetails` | `PriceListDetailId` | 4 | `PriceListId`+`ProductId`+`UnitId`+`PriceTypeId` | `IX_PriceListDetails_PriceList`, `IX_PriceListDetails_Product`, `IX_PriceListDetails_PriceType` | `sql/erp_full.sql` | 13 |
| 85 | `dbo.PriceListPriceTypes` | `PriceListPriceTypeId` | 2 | `PriceListId`+`PriceTypeId` | `IX_PriceListPriceTypes_PriceList`, `IX_PriceListPriceTypes_PriceType` | `sql/erp_full.sql` | 6 |
| 86 | `dbo.PriceLists` | `PriceListId` | 2 | `CompanyId`+`Code` | `IX_PriceLists_Company`, `IX_PriceLists_PriceType`, `IX_PriceLists_PriceTypeIds` | `sql/erp_full.sql` | 15 |
| 87 | `dbo.PriceTypes` | `PriceTypeId` | 0 | `Code` | `IX_PriceTypes_IsActive` | `sql/erp_full.sql` | 10 |
| 88 | `dbo.PrinterModel` | `PrinterModelId` | 1 | None found inline | `IX_PrinterModel_PrinterTypeId`, `IX_PrinterModel_IsActive` | `sql/erp_full.sql` | 10 |
| 89 | `dbo.PrinterType` | `PrinterTypeId` | 0 | None found inline | `IX_PrinterType_IsActive` | `sql/erp_full.sql` | 9 |
| 90 | `dbo.PrintOrientation` | `OrientationId` | 0 | None found inline | None found | `sql/erp_full.sql` | 4 |
| 91 | `dbo.PrintUnit` | `UnitId` | 0 | None found inline | None found | `sql/erp_full.sql` | 4 |
| 92 | `dbo.ProductBrands` | `Id` | 0 | None found inline | None found | `sql/erp_full.sql` | 10 |
| 93 | `dbo.ProductCategories` | `Id` | 0 | None found inline | None found | `sql/erp_full.sql` | 12 |
| 94 | `dbo.Products` | `Id` | 0 | None found inline | None found | `sql/erp_full.sql` | 27 |
| 95 | `dbo.ProductSubCategories` | `Id` | 0 | None found inline | None found | `sql/erp_full.sql` | 12 |
| 96 | `dbo.Purchase` | `PurchaseId` | 3 | `CompanyId`+`PurchaseNumber` | `IX_Purchase_Company_Date`, `IX_Purchase_Supplier`, `IX_Purchase_AccountingYearId`, `IX_Purchase_TaxId` | `sql/erp_full.sql` | 41 |
| 97 | `dbo.PurchaseItem` | `PurchaseItemId` | 1 | None found inline | `IX_PurchaseItem_PurchaseId`, `IX_PurchaseItem_ProductId`, `IX_PurchaseItem_TaxId`, `IX_PurchaseItem_PurchaseOrderId`, `IX_PurchaseItem_GRNId` | `sql/erp_full.sql` | 49 |
| 98 | `dbo.PurchaseReturn` | `PurchaseReturnId` | 3 | `CompanyId`+`ReturnNumber` | `IX_PurchaseReturn_Company_Date`, `IX_PurchaseReturn_AccountingYearId`, `IX_PurchaseReturn_PaymentTypeId`, `IX_PurchaseReturn_PaymentMethodId` | `sql/erp_full.sql` | 36 |
| 99 | `dbo.PurchaseReturnItem` | `PurchaseReturnItemId` | 1 | None found inline | `IX_PurchaseReturnItem_ReturnId`, `IX_PurchaseReturnItem_ProductId`, `IX_PurchaseReturnItem_TaxId` | `sql/erp_full.sql` | 31 |
| 100 | `dbo.RefreshTokens` | `RefreshTokenId` | 1 | None found inline | `IX_RefreshTokens_Token`, `IX_RefreshTokens_UserId`, `IX_Platform_RefreshTokens_Token`, `IX_Platform_RefreshTokens_UserId` | `sql/platform_schema.sql` | 7 |
| 101 | `dbo.RoleDataScopes` | `Id` | 1 | None found inline | `IX_RoleDataScopes_RoleId`, `IX_RoleDataScopes_ModuleId`, `IX_RoleDataScopes_ScreenId`, `IX_RoleDataScopes_CompanyId`, `IX_RoleDataScopes_BranchId`, `IX_RoleDataScopes_DepartmentId`, `IX_RoleDataScopes_WarehouseId` | `sql/erp_full.sql` | 20 |
| 102 | `dbo.RoleFieldPermissions` | `Id` | 3 | `RoleId`+`ScreenId`+`FieldId` | `IX_RoleFieldPermissions_RoleId`, `IX_RoleFieldPermissions_ScreenId`, `IX_RoleFieldPermissions_FieldId`, `IX_RoleFieldPermissions_IsActive` | `sql/erp_full.sql` | 15 |
| 103 | `dbo.RolePermissions` | `Id` | 7 | `RoleId`+`WorkspaceId`+`DomainId`+`ModuleId`+`SubModuleId`+`ScreenId`+`ActionId` | `IX_RolePermissions_RoleId`, `IX_RolePermissions_RoleId_ScreenId`, `IX_RolePermissions_WorkspaceId`, `IX_RolePermissions_DomainId`, `IX_RolePermissions_ModuleId`, `IX_RolePermissions_SubModuleId`, `IX_RolePermissions_ScreenId`, `IX_RolePermissions_ActionId`, `IX_RolePermissions_IsActive` | `sql/erp_full.sql` | 15 |
| 104 | `dbo.RolePermissionsLegacy` | `RolePermissionId` | 1 | `RoleId`+`PermissionCode` | `IX_RolePermissionsLegacy_RoleId` | `sql/erp_full.sql` | 5 |
| 105 | `dbo.Roles` | `RoleId` | 0 | None found inline | `IX_Roles_IsActive` | `sql/erp_full.sql` | 11 |
| 106 | `dbo.SalesInvoice` | `SalesInvoiceId` | 1 | `CompanyId`+`SalesInvoiceNo` | `IX_SalesInvoice_Company_Date`, `IX_SalesInvoice_CustomerId`, `IX_SalesInvoice_POSSession`, `IX_SalesInvoice_FinancialYearId` | `sql/erp_full.sql` | 37 |
| 107 | `dbo.SalesInvoiceItem` | `SalesInvoiceItemId` | 0 | None found inline | `IX_SalesInvoiceItem_InvoiceId`, `IX_SalesInvoiceItem_ProductId` | `sql/erp_full.sql` | 29 |
| 108 | `dbo.SalesReturn` | `SalesReturnId` | 0 | `CompanyId`+`ReturnNumber` | `IX_SalesReturn_Company_Date`, `IX_SalesReturn_Invoice`, `IX_SalesReturn_Customer` | `sql/erp_full.sql` | 33 |
| 109 | `dbo.SalesReturnItem` | `SalesReturnItemId` | 1 | None found inline | `IX_SalesReturnItem_ReturnId`, `IX_SalesReturnItem_ProductId` | `sql/erp_full.sql` | 27 |
| 110 | `dbo.Screens` | `Id` | 1 | `SubModuleId`+`ScreenCode` | `IX_Screens_SubModuleId`, `IX_Screens_IsActive` | `sql/erp_full.sql` | 12 |
| 111 | `dbo.ServiceCategories` | `ServiceCategoryId` | 0 | `CompanyId`+`Code` | `IX_ServiceCategories_Company` | `sql/erp_full.sql` | 11 |
| 112 | `dbo.Services` | `ServiceId` | 4 | `CompanyId`+`Code` | `IX_Services_Company` | `sql/erp_full.sql` | 16 |
| 113 | `dbo.Settings` | `SettingId` | 0 | None found inline | None found | `sql/platform_schema.sql` | 6 |
| 114 | `dbo.Sources` | `Id` | 0 | None found inline | `IX_Sources_IsActive`, `IX_Sources_SortOrder` | `sql/erp_full.sql` | 11 |
| 115 | `dbo.States` | `StateId` | 1 | `CountryId`+`StateCode` | `IX_States_CountryId`, `IX_States_IsActive` | `sql/erp_full.sql` | 11 |
| 116 | `dbo.Status` | `StatusId` | 0 | None found inline | None found | `sql/erp_full.sql` | 8 |
| 117 | `dbo.Stock` | `StockId` | 0 | `CompanyId`+`BranchId`+`WarehouseId`+`ProductId`+`UnitId` | `IX_Stock_Company_Product`, `IX_Stock_Warehouse` | `sql/erp_full.sql` | 12 |
| 118 | `dbo.StockAdjustment` | `StockAdjustmentId` | 0 | `CompanyId`+`AdjustmentNumber` | `IX_StockAdjustment_Company_Date`, `IX_StockAdjustment_Company_Date` | `sql/erp_full.sql` | 13 |
| 119 | `dbo.StockAdjustmentItem` | `StockAdjustmentItemId` | 1 | None found inline | `IX_StockAdjustmentItem_Header`, `IX_StockAdjustmentItem_Header` | `sql/erp_full.sql` | 7 |
| 120 | `dbo.StockCount` | `StockCountId` | 0 | `CompanyId`+`CountNumber` | `IX_StockCount_Company_Date`, `IX_StockCount_Company_Date` | `sql/erp_full.sql` | 12 |
| 121 | `dbo.StockCountItem` | `StockCountItemId` | 1 | None found inline | `IX_StockCountItem_Header`, `IX_StockCountItem_Header` | `sql/erp_full.sql` | 8 |
| 122 | `dbo.StockTransaction` | `StockTransactionId` | 0 | None found inline | `IX_StockTx_Company_Product`, `IX_StockTx_Reference` | `sql/erp_full.sql` | 17 |
| 123 | `dbo.StockTransfer` | `StockTransferId` | 0 | `CompanyId`+`TransferNumber` | `IX_StockTransfer_Company_Date`, `IX_StockTransfer_Company_Date` | `sql/erp_full.sql` | 13 |
| 124 | `dbo.StockTransferItem` | `StockTransferItemId` | 1 | None found inline | `IX_StockTransferItem_Header`, `IX_StockTransferItem_Header` | `sql/erp_full.sql` | 7 |
| 125 | `dbo.Stores` | `StoreId` | 2 | `CompanyId`+`BranchId`+`StoreCode` | `IX_Stores_Company`, `IX_Stores_Branch`, `IX_Stores_Code` | `sql/erp_full.sql` | 15 |
| 126 | `dbo.StoreTypes` | `Id` | 0 | None found inline | `IX_StoreTypes_IsActive`, `IX_StoreTypes_SortOrder` | `sql/erp_full.sql` | 11 |
| 127 | `dbo.SubModules` | `Id` | 1 | `ModuleId`+`SubModuleCode` | `IX_SubModules_ModuleId`, `IX_SubModules_IsActive` | `sql/erp_full.sql` | 12 |
| 128 | `dbo.Subscriptions` | `SubscriptionId` | 2 | None found inline | `IX_Subscriptions_TenantId`, `IX_Subscriptions_Status` | `sql/platform_schema.sql` | 12 |
| 129 | `dbo.Tag` | `TagId` | 0 | `TagName` | `IX_Tag_IsActive` | `sql/erp_full.sql` | 8 |
| 130 | `dbo.Taxes` | `Id` | 1 | None found inline | None found | `sql/erp_full.sql` | 16 |
| 131 | `dbo.TaxTypeSystems` | `Id` | 0 | None found inline | None found | `sql/erp_full.sql` | 7 |
| 132 | `dbo.TenantConfiguration` | `Id` | 0 | None found inline | `IX_TenantConfiguration_Tenant` | `sql/erp_full.sql` | 19 |
| 133 | `dbo.TenantConnections` | `ConnectionId` | 1 | None found inline | `IX_TenantConnections_TenantId` | `sql/platform_schema.sql` | 8 |
| 134 | `dbo.TenantMigrationHistory` | `TenantMigrationHistoryId` | 0 | None found inline | `IX_TenantMigrationHistory_TenantId`, `IX_TenantMigrationHistory_ExecutionId`, `UQ_TenantMigrationHistory_TenantRunning` | `sql/platform_schema.sql` | 15 |
| 135 | `dbo.Tenants` | `TenantId` | 0 | None found inline | `UQ_Tenants_TenantCode`, `UQ_Tenants_DatabaseName`, `IX_Tenants_Status`, `UQ_Tenants_AdminUsername` | `sql/platform_schema.sql` | 15 |
| 136 | `dbo.TenantUserMaps` | `TenantUserId` | 0 | None found inline | `UQ_TenantUserMaps_Username` | `sql/platform_schema.sql` | 4 |
| 137 | `dbo.TimeZones` | `TimeZoneId` | 0 | None found inline | `IX_TimeZones_IsActive` | `sql/erp_full.sql` | 10 |
| 138 | `dbo.UnitConversions` | `UnitConversionId` | 3 | None found inline | `IX_UnitConversions_Company`, `IX_UnitConversions_Product` | `sql/erp_full.sql` | 12 |
| 139 | `dbo.Units` | `Id` | 0 | None found inline | None found | `sql/erp_full.sql` | 11 |
| 140 | `dbo.UserDataScopeOverrides` | `Id` | 1 | None found inline | `IX_UserDataScopeOverrides_UserId`, `IX_UserDataScopeOverrides_ModuleId`, `IX_UserDataScopeOverrides_ScreenId`, `IX_UserDataScopeOverrides_ScopeType` | `sql/erp_full.sql` | 16 |
| 141 | `dbo.UserFieldPermissions` | `Id` | 3 | `UserId`+`ScreenId`+`FieldId` | `IX_UserFieldPermissions_UserId`, `IX_UserFieldPermissions_ScreenId`, `IX_UserFieldPermissions_FieldId`, `IX_UserFieldPermissions_IsActive` | `sql/erp_full.sql` | 14 |
| 142 | `dbo.UserPermissionOverrides` | `Id` | 7 | `UserId`+`WorkspaceId`+`DomainId`+`ModuleId`+`SubModuleId`+`ScreenId`+`ActionId` | `IX_UserPermissionOverrides_UserId`, `IX_UserPermissionOverrides_UserId_IsActive`, `IX_UserPermissionOverrides_WorkspaceId`, `IX_UserPermissionOverrides_DomainId`, `IX_UserPermissionOverrides_ModuleId`, `IX_UserPermissionOverrides_ScreenId`, `IX_UserPermissionOverrides_ActionId` | `sql/erp_full.sql` | 18 |
| 143 | `dbo.UserRoles` | `UserRoleId` | 2 | `UserId`+`RoleId` | `IX_UserRoles_UserId`, `IX_UserRoles_RoleId` | `sql/erp_full.sql` | 5 |
| 144 | `dbo.Users` | `UserId` | 1 | `CompanyId`+`Username` | `IX_Users_CompanyId_Username`, `IX_Users_Status` | `sql/erp_full.sql` | 15 |
| 145 | `dbo.Warehouses` | `Id` | 5 | `CompanyId`+`BranchId`+`WarehouseCode` | `IX_Warehouses_Company`, `IX_Warehouses_Branch`, `IX_Warehouses_Type`, `IX_Warehouses_Parent` | `sql/erp_full.sql` | 20 |
| 146 | `dbo.WarehouseTypes` | `WarehouseTypeId` | 0 | None found inline | `IX_WarehouseTypes_SortOrder`, `IX_WarehouseTypes_IsActive` | `sql/erp_full.sql` | 11 |
| 147 | `dbo.WorkflowPermissions` | `Id` | 4 | `RoleId`+`ModuleId`+`SubModuleId`+`ScreenId` | `IX_WorkflowPermissions_RoleId`, `IX_WorkflowPermissions_ModuleId`, `IX_WorkflowPermissions_SubModuleId`, `IX_WorkflowPermissions_ScreenId` | `sql/erp_full.sql` | 13 |
| 148 | `dbo.Workspaces` | `Id` | 0 | None found inline | `IX_Workspaces_IsActive`, `IX_Workspaces_SortOrder` | `sql/erp_full.sql` | 9 |

## 12. Database Column Dictionary

The preferred full-install schema definition is used for each table (tenant `erp_full.sql`; platform `platform_schema.sql`). Migration-only changes are described under §23. Column meaning is UNKNOWN unless the source explicitly documents it; no business meaning is inferred from a name.

| Table | Column | Type | Nullable | PK | Inline FK | Default | Identity | Meaning |
|---|---|---|---|---|---|---|---|---|
| `dbo.Actions` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Actions` | `ActionCode` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Actions` | `ActionName` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Actions` | `DisplayOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Actions` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Address` | `AddressId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Address` | `AddressTypeId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Address` | `AddressLine1` | `VARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Address` | `AddressLine2` | `VARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Address` | `AddressLine3` | `VARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Address` | `AddressLine4` | `VARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Address` | `AddressLine5` | `VARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Address` | `Landmark` | `VARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Address` | `CityId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Address` | `StateId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Address` | `CountryId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Address` | `PostalCode` | `VARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Address` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Address` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Address` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Address` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Address` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.AddressTypes` | `AddressTypeId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.AddressTypes` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.AddressTypes` | `Description` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.AddressTypes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.AddressTypes` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.AddressTypes` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.AddressTypes` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.AddressTypes` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.AddressTypes` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.ApiIdempotency` | `IdempotencyKey` | `NVARCHAR(128)` | No | Yes | No inline FK | — | No | UNKNOWN |
| `dbo.ApiIdempotency` | `CompanyId` | `BIGINT` | No | Yes | No inline FK | — | No | UNKNOWN |
| `dbo.ApiIdempotency` | `Endpoint` | `NVARCHAR(200)` | No | Yes | No inline FK | — | No | UNKNOWN |
| `dbo.ApiIdempotency` | `ReferenceId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ApiIdempotency` | `ResponseJson` | `NVARCHAR(MAX)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ApiIdempotency` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.ApplicationSettings` | `SettingId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.ApplicationSettings` | `SettingKey` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ApplicationSettings` | `SettingValue` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ApplicationSettings` | `Description` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ApplicationSettings` | `UpdatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ApplicationSettings` | `UpdatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.AuditLogs` | `AuditLogId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.AuditLogs` | `TenantId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.AuditLogs` | `EntityName` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.AuditLogs` | `EntityId` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.AuditLogs` | `Action` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.AuditLogs` | `PerformedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.AuditLogs` | `PerformedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.AuditLogs` | `OldValues` | `NVARCHAR(MAX)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.AuditLogs` | `NewValues` | `NVARCHAR(MAX)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.AuditLogs` | `IpAddress` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Barcodes` | `BarcodeId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Barcodes` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Barcodes` | `ProductId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Barcodes` | `UnitId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Barcodes` | `Barcode` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Barcodes` | `BarcodeType` | `VARCHAR(30)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Barcodes` | `IsPrimary` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Barcodes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Barcodes` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Barcodes` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Barcodes` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Barcodes` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `Id` | `INT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Branches` | `CompanyId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `BranchCode` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `BranchName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `ShortName` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `BranchTypeId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `ParentBranchId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `ManagerEmployeeId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `DefaultWarehouseId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `GSTNumber` | `NVARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `RegistrationNumber` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `IsHeadOffice` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `IsSalesBranch` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `IsPurchaseBranch` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `IsServiceBranch` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `SortOrder` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `IsActive` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `IsBlocked` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `IsDeleted` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `CreatedBy` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `ModifiedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Branches` | `ModifiedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BranchTypes` | `BranchTypeId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.BranchTypes` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.BranchTypes` | `Code` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.BranchTypes` | `Description` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BranchTypes` | `SortOrder` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.BranchTypes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.BranchTypes` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BranchTypes` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.BranchTypes` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BranchTypes` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.BranchTypes` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.BusinessPartnerRoles` | `BusinessPartnerRoleId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.BusinessPartnerRoles` | `Code` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartnerRoles` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartnerRoles` | `Description` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartnerRoles` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.BusinessPartnerRoles` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartnerRoles` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.BusinessPartnerRoles` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartnerRoles` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.BusinessPartnerRoles` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.BusinessPartners` | `Id` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.BusinessPartners` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartners` | `PartnerCode` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartners` | `PartnerName` | `VARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartners` | `PatnerRoleIds` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartners` | `ContactPerson` | `VARCHAR(150)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartners` | `MobileNo` | `VARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartners` | `Email` | `VARCHAR(150)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartners` | `TaxRegistrationNo` | `VARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartners` | `CreditLimit` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.BusinessPartners` | `CreditDays` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.BusinessPartners` | `PaymentTermId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartners` | `CurrencyId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartners` | `PriceListId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartners` | `Notes` | `VARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartners` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.BusinessPartners` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartners` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.BusinessPartners` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessPartners` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessTypes` | `BusinessTypeId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.BusinessTypes` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessTypes` | `Description` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessTypes` | `SortOrder` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.BusinessTypes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.BusinessTypes` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessTypes` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.BusinessTypes` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.BusinessTypes` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.BusinessTypes` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Cities` | `CityId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Cities` | `CountryId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Cities` | `StateId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Cities` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Cities` | `PostalCode` | `NVARCHAR(10)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Cities` | `Latitude` | `DECIMAL(10,7)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Cities` | `Longitude` | `DECIMAL(10,7)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Cities` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Cities` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Cities` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Cities` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Cities` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Cities` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Companies` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Companies` | `CompanyCode` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `CompanyName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `ShortName` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `Abbreviation` | `NVARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `BusinessTypeId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `IndustryTypeId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `GSTRegistrationTypeId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `GSTNumber` | `NVARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `PANNumber` | `NVARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `TANNumber` | `NVARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `CINNumber` | `NVARCHAR(30)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `RegistrationNumber` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `CurrencyId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `LanguageId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `TimeZoneId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Companies` | `IsBlocked` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Companies` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Companies` | `LastLoginDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `CreatedBy` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Companies` | `ModifiedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Companies` | `ModifiedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.CompanyGroups` | `CompanyGroupId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.CompanyGroups` | `GroupCode` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.CompanyGroups` | `GroupName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.CompanyGroups` | `ShortName` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.CompanyGroups` | `Description` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.CompanyGroups` | `ParentGroupId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.CompanyGroups` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.CompanyGroups` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.CompanyGroups` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.CompanyGroups` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.CompanyGroups` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.CompanyGroups` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Contact` | `ContactId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Contact` | `ContactTypeId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Contact` | `ContactName` | `VARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Contact` | `Designation` | `VARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Contact` | `Email` | `VARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Contact` | `Mobile` | `VARCHAR(30)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Contact` | `Phone` | `VARCHAR(30)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Contact` | `Website` | `VARCHAR(300)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Contact` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Contact` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Contact` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Contact` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Contact` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ContactTypes` | `ContactTypeId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.ContactTypes` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ContactTypes` | `Description` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ContactTypes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.ContactTypes` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ContactTypes` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.ContactTypes` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ContactTypes` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.ContactTypes` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.CounterAssignments` | `AssignmentId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.CounterAssignments` | `Id` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.CounterAssignments` | `CompanyId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.CounterAssignments` | `BranchId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.CounterAssignments` | `StoreId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.CounterAssignments` | `CounterId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.CounterAssignments` | `OperatorId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.CounterAssignments` | `IsPrimary` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.CounterAssignments` | `ValidFrom` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.CounterAssignments` | `ValidTo` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.CounterAssignments` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.CounterAssignments` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.CounterAssignments` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.CounterAssignments` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.CounterAssignments` | `UpdatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.CounterAssignments` | `UpdatedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Counters` | `CounterId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Counters` | `StoreId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Counters` | `CounterCode` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Counters` | `CounterName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Counters` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Counters` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Counters` | `CreatedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Counters` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Counters` | `UpdatedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Counters` | `UpdatedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Countries` | `CountryId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Countries` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Countries` | `ISOCode2` | `CHAR(2)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Countries` | `ISOCode3` | `CHAR(3)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Countries` | `PhoneCode` | `NVARCHAR(10)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Countries` | `CurrencyCode` | `NVARCHAR(10)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Countries` | `Nationality` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Countries` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Countries` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Countries` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Countries` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Countries` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Countries` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Coupons` | `CouponId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Coupons` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Coupons` | `OfferId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Coupons` | `Code` | `VARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Coupons` | `Name` | `VARCHAR(150)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Coupons` | `UsageLimit` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Coupons` | `UsagePerCustomer` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Coupons` | `UsedCount` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Coupons` | `StartDate` | `DATE` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Coupons` | `EndDate` | `DATE` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Coupons` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Coupons` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Coupons` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Coupons` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Coupons` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Currencies` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Currencies` | `CurrencyCode` | `NVARCHAR(10)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Currencies` | `CurrencyName` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Currencies` | `Symbol` | `NVARCHAR(10)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Currencies` | `ISOCode` | `NVARCHAR(10)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Currencies` | `DecimalPlaces` | `TINYINT` | No | No | No inline FK | 2 | No | UNKNOWN |
| `dbo.Currencies` | `IsBaseCurrency` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Currencies` | `SortOrder` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Currencies` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Currencies` | `CreatedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Currencies` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Currencies` | `ModifiedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Currencies` | `ModifiedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `Id` | `INT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Departments` | `CompanyId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `BranchId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `DepartmentCode` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `DepartmentName` | `NVARCHAR(150)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `ShortName` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `ParentDepartmentId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `ManagerEmployeeId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `SortOrder` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `IsActive` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `IsBlocked` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `IsDeleted` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `CreatedBy` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `ModifiedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Departments` | `ModifiedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `Id` | `INT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Designations` | `CompanyId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `DepartmentId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `DesignationCode` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `DesignationName` | `NVARCHAR(150)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `ShortName` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `LevelNo` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `Grade` | `NVARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `Description` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `SortOrder` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `IsDefault` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `IsActive` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `IsBlocked` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `IsDeleted` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `CreatedBy` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `ModifiedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Designations` | `ModifiedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `DiscountRuleId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.DiscountRules` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `Code` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `Name` | `VARCHAR(150)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `ProductId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `ServiceId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `ProductCategoryId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `PriceListId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `DiscountType` | `VARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `DiscountValue` | `DECIMAL(18,4)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `MinimumQuantity` | `DECIMAL(18,4)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `MinimumAmount` | `DECIMAL(18,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `MaximumDiscount` | `DECIMAL(18,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `EffectiveFrom` | `DATE` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `EffectiveTo` | `DATE` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.DiscountRules` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.DiscountRules` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DiscountRules` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DocumentTypes` | `DocumentTypeId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.DocumentTypes` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.DocumentTypes` | `Description` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DocumentTypes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.DocumentTypes` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DocumentTypes` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.DocumentTypes` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.DocumentTypes` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.DocumentTypes` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Domains` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Domains` | `WorkspaceId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Domains` | `DomainCode` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Domains` | `DomainName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Domains` | `Icon` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Domains` | `SortOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Domains` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Domains` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Domains` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Employees` | `Id` | `INT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Employees` | `CompanyId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `BranchId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `DepartmentId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `DesignationId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `EmployeeCode` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `EmployeeNumber` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `FirstName` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `MiddleName` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `LastName` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `DisplayName` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `GenderId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `MaritalStatusId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `DateOfBirth` | `DATE` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `DateOfJoining` | `DATE` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `DateOfLeaving` | `DATE` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `OfficialEmail` | `NVARCHAR(150)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `PersonalEmail` | `NVARCHAR(150)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `MobileNo` | `NVARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `AlternateMobileNo` | `NVARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `ReportingManagerId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `EmploymentTypeId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `IsActive` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `IsBlocked` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `IsDeleted` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `CreatedBy` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `ModifiedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Employees` | `ModifiedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.EmploymentTypes` | `EmploymentTypeId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.EmploymentTypes` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.EmploymentTypes` | `Code` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.EmploymentTypes` | `Description` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.EmploymentTypes` | `SortOrder` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.EmploymentTypes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.EmploymentTypes` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.EmploymentTypes` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.EmploymentTypes` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.EmploymentTypes` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.EmploymentTypes` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Entity` | `EntityId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Entity` | `EntityType` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Entity` | `EntityCode` | `VARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Entity` | `EntityName` | `VARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Entity` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Entity` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Entity` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Entity` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Entity` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityAddress` | `EntityAddressId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.EntityAddress` | `EntityId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityAddress` | `AddressId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityAddress` | `IsPrimary` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.EntityAddress` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.EntityAddress` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityAddress` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.EntityContact` | `EntityContactId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.EntityContact` | `EntityId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityContact` | `ContactId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityContact` | `IsPrimary` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.EntityContact` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.EntityContact` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityContact` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.EntityFile` | `EntityFileId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.EntityFile` | `EntityId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityFile` | `FileId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityFile` | `FileType` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityFile` | `IsPrimary` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.EntityFile` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.EntityFile` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityFile` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.EntityNote` | `EntityNoteId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.EntityNote` | `EntityId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityNote` | `NoteId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityNote` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.EntityNote` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityNote` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.EntityTag` | `EntityTagId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.EntityTag` | `EntityId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityTag` | `TagId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityTag` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.EntityTag` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.EntityTag` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.FieldPermissions` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.FieldPermissions` | `RoleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.FieldPermissions` | `PermissionModuleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.FieldPermissions` | `FieldName` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.FieldPermissions` | `CanView` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.FieldPermissions` | `CanEdit` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.FieldPermissions` | `IsMandatory` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.FieldPermissions` | `IsHidden` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.FieldPermissions` | `Scope` | `NVARCHAR(20)` | No | No | No inline FK | 'Company' | No | UNKNOWN |
| `dbo.FieldPermissions` | `ScopeId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.FieldPermissions` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.FieldPermissions` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.FieldPermissions` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.FieldPermissions` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Fields` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Fields` | `ScreenId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Fields` | `FieldCode` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Fields` | `FieldName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Fields` | `DisplayName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Fields` | `DataType` | `NVARCHAR(50)` | No | No | No inline FK | 'text' | No | UNKNOWN |
| `dbo.Fields` | `DisplayOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Fields` | `DefaultValue` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Fields` | `IsSystemField` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Fields` | `IsRequired` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Fields` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Fields` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Fields` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Files` | `FileId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Files` | `FileName` | `VARCHAR(255)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Files` | `OriginalFileName` | `VARCHAR(255)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Files` | `BucketName` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Files` | `ObjectKey` | `VARCHAR(1000)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Files` | `ContentType` | `VARCHAR(150)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Files` | `Extension` | `VARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Files` | `FileSize` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Files` | `StorageProvider` | `VARCHAR(30)` | No | No | No inline FK | 'MINIO' | No | UNKNOWN |
| `dbo.Files` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Files` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Files` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Files` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Files` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.FinancialYear` | `FinancialYearId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | No | No inline FK | — | Yes | UNKNOWN |
| `dbo.FinancialYear` | `CompanyId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.FinancialYear` | `Code` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.FinancialYear` | `Name` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.FinancialYear` | `StartDate` | `DATE` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.FinancialYear` | `EndDate` | `DATE` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.FinancialYear` | `IsCurrent` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.FinancialYear` | `IsClosed` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.FinancialYear` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.FinancialYear` | `CreatedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.FinancialYear` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.FinancialYear` | `ModifiedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.FinancialYear` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Genders` | `GenderId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Genders` | `Name` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Genders` | `Code` | `NVARCHAR(10)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Genders` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Genders` | `SortOrder` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.GSTRegistrationTypes` | `GSTRegistrationTypeId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.GSTRegistrationTypes` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.GSTRegistrationTypes` | `Description` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.GSTRegistrationTypes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.GSTRegistrationTypes` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.GSTRegistrationTypes` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.GSTRegistrationTypes` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.GSTRegistrationTypes` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.GSTRegistrationTypes` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.HsnSacs` | `HsnSacId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.HsnSacs` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.HsnSacs` | `Code` | `VARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.HsnSacs` | `GovernmentCode` | `VARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.HsnSacs` | `Name` | `VARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.HsnSacs` | `HsnSacType` | `VARCHAR(10)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.HsnSacs` | `Description` | `VARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.HsnSacs` | `TaxId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.HsnSacs` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.HsnSacs` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.HsnSacs` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.HsnSacs` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.HsnSacs` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ImportLogs` | `Id` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.ImportLogs` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ImportLogs` | `BranchId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ImportLogs` | `ImportType` | `VARCHAR(30)` | No | No | No inline FK | 'MASTER' | No | UNKNOWN |
| `dbo.ImportLogs` | `ModuleName` | `VARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ImportLogs` | `EntityName` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ImportLogs` | `FileName` | `VARCHAR(255)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ImportLogs` | `FileType` | `VARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ImportLogs` | `TotalRows` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.ImportLogs` | `SuccessRows` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.ImportLogs` | `FailedRows` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.ImportLogs` | `Status` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ImportLogs` | `ErrorMessage` | `VARCHAR(2000)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ImportLogs` | `ImportedBy` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ImportLogs` | `ImportedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.IndustryTypes` | `IndustryTypeId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.IndustryTypes` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.IndustryTypes` | `Description` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.IndustryTypes` | `SortOrder` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.IndustryTypes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.IndustryTypes` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.IndustryTypes` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.IndustryTypes` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.IndustryTypes` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.IndustryTypes` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceFont` | `FontId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceFont` | `Code` | `VARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceFont` | `Name` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceFont` | `FontFamily` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceFont` | `FontFileId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceFont` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceFont` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceFont` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.InvoiceFont` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceFont` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoicePaperSize` | `PaperSizeId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoicePaperSize` | `Code` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoicePaperSize` | `Name` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoicePaperSize` | `Width` | `DECIMAL(10,2)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoicePaperSize` | `Height` | `DECIMAL(10,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoicePaperSize` | `Unit` | `VARCHAR(10)` | No | No | No inline FK | 'MM' | No | UNKNOWN |
| `dbo.InvoicePaperSize` | `IsThermal` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoicePaperSize` | `IsCustom` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoicePaperSize` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoicePaperSize` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoicePaperSize` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.InvoicePaperSize` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoicePaperSize` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `InvoiceTemplateId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceTemplate` | `CompanyId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `TemplateCategoryId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `InvoiceTypeId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `PaperSizeId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `OrientationId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `Code` | `VARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `Name` | `VARCHAR(150)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `Description` | `VARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `Width` | `DECIMAL(10,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `Height` | `DECIMAL(10,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `IsDefault` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplate` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateAssignment` | `AssignmentId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceTemplateAssignment` | `InvoiceTemplateId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateAssignment` | `CompanyId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateAssignment` | `IndustryTypeId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateAssignment` | `InvoiceTypeId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateAssignment` | `PaperSizeId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateAssignment` | `IsDefault` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplateAssignment` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceTemplateAssignment` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateAssignment` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.InvoiceTemplateAssignment` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateAssignment` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateCategory` | `TemplateCategoryId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceTemplateCategory` | `Code` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateCategory` | `Name` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateCategory` | `Description` | `VARCHAR(300)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateCategory` | `DisplayOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplateCategory` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceTemplateCategory` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateCategory` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.InvoiceTemplateCategory` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateCategory` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateComponent` | `ComponentId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceTemplateComponent` | `Code` | `VARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateComponent` | `Name` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateComponent` | `ComponentType` | `VARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateComponent` | `Description` | `VARCHAR(300)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateComponent` | `DisplayOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplateComponent` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceTemplateComponent` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateComponent` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.InvoiceTemplateComponent` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateComponent` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateElement` | `ElementId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceTemplateElement` | `SectionId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateElement` | `ComponentId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateElement` | `ElementType` | `VARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateElement` | `ElementName` | `VARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateElement` | `X` | `DECIMAL(10,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateElement` | `Y` | `DECIMAL(10,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateElement` | `Width` | `DECIMAL(10,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateElement` | `Height` | `DECIMAL(10,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateElement` | `DisplayOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplateElement` | `IsVisible` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceTemplateField` | `TemplateFieldId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceTemplateField` | `ElementId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateField` | `VariableId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateField` | `FieldName` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateField` | `BindingPath` | `VARCHAR(300)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateField` | `Label` | `VARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateField` | `IsVisible` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceTemplateItemColumn` | `ItemColumnId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceTemplateItemColumn` | `ElementId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateItemColumn` | `FieldName` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateItemColumn` | `HeaderText` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateItemColumn` | `DisplayOrder` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateItemColumn` | `Width` | `DECIMAL(10,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateItemColumn` | `Alignment` | `VARCHAR(20)` | No | No | No inline FK | 'LEFT' | No | UNKNOWN |
| `dbo.InvoiceTemplateItemColumn` | `IsVisible` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceTemplatePrinter` | `TemplatePrinterId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceTemplatePrinter` | `InvoiceTemplateId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplatePrinter` | `PaperSizeId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplatePrinter` | `PrinterTypeId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplatePrinter` | `PrinterModelId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplatePrinter` | `PrinterName` | `VARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplatePrinter` | `IsDefault` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplatePrinter` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceTemplatePrinter` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplatePrinter` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.InvoiceTemplatePrinter` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplatePrinter` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `PrintSettingId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `TemplatePrinterId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `MarginTop` | `DECIMAL(10,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `MarginRight` | `DECIMAL(10,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `MarginBottom` | `DECIMAL(10,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `MarginLeft` | `DECIMAL(10,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `Scale` | `DECIMAL(6,2)` | No | No | No inline FK | 100 | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `Copies` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `AutoFit` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `CutPaper` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `PrintHeader` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `PrintFooter` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplatePrintSetting` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateSection` | `SectionId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceTemplateSection` | `TemplateVersionId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateSection` | `SectionCode` | `VARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateSection` | `SectionName` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateSection` | `DisplayOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplateSection` | `X` | `DECIMAL(10,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateSection` | `Y` | `DECIMAL(10,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateSection` | `Width` | `DECIMAL(10,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateSection` | `Height` | `DECIMAL(10,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateSection` | `IsVisible` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `StyleId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `ElementId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `FontId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `FontSize` | `DECIMAL(6,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `FontWeight` | `VARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `FontStyle` | `VARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `TextAlign` | `VARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `VerticalAlign` | `VARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `TextColor` | `VARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `BackgroundColor` | `VARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `BorderColor` | `VARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `PaddingTop` | `DECIMAL(8,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `PaddingRight` | `DECIMAL(8,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `PaddingBottom` | `DECIMAL(8,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `PaddingLeft` | `DECIMAL(8,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `BorderTop` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `BorderRight` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `BorderBottom` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplateStyle` | `BorderLeft` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplateVariable` | `VariableId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceTemplateVariable` | `Code` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateVariable` | `Name` | `VARCHAR(150)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateVariable` | `BindingPath` | `VARCHAR(300)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateVariable` | `DataType` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateVariable` | `Category` | `VARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateVariable` | `Description` | `VARCHAR(300)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateVariable` | `IsCollection` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplateVariable` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceTemplateVariable` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateVariable` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.InvoiceTemplateVariable` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateVariable` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateVersion` | `TemplateVersionId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceTemplateVersion` | `InvoiceTemplateId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateVersion` | `VersionNumber` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateVersion` | `TemplateJson` | `NVARCHAR(MAX)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateVersion` | `Status` | `VARCHAR(20)` | No | No | No inline FK | 'DRAFT' | No | UNKNOWN |
| `dbo.InvoiceTemplateVersion` | `IsPublished` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceTemplateVersion` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateVersion` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.InvoiceTemplateVersion` | `PublishedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceTemplateVersion` | `PublishedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceType` | `InvoiceTypeId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.InvoiceType` | `Code` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceType` | `Name` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceType` | `Description` | `VARCHAR(300)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceType` | `DisplayOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.InvoiceType` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.InvoiceType` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceType` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.InvoiceType` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.InvoiceType` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Languages` | `LanguageId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Languages` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Languages` | `Code` | `NVARCHAR(10)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Languages` | `CultureCode` | `NVARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Languages` | `IsRTL` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Languages` | `IsDefault` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Languages` | `SortOrder` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Languages` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Languages` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Languages` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Languages` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Languages` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Languages` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.MaritalStatuses` | `MaritalStatusId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.MaritalStatuses` | `Name` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.MaritalStatuses` | `Code` | `NVARCHAR(10)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.MaritalStatuses` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.MaritalStatuses` | `SortOrder` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Migrations` | `MigrationId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Migrations` | `Version` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Migrations` | `MigrationCode` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Migrations` | `MigrationName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Migrations` | `ScriptName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Migrations` | `Checksum` | `NVARCHAR(128)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Migrations` | `Description` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Migrations` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Migrations` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Migrations` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Migrations` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Migrations` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Migrations` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.ModulePermissions` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.ModulePermissions` | `RoleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ModulePermissions` | `PermissionModuleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ModulePermissions` | `PermissionActionId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ModulePermissions` | `Scope` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ModulePermissions` | `ScopeId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ModulePermissions` | `GrantedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ModulePermissions` | `GrantedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.ModulePermissions` | `IsRevoked` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.ModulePermissions` | `RevokedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ModulePermissions` | `RevokedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Modules` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Modules` | `DomainId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Modules` | `ModuleCode` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Modules` | `ModuleName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Modules` | `Icon` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Modules` | `RouteUrl` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Modules` | `SortOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Modules` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Modules` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Modules` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Note` | `NoteId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Note` | `NoteText` | `VARCHAR(2000)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Note` | `NoteType` | `VARCHAR(30)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Note` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Note` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Note` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Note` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Note` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OfferDetails` | `OfferDetailId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.OfferDetails` | `OfferId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.OfferDetails` | `ProductId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OfferDetails` | `ServiceId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OfferDetails` | `ProductCategoryId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OfferDetails` | `MinimumQuantity` | `DECIMAL(18,4)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OfferDetails` | `FreeQuantity` | `DECIMAL(18,4)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OfferDetails` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.OfferDetails` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OfferDetails` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.OfferDetails` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OfferDetails` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OfferDetails` | `ProductId` | `IS` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `OfferId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Offers` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `Code` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `Name` | `VARCHAR(150)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `OfferType` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `DiscountType` | `VARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `DiscountValue` | `DECIMAL(18,4)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `MinimumQuantity` | `DECIMAL(18,4)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `MinimumAmount` | `DECIMAL(18,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `MaximumDiscount` | `DECIMAL(18,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `StartDate` | `DATE` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `EndDate` | `DATE` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Offers` | `Description` | `VARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Offers` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Offers` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Operators` | `OperatorId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Operators` | `Id` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Operators` | `CompanyId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Operators` | `BranchId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Operators` | `UserId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Operators` | `OperatorTypeId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Operators` | `OperatorCode` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Operators` | `OperatorName` | `NVARCHAR(150)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Operators` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Operators` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Operators` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Operators` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Operators` | `UpdatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Operators` | `UpdatedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OperatorTypes` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.OperatorTypes` | `Code` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.OperatorTypes` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.OperatorTypes` | `Description` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OperatorTypes` | `SortOrder` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.OperatorTypes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.OperatorTypes` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.OperatorTypes` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OperatorTypes` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.OperatorTypes` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OperatorTypes` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.OrganizationTypes` | `OrganizationTypeId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.OrganizationTypes` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.OrganizationTypes` | `Code` | `NVARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.OrganizationTypes` | `Description` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OrganizationTypes` | `SortOrder` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.OrganizationTypes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.OrganizationTypes` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OrganizationTypes` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.OrganizationTypes` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.OrganizationTypes` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.OrganizationTypes` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Payment` | `PaymentId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Payment` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Payment` | `PaymentNo` | `NVARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Payment` | `PaymentDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Payment` | `PaymentTypeID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Payment` | `PaymentMethodID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Payment` | `ReferenceType` | `NVARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Payment` | `ReferenceId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Payment` | `BusinessPartnerId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Payment` | `Amount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Payment` | `ReferenceNo` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Payment` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Payment` | `StatusID` | `BIGINT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Payment` | `CreatedByUserID` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Payment` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Payment` | `UpdatedByUserID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Payment` | `UpdatedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentAllocation` | `PaymentAllocationId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PaymentAllocation` | `PaymentId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentAllocation` | `ReferenceType` | `NVARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentAllocation` | `ReferenceId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentAllocation` | `AllocatedAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PaymentAllocation` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.PaymentMethod` | `PaymentMethodId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PaymentMethod` | `Code` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethod` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethod` | `PaymentCategory` | `VARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethod` | `IsCash` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PaymentMethod` | `IsCredit` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PaymentMethod` | `RequiresReferenceNo` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PaymentMethod` | `DisplayOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PaymentMethod` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.PaymentMethod` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `PaymentMethodDetailId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | No | No inline FK | — | Yes | UNKNOWN |
| `dbo.PaymentMethodDetails` | `PaymentMethodId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `Code` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `DisplayName` | `NVARCHAR(150)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `UPIId` | `NVARCHAR(150)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `BankName` | `NVARCHAR(150)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `AccountNumber` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `IFSCCode` | `NVARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `TerminalName` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `CashCounterName` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `ReferenceValue` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `IsDefault` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `DisplayOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `CreatedByUserId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `UpdatedByUserId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentMethodDetails` | `UpdatedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentType` | `PaymentTypeId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PaymentType` | `Code` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentType` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PaymentType` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.PaymentType` | `DisplayOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PaymentType` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.PermissionActions` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PermissionActions` | `Code` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PermissionActions` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PermissionActions` | `SortOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PermissionActions` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.PermissionActions` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PermissionActions` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.PermissionActions` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PermissionActions` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.PermissionModules` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PermissionModules` | `Code` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PermissionModules` | `Name` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PermissionModules` | `ParentId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PermissionModules` | `Level` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PermissionModules` | `SortOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PermissionModules` | `IsVisible` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.PermissionModules` | `Icon` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PermissionModules` | `RoutePath` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PermissionModules` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PermissionModules` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.PermissionModules` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PermissionModules` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.PermissionModules` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Plans` | `PlanId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Plans` | `PlanCode` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Plans` | `PlanName` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Plans` | `Description` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Plans` | `CountryCode` | `NVARCHAR(50)` | No | No | No inline FK | 'US' | No | UNKNOWN |
| `dbo.Plans` | `CountryName` | `NVARCHAR(100)` | No | No | No inline FK | 'United States' | No | UNKNOWN |
| `dbo.Plans` | `CurrencyCode` | `NVARCHAR(10)` | No | No | No inline FK | 'USD' | No | UNKNOWN |
| `dbo.Plans` | `MonthlyPrice` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Plans` | `AnnualPrice` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Plans` | `MaxUsers` | `INT` | No | No | No inline FK | 5 | No | UNKNOWN |
| `dbo.Plans` | `MaxCompanies` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Plans` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Plans` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Plans` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Plans` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Plans` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Plans` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PlatformUsers` | `PlatformUserId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PlatformUsers` | `Username` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PlatformUsers` | `Email` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PlatformUsers` | `FullName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PlatformUsers` | `PasswordHash` | `NVARCHAR(255)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PlatformUsers` | `Role` | `NVARCHAR(50)` | No | No | No inline FK | 'PlatformAdmin' | No | UNKNOWN |
| `dbo.PlatformUsers` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.PlatformUsers` | `LastLoginDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PlatformUsers` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PlatformUsers` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.PlatformUsers` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PlatformUsers` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.PlatformUsers` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.POSCashMovement` | `POSCashMovementId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.POSCashMovement` | `POSSessionId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSCashMovement` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSCashMovement` | `BranchId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSCashMovement` | `Direction` | `NVARCHAR(10)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSCashMovement` | `Amount` | `DECIMAL(18,2)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSCashMovement` | `Reason` | `NVARCHAR(300)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSCashMovement` | `ReferenceNo` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSCashMovement` | `MovementDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.POSCashMovement` | `CreatedByUserID` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSCashMovement` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.POSHoldBill` | `POSHoldBillId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.POSHoldBill` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSHoldBill` | `BranchId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSHoldBill` | `StoreId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSHoldBill` | `CounterId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSHoldBill` | `HoldNumber` | `NVARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSHoldBill` | `HoldDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.POSHoldBill` | `CustomerId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSHoldBill` | `CustomerName` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSHoldBill` | `ItemCount` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.POSHoldBill` | `TotalAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.POSHoldBill` | `CartJson` | `NVARCHAR(MAX)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSHoldBill` | `Status` | `NVARCHAR(20)` | No | No | No inline FK | 'HELD', -- HELD \| RECALLED \| CANCELLED | No | UNKNOWN |
| `dbo.POSHoldBill` | `CreatedByUserID` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSHoldBill` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.POSHoldBill` | `RecalledByUserID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSHoldBill` | `RecalledAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSHoldBill` | `CancelledByUserID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSHoldBill` | `CancelledAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `POSSessionId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.POSSessions` | `CompanyId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `CompanyName` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `BranchId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `BranchName` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `StoreId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `StoreName` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `CounterId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `CounterName` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `CounterAssignmentId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `OperatorId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `OperatorNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `SessionNumber` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `OpeningCash` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.POSSessions` | `ExpectedClosingCash` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.POSSessions` | `ActualClosingCash` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.POSSessions` | `CashDifference` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.POSSessions` | `OpenedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.POSSessions` | `OpenedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `ClosedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `ClosedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `ClosingRemarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `Status` | `TINYINT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.POSSessions` | `Version` | `ROWVERSION` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.POSSessions` | `CreatedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `UpdatedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.POSSessions` | `UpdatedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceListDetails` | `PriceListDetailId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PriceListDetails` | `PriceListId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceListDetails` | `ProductId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceListDetails` | `UnitId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceListDetails` | `PriceTypeId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceListDetails` | `Price` | `DECIMAL(18,4)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceListDetails` | `MinimumQuantity` | `DECIMAL(18,4)` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.PriceListDetails` | `MaximumQuantity` | `DECIMAL(18,4)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceListDetails` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.PriceListDetails` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceListDetails` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.PriceListDetails` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceListDetails` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceListPriceTypes` | `PriceListPriceTypeId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PriceListPriceTypes` | `PriceListId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceListPriceTypes` | `PriceTypeId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceListPriceTypes` | `IsActive` | `BIT` | No | No | No inline FK | (1) | No | UNKNOWN |
| `dbo.PriceListPriceTypes` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceListPriceTypes` | `CreatedAt` | `DATETIME` | No | No | No inline FK | (GETDATE()) | No | UNKNOWN |
| `dbo.PriceLists` | `PriceListId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PriceLists` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceLists` | `PriceTypeId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceLists` | `CurrencyId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceLists` | `Code` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceLists` | `Name` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceLists` | `Description` | `VARCHAR(300)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceLists` | `EffectiveFrom` | `DATE` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceLists` | `EffectiveTo` | `DATE` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceLists` | `IsDefault` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PriceLists` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.PriceLists` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceLists` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.PriceLists` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceLists` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceTypes` | `PriceTypeId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PriceTypes` | `Code` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceTypes` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceTypes` | `Description` | `NVARCHAR(300)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceTypes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.PriceTypes` | `DisplayOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PriceTypes` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceTypes` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.PriceTypes` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PriceTypes` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrinterModel` | `PrinterModelId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PrinterModel` | `PrinterTypeId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrinterModel` | `Code` | `VARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrinterModel` | `Name` | `VARCHAR(150)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrinterModel` | `Manufacturer` | `VARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrinterModel` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.PrinterModel` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrinterModel` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.PrinterModel` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrinterModel` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrinterType` | `PrinterTypeId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PrinterType` | `Code` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrinterType` | `Name` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrinterType` | `Description` | `VARCHAR(300)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrinterType` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.PrinterType` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrinterType` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.PrinterType` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrinterType` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrintOrientation` | `OrientationId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PrintOrientation` | `Code` | `VARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrintOrientation` | `Name` | `VARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrintOrientation` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.PrintUnit` | `UnitId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PrintUnit` | `Code` | `VARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrintUnit` | `Name` | `VARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PrintUnit` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.ProductBrands` | `Id` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.ProductBrands` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductBrands` | `BrandCode` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductBrands` | `BrandName` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductBrands` | `Description` | `VARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductBrands` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.ProductBrands` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductBrands` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.ProductBrands` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductBrands` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductCategories` | `Id` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.ProductCategories` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductCategories` | `CategoryCode` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductCategories` | `CategoryName` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductCategories` | `Description` | `VARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductCategories` | `ParentCategoryId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductCategories` | `SortOrder` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductCategories` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.ProductCategories` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductCategories` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.ProductCategories` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductCategories` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `Id` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Products` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `BranchId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `WarehouseId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `ProductCode` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `ProductName` | `VARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `CategoryId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `SubCategoryId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `BrandId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `UOMId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `SKU` | `VARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `Barcode` | `VARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `MRP` | `DECIMAL(18,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `PurchasePrice` | `DECIMAL(18,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `SalesPrice` | `DECIMAL(18,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `TaxId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `HsnSacId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `IsStockItem` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Products` | `IsSaleable` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Products` | `IsPurchaseable` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Products` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Products` | `Description` | `VARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `EntityId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Products` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Products` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductSubCategories` | `Id` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.ProductSubCategories` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductSubCategories` | `CategoryId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductSubCategories` | `SubCategoryCode` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductSubCategories` | `SubCategoryName` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductSubCategories` | `Description` | `VARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductSubCategories` | `SortOrder` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductSubCategories` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.ProductSubCategories` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductSubCategories` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.ProductSubCategories` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ProductSubCategories` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `PurchaseId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Purchase` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `CompanyNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `BranchId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `BranchNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `WarehouseId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `SupplierId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `SupplierNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `PurchaseNumber` | `NVARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `PurchaseDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `SupplierInvoiceNumber` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `SupplierInvoiceDate` | `DATE` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `TotalGrossAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Purchase` | `TotalDiscountAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Purchase` | `TotalTaxableAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Purchase` | `TotalTaxAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Purchase` | `TotalCessAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Purchase` | `TotalRoundOff` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Purchase` | `GrandTotal` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Purchase` | `PaidAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Purchase` | `BalanceAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Purchase` | `PaymentTypeID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `PaymentMethodID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `StatusID` | `BIGINT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Purchase` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Purchase` | `CreatedByUserID` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Purchase` | `UpdatedByUserID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `UpdatedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `SupplierPONumber` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `ReferenceNumber` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `CurrencyId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `PurchaseTypeId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `AccountingYearId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `TaxId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `PriceListId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `IsGSTInclusive` | `BIT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `CancelledByUserID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `CancelledAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Purchase` | `CancellationReason` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `PurchaseItemId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PurchaseItem` | `PurchaseId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `ProductId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `ProductCodeSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `ProductNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `BrandID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `CategoryID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `SubCategoryID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `UnitID` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `UnitNameSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `HSNID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `HSNCodeSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `BarcodeSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `Quantity` | `DECIMAL(18,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `FreeQuantity` | `DECIMAL(18,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `PurchaseRate` | `DECIMAL(18,4)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `MRP` | `DECIMAL(18,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `RetailPrice` | `DECIMAL(18,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `WholesalePrice` | `DECIMAL(18,2)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `SaleRate` | `DECIMAL(18,4)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `DiscountPercentage` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `DiscountAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `IsGSTInclusive` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `TaxableValue` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `GSTRate` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `GSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `CGSTRate` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `CGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `SGSTRate` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `SGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `IGSTRate` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `IGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `CESSRate` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `CESSAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `LineTotal` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseItem` | `ManufacturingDate` | `DATE` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `ExpiryDate` | `DATE` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `TaxId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `CessId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `OrderedQuantity` | `DECIMAL(18,3)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `ReceivedQuantity` | `DECIMAL(18,3)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `ReturnedQuantity` | `DECIMAL(18,3)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `RemainingQuantity` | `DECIMAL(18,3)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `PurchaseOrderId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `PurchaseOrderItemId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `GRNId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `BatchNumber` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseItem` | `SerialNumber` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `PurchaseReturnId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PurchaseReturn` | `PurchaseId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `CompanyNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `BranchId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `BranchNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `WarehouseId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `SupplierId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `SupplierNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `ReturnNumber` | `NVARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `ReturnDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `TotalGrossAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturn` | `TotalDiscountAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturn` | `TotalTaxableAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturn` | `TotalTaxAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturn` | `TotalCessAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturn` | `TotalRoundOff` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturn` | `GrandTotal` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturn` | `StatusID` | `BIGINT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.PurchaseReturn` | `Reason` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `CreatedByUserID` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.PurchaseReturn` | `UpdatedByUserID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `UpdatedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `SupplierInvoiceNumber` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `ReferenceNumber` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `CurrencyId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `PurchaseReturnTypeId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `AccountingYearId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `PaymentTypeId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `PaymentMethodId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `ReturnReasonId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `CancelledByUserID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `CancelledAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturn` | `CancellationReason` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `PurchaseReturnItemId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.PurchaseReturnItem` | `PurchaseReturnId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `PurchaseItemId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `ProductId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `ProductCodeSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `ProductNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `UnitId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `UnitNameSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `HSNId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `HSNCodeSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `ReturnQuantity` | `DECIMAL(18,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `PurchaseRate` | `DECIMAL(18,4)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `DiscountAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `TaxableValue` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `GSTRate` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `GSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `CGSTRate` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `CGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `SGSTRate` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `SGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `IGSTRate` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `IGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `CESSRate` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `CESSAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `LineTotal` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `TaxId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `CessId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `BarcodeSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `BatchNumber` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `SerialNumber` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.PurchaseReturnItem` | `ReasonId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RefreshTokens` | `RefreshTokenId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.RefreshTokens` | `PlatformUserId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RefreshTokens` | `Token` | `NVARCHAR(500)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RefreshTokens` | `ExpiryDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RefreshTokens` | `IsRevoked` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.RefreshTokens` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.RefreshTokens` | `RevokedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleDataScopes` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.RoleDataScopes` | `RoleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleDataScopes` | `ModuleId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleDataScopes` | `ScreenId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleDataScopes` | `CompanyId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleDataScopes` | `BranchId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleDataScopes` | `DepartmentId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleDataScopes` | `WarehouseId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleDataScopes` | `BusinessUnitId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleDataScopes` | `CostCenterId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleDataScopes` | `ProfitCenterId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleDataScopes` | `CanView` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.RoleDataScopes` | `CanCreate` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.RoleDataScopes` | `CanEdit` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.RoleDataScopes` | `CanDelete` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.RoleDataScopes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.RoleDataScopes` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleDataScopes` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.RoleDataScopes` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleDataScopes` | `ModifiedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleFieldPermissions` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.RoleFieldPermissions` | `RoleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleFieldPermissions` | `ScreenId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleFieldPermissions` | `FieldId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleFieldPermissions` | `CanView` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.RoleFieldPermissions` | `CanEdit` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.RoleFieldPermissions` | `IsHidden` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.RoleFieldPermissions` | `IsReadOnly` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.RoleFieldPermissions` | `IsMandatory` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.RoleFieldPermissions` | `DisplayOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.RoleFieldPermissions` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.RoleFieldPermissions` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleFieldPermissions` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.RoleFieldPermissions` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RoleFieldPermissions` | `ModifiedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RolePermissions` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.RolePermissions` | `RoleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RolePermissions` | `WorkspaceId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RolePermissions` | `DomainId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RolePermissions` | `ModuleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RolePermissions` | `SubModuleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RolePermissions` | `ScreenId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RolePermissions` | `ActionId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RolePermissions` | `Allow` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.RolePermissions` | `DisplayOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.RolePermissions` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.RolePermissions` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RolePermissions` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.RolePermissions` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RolePermissions` | `ModifiedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RolePermissionsLegacy` | `RolePermissionId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.RolePermissionsLegacy` | `RoleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RolePermissionsLegacy` | `PermissionCode` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.RolePermissionsLegacy` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.RolePermissionsLegacy` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Roles` | `RoleId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Roles` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Roles` | `Code` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Roles` | `Description` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Roles` | `IsSystem` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Roles` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Roles` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Roles` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Roles` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Roles` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Roles` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoice` | `SalesInvoiceId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.SalesInvoice` | `SalesInvoiceNo` | `NVARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `InvoiceDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `SourceType` | `NVARCHAR(20)` | No | No | No inline FK | 'SALES' | No | UNKNOWN |
| `dbo.SalesInvoice` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `CompanyNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `BranchId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `WarehouseId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `CustomerId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `CustomerNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `SalesTypeId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `PriceListId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `the` | `link used to reconcile expected closing cash. */` | Unknown (DDL omits NULL/NOT NULL) | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `POSSessionId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `ReferenceNo` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `ReferenceDate` | `DATE` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `TotalGrossAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoice` | `TotalDiscountAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoice` | `TotalTaxableAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoice` | `TotalCGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoice` | `TotalSGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoice` | `TotalIGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoice` | `TotalCESSAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoice` | `TotalRoundOff` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoice` | `GrandTotal` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoice` | `PaidAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoice` | `BalanceAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoice` | `PaymentTypeID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `PaymentMethodID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `StatusID` | `BIGINT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.SalesInvoice` | `InvoiceStatus` | `NVARCHAR(20)` | No | No | No inline FK | 'POSTED' | No | UNKNOWN |
| `dbo.SalesInvoice` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.SalesInvoice` | `CreatedByUserID` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.SalesInvoice` | `UpdatedByUserID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoice` | `UpdatedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `SalesInvoiceItemId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.SalesInvoiceItem` | `SalesInvoiceId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `ProductId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `ProductCodeSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `ProductNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `UnitID` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `UnitNameSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `BatchId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `HSNID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `HSNCodeSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `BarcodeSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `Quantity` | `DECIMAL(18,3)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `FreeQuantity` | `DECIMAL(18,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `Rate` | `DECIMAL(18,4)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `GrossAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `DiscountPercentage` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `DiscountAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `TaxableAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `GSTPercent` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `CGSTPercent` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `SGSTPercent` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `IGSTPercent` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `CESSPercent` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `CGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `SGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `IGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `CESSAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `LineTotal` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesInvoiceItem` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `SalesReturnId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.SalesReturn` | `SalesInvoiceId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `CompanyNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `BranchId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `BranchNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `WarehouseId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `CustomerId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `CustomerNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `ReturnNumber` | `NVARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `ReturnDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `TotalGrossAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturn` | `TotalDiscountAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturn` | `TotalTaxableAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturn` | `TotalCGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturn` | `TotalSGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturn` | `TotalIGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturn` | `TotalCESSAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturn` | `TotalRoundOff` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturn` | `GrandTotal` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturn` | `StatusID` | `BIGINT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.SalesReturn` | `Reason` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `PaymentTypeId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `PaymentMethodId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `RefundAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturn` | `CreatedByUserID` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.SalesReturn` | `UpdatedByUserID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `UpdatedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `CancelledByUserID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `CancelledAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturn` | `CancellationReason` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturnItem` | `SalesReturnItemId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.SalesReturnItem` | `SalesReturnId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturnItem` | `SalesInvoiceItemId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturnItem` | `ProductId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturnItem` | `ProductCodeSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturnItem` | `ProductNameSnapshot` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturnItem` | `UnitID` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturnItem` | `UnitNameSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturnItem` | `HSNID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturnItem` | `HSNCodeSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturnItem` | `BarcodeSnapshot` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturnItem` | `BatchId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SalesReturnItem` | `ReturnQuantity` | `DECIMAL(18,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturnItem` | `FreeQuantity` | `DECIMAL(18,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturnItem` | `Rate` | `DECIMAL(18,4)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturnItem` | `DiscountAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturnItem` | `TaxableAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturnItem` | `GSTPercent` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturnItem` | `CGSTPercent` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturnItem` | `SGSTPercent` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturnItem` | `IGSTPercent` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturnItem` | `CESSPercent` | `DECIMAL(8,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturnItem` | `CGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturnItem` | `SGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturnItem` | `IGSTAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturnItem` | `CESSAmount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SalesReturnItem` | `LineTotal` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Screens` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Screens` | `SubModuleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Screens` | `ScreenCode` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Screens` | `ScreenName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Screens` | `PermissionCode` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Screens` | `ScreenType` | `NVARCHAR(20)` | No | No | No inline FK | 'MASTER' | No | UNKNOWN |
| `dbo.Screens` | `RouteUrl` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Screens` | `ComponentName` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Screens` | `SortOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Screens` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Screens` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Screens` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.ServiceCategories` | `ServiceCategoryId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.ServiceCategories` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ServiceCategories` | `Code` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ServiceCategories` | `Name` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.ServiceCategories` | `Description` | `VARCHAR(300)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ServiceCategories` | `DisplayOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.ServiceCategories` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.ServiceCategories` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ServiceCategories` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.ServiceCategories` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.ServiceCategories` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Services` | `ServiceId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Services` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Services` | `Code` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Services` | `Name` | `VARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Services` | `ServiceCategoryId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Services` | `UnitId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Services` | `HsnSacId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Services` | `DefaultTaxId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Services` | `StandardRate` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Services` | `IsTaxInclusive` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Services` | `Description` | `VARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Services` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Services` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Services` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Services` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Services` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Settings` | `SettingId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Settings` | `SettingKey` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Settings` | `SettingValue` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Settings` | `Description` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Settings` | `UpdatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Settings` | `UpdatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Sources` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Sources` | `Code` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Sources` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Sources` | `Description` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Sources` | `SortOrder` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Sources` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Sources` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Sources` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Sources` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Sources` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Sources` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.States` | `StateId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.States` | `CountryId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.States` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.States` | `StateCode` | `NVARCHAR(10)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.States` | `GSTStateCode` | `NVARCHAR(5)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.States` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.States` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.States` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.States` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.States` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.States` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Status` | `StatusId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Status` | `Code` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Status` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Status` | `Module` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Status` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Status` | `SortOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Status` | `CreatedBy` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Status` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Stock` | `StockId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Stock` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stock` | `BranchId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stock` | `WarehouseId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stock` | `ProductId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stock` | `UnitId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stock` | `Quantity` | `DECIMAL(18,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Stock` | `ReservedQuantity` | `DECIMAL(18,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Stock` | `AvailableQuantity` | `DECIMAL(18,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Stock` | `AverageCost` | `DECIMAL(18,4)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Stock` | `LastPurchaseRate` | `DECIMAL(18,4)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Stock` | `UpdatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.StockAdjustment` | `StockAdjustmentId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.StockAdjustment` | `AdjustmentNumber` | `NVARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockAdjustment` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockAdjustment` | `BranchId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockAdjustment` | `WarehouseId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockAdjustment` | `AdjustmentDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockAdjustment` | `Reason` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockAdjustment` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockAdjustment` | `Status` | `NVARCHAR(20)` | No | No | No inline FK | 'DRAFT', -- DRAFT \| POSTED \| CANCELLED | No | UNKNOWN |
| `dbo.StockAdjustment` | `CreatedByUserID` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockAdjustment` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.StockAdjustment` | `PostedByUserID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockAdjustment` | `PostedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockAdjustmentItem` | `StockAdjustmentItemId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.StockAdjustmentItem` | `StockAdjustmentId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockAdjustmentItem` | `ProductId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockAdjustmentItem` | `UnitId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockAdjustmentItem` | `QuantityDelta` | `DECIMAL(18,3)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockAdjustmentItem` | `Rate` | `DECIMAL(18,4)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.StockAdjustmentItem` | `Reason` | `NVARCHAR(300)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockCount` | `StockCountId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.StockCount` | `CountNumber` | `NVARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockCount` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockCount` | `BranchId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockCount` | `WarehouseId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockCount` | `CountDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockCount` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockCount` | `Status` | `NVARCHAR(20)` | No | No | No inline FK | 'DRAFT', -- DRAFT \| POSTED \| CANCELLED | No | UNKNOWN |
| `dbo.StockCount` | `CreatedByUserID` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockCount` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.StockCount` | `PostedByUserID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockCount` | `PostedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockCountItem` | `StockCountItemId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.StockCountItem` | `StockCountId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockCountItem` | `ProductId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockCountItem` | `UnitId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockCountItem` | `BookQuantity` | `DECIMAL(18,3)` | No | No | No inline FK | 0,  -- book qty at count time | No | UNKNOWN |
| `dbo.StockCountItem` | `CountedQuantity` | `DECIMAL(18,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.StockCountItem` | `Variance` | `DECIMAL(18,3)` | No | No | No inline FK | 0, -- counted - book | No | UNKNOWN |
| `dbo.StockCountItem` | `Rate` | `DECIMAL(18,4)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.StockTransaction` | `StockTransactionId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.StockTransaction` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransaction` | `BranchId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransaction` | `WarehouseId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransaction` | `ProductId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransaction` | `UnitId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransaction` | `TransactionType` | `VARCHAR(10)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransaction` | `ReferenceType` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransaction` | `ReferenceId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransaction` | `QuantityIn` | `DECIMAL(18,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.StockTransaction` | `QuantityOut` | `DECIMAL(18,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.StockTransaction` | `Rate` | `DECIMAL(18,4)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.StockTransaction` | `BalanceQuantity` | `DECIMAL(18,3)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.StockTransaction` | `TransactionDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransaction` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransaction` | `CreatedByUserID` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransaction` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.StockTransfer` | `StockTransferId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.StockTransfer` | `TransferNumber` | `NVARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransfer` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransfer` | `BranchId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransfer` | `FromWarehouseId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransfer` | `ToWarehouseId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransfer` | `TransferDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransfer` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransfer` | `Status` | `NVARCHAR(20)` | No | No | No inline FK | 'DRAFT', -- DRAFT \| POSTED \| CANCELLED | No | UNKNOWN |
| `dbo.StockTransfer` | `CreatedByUserID` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransfer` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.StockTransfer` | `PostedByUserID` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransfer` | `PostedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransferItem` | `StockTransferItemId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.StockTransferItem` | `StockTransferId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransferItem` | `ProductId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransferItem` | `UnitId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransferItem` | `Quantity` | `DECIMAL(18,3)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StockTransferItem` | `Rate` | `DECIMAL(18,4)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.StockTransferItem` | `Remarks` | `NVARCHAR(300)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stores` | `StoreId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Stores` | `CompanyId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stores` | `BranchId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stores` | `StoreCode` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stores` | `StoreName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stores` | `StoreType` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stores` | `Address` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stores` | `Phone` | `NVARCHAR(30)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stores` | `Email` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stores` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Stores` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Stores` | `CreatedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stores` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Stores` | `UpdatedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Stores` | `UpdatedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StoreTypes` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.StoreTypes` | `Code` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StoreTypes` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.StoreTypes` | `Description` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StoreTypes` | `SortOrder` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.StoreTypes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.StoreTypes` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.StoreTypes` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StoreTypes` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.StoreTypes` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.StoreTypes` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.SubModules` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.SubModules` | `ModuleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SubModules` | `SubModuleCode` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SubModules` | `SubModuleName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.SubModules` | `Icon` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SubModules` | `RouteUrl` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SubModules` | `SortOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.SubModules` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.SubModules` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SubModules` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.SubModules` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.SubModules` | `ModifiedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Subscriptions` | `SubscriptionId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Subscriptions` | `TenantId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Subscriptions` | `PlanId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Subscriptions` | `StartDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Subscriptions` | `EndDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Subscriptions` | `Amount` | `DECIMAL(18,2)` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Subscriptions` | `Status` | `NVARCHAR(20)` | No | No | No inline FK | 'Active' | No | UNKNOWN |
| `dbo.Subscriptions` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Subscriptions` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Subscriptions` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Subscriptions` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Subscriptions` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Tag` | `TagId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Tag` | `TagName` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Tag` | `Color` | `VARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Tag` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Tag` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Tag` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Tag` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Tag` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Taxes` | `Id` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Taxes` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Taxes` | `BranchId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Taxes` | `TaxTypeSystemId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Taxes` | `TaxCode` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Taxes` | `TaxName` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Taxes` | `TaxRate` | `DECIMAL(8,4)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Taxes` | `IsInclusive` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Taxes` | `EffectiveFrom` | `DATE` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Taxes` | `EffectiveTo` | `DATE` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Taxes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Taxes` | `Description` | `VARCHAR(300)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Taxes` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Taxes` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Taxes` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Taxes` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TaxTypeSystems` | `Id` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.TaxTypeSystems` | `Code` | `VARCHAR(30)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TaxTypeSystems` | `Name` | `VARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TaxTypeSystems` | `Description` | `VARCHAR(300)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TaxTypeSystems` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.TaxTypeSystems` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TaxTypeSystems` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.TenantConfiguration` | `Id` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.TenantConfiguration` | `TenantId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConfiguration` | `ApplicationType` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConfiguration` | `TransactionType` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConfiguration` | `FlowType` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConfiguration` | `PageCode` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConfiguration` | `FieldCode` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConfiguration` | `SequenceNo` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConfiguration` | `IsPageEnabled` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.TenantConfiguration` | `IsVisible` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.TenantConfiguration` | `IsRequired` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.TenantConfiguration` | `IsReadonly` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.TenantConfiguration` | `DisplayOrder` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConfiguration` | `DefaultValue` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConfiguration` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.TenantConfiguration` | `CreatedBy` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConfiguration` | `CreatedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.TenantConfiguration` | `UpdatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConfiguration` | `UpdatedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConnections` | `ConnectionId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.TenantConnections` | `TenantId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConnections` | `ServerName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConnections` | `DatabaseName` | `NVARCHAR(128)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConnections` | `ConnectionString` | `NVARCHAR(1000)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConnections` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.TenantConnections` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantConnections` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.TenantMigrationHistory` | `TenantMigrationHistoryId` | `BIGINT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.TenantMigrationHistory` | `TenantId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantMigrationHistory` | `MigrationId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantMigrationHistory` | `ExecutionId` | `UNIQUEIDENTIFIER` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantMigrationHistory` | `Version` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantMigrationHistory` | `MigrationCode` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantMigrationHistory` | `MigrationName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantMigrationHistory` | `Status` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantMigrationHistory` | `Checksum` | `NVARCHAR(128)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantMigrationHistory` | `StartedAt` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.TenantMigrationHistory` | `CompletedAt` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantMigrationHistory` | `DurationMs` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantMigrationHistory` | `ExecutedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantMigrationHistory` | `ErrorMessage` | `NVARCHAR(MAX)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantMigrationHistory` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Tenants` | `TenantId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Tenants` | `TenantCode` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Tenants` | `TenantName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Tenants` | `CompanyName` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Tenants` | `DatabaseName` | `NVARCHAR(128)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Tenants` | `PlanId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Tenants` | `ContactEmail` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Tenants` | `AdminUsername` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Tenants` | `AdminPassword` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Tenants` | `Status` | `NVARCHAR(20)` | No | No | No inline FK | 'Active' | No | UNKNOWN |
| `dbo.Tenants` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Tenants` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Tenants` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Tenants` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Tenants` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.TenantUserMaps` | `TenantUserId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.TenantUserMaps` | `Username` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantUserMaps` | `TenantCode` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TenantUserMaps` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.TimeZones` | `TimeZoneId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.TimeZones` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TimeZones` | `TimeZoneName` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.TimeZones` | `UTCOffset` | `NVARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TimeZones` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.TimeZones` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TimeZones` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.TimeZones` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.TimeZones` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.TimeZones` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.UnitConversions` | `UnitConversionId` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.UnitConversions` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UnitConversions` | `ProductId` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UnitConversions` | `FromUnitId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UnitConversions` | `ToUnitId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UnitConversions` | `ConversionFactor` | `DECIMAL(18,6)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UnitConversions` | `IsDefault` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.UnitConversions` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.UnitConversions` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UnitConversions` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.UnitConversions` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UnitConversions` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Units` | `Id` | `BIGINT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Units` | `CompanyId` | `BIGINT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Units` | `UnitCode` | `VARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Units` | `UnitName` | `VARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Units` | `Symbol` | `VARCHAR(20)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Units` | `DecimalPlaces` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Units` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Units` | `CreatedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Units` | `CreatedAt` | `DATETIME` | No | No | No inline FK | GETDATE() | No | UNKNOWN |
| `dbo.Units` | `ModifiedBy` | `BIGINT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Units` | `ModifiedAt` | `DATETIME` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `UserId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `ModuleId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `ScreenId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `ScopeType` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `ScopeValue` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `PermissionType` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `Allow` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `EffectiveFrom` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `EffectiveTo` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserDataScopeOverrides` | `ModifiedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserFieldPermissions` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.UserFieldPermissions` | `UserId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserFieldPermissions` | `ScreenId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserFieldPermissions` | `FieldId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserFieldPermissions` | `CanView` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.UserFieldPermissions` | `CanEdit` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.UserFieldPermissions` | `IsHidden` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.UserFieldPermissions` | `IsReadOnly` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.UserFieldPermissions` | `IsMandatory` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.UserFieldPermissions` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.UserFieldPermissions` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserFieldPermissions` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.UserFieldPermissions` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserFieldPermissions` | `ModifiedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.UserPermissionOverrides` | `UserId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `WorkspaceId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `DomainId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `ModuleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `SubModuleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `ScreenId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `ActionId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `PermissionType` | `NVARCHAR(20)` | No | No | No inline FK | 'Grant' | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `Allow` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `EffectiveFrom` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `EffectiveTo` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserPermissionOverrides` | `ModifiedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserRoles` | `UserRoleId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.UserRoles` | `UserId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserRoles` | `RoleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserRoles` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.UserRoles` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Users` | `UserId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Users` | `CompanyId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Users` | `Username` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Users` | `Email` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Users` | `Mobile` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Users` | `FullName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Users` | `PasswordHash` | `NVARCHAR(255)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Users` | `Status` | `NVARCHAR(20)` | No | No | No inline FK | 'Active' | No | UNKNOWN |
| `dbo.Users` | `IsSuperAdmin` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Users` | `LastLoginDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Users` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Users` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Users` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Users` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Users` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Warehouses` | `Id` | `INT` | Unknown (DDL omits NULL/NOT NULL) | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Warehouses` | `CompanyId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `BranchId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `WarehouseCode` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `WarehouseName` | `NVARCHAR(150)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `ShortName` | `NVARCHAR(50)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `WarehouseTypeId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `ParentWarehouseId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `ManagerEmployeeId` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `AllowNegativeStock` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `IsDefault` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `SortOrder` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `Remarks` | `NVARCHAR(500)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `IsActive` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `IsBlocked` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `IsDeleted` | `BIT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `CreatedBy` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `ModifiedBy` | `INT` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Warehouses` | `ModifiedDate` | `DATETIME2` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.WarehouseTypes` | `WarehouseTypeId` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.WarehouseTypes` | `Name` | `NVARCHAR(100)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.WarehouseTypes` | `Code` | `NVARCHAR(20)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.WarehouseTypes` | `Description` | `NVARCHAR(250)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.WarehouseTypes` | `SortOrder` | `INT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.WarehouseTypes` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.WarehouseTypes` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.WarehouseTypes` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.WarehouseTypes` | `ModifiedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.WarehouseTypes` | `ModifiedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.WarehouseTypes` | `IsDeleted` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.WorkflowPermissions` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.WorkflowPermissions` | `RoleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.WorkflowPermissions` | `ModuleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.WorkflowPermissions` | `SubModuleId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.WorkflowPermissions` | `ScreenId` | `INT` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.WorkflowPermissions` | `CanSubmit` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.WorkflowPermissions` | `CanApprove` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.WorkflowPermissions` | `CanReject` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.WorkflowPermissions` | `CanCancel` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.WorkflowPermissions` | `CanClose` | `BIT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.WorkflowPermissions` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.WorkflowPermissions` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.WorkflowPermissions` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |
| `dbo.Workspaces` | `Id` | `INT` | No | Yes | No inline FK | — | Yes | UNKNOWN |
| `dbo.Workspaces` | `WorkspaceCode` | `NVARCHAR(50)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Workspaces` | `WorkspaceName` | `NVARCHAR(200)` | No | No | No inline FK | — | No | UNKNOWN |
| `dbo.Workspaces` | `Icon` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Workspaces` | `Route` | `NVARCHAR(200)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Workspaces` | `SortOrder` | `INT` | No | No | No inline FK | 0 | No | UNKNOWN |
| `dbo.Workspaces` | `IsActive` | `BIT` | No | No | No inline FK | 1 | No | UNKNOWN |
| `dbo.Workspaces` | `CreatedBy` | `NVARCHAR(100)` | Yes | No | No inline FK | — | No | UNKNOWN |
| `dbo.Workspaces` | `CreatedDate` | `DATETIME2` | No | No | No inline FK | SYSUTCDATETIME() | No | UNKNOWN |

## 13. Database Relationship Map

### Declared foreign keys

Only parsed SQL foreign keys are listed here.

| From | To | Source |
|---|---|---|
| `dbo.Barcodes.ProductId` | `dbo.Products.Id` | `sql/erp_full.sql` |
| `dbo.Barcodes.UnitId` | `dbo.Units.Id` | `sql/erp_full.sql` |
| `dbo.Branches.CompanyId` | `Companies.Id` | `sql/erp_full.sql` |
| `dbo.Branches.BranchTypeId` | `BranchTypes.BranchTypeId` | `sql/erp_full.sql` |
| `dbo.Branches.ParentBranchId` | `Branches.Id` | `sql/erp_full.sql` |
| `dbo.Cities.CountryId` | `dbo.Countries.CountryId` | `sql/erp_full.sql` |
| `dbo.Cities.StateId` | `dbo.States.StateId` | `sql/erp_full.sql` |
| `dbo.Companies.BusinessTypeId` | `dbo.BusinessTypes.BusinessTypeId` | `sql/erp_full.sql` |
| `dbo.Companies.IndustryTypeId` | `dbo.IndustryTypes.IndustryTypeId` | `sql/erp_full.sql` |
| `dbo.Companies.GSTRegistrationTypeId` | `dbo.GSTRegistrationTypes.GSTRegistrationTypeId` | `sql/erp_full.sql` |
| `dbo.Companies.CurrencyId` | `dbo.Currencies.Id` | `sql/erp_full.sql` |
| `dbo.Companies.LanguageId` | `dbo.Languages.LanguageId` | `sql/erp_full.sql` |
| `dbo.Companies.TimeZoneId` | `dbo.TimeZones.TimeZoneId` | `sql/erp_full.sql` |
| `dbo.CompanyGroups.ParentGroupId` | `dbo.CompanyGroups.CompanyGroupId` | `sql/erp_full.sql` |
| `dbo.CounterAssignments.CompanyId` | `dbo.Companies.Id` | `sql/erp_full.sql` |
| `dbo.CounterAssignments.BranchId` | `dbo.Branches.Id` | `sql/erp_full.sql` |
| `dbo.CounterAssignments.StoreId` | `dbo.Stores.StoreId` | `sql/erp_full.sql` |
| `dbo.CounterAssignments.CounterId` | `dbo.Counters.CounterId` | `sql/erp_full.sql` |
| `dbo.CounterAssignments.OperatorId` | `dbo.Operators.OperatorId` | `sql/erp_full.sql` |
| `dbo.Counters.StoreId` | `Stores.StoreId` | `sql/erp_full.sql` |
| `dbo.Coupons.OfferId` | `dbo.Offers.OfferId` | `sql/erp_full.sql` |
| `dbo.Departments.CompanyId` | `Companies.Id` | `sql/erp_full.sql` |
| `dbo.Departments.BranchId` | `Branches.Id` | `sql/erp_full.sql` |
| `dbo.Departments.ParentDepartmentId` | `Departments.Id` | `sql/erp_full.sql` |
| `dbo.Designations.CompanyId` | `Companies.Id` | `sql/erp_full.sql` |
| `dbo.Designations.DepartmentId` | `Departments.Id` | `sql/erp_full.sql` |
| `dbo.DiscountRules.ProductId` | `dbo.Products.Id` | `sql/erp_full.sql` |
| `dbo.DiscountRules.ServiceId` | `dbo.Services.ServiceId` | `sql/erp_full.sql` |
| `dbo.DiscountRules.ProductCategoryId` | `dbo.ProductCategories.Id` | `sql/erp_full.sql` |
| `dbo.DiscountRules.PriceListId` | `dbo.PriceLists.PriceListId` | `sql/erp_full.sql` |
| `dbo.Domains.WorkspaceId` | `dbo.Workspaces.Id` | `sql/erp_full.sql` |
| `dbo.Employees.CompanyId` | `Companies.Id` | `sql/erp_full.sql` |
| `dbo.Employees.BranchId` | `Branches.Id` | `sql/erp_full.sql` |
| `dbo.Employees.DepartmentId` | `Departments.Id` | `sql/erp_full.sql` |
| `dbo.Employees.DesignationId` | `Designations.Id` | `sql/erp_full.sql` |
| `dbo.Employees.ReportingManagerId` | `Employees.Id` | `sql/erp_full.sql` |
| `dbo.EntityAddress.EntityId` | `dbo.Entity.EntityId` | `sql/erp_full.sql` |
| `dbo.EntityAddress.AddressId` | `dbo.Address.AddressId` | `sql/erp_full.sql` |
| `dbo.EntityContact.EntityId` | `dbo.Entity.EntityId` | `sql/erp_full.sql` |
| `dbo.EntityContact.ContactId` | `dbo.Contact.ContactId` | `sql/erp_full.sql` |
| `dbo.EntityFile.EntityId` | `dbo.Entity.EntityId` | `sql/erp_full.sql` |
| `dbo.EntityFile.FileId` | `dbo.Files.FileId` | `sql/erp_full.sql` |
| `dbo.EntityNote.EntityId` | `dbo.Entity.EntityId` | `sql/erp_full.sql` |
| `dbo.EntityNote.NoteId` | `dbo.Note.NoteId` | `sql/erp_full.sql` |
| `dbo.EntityTag.EntityId` | `dbo.Entity.EntityId` | `sql/erp_full.sql` |
| `dbo.EntityTag.TagId` | `dbo.Tag.TagId` | `sql/erp_full.sql` |
| `dbo.FieldPermissions.RoleId` | `dbo.Roles.RoleId` | `sql/erp_full.sql` |
| `dbo.FieldPermissions.PermissionModuleId` | `dbo.PermissionModules.Id` | `sql/erp_full.sql` |
| `dbo.Fields.ScreenId` | `dbo.Screens.Id` | `sql/erp_full.sql` |
| `dbo.FinancialYear.CompanyId` | `dbo.Companies.Id` | `sql/erp_full.sql` |
| `dbo.HsnSacs.TaxId` | `dbo.Taxes.Id` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplate.InvoiceTypeId` | `dbo.InvoiceType.InvoiceTypeId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplate.PaperSizeId` | `dbo.InvoicePaperSize.PaperSizeId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplate.OrientationId` | `dbo.PrintOrientation.OrientationId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplate.TemplateCategoryId` | `dbo.InvoiceTemplateCategory.TemplateCategoryId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplateAssignment.InvoiceTemplateId` | `dbo.InvoiceTemplate.InvoiceTemplateId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplateAssignment.CompanyId` | `dbo.Companies.Id` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplateAssignment.IndustryTypeId` | `dbo.IndustryTypes.IndustryTypeId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplateAssignment.InvoiceTypeId` | `dbo.InvoiceType.InvoiceTypeId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplateAssignment.PaperSizeId` | `dbo.InvoicePaperSize.PaperSizeId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplateElement.SectionId` | `dbo.InvoiceTemplateSection.SectionId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplateElement.ComponentId` | `dbo.InvoiceTemplateComponent.ComponentId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplateField.ElementId` | `dbo.InvoiceTemplateElement.ElementId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplateField.VariableId` | `dbo.InvoiceTemplateVariable.VariableId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplateItemColumn.ElementId` | `dbo.InvoiceTemplateElement.ElementId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplatePrinter.InvoiceTemplateId` | `dbo.InvoiceTemplate.InvoiceTemplateId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplatePrinter.PaperSizeId` | `dbo.InvoicePaperSize.PaperSizeId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplatePrinter.PrinterTypeId` | `dbo.PrinterType.PrinterTypeId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplatePrinter.PrinterModelId` | `dbo.PrinterModel.PrinterModelId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplatePrintSetting.TemplatePrinterId` | `dbo.InvoiceTemplatePrinter.TemplatePrinterId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplateSection.TemplateVersionId` | `dbo.InvoiceTemplateVersion.TemplateVersionId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplateStyle.ElementId` | `dbo.InvoiceTemplateElement.ElementId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplateStyle.FontId` | `dbo.InvoiceFont.FontId` | `sql/erp_full.sql` |
| `dbo.InvoiceTemplateVersion.InvoiceTemplateId` | `dbo.InvoiceTemplate.InvoiceTemplateId` | `sql/erp_full.sql` |
| `dbo.ModulePermissions.RoleId` | `dbo.Roles.RoleId` | `sql/erp_full.sql` |
| `dbo.ModulePermissions.PermissionModuleId` | `dbo.PermissionModules.Id` | `sql/erp_full.sql` |
| `dbo.ModulePermissions.PermissionActionId` | `dbo.PermissionActions.Id` | `sql/erp_full.sql` |
| `dbo.Modules.DomainId` | `dbo.Domains.Id` | `sql/erp_full.sql` |
| `dbo.OfferDetails.OfferId` | `dbo.Offers.OfferId` | `sql/erp_full.sql` |
| `dbo.OfferDetails.ProductId` | `dbo.Products.Id` | `sql/erp_full.sql` |
| `dbo.OfferDetails.ServiceId` | `dbo.Services.ServiceId` | `sql/erp_full.sql` |
| `dbo.OfferDetails.ProductCategoryId` | `dbo.ProductCategories.Id` | `sql/erp_full.sql` |
| `dbo.Operators.CompanyId` | `dbo.Companies.Id` | `sql/erp_full.sql` |
| `dbo.Operators.BranchId` | `dbo.Branches.Id` | `sql/erp_full.sql` |
| `dbo.Operators.UserId` | `dbo.Users.UserId` | `sql/erp_full.sql` |
| `dbo.Operators.OperatorTypeId` | `dbo.OperatorTypes.Id` | `sql/erp_full.sql` |
| `dbo.PaymentMethodDetails.PaymentMethodId` | `dbo.PaymentMethod.PaymentMethodId` | `sql/erp_full.sql` |
| `dbo.PermissionModules.ParentId` | `dbo.PermissionModules.Id` | `sql/erp_full.sql` |
| `dbo.POSSessions.CompanyId` | `Companies.Id` | `sql/erp_full.sql` |
| `dbo.POSSessions.StoreId` | `Stores.StoreId` | `sql/erp_full.sql` |
| `dbo.POSSessions.CounterId` | `Counters.CounterId` | `sql/erp_full.sql` |
| `dbo.POSSessions.OperatorId` | `Operators.OperatorId` | `sql/erp_full.sql` |
| `dbo.POSSessions.CounterAssignmentId` | `CounterAssignments.AssignmentId` | `sql/erp_full.sql` |
| `dbo.PriceListDetails.PriceListId` | `dbo.PriceLists.PriceListId` | `sql/erp_full.sql` |
| `dbo.PriceListDetails.ProductId` | `dbo.Products.Id` | `sql/erp_full.sql` |
| `dbo.PriceListDetails.UnitId` | `dbo.Units.Id` | `sql/erp_full.sql` |
| `dbo.PriceListDetails.PriceTypeId` | `dbo.PriceTypes.PriceTypeId` | `sql/erp_full.sql` |
| `dbo.PriceListPriceTypes.PriceListId` | `dbo.PriceLists.PriceListId` | `sql/erp_full.sql` |
| `dbo.PriceListPriceTypes.PriceTypeId` | `dbo.PriceTypes.PriceTypeId` | `sql/erp_full.sql` |
| `dbo.PriceLists.PriceTypeId` | `dbo.PriceTypes.PriceTypeId` | `sql/erp_full.sql` |
| `dbo.PriceLists.CurrencyId` | `dbo.Currencies.Id` | `sql/erp_full.sql` |
| `dbo.PrinterModel.PrinterTypeId` | `dbo.PrinterType.PrinterTypeId` | `sql/erp_full.sql` |
| `dbo.Purchase.TaxId` | `dbo.Taxes.Id` | `sql/erp_full.sql` |
| `dbo.Purchase.PriceListId` | `dbo.PriceLists.PriceListId` | `sql/erp_full.sql` |
| `dbo.Purchase.AccountingYearId` | `dbo.FinancialYear.FinancialYearId` | `sql/erp_full.sql` |
| `dbo.PurchaseItem.TaxId` | `dbo.Taxes.Id` | `sql/erp_full.sql` |
| `dbo.PurchaseReturn.AccountingYearId` | `dbo.FinancialYear.FinancialYearId` | `sql/erp_full.sql` |
| `dbo.PurchaseReturn.PaymentTypeId` | `dbo.PaymentType.PaymentTypeId` | `sql/erp_full.sql` |
| `dbo.PurchaseReturn.PaymentMethodId` | `dbo.PaymentMethod.PaymentMethodId` | `sql/erp_full.sql` |
| `dbo.PurchaseReturnItem.TaxId` | `dbo.Taxes.Id` | `sql/erp_full.sql` |
| `dbo.RefreshTokens.PlatformUserId` | `dbo.PlatformUsers.PlatformUserId` | `sql/platform_schema.sql` |
| `dbo.RoleDataScopes.RoleId` | `dbo.Roles.RoleId` | `sql/erp_full.sql` |
| `dbo.RoleFieldPermissions.RoleId` | `dbo.Roles.RoleId` | `sql/erp_full.sql` |
| `dbo.RoleFieldPermissions.ScreenId` | `dbo.Screens.Id` | `sql/erp_full.sql` |
| `dbo.RoleFieldPermissions.FieldId` | `dbo.Fields.Id` | `sql/erp_full.sql` |
| `dbo.RolePermissions.RoleId` | `dbo.Roles.RoleId` | `sql/erp_full.sql` |
| `dbo.RolePermissions.WorkspaceId` | `dbo.Workspaces.Id` | `sql/erp_full.sql` |
| `dbo.RolePermissions.DomainId` | `dbo.Domains.Id` | `sql/erp_full.sql` |
| `dbo.RolePermissions.ModuleId` | `dbo.Modules.Id` | `sql/erp_full.sql` |
| `dbo.RolePermissions.SubModuleId` | `dbo.SubModules.Id` | `sql/erp_full.sql` |
| `dbo.RolePermissions.ScreenId` | `dbo.Screens.Id` | `sql/erp_full.sql` |
| `dbo.RolePermissions.ActionId` | `dbo.Actions.Id` | `sql/erp_full.sql` |
| `dbo.RolePermissionsLegacy.RoleId` | `dbo.Roles.RoleId` | `sql/erp_full.sql` |
| `dbo.SalesInvoice.POSSessionId` | `dbo.POSSessions.POSSessionId` | `sql/erp_full.sql` |
| `dbo.SalesReturnItem.SalesReturnId` | `dbo.SalesReturn.SalesReturnId` | `sql/erp_full.sql` |
| `dbo.Screens.SubModuleId` | `dbo.SubModules.Id` | `sql/erp_full.sql` |
| `dbo.Services.ServiceCategoryId` | `dbo.ServiceCategories.ServiceCategoryId` | `sql/erp_full.sql` |
| `dbo.Services.UnitId` | `dbo.Units.Id` | `sql/erp_full.sql` |
| `dbo.Services.HsnSacId` | `dbo.HsnSacs.HsnSacId` | `sql/erp_full.sql` |
| `dbo.Services.DefaultTaxId` | `dbo.Taxes.Id` | `sql/erp_full.sql` |
| `dbo.States.CountryId` | `dbo.Countries.CountryId` | `sql/erp_full.sql` |
| `dbo.StockAdjustmentItem.StockAdjustmentId` | `dbo.StockAdjustment.StockAdjustmentId` | `sql/erp_full.sql` |
| `dbo.StockCountItem.StockCountId` | `dbo.StockCount.StockCountId` | `sql/erp_full.sql` |
| `dbo.StockTransferItem.StockTransferId` | `dbo.StockTransfer.StockTransferId` | `sql/erp_full.sql` |
| `dbo.Stores.CompanyId` | `Companies.Id` | `sql/erp_full.sql` |
| `dbo.Stores.BranchId` | `Branches.Id` | `sql/erp_full.sql` |
| `dbo.SubModules.ModuleId` | `dbo.Modules.Id` | `sql/erp_full.sql` |
| `dbo.Subscriptions.TenantId` | `dbo.Tenants.TenantId` | `sql/platform_schema.sql` |
| `dbo.Subscriptions.PlanId` | `dbo.Plans.PlanId` | `sql/platform_schema.sql` |
| `dbo.Taxes.TaxTypeSystemId` | `dbo.TaxTypeSystems.Id` | `sql/erp_full.sql` |
| `dbo.TenantConnections.TenantId` | `dbo.Tenants.TenantId` | `sql/platform_schema.sql` |
| `dbo.UnitConversions.ProductId` | `dbo.Products.Id` | `sql/erp_full.sql` |
| `dbo.UnitConversions.FromUnitId` | `dbo.Units.Id` | `sql/erp_full.sql` |
| `dbo.UnitConversions.ToUnitId` | `dbo.Units.Id` | `sql/erp_full.sql` |
| `dbo.UserDataScopeOverrides.UserId` | `dbo.Users.UserId` | `sql/erp_full.sql` |
| `dbo.UserFieldPermissions.UserId` | `dbo.Users.UserId` | `sql/erp_full.sql` |
| `dbo.UserFieldPermissions.ScreenId` | `dbo.Screens.Id` | `sql/erp_full.sql` |
| `dbo.UserFieldPermissions.FieldId` | `dbo.Fields.Id` | `sql/erp_full.sql` |
| `dbo.UserPermissionOverrides.UserId` | `dbo.Users.UserId` | `sql/erp_full.sql` |
| `dbo.UserPermissionOverrides.WorkspaceId` | `dbo.Workspaces.Id` | `sql/erp_full.sql` |
| `dbo.UserPermissionOverrides.DomainId` | `dbo.Domains.Id` | `sql/erp_full.sql` |
| `dbo.UserPermissionOverrides.ModuleId` | `dbo.Modules.Id` | `sql/erp_full.sql` |
| `dbo.UserPermissionOverrides.SubModuleId` | `dbo.SubModules.Id` | `sql/erp_full.sql` |
| `dbo.UserPermissionOverrides.ScreenId` | `dbo.Screens.Id` | `sql/erp_full.sql` |
| `dbo.UserPermissionOverrides.ActionId` | `dbo.Actions.Id` | `sql/erp_full.sql` |
| `dbo.UserRoles.UserId` | `dbo.Users.UserId` | `sql/erp_full.sql` |
| `dbo.UserRoles.RoleId` | `dbo.Roles.RoleId` | `sql/erp_full.sql` |
| `dbo.Users.CompanyId` | `dbo.Companies.Id` | `sql/erp_full.sql` |
| `dbo.Warehouses.CompanyId` | `Companies.Id` | `sql/erp_full.sql` |
| `dbo.Warehouses.BranchId` | `Branches.Id` | `sql/erp_full.sql` |
| `dbo.Warehouses.WarehouseTypeId` | `WarehouseTypes.WarehouseTypeId` | `sql/erp_full.sql` |
| `dbo.Warehouses.ParentWarehouseId` | `Warehouses.Id` | `sql/erp_full.sql` |
| `dbo.Warehouses.ManagerEmployeeId` | `Employees.Id` | `sql/erp_full.sql` |
| `dbo.WorkflowPermissions.RoleId` | `dbo.Roles.RoleId` | `sql/erp_full.sql` |
| `dbo.WorkflowPermissions.ModuleId` | `dbo.Modules.Id` | `sql/erp_full.sql` |
| `dbo.WorkflowPermissions.SubModuleId` | `dbo.SubModules.Id` | `sql/erp_full.sql` |
| `dbo.WorkflowPermissions.ScreenId` | `dbo.Screens.Id` | `sql/erp_full.sql` |

### Logical application relationships

Logical links evidenced by repository queries/comments, not necessarily database constraints:
- `Payment` and `PaymentAllocation` are used together by payment, sales, purchase, and report repository paths.
- Sales/purchase header and line tables connect to product, partner, company, branch, and warehouse identifiers in repository SQL.
- Stock/StockTransaction connect product and warehouse/location identifiers in inventory repositories.
- Platform resolver queries join `Tenants`, `TenantConnections`, `Subscriptions`, and `Plans`.
- Other edges are UNKNOWN until query-by-query review.

## 14. Screen → API Mapping

Component service injection and literal Angular HTTP paths are used. `UNKNOWN` means the static parser could not establish a direct mapping; generic/dynamic pages may still call APIs.

| Route | Component | Services | Literal API calls |
|---|---|---|---|
| `/login` | `LoginPage` | `AuthService`, `NavigationStoreService`, `ThemeService`, `ToastService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions`<br>`GET /api/permissions/my-navigation` |
| `/contact-administrator` | `ContactAdministratorPage` | `AuthService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions` |
| `/access-denied` | `AccessDeniedPage` | UNKNOWN / indirect | UNKNOWN / no literal URL |
| `/` | `AppShell` | `AuthService`, `NavigationStoreService`, `ThemeService`, `PermissionService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions`<br>`GET /api/permissions/my-navigation`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/dashboard` | `DashboardPage` | `InventoryService`, `PermissionService`, `SalesReportService`, `SalesService`, `ToastService` | `GET /api/inventory/dashboard`<br>`POST /api/inventory/opening`<br>`GET /api/inventory/adjustments?page=${page}&size=${size}`<br>`POST /api/inventory/adjustments`<br>`POST /api/inventory/adjustments/${id}/post`<br>`GET /api/inventory/transfers?page=${page}&size=${size}`<br>`POST /api/inventory/transfers`<br>`POST /api/inventory/transfers/${id}/post`<br>`GET /api/inventory/counts?page=${page}&size=${size}`<br>`POST /api/inventory/counts`<br>`POST /api/inventory/counts/${id}/post`<br>`GET /api/inventory/reconciliation`<br>`GET /api/inventory/valuation`<br>`GET /api/inventory/low-stock`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}`<br>`GET /api/sales-reports`<br>`GET /api/sales`<br>`GET /api/sales/lookups`<br>`GET /api/sales/next-number`<br>`GET /api/sales/${id}`<br>`GET /api/sales/${id}/items`<br>`GET /api/sales/${id}/payments`<br>`GET /api/sales/${id}/stock`<br>`GET /api/sales/products/${productId}/stock`<br>`POST /api/sales`<br>`PUT /api/sales/${id}`<br>`DELETE /api/sales/${id}` |
| `/workspace/:id` | `WorkspacePage` | `NavigationStoreService` | `GET /api/permissions/my-navigation` |
| `/users` | `UsersPage` | `AuthService`, `ToastService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions` |
| `/roles` | `RolesPage` | `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/business-master` | `BusinessMasterPage` | UNKNOWN / indirect | UNKNOWN / no literal URL |
| `/company` | `CompanyPage` | `AdministrationService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/branch` | `BranchPage` | `AuthService`, `OrganizationService`, `PermissionService`, `ToastService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/product-categories` | `CategoryPage` | `AdministrationService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/product-subcategories` | `SubCategoryPage` | `AdministrationService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/brands` | `BrandPage` | `AdministrationService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/units` | `UnitPage` | `AdministrationService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/products` | `ProductPage` | `AuthService`, `AdministrationService`, `BillingMasterService`, `PermissionService`, `ToastService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/tax-type-systems` | `TaxTypeSystemPage` | `AdministrationService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/taxes` | `TaxPage` | `AdministrationService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/price-types` | `PriceTypePage` | `BillingMasterService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/unit-conversions` | `UnitConversionPage` | `AdministrationService`, `BillingMasterService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/barcodes` | `BarcodePage` | `AdministrationService`, `BillingMasterService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/hsn-sacs` | `HsnSacPage` | `AdministrationService`, `BillingMasterService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/service-categories` | `ServiceCategoryPage` | `BillingMasterService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/services` | `ServicePage` | `AdministrationService`, `BillingMasterService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/price-lists` | `PriceListPage` | `AdministrationService`, `BillingMasterService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/price-master` | `PriceMasterPage` | `AdministrationService`, `BillingMasterService`, `StockService`, `OrganizationService`, `PermissionService`, `ThemeService`, `ToastService` | `GET /api/stock`<br>`GET /api/stock/transactions`<br>`GET /api/stock/price-master-products`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/discount-rules` | `DiscountRulePage` | `AdministrationService`, `BillingMasterService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/offers` | `OfferPage` | `AdministrationService`, `BillingMasterService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/coupons` | `CouponPage` | `BillingMasterService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/master-import` | `MasterImportPage` | `MasterImportService`, `ImportLogService`, `PermissionService`, `ToastService` | `GET /api/import/masters`<br>`GET /api/import/${encodeURIComponent(entityName)}/template`<br>`POST /api/import/preview`<br>`POST /api/import/confirm`<br>`GET /api/import/${name}/export/meta`<br>`POST /api/import/${name}/export/options`<br>`POST /api/import/${name}/export/preview`<br>`POST /api/import/${name}/export`<br>`GET /api/import-logs`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/import-logs` | `MasterImportPage` | `MasterImportService`, `ImportLogService`, `PermissionService`, `ToastService` | `GET /api/import/masters`<br>`GET /api/import/${encodeURIComponent(entityName)}/template`<br>`POST /api/import/preview`<br>`POST /api/import/confirm`<br>`GET /api/import/${name}/export/meta`<br>`POST /api/import/${name}/export/options`<br>`POST /api/import/${name}/export/preview`<br>`POST /api/import/${name}/export`<br>`GET /api/import-logs`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/tenant-configuration` | `TenantConfigurationPage` | `TenantConfigurationService`, `PermissionService`, `ToastService` | `GET /api/tenant-configuration`<br>`GET /api/tenant-configuration/grouped`<br>`GET /api/tenant-configuration/${id}`<br>`POST /api/tenant-configuration`<br>`PUT /api/tenant-configuration/${id}`<br>`DELETE /api/tenant-configuration/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/purchase-entry` | `PurchaseEntryPage` | `KeyboardShortcutService`, `PurchaseHubService`, `PurchaseService`, `PermissionService`, `ToastService` | `GET /api/purchases`<br>`GET /api/purchases/lookups`<br>`GET /api/purchases/next-number`<br>`GET /api/purchases/${id}`<br>`GET /api/purchases/${id}/items`<br>`GET /api/purchases/${id}/payments`<br>`GET /api/purchases/${id}/stock`<br>`POST /api/purchases`<br>`PUT /api/purchases/${id}`<br>`POST /api/purchases/${id}/cancel`<br>`GET /api/purchases/${id}/delete-check`<br>`DELETE /api/purchases/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/purchase-register` | `PurchaseRegisterPage` | `PurchaseHubService`, `DocumentPrintService`, `PurchaseService`, `PermissionService`, `ToastService` | `GET /api/purchases`<br>`GET /api/purchases/lookups`<br>`GET /api/purchases/next-number`<br>`GET /api/purchases/${id}`<br>`GET /api/purchases/${id}/items`<br>`GET /api/purchases/${id}/payments`<br>`GET /api/purchases/${id}/stock`<br>`POST /api/purchases`<br>`PUT /api/purchases/${id}`<br>`POST /api/purchases/${id}/cancel`<br>`GET /api/purchases/${id}/delete-check`<br>`DELETE /api/purchases/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/purchase-view/:id` | `PurchaseViewPage` | `DocumentPrintService`, `PurchaseService`, `PermissionService`, `ToastService` | `GET /api/purchases`<br>`GET /api/purchases/lookups`<br>`GET /api/purchases/next-number`<br>`GET /api/purchases/${id}`<br>`GET /api/purchases/${id}/items`<br>`GET /api/purchases/${id}/payments`<br>`GET /api/purchases/${id}/stock`<br>`POST /api/purchases`<br>`PUT /api/purchases/${id}`<br>`POST /api/purchases/${id}/cancel`<br>`GET /api/purchases/${id}/delete-check`<br>`DELETE /api/purchases/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/purchase-edit/:id` | `PurchaseEditPage` | `PurchaseService`, `PermissionService`, `ToastService` | `GET /api/purchases`<br>`GET /api/purchases/lookups`<br>`GET /api/purchases/next-number`<br>`GET /api/purchases/${id}`<br>`GET /api/purchases/${id}/items`<br>`GET /api/purchases/${id}/payments`<br>`GET /api/purchases/${id}/stock`<br>`POST /api/purchases`<br>`PUT /api/purchases/${id}`<br>`POST /api/purchases/${id}/cancel`<br>`GET /api/purchases/${id}/delete-check`<br>`DELETE /api/purchases/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/purchase-cancel/:id` | `PurchaseCancelPage` | `PurchaseService`, `PermissionService`, `ToastService` | `GET /api/purchases`<br>`GET /api/purchases/lookups`<br>`GET /api/purchases/next-number`<br>`GET /api/purchases/${id}`<br>`GET /api/purchases/${id}/items`<br>`GET /api/purchases/${id}/payments`<br>`GET /api/purchases/${id}/stock`<br>`POST /api/purchases`<br>`PUT /api/purchases/${id}`<br>`POST /api/purchases/${id}/cancel`<br>`GET /api/purchases/${id}/delete-check`<br>`DELETE /api/purchases/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/purchase-delete/:id` | `PurchaseDeletePage` | `PurchaseService`, `PermissionService`, `ToastService` | `GET /api/purchases`<br>`GET /api/purchases/lookups`<br>`GET /api/purchases/next-number`<br>`GET /api/purchases/${id}`<br>`GET /api/purchases/${id}/items`<br>`GET /api/purchases/${id}/payments`<br>`GET /api/purchases/${id}/stock`<br>`POST /api/purchases`<br>`PUT /api/purchases/${id}`<br>`POST /api/purchases/${id}/cancel`<br>`GET /api/purchases/${id}/delete-check`<br>`DELETE /api/purchases/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/sales-entry` | `SalesEntryPage` | `KeyboardShortcutService`, `SalesHubService`, `DocumentPrintService`, `AdministrationService`, `BillingMasterService`, `StockService`, `PermissionService`, `PosService`, `SalesService`, `ThemeService`, `ToastService` | `GET /api/stock`<br>`GET /api/stock/transactions`<br>`GET /api/stock/price-master-products`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}`<br>`GET /api/counter-assignments/by-counter/${counterId}`<br>`GET /api/pos/dashboard`<br>`GET /api/pos/sessions/current`<br>`POST /api/pos/sessions/${id}/close`<br>`POST /api/pos/cash-in`<br>`POST /api/pos/cash-out`<br>`POST /api/pos/holds`<br>`GET /api/pos/holds`<br>`POST /api/pos/holds/${id}/recall`<br>`POST /api/pos/holds/${id}/cancel`<br>`GET /api/pos/sessions/${sessionId}/summary`<br>`GET /api/sales`<br>`GET /api/sales/lookups`<br>`GET /api/sales/next-number`<br>`GET /api/sales/${id}`<br>`GET /api/sales/${id}/items`<br>`GET /api/sales/${id}/payments`<br>`GET /api/sales/${id}/stock`<br>`GET /api/sales/products/${productId}/stock`<br>`POST /api/sales`<br>`PUT /api/sales/${id}`<br>`DELETE /api/sales/${id}` |
| `/sales` | `SalesWorkspace` | `SalesHubService` | UNKNOWN / no literal URL |
| `/pos` | `PosPage` | `KeyboardShortcutService`, `StockService`, `PermissionService`, `SalesService`, `ToastService` | `GET /api/stock`<br>`GET /api/stock/transactions`<br>`GET /api/stock/price-master-products`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}`<br>`GET /api/sales`<br>`GET /api/sales/lookups`<br>`GET /api/sales/next-number`<br>`GET /api/sales/${id}`<br>`GET /api/sales/${id}/items`<br>`GET /api/sales/${id}/payments`<br>`GET /api/sales/${id}/stock`<br>`GET /api/sales/products/${productId}/stock`<br>`POST /api/sales`<br>`PUT /api/sales/${id}`<br>`DELETE /api/sales/${id}` |
| `/purchase` | `PurchaseWorkspace` | `PurchaseHubService`, `PermissionService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/purchases/:id` | `PurchaseManagementPage` | `PurchaseService`, `PermissionService`, `ToastService` | `GET /api/purchases`<br>`GET /api/purchases/lookups`<br>`GET /api/purchases/next-number`<br>`GET /api/purchases/${id}`<br>`GET /api/purchases/${id}/items`<br>`GET /api/purchases/${id}/payments`<br>`GET /api/purchases/${id}/stock`<br>`POST /api/purchases`<br>`PUT /api/purchases/${id}`<br>`POST /api/purchases/${id}/cancel`<br>`GET /api/purchases/${id}/delete-check`<br>`DELETE /api/purchases/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/sales-return` | `SalesReturnPage` | `PermissionService`, `SalesReturnService`, `SalesService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}`<br>`GET /api/sales-returns`<br>`GET /api/sales-returns/${id}`<br>`GET /api/sales-returns/next-number`<br>`POST /api/sales-returns`<br>`POST /api/sales-returns/${id}/cancel`<br>`GET /api/sales`<br>`GET /api/sales/lookups`<br>`GET /api/sales/next-number`<br>`GET /api/sales/${id}`<br>`GET /api/sales/${id}/items`<br>`GET /api/sales/${id}/payments`<br>`GET /api/sales/${id}/stock`<br>`GET /api/sales/products/${productId}/stock`<br>`POST /api/sales`<br>`PUT /api/sales/${id}`<br>`DELETE /api/sales/${id}` |
| `/inventory` | `InventoryPage` | `InventoryService`, `PurchaseService`, `PermissionService`, `ToastService` | `GET /api/inventory/dashboard`<br>`POST /api/inventory/opening`<br>`GET /api/inventory/adjustments?page=${page}&size=${size}`<br>`POST /api/inventory/adjustments`<br>`POST /api/inventory/adjustments/${id}/post`<br>`GET /api/inventory/transfers?page=${page}&size=${size}`<br>`POST /api/inventory/transfers`<br>`POST /api/inventory/transfers/${id}/post`<br>`GET /api/inventory/counts?page=${page}&size=${size}`<br>`POST /api/inventory/counts`<br>`POST /api/inventory/counts/${id}/post`<br>`GET /api/inventory/reconciliation`<br>`GET /api/inventory/valuation`<br>`GET /api/inventory/low-stock`<br>`GET /api/purchases`<br>`GET /api/purchases/lookups`<br>`GET /api/purchases/next-number`<br>`GET /api/purchases/${id}`<br>`GET /api/purchases/${id}/items`<br>`GET /api/purchases/${id}/payments`<br>`GET /api/purchases/${id}/stock`<br>`POST /api/purchases`<br>`PUT /api/purchases/${id}`<br>`POST /api/purchases/${id}/cancel`<br>`GET /api/purchases/${id}/delete-check`<br>`DELETE /api/purchases/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/purchase-returns` | `PurchaseReturnPage` | `DocumentPrintService`, `PurchaseService`, `PurchaseReturnService`, `PermissionService`, `ToastService` | `GET /api/purchases`<br>`GET /api/purchases/lookups`<br>`GET /api/purchases/next-number`<br>`GET /api/purchases/${id}`<br>`GET /api/purchases/${id}/items`<br>`GET /api/purchases/${id}/payments`<br>`GET /api/purchases/${id}/stock`<br>`POST /api/purchases`<br>`PUT /api/purchases/${id}`<br>`POST /api/purchases/${id}/cancel`<br>`GET /api/purchases/${id}/delete-check`<br>`DELETE /api/purchases/${id}`<br>`GET /api/purchase-returns`<br>`GET /api/purchase-returns/next-number`<br>`GET /api/purchase-returns/${id}`<br>`POST /api/purchase-returns`<br>`PUT /api/purchase-returns/${id}`<br>`POST /api/purchase-returns/${id}/cancel`<br>`GET /api/purchase-returns/${id}/delete-check`<br>`DELETE /api/purchase-returns/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/purchase-returns/new` | `PurchaseReturnEntryPage` | `PurchaseService`, `PurchaseReturnService`, `PermissionService`, `ToastService` | `GET /api/purchases`<br>`GET /api/purchases/lookups`<br>`GET /api/purchases/next-number`<br>`GET /api/purchases/${id}`<br>`GET /api/purchases/${id}/items`<br>`GET /api/purchases/${id}/payments`<br>`GET /api/purchases/${id}/stock`<br>`POST /api/purchases`<br>`PUT /api/purchases/${id}`<br>`POST /api/purchases/${id}/cancel`<br>`GET /api/purchases/${id}/delete-check`<br>`DELETE /api/purchases/${id}`<br>`GET /api/purchase-returns`<br>`GET /api/purchase-returns/next-number`<br>`GET /api/purchase-returns/${id}`<br>`POST /api/purchase-returns`<br>`PUT /api/purchase-returns/${id}`<br>`POST /api/purchase-returns/${id}/cancel`<br>`GET /api/purchase-returns/${id}/delete-check`<br>`DELETE /api/purchase-returns/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/purchase-returns/:id` | `PurchaseReturnManagementPage` | `PurchaseService`, `PurchaseReturnService`, `PermissionService`, `ToastService` | `GET /api/purchases`<br>`GET /api/purchases/lookups`<br>`GET /api/purchases/next-number`<br>`GET /api/purchases/${id}`<br>`GET /api/purchases/${id}/items`<br>`GET /api/purchases/${id}/payments`<br>`GET /api/purchases/${id}/stock`<br>`POST /api/purchases`<br>`PUT /api/purchases/${id}`<br>`POST /api/purchases/${id}/cancel`<br>`GET /api/purchases/${id}/delete-check`<br>`DELETE /api/purchases/${id}`<br>`GET /api/purchase-returns`<br>`GET /api/purchase-returns/next-number`<br>`GET /api/purchase-returns/${id}`<br>`POST /api/purchase-returns`<br>`PUT /api/purchase-returns/${id}`<br>`POST /api/purchase-returns/${id}/cancel`<br>`GET /api/purchase-returns/${id}/delete-check`<br>`DELETE /api/purchase-returns/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/stock` | `StockPage` | `StockService`, `PermissionService`, `ToastService` | `GET /api/stock`<br>`GET /api/stock/transactions`<br>`GET /api/stock/price-master-products`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/reports` | `ReportsWorkspace` | UNKNOWN / indirect | UNKNOWN / no literal URL |
| `/payment-type` | `PaymentTypePage` | `PaymentTypeService`, `PermissionService`, `ToastService` | `GET /api/payment-types`<br>`GET /api/payment-types/${id}`<br>`POST /api/payment-types`<br>`PUT /api/payment-types/${id}`<br>`DELETE /api/payment-types/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/payment-method` | `PaymentMethodPage` | `PaymentMethodService`, `PermissionService`, `ToastService` | `GET /api/payment-methods`<br>`GET /api/payment-methods/${id}`<br>`POST /api/payment-methods`<br>`PUT /api/payment-methods/${id}`<br>`DELETE /api/payment-methods/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/payment-method-detail/:paymentMethodId` | `PaymentMethodDetailPage` | `PaymentMethodService`, `PaymentMethodDetailService`, `PermissionService`, `ToastService` | `GET /api/payment-methods`<br>`GET /api/payment-methods/${id}`<br>`POST /api/payment-methods`<br>`PUT /api/payment-methods/${id}`<br>`DELETE /api/payment-methods/${id}`<br>`GET /api/payment-method-details`<br>`GET /api/payment-method-details/${id}`<br>`POST /api/payment-method-details`<br>`PUT /api/payment-method-details/${id}`<br>`DELETE /api/payment-method-details/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/payment-entry` | `PaymentEntryPage` | `PaymentService`, `PermissionService`, `ToastService` | `GET /api/payments`<br>`GET /api/payments/lookups`<br>`GET /api/payments/next-number`<br>`GET /api/payments/${id}`<br>`POST /api/payments`<br>`PUT /api/payments/${id}`<br>`DELETE /api/payments/${id}`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/payment` | `PaymentWorkspace` | UNKNOWN / indirect | UNKNOWN / no literal URL |
| `/finance-year` | `FinanceYear` | `AuthService`, `PermissionService`, `PosService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}`<br>`GET /api/counter-assignments/by-counter/${counterId}`<br>`GET /api/pos/dashboard`<br>`GET /api/pos/sessions/current`<br>`POST /api/pos/sessions/${id}/close`<br>`POST /api/pos/cash-in`<br>`POST /api/pos/cash-out`<br>`POST /api/pos/holds`<br>`GET /api/pos/holds`<br>`POST /api/pos/holds/${id}/recall`<br>`POST /api/pos/holds/${id}/cancel`<br>`GET /api/pos/sessions/${sessionId}/summary` |
| `/system-master` | `SystemMasterPage` | `AuthService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions` |
| `/settings` | `SettingsPage` | `AuthService`, `ThemeService`, `ToastService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions` |
| `/role-permission-matrix` | `RolePermissionMatrixPage` | `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/enterprise-permissions` | `EnterprisePermissionsPage` | `AuthService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions` |
| `/workspaces` | `WorkspacesPage` | `ToastService` | UNKNOWN / no literal URL |
| `/domains` | `DomainsPage` | `ToastService` | UNKNOWN / no literal URL |
| `/modules` | `ModulesPage` | `ToastService` | UNKNOWN / no literal URL |
| `/submodules` | `SubModulesPage` | `ToastService` | UNKNOWN / no literal URL |
| `/screens` | `ScreensPage` | `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/fields` | `FieldsPage` | `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/permission-actions-list` | `ActionsPage` | `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/user-permission-overrides` | `UserPermissionOverridesPage` | `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/role-field-permissions` | `RoleFieldPermissionsPage` | `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/data-scopes` | `DataScopesPage` | `AuthService`, `AdministrationService`, `OrganizationService`, `ToastService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions` |
| `/user-data-scope-overrides` | `UserDataScopeOverridesPage` | `AuthService`, `AdministrationService`, `OrganizationService`, `ToastService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions` |
| `/workflow-permissions` | `WorkflowPermissionsPage` | `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/business-partner-roles` | `BusinessPartnerRolesPage` | `ToastService` | UNKNOWN / no literal URL |
| `/business-partners` | `BusinessPartnersPage` | `ToastService` | UNKNOWN / no literal URL |
| `/invoice-templates` | `InvoiceTemplatePage` | `InvoiceTemplateService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/document-design` | `DocumentDesignPage` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/document-designer` | `DocumentDesignerPage` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/document-design/preview` | `DocumentPreviewPage` | `DocumentDesignService`, `ToastService` | UNKNOWN / no literal URL |
| `/document-design/master/template-components` | `TemplateComponentsMasterComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/invoice-types` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/document-design/master/types` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/invoice-paper-sizes` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/document-design/master/paper-sizes` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/invoice-template-categories` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/document-design/master/categories` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/invoice-template-variables` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/document-design/master/variables` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/invoice-fonts` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/document-design/master/fonts` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/print-orientations` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/document-design/master/orientations` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/document-design/master/units` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/printer-types` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/document-design/master/printer-types` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/printer-models` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/document-design/master/printer-models` | `DocumentMasterPageComponent` | `DocumentDesignService`, `PermissionService`, `ToastService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/login` | `LoginPage` | `AuthService`, `NavigationStoreService`, `ThemeService`, `ToastService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions`<br>`GET /api/permissions/my-navigation` |
| `/` | `AppShell` | `AuthService`, `NavigationStoreService`, `ThemeService`, `PermissionService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions`<br>`GET /api/permissions/my-navigation`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}` |
| `/dashboard` | `DashboardPage` | `InventoryService`, `PermissionService`, `SalesReportService`, `SalesService`, `ToastService` | `GET /api/inventory/dashboard`<br>`POST /api/inventory/opening`<br>`GET /api/inventory/adjustments?page=${page}&size=${size}`<br>`POST /api/inventory/adjustments`<br>`POST /api/inventory/adjustments/${id}/post`<br>`GET /api/inventory/transfers?page=${page}&size=${size}`<br>`POST /api/inventory/transfers`<br>`POST /api/inventory/transfers/${id}/post`<br>`GET /api/inventory/counts?page=${page}&size=${size}`<br>`POST /api/inventory/counts`<br>`POST /api/inventory/counts/${id}/post`<br>`GET /api/inventory/reconciliation`<br>`GET /api/inventory/valuation`<br>`GET /api/inventory/low-stock`<br>`GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}`<br>`GET /api/sales-reports`<br>`GET /api/sales`<br>`GET /api/sales/lookups`<br>`GET /api/sales/next-number`<br>`GET /api/sales/${id}`<br>`GET /api/sales/${id}/items`<br>`GET /api/sales/${id}/payments`<br>`GET /api/sales/${id}/stock`<br>`GET /api/sales/products/${productId}/stock`<br>`POST /api/sales`<br>`PUT /api/sales/${id}`<br>`DELETE /api/sales/${id}` |
| `/tenants` | `TenantsPage` | `ToastService` | UNKNOWN / no literal URL |
| `/plans` | `PlansPage` | `ToastService` | UNKNOWN / no literal URL |
| `/subscriptions` | `SubscriptionsPage` | `ToastService` | UNKNOWN / no literal URL |
| `/migrations` | `MigrationsPage` | `PermissionService`, `ToastService`, `MigrationService` | `GET /api/data-scopes/my`<br>`GET /api/roles/my`<br>`GET /api/data-scopes/role/${roleId}`<br>`GET /api/user-data-scope-overrides/user/${userId}`<br>`GET /api/tenants/${tenantId}/migration-status`<br>`POST /api/tenants/${tenantId}/migration/run`<br>`GET /api/tenants/${tenantId}/migration/history?take=${take}` |
| `/settings` | `SettingsPage` | `AuthService`, `ThemeService`, `ToastService` | `POST /api/auth/login`<br>`POST /api/auth/refresh`<br>`POST /api/auth/logout`<br>`GET /api/permission/user-permissions` |

## 15. API → Service → Repository → DB Mapping

Controller/service/repository links are joined by matching injected interface names to implementing classes. Table names are literal repository `dbo.*` references. An UNKNOWN link means static name matching could not resolve it.

| Controller | Service implementation | Repository implementation | Tables mentioned | Permissions |
|---|---|---|---|---|
| `ActionsController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `AddressTypesController` | `AddressTypeService` | UNKNOWN | UNKNOWN | Permissions.AddressTypesManage |
| `AdministrationController` | `AdministrationService` | UNKNOWN | UNKNOWN | Permissions.CurrenciesManage |
| `AuthController` | `AuthService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `BarcodesController` | `BarcodeService`, `CouponService`, `DiscountRuleService`, `HsnSacService`, `OfferDetailService`, `OfferService`, `PriceListDetailService`, `PriceListPriceTypeService`, `PriceListService`, `PriceTypeService`, `ServiceCategoryService`, `ServiceService`, `UnitConversionService` | UNKNOWN | UNKNOWN | Permissions.BarcodesView, Permissions.BarcodesCreate, Permissions.BarcodesEdit, Permissions.BarcodesDelete |
| `BaseController` | UNKNOWN | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `BusinessPartnerRolesController` | `BusinessPartnerRoleService`, `BusinessPartnerService`, `BusinessTypeService`, `CompanyGroupService`, `IndustryTypeService`, `OperatorTypeService`, `SourceService`, `StoreTypeService` | UNKNOWN | UNKNOWN | Permissions.BusinessPartnerRolesView, Permissions.BusinessPartnerRolesManage |
| `BusinessPartnersController` | `BusinessPartnerRoleService`, `BusinessPartnerService`, `BusinessTypeService`, `CompanyGroupService`, `IndustryTypeService`, `OperatorTypeService`, `SourceService`, `StoreTypeService` | UNKNOWN | UNKNOWN | Permissions.BusinessPartnersView, Permissions.BusinessPartnersManage |
| `BusinessTypesController` | `BusinessPartnerRoleService`, `BusinessPartnerService`, `BusinessTypeService`, `CompanyGroupService`, `IndustryTypeService`, `OperatorTypeService`, `SourceService`, `StoreTypeService` | UNKNOWN | UNKNOWN | Permissions.BusinessTypesManage |
| `CitiesController` | `CityService`, `CountryService`, `StateService` | UNKNOWN | UNKNOWN | Permissions.LocationsView, Permissions.LocationsCreate, Permissions.LocationsEdit, Permissions.LocationsDelete |
| `CompaniesController` | `CompanyService` | UNKNOWN | UNKNOWN | Permissions.CompaniesView, Permissions.CompaniesCreate, Permissions.CompaniesEdit |
| `CompanyGroupsController` | `BusinessPartnerRoleService`, `BusinessPartnerService`, `BusinessTypeService`, `CompanyGroupService`, `IndustryTypeService`, `OperatorTypeService`, `SourceService`, `StoreTypeService` | UNKNOWN | UNKNOWN | Permissions.CompanyGroupsManage |
| `ContactTypesController` | `ContactTypeService` | UNKNOWN | UNKNOWN | Permissions.ContactTypesManage |
| `CounterAssignmentsController` | `CounterAssignmentService`, `OperatorService` | UNKNOWN | UNKNOWN | Permissions.CounterAssignmentsView, Permissions.CounterAssignmentsCreate, Permissions.CounterAssignmentsEdit, Permissions.CounterAssignmentsDelete |
| `CountersController` | `CounterService`, `POSSessionService`, `StoreService` | UNKNOWN | UNKNOWN | Permissions.CountersView, Permissions.CountersCreate, Permissions.CountersEdit, Permissions.CountersDelete |
| `CountriesController` | `CityService`, `CountryService`, `StateService` | UNKNOWN | UNKNOWN | Permissions.LocationsView, Permissions.LocationsCreate, Permissions.LocationsEdit, Permissions.LocationsDelete |
| `CouponsController` | `BarcodeService`, `CouponService`, `DiscountRuleService`, `HsnSacService`, `OfferDetailService`, `OfferService`, `PriceListDetailService`, `PriceListPriceTypeService`, `PriceListService`, `PriceTypeService`, `ServiceCategoryService`, `ServiceService`, `UnitConversionService` | UNKNOWN | UNKNOWN | Permissions.CouponsView, Permissions.CouponsCreate, Permissions.CouponsEdit, Permissions.CouponsDelete |
| `DashboardController` | `DashboardService`, `DashboardService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `DataScopesController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `DiscountRulesController` | `BarcodeService`, `CouponService`, `DiscountRuleService`, `HsnSacService`, `OfferDetailService`, `OfferService`, `PriceListDetailService`, `PriceListPriceTypeService`, `PriceListService`, `PriceTypeService`, `ServiceCategoryService`, `ServiceService`, `UnitConversionService` | UNKNOWN | UNKNOWN | Permissions.DiscountRulesView, Permissions.DiscountRulesCreate, Permissions.DiscountRulesEdit, Permissions.DiscountRulesDelete |
| `DocumentCategoriesController` | `DocumentDesignService`, `DocumentSettingService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `DocumentComponentsController` | `DocumentDesignService`, `DocumentSettingService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `DocumentFontsController` | `DocumentDesignService`, `DocumentSettingService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `DocumentLookupsController` | `DocumentDesignService`, `DocumentSettingService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `DocumentMasterTypesController` | `DocumentDesignService`, `DocumentSettingService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `DocumentOrientationsController` | `DocumentDesignService`, `DocumentSettingService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `DocumentPaperSizesController` | `DocumentDesignService`, `DocumentSettingService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `DocumentPrinterModelsController` | `DocumentDesignService`, `DocumentSettingService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `DocumentPrinterTypesController` | `DocumentDesignService`, `DocumentSettingService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `DocumentSettingsController` | `DocumentDesignService`, `DocumentSettingService` | UNKNOWN | UNKNOWN | Permissions.InvoiceTemplatesView, Permissions.InvoiceTemplatesAssign |
| `DocumentTemplatesController` | `DocumentDesignService`, `DocumentSettingService` | UNKNOWN | UNKNOWN | Permissions.InvoiceTemplatesView, Permissions.InvoiceTemplatesCreate, Permissions.InvoiceTemplatesEdit, Permissions.InvoiceTemplatesDelete, Permissions.InvoiceTemplatesPreview, Permissions.InvoiceTemplatesPublish, Permissions.InvoiceTemplatesAssign |
| `DocumentTypesController` | `DocumentTypeService` | UNKNOWN | UNKNOWN | Permissions.DocumentTypesManage |
| `DocumentUnitsController` | `DocumentDesignService`, `DocumentSettingService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `DocumentVariablesController` | `DocumentDesignService`, `DocumentSettingService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `DomainsController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `EntitiesController` | `EntityService` | UNKNOWN | UNKNOWN | Permissions.EntitiesManage, Permissions.EntitiesView |
| `FieldPermissionsController` | `FieldPermissionService`, `ModulePermissionService`, `PermissionActionService`, `PermissionModuleService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `FieldsController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `FinancialYearsController` | `FinancialYearService` | UNKNOWN | UNKNOWN | Permissions.FinancialYearsView, Permissions.FinancialYearsCreate, Permissions.FinancialYearsEdit, Permissions.FinancialYearsDelete |
| `GstRegistrationTypesController` | `GstRegistrationTypeService` | UNKNOWN | UNKNOWN | Permissions.GstRegistrationTypesManage |
| `HsnSacsController` | `BarcodeService`, `CouponService`, `DiscountRuleService`, `HsnSacService`, `OfferDetailService`, `OfferService`, `PriceListDetailService`, `PriceListPriceTypeService`, `PriceListService`, `PriceTypeService`, `ServiceCategoryService`, `ServiceService`, `UnitConversionService` | UNKNOWN | UNKNOWN | Permissions.HsnSacsView, Permissions.HsnSacsCreate, Permissions.HsnSacsEdit, Permissions.HsnSacsDelete |
| `ImportLogsController` | `ImportLogService` | UNKNOWN | UNKNOWN | Permissions.ImportLogsView |
| `IndustryTypesController` | `BusinessPartnerRoleService`, `BusinessPartnerService`, `BusinessTypeService`, `CompanyGroupService`, `IndustryTypeService`, `OperatorTypeService`, `SourceService`, `StoreTypeService` | UNKNOWN | UNKNOWN | Permissions.IndustryTypesManage |
| `InventoryController` | `InventoryService` | UNKNOWN | UNKNOWN | Permissions.StockView, Permissions.StockManage |
| `LanguagesController` | `LanguageService` | UNKNOWN | UNKNOWN | Permissions.LanguagesManage |
| `MasterImportController` | `MasterExportService`, `MasterImportService` | UNKNOWN | UNKNOWN | Permissions.MasterImportView, Permissions.MasterImportManage |
| `ModulePermissionsController` | `FieldPermissionService`, `ModulePermissionService`, `PermissionActionService`, `PermissionModuleService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `ModulesController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `NavigationController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `OfferDetailsController` | `BarcodeService`, `CouponService`, `DiscountRuleService`, `HsnSacService`, `OfferDetailService`, `OfferService`, `PriceListDetailService`, `PriceListPriceTypeService`, `PriceListService`, `PriceTypeService`, `ServiceCategoryService`, `ServiceService`, `UnitConversionService` | UNKNOWN | UNKNOWN | Permissions.OffersView, Permissions.OffersCreate, Permissions.OffersEdit, Permissions.OffersDelete |
| `OffersController` | `BarcodeService`, `CouponService`, `DiscountRuleService`, `HsnSacService`, `OfferDetailService`, `OfferService`, `PriceListDetailService`, `PriceListPriceTypeService`, `PriceListService`, `PriceTypeService`, `ServiceCategoryService`, `ServiceService`, `UnitConversionService` | UNKNOWN | UNKNOWN | Permissions.OffersView, Permissions.OffersEdit, Permissions.OffersCreate, Permissions.OffersDelete |
| `OperatorsController` | `CounterAssignmentService`, `OperatorService` | UNKNOWN | UNKNOWN | Permissions.OperatorsView, Permissions.OperatorsCreate, Permissions.OperatorsEdit, Permissions.OperatorsDelete |
| `OperatorTypesController` | `BusinessPartnerRoleService`, `BusinessPartnerService`, `BusinessTypeService`, `CompanyGroupService`, `IndustryTypeService`, `OperatorTypeService`, `SourceService`, `StoreTypeService` | UNKNOWN | UNKNOWN | Permissions.OperatorTypesManage |
| `OrganizationController` | `BranchService`, `BranchTypeService`, `DepartmentService`, `DesignationService`, `EmployeeService`, `EmploymentTypeService`, `WarehouseService`, `WarehouseTypeService` | UNKNOWN | UNKNOWN | Permissions.BranchesView, Permissions.BranchesCreate, Permissions.BranchesEdit, Permissions.DepartmentsView, Permissions.DepartmentsCreate, Permissions.DepartmentsEdit, Permissions.DesignationsView, Permissions.DesignationsCreate, Permissions.DesignationsEdit, Permissions.EmployeesView, Permissions.EmployeesCreate, Permissions.EmployeesEdit, Permissions.EmployeesDelete, Permissions.WarehousesView, Permissions.WarehousesCreate, Permissions.WarehousesEdit |
| `OrganizationTypesController` | `OrganizationTypeService` | UNKNOWN | UNKNOWN | Permissions.OrganizationTypesManage |
| `PaymentMethodDetailsController` | `PaymentMethodDetailService`, `PaymentMethodService`, `PaymentService`, `PaymentTypeService` | UNKNOWN | UNKNOWN | Permissions.PaymentMethodDetailsView, Permissions.PaymentMethodDetailsManage |
| `PaymentMethodsController` | `PaymentMethodDetailService`, `PaymentMethodService`, `PaymentService`, `PaymentTypeService` | UNKNOWN | UNKNOWN | Permissions.PaymentMethodsView, Permissions.PaymentMethodsManage |
| `PaymentsController` | `PaymentMethodDetailService`, `PaymentMethodService`, `PaymentService`, `PaymentTypeService` | UNKNOWN | UNKNOWN | Permissions.PaymentsView, Permissions.PaymentsManage |
| `PaymentTypesController` | `PaymentMethodDetailService`, `PaymentMethodService`, `PaymentService`, `PaymentTypeService` | UNKNOWN | UNKNOWN | Permissions.PaymentTypesView, Permissions.PaymentTypesManage |
| `PermissionActionsController` | `FieldPermissionService`, `ModulePermissionService`, `PermissionActionService`, `PermissionModuleService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `PermissionModulesController` | `FieldPermissionService`, `ModulePermissionService`, `PermissionActionService`, `PermissionModuleService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `PermissionsController` | `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `POSOperationsController` | `POSOperationsService` | UNKNOWN | UNKNOWN | Permissions.SalesPOSView, Permissions.POSSessionView, Permissions.POSSessionEdit, Permissions.SalesPOSView, Permissions.SalesManage, Permissions.SalesPOSView, Permissions.SalesView |
| `POSSessionsController` | `CounterService`, `POSSessionService`, `StoreService` | UNKNOWN | UNKNOWN | Permissions.POSSessionView, Permissions.POSSessionCreate, Permissions.POSSessionEdit, Permissions.POSSessionDelete |
| `PriceListDetailsController` | `BarcodeService`, `CouponService`, `DiscountRuleService`, `HsnSacService`, `OfferDetailService`, `OfferService`, `PriceListDetailService`, `PriceListPriceTypeService`, `PriceListService`, `PriceTypeService`, `ServiceCategoryService`, `ServiceService`, `UnitConversionService` | UNKNOWN | UNKNOWN | Permissions.PriceListsView, Permissions.PriceListsCreate, Permissions.PriceListsEdit, Permissions.PriceListsDelete |
| `PriceListPriceTypesController` | `BarcodeService`, `CouponService`, `DiscountRuleService`, `HsnSacService`, `OfferDetailService`, `OfferService`, `PriceListDetailService`, `PriceListPriceTypeService`, `PriceListService`, `PriceTypeService`, `ServiceCategoryService`, `ServiceService`, `UnitConversionService` | UNKNOWN | UNKNOWN | Permissions.PriceListsView, Permissions.PriceListsEdit |
| `PriceListsController` | `BarcodeService`, `CouponService`, `DiscountRuleService`, `HsnSacService`, `OfferDetailService`, `OfferService`, `PriceListDetailService`, `PriceListPriceTypeService`, `PriceListService`, `PriceTypeService`, `ServiceCategoryService`, `ServiceService`, `UnitConversionService` | UNKNOWN | UNKNOWN | Permissions.PriceListsView, Permissions.PriceListsEdit, Permissions.PriceListsCreate, Permissions.PriceListsDelete |
| `PriceTypesController` | `BarcodeService`, `CouponService`, `DiscountRuleService`, `HsnSacService`, `OfferDetailService`, `OfferService`, `PriceListDetailService`, `PriceListPriceTypeService`, `PriceListService`, `PriceTypeService`, `ServiceCategoryService`, `ServiceService`, `UnitConversionService` | UNKNOWN | UNKNOWN | Permissions.PriceTypesView, Permissions.PriceTypesCreate, Permissions.PriceTypesEdit, Permissions.PriceTypesDelete |
| `ProductBrandsController` | `ProductBrandService`, `ProductCategoryService`, `ProductService`, `ProductSubCategoryService`, `ProductUnitService` | UNKNOWN | UNKNOWN | Permissions.BrandsView, Permissions.BrandsCreate, Permissions.BrandsEdit, Permissions.BrandsDelete |
| `ProductCategoriesController` | `ProductBrandService`, `ProductCategoryService`, `ProductService`, `ProductSubCategoryService`, `ProductUnitService` | UNKNOWN | UNKNOWN | Permissions.ProductCategoriesView, Permissions.ProductCategoriesCreate, Permissions.ProductCategoriesEdit, Permissions.ProductCategoriesDelete |
| `ProductsController` | `ProductBrandService`, `ProductCategoryService`, `ProductService`, `ProductSubCategoryService`, `ProductUnitService` | UNKNOWN | UNKNOWN | Permissions.ProductsView, Permissions.ProductsCreate, Permissions.ProductsEdit, Permissions.ProductsDelete |
| `ProductSubCategoriesController` | `ProductBrandService`, `ProductCategoryService`, `ProductService`, `ProductSubCategoryService`, `ProductUnitService` | UNKNOWN | UNKNOWN | Permissions.ProductSubCategoriesView, Permissions.ProductSubCategoriesCreate, Permissions.ProductSubCategoriesEdit, Permissions.ProductSubCategoriesDelete |
| `ProductUnitsController` | `ProductBrandService`, `ProductCategoryService`, `ProductService`, `ProductSubCategoryService`, `ProductUnitService` | UNKNOWN | UNKNOWN | Permissions.UnitsView, Permissions.UnitsCreate, Permissions.UnitsEdit, Permissions.UnitsDelete |
| `ProfileController` | `ProfileService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `PurchaseReportsController` | `PurchaseReportService` | UNKNOWN | UNKNOWN | Permissions.PurchasesView, Permissions.PurchasesReturnView, Permissions.PurchaseReturnView |
| `PurchaseReturnsController` | `PurchaseReturnService`, `StockService` | UNKNOWN | UNKNOWN | Permissions.PurchasesReturnView, Permissions.PurchasesReturnManage, Permissions.PurchaseReturnView, Permissions.PurchaseReturnDelete, Permissions.PurchasesReturnManage, Permissions.PurchaseReturnCreate, Permissions.PurchasesReturnManage, Permissions.PurchaseReturnEdit, Permissions.PurchasesReturnManage, Permissions.PurchaseReturnCancel, Permissions.PurchasesReturnManage, Permissions.PurchaseReturnDelete |
| `PurchasesController` | `BarcodeService`, `CouponService`, `DiscountRuleService`, `HsnSacService`, `OfferDetailService`, `OfferService`, `PriceListDetailService`, `PriceListPriceTypeService`, `PriceListService`, `PriceTypeService`, `ServiceCategoryService`, `ServiceService`, `UnitConversionService`, `CompanyService`, `BusinessPartnerRoleService`, `BusinessPartnerService`, `BusinessTypeService`, `CompanyGroupService`, `IndustryTypeService`, `OperatorTypeService`, `SourceService`, `StoreTypeService`, `BranchService`, `BranchTypeService`, `DepartmentService`, `DesignationService`, `EmployeeService`, `EmploymentTypeService`, `WarehouseService`, `WarehouseTypeService`, `PaymentMethodDetailService`, `PaymentMethodService`, `PaymentService`, `PaymentTypeService`, `ProductBrandService`, `ProductCategoryService`, `ProductService`, `ProductSubCategoryService`, `ProductUnitService`, `PurchaseService`, `TaxService`, `TaxTypeSystemService` | UNKNOWN | UNKNOWN | Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesEdit, Permissions.PurchasesCancel, Permissions.PurchasesDelete, Permissions.PurchasesView, Permissions.PurchasesManage, Permissions.PurchasesDelete, Permissions.PurchasesManage, Permissions.PurchasesCreate, Permissions.PurchasesManage, Permissions.PurchasesEdit, Permissions.PurchasesManage, Permissions.PurchasesCancel, Permissions.PurchasesManage, Permissions.PurchasesDelete |
| `RefundsController` | `SalesReturnService` | UNKNOWN | UNKNOWN | Permissions.SalesReturnManage, Permissions.PaymentsManage, Permissions.SalesReturnView, Permissions.PaymentsView |
| `RoleFieldPermissionsController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `RolePermissionsController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `RolesController` | `RoleService` | UNKNOWN | UNKNOWN | Permissions.RolesView, Permissions.RolesManage |
| `SaleEntryContextController` | `SaleEntryContextService` | UNKNOWN | UNKNOWN | Permissions.SalesView, Permissions.SalesManage |
| `SalesController` | `CompanyService`, `BusinessPartnerRoleService`, `BusinessPartnerService`, `BusinessTypeService`, `CompanyGroupService`, `IndustryTypeService`, `OperatorTypeService`, `SourceService`, `StoreTypeService`, `BranchService`, `BranchTypeService`, `DepartmentService`, `DesignationService`, `EmployeeService`, `EmploymentTypeService`, `WarehouseService`, `WarehouseTypeService`, `PaymentMethodDetailService`, `PaymentMethodService`, `PaymentService`, `PaymentTypeService`, `ProductBrandService`, `ProductCategoryService`, `ProductService`, `ProductSubCategoryService`, `ProductUnitService`, `SalesReturnService`, `SalesService` | UNKNOWN | UNKNOWN | Permissions.SalesView, Permissions.SalesManage, Permissions.SalesReturnManage |
| `SalesReportsController` | `SalesReportService` | UNKNOWN | UNKNOWN | Permissions.SalesView |
| `SalesReturnsController` | `SalesReturnService` | UNKNOWN | UNKNOWN | Permissions.SalesReturnView, Permissions.SalesReturnManage, Permissions.SalesReturnManage |
| `ScreensController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `ServiceCategoriesController` | `BarcodeService`, `CouponService`, `DiscountRuleService`, `HsnSacService`, `OfferDetailService`, `OfferService`, `PriceListDetailService`, `PriceListPriceTypeService`, `PriceListService`, `PriceTypeService`, `ServiceCategoryService`, `ServiceService`, `UnitConversionService` | UNKNOWN | UNKNOWN | Permissions.ServiceCategoriesView, Permissions.ServiceCategoriesCreate, Permissions.ServiceCategoriesEdit, Permissions.ServiceCategoriesDelete |
| `ServicesController` | `BarcodeService`, `CouponService`, `DiscountRuleService`, `HsnSacService`, `OfferDetailService`, `OfferService`, `PriceListDetailService`, `PriceListPriceTypeService`, `PriceListService`, `PriceTypeService`, `ServiceCategoryService`, `ServiceService`, `UnitConversionService` | UNKNOWN | UNKNOWN | Permissions.ServicesView, Permissions.ServicesCreate, Permissions.ServicesEdit, Permissions.ServicesDelete |
| `SettingsController` | `SettingsService` | UNKNOWN | UNKNOWN | Permissions.SettingsView, Permissions.SettingsEdit |
| `SourcesController` | `BusinessPartnerRoleService`, `BusinessPartnerService`, `BusinessTypeService`, `CompanyGroupService`, `IndustryTypeService`, `OperatorTypeService`, `SourceService`, `StoreTypeService` | UNKNOWN | UNKNOWN | Permissions.SourcesManage |
| `StatesController` | `CityService`, `CountryService`, `StateService` | UNKNOWN | UNKNOWN | Permissions.LocationsView, Permissions.LocationsCreate, Permissions.LocationsEdit, Permissions.LocationsDelete |
| `StockController` | `PurchaseReturnService`, `StockService` | UNKNOWN | UNKNOWN | Permissions.StockView |
| `StoresController` | `CounterService`, `POSSessionService`, `StoreService` | UNKNOWN | UNKNOWN | Permissions.StoresView, Permissions.StoresCreate, Permissions.StoresEdit, Permissions.StoresDelete |
| `StoreTypesController` | `BusinessPartnerRoleService`, `BusinessPartnerService`, `BusinessTypeService`, `CompanyGroupService`, `IndustryTypeService`, `OperatorTypeService`, `SourceService`, `StoreTypeService` | UNKNOWN | UNKNOWN | Permissions.StoreTypesManage |
| `SubModulesController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `TaxesController` | `TaxService`, `TaxTypeSystemService` | UNKNOWN | UNKNOWN | Permissions.TaxesView, Permissions.TaxesCreate, Permissions.TaxesEdit, Permissions.TaxesDelete |
| `TaxTypeSystemsController` | `TaxService`, `TaxTypeSystemService` | UNKNOWN | UNKNOWN | Permissions.TaxTypeSystemsView, Permissions.TaxTypeSystemsCreate, Permissions.TaxTypeSystemsEdit, Permissions.TaxTypeSystemsDelete |
| `TenantConfigurationController` | `TenantConfigurationService` | UNKNOWN | UNKNOWN | Permissions.TenantConfigView, Permissions.TenantConfigManage |
| `TimeZonesController` | `TimeZoneService` | UNKNOWN | UNKNOWN | Permissions.TimeZonesManage |
| `UnitConversionsController` | `BarcodeService`, `CouponService`, `DiscountRuleService`, `HsnSacService`, `OfferDetailService`, `OfferService`, `PriceListDetailService`, `PriceListPriceTypeService`, `PriceListService`, `PriceTypeService`, `ServiceCategoryService`, `ServiceService`, `UnitConversionService` | UNKNOWN | UNKNOWN | Permissions.UnitConversionsView, Permissions.UnitConversionsCreate, Permissions.UnitConversionsEdit, Permissions.UnitConversionsDelete |
| `UserDataScopeOverridesController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `UserFieldPermissionsController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `UserPermissionOverridesController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `UserPermissionsController` | `FieldPermissionService`, `ModulePermissionService`, `PermissionActionService`, `PermissionModuleService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `UsersController` | `UserService` | UNKNOWN | UNKNOWN | Permissions.UsersView, Permissions.UsersCreate, Permissions.UsersEdit, Permissions.UsersDelete |
| `WorkflowPermissionsController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `WorkspacesController` | `ActionService`, `DataScopeService`, `DomainService`, `FieldService`, `ModuleService`, `RoleFieldPermissionEntryService`, `RolePermissionEntryService`, `ScreenService`, `SubModuleService`, `UserDataScopeOverrideService`, `UserFieldPermissionEntryService`, `UserPermissionOverrideService`, `WorkflowPermissionEntryService`, `WorkspaceService`, `NavigationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `AuthController` | `PlatformAuthService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `BaseController` | UNKNOWN | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `DashboardController` | `DashboardService`, `DashboardService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `PlansController` | `PlanService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `SubscriptionsController` | `SubscriptionService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `TenantMigrationsController` | `TenantMigrationService` | UNKNOWN | UNKNOWN | No explicit permission parsed |
| `TenantsController` | `TenantService` | UNKNOWN | UNKNOWN | No explicit permission parsed |

## 16. Screen → API → DB Business Flow

Source-level flow summary only; QA and side effects are UNKNOWN unless tested.

| Feature | Screen evidence | API/controller | Service/repository/database evidence | Status |
|---|---|---|---|---|
| Authentication | `/login` | Auth controllers | `AuthService`, `UserRepository`, tenant resolver; Users and platform tenant maps | Source exists; runtime UNKNOWN |
| Sales | Sales entry/register/returns routes | Sales/SalesReturns controllers | SalesService/SalesRepository; SalesInvoice, SalesInvoiceItem, SalesReturn and related payment/stock references | Source exists; integrated runtime UNKNOWN |
| Purchase | Purchase entry/register/returns routes | Purchases/PurchaseReturns controllers | PurchaseService/PurchaseRepository; Purchase, PurchaseItem, PurchaseReturn tables and payment/stock references | T073 QA reported pending; runtime UNKNOWN |
| Inventory | Inventory and stock routes | Inventory/Stock controllers | InventoryService/InventoryRepository/StockRepository; Stock, StockTransaction, adjustment/transfer/count declarations | Migration/runtime state UNKNOWN |
| Payment | Payment workspace/type/method/detail routes | Payments/PaymentTypes controllers | Payment services/repositories; PaymentType, PaymentMethod, PaymentMethodDetails, Payment, PaymentAllocation | Source present; migration and runtime QA pending |
| POS | POS/session/counter routes | POS operations controllers | POS services/repositories; POSSessions, POSCashMovement, POSHoldBill and sales/stock references | Source present; runtime UNKNOWN |
| Reports | Sales/purchase reports routes | SalesReports/PurchaseReports controllers | Report services/repositories use transaction, partner, payment and stock sources | Source present; runtime/scope UNKNOWN |
| Platform tenancy | Platform dashboard/tenant/plan/subscription/migration routes | Platform controllers | Platform services/repositories; Tenants, TenantConnections, Subscriptions, Plans, Migrations, TenantMigrationHistory | Source present; deployed state UNKNOWN |
| Administration/security | User, role, scope, workspace and permission routes | Users/Roles/Administration/Permission controllers | Permission and data-scope repositories; roles, permissions, scopes, screens/actions and overrides tables | Source present; effective claims UNKNOWN |

## 17. Business Flow Matrix

| Flow | Source-backed outline | Status |
|---|---|---|
| Sales | Sale entry → API/service/repository → SalesInvoice and item rows; code also references payments/allocations, stock and returns. | Present in source; ordered side effects and runtime QA UNKNOWN. |
| Purchase | Purchase entry → API/service/repository → Purchase and item rows; source also references payment/allocation, tax, stock, return, cancellation and print. | T073 QA reported pending; runtime UNKNOWN. |
| Inventory | Inventory/stock routes → API/service/repository → Stock/StockTransaction and stock document tables. | Partial until migrations and full QA verified. |
| Payments | Payment entry/register → PaymentsController → PaymentService/Repository → Payment/PaymentAllocation. | Partial; current edits are not live/runtime verified. |
| Tenant migrations | Platform migrations UI → migration controller/service/executor → tenant connection/script/history. | Source exists; runtime database/catalog unknown. |

## 18. Permission / Security Map

- Tenant API `Program.cs` registers JWT authentication and a global `PermissionAuthorizationFilter`. The filter checks `[Permission(...)]` against permission claims; source has a super-admin bypass.
- Actions without `[Permission]` are authentication-only under the filter behavior; see API inventory.
- Tenant UI route tree applies `authGuard` to the root and `navigationChildGuard` to children. Many components perform local permission checks. UI checks are not a substitute for backend permission checks.
- Tenant context middleware reads `X-Tenant-Code` or the authenticated tenant-code claim and resolves a tenant connection through platform tables.
- `DataScopeResolver` defines Company, Branch and Warehouse scope lookup. No Store scope method exists in this resolver; other checks may exist elsewhere.
- Platform authorization must be evaluated per controller/action; live claims have not been tested.

### Shared permission constants

| Constant | Code |
|---|---|
| `DashboardView` | `dashboard.view` |
| `CompaniesView` | `companies.view` |
| `CompaniesCreate` | `companies.create` |
| `CompaniesEdit` | `companies.edit` |
| `UsersView` | `users.view` |
| `UsersCreate` | `users.create` |
| `UsersEdit` | `users.edit` |
| `UsersDelete` | `users.delete` |
| `RolesView` | `roles.view` |
| `RolesManage` | `roles.manage` |
| `BusinessTypesView` | `business-types.view` |
| `BusinessTypesManage` | `business-types.manage` |
| `BusinessPartnerRolesView` | `business-partner-roles.view` |
| `BusinessPartnerRolesManage` | `business-partner-roles.manage` |
| `BusinessPartnersView` | `business-partners.view` |
| `BusinessPartnersManage` | `business-partners.manage` |
| `IndustryTypesView` | `industry-types.view` |
| `IndustryTypesManage` | `industry-types.manage` |
| `CompanyGroupsView` | `company-groups.view` |
| `CompanyGroupsManage` | `company-groups.manage` |
| `LocationsView` | `locations.view` |
| `LocationsCreate` | `locations.create` |
| `LocationsEdit` | `locations.edit` |
| `LocationsDelete` | `locations.delete` |
| `LanguagesView` | `languages.view` |
| `LanguagesManage` | `languages.manage` |
| `TimeZonesView` | `timezones.view` |
| `TimeZonesManage` | `timezones.manage` |
| `GstRegistrationTypesView` | `gst-registration-types.view` |
| `GstRegistrationTypesManage` | `gst-registration-types.manage` |
| `AddressTypesView` | `address-types.view` |
| `AddressTypesManage` | `address-types.manage` |
| `ContactTypesView` | `contact-types.view` |
| `ContactTypesManage` | `contact-types.manage` |
| `DocumentTypesView` | `document-types.view` |
| `DocumentTypesManage` | `document-types.manage` |
| `OrganizationTypesView` | `organization-types.view` |
| `OrganizationTypesManage` | `organization-types.manage` |
| `BranchTypesView` | `branch-types.view` |
| `BranchTypesManage` | `branch-types.manage` |
| `WarehouseTypesView` | `warehouse-types.view` |
| `WarehouseTypesManage` | `warehouse-types.manage` |
| `EmploymentTypesView` | `employment-types.view` |
| `EmploymentTypesManage` | `employment-types.manage` |
| `StoreTypesView` | `store-types.view` |
| `StoreTypesManage` | `store-types.manage` |
| `OperatorTypesView` | `operator-types.view` |
| `OperatorTypesManage` | `operator-types.manage` |
| `SourcesView` | `sources.view` |
| `SourcesManage` | `sources.manage` |
| `BranchesView` | `branches.view` |
| `BranchesCreate` | `branches.create` |
| `BranchesEdit` | `branches.edit` |
| `BranchesDelete` | `branches.delete` |
| `DepartmentsView` | `departments.view` |
| `DepartmentsCreate` | `departments.create` |
| `DepartmentsEdit` | `departments.edit` |
| `DepartmentsDelete` | `departments.delete` |
| `DesignationsView` | `designations.view` |
| `DesignationsCreate` | `designations.create` |
| `DesignationsEdit` | `designations.edit` |
| `DesignationsDelete` | `designations.delete` |
| `EmployeesView` | `employees.view` |
| `EmployeesCreate` | `employees.create` |
| `EmployeesEdit` | `employees.edit` |
| `EmployeesDelete` | `employees.delete` |
| `WarehousesView` | `warehouses.view` |
| `WarehousesCreate` | `warehouses.create` |
| `WarehousesEdit` | `warehouses.edit` |
| `WarehousesDelete` | `warehouses.delete` |
| `StoresView` | `stores.view` |
| `StoresCreate` | `stores.create` |
| `StoresEdit` | `stores.edit` |
| `StoresDelete` | `stores.delete` |
| `FinancialYearsView` | `financial-years.view` |
| `FinancialYearsCreate` | `financial-years.create` |
| `FinancialYearsEdit` | `financial-years.edit` |
| `FinancialYearsDelete` | `financial-years.delete` |
| `CountersView` | `counters.view` |
| `CountersCreate` | `counters.create` |
| `CountersEdit` | `counters.edit` |
| `CountersDelete` | `counters.delete` |
| `POSSessionView` | `pos-sessions.view` |
| `POSSessionCreate` | `pos-sessions.create` |
| `POSSessionEdit` | `pos-sessions.edit` |
| `POSSessionDelete` | `pos-sessions.delete` |
| `OperatorsView` | `operators.view` |
| `OperatorsCreate` | `operators.create` |
| `OperatorsEdit` | `operators.edit` |
| `OperatorsDelete` | `operators.delete` |
| `CounterAssignmentsView` | `counter-assignments.view` |
| `CounterAssignmentsCreate` | `counter-assignments.create` |
| `CounterAssignmentsEdit` | `counter-assignments.edit` |
| `CounterAssignmentsDelete` | `counter-assignments.delete` |
| `ProductCategoriesView` | `product-categories.view` |
| `ProductCategoriesCreate` | `product-categories.create` |
| `ProductCategoriesEdit` | `product-categories.edit` |
| `ProductCategoriesDelete` | `product-categories.delete` |
| `ProductSubCategoriesView` | `product-subcategories.view` |
| `ProductSubCategoriesCreate` | `product-subcategories.create` |
| `ProductSubCategoriesEdit` | `product-subcategories.edit` |
| `ProductSubCategoriesDelete` | `product-subcategories.delete` |
| `BrandsView` | `brands.view` |
| `BrandsCreate` | `brands.create` |
| `BrandsEdit` | `brands.edit` |
| `BrandsDelete` | `brands.delete` |
| `UnitsView` | `units.view` |
| `UnitsCreate` | `units.create` |
| `UnitsEdit` | `units.edit` |
| `UnitsDelete` | `units.delete` |
| `ProductsView` | `products.view` |
| `ProductsCreate` | `products.create` |
| `ProductsEdit` | `products.edit` |
| `ProductsDelete` | `products.delete` |
| `TaxTypeSystemsView` | `tax-type-systems.view` |
| `TaxTypeSystemsCreate` | `tax-type-systems.create` |
| `TaxTypeSystemsEdit` | `tax-type-systems.edit` |
| `TaxTypeSystemsDelete` | `tax-type-systems.delete` |
| `TaxesView` | `taxes.view` |
| `TaxesCreate` | `taxes.create` |
| `TaxesEdit` | `taxes.edit` |
| `TaxesDelete` | `taxes.delete` |
| `PriceTypesView` | `price-types.view` |
| `PriceTypesCreate` | `price-types.create` |
| `PriceTypesEdit` | `price-types.edit` |
| `PriceTypesDelete` | `price-types.delete` |
| `UnitConversionsView` | `unit-conversions.view` |
| `UnitConversionsCreate` | `unit-conversions.create` |
| `UnitConversionsEdit` | `unit-conversions.edit` |
| `UnitConversionsDelete` | `unit-conversions.delete` |
| `BarcodesView` | `barcodes.view` |
| `BarcodesCreate` | `barcodes.create` |
| `BarcodesEdit` | `barcodes.edit` |
| `BarcodesDelete` | `barcodes.delete` |
| `HsnSacsView` | `hsn-sacs.view` |
| `HsnSacsCreate` | `hsn-sacs.create` |
| `HsnSacsEdit` | `hsn-sacs.edit` |
| `HsnSacsDelete` | `hsn-sacs.delete` |
| `ServiceCategoriesView` | `service-categories.view` |
| `ServiceCategoriesCreate` | `service-categories.create` |
| `ServiceCategoriesEdit` | `service-categories.edit` |
| `ServiceCategoriesDelete` | `service-categories.delete` |
| `ServicesView` | `services.view` |
| `ServicesCreate` | `services.create` |
| `ServicesEdit` | `services.edit` |
| `ServicesDelete` | `services.delete` |
| `PriceListsView` | `price-lists.view` |
| `PriceListsCreate` | `price-lists.create` |
| `PriceListsEdit` | `price-lists.edit` |
| `PriceListsDelete` | `price-lists.delete` |
| `PriceListDetailsView` | `price-list-details.view` |
| `PriceListDetailsCreate` | `price-list-details.create` |
| `PriceListDetailsEdit` | `price-list-details.edit` |
| `PriceListDetailsDelete` | `price-list-details.delete` |
| `DiscountRulesView` | `discount-rules.view` |
| `DiscountRulesCreate` | `discount-rules.create` |
| `DiscountRulesEdit` | `discount-rules.edit` |
| `DiscountRulesDelete` | `discount-rules.delete` |
| `OffersView` | `offers.view` |
| `OffersCreate` | `offers.create` |
| `OffersEdit` | `offers.edit` |
| `OffersDelete` | `offers.delete` |
| `OfferDetailsView` | `offer-details.view` |
| `OfferDetailsCreate` | `offer-details.create` |
| `OfferDetailsEdit` | `offer-details.edit` |
| `OfferDetailsDelete` | `offer-details.delete` |
| `CouponsView` | `coupons.view` |
| `CouponsCreate` | `coupons.create` |
| `CouponsEdit` | `coupons.edit` |
| `CouponsDelete` | `coupons.delete` |
| `MasterImportView` | `master-import.view` |
| `MasterImportManage` | `master-import.manage` |
| `ImportLogsView` | `import-logs.view` |
| `TenantConfigView` | `tenant-config.view` |
| `TenantConfigManage` | `tenant-config.manage` |
| `PurchasesView` | `purchases.view` |
| `PurchasesCreate` | `purchases.create` |
| `PurchasesEdit` | `purchases.edit` |
| `PurchasesCancel` | `purchases.cancel` |
| `PurchasesDelete` | `purchases.delete` |
| `PurchasesManage` | `purchases.manage` |
| `PurchasesReturnView` | `purchases.return.view` |
| `PurchasesReturnManage` | `purchases.return.manage` |
| `PurchaseReturnCreate` | `purchases-return.create` |
| `PurchaseReturnView` | `purchases-return.view` |
| `PurchaseReturnEdit` | `purchases-return.edit` |
| `PurchaseReturnCancel` | `purchases-return.cancel` |
| `PurchaseReturnDelete` | `purchases-return.delete` |
| `SalesView` | `sales.view` |
| `SalesManage` | `sales.manage` |
| `SalesReturnView` | `sales.return.view` |
| `SalesReturnManage` | `sales.return.manage` |
| `SalesPOSView` | `sales.pos.view` |
| `StockView` | `stock.view` |
| `StockManage` | `stock.manage` |
| `PaymentTypesView` | `payment-types.view` |
| `PaymentTypesManage` | `payment-types.manage` |
| `PaymentMethodsView` | `payment-methods.view` |
| `PaymentMethodsManage` | `payment-methods.manage` |
| `PaymentMethodDetailsView` | `payment-method-details.view` |
| `PaymentMethodDetailsManage` | `payment-method-details.manage` |
| `PaymentsView` | `payments.view` |
| `PaymentsManage` | `payments.manage` |
| `CurrenciesView` | `currencies.view` |
| `CurrenciesManage` | `currencies.manage` |
| `SettingsView` | `settings.view` |
| `SettingsEdit` | `settings.edit` |
| `EntitiesView` | `entities.view` |
| `EntitiesManage` | `entities.manage` |
| `InvoiceTemplatesView` | `invoice-templates.view` |
| `InvoiceTemplatesCreate` | `invoice-templates.create` |
| `InvoiceTemplatesEdit` | `invoice-templates.edit` |
| `InvoiceTemplatesDelete` | `invoice-templates.delete` |
| `InvoiceTemplatesDuplicate` | `invoice-templates.duplicate` |
| `InvoiceTemplatesPreview` | `invoice-templates.preview` |
| `InvoiceTemplatesPublish` | `invoice-templates.publish` |
| `InvoiceTemplatesAssign` | `invoice-templates.assign` |
| `InvoiceTemplatesPrint` | `invoice-templates.print` |
| `InvoiceTemplatesExport` | `invoice-templates.export` |
| `InvoiceTypesView` | `invoice-types.view` |
| `InvoiceTypesCreate` | `invoice-types.create` |
| `InvoiceTypesEdit` | `invoice-types.edit` |
| `InvoiceTypesDelete` | `invoice-types.delete` |
| `InvoicePaperSizesView` | `invoice-paper-sizes.view` |
| `InvoicePaperSizesCreate` | `invoice-paper-sizes.create` |
| `InvoicePaperSizesEdit` | `invoice-paper-sizes.edit` |
| `InvoicePaperSizesDelete` | `invoice-paper-sizes.delete` |
| `InvoiceTemplateCategoriesView` | `invoice-template-categories.view` |
| `InvoiceTemplateCategoriesCreate` | `invoice-template-categories.create` |
| `InvoiceTemplateCategoriesEdit` | `invoice-template-categories.edit` |
| `InvoiceTemplateCategoriesDelete` | `invoice-template-categories.delete` |
| `InvoiceTemplateComponentsView` | `invoice-template-components.view` |
| `InvoiceTemplateComponentsCreate` | `invoice-template-components.create` |
| `InvoiceTemplateComponentsEdit` | `invoice-template-components.edit` |
| `InvoiceTemplateComponentsDelete` | `invoice-template-components.delete` |
| `InvoiceTemplateVariablesView` | `invoice-template-variables.view` |
| `InvoiceTemplateVariablesCreate` | `invoice-template-variables.create` |
| `InvoiceTemplateVariablesEdit` | `invoice-template-variables.edit` |
| `InvoiceTemplateVariablesDelete` | `invoice-template-variables.delete` |
| `InvoiceFontsView` | `invoice-fonts.view` |
| `InvoiceFontsCreate` | `invoice-fonts.create` |
| `InvoiceFontsEdit` | `invoice-fonts.edit` |
| `InvoiceFontsDelete` | `invoice-fonts.delete` |
| `PrintOrientationsView` | `print-orientations.view` |
| `PrintOrientationsCreate` | `print-orientations.create` |
| `PrintOrientationsEdit` | `print-orientations.edit` |
| `PrintOrientationsDelete` | `print-orientations.delete` |
| `PrintUnitsView` | `print-units.view` |
| `PrintUnitsCreate` | `print-units.create` |
| `PrintUnitsEdit` | `print-units.edit` |
| `PrintUnitsDelete` | `print-units.delete` |
| `PrinterTypesView` | `printer-types.view` |
| `PrinterTypesCreate` | `printer-types.create` |
| `PrinterTypesEdit` | `printer-types.edit` |
| `PrinterTypesDelete` | `printer-types.delete` |
| `PrinterModelsView` | `printer-models.view` |
| `PrinterModelsCreate` | `printer-models.create` |
| `PrinterModelsEdit` | `printer-models.edit` |
| `PrinterModelsDelete` | `printer-models.delete` |
| `AuditView` | `audit.view` |
| `ProfileEdit` | `profile.edit` |
| `PermissionModulesView` | `permission-modules.view` |
| `PermissionModulesManage` | `permission-modules.manage` |
| `PermissionActionsView` | `permission-actions.view` |
| `PermissionActionsManage` | `permission-actions.manage` |

### UI permission literals

| File | Permission literal |
|---|---|
| `ONEERP.ERP.UI/src/app/core/services/permission.service.ts` | `branches.view` |
| `ONEERP.ERP.UI/src/app/core/services/permission.service.ts` | `companies.view` |
| `ONEERP.ERP.UI/src/app/pages/barcode/barcode.ts` | `barcodes.view` |
| `ONEERP.ERP.UI/src/app/pages/brand/brand.ts` | `brands.view` |
| `ONEERP.ERP.UI/src/app/pages/category/category.ts` | `product-categories.view` |
| `ONEERP.ERP.UI/src/app/pages/company/company.ts` | `companies.view` |
| `ONEERP.ERP.UI/src/app/pages/coupon/coupon.ts` | `coupons.view` |
| `ONEERP.ERP.UI/src/app/pages/dashboard/dashboard.ts` | `companies.view` |
| `ONEERP.ERP.UI/src/app/pages/dashboard/dashboard.ts` | `payments.view` |
| `ONEERP.ERP.UI/src/app/pages/dashboard/dashboard.ts` | `roles.view` |
| `ONEERP.ERP.UI/src/app/pages/dashboard/dashboard.ts` | `sales.view` |
| `ONEERP.ERP.UI/src/app/pages/dashboard/dashboard.ts` | `stock.view` |
| `ONEERP.ERP.UI/src/app/pages/dashboard/dashboard.ts` | `users.view` |
| `ONEERP.ERP.UI/src/app/pages/discount-rule/discount-rule.ts` | `discount-rules.view` |
| `ONEERP.ERP.UI/src/app/pages/document-design/document-design.ts` | `invoice-templates.create` |
| `ONEERP.ERP.UI/src/app/pages/document-design/document-design.ts` | `invoice-templates.edit` |
| `ONEERP.ERP.UI/src/app/pages/document-design/document-designer.ts` | `invoice-templates.edit` |
| `ONEERP.ERP.UI/src/app/pages/document-design/master/document-master-page/document-master-page.component.ts` | `invoice-templates.create` |
| `ONEERP.ERP.UI/src/app/pages/document-design/master/document-master-page/document-master-page.component.ts` | `invoice-templates.edit` |
| `ONEERP.ERP.UI/src/app/pages/document-design/master/template-components/template-components-master.component.ts` | `document-master.components.create` |
| `ONEERP.ERP.UI/src/app/pages/document-design/master/template-components/template-components-master.component.ts` | `document-master.components.delete` |
| `ONEERP.ERP.UI/src/app/pages/document-design/master/template-components/template-components-master.component.ts` | `document-master.components.edit` |
| `ONEERP.ERP.UI/src/app/pages/document-design/master/template-components/template-components-master.component.ts` | `document-master.components.manage` |
| `ONEERP.ERP.UI/src/app/pages/hsn-sac/hsn-sac.ts` | `hsn-sacs.view` |
| `ONEERP.ERP.UI/src/app/pages/inventory/inventory.ts` | `stock.manage` |
| `ONEERP.ERP.UI/src/app/pages/inventory/inventory.ts` | `stock.view` |
| `ONEERP.ERP.UI/src/app/pages/invoice-template/invoice-template.ts` | `invoice-templates.view` |
| `ONEERP.ERP.UI/src/app/pages/master-import/master-import.ts` | `master-import.manage` |
| `ONEERP.ERP.UI/src/app/pages/master-import/master-import.ts` | `master-import.view` |
| `ONEERP.ERP.UI/src/app/pages/offer/offer.ts` | `offers.edit` |
| `ONEERP.ERP.UI/src/app/pages/offer/offer.ts` | `offers.view` |
| `ONEERP.ERP.UI/src/app/pages/payment-entry/payment-entry.ts` | `payments.manage` |
| `ONEERP.ERP.UI/src/app/pages/payment-entry/payment-entry.ts` | `payments.view` |
| `ONEERP.ERP.UI/src/app/pages/payment-list/payment-list.ts` | `payments.manage` |
| `ONEERP.ERP.UI/src/app/pages/payment-method-detail/payment-method-detail.ts` | `payment-method-details.manage` |
| `ONEERP.ERP.UI/src/app/pages/payment-method-detail/payment-method-detail.ts` | `payment-method-details.view` |
| `ONEERP.ERP.UI/src/app/pages/payment-method/payment-method.ts` | `payment-method-details.view` |
| `ONEERP.ERP.UI/src/app/pages/payment-method/payment-method.ts` | `payment-methods.manage` |
| `ONEERP.ERP.UI/src/app/pages/payment-method/payment-method.ts` | `payment-methods.view` |
| `ONEERP.ERP.UI/src/app/pages/payment-type/payment-type.ts` | `payment-types.manage` |
| `ONEERP.ERP.UI/src/app/pages/payment-type/payment-type.ts` | `payment-types.view` |
| `ONEERP.ERP.UI/src/app/pages/pos/pos.ts` | `sales.manage` |
| `ONEERP.ERP.UI/src/app/pages/pos/pos.ts` | `sales.pos.view` |
| `ONEERP.ERP.UI/src/app/pages/pos/pos.ts` | `sales.view` |
| `ONEERP.ERP.UI/src/app/pages/price-list/price-list.ts` | `price-lists.edit` |
| `ONEERP.ERP.UI/src/app/pages/price-list/price-list.ts` | `price-lists.view` |
| `ONEERP.ERP.UI/src/app/pages/price-master/price-master.ts` | `price-master.edit` |
| `ONEERP.ERP.UI/src/app/pages/price-master/price-master.ts` | `price-master.view` |
| `ONEERP.ERP.UI/src/app/pages/price-type/price-type.ts` | `price-types.view` |
| `ONEERP.ERP.UI/src/app/pages/product/product.ts` | `products.view` |
| `ONEERP.ERP.UI/src/app/pages/purchase-cancel/purchase-cancel.ts` | `purchases.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-delete/purchase-delete.ts` | `purchases.delete` |
| `ONEERP.ERP.UI/src/app/pages/purchase-delete/purchase-delete.ts` | `purchases.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-edit/purchase-edit.ts` | `purchases.edit` |
| `ONEERP.ERP.UI/src/app/pages/purchase-edit/purchase-edit.ts` | `purchases.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-entry/purchase-entry.ts` | `purchases.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-entry/purchase-entry.ts` | `purchases.view` |
| `ONEERP.ERP.UI/src/app/pages/purchase-list/purchase-list.ts` | `purchases-return.create` |
| `ONEERP.ERP.UI/src/app/pages/purchase-list/purchase-list.ts` | `purchases.create` |
| `ONEERP.ERP.UI/src/app/pages/purchase-list/purchase-list.ts` | `purchases.delete` |
| `ONEERP.ERP.UI/src/app/pages/purchase-list/purchase-list.ts` | `purchases.edit` |
| `ONEERP.ERP.UI/src/app/pages/purchase-list/purchase-list.ts` | `purchases.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-list/purchase-list.ts` | `purchases.return.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-list/purchase-list.ts` | `purchases.return.view` |
| `ONEERP.ERP.UI/src/app/pages/purchase-list/purchase-list.ts` | `purchases.view` |
| `ONEERP.ERP.UI/src/app/pages/purchase-management/purchase-management.ts` | `purchases.delete` |
| `ONEERP.ERP.UI/src/app/pages/purchase-management/purchase-management.ts` | `purchases.edit` |
| `ONEERP.ERP.UI/src/app/pages/purchase-management/purchase-management.ts` | `purchases.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-management/purchase-management.ts` | `purchases.view` |
| `ONEERP.ERP.UI/src/app/pages/purchase-register/purchase-register-toolbar.ts` | `purchases.create` |
| `ONEERP.ERP.UI/src/app/pages/purchase-register/purchase-register-toolbar.ts` | `purchases.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-register/purchase-register.ts` | `purchases-return.create` |
| `ONEERP.ERP.UI/src/app/pages/purchase-register/purchase-register.ts` | `purchases.create` |
| `ONEERP.ERP.UI/src/app/pages/purchase-register/purchase-register.ts` | `purchases.delete` |
| `ONEERP.ERP.UI/src/app/pages/purchase-register/purchase-register.ts` | `purchases.edit` |
| `ONEERP.ERP.UI/src/app/pages/purchase-register/purchase-register.ts` | `purchases.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-register/purchase-register.ts` | `purchases.return.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-register/purchase-register.ts` | `purchases.return.view` |
| `ONEERP.ERP.UI/src/app/pages/purchase-register/purchase-register.ts` | `purchases.view` |
| `ONEERP.ERP.UI/src/app/pages/purchase-return-entry/purchase-return-entry.ts` | `purchases-return.create` |
| `ONEERP.ERP.UI/src/app/pages/purchase-return-entry/purchase-return-entry.ts` | `purchases.return.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-return-management/purchase-return-management.ts` | `purchases-return.delete` |
| `ONEERP.ERP.UI/src/app/pages/purchase-return-management/purchase-return-management.ts` | `purchases-return.edit` |
| `ONEERP.ERP.UI/src/app/pages/purchase-return-management/purchase-return-management.ts` | `purchases-return.view` |
| `ONEERP.ERP.UI/src/app/pages/purchase-return-management/purchase-return-management.ts` | `purchases.return.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-return-management/purchase-return-management.ts` | `purchases.return.view` |
| `ONEERP.ERP.UI/src/app/pages/purchase-return/purchase-return.ts` | `purchases-return.create` |
| `ONEERP.ERP.UI/src/app/pages/purchase-return/purchase-return.ts` | `purchases-return.delete` |
| `ONEERP.ERP.UI/src/app/pages/purchase-return/purchase-return.ts` | `purchases-return.edit` |
| `ONEERP.ERP.UI/src/app/pages/purchase-return/purchase-return.ts` | `purchases-return.view` |
| `ONEERP.ERP.UI/src/app/pages/purchase-return/purchase-return.ts` | `purchases.return.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-return/purchase-return.ts` | `purchases.return.view` |
| `ONEERP.ERP.UI/src/app/pages/purchase-view/purchase-view.ts` | `purchases-return.create` |
| `ONEERP.ERP.UI/src/app/pages/purchase-view/purchase-view.ts` | `purchases.delete` |
| `ONEERP.ERP.UI/src/app/pages/purchase-view/purchase-view.ts` | `purchases.edit` |
| `ONEERP.ERP.UI/src/app/pages/purchase-view/purchase-view.ts` | `purchases.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-view/purchase-view.ts` | `purchases.return.manage` |
| `ONEERP.ERP.UI/src/app/pages/purchase-view/purchase-view.ts` | `purchases.return.view` |
| `ONEERP.ERP.UI/src/app/pages/purchase/purchase.ts` | `purchases.view` |
| `ONEERP.ERP.UI/src/app/pages/reports/gst-purchase-report/gst-purchase-report.ts` | `purchases.view` |
| `ONEERP.ERP.UI/src/app/pages/reports/purchase-outstanding/purchase-outstanding.ts` | `purchases.view` |
| `ONEERP.ERP.UI/src/app/pages/reports/purchase-register/purchase-register.ts` | `purchases-return.create` |
| `ONEERP.ERP.UI/src/app/pages/reports/purchase-register/purchase-register.ts` | `purchases.delete` |
| `ONEERP.ERP.UI/src/app/pages/reports/purchase-register/purchase-register.ts` | `purchases.edit` |
| `ONEERP.ERP.UI/src/app/pages/reports/purchase-register/purchase-register.ts` | `purchases.manage` |
| `ONEERP.ERP.UI/src/app/pages/reports/purchase-register/purchase-register.ts` | `purchases.return.manage` |
| `ONEERP.ERP.UI/src/app/pages/reports/purchase-register/purchase-register.ts` | `purchases.return.view` |
| `ONEERP.ERP.UI/src/app/pages/reports/purchase-register/purchase-register.ts` | `purchases.view` |
| `ONEERP.ERP.UI/src/app/pages/reports/purchase-reports/purchase-reports-config.ts` | `purchases.return.view` |
| `ONEERP.ERP.UI/src/app/pages/reports/purchase-reports/purchase-reports-config.ts` | `purchases.view` |
| `ONEERP.ERP.UI/src/app/pages/reports/sales-reports/sales-reports-config.ts` | `sales.view` |
| `ONEERP.ERP.UI/src/app/pages/reports/stock-ledger/stock-ledger.ts` | `stock.view` |
| `ONEERP.ERP.UI/src/app/pages/reports/supplier-ledger/supplier-ledger.ts` | `purchases.view` |
| `ONEERP.ERP.UI/src/app/pages/sales-entry/sales-entry.ts` | `sales.manage` |
| `ONEERP.ERP.UI/src/app/pages/sales-entry/sales-entry.ts` | `sales.view` |
| `ONEERP.ERP.UI/src/app/pages/sales-list/sales-list.ts` | `sales.manage` |
| `ONEERP.ERP.UI/src/app/pages/sales-return/sales-return.ts` | `sales.return.manage` |
| `ONEERP.ERP.UI/src/app/pages/sales-return/sales-return.ts` | `sales.return.view` |
| `ONEERP.ERP.UI/src/app/pages/sales-return/sales-return.ts` | `sales.view` |
| `ONEERP.ERP.UI/src/app/pages/service-category/service-category.ts` | `service-categories.view` |
| `ONEERP.ERP.UI/src/app/pages/service/service.ts` | `services.view` |
| `ONEERP.ERP.UI/src/app/pages/shared/master-page/master-page.ts` | `branches.create` |
| `ONEERP.ERP.UI/src/app/pages/stock/stock.ts` | `stock.view` |
| `ONEERP.ERP.UI/src/app/pages/subcategory/subcategory.ts` | `product-subcategories.view` |
| `ONEERP.ERP.UI/src/app/pages/tax-type-system/tax-type-system.ts` | `tax-type-systems.view` |
| `ONEERP.ERP.UI/src/app/pages/tax/tax.ts` | `taxes.view` |
| `ONEERP.ERP.UI/src/app/pages/tenant-configuration/tenant-configuration.ts` | `tenant-config.manage` |
| `ONEERP.ERP.UI/src/app/pages/tenant-configuration/tenant-configuration.ts` | `tenant-config.view` |
| `ONEERP.ERP.UI/src/app/pages/unit-conversion/unit-conversion.ts` | `unit-conversions.view` |
| `ONEERP.ERP.UI/src/app/pages/unit/unit.ts` | `units.view` |
| `ONEERP.Platform.UI/src/app/core/models.ts` | `migrations.view` |

## 19. Tenant / Company / Branch / Warehouse / Store Scope

| Level | Source evidence | Status |
|---|---|---|
| Tenant | `TenantContextMiddleware`, `TenantConnectionResolver`, `TenantAccessor`; platform `Tenants`/`TenantConnections`. | Mechanism present; endpoint-by-endpoint isolation UNKNOWN. |
| Company | Current-user company claim/context; DataScopeResolver company methods; SQL repository filters. | Per-endpoint enforcement UNKNOWN. |
| Branch | DataScopeResolver branch lookup; branch identifiers in transaction/schema sources. | Per-endpoint enforcement UNKNOWN. |
| Warehouse | DataScopeResolver warehouse lookup; warehouse identifiers in stock/purchase sources. | Per-endpoint enforcement UNKNOWN. |
| Store | Stores/POS schemas and code exist; resolver has no Store method. | Global store-scope enforcement UNKNOWN. |
| User/Role | Users/Roles/UserRoles, claims, overrides, role permission tables. | Runtime identity enforcement UNKNOWN. |

## 20. API Coverage Analysis

| Check | Static finding | Runtime |
|---|---|---|
| HTTP action declarations | 600 parsed across 114 controller classes. | UNKNOWN; no live endpoint tests in this audit. |
| Literal Angular calls | 118 calls parsed from injectable classes. | UNKNOWN. |
| APIs without literal UI match | Not classified dead; dynamic or external clients can call them. | UNKNOWN. |
| Duplicate API routes/contracts | Require semantic and authorization review; text matches alone are not enough. | UNKNOWN. |
| Request/response compatibility | DTO declarations are present; full runtime contract checks not performed. | UNKNOWN. |

## 21. DB Coverage Analysis

| Check | Static finding |
|---|---|
| SQL files | 5 under sql/. |
| CREATE TABLE statements | 156; 148 consolidated table names. |
| Preferred-definition columns | 1973. |
| Views / procedures / functions / triggers | 0 / 0 / 0 / 0. |
| Live table/index/migration presence | Not queried. | UNKNOWN. |

## 22. UI Coverage Analysis

| Check | Static finding |
|---|---|
| Tenant UI routes | 111. |
| Platform UI routes | 10. |
| Angular components | 143 `@Component` declarations. |
| Injectable Angular classes | 38. |
| Angular specs | 8. |
| Components with no route match | May be embedded children/aliases/dynamic; not enough evidence to call dead. |
| Full Angular bundle build | Previous build hit network failure while inlining external Google Fonts; TS no-emit check passed. |

## 23. Duplicate / Dead / Unused Analysis

No code was deleted by this audit. Candidates only:

| Candidate | Evidence | Assessment |
|---|---|---|
| Exact duplicate route `ONEERP.ERP.UI|` | 2 declarations in the same app route tree. | Review matching/precedence. |
| Exact duplicate route `ONEERP.Platform.UI|` | 2 declarations in the same app route tree. | Review matching/precedence. |
| Component alias `ONEERP.ERP.UI|MasterImportPage` | `/master-import`, `/import-logs`. | Shared generic/alias component; not inherently duplicate. |
| Component alias `ONEERP.ERP.UI|DocumentMasterPageComponent` | `/invoice-types`, `/document-design/master/types`, `/invoice-paper-sizes`, `/document-design/master/paper-sizes`, `/invoice-template-categories`, `/document-design/master/categories`, `/invoice-template-variables`, `/document-design/master/variables`, `/invoice-fonts`, `/document-design/master/fonts`, `/print-orientations`, `/document-design/master/orientations`, `/document-design/master/units`, `/printer-types`, `/document-design/master/printer-types`, `/printer-models`, `/document-design/master/printer-models`. | Shared generic/alias component; not inherently duplicate. |
| Full schema/migration duplicate DDL | Same table families may be declared in `erp_full.sql` and migrations. | Expected install/migration overlap; inspect drift. |
| Dead components/services/endpoints | Static no-reference signals do not account for templates, injection, dynamic routes, external consumers or reflection. | UNKNOWN; no deletions. |

## 24. Broken / Partial Features and Known Limits

| Feature | Source evidence | Status |
|---|---|---|
| T073 Purchase business testing | Task prompt requires full purchase/payment/tax/stock/print reconciliation; conversation status says QA not completed. | Runtime QA pending. |
| Stage 5 Inventory | T074–T088 prompt range; inventory routes/repos and migrations exist. Tenant migration application and end-to-end test state were not queried. | Partial / UNKNOWN. |
| T089 Payment Workspace | UI/API/schema exist; current working-tree changes from previous requests compiled, but local DEV opened to login and payment flow was not exercised. | Partial; runtime QA pending. |
| T090 Payment Types / Methods | UI/API/schema exist; current working-tree changes add view permission gates, field validation, soft deactivation and v4 unique-code migration. Migration is un-applied; live QA absent. | Implementation present; migration/runtime QA pending. |
| Inventory document schema parity | Parsed `erp_migration_003_inventory_documents.sql` item-table column counts exceed matching `erp_full.sql` definitions for adjustment/transfer/count items. | Drift candidate requiring exact column comparison before migration. |
| Actual deployed DB state | No live tenant/platform schema or migration history inspected. | UNKNOWN. |
| Runtime/business regression | No authenticated feature flows run in this audit. | UNKNOWN. |

## 25. Pending Development List

| Priority | Module | Feature | Missing layer | Task | Dependency |
|---|---|---|---|---|---|
| P0 | Purchase | Purchase lifecycle | Live DEV business-flow/persistence/scope QA | Finish T073 acceptance cases and record evidence. | Prior T059–T072; Stage 4 → Stage 5. |
| P0 | Inventory | Stock workflows | Confirm/apply migration chain; reconcile schema drift; run QA | Complete Stage 5 T074–T088 in order. | Stage 4 completion. |
| P0 | Payments | Payment workspace | Authenticated runtime flow | Complete T089 happy path, validation, authorization, isolation, persistence, allocation/retry and regression checks. | Stage 5 per master order; T089 before T090. |
| P1 | Payments | Payment type/method/detail masters | Register/apply migration v4; runtime CRUD/permission QA | Complete T090 migration and test. | T089 gate. |
| P1 | Accounting | Receipts through balance sheet | Audit actual source gaps, then implement/test only in task order | T091–T103. | T090. |
| P1 | Platform migrations | Migration catalog/runtime | Restart updated Platform API; verify catalog; apply tenant migration through console | Verify ERP migrations v2–v4 on DEV. | Authenticated Platform console; review duplicate-code guard. |

## 26. Development Dependency Order

The master roadmap declares the following order:

| Stage | Range | Target |
|---|---|---|
| Stage 4 | T059–T073 | Purchase |
| Stage 5 | T074–T088 | Inventory / Stock |
| Stage 6 | T089–T103 | Payments & Accounting |
| Stage 7 | T104–T116 | GST / Tax & Compliance |
| Stage 8 | T117–T129 | Reports & Dashboard |
| Stage 9 | T130–T144 | CRM / Service / HRMS |
| Stage 10 | T145–T160 | Manufacturing / Industry / SaaS |

Global order: Sales → POS → Purchase → Inventory → Payments/Accounting → GST/Tax → Reports/Dashboard → CRM/Service/HRMS → Manufacturing/Industry/SaaS. Task titles extracted from stage prompt files:

- `T059` — Purchase Dashboard / Workspace (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T060` — Purchase Context & Supplier (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T061` — Purchase Entry (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T062` — Purchase Pricing & Tax (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T063` — Purchase Payment (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T064` — Purchase Posting (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T065` — Purchase Register (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T066` — Purchase Detail / Edit (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T067` — Purchase Cancellation (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T068` — Purchase Return (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T069` — Purchase Return Entry (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T070` — Debit Note (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T071` — Purchase Print (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T072` — Purchase Security / Idempotency (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T073` — Purchase Business-flow Testing (`ONE_DEV/STAGE_4_PURCHASE/STAGE_4_PURCHASE_TASK_PROMPTS.md`)
- `T074` — Inventory Dashboard (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T075` — Warehouse / Location (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T076` — Opening Stock (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T077` — Stock Ledger (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T078` — Stock Transaction (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T079` — Stock Adjustment (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T080` — Stock Transfer (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T081` — Stock Transfer Entry (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T082` — Stock Count (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T083` — Stock Reconciliation (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T084` — Stock Valuation (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T085` — Low Stock / Reorder (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T086` — Batch & Expiry (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T087` — Serial / Traceability (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T088` — Inventory Security / Performance / QA (`ONE_DEV/STAGE_5_INVENTORY/STAGE_5_INVENTORY_TASK_PROMPTS.md`)
- `T089` — Payment Workspace (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T090` — Payment Types / Methods (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T091` — Customer Receipt (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T092` — Supplier Payment (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T093` — Payment Allocation (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T094` — Refund Accounting (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T095` — Chart of Accounts (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T096` — Journal Entry (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T097` — Cash / Bank Book (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T098` — Day Book (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T099` — Party Ledger (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T100` — Trial Balance (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T101` — Profit & Loss (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T102` — Balance Sheet (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T103` — Accounting Security / Reconciliation QA (`ONE_DEV/STAGE_6_ACCOUNTING_PAYMENTS/STAGE_6_ACCOUNTING_PAYMENTS_TASK_PROMPTS.md`)
- `T104` — Tax Master Audit (`ONE_DEV/STAGE_7_GST_TAX/STAGE_7_GST_TAX_TASK_PROMPTS.md`)
- `T105` — GST Configuration (`ONE_DEV/STAGE_7_GST_TAX/STAGE_7_GST_TAX_TASK_PROMPTS.md`)
- `T106` — HSN / SAC (`ONE_DEV/STAGE_7_GST_TAX/STAGE_7_GST_TAX_TASK_PROMPTS.md`)
- `T107` — GST Calculation Engine (`ONE_DEV/STAGE_7_GST_TAX/STAGE_7_GST_TAX_TASK_PROMPTS.md`)
- `T108` — Sales GST (`ONE_DEV/STAGE_7_GST_TAX/STAGE_7_GST_TAX_TASK_PROMPTS.md`)
- `T109` — Purchase GST (`ONE_DEV/STAGE_7_GST_TAX/STAGE_7_GST_TAX_TASK_PROMPTS.md`)
- `T110` — Tax Summary (`ONE_DEV/STAGE_7_GST_TAX/STAGE_7_GST_TAX_TASK_PROMPTS.md`)
- `T111` — GST Register (`ONE_DEV/STAGE_7_GST_TAX/STAGE_7_GST_TAX_TASK_PROMPTS.md`)
- `T112` — Tax Adjustment (`ONE_DEV/STAGE_7_GST_TAX/STAGE_7_GST_TAX_TASK_PROMPTS.md`)
- `T113` — Credit / Debit Note Tax (`ONE_DEV/STAGE_7_GST_TAX/STAGE_7_GST_TAX_TASK_PROMPTS.md`)
- `T114` — GST Validation (`ONE_DEV/STAGE_7_GST_TAX/STAGE_7_GST_TAX_TASK_PROMPTS.md`)
- `T115` — Tax Reports Export (`ONE_DEV/STAGE_7_GST_TAX/STAGE_7_GST_TAX_TASK_PROMPTS.md`)
- `T116` — GST Security / Reconciliation / QA (`ONE_DEV/STAGE_7_GST_TAX/STAGE_7_GST_TAX_TASK_PROMPTS.md`)
- `T117` — Report Framework Audit (`ONE_DEV/STAGE_8_REPORTS_DASHBOARD/STAGE_8_REPORTS_DASHBOARD_TASK_PROMPTS.md`)
- `T118` — Sales Reports (`ONE_DEV/STAGE_8_REPORTS_DASHBOARD/STAGE_8_REPORTS_DASHBOARD_TASK_PROMPTS.md`)
- `T119` — Purchase Reports (`ONE_DEV/STAGE_8_REPORTS_DASHBOARD/STAGE_8_REPORTS_DASHBOARD_TASK_PROMPTS.md`)
- `T120` — Inventory Reports (`ONE_DEV/STAGE_8_REPORTS_DASHBOARD/STAGE_8_REPORTS_DASHBOARD_TASK_PROMPTS.md`)
- `T121` — Payment Reports (`ONE_DEV/STAGE_8_REPORTS_DASHBOARD/STAGE_8_REPORTS_DASHBOARD_TASK_PROMPTS.md`)
- `T122` — Tax Reports (`ONE_DEV/STAGE_8_REPORTS_DASHBOARD/STAGE_8_REPORTS_DASHBOARD_TASK_PROMPTS.md`)
- `T123` — Customer / Supplier Reports (`ONE_DEV/STAGE_8_REPORTS_DASHBOARD/STAGE_8_REPORTS_DASHBOARD_TASK_PROMPTS.md`)
- `T124` — Profitability Reports (`ONE_DEV/STAGE_8_REPORTS_DASHBOARD/STAGE_8_REPORTS_DASHBOARD_TASK_PROMPTS.md`)
- `T125` — Dashboard KPIs (`ONE_DEV/STAGE_8_REPORTS_DASHBOARD/STAGE_8_REPORTS_DASHBOARD_TASK_PROMPTS.md`)
- `T126` — Dashboard Drill-down (`ONE_DEV/STAGE_8_REPORTS_DASHBOARD/STAGE_8_REPORTS_DASHBOARD_TASK_PROMPTS.md`)
- `T127` — Export / Print (`ONE_DEV/STAGE_8_REPORTS_DASHBOARD/STAGE_8_REPORTS_DASHBOARD_TASK_PROMPTS.md`)
- `T128` — Report Security / Performance (`ONE_DEV/STAGE_8_REPORTS_DASHBOARD/STAGE_8_REPORTS_DASHBOARD_TASK_PROMPTS.md`)
- `T129` — Reports Business QA (`ONE_DEV/STAGE_8_REPORTS_DASHBOARD/STAGE_8_REPORTS_DASHBOARD_TASK_PROMPTS.md`)
- `T130` — CRM Foundation (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T131` — Lead Management (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T132` — Opportunity Management (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T133` — CRM Activities (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T134` — Service Management (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T135` — Service Billing (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T136` — Employee Master (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T137` — Attendance (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T138` — Leave Management (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T139` — Payroll Foundation (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T140` — Employee Expenses (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T141` — HR Permissions (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T142` — CRM/HR Reports (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T143` — CRM/HR Notifications (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T144` — CRM/Service/HR Business QA (`ONE_DEV/STAGE_9_CRM_HRMS_SERVICE/STAGE_9_CRM_HRMS_SERVICE_TASK_PROMPTS.md`)
- `T145` — Manufacturing Foundation (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T146` — BOM (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T147` — Work Order (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T148` — Material Issue (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T149` — Production Receipt (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T150` — Production Costing (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T151` — Quality / Rejection (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T152` — Industry Extension Framework (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T153` — Multi-Company / Group (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T154` — SaaS Tenant Administration (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T155` — Subscription / Plans (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T156` — Tenant Usage / Billing (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T157` — Feature Flags / Entitlements (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T158` — Tenant Provisioning / Migration (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T159` — Enterprise Security / Audit (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)
- `T160` — ERP Release / UAT (`ONE_DEV/STAGE_10_MANUFACTURING_INDUSTRY_SAAS/STAGE_10_MANUFACTURING_INDUSTRY_SAAS_TASK_PROMPTS.md`)

## 27. Recommended Next Task

### NEXT TASK #1

**Task:** T073 — Purchase Business-flow Testing.

**Why:** The master dependency order places Purchase before Inventory and Stage 6 payments/accounting. The T073 prompt identifies reconciliation of retail/wholesale and credit purchases, returns, cancellation, payment, tax, stock and print. Prior conversation status reports its runtime testing remains pending.

**Current state:** Purchase UI/API/service/repository/schema source exists; runtime evidence is UNKNOWN / reported pending.

**Required layers:** DB/migrations, API, business logic, UI, permissions, validation, payment/tax/stock/print integration, tenant/company/branch/warehouse/store scope, security and regression tests.

**Dependencies:** Confirm T059–T072 completion and Stage 4→5 order.

**Acceptance criteria:**
1. Execute retail/wholesale and credit purchase flows using authenticated disposable DEV tenant.
2. Verify validation rejection, permission denial and tenant/company/branch/warehouse/store isolation.
3. Reconcile totals, tax, stock IN, payments/allocations, returns, cancellation and print output against response and persisted DB rows.
4. Verify retry/duplicate behavior and persistence after refresh.
5. Run integration/regression cases and record exact routes, tables/columns, permissions, outcomes and remaining gaps.
6. Do not write directly to production DB during functional tests.

### ONEERP – NEXT TASK IMPLEMENTATION PROMPT

```text
Implement T073 — Purchase Business-flow Testing using the actual ONEERP source. First inspect Purchase UI routes/components/services; purchase and return API controllers/DTOs/services/repositories; SQL schema and migration history; payment, tax, stock, print, permission and data-scope paths. Reuse existing architecture and do not create duplicate APIs, tables, screens, or transaction engines. Run the supported purchase scenarios through the real authenticated DEV API path, covering retail/wholesale, credit, return, cancellation, payments/allocations, tax, stock IN, printing, validation rejection, authorization, tenant/company/branch/warehouse/store isolation, persistence, retries and regression. Record exact files/routes/tables/columns/permissions and evidence. Update ONEERP_MASTER_APPLICATION_INVENTORY.md only after test results are established. Do not mark complete until all acceptance criteria pass.
```

## 28. Final Application Health Summary

| Area | Status | Evidence boundary |
|---|---|---|
| Source projects and structure | PRESENT | Two Angular apps, two ASP.NET APIs, shared .NET project, SQL and test project. |
| Routes/components/services/controllers/repositories | PRESENT in source | Counts are parser-derived, not runtime coverage. |
| Tenant/platform database schemas | PRESENT as SQL source | Live database may differ; not queried. |
| Authentication/permissions/scopes | PARTIAL / UNKNOWN | Mechanisms found; effective claims and row-level isolation not exercised. |
| Business flows | PARTIAL / UNKNOWN | Modules and repository paths exist; runtime side effects not verified. |
| Migration application | UNKNOWN | Source contains migration scripts/catalog code; live history not read. |
| Builds | PARTIAL | Earlier API and temporary-output Platform API builds and ERP UI TS check passed; Angular build had external font network failure. |
| Production readiness | UNKNOWN | Deployment, live DB, backups, load, monitoring and runtime regression were not tested. |

## 29. Final Inventory Counts

Static current-source counts:

| Inventory | Count | Qualification |
|---|---:|---|
| Route-derived tenant groups | 96 | First-segment count, not authoritative business modules. |
| Tenant UI route declarations | 111 | Includes redirects/root entries. |
| Platform UI route declarations | 10 | Includes redirects/wildcard. |
| Unique route component classes | 87 | Static lazy imports. |
| Angular components | 143 | Both apps, `@Component` declarations. |
| Injectable Angular service classes | 38 | Parsed from service source. |
| API endpoint attributes | 600 | Parsed action attributes; route parser limitations apply. |
| API controller classes | 114 | Both APIs. |
| Backend service classes | 124 | Classes under Services directories. |
| Repository classes | 135 | Classes under Repositories directories. |
| SQL table declarations | 156 | Includes full schema and migrations. |
| Unique table names | 148 | Consolidated. |
| Column declarations | 1973 | Preferred DDL definition per unique table. |
| SQL views | 0 | Declaration scan. |
| SQL stored procedures | 0 | Declaration scan. |
| SQL functions | 0 | Declaration scan. |
| SQL triggers | 0 | Declaration scan. |
| Angular spec files | 8 | Files found; execution status not established here. |
| Complete features | 0 verified by this static audit | Presence alone is not completion. |
| Broken features | 0 runtime failures proven in this static pass | No runtime feature tests; this does not imply none exist. |
| UI-only/API-only/DB-only totals | UNKNOWN | Requires exhaustive validated crosswalk and live DB. |
| Duplicate candidates | See §23 | Aliases/installation migrations are not assumed defects. |
| Pending P0 groups | 3 | T073 QA, Stage 5 gates, T089 QA. |
| Pending P1 groups | 3 | T090 migration/QA, T091–T103 audit, migration deployment verification. |

## Source Files Used

- Tenant UI: `ONEERP.ERP.UI/src/app/**`, `angular.json`, `package.json`, environments and proxy config.
- Platform UI: `ONEERP.Platform.UI/src/app/**`, `angular.json`, `package.json`, environments and proxy config.
- Tenant API: `ONEERP.ERP.API/Program.cs`, Controllers, DTOs, Data, Middleware, Models, Repositories, Security and Services.
- Platform API: `ONEERP.Platform.API/Program.cs`, Controllers, Data, Models, Repositories, Security and Services.
- Shared: `ONEERP.Shared/Constants/Permissions.cs` and shared source files.
- SQL: `sql/erp_full.sql`, `sql/platform_schema.sql`, `sql/erp_migration_*.sql`.
- Roadmap: `ONE_DEV/00_MASTER_STAGE_4_TO_10_DEPENDENCY_ORDER.md` and Stage 4–10 task prompt documents.

## Update Rule

After every task, update this document with actual changed screens/routes/APIs/tables/columns/flows/permissions, migration state, and verified build/test outcomes. Preserve UNKNOWN for anything not directly observed.