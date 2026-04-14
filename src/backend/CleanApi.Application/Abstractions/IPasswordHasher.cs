namespace CleanApi.Application.Abstractions;

/// <summary>Hash and verify passwords without exposing storage details to the application layer.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
