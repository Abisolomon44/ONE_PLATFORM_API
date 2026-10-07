using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

/* ============================================================================
   DOCUMENT MASTER - routes: /api/document/master/{types|categories|components|
   variables|fonts|paper-sizes|printer-types|printer-models|orientations|units}
   Permission map (existing invoice-* constants):
     types             -> InvoiceTypes          {view|manage}
     categories        -> InvoiceTemplateCategories
     components        -> InvoiceTemplateComponents
     variables         -> InvoiceTemplateVariables
     fonts             -> InvoiceFonts
     paper-sizes       -> InvoicePaperSizes
     printer-types     -> PrinterTypes
     printer-models    -> PrinterModels
     orientations      -> PrintOrientations
     units             -> PrintUnits           (added)
   ========================================================================= */

public abstract class DocumentMasterControllerBase : ControllerBase
{
    protected readonly IDocumentMasterService _service;
    protected readonly ICurrentUser _user;

    protected DocumentMasterControllerBase(IDocumentMasterService service, ICurrentUser user)
    {
        _service = service;
        _user = user;
    }

    protected abstract string Kind { get; }

    private static readonly string[] Kinds =
    [
        "types", "categories", "components", "variables", "fonts",
        "paper-sizes", "printer-types", "printer-models", "orientations", "units"
    ];

    protected abstract Task<IEnumerable<DocumentMasterRowDto>> GetItemsAsync();
    protected abstract Task<DocumentMasterRowDto> GetItemAsync(long id);
    protected abstract Task<DocumentMasterRowDto> CreateItemAsync(SaveDocumentMasterRequest request);
    protected abstract Task<DocumentMasterRowDto> UpdateItemAsync(long id, SaveDocumentMasterRequest request);
    protected abstract Task DeleteItemAsync(long id);

    private bool HasPermission(string code) =>
        _user.HasPermission(code) || _user.HasPermission(code.Replace("create", "manage").Replace("edit", "manage"));

    private async Task<IActionResult> GateAsync(string code, Func<Task> action)
    {
        if (!HasPermission(code))
            return StatusCode(403, ApiResponse<object>.Fail("Permission denied."));
        await action();
        return Ok(ApiResponse.Ok("Operation completed."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocumentMasterRowDto>), 200)]
    public async Task<ActionResult<IEnumerable<DocumentMasterRowDto>>> GetAll([FromQuery] bool includeInactive = false)
        => Ok(ApiResponse<IEnumerable<DocumentMasterRowDto>>.Ok(await GetItemsAsync()));

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(DocumentMasterRowDto), 200)]
    public async Task<ActionResult<DocumentMasterRowDto>> GetById(long id)
        => Ok(ApiResponse<DocumentMasterRowDto>.Ok(await GetItemAsync(id)));

    [HttpPost]
    [ProducesResponseType(typeof(DocumentMasterRowDto), 200)]
    public async Task<ActionResult<DocumentMasterRowDto>> Create([FromBody] SaveDocumentMasterRequest request)
        => Ok(ApiResponse<DocumentMasterRowDto>.Ok(await CreateItemAsync(request), "Created successfully."));

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(DocumentMasterRowDto), 200)]
    public async Task<ActionResult<DocumentMasterRowDto>> Update(long id, [FromBody] SaveDocumentMasterRequest request)
        => Ok(ApiResponse<DocumentMasterRowDto>.Ok(await UpdateItemAsync(id, request), "Updated successfully."));

    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(void), 200)]
    public async Task<IActionResult> Delete(long id)
    {
        var code = $"document-master.{Kind}.delete";
        if (!HasPermission(code))
            return StatusCode(403, ApiResponse<object>.Fail("Permission denied."));
        await DeleteItemAsync(id);
        return Ok(ApiResponse.Ok("Deleted successfully."));
    }
}

/* ------------------------- /api/document/master/types ------------------------- */

[Authorize]
[Route("api/document/master/types")]
public class DocumentMasterTypesController : DocumentMasterControllerBase
{
    private readonly IInvoiceTypeRepository _repo;

    public DocumentMasterTypesController(IDocumentMasterService service, IInvoiceTypeRepository repo, ICurrentUser user)
        : base(service, user)
    {
        _repo = repo;
    }

    protected override string Kind => "types";

    protected override Task<IEnumerable<DocumentMasterRowDto>> GetItemsAsync() => _service.GetAllAsync(Kind, false);
    protected override Task<DocumentMasterRowDto> GetItemAsync(long id) => _service.GetByIdAsync(Kind, id);
    protected override Task<DocumentMasterRowDto> CreateItemAsync(SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, null, r);
    protected override Task<DocumentMasterRowDto> UpdateItemAsync(long id, SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, id, r);
    protected override Task DeleteItemAsync(long id) => _service.DeleteAsync(Kind, id);
}

/* ------------------------- /api/document/master/categories ------------------------- */

[Authorize]
[Route("api/document/master/categories")]
public class DocumentCategoriesController : DocumentMasterControllerBase
{
    private readonly ITemplateCategoryRepository _repo;

    public DocumentCategoriesController(IDocumentMasterService service, ITemplateCategoryRepository repo, ICurrentUser user)
        : base(service, user)
    {
        _repo = repo;
    }

    protected override string Kind => "categories";

    protected override Task<IEnumerable<DocumentMasterRowDto>> GetItemsAsync() => _service.GetAllAsync(Kind, false);
    protected override Task<DocumentMasterRowDto> GetItemAsync(long id) => _service.GetByIdAsync(Kind, id);
    protected override Task<DocumentMasterRowDto> CreateItemAsync(SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, null, r);
    protected override Task<DocumentMasterRowDto> UpdateItemAsync(long id, SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, id, r);
    protected override Task DeleteItemAsync(long id) => _service.DeleteAsync(Kind, id);
}

/* ------------------------- /api/document/master/components ------------------------- */

[Authorize]
[Route("api/document/master/components")]
public class DocumentComponentsController : DocumentMasterControllerBase
{
    private readonly ITemplateComponentRepository _repo;

    public DocumentComponentsController(IDocumentMasterService service, ITemplateComponentRepository repo, ICurrentUser user)
        : base(service, user)
    {
        _repo = repo;
    }

    protected override string Kind => "components";

    protected override Task<IEnumerable<DocumentMasterRowDto>> GetItemsAsync() => _service.GetAllAsync(Kind, false);
    protected override Task<DocumentMasterRowDto> GetItemAsync(long id) => _service.GetByIdAsync(Kind, id);
    protected override Task<DocumentMasterRowDto> CreateItemAsync(SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, null, r);
    protected override Task<DocumentMasterRowDto> UpdateItemAsync(long id, SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, id, r);
    protected override Task DeleteItemAsync(long id) => _service.DeleteAsync(Kind, id);
}

/* ------------------------- /api/document/master/variables ------------------------- */

[Authorize]
[Route("api/document/master/variables")]
public class DocumentVariablesController : DocumentMasterControllerBase
{
    private readonly ITemplateVariableRepository _repo;

    public DocumentVariablesController(IDocumentMasterService service, ITemplateVariableRepository repo, ICurrentUser user)
        : base(service, user)
    {
        _repo = repo;
    }

    protected override string Kind => "variables";

    protected override Task<IEnumerable<DocumentMasterRowDto>> GetItemsAsync() => _service.GetAllAsync(Kind, false);
    protected override Task<DocumentMasterRowDto> GetItemAsync(long id) => _service.GetByIdAsync(Kind, id);
    protected override Task<DocumentMasterRowDto> CreateItemAsync(SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, null, r);
    protected override Task<DocumentMasterRowDto> UpdateItemAsync(long id, SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, id, r);
    protected override Task DeleteItemAsync(long id) => _service.DeleteAsync(Kind, id);
}

/* ------------------------- /api/document/master/fonts ------------------------- */

[Authorize]
[Route("api/document/master/fonts")]
public class DocumentFontsController : DocumentMasterControllerBase
{
    private readonly IInvoiceFontRepository _repo;

    public DocumentFontsController(IDocumentMasterService service, IInvoiceFontRepository repo, ICurrentUser user)
        : base(service, user)
    {
        _repo = repo;
    }

    protected override string Kind => "fonts";

    protected override Task<IEnumerable<DocumentMasterRowDto>> GetItemsAsync() => _service.GetAllAsync(Kind, false);
    protected override Task<DocumentMasterRowDto> GetItemAsync(long id) => _service.GetByIdAsync(Kind, id);
    protected override Task<DocumentMasterRowDto> CreateItemAsync(SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, null, r);
    protected override Task<DocumentMasterRowDto> UpdateItemAsync(long id, SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, id, r);
    protected override Task DeleteItemAsync(long id) => _service.DeleteAsync(Kind, id);
}

/* ------------------------- /api/document/master/paper-sizes ------------------------- */

[Authorize]
[Route("api/document/master/paper-sizes")]
public class DocumentPaperSizesController : DocumentMasterControllerBase
{
    private readonly IDocumentPaperSizeRepository _repo;

    public DocumentPaperSizesController(IDocumentMasterService service, IDocumentPaperSizeRepository repo, ICurrentUser user)
        : base(service, user)
    {
        _repo = repo;
    }

    protected override string Kind => "paper-sizes";

    protected override Task<IEnumerable<DocumentMasterRowDto>> GetItemsAsync() => _service.GetAllAsync(Kind, false);
    protected override Task<DocumentMasterRowDto> GetItemAsync(long id) => _service.GetByIdAsync(Kind, id);
    protected override Task<DocumentMasterRowDto> CreateItemAsync(SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, null, r);
    protected override Task<DocumentMasterRowDto> UpdateItemAsync(long id, SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, id, r);
    protected override Task DeleteItemAsync(long id) => _service.DeleteAsync(Kind, id);
}

/* ------------------------- /api/document/master/printer-types ------------------------- */

[Authorize]
[Route("api/document/master/printer-types")]
public class DocumentPrinterTypesController : DocumentMasterControllerBase
{
    private readonly IDocumentPrinterTypeRepository _repo;

    public DocumentPrinterTypesController(IDocumentMasterService service, IDocumentPrinterTypeRepository repo, ICurrentUser user)
        : base(service, user)
    {
        _repo = repo;
    }

    protected override string Kind => "printer-types";

    protected override Task<IEnumerable<DocumentMasterRowDto>> GetItemsAsync() => _service.GetAllAsync(Kind, false);
    protected override Task<DocumentMasterRowDto> GetItemAsync(long id) => _service.GetByIdAsync(Kind, id);
    protected override Task<DocumentMasterRowDto> CreateItemAsync(SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, null, r);
    protected override Task<DocumentMasterRowDto> UpdateItemAsync(long id, SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, id, r);
    protected override Task DeleteItemAsync(long id) => _service.DeleteAsync(Kind, id);
}

/* ------------------------- /api/document/master/printer-models ------------------------- */

[Authorize]
[Route("api/document/master/printer-models")]
public class DocumentPrinterModelsController : DocumentMasterControllerBase
{
    private readonly IDocumentPrinterModelRepository _repo;
    private readonly IDocumentPrinterTypeRepository _typeRepo;

    public DocumentPrinterModelsController(
        IDocumentMasterService service,
        IDocumentPrinterModelRepository repo,
        IDocumentPrinterTypeRepository typeRepo,
        ICurrentUser user)
        : base(service, user)
    {
        _repo = repo;
        _typeRepo = typeRepo;
    }

    protected override string Kind => "printer-models";

    protected override Task<IEnumerable<DocumentMasterRowDto>> GetItemsAsync() => _service.GetAllAsync(Kind, false);
    protected override Task<DocumentMasterRowDto> GetItemAsync(long id) => _service.GetByIdAsync(Kind, id);
    protected override async Task<DocumentMasterRowDto> CreateItemAsync(SaveDocumentMasterRequest r)
    {
        await RequirePrinterTypeAsync(r);
        return await _service.SaveAsync(Kind, null, r);
    }
    protected override async Task<DocumentMasterRowDto> UpdateItemAsync(long id, SaveDocumentMasterRequest r)
    {
        await RequirePrinterTypeAsync(r);
        return await _service.SaveAsync(Kind, id, r);
    }

    private async Task RequirePrinterTypeAsync(SaveDocumentMasterRequest r)
    {
        if (r.PrinterTypeId is not long printerTypeId || printerTypeId <= 0
            || await _typeRepo.GetByIdAsync(printerTypeId) is null)
            throw new DomainException("Printer type is required.", 400);
    }
    protected override Task DeleteItemAsync(long id) => _service.DeleteAsync(Kind, id);
}

/* ------------------------- /api/document/master/orientations ------------------------- */

[Authorize]
[Route("api/document/master/orientations")]
public class DocumentOrientationsController : DocumentMasterControllerBase
{
    private readonly IPrintOrientationRepository _repo;

    public DocumentOrientationsController(IDocumentMasterService service, IPrintOrientationRepository repo, ICurrentUser user)
        : base(service, user)
    {
        _repo = repo;
    }

    protected override string Kind => "orientations";

    protected override Task<IEnumerable<DocumentMasterRowDto>> GetItemsAsync() => _service.GetAllAsync(Kind, false);
    protected override Task<DocumentMasterRowDto> GetItemAsync(long id) => _service.GetByIdAsync(Kind, id);
    protected override Task<DocumentMasterRowDto> CreateItemAsync(SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, null, r);
    protected override Task<DocumentMasterRowDto> UpdateItemAsync(long id, SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, id, r);
    protected override Task DeleteItemAsync(long id) => _service.DeleteAsync(Kind, id);
}

/* ------------------------- /api/document/master/units ------------------------- */

[Authorize]
[Route("api/document/master/units")]
public class DocumentUnitsController : DocumentMasterControllerBase
{
    private readonly IPrintUnitRepository _repo;

    public DocumentUnitsController(IDocumentMasterService service, IPrintUnitRepository repo, ICurrentUser user)
        : base(service, user)
    {
        _repo = repo;
    }

    protected override string Kind => "units";

    protected override Task<IEnumerable<DocumentMasterRowDto>> GetItemsAsync() => _service.GetAllAsync(Kind, false);
    protected override Task<DocumentMasterRowDto> GetItemAsync(long id) => _service.GetByIdAsync(Kind, id);
    protected override Task<DocumentMasterRowDto> CreateItemAsync(SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, null, r);
    protected override Task<DocumentMasterRowDto> UpdateItemAsync(long id, SaveDocumentMasterRequest r) => _service.SaveAsync(Kind, id, r);
    protected override Task DeleteItemAsync(long id) => _service.DeleteAsync(Kind, id);
}

/* ============================================================================
   DOCUMENT LOOKUPS - /api/document-lookups/{invoice-types|paper-sizes|...}
   (also serves the old /api/invoice-template-lookups prefix for dropdowns)
   ========================================================================= */

[Authorize]
[Route("api/document-lookups")]
public class DocumentLookupsController : ControllerBase
{
    private readonly IInvoiceTemplateLookupRepository _lookup;

    public DocumentLookupsController(IInvoiceTemplateLookupRepository lookup)
    {
        _lookup = lookup;
    }

    [HttpGet("invoice-types")]
    [ProducesResponseType(typeof(IEnumerable<DocumentLookupDto>), 200)]
    public async Task<ActionResult<IEnumerable<DocumentLookupDto>>> InvoiceTypes([FromQuery] bool includeInactive = false)
        => Ok(await LookupsAsync(
            () => _lookup.GetInvoiceTypesAsync(includeInactive),
            t => new DocumentLookupDto { Id = t.InvoiceTypeId, Code = t.Code, Name = t.Name, SortOrder = t.DisplayOrder, IsActive = t.IsActive }));

    [HttpGet("paper-sizes")]
    [ProducesResponseType(typeof(IEnumerable<DocumentLookupDto>), 200)]
    public async Task<ActionResult<IEnumerable<DocumentLookupDto>>> PaperSizes([FromQuery] bool includeInactive = false)
        => Ok(await LookupsAsync(
            () => _lookup.GetPaperSizesAsync(includeInactive),
            p => new DocumentLookupDto { Id = p.PaperSizeId, Code = p.Code, Name = p.Name, Width = p.Width, Height = p.Height, Unit = p.Unit, IsThermal = p.IsThermal, IsCustom = p.IsCustom, SortOrder = 0, IsActive = p.IsActive }));

    [HttpGet("printer-types")]
    [ProducesResponseType(typeof(IEnumerable<DocumentLookupDto>), 200)]
    public async Task<ActionResult<IEnumerable<DocumentLookupDto>>> PrinterTypes([FromQuery] bool includeInactive = false)
        => Ok(await LookupsAsync(
            () => _lookup.GetPrinterTypesAsync(includeInactive),
            t => new DocumentLookupDto { Id = t.PrinterTypeId, Code = t.Code, Name = t.Name, SortOrder = 0, IsActive = t.IsActive }));

    [HttpGet("printer-models")]
    [ProducesResponseType(typeof(IEnumerable<DocumentLookupDto>), 200)]
    public async Task<ActionResult<IEnumerable<DocumentLookupDto>>> PrinterModels(
        [FromQuery] long? printerTypeId = null,
        [FromQuery] bool includeInactive = false)
        => Ok(await LookupsAsync(
            () => _lookup.GetPrinterModelsAsync(printerTypeId, includeInactive),
            m => new DocumentLookupDto { Id = m.PrinterModelId, Code = m.Code, Name = m.Name, Category = m.Manufacturer, PrinterTypeId = m.PrinterTypeId, SortOrder = 0, IsActive = m.IsActive }));

    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<DocumentLookupDto>), 200)]
    public async Task<ActionResult<IEnumerable<DocumentLookupDto>>> Categories([FromQuery] bool includeInactive = false)
        => Ok(await LookupsAsync(
            () => _lookup.GetCategoriesAsync(includeInactive),
            c => new DocumentLookupDto { Id = c.TemplateCategoryId, Code = c.Code, Name = c.Name, Category = c.Description, SortOrder = c.DisplayOrder, IsActive = c.IsActive }));

    [HttpGet("components")]
    [ProducesResponseType(typeof(IEnumerable<DocumentLookupDto>), 200)]
    public async Task<ActionResult<IEnumerable<DocumentLookupDto>>> Components([FromQuery] bool includeInactive = false)
        => Ok(await LookupsAsync(
            () => _lookup.GetComponentsAsync(includeInactive),
            c => new DocumentLookupDto { Id = c.ComponentId, Code = c.Code, Name = c.Name, Category = c.ComponentType, ComponentType = c.ComponentType, SortOrder = c.DisplayOrder, IsActive = c.IsActive }));

    [HttpGet("variables")]
    [ProducesResponseType(typeof(IEnumerable<DocumentLookupDto>), 200)]
    public async Task<ActionResult<IEnumerable<DocumentLookupDto>>> Variables([FromQuery] bool includeInactive = false)
        => Ok(await LookupsAsync(
            () => _lookup.GetVariablesAsync(includeInactive),
            v => new DocumentLookupDto { Id = v.VariableId, Code = v.Code, Name = v.Name, Category = v.Category, BindingPath = v.BindingPath, DataType = v.DataType, IsCollection = v.IsCollection, SortOrder = 0, IsActive = v.IsActive }));

    [HttpGet("fonts")]
    [ProducesResponseType(typeof(IEnumerable<DocumentLookupDto>), 200)]
    public async Task<ActionResult<IEnumerable<DocumentLookupDto>>> Fonts([FromQuery] bool includeInactive = false)
        => Ok(await LookupsAsync(
            () => _lookup.GetFontsAsync(includeInactive),
            f => new DocumentLookupDto { Id = f.FontId, Code = f.Code, Name = f.Name, FontFamily = f.FontFamily, SortOrder = 0, IsActive = f.IsActive }));

    [HttpGet("orientations")]
    [ProducesResponseType(typeof(IEnumerable<DocumentLookupDto>), 200)]
    public async Task<ActionResult<IEnumerable<DocumentLookupDto>>> Orientations([FromQuery] bool includeInactive = false)
        => Ok(await LookupsAsync(
            () => _lookup.GetOrientationsAsync(includeInactive),
            o => new DocumentLookupDto { Id = o.OrientationId, Code = o.Code, Name = o.Name, SortOrder = 0, IsActive = o.IsActive }));

    [HttpGet("units")]
    [ProducesResponseType(typeof(IEnumerable<DocumentLookupDto>), 200)]
    public async Task<ActionResult<IEnumerable<DocumentLookupDto>>> Units([FromQuery] bool includeInactive = false)
        => Ok(await LookupsAsync(
            () => _lookup.GetUnitsAsync(includeInactive),
            u => new DocumentLookupDto { Id = u.UnitId, Code = u.Code, Name = u.Name, SortOrder = 0, IsActive = u.IsActive }));

    private static async Task<IEnumerable<DocumentLookupDto>> LookupsAsync<T>(
        Func<Task<IEnumerable<T>>> raw,
        Func<T, DocumentLookupDto> map)
    {
        var list = new List<DocumentLookupDto>();
        foreach (var item in await raw()) list.Add(map(item));
        return list;
    }
}

/* ============================================================================
   DOCUMENT TEMPLATES - /api/document-templates (+ legacy /api/invoice-templates)
   ========================================================================= */

[Authorize]
[Route("api/invoice-templates")]
[Route("api/document-templates")]
public class DocumentTemplatesController : ControllerBase
{
    private readonly IDocumentDesignService _design;
    private readonly IDocumentSettingService _setting;
    private readonly IInvoiceTemplateLookupRepository _lookup;
    private readonly ICurrentUser _user;

    public DocumentTemplatesController(
        IDocumentDesignService design,
        IDocumentSettingService setting,
        IInvoiceTemplateLookupRepository lookup,
        ICurrentUser user)
    {
        _design = design;
        _setting = setting;
        _lookup = lookup;
        _user = user;
    }

    private bool HasPermission(string code) => _user.HasPermission(code);

    /* ------------------------------------------------------------------ GET */
    [HttpGet]
    [Permission(Permissions.InvoiceTemplatesView)]
    [ProducesResponseType(typeof(PaginatedResult<DocumentTemplateListItemDto>), 200)]
    public async Task<ActionResult<PaginatedResult<DocumentTemplateListItemDto>>> GetPaged(
        [FromQuery] int page = 1, [FromQuery] int size = 50, [FromQuery] string search = "")
    {
        var result = await _design.GetTemplatesPagedAsync(page, size, search);
        return Ok(ApiResponse<PaginatedResult<DocumentTemplateListItemDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Permission(Permissions.InvoiceTemplatesView)]
    [ProducesResponseType(typeof(DocumentTemplateListItemDto), 200)]
    public async Task<ActionResult<DocumentTemplateListItemDto>> GetById(long id)
    {
        var item = await _design.GetTemplateAsync(id);
        return Ok(ApiResponse<DocumentTemplateListItemDto>.Ok(item));
    }

    /// <summary>Exact-code lookup used by idempotent creation flows ("Create Default
    /// A4 Sales Invoice"): sees inactive + global (CompanyId NULL) rows, matching
    /// the duplicate-code guard, so the hub never 409s on an existing template.</summary>
    [HttpGet("by-code/{code}")]
    [Permission(Permissions.InvoiceTemplatesView)]
    [ProducesResponseType(typeof(DocumentTemplateListItemDto), 200)]
    public async Task<ActionResult<DocumentTemplateListItemDto>> GetByCode(string code)
    {
        var item = await _design.GetTemplateByCodeAsync(code);
        return Ok(ApiResponse<DocumentTemplateListItemDto>.Ok(item));
    }

    /* ------------------------------------------------------------------ CREATE */
    [HttpPost]
    [Permission(Permissions.InvoiceTemplatesCreate)]
    [ProducesResponseType(typeof(DocumentTemplateListItemDto), 200)]
    public async Task<ActionResult<DocumentTemplateListItemDto>> Create([FromBody] SaveDocumentTemplateRequest request)
    {
        var item = await _design.CreateTemplateAsync(request);
        return Ok(ApiResponse<DocumentTemplateListItemDto>.Ok(item, "Document template created successfully."));
    }

    /* ------------------------------------------------------------------ UPDATE */
    [HttpPut("{id:long}")]
    [Permission(Permissions.InvoiceTemplatesEdit)]
    [ProducesResponseType(typeof(DocumentTemplateListItemDto), 200)]
    public async Task<ActionResult<DocumentTemplateListItemDto>> Update(long id, [FromBody] SaveDocumentTemplateRequest request)
    {
        var item = await _design.UpdateTemplateAsync(id, request);
        return Ok(ApiResponse<DocumentTemplateListItemDto>.Ok(item, "Document template updated successfully."));
    }

    /* ------------------------------------------------------------------ DELETE */
    [HttpDelete("{id:long}")]
    [Permission(Permissions.InvoiceTemplatesDelete)]
    public async Task<IActionResult> Delete(long id)
    {
        await _design.DeleteTemplateAsync(id);
        return Ok(ApiResponse.Ok("Document template deleted successfully."));
    }

    /* ------------------------------------------------------------------ SET-DEFAULT */
    [HttpPost("{id:long}/set-default")]
    [Permission(Permissions.InvoiceTemplatesEdit)]
    public async Task<IActionResult> SetDefault(long id)
    {
        await _design.SetDefaultAsync(id);
        return Ok(ApiResponse.Ok("Default template set."));
    }

    /* ------------------------------------------------------------------ VERSIONS */
    [HttpGet("{id:long}/versions")]
    [Permission(Permissions.InvoiceTemplatesView)]
    [ProducesResponseType(typeof(IEnumerable<DocumentTemplateVersionDto>), 200)]
    public async Task<ActionResult<IEnumerable<DocumentTemplateVersionDto>>> GetVersions(long id)
    {
        var list = await _design.GetVersionsAsync(id);
        return Ok(ApiResponse<IEnumerable<DocumentTemplateVersionDto>>.Ok(list));
    }

    [HttpPost("{id:long}/versions")]
    [Permission(Permissions.InvoiceTemplatesCreate)]
    [ProducesResponseType(typeof(DocumentTemplateVersionDto), 200)]
    public async Task<ActionResult<DocumentTemplateVersionDto>> CreateVersion(long id)
    {
        var item = await _design.CreateVersionAsync(id);
        return Ok(ApiResponse<DocumentTemplateVersionDto>.Ok(item, "New draft version created."));
    }

    [HttpGet("versions/{versionId:long}/designer")]
    [Permission(Permissions.InvoiceTemplatesView)]
    [ProducesResponseType(typeof(DesignerVersionDto), 200)]
    public async Task<ActionResult<DesignerVersionDto>> GetDesigner(long versionId)
    {
        var item = await _design.GetDesignerAsync(versionId);
        return Ok(ApiResponse<DesignerVersionDto>.Ok(item));
    }

    [HttpPut("versions/{versionId:long}/designer")]
    [Permission(Permissions.InvoiceTemplatesEdit)]
    [ProducesResponseType(typeof(void), 200)]
    public async Task<IActionResult> SaveDesigner(long versionId, [FromBody] SaveDesignerVersionRequest request)
    {
        await _design.SaveDesignerAsync(versionId, request);
        return Ok(ApiResponse.Ok("Design saved."));
    }

    /// <summary>GET preview of the saved designer version with sales, purchase, purchase-return, or debit-note data.</summary>
    [HttpGet("versions/{versionId:long}/preview")]
    [Permission(Permissions.InvoiceTemplatesPreview)]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<ActionResult<string>> Preview(long versionId, [FromQuery] long? salesInvoiceId = null,
        [FromQuery] long? purchaseId = null, [FromQuery] long? purchaseReturnId = null, [FromQuery] bool debitNote = false)
    {
        if ((salesInvoiceId.HasValue ? 1 : 0) + (purchaseId.HasValue ? 1 : 0) + (purchaseReturnId.HasValue ? 1 : 0) > 1)
            return BadRequest(ApiResponse<string>.Fail("Specify only one transaction to preview."));
        var html = await _design.RenderPreviewAsync(versionId, salesInvoiceId, purchaseId, purchaseReturnId, debitNote);
        return Ok(ApiResponse<string>.Ok(html));
    }

    [HttpPost("versions/{versionId:long}/publish")]
    [Permission(Permissions.InvoiceTemplatesPublish)]
    [ProducesResponseType(typeof(void), 200)]
    public async Task<IActionResult> Publish(long versionId)
    {
        await _design.PublishAsync(versionId);
        return Ok(ApiResponse.Ok("Template published successfully."));
    }

    [HttpPost("versions/{versionId:long}/clone")]
    [Permission(Permissions.InvoiceTemplatesCreate)]
    [ProducesResponseType(typeof(DocumentTemplateVersionDto), 200)]
    public async Task<ActionResult<DocumentTemplateVersionDto>> CloneToDraft(long versionId)
    {
        var item = await _design.CloneToDraftAsync(versionId);
        return Ok(ApiResponse<DocumentTemplateVersionDto>.Ok(item, "Draft cloned from published version."));
    }

    /* ------------------------------------------------------------------ PRINTERS */
    [HttpGet("templates/{templateId:long}/printers")]
    [Permission(Permissions.InvoiceTemplatesView)]
    [ProducesResponseType(typeof(IEnumerable<TemplatePrinterDto>), 200)]
    public async Task<ActionResult<IEnumerable<TemplatePrinterDto>>> GetPrinters(long templateId)
    {
        var list = await _setting.GetPrintersAsync(templateId);
        return Ok(ApiResponse<IEnumerable<TemplatePrinterDto>>.Ok(list));
    }

    [HttpPost("templates/{templateId:long}/printers")]
    [Permission(Permissions.InvoiceTemplatesAssign)]
    [ProducesResponseType(typeof(TemplatePrinterDto), 200)]
    public async Task<ActionResult<TemplatePrinterDto>> SavePrinter(long templateId, [FromBody] SaveTemplatePrinterRequest request)
    {
        var item = await _setting.SavePrinterAsync(request with { InvoiceTemplateId = templateId });
        return Ok(ApiResponse<TemplatePrinterDto>.Ok(item, "Printer configuration saved."));
    }

    [HttpPut("templates/{templateId:long}/printers/{printerId:long}")]
    [Permission(Permissions.InvoiceTemplatesEdit)]
    [ProducesResponseType(typeof(TemplatePrinterDto), 200)]
    public async Task<ActionResult<TemplatePrinterDto>> UpdatePrinter(long templateId, long printerId, [FromBody] SaveTemplatePrinterRequest request)
    {
        var item = await _setting.SavePrinterAsync(request with { InvoiceTemplateId = templateId }, printerId);
        return Ok(ApiResponse<TemplatePrinterDto>.Ok(item, "Printer configuration updated."));
    }

    [HttpDelete("templates/{templateId:long}/printers/{printerId:long}")]
    [Permission(Permissions.InvoiceTemplatesEdit)]
    public async Task<IActionResult> DeletePrinter(long templateId, long printerId)
    {
        await _setting.DeletePrinterAsync(printerId);
        return Ok(ApiResponse.Ok("Printer configuration deleted."));
    }

    [HttpPut("templates/{templateId:long}/printers/{printerId:long}/print-setting")]
    [Permission(Permissions.InvoiceTemplatesEdit)]
    [ProducesResponseType(typeof(InvoiceTemplatePrintSettingDto), 200)]
    public async Task<ActionResult<InvoiceTemplatePrintSettingDto>> UpdatePrintSetting(
        long templateId, long printerId, [FromBody] InvoiceTemplatePrintSettingDto request)
    {
        var saved = await _setting.UpdatePrintSettingAsync(templateId, printerId, request);
        return Ok(ApiResponse<InvoiceTemplatePrintSettingDto>.Ok(saved, "Print settings saved."));
    }

    /* ------------------------------------------------------------------ ASSIGNMENTS */
    [HttpGet("{templateId:long}/assignments")]
    [Permission(Permissions.InvoiceTemplatesView)]
    [ProducesResponseType(typeof(IEnumerable<TemplateAssignmentDto>), 200)]
    public async Task<ActionResult<IEnumerable<TemplateAssignmentDto>>> GetAssignments(long templateId)
    {
        var list = await _setting.GetAssignmentsAsync();
        return Ok(ApiResponse<IEnumerable<TemplateAssignmentDto>>.Ok(list));
    }

    [HttpPost("{templateId:long}/assignments")]
    [Permission(Permissions.InvoiceTemplatesAssign)]
    [ProducesResponseType(typeof(TemplateAssignmentDto), 201)]
    public async Task<ActionResult<TemplateAssignmentDto>> CreateAssignment(long templateId, [FromBody] CreateTemplateAssignmentRequest request)
    {
        var item = await _setting.CreateAssignmentAsync(request with { InvoiceTemplateId = templateId });
        return Ok(ApiResponse<TemplateAssignmentDto>.Ok(item, "Assignment saved."));
    }

    [HttpDelete("assignments/{assignmentId:long}")]
    [Permission(Permissions.InvoiceTemplatesAssign)]
    public async Task<IActionResult> DeleteAssignment(long assignmentId)
    {
        await _setting.DeleteAssignmentAsync(assignmentId);
        return Ok(ApiResponse.Ok("Assignment deleted."));
    }

    /* ------------------------------------------------------------------ RUNTIME RESOLUTION */
    [HttpGet("resolve")]
    [Permission(Permissions.InvoiceTemplatesView)]
    [ProducesResponseType(typeof(ResolvedDocumentTemplateDto), 200)]
    public async Task<ActionResult<ResolvedDocumentTemplateDto>> Resolve(
        [FromQuery] int? companyId,
        [FromQuery] long? invoiceTypeId,
        [FromQuery] long? paperSizeId)
    {
        var item = await _design.ResolveDefaultAsync(companyId, invoiceTypeId, paperSizeId)
            ?? throw new NotFoundException("No default template found for the given criteria.");
        return Ok(ApiResponse<ResolvedDocumentTemplateDto>.Ok(item));
    }
}

/* ============================================================================
   DOCUMENT SETTINGS - /api/document-settings
   ========================================================================= */

[Authorize]
[Route("api/document-settings")]
public class DocumentSettingsController : ControllerBase
{
    private readonly IDocumentSettingService _setting;
    private readonly ICurrentUser _user;

    public DocumentSettingsController(IDocumentSettingService setting, ICurrentUser user)
    {
        _setting = setting;
        _user = user;
    }

    private bool HasPermission(string code) => _user.HasPermission(code);

    [HttpGet("assignments")]
    [Permission(Permissions.InvoiceTemplatesView)]
    [ProducesResponseType(typeof(IEnumerable<TemplateAssignmentDto>), 200)]
    public async Task<ActionResult<IEnumerable<TemplateAssignmentDto>>> GetAssignments()
    {
        var list = await _setting.GetAssignmentsAsync();
        return Ok(ApiResponse<IEnumerable<TemplateAssignmentDto>>.Ok(list));
    }

    [HttpPost("assignments")]
    [Permission(Permissions.InvoiceTemplatesAssign)]
    [ProducesResponseType(typeof(TemplateAssignmentDto), 200)]
    public async Task<ActionResult<TemplateAssignmentDto>> CreateAssignment([FromBody] CreateTemplateAssignmentRequest request)
    {
        var item = await _setting.CreateAssignmentAsync(request);
        return Ok(ApiResponse<TemplateAssignmentDto>.Ok(item, "Assignment saved."));
    }

    [HttpDelete("assignments/{assignmentId:long}")]
    [Permission(Permissions.InvoiceTemplatesAssign)]
    public async Task<IActionResult> DeleteAssignment(long assignmentId)
    {
        await _setting.DeleteAssignmentAsync(assignmentId);
        return Ok(ApiResponse.Ok("Assignment deleted."));
    }

    [HttpGet("resolve")]
    [Permission(Permissions.InvoiceTemplatesView)]
    [ProducesResponseType(typeof(ResolvedDocumentTemplateDto), 200)]
    public async Task<ActionResult<ResolvedDocumentTemplateDto>> Resolve(
        [FromQuery] int? companyId,
        [FromQuery] long? invoiceTypeId,
        [FromQuery] long? paperSizeId)
    {
        var item = await _setting.ResolveDefaultAsync(companyId, invoiceTypeId, paperSizeId)
            ?? throw new NotFoundException("No usable document template found.");
        return Ok(ApiResponse<ResolvedDocumentTemplateDto>.Ok(item));
    }
}
