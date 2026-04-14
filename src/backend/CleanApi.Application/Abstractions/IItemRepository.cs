using CleanApi.Domain.Entities;

namespace CleanApi.Application.Abstractions;

/// <summary>Persistence port for <see cref="Item"/> CRUD.</summary>
public interface IItemRepository
{
    Task<IReadOnlyList<Item>> ListAsync(CancellationToken cancellationToken = default);
    Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Item> InsertAsync(Item item, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Item item, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
