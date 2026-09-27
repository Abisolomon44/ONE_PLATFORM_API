using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

/* ============================================================================
   DOCUMENT DESIGN (Screen 2) service. Designs are persisted into
   InvoiceTemplateVersion.TemplateJson plus the relational Section/Element/
   Field/ItemColumn/Style rows. Templates are stored in InvoiceTemplate.
   ============================================================================ */

public interface IDocumentDesignService
{
    Task<string> RenderPreviewAsync(long versionId, long? salesInvoiceId);
    Task<DocumentLookupsDto> GetLookupsAsync(bool includeInactive = false);
    Task<PaginatedResult<DocumentTemplateListItemDto>> GetTemplatesPagedAsync(int page = 1, int size = 50, string search = "");
    Task<DocumentTemplateListItemDto> GetTemplateAsync(long id);
    Task<DocumentTemplateListItemDto> CreateTemplateAsync(SaveDocumentTemplateRequest request);
    Task<DocumentTemplateListItemDto> UpdateTemplateAsync(long id, SaveDocumentTemplateRequest request);
    Task DeleteTemplateAsync(long id);
    Task SetDefaultAsync(long id);
    Task<IEnumerable<DocumentTemplateVersionDto>> GetVersionsAsync(long templateId);
    Task<DocumentTemplateVersionDto> CreateVersionAsync(long templateId);
    Task<DesignerVersionDto> GetDesignerAsync(long versionId);
    Task SaveDesignerAsync(long versionId, SaveDesignerVersionRequest request);
    Task<PublishValidationResultDto> ValidatePublishAsync(long versionId);
    Task PublishAsync(long versionId);
    Task<DocumentTemplateVersionDto> CloneToDraftAsync(long versionId);
    Task<ResolvedDocumentTemplateDto?> ResolveDefaultAsync(int? companyId, long? invoiceTypeId, long? paperSizeId);
    Task<DocumentTemplateListItemDto> GetTemplateByCodeAsync(string code);
}

public class DocumentDesignService : IDocumentDesignService
{
    private readonly ONEERP.ERP.API.Repositories.ISalesRepository _salesRepo;
    private readonly IInvoiceTemplateRepository _templateRepo;
    private readonly IInvoiceTemplateVersionRepository _versionRepo;
    private readonly IDocumentQueryRepository _queryRepo;
    private readonly IInvoiceTemplateLookupRepository _lookupRepo;
    private readonly IInvoiceTemplateAssignmentRepository _assignmentRepo;
    private readonly IDataScopeResolver _scopeResolver;
    private readonly ICurrentUser _user;
    private readonly IAuditService _audit;

    public DocumentDesignService(
        IInvoiceTemplateRepository templateRepo,
        IInvoiceTemplateVersionRepository versionRepo,
        IDocumentQueryRepository queryRepo,
        IInvoiceTemplateLookupRepository lookupRepo,
        IInvoiceTemplateAssignmentRepository assignmentRepo,
        IDataScopeResolver scopeResolver,
        ICurrentUser user,
        IAuditService audit,
        ONEERP.ERP.API.Repositories.ISalesRepository salesRepo)
    {
        _templateRepo = templateRepo;
        _versionRepo = versionRepo;
        _salesRepo = salesRepo;
        _queryRepo = queryRepo;
        _lookupRepo = lookupRepo;
        _assignmentRepo = assignmentRepo;
        _scopeResolver = scopeResolver;
        _user = user;
        _audit = audit;
    }

    /* ------------------------------------------------------------------ */
    /* Lookups + resolution helpers                                       */
    /* ------------------------------------------------------------------ */

    public async Task<DocumentLookupsDto> GetLookupsAsync(bool includeInactive = false)
    {
        var d = new DocumentLookupsDto();
        foreach (var e in await _lookupRepo.GetInvoiceTypesAsync(includeInactive))
            d.InvoiceTypes.Add(MapInvoiceType(e));
        foreach (var e in await _lookupRepo.GetCategoriesAsync(includeInactive))
            d.Categories.Add(MapCategory(e));
        foreach (var e in await _lookupRepo.GetComponentsAsync(includeInactive))
            d.Components.Add(MapComponent(e));
        foreach (var e in await _lookupRepo.GetVariablesAsync(includeInactive))
            d.Variables.Add(MapVariable(e));
        foreach (var e in await _lookupRepo.GetFontsAsync(includeInactive))
            d.Fonts.Add(MapFont(e));
        foreach (var e in await _lookupRepo.GetPaperSizesAsync(includeInactive))
            d.PaperSizes.Add(MapPaperSize(e));
        foreach (var e in await _lookupRepo.GetPrinterTypesAsync(includeInactive))
            d.PrinterTypes.Add(MapPrinterType(e));
        foreach (var e in await _lookupRepo.GetPrinterModelsAsync(includeInactive: includeInactive))
            d.PrinterModels.Add(MapPrinterModel(e));
        foreach (var e in await _lookupRepo.GetOrientationsAsync(includeInactive))
            d.Orientations.Add(MapOrientation(e));
        foreach (var e in await _lookupRepo.GetUnitsAsync(includeInactive))
            d.Units.Add(MapUnit(e));
        return d;
    }

    private static DocumentLookupDto MapInvoiceType(InvoiceType t) => new()
    {
        Id = t.InvoiceTypeId, Code = t.Code, Name = t.Name, SortOrder = t.DisplayOrder, IsActive = t.IsActive
    };

    private static DocumentLookupDto MapCategory(InvoiceTemplateCategory c) => new()
    {
        Id = c.TemplateCategoryId, Code = c.Code, Name = c.Name, Category = c.Description, SortOrder = c.DisplayOrder, IsActive = c.IsActive
    };

    private static DocumentLookupDto MapComponent(InvoiceTemplateComponent c) => new()
    {
        Id = c.ComponentId, Code = c.Code, Name = c.Name, Category = c.ComponentType, ComponentType = c.ComponentType, SortOrder = c.DisplayOrder, IsActive = c.IsActive
    };

    private static DocumentLookupDto MapVariable(InvoiceTemplateVariable v) => new()
    {
        Id = v.VariableId, Code = v.Code, Name = v.Name, Category = v.Category, BindingPath = v.BindingPath, DataType = v.DataType, IsCollection = v.IsCollection, SortOrder = 0, IsActive = v.IsActive
    };

    private static DocumentLookupDto MapFont(InvoiceFont f) => new()
    {
        Id = f.FontId, Code = f.Code, Name = f.Name, FontFamily = f.FontFamily, SortOrder = 0, IsActive = f.IsActive
    };

    private static DocumentLookupDto MapPaperSize(InvoicePaperSize p) => new()
    {
        Id = p.PaperSizeId, Code = p.Code, Name = p.Name, Width = p.Width, Height = p.Height, Unit = p.Unit, IsThermal = p.IsThermal, IsCustom = p.IsCustom, SortOrder = 0, IsActive = p.IsActive
    };

    private static DocumentLookupDto MapPrinterType(PrinterType t) => new()
    {
        Id = t.PrinterTypeId, Code = t.Code, Name = t.Name, SortOrder = 0, IsActive = t.IsActive
    };

    private static DocumentLookupDto MapPrinterModel(PrinterModel m) => new()
    {
        Id = m.PrinterModelId, Code = m.Code, Name = m.Name, Category = m.Manufacturer, PrinterTypeId = m.PrinterTypeId, SortOrder = 0, IsActive = m.IsActive
    };

    private static DocumentLookupDto MapOrientation(PrintOrientation o) => new()
    {
        Id = o.OrientationId, Code = o.Code, Name = o.Name, SortOrder = 0, IsActive = o.IsActive
    };

    private static DocumentLookupDto MapUnit(PrintUnit u) => new()
    {
        Id = u.UnitId, Code = u.Code, Name = u.Name, SortOrder = 0, IsActive = u.IsActive
    };

    /* ------------------------------------------------------------------ */
    /* Template header CRUD                                               */
    /* ------------------------------------------------------------------ */

    public async Task<PaginatedResult<DocumentTemplateListItemDto>> GetTemplatesPagedAsync(int page = 1, int size = 50, string search = "")
    {
        var allowed = await _scopeResolver.GetAllowedCompanyIdsAsync();
        var (items, total) = await _queryRepo.GetTemplatesPagedAsync(page, size, search, allowed);
        return new PaginatedResult<DocumentTemplateListItemDto>
        {
            Items = items.ToList(),
            TotalCount = total,
            PageNumber = page,
            PageSize = size
        };
    }

    public async Task<DocumentTemplateListItemDto> GetTemplateAsync(long id)
    {
        var row = await _queryRepo.GetTemplateRowAsync(id)
            ?? throw new NotFoundException($"Document template '{id}' was not found.");
        if (row.CompanyId is int companyId && !await _scopeResolver.CanAccessCompanyAsync(companyId))
            throw new DomainException("You do not have permission to access this template.", 403);
        return row;
    }

    /// <summary>Exact-code lookup for idempotent create flows. Ignores IsActive and
    /// company scoping on purpose (same visibility as CodeInUseAsync).</summary>
    public async Task<DocumentTemplateListItemDto> GetTemplateByCodeAsync(string code)
    {
        var row = await _queryRepo.GetTemplateRowByCodeAsync((code ?? string.Empty).Trim())
            ?? throw new NotFoundException($"Document template with code '{code}' was not found.");
        if (row.CompanyId is int companyId && !await _scopeResolver.CanAccessCompanyAsync(companyId))
            throw new DomainException("You do not have permission to access this template.", 403);
        return row;
    }

    public async Task<DocumentTemplateListItemDto> CreateTemplateAsync(SaveDocumentTemplateRequest request)
    {
        if (await _lookupRepo.GetInvoiceTypeByIdAsync(request.InvoiceTypeId) is null)
            throw new DomainException("Invalid document type (InvoiceTypeId).", 400);
        if (await _lookupRepo.GetPaperSizeByIdAsync(request.PaperSizeId) is null)
            throw new DomainException("Invalid paper size (PaperSizeId).", 400);
        if (await _lookupRepo.GetOrientationByIdAsync(request.OrientationId) is null)
            throw new DomainException("Invalid print orientation (OrientationId).", 400);

        var code = request.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Document template code is required.", 400);
        if (await _templateRepo.CodeInUseAsync(request.CompanyId, code))
            throw new DomainException($"A document template with code '{code}' already exists.", 409);

        if (request.IsDefault)
            await _queryRepo.ClearTemplateDefaultAsync();

        var entity = new InvoiceTemplate
        {
            CompanyId = request.CompanyId,
            TemplateCategoryId = request.TemplateCategoryId,
            InvoiceTypeId = request.InvoiceTypeId,
            PaperSizeId = request.PaperSizeId,
            OrientationId = request.OrientationId,
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description,
            Width = request.Width,
            Height = request.Height,
            IsDefault = request.IsDefault,
            IsActive = true,
            CreatedBy = _user.UserId
        };
        entity.InvoiceTemplateId = await _templateRepo.InsertAsync(entity);

        // Open draft version 1 so the designer has content immediately.
        var version = new InvoiceTemplateVersion
        {
            InvoiceTemplateId = entity.InvoiceTemplateId,
            VersionNumber = 1,
            TemplateJson = "{}",
            Status = "DRAFT",
            IsPublished = false,
            CreatedBy = _user.UserId
        };
        await _versionRepo.InsertAsync(version);

        return await _queryRepo.GetTemplateRowAsync(entity.InvoiceTemplateId)
            ?? throw new InvalidOperationException("Template row was not returned after insert.");
    }

    public async Task<DocumentTemplateListItemDto> UpdateTemplateAsync(long id, SaveDocumentTemplateRequest request)
    {
        var entity = await _templateRepo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Document template '{id}' was not found.");
        if (entity.IsActive == false)
            throw new NotFoundException($"Document template '{id}' was not found.");

        if (await _lookupRepo.GetInvoiceTypeByIdAsync(request.InvoiceTypeId) is null)
            throw new DomainException("Invalid document type (InvoiceTypeId).", 400);
        if (await _lookupRepo.GetPaperSizeByIdAsync(request.PaperSizeId) is null)
            throw new DomainException("Invalid paper size (PaperSizeId).", 400);
        if (await _lookupRepo.GetOrientationByIdAsync(request.OrientationId) is null)
            throw new DomainException("Invalid print orientation (OrientationId).", 400);

        var code = request.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Document template code is required.", 400);
        if (await _templateRepo.CodeInUseAsync(request.CompanyId, code, id))
            throw new DomainException($"A document template with code '{code}' already exists.", 409);

        if (request.IsDefault)
            await _queryRepo.ClearTemplateDefaultAsync();

        entity.CompanyId = request.CompanyId;
        entity.TemplateCategoryId = request.TemplateCategoryId;
        entity.InvoiceTypeId = request.InvoiceTypeId;
        entity.PaperSizeId = request.PaperSizeId;
        entity.OrientationId = request.OrientationId;
        entity.Code = code;
        entity.Name = request.Name.Trim();
        entity.Description = request.Description;
        entity.Width = request.Width;
        entity.Height = request.Height;
        entity.IsDefault = request.IsDefault;
        entity.IsActive = true;
        entity.ModifiedBy = _user.UserId;
        await _templateRepo.UpdateAsync(entity);

        return await _queryRepo.GetTemplateRowAsync(id)
            ?? throw new InvalidOperationException("Template row was not returned after update.");
    }

    public async Task DeleteTemplateAsync(long id)
    {
        var entity = await _templateRepo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Document template '{id}' was not found.");
        if (entity.IsActive == false)
            throw new NotFoundException($"Document template '{id}' was not found.");

        if (await _queryRepo.HasActiveAssignmentsAsync(id))
            throw new DomainException(
                "Cannot delete a template that is assigned to one or more companies. Remove assignments first.", 409);

        await _templateRepo.SoftDeleteAsync(id);
        await _audit.WriteAsync("DocumentTemplate", id.ToString(), "Delete", _user.Username);
    }

    public async Task SetDefaultAsync(long id)
    {
        var entity = await _templateRepo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Document template '{id}' was not found.");
        if (entity.IsActive == false)
            throw new NotFoundException($"Document template '{id}' was not found.");

        var invoiceType = await _lookupRepo.GetInvoiceTypeByIdAsync(entity.InvoiceTypeId);
        if (invoiceType is null || !invoiceType.IsActive)
            throw new NotFoundException("Document type is inactive.");

        await _queryRepo.ClearTemplateDefaultAsync();
        entity.IsDefault = true;
        entity.ModifiedBy = _user.UserId;
        await _templateRepo.UpdateAsync(entity);
    }

    /* ------------------------------------------------------------------ */
    /* Versions + designer save + publish workflow                        */
    /* ------------------------------------------------------------------ */

    public async Task<IEnumerable<DocumentTemplateVersionDto>> GetVersionsAsync(long templateId)
    {
        var items = await _versionRepo.GetByTemplateAsync(templateId);
        return items.Select(MapVersion);
    }

    private static DocumentTemplateVersionDto MapVersion(InvoiceTemplateVersion v) => new()
    {
        TemplateVersionId = v.TemplateVersionId,
        InvoiceTemplateId = v.InvoiceTemplateId,
        VersionNumber = v.VersionNumber,
        Status = v.Status,
        IsPublished = v.IsPublished,
        CreatedAt = v.CreatedAt,
        CreatedBy = v.CreatedBy,
        PublishedBy = v.PublishedBy,
        PublishedAt = v.PublishedAt,
        TemplateJson = v.TemplateJson
    };

    public async Task<DocumentTemplateVersionDto> CreateVersionAsync(long templateId)
    {
        var template = await _templateRepo.GetByIdAsync(templateId)
            ?? throw new NotFoundException($"Document template '{templateId}' was not found.");
        if (template.IsActive == false)
            throw new NotFoundException($"Document template '{templateId}' was not found.");

        if (await _versionRepo.GetLatestDraftAsync(templateId) is { } draft && draft.Status == "DRAFT")
            throw new DomainException("An open draft version already exists. Complete or publish it first.", 409);

        var number = (await _versionRepo.GetLatestAsync(templateId))?.VersionNumber ?? 0;
        var version = new InvoiceTemplateVersion
        {
            InvoiceTemplateId = templateId,
            VersionNumber = number + 1,
            TemplateJson = "{}",
            Status = "DRAFT",
            IsPublished = false,
            CreatedBy = _user.UserId
        };
        await _versionRepo.InsertAsync(version);
        return MapVersion(await _versionRepo.GetByIdAsync(version.TemplateVersionId)
            ?? throw new InvalidOperationException("New version was not returned after insert."));
    }

    public async Task<DesignerVersionDto> GetDesignerAsync(long versionId)
    {
        var version = await _versionRepo.GetByIdAsync(versionId)
            ?? throw new NotFoundException($"Template version '{versionId}' was not found.");
        return await _versionRepo.GetDesignerAsync(versionId);
    }

    public async Task SaveDesignerAsync(long versionId, SaveDesignerVersionRequest request)
    {
        var version = await _versionRepo.GetByIdAsync(versionId)
            ?? throw new NotFoundException($"Template version '{versionId}' was not found.");
        if (version.Status != "DRAFT")
            throw new DomainException("Only DRAFT versions can be edited.", 409);

        if (request.Sections is null || request.Sections.Count == 0)
            throw new DomainException("At least one section is required before saving the design.", 422);

        await _versionRepo.ReplaceDesignerAsync(versionId, request.Sections);
        // Keep the durable TemplateJson snapshot in sync so the design survives
        // refresh / logout even if relational rows were touched directly.
        await _versionRepo.UpdateTemplateJsonAsync(
            versionId, System.Text.Json.JsonSerializer.Serialize(request.Sections));
        await _audit.WriteAsync("DocumentDesign", versionId.ToString(), "Save", _user.Username);
    }

    public async Task<PublishValidationResultDto> ValidatePublishAsync(long versionId)
    {
        var result = new PublishValidationResultDto();
        try
        {
            var version = await _versionRepo.GetByIdAsync(versionId)
                ?? throw new NotFoundException("Template version not found.");
            var template = await _templateRepo.GetByIdAsync(version.InvoiceTemplateId)
                ?? throw new NotFoundException("Owner template not found.");
            if (template.IsActive == false)
                result.Errors.Add("Template is inactive.");
            if (await _lookupRepo.GetInvoiceTypeByIdAsync(template.InvoiceTypeId) is null)
                result.Errors.Add("Document type is missing.");
            if (await _lookupRepo.GetPaperSizeByIdAsync(template.PaperSizeId) is null)
                result.Errors.Add("Paper size is missing.");

            if (result.Errors.Count == 0)
            {
                var design = await _versionRepo.GetDesignerAsync(versionId);
                // includeInactive: a valid design must not fail publish just because
                // its component master row was deactivated after the layout was saved.
                var components = (await _lookupRepo.GetComponentsAsync(includeInactive: true)).ToDictionary(c => c.ComponentId, c => c.ComponentType);
                var errors = ValidateForPublish(template, design, components);
                result.Errors.AddRange(errors);
            }
            result.IsValid = result.Errors.Count == 0;
        }
        catch (NotFoundException)
        {
            result.IsValid = false;
        }
        return result;
    }

    private static List<string> ValidateForPublish(
        InvoiceTemplate template,
        DesignerVersionDto design,
        IReadOnlyDictionary<long, string> componentTypes)
    {
        var errors = new List<string>();

        if (design.Sections.Count(s => s.IsVisible) == 0)
            errors.Add("At least one visible section is required.");

        var anyElement = false;
        var tableCount = 0;
        var mappedFields = 0;
        var knownComponents = componentTypes.Keys.ToHashSet();

        foreach (var section in design.Sections)
        {
            if (!section.IsVisible) continue;
            anyElement = true;
            foreach (var element in section.Elements)
            {
                if (!knownComponents.Contains(element.ComponentId))
                    errors.Add($"Element '{element.ElementName ?? "unnamed"}' references an unknown component.");
                if (element.ComponentId > 0 && componentTypes.TryGetValue(element.ComponentId, out var type))
                {
                    if (type == "TABLE") tableCount++;
                    if (element.ItemColumns.Count > 0) mappedFields += 100;
                }
                mappedFields += element.Fields.Count(f => f.VariableId > 0);

                if (element.ElementType == "TEXT" || element.ElementType == "variable")
                {
                    if (element.Fields.Count == 0 && element.ItemColumns.Count == 0)
                        errors.Add("Element must contain at least one mapped field or item column.");
                }
            }
        }

        if (!anyElement)
            errors.Add("At least one usable element is required in a visible section.");
        if (template.InvoiceTypeId > 0 && tableCount == 0)
            errors.Add("An ITEM_TABLE element is required for this document type.");
        if (errors.Count == 0 && mappedFields == 0)
            errors.Add("Map at least one variable field before publishing.");

        return errors;
    }

    public async Task PublishAsync(long versionId)
    {
        var version = await _versionRepo.GetByIdAsync(versionId)
            ?? throw new NotFoundException($"Template version '{versionId}' was not found.");
        if (version.Status != "DRAFT")
            throw new DomainException("Only draft versions can be published.", 409);

        var template = await _templateRepo.GetByIdAsync(version.InvoiceTemplateId)
            ?? throw new NotFoundException("Owner template was not found.");
        // includeInactive: see ValidatePublishAsync — deactivated component rows
        // must not block publishing or break preview rendering of saved designs.
        var components = (await _lookupRepo.GetComponentsAsync(includeInactive: true)).ToDictionary(c => c.ComponentId, c => c.ComponentType);
        var errors = ValidateForPublish(template, await _versionRepo.GetDesignerAsync(versionId), components);
        if (template.InvoiceTypeId <= 0)
            errors.Add("Document type is missing or inactive.");
        if (template.PaperSizeId <= 0)
            errors.Add("Paper size is missing or inactive.");
        if (errors.Count > 0)
            throw new DomainException(string.Join(" ", errors), 422);

        // Archive any previously published version of this template. It is
        // normal for none to exist (first publish) - only fail if the row
        // update itself misbehaved.
        if (!await _queryRepo.ArchivePublishedVersionAsync(versionId))
        {
            var published = await _versionRepo.GetByTemplateAsync(version.InvoiceTemplateId);
            if (published.Any(v => v.IsPublished && v.TemplateVersionId != versionId))
                throw new DomainException("A previously published version could not be archived.", 409);
        }

        if (!await _queryRepo.MarkVersionPublishedAsync(versionId, _user.UserId))
            throw new DomainException("Publish failed. The version may have been modified concurrently.", 409);

        await _audit.WriteAsync("DocumentDesign", versionId.ToString(), "Publish", _user.Username);
    }

    public async Task<DocumentTemplateVersionDto> CloneToDraftAsync(long versionId)
    {
        var version = await _versionRepo.GetByIdAsync(versionId)
            ?? throw new NotFoundException($"Template version '{versionId}' was not found.");
        var newId = await _versionRepo.CloneToDraftAsync(version.InvoiceTemplateId);
        return MapVersion(await _versionRepo.GetByIdAsync(newId)
            ?? throw new InvalidOperationException("Cloned version was not returned after insert."));
    }

    /// <summary>
    /// Renders the A4 preview HTML for a version. When salesInvoiceId is given
    /// the real invoice is loaded from the database; otherwise sample data is
    /// used (designer preview mode).
    /// </summary>
    public async Task<string> RenderPreviewAsync(long versionId, long? salesInvoiceId)
    {
        var version = await _versionRepo.GetByIdAsync(versionId)
            ?? throw new NotFoundException($"Template version '{versionId}' was not found.");
        var template = await _templateRepo.GetByIdAsync(version.InvoiceTemplateId)
            ?? throw new NotFoundException("Owner template was not found.");
        var design = await _versionRepo.GetDesignerAsync(versionId);
        var components = (await _lookupRepo.GetComponentsAsync(includeInactive: true)).ToDictionary(c => c.ComponentId, c => c.ComponentType);
        var paper = await _lookupRepo.GetPaperSizeByIdAsync(template.PaperSizeId);
        var w = paper?.Width is decimal pw && pw > 0 ? pw : (template.Width is decimal tw && tw > 0 ? tw : 210m);
        var h = paper?.Height is decimal ph && ph > 0 ? ph : (template.Height is decimal th && th > 0 ? th : 297m);

        SalesInvoicePrintDto? printData = null;
        if (salesInvoiceId is long sid && sid > 0)
        {
            printData = await _salesRepo.GetPrintDataAsync(sid)
                ?? throw new NotFoundException($"Sales invoice '{sid}' was not found.");
        }
        return DocumentPreviewHtml.Render(design, components, w, h, printData);
    }

    /* ------------------------------------------------------------------ */
    /* Runtime resolution (assignment -> default template -> published)   */
    /* ------------------------------------------------------------------ */

    public async Task<ResolvedDocumentTemplateDto?> ResolveDefaultAsync(int? companyId, long? invoiceTypeId, long? paperSizeId)
    {
        if (invoiceTypeId is not long invoiceType || invoiceType <= 0)
            throw new DomainException("Document type is required for template resolution.", 400);

        var assignmentTemplateId = await _queryRepo.ResolveTemplateIdAsync(companyId, invoiceType, paperSizeId);
        var templateId = assignmentTemplateId
            ?? (await _lookupRepo.GetDefaultTemplateAsync(invoiceType, paperSizeId))?.InvoiceTemplateId
            ?? 0;

        var template = templateId > 0
            ? await _templateRepo.GetByIdAsync(templateId)
            : null;
        if (template is null || template.IsActive == false)
            throw new NotFoundException("No usable document template found for the requested criteria.");
        if (template.CompanyId is int cid && !await _scopeResolver.CanAccessCompanyAsync(cid))
            throw new DomainException("You do not have permission to use this template.", 403);

        var version = await _versionRepo.GetPublishedAsync(templateId)
            ?? throw new NotFoundException("No published template version exists yet.");
        var design = await _versionRepo.GetDesignerAsync(version.TemplateVersionId);

        return new ResolvedDocumentTemplateDto
        {
            InvoiceTemplateId = template.InvoiceTemplateId,
            TemplateCode = template.Code,
            TemplateName = template.Name,
            TemplateVersionId = version.TemplateVersionId,
            VersionNumber = version.VersionNumber,
            InvoiceTypeId = template.InvoiceTypeId,
            InvoiceTypeName = "",
            PaperSizeId = template.PaperSizeId,
            PaperSizeName = "",
            Width = template.Width,
            Height = template.Height,
            Design = design
        };
    }
}
