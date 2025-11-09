using Tareas.Application.Abstractions.Repositories;
using Tareas.Application.Abstractions.Services;
using Tareas.Application.DTOs;
using Tareas.Domain.Entities;

namespace Tareas.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<TaskDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllAsync(cancellationToken);
        return tasks.Select(MapToDto);
    }

    public async Task<TaskDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        return task is null ? null : MapToDto(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _taskRepository.CreateAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<TaskDto?> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Title = request.Title;
        existing.Description = request.Description;
        existing.Status = request.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        var success = await _taskRepository.UpdateAsync(existing, cancellationToken);
        return success ? MapToDto(existing) : null;
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        _taskRepository.DeleteAsync(id, cancellationToken);

    private static TaskDto MapToDto(TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.Status, task.CreatedAt, task.UpdatedAt);
}

