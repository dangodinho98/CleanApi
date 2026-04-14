using CleanApi.Application.Abstractions;
using CleanApi.Domain.Entities;
using CleanApi.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CleanApi.Infrastructure.Persistence;

/// <summary>ADO.NET implementation of <see cref="CleanApi.Application.Abstractions.IUserRepository"/>.</summary>
public sealed class SqlServerUserRepository(ISqlConnectionFactory connections) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var conn = connections.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(
            """
            SELECT Id, Email, PasswordHash, DisplayName, CreatedAtUtc
            FROM dbo.Users
            WHERE Email = @Email;
            """,
            conn);

        cmd.Parameters.AddWithValue("@Email", email);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return Map(reader);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var conn = connections.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(
            """
            SELECT Id, Email, PasswordHash, DisplayName, CreatedAtUtc
            FROM dbo.Users
            WHERE Id = @Id;
            """,
            conn);

        cmd.Parameters.AddWithValue("@Id", id);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return Map(reader);
    }

    public async Task<User> InsertAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var conn = connections.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(
            """
            INSERT INTO dbo.Users (Id, Email, PasswordHash, DisplayName, CreatedAtUtc)
            OUTPUT INSERTED.CreatedAtUtc
            VALUES (@Id, @Email, @PasswordHash, @DisplayName, @CreatedAtUtc);
            """,
            conn);

        cmd.Parameters.AddWithValue("@Id", user.Id);
        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@DisplayName", user.DisplayName);
        cmd.Parameters.AddWithValue("@CreatedAtUtc", user.CreatedAtUtc);

        var createdAt = (DateTime)(await cmd.ExecuteScalarAsync(cancellationToken) ?? user.CreatedAtUtc);
        user.CreatedAtUtc = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);
        return user;
    }

    private static User Map(SqlDataReader reader) => new()
    {
        Id = reader.GetGuid(reader.GetOrdinal("Id")),
        Email = reader.GetString(reader.GetOrdinal("Email")),
        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
        DisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
        CreatedAtUtc = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")), DateTimeKind.Utc)
    };
}
