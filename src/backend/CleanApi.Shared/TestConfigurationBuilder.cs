using Microsoft.Extensions.Configuration;

namespace CleanApi.Shared;

/// <summary>Loads test <c>appsettings</c> and exposes SQL connection settings for integration tests.</summary>
public sealed class TestConfigurationBuilder
{
    public const string SqlConnectionSettingKey = "ConnectionString";

    private string _basePath = AppContext.BaseDirectory;

    public static TestConfigurationBuilder Create() => new();

    public TestConfigurationBuilder UseBasePath(string basePath)
    {
        _basePath = basePath;
        return this;
    }

    public IConfigurationRoot Build()
    {
        return new ConfigurationBuilder()
            .SetBasePath(_basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    /// <summary>Reads <see cref="SqlConnectionSettingKey"/> after building configuration from the test output folder.</summary>
    public static SqlTestSettings LoadSqlTestSettings(string? basePath = null)
    {
        var b = Create();
        if (basePath is not null)
            b.UseBasePath(basePath);

        var connectionString = b.Build()[SqlConnectionSettingKey];
        return new SqlTestSettings(connectionString, string.IsNullOrWhiteSpace(connectionString));
    }
}

/// <summary>Outcome of resolving the integration-test SQL connection string.</summary>
public readonly record struct SqlTestSettings(string? ConnectionString, bool Skip);
