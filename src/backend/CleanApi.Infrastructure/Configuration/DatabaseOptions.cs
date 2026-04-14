using CleanApi.Shared.Constants;

namespace CleanApi.Infrastructure.Configuration;

/// <summary>Binds the <c>Database</c> configuration section (SQL connection string).</summary>
public sealed class DatabaseOptions
{
    public const string SectionName = Database.SectionName;

    public string ConnectionString { get; set; } = string.Empty;
}
