using CleanApi.Infrastructure.Configuration;
using CleanApi.Infrastructure.Data;
using CleanApi.Infrastructure.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CleanApi.Infrastructure.Tests.IntegrationTests.Builders;

public sealed class SqlIntegrationContextBuilder
{
    private string? _connectionString;

    public SqlIntegrationContextBuilder WithConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    public async Task<SqlIntegrationContext> BuildAsync(CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_connectionString);

        var options = Options.Create(new DatabaseOptions { ConnectionString = _connectionString });
        var factory = new SqlConnectionFactory(options);
        var initializer = new DatabaseInitializer(factory, NullLogger<DatabaseInitializer>.Instance);
        await initializer.InitializeAsync(cancellationToken);

        return new SqlIntegrationContext(factory);
    }
}

public sealed class SqlIntegrationContext(SqlConnectionFactory factory)
{
    public SqlConnectionFactory Factory { get; } = factory;

    public SqlServerItemRepository Items { get; } = new SqlServerItemRepository(factory);

    public SqlServerUserRepository Users { get; } = new SqlServerUserRepository(factory);
}
