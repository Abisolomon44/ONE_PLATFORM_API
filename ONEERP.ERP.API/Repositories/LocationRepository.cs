using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

public interface ICountryRepository
{
    Task<IEnumerable<Country>> GetAllAsync(bool includeInactive = false);
    Task<Country?> GetByIdAsync(int id);
    Task<Country?> GetByISOCode2Async(string isoCode2);
    Task<Country?> GetByISOCode3Async(string isoCode3);
    Task<bool> HasStatesAsync(int countryId);
    Task<int> InsertAsync(Country entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Country entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public interface IStateRepository
{
    Task<IEnumerable<State>> GetAllAsync(int? countryId, bool includeInactive = false);
    Task<State?> GetByIdAsync(int id);
    Task<State?> GetByCodeAsync(int countryId, string stateCode);
    Task<bool> HasCitiesAsync(int stateId);
    Task<int> InsertAsync(State entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(State entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public interface ICityRepository
{
    Task<IEnumerable<City>> GetAllAsync(int? countryId, int? stateId, bool includeInactive = false);
    Task<City?> GetByIdAsync(int id);
    Task<City?> GetByNameAsync(int stateId, string name);
    Task<int> InsertAsync(City entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(City entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}

public class CountryRepository : TenantRepositoryBase, ICountryRepository
{
    public CountryRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<Country>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? "SELECT * FROM dbo.Countries WHERE IsDeleted = 0 ORDER BY [Name]"
            : "SELECT * FROM dbo.Countries WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY [Name]";
        return await Sql.QueryAsync<Country>(connection, sql);
    }

    public async Task<Country?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Country>(connection,
            "SELECT * FROM dbo.Countries WHERE CountryId = @id AND IsDeleted = 0", new { id });
    }

    public async Task<Country?> GetByISOCode2Async(string isoCode2)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Country>(connection,
            "SELECT * FROM dbo.Countries WHERE ISOCode2 = @isoCode2 AND IsDeleted = 0", new { isoCode2 });
    }

    public async Task<Country?> GetByISOCode3Async(string isoCode3)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<Country>(connection,
            "SELECT * FROM dbo.Countries WHERE ISOCode3 = @isoCode3 AND IsDeleted = 0", new { isoCode3 });
    }

    public async Task<bool> HasStatesAsync(int countryId)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.States WHERE CountryId = @countryId AND IsDeleted = 0) THEN 1 ELSE 0 END", new { countryId });
    }

    public async Task<int> InsertAsync(Country entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Countries ([Name], ISOCode2, ISOCode3, PhoneCode, CurrencyCode, Nationality, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@Name, @ISOCode2, @ISOCode3, @PhoneCode, @CurrencyCode, @Nationality, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(Country entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Countries
                SET [Name] = @Name,
                    ISOCode2 = @ISOCode2,
                    ISOCode3 = @ISOCode3,
                    PhoneCode = @PhoneCode,
                    CurrencyCode = @CurrencyCode,
                    Nationality = @Nationality,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE CountryId = @CountryId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Countries SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE CountryId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

public class StateRepository : TenantRepositoryBase, IStateRepository
{
    public StateRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<State>> GetAllAsync(int? countryId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? @"SELECT s.*, c.[Name] AS CountryName FROM dbo.States s
                 INNER JOIN dbo.Countries c ON c.CountryId = s.CountryId
                 WHERE s.IsDeleted = 0 AND (@countryId IS NULL OR s.CountryId = @countryId)
                 ORDER BY c.[Name], s.[Name]"
            : @"SELECT s.*, c.[Name] AS CountryName FROM dbo.States s
                 INNER JOIN dbo.Countries c ON c.CountryId = s.CountryId
                 WHERE s.IsDeleted = 0 AND s.IsActive = 1 AND (@countryId IS NULL OR s.CountryId = @countryId)
                 ORDER BY c.[Name], s.[Name]";
        return await Sql.QueryAsync<State>(connection, sql, new { countryId });
    }

    public async Task<State?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<State>(connection, @"
            SELECT s.*, c.[Name] AS CountryName FROM dbo.States s
            INNER JOIN dbo.Countries c ON c.CountryId = s.CountryId
            WHERE s.StateId = @id AND s.IsDeleted = 0", new { id });
    }

    public async Task<State?> GetByCodeAsync(int countryId, string stateCode)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<State>(connection,
            "SELECT * FROM dbo.States WHERE CountryId = @countryId AND StateCode = @stateCode AND IsDeleted = 0", new { countryId, stateCode });
    }

    public async Task<bool> HasCitiesAsync(int stateId)
    {
        using var connection = OpenTenant();
        return await Sql.ExecuteScalarAsync<bool>(connection,
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Cities WHERE StateId = @stateId AND IsDeleted = 0) THEN 1 ELSE 0 END", new { stateId });
    }

    public async Task<int> InsertAsync(State entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.States (CountryId, [Name], StateCode, GSTStateCode, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@CountryId, @Name, @StateCode, @GSTStateCode, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(State entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.States
                SET CountryId = @CountryId,
                    [Name] = @Name,
                    StateCode = @StateCode,
                    GSTStateCode = @GSTStateCode,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE StateId = @StateId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.States SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE StateId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}

public class CityRepository : TenantRepositoryBase, ICityRepository
{
    public CityRepository(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<IEnumerable<City>> GetAllAsync(int? countryId, int? stateId, bool includeInactive = false)
    {
        using var connection = OpenTenant();
        var sql = includeInactive
            ? @"SELECT ci.*, co.[Name] AS CountryName, st.[Name] AS StateName FROM dbo.Cities ci
                 INNER JOIN dbo.Countries co ON co.CountryId = ci.CountryId
                 INNER JOIN dbo.States st ON st.StateId = ci.StateId
                 WHERE ci.IsDeleted = 0
                   AND (@countryId IS NULL OR ci.CountryId = @countryId)
                   AND (@stateId IS NULL OR ci.StateId = @stateId)
                 ORDER BY co.[Name], st.[Name], ci.[Name]"
            : @"SELECT ci.*, co.[Name] AS CountryName, st.[Name] AS StateName FROM dbo.Cities ci
                 INNER JOIN dbo.Countries co ON co.CountryId = ci.CountryId
                 INNER JOIN dbo.States st ON st.StateId = ci.StateId
                 WHERE ci.IsDeleted = 0 AND ci.IsActive = 1
                   AND (@countryId IS NULL OR ci.CountryId = @countryId)
                   AND (@stateId IS NULL OR ci.StateId = @stateId)
                 ORDER BY co.[Name], st.[Name], ci.[Name]";
        return await Sql.QueryAsync<City>(connection, sql, new { countryId, stateId });
    }

    public async Task<City?> GetByIdAsync(int id)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<City>(connection, @"
            SELECT ci.*, co.[Name] AS CountryName, st.[Name] AS StateName FROM dbo.Cities ci
            INNER JOIN dbo.Countries co ON co.CountryId = ci.CountryId
            INNER JOIN dbo.States st ON st.StateId = ci.StateId
            WHERE ci.CityId = @id AND ci.IsDeleted = 0", new { id });
    }

    public async Task<City?> GetByNameAsync(int stateId, string name)
    {
        using var connection = OpenTenant();
        return await Sql.QuerySingleOrDefaultAsync<City>(connection,
            "SELECT * FROM dbo.Cities WHERE StateId = @stateId AND [Name] = @name AND IsDeleted = 0", new { stateId, name });
    }

    public async Task<int> InsertAsync(City entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                INSERT INTO dbo.Cities (CountryId, StateId, [Name], PostalCode, Latitude, Longitude, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@CountryId, @StateId, @Name, @PostalCode, @Latitude, @Longitude, @IsActive, @CreatedBy, SYSUTCDATETIME(), @ModifiedBy, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await Sql.QuerySingleOrDefaultAsync<int>(conn, sql, entity, transaction);
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> UpdateAsync(City entity, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = @"
                UPDATE dbo.Cities
                SET CountryId = @CountryId,
                    StateId = @StateId,
                    [Name] = @Name,
                    PostalCode = @PostalCode,
                    Latitude = @Latitude,
                    Longitude = @Longitude,
                    IsActive = @IsActive,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = SYSUTCDATETIME()
                WHERE CityId = @CityId;";
            return await Sql.ExecuteAsync(conn, sql, entity, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }

    public async Task<bool> SoftDeleteAsync(int id, string? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var conn = connection ?? OpenTenant();
        var own = connection is null;
        try
        {
            const string sql = "UPDATE dbo.Cities SET IsDeleted = 1, IsActive = 0, ModifiedBy = @modifiedBy, ModifiedDate = SYSUTCDATETIME() WHERE CityId = @id;";
            return await Sql.ExecuteAsync(conn, sql, new { id, modifiedBy }, transaction) > 0;
        }
        finally
        {
            if (own) conn.Dispose();
        }
    }
}
