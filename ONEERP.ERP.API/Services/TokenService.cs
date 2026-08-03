using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ONEERP.ERP.API.Data;
using ONEERP.ERP.API.Models;
using SharedClaimTypes = ONEERP.Shared.Constants.ClaimTypes;

namespace ONEERP.ERP.API.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user, TenantSession tenant, IEnumerable<Role> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user, TenantSession tenant, IEnumerable<Role> roles, IEnumerable<string> permissions)
    {
        var jwt = _configuration.GetSection("Jwt");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, string.Join(",", roles.Select(r => r.Name))),
            new(SharedClaimTypes.TenantId, tenant.TenantId.ToString()),
            new(SharedClaimTypes.TenantCode, tenant.TenantCode),
            new(SharedClaimTypes.CompanyId, user.CompanyId.ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role.Name));

        foreach (var permission in permissions)
            claims.Add(new Claim(SharedClaimTypes.Permission, permission));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["AccessTokenMinutes"] ?? "15")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
