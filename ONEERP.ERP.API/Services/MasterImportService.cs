using System.Globalization;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface IMasterImportService
{
    List<MasterImportMetaDto> GetMasters();
    Task<ImportPreviewResponse> PreviewAsync(string entityName, List<Dictionary<string, object?>> rows, long companyId);
    Task<ImportConfirmResponse> ConfirmAsync(ImportConfirmRequest request, ICurrentUser user);
}

public class MasterImportService : IMasterImportService
{
    private readonly ICurrentUser _currentUser;
    private readonly IImportLogService _importLogService;

    private readonly IProductService _productService;
    private readonly IProductCategoryService _productCategoryService;
    private readonly IProductSubCategoryService _productSubCategoryService;
    private readonly IProductBrandService _productBrandService;
    private readonly IProductUnitService _productUnitService;
    private readonly ITaxService _taxService;
    private readonly ITaxTypeSystemService _taxTypeSystemService;
    private readonly ICompanyService _companyService;
    private readonly IBranchService _branchService;
    private readonly IBusinessTypeService _businessTypeService;
    private readonly IIndustryTypeService _industryTypeService;
    private readonly IGstRegistrationTypeService _gstRegistrationTypeService;
    private readonly IAdministrationService _administrationService;
    private readonly ILanguageService _languageService;
    private readonly ITimeZoneService _timeZoneService;

    private readonly Dictionary<string, MasterImportMetaDto> _definitions;

    public MasterImportService(
        ICurrentUser currentUser,
        IImportLogService importLogService,
        IProductService productService,
        IProductCategoryService productCategoryService,
        IProductSubCategoryService productSubCategoryService,
        IProductBrandService productBrandService,
        IProductUnitService productUnitService,
        ITaxService taxService,
        ITaxTypeSystemService taxTypeSystemService,
        ICompanyService companyService,
        IBranchService branchService,
        IBusinessTypeService businessTypeService,
        IIndustryTypeService industryTypeService,
        IGstRegistrationTypeService gstRegistrationTypeService,
        IAdministrationService administrationService,
        ILanguageService languageService,
        ITimeZoneService timeZoneService)
    {
        _currentUser = currentUser;
        _importLogService = importLogService;
        _productService = productService;
        _productCategoryService = productCategoryService;
        _productSubCategoryService = productSubCategoryService;
        _productBrandService = productBrandService;
        _productUnitService = productUnitService;
        _taxService = taxService;
        _taxTypeSystemService = taxTypeSystemService;
        _companyService = companyService;
        _branchService = branchService;
        _businessTypeService = businessTypeService;
        _industryTypeService = industryTypeService;
        _gstRegistrationTypeService = gstRegistrationTypeService;
        _administrationService = administrationService;
        _languageService = languageService;
        _timeZoneService = timeZoneService;

        _definitions = BuildDefinitions();
    }

    /* ---------------- Metadata ---------------- */

    public List<MasterImportMetaDto> GetMasters() => _definitions.Values.ToList();

    private static Dictionary<string, MasterImportMetaDto> BuildDefinitions()
    {
        var defs = new Dictionary<string, MasterImportMetaDto>();

        defs["Products"] = new MasterImportMetaDto
        {
            Name = "Products",
            Label = "Products",
            Description = "Bulk import products with category, brand, UOM, tax and pricing.",
            Columns = new List<ImportColumnMetaDto>
            {
                Col("productCode", "Product Code", true, "text", unique: true),
                Col("productName", "Product Name", true),
                Col("categoryId", "Category", false, "reference", "ProductCategories", "categoryName"),
                Col("subCategoryId", "Sub Category", false, "reference", "ProductSubCategories", "subCategoryName"),
                Col("brandId", "Brand", false, "reference", "ProductBrands", "brandName"),
                Col("uomId", "UOM", true, "reference", "ProductUnits", "unitName"),
                Col("branchId", "Branch", false, "reference", "Branches", "branchName"),
                Col("taxId", "Tax", false, "reference", "Taxes", "taxName"),
                Col("sku", "SKU", false),
                Col("barcode", "Barcode", false),
                Col("mrp", "MRP", false, "number"),
                Col("purchasePrice", "Purchase Price", false, "number"),
                Col("salesPrice", "Sales Price", false, "number"),
                Col("isStockItem", "Stock Item", false, "boolean"),
                Col("isSaleable", "Saleable", false, "boolean"),
                Col("isPurchaseable", "Purchaseable", false, "boolean"),
                Col("description", "Description", false)
            }
        };

        defs["ProductCategories"] = new MasterImportMetaDto
        {
            Name = "ProductCategories",
            Label = "Product Categories",
            Description = "Bulk import product categories.",
            Columns = new List<ImportColumnMetaDto>
            {
                Col("categoryCode", "Category Code", true, "text", unique: true),
                Col("categoryName", "Category Name", true),
                Col("parentCategoryId", "Parent Category", false, "reference", "ProductCategories", "categoryName"),
                Col("description", "Description", false),
                Col("sortOrder", "Sort Order", false, "number")
            }
        };

        defs["ProductSubCategories"] = new MasterImportMetaDto
        {
            Name = "ProductSubCategories",
            Label = "Product Sub Categories",
            Description = "Bulk import product sub-categories.",
            Columns = new List<ImportColumnMetaDto>
            {
                Col("subCategoryCode", "Sub Category Code", true, "text", unique: true),
                Col("subCategoryName", "Sub Category Name", true),
                Col("categoryId", "Category", false, "reference", "ProductCategories", "categoryName"),
                Col("description", "Description", false),
                Col("sortOrder", "Sort Order", false, "number")
            }
        };

        defs["ProductBrands"] = new MasterImportMetaDto
        {
            Name = "ProductBrands",
            Label = "Product Brands",
            Description = "Bulk import product brands.",
            Columns = new List<ImportColumnMetaDto>
            {
                Col("brandCode", "Brand Code", true, "text", unique: true),
                Col("brandName", "Brand Name", true),
                Col("description", "Description", false)
            }
        };

        defs["ProductUnits"] = new MasterImportMetaDto
        {
            Name = "ProductUnits",
            Label = "Units",
            Description = "Bulk import measurement units.",
            Columns = new List<ImportColumnMetaDto>
            {
                Col("unitCode", "Unit Code", true, "text", unique: true),
                Col("unitName", "Unit Name", true),
                Col("symbol", "Symbol", false),
                Col("decimalPlaces", "Decimal Places", false, "number")
            }
        };

        defs["TaxTypeSystems"] = new MasterImportMetaDto
        {
            Name = "TaxTypeSystems",
            Label = "Tax Type Systems",
            Description = "Bulk import tax type systems.",
            Columns = new List<ImportColumnMetaDto>
            {
                Col("code", "Tax Type Code", true, "text", unique: true),
                Col("name", "Tax Type Name", true),
                Col("description", "Description", false)
            }
        };

        defs["Taxes"] = new MasterImportMetaDto
        {
            Name = "Taxes",
            Label = "Taxes",
            Description = "Bulk import taxes (company-scoped).",
            Columns = new List<ImportColumnMetaDto>
            {
                Col("taxCode", "Tax Code", true, "text", unique: true),
                Col("taxName", "Tax Name", true),
                Col("taxTypeId", "Tax Type", true, "reference", "TaxTypeSystems", "name"),
                Col("taxRate", "Tax Rate", false, "number"),
                Col("isInclusive", "Is Inclusive", false, "boolean"),
                Col("branchId", "Branch", false, "reference", "Branches", "branchName"),
                Col("effectiveFrom", "Effective From", false, "date"),
                Col("effectiveTo", "Effective To", false, "date"),
                Col("description", "Description", false)
            }
        };

        defs["Companies"] = new MasterImportMetaDto
        {
            Name = "Companies",
            Label = "Companies",
            Description = "Bulk import companies.",
            Columns = new List<ImportColumnMetaDto>
            {
                Col("companyCode", "Company Code", true, "text", unique: true),
                Col("companyName", "Company Name", true),
                Col("shortName", "Short Name", false),
                Col("abbreviation", "Abbreviation", false),
                Col("businessTypeId", "Business Type", true, "reference", "BusinessTypes", "name"),
                Col("industryTypeId", "Industry Type", true, "reference", "IndustryTypes", "name"),
                Col("gstRegistrationTypeId", "GST Registration Type", false, "reference", "GstRegistrationTypes", "name"),
                Col("gstNumber", "GST Number", false),
                Col("panNumber", "PAN Number", false),
                Col("tanNumber", "TAN Number", false),
                Col("cinNumber", "CIN Number", false),
                Col("registrationNumber", "Registration Number", false),
                Col("currencyId", "Currency", true, "reference", "Currencies", "currencyCode"),
                Col("languageId", "Language", true, "reference", "Languages", "name"),
                Col("timeZoneId", "Time Zone", true, "reference", "TimeZones", "name"),
                Col("multiBranchEnabled", "Multi Branch", false, "boolean"),
                Col("multiWarehouseEnabled", "Multi Warehouse", false, "boolean"),
                Col("multiCurrencyEnabled", "Multi Currency", false, "boolean")
            }
        };

        return defs;
    }

    private static ImportColumnMetaDto Col(
        string key, string label, bool required, string type = "text",
        string? referenceEntity = null, string? referenceDisplay = null, bool unique = false)
        => new ImportColumnMetaDto
        {
            Key = key,
            Label = label,
            Required = required,
            Type = type,
            ReferenceEntity = referenceEntity,
            ReferenceDisplay = referenceDisplay,
            Unique = unique
        };

    /* ---------------- Preview ---------------- */

    public async Task<ImportPreviewResponse> PreviewAsync(string entityName, List<Dictionary<string, object?>> rows, long companyId)
    {
        if (!_definitions.TryGetValue(entityName, out var def))
            throw new NotFoundException($"Import definition '{entityName}' was not found.");

        var referenceMaps = await LoadReferenceMaps(def, companyId);
        var seenUnique = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var results = new List<ImportRowResultDto>();

        for (var i = 0; i < rows.Count; i++)
        {
            var (resolved, errors) = ValidateRow(def.Columns, rows[i], referenceMaps, seenUnique);
            results.Add(new ImportRowResultDto
            {
                RowNumber = i + 1,
                Valid = errors.Count == 0,
                Data = resolved,
                Errors = errors
            });
        }

        return new ImportPreviewResponse { Rows = results };
    }

    private (Dictionary<string, object?> Resolved, List<string> Errors) ValidateRow(
        List<ImportColumnMetaDto> columns,
        Dictionary<string, object?> row,
        Dictionary<string, Dictionary<string, long>> referenceMaps,
        Dictionary<string, string> seenUnique)
    {
        var errors = new List<string>();
        var resolved = new Dictionary<string, object?>();

        foreach (var col in columns)
        {
            var raw = row.TryGetValue(col.Key, out var v) ? v : null;
            var isEmpty = raw is null || (raw is string s && string.IsNullOrWhiteSpace(s));

            if (isEmpty)
            {
                if (col.Required)
                    errors.Add($"{col.Label} is required.");
                else
                    resolved[col.Key] = null;
                continue;
            }

            if (col.ReferenceEntity is not null)
            {
                var map = referenceMaps.GetValueOrDefault(col.ReferenceEntity, new Dictionary<string, long>());
                var key = raw!.ToString()!.Trim().ToLowerInvariant();
                if (map.TryGetValue(key, out var id))
                    resolved[col.Key] = id;
                else
                    errors.Add($"Invalid {col.Label}: '{raw}'.");
                continue;
            }

            try
            {
                resolved[col.Key] = Coerce(raw!, col.Type);
            }
            catch
            {
                errors.Add($"Invalid {col.Label}: '{raw}' is not a valid {col.Type}.");
            }
        }

        foreach (var col in columns.Where(c => c.Unique))
        {
            var val = row.TryGetValue(col.Key, out var v) && v is not null ? v.ToString()!.Trim() : "";
            if (string.IsNullOrWhiteSpace(val)) continue;
            if (seenUnique.ContainsKey(val))
                errors.Add($"Duplicate {col.Label}: '{val}' in file.");
            else
                seenUnique[val] = val;
        }

        return (resolved, errors);
    }

    private static object Coerce(object raw, string type) => type switch
    {
        "number" => Convert.ToDecimal(raw),
        "boolean" => ParseBool(raw),
        "date" => DateTime.Parse(raw.ToString()!, CultureInfo.InvariantCulture),
        _ => raw.ToString()!
    };

    private static bool ParseBool(object raw)
    {
        var s = raw.ToString()!.Trim().ToLowerInvariant();
        return s is "true" or "1" or "yes" or "y";
    }

    /* ---------------- Confirm ---------------- */

    public async Task<ImportConfirmResponse> ConfirmAsync(ImportConfirmRequest request, ICurrentUser user)
    {
        if (!_definitions.TryGetValue(request.EntityName, out var def))
            throw new NotFoundException($"Import definition '{request.EntityName}' was not found.");

        var referenceMaps = await LoadReferenceMaps(def, user.CompanyId);
        var seenUnique = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var validRows = new List<Dictionary<string, object?>>();
        var response = new ImportConfirmResponse
        {
            TotalRows = request.Rows.Count,
            Status = "PROCESSING"
        };

        foreach (var row in request.Rows)
        {
            var (resolved, errors) = ValidateRow(def.Columns, row, referenceMaps, seenUnique);
            if (errors.Count == 0)
                validRows.Add(resolved);
        }

        foreach (var resolved in validRows)
        {
            try
            {
                await ApplyCreate(request.EntityName, resolved, user.CompanyId);
                response.SuccessRows++;
            }
            catch (Exception ex)
            {
                response.FailedRows++;
                response.Errors.Add($"Row skipped: {ex.Message}");
            }
        }

        response.FailedRows += (request.Rows.Count - validRows.Count);
        response.Status = response.FailedRows == 0 ? "COMPLETED"
            : response.SuccessRows == 0 ? "FAILED" : "PARTIAL";

        var log = new ImportLog
        {
            CompanyId = user.CompanyId,
            ImportType = "MASTER",
            ModuleName = def.Label,
            EntityName = request.EntityName,
            FileName = request.FileName,
            FileType = request.FileType,
            TotalRows = response.TotalRows,
            SuccessRows = response.SuccessRows,
            FailedRows = response.FailedRows,
            Status = response.Status,
            ImportedBy = user.UserId,
            ImportedAt = DateTime.UtcNow
        };

        try
        {
            log.Id = await _importLogService.CreateAsync(log);
            response.LogId = log.Id;
        }
        catch
        {
            /* logging must not fail the import */
        }

        return response;
    }

    /* ---------------- Apply (create) ---------------- */

    private async Task ApplyCreate(string entityName, Dictionary<string, object?> res, long companyId)
    {
        switch (entityName)
        {
            case "Products":
                await _productService.CreateAsync(companyId, new CreateProductRequest(
                    ProductCode: GetString(res, "productCode"),
                    ProductName: GetString(res, "productName"),
                    CompanyId: companyId,
                    CategoryId: GetLongOrNull(res, "categoryId"),
                    SubCategoryId: GetLongOrNull(res, "subCategoryId"),
                    BrandId: GetLongOrNull(res, "brandId"),
                    UOMId: GetLong(res, "uomId"),
                    BranchId: GetLongOrNull(res, "branchId"),
                    SKU: GetStringOrNull(res, "sku"),
                    Barcode: GetStringOrNull(res, "barcode"),
                    MRP: GetDecimalOrNull(res, "mrp"),
                    PurchasePrice: GetDecimalOrNull(res, "purchasePrice"),
                    SalesPrice: GetDecimalOrNull(res, "salesPrice"),
                    TaxId: GetLongOrNull(res, "taxId"),
                    IsStockItem: GetBool(res, "isStockItem", true),
                    IsSaleable: GetBool(res, "isSaleable", true),
                    IsPurchaseable: GetBool(res, "isPurchaseable", true),
                    Description: GetStringOrNull(res, "description")));
                break;

            case "ProductCategories":
                await _productCategoryService.CreateAsync(companyId, new CreateProductCategoryRequest(
                    CategoryCode: GetString(res, "categoryCode"),
                    CategoryName: GetString(res, "categoryName"),
                    Description: GetStringOrNull(res, "description"),
                    ParentCategoryId: GetLongOrNull(res, "parentCategoryId"),
                    SortOrder: GetIntOrNull(res, "sortOrder")));
                break;

            case "ProductSubCategories":
                await _productSubCategoryService.CreateAsync(companyId, new CreateProductSubCategoryRequest(
                    CategoryId: GetLongOrNull(res, "categoryId"),
                    SubCategoryCode: GetString(res, "subCategoryCode"),
                    SubCategoryName: GetString(res, "subCategoryName"),
                    Description: GetStringOrNull(res, "description"),
                    SortOrder: GetIntOrNull(res, "sortOrder")));
                break;

            case "ProductBrands":
                await _productBrandService.CreateAsync(companyId, new CreateProductBrandRequest(
                    BrandCode: GetString(res, "brandCode"),
                    BrandName: GetString(res, "brandName"),
                    Description: GetStringOrNull(res, "description")));
                break;

            case "ProductUnits":
                await _productUnitService.CreateAsync(companyId, new CreateProductUnitRequest(
                    UnitCode: GetString(res, "unitCode"),
                    UnitName: GetString(res, "unitName"),
                    Symbol: GetStringOrNull(res, "symbol"),
                    DecimalPlaces: GetIntOrNull(res, "decimalPlaces")));
                break;

            case "TaxTypeSystems":
                await _taxTypeSystemService.CreateAsync(new CreateTaxTypeSystemRequest
                {
                    Code = GetString(res, "code"),
                    Name = GetString(res, "name"),
                    Description = GetStringOrNull(res, "description")
                });
                break;

            case "Taxes":
                await _taxService.CreateAsync(companyId, new CreateTaxRequest
                {
                    BranchId = GetLongOrNull(res, "branchId"),
                    TaxTypeSystemId = GetLong(res, "taxTypeId"),
                    TaxCode = GetString(res, "taxCode"),
                    TaxName = GetString(res, "taxName"),
                    TaxRate = GetDecimal(res, "taxRate", 0),
                    IsInclusive = GetBool(res, "isInclusive", false),
                    EffectiveFrom = GetDateOrNull(res, "effectiveFrom"),
                    EffectiveTo = GetDateOrNull(res, "effectiveTo"),
                    Description = GetStringOrNull(res, "description")
                });
                break;

            case "Companies":
                await _companyService.CreateAsync(new CreateCompanyRequest(
                    CompanyCode: GetString(res, "companyCode"),
                    CompanyName: GetString(res, "companyName"),
                    ShortName: GetStringOrNull(res, "shortName"),
                    Abbreviation: GetStringOrNull(res, "abbreviation"),
                    BusinessTypeId: (int)GetLong(res, "businessTypeId"),
                    IndustryTypeId: (int)GetLong(res, "industryTypeId"),
                    GSTRegistrationTypeId: (int?)GetLongOrNull(res, "gstRegistrationTypeId"),
                    GSTNumber: GetStringOrNull(res, "gstNumber"),
                    PANNumber: GetStringOrNull(res, "panNumber"),
                    TANNumber: GetStringOrNull(res, "tanNumber"),
                    CINNumber: GetStringOrNull(res, "cinNumber"),
                    RegistrationNumber: GetStringOrNull(res, "registrationNumber"),
                    CurrencyId: (int)GetLong(res, "currencyId"),
                    LanguageId: (int)GetLong(res, "languageId"),
                    TimeZoneId: (int)GetLong(res, "timeZoneId"),
                    CompanyGroupId: GetIntOrNull(res, "companyGroupId"),
                    BusinessUnitId: GetIntOrNull(res, "businessUnitId"),
                    Website: GetStringOrNull(res, "website"),
                    Email: GetStringOrNull(res, "email"),
                    Phone: GetStringOrNull(res, "phone"),
                    Mobile: GetStringOrNull(res, "mobile"),
                    LogoUrl: GetStringOrNull(res, "logoUrl"),
                    DefaultFinancialYearId: GetIntOrNull(res, "defaultFinancialYearId"),
                    MultiBranchEnabled: GetBool(res, "multiBranchEnabled", false),
                    MultiWarehouseEnabled: GetBool(res, "multiWarehouseEnabled", false),
                    MultiCurrencyEnabled: GetBool(res, "multiCurrencyEnabled", false),
                    DateFormat: GetStringOrNull(res, "dateFormat"),
                    TimeFormat: GetStringOrNull(res, "timeFormat"),
                    NumberFormat: GetStringOrNull(res, "numberFormat"),
                    DefaultWarehouseId: GetIntOrNull(res, "defaultWarehouseId"),
                    Theme: GetStringOrNull(res, "theme"),
                    PrimaryColor: GetStringOrNull(res, "primaryColor"),
                    SecondaryColor: GetStringOrNull(res, "secondaryColor"),
                    Remarks: GetStringOrNull(res, "remarks")));
                break;

            default:
                throw new NotFoundException($"Import for '{entityName}' is not supported.");
        }
    }

    /* ---------------- Reference maps ---------------- */

    private async Task<Dictionary<string, Dictionary<string, long>>> LoadReferenceMaps(MasterImportMetaDto def, long companyId)
    {
        var needed = def.Columns
            .Where(c => c.ReferenceEntity is not null)
            .Select(c => c.ReferenceEntity!)
            .Distinct()
            .ToList();

        var maps = new Dictionary<string, Dictionary<string, long>>();
        foreach (var refEntity in needed)
            maps[refEntity] = await LoadRefMap(refEntity, companyId);
        return maps;
    }

    private async Task<Dictionary<string, long>> LoadRefMap(string refEntity, long companyId)
    {
        switch (refEntity)
        {
            case "ProductCategories":
                {
                    var items = await _productCategoryService.GetAllAsync(companyId, true);
                    return ToMap(items, x => x.Id, x => x.CategoryName, x => x.CategoryCode);
                }
            case "ProductSubCategories":
                {
                    var items = await _productSubCategoryService.GetAllAsync(companyId, true);
                    return ToMap(items, x => x.Id, x => x.SubCategoryName);
                }
            case "ProductBrands":
                {
                    var items = await _productBrandService.GetAllAsync(companyId, true);
                    return ToMap(items, x => x.Id, x => x.BrandName);
                }
            case "ProductUnits":
                {
                    var items = await _productUnitService.GetAllAsync(companyId, true);
                    return ToMap(items, x => x.Id, x => x.UnitName);
                }
            case "Taxes":
                {
                    var items = (await _taxService.GetPagedAsync(companyId, 1, 10000, "")).Items;
                    return ToMap(items, x => x.Id, x => x.TaxName);
                }
            case "TaxTypeSystems":
                {
                    var items = await _taxTypeSystemService.GetAllAsync(true);
                    return ToMap(items, x => x.Id, x => x.Name, x => x.Code);
                }
            case "Branches":
                {
                    var items = (await _branchService.GetPagedAsync((int)companyId, 1, 10000, "")).Items;
                    return ToMap(items, x => x.Id, x => x.BranchName);
                }
            case "BusinessTypes":
                {
                    var items = await _businessTypeService.GetAllAsync(true);
                    return ToMap(items, x => x.BusinessTypeId, x => x.Name);
                }
            case "IndustryTypes":
                {
                    var items = await _industryTypeService.GetAllAsync(true);
                    return ToMap(items, x => x.IndustryTypeId, x => x.Name);
                }
            case "GstRegistrationTypes":
                {
                    var items = await _gstRegistrationTypeService.GetAllAsync(true);
                    return ToMap(items, x => x.GstRegistrationTypeId, x => x.Name);
                }
            case "Currencies":
                {
                    var items = await _administrationService.GetAllAsync(true);
                    return ToMap(items, x => x.Id, x => x.CurrencyCode);
                }
            case "Languages":
                {
                    var items = await _languageService.GetAllAsync(true);
                    return ToMap(items, x => x.LanguageId, x => x.Name);
                }
            case "TimeZones":
                {
                    var items = await _timeZoneService.GetAllAsync(true);
                    return ToMap(items, x => x.TimeZoneId, x => x.Name);
                }
            default:
                return new Dictionary<string, long>();
        }
    }

    private static Dictionary<string, long> ToMap<T>(
        IEnumerable<T> items, Func<T, object> id, params Func<T, string?>[] displays)
    {
        var map = new Dictionary<string, long>();
        foreach (var it in items)
        {
            var idVal = Convert.ToInt64(id(it));
            foreach (var d in displays)
            {
                var s = d(it)?.Trim().ToLowerInvariant();
                if (!string.IsNullOrEmpty(s) && !map.ContainsKey(s))
                    map[s] = idVal;
            }
        }
        return map;
    }

    /* ---------------- Helpers ---------------- */

    private static string GetString(Dictionary<string, object?> d, string k)
        => d.TryGetValue(k, out var v) && v is not null ? v.ToString()! : "";

    private static string? GetStringOrNull(Dictionary<string, object?> d, string k)
        => d.TryGetValue(k, out var v) && v is not null ? v.ToString() : null;

    private static long? GetLongOrNull(Dictionary<string, object?> d, string k)
        => d.TryGetValue(k, out var v) && v is not null ? Convert.ToInt64(v) : null;

    private static long GetLong(Dictionary<string, object?> d, string k)
        => d.TryGetValue(k, out var v) && v is not null ? Convert.ToInt64(v) : 0;

    private static int? GetIntOrNull(Dictionary<string, object?> d, string k)
        => d.TryGetValue(k, out var v) && v is not null ? Convert.ToInt32(v) : null;

    private static decimal? GetDecimalOrNull(Dictionary<string, object?> d, string k)
        => d.TryGetValue(k, out var v) && v is not null ? Convert.ToDecimal(v) : null;

    private static decimal GetDecimal(Dictionary<string, object?> d, string k, decimal def)
        => d.TryGetValue(k, out var v) && v is not null ? Convert.ToDecimal(v) : def;

    private static bool GetBool(Dictionary<string, object?> d, string k, bool def)
    {
        if (d.TryGetValue(k, out var v) && v is not null)
        {
            if (v is bool b) return b;
            return ParseBool(v);
        }
        return def;
    }

    private static DateTime? GetDateOrNull(Dictionary<string, object?> d, string k)
        => d.TryGetValue(k, out var v) && v is not null
            ? DateTime.Parse(v.ToString()!, CultureInfo.InvariantCulture)
            : null;
}
