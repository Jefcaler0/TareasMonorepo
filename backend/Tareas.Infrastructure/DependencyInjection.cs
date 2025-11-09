using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tareas.Application.Abstractions.Repositories;
using Tareas.Infrastructure.Repositories;

namespace Tareas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = ResolveConnectionString(configuration);

        services.AddScoped<ITaskRepository>(provider =>
        {
            var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TaskRepository>>();
            return new TaskRepository(connectionString, logger);
        });

        return services;
    }

    private static string ResolveConnectionString(IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        connectionString = configuration.GetConnectionString("DefaultConnection") ?? configuration["Database:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured. Set the DB_CONNECTION_STRING environment variable or provide ConnectionStrings:DefaultConnection in configuration.");
        }

        return connectionString;
    }
}

