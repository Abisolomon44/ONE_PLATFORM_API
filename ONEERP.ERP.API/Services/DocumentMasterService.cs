using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

/* ============================================================================
   DOCUMENT MASTER (Screen 1) service - the 10 lookup masters.
   All backed by EXISTING Invoice* tables; no new schema.
   ============================================================================ */

public interface IDocumentMasterService
{
    Task<IEnumerable<DocumentMasterRowDto>> GetAllAsync(string kind, bool includeInactive);
    Task<DocumentMasterRowDto> GetByIdAsync(string kind, long id);
    Task<DocumentMasterRowDto> SaveAsync(string kind, long? id, SaveDocumentMasterRequest request);
    Task DeleteAsync(string kind, long id);
}

public class DocumentMasterService : IDocumentMasterService
{
    private readonly IInvoiceTypeRepository _typeRepo;
    private readonly ITemplateCategoryRepository _categoryRepo;
    private readonly ITemplateComponentRepository _componentRepo;
    private readonly ITemplateVariableRepository _variableRepo;
    private readonly IInvoiceFontRepository _fontRepo;
    private readonly IDocumentPaperSizeRepository _paperSizeRepo;
    private readonly IDocumentPrinterTypeRepository _printerTypeRepo;
    private readonly IDocumentPrinterModelRepository _printerModelRepo;
    private readonly IPrintOrientationRepository _orientationRepo;
    private readonly IPrintUnitRepository _unitRepo;
    private readonly IDocumentMasterGuardRepository _guard;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public DocumentMasterService(
        IInvoiceTypeRepository typeRepo,
        ITemplateCategoryRepository categoryRepo,
        ITemplateComponentRepository componentRepo,
        ITemplateVariableRepository variableRepo,
        IInvoiceFontRepository fontRepo,
        IDocumentPaperSizeRepository paperSizeRepo,
        IDocumentPrinterTypeRepository printerTypeRepo,
        IDocumentPrinterModelRepository printerModelRepo,
        IPrintOrientationRepository orientationRepo,
        IPrintUnitRepository unitRepo,
        IDocumentMasterGuardRepository guard,
        IAuditService audit,
        ICurrentUser user)
    {
        _typeRepo = typeRepo;
        _categoryRepo = categoryRepo;
        _componentRepo = componentRepo;
        _variableRepo = variableRepo;
        _fontRepo = fontRepo;
        _paperSizeRepo = paperSizeRepo;
        _printerTypeRepo = printerTypeRepo;
        _printerModelRepo = printerModelRepo;
        _orientationRepo = orientationRepo;
        _unitRepo = unitRepo;
        _guard = guard;
        _audit = audit;
        _user = user;
    }

    private static string[] Kinds =
    [
        "types", "categories", "components", "variables", "fonts",
        "paper-sizes", "printer-types", "printer-models", "orientations", "units"
    ];

    private static bool IsValidKind(string kind) => Kinds.Contains(kind, StringComparer.OrdinalIgnoreCase);

    private static void ValidateKind(string kind)
    {
        if (!IsValidKind(kind))
            throw new DomainException($"Unknown document master '{kind}'.", 400);
    }

    public async Task<IEnumerable<DocumentMasterRowDto>> GetAllAsync(string kind, bool includeInactive = false)
    {
        ValidateKind(kind);
        var rows = new List<DocumentMasterRowDto>();
        switch (kind)
        {
            case "types": foreach (var e in await _typeRepo.GetAllAsync(includeInactive)) rows.Add(MapRow(kind, e)); break;
            case "categories": foreach (var e in await _categoryRepo.GetAllAsync(includeInactive)) rows.Add(MapRow(kind, e)); break;
            case "components": foreach (var e in await _componentRepo.GetAllAsync(includeInactive)) rows.Add(MapRow(kind, e)); break;
            case "variables": foreach (var e in await _variableRepo.GetAllAsync(includeInactive)) rows.Add(MapRow(kind, e)); break;
            case "fonts": foreach (var e in await _fontRepo.GetAllAsync(includeInactive)) rows.Add(MapRow(kind, e)); break;
            case "paper-sizes": foreach (var e in await _paperSizeRepo.GetAllAsync(includeInactive)) rows.Add(MapRow(kind, e)); break;
            case "printer-types": foreach (var e in await _printerTypeRepo.GetAllAsync(includeInactive)) rows.Add(MapRow(kind, e)); break;
            case "printer-models": foreach (var e in await _printerModelRepo.GetAllAsync(includeInactive)) rows.Add(MapRow(kind, e)); break;
            case "orientations": foreach (var e in await _orientationRepo.GetAllAsync(includeInactive)) rows.Add(MapRow(kind, e)); break;
            case "units": foreach (var e in await _unitRepo.GetAllAsync(includeInactive)) rows.Add(MapRow(kind, e)); break;
            default: throw new InvalidOperationException($"Unknown master kind '{kind}'.");
        }
        return rows;
    }

    public async Task<DocumentMasterRowDto> GetByIdAsync(string kind, long id)
    {
        ValidateKind(kind);
        object? entity = kind switch
        {
            "types" => await _typeRepo.GetByIdAsync(id),
            "categories" => await _categoryRepo.GetByIdAsync(id),
            "components" => await _componentRepo.GetByIdAsync(id),
            "variables" => await _variableRepo.GetByIdAsync(id),
            "fonts" => await _fontRepo.GetByIdAsync(id),
            "paper-sizes" => await _paperSizeRepo.GetByIdAsync(id),
            "printer-types" => await _printerTypeRepo.GetByIdAsync(id),
            "printer-models" => await _printerModelRepo.GetByIdAsync(id),
            "orientations" => await _orientationRepo.GetByIdAsync(id),
            "units" => await _unitRepo.GetByIdAsync(id),
            _ => throw new InvalidOperationException()
        };
        if (entity is null)
            throw new NotFoundException($"Record not found for kind '{kind}'.");
        return MapRow(kind, entity);
    }

    public async Task<DocumentMasterRowDto> SaveAsync(string kind, long? id, SaveDocumentMasterRequest request)
    {
        ValidateKind(kind);

        var code = request.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Code is required.", 400);

        if (id is null)
        {
            if (await IsCodeInUse(kind, code, null))
                throw new DomainException($"A '{kind}' with code '{code}' already exists.", 409);

            var entity = CreateEntity(kind, request, code);
            entity.GetType().GetProperty("Code")?.SetValue(entity, code);
            id = await InsertAsync(kind, entity);
        }
        else
        {
            var entity = await LoadAsync(kind, id.Value)
                ?? throw new NotFoundException($"Record '{id}' for kind '{kind}' was not found.");
            var codeInUse = await IsCodeInUse(kind, code, id);
            if (codeInUse)
                throw new DomainException($"A '{kind}' with code '{code}' already exists.", 409);

            UpdateEntity(kind, entity, request, code);
            await UpdateAsync(kind, entity);
        }

        await _audit.WriteAsync(kind.Camelize(), id?.ToString() ?? "new", (id is null ? "Create" : "Update"), _user.Username);
        return await GetByIdAsync(kind, id ?? 0);
    }

    public async Task DeleteAsync(string kind, long id)
    {
        ValidateKind(kind);
        var used = kind switch
        {
            "types" => await _guard.InvoiceTypeInUseAsync(id),
            "categories" => await _guard.CategoryInUseAsync(id),
            "components" => await _guard.ComponentInUseAsync(id),
            "variables" => await _guard.VariableInUseAsync(id),
            "fonts" => await _guard.FontInUseAsync(id),
            "paper-sizes" => await _guard.PaperSizeInUseAsync(id),
            "printer-types" => await _guard.PrinterTypeInUseAsync(id),
            "printer-models" => await _guard.PrinterModelInUseAsync(id),
            "orientations" => await _guard.OrientationInUseAsync(id),
            "units" => await _guard.UnitInUseAsync(id),
            _ => false
        };
        if (used)
            throw new DomainException($"Cannot delete this '{kind}' because it is referenced by other records.", 409);

        switch (kind)
        {
            case "types": await _typeRepo.DeleteAsync(id); break;
            case "categories": await _categoryRepo.DeleteAsync(id); break;
            case "components": await _componentRepo.DeleteAsync(id); break;
            case "variables": await _variableRepo.DeleteAsync(id); break;
            case "fonts": await _fontRepo.DeleteAsync(id); break;
            case "paper-sizes": await _paperSizeRepo.DeleteAsync(id); break;
            case "printer-types": await _printerTypeRepo.DeleteAsync(id); break;
            case "printer-models": await _printerModelRepo.DeleteAsync(id); break;
            case "orientations": await _orientationRepo.DeleteAsync(id); break;
            case "units": await _unitRepo.DeleteAsync(id); break;
            default: throw new InvalidOperationException();
        }
        await _audit.WriteAsync(kind.Camelize(), id.ToString(), "Delete", _user.Username);
    }

    /* ------------------------------------------------------------------ */
    /* Internals                                                          */
    /* ------------------------------------------------------------------ */

    private async Task<bool> IsCodeInUse(string kind, string code, long? excludeId) => kind switch
    {
        "types" => await _typeRepo.CodeInUseAsync(code, excludeId),
        "categories" => await _categoryRepo.CodeInUseAsync(code, excludeId),
        "components" => await _componentRepo.CodeInUseAsync(code, excludeId),
        "variables" => await _variableRepo.CodeInUseAsync(code, excludeId),
        "fonts" => await _fontRepo.CodeInUseAsync(code, excludeId),
        "paper-sizes" => await _paperSizeRepo.CodeInUseAsync(code, excludeId),
        "printer-types" => await _printerTypeRepo.CodeInUseAsync(code, excludeId),
        "printer-models" => await _printerModelRepo.CodeInUseAsync(code, excludeId),
        "orientations" => await _orientationRepo.CodeInUseAsync(code, excludeId),
        "units" => await _unitRepo.CodeInUseAsync(code, excludeId),
        _ => false
    };

    private async Task<long> InsertAsync(string kind, object entity)
    {
        switch (kind)
        {
            case "types": return await _typeRepo.InsertAsync((InvoiceType)entity);
            case "categories": return await _categoryRepo.InsertAsync((InvoiceTemplateCategory)entity);
            case "components": return await _componentRepo.InsertAsync((InvoiceTemplateComponent)entity);
            case "variables": return await _variableRepo.InsertAsync((InvoiceTemplateVariable)entity);
            case "fonts": return await _fontRepo.InsertAsync((InvoiceFont)entity);
            case "paper-sizes": return await _paperSizeRepo.InsertAsync((InvoicePaperSize)entity);
            case "printer-types": return await _printerTypeRepo.InsertAsync((PrinterType)entity);
            case "printer-models": return await _printerModelRepo.InsertAsync((PrinterModel)entity);
            case "orientations": return await _orientationRepo.InsertAsync((PrintOrientation)entity);
            case "units": return await _unitRepo.InsertAsync((PrintUnit)entity);
            default: throw new InvalidOperationException();
        }
    }

    private async Task UpdateAsync(string kind, object entity)
    {
        switch (kind)
        {
            case "types": await _typeRepo.UpdateAsync((InvoiceType)entity); break;
            case "categories": await _categoryRepo.UpdateAsync((InvoiceTemplateCategory)entity); break;
            case "components": await _componentRepo.UpdateAsync((InvoiceTemplateComponent)entity); break;
            case "variables": await _variableRepo.UpdateAsync((InvoiceTemplateVariable)entity); break;
            case "fonts": await _fontRepo.UpdateAsync((InvoiceFont)entity); break;
            case "paper-sizes": await _paperSizeRepo.UpdateAsync((InvoicePaperSize)entity); break;
            case "printer-types": await _printerTypeRepo.UpdateAsync((PrinterType)entity); break;
            case "printer-models": await _printerModelRepo.UpdateAsync((PrinterModel)entity); break;
            case "orientations": await _orientationRepo.UpdateAsync((PrintOrientation)entity); break;
            case "units": await _unitRepo.UpdateAsync((PrintUnit)entity); break;
            default: throw new InvalidOperationException();
        }
    }

    private async Task<object?> LoadAsync(string kind, long id)
    {
        switch (kind)
        {
            case "types": return await _typeRepo.GetByIdAsync(id);
            case "categories": return await _categoryRepo.GetByIdAsync(id);
            case "components": return await _componentRepo.GetByIdAsync(id);
            case "variables": return await _variableRepo.GetByIdAsync(id);
            case "fonts": return await _fontRepo.GetByIdAsync(id);
            case "paper-sizes": return await _paperSizeRepo.GetByIdAsync(id);
            case "printer-types": return await _printerTypeRepo.GetByIdAsync(id);
            case "printer-models": return await _printerModelRepo.GetByIdAsync(id);
            case "orientations": return await _orientationRepo.GetByIdAsync(id);
            case "units": return await _unitRepo.GetByIdAsync(id);
            default: throw new InvalidOperationException();
        }
    }

    private void UpdateEntity(string kind, object entity, SaveDocumentMasterRequest request, string code)
    {
        switch (kind)
        {
            case "types":
                ((InvoiceType)entity).Code = code;
                ((InvoiceType)entity).Name = request.Name.Trim();
                ((InvoiceType)entity).Description = request.Description;
                ((InvoiceType)entity).DisplayOrder = request.DisplayOrder;
                ((InvoiceType)entity).IsActive = request.IsActive;
                ((InvoiceType)entity).ModifiedBy = _user.UserId;
                break;
            case "categories":
                ((InvoiceTemplateCategory)entity).Code = code;
                ((InvoiceTemplateCategory)entity).Name = request.Name.Trim();
                ((InvoiceTemplateCategory)entity).Description = request.Description;
                ((InvoiceTemplateCategory)entity).DisplayOrder = request.DisplayOrder;
                ((InvoiceTemplateCategory)entity).IsActive = request.IsActive;
                ((InvoiceTemplateCategory)entity).ModifiedBy = _user.UserId;
                break;
            case "components":
                ((InvoiceTemplateComponent)entity).Code = code;
                ((InvoiceTemplateComponent)entity).Name = request.Name.Trim();
                ((InvoiceTemplateComponent)entity).ComponentType = request.ComponentType ?? "";
                ((InvoiceTemplateComponent)entity).Description = request.Description;
                ((InvoiceTemplateComponent)entity).DisplayOrder = request.DisplayOrder;
                ((InvoiceTemplateComponent)entity).IsActive = request.IsActive;
                ((InvoiceTemplateComponent)entity).ModifiedBy = _user.UserId;
                break;
            case "variables":
                ((InvoiceTemplateVariable)entity).Code = code;
                ((InvoiceTemplateVariable)entity).Name = request.Name.Trim();
                ((InvoiceTemplateVariable)entity).BindingPath = request.BindingPath ?? string.Empty;
                ((InvoiceTemplateVariable)entity).DataType = request.DataType ?? string.Empty;
                ((InvoiceTemplateVariable)entity).Category = request.Category;
                ((InvoiceTemplateVariable)entity).Description = request.Description;
                ((InvoiceTemplateVariable)entity).IsCollection = request.IsCollection;
                ((InvoiceTemplateVariable)entity).IsActive = request.IsActive;
                ((InvoiceTemplateVariable)entity).ModifiedBy = _user.UserId;
                break;
            case "fonts":
                ((InvoiceFont)entity).Code = code;
                ((InvoiceFont)entity).Name = request.Name.Trim();
                ((InvoiceFont)entity).FontFamily = string.IsNullOrWhiteSpace(request.FontFamily) ? request.Name : request.FontFamily;
                ((InvoiceFont)entity).FontFileId = request.FontFileId;
                ((InvoiceFont)entity).IsActive = request.IsActive;
                ((InvoiceFont)entity).ModifiedBy = _user.UserId;
                break;
            case "paper-sizes":
                ((InvoicePaperSize)entity).Code = code;
                ((InvoicePaperSize)entity).Name = request.Name.Trim();
                ((InvoicePaperSize)entity).Width = request.Width ?? 0m;
                ((InvoicePaperSize)entity).Height = request.Height;
                ((InvoicePaperSize)entity).Unit = string.IsNullOrWhiteSpace(request.Unit) ? "MM" : request.Unit;
                ((InvoicePaperSize)entity).IsThermal = request.IsThermal;
                ((InvoicePaperSize)entity).IsCustom = request.IsCustom;
                ((InvoicePaperSize)entity).IsActive = request.IsActive;
                ((InvoicePaperSize)entity).ModifiedBy = _user.UserId;
                break;
            case "printer-types":
                ((PrinterType)entity).Code = code;
                ((PrinterType)entity).Name = request.Name.Trim();
                ((PrinterType)entity).Description = request.Description;
                ((PrinterType)entity).IsActive = request.IsActive;
                ((PrinterType)entity).ModifiedBy = _user.UserId;
                break;
            case "printer-models":
                ((PrinterModel)entity).PrinterTypeId = request.PrinterTypeId ?? 0L;
                ((PrinterModel)entity).Code = code;
                ((PrinterModel)entity).Name = request.Name.Trim();
                ((PrinterModel)entity).Manufacturer = request.Manufacturer;
                ((PrinterModel)entity).IsActive = request.IsActive;
                ((PrinterModel)entity).ModifiedBy = _user.UserId;
                break;
            case "orientations":
                ((PrintOrientation)entity).Code = code;
                ((PrintOrientation)entity).Name = request.Name.Trim();
                ((PrintOrientation)entity).IsActive = request.IsActive;
                break;
            case "units":
                ((PrintUnit)entity).Code = code;
                ((PrintUnit)entity).Name = request.Name.Trim();
                ((PrintUnit)entity).IsActive = request.IsActive;
                break;
        }
    }

    private object CreateEntity(string kind, SaveDocumentMasterRequest request, string code)
    {
        return kind switch
        {
            "types" => new InvoiceType
            {
                Code = code,
                Name = request.Name.Trim(),
                Description = request.Description,
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive,
                CreatedBy = _user.UserId
            },
            "categories" => new InvoiceTemplateCategory
            {
                Code = code,
                Name = request.Name.Trim(),
                Description = request.Description,
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive,
                CreatedBy = _user.UserId
            },
            "components" => new InvoiceTemplateComponent
            {
                Code = code,
                Name = request.Name.Trim(),
                ComponentType = request.ComponentType ?? "",
                Description = request.Description,
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive,
                CreatedBy = _user.UserId
            },
            "variables" => new InvoiceTemplateVariable
            {
                Code = code,
                Name = request.Name.Trim(),
                BindingPath = request.BindingPath ?? string.Empty,
                DataType = request.DataType ?? string.Empty,
                Category = request.Category,
                Description = request.Description,
                IsCollection = request.IsCollection,
                IsActive = request.IsActive,
                CreatedBy = _user.UserId
            },
            "fonts" => new InvoiceFont
            {
                Code = code,
                Name = request.Name.Trim(),
                FontFamily = string.IsNullOrWhiteSpace(request.FontFamily) ? request.Name : request.FontFamily,
                FontFileId = request.FontFileId,
                IsActive = request.IsActive,
                CreatedBy = _user.UserId
            },
            "paper-sizes" => new InvoicePaperSize
            {
                Code = code,
                Name = request.Name.Trim(),
                Width = request.Width ?? 0m,
                Height = request.Height,
                Unit = string.IsNullOrWhiteSpace(request.Unit) ? "MM" : request.Unit,
                IsThermal = request.IsThermal,
                IsCustom = request.IsCustom,
                IsActive = request.IsActive,
                CreatedBy = _user.UserId
            },
            "printer-types" => new PrinterType
            {
                Code = code,
                Name = request.Name.Trim(),
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedBy = _user.UserId
            },
            "printer-models" => new PrinterModel
            {
                PrinterTypeId = request.PrinterTypeId ?? 0L,
                Code = code,
                Name = request.Name.Trim(),
                Manufacturer = request.Manufacturer,
                IsActive = request.IsActive,
                CreatedBy = _user.UserId
            },
            "orientations" => new PrintOrientation
            {
                Code = code,
                Name = request.Name.Trim(),
                IsActive = request.IsActive
            },
            "units" => new PrintUnit
            {
                Code = code,
                Name = request.Name.Trim(),
                IsActive = request.IsActive
            },
            _ => throw new InvalidOperationException()
        };
    }

    private DocumentMasterRowDto MapRow(string kind, object entity) => kind switch
    {
        "types" => new DocumentMasterRowDto
        {
            Id = ((InvoiceType)entity).InvoiceTypeId,
            Code = ((InvoiceType)entity).Code,
            Name = ((InvoiceType)entity).Name,
            Description = ((InvoiceType)entity).Description,
            DisplayOrder = ((InvoiceType)entity).DisplayOrder,
            IsActive = ((InvoiceType)entity).IsActive,
            CreatedAt = ((InvoiceType)entity).CreatedAt,
            ModifiedAt = ((InvoiceType)entity).ModifiedAt
        },
        "categories" => new DocumentMasterRowDto
        {
            Id = ((InvoiceTemplateCategory)entity).TemplateCategoryId,
            Code = ((InvoiceTemplateCategory)entity).Code,
            Name = ((InvoiceTemplateCategory)entity).Name,
            Description = ((InvoiceTemplateCategory)entity).Description,
            DisplayOrder = ((InvoiceTemplateCategory)entity).DisplayOrder,
            IsActive = ((InvoiceTemplateCategory)entity).IsActive,
            CreatedAt = ((InvoiceTemplateCategory)entity).CreatedAt,
            ModifiedAt = ((InvoiceTemplateCategory)entity).ModifiedAt
        },
        "components" => new DocumentMasterRowDto
        {
            Id = ((InvoiceTemplateComponent)entity).ComponentId,
            Code = ((InvoiceTemplateComponent)entity).Code,
            Name = ((InvoiceTemplateComponent)entity).Name,
            ComponentType = ((InvoiceTemplateComponent)entity).ComponentType,
            Description = ((InvoiceTemplateComponent)entity).Description,
            DisplayOrder = ((InvoiceTemplateComponent)entity).DisplayOrder,
            IsActive = ((InvoiceTemplateComponent)entity).IsActive,
            CreatedAt = ((InvoiceTemplateComponent)entity).CreatedAt,
            ModifiedAt = ((InvoiceTemplateComponent)entity).ModifiedAt
        },
        "variables" => new DocumentMasterRowDto
        {
            Id = ((InvoiceTemplateVariable)entity).VariableId,
            Code = ((InvoiceTemplateVariable)entity).Code,
            Name = ((InvoiceTemplateVariable)entity).Name,
            Description = ((InvoiceTemplateVariable)entity).Description,
            DisplayOrder = 0,
            IsActive = ((InvoiceTemplateVariable)entity).IsActive,
            BindingPath = ((InvoiceTemplateVariable)entity).BindingPath,
            DataType = ((InvoiceTemplateVariable)entity).DataType,
            Category = ((InvoiceTemplateVariable)entity).Category,
            IsCollection = ((InvoiceTemplateVariable)entity).IsCollection
        },
        "fonts" => new DocumentMasterRowDto
        {
            Id = ((InvoiceFont)entity).FontId,
            Code = ((InvoiceFont)entity).Code,
            Name = ((InvoiceFont)entity).Name,
            Description = null,
            DisplayOrder = 0,
            IsActive = ((InvoiceFont)entity).IsActive,
            FontFamily = ((InvoiceFont)entity).FontFamily,
            FontFileId = ((InvoiceFont)entity).FontFileId
        },
        "paper-sizes" => new DocumentMasterRowDto
        {
            Id = ((InvoicePaperSize)entity).PaperSizeId,
            Code = ((InvoicePaperSize)entity).Code,
            Name = ((InvoicePaperSize)entity).Name,
            Width = ((InvoicePaperSize)entity).Width,
            Height = ((InvoicePaperSize)entity).Height,
            Unit = ((InvoicePaperSize)entity).Unit,
            IsThermal = ((InvoicePaperSize)entity).IsThermal,
            IsCustom = ((InvoicePaperSize)entity).IsCustom,
            IsActive = ((InvoicePaperSize)entity).IsActive,
            CreatedAt = ((InvoicePaperSize)entity).CreatedAt,
            ModifiedAt = ((InvoicePaperSize)entity).ModifiedAt
        },
        "printer-types" => new DocumentMasterRowDto
        {
            Id = ((PrinterType)entity).PrinterTypeId,
            Code = ((PrinterType)entity).Code,
            Name = ((PrinterType)entity).Name,
            Description = ((PrinterType)entity).Description,
            IsActive = ((PrinterType)entity).IsActive,
            CreatedAt = ((PrinterType)entity).CreatedAt,
            ModifiedAt = ((PrinterType)entity).ModifiedAt
        },
        "printer-models" => new DocumentMasterRowDto
        {
            Id = ((PrinterModel)entity).PrinterModelId,
            Code = ((PrinterModel)entity).Code,
            Name = ((PrinterModel)entity).Name,
            Description = ((PrinterModel)entity).Manufacturer,
            DisplayOrder = 0,
            IsActive = ((PrinterModel)entity).IsActive
        },
        "orientations" => new DocumentMasterRowDto
        {
            Id = ((PrintOrientation)entity).OrientationId,
            Code = ((PrintOrientation)entity).Code,
            Name = ((PrintOrientation)entity).Name,
            IsActive = ((PrintOrientation)entity).IsActive
        },
        "units" => new DocumentMasterRowDto
        {
            Id = ((PrintUnit)entity).UnitId,
            Code = ((PrintUnit)entity).Code,
            Name = ((PrintUnit)entity).Name,
            IsActive = ((PrintUnit)entity).IsActive
        },
        _ => throw new InvalidOperationException()
    };
}

internal static class StringExt
{
    internal static string Camelize(this string s) =>
        string.Concat(s.Split('-').Select(x => char.ToUpperInvariant(x[0]) + x.Substring(1).ToLowerInvariant()));
}
