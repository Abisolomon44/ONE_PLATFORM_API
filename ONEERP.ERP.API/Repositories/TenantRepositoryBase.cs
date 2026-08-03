using System.Data;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Repositories;

/// <summary>
/// Base repository for all tenant-scoped repositories. Provides access to the
/// current tenant database connection (via TenantAccessor) and the platform
/// database connection.
/// </summary>
public abstract class TenantRepositoryBase
{
    protected readonly ISqlHelper Sql;
    protected readonly TenantAccessor Accessor;
    protected readonly IPlatformDbConnectionFactory PlatformFactory;

    protected TenantRepositoryBase(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
    {
        Sql = sql;
        Accessor = accessor;
        PlatformFactory = platformFactory;
    }

    protected IDbConnection OpenTenant() => Accessor.OpenTenantConnection();
}
