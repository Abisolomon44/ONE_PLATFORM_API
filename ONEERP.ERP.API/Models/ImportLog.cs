namespace ONEERP.ERP.API.Models;

/// <summary>
/// Common log for every (master / transaction) import run.
/// One row per import file processed.
/// </summary>
public class ImportLog
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public long? BranchId { get; set; }

    public string ImportType { get; set; } = "MASTER";
    public string ModuleName { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = "CSV";

    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int FailedRows { get; set; }

    public string Status { get; set; } = "PROCESSING";

    public string? ErrorMessage { get; set; }

    public long ImportedBy { get; set; }
    public DateTime ImportedAt { get; set; }
}
