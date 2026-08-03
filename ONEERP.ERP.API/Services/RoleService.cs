using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;

namespace ONEERP.ERP.API.Services;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllAsync(bool includeInactive = false);
    Task<RoleDto> GetByIdAsync(int roleId);
    Task<RoleDto> CreateAsync(CreateRoleRequest request);
    Task<RoleDto> UpdateAsync(int roleId, UpdateRoleRequest request);
    Task<bool> DeleteAsync(int roleId);
    Task<RoleDto> SetPermissionsAsync(int roleId, SetRolePermissionsRequest request);
    Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
}

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public RoleService(IRoleRepository roleRepository, IAuditService auditService, ICurrentUser currentUser)
    {
        _roleRepository = roleRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
    {
        var modules = new (string Module, string[] Codes)[]
        {
            ("Dashboard", new[] { "dashboard.view" }),
            ("Companies", new[] { "companies.view", "companies.create", "companies.edit" }),
            ("Users", new[] { "users.view", "users.create", "users.edit", "users.delete" }),
            ("Roles", new[] { "roles.view", "roles.manage" }),
            ("Business Types", new[] { "business-types.view", "business-types.manage" }),
            ("Industry Types", new[] { "industry-types.view", "industry-types.manage" }),
            ("Company Groups", new[] { "company-groups.view", "company-groups.manage" }),
            ("Locations", new[] { "locations.view", "locations.create", "locations.edit", "locations.delete" }),
            ("Languages", new[] { "languages.view", "languages.manage" }),
            ("Time Zones", new[] { "timezones.view", "timezones.manage" }),
            ("GST Registration Types", new[] { "gst-registration-types.view", "gst-registration-types.manage" }),
            ("Address Types", new[] { "address-types.view", "address-types.manage" }),
            ("Contact Types", new[] { "contact-types.view", "contact-types.manage" }),
            ("Document Types", new[] { "document-types.view", "document-types.manage" }),
            ("Organization Types", new[] { "organization-types.view", "organization-types.manage" }),
            ("Settings", new[] { "settings.view", "settings.edit" }),
            ("Audit", new[] { "audit.view" }),
            ("Profile", new[] { "profile.edit" })
        };

        var result = modules
            .SelectMany(m => m.Codes.Select(code => new PermissionDto
            {
                Code = code,
                Name = Humanize(code),
                Module = m.Module
            }))
            .ToList();

        return Task.FromResult<IEnumerable<PermissionDto>>(result);
    }

    private static string Humanize(string code)
    {
        var parts = code.Split('.');
        if (parts.Length != 2)
            return code;

        var area = parts[0];
        var action = parts[1] switch
        {
            "view" => "View",
            "create" => "Create",
            "edit" => "Edit",
            "delete" => "Delete",
            "manage" => "Manage",
            _ => parts[1]
        };

        return $"{char.ToUpperInvariant(area[0])}{area[1..]} - {action}";
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync(bool includeInactive = false)
    {
        var roles = await _roleRepository.GetAllAsync(includeInactive);
        var allPermissions = await _roleRepository.GetAllPermissionsAsync();

        var permissionByRole = allPermissions
            .GroupBy(p => p.RoleId)
            .ToDictionary(g => g.Key, g => g.Select(p => p.PermissionCode).ToList());

        return roles.Select(r => new RoleDto
        {
            RoleId = r.RoleId,
            Name = r.Name,
            Code = r.Code,
            Description = r.Description,
            IsSystem = r.IsSystem,
            IsActive = r.IsActive,
            Permissions = permissionByRole.TryGetValue(r.RoleId, out var perms) ? perms : new List<string>()
        });
    }

    public async Task<RoleDto> GetByIdAsync(int roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException($"Role '{roleId}' was not found.");

        var permissions = await _roleRepository.GetPermissionsForRoleAsync(roleId);

        return new RoleDto
        {
            RoleId = role.RoleId,
            Name = role.Name,
            Code = role.Code,
            Description = role.Description,
            IsSystem = role.IsSystem,
            IsActive = role.IsActive,
            Permissions = permissions.ToList()
        };
    }

    public async Task<RoleDto> CreateAsync(CreateRoleRequest request)
    {
        if (await _roleRepository.GetByCodeAsync(request.Code) is not null)
            throw new DomainException($"A role with code '{request.Code}' already exists.");

        var role = new Role
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim(),
            Description = request.Description,
            IsSystem = false,
            IsActive = true,
            CreatedBy = _currentUser.Username,
            ModifiedBy = _currentUser.Username
        };

        role.RoleId = await _roleRepository.InsertAsync(role);

        if (request.Permissions is { Count: > 0 })
            await _roleRepository.SetPermissionsAsync(role.RoleId, request.Permissions, _currentUser.Username);

        await _auditService.WriteAsync("Role", role.RoleId.ToString(), "Create", _currentUser.Username);
        return await GetByIdAsync(role.RoleId);
    }

    public async Task<RoleDto> UpdateAsync(int roleId, UpdateRoleRequest request)
    {
        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException($"Role '{roleId}' was not found.");

        if (role.IsSystem && !request.IsActive)
            throw new DomainException("System roles cannot be deactivated.");

        role.Name = request.Name.Trim();
        role.Description = request.Description;
        role.IsActive = request.IsActive;
        role.ModifiedBy = _currentUser.Username;

        await _roleRepository.UpdateAsync(role);

        if (request.Permissions is not null)
            await _roleRepository.SetPermissionsAsync(roleId, request.Permissions, _currentUser.Username);

        await _auditService.WriteAsync("Role", roleId.ToString(), "Update", _currentUser.Username);
        return await GetByIdAsync(roleId);
    }

    public async Task<bool> DeleteAsync(int roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException($"Role '{roleId}' was not found.");

        if (role.IsSystem)
            throw new DomainException("System roles cannot be deleted.");

        await _roleRepository.SoftDeleteAsync(roleId, _currentUser.Username);
        await _auditService.WriteAsync("Role", roleId.ToString(), "Delete", _currentUser.Username);
        return true;
    }

    public async Task<RoleDto> SetPermissionsAsync(int roleId, SetRolePermissionsRequest request)
    {
        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException($"Role '{roleId}' was not found.");

        await _roleRepository.SetPermissionsAsync(roleId, request.PermissionCodes, _currentUser.Username);
        await _auditService.WriteAsync("Role", roleId.ToString(), "UpdatePermissions", _currentUser.Username);
        return await GetByIdAsync(roleId);
    }
}
