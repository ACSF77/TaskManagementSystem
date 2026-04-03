using Microsoft.Data.Sqlite;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public TaskRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt, UpdatedAt
            FROM Tasks WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id.ToString());

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapTask(reader);

        return null;
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt, UpdatedAt
            FROM Tasks ORDER BY CreatedAt DESC";

        var tasks = new List<TaskItem>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            tasks.Add(MapTask(reader));

        return tasks;
    }

    public async Task CreateAsync(TaskItem task)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Tasks (Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt, UpdatedAt)
            VALUES (@Id, @Title, @Description, @Status, @DueDate, @AssignedUserId, @CreatedByUserId, @CreatedAt, @UpdatedAt)";

        command.Parameters.AddWithValue("@Id", task.Id.ToString());
        command.Parameters.AddWithValue("@Title", task.Title);
        command.Parameters.AddWithValue("@Description", (object?)task.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", (int)task.Status);
        command.Parameters.AddWithValue("@DueDate", task.DueDate.ToString("o"));
        command.Parameters.AddWithValue("@AssignedUserId", task.AssignedUserId.HasValue ? task.AssignedUserId.Value.ToString() : DBNull.Value);
        command.Parameters.AddWithValue("@CreatedByUserId", task.CreatedByUserId.ToString());
        command.Parameters.AddWithValue("@CreatedAt", task.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("@UpdatedAt", task.UpdatedAt.HasValue ? task.UpdatedAt.Value.ToString("o") : DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(TaskItem task)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Tasks SET
                Title = @Title,
                Description = @Description,
                Status = @Status,
                DueDate = @DueDate,
                AssignedUserId = @AssignedUserId,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        command.Parameters.AddWithValue("@Id", task.Id.ToString());
        command.Parameters.AddWithValue("@Title", task.Title);
        command.Parameters.AddWithValue("@Description", (object?)task.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", (int)task.Status);
        command.Parameters.AddWithValue("@DueDate", task.DueDate.ToString("o"));
        command.Parameters.AddWithValue("@AssignedUserId", task.AssignedUserId.HasValue ? task.AssignedUserId.Value.ToString() : DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedAt", task.UpdatedAt.HasValue ? task.UpdatedAt.Value.ToString("o") : DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Tasks WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id.ToString());

        await command.ExecuteNonQueryAsync();
    }

    private static TaskItem MapTask(SqliteDataReader reader)
    {
        return TaskItem.FromStorage(
            Guid.Parse(reader.GetString(0)),
            reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2),
            (TaskItemStatus)reader.GetInt32(3),
            DateTime.Parse(reader.GetString(4)).ToUniversalTime(),
            reader.IsDBNull(5) ? null : Guid.Parse(reader.GetString(5)),
            Guid.Parse(reader.GetString(6)),
            DateTime.Parse(reader.GetString(7)).ToUniversalTime(),
            reader.IsDBNull(8) ? null : DateTime.Parse(reader.GetString(8)).ToUniversalTime()
        );
    }
}
