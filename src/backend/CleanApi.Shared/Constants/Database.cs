namespace CleanApi.Shared.Constants;

/// <summary>Configuration key names and allowed SQL Server catalog names for the app and tests.</summary>
public static class Database
{
    public const string SectionName = "Database";

    public const string ConnectionStringKey = "ConnectionString";

    public static string ConnectionStringPath => $"{SectionName}:{ConnectionStringKey}";

    /// <summary>Default app catalog (local dev / production connection strings).</summary>
    public const string ApplicationCatalog = "CleanApi";

    /// <summary>Catalog used by integration tests.</summary>
    public const string IntegrationTestCatalog = "CleanApiTests";

    /// <summary>Resolves <paramref name="initialCatalog"/> to the canonical name, or <c>null</c> if not allowed.</summary>
    public static string? TryGetCanonicalCatalog(string? initialCatalog)
    {
        if (string.IsNullOrWhiteSpace(initialCatalog))
            return null;
        if (string.Equals(initialCatalog, ApplicationCatalog, StringComparison.OrdinalIgnoreCase))
            return ApplicationCatalog;
        if (string.Equals(initialCatalog, IntegrationTestCatalog, StringComparison.OrdinalIgnoreCase))
            return IntegrationTestCatalog;
        return null;
    }
}
