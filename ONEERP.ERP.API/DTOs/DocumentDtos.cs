namespace ONEERP.ERP.API.DTOs;

/* ---------------------------------------------------------------------------
   DOCUMENT module DTOs. The UI contract already lives in
   ONEERP.ERP.UI/src/app/core/services/invoice-template.service.ts - these
   DTOs mirror it 1:1 (camelCase JSON via Program.cs naming policy).
   --------------------------------------------------------------------------- */

/* ------------------------- Document Master (10 tabs) ------------------------- */

public class DocumentMasterRowDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string? ComponentType { get; set; }
    public string? BindingPath { get; set; }
    public string? DataType { get; set; }
    public string? Category { get; set; }
    public bool IsCollection { get; set; }
    public string? FontFamily { get; set; }
    public long? FontFileId { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? Unit { get; set; }
    public bool IsThermal { get; set; }
    public bool IsCustom { get; set; }
    public long? PrinterTypeId { get; set; }
    public string? PrinterTypeName { get; set; }
    public string? Manufacturer { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public record SaveDocumentMasterRequest(
    string Code,
    string Name,
    string? Description,
    int DisplayOrder,
    string? ComponentType,
    string? BindingPath,
    string? DataType,
    string? Category,
    bool IsCollection,
    string? FontFamily,
    long? FontFileId,
    decimal? Width,
    decimal? Height,
    string? Unit,
    bool IsThermal,
    bool IsCustom,
    long? PrinterTypeId,
    string? Manufacturer,
    bool IsActive);

/* ------------------------- Designer lookups ------------------------- */

public class DocumentLookupDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public string? Category { get; set; }
    public string? ComponentType { get; set; }
    public string? BindingPath { get; set; }
    public string? DataType { get; set; }
    public bool IsCollection { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? Unit { get; set; }
    public bool IsThermal { get; set; }
    public bool IsCustom { get; set; }
    public string? FontFamily { get; set; }
    public long? PrinterTypeId { get; set; }
}

public class DocumentLookupsDto
{
    public List<DocumentLookupDto> InvoiceTypes { get; set; } = new();
    public List<DocumentLookupDto> Categories { get; set; } = new();
    public List<DocumentLookupDto> Components { get; set; } = new();
    public List<DocumentLookupDto> Variables { get; set; } = new();
    public List<DocumentLookupDto> Fonts { get; set; } = new();
    public List<DocumentLookupDto> PaperSizes { get; set; } = new();
    public List<DocumentLookupDto> PrinterTypes { get; set; } = new();
    public List<DocumentLookupDto> PrinterModels { get; set; } = new();
    public List<DocumentLookupDto> Orientations { get; set; } = new();
    public List<DocumentLookupDto> Units { get; set; } = new();
}

/* ------------------------- Templates (header CRUD) ------------------------- */

public class DocumentTemplateListItemDto
{
    public long InvoiceTemplateId { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public long? TemplateCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public long InvoiceTypeId { get; set; }
    public string InvoiceTypeName { get; set; } = string.Empty;
    public long PaperSizeId { get; set; }
    public string PaperSizeName { get; set; } = string.Empty;
    public long OrientationId { get; set; }
    public string OrientationName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public int LatestVersionNumber { get; set; }
    public string LatestStatus { get; set; } = string.Empty;
    public bool HasPublishedVersion { get; set; }
}

public record SaveDocumentTemplateRequest(
    int? CompanyId,
    long? TemplateCategoryId,
    long InvoiceTypeId,
    long PaperSizeId,
    long OrientationId,
    string Code,
    string Name,
    string? Description,
    decimal? Width,
    decimal? Height,
    bool IsDefault = false,
    bool IsActive = true);

public class DocumentTemplateVersionDto
{
    public long TemplateVersionId { get; set; }
    public long InvoiceTemplateId { get; set; }
    public int VersionNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public string? TemplateJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? PublishedBy { get; set; }
    public DateTime? PublishedAt { get; set; }
}

/* Designer payload DTOs (DesignerStyleDto, DesignerFieldDto, DesignerItemColumnDto,
   DesignerElementDto, DesignerSectionDto, DesignerVersionDto, SaveDesignerVersionRequest),
   printer DTOs (TemplatePrinterDto, InvoiceTemplatePrintSettingDto, SaveTemplatePrinterRequest)
   and assignment DTOs (TemplateAssignmentDto, CreateTemplateAssignmentRequest) are defined in
   InvoiceTemplateDtos.cs and reused here via the same namespace. */

public class PublishValidationResultDto
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

/* ------------------------- Runtime resolution ------------------------- */

public class ResolvedDocumentTemplateDto
{
    public long InvoiceTemplateId { get; set; }
    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public long TemplateVersionId { get; set; }
    public int VersionNumber { get; set; }
    public long InvoiceTypeId { get; set; }
    public string InvoiceTypeName { get; set; } = string.Empty;
    public long PaperSizeId { get; set; }
    public string PaperSizeName { get; set; } = string.Empty;
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public DesignerVersionDto? Design { get; set; }
}
