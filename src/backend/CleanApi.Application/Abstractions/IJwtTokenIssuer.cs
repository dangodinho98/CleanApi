namespace CleanApi.Application.Abstractions;

/// <summary>Issues signed bearer tokens for authenticated users.</summary>
public interface IJwtTokenIssuer
{
    string CreateToken(Guid userId, string email, string displayName);
}
