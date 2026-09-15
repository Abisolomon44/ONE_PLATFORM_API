using ONEERP.ERP.API.DTOs;

namespace ONEERP.ERP.API.Services;

/// <summary>
/// A resolved reference map for one master-data entity:
///   Map     – display value -> id (used by import validation / lookup)
///   Options – distinct display values (used for template dropdowns)
///   Display – id -> display value (used by export to render human-readable values)
/// </summary>
public sealed class RefLookup
{
    public Dictionary<string, long> Map { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Options { get; } = new();
    public Dictionary<long, string> Display { get; } = new();
}

public interface IMasterReferenceCatalog
{
    /// <summary>Loads the reference maps referenced by the given master definition.</summary>
    Task<Dictionary<string, RefLookup>> LoadReferenceMapsAsync(MasterImportMetaDto def, long companyId);

    /// <summary>Loads a single reference map by reference entity name.</summary>
    Task<RefLookup> LoadRefLookupAsync(string refEntity, long companyId);
}

public class MasterReferenceCatalog : IMasterReferenceCatalog
{
    private readonly IProductCategoryService _productCategoryService;
    private readonly IProductSubCategoryService _productSubCategoryService;
    private readonly IProductBrandService _productBrandService;
    private readonly IProductUnitService _productUnitService;
    private readonly ITaxService _taxService;
    private readonly ITaxTypeSystemService _taxTypeSystemService;
    private readonly IBranchService _branchService;
    private readonly ICompanyService _companyService;
    private readonly IBusinessTypeService _businessTypeService;
    private readonly IIndustryTypeService _industryTypeService;
    private readonly IGstRegistrationTypeService _gstRegistrationTypeService;
    private readonly IAdministrationService _administrationService;
    private readonly ILanguageService _languageService;
    private readonly ITimeZoneService _timeZoneService;

    public MasterReferenceCatalog(
        IProductCategoryService productCategoryService,
        IProductSubCategoryService productSubCategoryService,
        IProductBrandService productBrandService,
        IProductUnitService productUnitService,
        ITaxService taxService,
        ITaxTypeSystemService taxTypeSystemService,
        IBranchService branchService,
        ICompanyService companyService,
        IBusinessTypeService businessTypeService,
        IIndustryTypeService industryTypeService,
        IGstRegistrationTypeService gstRegistrationTypeService,
        IAdministrationService administrationService,
        ILanguageService languageService,
        ITimeZoneService timeZoneService)
    {
        _productCategoryService = productCategoryService;
        _productSubCategoryService = productSubCategoryService;
        _productBrandService = productBrandService;
        _productUnitService = productUnitService;
        _taxService = taxService;
        _taxTypeSystemService = taxTypeSystemService;
        _branchService = branchService;
        _companyService = companyService;
        _businessTypeService = businessTypeService;
        _industryTypeService = industryTypeService;
        _gstRegistrationTypeService = gstRegistrationTypeService;
        _administrationService = administrationService;
        _languageService = languageService;
        _timeZoneService = timeZoneService;
    }

    public async Task<Dictionary<string, RefLookup>> LoadReferenceMapsAsync(MasterImportMetaDto def, long companyId)
    {
        var needed = def.Columns
            .Where(c => c.ReferenceEntity is not null)
            .Select(c => c.ReferenceEntity!)
            .Distinct()
            .ToList();

        var maps = new Dictionary<string, RefLookup>();
        foreach (var refEntity in needed)
            maps[refEntity] = await LoadRefLookupAsync(refEntity, companyId);
        return maps;
    }

    public async Task<RefLookup> LoadRefLookupAsync(string refEntity, long companyId)
    {
        switch (refEntity)
        {
            case "ProductCategories":
                {
                    var items = await _productCategoryService.GetAllAsync(companyId, true);
                    return BuildLookup(items, x => x.Id, x => x.CategoryName, x => x.CategoryCode);
                }
            case "ProductSubCategories":
                {
                    var items = await _productSubCategoryService.GetAllAsync(companyId, true);
                    return BuildLookup(items, x => x.Id, x => x.SubCategoryName);
                }
            case "ProductBrands":
                {
                    var items = await _productBrandService.GetAllAsync(companyId, true);
                    return BuildLookup(items, x => x.Id, x => x.BrandName);
                }
            case "ProductUnits":
                {
                    var items = await _productUnitService.GetAllAsync(companyId, true);
                    return BuildLookup(items, x => x.Id, x => x.UnitName);
                }
            case "Taxes":
                {
                    var items = (await _taxService.GetPagedAsync(companyId, 1, 10000, "")).Items;
                    return BuildLookup(items, x => x.Id, x => x.TaxName);
                }
            case "TaxTypeSystems":
                {
                    var items = await _taxTypeSystemService.GetAllAsync(true);
                    return BuildLookup(items, x => x.Id, x => x.Name, x => x.Code);
                }
            case "Branches":
                {
                    var items = (await _branchService.GetPagedAsync((int)companyId, 1, 10000, "")).Items;
                    return BuildLookup(items, x => x.Id, x => x.BranchName);
                }
            case "Companies":
                {
                    var items = (await _companyService.GetPagedAsync(1, 10000, "")).Items;
                    return BuildLookup(items, x => x.Id, x => x.CompanyName, x => x.CompanyCode);
                }
            case "BusinessTypes":
                {
                    var items = await _businessTypeService.GetAllAsync(true);
                    return BuildLookup(items, x => x.BusinessTypeId, x => x.Name);
                }
            case "IndustryTypes":
                {
                    var items = await _industryTypeService.GetAllAsync(true);
                    return BuildLookup(items, x => x.IndustryTypeId, x => x.Name);
                }
            case "GstRegistrationTypes":
                {
                    var items = await _gstRegistrationTypeService.GetAllAsync(true);
                    return BuildLookup(items, x => x.GstRegistrationTypeId, x => x.Name);
                }
            case "Currencies":
                {
                    var items = await _administrationService.GetAllAsync(true);
                    return BuildLookup(items, x => x.Id, x => x.CurrencyCode);
                }
            case "Languages":
                {
                    var items = await _languageService.GetAllAsync(true);
                    return BuildLookup(items, x => x.LanguageId, x => x.Name);
                }
            case "TimeZones":
                {
                    var items = await _timeZoneService.GetAllAsync(true);
                    return BuildLookup(items, x => x.TimeZoneId, x => x.Name);
                }
            default:
                return new RefLookup();
        }
    }

    private static RefLookup BuildLookup<T>(
        IEnumerable<T> items, Func<T, object> id, params Func<T, string?>[] displays)
    {
        var lu = new RefLookup();
        foreach (var it in items)
        {
            var idVal = Convert.ToInt64(id(it));
            lu.Map[idVal.ToString()] = idVal;
            string? firstDisplay = null;
            foreach (var d in displays)
            {
                var s = d(it)?.Trim();
                if (string.IsNullOrEmpty(s)) continue;
                firstDisplay ??= s;
                lu.Map.TryAdd(s.ToLowerInvariant(), idVal);
                if (!lu.Options.Contains(s, StringComparer.OrdinalIgnoreCase))
                    lu.Options.Add(s);
            }
            if (firstDisplay is not null)
                lu.Display.TryAdd(idVal, firstDisplay);
        }
        lu.Options.Sort(StringComparer.OrdinalIgnoreCase);
        return lu;
    }
}