namespace ONEERP.ERP.API.Models;

/* ---------------------------------------------------------------------------
   Invoice Template Design - entities (tables created by
   sql/erp_migration_002_invoice_templates.sql)
   --------------------------------------------------------------------------- */

public class InvoiceType
{
    public long InvoiceTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class InvoicePaperSize
{
    public long PaperSizeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Width { get; set; }
    public decimal? Height { get; set; }
    public string Unit { get; set; } = "MM";
    public bool IsThermal { get; set; }
    public bool IsCustom { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class PrinterType
{
    public long PrinterTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class PrinterModel
{
    public long PrinterModelId { get; set; }
    public long PrinterTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class InvoiceTemplateCategory
{
    public long TemplateCategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class InvoiceTemplateComponent
{
    public long ComponentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class InvoiceTemplateVariable
{
    public long VariableId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BindingPath { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public bool IsCollection { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class InvoiceFont
{
    public long FontId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FontFamily { get; set; } = string.Empty;
    public long? FontFileId { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class PrintOrientation
{
    public long OrientationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class PrintUnit
{
    public long UnitId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class InvoiceTemplate
{
    public long InvoiceTemplateId { get; set; }
    public int? CompanyId { get; set; }
    public long? TemplateCategoryId { get; set; }
    public long InvoiceTypeId { get; set; }
    public long PaperSizeId { get; set; }
    public long OrientationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class InvoiceTemplateAssignment
{
    public long AssignmentId { get; set; }
    public long InvoiceTemplateId { get; set; }
    public int? CompanyId { get; set; }
    public int? IndustryTypeId { get; set; }
    public long? InvoiceTypeId { get; set; }
    public long? PaperSizeId { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class InvoiceTemplateVersion
{
    public long TemplateVersionId { get; set; }
    public long InvoiceTemplateId { get; set; }
    public int VersionNumber { get; set; }
    public string TemplateJson { get; set; } = "{}";
    public string Status { get; set; } = "DRAFT";
    public bool IsPublished { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? PublishedBy { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class InvoiceTemplateSection
{
    public long SectionId { get; set; }
    public long TemplateVersionId { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public decimal? X { get; set; }
    public decimal? Y { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public bool IsVisible { get; set; } = true;
}

public class InvoiceTemplateElement
{
    public long ElementId { get; set; }
    public long SectionId { get; set; }
    public long ComponentId { get; set; }
    public string ElementType { get; set; } = string.Empty;
    public string? ElementName { get; set; }
    public decimal? X { get; set; }
    public decimal? Y { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;
}

public class InvoiceTemplateField
{
    public long TemplateFieldId { get; set; }
    public long ElementId { get; set; }
    public long VariableId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string BindingPath { get; set; } = string.Empty;
    public string? Label { get; set; }
    public bool IsVisible { get; set; } = true;
}

public class InvoiceTemplateItemColumn
{
    public long ItemColumnId { get; set; }
    public long ElementId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string HeaderText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public decimal? Width { get; set; }
    public string Alignment { get; set; } = "LEFT";
    public bool IsVisible { get; set; } = true;
}

public class InvoiceTemplateStyle
{
    public long StyleId { get; set; }
    public long ElementId { get; set; }
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

public class InvoiceTemplatePrinter
{
    public long TemplatePrinterId { get; set; }
    public long InvoiceTemplateId { get; set; }
    public long PaperSizeId { get; set; }
    public long PrinterTypeId { get; set; }
    public long? PrinterModelId { get; set; }
    public string? PrinterName { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class InvoiceTemplatePrintSetting
{
    public long PrintSettingId { get; set; }
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
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}