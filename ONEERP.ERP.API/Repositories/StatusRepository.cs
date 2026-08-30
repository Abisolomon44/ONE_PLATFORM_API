using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface IStatusRepository
{
    Task<long> GetIdByCodeAsync(string code);
    Task<IEnumerable<Status>> GetAllAsync();
}

public class StatusRepository : TenantRepositoryBase, IStatusRepository
{
    public StatusRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<long> GetIdByCodeAsync(string code)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<long>(connection,
            "SELECT ISNULL(StatusId, 0) FROM dbo.[Status] WHERE Code = @code", new { code });
    }

    public async Task<IEnumerable<Status>> GetAllAsync()
    {
        using var connection = OpenTenant();
        return (await Sql.QueryAsync<Status>(connection, "SELECT * FROM dbo.[Status] WHERE IsActive = 1")).ToList();
    }
}
