using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using Tareas.Application.Abstractions.Services;
using Tareas.Application.DTOs;

namespace Tareas.API.Endpoints;

public static class TaskEndpoints
{
    public static RouteGroupBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks");
        group.WithTags("Tasks");

        group.MapGet("/", GetAllTasks)
            .WithName("GetTasks")
            .Produces<IEnumerable<TaskDto>>(StatusCodes.Status200OK)
            .WithOpenApi();

        group.MapGet("/{id:int}", GetTaskById)
            .WithName("GetTaskById")
            .Produces<TaskDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        group.MapPost("/", CreateTask)
            .WithName("CreateTask")
            .Produces<TaskDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithOpenApi();

        group.MapPut("/{id:int}", UpdateTask)
            .WithName("UpdateTask")
            .Produces<TaskDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .WithOpenApi();

        group.MapDelete("/{id:int}", DeleteTask)
            .WithName("DeleteTask")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return group;
    }

    private static async Task<Ok<IEnumerable<TaskDto>>> GetAllTasks(
        ITaskService service,
        [FromServices] ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(TaskEndpoints));
        logger.LogInformation("Fetching all tasks");
        var tasks = await service.GetAllAsync(cancellationToken);
        logger.LogInformation("Fetched {Count} tasks", tasks.Count());
        return TypedResults.Ok(tasks);
    }

    private static async Task<Results<Ok<TaskDto>, NotFound>> GetTaskById(
        int id,
        ITaskService service,
        [FromServices] ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(TaskEndpoints));
        logger.LogInformation("Fetching task {TaskId}", id);
        var task = await service.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            logger.LogWarning("Task {TaskId} not found", id);
            return TypedResults.NotFound();
        }

        logger.LogInformation("Task {TaskId} fetched successfully", id);
        return TypedResults.Ok(task);
    }

    private static async Task<Results<Created<TaskDto>, ValidationProblem>> CreateTask(
        CreateTaskRequest request,
        ITaskService service,
        [FromServices] ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(TaskEndpoints));
        var validationErrors = ValidateCreateOrUpdate(request.Title, request.Description);
        if (validationErrors is not null)
        {
            logger.LogWarning("Validation failed when creating task: {@Errors}", validationErrors);
            return TypedResults.ValidationProblem(validationErrors);
        }

        var created = await service.CreateAsync(request, cancellationToken);
        logger.LogInformation("Task {TaskId} created successfully", created.Id);
        return TypedResults.Created($"/api/tasks/{created.Id}", created);
    }

    private static async Task<Results<Ok<TaskDto>, NotFound, ValidationProblem>> UpdateTask(
        int id,
        UpdateTaskRequest request,
        ITaskService service,
        [FromServices] ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(TaskEndpoints));
        var validationErrors = ValidateCreateOrUpdate(request.Title, request.Description);
        if (validationErrors is not null)
        {
            logger.LogWarning("Validation failed when updating task {TaskId}: {@Errors}", id, validationErrors);
            return TypedResults.ValidationProblem(validationErrors);
        }

        var updated = await service.UpdateAsync(id, request, cancellationToken);
        if (updated is null)
        {
            logger.LogWarning("Task {TaskId} not found for update", id);
            return TypedResults.NotFound();
        }

        logger.LogInformation("Task {TaskId} updated successfully", id);
        return TypedResults.Ok(updated);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteTask(
        int id,
        ITaskService service,
        [FromServices] ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(TaskEndpoints));
        var deleted = await service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            logger.LogWarning("Task {TaskId} not found for deletion", id);
            return TypedResults.NotFound();
        }

        logger.LogInformation("Task {TaskId} deleted successfully", id);
        return TypedResults.NoContent();
    }

    private static Dictionary<string, string[]>? ValidateCreateOrUpdate(string title, string? description)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(title))
        {
            errors["title"] = new[] { "El título es obligatorio." };
        }
        else if (title.Length > 100)
        {
            errors["title"] = new[] { "El título no debe superar los 100 caracteres." };
        }

        if (!string.IsNullOrEmpty(description) && description.Length > 500)
        {
            errors["description"] = new[] { "La descripción no debe superar los 500 caracteres." };
        }

        return errors.Count == 0 ? null : errors;
    }
}

