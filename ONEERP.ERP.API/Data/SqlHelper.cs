using System.Data;
using Dapper;

namespace ONEERP.ERP.API.Data;

/// <summary>
/// Thin, fully-asynchronous Dapper wrapper used by every repository.
/// All queries are parameterized, which prevents SQL injection.
/// </summary>
public interface ISqlHelper
{
    Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object? parameters = null, IDbTransaction? transaction = null, int? commandTimeout = null);
    Task<T?> QuerySingleOrDefaultAsync<T>(IDbConnection connection, string sql, object? parameters = null, IDbTransaction? transaction = null, int? commandTimeout = null);
    Task<T?> QueryFirstOrDefaultAsync<T>(IDbConnection connection, string sql, object? parameters = null, IDbTransaction? transaction = null, int? commandTimeout = null);
    Task<int> ExecuteAsync(IDbConnection connection, string sql, object? parameters = null, IDbTransaction? transaction = null, int? commandTimeout = null);
    Task<T?> ExecuteScalarAsync<T>(IDbConnection connection, string sql, object? parameters = null, IDbTransaction? transaction = null, int? commandTimeout = null);
}

public class SqlHelper : ISqlHelper
{
    public Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object? parameters = null, IDbTransaction? transaction = null, int? commandTimeout = null)
        => connection.QueryAsync<T>(sql, parameters, transaction, commandTimeout);

    public Task<T?> QuerySingleOrDefaultAsync<T>(IDbConnection connection, string sql, object? parameters = null, IDbTransaction? transaction = null, int? commandTimeout = null)
        => connection.QuerySingleOrDefaultAsync<T>(sql, parameters, transaction, commandTimeout);

    public Task<T?> QueryFirstOrDefaultAsync<T>(IDbConnection connection, string sql, object? parameters = null, IDbTransaction? transaction = null, int? commandTimeout = null)
        => connection.QueryFirstOrDefaultAsync<T>(sql, parameters, transaction, commandTimeout);

    public Task<int> ExecuteAsync(IDbConnection connection, string sql, object? parameters = null, IDbTransaction? transaction = null, int? commandTimeout = null)
        => connection.ExecuteAsync(sql, parameters, transaction, commandTimeout);

    public Task<T?> ExecuteScalarAsync<T>(IDbConnection connection, string sql, object? parameters = null, IDbTransaction? transaction = null, int? commandTimeout = null)
        => connection.ExecuteScalarAsync<T>(sql, parameters, transaction, commandTimeout);
}
