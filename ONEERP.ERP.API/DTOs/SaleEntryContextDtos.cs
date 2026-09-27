namespace ONEERP.ERP.API.DTOs;

public class SaleEntryCompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class SaleEntryBranchDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class SaleEntryStoreDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class SaleEntryCounterDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Assignment { get; set; } = string.Empty;
    public int? StoreId { get; set; }
    public string? StoreName { get; set; }
}

public class SaleEntryOperatorDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class SaleEntryPOSSessionDto
{
    public long Id { get; set; }
    public string SessionNumber { get; set; } = string.Empty;
    public byte Status { get; set; }
    public decimal OpeningCash { get; set; }
    public DateTime OpenedAt { get; set; }
    public string OpenedBy { get; set; } = string.Empty;
    public string StatusText => Status switch
    {
        1 => "OPEN",
        2 => "CLOSED",
        3 => "SUSPENDED",
        _ => "UNKNOWN"
    };
}

public class SaleEntryContextResponse
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }

    public List<SaleEntryCompanyDto> Companies { get; set; } = new();
    public List<SaleEntryBranchDto> Branches { get; set; } = new();
    public List<SaleEntryStoreDto> Stores { get; set; } = new();

    public SaleEntryCompanyDto? Company { get; set; }
    public SaleEntryBranchDto? Branch { get; set; }
    public SaleEntryStoreDto? Store { get; set; }

    public SaleEntryCounterDto? Counter { get; set; }
    public SaleEntryOperatorDto? Operator { get; set; }
    public SaleEntryPOSSessionDto? PosSession { get; set; }
}

public class SaleEntryContextValidateRequest
{
    public int CompanyId { get; set; }
    public int BranchId { get; set; }
    public int StoreId { get; set; }
    public int CounterId { get; set; }
    public int OperatorId { get; set; }
    public long PosSessionId { get; set; }
}

public class SaleEntryContextValidateResponse
{
    public bool IsValid { get; set; }
    public string? Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ContextToken { get; set; }
}