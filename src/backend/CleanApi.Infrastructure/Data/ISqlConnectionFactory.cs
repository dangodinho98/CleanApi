using Microsoft.Data.SqlClient;

namespace CleanApi.Infrastructure.Data;

/// <summary>Factory for ADO.NET connections to the primary SQL Server catalog.</summary>
public interface ISqlConnectionFactory
{
    SqlConnection CreateConnection();

    /// <summary>Configured ADO.NET connection string (same catalog as <see cref="CreateConnection"/>).</summary>
    string ConnectionString { get; }
}
