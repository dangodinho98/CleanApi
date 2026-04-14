using CleanApi.Application.Abstractions;
using CleanApi.Domain.Entities;
using CleanApi.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace CleanApi.Infrastructure.Persistence;

/// <summary>ADO.NET implementation of <see cref="CleanApi.Application.Abstractions.IItemRepository"/>.</summary>
public sealed class SqlServerItemRepository(ISqlConnectionFactory connections) : IItemRepository
{
    public async Task<IReadOnlyList<Item>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var conn = connections.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(
            """
            SELECT Id, Title, Description, CreatedAtUtc, OwnerUserId
            FROM dbo.Items
            ORDER BY CreatedAtUtc DESC;
            """,
            conn);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var list = new List<Item>();
        while (await reader.ReadAsync(cancellationToken))
            list.Add(Map(reader));

        return list;
    }

    public async Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var conn = connections.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(
            """
            SELECT Id, Title, Description, CreatedAtUtc, OwnerUserId
            FROM dbo.Items
            WHERE Id = @Id;
            """,
            conn);

        cmd.Parameters.AddWithValue("@Id", id);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return Map(reader);
    }

    public async Task<Item> InsertAsync(Item item, CancellationToken cancellationToken = default)
    {
        await using var conn = connections.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(
            """
            INSERT INTO dbo.Items (Id, Title, Description, CreatedAtUtc, OwnerUserId)
            OUTPUT INSERTED.CreatedAtUtc
            VALUES (@Id, @Title, @Description, @CreatedAtUtc, @OwnerUserId);
            """,
            conn);

        cmd.Parameters.AddWithValue("@Id", item.Id);
        cmd.Parameters.AddWithValue("@Title", item.Title);
        cmd.Parameters.AddWithValue("@Description", item.Description);
        cmd.Parameters.AddWithValue("@CreatedAtUtc", item.CreatedAtUtc);
        cmd.Parameters.AddWithValue("@OwnerUserId", (object?)item.OwnerUserId ?? DBNull.Value);

        var createdAt = (DateTime)(await cmd.ExecuteScalarAsync(cancellationToken) ?? item.CreatedAtUtc);
        item.CreatedAtUtc = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);
        return item;
    }

    public async Task<bool> UpdateAsync(Item item, CancellationToken cancellationToken = default)
    {
        await using var conn = connections.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(
            """
            UPDATE dbo.Items
            SET Title = @Title,
                Description = @Description,
                OwnerUserId = @OwnerUserId
            WHERE Id = @Id;
            """,
            conn);

        cmd.Parameters.AddWithValue("@Id", item.Id);
        cmd.Parameters.AddWithValue("@Title", item.Title);
        cmd.Parameters.AddWithValue("@Description", item.Description);
        cmd.Parameters.AddWithValue("@OwnerUserId", (object?)item.OwnerUserId ?? DBNull.Value);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var conn = connections.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("DELETE FROM dbo.Items WHERE Id = @Id;", conn);
        cmd.Parameters.AddWithValue("@Id", id);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    private static Item Map(SqlDataReader reader)
    {
        var ownerOrdinal = reader.GetOrdinal("OwnerUserId");
        return new Item
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Description = reader.GetString(reader.GetOrdinal("Description")),
            CreatedAtUtc = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")), DateTimeKind.Utc),
            OwnerUserId = reader.IsDBNull(ownerOrdinal) ? null : reader.GetGuid(ownerOrdinal)
        };
    }
}
