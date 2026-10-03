using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface ISalesReportService
{
    Task<SalesReportResult> RunAsync(long companyId, SalesReportFilter filter);
}

public class SalesReportService : ISalesReportService
{
    private readonly ISalesReportRepository _repo;
    private readonly ICurrentUser _currentUser;

    public SalesReportService(ISalesReportRepository repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<SalesReportResult> RunAsync(long companyId, SalesReportFilter filter)
    {
        // Every sales report reads SalesInvoice data, so sales.view is the gate.
        // UI hiding is not security — this runs before any query touches the DB.
        if (!_currentUser.HasPermission(Permissions.SalesView) && !_currentUser.IsSuperAdmin)
            throw new DomainException("You do not have permission to view this report.", 403);

        filter.Page = Math.Max(1, filter.Page);
        filter.Size = Math.Clamp(filter.Size, 1, 500);
        if (filter.GroupBy is "none" or "" or null) filter.GroupBy = null;
        return await _repo.RunAsync(companyId, filter);
    }
}
