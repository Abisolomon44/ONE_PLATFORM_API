using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using ONEERP.ERP.API.DTOs;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface IMasterExportService
{
    Task<MasterExportMetaDto> GetMetaAsync(string entityName);
    Task<Dictionary<string, List<ExportFilterOptionDto>>> GetFilterOptionsAsync(string entityName, long companyId);
    Task<ExportPreviewResponseDto> PreviewAsync(ExportQueryDto query, long companyId);
    Task<ExportFileResultDto> ExportAsync(ExportQueryDto query, long companyId);
}

public class MasterExportService : IMasterExportService
{
    private const int PageFetchSize = 1000;
    private const int MaxExportRows = 100_000;

    private readonly IMasterImportService _importService;
    private readonly IMasterReferenceCatalog _referenceCatalog;

    private readonly IProductService _productService;
    private readonly IProductCategoryService _productCategoryService;
    private readonly IProductSubCategoryService _productSubCategoryService;
    private readonly IProductBrandService _productBrandService;
    private readonly IProductUnitService _productUnitService;
    private readonly ITaxService _taxService;
    private readonly ITaxTypeSystemService _taxTypeSystemService;
    private readonly ICompanyService _companyService;

    public MasterExportService(
        IMasterImportService importService,
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
        _importService = importService;
        _referenceCatalog = referenceCatalog;
        _productService = productService;
        _productCategoryService = productCategoryService;
        _productSubCategoryService = productSubCategoryService;
        _productBrandService = productBrandService;
        _productUnitService = productUnitService;
        _taxService = taxService;
        _taxTypeSystemService = taxTypeSystemService;
        _companyService = companyService;
    }

    /* ================================================================
       Metadata
       ================================================================ */

    public Task<MasterExportMetaDto> GetMetaAsync(string entityName)
    {
        var def = GetDef(entityName);
        return Task.FromResult(new MasterExportMetaDto
        {
            Name = def.Name,
            Label = def.Label,
            Description = def.Description,
            Columns = def.Columns,
            Filters = BuildFilterMeta(entityName)
        });
    }

    public async Task<Dictionary<string, List<ExportFilterOptionDto>>> GetFilterOptionsAsync(string entityName, long companyId)
    {
        var filters = BuildFilterMeta(entityName);
        var result = new Dictionary<string, List<ExportFilterOptionDto>>();

        foreach (var f in filters)
        {
            var opts = new List<ExportFilterOptionDto>();
            switch (f.Type)
            {
                case "reference":
                    {
                        var lu = await _referenceCatalog.LoadRefLookupAsync(f.ReferenceEntity!, companyId);
                        foreach (var (id, display) in lu.Display.OrderBy(kv => kv.Value, StringComparer.OrdinalIgnoreCase))
                            opts.Add(new ExportFilterOptionDto { Value = id.ToString(), Label = display });
                    }
                    break;
                case "status":
                    opts.Add(new ExportFilterOptionDto { Value = "true", Label = "Active" });
                    opts.Add(new ExportFilterOptionDto { Value = "false", Label = "Inactive" });
                    break;
            }
            result[f.Key] = opts;
        }
        return result;
    }

    /* ================================================================
       Preview / Export
       ================================================================ */

    public async Task<ExportPreviewResponseDto> PreviewAsync(ExportQueryDto query, long companyId)
    {
        var (cols, rows) = await BuildRowsAsync(query, companyId);
        var page = query.Page < 1 ? 1 : query.Page;
        var size = query.PageSize < 1 ? 50 : Math.Min(query.PageSize, 200);
        var paged = rows.Skip((page - 1) * size).Take(size).ToList();

        return new ExportPreviewResponseDto
        {
            Columns = cols,
            Rows = paged,
            Page = page,
            PageSize = size,
            TotalCount = rows.Count,
            TotalPages = rows.Count == 0 ? 0 : (int)Math.Ceiling(rows.Count / (double)size)
        };
    }

    public async Task<ExportFileResultDto> ExportAsync(ExportQueryDto query, long companyId)
    {
        var def = GetDef(query.EntityName);
        var (cols, rows) = await BuildRowsAsync(query, companyId);
        var format = (query.Format ?? "xlsx").ToLowerInvariant();

        var fileNameBase = $"{def.Name}_export";
        if (format == "csv")
        {
            return new ExportFileResultDto
            {
                Bytes = BuildCsv(cols, rows, query.IncludeHeaders),
                ContentType = "text/csv; charset=utf-8",
                FileName = $"{fileNameBase}.csv"
            };
        }

        return new ExportFileResultDto
        {
            Bytes = BuildXlsx(cols, rows, query.IncludeHeaders),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = $"{fileNameBase}.xlsx"
        };
    }

    /* ================================================================
       Core: build rows from raw data + filters + display resolution
       ================================================================ */

    private async Task<(List<ImportColumnMetaDto> Cols, List<Dictionary<string, object?>> Rows)>
        BuildRowsAsync(ExportQueryDto query, long companyId)
    {
        var def = GetDef(query.EntityName);
        var rawRows = await LoadRawRowsAsync(query.EntityName, companyId, query.IncludeInactive);
        var filterDefs = BuildFilterMeta(query.EntityName);

        // Filter
        var filtered = rawRows.Where(r => Matches(r, filterDefs, query.Filters, query.Search)).ToList();

        // Column selection
        var selectedCols = def.Columns
            .Where(c => query.Columns.Count == 0 || query.Columns.Contains(c.Key))
            .ToList();

        // Display-name resolution for reference columns
        if (query.UseDisplayNames && selectedCols.Any(c => c.ReferenceEntity is not null))
        {
            var refMaps = await _referenceCatalog.LoadReferenceMapsAsync(def, companyId);
            foreach (var row in filtered)
            {
                foreach (var col in selectedCols.Where(c => c.ReferenceEntity is not null))
                {
                    if (!row.TryGetValue(col.Key, out var idVal) || idVal is null) continue;
                    if (refMaps.TryGetValue(col.ReferenceEntity!, out var lu) && idVal is long id)
                        row[col.Key] = lu.Display.TryGetValue(id, out var display) ? display : null;
                }
            }
        }

        // Drop fully-empty columns if requested
        if (!query.IncludeEmptyColumns)
        {
            var empty = selectedCols
                .Where(c => filtered.All(r => IsEmptyValue(r.TryGetValue(c.Key, out var v) ? v : null)))
                .Select(c => c.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            selectedCols = selectedCols.Where(c => !empty.Contains(c.Key)).ToList();
        }

        return (selectedCols, filtered);
    }

    /* ================================================================
       Filter matching
       ================================================================ */

    private static bool Matches(
        Dictionary<string, object?> row,
        List<ExportFilterMetaDto> filterDefs,
        Dictionary<string, string> filters,
        string? search)
    {
        foreach (var f in filterDefs)
        {
            var raw = filters.TryGetValue(f.Key, out var val) ? val : null;
            if (string.IsNullOrWhiteSpace(raw)) continue;

            switch (f.Type)
            {
                case "status":
                    var isActive = row.TryGetValue("__isActive", out var a) && a is true;
                    if (bool.TryParse(raw, out var wantActive) && isActive != wantActive) return false;
                    break;

                case "date":
                    if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var wantDate))
                    {
                        if (row.TryGetValue(f.Key, out var dv) && dv is DateTime dt)
                        {
                            if (dt.Date != wantDate.Date) return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;

                case "reference":
                default:
                    // text / reference / anything else: exact match on string representation
                    var v = row.TryGetValue(f.Key, out var rv) ? rv : null;
                    var cellStr = v switch
                    {
                        null => "",
                        long l => l.ToString(),
                        int i => i.ToString(),
                        _ => v?.ToString() ?? ""
                    };
                    if (!string.Equals(cellStr, raw, StringComparison.OrdinalIgnoreCase))
                        return false;
                    break;
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var needle = search.Trim().ToLowerInvariant();
            var haystack = row.TryGetValue("__searchText", out var sv) ? sv?.ToString() ?? "" : "";
            if (!haystack.Contains(needle, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private static bool IsEmptyValue(object? v) => v switch
    {
        null => true,
        string s => string.IsNullOrWhiteSpace(s),
        bool => false,
        _ => false
    };

    /* ================================================================
       Load raw rows per entity
       ================================================================ */

    private async Task<List<Dictionary<string, object?>>> LoadRawRowsAsync(string entityName, long companyId, bool includeInactive)
    {
        return entityName switch
        {
            "Products" => await LoadProductsRaw(companyId, includeInactive),
            "ProductCategories" => LoadCategoriesRaw(await _productCategoryService.GetAllAsync(companyId, includeInactive), companyId),
            "ProductSubCategories" => LoadSubCategoriesRaw(await _productSubCategoryService.GetAllAsync(companyId, includeInactive), companyId),
            "ProductBrands" => LoadBrandsRaw(await _productBrandService.GetAllAsync(companyId, includeInactive), companyId),
            "ProductUnits" => LoadUnitsRaw(await _productUnitService.GetAllAsync(companyId, includeInactive), companyId),
            "TaxTypeSystems" => LoadTaxTypeSystemsRaw(await _taxTypeSystemService.GetAllAsync(includeInactive)),
            "Taxes" => await LoadTaxesRaw(companyId, includeInactive),
            "Companies" => await LoadCompaniesRaw(includeInactive),
            _ => throw new NotFoundException($"Export for '{entityName}' is not supported.")
        };
    }

    private async Task<List<Dictionary<string, object?>>> LoadProductsRaw(long companyId, bool includeInactive)
    {
        var all = await LoadPagedAsync(p => _productService.GetPagedAsync(companyId, p, PageFetchSize, ""));
        var rows = new List<Dictionary<string, object?>>();
        foreach (var p in all)
        {
            if (!includeInactive && !p.IsActive) continue;
            rows.Add(new Dictionary<string, object?>
            {
                ["productCode"] = p.ProductCode,
                ["productName"] = p.ProductName,
                ["categoryId"] = p.CategoryId,
                ["subCategoryId"] = p.SubCategoryId,
                ["brandId"] = p.BrandId,
                ["uomId"] = p.UOMId > 0 ? (long?)p.UOMId : null,
                ["branchId"] = p.BranchId,
                ["taxId"] = p.TaxId,
                ["sku"] = p.SKU,
                ["barcode"] = p.Barcode,
                ["mrp"] = p.MRP,
                ["purchasePrice"] = p.PurchasePrice,
                ["salesPrice"] = p.SalesPrice,
                ["isStockItem"] = p.IsStockItem,
                ["isSaleable"] = p.IsSaleable,
                ["isPurchaseable"] = p.IsPurchaseable,
                ["description"] = p.Description,
                ["companyId"] = (long)p.CompanyId,
                ["__isActive"] = p.IsActive,
                ["__searchText"] = SearchText(p.ProductCode, p.ProductName, p.SKU, p.Barcode, p.Description)
            });
            if (rows.Count >= MaxExportRows) break;
        }
        return rows;
    }

    private List<Dictionary<string, object?>> LoadCategoriesRaw(IEnumerable<ProductCategoryDto> cats, long companyId)
    {
        return cats.Select(c => new Dictionary<string, object?>
        {
            ["categoryCode"] = c.CategoryCode,
            ["categoryName"] = c.CategoryName,
            ["parentCategoryId"] = c.ParentCategoryId,
            ["description"] = c.Description,
            ["sortOrder"] = c.SortOrder,
            ["companyId"] = companyId,
            ["__isActive"] = c.IsActive,
            ["__searchText"] = SearchText(c.CategoryCode, c.CategoryName, c.Description)
        }).ToList();
    }

    private List<Dictionary<string, object?>> LoadSubCategoriesRaw(IEnumerable<ProductSubCategoryDto> items, long companyId)
    {
        return items.Select(s => new Dictionary<string, object?>
        {
            ["subCategoryCode"] = s.SubCategoryCode,
            ["subCategoryName"] = s.SubCategoryName,
            ["categoryId"] = s.CategoryId,
            ["description"] = s.Description,
            ["sortOrder"] = s.SortOrder,
            ["companyId"] = companyId,
            ["__isActive"] = s.IsActive,
            ["__searchText"] = SearchText(s.SubCategoryCode, s.SubCategoryName, s.Description)
        }).ToList();
    }

    private List<Dictionary<string, object?>> LoadBrandsRaw(IEnumerable<ProductBrandDto> brands, long companyId)
    {
        return brands.Select(b => new Dictionary<string, object?>
        {
            ["brandCode"] = b.BrandCode,
            ["brandName"] = b.BrandName,
            ["description"] = b.Description,
            ["companyId"] = companyId,
            ["__isActive"] = b.IsActive,
            ["__searchText"] = SearchText(b.BrandCode, b.BrandName, b.Description)
        }).ToList();
    }

    private List<Dictionary<string, object?>> LoadUnitsRaw(IEnumerable<ProductUnitDto> units, long companyId)
    {
        return units.Select(u => new Dictionary<string, object?>
        {
            ["unitCode"] = u.UnitCode,
            ["unitName"] = u.UnitName,
            ["symbol"] = u.Symbol,
            ["decimalPlaces"] = (object)u.DecimalPlaces,
            ["companyId"] = companyId,
            ["__isActive"] = u.IsActive,
            ["__searchText"] = SearchText(u.UnitCode, u.UnitName, u.Symbol)
        }).ToList();
    }

    private List<Dictionary<string, object?>> LoadTaxTypeSystemsRaw(IEnumerable<TaxTypeSystemDto> items)
    {
        return items.Select(t => new Dictionary<string, object?>
        {
            ["code"] = t.Code,
            ["name"] = t.Name,
            ["description"] = t.Description,
            ["__isActive"] = t.IsActive,
            ["__searchText"] = SearchText(t.Code, t.Name, t.Description)
        }).ToList();
    }

    private async Task<List<Dictionary<string, object?>>> LoadTaxesRaw(long companyId, bool includeInactive)
    {
        var all = await LoadPagedAsync(p => _taxService.GetPagedAsync(companyId, p, PageFetchSize, ""));
        var rows = new List<Dictionary<string, object?>>();
        foreach (var t in all)
        {
            if (!includeInactive && !t.IsActive) continue;
            rows.Add(new Dictionary<string, object?>
            {
                ["taxCode"] = t.TaxCode,
                ["taxName"] = t.TaxName,
                ["taxTypeId"] = t.TaxTypeSystemId,
                ["taxRate"] = t.TaxRate,
                ["isInclusive"] = t.IsInclusive,
                ["branchId"] = t.BranchId,
                ["effectiveFrom"] = t.EffectiveFrom,
                ["effectiveTo"] = t.EffectiveTo,
                ["description"] = t.Description,
                ["companyId"] = (long)t.CompanyId,
                ["__isActive"] = t.IsActive,
                ["__searchText"] = SearchText(t.TaxCode, t.TaxName, t.Description, t.TaxTypeSystemName)
            });
            if (rows.Count >= MaxExportRows) break;
        }
        return rows;
    }

    private async Task<List<Dictionary<string, object?>>> LoadCompaniesRaw(bool includeInactive)
    {
        var all = await LoadPagedAsync(p => _companyService.GetPagedAsync(p, PageFetchSize, ""));
        var rows = new List<Dictionary<string, object?>>();
        foreach (var c in all)
        {
            if (!includeInactive && !c.IsActive) continue;
            rows.Add(new Dictionary<string, object?>
            {
                ["companyCode"] = c.CompanyCode,
                ["companyName"] = c.CompanyName,
                ["shortName"] = c.ShortName,
                ["abbreviation"] = c.Abbreviation,
                ["businessTypeId"] = (object)c.BusinessTypeId,
                ["industryTypeId"] = (object)c.IndustryTypeId,
                ["gstRegistrationTypeId"] = c.GSTRegistrationTypeId,
                ["gstNumber"] = c.GSTNumber,
                ["panNumber"] = c.PANNumber,
                ["tanNumber"] = c.TANNumber,
                ["cinNumber"] = c.CINNumber,
                ["registrationNumber"] = c.RegistrationNumber,
                ["currencyId"] = (object)c.CurrencyId,
                ["languageId"] = (object)c.LanguageId,
                ["timeZoneId"] = (object)c.TimeZoneId,
                ["multiBranchEnabled"] = c.MultiBranchEnabled,
                ["multiWarehouseEnabled"] = c.MultiWarehouseEnabled,
                ["multiCurrencyEnabled"] = c.MultiCurrencyEnabled,
                ["companyId"] = (long)c.Id,
                ["__isActive"] = c.IsActive,
                ["__searchText"] = SearchText(c.CompanyCode, c.CompanyName, c.ShortName, c.Abbreviation, c.GSTNumber, c.PANNumber)
            });
            if (rows.Count >= MaxExportRows) break;
        }
        return rows;
    }

    /* ================================================================
       Paged loader helper
       ================================================================ */

    private static async Task<List<T>> LoadPagedAsync<T>(Func<int, Task<PaginatedResult<T>>> fetcher)
    {
        var result = new List<T>();
        var page = 1;
        while (true)
        {
            var p = await fetcher(page);
            result.AddRange(p.Items);
            if (!p.HasNext || result.Count >= MaxExportRows) break;
            page++;
        }
        return result;
    }

    /* ================================================================
       CSV / XLSX serialization
       ================================================================ */

    private static byte[] BuildCsv(List<ImportColumnMetaDto> cols, List<Dictionary<string, object?>> rows, bool includeHeaders)
    {
        var sb = new StringBuilder();
        if (includeHeaders)
            sb.AppendLine(string.Join(",", cols.Select(c => CsvEscape(c.Key))));

        foreach (var row in rows)
            sb.AppendLine(string.Join(",", cols.Select(c =>
                CsvEscape(FormatExportValue(row.TryGetValue(c.Key, out var v) ? v : null)))));

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return Encoding.UTF8.GetPreamble().Concat(bytes).ToArray();
    }

    private static byte[] BuildXlsx(List<ImportColumnMetaDto> cols, List<Dictionary<string, object?>> rows, bool includeHeaders)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Export");
        var r = 1;

        if (includeHeaders)
        {
            for (var i = 0; i < cols.Count; i++)
            {
                var cell = ws.Cell(r, i + 1);
                cell.Value = cols[i].Key;
                cell.Style.Font.Bold = true;
            }
            r = 2;
        }

        foreach (var row in rows)
        {
            for (var i = 0; i < cols.Count; i++)
                ws.Cell(r, i + 1).Value = FormatExportValue(row.TryGetValue(cols[i].Key, out var v) ? v : null);
            r++;
        }

        if (r > 1)
            ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static string FormatExportValue(object? v) => v switch
    {
        null => "",
        bool b => b ? "true" : "false",
        DateTime dt => dt.TimeOfDay == TimeSpan.Zero
            ? dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        decimal m => m.ToString(CultureInfo.InvariantCulture),
        double dbl => dbl.ToString(CultureInfo.InvariantCulture),
        float f => f.ToString(CultureInfo.InvariantCulture),
        int i => i.ToString(),
        long l => l.ToString(),
        _ => v.ToString() ?? ""
    };

    private static string CsvEscape(string? v)
    {
        if (string.IsNullOrEmpty(v)) return "";
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n') || v.Contains('\r'))
            return "\"" + v.Replace("\"", "\"\"") + "\"";
        return v;
    }

    private static string SearchText(params string?[] values)
        => string.Join(" ", values.Where(v => !string.IsNullOrWhiteSpace(v))).ToLowerInvariant();

    /* ================================================================
       Filter meta definitions per master entity
       ================================================================ */

    private static List<ExportFilterMetaDto> BuildFilterMeta(string entityName) => entityName switch
    {
        "Products" => new()
        {
            Ref("branchId",      "Branch",       "Branches",                "branchName"),
            Ref("categoryId",    "Category",     "ProductCategories",       "categoryName"),
            Ref("subCategoryId", "Sub Category", "ProductSubCategories",    "subCategoryName"),
            Ref("brandId",       "Brand",        "ProductBrands",           "brandName"),
            Ref("taxId",         "Tax",          "Taxes",                   "taxName"),
            Status("Status"),
            Text("Search"),
        },

        "ProductCategories" => new()
        {
            Ref("parentCategoryId", "Parent Category", "ProductCategories", "categoryName"),
            Status("Status"),
            Text("Search"),
        },

        "ProductSubCategories" => new()
        {
            Ref("categoryId", "Category", "ProductCategories", "categoryName"),
            Status("Status"),
            Text("Search"),
        },

        "ProductBrands" => new()
        {
            Status("Status"),
            Text("Search"),
        },

        "ProductUnits" => new()
        {
            Status("Status"),
            Text("Search"),
        },

        "TaxTypeSystems" => new()
        {
            Status("Status"),
            Text("Search"),
        },

        "Taxes" => new()
        {
            Ref("taxTypeId",     "Tax Type",  "TaxTypeSystems", "name"),
            Ref("branchId",      "Branch",    "Branches",       "branchName"),
            Status("Status"),
            Date("effectiveFrom", "Effective From"),
            Date("effectiveTo",   "Effective To"),
            Text("Search"),
        },

        "Companies" => new()
        {
            Ref("businessTypeId",       "Business Type",       "BusinessTypes",       "name"),
            Ref("industryTypeId",       "Industry Type",       "IndustryTypes",       "name"),
            Ref("gstRegistrationTypeId", "GST Reg. Type",      "GstRegistrationTypes","name"),
            Ref("currencyId",           "Currency",            "Currencies",          "currencyCode"),
            Status("Status"),
            Text("Search"),
        },

        _ => throw new NotFoundException($"Export filters for '{entityName}' are not defined.")
    };

    private static ExportFilterMetaDto Text(string label) => new() { Key = "search", Label = label, Type = "text" };
    private static ExportFilterMetaDto Status(string label) => new() { Key = "status", Label = label, Type = "status" };

    private static ExportFilterMetaDto Date(string key, string label)
        => new() { Key = key, Label = label, Type = "date" };

    private static ExportFilterMetaDto Ref(string key, string label, string entity, string display)
        => new() { Key = key, Label = label, Type = "reference", ReferenceEntity = entity, ReferenceDisplay = display };

    /* ================================================================
       Helpers
       ================================================================ */

    private MasterImportMetaDto GetDef(string entityName)
        => _importService.GetMasters().FirstOrDefault(m => m.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase))
           ?? throw new NotFoundException($"Import/export definition '{entityName}' was not found.");
}