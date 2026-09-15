namespace ONEERP.ERP.API.DTOs;

/* ---------------- Metadata (drives the UI) ---------------- */

public class MasterImportMetaDto
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ImportColumnMetaDto> Columns { get; set; } = new();
}

public class ImportColumnMetaDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string Type { get; set; } = "text"; // text | number | boolean | date | reference
    public string? ReferenceEntity { get; set; } // e.g. "ProductCategories"
    public string? ReferenceDisplay { get; set; } // e.g. "categoryName"
    public bool Unique { get; set; } // used for duplicate detection
}

/* ---------------- Preview ---------------- */

public class ImportPreviewRequest
{
    public string EntityName { get; set; } = string.Empty;
    public List<Dictionary<string, string>> Rows { get; set; } = new();
}

public class ImportRowResultDto
{
    public int RowNumber { get; set; }
    public bool Valid { get; set; }
    public Dictionary<string, object?> Data { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class ImportPreviewResponse
{
    public List<ImportRowResultDto> Rows { get; set; } = new();
    public int TotalRows => Rows.Count;
    public int ValidCount => Rows.Count(r => r.Valid);
    public int ErrorCount => Rows.Count(r => !r.Valid);
}

/* ---------------- Confirm ---------------- */

public class ImportConfirmRequest
{
    public string EntityName { get; set; } = string.Empty;
    public List<Dictionary<string, string>> Rows { get; set; } = new();
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = "CSV";
}

public class ImportConfirmResponse
{
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int FailedRows { get; set; }
    public string Status { get; set; } = string.Empty; // COMPLETED | PARTIAL | FAILED
    public long LogId { get; set; }
    public List<string> Errors { get; set; } = new();
}

/* ---------------- Log ---------------- */

public class ImportLogDto
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public long? BranchId { get; set; }
    public string ImportType { get; set; } = "MASTER";
    public string ModuleName { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int FailedRows { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public long ImportedBy { get; set; }
    public DateTime ImportedAt { get; set; }
}
