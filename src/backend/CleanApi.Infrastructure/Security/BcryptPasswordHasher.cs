using CleanApi.Application.Abstractions;

namespace CleanApi.Infrastructure.Security;

/// <summary>BCrypt-backed <see cref="CleanApi.Application.Abstractions.IPasswordHasher"/>.</summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
