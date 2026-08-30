SET NOCOUNT ON;
GO

-- Seed sample purchases for Company 1 (ERPNEXT) so the Purchase Ledger
-- inside the Purchase Workspace shows real data.

IF NOT EXISTS (SELECT 1 FROM dbo.Purchase WHERE CompanyId = 1 AND PurchaseNumber = 'PUR-1001')
BEGIN
    DECLARE @pid1 BIGINT;

    INSERT INTO dbo.Purchase
        (CompanyId, CompanyNameSnapshot, BranchId, BranchNameSnapshot,          WarehouseId,
         SupplierId, SupplierNameSnapshot, PurchaseNumber, PurchaseDate, SupplierInvoiceNumber, SupplierInvoiceDate,
         TotalGrossAmount, TotalDiscountAmount, TotalTaxableAmount, TotalTaxAmount, TotalCessAmount, TotalRoundOff,
         GrandTotal, PaidAmount, BalanceAmount, PaymentTypeID, PaymentMethodID, StatusID, Remarks,
         IsActive, CreatedByUserID, CreatedAt)
    VALUES
        (1, 'ERPNEXT', 1, 'CHENNAI',          1,
         1, 'Abiraj', 'PUR-1001', '2026-08-10', 'INV-5001', '2026-08-10',
         1500.00, 0.00, 1500.00, 270.00, 0.00, 0.00,
         1770.00, 1770.00, 0.00, 1, 1, 2, 'Seeded sample purchase (posted, fully paid)',
         1, 1, GETDATE());

    SET @pid1 = SCOPE_IDENTITY();

    INSERT INTO dbo.PurchaseItem
        (PurchaseId, ProductId, ProductCodeSnapshot, ProductNameSnapshot, BrandID, CategoryID, SubCategoryID,
         UnitID, UnitNameSnapshot, HSNID, HSNCodeSnapshot, BarcodeSnapshot,
         Quantity, FreeQuantity, PurchaseRate, MRP, RetailPrice, WholesalePrice, SaleRate,
         DiscountPercentage, DiscountAmount, IsGSTInclusive, TaxableValue,
         GSTRate, GSTAmount, CGSTRate, CGSTAmount, SGSTRate, SGSTAmount, IGSTRate, IGSTAmount,
         CESSRate, CESSAmount, LineTotal, ManufacturingDate, ExpiryDate, Remarks)
    VALUES
        (@pid1, 1, 'P-1', 'BOMB', 2, 2, 1,
         5, 'Pack', 2, NULL, NULL,
         10, 0, 150.00, 100.00, 150.00, NULL, 150.00,
         0.00, 0.00, 0, 1500.00,
         18.00, 270.00, 9.00, 135.00, 9.00, 135.00, 0.00, 0.00,
         0.00, 0.00, 1770.00, NULL, NULL, NULL);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Purchase WHERE CompanyId = 1 AND PurchaseNumber = 'PUR-1002')
BEGIN
    DECLARE @pid2 BIGINT;

    INSERT INTO dbo.Purchase
        (CompanyId, CompanyNameSnapshot, BranchId, BranchNameSnapshot,          WarehouseId,
         SupplierId, SupplierNameSnapshot, PurchaseNumber, PurchaseDate, SupplierInvoiceNumber, SupplierInvoiceDate,
         TotalGrossAmount, TotalDiscountAmount, TotalTaxableAmount, TotalTaxAmount, TotalCessAmount, TotalRoundOff,
         GrandTotal, PaidAmount, BalanceAmount, PaymentTypeID, PaymentMethodID, StatusID, Remarks,
         IsActive, CreatedByUserID, CreatedAt)
    VALUES
        (1, 'ERPNEXT', 1, 'CHENNAI',          1,
         1, 'Abiraj', 'PUR-1002', '2026-08-20', 'INV-5002', '2026-08-20',
         500.00, 25.00, 475.00, 57.00, 0.00, 0.00,
         532.00, 0.00, 532.00, 1, 6, 1, 'Seeded sample purchase (draft, unpaid)',
         1, 1, GETDATE());

    SET @pid2 = SCOPE_IDENTITY();

    INSERT INTO dbo.PurchaseItem
        (PurchaseId, ProductId, ProductCodeSnapshot, ProductNameSnapshot, BrandID, CategoryID, SubCategoryID,
         UnitID, UnitNameSnapshot, HSNID, HSNCodeSnapshot, BarcodeSnapshot,
         Quantity, FreeQuantity, PurchaseRate, MRP, RetailPrice, WholesalePrice, SaleRate,
         DiscountPercentage, DiscountAmount, IsGSTInclusive, TaxableValue,
         GSTRate, GSTAmount, CGSTRate, CGSTAmount, SGSTRate, SGSTAmount, IGSTRate, IGSTAmount,
         CESSRate, CESSAmount, LineTotal, ManufacturingDate, ExpiryDate, Remarks)
    VALUES
        (@pid2, 2, 'PRC-001', 'APPLE 23', 2, 2, 1,
         3, 'Pack', 2, NULL, NULL,
         5, 0, 100.00, NULL, NULL, NULL, NULL,
         5.00, 25.00, 0, 475.00,
         12.00, 57.00, 6.00, 28.50, 6.00, 28.50, 0.00, 0.00,
         0.00, 0.00, 532.00, NULL, NULL, NULL);
END
GO

SELECT 'SEEDED' Result, COUNT(*) AS PurchaseCount FROM dbo.Purchase WHERE CompanyId = 1;
GO
