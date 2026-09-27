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
    public int? CounterAssignmentId { get; set; }
    public int? OperatorId { get; set; }
    public string? OperatorNameSnapshot { get; set; }
    public string SessionNumber { get; set; } = string.Empty;
    public decimal OpeningCash { get; set; }
    public decimal ExpectedClosingCash { get; set; }
    public decimal ActualClosingCash { get; set; }
    public decimal CashDifference { get; set; }
    public DateTime OpenedAt { get; set; }
    public int? OpenedBy { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int? ClosedBy { get; set; }
    public string? ClosingRemarks { get; set; }
    public byte Status { get; set; }
    public byte[]? Version { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Opens a session. Only StoreId, CounterId, OperatorId and OpeningCash come from
/// the client; CompanyId/BranchId/CounterAssignmentId/SessionNumber/OpenedAt/
/// OpenedBy are resolved server-side and Status is always forced to Open (1).
/// </summary>
public record CreatePOSSessionRequest(
    int StoreId,
    int CounterId,
    int OperatorId,
    decimal OpeningCash);

/// <summary>
/// Closes or voids a session. ExpectedClosingCash and CashDifference are computed
/// server-side; Version carries the optimistic concurrency token.
/// </summary>
public record UpdatePOSSessionRequest(
    decimal? ActualClosingCash,
    byte Status,
    string? ClosingRemarks,
    byte[]? Version);

public class OperatorDto
{
    public int OperatorId { get; set; }
    public int Id => OperatorId;
    public int CompanyId { get; set; }
    public int? BranchId { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int? OperatorTypeId { get; set; }
    public string? OperatorTypeCode { get; set; }
    public string? OperatorTypeName { get; set; }
    public string OperatorCode { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record CreateOperatorRequest(
    int CompanyId,
    int? BranchId,
    int UserId,
    int? OperatorTypeId,
    string OperatorCode,
    string OperatorName,
    bool IsActive);

public record UpdateOperatorRequest(
    int? BranchId,
    int? OperatorTypeId,
    string? OperatorCode,
    string? OperatorName,
    bool IsActive);

public class CounterOperatorAssignmentDto
{
    public int AssignmentId { get; set; }
    public int Id => AssignmentId;
    public int CompanyId { get; set; }
    public int? BranchId { get; set; }
    public int StoreId { get; set; }
    public string? StoreName { get; set; }
    public int CounterId { get; set; }
    public string? CounterCode { get; set; }
    public string? CounterName { get; set; }
    public int OperatorId { get; set; }
    public string? OperatorCode { get; set; }
    public string? OperatorName { get; set; }
    public bool IsPrimary { get; set; }
    public string? ValidFrom { get; set; }
    public string? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record CreateCounterAssignmentRequest(
    int CompanyId,
    int? BranchId,
    int StoreId,
    int CounterId,
    int OperatorId,
    bool IsPrimary,
    string? ValidFrom,
    string? ValidTo,
    bool IsActive);

public record UpdateCounterAssignmentRequest(
    int? BranchId,
    int? StoreId,
    int? CounterId,
    int? OperatorId,
    bool IsPrimary,
    string? ValidFrom,
    string? ValidTo,
    bool IsActive);