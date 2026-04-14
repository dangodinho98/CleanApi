using CleanApi.Infrastructure.Configuration;
using CleanApi.Shared.Constants;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace CleanApi.Infrastructure.Data;

/// <summary>Opens <see cref="SqlConnection"/> instances using configured <see cref="DatabaseOptions"/>.</summary>
public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString
            ?? throw new InvalidOperationException($"{Database.ConnectionStringPath} is not configured.");
    }

    public string ConnectionString => _connectionString;

    public SqlConnection CreateConnection() => new(_connectionString);
}
