using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using indexApp.Features.Auth.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace indexApp.Features.Auth;

public interface IJwtTokenService
{
    LoginResult CreateToken(AdminUser user);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IOptions<AuthOptions> options;

    public JwtTokenService(IOptions<AuthOptions> options)
    {
        this.options = options;
    }

    public LoginResult CreateToken(AdminUser user)
    {
        var jwt = options.Value.Jwt;
        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(jwt.ExpirationMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("admin_user_id", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return new LoginResult(
            user.Id.ToString(),
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc,
            user.Email,
            user.Role);
    }
}
