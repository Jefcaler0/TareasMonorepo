using Microsoft.Extensions.DependencyInjection;
using Tareas.Application.Abstractions.Services;
using Tareas.Application.Services;

namespace Tareas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITaskService, TaskService>();
        return services;
    }
}

