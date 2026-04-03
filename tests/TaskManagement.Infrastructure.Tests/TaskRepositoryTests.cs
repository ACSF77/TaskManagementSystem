using FluentAssertions;
using Microsoft.Data.Sqlite;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;

namespace TaskManagement.Infrastructure.Tests;

public class TaskRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SqliteConnectionFactory _factory;
    private readonly TaskRepository _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=TaskRepoTests;Mode=Memory;Cache=Shared");
        _connection.Open();

        _factory = new SqliteConnectionFactory("Data Source=TaskRepoTests;Mode=Memory;Cache=Shared");

        InitializeDatabase();
        _sut = new TaskRepository(_factory);
    }

    private void InitializeDatabase()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
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
            )";
        cmd.ExecuteNonQuery();

        // Insert a user for FK integrity
        using var userCmd = _connection.CreateCommand();
        userCmd.CommandText = "INSERT INTO Users (Id, Username, Email, PasswordHash, CreatedAt) VALUES (@Id, 'testuser', 'test@test.com', 'hash', datetime('now'))";
        userCmd.Parameters.AddWithValue("@Id", _userId.ToString());
        userCmd.ExecuteNonQuery();
    }

    [Fact]
    public async Task CreateAsync_ThenGetById_ShouldReturnTask()
    {
        var task = TaskItem.Create("Test Task", "Description", DateTime.UtcNow.AddDays(5), _userId);

        await _sut.CreateAsync(task);
        var result = await _sut.GetByIdAsync(task.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
        result.Title.Should().Be("Test Task");
        result.Description.Should().Be("Description");
        result.Status.Should().Be(TaskItemStatus.Todo);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTasks()
    {
        var task1 = TaskItem.Create("Task 1", null, DateTime.UtcNow.AddDays(1), _userId);
        var task2 = TaskItem.Create("Task 2", null, DateTime.UtcNow.AddDays(2), _userId);
        await _sut.CreateAsync(task1);
        await _sut.CreateAsync(task2);

        var result = await _sut.GetAllAsync();

        result.Should().Contain(t => t.Title == "Task 1");
        result.Should().Contain(t => t.Title == "Task 2");
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var task = TaskItem.Create("Original", "Desc", DateTime.UtcNow.AddDays(5), _userId);
        await _sut.CreateAsync(task);

        task.Update("Updated", "New Desc", TaskItemStatus.InProgress, DateTime.UtcNow.AddDays(10), null);
        await _sut.UpdateAsync(task);

        var result = await _sut.GetByIdAsync(task.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated");
        result.Description.Should().Be("New Desc");
        result.Status.Should().Be(TaskItemStatus.InProgress);
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTask()
    {
        var task = TaskItem.Create("Delete Me", null, DateTime.UtcNow.AddDays(1), _userId);
        await _sut.CreateAsync(task);

        await _sut.DeleteAsync(task.Id);

        var result = await _sut.GetByIdAsync(task.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExisting_ShouldReturnNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithNullDescription_ShouldWork()
    {
        var task = TaskItem.Create("No Desc", null, DateTime.UtcNow.AddDays(1), _userId);

        await _sut.CreateAsync(task);
        var result = await _sut.GetByIdAsync(task.Id);

        result.Should().NotBeNull();
        result!.Description.Should().BeNull();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
