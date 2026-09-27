using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

/* ---------------------------------------------------------------------------
   DOCUMENT MASTER repositories - the 10 lookup masters behind the
   Document Master screen. These are plain CRUD wrappers over the EXISTING
   Invoice* tables in sql/erp_full.sql (no schema changes).
   --------------------------------------------------------------------------- */

public interface IDocumentMasterRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(bool includeInactive = false);
    Task<T?> GetByIdAsync(long id);
    Task<long> InsertAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(long id);
    Task<bool> CodeInUseAsync(string code, long? excludeId = null);
}

public abstract class DocumentMasterRepositoryBase<T> : TenantRepositoryBase, IDocumentMasterRepository<T>
    where T : class
{
    protected abstract string Table { get; }
    protected abstract string IdColumn { get; }
    /// <summary>Soft delete ( IsActive=0 ) when the table has audit columns; hard delete otherwise.</summary>
    protected virtual bool SoftDelete => true;

    protected DocumentMasterRepositoryBase(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public virtual async Task<IEnumerable<T>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? $"SELECT * FROM dbo.{Table} ORDER BY 1"
            : $"SELECT * FROM dbo.{Table} WHERE IsActive = 1 ORDER BY 1";
        return await Sql.QueryAsync<T>(conn, sql);
    }

    public virtual async Task<T?> GetByIdAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<T>(conn,
            $"SELECT * FROM dbo.{Table} WHERE {IdColumn} = @id", new { id });
    }

    public async Task<bool> CodeInUseAsync(string code, long? excludeId = null)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(conn,
            $"SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.{Table} WHERE Code = @code AND (@excludeId IS NULL OR {IdColumn} <> @excludeId)) THEN 1 ELSE 0 END",
            new { code, excludeId });
    }

    public abstract Task<long> InsertAsync(T entity);

    public abstract Task<bool> UpdateAsync(T entity);

    public virtual async Task<bool> DeleteAsync(long id)
    {
        using var conn = OpenTenant();
        var sql = SoftDelete
            ? $"UPDATE dbo.{Table} SET IsActive = 0, ModifiedAt = GETDATE(), ModifiedBy = @modifiedBy WHERE {IdColumn} = @id"
            : $"DELETE FROM dbo.{Table} WHERE {IdColumn} = @id";
        return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy = (long?)null }) > 0;
    }
}

/* ------------------------- 1. InvoiceType ------------------------- */

public interface IInvoiceTypeRepository : IDocumentMasterRepository<InvoiceType>
{
}

public class InvoiceTypeRepository : DocumentMasterRepositoryBase<InvoiceType>, IInvoiceTypeRepository
{
    protected override string Table => "InvoiceType";
    protected override string IdColumn => "InvoiceTypeId";

    public InvoiceTypeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public override async Task<IEnumerable<InvoiceType>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.InvoiceType ORDER BY DisplayOrder, Name"
            : "SELECT * FROM dbo.InvoiceType WHERE IsActive = 1 ORDER BY DisplayOrder, Name";
        return await Sql.QueryAsync<InvoiceType>(conn, sql);
    }

    public override async Task<long> InsertAsync(InvoiceType e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.InvoiceType (Code, Name, Description, DisplayOrder, IsActive, CreatedBy)
            VALUES (@Code, @Name, @Description, @DisplayOrder, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, e);
    }

    public override async Task<bool> UpdateAsync(InvoiceType e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.InvoiceType
            SET Code = @Code, Name = @Name, Description = @Description, DisplayOrder = @DisplayOrder,
                IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
            WHERE InvoiceTypeId = @InvoiceTypeId;";
        return await Sql.ExecuteAsync(conn, sql, e) > 0;
    }
}

/* ------------------------- 2. InvoiceTemplateCategory ------------------------- */

public interface ITemplateCategoryRepository : IDocumentMasterRepository<InvoiceTemplateCategory>
{
}

public class TemplateCategoryRepository : DocumentMasterRepositoryBase<InvoiceTemplateCategory>, ITemplateCategoryRepository
{
    protected override string Table => "InvoiceTemplateCategory";
    protected override string IdColumn => "TemplateCategoryId";

    public TemplateCategoryRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public override async Task<IEnumerable<InvoiceTemplateCategory>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.InvoiceTemplateCategory ORDER BY DisplayOrder, Name"
            : "SELECT * FROM dbo.InvoiceTemplateCategory WHERE IsActive = 1 ORDER BY DisplayOrder, Name";
        return await Sql.QueryAsync<InvoiceTemplateCategory>(conn, sql);
    }

    public override async Task<long> InsertAsync(InvoiceTemplateCategory e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.InvoiceTemplateCategory (Code, Name, Description, DisplayOrder, IsActive, CreatedBy)
            VALUES (@Code, @Name, @Description, @DisplayOrder, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, e);
    }

    public override async Task<bool> UpdateAsync(InvoiceTemplateCategory e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.InvoiceTemplateCategory
            SET Code = @Code, Name = @Name, Description = @Description, DisplayOrder = @DisplayOrder,
                IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
            WHERE TemplateCategoryId = @TemplateCategoryId;";
        return await Sql.ExecuteAsync(conn, sql, e) > 0;
    }
}

/* ------------------------- 3. InvoiceTemplateComponent ------------------------- */

public interface ITemplateComponentRepository : IDocumentMasterRepository<InvoiceTemplateComponent>
{
}

public class TemplateComponentRepository : DocumentMasterRepositoryBase<InvoiceTemplateComponent>, ITemplateComponentRepository
{
    protected override string Table => "InvoiceTemplateComponent";
    protected override string IdColumn => "ComponentId";

    public TemplateComponentRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public override async Task<IEnumerable<InvoiceTemplateComponent>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.InvoiceTemplateComponent ORDER BY DisplayOrder, Name"
            : "SELECT * FROM dbo.InvoiceTemplateComponent WHERE IsActive = 1 ORDER BY DisplayOrder, Name";
        return await Sql.QueryAsync<InvoiceTemplateComponent>(conn, sql);
    }

    public override async Task<long> InsertAsync(InvoiceTemplateComponent e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.InvoiceTemplateComponent (Code, Name, ComponentType, Description, DisplayOrder, IsActive, CreatedBy)
            VALUES (@Code, @Name, @ComponentType, @Description, @DisplayOrder, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, e);
    }

    public override async Task<bool> UpdateAsync(InvoiceTemplateComponent e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.InvoiceTemplateComponent
            SET Code = @Code, Name = @Name, ComponentType = @ComponentType, Description = @Description,
                DisplayOrder = @DisplayOrder, IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
            WHERE ComponentId = @ComponentId;";
        return await Sql.ExecuteAsync(conn, sql, e) > 0;
    }

    public override async Task<bool> DeleteAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "UPDATE dbo.InvoiceTemplateComponent SET IsActive = 0, ModifiedAt = GETDATE(), ModifiedBy = @modifiedBy WHERE ComponentId = @id",
            new { id, modifiedBy = (long?)null }) > 0;
    }
}

/* ------------------------- 4. InvoiceTemplateVariable ------------------------- */

public interface ITemplateVariableRepository : IDocumentMasterRepository<InvoiceTemplateVariable>
{
}

public class TemplateVariableRepository : DocumentMasterRepositoryBase<InvoiceTemplateVariable>, ITemplateVariableRepository
{
    protected override string Table => "InvoiceTemplateVariable";
    protected override string IdColumn => "VariableId";

    public TemplateVariableRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public override async Task<IEnumerable<InvoiceTemplateVariable>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.InvoiceTemplateVariable ORDER BY Category, Name"
            : "SELECT * FROM dbo.InvoiceTemplateVariable WHERE IsActive = 1 ORDER BY Category, Name";
        return await Sql.QueryAsync<InvoiceTemplateVariable>(conn, sql);
    }

    public override async Task<long> InsertAsync(InvoiceTemplateVariable e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.InvoiceTemplateVariable (Code, Name, BindingPath, DataType, Category, Description, IsCollection, IsActive, CreatedBy)
            VALUES (@Code, @Name, @BindingPath, @DataType, @Category, @Description, @IsCollection, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, e);
    }

    public override async Task<bool> UpdateAsync(InvoiceTemplateVariable e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.InvoiceTemplateVariable
            SET Code = @Code, Name = @Name, BindingPath = @BindingPath, DataType = @DataType, Category = @Category,
                Description = @Description, IsCollection = @IsCollection, IsActive = @IsActive,
                ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
            WHERE VariableId = @VariableId;";
        return await Sql.ExecuteAsync(conn, sql, e) > 0;
    }

    public override async Task<bool> DeleteAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "UPDATE dbo.InvoiceTemplateVariable SET IsActive = 0, ModifiedAt = GETDATE(), ModifiedBy = @modifiedBy WHERE VariableId = @id",
            new { id, modifiedBy = (long?)null }) > 0;
    }
}

/* ------------------------- 5. InvoiceFont ------------------------- */

public interface IInvoiceFontRepository : IDocumentMasterRepository<InvoiceFont>
{
}

public class InvoiceFontRepository : DocumentMasterRepositoryBase<InvoiceFont>, IInvoiceFontRepository
{
    protected override string Table => "InvoiceFont";
    protected override string IdColumn => "FontId";

    public InvoiceFontRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public override async Task<IEnumerable<InvoiceFont>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.InvoiceFont ORDER BY Name"
            : "SELECT * FROM dbo.InvoiceFont WHERE IsActive = 1 ORDER BY Name";
        return await Sql.QueryAsync<InvoiceFont>(conn, sql);
    }

    public override async Task<long> InsertAsync(InvoiceFont e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.InvoiceFont (Code, Name, FontFamily, FontFileId, IsActive, CreatedBy)
            VALUES (@Code, @Name, @FontFamily, @FontFileId, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, e);
    }

    public override async Task<bool> UpdateAsync(InvoiceFont e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.InvoiceFont
            SET Code = @Code, Name = @Name, FontFamily = @FontFamily, FontFileId = @FontFileId, IsActive = @IsActive,
                ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
            WHERE FontId = @FontId;";
        return await Sql.ExecuteAsync(conn, sql, e) > 0;
    }

    public override async Task<bool> DeleteAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "UPDATE dbo.InvoiceFont SET IsActive = 0, ModifiedAt = GETDATE(), ModifiedBy = @modifiedBy WHERE FontId = @id",
            new { id, modifiedBy = (long?)null }) > 0;
    }
}

/* ------------------------- 6. InvoicePaperSize ------------------------- */

public interface IDocumentPaperSizeRepository : IDocumentMasterRepository<InvoicePaperSize>
{
}

public class DocumentPaperSizeRepository : DocumentMasterRepositoryBase<InvoicePaperSize>, IDocumentPaperSizeRepository
{
    protected override string Table => "InvoicePaperSize";
    protected override string IdColumn => "PaperSizeId";

    public DocumentPaperSizeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public override async Task<IEnumerable<InvoicePaperSize>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.InvoicePaperSize ORDER BY IsThermal, Width, Name"
            : "SELECT * FROM dbo.InvoicePaperSize WHERE IsActive = 1 ORDER BY IsThermal, Width, Name";
        return await Sql.QueryAsync<InvoicePaperSize>(conn, sql);
    }

    public override async Task<long> InsertAsync(InvoicePaperSize e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.InvoicePaperSize (Code, Name, Width, Height, Unit, IsThermal, IsCustom, IsActive, CreatedBy)
            VALUES (@Code, @Name, @Width, @Height, @Unit, @IsThermal, @IsCustom, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, e);
    }

    public override async Task<bool> UpdateAsync(InvoicePaperSize e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.InvoicePaperSize
            SET Code = @Code, Name = @Name, Width = @Width, Height = @Height, Unit = @Unit,
                IsThermal = @IsThermal, IsCustom = @IsCustom, IsActive = @IsActive,
                ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
            WHERE PaperSizeId = @PaperSizeId;";
        return await Sql.ExecuteAsync(conn, sql, e) > 0;
    }

    public override async Task<bool> DeleteAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "UPDATE dbo.InvoicePaperSize SET IsActive = 0, ModifiedAt = GETDATE(), ModifiedBy = @modifiedBy WHERE PaperSizeId = @id",
            new { id, modifiedBy = (long?)null }) > 0;
    }
}

/* ------------------------- 7. PrinterType ------------------------- */

public interface IDocumentPrinterTypeRepository : IDocumentMasterRepository<PrinterType>
{
}

public class DocumentPrinterTypeRepository : DocumentMasterRepositoryBase<PrinterType>, IDocumentPrinterTypeRepository
{
    protected override string Table => "PrinterType";
    protected override string IdColumn => "PrinterTypeId";

    public DocumentPrinterTypeRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public override async Task<IEnumerable<PrinterType>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.PrinterType ORDER BY Name"
            : "SELECT * FROM dbo.PrinterType WHERE IsActive = 1 ORDER BY Name";
        return await Sql.QueryAsync<PrinterType>(conn, sql);
    }

    public override async Task<long> InsertAsync(PrinterType e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.PrinterType (Code, Name, Description, IsActive, CreatedBy)
            VALUES (@Code, @Name, @Description, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, e);
    }

    public override async Task<bool> UpdateAsync(PrinterType e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.PrinterType
            SET Code = @Code, Name = @Name, Description = @Description, IsActive = @IsActive,
                ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
            WHERE PrinterTypeId = @PrinterTypeId;";
        return await Sql.ExecuteAsync(conn, sql, e) > 0;
    }
}

/* ------------------------- 8. PrinterModel ------------------------- */

public interface IDocumentPrinterModelRepository : IDocumentMasterRepository<PrinterModel>
{
}

public class DocumentPrinterModelRepository : DocumentMasterRepositoryBase<PrinterModel>, IDocumentPrinterModelRepository
{
    protected override string Table => "PrinterModel";
    protected override string IdColumn => "PrinterModelId";

    public DocumentPrinterModelRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public override async Task<IEnumerable<PrinterModel>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.PrinterModel ORDER BY Name"
            : "SELECT * FROM dbo.PrinterModel WHERE IsActive = 1 ORDER BY Name";
        return await Sql.QueryAsync<PrinterModel>(conn, sql);
    }

    public override async Task<long> InsertAsync(PrinterModel e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.PrinterModel (PrinterTypeId, Code, Name, Manufacturer, IsActive, CreatedBy)
            VALUES (@PrinterTypeId, @Code, @Name, @Manufacturer, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, e);
    }

    public override async Task<bool> UpdateAsync(PrinterModel e)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.PrinterModel
            SET PrinterTypeId = @PrinterTypeId, Code = @Code, Name = @Name, Manufacturer = @Manufacturer,
                IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
            WHERE PrinterModelId = @PrinterModelId;";
        return await Sql.ExecuteAsync(conn, sql, e) > 0;
    }
}

/* ------------------------- 9. PrintOrientation ------------------------- */

public interface IPrintOrientationRepository : IDocumentMasterRepository<PrintOrientation>
{
}

public class PrintOrientationRepository : DocumentMasterRepositoryBase<PrintOrientation>, IPrintOrientationRepository
{
    protected override string Table => "PrintOrientation";
    protected override string IdColumn => "OrientationId";
    // No audit columns on this lookup table - hard delete.
    protected override bool SoftDelete => false;

    public PrintOrientationRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public override async Task<long> InsertAsync(PrintOrientation e)
    {
        using var conn = OpenTenant();
        const string sql = "INSERT INTO dbo.PrintOrientation (Code, Name, IsActive) VALUES (@Code, @Name, @IsActive); SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, e);
    }

    public override async Task<bool> UpdateAsync(PrintOrientation e)
    {
        using var conn = OpenTenant();
        const string sql = "UPDATE dbo.PrintOrientation SET Code = @Code, Name = @Name, IsActive = @IsActive WHERE OrientationId = @OrientationId;";
        return await Sql.ExecuteAsync(conn, sql, e) > 0;
    }
}

/* ------------------------- 10. PrintUnit ------------------------- */

public interface IPrintUnitRepository : IDocumentMasterRepository<PrintUnit>
{
}

public class PrintUnitRepository : DocumentMasterRepositoryBase<PrintUnit>, IPrintUnitRepository
{
    protected override string Table => "PrintUnit";
    protected override string IdColumn => "UnitId";
    // No audit columns on this lookup table - hard delete.
    protected override bool SoftDelete => false;

    public PrintUnitRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public override async Task<long> InsertAsync(PrintUnit e)
    {
        using var conn = OpenTenant();
        const string sql = "INSERT INTO dbo.PrintUnit (Code, Name, IsActive) VALUES (@Code, @Name, @IsActive); SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, e);
    }

    public override async Task<bool> UpdateAsync(PrintUnit e)
    {
        using var conn = OpenTenant();
        const string sql = "UPDATE dbo.PrintUnit SET Code = @Code, Name = @Name, IsActive = @IsActive WHERE UnitId = @UnitId;";
        return await Sql.ExecuteAsync(conn, sql, e) > 0;
    }
}

/* ---------------------------------------------------------------------------
   FK-in-use guards - used by the master delete endpoints to protect rows that
   are referenced by templates / designer payloads / printer configs.
   --------------------------------------------------------------------------- */

public interface IDocumentMasterGuardRepository
{
    Task<bool> InvoiceTypeInUseAsync(long invoiceTypeId);
    Task<bool> PaperSizeInUseAsync(long paperSizeId);
    Task<bool> PrinterTypeInUseAsync(long printerTypeId);
    Task<bool> PrinterModelInUseAsync(long printerModelId);
    Task<bool> CategoryInUseAsync(long templateCategoryId);
    Task<bool> ComponentInUseAsync(long componentId);
    Task<bool> VariableInUseAsync(long variableId);
    Task<bool> FontInUseAsync(long fontId);
    Task<bool> OrientationInUseAsync(long orientationId);
    Task<bool> UnitInUseAsync(long unitId);
}

public class DocumentMasterGuardRepository : TenantRepositoryBase, IDocumentMasterGuardRepository
{
    public DocumentMasterGuardRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<bool> InvoiceTypeInUseAsync(long invoiceTypeId)
    {
        using var conn = OpenTenant();
        var template = await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.InvoiceTemplate WHERE InvoiceTypeId = @id) THEN 1 ELSE 0 END", new { id = invoiceTypeId });
        if (template) return true;
        return await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.InvoiceTemplateAssignment WHERE InvoiceTypeId = @id) THEN 1 ELSE 0 END", new { id = invoiceTypeId });
    }

    public async Task<bool> PaperSizeInUseAsync(long paperSizeId)
    {
        using var conn = OpenTenant();
        var template = await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.InvoiceTemplate WHERE PaperSizeId = @id) THEN 1 ELSE 0 END", new { id = paperSizeId });
        if (template) return true;
        return await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.InvoiceTemplatePrinter WHERE PaperSizeId = @id) THEN 1 ELSE 0 END", new { id = paperSizeId });
    }

    public async Task<bool> PrinterTypeInUseAsync(long printerTypeId)
    {
        using var conn = OpenTenant();
        var model = await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.PrinterModel WHERE PrinterTypeId = @id) THEN 1 ELSE 0 END", new { id = printerTypeId });
        if (model) return true;
        return await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.InvoiceTemplatePrinter WHERE PrinterTypeId = @id) THEN 1 ELSE 0 END", new { id = printerTypeId });
    }

    public async Task<bool> PrinterModelInUseAsync(long printerModelId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.InvoiceTemplatePrinter WHERE PrinterModelId = @id) THEN 1 ELSE 0 END", new { id = printerModelId });
    }

    public async Task<bool> CategoryInUseAsync(long templateCategoryId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.InvoiceTemplate WHERE TemplateCategoryId = @id) THEN 1 ELSE 0 END", new { id = templateCategoryId });
    }

    public async Task<bool> ComponentInUseAsync(long componentId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.InvoiceTemplateElement WHERE ComponentId = @id) THEN 1 ELSE 0 END", new { id = componentId });
    }

    public async Task<bool> VariableInUseAsync(long variableId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.InvoiceTemplateField WHERE VariableId = @id) THEN 1 ELSE 0 END", new { id = variableId });
    }

    public async Task<bool> FontInUseAsync(long fontId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.InvoiceTemplateStyle WHERE FontId = @id) THEN 1 ELSE 0 END", new { id = fontId });
    }

    public async Task<bool> OrientationInUseAsync(long orientationId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.InvoiceTemplate WHERE OrientationId = @id) THEN 1 ELSE 0 END", new { id = orientationId });
    }

    public async Task<bool> UnitInUseAsync(long unitId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.InvoicePaperSize WHERE Unit = (SELECT Code FROM dbo.PrintUnit WHERE UnitId = @id)) THEN 1 ELSE 0 END", new { id = unitId });
    }
}
