using CleanApi.Shared;

namespace CleanApi.Infrastructure.Tests.IntegrationTests;

public sealed class SqlServerIntegrationFixture
{
    public string? ConnectionString { get; }

    public bool Skip { get; }

    public SqlServerIntegrationFixture()
    {
        var settings = TestConfigurationBuilder.LoadSqlTestSettings();
        ConnectionString = settings.ConnectionString;
        Skip = settings.Skip;
    }
}

[CollectionDefinition(nameof(SqlServerIntegrationFixture))]
public sealed class SqlServerIntegrationFixtureCollection : ICollectionFixture<SqlServerIntegrationFixture>
{
}
