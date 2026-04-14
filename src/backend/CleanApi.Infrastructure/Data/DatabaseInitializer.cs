using CleanApi.Shared.Constants;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace CleanApi.Infrastructure.Data;

/// <summary>Ensures the SQL catalog exists, applies idempotent schema batches, and seeds demo rows.</summary>
public sealed class DatabaseInitializer(ISqlConnectionFactory connections, ILogger<DatabaseInitializer> logger)
{
    public static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DemoItemId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = connections.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"{Database.ConnectionStringPath} is not configured.");

        await EnsureDatabaseExistsAsync(connectionString, cancellationToken);

        await using var conn = connections.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        foreach (var batch in SchemaBatches())
        {
            await using var cmd = new SqlCommand(batch, conn) { CommandTimeout = 120 };
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await SeedDemoDataAsync(conn, cancellationToken);

        logger.LogInformation("Database schema and demo seed verified.");
    }

    private static async Task EnsureDatabaseExistsAsync(string connectionString, CancellationToken cancellationToken)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
            throw new InvalidOperationException($"{Database.ConnectionStringPath} must set Database or Initial Catalog.");

        var catalog = Database.TryGetCanonicalCatalog(builder.InitialCatalog)
            ?? throw new InvalidOperationException(
                $"{Database.ConnectionStringPath} Initial Catalog must be '{Database.ApplicationCatalog}' " +
                $"or '{Database.IntegrationTestCatalog}'. Current: {builder.InitialCatalog}");

        builder.InitialCatalog = "master";

        await using var master = new SqlConnection(builder.ConnectionString);
        await master.OpenAsync(cancellationToken);

        // Fixed identifiers only (CleanApi / CleanApiTests) — avoids dynamic SQL / QUOTENAME edge cases on SQL Server.
        var createBatch = catalog == Database.ApplicationCatalog
            ? """
              IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CleanApi')
                  CREATE DATABASE [CleanApi];
              """
            : """
              IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CleanApiTests')
                  CREATE DATABASE [CleanApiTests];
              """;

        await using var cmd = new SqlCommand(createBatch, master) { CommandTimeout = 120 };
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> RowExistsAsync(SqlConnection conn, string sql, Action<SqlParameterCollection> bind, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(sql, conn);
        bind(cmd.Parameters);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is not null;
    }

    private static IEnumerable<string> SchemaBatches()
    {
        yield return """
            IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Users (
                    Id uniqueidentifier NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
                    Email nvarchar(256) NOT NULL,
                    PasswordHash nvarchar(max) NOT NULL,
                    DisplayName nvarchar(200) NOT NULL,
                    CreatedAtUtc datetime2 NOT NULL CONSTRAINT DF_Users_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CONSTRAINT UQ_Users_Email UNIQUE (Email)
                );
            END
            """;

        yield return """
            IF OBJECT_ID(N'dbo.Items', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Items (
                    Id uniqueidentifier NOT NULL CONSTRAINT PK_Items PRIMARY KEY,
                    Title nvarchar(200) NOT NULL,
                    Description nvarchar(max) NOT NULL,
                    CreatedAtUtc datetime2 NOT NULL CONSTRAINT DF_Items_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    OwnerUserId uniqueidentifier NULL,
                    CONSTRAINT FK_Items_Users_Owner FOREIGN KEY (OwnerUserId) REFERENCES dbo.Users(Id)
                );
            END
            """;
    }

    private static async Task SeedDemoDataAsync(SqlConnection conn, CancellationToken cancellationToken)
    {
        const string demoEmail = "demo@example.com";

        var demoUserExists = await RowExistsAsync(
            conn,
            "SELECT 1 FROM dbo.Users WHERE Email = @Email;",
            p => p.AddWithValue("@Email", demoEmail),
            cancellationToken);

        if (!demoUserExists)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword("Demo#123");

            await using var insertUser = new SqlCommand(
                """
                INSERT INTO dbo.Users (Id, Email, PasswordHash, DisplayName, CreatedAtUtc)
                VALUES (@Id, @Email, @PasswordHash, @DisplayName, SYSUTCDATETIME());
                """,
                conn);

            insertUser.Parameters.AddWithValue("@Id", DemoUserId);
            insertUser.Parameters.AddWithValue("@Email", demoEmail);
            insertUser.Parameters.AddWithValue("@PasswordHash", hash);
            insertUser.Parameters.AddWithValue("@DisplayName", "Demo User");
            await insertUser.ExecuteNonQueryAsync(cancellationToken);
        }

        var demoItemExists = await RowExistsAsync(
            conn,
            "SELECT 1 FROM dbo.Items WHERE Id = @Id;",
            p => p.AddWithValue("@Id", DemoItemId),
            cancellationToken);

        if (!demoItemExists)
        {
            await using var insertItem = new SqlCommand(
                """
                INSERT INTO dbo.Items (Id, Title, Description, CreatedAtUtc, OwnerUserId)
                VALUES (@Id, @Title, @Description, SYSUTCDATETIME(), @OwnerUserId);
                """,
                conn);

            insertItem.Parameters.AddWithValue("@Id", DemoItemId);
            insertItem.Parameters.AddWithValue("@Title", "Seeded item");
            insertItem.Parameters.AddWithValue("@Description", "This row ships with the database for demo purposes.");
            insertItem.Parameters.AddWithValue("@OwnerUserId", DemoUserId);
            await insertItem.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
