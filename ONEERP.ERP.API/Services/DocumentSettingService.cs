using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

/* ============================================================================
   DOCUMENT SETTING (Screen 3) service: template assignments, printer
   configuration with print settings and the runtime default-template
   resolution used by sales/POS/purchase printing flows.
   Works on the EXISTING Invoice* tables only.
   ============================================================================ */

public interface IDocumentSettingService
{
    Task<IEnumerable<TemplateAssignmentDto>> GetAssignmentsAsync();
    Task<TemplateAssignmentDto> CreateAssignmentAsync(CreateTemplateAssignmentRequest request);
    Task DeleteAssignmentAsync(long assignmentId);
    Task<IEnumerable<TemplatePrinterDto>> GetPrintersAsync(long templateId);
         Task<TemplatePrinterDto> SavePrinterAsync(SaveTemplatePrinterRequest request, long? templatePrinterId = null);
         Task DeletePrinterAsync(long templatePrinterId);
         Task<InvoiceTemplatePrintSettingDto> UpdatePrintSettingAsync(long templateId, long printerId, InvoiceTemplatePrintSettingDto setting);
    Task<ResolvedDocumentTemplateDto?> ResolveDefaultAsync(int? companyId, long? invoiceTypeId, long? paperSizeId);
}

public class DocumentSettingService : IDocumentSettingService
{
    private readonly IInvoiceTemplateRepository _templateRepo;
    private readonly IInvoiceTemplateAssignmentRepository _assignmentRepo;
    private readonly IInvoiceTemplatePrinterRepository _printerRepo;
    private readonly IInvoiceTemplateVersionRepository _versionRepo;
    private readonly IInvoiceTemplateLookupRepository _lookupRepo;
    private readonly IDocumentQueryRepository _queryRepo;
    private readonly IDataScopeResolver _scopeResolver;
    private readonly ICurrentUser _user;
    private readonly IAuditService _audit;

    public DocumentSettingService(
        IInvoiceTemplateRepository templateRepo,
        IInvoiceTemplateAssignmentRepository assignmentRepo,
        IInvoiceTemplatePrinterRepository printerRepo,
        IInvoiceTemplateVersionRepository versionRepo,
        IInvoiceTemplateLookupRepository lookupRepo,
        IDocumentQueryRepository queryRepo,
        IDataScopeResolver scopeResolver,
        ICurrentUser user,
        IAuditService audit)
    {
        _templateRepo = templateRepo;
        _assignmentRepo = assignmentRepo;
        _printerRepo = printerRepo;
        _versionRepo = versionRepo;
        _lookupRepo = lookupRepo;
        _queryRepo = queryRepo;
        _scopeResolver = scopeResolver;
        _user = user;
        _audit = audit;
    }

    /* ------------------------------------------------------------------ */
    /* Template assignments                                               */
    /* ------------------------------------------------------------------ */

    public async Task<IEnumerable<TemplateAssignmentDto>> GetAssignmentsAsync()
    {
        var allowed = await _scopeResolver.GetAllowedCompanyIdsAsync();
        return (await _queryRepo.GetAssignmentsAsync(null)).Where(a =>
            allowed == null || a.CompanyId == null || allowed.Contains(a.CompanyId.Value));
    }

    public async Task<TemplateAssignmentDto> CreateAssignmentAsync(CreateTemplateAssignmentRequest request)
    {
        var templateId = request.InvoiceTemplateId > 0 ? request.InvoiceTemplateId : 0;
        var template = await _templateRepo.GetByIdAsync(templateId)
            ?? throw new NotFoundException($"Document template {templateId} was not found in this database.");
        if (template.IsActive == false)
            throw new NotFoundException($"Document template {templateId} is inactive.");

        if (template.CompanyId is int cid && !await _scopeResolver.CanAccessCompanyAsync(cid))
            throw new DomainException("You do not have permission to use this template.", 403);

        var hasScope = request.CompanyId is not null || request.IndustryTypeId is not null
                       || request.InvoiceTypeId is not null || request.PaperSizeId is not null;
        if (!hasScope)
            throw new DomainException("Assignment must specify at least one scope criterion (company, industry, document type or paper size).", 400);

        if (request.CompanyId is int reqCid && !await _scopeResolver.CanAccessCompanyAsync(reqCid))
            throw new DomainException("Selected company is not accessible.", 403);

        if (request.IndustryTypeId is int ind && (await _lookupRepo.GetIndustryTypeByIdAsync(ind)) is null)
            throw new DomainException("Invalid industry type.", 400);
        if (request.InvoiceTypeId is long t && (await _lookupRepo.GetInvoiceTypeByIdAsync(t)) is null)
            throw new DomainException("Invalid document type (InvoiceTypeId).", 400);
        if (request.PaperSizeId is long ps && (await _lookupRepo.GetPaperSizeByIdAsync(ps)) is null)
            throw new DomainException("Invalid paper size (PaperSizeId).", 400);

        if (await _queryRepo.FindActiveAssignmentAsync(request.InvoiceTemplateId, request.CompanyId, request.IndustryTypeId, request.InvoiceTypeId, request.PaperSizeId) is long existing)
            throw new DomainException($"An active assignment for this scope already exists (AssignmentId={existing}).", 409);

        if (request.IsDefault)
            await _queryRepo.ClearTemplateDefaultAsync();

        var entity = new InvoiceTemplateAssignment
        {
            InvoiceTemplateId = templateId,
            CompanyId = request.CompanyId,
            IndustryTypeId = request.IndustryTypeId,
            InvoiceTypeId = request.InvoiceTypeId,
            PaperSizeId = request.PaperSizeId,
            IsDefault = request.IsDefault,
            IsActive = request.IsActive,
            CreatedBy = _user.UserId
        };
        entity.AssignmentId = await _assignmentRepo.InsertAsync(entity);

        await _audit.WriteAsync("DocumentAssignment", entity.AssignmentId.ToString(), "Create", _user.Username);
        return MapAssignment(entity);
    }

    public async Task DeleteAssignmentAsync(long assignmentId)
    {
        var entity = await _assignmentRepo.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException($"Document assignment '{assignmentId}' was not found.");
        await _assignmentRepo.DeleteAsync(assignmentId);
        await _audit.WriteAsync("DocumentAssignment", assignmentId.ToString(), "Delete", _user.Username);
    }

    /* ------------------------------------------------------------------ */
    /* Printers + print settings                                          */
    /* ------------------------------------------------------------------ */

    public async Task<IEnumerable<TemplatePrinterDto>> GetPrintersAsync(long templateId)
    {
        var template = await _templateRepo.GetByIdAsync(templateId)
            ?? throw new NotFoundException($"Document template '{templateId}' was not found.");
        if (template.IsActive == false)
            throw new NotFoundException($"Document template '{templateId}' was not found.");
        if (template.CompanyId is int cid && !await _scopeResolver.CanAccessCompanyAsync(cid))
            throw new DomainException("You do not have permission to use this template.", 403);

        var result = new List<TemplatePrinterDto>();
        foreach (var p in await _printerRepo.GetByTemplateAsync(templateId))
        {
            var dto = new TemplatePrinterDto
            {
                TemplatePrinterId = p.TemplatePrinterId,
                InvoiceTemplateId = p.InvoiceTemplateId,
                PaperSizeId = p.PaperSizeId,
                PrinterTypeId = p.PrinterTypeId,
                PrinterModelId = p.PrinterModelId,
                PrinterName = p.PrinterName,
                IsDefault = p.IsDefault,
                IsActive = p.IsActive,
            };
            dto.PrintSetting = await _printerRepo.GetSettingAsync(p.TemplatePrinterId) is { } setting
                ? MapPrintSetting(setting)
                : null;
            if (dto.PrintSetting is null)
            {
                dto.PrintSetting = new InvoiceTemplatePrintSettingDto
                {
                    Scale = 100, Copies = 1, AutoFit = true, CutPaper = false,
                    PrintHeader = true, PrintFooter = true
                };
            }
            result.Add(dto);
        }
        return result;
    }

    public async Task<InvoiceTemplatePrintSettingDto> UpdatePrintSettingAsync(long templateId, long printerId, InvoiceTemplatePrintSettingDto setting)
    {
        var template = await _templateRepo.GetByIdAsync(templateId)
            ?? throw new NotFoundException($"Document template {templateId} was not found in this database.");
        if (template.IsActive == false)
            throw new NotFoundException($"Document template {templateId} is inactive.");
        if (template.CompanyId is int cid && !await _scopeResolver.CanAccessCompanyAsync(cid))
            throw new DomainException("You do not have permission to use this template.", 403);

        var printer = await _printerRepo.GetByIdAsync(printerId)
            ?? throw new NotFoundException($"Printer configuration '{printerId}' was not found.");
        if (printer.InvoiceTemplateId != templateId)
            throw new DomainException("Printer configuration does not belong to this template.", 400);

        setting.TemplatePrinterId = printerId;
        if (setting.PrintSettingId is null or <= 0)
            setting.PrintSettingId = (await _printerRepo.GetSettingAsync(printerId))?.PrintSettingId;

        await _queryRepo.UpsertPrintSettingAsync(setting);
        await _audit.WriteAsync("TemplatePrinterPrintSetting", printerId.ToString(), "Save", _user.Username);
        return setting;
    }

    private static InvoiceTemplatePrintSettingDto MapPrintSetting(InvoiceTemplatePrintSetting s) => new()
    {
        PrintSettingId = s.PrintSettingId,
        TemplatePrinterId = s.TemplatePrinterId,
        MarginTop = s.MarginTop,
        MarginRight = s.MarginRight,
        MarginBottom = s.MarginBottom,
        MarginLeft = s.MarginLeft,
        Scale = s.Scale,
        Copies = s.Copies,
        AutoFit = s.AutoFit,
        CutPaper = s.CutPaper,
        PrintHeader = s.PrintHeader,
        PrintFooter = s.PrintFooter
    };

    public async Task DeletePrinterAsync(long templatePrinterId)
    {
        var entity = await _printerRepo.GetByIdAsync(templatePrinterId)
            ?? throw new NotFoundException($"Printer configuration '{templatePrinterId}' was not found.");
        await _printerRepo.DeleteAsync(templatePrinterId);
        await _audit.WriteAsync("TemplatePrinter", templatePrinterId.ToString(), "Delete", _user.Username);
    }

    public async Task<TemplatePrinterDto> SavePrinterAsync(SaveTemplatePrinterRequest request, long? printerIdToUpdate = null)
    {
        var templateId = request.InvoiceTemplateId > 0 ? request.InvoiceTemplateId : 0;
        var template = await _templateRepo.GetByIdAsync(templateId)
            ?? throw new NotFoundException($"Document template {templateId} was not found in this database.");
        if (template.IsActive == false)
            throw new NotFoundException($"Document template {templateId} is inactive.");
        if (template.CompanyId is int cid && !await _scopeResolver.CanAccessCompanyAsync(cid))
            throw new DomainException("You do not have permission to use this template.", 403);

        if (request.PaperSizeId <= 0)
            throw new DomainException("Paper size is required.", 400);
        if (request.PrinterTypeId <= 0)
            throw new DomainException("Printer type is required.", 400);

        if (request.PrinterModelId is long modelId && modelId > 0)
        {
            var model = await _lookupRepo.GetPrinterModelByIdAsync(modelId)
                ?? throw new DomainException("Selected printer model is invalid for this type.", 400);
            if (model.PrinterTypeId != request.PrinterTypeId)
                throw new DomainException("Selected printer model is not valid for the chosen printer type.", 400);
        }
        if (await _lookupRepo.GetPaperSizeByIdAsync(request.PaperSizeId) is null)
            throw new DomainException("Selected paper size is invalid.", 400);

        if (request.IsDefault)
            await _queryRepo.ClearPrinterDefaultAsync(templateId);

        long foundPrinterId = 0;
        if (printerIdToUpdate is long requestedId && requestedId > 0)
        {
            var current = await _printerRepo.GetByIdAsync(requestedId)
                ?? throw new NotFoundException($"Printer configuration '{requestedId}' was not found.");
            if (current.InvoiceTemplateId != templateId)
                throw new DomainException("Printer configuration does not belong to this template.", 400);
            foundPrinterId = requestedId;
        }
        else
        {
            foundPrinterId = await _queryRepo.FindPrinterAsync(templateId, request.PaperSizeId, request.PrinterTypeId) ?? 0;
        }

        long templatePrinterId = 0;
        if (foundPrinterId == 0)
        {
            var entity = new InvoiceTemplatePrinter
            {
                InvoiceTemplateId = templateId,
                PaperSizeId = request.PaperSizeId,
                PrinterTypeId = request.PrinterTypeId,
                PrinterModelId = request.PrinterModelId,
                PrinterName = request.PrinterName,
                IsDefault = request.IsDefault,
                IsActive = true,
                CreatedBy = _user.UserId
            };
            templatePrinterId = await _printerRepo.InsertAsync(entity);
        }
        else
        {
            templatePrinterId = foundPrinterId;
            var entity = await _printerRepo.GetByIdAsync(foundPrinterId)
                ?? throw new InvalidOperationException("Printer was not returned after find.");
            entity.PaperSizeId = request.PaperSizeId;
            entity.PrinterTypeId = request.PrinterTypeId;
            entity.PrinterModelId = request.PrinterModelId;
            entity.PrinterName = request.PrinterName;
            entity.IsDefault = request.IsDefault;
            entity.IsActive = true;
            entity.ModifiedBy = _user.UserId;
            await _printerRepo.UpdateAsync(entity);
        }

        // Upsert the print settings for this printer.
        if (request.PrintSetting is not null)
        {
            request.PrintSetting.TemplatePrinterId = templatePrinterId;
            await _queryRepo.UpsertPrintSettingAsync(request.PrintSetting);
        }

        await _audit.WriteAsync("TemplatePrinter", templatePrinterId.ToString(), "Save", _user.Username);
        return (await _queryRepo.GetPrinterRowAsync(templatePrinterId))
            ?? throw new InvalidOperationException("Printer row was not returned after save.");
    }

    /* ------------------------------------------------------------------ */
    /* Runtime resolution                                                 */
    /* ------------------------------------------------------------------ */

    public async Task<ResolvedDocumentTemplateDto?> ResolveDefaultAsync(int? companyId, long? invoiceTypeId, long? paperSizeId)
    {
        if (invoiceTypeId is not long invoiceType || invoiceType <= 0)
            throw new DomainException("Document type is required for template resolution.", 400);

        var assignmentTemplateId = await _queryRepo.ResolveTemplateIdAsync(companyId, invoiceType, paperSizeId);
        var templateId = assignmentTemplateId
            ?? (await _lookupRepo.GetDefaultTemplateAsync(invoiceType, paperSizeId))?.InvoiceTemplateId
            ?? 0;

        if (templateId <= 0)
            throw new NotFoundException("No usable document template found for the requested criteria.");

        var template = await _templateRepo.GetByIdAsync(templateId)
            ?? throw new NotFoundException("Resolved template was not returned.");
        if (template.IsActive == false)
            throw new NotFoundException("Resolved template is inactive.");
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

    private static TemplateAssignmentDto MapAssignment(InvoiceTemplateAssignment a) => new()
    {
        AssignmentId = a.AssignmentId,
        InvoiceTemplateId = a.InvoiceTemplateId,
        CompanyId = a.CompanyId,
        IndustryTypeId = a.IndustryTypeId,
        InvoiceTypeId = a.InvoiceTypeId,
        PaperSizeId = a.PaperSizeId,
        IsDefault = a.IsDefault,
        IsActive = a.IsActive
    };
}
