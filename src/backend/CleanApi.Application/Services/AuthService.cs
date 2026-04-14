using CleanApi.Application.Abstractions;
using CleanApi.Domain.Entities;
using System.Text.RegularExpressions;

namespace CleanApi.Application.Services;

/// <summary>Registration, login, and user lookup; coordinates hashing and JWT issuance.</summary>
public class AuthService(IUserRepository users, IPasswordHasher passwordHasher, IJwtTokenIssuer jwtTokenIssuer)
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public const int MinPasswordLength = 8;
    public const int MaxDisplayNameLength = 200;

    public async Task<(User User, string Token)> RegisterAsync(string email, string password, string displayName, CancellationToken cancellationToken = default)
    {
        ValidateEmail(email);
        ValidatePassword(password);
        ValidateDisplayName(displayName);

        var normalized = email.Trim().ToLowerInvariant();
        var existing = await users.GetByEmailAsync(normalized, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalized,
            PasswordHash = passwordHasher.Hash(password),
            DisplayName = displayName.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        var created = await users.InsertAsync(user, cancellationToken);
        var token = jwtTokenIssuer.CreateToken(created.Id, created.Email, created.DisplayName);
        return (created, token);
    }

    public async Task<(User User, string Token)> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(password);

        ValidateEmail(email);

        var normalized = email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(normalized, cancellationToken);
        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
            throw new InvalidOperationException("Invalid email or password.");

        var token = jwtTokenIssuer.CreateToken(user.Id, user.Email, user.DisplayName);
        return (user, token);
    }

    public Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        users.GetByIdAsync(id, cancellationToken);

    private static void ValidateEmail(string email)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (!EmailRegex.IsMatch(email.Trim()))
            throw new ArgumentException("Email format is invalid.", nameof(email));
    }

    private static void ValidatePassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (password.Length < MinPasswordLength)
            throw new ArgumentException($"Password must be at least {MinPasswordLength} characters.", nameof(password));
    }

    private static void ValidateDisplayName(string displayName)
    {
        ArgumentNullException.ThrowIfNull(displayName);

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));

        if (displayName.Trim().Length > MaxDisplayNameLength)
            throw new ArgumentException($"Display name must be at most {MaxDisplayNameLength} characters.", nameof(displayName));
    }
}
