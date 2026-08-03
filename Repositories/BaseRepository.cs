using System.Data;
using ONEERP.Platform.API.Data;

namespace ONEERP.Platform.API.Repositories;

public abstract class BaseRepository
{
    protected readonly IDbConnectionFactory Factory;
    protected readonly ISqlHelper Sql;

    protected BaseRepository(IDbConnectionFactory factory, ISqlHelper sql)
    {
        Factory = factory;
        Sql = sql;
    }

    protected IDbConnection OpenPlatform() => Factory.CreatePlatformConnection();
}
