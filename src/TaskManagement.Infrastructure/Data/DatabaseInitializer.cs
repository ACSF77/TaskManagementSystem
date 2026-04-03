using Microsoft.Data.Sqlite;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Data;

public class DatabaseInitializer
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public DatabaseInitializer(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY NOT NULL,
                Username TEXT NOT NULL UNIQUE,
                Email TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS Tasks (
                Id TEXT PRIMARY KEY NOT NULL,
                Title TEXT NOT NULL,
                Description TEXT,
                Status INTEGER NOT NULL DEFAULT 0,
                DueDate TEXT NOT NULL,
                AssignedUserId TEXT,
                CreatedByUserId TEXT NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                UpdatedAt TEXT,
                FOREIGN KEY (AssignedUserId) REFERENCES Users(Id),
                FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
            );

            CREATE INDEX IF NOT EXISTS IX_Tasks_AssignedUserId ON Tasks(AssignedUserId);
            CREATE INDEX IF NOT EXISTS IX_Tasks_CreatedByUserId ON Tasks(CreatedByUserId);
            CREATE INDEX IF NOT EXISTS IX_Tasks_Status ON Tasks(Status);";

        await command.ExecuteNonQueryAsync();
    }

    public async Task SeedAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        // Check if data already exists
        using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = "SELECT COUNT(*) FROM Users";
        var count = (long)(await checkCommand.ExecuteScalarAsync())!;

        if (count == 0)
        {
            await SeedUsersAsync(connection);
            await SeedTasksAsync(connection);
        }
    }

    private async Task SeedUsersAsync(SqliteConnection connection)
    {
        var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", workFactor: 11);
        var johnHash = BCrypt.Net.BCrypt.HashPassword("John123!", workFactor: 11);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Users (Id, Username, Email, PasswordHash, CreatedAt) VALUES
            ('a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'admin', 'admin@taskboard.com', @AdminHash, '2026-01-01T00:00:00Z');

            INSERT INTO Users (Id, Username, Email, PasswordHash, CreatedAt) VALUES
            ('b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 'johndoe', 'john@taskboard.com', @JohnHash, '2026-01-15T00:00:00Z');";

        command.Parameters.AddWithValue("@AdminHash", adminHash);
        command.Parameters.AddWithValue("@JohnHash", johnHash);

        await command.ExecuteNonQueryAsync();
    }

    private async Task SeedTasksAsync(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Tasks (Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt) VALUES
            ('c0000001-0000-4000-8000-000000000001', 'Set up project repository', 'Initialize the Git repository and configure CI/CD pipeline', 2, '2026-04-10T00:00:00Z', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', '2026-03-01T00:00:00Z');

            INSERT INTO Tasks (Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt) VALUES
            ('c0000001-0000-4000-8000-000000000002', 'Design database schema', 'Create the ERD and define all tables and relationships', 2, '2026-04-12T00:00:00Z', 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', '2026-03-05T00:00:00Z');

            INSERT INTO Tasks (Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt) VALUES
            ('c0000001-0000-4000-8000-000000000003', 'Implement user authentication', 'Build JWT-based login and registration endpoints', 1, '2026-04-20T00:00:00Z', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', '2026-03-10T00:00:00Z');

            INSERT INTO Tasks (Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt) VALUES
            ('c0000001-0000-4000-8000-000000000004', 'Build task CRUD API', 'Create REST endpoints for task management with full CRUD support', 1, '2026-04-25T00:00:00Z', 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', '2026-03-12T00:00:00Z');

            INSERT INTO Tasks (Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt) VALUES
            ('c0000001-0000-4000-8000-000000000005', 'Create React frontend', 'Build responsive task board UI with Kanban columns', 0, '2026-05-01T00:00:00Z', 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', '2026-03-15T00:00:00Z');

            INSERT INTO Tasks (Id, Title, Description, Status, DueDate, AssignedUserId, CreatedByUserId, CreatedAt) VALUES
            ('c0000001-0000-4000-8000-000000000006', 'Write unit and integration tests', 'Achieve comprehensive test coverage across all layers', 0, '2026-05-05T00:00:00Z', NULL, 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', '2026-03-20T00:00:00Z');";

        await command.ExecuteNonQueryAsync();
    }
}
