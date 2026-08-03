using System.Security.Claims;
using ONEERP.Shared.Exceptions;

namespace ONEERP.Platform.API.Services;

/// <summary>
/// Resolves the currently authenticated platform user from the HTTP context.
/// </summary>
public interface ICurrentUser
{
    int UserId { get; }
    string Username { get; }
    string? DisplayName { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var value = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : throw new UnauthorizedAccess();
        }
    }

    public string Username =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    public string? DisplayName =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.GivenName);
}
