using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

/* ---------------- Product Categories ---------------- */

public interface IProductCategoryService
{
    Task<PaginatedResult<ProductCategoryDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<ProductCategoryDto> GetByIdAsync(long id);
    Task<string> GetNextCodeAsync();
    Task<IEnumerable<ProductCategoryDto>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<ProductCategoryDto> CreateAsync(long companyId, CreateProductCategoryRequest request);
    Task<ProductCategoryDto> UpdateAsync(long id, long companyId, UpdateProductCategoryRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class ProductCategoryService : IProductCategoryService
{
    private readonly IProductCategoryRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public ProductCategoryService(IProductCategoryRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<ProductCategoryDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(companyId, search);
        return new PaginatedResult<ProductCategoryDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<ProductCategoryDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Product category '{id}' was not found.");
        return ToDto(entity);
    }

    public async Task<string> GetNextCodeAsync()
    {
        return await _repository.GetNextCodeAsync(_currentUser.CompanyId);
    }

    public async Task<IEnumerable<ProductCategoryDto>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(companyId, includeInactive);
        return items.Select(ToDto).ToList();
    }

    public async Task<ProductCategoryDto> CreateAsync(long companyId, CreateProductCategoryRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.CategoryCode)
            ? await _repository.GetNextCodeAsync(companyId)
            : request.CategoryCode.Trim().ToUpper();
        if (await _repository.CodeInUseAsync(companyId, code))
            throw new DomainException($"Category code '{code}' is already in use.");

        var entity = new ProductCategory
        {
            CompanyId = companyId,
            CategoryCode = code,
            CategoryName = request.CategoryName.Trim(),
            Description = request.Description?.Trim(),
            ParentCategoryId = request.ParentCategoryId,
            SortOrder = request.SortOrder,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.Id = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("ProductCategory", entity.Id.ToString(), "Create", _currentUser.Username);
        return ToDto(entity);
    }

    public async Task<ProductCategoryDto> UpdateAsync(long id, long companyId, UpdateProductCategoryRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Product category '{id}' was not found.");

        var code = request.CategoryCode.Trim();
        if (await _repository.CodeInUseAsync(companyId, code, id))
            throw new DomainException($"Category code '{code}' is already in use.");

        entity.CategoryCode = code;
        entity.CategoryName = request.CategoryName.Trim();
        entity.Description = request.Description?.Trim();
        entity.ParentCategoryId = request.ParentCategoryId;
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("ProductCategory", id.ToString(), "Update", _currentUser.Username);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Product category '{id}' was not found.");
        await _repository.SoftDeleteAsync(id, companyId, _currentUser.UserId);
        await _auditService.WriteAsync("ProductCategory", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static ProductCategoryDto ToDto(ProductCategory c) => new()
    {
        Id = c.Id,
        CompanyId = c.CompanyId,
        CategoryCode = c.CategoryCode,
        CategoryName = c.CategoryName,
        Description = c.Description,
        ParentCategoryId = c.ParentCategoryId,
        SortOrder = c.SortOrder,
        IsActive = c.IsActive
    };
}

/* ---------------- Product Sub Categories ---------------- */

public interface IProductSubCategoryService
{
    Task<PaginatedResult<ProductSubCategoryDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<ProductSubCategoryDto> GetByIdAsync(long id);
    Task<string> GetNextCodeAsync();
    Task<IEnumerable<ProductSubCategoryDto>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<ProductSubCategoryDto> CreateAsync(long companyId, CreateProductSubCategoryRequest request);
    Task<ProductSubCategoryDto> UpdateAsync(long id, long companyId, UpdateProductSubCategoryRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class ProductSubCategoryService : IProductSubCategoryService
{
    private readonly IProductSubCategoryRepository _repository;
    private readonly IProductCategoryRepository _categoryRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public ProductSubCategoryService(IProductSubCategoryRepository repository, IProductCategoryRepository categoryRepository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<ProductSubCategoryDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = (await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search)).ToList();
        var total = await _repository.CountAsync(companyId, search);

        var names = await _categoryRepository.GetNamesAsync(companyId, items.Select(i => i.CategoryId));
        var dtos = items.Select(i =>
        {
            var d = ToDto(i);
            d.CategoryName = names.TryGetValue(i.CategoryId, out var n) ? n : string.Empty;
            return d;
        }).ToList();

        return new PaginatedResult<ProductSubCategoryDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<ProductSubCategoryDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Product sub category '{id}' was not found.");
        var names = await _categoryRepository.GetNamesAsync(entity.CompanyId, new[] { entity.CategoryId });
        var dto = ToDto(entity);
        dto.CategoryName = names.TryGetValue(entity.CategoryId, out var n) ? n : string.Empty;
        return dto;
    }

    public async Task<string> GetNextCodeAsync()
    {
        return await _repository.GetNextCodeAsync(_currentUser.CompanyId);
    }

    public async Task<IEnumerable<ProductSubCategoryDto>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(companyId, includeInactive);
        return items.Select(ToDto).ToList();
    }

    public async Task<ProductSubCategoryDto> CreateAsync(long companyId, CreateProductSubCategoryRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.SubCategoryCode)
            ? await _repository.GetNextCodeAsync(companyId)
            : request.SubCategoryCode.Trim().ToUpper();
        if (await _repository.CodeInUseAsync(companyId, code))
            throw new DomainException($"Sub category code '{code}' is already in use.");

        var entity = new ProductSubCategory
        {
            CompanyId = companyId,
            CategoryId = request.CategoryId!.Value,
            SubCategoryCode = code,
            SubCategoryName = request.SubCategoryName.Trim(),
            Description = request.Description?.Trim(),
            SortOrder = request.SortOrder,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.Id = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("ProductSubCategory", entity.Id.ToString(), "Create", _currentUser.Username);
        var dto = ToDto(entity);
        var names = await _categoryRepository.GetNamesAsync(companyId, new[] { entity.CategoryId });
        dto.CategoryName = names.TryGetValue(entity.CategoryId, out var n) ? n : string.Empty;
        return dto;
    }

    public async Task<ProductSubCategoryDto> UpdateAsync(long id, long companyId, UpdateProductSubCategoryRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Product sub category '{id}' was not found.");

        var code = request.SubCategoryCode.Trim();
        if (await _repository.CodeInUseAsync(companyId, code, id))
            throw new DomainException($"Sub category code '{code}' is already in use.");

        entity.CategoryId = request.CategoryId!.Value;
        entity.SubCategoryCode = code;
        entity.SubCategoryName = request.SubCategoryName.Trim();
        entity.Description = request.Description?.Trim();
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("ProductSubCategory", id.ToString(), "Update", _currentUser.Username);
        var dto = ToDto(entity);
        var names = await _categoryRepository.GetNamesAsync(companyId, new[] { entity.CategoryId });
        dto.CategoryName = names.TryGetValue(entity.CategoryId, out var n) ? n : string.Empty;
        return dto;
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Product sub category '{id}' was not found.");
        await _repository.SoftDeleteAsync(id, companyId, _currentUser.UserId);
        await _auditService.WriteAsync("ProductSubCategory", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static ProductSubCategoryDto ToDto(ProductSubCategory s) => new()
    {
        Id = s.Id,
        CompanyId = s.CompanyId,
        CategoryId = s.CategoryId,
        SubCategoryCode = s.SubCategoryCode,
        SubCategoryName = s.SubCategoryName,
        Description = s.Description,
        SortOrder = s.SortOrder,
        IsActive = s.IsActive
    };
}

/* ---------------- Product Brands ---------------- */

public interface IProductBrandService
{
    Task<PaginatedResult<ProductBrandDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<ProductBrandDto> GetByIdAsync(long id);
    Task<string> GetNextCodeAsync();
    Task<IEnumerable<ProductBrandDto>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<ProductBrandDto> CreateAsync(long companyId, CreateProductBrandRequest request);
    Task<ProductBrandDto> UpdateAsync(long id, long companyId, UpdateProductBrandRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class ProductBrandService : IProductBrandService
{
    private readonly IProductBrandRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public ProductBrandService(IProductBrandRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<ProductBrandDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(companyId, search);
        return new PaginatedResult<ProductBrandDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<ProductBrandDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Brand '{id}' was not found.");
        return ToDto(entity);
    }

    public async Task<string> GetNextCodeAsync()
    {
        return await _repository.GetNextCodeAsync(_currentUser.CompanyId);
    }

    public async Task<IEnumerable<ProductBrandDto>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(companyId, includeInactive);
        return items.Select(ToDto).ToList();
    }

    public async Task<ProductBrandDto> CreateAsync(long companyId, CreateProductBrandRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.BrandCode)
            ? await _repository.GetNextCodeAsync(companyId)
            : request.BrandCode.Trim().ToUpper();
        if (await _repository.CodeInUseAsync(companyId, code))
            throw new DomainException($"Brand code '{code}' is already in use.");

        var entity = new ProductBrand
        {
            CompanyId = companyId,
            BrandCode = code,
            BrandName = request.BrandName.Trim(),
            Description = request.Description?.Trim(),
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.Id = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("ProductBrand", entity.Id.ToString(), "Create", _currentUser.Username);
        return ToDto(entity);
    }

    public async Task<ProductBrandDto> UpdateAsync(long id, long companyId, UpdateProductBrandRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Brand '{id}' was not found.");

        var code = request.BrandCode.Trim();
        if (await _repository.CodeInUseAsync(companyId, code, id))
            throw new DomainException($"Brand code '{code}' is already in use.");

        entity.BrandCode = code;
        entity.BrandName = request.BrandName.Trim();
        entity.Description = request.Description?.Trim();
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("ProductBrand", id.ToString(), "Update", _currentUser.Username);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Brand '{id}' was not found.");
        await _repository.SoftDeleteAsync(id, companyId, _currentUser.UserId);
        await _auditService.WriteAsync("ProductBrand", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static ProductBrandDto ToDto(ProductBrand b) => new()
    {
        Id = b.Id,
        CompanyId = b.CompanyId,
        BrandCode = b.BrandCode,
        BrandName = b.BrandName,
        Description = b.Description,
        IsActive = b.IsActive
    };
}

/* ---------------- Product Units ---------------- */

public interface IProductUnitService
{
    Task<PaginatedResult<ProductUnitDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<ProductUnitDto> GetByIdAsync(long id);
    Task<string> GetNextCodeAsync();
    Task<IEnumerable<ProductUnitDto>> GetAllAsync(long companyId, bool includeInactive = false);
    Task<ProductUnitDto> CreateAsync(long companyId, CreateProductUnitRequest request);
    Task<ProductUnitDto> UpdateAsync(long id, long companyId, UpdateProductUnitRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class ProductUnitService : IProductUnitService
{
    private readonly IProductUnitRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public ProductUnitService(IProductUnitRepository repository, IAuditService auditService, ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<ProductUnitDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(companyId, search);
        return new PaginatedResult<ProductUnitDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<ProductUnitDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Unit '{id}' was not found.");
        return ToDto(entity);
    }

    public async Task<string> GetNextCodeAsync()
    {
        return await _repository.GetNextCodeAsync(_currentUser.CompanyId);
    }

    public async Task<IEnumerable<ProductUnitDto>> GetAllAsync(long companyId, bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(companyId, includeInactive);
        return items.Select(ToDto).ToList();
    }

    public async Task<ProductUnitDto> CreateAsync(long companyId, CreateProductUnitRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.UnitCode)
            ? await _repository.GetNextCodeAsync(companyId)
            : request.UnitCode.Trim().ToUpper();
        if (await _repository.CodeInUseAsync(companyId, code))
            throw new DomainException($"Unit code '{code}' is already in use.");

        var entity = new ProductUnit
        {
            CompanyId = companyId,
            UnitCode = code,
            UnitName = request.UnitName.Trim(),
            Symbol = request.Symbol?.Trim(),
            DecimalPlaces = request.DecimalPlaces ?? 0,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.Id = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("ProductUnit", entity.Id.ToString(), "Create", _currentUser.Username);
        return ToDto(entity);
    }

    public async Task<ProductUnitDto> UpdateAsync(long id, long companyId, UpdateProductUnitRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Unit '{id}' was not found.");

        var code = request.UnitCode.Trim();
        if (await _repository.CodeInUseAsync(companyId, code, id))
            throw new DomainException($"Unit code '{code}' is already in use.");

        entity.UnitCode = code;
        entity.UnitName = request.UnitName.Trim();
        entity.Symbol = request.Symbol?.Trim();
        entity.DecimalPlaces = request.DecimalPlaces ?? 0;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("ProductUnit", id.ToString(), "Update", _currentUser.Username);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Unit '{id}' was not found.");
        await _repository.SoftDeleteAsync(id, companyId, _currentUser.UserId);
        await _auditService.WriteAsync("ProductUnit", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static ProductUnitDto ToDto(ProductUnit u) => new()
    {
        Id = u.Id,
        CompanyId = u.CompanyId,
        UnitCode = u.UnitCode,
        UnitName = u.UnitName,
        Symbol = u.Symbol,
        DecimalPlaces = u.DecimalPlaces,
        IsActive = u.IsActive
    };
}

/* ---------------- Products ---------------- */

public interface IProductService
{
    Task<PaginatedResult<ProductDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search);
    Task<ProductDto> GetByIdAsync(long id);
    Task<string> GetNextCodeAsync();
    Task<ProductDto> CreateAsync(long companyId, CreateProductRequest request);
    Task<ProductDto> UpdateAsync(long id, long companyId, UpdateProductRequest request);
    Task<bool> DeleteAsync(long id, long companyId);
}

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IProductCategoryRepository _categoryRepository;
    private readonly IProductSubCategoryRepository _subCategoryRepository;
    private readonly IProductBrandRepository _brandRepository;
    private readonly IProductUnitRepository _unitRepository;
    private readonly IHsnSacRepository _hsnSacRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public ProductService(
        IProductRepository repository,
        IProductCategoryRepository categoryRepository,
        IProductSubCategoryRepository subCategoryRepository,
        IProductBrandRepository brandRepository,
        IProductUnitRepository unitRepository,
        IHsnSacRepository hsnSacRepository,
        IAuditService auditService,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _subCategoryRepository = subCategoryRepository;
        _brandRepository = brandRepository;
        _unitRepository = unitRepository;
        _hsnSacRepository = hsnSacRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<ProductDto>> GetPagedAsync(long companyId, int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;
        var items = await _repository.GetPagedAsync(companyId, normalizedPage, normalizedSize, search);
        var total = await _repository.CountAsync(companyId, search);
        var dtos = items.Select(ToDto).ToList();
        await EnrichAsync(companyId, dtos);
        return new PaginatedResult<ProductDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<ProductDto> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Product '{id}' was not found.");
        var dto = ToDto(entity);
        await EnrichAsync(_currentUser.CompanyId, new List<ProductDto> { dto });
        return dto;
    }

    public async Task<string> GetNextCodeAsync()
        => await _repository.GetNextCodeAsync(_currentUser.CompanyId);

    public async Task<ProductDto> CreateAsync(long companyId, CreateProductRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.ProductCode)
            ? await _repository.GetNextCodeAsync(companyId)
            : request.ProductCode.Trim().ToUpper();
        if (await _repository.CodeInUseAsync(companyId, code))
            throw new DomainException($"Product code '{code}' is already in use.");

        var entity = new Product
        {
            CompanyId = companyId,
            BranchId = request.BranchId,
            ProductCode = code,
            ProductName = request.ProductName.Trim(),
            CategoryId = request.CategoryId,
            SubCategoryId = request.SubCategoryId,
            BrandId = request.BrandId,
            UOMId = request.UOMId,
            SKU = request.SKU?.Trim(),
            Barcode = request.Barcode?.Trim(),
            MRP = request.MRP,
            PurchasePrice = request.PurchasePrice,
            SalesPrice = request.SalesPrice,
            TaxId = request.TaxId,
            HsnSacId = request.HsnSacId,
            IsStockItem = request.IsStockItem,
            IsSaleable = request.IsSaleable,
            IsPurchaseable = request.IsPurchaseable,
            IsActive = true,
            Description = request.Description?.Trim(),
            EntityId = request.EntityId,
            CreatedBy = _currentUser.UserId,
            ModifiedBy = _currentUser.UserId
        };

        entity.Id = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("Product", entity.Id.ToString(), "Create", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<ProductDto> { dto });
        return dto;
    }

    public async Task<ProductDto> UpdateAsync(long id, long companyId, UpdateProductRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Product '{id}' was not found.");

        var code = string.IsNullOrWhiteSpace(request.ProductCode)
            ? entity.ProductCode
            : request.ProductCode.Trim().ToUpper();
        if (await _repository.CodeInUseAsync(companyId, code, id))
            throw new DomainException($"Product code '{code}' is already in use.");

        entity.BranchId = request.BranchId;
        entity.ProductCode = code;
        entity.ProductName = request.ProductName.Trim();
        entity.CategoryId = request.CategoryId;
        entity.SubCategoryId = request.SubCategoryId;
        entity.BrandId = request.BrandId;
        entity.UOMId = request.UOMId;
        entity.SKU = request.SKU?.Trim();
        entity.Barcode = request.Barcode?.Trim();
        entity.MRP = request.MRP;
        entity.PurchasePrice = request.PurchasePrice;
        entity.SalesPrice = request.SalesPrice;
        entity.TaxId = request.TaxId;
        entity.HsnSacId = request.HsnSacId;
        entity.IsStockItem = request.IsStockItem;
        entity.IsSaleable = request.IsSaleable;
        entity.IsPurchaseable = request.IsPurchaseable;
        entity.IsActive = request.IsActive;
        entity.Description = request.Description?.Trim();
        entity.EntityId = request.EntityId;
        entity.ModifiedBy = _currentUser.UserId;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("Product", id.ToString(), "Update", _currentUser.Username);
        var dto = ToDto(entity);
        await EnrichAsync(companyId, new List<ProductDto> { dto });
        return dto;
    }

    public async Task<bool> DeleteAsync(long id, long companyId)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Product '{id}' was not found.");
        var result = await _repository.SoftDeleteAsync(id, companyId, _currentUser.UserId);
        await _auditService.WriteAsync("Product", id.ToString(), "Delete", _currentUser.Username);
        return result;
    }

    private static ProductDto ToDto(Product e) => new()
    {
        Id = e.Id,
        EntityId = e.EntityId,
        CompanyId = e.CompanyId,
        BranchId = e.BranchId,
        ProductCode = e.ProductCode,
        ProductName = e.ProductName,
        CategoryId = e.CategoryId,
        SubCategoryId = e.SubCategoryId,
        BrandId = e.BrandId,
        UOMId = e.UOMId,
        SKU = e.SKU,
        Barcode = e.Barcode,
        MRP = e.MRP,
        PurchasePrice = e.PurchasePrice,
        SalesPrice = e.SalesPrice,
        TaxId = e.TaxId,
        HsnSacId = e.HsnSacId,
        IsStockItem = e.IsStockItem,
        IsSaleable = e.IsSaleable,
        IsPurchaseable = e.IsPurchaseable,
        IsActive = e.IsActive,
        Description = e.Description,
        CreatedAt = e.CreatedAt,
        ModifiedAt = e.ModifiedAt
    };

    private async Task EnrichAsync(long companyId, List<ProductDto> dtos)
    {
        var catIds = dtos.Where(d => d.CategoryId.HasValue).Select(d => d.CategoryId!.Value).Distinct().ToList();
        var subIds = dtos.Where(d => d.SubCategoryId.HasValue).Select(d => d.SubCategoryId!.Value).Distinct().ToList();
        var brandIds = dtos.Where(d => d.BrandId.HasValue).Select(d => d.BrandId!.Value).Distinct().ToList();
        var uomIds = dtos.Where(d => d.UOMId > 0).Select(d => d.UOMId).Distinct().ToList();
        var hsnIds = dtos.Where(d => d.HsnSacId.HasValue).Select(d => d.HsnSacId!.Value).Distinct().ToList();

        var catNames = catIds.Count > 0 ? await _categoryRepository.GetNamesAsync(companyId, catIds) : new Dictionary<long, string>();
        var subNames = subIds.Count > 0 ? await _subCategoryRepository.GetNamesAsync(companyId, subIds) : new Dictionary<long, string>();
        var brandNames = brandIds.Count > 0 ? await _brandRepository.GetNamesAsync(companyId, brandIds) : new Dictionary<long, string>();
        var uomNames = uomIds.Count > 0 ? await _unitRepository.GetNamesAsync(companyId, uomIds) : new Dictionary<long, string>();
        var hsnNames = hsnIds.Count > 0 ? await _hsnSacRepository.GetNamesAsync(companyId, hsnIds) : new Dictionary<long, string>();

        foreach (var d in dtos)
        {
            if (d.CategoryId.HasValue && catNames.TryGetValue(d.CategoryId.Value, out var cn)) d.CategoryName = cn;
            if (d.SubCategoryId.HasValue && subNames.TryGetValue(d.SubCategoryId.Value, out var sn)) d.SubCategoryName = sn;
            if (d.BrandId.HasValue && brandNames.TryGetValue(d.BrandId.Value, out var bn)) d.BrandName = bn;
            if (uomNames.TryGetValue(d.UOMId, out var un)) d.UOMName = un;
            if (d.HsnSacId.HasValue && hsnNames.TryGetValue(d.HsnSacId.Value, out var hn)) d.HsnSacCode = hn;
        }
    }
}
