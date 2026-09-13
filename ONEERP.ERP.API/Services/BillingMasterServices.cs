using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

/* ---------------- Price Types (system master) ---------------- */

public interface IPriceTypeService
{
    Task<PaginatedResult<PriceTypeDto>> GetPagedAsync(int pageNumber, int pageSize, string search);
    Task<PriceTypeDto> GetByIdAsync(long id);
    Task<string> GetNextCodeAsync(string prefix = "PRICETYPE");
    Task<IEnumerable<PriceTypeDto>> GetAllAsync(bool includeInactive = false);
    Task<PriceTypeDto> CreateAsync(CreatePriceTypeRequest request);
    Task<PriceTypeDto> UpdateAsync(long id, UpdatePriceTypeRequest request);
    Task<bool> DeleteAsync(long id);
}

public class PriceTypeService : IPriceTypeService
{
    private readonly IPriceTypeRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public PriceTypeService(IPriceTypeRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<PriceTypeDto>> GetPagedAsync(int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(normalizedPage, normalizedSize, search ?? string.Empty);
        var total = await _repository.CountAsync(search ?? string.Empty);
        return new PaginatedResult<PriceTypeDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<PriceTypeDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Price type '{id}' was not found.");
        return ToDto(entity);
    }

    public async Task<string> GetNextCodeAsync(string prefix = "PRICETYPE")
        => await _repository.GetNextCodeAsync(prefix);

    public async Task<IEnumerable<PriceTypeDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(ToDto).ToList();
    }

    public async Task<PriceTypeDto> CreateAsync(CreatePriceTypeRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.Code)
            ? await _repository.GetNextCodeAsync()
            : request.Code.Trim().ToUpper();
        if (await _repository.CodeInUseAsync(code))
            throw new DomainException($"Price type code '{code}' is already in use.");

        var entity = new PriceType
        {
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            DisplayOrder = request.DisplayOrder ?? 0,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.PriceTypeId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("PriceType", entity.PriceTypeId.ToString(), "Create", _currentUser.Username);
        return ToDto(entity);
    }

    public async Task<PriceTypeDto> UpdateAsync(long id, UpdatePriceTypeRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Price type '{id}' was not found.");

        var code = string.IsNullOrWhiteSpace(request.Code) ? entity.Code : request.Code.Trim().ToUpper();
        if (code != entity.Code && await _repository.CodeInUseAsync(code, id))
            throw new DomainException($"Price type code '{code}' is already in use.");

        entity.Code = code;
        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.DisplayOrder = request.DisplayOrder;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("PriceType", id.ToString(), "Update", _currentUser.Username);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Price type '{id}' was not found.");
        await _repository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("PriceType", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static PriceTypeDto ToDto(PriceType e) => new()
    {
        PriceTypeId = e.PriceTypeId,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        DisplayOrder = e.DisplayOrder,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt
    };
}

/* ---------------- Unit Conversions (company) ---------------- */

public interface IUnitConversionService
{
    Task<PaginatedResult<UnitConversionDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<UnitConversionDto> GetByIdAsync(long id);
    Task<IEnumerable<UnitConversionDto>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<UnitConversionDto> CreateAsync(long companyId, CreateUnitConversionRequest request);
    Task<UnitConversionDto> UpdateAsync(long id, long companyId, UpdateUnitConversionRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class UnitConversionService : IUnitConversionService
{
    private readonly IUnitConversionRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public UnitConversionService(IUnitConversionRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<UnitConversionDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search ?? string.Empty);
        var total = await _repository.CountAsync(companyId, search ?? string.Empty);
        var dtos = items.Select(ToDto).ToList();
        await EnrichAsync(companyId, dtos);
        return new PaginatedResult<UnitConversionDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<UnitConversionDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Unit conversion '{id}' was not found.");
        var dto = ToDto(entity);
        await EnrichAsync(entity.CompanyId, new List<UnitConversionDto> { dto });
        return dto;
    }

    public async Task<IEnumerable<UnitConversionDto>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(companyId, includeInactive);
        var dtos = items.Select(ToDto).ToList();
        await EnrichAsync(companyId, dtos);
        return dtos;
    }

    public async Task<UnitConversionDto> CreateAsync(long companyId, CreateUnitConversionRequest request)
    {
        if (request.FromUnitId == request.ToUnitId)
            throw new DomainException("From unit and to unit must be different.");
        if (await _repository.CodeInUseAsync(companyId, request.FromUnitId, request.ToUnitId, request.ProductId))
            throw new DomainException("A unit conversion for the same product/units already exists.");

        var entity = new UnitConversion
        {
            CompanyId = companyId,
            ProductId = request.ProductId,
            FromUnitId = request.FromUnitId,
            ToUnitId = request.ToUnitId,
            ConversionFactor = request.ConversionFactor,
            IsDefault = request.IsDefault,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.UnitConversionId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("UnitConversion", entity.UnitConversionId.ToString(), "Create", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<UnitConversionDto> { dto });
        return dto;
    }

    public async Task<UnitConversionDto> UpdateAsync(long id, long companyId, UpdateUnitConversionRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Unit conversion '{id}' was not found.");

        if (request.FromUnitId == request.ToUnitId)
            throw new DomainException("From unit and to unit must be different.");
        if (await _repository.CodeInUseAsync(companyId, request.FromUnitId, request.ToUnitId, request.ProductId, id))
            throw new DomainException("A unit conversion for the same product/units already exists.");

        entity.ProductId = request.ProductId;
        entity.FromUnitId = request.FromUnitId;
        entity.ToUnitId = request.ToUnitId;
        entity.ConversionFactor = request.ConversionFactor;
        entity.IsDefault = request.IsDefault;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("UnitConversion", id.ToString(), "Update", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<UnitConversionDto> { dto });
        return dto;
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Unit conversion '{id}' was not found.");
        await _repository.SoftDeleteAsync(id, companyId, _currentUser.UserId);
        await _auditService.WriteAsync("UnitConversion", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static UnitConversionDto ToDto(UnitConversion e) => new()
    {
        UnitConversionId = e.UnitConversionId,
        CompanyId = e.CompanyId,
        ProductId = e.ProductId,
        FromUnitId = e.FromUnitId,
        ToUnitId = e.ToUnitId,
        ConversionFactor = e.ConversionFactor,
        IsDefault = e.IsDefault,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt
    };

    private async Task EnrichAsync(long companyId, List<UnitConversionDto> dtos)
    {
        var productIds = dtos.Where(d => d.ProductId.HasValue).Select(d => d.ProductId!.Value).Distinct().ToList();
        var unitIds = dtos.SelectMany(d => new[] { d.FromUnitId, d.ToUnitId }).Distinct().ToList();

        var productNames = productIds.Count > 0 ? await _repository.GetProductNamesAsync(companyId, productIds) : new Dictionary<long, string>();
        var unitNames = unitIds.Count > 0 ? await _repository.GetUnitNamesAsync(companyId, unitIds) : new Dictionary<long, string>();

        foreach (var d in dtos)
        {
            if (d.ProductId.HasValue && productNames.TryGetValue(d.ProductId.Value, out var pn)) d.ProductName = pn;
            if (unitNames.TryGetValue(d.FromUnitId, out var fu)) d.FromUnitName = fu;
            if (unitNames.TryGetValue(d.ToUnitId, out var tu)) d.ToUnitName = tu;
        }
    }
}

/* ---------------- Barcodes (company) ---------------- */

public interface IBarcodeService
{
    Task<PaginatedResult<BarcodeDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<BarcodeDto> GetByIdAsync(long id);
    Task<IEnumerable<BarcodeDto>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<BarcodeDto> CreateAsync(long companyId, CreateBarcodeRequest request);
    Task<BarcodeDto> UpdateAsync(long id, long companyId, UpdateBarcodeRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class BarcodeService : IBarcodeService
{
    private readonly IBarcodeRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public BarcodeService(IBarcodeRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<BarcodeDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search ?? string.Empty);
        var total = await _repository.CountAsync(companyId, search ?? string.Empty);
        var dtos = items.Select(ToDto).ToList();
        await EnrichAsync(companyId, dtos);
        return new PaginatedResult<BarcodeDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<BarcodeDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Barcode '{id}' was not found.");
        var dto = ToDto(entity);
        await EnrichAsync(entity.CompanyId, new List<BarcodeDto> { dto });
        return dto;
    }

    public async Task<IEnumerable<BarcodeDto>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(companyId, includeInactive);
        var dtos = items.Select(ToDto).ToList();
        await EnrichAsync(companyId, dtos);
        return dtos;
    }

    public async Task<BarcodeDto> CreateAsync(long companyId, CreateBarcodeRequest request)
    {
        var value = request.Barcode.Trim();
        if (await _repository.ValueInUseAsync(companyId, value))
            throw new DomainException($"Barcode '{value}' is already in use.");

        var entity = new Barcode
        {
            CompanyId = companyId,
            ProductId = request.ProductId,
            UnitId = request.UnitId,
            BarcodeValue = value,
            BarcodeType = request.BarcodeType?.Trim(),
            IsPrimary = request.IsPrimary,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.BarcodeId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("Barcode", entity.BarcodeId.ToString(), "Create", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<BarcodeDto> { dto });
        return dto;
    }

    public async Task<BarcodeDto> UpdateAsync(long id, long companyId, UpdateBarcodeRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Barcode '{id}' was not found.");

        var value = request.Barcode.Trim();
        if (await _repository.ValueInUseAsync(companyId, value, id))
            throw new DomainException($"Barcode '{value}' is already in use.");

        entity.ProductId = request.ProductId;
        entity.UnitId = request.UnitId;
        entity.BarcodeValue = value;
        entity.BarcodeType = request.BarcodeType?.Trim();
        entity.IsPrimary = request.IsPrimary;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("Barcode", id.ToString(), "Update", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<BarcodeDto> { dto });
        return dto;
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Barcode '{id}' was not found.");
        await _repository.SoftDeleteAsync(id, companyId, _currentUser.UserId);
        await _auditService.WriteAsync("Barcode", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static BarcodeDto ToDto(Barcode e) => new()
    {
        BarcodeId = e.BarcodeId,
        CompanyId = e.CompanyId,
        Barcode = e.BarcodeValue,
        ProductId = e.ProductId,
        UnitId = e.UnitId,
        BarcodeType = e.BarcodeType,
        IsPrimary = e.IsPrimary,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt
    };

    private async Task EnrichAsync(long companyId, List<BarcodeDto> dtos)
    {
        var productIds = dtos.Select(d => d.ProductId).Distinct().ToList();
        var unitIds = dtos.Where(d => d.UnitId.HasValue).Select(d => d.UnitId!.Value).Distinct().ToList();

        var productNames = productIds.Count > 0 ? await _repository.GetProductNamesAsync(companyId, productIds) : new Dictionary<long, string>();
        var unitNames = unitIds.Count > 0 ? await _repository.GetUnitNamesAsync(companyId, unitIds) : new Dictionary<long, string>();

        foreach (var d in dtos)
        {
            if (productNames.TryGetValue(d.ProductId, out var pn)) d.ProductName = pn;
            if (d.UnitId.HasValue && unitNames.TryGetValue(d.UnitId.Value, out var un)) d.UnitName = un;
        }
    }
}

/* ---------------- HSN/SAC (company) ---------------- */

public interface IHsnSacService
{
    Task<PaginatedResult<HsnSacDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<HsnSacDto> GetByIdAsync(long id);
    Task<string> GetNextCodeAsync(string prefix = "HSN");
    Task<IEnumerable<HsnSacDto>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<HsnSacDto> CreateAsync(long companyId, CreateHsnSacRequest request);
    Task<HsnSacDto> UpdateAsync(long id, long companyId, UpdateHsnSacRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class HsnSacService : IHsnSacService
{
    private readonly IHsnSacRepository _repository;
    private readonly ITaxRepository _taxRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public HsnSacService(IHsnSacRepository repository, ITaxRepository taxRepository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _taxRepository = taxRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<HsnSacDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search ?? string.Empty);
        var total = await _repository.CountAsync(companyId, search ?? string.Empty);
        var dtos = items.Select(ToDto).ToList();
        await EnrichTaxNameAsync(companyId, dtos);
        return new PaginatedResult<HsnSacDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<HsnSacDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"HSN/SAC '{id}' was not found.");
        var dto = ToDto(entity);
        await EnrichTaxNameAsync(entity.CompanyId, new List<HsnSacDto> { dto });
        return dto;
    }

    public async Task<string> GetNextCodeAsync(string prefix = "HSN")
        => await _repository.GetNextCodeAsync(_currentUser.CompanyId, prefix);

    public async Task<IEnumerable<HsnSacDto>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        var items = (await _repository.GetAllAsync(companyId, includeInactive)).ToList();
        var dtos = items.Select(ToDto).ToList();
        await EnrichTaxNameAsync(companyId, dtos);
        return dtos;
    }

    public async Task<HsnSacDto> CreateAsync(long companyId, CreateHsnSacRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.Code)
            ? await _repository.GetNextCodeAsync(companyId)
            : request.Code.Trim().ToUpper();
        if (await _repository.CodeInUseAsync(companyId, code))
            throw new DomainException($"HSN/SAC code '{code}' is already in use.");

        var entity = new HsnSac
        {
            CompanyId = companyId,
            Code = code,
            Name = request.Name.Trim(),
            HsnSacType = request.HsnSacType.ToUpper(),
            Description = request.Description?.Trim(),
            TaxId = request.TaxId,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.HsnSacId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("HsnSac", entity.HsnSacId.ToString(), "Create", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichTaxNameAsync(companyId, new List<HsnSacDto> { dto });
        return dto;
    }

    public async Task<HsnSacDto> UpdateAsync(long id, long companyId, UpdateHsnSacRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"HSN/SAC '{id}' was not found.");

        var code = string.IsNullOrWhiteSpace(request.Code) ? entity.Code : request.Code.Trim().ToUpper();
        if (code != entity.Code && await _repository.CodeInUseAsync(companyId, code, id))
            throw new DomainException($"HSN/SAC code '{code}' is already in use.");

        entity.Code = code;
        entity.Name = request.Name.Trim();
        entity.HsnSacType = request.HsnSacType.ToUpper();
        entity.Description = request.Description?.Trim();
        entity.TaxId = request.TaxId;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("HsnSac", id.ToString(), "Update", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichTaxNameAsync(companyId, new List<HsnSacDto> { dto });
        return dto;
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"HSN/SAC '{id}' was not found.");
        await _repository.SoftDeleteAsync(id, companyId, _currentUser.UserId);
        await _auditService.WriteAsync("HsnSac", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static HsnSacDto ToDto(HsnSac e) => new()
    {
        HsnSacId = e.HsnSacId,
        CompanyId = e.CompanyId,
        Code = e.Code,
        Name = e.Name,
        HsnSacType = e.HsnSacType,
        Description = e.Description,
        TaxId = e.TaxId,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt
    };

    private async Task EnrichTaxNameAsync(long companyId, List<HsnSacDto> dtos)
    {
        var taxIds = dtos.Where(d => d.TaxId.HasValue).Select(d => d.TaxId!.Value).Distinct().ToList();
        if (taxIds.Count == 0) return;
        var names = await _taxRepository.GetNamesAsync(companyId, taxIds);
        foreach (var d in dtos)
        {
            if (d.TaxId.HasValue && names.TryGetValue(d.TaxId.Value, out var n)) d.TaxName = n;
        }
    }
}

/* ---------------- Service Categories (company) ---------------- */

public interface IServiceCategoryService
{
    Task<PaginatedResult<ServiceCategoryDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<ServiceCategoryDto> GetByIdAsync(long id);
    Task<string> GetNextCodeAsync(string prefix = "SVCAT");
    Task<IEnumerable<ServiceCategoryDto>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<ServiceCategoryDto> CreateAsync(long companyId, CreateServiceCategoryRequest request);
    Task<ServiceCategoryDto> UpdateAsync(long id, long companyId, UpdateServiceCategoryRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class ServiceCategoryService : IServiceCategoryService
{
    private readonly IServiceCategoryRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public ServiceCategoryService(IServiceCategoryRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<ServiceCategoryDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search ?? string.Empty);
        var total = await _repository.CountAsync(companyId, search ?? string.Empty);
        return new PaginatedResult<ServiceCategoryDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<ServiceCategoryDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Service category '{id}' was not found.");
        return ToDto(entity);
    }

    public async Task<string> GetNextCodeAsync(string prefix = "SVCAT")
        => await _repository.GetNextCodeAsync(_currentUser.CompanyId, prefix);

    public async Task<IEnumerable<ServiceCategoryDto>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(companyId, includeInactive);
        return items.Select(ToDto).ToList();
    }

    public async Task<ServiceCategoryDto> CreateAsync(long companyId, CreateServiceCategoryRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.Code)
            ? await _repository.GetNextCodeAsync(companyId)
            : request.Code.Trim().ToUpper();
        if (await _repository.CodeInUseAsync(companyId, code))
            throw new DomainException($"Service category code '{code}' is already in use.");

        var entity = new ServiceCategory
        {
            CompanyId = companyId,
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            DisplayOrder = request.DisplayOrder ?? 0,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.ServiceCategoryId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("ServiceCategory", entity.ServiceCategoryId.ToString(), "Create", _currentUser.Username);
        return ToDto(entity);
    }

    public async Task<ServiceCategoryDto> UpdateAsync(long id, long companyId, UpdateServiceCategoryRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Service category '{id}' was not found.");

        var code = string.IsNullOrWhiteSpace(request.Code) ? entity.Code : request.Code.Trim().ToUpper();
        if (code != entity.Code && await _repository.CodeInUseAsync(companyId, code, id))
            throw new DomainException($"Service category code '{code}' is already in use.");

        entity.Code = code;
        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.DisplayOrder = request.DisplayOrder;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("ServiceCategory", id.ToString(), "Update", _currentUser.Username);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Service category '{id}' was not found.");
        await _repository.SoftDeleteAsync(id, companyId, _currentUser.UserId);
        await _auditService.WriteAsync("ServiceCategory", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static ServiceCategoryDto ToDto(ServiceCategory e) => new()
    {
        ServiceCategoryId = e.ServiceCategoryId,
        CompanyId = e.CompanyId,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        DisplayOrder = e.DisplayOrder,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt
    };
}

/* ---------------- Services (company) ---------------- */

public interface IServiceService
{
    Task<PaginatedResult<ServiceDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<ServiceDto> GetByIdAsync(long id);
    Task<string> GetNextCodeAsync(string prefix = "SRV");
    Task<IEnumerable<ServiceDto>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<ServiceDto> CreateAsync(long companyId, CreateServiceRequest request);
    Task<ServiceDto> UpdateAsync(long id, long companyId, UpdateServiceRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class ServiceService : IServiceService
{
    private readonly IServiceRepository _repository;
    private readonly IServiceCategoryRepository _categoryRepository;
    private readonly IHsnSacRepository _hsnSacRepository;
    private readonly IProductUnitRepository _unitRepository;
    private readonly ITaxRepository _taxRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public ServiceService(
        IServiceRepository repository,
        IServiceCategoryRepository categoryRepository,
        IHsnSacRepository hsnSacRepository,
        IProductUnitRepository unitRepository,
        ITaxRepository taxRepository,
        IAuditService auditService,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _hsnSacRepository = hsnSacRepository;
        _unitRepository = unitRepository;
        _taxRepository = taxRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<ServiceDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search ?? string.Empty);
        var total = await _repository.CountAsync(companyId, search ?? string.Empty);
        var dtos = items.Select(ToDto).ToList();
        await EnrichAsync(companyId, dtos);
        return new PaginatedResult<ServiceDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<ServiceDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Service '{id}' was not found.");
        var dto = ToDto(entity);
        await EnrichAsync(entity.CompanyId, new List<ServiceDto> { dto });
        return dto;
    }

    public async Task<string> GetNextCodeAsync(string prefix = "SRV")
        => await _repository.GetNextCodeAsync(_currentUser.CompanyId, prefix);

    public async Task<IEnumerable<ServiceDto>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(companyId, includeInactive);
        var dtos = items.Select(ToDto).ToList();
        await EnrichAsync(companyId, dtos);
        return dtos;
    }

    public async Task<ServiceDto> CreateAsync(long companyId, CreateServiceRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.Code)
            ? await _repository.GetNextCodeAsync(companyId)
            : request.Code.Trim().ToUpper();
        if (await _repository.CodeInUseAsync(companyId, code))
            throw new DomainException($"Service code '{code}' is already in use.");

        var entity = new Service
        {
            CompanyId = companyId,
            Code = code,
            Name = request.Name.Trim(),
            ServiceCategoryId = request.ServiceCategoryId,
            UnitId = request.UnitId,
            HsnSacId = request.HsnSacId,
            DefaultTaxId = request.DefaultTaxId,
            StandardRate = request.StandardRate,
            IsTaxInclusive = request.IsTaxInclusive,
            Description = request.Description?.Trim(),
            IsActive = true,
            EntityId = request.EntityId,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.ServiceId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("Service", entity.ServiceId.ToString(), "Create", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<ServiceDto> { dto });
        return dto;
    }

    public async Task<ServiceDto> UpdateAsync(long id, long companyId, UpdateServiceRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Service '{id}' was not found.");

        var code = string.IsNullOrWhiteSpace(request.Code) ? entity.Code : request.Code.Trim().ToUpper();
        if (code != entity.Code && await _repository.CodeInUseAsync(companyId, code, id))
            throw new DomainException($"Service code '{code}' is already in use.");

        entity.Code = code;
        entity.Name = request.Name.Trim();
        entity.ServiceCategoryId = request.ServiceCategoryId;
        entity.UnitId = request.UnitId;
        entity.HsnSacId = request.HsnSacId;
        entity.DefaultTaxId = request.DefaultTaxId;
        entity.StandardRate = request.StandardRate;
        entity.IsTaxInclusive = request.IsTaxInclusive;
        entity.Description = request.Description?.Trim();
        entity.IsActive = request.IsActive;
        entity.EntityId = request.EntityId;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("Service", id.ToString(), "Update", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<ServiceDto> { dto });
        return dto;
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Service '{id}' was not found.");
        await _repository.SoftDeleteAsync(id, companyId, _currentUser.UserId);
        await _auditService.WriteAsync("Service", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static ServiceDto ToDto(Service e) => new()
    {
        ServiceId = e.ServiceId,
        EntityId = e.EntityId,
        CompanyId = e.CompanyId,
        Code = e.Code,
        Name = e.Name,
        ServiceCategoryId = e.ServiceCategoryId,
        UnitId = e.UnitId,
        HsnSacId = e.HsnSacId,
        DefaultTaxId = e.DefaultTaxId,
        StandardRate = e.StandardRate,
        IsTaxInclusive = e.IsTaxInclusive,
        Description = e.Description,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt
    };

    private async Task EnrichAsync(long companyId, List<ServiceDto> dtos)
    {
        var categoryIds = dtos.Where(d => d.ServiceCategoryId.HasValue).Select(d => d.ServiceCategoryId!.Value).Distinct().ToList();
        var unitIds = dtos.Where(d => d.UnitId.HasValue).Select(d => d.UnitId!.Value).Distinct().ToList();
        var hsnIds = dtos.Where(d => d.HsnSacId.HasValue).Select(d => d.HsnSacId!.Value).Distinct().ToList();
        var taxIds = dtos.Where(d => d.DefaultTaxId.HasValue).Select(d => d.DefaultTaxId!.Value).Distinct().ToList();

        var categoryNames = categoryIds.Count > 0 ? await _categoryRepository.GetNamesAsync(companyId, categoryIds) : new Dictionary<long, string>();
        var unitNames = unitIds.Count > 0 ? await _unitRepository.GetNamesAsync(companyId, unitIds) : new Dictionary<long, string>();
        var hsnCodes = hsnIds.Count > 0 ? await _hsnSacRepository.GetNamesAsync(companyId, hsnIds) : new Dictionary<long, string>();
        var taxNames = taxIds.Count > 0 ? await _taxRepository.GetNamesAsync(companyId, taxIds) : new Dictionary<long, string>();

        foreach (var d in dtos)
        {
            if (d.ServiceCategoryId.HasValue && categoryNames.TryGetValue(d.ServiceCategoryId.Value, out var cn)) d.ServiceCategoryName = cn;
            if (d.UnitId.HasValue && unitNames.TryGetValue(d.UnitId.Value, out var un)) d.UnitName = un;
            if (d.HsnSacId.HasValue && hsnCodes.TryGetValue(d.HsnSacId.Value, out var hc)) d.HsnSacCode = hc;
            if (d.DefaultTaxId.HasValue && taxNames.TryGetValue(d.DefaultTaxId.Value, out var tn)) d.DefaultTaxName = tn;
        }
    }
}

/* ---------------- Price Lists (company) ---------------- */

public interface IPriceListService
{
    Task<PaginatedResult<PriceListDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<PriceListDto> GetByIdAsync(long id);
    Task<string> GetNextCodeAsync(string prefix = "PL");
    Task<IEnumerable<PriceListDto>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<PriceListDto> CreateAsync(long companyId, CreatePriceListRequest request);
    Task<PriceListDto> UpdateAsync(long id, long companyId, UpdatePriceListRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class PriceListService : IPriceListService
{
    private readonly IPriceListRepository _repository;
    private readonly IPriceTypeRepository _priceTypeRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public PriceListService(IPriceListRepository repository, IPriceTypeRepository priceTypeRepository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _priceTypeRepository = priceTypeRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<PriceListDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search ?? string.Empty);
        var total = await _repository.CountAsync(companyId, search ?? string.Empty);
        var dtos = items.Select(ToDto).ToList();
        await EnrichAsync(companyId, dtos);
        return new PaginatedResult<PriceListDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<PriceListDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Price list '{id}' was not found.");
        var dto = ToDto(entity);
        await EnrichAsync(entity.CompanyId, new List<PriceListDto> { dto });
        return dto;
    }

    public async Task<string> GetNextCodeAsync(string prefix = "PL")
        => await _repository.GetNextCodeAsync(_currentUser.CompanyId, prefix);

    public async Task<IEnumerable<PriceListDto>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(companyId, includeInactive);
        var dtos = items.Select(ToDto).ToList();
        await EnrichAsync(companyId, dtos);
        return dtos;
    }

    public async Task<PriceListDto> CreateAsync(long companyId, CreatePriceListRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.Code)
            ? await _repository.GetNextCodeAsync(companyId)
            : request.Code.Trim().ToUpper();
        if (await _repository.CodeInUseAsync(companyId, code))
            throw new DomainException($"Price list code '{code}' is already in use.");

        var entity = new PriceList
        {
            CompanyId = companyId,
            PriceTypeId = request.PriceTypeId,
            CurrencyId = request.CurrencyId,
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            IsDefault = request.IsDefault,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.PriceListId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("PriceList", entity.PriceListId.ToString(), "Create", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<PriceListDto> { dto });
        return dto;
    }

    public async Task<PriceListDto> UpdateAsync(long id, long companyId, UpdatePriceListRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Price list '{id}' was not found.");

        var code = string.IsNullOrWhiteSpace(request.Code) ? entity.Code : request.Code.Trim().ToUpper();
        if (code != entity.Code && await _repository.CodeInUseAsync(companyId, code, id))
            throw new DomainException($"Price list code '{code}' is already in use.");

        entity.PriceTypeId = request.PriceTypeId;
        entity.CurrencyId = request.CurrencyId;
        entity.Code = code;
        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.EffectiveFrom = request.EffectiveFrom;
        entity.EffectiveTo = request.EffectiveTo;
        entity.IsDefault = request.IsDefault;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("PriceList", id.ToString(), "Update", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<PriceListDto> { dto });
        return dto;
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Price list '{id}' was not found.");
        await _repository.SoftDeleteAsync(id, companyId, _currentUser.UserId);
        await _auditService.WriteAsync("PriceList", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static PriceListDto ToDto(PriceList e) => new()
    {
        PriceListId = e.PriceListId,
        CompanyId = e.CompanyId,
        PriceTypeId = e.PriceTypeId,
        CurrencyId = e.CurrencyId,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        EffectiveFrom = e.EffectiveFrom,
        EffectiveTo = e.EffectiveTo,
        IsDefault = e.IsDefault,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt
    };

    private async Task EnrichAsync(long companyId, List<PriceListDto> dtos)
    {
        var priceTypeIds = dtos.Select(d => d.PriceTypeId).Distinct().ToList();
        var currencyIds = dtos.Select(d => d.CurrencyId).Distinct().ToList();

        var priceTypeNames = await _priceTypeRepository.GetNamesAsync(priceTypeIds);
        var currencyNames = currencyIds.Count > 0 ? await _repository.GetCurrencyNamesAsync(currencyIds) : new Dictionary<int, string>();

        foreach (var d in dtos)
        {
            if (priceTypeNames.TryGetValue(d.PriceTypeId, out var pn)) d.PriceTypeName = pn;
            if (currencyNames.TryGetValue(d.CurrencyId, out var cn)) d.CurrencyName = cn;
        }
    }
}

/* ---------------- Price List Details (child of PriceList) ---------------- */

public interface IPriceListDetailService
{
    Task<IEnumerable<PriceListDetailDto>> GetByPriceListAsync(long priceListId);
    Task<PriceListDetailDto> GetByIdAsync(long id);
    Task<PriceListDetailDto> CreateAsync(long companyId, CreatePriceListDetailRequest request);
    Task<PriceListDetailDto> UpdateAsync(long id, long companyId, UpdatePriceListDetailRequest request);
    Task<bool> DeleteAsync(long id);
    Task<int> ReplaceAsync(long companyId, long priceListId, PriceListDetailsReplaceRequest request);
}

public class PriceListDetailService : IPriceListDetailService
{
    private readonly IPriceListDetailRepository _detailRepository;
    private readonly IPriceListRepository _priceListRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public PriceListDetailService(IPriceListDetailRepository detailRepository, IPriceListRepository priceListRepository, IAuditService auditService, ICurrentUser currentUser)
    {
        _detailRepository = detailRepository;
        _priceListRepository = priceListRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<PriceListDetailDto>> GetByPriceListAsync(long priceListId)
    {
        var items = await _detailRepository.GetByPriceListAsync(priceListId);
        var dtos = items.Select(ToDto).ToList();
        await EnrichAsync(_currentUser.CompanyId, dtos);
        return dtos;
    }

    public async Task<PriceListDetailDto> GetByIdAsync(long id)
    {
        var entity = await _detailRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Price list detail '{id}' was not found.");
        var dto = ToDto(entity);
        await EnrichAsync(_currentUser.CompanyId, new List<PriceListDetailDto> { dto });
        return dto;
    }

    public async Task<PriceListDetailDto> CreateAsync(long companyId, CreatePriceListDetailRequest request)
    {
        await EnsurePriceListAsync(companyId, request.PriceListId);
        if (await _detailRepository.ProductInUseAsync(request.PriceListId, request.ProductId, request.UnitId))
            throw new DomainException("The product/unit combination already exists in this price list.");

        var entity = new PriceListDetail
        {
            PriceListId = request.PriceListId,
            ProductId = request.ProductId,
            UnitId = request.UnitId,
            Price = request.Price,
            MinimumQuantity = request.MinimumQuantity,
            MaximumQuantity = request.MaximumQuantity,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.PriceListDetailId = await _detailRepository.InsertAsync(entity);
        await _auditService.WriteAsync("PriceListDetail", entity.PriceListDetailId.ToString(), "Create", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<PriceListDetailDto> { dto });
        return dto;
    }

    public async Task<PriceListDetailDto> UpdateAsync(long id, long companyId, UpdatePriceListDetailRequest request)
    {
        var entity = await _detailRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Price list detail '{id}' was not found.");
        await EnsurePriceListAsync(companyId, request.PriceListId);
        if (await _detailRepository.ProductInUseAsync(request.PriceListId, request.ProductId, request.UnitId, id))
            throw new DomainException("The product/unit combination already exists in this price list.");

        entity.PriceListId = request.PriceListId;
        entity.ProductId = request.ProductId;
        entity.UnitId = request.UnitId;
        entity.Price = request.Price;
        entity.MinimumQuantity = request.MinimumQuantity;
        entity.MaximumQuantity = request.MaximumQuantity;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _detailRepository.UpdateAsync(entity);
        await _auditService.WriteAsync("PriceListDetail", id.ToString(), "Update", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<PriceListDetailDto> { dto });
        return dto;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _detailRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Price list detail '{id}' was not found.");
        await _detailRepository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("PriceListDetail", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    public async Task<int> ReplaceAsync(long companyId, long priceListId, PriceListDetailsReplaceRequest request)
    {
        await EnsurePriceListAsync(companyId, priceListId);

        var duplicates = request.Items
            .Where(i => request.Items.Any(o => o != i && o.ProductId == i.ProductId && o.UnitId == i.UnitId))
            .Select(i => $"product {i.ProductId}")
            .Distinct()
            .ToList();
        if (duplicates.Count > 0)
            throw new DomainException($"Duplicate entries in price list details for {string.Join(", ", duplicates)}.");

        var details = request.Items.Select(i => new PriceListDetail
        {
            PriceListId = priceListId,
            ProductId = i.ProductId,
            UnitId = i.UnitId,
            Price = i.Price,
            MinimumQuantity = i.MinimumQuantity,
            MaximumQuantity = i.MaximumQuantity,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        }).ToList();

        await _detailRepository.DeleteByPriceListAsync(priceListId);
        foreach (var d in details)
            await _detailRepository.InsertAsync(d);

        await _auditService.WriteAsync("PriceList", priceListId.ToString(), "ReplaceDetails", _currentUser.Username);
        return details.Count;
    }

    private async Task EnsurePriceListAsync(long companyId, long priceListId)
    {
        var priceList = await _priceListRepository.GetByIdAsync(priceListId)
            ?? throw new NotFoundException($"Price list '{priceListId}' was not found.");
        if (priceList.CompanyId != companyId)
            throw new DomainException("Price list does not belong to the current company.");
    }

    private static PriceListDetailDto ToDto(PriceListDetail e) => new()
    {
        PriceListDetailId = e.PriceListDetailId,
        PriceListId = e.PriceListId,
        ProductId = e.ProductId,
        UnitId = e.UnitId,
        Price = e.Price,
        MinimumQuantity = e.MinimumQuantity,
        MaximumQuantity = e.MaximumQuantity,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt
    };

    private async Task EnrichAsync(long companyId, List<PriceListDetailDto> dtos)
    {
        var productIds = dtos.Select(d => d.ProductId).Distinct().ToList();
        var unitIds = dtos.Where(d => d.UnitId.HasValue).Select(d => d.UnitId!.Value).Distinct().ToList();

        var productNames = await _detailRepository.GetProductNamesAsync(companyId, productIds);
        var unitNames = unitIds.Count > 0 ? await _detailRepository.GetUnitNamesAsync(companyId, unitIds) : new Dictionary<long, string>();

        foreach (var d in dtos)
        {
            if (productNames.TryGetValue(d.ProductId, out var pn)) d.ProductName = pn;
            if (d.UnitId.HasValue && unitNames.TryGetValue(d.UnitId.Value, out var un)) d.UnitName = un;
        }
    }
}

/* ---------------- Discount Rules (company) ---------------- */

public interface IDiscountRuleService
{
    Task<PaginatedResult<DiscountRuleDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<DiscountRuleDto> GetByIdAsync(long id);
    Task<string> GetNextCodeAsync(string prefix = "DR");
    Task<DiscountRuleDto> CreateAsync(long companyId, CreateDiscountRuleRequest request);
    Task<DiscountRuleDto> UpdateAsync(long id, long companyId, UpdateDiscountRuleRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class DiscountRuleService : IDiscountRuleService
{
    private readonly IDiscountRuleRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public DiscountRuleService(IDiscountRuleRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<DiscountRuleDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search ?? string.Empty);
        var total = await _repository.CountAsync(companyId, search ?? string.Empty);
        var dtos = items.Select(ToDto).ToList();
        await EnrichAsync(companyId, dtos);
        return new PaginatedResult<DiscountRuleDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<DiscountRuleDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Discount rule '{id}' was not found.");
        var dto = ToDto(entity);
        await EnrichAsync(entity.CompanyId, new List<DiscountRuleDto> { dto });
        return dto;
    }

    public async Task<string> GetNextCodeAsync(string prefix = "DR")
        => await _repository.GetNextCodeAsync(_currentUser.CompanyId, prefix);

    public async Task<DiscountRuleDto> CreateAsync(long companyId, CreateDiscountRuleRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.Code)
            ? await _repository.GetNextCodeAsync(companyId)
            : request.Code.Trim().ToUpper();
        if (await _repository.CodeInUseAsync(companyId, code))
            throw new DomainException($"Discount rule code '{code}' is already in use.");

        var entity = new DiscountRule
        {
            CompanyId = companyId,
            Code = code,
            Name = request.Name.Trim(),
            ProductId = request.ProductId,
            ServiceId = request.ServiceId,
            ProductCategoryId = request.ProductCategoryId,
            PriceListId = request.PriceListId,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinimumQuantity = request.MinimumQuantity,
            MinimumAmount = request.MinimumAmount,
            MaximumDiscount = request.MaximumDiscount,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.DiscountRuleId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("DiscountRule", entity.DiscountRuleId.ToString(), "Create", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<DiscountRuleDto> { dto });
        return dto;
    }

    public async Task<DiscountRuleDto> UpdateAsync(long id, long companyId, UpdateDiscountRuleRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Discount rule '{id}' was not found.");

        var code = string.IsNullOrWhiteSpace(request.Code) ? entity.Code : request.Code.Trim().ToUpper();
        if (code != entity.Code && await _repository.CodeInUseAsync(companyId, code, id))
            throw new DomainException($"Discount rule code '{code}' is already in use.");

        entity.Code = code;
        entity.Name = request.Name.Trim();
        entity.ProductId = request.ProductId;
        entity.ServiceId = request.ServiceId;
        entity.ProductCategoryId = request.ProductCategoryId;
        entity.PriceListId = request.PriceListId;
        entity.DiscountType = request.DiscountType;
        entity.DiscountValue = request.DiscountValue;
        entity.MinimumQuantity = request.MinimumQuantity;
        entity.MinimumAmount = request.MinimumAmount;
        entity.MaximumDiscount = request.MaximumDiscount;
        entity.EffectiveFrom = request.EffectiveFrom;
        entity.EffectiveTo = request.EffectiveTo;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("DiscountRule", id.ToString(), "Update", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<DiscountRuleDto> { dto });
        return dto;
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Discount rule '{id}' was not found.");
        await _repository.SoftDeleteAsync(id, companyId, _currentUser.UserId);
        await _auditService.WriteAsync("DiscountRule", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static DiscountRuleDto ToDto(DiscountRule e) => new()
    {
        DiscountRuleId = e.DiscountRuleId,
        CompanyId = e.CompanyId,
        Code = e.Code,
        Name = e.Name,
        ProductId = e.ProductId,
        ServiceId = e.ServiceId,
        ProductCategoryId = e.ProductCategoryId,
        PriceListId = e.PriceListId,
        DiscountType = e.DiscountType,
        DiscountValue = e.DiscountValue,
        MinimumQuantity = e.MinimumQuantity,
        MinimumAmount = e.MinimumAmount,
        MaximumDiscount = e.MaximumDiscount,
        EffectiveFrom = e.EffectiveFrom,
        EffectiveTo = e.EffectiveTo,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt
    };

    private async Task EnrichAsync(long companyId, List<DiscountRuleDto> dtos)
    {
        var productIds = dtos.Where(d => d.ProductId.HasValue).Select(d => d.ProductId!.Value).Distinct().ToList();
        var serviceIds = dtos.Where(d => d.ServiceId.HasValue).Select(d => d.ServiceId!.Value).Distinct().ToList();
        var categoryIds = dtos.Where(d => d.ProductCategoryId.HasValue).Select(d => d.ProductCategoryId!.Value).Distinct().ToList();
        var priceListIds = dtos.Where(d => d.PriceListId.HasValue).Select(d => d.PriceListId!.Value).Distinct().ToList();

        var productNames = productIds.Count > 0 ? await _repository.GetProductNamesAsync(companyId, productIds) : new Dictionary<long, string>();
        var serviceNames = serviceIds.Count > 0 ? await _repository.GetServiceNamesAsync(companyId, serviceIds) : new Dictionary<long, string>();
        var categoryNames = categoryIds.Count > 0 ? await _repository.GetCategoryNamesAsync(companyId, categoryIds) : new Dictionary<long, string>();
        var priceListNames = priceListIds.Count > 0 ? await _repository.GetPriceListNamesAsync(companyId, priceListIds) : new Dictionary<long, string>();

        foreach (var d in dtos)
        {
            if (d.ProductId.HasValue && productNames.TryGetValue(d.ProductId.Value, out var pn)) d.ProductName = pn;
            if (d.ServiceId.HasValue && serviceNames.TryGetValue(d.ServiceId.Value, out var sn)) d.ServiceName = sn;
            if (d.ProductCategoryId.HasValue && categoryNames.TryGetValue(d.ProductCategoryId.Value, out var cn)) d.ProductCategoryName = cn;
            if (d.PriceListId.HasValue && priceListNames.TryGetValue(d.PriceListId.Value, out var pln)) d.PriceListName = pln;
        }
    }
}

/* ---------------- Offers (company) ---------------- */

public interface IOfferService
{
    Task<PaginatedResult<OfferDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<OfferDto> GetByIdAsync(long id);
    Task<string> GetNextCodeAsync(string prefix = "OFFER");
    Task<OfferDto> CreateAsync(long companyId, CreateOfferRequest request);
    Task<OfferDto> UpdateAsync(long id, long companyId, UpdateOfferRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class OfferService : IOfferService
{
    private readonly IOfferRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public OfferService(IOfferRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<OfferDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search ?? string.Empty);
        var total = await _repository.CountAsync(companyId, search ?? string.Empty);
        var dtos = items.Select(ToDto).ToList();
        return new PaginatedResult<OfferDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<OfferDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Offer '{id}' was not found.");
        return ToDto(entity);
    }

    public async Task<string> GetNextCodeAsync(string prefix = "OFFER")
        => await _repository.GetNextCodeAsync(_currentUser.CompanyId, prefix);

    public async Task<OfferDto> CreateAsync(long companyId, CreateOfferRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.Code)
            ? await _repository.GetNextCodeAsync(companyId)
            : request.Code.Trim().ToUpper();
        if (await _repository.CodeInUseAsync(companyId, code))
            throw new DomainException($"Offer code '{code}' is already in use.");

        var entity = new Offer
        {
            CompanyId = companyId,
            Code = code,
            Name = request.Name.Trim(),
            OfferType = request.OfferType,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinimumQuantity = request.MinimumQuantity,
            MinimumAmount = request.MinimumAmount,
            MaximumDiscount = request.MaximumDiscount,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate?.Date,
            IsActive = true,
            Description = request.Description?.Trim(),
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.OfferId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("Offer", entity.OfferId.ToString(), "Create", _currentUser.Username);
        return ToDto(entity);
    }

    public async Task<OfferDto> UpdateAsync(long id, long companyId, UpdateOfferRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Offer '{id}' was not found.");

        var code = string.IsNullOrWhiteSpace(request.Code) ? entity.Code : request.Code.Trim().ToUpper();
        if (code != entity.Code && await _repository.CodeInUseAsync(companyId, code, id))
            throw new DomainException($"Offer code '{code}' is already in use.");

        entity.Code = code;
        entity.Name = request.Name.Trim();
        entity.OfferType = request.OfferType;
        entity.DiscountType = request.DiscountType;
        entity.DiscountValue = request.DiscountValue;
        entity.MinimumQuantity = request.MinimumQuantity;
        entity.MinimumAmount = request.MinimumAmount;
        entity.MaximumDiscount = request.MaximumDiscount;
        entity.StartDate = request.StartDate.Date;
        entity.EndDate = request.EndDate?.Date;
        entity.IsActive = request.IsActive;
        entity.Description = request.Description?.Trim();
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("Offer", id.ToString(), "Update", _currentUser.Username);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Offer '{id}' was not found.");
        await _repository.SoftDeleteAsync(id, companyId, _currentUser.UserId);
        await _auditService.WriteAsync("Offer", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static OfferDto ToDto(Offer e) => new()
    {
        OfferId = e.OfferId,
        CompanyId = e.CompanyId,
        Code = e.Code,
        Name = e.Name,
        OfferType = e.OfferType,
        DiscountType = e.DiscountType,
        DiscountValue = e.DiscountValue,
        MinimumQuantity = e.MinimumQuantity,
        MinimumAmount = e.MinimumAmount,
        MaximumDiscount = e.MaximumDiscount,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        IsActive = e.IsActive,
        Description = e.Description,
        CreatedAt = e.CreatedAt
    };
}

/* ---------------- Offer Details (child of Offer) ---------------- */

public interface IOfferDetailService
{
    Task<IEnumerable<OfferDetailDto>> GetByOfferAsync(long offerId);
    Task<OfferDetailDto> GetByIdAsync(long id);
    Task<OfferDetailDto> CreateAsync(long companyId, CreateOfferDetailRequest request);
    Task<OfferDetailDto> UpdateAsync(long id, long companyId, UpdateOfferDetailRequest request);
    Task<bool> DeleteAsync(long id);
    Task<int> ReplaceAsync(long companyId, long offerId, OfferDetailsReplaceRequest request);
}

public class OfferDetailService : IOfferDetailService
{
    private readonly IOfferDetailRepository _detailRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public OfferDetailService(IOfferDetailRepository detailRepository, IOfferRepository offerRepository, IAuditService auditService, ICurrentUser currentUser)
    {
        _detailRepository = detailRepository;
        _offerRepository = offerRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<OfferDetailDto>> GetByOfferAsync(long offerId)
    {
        var items = await _detailRepository.GetByOfferAsync(offerId);
        var dtos = items.Select(ToDto).ToList();
        await EnrichAsync(_currentUser.CompanyId, dtos);
        return dtos;
    }

    public async Task<OfferDetailDto> GetByIdAsync(long id)
    {
        var entity = await _detailRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Offer detail '{id}' was not found.");
        var dto = ToDto(entity);
        await EnrichAsync(_currentUser.CompanyId, new List<OfferDetailDto> { dto });
        return dto;
    }

    public async Task<OfferDetailDto> CreateAsync(long companyId, CreateOfferDetailRequest request)
    {
        await EnsureOfferAsync(companyId, request.OfferId);
        if (await _detailRepository.TargetInUseAsync(request.OfferId, request.ProductId, request.ServiceId, request.ProductCategoryId))
            throw new DomainException("The product/service/category target already exists in this offer.");

        var entity = new OfferDetail
        {
            OfferId = request.OfferId,
            ProductId = request.ProductId,
            ServiceId = request.ServiceId,
            ProductCategoryId = request.ProductCategoryId,
            MinimumQuantity = request.MinimumQuantity,
            FreeQuantity = request.FreeQuantity,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.OfferDetailId = await _detailRepository.InsertAsync(entity);
        await _auditService.WriteAsync("OfferDetail", entity.OfferDetailId.ToString(), "Create", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<OfferDetailDto> { dto });
        return dto;
    }

    public async Task<OfferDetailDto> UpdateAsync(long id, long companyId, UpdateOfferDetailRequest request)
    {
        var entity = await _detailRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Offer detail '{id}' was not found.");
        await EnsureOfferAsync(companyId, request.OfferId);
        if (await _detailRepository.TargetInUseAsync(request.OfferId, request.ProductId, request.ServiceId, request.ProductCategoryId, id))
            throw new DomainException("The product/service/category target already exists in this offer.");

        entity.OfferId = request.OfferId;
        entity.ProductId = request.ProductId;
        entity.ServiceId = request.ServiceId;
        entity.ProductCategoryId = request.ProductCategoryId;
        entity.MinimumQuantity = request.MinimumQuantity;
        entity.FreeQuantity = request.FreeQuantity;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _detailRepository.UpdateAsync(entity);
        await _auditService.WriteAsync("OfferDetail", id.ToString(), "Update", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<OfferDetailDto> { dto });
        return dto;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _detailRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Offer detail '{id}' was not found.");
        await _detailRepository.SoftDeleteAsync(id, _currentUser.UserId);
        await _auditService.WriteAsync("OfferDetail", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    public async Task<int> ReplaceAsync(long companyId, long offerId, OfferDetailsReplaceRequest request)
    {
        await EnsureOfferAsync(companyId, offerId);

        var keys = request.Items
            .Select(i => TargetKey(i.ProductId, i.ServiceId, i.ProductCategoryId))
            .Where(k => k != null)
            .ToList();
        var duplicateKeys = keys.GroupBy(k => k).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateKeys.Count > 0)
            throw new DomainException($"Duplicate entries in offer details for {string.Join(", ", duplicateKeys)}.");

        var details = request.Items.Select(i => new OfferDetail
        {
            OfferId = offerId,
            ProductId = i.ProductId,
            ServiceId = i.ServiceId,
            ProductCategoryId = i.ProductCategoryId,
            MinimumQuantity = i.MinimumQuantity,
            FreeQuantity = i.FreeQuantity,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        }).ToList();

        await _detailRepository.DeleteByOfferAsync(offerId);
        foreach (var d in details)
            await _detailRepository.InsertAsync(d);

        await _auditService.WriteAsync("Offer", offerId.ToString(), "ReplaceDetails", _currentUser.Username);
        return details.Count;
    }

    private static string? TargetKey(long? productId, long? serviceId, long? productCategoryId)
    {
        if (productId.HasValue) return $"product:{productId.Value}";
        if (serviceId.HasValue) return $"service:{serviceId.Value}";
        if (productCategoryId.HasValue) return $"category:{productCategoryId.Value}";
        return null;
    }

    private async Task EnsureOfferAsync(long companyId, long offerId)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId)
            ?? throw new NotFoundException($"Offer '{offerId}' was not found.");
        if (offer.CompanyId != companyId)
            throw new DomainException("Offer does not belong to the current company.");
    }

    private static OfferDetailDto ToDto(OfferDetail e) => new()
    {
        OfferDetailId = e.OfferDetailId,
        OfferId = e.OfferId,
        ProductId = e.ProductId,
        ServiceId = e.ServiceId,
        ProductCategoryId = e.ProductCategoryId,
        MinimumQuantity = e.MinimumQuantity,
        FreeQuantity = e.FreeQuantity,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt
    };

    private async Task EnrichAsync(long companyId, List<OfferDetailDto> dtos)
    {
        var productIds = dtos.Where(d => d.ProductId.HasValue).Select(d => d.ProductId!.Value).Distinct().ToList();
        var serviceIds = dtos.Where(d => d.ServiceId.HasValue).Select(d => d.ServiceId!.Value).Distinct().ToList();
        var categoryIds = dtos.Where(d => d.ProductCategoryId.HasValue).Select(d => d.ProductCategoryId!.Value).Distinct().ToList();

        var productNames = productIds.Count > 0 ? await _detailRepository.GetProductNamesAsync(companyId, productIds) : new Dictionary<long, string>();
        var serviceNames = serviceIds.Count > 0 ? await _detailRepository.GetServiceNamesAsync(companyId, serviceIds) : new Dictionary<long, string>();
        var categoryNames = categoryIds.Count > 0 ? await _detailRepository.GetCategoryNamesAsync(companyId, categoryIds) : new Dictionary<long, string>();

        foreach (var d in dtos)
        {
            if (d.ProductId.HasValue && productNames.TryGetValue(d.ProductId.Value, out var pn)) d.ProductName = pn;
            if (d.ServiceId.HasValue && serviceNames.TryGetValue(d.ServiceId.Value, out var sn)) d.ServiceName = sn;
            if (d.ProductCategoryId.HasValue && categoryNames.TryGetValue(d.ProductCategoryId.Value, out var cn)) d.ProductCategoryName = cn;
        }
    }
}

/* ---------------- Coupons (company) ---------------- */

public interface ICouponService
{
    Task<PaginatedResult<CouponDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<CouponDto> GetByIdAsync(long id);
    Task<string> GetNextCodeAsync(string prefix = "COUPON");
    Task<CouponDto> CreateAsync(long companyId, CreateCouponRequest request);
    Task<CouponDto> UpdateAsync(long id, long companyId, UpdateCouponRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class CouponService : ICouponService
{
    private readonly ICouponRepository _repository;
    private readonly IOfferRepository _offerRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public CouponService(ICouponRepository repository, IOfferRepository offerRepository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _offerRepository = offerRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<CouponDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search ?? string.Empty);
        var total = await _repository.CountAsync(companyId, search ?? string.Empty);
        var dtos = items.Select(ToDto).ToList();
        await EnrichAsync(companyId, dtos);
        return new PaginatedResult<CouponDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<CouponDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Coupon '{id}' was not found.");
        var dto = ToDto(entity);
        await EnrichAsync(entity.CompanyId, new List<CouponDto> { dto });
        return dto;
    }

    public async Task<string> GetNextCodeAsync(string prefix = "COUPON")
        => await _repository.GetNextCodeAsync(_currentUser.CompanyId, prefix);

    public async Task<CouponDto> CreateAsync(long companyId, CreateCouponRequest request)
    {
        await EnsureOfferAsync(companyId, request.OfferId);
        var code = string.IsNullOrWhiteSpace(request.Code)
            ? await _repository.GetNextCodeAsync(companyId)
            : request.Code.Trim().ToUpper();
        if (await _repository.CodeInUseAsync(companyId, code))
            throw new DomainException($"Coupon code '{code}' is already in use.");

        var entity = new Coupon
        {
            CompanyId = companyId,
            OfferId = request.OfferId,
            Code = code,
            Name = request.Name?.Trim(),
            UsageLimit = request.UsageLimit,
            UsagePerCustomer = request.UsagePerCustomer,
            UsedCount = 0,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate?.Date,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.CouponId = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("Coupon", entity.CouponId.ToString(), "Create", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<CouponDto> { dto });
        return dto;
    }

    public async Task<CouponDto> UpdateAsync(long id, long companyId, UpdateCouponRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Coupon '{id}' was not found.");
        await EnsureOfferAsync(companyId, request.OfferId);

        var code = string.IsNullOrWhiteSpace(request.Code) ? entity.Code : request.Code.Trim().ToUpper();
        if (code != entity.Code && await _repository.CodeInUseAsync(companyId, code, id))
            throw new DomainException($"Coupon code '{code}' is already in use.");

        entity.OfferId = request.OfferId;
        entity.Code = code;
        entity.Name = request.Name?.Trim();
        entity.UsageLimit = request.UsageLimit;
        entity.UsagePerCustomer = request.UsagePerCustomer;
        entity.StartDate = request.StartDate.Date;
        entity.EndDate = request.EndDate?.Date;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("Coupon", id.ToString(), "Update", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<CouponDto> { dto });
        return dto;
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Coupon '{id}' was not found.");
        await _repository.SoftDeleteAsync(id, companyId, _currentUser.UserId);
        await _auditService.WriteAsync("Coupon", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static CouponDto ToDto(Coupon e) => new()
    {
        CouponId = e.CouponId,
        CompanyId = e.CompanyId,
        OfferId = e.OfferId,
        Code = e.Code,
        Name = e.Name,
        UsageLimit = e.UsageLimit,
        UsagePerCustomer = e.UsagePerCustomer,
        UsedCount = e.UsedCount,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt
    };

    private async Task EnsureOfferAsync(long companyId, long offerId)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId)
            ?? throw new NotFoundException($"Offer '{offerId}' was not found.");
        if (offer.CompanyId != companyId)
            throw new DomainException("Offer does not belong to the current company.");
    }

    private async Task EnrichAsync(long companyId, List<CouponDto> dtos)
    {
        var offerIds = dtos.Select(d => d.OfferId).Distinct().ToList();
        var offerNames = await _offerRepository.GetOfferNamesAsync(companyId, offerIds);
        foreach (var d in dtos)
        {
            if (offerNames.TryGetValue(d.OfferId, out var on)) d.OfferName = on;
        }
    }
}