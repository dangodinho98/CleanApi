using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CleanApi.Application.Abstractions;
using CleanApi.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CleanApi.Infrastructure.Security;

/// <summary>Builds HS256 JWTs from <see cref="JwtSettings"/> (issuer, audience, signing key, lifetime).</summary>
public sealed class JwtTokenIssuer(IOptions<JwtSettings> settings) : IJwtTokenIssuer
{
    private readonly JwtSettings _settings = settings.Value;
    private readonly JwtSecurityTokenHandler _handler = new();

    public string CreateToken(Guid userId, string email, string displayName)
    {
        if (string.IsNullOrWhiteSpace(_settings.SigningKey))
            throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Email, email),
            new("display_name", displayName),
            new(ClaimTypes.GivenName, displayName)
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: creds);

        return _handler.WriteToken(token);
    }
}
