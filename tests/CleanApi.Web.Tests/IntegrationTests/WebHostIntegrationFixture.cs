using CleanApi.Shared;
using CleanApi.Web.Tests.IntegrationTests.Builders;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CleanApi.Web.Tests.IntegrationTests;

public sealed class WebHostIntegrationFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program>? Factory { get; private set; }

    public bool Skip { get; private set; }

    public async Task InitializeAsync()
    {
        var settings = TestConfigurationBuilder.LoadSqlTestSettings();
        if (settings.Skip)
        {
            Skip = true;
            return;
        }

        Factory = new WebApplicationTestHostBuilder()
            .WithTestSqlConnection(settings.ConnectionString!)
            .Build();

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
            Factory = null;
        }
    }
}

[CollectionDefinition(nameof(WebHostIntegrationFixture))]
public sealed class WebHostIntegrationFixtureCollection : ICollectionFixture<WebHostIntegrationFixture>
{
}
