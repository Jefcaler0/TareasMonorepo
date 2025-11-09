using Tareas.Application.DTOs;

namespace Tareas.Application.Abstractions.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TaskDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskDto?> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

