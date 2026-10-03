using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface IPOSOperationsService
{
    Task<POSDashboardDto> GetDashboardAsync(long companyId, long? branchId);
    Task<POSSession> GetCurrentSessionAsync(long companyId, long? counterId);
    Task<POSSession> CloseSessionAsync(long sessionId, long userId, ClosePOSSessionRequest request);
    Task<POSCashMovementDto> AddCashMovementAsync(long companyId, long userId, POSCashMovementRequest request, string direction);
    Task<POSHoldBillDto> CreateHoldAsync(long companyId, long userId, CreatePOSHoldRequest request);
    Task<(List<POSHoldBillDto> Items, int Total)> GetHoldsAsync(long companyId, long branchId, int page, int size);
    Task<POSHoldBillDto> RecallHoldAsync(long id, long userId, long companyId);
    Task CancelHoldAsync(long id, long userId, long companyId);
    Task<POSShiftSummaryDto> GetShiftSummaryAsync(long companyId, long sessionId);
}

public class POSOperationsService : IPOSOperationsService
{
    private readonly IPOSOperationsRepository _repo;
    private readonly IPOSSessionRepository _sessionRepo;
    private readonly ICurrentUser _currentUser;

    public POSOperationsService(IPOSOperationsRepository repo, IPOSSessionRepository sessionRepo, ICurrentUser currentUser)
    {
        _repo = repo;
        _sessionRepo = sessionRepo;
        _currentUser = currentUser;
    }

    // ============================================================
    // T041 — dashboard. Session-scoped only; no invented metrics.
    // ============================================================
    public async Task<POSDashboardDto> GetDashboardAsync(long companyId, long? branchId)
    {
        var session = await GetCurrentSessionAsync(companyId, null);
        var dto = new POSDashboardDto();
        if (session == null) return dto;

        dto.CurrentSessionId = session.POSSessionId;
        dto.CurrentSessionNumber = session.SessionNumber;
        dto.SessionStatus = session.Status == 1 ? "OPEN" : session.Status == 2 ? "CLOSED" : "VOID";
        dto.OpeningCash = session.OpeningCash;
        dto.ExpectedClosingCash = session.ExpectedClosingCash;
        dto.CashIn = await _repo.GetCashTotalAsync(session.POSSessionId, "IN");
        dto.CashOut = await _repo.GetCashTotalAsync(session.POSSessionId, "OUT");

        var agg = await _repo.GetDashboardAggAsync(companyId, branchId, session.POSSessionId);
        dto.PosSalesToday = agg?.PosSalesToday ?? 0;
        dto.InvoiceCountToday = agg?.InvoiceCountToday ?? 0;
        return dto;
    }

    // ============================================================
    // T043/T044 — current session + validation chain:
    // JWT/user → company scope → counter scope → OPEN status.
    // Session state authority is the DB, never the frontend.
    // ============================================================
    public async Task<POSSession> GetCurrentSessionAsync(long companyId, long? counterId)
    {
        IEnumerable<POSSession> open;
        if (counterId.HasValue)
        {
            open = await _sessionRepo.GetActiveByCounterAsync((int)counterId.Value);
        }
        else
        {
            // No counter context → all open sessions of the company; the
            // operator must hold exactly one to bill.
            open = await _sessionRepo.GetPagedAsync((int)companyId, 0, 0, 1, 1, 10, string.Empty);
        }
        var session = open.FirstOrDefault(s => s.CompanyId == companyId)
            ?? throw new DomainException("No OPEN POS session found. Open a session before billing.", 400);
        return session;
    }

    // ============================================================
    // T053 — single authoritative close command. Only OPEN sessions,
    // correct company scope; cash difference derived, never trusted.
    // ============================================================
    public async Task<POSSession> CloseSessionAsync(long sessionId, long userId, ClosePOSSessionRequest request)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId)
            ?? throw new DomainException($"POS session '{sessionId}' was not found.");
        if (session.CompanyId != _currentUser.CompanyId)
            throw new DomainException("POS session does not belong to your company.", 403);
        if (session.Status != 1)
            throw new DomainException("Only an OPEN session can be closed.");

        if (request.ActualClosingCash < 0)
            throw new DomainException("Actual closing cash cannot be negative.");

        session.ActualClosingCash = request.ActualClosingCash;
        session.CashDifference = request.ActualClosingCash - session.ExpectedClosingCash;
        session.ClosedAt = DateTime.UtcNow;
        session.ClosedBy = (int)userId;
        session.ClosingRemarks = request.ClosingRemarks;
        session.Status = 2;

        if (!await _sessionRepo.UpdateAsync(session, session.Version))
            throw new DomainException("POS session was modified by another user. Refresh and retry.", 409);
        return session;
    }

    // ============================================================
    // T049/T050 — Cash In / Cash Out on an OPEN session only.
    // ============================================================
    public async Task<POSCashMovementDto> AddCashMovementAsync(long companyId, long userId, POSCashMovementRequest request, string direction)
    {
        direction = direction.ToUpperInvariant() switch { "IN" => "IN", "OUT" => "OUT", _ => throw new DomainException("Direction must be IN or OUT.") };
        var session = await _sessionRepo.GetByIdAsync(request.POSSessionId)
            ?? throw new DomainException($"POS session '{request.POSSessionId}' was not found.");
        if (session.CompanyId != companyId)
            throw new DomainException("POS session does not belong to your company.", 403);
        if (session.Status != 1)
            throw new DomainException("Cash movements require an OPEN session.");
        if (request.Amount <= 0)
            throw new DomainException("Amount must be greater than 0.");

        var m = new POSCashMovement
        {
            POSSessionId = session.POSSessionId,
            CompanyId = companyId,
            BranchId = session.BranchId ?? 0,
            Direction = direction,
            Amount = request.Amount,
            Reason = request.Reason,
            ReferenceNo = request.ReferenceNo,
            MovementDate = DateTime.UtcNow,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow,
        };
        m.POSCashMovementId = await _repo.InsertCashMovementAsync(m);
        return ToDto(m);
    }

    // ============================================================
    // T047 — persistent hold (replaces browser-tab holds).
    // ============================================================
    public async Task<POSHoldBillDto> CreateHoldAsync(long companyId, long userId, CreatePOSHoldRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CartJson))
            throw new DomainException("Cart payload is required to hold a bill.");
        if (request.ItemCount <= 0)
            throw new DomainException("A held bill must contain at least one item.");

        var h = new POSHoldBill
        {
            CompanyId = companyId,
            BranchId = request.BranchId,
            StoreId = request.StoreId,
            CounterId = request.CounterId,
            HoldNumber = await _repo.GetNextHoldNumberAsync(companyId),
            HoldDate = DateTime.UtcNow,
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            ItemCount = request.ItemCount,
            TotalAmount = request.TotalAmount,
            CartJson = request.CartJson,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow,
        };
        h.POSHoldBillId = await _repo.InsertHoldAsync(h);
        return ToDto(h, includeCart: false);
    }

    public async Task<(List<POSHoldBillDto> Items, int Total)> GetHoldsAsync(long companyId, long branchId, int page, int size)
    {
        var (items, total) = await _repo.GetHoldsAsync(companyId, branchId, "HELD", Math.Max(1, page), Math.Clamp(size, 1, 100));
        return (items.Select(i => ToDto(i, includeCart: false)).ToList(), total);
    }

    // T048 — recall restores the cart payload; it never posts anything.
    public async Task<POSHoldBillDto> RecallHoldAsync(long id, long userId, long companyId)
    {
        var h = await _repo.GetHoldAsync(id)
            ?? throw new DomainException($"Held bill '{id}' was not found.");
        if (h.CompanyId != companyId)
            throw new DomainException("Held bill does not belong to your company.", 403);
        if (h.Status != "HELD")
            throw new DomainException($"Held bill is {h.Status} and cannot be recalled.");
        if (!await _repo.RecallHoldAsync(id, userId))
            throw new DomainException("Held bill was recalled by another user.", 409);
        return ToDto((await _repo.GetHoldAsync(id))!, includeCart: true);
    }

    public async Task CancelHoldAsync(long id, long userId, long companyId)
    {
        var h = await _repo.GetHoldAsync(id)
            ?? throw new DomainException($"Held bill '{id}' was not found.");
        if (h.CompanyId != companyId)
            throw new DomainException("Held bill does not belong to your company.", 403);
        if (!await _repo.CancelHoldAsync(id, userId))
            throw new DomainException("Held bill is no longer held.", 409);
    }

    // ============================================================
    // T055 — shift summary from POSSessions + SalesInvoice + SalesReturn
    // + POSCashMovement. Expected cash = opening + sales cash position
    // is derived by the session row; difference = actual − expected.
    // ============================================================
    public async Task<POSShiftSummaryDto> GetShiftSummaryAsync(long companyId, long sessionId)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId)
            ?? throw new DomainException($"POS session '{sessionId}' was not found.");
        if (session.CompanyId != companyId)
            throw new DomainException("POS session does not belong to your company.", 403);

        var agg = await _repo.GetShiftAggAsync(companyId, sessionId) ?? new ShiftAgg();
        return new POSShiftSummaryDto
        {
            POSSessionId = session.POSSessionId,
            SessionNumber = session.SessionNumber,
            Status = session.Status == 1 ? "OPEN" : session.Status == 2 ? "CLOSED" : "VOID",
            OpenedAt = session.OpenedAt,
            ClosedAt = session.ClosedAt,
            OpeningCash = session.OpeningCash,
            CashIn = await _repo.GetCashTotalAsync(sessionId, "IN"),
            CashOut = await _repo.GetCashTotalAsync(sessionId, "OUT"),
            InvoiceCount = agg.InvoiceCount,
            GrossSales = agg.Gross,
            Discount = agg.Discount,
            Taxable = agg.Taxable,
            Tax = agg.Tax,
            GrandTotal = agg.GrandTotal,
            Paid = agg.Paid,
            Balance = agg.Balance,
            ReturnCount = agg.ReturnCount,
            ReturnTotal = agg.ReturnTotal,
            ExpectedClosingCash = session.ExpectedClosingCash,
            ActualClosingCash = session.ActualClosingCash,
            CashDifference = session.CashDifference,
        };
    }

    private static POSCashMovementDto ToDto(POSCashMovement m) => new()
    {
        POSCashMovementId = m.POSCashMovementId,
        POSSessionId = m.POSSessionId,
        Direction = m.Direction,
        Amount = m.Amount,
        Reason = m.Reason,
        ReferenceNo = m.ReferenceNo,
        MovementDate = m.MovementDate,
    };

    private static POSHoldBillDto ToDto(POSHoldBill h, bool includeCart) => new()
    {
        POSHoldBillId = h.POSHoldBillId,
        HoldNumber = h.HoldNumber,
        HoldDate = h.HoldDate,
        CustomerId = h.CustomerId,
        CustomerName = h.CustomerName,
        ItemCount = h.ItemCount,
        TotalAmount = h.TotalAmount,
        Status = h.Status,
        CartJson = includeCart ? h.CartJson : null,
    };
}
