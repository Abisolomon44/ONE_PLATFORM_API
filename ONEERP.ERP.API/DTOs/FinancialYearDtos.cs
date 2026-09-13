using System;

namespace ONEERP.ERP.API.DTOs;

public class FinancialYearDto
{
    public long FinancialYearId { get; set; }
    public long Id => FinancialYearId;
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsClosed { get; set; }
    public bool IsActive { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public record CreateFinancialYearRequest(
    int CompanyId,
    string Code,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool IsCurrent,
    bool IsClosed,
    bool IsActive);

public record UpdateFinancialYearRequest(
    string Code,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool IsCurrent,
    bool IsClosed,
    bool IsActive);