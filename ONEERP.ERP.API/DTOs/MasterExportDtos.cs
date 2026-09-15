namespace ONEERP.ERP.API.DTOs;

/* ---------------- Export metadata (drives the Export tab) ---------------- */

public class ExportFilterMetaDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "reference"; // reference | status | date | text
    public string? ReferenceEntity { get; set; }
    public string? ReferenceDisplay { get; set; }
}

public class MasterExportMetaDto
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ImportColumnMetaDto> Columns { get; set; } = new();
    public List<ExportFilterMetaDto> Filters { get; set; } = new();
}

public class ExportFilterOptionDto
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

/* ---------------- Export query / results ---------------- */

public class ExportQueryDto
{
    public string EntityName { get; set; } = string.Empty;
    public string Format { get; set; } = "xlsx"; // xlsx | csv
    public Dictionary<string, string> Filters { get; set; } = new();
    public List<string> Columns { get; set; } = new();
    public string? Search { get; set; }
    public bool IncludeInactive { get; set; }
    public bool UseDisplayNames { get; set; } = true;
    public bool IncludeEmptyColumns { get; set; }
    public bool IncludeHeaders { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class ExportPreviewResponseDto
{
    public List<ImportColumnMetaDto> Columns { get; set; } = new();
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class ExportFileResultDto
{
    public byte[] Bytes { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}