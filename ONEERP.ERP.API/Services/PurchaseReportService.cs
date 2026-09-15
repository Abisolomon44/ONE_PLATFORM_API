using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface IPurchaseReportService
{
    Task<PurchaseReportResult> RunAsync(long companyId, PurchaseReportFilter filter);
}

public class PurchaseReportService : IPurchaseReportService
{
    private readonly IPurchaseReportRepository _repo;
    private readonly ICurrentUser _currentUser;

    public PurchaseReportService(IPurchaseReportRepository repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<PurchaseReportResult> RunAsync(long companyId, PurchaseReportFilter filter)
    {
        // Returns + reconciliation expose purchase-return data, so owners of
        // either purchases.view or purchases.return.view may open them. All other
        // reports require purchases.view.
        var needsReturnView = filter.Report is "returns" or "reconciliation";
        var permitted = needsReturnView
            ? _currentUser.HasAnyPermission(Permissions.PurchasesView, Permissions.PurchasesReturnView)
            : _currentUser.HasPermission(Permissions.PurchasesView);
        if (!permitted && !_currentUser.IsSuperAdmin)
            throw new DomainException("You do not have permission to view this report.", 403);

        filter.Page = Math.Max(1, filter.Page);
        filter.Size = Math.Clamp(filter.Size, 1, 500);
        if (filter.GroupBy is "none" or "" or null) filter.GroupBy = null;
        if (string.IsNullOrWhiteSpace(filter.Mode)) filter.Mode = null;

        return await _repo.RunAsync(companyId, filter);
    }
}