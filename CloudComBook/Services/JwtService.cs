using CloudComBook.API.Configuration;
using CloudComBook.Shared.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CloudComBook.API.Services;

public class JwtService
{
    private readonly JwtOptions _options;

    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Login),
            new(ClaimTypes.Name, string.IsNullOrWhiteSpace(user.FullName) ? user.Login : user.FullName),
            new("UserId", user.Id.ToString()),
            new("login", user.Login),
            new("community_id", user.CommunityId?.ToString() ?? "")
        };

        // супер-адмін має обидві ролі, тож усі наявні [Authorize(Roles = "admin")] працюють без змін
        if (user.Role == "superadmin")
        {
            claims.Add(new Claim(ClaimTypes.Role, "superadmin"));
            claims.Add(new Claim(ClaimTypes.Role, "admin"));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_options.ExpireHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}