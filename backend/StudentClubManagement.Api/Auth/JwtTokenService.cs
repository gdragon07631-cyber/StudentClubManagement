using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StudentClubManagement.Models;

namespace StudentClubManagement.Api;

public class JwtOptions
{
    public string Issuer { get; set; } = "StudentClubManagement.Api";
    public string Audience { get; set; } = "StudentClubManagement.Web";
    public string Key { get; set; } = "";
    public int ExpireHours { get; set; } = 8;
}

/// <summary>Lỗi nghiệp vụ trả về cho Frontend: { "message": "..." }.</summary>
public record ApiError(string Message);

public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, int UserId, string UserName, string? Email, string[] Roles);

public class JwtTokenService(JwtOptions options, UserManager<ApplicationUser> users)
{
    public async Task<LoginResponse> CreateAsync(ApplicationUser user)
    {
        var roles = (await users.GetRolesAsync(user)).ToArray();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? ""),
        };
        if (!string.IsNullOrEmpty(user.Email)) claims.Add(new Claim(ClaimTypes.Email, user.Email));
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        var expires = DateTime.UtcNow.AddHours(options.ExpireHours);
        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = options.Issuer, Audience = options.Audience, Expires = expires,
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)), SecurityAlgorithms.HmacSha256)
        });
        return new LoginResponse(token, expires, user.Id, user.UserName ?? "", user.Email, roles);
    }
}

