using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

/* ---------------------------------------------------------------------------
   Workspace Service
   --------------------------------------------------------------------------- */
public interface IWorkspaceService
{
    Task<IEnumerable<WorkspaceDto>> GetAllAsync();
    Task<WorkspaceDto> GetByIdAsync(int id);
    Task<WorkspaceDto> CreateAsync(CreateWorkspaceRequest request);
    Task<WorkspaceDto> UpdateAsync(int id, UpdateWorkspaceRequest request);
    Task<bool> DeleteAsync(int id);
}

public class WorkspaceService : IWorkspaceService
{
    private readonly IWorkspaceRepository _repo;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public WorkspaceService(IWorkspaceRepository repo, IAuditService audit, ICurrentUser user)
    { _repo = repo; _audit = audit; _user = user; }

    public async Task<IEnumerable<WorkspaceDto>> GetAllAsync()
        => (await _repo.GetAllAsync()).Select(Map).ToList();

    public async Task<WorkspaceDto> GetByIdAsync(int id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Workspace '{id}' not found.");
        return Map(e);
    }

    public async Task<WorkspaceDto> CreateAsync(CreateWorkspaceRequest r)
    {
        var e = new Workspace
        {
            WorkspaceCode = r.WorkspaceCode, WorkspaceName = r.WorkspaceName,
            Icon = r.Icon, Route = r.Route, SortOrder = r.SortOrder,
            IsActive = r.IsActive, CreatedBy = _user.Username
        };
        e.Id = await _repo.InsertAsync(e);
        await _audit.WriteAsync("Workspace", e.Id.ToString(), "Create", _user.Username);
        return Map(e);
    }

    public async Task<WorkspaceDto> UpdateAsync(int id, UpdateWorkspaceRequest r)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Workspace '{id}' not found.");
        e.WorkspaceCode = r.WorkspaceCode;
        e.WorkspaceName = r.WorkspaceName;
        e.Icon = r.Icon;
        e.Route = r.Route;
        e.SortOrder = r.SortOrder;
        e.IsActive = r.IsActive;
        await _repo.UpdateAsync(e);
        await _audit.WriteAsync("Workspace", id.ToString(), "Update", _user.Username);
        return Map(e);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Workspace '{id}' not found.");
        var r = await _repo.DeleteAsync(id);
        if (r) await _audit.WriteAsync("Workspace", id.ToString(), "Delete", _user.Username);
        return r;
    }

    private static WorkspaceDto Map(Workspace e) => new()
    {
        Id = e.Id, WorkspaceCode = e.WorkspaceCode, WorkspaceName = e.WorkspaceName,
        Icon = e.Icon, Route = e.Route, SortOrder = e.SortOrder,
        IsActive = e.IsActive, CreatedDate = e.CreatedDate
    };
}

/* ---------------------------------------------------------------------------
   Domain Service
   --------------------------------------------------------------------------- */
public interface IDomainService
{
    Task<IEnumerable<DomainDto>> GetAllAsync();
    Task<IEnumerable<DomainDto>> GetByWorkspaceAsync(int workspaceId);
    Task<DomainDto> GetByIdAsync(int id);
    Task<DomainDto> CreateAsync(CreateDomainRequest request);
    Task<DomainDto> UpdateAsync(int id, UpdateDomainRequest request);
    Task<bool> DeleteAsync(int id);
}

public class DomainService : IDomainService
{
    private readonly IDomainRepository _repo;
    private readonly IWorkspaceRepository _wsRepo;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public DomainService(IDomainRepository repo, IWorkspaceRepository wsRepo, IAuditService audit, ICurrentUser user)
    { _repo = repo; _wsRepo = wsRepo; _audit = audit; _user = user; }

    public async Task<IEnumerable<DomainDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        var wsNames = (await _wsRepo.GetAllAsync(true)).ToDictionary(w => w.Id, w => w.WorkspaceName);
        return items.Select(e => Map(e, wsNames.TryGetValue(e.WorkspaceId, out var wn) ? wn : null)).ToList();
    }

    public async Task<IEnumerable<DomainDto>> GetByWorkspaceAsync(int workspaceId)
    {
        var items = await _repo.GetByWorkspaceAsync(workspaceId);
        var ws = await _wsRepo.GetByIdAsync(workspaceId);
        return items.Select(e => Map(e, ws?.WorkspaceName)).ToList();
    }

    public async Task<DomainDto> GetByIdAsync(int id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Domain '{id}' not found.");
        var ws = await _wsRepo.GetByIdAsync(e.WorkspaceId);
        return Map(e, ws?.WorkspaceName);
    }

    public async Task<DomainDto> CreateAsync(CreateDomainRequest r)
    {
        var e = new Domain
        {
            WorkspaceId = r.WorkspaceId, DomainCode = r.DomainCode, DomainName = r.DomainName,
            Icon = r.Icon, SortOrder = r.SortOrder, IsActive = r.IsActive, CreatedBy = _user.Username
        };
        e.Id = await _repo.InsertAsync(e);
        await _audit.WriteAsync("Domain", e.Id.ToString(), "Create", _user.Username);
        var ws = await _wsRepo.GetByIdAsync(e.WorkspaceId);
        return Map(e, ws?.WorkspaceName);
    }

    public async Task<DomainDto> UpdateAsync(int id, UpdateDomainRequest r)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Domain '{id}' not found.");
        e.DomainCode = r.DomainCode;
        e.DomainName = r.DomainName;
        e.Icon = r.Icon;
        e.SortOrder = r.SortOrder;
        e.IsActive = r.IsActive;
        await _repo.UpdateAsync(e);
        await _audit.WriteAsync("Domain", id.ToString(), "Update", _user.Username);
        var ws = await _wsRepo.GetByIdAsync(e.WorkspaceId);
        return Map(e, ws?.WorkspaceName);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Domain '{id}' not found.");
        var r = await _repo.DeleteAsync(id);
        if (r) await _audit.WriteAsync("Domain", id.ToString(), "Delete", _user.Username);
        return r;
    }

    private static DomainDto Map(Domain e, string? wsName = null) => new()
    {
        Id = e.Id, WorkspaceId = e.WorkspaceId, WorkspaceName = wsName,
        DomainCode = e.DomainCode, DomainName = e.DomainName, Icon = e.Icon,
        SortOrder = e.SortOrder, IsActive = e.IsActive, CreatedDate = e.CreatedDate
    };
}

/* ---------------------------------------------------------------------------
   Module Service
   --------------------------------------------------------------------------- */
public interface IModuleService
{
    Task<IEnumerable<ModuleDto>> GetAllAsync();
    Task<IEnumerable<ModuleDto>> GetByDomainAsync(int domainId);
    Task<ModuleDto> GetByIdAsync(int id);
    Task<ModuleDto> CreateAsync(CreateModuleRequest request);
    Task<ModuleDto> UpdateAsync(int id, UpdateModuleRequest request);
    Task<bool> DeleteAsync(int id);
}

public class ModuleService : IModuleService
{
    private readonly IModuleRepository _repo;
    private readonly IDomainRepository _domRepo;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public ModuleService(IModuleRepository repo, IDomainRepository domRepo, IAuditService audit, ICurrentUser user)
    { _repo = repo; _domRepo = domRepo; _audit = audit; _user = user; }

    public async Task<IEnumerable<ModuleDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        var domNames = (await _domRepo.GetAllAsync(true)).ToDictionary(d => d.Id, d => d.DomainName);
        return items.Select(e => Map(e, domNames.TryGetValue(e.DomainId, out var dn) ? dn : null)).ToList();
    }

    public async Task<IEnumerable<ModuleDto>> GetByDomainAsync(int domainId)
    {
        var items = await _repo.GetByDomainAsync(domainId);
        var dom = await _domRepo.GetByIdAsync(domainId);
        return items.Select(e => Map(e, dom?.DomainName)).ToList();
    }

    public async Task<ModuleDto> GetByIdAsync(int id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Module '{id}' not found.");
        var dom = await _domRepo.GetByIdAsync(e.DomainId);
        return Map(e, dom?.DomainName);
    }

    public async Task<ModuleDto> CreateAsync(CreateModuleRequest r)
    {
        var e = new Module
        {
            DomainId = r.DomainId, ModuleCode = r.ModuleCode, ModuleName = r.ModuleName,
            Icon = r.Icon, RouteUrl = r.RouteUrl, SortOrder = r.SortOrder,
            IsActive = r.IsActive, CreatedBy = _user.Username
        };
        e.Id = await _repo.InsertAsync(e);
        await _audit.WriteAsync("Module", e.Id.ToString(), "Create", _user.Username);
        var dom = await _domRepo.GetByIdAsync(e.DomainId);
        return Map(e, dom?.DomainName);
    }

    public async Task<ModuleDto> UpdateAsync(int id, UpdateModuleRequest r)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Module '{id}' not found.");
        e.ModuleCode = r.ModuleCode;
        e.ModuleName = r.ModuleName;
        e.Icon = r.Icon;
        e.RouteUrl = r.RouteUrl;
        e.SortOrder = r.SortOrder;
        e.IsActive = r.IsActive;
        await _repo.UpdateAsync(e);
        await _audit.WriteAsync("Module", id.ToString(), "Update", _user.Username);
        var dom = await _domRepo.GetByIdAsync(e.DomainId);
        return Map(e, dom?.DomainName);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Module '{id}' not found.");
        var r = await _repo.DeleteAsync(id);
        if (r) await _audit.WriteAsync("Module", id.ToString(), "Delete", _user.Username);
        return r;
    }

    private static ModuleDto Map(Module e, string? domName = null) => new()
    {
        Id = e.Id, DomainId = e.DomainId, DomainName = domName,
        ModuleCode = e.ModuleCode, ModuleName = e.ModuleName, Icon = e.Icon,
        RouteUrl = e.RouteUrl, SortOrder = e.SortOrder, IsActive = e.IsActive,
        CreatedDate = e.CreatedDate
    };
}

/* ---------------------------------------------------------------------------
   SubModule Service
   --------------------------------------------------------------------------- */
public interface ISubModuleService
{
    Task<IEnumerable<SubModuleDto>> GetAllAsync();
    Task<IEnumerable<SubModuleDto>> GetByModuleAsync(int moduleId);
    Task<SubModuleDto> GetByIdAsync(int id);
    Task<SubModuleDto> CreateAsync(CreateSubModuleRequest request);
    Task<SubModuleDto> UpdateAsync(int id, UpdateSubModuleRequest request);
    Task<bool> DeleteAsync(int id);
}

public class SubModuleService : ISubModuleService
{
    private readonly ISubModuleRepository _repo;
    private readonly IModuleRepository _modRepo;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public SubModuleService(ISubModuleRepository repo, IModuleRepository modRepo, IAuditService audit, ICurrentUser user)
    { _repo = repo; _modRepo = modRepo; _audit = audit; _user = user; }

    public async Task<IEnumerable<SubModuleDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        var modNames = (await _modRepo.GetAllAsync(true)).ToDictionary(m => m.Id, m => m.ModuleName);
        return items.Select(e => Map(e, modNames.TryGetValue(e.ModuleId, out var mn) ? mn : null)).ToList();
    }

    public async Task<IEnumerable<SubModuleDto>> GetByModuleAsync(int moduleId)
    {
        var items = await _repo.GetByModuleAsync(moduleId);
        var mod = await _modRepo.GetByIdAsync(moduleId);
        return items.Select(e => Map(e, mod?.ModuleName)).ToList();
    }

    public async Task<SubModuleDto> GetByIdAsync(int id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"SubModule '{id}' not found.");
        var mod = await _modRepo.GetByIdAsync(e.ModuleId);
        return Map(e, mod?.ModuleName);
    }

    public async Task<SubModuleDto> CreateAsync(CreateSubModuleRequest r)
    {
        var e = new SubModule
        {
            ModuleId = r.ModuleId, SubModuleCode = r.SubModuleCode, SubModuleName = r.SubModuleName,
            Icon = r.Icon, RouteUrl = r.RouteUrl, SortOrder = r.SortOrder,
            IsActive = r.IsActive, CreatedBy = _user.Username
        };
        e.Id = await _repo.InsertAsync(e);
        await _audit.WriteAsync("SubModule", e.Id.ToString(), "Create", _user.Username);
        var mod = await _modRepo.GetByIdAsync(e.ModuleId);
        return Map(e, mod?.ModuleName);
    }

    public async Task<SubModuleDto> UpdateAsync(int id, UpdateSubModuleRequest r)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"SubModule '{id}' not found.");
        e.SubModuleCode = r.SubModuleCode;
        e.SubModuleName = r.SubModuleName;
        e.Icon = r.Icon;
        e.RouteUrl = r.RouteUrl;
        e.SortOrder = r.SortOrder;
        e.IsActive = r.IsActive;
        e.ModifiedBy = _user.Username;
        await _repo.UpdateAsync(e);
        await _audit.WriteAsync("SubModule", id.ToString(), "Update", _user.Username);
        var mod = await _modRepo.GetByIdAsync(e.ModuleId);
        return Map(e, mod?.ModuleName);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"SubModule '{id}' not found.");
        var r = await _repo.DeleteAsync(id);
        if (r) await _audit.WriteAsync("SubModule", id.ToString(), "Delete", _user.Username);
        return r;
    }

    private static SubModuleDto Map(SubModule e, string? modName = null) => new()
    {
        Id = e.Id, ModuleId = e.ModuleId, ModuleName = modName,
        SubModuleCode = e.SubModuleCode, SubModuleName = e.SubModuleName, Icon = e.Icon,
        RouteUrl = e.RouteUrl, SortOrder = e.SortOrder, IsActive = e.IsActive,
        CreatedDate = e.CreatedDate
    };
}

/* ---------------------------------------------------------------------------
   Screen Service
   --------------------------------------------------------------------------- */
public interface IScreenService
{
    Task<IEnumerable<ScreenDto>> GetAllAsync();
    Task<IEnumerable<ScreenDto>> GetBySubModuleAsync(int subModuleId);
    Task<ScreenDto> GetByIdAsync(int id);
    Task<ScreenDto> CreateAsync(CreateScreenRequest request);
    Task<ScreenDto> UpdateAsync(int id, UpdateScreenRequest request);
    Task<bool> DeleteAsync(int id);
}

public class ScreenService : IScreenService
{
    private readonly IScreenRepository _repo;
    private readonly ISubModuleRepository _subModRepo;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public ScreenService(IScreenRepository repo, ISubModuleRepository subModRepo, IAuditService audit, ICurrentUser user)
    { _repo = repo; _subModRepo = subModRepo; _audit = audit; _user = user; }

    public async Task<IEnumerable<ScreenDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        var subModNames = (await _subModRepo.GetAllAsync(true)).ToDictionary(s => s.Id, s => s.SubModuleName);
        return items.Select(e => Map(e, subModNames.TryGetValue(e.SubModuleId, out var sn) ? sn : null)).ToList();
    }

    public async Task<IEnumerable<ScreenDto>> GetBySubModuleAsync(int subModuleId)
    {
        var items = await _repo.GetBySubModuleAsync(subModuleId);
        var subMod = await _subModRepo.GetByIdAsync(subModuleId);
        return items.Select(e => Map(e, subMod?.SubModuleName)).ToList();
    }

    public async Task<ScreenDto> GetByIdAsync(int id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Screen '{id}' not found.");
        var subMod = await _subModRepo.GetByIdAsync(e.SubModuleId);
        return Map(e, subMod?.SubModuleName);
    }

    public async Task<ScreenDto> CreateAsync(CreateScreenRequest r)
    {
        var e = new Screen
        {
            SubModuleId = r.SubModuleId, ScreenCode = r.ScreenCode, ScreenName = r.ScreenName,
            PermissionCode = r.PermissionCode, ScreenType = r.ScreenType, RouteUrl = r.RouteUrl,
            ComponentName = r.ComponentName, SortOrder = r.SortOrder, IsActive = r.IsActive, CreatedBy = _user.Username
        };
        e.Id = await _repo.InsertAsync(e);
        await _audit.WriteAsync("Screen", e.Id.ToString(), "Create", _user.Username);
        var subMod = await _subModRepo.GetByIdAsync(e.SubModuleId);
        return Map(e, subMod?.SubModuleName);
    }

    public async Task<ScreenDto> UpdateAsync(int id, UpdateScreenRequest r)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Screen '{id}' not found.");
        e.ScreenCode = r.ScreenCode;
        e.ScreenName = r.ScreenName;
        e.PermissionCode = r.PermissionCode;
        e.ScreenType = r.ScreenType;
        e.RouteUrl = r.RouteUrl;
        e.ComponentName = r.ComponentName;
        e.SortOrder = r.SortOrder;
        e.IsActive = r.IsActive;
        await _repo.UpdateAsync(e);
        await _audit.WriteAsync("Screen", id.ToString(), "Update", _user.Username);
        var subMod = await _subModRepo.GetByIdAsync(e.SubModuleId);
        return Map(e, subMod?.SubModuleName);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Screen '{id}' not found.");
        var r = await _repo.DeleteAsync(id);
        if (r) await _audit.WriteAsync("Screen", id.ToString(), "Delete", _user.Username);
        return r;
    }

    private static ScreenDto Map(Screen e, string? subModName = null) => new()
    {
        Id = e.Id, SubModuleId = e.SubModuleId, SubModuleName = subModName,
        ScreenCode = e.ScreenCode, ScreenName = e.ScreenName, PermissionCode = e.PermissionCode, ScreenType = e.ScreenType,
        RouteUrl = e.RouteUrl, ComponentName = e.ComponentName, SortOrder = e.SortOrder,
        IsActive = e.IsActive, CreatedDate = e.CreatedDate
    };
}

/* ---------------------------------------------------------------------------
   Field Service
   --------------------------------------------------------------------------- */
public interface IFieldService
{
    Task<IEnumerable<FieldDto>> GetAllAsync();
    Task<IEnumerable<FieldDto>> GetByScreenAsync(int screenId);
    Task<FieldDto> GetByIdAsync(int id);
    Task<FieldDto> CreateAsync(CreateFieldRequest request);
    Task<FieldDto> UpdateAsync(int id, UpdateFieldRequest request);
    Task<bool> DeleteAsync(int id);
}

public class FieldService : IFieldService
{
    private readonly IFieldRepository _repo;
    private readonly IScreenRepository _scrRepo;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public FieldService(IFieldRepository repo, IScreenRepository scrRepo, IAuditService audit, ICurrentUser user)
    { _repo = repo; _scrRepo = scrRepo; _audit = audit; _user = user; }

    public async Task<IEnumerable<FieldDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        var scrNames = (await _scrRepo.GetAllAsync(true)).ToDictionary(s => s.Id, s => s.ScreenName);
        return items.Select(e => Map(e, scrNames.TryGetValue(e.ScreenId, out var sn) ? sn : null)).ToList();
    }

    public async Task<IEnumerable<FieldDto>> GetByScreenAsync(int screenId)
    {
        var items = await _repo.GetByScreenAsync(screenId);
        var scr = await _scrRepo.GetByIdAsync(screenId);
        return items.Select(e => Map(e, scr?.ScreenName)).ToList();
    }

    public async Task<FieldDto> GetByIdAsync(int id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Field '{id}' not found.");
        var scr = await _scrRepo.GetByIdAsync(e.ScreenId);
        return Map(e, scr?.ScreenName);
    }

    public async Task<FieldDto> CreateAsync(CreateFieldRequest r)
    {
        var e = new Field
        {
            ScreenId = r.ScreenId, FieldCode = r.FieldCode, FieldName = r.FieldName,
            DisplayName = r.DisplayName, DataType = r.DataType, DisplayOrder = r.DisplayOrder,
            DefaultValue = r.DefaultValue, IsSystemField = r.IsSystemField, IsRequired = r.IsRequired,
            IsActive = r.IsActive, CreatedBy = _user.Username
        };
        e.Id = await _repo.InsertAsync(e);
        await _audit.WriteAsync("Field", e.Id.ToString(), "Create", _user.Username);
        var scr = await _scrRepo.GetByIdAsync(e.ScreenId);
        return Map(e, scr?.ScreenName);
    }

    public async Task<FieldDto> UpdateAsync(int id, UpdateFieldRequest r)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Field '{id}' not found.");
        e.FieldCode = r.FieldCode;
        e.FieldName = r.FieldName;
        e.DisplayName = r.DisplayName;
        e.DataType = r.DataType;
        e.DisplayOrder = r.DisplayOrder;
        e.DefaultValue = r.DefaultValue;
        e.IsRequired = r.IsRequired;
        e.IsActive = r.IsActive;
        await _repo.UpdateAsync(e);
        await _audit.WriteAsync("Field", id.ToString(), "Update", _user.Username);
        var scr = await _scrRepo.GetByIdAsync(e.ScreenId);
        return Map(e, scr?.ScreenName);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Field '{id}' not found.");
        var r = await _repo.DeleteAsync(id);
        if (r) await _audit.WriteAsync("Field", id.ToString(), "Delete", _user.Username);
        return r;
    }

    private static FieldDto Map(Field e, string? scrName = null) => new()
    {
        Id = e.Id, ScreenId = e.ScreenId, ScreenName = scrName,
        FieldCode = e.FieldCode, FieldName = e.FieldName, DisplayName = e.DisplayName,
        DataType = e.DataType, DisplayOrder = e.DisplayOrder, DefaultValue = e.DefaultValue,
        IsSystemField = e.IsSystemField, IsRequired = e.IsRequired, IsActive = e.IsActive,
        CreatedDate = e.CreatedDate
    };
}

/* ---------------------------------------------------------------------------
   Action Service (PermissionActionEntry)
   --------------------------------------------------------------------------- */
public interface IActionService
{
    Task<IEnumerable<ActionDto>> GetAllAsync();
    Task<ActionDto> GetByIdAsync(int id);
    Task<ActionDto> CreateAsync(CreateActionRequest request);
    Task<ActionDto> UpdateAsync(int id, UpdateActionRequest request);
    Task<bool> DeleteAsync(int id);
}

public class ActionService : IActionService
{
    private readonly IActionRepository _repo;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public ActionService(IActionRepository repo, IAuditService audit, ICurrentUser user)
    { _repo = repo; _audit = audit; _user = user; }

    public async Task<IEnumerable<ActionDto>> GetAllAsync()
        => (await _repo.GetAllAsync()).Select(Map).ToList();

    public async Task<ActionDto> GetByIdAsync(int id)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Action '{id}' not found.");
        return Map(e);
    }

    public async Task<ActionDto> CreateAsync(CreateActionRequest r)
    {
        if (await _repo.GetByCodeAsync(r.ActionCode) is not null)
            throw new DomainException($"Action '{r.ActionCode}' already exists.");
        var e = new PermissionActionEntry
        {
            ActionCode = r.ActionCode, ActionName = r.ActionName,
            DisplayOrder = r.DisplayOrder, IsActive = r.IsActive
        };
        e.Id = await _repo.InsertAsync(e);
        await _audit.WriteAsync("Action", e.Id.ToString(), "Create", _user.Username);
        return Map(e);
    }

    public async Task<ActionDto> UpdateAsync(int id, UpdateActionRequest r)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Action '{id}' not found.");
        e.ActionCode = r.ActionCode;
        e.ActionName = r.ActionName;
        e.DisplayOrder = r.DisplayOrder;
        e.IsActive = r.IsActive;
        await _repo.UpdateAsync(e);
        await _audit.WriteAsync("Action", id.ToString(), "Update", _user.Username);
        return Map(e);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Action '{id}' not found.");
        var r = await _repo.DeleteAsync(id);
        if (r) await _audit.WriteAsync("Action", id.ToString(), "Delete", _user.Username);
        return r;
    }

    private static ActionDto Map(PermissionActionEntry e) => new()
    {
        Id = e.Id, ActionCode = e.ActionCode, ActionName = e.ActionName,
        DisplayOrder = e.DisplayOrder, IsActive = e.IsActive
    };
}

/* ---------------------------------------------------------------------------
   RolePermissionEntry Service (hierarchical)
   --------------------------------------------------------------------------- */
public interface IRolePermissionEntryService
{
    Task<IEnumerable<RolePermissionEntryDto>> GetByRoleAsync(int roleId);
    Task<RolePermissionEntryDto> AssignAsync(AssignRolePermissionRequest request);
    Task<bool> BulkAssignAsync(BulkAssignRolePermissionRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAllForRoleAsync(int roleId);
}

public class RolePermissionEntryService : IRolePermissionEntryService
{
    private readonly IRolePermissionEntryRepository _repo;
    private readonly IRoleRepository _roleRepo;
    private readonly IWorkspaceRepository _wsRepo;
    private readonly IDomainRepository _domRepo;
    private readonly IModuleRepository _modRepo;
    private readonly ISubModuleRepository _subModRepo;
    private readonly IScreenRepository _scrRepo;
    private readonly IActionRepository _actRepo;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public RolePermissionEntryService(
        IRolePermissionEntryRepository repo, IRoleRepository roleRepo,
        IWorkspaceRepository wsRepo, IDomainRepository domRepo, IModuleRepository modRepo,
        ISubModuleRepository subModRepo, IScreenRepository scrRepo, IActionRepository actRepo, IAuditService audit, ICurrentUser user)
    { _repo = repo; _roleRepo = roleRepo; _wsRepo = wsRepo; _domRepo = domRepo; _modRepo = modRepo; _subModRepo = subModRepo; _scrRepo = scrRepo; _actRepo = actRepo; _audit = audit; _user = user; }

    public async Task<IEnumerable<RolePermissionEntryDto>> GetByRoleAsync(int roleId)
    {
        var items = await _repo.GetByRoleAsync(roleId);
        var roleNames = await _roleRepo.GetAllNamesAsync();
        var wsNames = (await _wsRepo.GetAllAsync(true)).ToDictionary(w => w.Id, w => w.WorkspaceName);
        var domNames = (await _domRepo.GetAllAsync(true)).ToDictionary(d => d.Id, d => d.DomainName);
        var modNames = (await _modRepo.GetAllAsync(true)).ToDictionary(m => m.Id, m => m.ModuleName);
        var subModNames = (await _subModRepo.GetAllAsync(true)).ToDictionary(s => s.Id, s => s.SubModuleName);
        var scrNames = (await _scrRepo.GetAllAsync(true)).ToDictionary(s => s.Id, s => s.ScreenName);
        var actNames = (await _actRepo.GetAllAsync(true)).ToDictionary(a => a.Id, a => a.ActionName);

        return items.Select(e => new RolePermissionEntryDto
        {
            Id = e.Id,
            RoleId = e.RoleId,
            RoleName = roleNames.TryGetValue(e.RoleId, out var rn) ? rn : null,
            WorkspaceId = e.WorkspaceId,
            WorkspaceName = wsNames.TryGetValue(e.WorkspaceId, out var wn) ? wn : null,
            DomainId = e.DomainId,
            DomainName = domNames.TryGetValue(e.DomainId, out var dn) ? dn : null,
            ModuleId = e.ModuleId,
            ModuleName = modNames.TryGetValue(e.ModuleId, out var mn) ? mn : null,
            SubModuleId = e.SubModuleId,
            SubModuleName = subModNames.TryGetValue(e.SubModuleId, out var smn) ? smn : null,
            ScreenId = e.ScreenId,
            ScreenName = scrNames.TryGetValue(e.ScreenId, out var sn) ? sn : null,
            ActionId = e.ActionId,
            ActionName = actNames.TryGetValue(e.ActionId, out var an) ? an : null,
            Allow = e.Allow,
            DisplayOrder = e.DisplayOrder,
            IsActive = e.IsActive,
            CreatedDate = e.CreatedDate
        }).ToList();
    }

    public async Task<RolePermissionEntryDto> AssignAsync(AssignRolePermissionRequest r)
    {
        var e = new RolePermissionEntry
        {
            RoleId = r.RoleId, WorkspaceId = r.WorkspaceId, DomainId = r.DomainId,
            ModuleId = r.ModuleId, SubModuleId = r.SubModuleId, ScreenId = r.ScreenId, ActionId = r.ActionId,
            Allow = r.Allow, DisplayOrder = r.DisplayOrder,
            IsActive = true, CreatedBy = _user.Username
        };
        e.Id = await _repo.InsertAsync(e);
        await _audit.WriteAsync("RolePermission", e.Id.ToString(), "Assign", _user.Username);
        return (await GetByRoleAsync(r.RoleId)).First(x => x.Id == e.Id);
    }

    public async Task<bool> BulkAssignAsync(BulkAssignRolePermissionRequest r)
    {
        var entities = r.Permissions.Select(p => new RolePermissionEntry
        {
            RoleId = r.RoleId, WorkspaceId = p.WorkspaceId, DomainId = p.DomainId,
            ModuleId = p.ModuleId, SubModuleId = p.SubModuleId, ScreenId = p.ScreenId, ActionId = p.ActionId,
            Allow = p.Allow, IsActive = true, CreatedBy = _user.Username
        }).ToList();

        // 1) Persist the hierarchical matrix (atomic delete + insert).
        await _repo.BulkReplaceAsync(r.RoleId, entities);

        // 2) Bridge to the API authorization model. Derive RolePermissionsLegacy
        //    codes (e.g. "branches.view") from each screen's PermissionCode plus
        //    the action's ActionCode. Screens without a PermissionCode are not
        //    managed by the matrix, so their existing legacy codes are preserved.
        var screens = (await _scrRepo.GetAllAsync(true)).ToDictionary(s => s.Id, s => s.PermissionCode);
        var managedPrefixes = screens.Values
            .Where(pc => !string.IsNullOrWhiteSpace(pc))
            .Select(pc => pc!)
            .Distinct()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existing = (await _roleRepo.GetPermissionsForRoleAsync(r.RoleId)).ToList();
        var preserved = existing
            .Where(c => !managedPrefixes.Any(prefix => c.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var actions = (await _actRepo.GetAllAsync(true)).ToDictionary(a => a.Id, a => a.ActionCode);
        var derived = new List<string>();
        foreach (var p in r.Permissions)
        {
            if (!p.Allow) continue;
            if (!screens.TryGetValue(p.ScreenId, out var screenCode) || string.IsNullOrWhiteSpace(screenCode)) continue;
            if (!actions.TryGetValue(p.ActionId, out var actionCode) || string.IsNullOrWhiteSpace(actionCode)) continue;
            derived.Add($"{screenCode}.{actionCode}");
        }

        var finalCodes = preserved
            .Union(derived, StringComparer.OrdinalIgnoreCase)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        await _roleRepo.SetPermissionsAsync(r.RoleId, finalCodes, _user.Username);

        await _audit.WriteAsync("RolePermission", r.RoleId.ToString(), "BulkAssign", _user.Username);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var r = await _repo.DeleteAsync(id);
        if (r) await _audit.WriteAsync("RolePermission", id.ToString(), "Delete", _user.Username);
        return r;
    }

    public async Task<bool> DeleteAllForRoleAsync(int roleId)
    {
        var r = await _repo.DeleteAllForRoleAsync(roleId);
        if (r) await _audit.WriteAsync("RolePermission", roleId.ToString(), "DeleteAll", _user.Username);
        return r;
    }
}

/* ---------------------------------------------------------------------------
   UserPermissionOverride Service
   --------------------------------------------------------------------------- */
public interface IUserPermissionOverrideService
{
    Task<IEnumerable<UserPermissionOverrideDto>> GetByUserAsync(int userId);
    Task<UserPermissionOverrideDto> CreateAsync(CreateUserPermissionOverrideRequest request);
    Task<UserPermissionOverrideDto> UpdateAsync(int id, UpdateUserPermissionOverrideRequest request);
    Task<bool> DeleteAsync(int id);
}

public class UserPermissionOverrideService : IUserPermissionOverrideService
{
    private readonly IUserPermissionOverrideRepository _repo;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public UserPermissionOverrideService(IUserPermissionOverrideRepository repo, IAuditService audit, ICurrentUser user)
    { _repo = repo; _audit = audit; _user = user; }

    public async Task<IEnumerable<UserPermissionOverrideDto>> GetByUserAsync(int userId)
        => (await _repo.GetByUserAsync(userId)).Select(Map).ToList();

    public async Task<UserPermissionOverrideDto> CreateAsync(CreateUserPermissionOverrideRequest r)
    {
        var e = new UserPermissionOverride
        {
            UserId = r.UserId, WorkspaceId = r.WorkspaceId, DomainId = r.DomainId,
            ModuleId = r.ModuleId, SubModuleId = r.SubModuleId, ScreenId = r.ScreenId, ActionId = r.ActionId,
            PermissionType = r.PermissionType, Allow = r.Allow,
            EffectiveFrom = r.EffectiveFrom ?? DateTime.UtcNow, EffectiveTo = r.EffectiveTo,
            Remarks = r.Remarks, IsActive = true, CreatedBy = _user.Username
        };
        e.Id = await _repo.InsertAsync(e);
        await _audit.WriteAsync("UserPermissionOverride", e.Id.ToString(), "Create", _user.Username);
        return Map(e);
    }

    public async Task<UserPermissionOverrideDto> UpdateAsync(int id, UpdateUserPermissionOverrideRequest r)
    {
        var e = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"UserPermissionOverride '{id}' not found.");
        e.PermissionType = r.PermissionType;
        e.Allow = r.Allow;
        e.EffectiveFrom = r.EffectiveFrom ?? e.EffectiveFrom;
        e.EffectiveTo = r.EffectiveTo;
        e.Remarks = r.Remarks;
        e.IsActive = r.IsActive;
        e.ModifiedBy = _user.Username;
        await _repo.UpdateAsync(e);
        await _audit.WriteAsync("UserPermissionOverride", id.ToString(), "Update", _user.Username);
        return Map(e);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var r = await _repo.DeleteAsync(id);
        if (r) await _audit.WriteAsync("UserPermissionOverride", id.ToString(), "Delete", _user.Username);
        return r;
    }

    private static UserPermissionOverrideDto Map(UserPermissionOverride e) => new()
    {
        Id = e.Id, UserId = e.UserId, WorkspaceId = e.WorkspaceId, DomainId = e.DomainId,
        ModuleId = e.ModuleId, SubModuleId = e.SubModuleId, ScreenId = e.ScreenId, ActionId = e.ActionId,
        PermissionType = e.PermissionType, Allow = e.Allow,
        EffectiveFrom = e.EffectiveFrom, EffectiveTo = e.EffectiveTo,
        Remarks = e.Remarks, IsActive = e.IsActive, CreatedDate = e.CreatedDate
    };
}

/* ---------------------------------------------------------------------------
   RoleFieldPermissionEntry Service
   --------------------------------------------------------------------------- */
public interface IRoleFieldPermissionEntryService
{
    Task<IEnumerable<RoleFieldPermissionEntryDto>> GetByRoleAsync(int roleId);
    Task<IEnumerable<RoleFieldPermissionEntryDto>> GetByRoleScreenAsync(int roleId, int screenId);
    Task<RoleFieldPermissionEntryDto> SetAsync(SetRoleFieldPermissionRequest request);
    Task<bool> BulkSetAsync(BulkSetRoleFieldPermissionRequest request);
    Task<bool> DeleteAsync(int id);
}

public class RoleFieldPermissionEntryService : IRoleFieldPermissionEntryService
{
    private readonly IRoleFieldPermissionEntryRepository _repo;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public RoleFieldPermissionEntryService(IRoleFieldPermissionEntryRepository repo, IAuditService audit, ICurrentUser user)
    { _repo = repo; _audit = audit; _user = user; }

    public async Task<IEnumerable<RoleFieldPermissionEntryDto>> GetByRoleAsync(int roleId)
        => (await _repo.GetByRoleAsync(roleId)).Select(Map).ToList();

    public async Task<IEnumerable<RoleFieldPermissionEntryDto>> GetByRoleScreenAsync(int roleId, int screenId)
        => (await _repo.GetByRoleScreenAsync(roleId, screenId)).Select(Map).ToList();

    public async Task<RoleFieldPermissionEntryDto> SetAsync(SetRoleFieldPermissionRequest r)
    {
        var e = new RoleFieldPermissionEntry
        {
            RoleId = r.RoleId, ScreenId = r.ScreenId, FieldId = r.FieldId,
            CanView = r.CanView, CanEdit = r.CanEdit, IsHidden = r.IsHidden,
            IsReadOnly = r.IsReadOnly, IsMandatory = r.IsMandatory,
            DisplayOrder = r.DisplayOrder, IsActive = r.IsActive, CreatedBy = _user.Username
        };
        e.Id = await _repo.InsertAsync(e);
        await _audit.WriteAsync("RoleFieldPermission", e.Id.ToString(), "Set", _user.Username);
        return Map(e);
    }

    public async Task<bool> BulkSetAsync(BulkSetRoleFieldPermissionRequest r)
    {
        var entities = r.Fields.Select(f => new RoleFieldPermissionEntry
        {
            RoleId = r.RoleId, ScreenId = r.ScreenId, FieldId = f.FieldId,
            CanView = f.CanView, CanEdit = f.CanEdit, IsHidden = f.IsHidden,
            IsReadOnly = f.IsReadOnly, IsMandatory = f.IsMandatory,
            DisplayOrder = f.DisplayOrder, IsActive = true, CreatedBy = _user.Username
        });
        await _repo.BulkInsertAsync(entities);
        await _audit.WriteAsync("RoleFieldPermission", r.RoleId.ToString(), "BulkSet", _user.Username);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var r = await _repo.DeleteAsync(id);
        if (r) await _audit.WriteAsync("RoleFieldPermission", id.ToString(), "Delete", _user.Username);
        return r;
    }

    private static RoleFieldPermissionEntryDto Map(RoleFieldPermissionEntry e) => new()
    {
        Id = e.Id, RoleId = e.RoleId, ScreenId = e.ScreenId, FieldId = e.FieldId,
        CanView = e.CanView, CanEdit = e.CanEdit, IsHidden = e.IsHidden,
        IsReadOnly = e.IsReadOnly, IsMandatory = e.IsMandatory,
        DisplayOrder = e.DisplayOrder, IsActive = e.IsActive, CreatedDate = e.CreatedDate
    };
}

/* ---------------------------------------------------------------------------
   UserFieldPermissionEntry Service
   --------------------------------------------------------------------------- */
public interface IUserFieldPermissionEntryService
{
    Task<IEnumerable<UserFieldPermissionEntryDto>> GetByUserAsync(int userId);
    Task<IEnumerable<UserFieldPermissionEntryDto>> GetByUserScreenAsync(int userId, int screenId);
    Task<UserFieldPermissionEntryDto> SetAsync(SetUserFieldPermissionRequest request);
    Task<bool> DeleteAsync(int id);
}

public class UserFieldPermissionEntryService : IUserFieldPermissionEntryService
{
    private readonly IUserFieldPermissionEntryRepository _repo;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public UserFieldPermissionEntryService(IUserFieldPermissionEntryRepository repo, IAuditService audit, ICurrentUser user)
    { _repo = repo; _audit = audit; _user = user; }

    public async Task<IEnumerable<UserFieldPermissionEntryDto>> GetByUserAsync(int userId)
        => (await _repo.GetByUserAsync(userId)).Select(Map).ToList();

    public async Task<IEnumerable<UserFieldPermissionEntryDto>> GetByUserScreenAsync(int userId, int screenId)
        => (await _repo.GetByUserScreenAsync(userId, screenId)).Select(Map).ToList();

    public async Task<UserFieldPermissionEntryDto> SetAsync(SetUserFieldPermissionRequest r)
    {
        var e = new UserFieldPermissionEntry
        {
            UserId = r.UserId, ScreenId = r.ScreenId, FieldId = r.FieldId,
            CanView = r.CanView, CanEdit = r.CanEdit, IsHidden = r.IsHidden,
            IsReadOnly = r.IsReadOnly, IsMandatory = r.IsMandatory,
            IsActive = r.IsActive, CreatedBy = _user.Username
        };
        e.Id = await _repo.InsertAsync(e);
        await _audit.WriteAsync("UserFieldPermission", e.Id.ToString(), "Set", _user.Username);
        return Map(e);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var r = await _repo.DeleteAsync(id);
        if (r) await _audit.WriteAsync("UserFieldPermission", id.ToString(), "Delete", _user.Username);
        return r;
    }

    private static UserFieldPermissionEntryDto Map(UserFieldPermissionEntry e) => new()
    {
        Id = e.Id, UserId = e.UserId, ScreenId = e.ScreenId, FieldId = e.FieldId,
        CanView = e.CanView, CanEdit = e.CanEdit, IsHidden = e.IsHidden,
        IsReadOnly = e.IsReadOnly, IsMandatory = e.IsMandatory,
        IsActive = e.IsActive, CreatedDate = e.CreatedDate
    };
}

/* ---------------------------------------------------------------------------
   DataScope Service (multi-row per role, CRUD)
   --------------------------------------------------------------------------- */
public interface IDataScopeService
{
    Task<IEnumerable<DataScopeDto>> GetByRoleAsync(int roleId);
    Task<DataScopeDto?> GetByIdAsync(int id);
    Task<DataScopeDto> CreateAsync(SetDataScopeRequest request);
    Task<DataScopeDto> UpdateAsync(int id, SetDataScopeRequest request);
    Task<bool> DeleteAsync(int id);
}

public class DataScopeService : IDataScopeService
{
    private readonly IDataScopeRepository _repo;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public DataScopeService(IDataScopeRepository repo, IAuditService audit, ICurrentUser user)
    { _repo = repo; _audit = audit; _user = user; }

    public async Task<IEnumerable<DataScopeDto>> GetByRoleAsync(int roleId)
    {
        var items = await _repo.GetByRoleAsync(roleId);
        return items.Select(Map).ToList();
    }

    public async Task<DataScopeDto?> GetByIdAsync(int id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e is null ? null : Map(e);
    }

    public async Task<DataScopeDto> CreateAsync(SetDataScopeRequest r)
    {
        var e = new DataScope
        {
            RoleId = r.RoleId, ModuleId = r.ModuleId, ScreenId = r.ScreenId,
            CompanyId = r.CompanyId, BranchId = r.BranchId, DepartmentId = r.DepartmentId,
            WarehouseId = r.WarehouseId, BusinessUnitId = r.BusinessUnitId,
            CostCenterId = r.CostCenterId, ProfitCenterId = r.ProfitCenterId,
            CanView = r.CanView, CanCreate = r.CanCreate, CanEdit = r.CanEdit, CanDelete = r.CanDelete,
            IsActive = r.IsActive, CreatedBy = _user.Username
        };
        e.Id = await _repo.InsertAsync(e);
        await _audit.WriteAsync("DataScope", e.Id.ToString(), "Create", _user.Username);
        return Map(e);
    }

    public async Task<DataScopeDto> UpdateAsync(int id, SetDataScopeRequest r)
    {
        var existing = await _repo.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Data scope not found");
        existing.ModuleId = r.ModuleId;
        existing.ScreenId = r.ScreenId;
        existing.CompanyId = r.CompanyId;
        existing.BranchId = r.BranchId;
        existing.DepartmentId = r.DepartmentId;
        existing.WarehouseId = r.WarehouseId;
        existing.BusinessUnitId = r.BusinessUnitId;
        existing.CostCenterId = r.CostCenterId;
        existing.ProfitCenterId = r.ProfitCenterId;
        existing.CanView = r.CanView;
        existing.CanCreate = r.CanCreate;
        existing.CanEdit = r.CanEdit;
        existing.CanDelete = r.CanDelete;
        existing.IsActive = r.IsActive;
        existing.ModifiedBy = _user.Username;
        await _repo.UpdateAsync(existing);
        await _audit.WriteAsync("DataScope", id.ToString(), "Update", _user.Username);
        return Map(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var r = await _repo.DeleteAsync(id);
        if (r) await _audit.WriteAsync("DataScope", id.ToString(), "Delete", _user.Username);
        return r;
    }

    private static DataScopeDto Map(DataScope e) => new()
    {
        Id = e.Id, RoleId = e.RoleId, ModuleId = e.ModuleId, ScreenId = e.ScreenId,
        CompanyId = e.CompanyId, BranchId = e.BranchId, DepartmentId = e.DepartmentId,
        WarehouseId = e.WarehouseId, BusinessUnitId = e.BusinessUnitId,
        CostCenterId = e.CostCenterId, ProfitCenterId = e.ProfitCenterId,
        CanView = e.CanView, CanCreate = e.CanCreate, CanEdit = e.CanEdit, CanDelete = e.CanDelete,
        IsActive = e.IsActive, CreatedDate = e.CreatedDate
    };
}

/* ---------------------------------------------------------------------------
   UserDataScopeOverride Service
   --------------------------------------------------------------------------- */
public interface IUserDataScopeOverrideService
{
    Task<IEnumerable<UserDataScopeOverrideDto>> GetByUserAsync(int userId);
    Task<UserDataScopeOverrideDto?> GetByIdAsync(int id);
    Task<UserDataScopeOverrideDto> CreateAsync(SetUserDataScopeOverrideRequest request);
    Task<UserDataScopeOverrideDto> UpdateAsync(int id, SetUserDataScopeOverrideRequest request);
    Task<bool> DeleteAsync(int id);
}

public class UserDataScopeOverrideService : IUserDataScopeOverrideService
{
    private readonly IUserDataScopeOverrideRepository _repo;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public UserDataScopeOverrideService(IUserDataScopeOverrideRepository repo, IAuditService audit, ICurrentUser user)
    { _repo = repo; _audit = audit; _user = user; }

    public async Task<IEnumerable<UserDataScopeOverrideDto>> GetByUserAsync(int userId)
    {
        var items = await _repo.GetByUserAsync(userId);
        return items.Select(Map).ToList();
    }

    public async Task<UserDataScopeOverrideDto?> GetByIdAsync(int id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e is null ? null : Map(e);
    }

    public async Task<UserDataScopeOverrideDto> CreateAsync(SetUserDataScopeOverrideRequest r)
    {
        var e = new UserDataScopeOverride
        {
            UserId = r.UserId, ModuleId = r.ModuleId, ScreenId = r.ScreenId,
            ScopeType = r.ScopeType, ScopeValue = r.ScopeValue,
            PermissionType = r.PermissionType, Allow = r.Allow,
            EffectiveFrom = r.EffectiveFrom ?? DateTime.UtcNow,
            EffectiveTo = r.EffectiveTo, Remarks = r.Remarks,
            IsActive = r.IsActive, CreatedBy = _user.Username
        };
        e.Id = await _repo.InsertAsync(e);
        await _audit.WriteAsync("UserDataScopeOverride", e.Id.ToString(), "Create", _user.Username);
        return Map(e);
    }

    public async Task<UserDataScopeOverrideDto> UpdateAsync(int id, SetUserDataScopeOverrideRequest r)
    {
        var existing = await _repo.GetByIdAsync(id)
            ?? throw new InvalidOperationException("User data scope override not found");
        existing.ScopeType = r.ScopeType;
        existing.ScopeValue = r.ScopeValue;
        existing.PermissionType = r.PermissionType;
        existing.Allow = r.Allow;
        existing.EffectiveFrom = r.EffectiveFrom ?? existing.EffectiveFrom;
        existing.EffectiveTo = r.EffectiveTo;
        existing.Remarks = r.Remarks;
        existing.IsActive = r.IsActive;
        existing.ModifiedBy = _user.Username;
        await _repo.UpdateAsync(existing);
        await _audit.WriteAsync("UserDataScopeOverride", id.ToString(), "Update", _user.Username);
        return Map(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var r = await _repo.DeleteAsync(id);
        if (r) await _audit.WriteAsync("UserDataScopeOverride", id.ToString(), "Delete", _user.Username);
        return r;
    }

    private static UserDataScopeOverrideDto Map(UserDataScopeOverride e) => new()
    {
        Id = e.Id, UserId = e.UserId, ModuleId = e.ModuleId, ScreenId = e.ScreenId,
        ScopeType = e.ScopeType, ScopeValue = e.ScopeValue,
        PermissionType = e.PermissionType, Allow = e.Allow,
        EffectiveFrom = e.EffectiveFrom, EffectiveTo = e.EffectiveTo,
        Remarks = e.Remarks, IsActive = e.IsActive, CreatedDate = e.CreatedDate
    };
}

/* ---------------------------------------------------------------------------
   WorkflowPermissionEntry Service
   --------------------------------------------------------------------------- */
public interface IWorkflowPermissionEntryService
{
    Task<IEnumerable<WorkflowPermissionEntryDto>> GetByRoleAsync(int roleId);
    Task<WorkflowPermissionEntryDto> SetAsync(SetWorkflowPermissionRequest request);
    Task<bool> DeleteAsync(int id);
}

public class WorkflowPermissionEntryService : IWorkflowPermissionEntryService
{
    private readonly IWorkflowPermissionEntryRepository _repo;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _user;

    public WorkflowPermissionEntryService(IWorkflowPermissionEntryRepository repo, IAuditService audit, ICurrentUser user)
    { _repo = repo; _audit = audit; _user = user; }

    public async Task<IEnumerable<WorkflowPermissionEntryDto>> GetByRoleAsync(int roleId)
        => (await _repo.GetByRoleAsync(roleId)).Select(Map).ToList();

    public async Task<WorkflowPermissionEntryDto> SetAsync(SetWorkflowPermissionRequest r)
    {
        var e = new WorkflowPermissionEntry
        {
            RoleId = r.RoleId, ModuleId = r.ModuleId, SubModuleId = r.SubModuleId, ScreenId = r.ScreenId,
            CanSubmit = r.CanSubmit, CanApprove = r.CanApprove, CanReject = r.CanReject,
            CanCancel = r.CanCancel, CanClose = r.CanClose,
            IsActive = r.IsActive, CreatedBy = _user.Username
        };
        e.Id = await _repo.InsertAsync(e);
        await _audit.WriteAsync("WorkflowPermission", e.Id.ToString(), "Set", _user.Username);
        return Map(e);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var r = await _repo.DeleteAsync(id);
        if (r) await _audit.WriteAsync("WorkflowPermission", id.ToString(), "Delete", _user.Username);
        return r;
    }

    private static WorkflowPermissionEntryDto Map(WorkflowPermissionEntry e) => new()
    {
        Id = e.Id, RoleId = e.RoleId, ModuleId = e.ModuleId, SubModuleId = e.SubModuleId, ScreenId = e.ScreenId,
        CanSubmit = e.CanSubmit, CanApprove = e.CanApprove, CanReject = e.CanReject,
        CanCancel = e.CanCancel, CanClose = e.CanClose,
        IsActive = e.IsActive, CreatedDate = e.CreatedDate
    };
}
