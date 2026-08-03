using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Models;
using ONEERP.ERP.API.Repositories;
using ONEERP.Shared.Exceptions;
using ONEERP.Shared.Helpers;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Services;

public interface IUserService
{
    Task<PaginatedResult<UserWithRolesDto>> GetPagedAsync(int pageNumber, int pageSize, string search);
    Task<UserWithRolesDto> GetByIdAsync(int userId);
    Task<UserDto> CreateAsync(CreateUserRequest request, int companyId, string currentUser);
    Task<UserDto> UpdateAsync(int userId, UpdateUserRequest request, string currentUser);
    Task<bool> DeleteAsync(int userId, string currentUser);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly TenantAccessor _accessor;
    private readonly IAuditService _auditService;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        TenantAccessor accessor,
        IAuditService auditService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _accessor = accessor;
        _auditService = auditService;
    }

    public async Task<PaginatedResult<UserWithRolesDto>> GetPagedAsync(int pageNumber, int pageSize, string search)
    {
        var normalizedPage = pageNumber < 1 ? 1 : pageNumber;
        var normalizedSize = pageSize < 1 ? 10 : pageSize;

        var users = await _userRepository.GetPagedAsync(normalizedPage, normalizedSize, search);
        var total = await _userRepository.CountAsync(search);
        var userIds = users.Select(u => u.UserId).ToList();
        var assignments = await _roleRepository.GetRoleAssignmentsForUsersAsync(userIds);

        var grouped = assignments.GroupBy(a => a.UserId).ToDictionary(
            g => g.Key,
            g => g.Select(a => (RoleId: a.RoleId, RoleName: a.RoleName)).ToList());

        return new PaginatedResult<UserWithRolesDto>
        {
            Items = users.Select(u => new UserWithRolesDto
            {
                UserId = u.UserId,
                CompanyId = u.CompanyId,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                Mobile = u.Mobile,
                Status = u.Status,
                IsSuperAdmin = u.IsSuperAdmin,
                LastLoginDate = u.LastLoginDate,
                CreatedDate = u.CreatedDate,
                RoleIds = grouped.TryGetValue(u.UserId, out var assignments1) ? assignments1.Select(a => a.RoleId).ToList() : new List<int>(),
                RoleNames = grouped.TryGetValue(u.UserId, out var assignments2) ? assignments2.Select(a => a.RoleName).ToList() : new List<string>()
            }).ToList(),
            TotalCount = total,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public async Task<UserWithRolesDto> GetByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User '{userId}' was not found.");

        var roles = await _roleRepository.GetRolesForUserAsync(userId);

        return new UserWithRolesDto
        {
            UserId = user.UserId,
            CompanyId = user.CompanyId,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Mobile = user.Mobile,
            Status = user.Status,
            IsSuperAdmin = user.IsSuperAdmin,
            LastLoginDate = user.LastLoginDate,
            CreatedDate = user.CreatedDate,
            RoleIds = roles.Select(r => r.RoleId).ToList(),
            RoleNames = roles.Select(r => r.Name).ToList()
        };
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, int companyId, string currentUser)
    {
        var username = request.Username.Trim();

        if (await _userRepository.UsernameInUseAsync(username))
            throw new DomainException($"Username '{username}' is already in use.");

        if (await _userRepository.EmailInUseAsync(request.Email.Trim()))
            throw new DomainException($"Email '{request.Email}' is already in use.");

        var passwordErrors = PasswordPolicy.Validate(request.Password);
        if (passwordErrors.Count > 0)
            throw new DomainException("Password does not meet the policy.", 400);

        var user = new User
        {
            CompanyId = companyId,
            Username = username,
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Mobile = request.Mobile,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Status = request.Status,
            IsSuperAdmin = false,
            CreatedBy = currentUser,
            ModifiedBy = currentUser
        };

        using (var connection = _accessor.OpenTenantConnection())
        {
            connection.Open();
            using var transaction = connection.BeginTransaction();

            user.UserId = await _userRepository.InsertAsync(user, connection, transaction);
            await _userRepository.AssignRolesAsync(user.UserId, request.RoleIds, currentUser, connection, transaction);

            transaction.Commit();
        }

        await _auditService.WriteAsync("User", user.UserId.ToString(), "Create", currentUser);
        return ToDto(user, request.RoleIds);
    }

    public async Task<UserDto> UpdateAsync(int userId, UpdateUserRequest request, string currentUser)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User '{userId}' was not found.");

        if (user.IsSuperAdmin && request.Status != ONEERP.Shared.Constants.EntityStatus.Active)
            throw new DomainException("A super admin cannot be deactivated.");

        var existingByEmail = await _userRepository.GetByEmailAsync(request.Email.Trim());
        if (existingByEmail is not null && existingByEmail.UserId != userId)
            throw new DomainException($"Email '{request.Email}' is already in use by another user.");

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim();
        user.Mobile = request.Mobile;
        user.Status = request.Status;
        user.ModifiedBy = currentUser;

        using (var connection = _accessor.OpenTenantConnection())
        {
            connection.Open();
            using var transaction = connection.BeginTransaction();

            await _userRepository.UpdateAsync(user, connection, transaction);
            await _userRepository.RemoveRolesAsync(userId, connection, transaction);
            await _userRepository.AssignRolesAsync(userId, request.RoleIds, currentUser, connection, transaction);

            transaction.Commit();
        }

        await _auditService.WriteAsync("User", userId.ToString(), "Update", currentUser);
        return ToDto(user, request.RoleIds);
    }

    public async Task<bool> DeleteAsync(int userId, string currentUser)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User '{userId}' was not found.");

        if (user.IsSuperAdmin)
            throw new DomainException("A super admin cannot be deleted.");

        await _userRepository.SoftDeleteAsync(userId, currentUser);
        await _auditService.WriteAsync("User", userId.ToString(), "Delete", currentUser);
        return true;
    }

    private static UserDto ToDto(User u, IEnumerable<int> roleIds) => new()
    {
        UserId = u.UserId,
        CompanyId = u.CompanyId,
        Username = u.Username,
        FullName = u.FullName,
        Email = u.Email,
        Mobile = u.Mobile,
        Status = u.Status,
        IsSuperAdmin = u.IsSuperAdmin,
        Roles = new List<string>()
    };
}
