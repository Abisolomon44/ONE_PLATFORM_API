using System.Globalization;
using ClosedXML.Excel;
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
    Task<ImportPreviewResponse> PreviewAsync(string entityName, List<Dictionary<string, string>> rows, long companyId);
    Task<ImportConfirmResponse> ConfirmAsync(ImportConfirmRequest request, ICurrentUser user);
    Task<byte[]> GenerateTemplateAsync(string entityName, long companyId);
}

public class MasterImportService : IMasterImportService
{
    private readonly ICurrentUser _currentUser;
    private readonly IImportLogService _importLogService;
    private readonly IMasterReferenceCatalog _referenceCatalog;

    private readonly IProductService _productService;
    private readonly IProductCategoryService _productCategoryService;
    private readonly IProductSubCategoryService _productSubCategoryService;
    private readonly IProductBrandService _productBrandService;
    private readonly IProductUnitService _productUnitService;
    private readonly ITaxService _taxService;
    private readonly ITaxTypeSystemService _taxTypeSystemService;
    private readonly ICompanyService _companyService;

    private readonly Dictionary<string, MasterImportMetaDto> _definitions;

    public MasterImportService(
        ICurrentUser currentUser,
        IImportLogService importLogService,
        IMasterReferenceCatalog referenceCatalog,
        IProductService productService,
        IProductCategoryService productCategoryService,
        IProductSubCategoryService productSubCategoryService,
        IProductBrandService productBrandService,
        IProductUnitService productUnitService,
        ITaxService taxService,
        ITaxTypeSystemService taxTypeSystemService,
        ICompanyService companyService)
    {
        _currentUser = currentUser;
        _importLogService = importLogService;
        _referenceCatalog = referenceCatalog;
        _productService = productService;
        _productCategoryService = productCategoryService;
        _productSubCategoryService = productSubCategoryService;
        _productBrandService = productBrandService;
        _productUnitService = productUnitService;
        _taxService = taxService;
        _taxTypeSystemService = taxTypeSystemService;
        _companyService = companyService;

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

    public async Task<ImportPreviewResponse> PreviewAsync(string entityName, List<Dictionary<string, string>> rows, long companyId)
    {
        if (!_definitions.TryGetValue(entityName, out var def))
            throw new NotFoundException($"Import definition '{entityName}' was not found.");

        var referenceMaps = await _referenceCatalog.LoadReferenceMapsAsync(def, companyId);
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
        Dictionary<string, string> row,
        Dictionary<string, RefLookup> referenceMaps,
        Dictionary<string, string> seenUnique)
    {
        var errors = new List<string>();
        var resolved = new Dictionary<string, object?>();

        foreach (var col in columns)
        {
            var raw = ReadValue(row, col);
            var isEmpty = string.IsNullOrWhiteSpace(raw)
                || IsNullishToken(raw)
                || (col.ReferenceEntity is not null && raw == "0");

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
                var lu = referenceMaps.GetValueOrDefault(col.ReferenceEntity);
                var key = raw!.ToLowerInvariant();
                if (lu is not null && lu.Map.TryGetValue(key, out var id))
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
            var val = row.TryGetValue(col.Key, out var u) && u is not null ? u.Trim() : "";
            if (string.IsNullOrWhiteSpace(val)) continue;
            if (seenUnique.ContainsKey(val))
                errors.Add($"Duplicate {col.Label}: '{val}' in file.");
            else
                seenUnique[val] = val;
        }

        return (resolved, errors);
    }

    private static string? ReadValue(Dictionary<string, string> row, ImportColumnMetaDto col)
    {
        if (row.TryGetValue(col.Key, out var v) && !string.IsNullOrWhiteSpace(v))
            return v.Trim();
        if (row.TryGetValue(col.Label, out var v2) && !string.IsNullOrWhiteSpace(v2))
            return v2.Trim();
        return null;
    }

    private static bool IsNullishToken(string raw)
        => raw.ToLowerInvariant() is "null" or "nil" or "n/a" or "na" or "none";

    private static object Coerce(string raw, string type) => type switch
    {
        "number" => decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var dec)
            ? dec
            : throw new FormatException("number"),
        "boolean" => ParseBool(raw),
        "date" => DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
            ? dt
            : throw new FormatException("date"),
        _ => raw
    };

    private static bool ParseBool(string raw)
    {
        var s = raw.Trim().ToLowerInvariant();
        return s is "true" or "1" or "yes" or "y";
    }

    /* ---------------- Confirm ---------------- */

    public async Task<ImportConfirmResponse> ConfirmAsync(ImportConfirmRequest request, ICurrentUser user)
    {
        if (!_definitions.TryGetValue(request.EntityName, out var def))
            throw new NotFoundException($"Import definition '{request.EntityName}' was not found.");

var referenceMaps = await _referenceCatalog.LoadReferenceMapsAsync(def, user.CompanyId);
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

    /* ---------------- Template (xlsx) ---------------- */

    public async Task<byte[]> GenerateTemplateAsync(string entityName, long companyId)
    {
        if (!_definitions.TryGetValue(entityName, out var def))
            throw new NotFoundException($"Import definition '{entityName}' was not found.");

        var referenceMaps = await _referenceCatalog.LoadReferenceMapsAsync(def, companyId);
        var refOrder = referenceMaps.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();

        using var wb = new XLWorkbook();
        var dataWs = wb.Worksheets.Add("Import");
        var refWs = wb.Worksheets.Add("References");
        refWs.Visibility = XLWorksheetVisibility.VeryHidden;

        for (var i = 0; i < def.Columns.Count; i++)
        {
            var col = def.Columns[i];
            var cell = dataWs.Cell(1, i + 1);
            cell.Value = col.Key;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = col.Required ? XLColor.Red : XLColor.Black;
        }

        for (var i = 0; i < refOrder.Count; i++)
        {
            var entity = refOrder[i];
            var lu = referenceMaps[entity];
            refWs.Cell(1, i + 1).Value = entity;
            for (var j = 0; j < lu.Options.Count; j++)
                refWs.Cell(2 + j, i + 1).Value = lu.Options[j];
        }

        const int dataRows = 5000;
        foreach (var col in def.Columns.Where(c => c.ReferenceEntity is not null))
        {
            var colId = def.Columns.IndexOf(col) + 1;
            var entity = col.ReferenceEntity!;
            if (!referenceMaps.TryGetValue(entity, out var lu) || lu.Options.Count == 0) continue;

            var refCol = refOrder.IndexOf(entity) + 1;
            var options = refWs.Range(2, refCol, 1 + lu.Options.Count, refCol);
            var dv = dataWs.Range(2, colId, 2 + dataRows - 1, colId).CreateDataValidation();
            dv.List(options, true);
            dv.ShowErrorMessage = true;
            dv.ErrorStyle = XLErrorStyle.Stop;
            dv.ErrorMessage = $"Please select a valid {col.Label} from the list.";
        }

        dataWs.SheetView.FreezeRows(1);
        dataWs.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
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
            return ParseBool(v.ToString()!);
        }
        return def;
    }

    private static DateTime? GetDateOrNull(Dictionary<string, object?> d, string k)
        => d.TryGetValue(k, out var v) && v is not null
            ? v is DateTime dt ? dt : DateTime.Parse(v.ToString()!, CultureInfo.InvariantCulture)
            : null;
}
