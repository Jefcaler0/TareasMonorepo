using TaskStatus = Tareas.Domain.Enums.TaskStatus;

namespace Tareas.Application.DTOs;

public record TaskDto(
    int Id,
    string Title,
    string? Description,
    TaskStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);

