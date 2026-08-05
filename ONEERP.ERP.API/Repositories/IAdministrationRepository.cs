using System.Data;
using ONEERP.ERP.API.Models;

namespace ONEERP.ERP.API.Repositories;

/// <summary>
/// Contract for currency master data persistence using Dapper.
/// </summary>
public interface IAdministrationRepository
{
    Task<IEnumerable<Currency>> GetAllAsync(bool includeInactive = false);
    Task<Currency?> GetByIdAsync(int id);
    Task<Currency?> GetByCodeAsync(string currencyCode);
    Task<int> InsertAsync(Currency entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> UpdateAsync(Currency entity, IDbConnection? connection = null, IDbTransaction? transaction = null);
    Task<bool> SoftDeleteAsync(int id, int? modifiedBy, IDbConnection? connection = null, IDbTransaction? transaction = null);
}
