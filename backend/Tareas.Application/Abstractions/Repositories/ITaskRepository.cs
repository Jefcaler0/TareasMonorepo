using Tareas.Domain.Entities;

namespace Tareas.Application.Abstractions.Repositories;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

