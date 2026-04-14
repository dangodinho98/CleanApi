namespace CleanApi.Domain.Entities;

/// <summary>User-owned content row (title, description, optional owner).</summary>
public class Item
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public Guid? OwnerUserId { get; set; }
}
