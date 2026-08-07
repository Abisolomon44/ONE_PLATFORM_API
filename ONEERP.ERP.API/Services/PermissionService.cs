using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface IPermissionModuleService
{
    Task<IEnumerable<PermissionModuleDto>> GetAllAsync(bool includeInactive = false);
    Task<PermissionModuleDto> GetByIdAsync(int id);
    Task<PermissionModuleDto> GetByCodeAsync(string code);
    Task<PermissionModuleDto> CreateAsync(CreatePermissionModuleRequest request);
    Task<PermissionModuleDto> UpdateAsync(int id, UpdatePermissionModuleRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IPermissionActionService
{
    Task<IEnumerable<PermissionActionDto>> GetAllAsync(bool includeInactive = false);
    Task<PermissionActionDto> GetByIdAsync(int id);
    Task<PermissionActionDto> GetByCodeAsync(string code);
    Task<PermissionActionDto> CreateAsync(CreatePermissionActionRequest request);
    Task<PermissionActionDto> UpdateAsync(int id, UpdatePermissionActionRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IModulePermissionService
{
    Task<IEnumerable<ModulePermissionDto>> GetByRoleAsync(int roleId);
    Task<IEnumerable<ModulePermissionDto>> GetByModuleAsync(int moduleId);
    Task<IEnumerable<UserModulePermissionDto>> GetUserPermissionsAsync(int userId);
    Task AssignAsync(AssignModulePermissionRequest request);
    Task RevokeAsync(RevokeModulePermissionRequest request);
    Task<bool> RevokeAllForRoleAsync(int roleId);
}

public interface IFieldPermissionService
{
    Task<IEnumerable<FieldPermissionDto>> GetByRoleModuleAsync(int roleId, int moduleId);
    Task<FieldPermissionDto> SetAsync(SetFieldPermissionRequest request);
    Task<bool> DeleteAsync(int id);
}

public class PermissionModuleService : IPermissionModuleService
{
    private readonly IPermissionModuleRepository _repository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public PermissionModuleService(
        IPermissionModuleRepository repository,
        IRoleRepository roleRepository,
        IAuditService auditService,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _roleRepository = roleRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<PermissionModuleDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(true);
        var dict = items.ToDictionary(m => m.Id, m => m);

        var list = new List<PermissionModuleDto>();
        foreach (var item in items)
        {
            string? parentName = null;
            if (item.ParentId.HasValue && dict.TryGetValue(item.ParentId.Value, out var parent))
                parentName = parent.Name;
            list.Add(Map(item, parentName));
        }

        return list;
    }

    public async Task<PermissionModuleDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Permission module '{id}' was not found.");
        return await MapWithParentAsync(item);
    }

    public async Task<PermissionModuleDto> GetByCodeAsync(string code)
    {
        var item = await _repository.GetByCodeAsync(code)
            ?? throw new NotFoundException($"Permission module '{code}' was not found.");
        return await MapWithParentAsync(item);
    }

    public async Task<PermissionModuleDto> CreateAsync(CreatePermissionModuleRequest request)
    {
        if (await _repository.GetByCodeAsync(request.Code) is not null)
            throw new DomainException($"A permission module with code '{request.Code}' already exists.");

        var entity = new PermissionModule
        {
            Code = request.Code,
            Name = request.Name,
            ParentId = request.ParentId,
            Level = request.Level,
            SortOrder = request.SortOrder,
            IsVisible = request.IsVisible,
            Icon = request.Icon,
            RoutePath = request.RoutePath,
            CreatedBy = _currentUser.Username
        };

        entity.Id = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("PermissionModule", entity.Id.ToString(), "Create", _currentUser.Username);
        return await MapWithParentAsync(entity);
    }

    public async Task<PermissionModuleDto> UpdateAsync(int id, UpdatePermissionModuleRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Permission module '{id}' was not found.");

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.ParentId = request.ParentId;
        entity.Level = request.Level;
        entity.SortOrder = request.SortOrder;
        entity.IsVisible = request.IsVisible;
        entity.Icon = request.Icon;
        entity.RoutePath = request.RoutePath;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("PermissionModule", id.ToString(), "Update", _currentUser.Username);
        return await MapWithParentAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Permission module '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("PermissionModule", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private async Task<PermissionModuleDto> MapWithParentAsync(PermissionModule entity)
    {
        string? parentName = null;
        if (entity.ParentId.HasValue)
        {
            var parent = await _repository.GetByIdAsync(entity.ParentId.Value);
            parentName = parent?.Name;
        }

        return Map(entity, parentName);
    }

    private static PermissionModuleDto Map(PermissionModule entity, string? parentName = null) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        ParentId = entity.ParentId,
        ParentName = parentName,
        Level = entity.Level,
        SortOrder = entity.SortOrder,
        IsVisible = entity.IsVisible,
        Icon = entity.Icon,
        RoutePath = entity.RoutePath,
        CreatedDate = entity.CreatedDate,
        ModifiedDate = entity.ModifiedDate
    };
}

public class PermissionActionService : IPermissionActionService
{
    private readonly IPermissionActionRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public PermissionActionService(
        IPermissionActionRepository repository,
        IAuditService auditService,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<PermissionActionDto>> GetAllAsync(bool includeInactive = false)
    {
        var items = await _repository.GetAllAsync(includeInactive);
        return items.Select(Map).ToList();
    }

    public async Task<PermissionActionDto> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Permission action '{id}' was not found.");
        return Map(item);
    }

    public async Task<PermissionActionDto> GetByCodeAsync(string code)
    {
        var item = await _repository.GetByCodeAsync(code)
            ?? throw new NotFoundException($"Permission action '{code}' was not found.");
        return Map(item);
    }

    public async Task<PermissionActionDto> CreateAsync(CreatePermissionActionRequest request)
    {
        if (await _repository.GetByCodeAsync(request.Code) is not null)
            throw new DomainException($"A permission action with code '{request.Code}' already exists.");

        var entity = new PermissionAction
        {
            Code = request.Code,
            Name = request.Name,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreatedBy = _currentUser.Username
        };

        entity.Id = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("PermissionAction", entity.Id.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<PermissionActionDto> UpdateAsync(int id, UpdatePermissionActionRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Permission action '{id}' was not found.");

        if (await _repository.GetByCodeAsync(request.Code) is { } existing && existing.Id != id)
            throw new DomainException($"A permission action with code '{request.Code}' already exists.");

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;
        entity.ModifiedBy = _currentUser.Username;

        await _repository.UpdateAsync(entity);
        await _auditService.WriteAsync("PermissionAction", id.ToString(), "Update", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Permission action '{id}' was not found.");

        await _repository.SoftDeleteAsync(id, _currentUser.Username);
        await _auditService.WriteAsync("PermissionAction", id.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    private static PermissionActionDto Map(PermissionAction entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        SortOrder = entity.SortOrder,
        IsActive = entity.IsActive
    };
}

public class ModulePermissionService : IModulePermissionService
{
    private readonly IModulePermissionRepository _repository;
    private readonly IPermissionModuleRepository _moduleRepository;
    private readonly IPermissionActionRepository _actionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public ModulePermissionService(
        IModulePermissionRepository repository,
        IPermissionModuleRepository moduleRepository,
        IPermissionActionRepository actionRepository,
        IRoleRepository roleRepository,
        IAuditService auditService,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _moduleRepository = moduleRepository;
        _actionRepository = actionRepository;
        _roleRepository = roleRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<ModulePermissionDto>> GetByRoleAsync(int roleId)
    {
        var permissions = await _repository.GetByRoleAsync(roleId);
        var roleNames = await _roleRepository.GetAllNamesAsync();
        var moduleNames = (await _moduleRepository.GetAllAsync(true)).ToDictionary(m => m.Id, m => m.Name);
        var actionNames = (await _actionRepository.GetAllAsync(true)).ToDictionary(a => a.Id, a => a.Name);

        return permissions.Select(p => Map(p, roleNames, moduleNames, actionNames)).ToList();
    }

    public async Task<IEnumerable<ModulePermissionDto>> GetByModuleAsync(int moduleId)
    {
        var permissions = await _repository.GetByModuleAsync(moduleId);
        var roleNames = await _roleRepository.GetAllNamesAsync();
        var moduleNames = (await _moduleRepository.GetAllAsync(true)).ToDictionary(m => m.Id, m => m.Name);
        var actionNames = (await _actionRepository.GetAllAsync(true)).ToDictionary(a => a.Id, a => a.Name);

        return permissions.Select(p => Map(p, roleNames, moduleNames, actionNames)).ToList();
    }

    public async Task AssignAsync(AssignModulePermissionRequest request)
    {
        await _repository.AssignAsync(
            request.RoleId, request.PermissionModuleId, request.PermissionActionId,
            request.Scope, request.ScopeId, _currentUser.Username);

        await _auditService.WriteAsync("ModulePermission", 
            $"{request.RoleId}:{request.PermissionModuleId}:{request.PermissionActionId}", "Assign", _currentUser.Username);
    }

    public async Task RevokeAsync(RevokeModulePermissionRequest request)
    {
        await _repository.RevokeAsync(
            request.RoleId, request.PermissionModuleId, request.PermissionActionId,
            request.Scope, request.ScopeId, _currentUser.Username);

        await _auditService.WriteAsync("ModulePermission",
            $"{request.RoleId}:{request.PermissionModuleId}:{request.PermissionActionId}", "Revoke", _currentUser.Username);
    }

    public async Task<bool> RevokeAllForRoleAsync(int roleId)
    {
        var result = await _repository.RevokeAllForRoleAsync(roleId);
        await _auditService.WriteAsync("ModulePermission", roleId.ToString(), "RevokeAll", _currentUser.Username);
        return result;
    }

    public async Task<IEnumerable<UserModulePermissionDto>> GetUserPermissionsAsync(int userId)
    {
        var permissions = await _repository.GetByUserAsync(userId);
        var moduleDict = (await _moduleRepository.GetAllAsync(true))
            .ToDictionary(m => m.Id, m => m.Code);
        var actionDict = (await _actionRepository.GetAllAsync(true))
            .ToDictionary(a => a.Id, a => a.Code);

        return permissions.Select(p => new UserModulePermissionDto
        {
            Code = moduleDict.TryGetValue(p.PermissionModuleId, out var mc) && actionDict.TryGetValue(p.PermissionActionId, out var ac)
                ? $"{mc}.{ac}"
                : "unknown.unknown",
            Scope = p.Scope,
            ScopeId = p.ScopeId
        }).ToList();
    }

    private static ModulePermissionDto Map(
        ModulePermission entity,
        Dictionary<int, string> roleNames,
        Dictionary<int, string> moduleNames,
        Dictionary<int, string> actionNames) => new()
    {
        Id = entity.Id,
        RoleId = entity.RoleId,
        RoleName = roleNames.TryGetValue(entity.RoleId, out var rn) ? rn : null,
        PermissionModuleId = entity.PermissionModuleId,
        ModuleName = moduleNames.TryGetValue(entity.PermissionModuleId, out var mn) ? mn : null,
        PermissionActionId = entity.PermissionActionId,
        ActionName = actionNames.TryGetValue(entity.PermissionActionId, out var an) ? an : null,
        Scope = entity.Scope,
        ScopeId = entity.ScopeId,
        GrantedBy = entity.GrantedBy,
        GrantedDate = entity.GrantedDate,
        IsRevoked = entity.IsRevoked
    };
}

public class FieldPermissionService : IFieldPermissionService
{
    private readonly IFieldPermissionRepository _repository;
    private readonly IPermissionModuleRepository _moduleRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public FieldPermissionService(
        IFieldPermissionRepository repository,
        IPermissionModuleRepository moduleRepository,
        IAuditService auditService,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _moduleRepository = moduleRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<FieldPermissionDto>> GetByRoleModuleAsync(int roleId, int moduleId)
    {
        var items = await _repository.GetByRoleModuleAsync(roleId, moduleId);
        return items.Select(Map).ToList();
    }

    public async Task<FieldPermissionDto> SetAsync(SetFieldPermissionRequest request)
    {
        var existing = await _repository.GetByRoleModuleFieldAsync(
            request.RoleId, request.PermissionModuleId, request.FieldName, request.Scope, request.ScopeId);

        if (existing is not null)
        {
            existing.CanView = request.CanView;
            existing.CanEdit = request.CanEdit;
            existing.IsMandatory = request.IsMandatory;
            existing.IsHidden = request.IsHidden;
            existing.ModifiedBy = _currentUser.Username;

            await _repository.UpdateAsync(existing);
            await _auditService.WriteAsync("FieldPermission", existing.Id.ToString(), "Update", _currentUser.Username);
            return Map(existing);
        }

        var entity = new FieldPermission
        {
            RoleId = request.RoleId,
            PermissionModuleId = request.PermissionModuleId,
            FieldName = request.FieldName,
            CanView = request.CanView,
            CanEdit = request.CanEdit,
            IsMandatory = request.IsMandatory,
            IsHidden = request.IsHidden,
            Scope = request.Scope,
            ScopeId = request.ScopeId,
            CreatedBy = _currentUser.Username
        };

        entity.Id = await _repository.InsertAsync(entity);
        await _auditService.WriteAsync("FieldPermission", entity.Id.ToString(), "Create", _currentUser.Username);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _repository.DeleteAsync(id);
        if (result)
            await _auditService.WriteAsync("FieldPermission", id.ToString(), "Delete", _currentUser.Username);
        return result;
    }

    private static FieldPermissionDto Map(FieldPermission entity) => new()
    {
        Id = entity.Id,
        RoleId = entity.RoleId,
        PermissionModuleId = entity.PermissionModuleId,
        FieldName = entity.FieldName,
        CanView = entity.CanView,
        CanEdit = entity.CanEdit,
        IsMandatory = entity.IsMandatory,
        IsHidden = entity.IsHidden,
        Scope = entity.Scope,
        ScopeId = entity.ScopeId,
        CreatedDate = entity.CreatedDate,
        ModifiedDate = entity.ModifiedDate
    };
}