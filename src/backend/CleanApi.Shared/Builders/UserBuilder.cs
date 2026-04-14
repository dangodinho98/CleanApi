using CleanApi.Domain.Entities;

namespace CleanApi.Shared.Builders;

/// <summary>Fluent builder for test <see cref="CleanApi.Domain.Entities.User"/> instances.</summary>
public sealed class UserBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _email = "user@test.com";
    private string _passwordHash = "hash";
    private string _displayName = "User";
    private DateTime _createdAtUtc = DateTime.UtcNow;

    public static UserBuilder Default() => new();

    public UserBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public UserBuilder WithDisplayName(string displayName)
    {
        _displayName = displayName;
        return this;
    }

    public UserBuilder WithCreatedAtUtc(DateTime createdAtUtc)
    {
        _createdAtUtc = createdAtUtc;
        return this;
    }

    public User Build() =>
        new()
        {
            Id = _id,
            Email = _email,
            PasswordHash = _passwordHash,
            DisplayName = _displayName,
            CreatedAtUtc = _createdAtUtc
        };
}
