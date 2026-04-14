namespace CleanApi.Infrastructure.Configuration;

/// <summary>Binds the <c>Jwt</c> configuration section (issuer, audience, signing key, token lifetime).</summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "CleanApi";
    public string Audience { get; set; } = "CleanApiClients";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
