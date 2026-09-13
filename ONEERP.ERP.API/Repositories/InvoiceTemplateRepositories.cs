using System.Data;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

/* ---------------------------------------------------------------------------
   Lookup repository - seeded system masters used by the designer UI
   --------------------------------------------------------------------------- */
public interface IInvoiceTemplateLookupRepository
{
    Task<IEnumerable<InvoiceType>> GetInvoiceTypesAsync(bool includeInactive = false);
    Task<IEnumerable<InvoicePaperSize>> GetPaperSizesAsync(bool includeInactive = false);
    Task<IEnumerable<PrinterType>> GetPrinterTypesAsync(bool includeInactive = false);
    Task<IEnumerable<PrinterModel>> GetPrinterModelsAsync(long? printerTypeId = null, bool includeInactive = false);
    Task<IEnumerable<InvoiceTemplateCategory>> GetCategoriesAsync(bool includeInactive = false);
    Task<IEnumerable<InvoiceTemplateComponent>> GetComponentsAsync(bool includeInactive = false);
    Task<IEnumerable<InvoiceTemplateVariable>> GetVariablesAsync(bool includeInactive = false);
    Task<IEnumerable<InvoiceFont>> GetFontsAsync(bool includeInactive = false);
    Task<IEnumerable<PrintOrientation>> GetOrientationsAsync(bool includeInactive = false);
    Task<IEnumerable<PrintUnit>> GetUnitsAsync(bool includeInactive = false);
}

public class InvoiceTemplateLookupRepository : TenantRepositoryBase, IInvoiceTemplateLookupRepository
{
    public InvoiceTemplateLookupRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<InvoiceType>> GetInvoiceTypesAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.InvoiceType ORDER BY DisplayOrder, Name"
            : "SELECT * FROM dbo.InvoiceType WHERE IsActive = 1 ORDER BY DisplayOrder, Name";
        return await Sql.QueryAsync<InvoiceType>(conn, sql);
    }

    public async Task<IEnumerable<InvoicePaperSize>> GetPaperSizesAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.InvoicePaperSize ORDER BY IsThermal, Width, Name"
            : "SELECT * FROM dbo.InvoicePaperSize WHERE IsActive = 1 ORDER BY IsThermal, Width, Name";
        return await Sql.QueryAsync<InvoicePaperSize>(conn, sql);
    }

    public async Task<IEnumerable<PrinterType>> GetPrinterTypesAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.PrinterType ORDER BY Name"
            : "SELECT * FROM dbo.PrinterType WHERE IsActive = 1 ORDER BY Name";
        return await Sql.QueryAsync<PrinterType>(conn, sql);
    }

    public async Task<IEnumerable<PrinterModel>> GetPrinterModelsAsync(long? printerTypeId = null, bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.PrinterModel WHERE (@printerTypeId IS NULL OR PrinterTypeId = @printerTypeId) ORDER BY Name"
            : "SELECT * FROM dbo.PrinterModel WHERE IsActive = 1 AND (@printerTypeId IS NULL OR PrinterTypeId = @printerTypeId) ORDER BY Name";
        return await Sql.QueryAsync<PrinterModel>(conn, sql, new { printerTypeId });
    }

    public async Task<IEnumerable<InvoiceTemplateCategory>> GetCategoriesAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.InvoiceTemplateCategory ORDER BY DisplayOrder, Name"
            : "SELECT * FROM dbo.InvoiceTemplateCategory WHERE IsActive = 1 ORDER BY DisplayOrder, Name";
        return await Sql.QueryAsync<InvoiceTemplateCategory>(conn, sql);
    }

    public async Task<IEnumerable<InvoiceTemplateComponent>> GetComponentsAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.InvoiceTemplateComponent ORDER BY DisplayOrder, Name"
            : "SELECT * FROM dbo.InvoiceTemplateComponent WHERE IsActive = 1 ORDER BY DisplayOrder, Name";
        return await Sql.QueryAsync<InvoiceTemplateComponent>(conn, sql);
    }

    public async Task<IEnumerable<InvoiceTemplateVariable>> GetVariablesAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.InvoiceTemplateVariable ORDER BY Category, Name"
            : "SELECT * FROM dbo.InvoiceTemplateVariable WHERE IsActive = 1 ORDER BY Category, Name";
        return await Sql.QueryAsync<InvoiceTemplateVariable>(conn, sql);
    }

    public async Task<IEnumerable<InvoiceFont>> GetFontsAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.InvoiceFont ORDER BY Name"
            : "SELECT * FROM dbo.InvoiceFont WHERE IsActive = 1 ORDER BY Name";
        return await Sql.QueryAsync<InvoiceFont>(conn, sql);
    }

    public async Task<IEnumerable<PrintOrientation>> GetOrientationsAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.PrintOrientation ORDER BY OrientationId"
            : "SELECT * FROM dbo.PrintOrientation WHERE IsActive = 1 ORDER BY OrientationId";
        return await Sql.QueryAsync<PrintOrientation>(conn, sql);
    }

    public async Task<IEnumerable<PrintUnit>> GetUnitsAsync(bool includeInactive = false)
    {
        using var conn = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.PrintUnit ORDER BY UnitId"
            : "SELECT * FROM dbo.PrintUnit WHERE IsActive = 1 ORDER BY UnitId";
        return await Sql.QueryAsync<PrintUnit>(conn, sql);
    }
}

/* ---------------------------------------------------------------------------
   InvoiceTemplate repository (header CRUD)
   --------------------------------------------------------------------------- */
public interface IInvoiceTemplateRepository
{
    Task<IEnumerable<InvoiceTemplate>> GetAllAsync(IReadOnlyCollection<int>? allowedCompanyIds);
    Task<InvoiceTemplate?> GetByIdAsync(long id);
    Task<bool> CodeInUseAsync(int? companyId, string code, long? excludeId = null);
    Task<long> InsertAsync(InvoiceTemplate entity);
    Task<bool> UpdateAsync(InvoiceTemplate entity);
    Task<bool> SoftDeleteAsync(long id);
    Task<InvoiceTemplate?> GetDefaultByInvoiceTypeAsync(int? companyId, long invoiceTypeId, long paperSizeId);
}

public class InvoiceTemplateRepository : TenantRepositoryBase, IInvoiceTemplateRepository
{
    public InvoiceTemplateRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<InvoiceTemplate>> GetAllAsync(IReadOnlyCollection<int>? allowedCompanyIds)
    {
        using var conn = OpenTenant();
        if (allowedCompanyIds is null)
            return await Sql.QueryAsync<InvoiceTemplate>(conn,
                "SELECT * FROM dbo.InvoiceTemplate WHERE IsActive = 1 ORDER BY ModifiedAt DESC, InvoiceTemplateId DESC");

        return await Sql.QueryAsync<InvoiceTemplate>(conn,
            "SELECT * FROM dbo.InvoiceTemplate WHERE IsActive = 1 AND CompanyId IN @ids ORDER BY ModifiedAt DESC, InvoiceTemplateId DESC",
            new { ids = allowedCompanyIds.ToArray() });
    }

    public async Task<InvoiceTemplate?> GetByIdAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<InvoiceTemplate>(conn,
            "SELECT * FROM dbo.InvoiceTemplate WHERE InvoiceTemplateId = @id", new { id });
    }

    public async Task<bool> CodeInUseAsync(int? companyId, string code, long? excludeId = null)
    {
        using var conn = OpenTenant();
        var sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.InvoiceTemplate
                WHERE UPPER(Code) = UPPER(@code)
                  AND ((CompanyId = @companyId) OR (CompanyId IS NULL AND @companyId IS NULL))
                  AND (@excludeId IS NULL OR InvoiceTemplateId <> @excludeId)
            ) THEN 1 ELSE 0 END";
        return await Sql.ExecuteScalarAsync<bool>(conn, sql, new { companyId, code, excludeId });
    }

    public async Task<long> InsertAsync(InvoiceTemplate entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.InvoiceTemplate (CompanyId, TemplateCategoryId, InvoiceTypeId, PaperSizeId, OrientationId,
                Code, Name, Description, Width, Height, IsDefault, IsActive, CreatedBy)
            VALUES (@CompanyId, @TemplateCategoryId, @InvoiceTypeId, @PaperSizeId, @OrientationId,
                @Code, @Name, @Description, @Width, @Height, @IsDefault, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, entity);
    }

    public async Task<bool> UpdateAsync(InvoiceTemplate entity)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.InvoiceTemplate
            SET CompanyId = @CompanyId, TemplateCategoryId = @TemplateCategoryId, InvoiceTypeId = @InvoiceTypeId,
                PaperSizeId = @PaperSizeId, OrientationId = @OrientationId, Code = @Code, Name = @Name,
                Description = @Description, Width = @Width, Height = @Height,
                IsDefault = @IsDefault, IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
            WHERE InvoiceTemplateId = @InvoiceTemplateId;";
        return await Sql.ExecuteAsync(conn, sql, entity) > 0;
    }

    public async Task<bool> SoftDeleteAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "UPDATE dbo.InvoiceTemplate SET IsActive = 0, ModifiedAt = GETDATE() WHERE InvoiceTemplateId = @id", new { id }) > 0;
    }

    public async Task<InvoiceTemplate?> GetDefaultByInvoiceTypeAsync(int? companyId, long invoiceTypeId, long paperSizeId)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<InvoiceTemplate>(conn, @"
            SELECT TOP 1 * FROM dbo.InvoiceTemplate
            WHERE IsActive = 1 AND InvoiceTypeId = @invoiceTypeId AND PaperSizeId = @paperSizeId
              AND (CompanyId = @companyId OR (CompanyId IS NULL AND @companyId IS NULL))
            ORDER BY IsDefault DESC, InvoiceTemplateId DESC", new { companyId, invoiceTypeId, paperSizeId });
    }
}

/* ---------------------------------------------------------------------------
   InvoiceTemplateVersion + designer persistence
   --------------------------------------------------------------------------- */
public interface IInvoiceTemplateVersionRepository
{
    Task<IEnumerable<InvoiceTemplateVersion>> GetByTemplateAsync(long templateId);
    Task<InvoiceTemplateVersion?> GetByIdAsync(long id);
    Task<InvoiceTemplateVersion?> GetLatestAsync(long templateId);
    Task<InvoiceTemplateVersion?> GetLatestDraftAsync(long templateId);
    Task<InvoiceTemplateVersion?> GetPublishedAsync(long templateId);
    Task<InvoiceTemplateVersion?> GetByNumberAsync(long templateId, int versionNumber);
    Task<long> InsertAsync(InvoiceTemplateVersion version);
    Task<bool> UpdateStatusAsync(long versionId, string status, bool isPublished, long? publishedBy, DateTime? publishedAt);
    Task<long> CloneToDraftAsync(long templateId);
    Task<DesignerVersionDto> GetDesignerAsync(long templateVersionId);
    Task<bool> ReplaceDesignerAsync(long templateVersionId, IReadOnlyList<DesignerSectionDto> sections);
}

public class InvoiceTemplateVersionRepository : TenantRepositoryBase, IInvoiceTemplateVersionRepository
{
    public InvoiceTemplateVersionRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<InvoiceTemplateVersion>> GetByTemplateAsync(long templateId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<InvoiceTemplateVersion>(conn,
            "SELECT * FROM dbo.InvoiceTemplateVersion WHERE InvoiceTemplateId = @templateId ORDER BY VersionNumber DESC",
            new { templateId });
    }

    public async Task<InvoiceTemplateVersion?> GetByIdAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<InvoiceTemplateVersion>(conn,
            "SELECT * FROM dbo.InvoiceTemplateVersion WHERE TemplateVersionId = @id", new { id });
    }

    public async Task<InvoiceTemplateVersion?> GetLatestAsync(long templateId)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<InvoiceTemplateVersion>(conn, @"
            SELECT TOP 1 * FROM dbo.InvoiceTemplateVersion
            WHERE InvoiceTemplateId = @templateId ORDER BY VersionNumber DESC", new { templateId });
    }

    public async Task<InvoiceTemplateVersion?> GetLatestDraftAsync(long templateId)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<InvoiceTemplateVersion>(conn, @"
            SELECT TOP 1 * FROM dbo.InvoiceTemplateVersion
            WHERE InvoiceTemplateId = @templateId AND Status = 'DRAFT'
            ORDER BY VersionNumber DESC", new { templateId });
    }

    public async Task<InvoiceTemplateVersion?> GetPublishedAsync(long templateId)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<InvoiceTemplateVersion>(conn, @"
            SELECT TOP 1 * FROM dbo.InvoiceTemplateVersion
            WHERE InvoiceTemplateId = @templateId AND IsPublished = 1
            ORDER BY VersionNumber DESC", new { templateId });
    }

    public async Task<InvoiceTemplateVersion?> GetByNumberAsync(long templateId, int versionNumber)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<InvoiceTemplateVersion>(conn,
            "SELECT * FROM dbo.InvoiceTemplateVersion WHERE InvoiceTemplateId = @templateId AND VersionNumber = @versionNumber",
            new { templateId, versionNumber });
    }

    public async Task<long> InsertAsync(InvoiceTemplateVersion version)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.InvoiceTemplateVersion (InvoiceTemplateId, VersionNumber, TemplateJson, Status, IsPublished, CreatedBy)
            VALUES (@InvoiceTemplateId, @VersionNumber, @TemplateJson, @Status, @IsPublished, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, version);
    }

    public async Task<bool> UpdateStatusAsync(long versionId, string status, bool isPublished, long? publishedBy, DateTime? publishedAt)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn, @"
            UPDATE dbo.InvoiceTemplateVersion
            SET Status = @status, IsPublished = @isPublished, PublishedBy = @publishedBy, PublishedAt = @publishedAt
            WHERE TemplateVersionId = @versionId AND Status = 'DRAFT'", new { versionId, status, isPublished, publishedBy, publishedAt }) > 0;
    }

    /// <summary>Creates a new DRAFT version (max+1) that copies the latest version's design.</summary>
    public async Task<long> CloneToDraftAsync(long templateId)
    {
        using var conn = OpenTenant();
        if (conn.State != ConnectionState.Open) conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            var latest = await Sql.QuerySingleOrDefaultAsync<InvoiceTemplateVersion>(conn, @"
                SELECT TOP 1 * FROM dbo.InvoiceTemplateVersion
                WHERE InvoiceTemplateId = @templateId ORDER BY VersionNumber DESC", new { templateId }, tx);

            var nextNumber = (latest?.VersionNumber ?? 0) + 1;
            var version = new InvoiceTemplateVersion
            {
                InvoiceTemplateId = templateId,
                VersionNumber = nextNumber,
                TemplateJson = latest?.TemplateJson ?? "{}",
                Status = "DRAFT",
                IsPublished = false
            };
            const string insertSql = @"
                INSERT INTO dbo.InvoiceTemplateVersion (InvoiceTemplateId, VersionNumber, TemplateJson, Status, IsPublished, CreatedBy)
                VALUES (@InvoiceTemplateId, @VersionNumber, @TemplateJson, @Status, @IsPublished, @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() AS bigint);";
            var versionId = await Sql.QuerySingleOrDefaultAsync<long>(conn, insertSql, version, tx);

            if (latest is not null)
            {
                var sections = await LoadDesignerRowsAsync(conn, tx, latest.TemplateVersionId);
                await InsertSectionsAsync(conn, tx, versionId, sections);
            }

            tx.Commit();
            return versionId;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <summary>Materializes the designer version (sections -> elements -> fields/item columns/style).</summary>
    public async Task<DesignerVersionDto> GetDesignerAsync(long templateVersionId)
    {
        using var conn = OpenTenant();
        var version = await Sql.QuerySingleOrDefaultAsync<InvoiceTemplateVersion>(conn,
            "SELECT * FROM dbo.InvoiceTemplateVersion WHERE TemplateVersionId = @templateVersionId", new { templateVersionId })
            ?? throw new ONEERP.Shared.Exceptions.NotFoundException($"Template version '{templateVersionId}' was not found.");

        var sections = (await Sql.QueryAsync<InvoiceTemplateSection>(conn,
            "SELECT * FROM dbo.InvoiceTemplateSection WHERE TemplateVersionId = @templateVersionId ORDER BY DisplayOrder",
            new { templateVersionId })).ToList();

        var sectionIds = sections.Select(s => s.SectionId).ToArray();
        var elements = sectionIds.Length == 0
            ? new List<InvoiceTemplateElement>()
            : (await Sql.QueryAsync<InvoiceTemplateElement>(conn,
                "SELECT * FROM dbo.InvoiceTemplateElement WHERE SectionId IN @ids ORDER BY DisplayOrder",
                new { ids = sectionIds })).ToList();

        var elementIds = elements.Select(e => e.ElementId).ToArray();
        var fields = elementIds.Length == 0
            ? new List<InvoiceTemplateField>()
            : (await Sql.QueryAsync<InvoiceTemplateField>(conn,
                "SELECT * FROM dbo.InvoiceTemplateField WHERE ElementId IN @ids", new { ids = elementIds })).ToList();
        var columns = elementIds.Length == 0
            ? new List<InvoiceTemplateItemColumn>()
            : (await Sql.QueryAsync<InvoiceTemplateItemColumn>(conn,
                "SELECT * FROM dbo.InvoiceTemplateItemColumn WHERE ElementId IN @ids ORDER BY DisplayOrder", new { ids = elementIds })).ToList();
        var styles = elementIds.Length == 0
            ? new List<InvoiceTemplateStyle>()
            : (await Sql.QueryAsync<InvoiceTemplateStyle>(conn,
                "SELECT * FROM dbo.InvoiceTemplateStyle WHERE ElementId IN @ids", new { ids = elementIds })).ToList();

        var elementDtos = elements.Select(e => new DesignerElementDto
        {
            ElementId = e.ElementId,
            ComponentId = e.ComponentId,
            ElementType = e.ElementType,
            ElementName = e.ElementName,
            X = e.X,
            Y = e.Y,
            Width = e.Width,
            Height = e.Height,
            DisplayOrder = e.DisplayOrder,
            IsVisible = e.IsVisible,
            Fields = fields.Where(f => f.ElementId == e.ElementId).Select(f => new DesignerFieldDto
            {
                TemplateFieldId = f.TemplateFieldId,
                VariableId = f.VariableId,
                FieldName = f.FieldName,
                BindingPath = f.BindingPath,
                Label = f.Label,
                IsVisible = f.IsVisible
            }).ToList(),
            ItemColumns = columns.Where(c => c.ElementId == e.ElementId).Select(c => new DesignerItemColumnDto
            {
                ItemColumnId = c.ItemColumnId,
                FieldName = c.FieldName,
                HeaderText = c.HeaderText,
                DisplayOrder = c.DisplayOrder,
                Width = c.Width,
                Alignment = c.Alignment,
                IsVisible = c.IsVisible
            }).ToList(),
            Style = styles.FirstOrDefault(s => s.ElementId == e.ElementId) is { } st ? new DesignerStyleDto
            {
                FontId = st.FontId,
                FontSize = st.FontSize,
                FontWeight = st.FontWeight,
                TextAlign = st.TextAlign,
                VerticalAlign = st.VerticalAlign,
                PaddingTop = st.PaddingTop,
                PaddingRight = st.PaddingRight,
                PaddingBottom = st.PaddingBottom,
                PaddingLeft = st.PaddingLeft,
                BorderTop = st.BorderTop,
                BorderRight = st.BorderRight,
                BorderBottom = st.BorderBottom,
                BorderLeft = st.BorderLeft
            } : null
        }).ToList();

        return new DesignerVersionDto
        {
            TemplateVersionId = version.TemplateVersionId,
            InvoiceTemplateId = version.InvoiceTemplateId,
            VersionNumber = version.VersionNumber,
            Status = version.Status,
            IsPublished = version.IsPublished,
            Sections = sections.Select(s => new DesignerSectionDto
            {
                SectionId = s.SectionId,
                SectionCode = s.SectionCode,
                SectionName = s.SectionName,
                DisplayOrder = s.DisplayOrder,
                X = s.X,
                Y = s.Y,
                Width = s.Width,
                Height = s.Height,
                IsVisible = s.IsVisible,
                Elements = elementDtos.Where(e => e.ElementId.HasValue && sectionElementMap(e.ElementId.Value, s, elements)).ToList()
            }).ToList()
        };
    }

    private static bool sectionElementMap(long elementId, InvoiceTemplateSection section, List<InvoiceTemplateElement> elements)
        => elements.Any(e => e.ElementId == elementId && e.SectionId == section.SectionId);

    /// <summary>Atomically replaces a DRAFT version's design (sections, elements, fields, item columns, styles).
    /// All DDL row changes run inside one transaction; any failure rolls everything back (Step 32).</summary>
    public async Task<bool> ReplaceDesignerAsync(long templateVersionId, IReadOnlyList<DesignerSectionDto> sections)
    {
        using var conn = OpenTenant();
        if (conn.State != ConnectionState.Open) conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            var existingSections = (await Sql.QueryAsync<InvoiceTemplateSection>(conn,
                "SELECT * FROM dbo.InvoiceTemplateSection WHERE TemplateVersionId = @templateVersionId", new { templateVersionId }, tx)).ToList();
            var existingSectionIds = existingSections.Select(s => s.SectionId).ToArray();
            var existingElements = existingSectionIds.Length == 0
                ? new List<InvoiceTemplateElement>()
                : (await Sql.QueryAsync<InvoiceTemplateElement>(conn,
                    "SELECT * FROM dbo.InvoiceTemplateElement WHERE SectionId IN @ids", new { ids = existingSectionIds }, tx)).ToList();
            var existingElementIds = existingElements.Select(e => e.ElementId).ToArray();

            if (existingElementIds.Length > 0)
            {
                await Sql.ExecuteAsync(conn, "DELETE FROM dbo.InvoiceTemplateStyle WHERE ElementId IN @ids", new { ids = existingElementIds }, tx);
                await Sql.ExecuteAsync(conn, "DELETE FROM dbo.InvoiceTemplateItemColumn WHERE ElementId IN @ids", new { ids = existingElementIds }, tx);
                await Sql.ExecuteAsync(conn, "DELETE FROM dbo.InvoiceTemplateField WHERE ElementId IN @ids", new { ids = existingElementIds }, tx);
            }
            if (existingSectionIds.Length > 0)
                await Sql.ExecuteAsync(conn, "DELETE FROM dbo.InvoiceTemplateElement WHERE SectionId IN @ids", new { ids = existingSectionIds }, tx);
            if (existingSectionIds.Length > 0)
                await Sql.ExecuteAsync(conn, "DELETE FROM dbo.InvoiceTemplateSection WHERE TemplateVersionId = @templateVersionId", new { templateVersionId }, tx);

            await InsertSectionsAsync(conn, tx, templateVersionId, sections.ToList());

            await Sql.ExecuteAsync(conn, @"
                UPDATE dbo.InvoiceTemplateVersion SET TemplateJson = @json, ModifiedAt = GETDATE()
                WHERE TemplateVersionId = @templateVersionId",
                new { templateVersionId, json = ToJson(sections) }, tx);

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private async Task<List<DesignerSectionDto>> LoadDesignerRowsAsync(IDbConnection conn, IDbTransaction tx, long templateVersionId)
    {
        var sections = (await Sql.QueryAsync<InvoiceTemplateSection>(conn,
            "SELECT * FROM dbo.InvoiceTemplateSection WHERE TemplateVersionId = @templateVersionId ORDER BY DisplayOrder",
            new { templateVersionId }, tx)).ToList();
        var sectionIds = sections.Select(s => s.SectionId).ToArray();
        var elements = sectionIds.Length == 0
            ? new List<InvoiceTemplateElement>()
            : (await Sql.QueryAsync<InvoiceTemplateElement>(conn,
                "SELECT * FROM dbo.InvoiceTemplateElement WHERE SectionId IN @ids ORDER BY DisplayOrder", new { ids = sectionIds }, tx)).ToList();
        var elementIds = elements.Select(e => e.ElementId).ToArray();
        var fields = elementIds.Length == 0 ? new List<InvoiceTemplateField>()
            : (await Sql.QueryAsync<InvoiceTemplateField>(conn, "SELECT * FROM dbo.InvoiceTemplateField WHERE ElementId IN @ids", new { ids = elementIds }, tx)).ToList();
        var columns = elementIds.Length == 0 ? new List<InvoiceTemplateItemColumn>()
            : (await Sql.QueryAsync<InvoiceTemplateItemColumn>(conn, "SELECT * FROM dbo.InvoiceTemplateItemColumn WHERE ElementId IN @ids ORDER BY DisplayOrder", new { ids = elementIds }, tx)).ToList();
        var styles = elementIds.Length == 0 ? new List<InvoiceTemplateStyle>()
            : (await Sql.QueryAsync<InvoiceTemplateStyle>(conn, "SELECT * FROM dbo.InvoiceTemplateStyle WHERE ElementId IN @ids", new { ids = elementIds }, tx)).ToList();

        return sections.Select(s => new DesignerSectionDto
        {
            SectionCode = s.SectionCode,
            SectionName = s.SectionName,
            DisplayOrder = s.DisplayOrder,
            X = s.X, Y = s.Y, Width = s.Width, Height = s.Height,
            IsVisible = s.IsVisible,
            Elements = elements.Where(e => e.SectionId == s.SectionId).Select(e => new DesignerElementDto
            {
                ComponentId = e.ComponentId,
                ElementType = e.ElementType,
                ElementName = e.ElementName,
                X = e.X, Y = e.Y, Width = e.Width, Height = e.Height,
                DisplayOrder = e.DisplayOrder,
                IsVisible = e.IsVisible,
                Fields = fields.Where(f => f.ElementId == e.ElementId).Select(f => new DesignerFieldDto
                {
                    VariableId = f.VariableId,
                    FieldName = f.FieldName,
                    BindingPath = f.BindingPath,
                    Label = f.Label,
                    IsVisible = f.IsVisible
                }).ToList(),
                ItemColumns = columns.Where(c => c.ElementId == e.ElementId).Select(c => new DesignerItemColumnDto
                {
                    FieldName = c.FieldName,
                    HeaderText = c.HeaderText,
                    DisplayOrder = c.DisplayOrder,
                    Width = c.Width,
                    Alignment = c.Alignment,
                    IsVisible = c.IsVisible
                }).ToList(),
                Style = styles.FirstOrDefault(s => s.ElementId == e.ElementId) is { } st ? new DesignerStyleDto
                {
                    FontId = st.FontId, FontSize = st.FontSize, FontWeight = st.FontWeight,
                    TextAlign = st.TextAlign, VerticalAlign = st.VerticalAlign,
                    PaddingTop = st.PaddingTop, PaddingRight = st.PaddingRight,
                    PaddingBottom = st.PaddingBottom, PaddingLeft = st.PaddingLeft,
                    BorderTop = st.BorderTop, BorderRight = st.BorderRight,
                    BorderBottom = st.BorderBottom, BorderLeft = st.BorderLeft
                } : null
            }).ToList()
        }).ToList();
    }

    private async Task InsertSectionsAsync(IDbConnection conn, IDbTransaction tx, long templateVersionId, IReadOnlyList<DesignerSectionDto> sections)
    {
        const string sectionSql = @"
            INSERT INTO dbo.InvoiceTemplateSection (TemplateVersionId, SectionCode, SectionName, DisplayOrder, X, Y, Width, Height, IsVisible)
            VALUES (@TemplateVersionId, @SectionCode, @SectionName, @DisplayOrder, @X, @Y, @Width, @Height, @IsVisible);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        const string elementSql = @"
            INSERT INTO dbo.InvoiceTemplateElement (SectionId, ComponentId, ElementType, ElementName, X, Y, Width, Height, DisplayOrder, IsVisible)
            VALUES (@SectionId, @ComponentId, @ElementType, @ElementName, @X, @Y, @Width, @Height, @DisplayOrder, @IsVisible);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        const string fieldSql = @"
            INSERT INTO dbo.InvoiceTemplateField (ElementId, VariableId, FieldName, BindingPath, Label, IsVisible)
            VALUES (@ElementId, @VariableId, @FieldName, @BindingPath, @Label, @IsVisible);";
        const string columnSql = @"
            INSERT INTO dbo.InvoiceTemplateItemColumn (ElementId, FieldName, HeaderText, DisplayOrder, Width, Alignment, IsVisible)
            VALUES (@ElementId, @FieldName, @HeaderText, @DisplayOrder, @Width, @Alignment, @IsVisible);";
        const string styleSql = @"
            INSERT INTO dbo.InvoiceTemplateStyle (ElementId, FontId, FontSize, FontWeight, TextAlign, VerticalAlign,
                PaddingTop, PaddingRight, PaddingBottom, PaddingLeft, BorderTop, BorderRight, BorderBottom, BorderLeft)
            VALUES (@ElementId, @FontId, @FontSize, @FontWeight, @TextAlign, @VerticalAlign,
                @PaddingTop, @PaddingRight, @PaddingBottom, @PaddingLeft, @BorderTop, @BorderRight, @BorderBottom, @BorderLeft);";

        foreach (var section in sections)
        {
            var sectionId = await Sql.QuerySingleOrDefaultAsync<long>(conn, sectionSql, new
            {
                templateVersionId, section.SectionCode, section.SectionName, section.DisplayOrder,
                section.X, section.Y, section.Width, section.Height, section.IsVisible
            }, tx);

            var order = 0;
            foreach (var element in section.Elements)
            {
                var elementId = await Sql.QuerySingleOrDefaultAsync<long>(conn, elementSql, new
                {
                    SectionId = sectionId, element.ComponentId, element.ElementType, element.ElementName,
                    element.X, element.Y, element.Width, element.Height,
                    DisplayOrder = element.DisplayOrder != 0 ? element.DisplayOrder : order++, element.IsVisible
                }, tx);

                foreach (var field in element.Fields)
                {
                    if (field.VariableId <= 0) continue;
                    await Sql.ExecuteAsync(conn, fieldSql, new
                    {
                        ElementId = elementId, field.VariableId, field.FieldName, field.BindingPath, field.Label, field.IsVisible
                    }, tx);
                }

                foreach (var column in element.ItemColumns)
                {
                    await Sql.ExecuteAsync(conn, columnSql, new
                    {
                        ElementId = elementId, column.FieldName, column.HeaderText,
                        column.DisplayOrder, column.Width, column.Alignment, column.IsVisible
                    }, tx);
                }

                if (element.Style is { } style)
                {
                    await Sql.ExecuteAsync(conn, styleSql, new
                    {
                        ElementId = elementId, style.FontId, style.FontSize, style.FontWeight,
                        style.TextAlign, style.VerticalAlign, style.PaddingTop, style.PaddingRight,
                        style.PaddingBottom, style.PaddingLeft, style.BorderTop, style.BorderRight,
                        style.BorderBottom, style.BorderLeft
                    }, tx);
                }
            }
        }
    }

    private static string ToJson(IReadOnlyList<DesignerSectionDto> sections)
        => System.Text.Json.JsonSerializer.Serialize(new { sections },
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
}

/* ---------------------------------------------------------------------------
   Template printer + print settings repository
   --------------------------------------------------------------------------- */
public interface IInvoiceTemplatePrinterRepository
{
    Task<IEnumerable<InvoiceTemplatePrinter>> GetByTemplateAsync(long templateId);
    Task<InvoiceTemplatePrinter?> GetByIdAsync(long id);
    Task<long> InsertAsync(InvoiceTemplatePrinter printer, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(InvoiceTemplatePrinter printer);
    Task<bool> DeleteAsync(long id);
    Task<InvoiceTemplatePrintSetting?> GetSettingAsync(long templatePrinterId);
    Task<long> InsertSettingAsync(InvoiceTemplatePrintSetting setting);
    Task<bool> UpdateSettingAsync(InvoiceTemplatePrintSetting setting);
}

public class InvoiceTemplatePrinterRepository : TenantRepositoryBase, IInvoiceTemplatePrinterRepository
{
    public InvoiceTemplatePrinterRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<InvoiceTemplatePrinter>> GetByTemplateAsync(long templateId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<InvoiceTemplatePrinter>(conn,
            "SELECT * FROM dbo.InvoiceTemplatePrinter WHERE InvoiceTemplateId = @templateId ORDER BY IsDefault DESC, TemplatePrinterId",
            new { templateId });
    }

    public async Task<InvoiceTemplatePrinter?> GetByIdAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<InvoiceTemplatePrinter>(conn,
            "SELECT * FROM dbo.InvoiceTemplatePrinter WHERE TemplatePrinterId = @id", new { id });
    }

    public async Task<long> InsertAsync(InvoiceTemplatePrinter printer, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        const string sql = @"
            INSERT INTO dbo.InvoiceTemplatePrinter (InvoiceTemplateId, PaperSizeId, PrinterTypeId, PrinterModelId, PrinterName, IsDefault, IsActive, CreatedBy)
            VALUES (@InvoiceTemplateId, @PaperSizeId, @PrinterTypeId, @PrinterModelId, @PrinterName, @IsDefault, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, printer, transaction);
    }

    public async Task<bool> UpdateAsync(InvoiceTemplatePrinter printer)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.InvoiceTemplatePrinter
            SET PaperSizeId = @PaperSizeId, PrinterTypeId = @PrinterTypeId, PrinterModelId = @PrinterModelId,
                PrinterName = @PrinterName, IsDefault = @IsDefault, IsActive = @IsActive,
                ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
            WHERE TemplatePrinterId = @TemplatePrinterId;";
        return await Sql.ExecuteAsync(conn, sql, printer) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var conn = OpenTenant();
        if (conn.State != ConnectionState.Open) conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            await Sql.ExecuteAsync(conn, "DELETE FROM dbo.InvoiceTemplatePrintSetting WHERE TemplatePrinterId = @id", new { id }, tx);
            var ok = await Sql.ExecuteAsync(conn, "DELETE FROM dbo.InvoiceTemplatePrinter WHERE TemplatePrinterId = @id", new { id }, tx) > 0;
            tx.Commit();
            return ok;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<InvoiceTemplatePrintSetting?> GetSettingAsync(long templatePrinterId)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<InvoiceTemplatePrintSetting>(conn,
            "SELECT * FROM dbo.InvoiceTemplatePrintSetting WHERE TemplatePrinterId = @templatePrinterId", new { templatePrinterId });
    }

    public async Task<long> InsertSettingAsync(InvoiceTemplatePrintSetting setting)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.InvoiceTemplatePrintSetting (TemplatePrinterId, MarginTop, MarginRight, MarginBottom, MarginLeft,
                Scale, Copies, AutoFit, CutPaper, PrintHeader, PrintFooter, CreatedBy)
            VALUES (@TemplatePrinterId, @MarginTop, @MarginRight, @MarginBottom, @MarginLeft,
                @Scale, @Copies, @AutoFit, @CutPaper, @PrintHeader, @PrintFooter, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, setting);
    }

    public async Task<bool> UpdateSettingAsync(InvoiceTemplatePrintSetting setting)
    {
        using var conn = OpenTenant();
        const string sql = @"
            UPDATE dbo.InvoiceTemplatePrintSetting
            SET MarginTop = @MarginTop, MarginRight = @MarginRight, MarginBottom = @MarginBottom, MarginLeft = @MarginLeft,
                Scale = @Scale, Copies = @Copies, AutoFit = @AutoFit, CutPaper = @CutPaper,
                PrintHeader = @PrintHeader, PrintFooter = @PrintFooter,
                ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
            WHERE PrintSettingId = @PrintSettingId;";
        return await Sql.ExecuteAsync(conn, sql, setting) > 0;
    }
}

/* ---------------------------------------------------------------------------
   Template assignment repository
   --------------------------------------------------------------------------- */
public interface IInvoiceTemplateAssignmentRepository
{
    Task<IEnumerable<InvoiceTemplateAssignment>> GetAllAsync();
    Task<InvoiceTemplateAssignment?> GetByIdAsync(long id);
    Task<long> InsertAsync(InvoiceTemplateAssignment assignment);
    Task<bool> DeleteAsync(long id);
}

public class InvoiceTemplateAssignmentRepository : TenantRepositoryBase, IInvoiceTemplateAssignmentRepository
{
    public InvoiceTemplateAssignmentRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    public async Task<IEnumerable<InvoiceTemplateAssignment>> GetAllAsync()
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<InvoiceTemplateAssignment>(conn,
            "SELECT * FROM dbo.InvoiceTemplateAssignment WHERE IsActive = 1 ORDER BY AssignmentId");
    }

    public async Task<InvoiceTemplateAssignment?> GetByIdAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<InvoiceTemplateAssignment>(conn,
            "SELECT * FROM dbo.InvoiceTemplateAssignment WHERE AssignmentId = @id", new { id });
    }

    public async Task<long> InsertAsync(InvoiceTemplateAssignment assignment)
    {
        using var conn = OpenTenant();
        const string sql = @"
            INSERT INTO dbo.InvoiceTemplateAssignment (InvoiceTemplateId, CompanyId, IndustryTypeId, InvoiceTypeId, PaperSizeId, IsDefault, IsActive, CreatedBy)
            VALUES (@InvoiceTemplateId, @CompanyId, @IndustryTypeId, @InvoiceTypeId, @PaperSizeId, @IsDefault, @IsActive, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        return await Sql.QuerySingleOrDefaultAsync<long>(conn, sql, assignment);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "DELETE FROM dbo.InvoiceTemplateAssignment WHERE AssignmentId = @id", new { id }) > 0;
    }
}