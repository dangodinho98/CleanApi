using CleanApi.Infrastructure.Configuration;
using CleanApi.Infrastructure.Security;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace CleanApi.Infrastructure.Tests.UnitTests;

public sealed class JwtTokenIssuerTests
{
    [Fact]
    public void CreateTokenThrowsWhenSigningKeyMissing()
    {
        var sut = new JwtTokenIssuer(Options.Create(new JwtSettings { SigningKey = "" }));

        Assert.Throws<InvalidOperationException>(() =>
            sut.CreateToken(Guid.NewGuid(), "a@test.com", "N"));
    }

    [Fact]
    public void CreateTokenEmbedsSubjectEmailDisplayAudienceAndIssuer()
    {
        var userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var sut = new JwtTokenIssuer(Options.Create(new JwtSettings
        {
            SigningKey = new string('x', 48),
            Issuer = "unit-issuer",
            Audience = "unit-audience",
            ExpirationMinutes = 30
        }));

        var jwt = sut.CreateToken(userId, "who@test.com", "Who");

        var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        Assert.Equal(userId.ToString(), token.Subject);
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "who@test.com");
        Assert.Contains(token.Claims, c => c.Type == "display_name" && c.Value == "Who");
        Assert.Equal("unit-issuer", token.Issuer);
        Assert.Contains(token.Audiences, a => a == "unit-audience");
    }
}
