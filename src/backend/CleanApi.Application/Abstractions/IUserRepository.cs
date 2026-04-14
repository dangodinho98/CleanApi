using CleanApi.Domain.Entities;

namespace CleanApi.Application.Abstractions;

/// <summary>Persistence port for user lookup and insert.</summary>
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User> InsertAsync(User user, CancellationToken cancellationToken = default);
}
