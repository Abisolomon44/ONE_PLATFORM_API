using System.Data;
using System.Data.Common;
using Dapper;

namespace ONEERP.Platform.API.Data;

/// <summary>
/// Tenant-scoped exclusive migration lock built on SQL Server's
/// sys.sp_getapplock. The lock is session-owned, so it is held for exactly as
/// long as the engine keeps the connection open and is released even if the
/// process crashes (SQL Server drops session locks on disconnect).
/// </summary>
public interface ITenantMigrationLock
{
    /// <summary>
    /// Tries to take the exclusive migration lock for a tenant.
    /// Returns a handle that must be disposed to release the lock, or null
    /// when the lock is already held by another administrator.
    /// </summary>
    Task<IAsyncDisposable?> AcquireAsync(int tenantId, int timeoutMs, CancellationToken ct = default);
}

public class TenantMigrationLock : ITenantMigrationLock
{
    private const int Granted = 0;
    private const int GrantedAfterWait = 1;

    private readonly IDbConnectionFactory _factory;

    public TenantMigrationLock(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IAsyncDisposable?> AcquireAsync(int tenantId, int timeoutMs, CancellationToken ct = default)
    {
        // The factory always produces SqlConnection, which exposes the async
        // ADO.NET surface the lock handle needs.
        var connection = (DbConnection)_factory.CreatePlatformConnection();
        try
        {
            await connection.OpenAsync(ct);

            var result = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                @"DECLARE @result INT;
                  EXEC @result = sys.sp_getapplock
                       @Resource    = @resource,
                       @LockMode    = 'Exclusive',
                       @LockOwner   = 'Session',
                       @LockTimeout = @timeout,
                       @DbPrincipal = 'public';
                  SELECT @result;",
                new { resource = ResourceName(tenantId), timeout = timeoutMs },
                cancellationToken: ct));

            if (result is not (Granted or GrantedAfterWait))
                return null;

            return new LockHandle(connection, ResourceName(tenantId));        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private static string ResourceName(int tenantId) => $"ONEERP:tenant-migration:{tenantId}";

    private sealed class LockHandle : IAsyncDisposable
    {
        private readonly DbConnection _connection;
        private readonly string _resource;
        private int _released;

        public LockHandle(DbConnection connection, string resource)
        {
            _connection = connection;
            _resource = resource;
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _released, 1) == 1)
                return;

            try
            {
                if (_connection.State == ConnectionState.Open)
                {
                    await _connection.ExecuteAsync(new CommandDefinition(
                        "EXEC sys.sp_releaseapplock @Resource = @resource, @LockOwner = 'Session', @DbPrincipal = 'public';",
                        new { resource = _resource }));
                }
            }
            catch
            {
                // Releasing is best-effort: closing the session releases it anyway.
            }
            finally
            {
                await _connection.DisposeAsync();
            }
        }
    }
}
