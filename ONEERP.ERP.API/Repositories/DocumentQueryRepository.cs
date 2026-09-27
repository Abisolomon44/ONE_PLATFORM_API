using ONEERP.ERP.API.DTOs;

namespace ONEERP.ERP.API.Repositories;

/* ---------------------------------------------------------------------------
   Read-model + lifecycle queries for the DOCUMENT module (templates list with
   joins, assignments grid, runtime resolution, version publish/archive and
   default-flag housekeeping). Works on the EXISTING Invoice* tables only.
   --------------------------------------------------------------------------- */

public interface IDocumentQueryRepository
{
    Task<(IEnumerable<DocumentTemplateListItemDto> Items, int Total)> GetTemplatesPagedAsync(int page, int size, string search, IReadOnlyCollection<int>? allowedCompanyIds);
    Task<DocumentTemplateListItemDto?> GetTemplateRowAsync(long id);
    Task<DocumentTemplateListItemDto?> GetTemplateRowByCodeAsync(string code);
    Task<bool> HasActiveAssignmentsAsync(long templateId);
    Task<long?> FindActiveAssignmentAsync(long invoiceTemplateId, int? companyId, int? industryTypeId, long? invoiceTypeId, long? paperSizeId);
    Task<IEnumerable<TemplateAssignmentDto>> GetAssignmentsAsync(long? templateId);
    Task<long?> ResolveTemplateIdAsync(int? companyId, long invoiceTypeId, long? paperSizeId);
    Task<bool> ArchivePublishedVersionAsync(long versionId);
    Task<bool> MarkVersionPublishedAsync(long versionId, long publishedBy);
    Task<bool> ClearTemplateDefaultAsync();
    Task<bool> ClearPrinterDefaultAsync(long templateId);
    Task<TemplatePrinterDto?> GetPrinterRowAsync(long templatePrinterId);
    Task<long?> FindPrinterAsync(long templateId, long paperSizeId, long printerTypeId);

    // Ref-lookup helpers used by the design/setting services (no service owns a single table).
    Task<bool> InvoiceTypeExistsAsync(long id);
    Task<bool> PaperSizeExistsAsync(long id);
    Task<bool> OrientationExistsAsync(long id);
    Task<bool> IndustryTypeExistsAsync(int id);
    Task<long?> GetDefaultTemplateAsync(long invoiceTypeId, long? paperSizeId);
    Task<DocumentTemplateListItemDto?> GetDefaultTemplateRowAsync(long invoiceTypeId, long? paperSizeId);
    Task<string?> GetInvoiceTypeNameAsync(long id);
    Task<string?> GetPaperSizeNameAsync(long id);
    Task<bool> UpsertPrintSettingAsync(InvoiceTemplatePrintSettingDto setting);
}

public class DocumentQueryRepository : TenantRepositoryBase, IDocumentQueryRepository
{
    public DocumentQueryRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory) { }

    private const string TemplateSelectSql = @"
        SELECT t.InvoiceTemplateId, t.CompanyId, c.CompanyName, t.TemplateCategoryId, cat.[Name] AS CategoryName,
               t.InvoiceTypeId, it.[Name] AS InvoiceTypeName, t.PaperSizeId, ps.[Name] AS PaperSizeName,
               t.OrientationId, o.[Name] AS OrientationName, t.Code, t.[Name] AS Name, t.Description,
               t.Width, t.Height, t.IsDefault, t.IsActive, t.CreatedAt, t.ModifiedAt,
               ISNULL(v.VersionNumber, 0) AS LatestVersionNumber, ISNULL(v.[Status], '') AS LatestStatus,
               CASE WHEN pv.TemplateVersionId IS NOT NULL THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS HasPublishedVersion
        FROM dbo.InvoiceTemplate t
        LEFT JOIN dbo.Companies c ON c.Id = t.CompanyId
        LEFT JOIN dbo.InvoiceTemplateCategory cat ON cat.TemplateCategoryId = t.TemplateCategoryId
        JOIN dbo.InvoiceType it ON it.InvoiceTypeId = t.InvoiceTypeId
        JOIN dbo.InvoicePaperSize ps ON ps.PaperSizeId = t.PaperSizeId
        JOIN dbo.PrintOrientation o ON o.OrientationId = t.OrientationId
        OUTER APPLY (SELECT TOP 1 VersionNumber, [Status] FROM dbo.InvoiceTemplateVersion
                     WHERE InvoiceTemplateId = t.InvoiceTemplateId ORDER BY VersionNumber DESC) v
        OUTER APPLY (SELECT TOP 1 TemplateVersionId FROM dbo.InvoiceTemplateVersion
                     WHERE InvoiceTemplateId = t.InvoiceTemplateId AND IsPublished = 1) pv";

    public async Task<(IEnumerable<DocumentTemplateListItemDto> Items, int Total)> GetTemplatesPagedAsync(
        int page, int size, string search, IReadOnlyCollection<int>? allowedCompanyIds)
    {
        if (allowedCompanyIds is not null && allowedCompanyIds.Count == 0)
            return (Array.Empty<DocumentTemplateListItemDto>(), 0);

        using var conn = OpenTenant();
        var companyFilter = allowedCompanyIds is null ? "" : " AND t.CompanyId IN @ids";
        var where = " WHERE t.IsActive = 1 AND (@search = '' OR t.Code LIKE '%' + @search + '%' OR t.[Name] LIKE '%' + @search + '%')" + companyFilter;
        var sql = TemplateSelectSql + where + @"
            ORDER BY t.ModifiedAt DESC, t.InvoiceTemplateId DESC
            OFFSET @offset ROWS FETCH NEXT @size ROWS ONLY";
        var items = await Sql.QueryAsync<DocumentTemplateListItemDto>(conn, sql,
            new { search = search ?? string.Empty, ids = allowedCompanyIds?.ToArray(), offset = (page - 1) * size, size });

        var total = await Sql.ExecuteScalarAsync<int>(conn, @"
            SELECT COUNT(1) FROM dbo.InvoiceTemplate t" + where,
            new { search = search ?? string.Empty, ids = allowedCompanyIds?.ToArray() });

        return (items, total);
    }

    public async Task<DocumentTemplateListItemDto?> GetTemplateRowAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.QueryFirstOrDefaultAsync<DocumentTemplateListItemDto>(
            conn, TemplateSelectSql + " WHERE t.InvoiceTemplateId = @id", new { id });
    }

    /// <summary>Exact-code lookup used by idempotent create flows (spec §6:
    /// "if exists: LOAD EXISTING"). Deliberately ignores IsActive and company
    /// scoping so it agrees with CodeInUseAsync — the paged list hides
    /// inactive/global rows, which caused false "not exists" → 409 on create.</summary>
    public async Task<DocumentTemplateListItemDto?> GetTemplateRowByCodeAsync(string code)
    {
        using var conn = OpenTenant();
        return await Sql.QueryFirstOrDefaultAsync<DocumentTemplateListItemDto>(
            conn, TemplateSelectSql + " WHERE UPPER(t.Code) = UPPER(@code)", new { code });
    }

    public async Task<bool> HasActiveAssignmentsAsync(long templateId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(conn, @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.InvoiceTemplateAssignment
                WHERE InvoiceTemplateId = @templateId AND IsActive = 1) THEN 1 ELSE 0 END",
            new { templateId });
    }

    public async Task<long?> FindActiveAssignmentAsync(long invoiceTemplateId, int? companyId, int? industryTypeId, long? invoiceTypeId, long? paperSizeId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<long?>(conn, @"
            SELECT TOP 1 AssignmentId FROM dbo.InvoiceTemplateAssignment
            WHERE IsActive = 1 AND InvoiceTemplateId <> @invoiceTemplateId
              AND ((CompanyId = @companyId) OR (CompanyId IS NULL AND @companyId IS NULL))
              AND ((IndustryTypeId = @industryTypeId) OR (IndustryTypeId IS NULL AND @industryTypeId IS NULL))
              AND ((InvoiceTypeId = @invoiceTypeId) OR (InvoiceTypeId IS NULL AND @invoiceTypeId IS NULL))
              AND ((PaperSizeId = @paperSizeId) OR (PaperSizeId IS NULL AND @paperSizeId IS NULL))
            ORDER BY AssignmentId",
            new { invoiceTemplateId, companyId, industryTypeId, invoiceTypeId, paperSizeId });
    }

    public async Task<IEnumerable<TemplateAssignmentDto>> GetAssignmentsAsync(long? templateId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryAsync<TemplateAssignmentDto>(conn, @"
            SELECT a.AssignmentId, a.InvoiceTemplateId, t.Code AS TemplateCode, t.[Name] AS TemplateName,
                   a.CompanyId, c.CompanyName, a.IndustryTypeId, ind.[Name] AS IndustryTypeName,
                   a.InvoiceTypeId, it.[Name] AS InvoiceTypeName, a.PaperSizeId, ps.[Name] AS PaperSizeName,
                   a.IsDefault, a.IsActive
            FROM dbo.InvoiceTemplateAssignment a
            JOIN dbo.InvoiceTemplate t ON t.InvoiceTemplateId = a.InvoiceTemplateId
            LEFT JOIN dbo.Companies c ON c.Id = a.CompanyId
            LEFT JOIN dbo.IndustryTypes ind ON ind.IndustryTypeId = a.IndustryTypeId
            LEFT JOIN dbo.InvoiceType it ON it.InvoiceTypeId = a.InvoiceTypeId
            LEFT JOIN dbo.InvoicePaperSize ps ON ps.PaperSizeId = a.PaperSizeId
            WHERE a.IsActive = 1 AND (@templateId IS NULL OR a.InvoiceTemplateId = @templateId)
            ORDER BY a.AssignmentId",
            new { templateId });
    }

    /// <summary>Runtime resolution: most specific active assignment wins (company > doc type > paper, then default).</summary>
    public async Task<long?> ResolveTemplateIdAsync(int? companyId, long invoiceTypeId, long? paperSizeId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<long?>(conn, @"
            SELECT TOP 1 a.InvoiceTemplateId
            FROM dbo.InvoiceTemplateAssignment a
            WHERE a.IsActive = 1
              AND (a.CompanyId = @companyId OR a.CompanyId IS NULL OR @companyId IS NULL)
              AND (a.InvoiceTypeId = @invoiceTypeId OR a.InvoiceTypeId IS NULL)
              AND (a.PaperSizeId = @paperSizeId OR a.PaperSizeId IS NULL)
            ORDER BY (CASE WHEN a.CompanyId IS NOT NULL THEN 0 ELSE 1 END),
                     (CASE WHEN a.InvoiceTypeId IS NOT NULL THEN 0 ELSE 1 END),
                     (CASE WHEN a.PaperSizeId IS NOT NULL THEN 0 ELSE 1 END),
                     a.IsDefault DESC, a.AssignmentId DESC",
            new { companyId, invoiceTypeId, paperSizeId });
    }

    public async Task<bool> ArchivePublishedVersionAsync(long versionId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn, @"
            UPDATE dbo.InvoiceTemplateVersion
            SET [Status] = 'ARCHIVED', IsPublished = 0
            WHERE TemplateVersionId = @versionId AND IsPublished = 1",
            new { versionId }) > 0;
    }

    public async Task<bool> MarkVersionPublishedAsync(long versionId, long publishedBy)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn, @"
            UPDATE dbo.InvoiceTemplateVersion
            SET [Status] = 'PUBLISHED', IsPublished = 1, PublishedBy = @publishedBy, PublishedAt = GETDATE()
            WHERE TemplateVersionId = @versionId AND [Status] = 'DRAFT'",
            new { versionId, publishedBy }) > 0;
    }

    public async Task<bool> ClearTemplateDefaultAsync()
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn,
            "UPDATE dbo.InvoiceTemplate SET IsDefault = 0 WHERE IsDefault = 1") > 0;
    }

    public async Task<bool> ClearPrinterDefaultAsync(long templateId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteAsync(conn, @"
            UPDATE dbo.InvoiceTemplatePrinter SET IsDefault = 0
            WHERE InvoiceTemplateId = @templateId AND IsDefault = 1",
            new { templateId }) > 0;
    }

    public async Task<TemplatePrinterDto?> GetPrinterRowAsync(long templatePrinterId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryFirstOrDefaultAsync<TemplatePrinterDto>(conn, @"
            SELECT p.TemplatePrinterId, p.InvoiceTemplateId, p.PaperSizeId, ps.[Name] AS PaperSizeName,
                   p.PrinterTypeId, pt.[Name] AS PrinterTypeName, p.PrinterModelId, pm.[Name] AS PrinterModelName,
                   p.PrinterName, p.IsDefault, p.IsActive
            FROM dbo.InvoiceTemplatePrinter p
            JOIN dbo.InvoicePaperSize ps ON ps.PaperSizeId = p.PaperSizeId
            JOIN dbo.PrinterType pt ON pt.PrinterTypeId = p.PrinterTypeId
            LEFT JOIN dbo.PrinterModel pm ON pm.PrinterModelId = p.PrinterModelId
            WHERE p.TemplatePrinterId = @templatePrinterId",
            new { templatePrinterId });
    }

    public async Task<long?> FindPrinterAsync(long templateId, long paperSizeId, long printerTypeId)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<long?>(conn, @"
            SELECT TOP 1 TemplatePrinterId FROM dbo.InvoiceTemplatePrinter
            WHERE InvoiceTemplateId = @templateId AND PaperSizeId = @paperSizeId AND PrinterTypeId = @printerTypeId
            ORDER BY TemplatePrinterId",
            new { templateId, paperSizeId, printerTypeId });
    }

    public async Task<bool> InvoiceTypeExistsAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.InvoiceType WHERE InvoiceTypeId = @id) THEN 1 ELSE 0 END", new { id });
    }

    public async Task<bool> PaperSizeExistsAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.InvoicePaperSize WHERE PaperSizeId = @id) THEN 1 ELSE 0 END", new { id });
    }

    public async Task<bool> OrientationExistsAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.PrintOrientation WHERE OrientationId = @id) THEN 1 ELSE 0 END", new { id });
    }

    public async Task<bool> IndustryTypeExistsAsync(int id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(conn,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.IndustryTypes WHERE IndustryTypeId = @id) THEN 1 ELSE 0 END", new { id });
    }

    public async Task<long?> GetDefaultTemplateAsync(long invoiceTypeId, long? paperSizeId)
    {
        using var conn = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<long?>(conn, @"
            SELECT TOP 1 CAST(t.InvoiceTemplateId AS bigint)
            FROM dbo.InvoiceTemplate t
            WHERE t.IsActive = 1 AND t.InvoiceTypeId = @invoiceTypeId
              AND (t.PaperSizeId = @paperSizeId OR (@paperSizeId IS NULL AND t.PaperSizeId IS NULL))
            ORDER BY t.IsDefault DESC, t.InvoiceTemplateId DESC",
            new { invoiceTypeId, paperSizeId });
    }

    public async Task<DocumentTemplateListItemDto?> GetDefaultTemplateRowAsync(long invoiceTypeId, long? paperSizeId)
    {
        using var conn = OpenTenant();
        return await Sql.QueryFirstOrDefaultAsync<DocumentTemplateListItemDto>(conn, @"
            SELECT TOP 1 * FROM (
                SELECT t.*, ISNULL(v.VersionNumber, 0) AS LatestVersionNumber, ISNULL(v.[Status], '') AS LatestStatus,
                       CASE WHEN pv.TemplateVersionId IS NOT NULL THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS HasPublishedVersion
                FROM dbo.InvoiceTemplate t
                OUTER APPLY (SELECT TOP 1 VersionNumber, [Status] FROM dbo.InvoiceTemplateVersion
                             WHERE InvoiceTemplateId = t.InvoiceTemplateId ORDER BY VersionNumber DESC) v
                OUTER APPLY (SELECT TOP 1 TemplateVersionId FROM dbo.InvoiceTemplateVersion
                             WHERE InvoiceTemplateId = t.InvoiceTemplateId AND IsPublished = 1) pv
                WHERE t.IsActive = 1 AND t.InvoiceTypeId = @invoiceTypeId
                  AND (t.PaperSizeId = @paperSizeId OR (@paperSizeId IS NULL AND t.PaperSizeId IS NULL))
                ORDER BY t.IsDefault DESC, t.InvoiceTemplateId DESC) t",
            new { invoiceTypeId, paperSizeId });
    }

    public async Task<string?> GetInvoiceTypeNameAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<string?>(conn,
            "SELECT [Name] FROM dbo.InvoiceType WHERE InvoiceTypeId = @id", new { id });
    }

    public async Task<string?> GetPaperSizeNameAsync(long id)
    {
        using var conn = OpenTenant();
        return await Sql.ExecuteScalarAsync<string?>(conn,
            "SELECT [Name] FROM dbo.InvoicePaperSize WHERE PaperSizeId = @id", new { id });
    }

    public async Task<bool> UpsertPrintSettingAsync(InvoiceTemplatePrintSettingDto setting)
    {
        using var conn = OpenTenant();
        if (setting.PrintSettingId > 0)
            return await Sql.ExecuteAsync(conn, @"
                UPDATE dbo.InvoiceTemplatePrintSetting
                SET MarginTop = @MarginTop, MarginRight = @MarginRight, MarginBottom = @MarginBottom, MarginLeft = @MarginLeft,
                    Scale = @Scale, Copies = @Copies, AutoFit = @AutoFit, CutPaper = @CutPaper,
                    PrintHeader = @PrintHeader, PrintFooter = @PrintFooter,
                    ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                WHERE PrintSettingId = @PrintSettingId", setting) > 0;
        var id = await Sql.QuerySingleOrDefaultAsync<long>(conn, @"
            INSERT INTO dbo.InvoiceTemplatePrintSetting (TemplatePrinterId, MarginTop, MarginRight, MarginBottom, MarginLeft,
                Scale, Copies, AutoFit, CutPaper, PrintHeader, PrintFooter, CreatedBy)
            VALUES (@TemplatePrinterId, @MarginTop, @MarginRight, @MarginBottom, @MarginLeft,
                @Scale, @Copies, @AutoFit, @CutPaper, @PrintHeader, @PrintFooter, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);", setting);
        setting.PrintSettingId = id;
        return true;
    }
}
