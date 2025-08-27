using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Adra.Application.DTOs;
using Adra.Application.Interfaces;
using Adra.Core.Entities;
using Adra.Core.Common;

namespace Adra.Application.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;

    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public LoginResponse GenerateToken(User user, List<string> roles)
    {
        if (!_jwtSettings.IsValid())
            throw new InvalidOperationException("JWT settings are not properly configured");

        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
        var issuer = _jwtSettings.Issuer;
        var audience = _jwtSettings.Audience;
        var tokenLifetime = _jwtSettings.TokenLifetimeInMinutes;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(tokenLifetime),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new LoginResponse
        {
            AccessToken = tokenHandler.WriteToken(token),
            ExpiresAt = tokenDescriptor.Expires.Value,
            Roles = roles
        };
    }
}
