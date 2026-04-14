using CleanApi.Shared.Constants;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CleanApi.Web.Tests.IntegrationTests.Builders;

public sealed class WebApplicationTestHostBuilder
{
    private string? _connectionString;

    public WebApplicationTestHostBuilder WithTestSqlConnection(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    public WebApplicationFactory<Program> Build()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_connectionString);

        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting(Database.ConnectionStringPath, _connectionString);
            builder.UseSetting("Jwt:Issuer", "integration");
            builder.UseSetting("Jwt:Audience", "integration");
            builder.UseSetting("Jwt:SigningKey", new string('z', 48));
            builder.UseSetting("Jwt:ExpirationMinutes", "120");
        });
    }
}
