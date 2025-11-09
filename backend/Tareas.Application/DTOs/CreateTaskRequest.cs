using System.ComponentModel.DataAnnotations;
using TaskStatus = Tareas.Domain.Enums.TaskStatus;

namespace Tareas.Application.DTOs;

public class CreateTaskRequest
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Pending;
}

