using CleanApi.Application.Abstractions;
using CleanApi.Domain.Entities;

namespace CleanApi.Application.Services;

/// <summary>Application rules for listing, creating, updating, and deleting items.</summary>
public class ItemService(IItemRepository items)
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 4000;

    public Task<IReadOnlyList<Item>> ListAsync(CancellationToken cancellationToken = default) =>
        items.ListAsync(cancellationToken);

    public Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        items.GetByIdAsync(id, cancellationToken);

    public async Task<Item> CreateAsync(string title, string description, Guid? ownerUserId, CancellationToken cancellationToken = default)
    {
        ValidateTitle(title);
        ValidateDescription(description);

        var item = new Item
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            OwnerUserId = ownerUserId
        };

        return await items.InsertAsync(item, cancellationToken);
    }

    public async Task<bool> UpdateAsync(Guid id, string title, string description, CancellationToken cancellationToken = default)
    {
        ValidateTitle(title);
        ValidateDescription(description);

        var existing = await items.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return false;

        existing.Title = title.Trim();
        existing.Description = description.Trim();
        return await items.UpdateAsync(existing, cancellationToken);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        items.DeleteAsync(id, cancellationToken);

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (title.Trim().Length > MaxTitleLength)
            throw new ArgumentException($"Title must be at most {MaxTitleLength} characters.", nameof(title));
    }

    private static void ValidateDescription(string description)
    {
        ArgumentNullException.ThrowIfNull(description);
        if (description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description must be at most {MaxDescriptionLength} characters.", nameof(description));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));
    }
}
