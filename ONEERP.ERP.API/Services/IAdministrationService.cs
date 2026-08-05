using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Requests;

namespace ONEERP.ERP.API.Services;

/// <summary>
/// Application service contract for currency administration.
/// Contains business validation; delegates persistence to the repository.
/// </summary>
public interface IAdministrationService
{
    Task<IEnumerable<CurrencyDto>> GetAllAsync(bool includeInactive = false);
    Task<CurrencyDto> GetByIdAsync(int id);
    Task<CurrencyDto> CreateAsync(SaveCurrencyRequest request);
    Task<CurrencyDto> UpdateAsync(int id, SaveCurrencyRequest request);
    Task<bool> DeleteAsync(int id);
}
