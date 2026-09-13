using System;

namespace ONEERP.ERP.API.DTOs;

public class StoreDto
{
    public int StoreId { get; set; }
    public int Id => StoreId;
    public long? EntityId { get; set; }
    public int CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string StoreCode { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? StoreType { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record CreateStoreRequest(
    int CompanyId,
    int? BranchId,
    string StoreCode,
    string StoreName,
    string? StoreType,
    string? Address,
    string? Phone,
    string? Email,
    bool IsActive,
    long? EntityId = null);

public record UpdateStoreRequest(
    int? BranchId,
    string StoreCode,
    string StoreName,
    string? StoreType,
    string? Address,
    string? Phone,
    string? Email,
    bool IsActive,
    long? EntityId = null);

public class CounterDto
{
    public int CounterId { get; set; }
    public int Id => CounterId;
    public int StoreId { get; set; }
    public string CounterCode { get; set; } = string.Empty;
    public string CounterName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record CreateCounterRequest(
    int StoreId,
    string CounterCode,
    string CounterName,
    bool IsActive);

public record UpdateCounterRequest(
    string CounterCode,
    string CounterName,
    bool IsActive);

public class POSSessionDto
{
    public long POSSessionId { get; set; }
    public long Id => POSSessionId;
    public int CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? StoreId { get; set; }
    public string? StoreName { get; set; }
    public int? CounterId { get; set; }
    public string? CounterName { get; set; }
    public int? CashierUserId { get; set; }
    public string? CashierUserName { get; set; }
    public string SessionNumber { get; set; } = string.Empty;
    public decimal OpeningCash { get; set; }
    public decimal? ClosingCash { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public byte Status { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record CreatePOSSessionRequest(
    int CompanyId,
    int? BranchId,
    int? StoreId,
    int? CounterId,
    int? CashierUserId,
    string? CompanyName,
    string? BranchName,
    string? StoreName,
    string? CounterName,
    string? CashierUserName,
    decimal OpeningCash);

public record UpdatePOSSessionRequest(
    decimal? ClosingCash,
    byte Status);