using System.Data;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface IIdempotencyService
{
    /// <summary>
    /// T039 — registers an idempotency attempt for a POST endpoint. Returns the
    /// ReferenceId stored by a previous successful call with the same key, or
    /// null when this is the first attempt (and the slot is now reserved).
    /// </summary>
    Task<long?> TryBeginAsync(long companyId, string endpoint, string idempotencyKey);

    /// <summary>Stores the created resource id so a retried POST can be answered from it.</summary>
    Task CompleteAsync(long companyId, string endpoint, string idempotencyKey, long referenceId);
}

public class IdempotencyService : TenantRepositoryBase, IIdempotencyService
{
    public IdempotencyService(ISqlHelper sql, TenantAccessor accessor, IPlatformDbConnectionFactory platformFactory)
        : base(sql, accessor, platformFactory)
    {
    }

    public async Task<long?> TryBeginAsync(long companyId, string endpoint, string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey)) return null;
        var key = idempotencyKey.Trim();
        using var conn = OpenTenant();
        var existing = await Sql.QuerySingleOrDefaultAsync<long?>(conn,
            @"SELECT ReferenceId FROM dbo.ApiIdempotency
              WHERE CompanyId = @companyId AND Endpoint = @endpoint AND IdempotencyKey = @key",
            new { companyId, endpoint, key });
        if (existing.HasValue) return existing.Value;

        try
        {
            await Sql.ExecuteAsync(conn,
                @"INSERT INTO dbo.ApiIdempotency (IdempotencyKey, CompanyId, Endpoint)
                  VALUES (@key, @companyId, @endpoint)",
                new { key, companyId, endpoint });
            return null;
        }
        catch (Exception)
        {
            // Concurrent duplicate — treat as a replay of the winner.
            var winner = await Sql.QuerySingleOrDefaultAsync<long?>(conn,
                @"SELECT ReferenceId FROM dbo.ApiIdempotency
                  WHERE CompanyId = @companyId AND Endpoint = @endpoint AND IdempotencyKey = @key",
                new { companyId, endpoint, key });
            if (winner.HasValue) return winner.Value;
            return null;
        }
    }

    public async Task CompleteAsync(long companyId, string endpoint, string idempotencyKey, long referenceId)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey)) return;
        using var conn = OpenTenant();
        await Sql.ExecuteAsync(conn,
            @"UPDATE dbo.ApiIdempotency SET ReferenceId = @referenceId
              WHERE CompanyId = @companyId AND Endpoint = @endpoint AND IdempotencyKey = @key",
            new { referenceId, companyId, endpoint, key = idempotencyKey.Trim() });
    }
}
