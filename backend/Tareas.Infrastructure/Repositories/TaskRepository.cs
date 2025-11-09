using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Tareas.Application.Abstractions.Repositories;
using Tareas.Domain.Entities;

namespace Tareas.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly string _connectionString;
    private readonly ILogger<TaskRepository> _logger;

    public TaskRepository(string connectionString, ILogger<TaskRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = CreateConnection();
            return await connection.QueryAsync<TaskItem>(
                "todo.spTasks_GetAll",
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tasks");
            throw;
        }
    }

    public async Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<TaskItem>(
                "todo.spTasks_GetById",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task with id {TaskId}", id);
            throw;
        }
    }

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Title", task.Title);
            parameters.Add("@Description", task.Description);
            parameters.Add("@Status", task.Status);
            parameters.Add("@CreatedAt", task.CreatedAt);
            parameters.Add("@UpdatedAt", task.UpdatedAt);
            parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "todo.spTasks_Insert",
                parameters,
                commandType: CommandType.StoredProcedure);

            task.Id = parameters.Get<int>("@NewId");
            return task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating a new task");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                "todo.spTasks_Update",
                new
                {
                    task.Id,
                    task.Title,
                    task.Description,
                    task.Status,
                    task.UpdatedAt
                },
                commandType: CommandType.StoredProcedure);

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task with id {TaskId}", task.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                "todo.spTasks_Delete",
                new { Id = id },
                commandType: CommandType.StoredProcedure);

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task with id {TaskId}", id);
            throw;
        }
    }

    private SqlConnection CreateConnection() => new(_connectionString);
}

