using Dapper;

namespace ONEERP.Platform.API.Data;

/// <summary>
/// Ensures the built-in product reorder-level migration is available to the
/// tenant migration UI. This only registers migration metadata in the platform
/// database; tenant SQL is executed later by TenantMigrationService.
/// </summary>
public sealed class MigrationCatalogSeeder(IDbConnectionFactory connectionFactory)
{
    private const string RegisterReorderLevelMigrationSql = """
        IF EXISTS (
            SELECT 1 FROM dbo.Migrations
            WHERE Version = 2 AND MigrationCode <> N'ERP-002-REORDER-LEVEL'
        )
            THROW 51020, 'Tenant migration version 2 is already assigned to another migration.', 1;

        IF EXISTS (
            SELECT 1 FROM dbo.Migrations
            WHERE MigrationCode = N'ERP-002-REORDER-LEVEL'
              AND (Version <> 2 OR ScriptName <> N'erp_migration_002_products_reorder_level.sql')
        )
            THROW 51021, 'ERP-002-REORDER-LEVEL is registered with unexpected metadata.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Migrations WHERE MigrationCode = N'ERP-002-REORDER-LEVEL')
        BEGIN
            INSERT INTO dbo.Migrations
                (Version, MigrationCode, MigrationName, ScriptName, [Description], IsActive, CreatedBy)
            VALUES
                (2, N'ERP-002-REORDER-LEVEL', N'Add product reorder level',
                 N'erp_migration_002_products_reorder_level.sql',
                 N'Adds nullable Products.ReorderLevel for the tenant low-stock inventory report.',
                 1, N'system');
        END;
        """;

    private const string RegisterInventoryDocumentsMigrationSql = """
        IF EXISTS (
            SELECT 1 FROM dbo.Migrations
            WHERE Version = 3 AND MigrationCode <> N'ERP-003-INVENTORY-DOCUMENTS'
        )
            THROW 51022, 'Tenant migration version 3 is already assigned to another migration.', 1;

        IF EXISTS (
            SELECT 1 FROM dbo.Migrations
            WHERE MigrationCode = N'ERP-003-INVENTORY-DOCUMENTS'
              AND (Version <> 3 OR ScriptName <> N'erp_migration_003_inventory_documents.sql')
        )
            THROW 51023, 'ERP-003-INVENTORY-DOCUMENTS is registered with unexpected metadata.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Migrations WHERE MigrationCode = N'ERP-003-INVENTORY-DOCUMENTS')
        BEGIN
            INSERT INTO dbo.Migrations
                (Version, MigrationCode, MigrationName, ScriptName, [Description], IsActive, CreatedBy)
            VALUES
                (3, N'ERP-003-INVENTORY-DOCUMENTS', N'Create inventory adjustment, transfer, and count tables',
                 N'erp_migration_003_inventory_documents.sql',
                 N'Creates the tenant stock adjustment, warehouse transfer, and stock count document tables.',
                 1, N'system');
        END;
        """;

    private const string RegisterPaymentMasterMigrationSql = """
        IF EXISTS (
            SELECT 1 FROM dbo.Migrations
            WHERE Version = 4 AND MigrationCode <> N'ERP-004-PAYMENT-MASTER-CODES'
        )
            THROW 51024, 'Tenant migration version 4 is already assigned to another migration.', 1;

        IF EXISTS (
            SELECT 1 FROM dbo.Migrations
            WHERE MigrationCode = N'ERP-004-PAYMENT-MASTER-CODES'
              AND (Version <> 4 OR ScriptName <> N'erp_migration_004_payment_master_unique_codes.sql')
        )
            THROW 51025, 'ERP-004-PAYMENT-MASTER-CODES is registered with unexpected metadata.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Migrations WHERE MigrationCode = N'ERP-004-PAYMENT-MASTER-CODES')
        BEGIN
            INSERT INTO dbo.Migrations
                (Version, MigrationCode, MigrationName, ScriptName, [Description], IsActive, CreatedBy)
            VALUES
                (4, N'ERP-004-PAYMENT-MASTER-CODES', N'Enforce unique payment master codes',
                 N'erp_migration_004_payment_master_unique_codes.sql',
                 N'Adds unique code indexes to payment types and payment methods after checking for duplicate data.',
                 1, N'system');
        END;
        """;

    private const string RegisterBatchAProductFoundationMigrationSql = """
        IF EXISTS (SELECT 1 FROM dbo.Migrations WHERE Version = 5 AND MigrationCode <> N'ERP-005-BATCH-A-PRODUCT-FOUNDATION')
            THROW 51026, 'Tenant migration version 5 is already assigned to another migration.', 1;
        IF EXISTS (SELECT 1 FROM dbo.Migrations WHERE MigrationCode = N'ERP-005-BATCH-A-PRODUCT-FOUNDATION'
              AND (Version <> 5 OR ScriptName <> N'erp_migration_005_batch_a_product_foundation.sql'))
            THROW 51027, 'ERP-005-BATCH-A-PRODUCT-FOUNDATION is registered with unexpected metadata.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Migrations WHERE MigrationCode = N'ERP-005-BATCH-A-PRODUCT-FOUNDATION')
        BEGIN
            INSERT INTO dbo.Migrations (Version, MigrationCode, MigrationName, ScriptName, [Description], IsActive, CreatedBy)
            VALUES (5, N'ERP-005-BATCH-A-PRODUCT-FOUNDATION', N'Create Batch A product foundation tables',
                N'erp_migration_005_batch_a_product_foundation.sql',
                N'Adds variants, models, specifications, unit mappings, serials, batches, expiry, and warranties.', 1, N'system');
        END;
        """;

    private const string RegisterBatchBCustomerPricingLoyaltyMigrationSql = """
        IF EXISTS (SELECT 1 FROM dbo.Migrations WHERE Version = 6 AND MigrationCode <> N'ERP-006-BATCH-B-CUSTOMER-PRICING-LOYALTY')
            THROW 51028, 'Tenant migration version 6 is already assigned to another migration.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Migrations WHERE MigrationCode = N'ERP-006-BATCH-B-CUSTOMER-PRICING-LOYALTY')
            INSERT INTO dbo.Migrations (Version, MigrationCode, MigrationName, ScriptName, [Description], IsActive, CreatedBy)
            VALUES (6, N'ERP-006-BATCH-B-CUSTOMER-PRICING-LOYALTY', N'Create Batch B customer pricing and loyalty tables', N'erp_migration_006_batch_b_customer_pricing_loyalty.sql', N'Adds customer price agreements and loyalty accounts and transactions.', 1, N'system');
        """;

    private const string RegisterPurchaseWorkflowMigrationSql = """
        IF EXISTS (SELECT 1 FROM dbo.Migrations WHERE Version = 8 AND MigrationCode <> N'ERP-008-PURCHASE-WORKFLOW')
            THROW 51032, 'Tenant migration version 8 is already assigned to another migration.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Migrations WHERE MigrationCode = N'ERP-008-PURCHASE-WORKFLOW')
            INSERT INTO dbo.Migrations (Version, MigrationCode, MigrationName, ScriptName, [Description], IsActive, CreatedBy)
            VALUES (8, N'ERP-008-PURCHASE-WORKFLOW', N'Purchase workflow: settings, approval rules, PO, GRN, sequences', N'erp_migration_008_purchase_workflow.sql', N'Adds PurchaseSettings, PurchaseApprovalSettings, PurchaseOrder(+Item), GoodsReceipt(+Item) and DocumentSequence tables for Billing/ERP purchase modes.', 1, N'system');
        """;

    private const string RegisterCompanyRowversionConcurrencyMigrationSql = """
        IF EXISTS (SELECT 1 FROM dbo.Migrations WHERE Version = 7 AND MigrationCode <> N'ERP-007-COMPANY-ROWVERSION-CONCURRENCY')
            THROW 51030, 'Tenant migration version 7 is already assigned to another migration.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Migrations WHERE MigrationCode = N'ERP-007-COMPANY-ROWVERSION-CONCURRENCY')
            INSERT INTO dbo.Migrations (Version, MigrationCode, MigrationName, ScriptName, [Description], IsActive, CreatedBy)
            VALUES (7, N'ERP-007-COMPANY-ROWVERSION-CONCURRENCY', N'Add rowversion optimistic concurrency to Companies', N'erp_migration_007_company_rowversion_concurrency.sql', N'Adds ConcurrencyVersion rowversion column to dbo.Companies for optimistic concurrency.', 1, N'system');
        """;

    private const string RegisterAtomicIdempotencyMigrationSql = """
        IF EXISTS (SELECT 1 FROM dbo.Migrations WHERE Version = 9 AND MigrationCode <> N'ERP-009-ATOMIC-IDEMPOTENCY')
            THROW 51034, 'Tenant migration version 9 is already assigned to another migration.', 1;
        IF EXISTS (SELECT 1 FROM dbo.Migrations WHERE MigrationCode = N'ERP-009-ATOMIC-IDEMPOTENCY'
              AND (Version <> 9 OR ScriptName <> N'erp_migration_009_atomic_idempotency.sql'))
            THROW 51035, 'ERP-009-ATOMIC-IDEMPOTENCY is registered with unexpected metadata.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Migrations WHERE MigrationCode = N'ERP-009-ATOMIC-IDEMPOTENCY')
            INSERT INTO dbo.Migrations (Version, MigrationCode, MigrationName, ScriptName, [Description], IsActive, CreatedBy)
            VALUES (9, N'ERP-009-ATOMIC-IDEMPOTENCY', N'Add atomic idempotency response snapshots', N'erp_migration_009_atomic_idempotency.sql', N'Adds request hashes and response snapshots to tenant idempotency records.', 1, N'system');
        """;

    private const string RegisterStockPostingIdentityMigrationSql = """
        IF EXISTS (SELECT 1 FROM dbo.Migrations WHERE Version = 10 AND MigrationCode <> N'ERP-010-STOCK-POSTING-IDENTITY')
            THROW 51036, 'Tenant migration version 10 is already assigned to another migration.', 1;
        IF EXISTS (SELECT 1 FROM dbo.Migrations WHERE MigrationCode = N'ERP-010-STOCK-POSTING-IDENTITY'
              AND (Version <> 10 OR ScriptName <> N'erp_migration_010_stock_posting_identity.sql'))
            THROW 51037, 'ERP-010-STOCK-POSTING-IDENTITY is registered with unexpected metadata.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Migrations WHERE MigrationCode = N'ERP-010-STOCK-POSTING-IDENTITY')
            INSERT INTO dbo.Migrations (Version, MigrationCode, MigrationName, ScriptName, [Description], IsActive, CreatedBy)
            VALUES (10, N'ERP-010-STOCK-POSTING-IDENTITY', N'Add stable stock posting identities', N'erp_migration_010_stock_posting_identity.sql', N'Adds source-line posting identity and reversal uniqueness to stock transactions.', 1, N'system');
        """;

    public async Task EnsureRegisteredAsync(CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreatePlatformConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            connection.Open();

        await connection.ExecuteAsync(new CommandDefinition(
            RegisterReorderLevelMigrationSql,
            cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(
            RegisterInventoryDocumentsMigrationSql,
            cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(
            RegisterPaymentMasterMigrationSql,
            cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(
            RegisterBatchAProductFoundationMigrationSql,
            cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(
            RegisterBatchBCustomerPricingLoyaltyMigrationSql,
            cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(
            RegisterCompanyRowversionConcurrencyMigrationSql,
            cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(
            RegisterPurchaseWorkflowMigrationSql,
            cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(
            RegisterAtomicIdempotencyMigrationSql,
            cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(
            RegisterStockPostingIdentityMigrationSql,
            cancellationToken: cancellationToken));
    }
}
