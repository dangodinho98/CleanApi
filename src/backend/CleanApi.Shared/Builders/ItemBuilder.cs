using CleanApi.Domain.Entities;

namespace CleanApi.Shared.Builders;

/// <summary>Fluent builder for test <see cref="CleanApi.Domain.Entities.Item"/> instances.</summary>
public sealed class ItemBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _title = "Title";
    private string _description = "Description";
    private DateTime _createdAtUtc = DateTime.UtcNow;
    private Guid? _ownerUserId;

    public static ItemBuilder Default() => new();

    public ItemBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ItemBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ItemBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ItemBuilder WithCreatedAtUtc(DateTime createdAtUtc)
    {
        _createdAtUtc = createdAtUtc;
        return this;
    }

    public ItemBuilder WithOwnerUserId(Guid? ownerUserId)
    {
        _ownerUserId = ownerUserId;
        return this;
    }

    public Item Build() =>
        new()
        {
            Id = _id,
            Title = _title,
            Description = _description,
            CreatedAtUtc = _createdAtUtc,
            OwnerUserId = _ownerUserId
        };
}
