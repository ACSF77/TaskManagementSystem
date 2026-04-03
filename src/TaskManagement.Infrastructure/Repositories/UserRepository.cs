using Microsoft.Data.Sqlite;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public UserRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Username, Email, PasswordHash, CreatedAt FROM Users WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id.ToString());

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapUser(reader);

        return null;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Username, Email, PasswordHash, CreatedAt FROM Users WHERE Username = @Username COLLATE NOCASE";
        command.Parameters.AddWithValue("@Username", username);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapUser(reader);

        return null;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Username, Email, PasswordHash, CreatedAt FROM Users WHERE Email = @Email COLLATE NOCASE";
        command.Parameters.AddWithValue("@Email", email);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapUser(reader);

        return null;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Username, Email, PasswordHash, CreatedAt FROM Users ORDER BY Username";

        var users = new List<User>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            users.Add(MapUser(reader));

        return users;
    }

    public async Task CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Users (Id, Username, Email, PasswordHash, CreatedAt)
            VALUES (@Id, @Username, @Email, @PasswordHash, @CreatedAt)";

        command.Parameters.AddWithValue("@Id", user.Id.ToString());
        command.Parameters.AddWithValue("@Username", user.Username);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt.ToString("o"));

        await command.ExecuteNonQueryAsync();
    }

    private static User MapUser(SqliteDataReader reader)
    {
        return User.FromStorage(
            Guid.Parse(reader.GetString(0)),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            DateTime.Parse(reader.GetString(4)).ToUniversalTime()
        );
    }
}
