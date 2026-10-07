# ONE ERP – Common Dashboard API Inspection & Gap Analysis

**Scope:** Audit only. No code modified, no APIs created. All findings verified against actual source: controllers, services, repositories, DTOs, and Angular dashboard page.
**Verdict:** **PARTIALLY** – see §12.

---

## 1. EXISTING DASHBOARD STATUS

**Answer: C – should be rebuilt (content-wise), D – business dashboard API missing.**

- `GET /api/dashboard` (`DashboardController` → `DashboardService.GetAsync`) returns **admin/platform info** (company, user, tenant, roles) — **zero business KPIs**.
- UI (`ONEERP.ERP.UI/src/app/pages/dashboard/dashboard.ts`) calls only `/api/dashboard`.
- ⚠️ Security: `/api/dashboard` has `[Authorize]` only, **no `[Permission]` attribute** — inconsistent with every other controller.

## 2. COMPLETE RELEVANT API INVENTORY

| # | Module | Controller | Method | Route | Auth | Dashboard suitability |
|---|---|---|---|---|---|---|
| 1 | Dashboard | DashboardController | GET | `/api/dashboard` | JWT, no Permission | NOT SUITABLE (admin info) |
| 2 | Sales Reports | SalesReportsController | GET | `/api/sales-reports?Report=…` | `[Permission(SalesView)]` | PRIMARY REUSE |
| 3 | Purchase Reports | PurchaseReportsController | GET | `/api/purchase-reports?Report=…` | `[Permission(PurchasesView/PurchaseReturnView)]` | OPTIONAL (V1) |
| 4 | Inventory | InventoryController | GET | `/api/inventory/dashboard` | `[Permission(StockView)]` | REUSE |
| 5 | Inventory | InventoryController | GET | `/api/inventory/valuation` | `[Permission(StockView)]` | REUSE (paginated) |
| 6 | Inventory | InventoryController | GET | `/api/inventory/low-stock` | `[Permission(StockView)]` | REUSE |
| 7 | Payments | PaymentsController | GET | `/api/payments` (page,size,search) | `[Permission(PaymentsView)]` | PARTIAL – row-level only, no date/method aggregation |
| 8 | POS | POSOperationsController | GET | `/api/pos/dashboard` | JWT | REUSE (POS section) |
| 9 | Sales | SalesController | GET | `/api/sales` (paged, search) | JWT | REUSE (Recent Sales) |
| 10 | Master Data | MasterDataController | GET | `/api/business-partners` | JWT | NOT SUITABLE for outstanding (no balance field) |

**Sales report keys** (verified in `SalesReportRepository.cs` switch): `overview, register, detail, product, customer, daily, monthly, tax, hsn, payments, outstanding, pos, price-list, allocation`.
**Purchase report keys:** `dashboard, register, items, supplier, product-price, quantity, tax, payment, returns, reconciliation`.
**Filter:** `SalesReportFilter` (Report, DateFrom/To, BranchId, WarehouseId, CustomerId, ProductId, SalesTypeId, PriceListId, PaymentTypeID, PaymentMethodID, HsnId, GstRate, Search). CompanyId is resolved server-side from `ICurrentUser`.

## 3. DASHBOARD WIDGET → API MAPPING

| Widget | Required Data | Existing API | Status | DB Tables | Missing Data | Reuse / New |
|---|---|---|---|---|---|---|
| Sales Today | SUM GrandTotal (today) | `/api/sales-reports?Report=overview&DateFrom=<today>&DateTo=<today>` | EXISTS | SalesInvoice | — | Reuse |
| Sales Trend 7d/30d/month | daily / monthly totals | `Report=daily` / `Report=monthly` | EXISTS | SalesInvoice | — | Reuse |
| Collection Today | payment totals today | `Report=payments&DateFrom=<today>&DateTo=<today>` | PARTIAL | Payment + PaymentType/PaymentMethod | grouped-by-method total | Reuse + client group (V1) / PROPOSED 1 |
| Payment Summary (Cash/UPI/Card/Bank/Credit) | total per method | `Report=payments` rows | PARTIAL | Payment, PaymentMethod | server-side GROUP BY | Reuse + client group (V1) / PROPOSED 1 |
| Outstanding (total) | SUM BalanceAmount | `Report=outstanding` | EXISTS | SalesInvoice + PaymentAllocation agg | — | Reuse |
| Customer Outstanding | balance per customer | `Report=outstanding` per-invoice rows | PARTIAL | SalesInvoice | GROUP BY CustomerId | Reuse + client group (V1) / PROPOSED 2 |
| Stock Value | total valuation | `/api/inventory/dashboard` (`InventoryDashboardDto.StockValue` + OutOfStock/LowStock counts) | EXISTS | Stock / valuation query | — | Reuse |
| Top Selling Products | qty + amount per product | `Report=product` | EXISTS | SalesInvoiceItem | — | Reuse |
| Low Stock Alerts | current stock vs threshold | `/api/inventory/low-stock` (paginated; uses real `Products.ReorderLevel`) | EXISTS | Stock, Products | — | Reuse |
| Recent Sales | last N invoices | `/api/sales?page=1&pageSize=10` (SalesInvoiceId DESC) | EXISTS | SalesInvoice | PaymentStatus per row via report only – acceptable | Reuse |
| Quick Actions | — | UI-only router links | N/A | — | — | No API |
| Purchase KPI | purchase totals | `/api/purchase-reports?Report=dashboard` | EXISTS | PurchaseInvoice | — | OPTIONAL for V1 |

**Payment method seed values (verified in `sql/erp_full.sql`, not assumed):** PAYMETHOD-001 Cash/CASH, 002 UPI/DIGITAL, 003 Credit Card/CARD, 004 Debit Card/CARD, 005 Bank Transfer/BANK, 006 Cheque/BANK, 007 Credit Account/CREDIT, 008 Wallet/DIGITAL, 009 Other/OTHER.

## 4. EXISTING API REUSE LIST (A)

- `/api/sales-reports?Report=overview|daily|monthly|product|payments|outstanding`
- `/api/inventory/dashboard`, `/api/inventory/low-stock`, `/api/inventory/valuation`
- `/api/pos/dashboard`
- `/api/sales` (paged, for Recent Sales)
- `/api/purchase-reports?Report=dashboard` (optional)

## 5. MISSING API LIST (C) – PROPOSED ONLY, NOT IMPLEMENTED

**PROPOSED 1 – collection summary**
- Endpoint: `GET /api/sales-reports?Report=collection-summary&DateFrom&DateTo`
- Purpose: payments grouped by payment method.
- Response: `[{ methodCode, methodName, total, count }]`
- Tables: Payment, PaymentMethod. Aggregation: SQL GROUP BY PaymentMethodID, `ReferenceType='SALES'`, date range.
- Security: `[Permission(SalesView)]`, CompanyId server-side.

**PROPOSED 2 – customer outstanding grouping**
- Endpoint: `GET /api/sales-reports?Report=outstanding&GroupBy=customer`
- Purpose: outstanding grouped by customer.
- Response: `[{ customerId, customerName, balance }]`
- Tables: SalesInvoice. Aggregation: SQL GROUP BY CustomerId over unpaid invoices.

**B – existing APIs needing small response extension:** none strictly required; `InventoryDashboardDto` already covers Stock Value + low-stock counts in one call.

## 6. BROKEN / DUPLICATE API LIST

- NOT SUITABLE: `/api/dashboard` (admin info; no `[Permission]`).
- NOT SUITABLE: `/api/business-partners` for outstanding (BusinessPartnerDto has CreditLimit but no balance/outstanding field).
- PARTIAL: `/api/payments` (no date/method aggregation endpoint).
- No BROKEN or DUPLICATE endpoints among the relevant set.
- Known unrelated issue (T072 audit): some GetByIdAsync repositories filter by id only, not CompanyId — cross-tenant hole, separate fix.

## 7. DATABASE TABLE → DASHBOARD MAPPING

| Widget | Table(s) | Key columns (verified) |
|---|---|---|
| Sales Today / Trend | SalesInvoice | InvoiceDate, GrandTotal, PaidAmount, BalanceAmount |
| Top Products | SalesInvoiceItem | ProductId, Quantity, Amount |
| Collection / Payment Summary | Payment (+PaymentMethod) | ReferenceType='SALES', PaymentMethodID, Amount |
| Outstanding | SalesInvoice + PaymentAllocation (agg, ReferenceType='SALES') | BalanceAmount, PaidAmount |
| Stock Value / Low Stock | Stock, Products | valuation query; Products.ReorderLevel |

## 8. SECURITY / COMPANY / BRANCH SCOPE FINDINGS

- ✅ Sales/Purchase/Inventory/POS reports: CompanyId scoped server-side via `ICurrentUser`; Branch/Warehouse are optional parameters.
- ❌ `/api/dashboard` has no `[Permission]` — add when rebuilt.
- The dashboard must never rely on Angular-side filtering for company scope (it doesn't need to — reports already scope server-side).

## 9. PERFORMANCE FINDINGS

- ✅ Reports are SQL-side aggregates; inventory valuation/low-stock are paginated.
- ❌ Client-side grouping of `payments` and `outstanding` pulls all rows for the period — fine for V1 volumes; Proposals 1–2 move it server-side.
- Recommendation: **multiple widget calls against existing report APIs** (parallel, per-period caching) rather than one mega summary API. No thousands-of-invoices loads or N+1 patterns found in the report paths.

## 10. FINAL COMMON DASHBOARD API PLAN

- **A. Reuse:** see §4.
- **B. Small extensions:** none required.
- **C. New (PROPOSED ONLY):** PROPOSED 1, PROPOSED 2 (§5).
- **D. Not required:** `/api/dashboard` (rebuild later with `[Permission]`), `/api/business-partners` for outstanding, `/api/payments` for aggregation.

## 11. IMPLEMENTATION ORDER

- **P0:** Wire dashboard to existing APIs (overview, daily/monthly, payments, outstanding, inventory/dashboard, low-stock, sales paged). Zero new backend work.
- **P1:** PROPOSED 1 + PROPOSED 2; add `[Permission]` to dashboard endpoints.
- **P2:** Purchase KPI widget, POS section deepening.

## 12. FINAL ANSWER

**"Can the current APIs fully support the Common Dashboard?" → PARTIALLY.**

Every widget except two has an existing, company-scoped, SQL-aggregated API. **Collection-by-method totals** and **customer-grouped outstanding** are only available as row-level data (groupable client-side for V1, or via the two proposed report keys). The current `/api/dashboard` is irrelevant to business KPIs and unguarded by permissions. Strictly for V1, **zero new APIs are mandatory** — the dashboard can be built entirely on reuse, with the two proposals as P1 optimizations.
