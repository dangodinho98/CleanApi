using CleanApi.Application.Abstractions;
using CleanApi.Infrastructure.Data;
using CleanApi.Infrastructure.Persistence;
using CleanApi.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace CleanApi.Infrastructure;

/// <summary>Registers infrastructure implementations (SQL repositories, JWT, password hashing, initializer).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<IItemRepository, SqlServerItemRepository>();
        services.AddScoped<IUserRepository, SqlServerUserRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>();
        return services;
    }
}
