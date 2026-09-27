# Document Type & Template — Related Tables and Data Seed List

**Source file:** `sql/erp_full.sql` (6,597 lines, single batch, executed by Dapper during tenant provisioning)
**Module block:** lines **5568–6597** (`Invoice Template Design`, merged from the original invoice template migration)
**Legacy system master:** lines **676–694** (DDL) and **2251–2266** (seed)

---

## 0. Two Different "Document Type" Concepts

`erp_full.sql` contains **two unrelated tables** that both mean "document type". They share no FK and are never joined.

| | A. `dbo.DocumentTypes` | B. `dbo.InvoiceType` |
|---|---|---|
| Purpose | Legacy **system master** — classifies *uploaded certificate documents* (GST cert, PAN, trade licence…) | **Designer's document type** — classifies *printable business documents* (Sales Invoice, POS Receipt…) |
| DDL line | 680 | 5589 |
| Key | `DocumentTypeId INT` | `InvoiceTypeId BIGINT` |
| Has `Code` column | **No** — `Name` only (unique) | Yes — `Code VARCHAR(30)` unique |
| Soft delete | Yes (`IsDeleted`) | No (hard delete via `IsActive`) |
| Audit columns | `CreatedBy/Date`, `ModifiedBy/Date` (`DATETIME2`) | `CreatedBy/At`, `ModifiedBy/At` (`DATETIME` + local `GETDATE()`) |
| Referenced by any FK | **No — zero inbound references** | Yes: `InvoiceTemplate`, `InvoiceTemplateAssignment` |
| Seed rows | 8 | 12 |
| Used by the designer / print path | **No** | **Yes** — the document-type dimension of every template |
| C# repository | `DocumentTypeRepository.cs` | `InvoiceTemplateRepositories.cs`, `DocumentMasterRepositories.cs`, `DocumentQueryRepository.cs` |

> **Do not treat them as one.** Section 1 below documents A. Sections 2–5 document B and the rest of the template module.

---

## 1. Legacy `DocumentTypes` System Master

### 1.1 Table — line 680

| Column | Type | Notes |
|---|---|---|
| `DocumentTypeId` | `INT IDENTITY(1,1)` | PK `PK_DocumentTypes` |
| `[Name]` | `NVARCHAR(100)` | NOT NULL, `UQ_DocumentTypes_Name` |
| `[Description]` | `NVARCHAR(250)` | NULL |
| `IsActive` | `BIT` | `DF_DocumentTypes_IsActive` DEFAULT 1 |
| `CreatedBy` | `NVARCHAR(100)` | NULL |
| `CreatedDate` | `DATETIME2` | `SYSUTCDATETIME()` |
| `ModifiedBy` | `NVARCHAR(100)` | NULL |
| `ModifiedDate` | `DATETIME2` | `SYSUTCDATETIME()` |
| `IsDeleted` | `BIT` | DEFAULT 0 |

**Index:** `IX_DocumentTypes_IsActive (IsActive)`
**FKs:** none
**Guard:** `IF NOT EXISTS (… OBJECT_ID(N'[dbo].[DocumentTypes]') AND type = N'U')`

### 1.2 Seed data — lines 2254–2265

Guard: `IF NOT EXISTS (SELECT 1 FROM dbo.DocumentTypes)` — all-or-nothing, **not** row-by-row. Adding one new document type later requires a separate migration.

| # | `[Name]` | `[Description]` |
|---|---|---|
| 1 | GST Certificate | GST Registration |
| 2 | PAN Card | Permanent Account Number |
| 3 | TAN Certificate | Tax Deduction Number |
| 4 | CIN Certificate | Company Registration |
| 5 | Trade License | Trade License |
| 6 | MSME Certificate | MSME Registration |
| 7 | Logo | Company Logo |
| 8 | Digital Signature | Digital Signature |

### 1.3 Permission codes

`document-types.view`, `document-types.manage` — granted to `SuperAdmin` and `Administrator` at line 4882 (legacy flat `RolePermissionsLegacy` seed).

**Actual file attachments are stored independently.** `DocumentTypes` only names the classification; the bytes live in `dbo.[Files]` (line 5347, MinIO/S3 object metadata) linked via `dbo.EntityFile` (line 5374) to `dbo.Entity` (line 2676) using `FileType` (`LOGO` / `DOCUMENT` / `IMAGE` / `ATTACHMENT`). **There is no FK from `EntityFile` to `DocumentTypes`** — the classification is a UI convention only.

---

## 2. Invoice Template Design Module — 20 Tables in 5 Layers

All tables in this block follow one convention set (stated in the block header at line 5577):

- Re-runnable: `IF NOT EXISTS (… sys.objects … OBJECT_ID(N'[dbo].[X]') AND type = N'U')` on every object
- PK is always `BIGINT IDENTITY(1,1)`
- `CompanyId INT` / `IndustryTypeId INT` to match `dbo.Companies.Id (INT)` and `dbo.IndustryTypes.IndustryTypeId (INT)` so FKs can be declared
- Audit columns are `CreatedBy BIGINT NULL`, `CreatedAt DATETIME DEFAULT GETDATE()`, `ModifiedBy BIGINT NULL`, `ModifiedAt DATETIME NULL` (local server time, **not** UTC)
- Lookup columns are `VARCHAR`, never `NVARCHAR`

### Layer 0 — Lookup / dropdown masters (10 tables)

| # | Table | DDL line | PK | Purpose | FKs | Seeded |
|---|---|---|---|---|---|---|
| 01 | `InvoiceType` | 5589 | `InvoiceTypeId` | **WHAT** document is printed | — | **12 rows** |
| 02 | `InvoicePaperSize` | 5630 | `PaperSizeId` | Physical page / roll size | — | **8 rows** + 4 safety UPDATEs |
| 03 | `PrinterType` | 5674 | `PrinterTypeId` | Printing technology | — | **5 rows** |
| 04 | `PrinterModel` | 5706 | `PrinterModelId` | Optional physical printer models | → `PrinterType` | **none** |
| 05 | `InvoiceTemplateCategory` | 5730 | `TemplateCategoryId` | Classification of the *template itself* | — | **7 rows** |
| 06 | `InvoiceTemplateComponent` | 5765 | `ComponentId` | Drag/drop building-block palette | — | **28 rows** |
| 07 | `InvoiceTemplateVariable` | 5823 | `VariableId` | Data-binding path dictionary | — | **27 rows** + **17 extended** |
| 08 | `InvoiceFont` | 5909 | `FontId` | Seeded font choices | — | **5 rows** |
| 09 | `PrintOrientation` | 5942 | `OrientationId` | Portrait / landscape | — | **2 rows** |
| 10 | `PrintUnit` | 5964 | `UnitId` | MM / PX / inch | — | **3 rows** |

All 10 have a `UQ_*_Code` unique constraint on `Code VARCHAR(30)` (50/100/20 on some) and an `IsActive BIT DEFAULT 1` column. `PrintOrientation` and `PrintUnit` are the only two with **no audit columns at all** (just `Code`, `Name`, `IsActive`) — which is why the C# repositories force hard DELETE for them.

### Layer 1 — Template + selection (2 tables)

| # | Table | DDL line | PK | Purpose |
|---|---|---|---|---|
| 11 | `InvoiceTemplate` | 5987 | `InvoiceTemplateId` | **The template header** |
| 12 | `InvoiceTemplateAssignment` | 6024 | `AssignmentId` | **Which template is used at runtime** |

`InvoiceTemplate` (5987): `CompanyId INT NULL` (NULL = global), `TemplateCategoryId BIGINT NULL`, `InvoiceTypeId BIGINT NOT NULL`, `PaperSizeId BIGINT NOT NULL`, `OrientationId BIGINT NOT NULL`, `Code VARCHAR(50)`, `Name VARCHAR(150)`, `Description VARCHAR(500)`, `Width/Height DECIMAL(10,2) NULL`, `IsDefault BIT DEFAULT 0`, `IsActive BIT DEFAULT 1`.
Constraints: FKs to `InvoiceType`, `InvoicePaperSize`, `PrintOrientation`, `InvoiceTemplateCategory`; `UQ_InvoiceTemplate_Company_Code UNIQUE (CompanyId, Code)`.
Indexes: `CompanyId`, `InvoiceTypeId`, `PaperSizeId`, `IsActive`.
*Note:* has **no `IndustryTypeId` column** — that dimension lives only on `InvoiceTemplateAssignment`.

`InvoiceTemplateAssignment` (6024): `InvoiceTemplateId BIGINT NOT NULL`, `CompanyId INT NULL`, `IndustryTypeId INT NULL`, `InvoiceTypeId BIGINT NULL`, `PaperSizeId BIGINT NULL`, `IsDefault BIT`, `IsActive BIT`.
Constraints: FKs to `InvoiceTemplate`, `Companies(Id)`, `IndustryTypes`, `InvoiceType`, `InvoicePaperSize`.
Indexes: `IX_..._TemplateId`, `IX_..._Lookup (CompanyId, IndustryTypeId, InvoiceTypeId, PaperSizeId)`.
*Note:* NULL on any scope column = "applies to any value" (wildcard).

### Layer 2 — Lifecycle (1 table)

| # | Table | DDL line | PK | Purpose |
|---|---|---|---|---|
| 13 | `InvoiceTemplateVersion` | 6054 | `TemplateVersionId` | Template lifecycle `DRAFT → PUBLISHED → ARCHIVED` |

`InvoiceTemplateVersion` (6054): `InvoiceTemplateId BIGINT NOT NULL`, `VersionNumber INT NOT NULL`, `TemplateJson NVARCHAR(MAX) NOT NULL`, `Status VARCHAR(20) DEFAULT 'DRAFT'`, `IsPublished BIT DEFAULT 0`, `CreatedBy`, `CreatedAt`, `PublishedBy`, `PublishedAt DATETIME NULL`.
Constraints: FK → `InvoiceTemplate`; `UQ_InvoiceTemplateVersion_Template_Number UNIQUE (InvoiceTemplateId, VersionNumber)`.
Indexes: `TemplateVersionId`, `IsPublished`.
*Note:* this is the **only table in the block with `PublishedBy` / `PublishedAt` instead of `ModifiedBy` / `ModifiedAt`.**

### Layer 3 — Designer canvas (5 tables)

These form the chain **Version → Section → Element → {Field | ItemColumn | Style}**.

| # | Table | DDL line | PK | Parent FK | Purpose |
|---|---|---|---|---|---|
| 14 | `InvoiceTemplateSection` | 6079 | `SectionId` | `TemplateVersionId` | Region/band of the canvas (Header, Items, Totals…) |
| 15 | `InvoiceTemplateElement` | 6102 | `ElementId` | `SectionId`, `ComponentId` | One dropped component instance |
| 16 | `InvoiceTemplateField` | 6128 | `TemplateFieldId` | `ElementId`, `VariableId` | Data binding: element → variable |
| 17 | `InvoiceTemplateItemColumn` | 6150 | `ItemColumnId` | `ElementId` | Item-table column definitions |
| 18 | `InvoiceTemplateStyle` | 6171 | `StyleId` | `ElementId`, `FontId` | Visual style for one element |

Key columns:
- **Section:** `SectionCode VARCHAR(50)`, `SectionName VARCHAR(100)`, `DisplayOrder INT`, `X/Y/Width/Height DECIMAL(10,2)`, `IsVisible BIT`. Index on `TemplateVersionId`. *(No audit columns.)*
- **Element:** `ElementType VARCHAR(50)` (seed uses `static`, `variable`, `item-column`), `ElementName VARCHAR(100) NULL`, `X/Y/Width/Height`, `DisplayOrder`, `IsVisible`. Indexes on `SectionId`, `ComponentId`.
- **Field:** `VariableId BIGINT NOT NULL`, `FieldName VARCHAR(100)`, `BindingPath VARCHAR(300)`, `Label VARCHAR(100) NULL`, `IsVisible`. Indexes on `ElementId`, `VariableId`. *(No audit columns.)*
- **ItemColumn:** `FieldName VARCHAR(100)`, `HeaderText VARCHAR(100)`, `DisplayOrder INT NOT NULL`, `Width DECIMAL(10,2)`, `Alignment VARCHAR(20) DEFAULT 'LEFT'`, `IsVisible`. Index on `ElementId`. *(No audit columns.)*
- **Style:** `FontId BIGINT NULL`, `FontSize DECIMAL(6,2)`, `FontWeight/FontStyle/TextAlign/VerticalAlign VARCHAR(20)`, `TextColor/BackgroundColor/BorderColor VARCHAR(20)`, `PaddingTop/Right/Bottom/Left DECIMAL(8,2)`, `BorderTop/Right/Bottom/Left BIT DEFAULT 0`. Constraints: FKs to `InvoiceTemplateElement`, `InvoiceFont`; **`UQ_InvoiceTemplateStyle_Element UNIQUE (ElementId)`** — exactly one style row per element. Index on `FontId`.

### Layer 4 — Print delivery (2 tables)

| # | Table | DDL line | PK | Purpose |
|---|---|---|---|---|
| 19 | `InvoiceTemplatePrinter` | 6544 | `TemplatePrinterId` | Printer binding for a template |
| 20 | `InvoiceTemplatePrintSetting` | 6573 | `PrintSettingId` | Per-printer print defaults |

`InvoiceTemplatePrinter` (6544): `InvoiceTemplateId`, `PaperSizeId`, `PrinterTypeId` all `BIGINT NOT NULL`; `PrinterModelId BIGINT NULL`; `PrinterName VARCHAR(200) NULL`; `IsDefault`, `IsActive`. FKs to `InvoiceTemplate`, `InvoicePaperSize`, `PrinterType`, `PrinterModel`. Indexes on `InvoiceTemplateId`, `PaperSizeId`.

`InvoiceTemplatePrintSetting` (6573): `TemplatePrinterId BIGINT NOT NULL`; `MarginTop/Right/Bottom/Left DECIMAL(10,2) DEFAULT 0`; `Scale DECIMAL(6,2) DEFAULT 100`; `Copies INT DEFAULT 1`; `AutoFit BIT DEFAULT 1`; `CutPaper BIT DEFAULT 0`; `PrintHeader BIT DEFAULT 1`; `PrintFooter BIT DEFAULT 1`. FK → `InvoiceTemplatePrinter`. Index on `TemplatePrinterId`.

---

## 3. Relationship Map

```
Companies(Id INT) ──────────┐
IndustryTypes(IndustryTypeId INT) ──┐
                                  ▼
                         InvoiceTemplateAssignment ──► InvoiceTemplatePrinter ──► InvoiceTemplatePrintSetting
                                  │                                 ▲
                                  └──── InvoiceTemplateId ─────────┘
                                            │
                                            ▼
                                   InvoiceTemplate (Code, CompanyId, IsDefault)
                                            │
              ┌─────────────┬───────────────┼──────────────┬─────────────┐
              ▼             ▼               ▼              ▼             ▼
      InvoiceType   InvoicePaperSize  PrintOrientation  InvoiceTemplate  InvoiceTemplate
                                                Category    Printer
                                            │
                                            ▼
                                 InvoiceTemplateVersion
                                 (VersionNumber, Status, IsPublished)
                                            │
                                            ▼
                                 InvoiceTemplateSection
                                 (SectionCode, X/Y/W/H, IsVisible)
                                            │
                                            ▼
                                 InvoiceTemplateElement
                                 (ElementType, X/Y/W/H, IsVisible)
                                            │
              ┌─────────────────────────────┼─────────────────────────────┐
              ▼                             ▼                             ▼
   InvoiceTemplateField        InvoiceTemplateItemColumn        InvoiceTemplateStyle
   (BindingPath, Label)        (FieldName, HeaderText,          (FontId, FontSize,
              │                  Alignment, Width)               FontWeight, Colors,
              ▼                                                     Borders, Padding)
   InvoiceTemplateVariable
   (BindingPath, DataType, Category)

Separate, unlinked:
   DocumentTypes (legacy system master — zero FKs)
   [Files] ──► EntityFile ──► Entity  (MinIO object storage, FileType discriminator)
```

---

## 4. Complete Data Seed List

### 4.1 Lookup master seeds

**`InvoiceType` — 12 rows** (line 5609, guard `WHERE Code = 'SALES'`)

| Code | Name | Description | DisplayOrder |
|---|---|---|---|
| `SALES` | Sales Invoice | Standard sales invoice | 1 |
| `POS` | POS Receipt | Cash counter / walk-in receipts (thermal) | 2 |
| `SERVICE` | Service Invoice | Service delivery / job-work invoice | 3 |
| `HYBRID` | Hybrid Invoice | Goods + services combined invoice | 4 |
| `WHOLESALE` | Wholesale Invoice | Bulk / distribution invoice | 5 |
| `EXPORT` | Export Invoice | Export / cross-border invoice | 6 |
| `PROFORMA` | Proforma Invoice | Quotation-style advance invoice | 7 |
| `CREDIT_NOTE` | Credit Note | Customer credit / returns adjustment | 8 |
| `DEBIT_NOTE` | Debit Note | Debit / additional-charge note | 9 |
| `SALES_RETURN` | Sales Return | Sales return document | 10 |
| `PURCHASE` | Purchase Invoice | Vendor / supplier purchase invoice | 11 |
| `PURCHASE_RETURN` | Purchase Return | Vendor return document | 12 |

**`InvoicePaperSize` — 8 rows** (line 5652, guard `WHERE Code = 'A4'`) + 4 safety `UPDATE`s at lines 5664–5667 that re-assert correct physics on every re-run.

| Code | Name | Width | Height | Unit | IsThermal | IsCustom |
|---|---|---|---|---|---|---|
| `A4` | A4 | 210.00 | 297.00 | MM | 0 | 0 |
| `A5` | A5 | 148.00 | 210.00 | MM | 0 | 0 |
| `A6` | A6 | 105.00 | 148.00 | MM | 0 | 0 |
| `58MM` | 58mm Thermal | 58.00 | NULL | MM | 1 | 0 |
| `80MM` | 80mm Thermal | 80.00 | NULL | MM | 1 | 0 |
| `LETTER` | Letter | 215.90 | 279.40 | MM | 0 | 0 |
| `LEGAL` | Legal | 215.90 | 355.60 | MM | 0 | 0 |
| `CUSTOM` | Custom | 0.00 | 0.00 | MM | 0 | 1 |

Safety updates: `A4`→210×297, `A5`→148×210, `58MM`→58×NULL thermal, `80MM`→80×NULL thermal.

**`PrinterType` — 5 rows** (line 5692, guard `WHERE Code = 'THERMAL'`)

| Code | Name | Description |
|---|---|---|
| `THERMAL` | Thermal | Heat-based direct thermal printing (receipt printers) |
| `LASER` | Laser | Laser / LED page printing |
| `INKJET` | Inkjet | Inkjet page printing |
| `DOT_MATRIX` | Dot Matrix | Impact / dot matrix printing |
| `PDF` | PDF | Render to PDF file (no physical printer) |

**`InvoiceTemplateCategory` — 7 rows** (line 5749, guard `WHERE Code = 'GENERAL'`)

`GENERAL` General (1) · `RETAIL` Retail (2) · `WHOLESALE` Wholesale (3) · `SERVICE` Service (4) · `POS` POS (5) · `EXPORT` Export (6) · `GST` GST (7)

**`InvoiceTemplateComponent` — 28 rows** (line 5786, guard `WHERE Code = 'LOGO'`)

| # | Code | Name | ComponentType | Order |
|---|---|---|---|---|
| 1 | `LOGO` | Logo | IMAGE | 1 |
| 2 | `COMPANY_HEADER` | Company Header | TEXT | 2 |
| 3 | `COMPANY_NAME` | Company Name | TEXT | 3 |
| 4 | `COMPANY_ADDRESS` | Company Address | TEXT | 4 |
| 5 | `COMPANY_CONTACT` | Company Contact | TEXT | 5 |
| 6 | `GSTIN` | GSTIN | TEXT | 6 |
| 7 | `INVOICE_INFO` | Invoice Info | TEXT | 7 |
| 8 | `CUSTOMER` | Customer | TEXT | 8 |
| 9 | `CUSTOMER_ADDRESS` | Customer Address | TEXT | 9 |
| 10 | `BILLING_ADDRESS` | Billing Address | TEXT | 10 |
| 11 | `SHIPPING_ADDRESS` | Shipping Address | TEXT | 11 |
| 12 | `ITEM_TABLE` | Item Table | **TABLE** | 12 |
| 13 | `DISCOUNT` | Discount | TEXT | 13 |
| 14 | `TAX` | Tax | TEXT | 14 |
| 15 | `TAX_SUMMARY` | Tax Summary | **TABLE** | 15 |
| 16 | `SUBTOTAL` | Subtotal | TEXT | 16 |
| 17 | `TOTAL` | Total | TEXT | 17 |
| 18 | `PAYMENT` | Payment | TEXT | 18 |
| 19 | `BANK_DETAILS` | Bank Details | TEXT | 19 |
| 20 | `QR_CODE` | QR Code | QRCODE | 20 |
| 21 | `BARCODE` | Barcode | BARCODE | 21 |
| 22 | `TERMS` | Terms | TEXT | 22 |
| 23 | `NOTES` | Notes | TEXT | 23 |
| 24 | `SIGNATURE` | Signature | IMAGE | 24 |
| 25 | `FOOTER` | Footer | TEXT | 25 |
| 26 | `CUSTOM_TEXT` | Custom Text | TEXT | 26 |
| 27 | `CUSTOM_IMAGE` | Custom Image | IMAGE | 27 |
| 28 | `DIVIDER` | Divider | DIVIDER | 28 |

`ComponentType` values in use: `TEXT`, `IMAGE`, `TABLE`, `QRCODE`, `BARCODE`, `DIVIDER`.

**`InvoiceTemplateVariable` — 44 rows total** (27 base + 17 extended)

*Base set, line 5846 — guard `WHERE BindingPath = 'Company.Name'` (all-or-nothing). `Code` == `BindingPath` for every base row.*

| Category | Rows | BindingPaths |
|---|---|---|
| `Company` | 5 | `Company.Name`, `Company.LegalName`, `Company.Address`, `Company.GSTIN`, `Company.PAN` |
| `Invoice` | 8 | `Invoice.Number`, `Invoice.Date`, `Invoice.DueDate`, `Invoice.Type`, `Invoice.SubTotal`, `Invoice.Discount`, `Invoice.Tax`, `Invoice.GrandTotal` |
| `Customer` | 3 | `Customer.Name`, `Customer.Address`, `Customer.GSTIN` |
| `Payment` | 2 | `Payment.Method`, `Payment.Amount` |
| `Item` | 9 | `Item.ProductName`, `Item.SKU`, `Item.HSNSAC`, `Item.Unit`, `Item.Quantity`, `Item.Rate`, `Item.Discount`, `Item.Tax`, `Item.Amount` |

`DataType` values in use: `STRING`, `DATETIME`, `DECIMAL`. All base rows have `IsCollection = 0`.

*Extended set, line 5880 — `INSERT … SELECT … FROM (VALUES …) WHERE NOT EXISTS`, i.e. **row-by-row idempotent**, so this one can be appended to safely.*

| Category | Rows | BindingPaths |
|---|---|---|
| `Invoice` | 9 | `Invoice.PaidAmount`, `Invoice.BalanceAmount`, `Invoice.RoundOff`, `Invoice.TaxableAmount`, `Invoice.CGST`, `Invoice.SGST`, `Invoice.IGST`, `Invoice.CESS`, `Invoice.Remarks` |
| `Company` | 3 | `Company.Phone`, `Company.Email`, `Company.LogoUrl` |
| `Customer` | 1 | `Customer.Phone` |
| `Branch` | 1 | `Branch.Name` |
| `Warehouse` | 1 | `Warehouse.Name` |
| `Item` | 2 | `Item.ProductCode`, `Item.SlNo` (`DataType = NUMBER`) |

**`InvoiceFont` — 5 rows** (line 5928, guard `WHERE Code = 'ARIAL'`)

| Code | Name | FontFamily |
|---|---|---|
| `ARIAL` | Arial | Arial |
| `ROBOTO` | Roboto | Roboto |
| `INTER` | Inter | Inter |
| `TAHOMA` | Tahoma | Tahoma |
| `COURIER_NEW` | Courier New | Courier New |

`FontFileId BIGINT NULL` is reserved for uploaded font files and is never populated by the seed.

**`PrintOrientation` — 2 rows** (line 5953): `PORTRAIT` Portrait, `LANDSCAPE` Landscape
**`PrintUnit` — 3 rows** (line 5975): `MM` Millimeter, `PX` Pixel, `INCH` Inch

**`PrinterModel` — NO SEED.** Structure only. Intentionally left empty.

### 4.2 Column-level idempotent upgrades (lines 6200–6209)

Applied to existing installs via `IF COL_LENGTH(...) IS NULL → ALTER TABLE ADD`:

| Table | Columns added | Guard |
|---|---|---|
| `InvoiceTemplateStyle` | `FontStyle VARCHAR(20) NULL` | `COL_LENGTH('dbo.InvoiceTemplateStyle','FontStyle')` |
| `InvoiceTemplateStyle` | `TextColor VARCHAR(20) NULL` | `…'TextColor'` |
| `InvoiceTemplateStyle` | `BackgroundColor VARCHAR(20) NULL` | `…'BackgroundColor'` |
| `InvoiceTemplateStyle` | `BorderColor VARCHAR(20) NULL` | `…'BorderColor'` |

### 4.3 `SALES-INVOICE-A4` Default Template Seed (lines 6211–6537)

Provisions a printable A4 Sales Invoice for a brand-new tenant so nothing needs clicking in the designer.

**Header (lines 6239–6243)**

| Column | Value |
|---|---|
| `Code` | `SALES-INVOICE-A4` |
| `Name` | `Sales Invoice A4` |
| `Description` | `Default A4 Sales Invoice Template` |
| `CompanyId` | `NULL` (global) |
| `TemplateCategoryId` | resolved from `InvoiceTemplateCategory.Code = 'GST'` |
| `InvoiceTypeId` | resolved from `InvoiceType.Code = 'SALES'` |
| `PaperSizeId` | resolved from `InvoicePaperSize.Code = 'A4'` |
| `OrientationId` | resolved from `PrintOrientation.Code = 'PORTRAIT'` |
| `Width` / `Height` | `210.00` / `297.00` |
| `IsDefault` / `IsActive` | `1` / `1` |

**Version (lines 6252–6253):** `VersionNumber = 1`, `TemplateJson = '{}'`, `Status = 'PUBLISHED'`, `IsPublished = 1`, `PublishedAt = GETDATE()`.
Seeded **PUBLISHED** deliberately, so runtime resolution (`SALES` + `A4`) finds something printable immediately. Later designer edits stay `DRAFT` until explicitly published.

**Canvas contents — 12 sections / 17 elements / 17 styles / 23 fields / 9 item columns**

| SectionCode | Name | Ord | X | Y | W | H | Elements |
|---|---|---|---|---|---|---|---|
| `HEADER` | Header | 1 | 10 | 10 | 190 | 30 | 4 |
| `INVOICE_INFO` | Invoice Information | 2 | 10 | 44 | 190 | 20 | 2 |
| `BILL_TO` | Bill To / Customer | 3 | 10 | 68 | 190 | 24 | 1 |
| `ITEMS` | Items | 4 | 10 | 96 | 190 | 80 | 1 |
| `TAX_SUMMARY` | Tax Summary | 5 | 10 | 180 | 190 | 24 | 1 |
| `TOTALS` | Totals | 6 | 10 | 208 | 190 | 26 | 2 |
| `PAYMENT` | Payment | 7 | 10 | 238 | 190 | 14 | 1 |
| `AMOUNT_IN_WORDS` | Amount in Words | 8 | 10 | 256 | 190 | 10 | 1 |
| `BANK_DETAILS` | Bank Details | 9 | 10 | 268 | 190 | 10 | 1 |
| `TERMS` | Terms & Conditions | 10 | 10 | 280 | 110 | 9 | 1 |
| `SIGNATURE` | Authorized Signature | 11 | 125 | 280 | 75 | 9 | 1 |
| `FOOTER` | Footer | 12 | 10 | 291 | 190 | 6 | 1 |

Elements per section, with the component code, `ElementType`, and font:

| Section | Component | Type | ElementName | X, Y, W, H | Font size / weight |
|---|---|---|---|---|---|
| HEADER | `LOGO` | static | Company Logo | 0, 0, 40, 22 | 3.2 |
| HEADER | `COMPANY_NAME` | variable | Company Name | 45, 0, 100, 10 | 5.0 bold |
| HEADER | `CUSTOM_TEXT` | variable | Company Address / Contact / GSTIN | 45, 11, 110, 18 | 3.0 |
| HEADER | `CUSTOM_TEXT` | static | TAX INVOICE | 145, 0, 45, 10 | 5.5 bold RIGHT |
| INVOICE_INFO | `CUSTOMER` | static | BILL TO | 0, 0, 60, 6 | 3.4 bold |
| INVOICE_INFO | `INVOICE_INFO` | variable | Invoice No / Date / Due Date | 100, 0, 90, 19 | 3.2 RIGHT |
| BILL_TO | `CUSTOMER` | variable | Customer Info | 0, 0, 95, 23 | 3.2 LEFT |
| ITEMS | `ITEM_TABLE` | item-column | Item Table | 0, 0, 190, 78 | 3.0, `BorderBottom=1`, `BorderColor='#999999'` |
| TAX_SUMMARY | `TAX_SUMMARY` | variable | Tax Rate Summary | 0, 0, 90, 23 | 3.0 LEFT |
| TOTALS | `SUBTOTAL` | variable | Gross / Discount / Taxable | 100, 0, 90, 15 | 3.2 LEFT |
| TOTALS | `TOTAL` | variable | Grand Total | 100, 16, 90, 9 | 4.2 bold LEFT, `BorderTop=1` |
| PAYMENT | `PAYMENT` | variable | Payment Details | 0, 0, 120, 13 | 3.0 LEFT |
| AMOUNT_IN_WORDS | `CUSTOM_TEXT` | static | Amount in Words | 0, 0, 190, 9 | 2.8 italic LEFT |
| BANK_DETAILS | `BANK_DETAILS` | static | Bank Details | 0, 0, 190, 9 | 2.8 LEFT |
| TERMS | `TERMS` | static | Terms & Conditions | 0, 0, 108, 8 | 2.5 LEFT |
| SIGNATURE | `SIGNATURE` | static | Authorized Signatory | 0, 0, 75, 8 | 3.0 RIGHT |
| FOOTER | `FOOTER` | static | Thank You For Your Business | 0, 0, 190, 5 | 2.8 CENTER |

All 17 styles use the font resolved from `InvoiceFont.Code = 'ARIAL'`.

The 23 bound fields (label in brackets):

| Element | Field / BindingPath | Label |
|---|---|---|
| Company Name | `Company.Name` | — |
| Company Address / Contact / GSTIN | `Company.Address` | — |
| | `Company.Phone` | Phone |
| | `Company.GSTIN` | GSTIN |
| Invoice No / Date / Due Date | `Invoice.Number` | Invoice No |
| | `Invoice.Date` | Invoice Date |
| | `Invoice.DueDate` | Due Date |
| Customer Info | `Customer.Name` | — |
| | `Customer.Address` | — |
| | `Customer.GSTIN` | GSTIN |
| | `Customer.Phone` | Contact |
| Tax Rate Summary | `Invoice.TaxableAmount` | Taxable Amount |
| | `Invoice.CGST` | CGST |
| | `Invoice.SGST` | SGST |
| | `Invoice.IGST` | IGST |
| | `Invoice.CESS` | CESS |
| Gross / Discount / Taxable | `Invoice.SubTotal` | Subtotal |
| | `Invoice.Discount` | Discount |
| | `Invoice.Tax` | Tax (GST) |
| Grand Total | `Invoice.GrandTotal` | GRAND TOTAL |
| Payment Details | `Payment.Method` | Payment Mode |
| | `Invoice.PaidAmount` | Amount Paid |
| | `Invoice.BalanceAmount` | Balance |

The 9 `InvoiceTemplateItemColumn` rows on the Item Table element:

| # | FieldName | HeaderText | Width | Alignment |
|---|---|---|---|---|
| 1 | `SlNo` | `#` | 10 | CENTER |
| 2 | `ProductName` | `Product / Description` | 64 | LEFT |
| 3 | `HsnCode` | `HSN/SAC` | 18 | CENTER |
| 4 | `UnitName` | `Unit` | 14 | CENTER |
| 5 | `Quantity` | `Qty` | 14 | RIGHT |
| 6 | `Rate` | `Rate` | 20 | RIGHT |
| 7 | `DiscountAmount` | `Discount` | 18 | RIGHT |
| 8 | `TaxAmount` | `GST` | 16 | RIGHT |
| 9 | `LineTotal` | `Amount` | 16 | RIGHT |

**Default assignment (lines 6534–6537):**

```sql
INSERT INTO dbo.InvoiceTemplateAssignment
    (InvoiceTemplateId, CompanyId, InvoiceTypeId, PaperSizeId, IsDefault, IsActive)
VALUES (@a4_tpl, NULL, @a4_sales, @a4_paper, 1, 1);
```
`CompanyId = NULL` = global, `IndustryTypeId` left NULL (wildcard). Guarded by `NOT EXISTS (… WHERE InvoiceTemplateId = @a4_tpl AND IsActive = 1)`.

### 4.4 Idempotency rules of the A4 seed

| Rule | Implementation |
|---|---|
| Never duplicate an existing template | `IF @a4_tpl IS NULL AND @a4_sales IS NOT NULL AND @a4_paper IS NOT NULL AND @a4_orient IS NOT NULL` |
| Never overwrite a user design | Child inserts run only `IF @a4_ver IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.InvoiceTemplateSection WHERE TemplateVersionId = @a4_ver)` |
| Never duplicate the assignment | `AND NOT EXISTS (… WHERE InvoiceTemplateId = @a4_tpl AND IsActive = 1)` |
| Missing variables degrade gracefully | Every field insert is `INSERT … SELECT @elm, VariableId, … FROM dbo.InvoiceTemplateVariable WHERE BindingPath = '…'` — returns 0 rows instead of failing the batch |
| Component ids resolved by code | `SET @a4_comp = (SELECT ComponentId FROM dbo.InvoiceTemplateComponent WHERE Code = '…')` immediately before each element insert, wrapped in `IF @a4_comp IS NOT NULL` |

### 4.5 No seed at all

`PrinterModel`, `InvoiceTemplatePrinter`, `InvoiceTemplatePrintSetting`, `InvoiceTemplateSection`, `InvoiceTemplateElement`, `InvoiceTemplateField`, `InvoiceTemplateItemColumn`, `InvoiceTemplateStyle` (all other than via the A4 template).

---

## 5. Security / Navigation Seed for the Module

### 5.1 Workspace tree

| Level | Code | Name | Route | Icon | Sort |
|---|---|---|---|---|---|
| Workspace | `INVOICE_DESIGN` | Invoice & Print Design | `/invoice-templates` | `file-text` | 12 |
| Domain | `DOM-INVOICEPRINT` | Invoice & Print | — | `file-text` | 1 |
| Module | `MOD-TEMPLATEDESIGN` | Template Design | `/invoice-templates` | `layout-template` | 1 |
| SubModule | `SUB-TEMPLATEMASTER` | Invoice Templates | `/invoice-templates` | `file-text` | 1 |
| Module | `MOD-PRINTSETUP` | Print Setup | `/invoice-templates` | `printer` | 2 |
| SubModule | `SUB-PRINTSETUP` | Print Configuration | `/invoice-templates` | `printer` | 1 |

### 5.2 Screens (11)

| SubModule | ScreenCode | ScreenName | RouteUrl | Component | Sort |
|---|---|---|---|---|---|
| `SUB-TEMPLATEMASTER` | `INVOICE_TEMPLATES` | Invoice Design | `/invoice-templates` | `InvoiceTemplatePage` | 1 |
| `SUB-TEMPLATEMASTER` | `DOCUMENT_DESIGN` | Document Design | `/document-design` | `DocumentDesignPage` | 2 |
| `SUB-PRINTSETUP` | `INVOICE_TYPES` | Invoice Types | `/invoice-types` | `InvoiceTypePage` | 1 |
| `SUB-PRINTSETUP` | `INVOICE_PAPER_SIZES` | Paper Sizes | `/invoice-paper-sizes` | `InvoicePaperSizePage` | 2 |
| `SUB-PRINTSETUP` | `INVOICE_TEMPLATE_CATEGORIES` | Template Categories | `/invoice-template-categories` | `InvoiceTemplateCategoryPage` | 3 |
| `SUB-PRINTSETUP` | `INVOICE_TEMPLATE_COMPONENTS` | Template Components | `/invoice-template-components` | `InvoiceTemplateComponentPage` | 4 |
| `SUB-PRINTSETUP` | `INVOICE_TEMPLATE_VARIABLES` | Template Variables | `/invoice-template-variables` | `InvoiceTemplateVariablePage` | 5 |
| `SUB-PRINTSETUP` | `INVOICE_FONTS` | Invoice Fonts | `/invoice-fonts` | `InvoiceFontPage` | 6 |
| `SUB-PRINTSETUP` | `PRINT_ORIENTATIONS` | Print Orientations | `/print-orientations` | `PrintOrientationPage` | 7 |
| `SUB-PRINTSETUP` | `PRINTER_TYPES` | Printer Types | `/printer-types` | `PrinterTypePage` | 8 |
| `SUB-PRINTSETUP` | `PRINTER_MODELS` | Printer Models | `/printer-models` | `PrinterModelPage` | 9 |

All `ScreenType = 'MASTER'`, `IsActive = 1`, `CreatedBy = 'system'`.

### 5.3 Action seeded for this module

`set-default` / `Set Default` / `DisplayOrder 18` — consumed by the designer toolbar (line 4439). The standard CRUD actions (`view`, `create`, `edit`, `delete`, and `duplicate`, `preview`, `publish`, `assign`, `print`, `export`) come from the global action seed.

### 5.4 Permission codes

`Screens.PermissionCode` mapping (line 5148–5158) — flat codes used by `[Permission]` attribute checks, kept in sync with `ONEERP.Shared/Constants/Permissions.cs`:

`INVOICE_TEMPLATES` → `invoice-templates` · `DOCUMENT_DESIGN` → `document-design` · `INVOICE_TYPES` → `invoice-types` · `INVOICE_PAPER_SIZES` → `invoice-paper-sizes` · `INVOICE_TEMPLATE_CATEGORIES` → `invoice-template-categories` · `INVOICE_TEMPLATE_COMPONENTS` → `invoice-template-components` · `INVOICE_TEMPLATE_VARIABLES` → `invoice-template-variables` · `INVOICE_FONTS` → `invoice-fonts` · `PRINT_ORIENTATIONS` → `print-orientations` · `PRINTER_TYPES` → `printer-types` · `PRINTER_MODELS` → `printer-models`

Designer-only codes (`invoice-templates.*`, line 4923–4925): `.view`, `.create`, `.edit`, `.delete`, `.duplicate`, `.preview`, `.publish`, `.assign`, `.print`, `.export`.
Lookup masters get standard `.view/.create/.edit/.delete` (lines 4926–4934).

**Role grants:** `SuperAdmin` (all screens × all actions) and `Administrator` (all screens except the 7 security-config screens) get full access. No business role (`SalesAdmin`, `PurchaseAdmin`, `StoreManager`, `POSCashier`, `FinanceYearOfficer`) is granted any template screen in the matrix at lines 4987–5065 — template management is admin-only out of the box.

---

## 6. Runtime Template Resolution Order

Implemented in `DocumentQueryRepository.ResolveTemplateIdAsync` + `DocumentDesignService` (mirrored in `DocumentSettingService`).

**Step 1 — `InvoiceTemplateAssignment` lookup**

```sql
SELECT TOP 1 a.InvoiceTemplateId
FROM dbo.InvoiceTemplateAssignment a
WHERE a.IsActive = 1
  AND (a.CompanyId     = @companyId     OR a.CompanyId     IS NULL OR @companyId     IS NULL)
  AND (a.InvoiceTypeId = @invoiceTypeId OR a.InvoiceTypeId IS NULL)
  AND (a.PaperSizeId   = @paperSizeId   OR a.PaperSizeId   IS NULL)
ORDER BY (CompanyId IS NOT NULL), (InvoiceTypeId IS NOT NULL), (PaperSizeId IS NOT NULL),
         a.IsDefault DESC, a.AssignmentId DESC
```
Most specific scope wins; ties broken by `IsDefault` then newest.

**Step 2 — fallback** `InvoiceTemplate WHERE IsActive = 1 AND IsDefault = 1 AND InvoiceTypeId = @ AND (@paperSizeId IS NULL OR PaperSizeId = @)`.

**Step 3 — version** `InvoiceTemplateVersion WHERE InvoiceTemplateId = @ AND IsPublished = 1 ORDER BY VersionNumber DESC`.

**Step 4 — canvas** `InvoiceTemplateSection` → `InvoiceTemplateElement` → `InvoiceTemplateField` / `InvoiceTemplateItemColumn` / `InvoiceTemplateStyle`.

**Step 5 — render** paper dimensions from `InvoicePaperSize.Width/Height`, else `InvoiceTemplate.Width/Height`, else 210×297.

---

## 7. Gotchas Worth Knowing

1. **`IndustryTypeId` is stored but never used at resolution.** It is on `InvoiceTemplateAssignment` and in the composite lookup index, but is not part of the `ORDER BY` specificity chain.
2. **`ClearTemplateDefaultAsync` is global, not per-company** — it clears `IsDefault` on all templates regardless of `CompanyId`, so setting a company default un-defaults the global `SALES-INVOICE-A4` too.
3. **Binding is by hardcoded string switch, not by `VariableId`/`BindingPath` lookup.** The renderer ignores `InvoiceTemplateField.VariableId` and `FieldName`, and matches `BindingPath` against a hardcoded list. Adding a row to `InvoiceTemplateVariable` alone will render blank — the renderer switch must be updated too. Same for `InvoiceTemplateItemColumn.FieldName` (12-name switch, with a `DefaultColumns()` fallback).
4. **`InvoiceTemplateStyle.FontId` is persisted but the renderer ignores it** — font family is hardcoded to `Arial,Helvetica,sans-serif`. The seeded `InvoiceFont` rows drive the UI picker only.
5. **`InvoiceTemplatePrintSetting` is persisted but never consumed** by any live renderer (no PDF or thermal engine is wired up; output is HTML printed from an iframe). Only the unregistered `DocumentRenderService` reads `Scale`/`Copies`/`Margins`/`CutPaper`.
6. **`PrintOrientation.OrientationId` is stored on the template but never applied** by the renderer — pages always render portrait.
7. **`IsThermal` on `InvoicePaperSize` is a list/filter flag only** — it never selects a rendering path, despite `58MM`/`80MM` being seeded as thermal.
8. **The base lookup seeds are all-or-nothing** (`IF NOT EXISTS (SELECT 1 FROM dbo.X WHERE Code = '…')`). If a tenant already has a partial/custom set, the whole set is skipped. Only the extended `InvoiceTemplateVariable` set (line 5880) and the A4 template use row-by-row idempotency.
9. **`PrintOrientation` / `PrintUnit` have no audit columns**, so the C# repositories force hard DELETE for them while other masters soft delete.
10. **`TemplateJson` is seeded as `'{}'`** and is not the render source of truth — the renderer reads the relational Section/Element/Field/ItemColumn/Style rows instead.
