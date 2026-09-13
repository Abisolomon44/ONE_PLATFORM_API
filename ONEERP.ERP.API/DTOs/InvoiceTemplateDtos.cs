namespace ONEERP.ERP.API.DTOs;

/* ---------------------------------------------------------------------------
   Invoice Template Design - DTOs (one common designer for all document types)
   --------------------------------------------------------------------------- */

public class InvoiceTemplateListItemDto
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

public record CreateInvoiceTemplateRequest(
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

public record UpdateInvoiceTemplateRequest(
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

public class InvoiceTemplateVersionDto
{
    public long TemplateVersionId { get; set; }
    public long InvoiceTemplateId { get; set; }
    public int VersionNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public string? TemplateJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? PublishedBy { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime? PublishedAt { get; set; }
}

/* ---------------------------------------------------------------------------
   Designer - hierarchical version payload (sections -> elements -> fields /
   item columns / style). Round-trips between the designer UI and the API and
   is also stored (serialized) as the immutable published TemplateJson.
   --------------------------------------------------------------------------- */
public class DesignerStyleDto
{
    public long? FontId { get; set; }
    public decimal? FontSize { get; set; }
    public string? FontWeight { get; set; }
    public string? TextAlign { get; set; }
    public string? VerticalAlign { get; set; }
    public decimal? PaddingTop { get; set; }
    public decimal? PaddingRight { get; set; }
    public decimal? PaddingBottom { get; set; }
    public decimal? PaddingLeft { get; set; }
    public bool BorderTop { get; set; }
    public bool BorderRight { get; set; }
    public bool BorderBottom { get; set; }
    public bool BorderLeft { get; set; }
}

public class DesignerFieldDto
{
    public long? TemplateFieldId { get; set; }
    public long VariableId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string BindingPath { get; set; } = string.Empty;
    public string? Label { get; set; }
    public bool IsVisible { get; set; } = true;
}

public class DesignerItemColumnDto
{
    public long? ItemColumnId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string HeaderText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public decimal? Width { get; set; }
    public string Alignment { get; set; } = "LEFT";
    public bool IsVisible { get; set; } = true;
}

public class DesignerElementDto
{
    public long? ElementId { get; set; }
    public long ComponentId { get; set; }
    public string? ComponentCode { get; set; }
    public string ElementType { get; set; } = string.Empty;
    public string? ElementName { get; set; }
    public decimal? X { get; set; }
    public decimal? Y { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;
    public List<DesignerFieldDto> Fields { get; set; } = new();
    public List<DesignerItemColumnDto> ItemColumns { get; set; } = new();
    public DesignerStyleDto? Style { get; set; }
}

public class DesignerSectionDto
{
    public long? SectionId { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public decimal? X { get; set; }
    public decimal? Y { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public bool IsVisible { get; set; } = true;
    public List<DesignerElementDto> Elements { get; set; } = new();
}

public class DesignerVersionDto
{
    public long TemplateVersionId { get; set; }
    public long InvoiceTemplateId { get; set; }
    public int VersionNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public List<DesignerSectionDto> Sections { get; set; } = new();
}

public class SaveDesignerVersionRequest
{
    public List<DesignerSectionDto> Sections { get; set; } = new();
}

public class PublishPreviewRequest { /* placeholder for render/sample overrides */ }

public class SampleInvoiceDataDto
{
    public InvoiceTemplateListItemDto? Template { get; set; }
    public DesignerVersionDto? Version { get; set; }
    public string? PaperSizeCode { get; set; }
    public object? SampleData { get; set; }
}

/* ---------------------------------------------------------------------------
   Printer configuration + print settings
   --------------------------------------------------------------------------- */
public class TemplatePrinterDto
{
    public long? TemplatePrinterId { get; set; }
    public long InvoiceTemplateId { get; set; }
    public long PaperSizeId { get; set; }
    public string? PaperSizeName { get; set; }
    public long PrinterTypeId { get; set; }
    public string? PrinterTypeName { get; set; }
    public long? PrinterModelId { get; set; }
    public string? PrinterModelName { get; set; }
    public string? PrinterName { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public InvoiceTemplatePrintSettingDto? PrintSetting { get; set; }
}

public class InvoiceTemplatePrintSettingDto
{
    public long? PrintSettingId { get; set; }
    public long TemplatePrinterId { get; set; }
    public decimal MarginTop { get; set; }
    public decimal MarginRight { get; set; }
    public decimal MarginBottom { get; set; }
    public decimal MarginLeft { get; set; }
    public decimal Scale { get; set; } = 100;
    public int Copies { get; set; } = 1;
    public bool AutoFit { get; set; } = true;
    public bool CutPaper { get; set; }
    public bool PrintHeader { get; set; } = true;
    public bool PrintFooter { get; set; } = true;
}

public record SaveTemplatePrinterRequest(
    long InvoiceTemplateId,
    long PaperSizeId,
    long PrinterTypeId,
    long? PrinterModelId,
    string? PrinterName,
    bool IsDefault = false,
    InvoiceTemplatePrintSettingDto? PrintSetting = null);

/* ---------------------------------------------------------------------------
   Assignments
   --------------------------------------------------------------------------- */
public class TemplateAssignmentDto
{
    public long AssignmentId { get; set; }
    public long InvoiceTemplateId { get; set; }
    public string? TemplateCode { get; set; }
    public string? TemplateName { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int? IndustryTypeId { get; set; }
    public string? IndustryTypeName { get; set; }
    public long? InvoiceTypeId { get; set; }
    public string? InvoiceTypeName { get; set; }
    public long? PaperSizeId { get; set; }
    public string? PaperSizeName { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public record CreateTemplateAssignmentRequest(
    long InvoiceTemplateId,
    int? CompanyId,
    int? IndustryTypeId,
    long? InvoiceTypeId,
    long? PaperSizeId,
    bool IsDefault = false,
    bool IsActive = true);