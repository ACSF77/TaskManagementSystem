using FluentAssertions;
using Microsoft.Data.Sqlite;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;

namespace TaskManagement.Infrastructure.Tests;

public class UserRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SqliteConnectionFactory _factory;
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        // Use a shared in-memory database
        _connection = new SqliteConnection("Data Source=UserRepoTests;Mode=Memory;Cache=Shared");
        _connection.Open();

        _factory = new SqliteConnectionFactory("Data Source=UserRepoTests;Mode=Memory;Cache=Shared");

        InitializeDatabase();
        _sut = new UserRepository(_factory);
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
            )";
        cmd.ExecuteNonQuery();
    }

    [Fact]
    public async Task CreateAsync_ThenGetById_ShouldReturnUser()
    {
        var user = User.Create("testuser", "test@example.com", "hashedpw");

        await _sut.CreateAsync(user);
        var result = await _sut.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByUsernameAsync_WithExistingUser_ShouldReturn()
    {
        var user = User.Create("findme", "find@example.com", "hash");
        await _sut.CreateAsync(user);

        var result = await _sut.GetByUsernameAsync("findme");

        result.Should().NotBeNull();
        result!.Username.Should().Be("findme");
    }

    [Fact]
    public async Task GetByUsernameAsync_CaseInsensitive_ShouldReturn()
    {
        var user = User.Create("CaseUser", "case@example.com", "hash");
        await _sut.CreateAsync(user);

        var result = await _sut.GetByUsernameAsync("caseuser");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingUser_ShouldReturn()
    {
        var user = User.Create("emailuser", "email@example.com", "hash");
        await _sut.CreateAsync(user);

        var result = await _sut.GetByEmailAsync("email@example.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("email@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExisting_ShouldReturnNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        var user1 = User.Create("alluser1", "all1@example.com", "hash");
        var user2 = User.Create("alluser2", "all2@example.com", "hash");
        await _sut.CreateAsync(user1);
        await _sut.CreateAsync(user2);

        var result = await _sut.GetAllAsync();

        result.Should().Contain(u => u.Username == "alluser1");
        result.Should().Contain(u => u.Username == "alluser2");
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
