# Batch A — Product Foundation

## Features
A1 Product Structure
A2 Product Variant / Model / Specification
A3 Multi-Unit / UOM Conversion
A4 Barcode Printing
A5 Serial Number / IMEI Management
A6 Batch / Expiry Management
A7 Warranty Management

## Dependency order
A1 → A2 → A3 → A4 → A5 → A6 → A7

## UI functional checklist
1 Product Master
2 Product Category
3 Product Subcategory
4 Brand
5 Unit Master
6 Product Tax / HSN-SAC mapping
7 Product Barcode
8 Product Variant
9 Product Model
10 Product Specification
11 Variant Attribute / Value
12 Variant SKU
13 Unit Conversion
14 Product Unit Mapping
15 Barcode Management
16 Barcode Generation
17 Barcode Printing / Label Printing
18 Serial Number Management
19 IMEI Management
20 Serial / IMEI History
21 Batch Management
22 Batch / Expiry Tracking
23 Expiry Status / Expiring Products
24 Warranty Master
25 Product Warranty
26 Serial / IMEI Warranty
27 Warranty History

IMPORTANT: These are functional checklist items, NOT automatically 27 new routes. Inspect existing implementation and classify each EXISTING / PARTIAL / MISSING / DUPLICATE / NOT REQUIRED.

For every A1-A7 feature first produce:
Workspace/Module → Dependency Table List → Integration Table List → API List → UI Screen List → Permission → Business Flow → Gap Analysis.
Only then implement.

Workspace candidates:
BUSINESS MASTER: product, category, subcategory, brand, unit, variant, model, specification, UOM, barcode
INVENTORY: serial/IMEI, batch, expiry
SERVICE or existing appropriate workspace: warranty
Inspect actual workspace configuration before finalizing.
