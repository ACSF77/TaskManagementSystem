using Microsoft.Data.Sqlite;

namespace TaskManagement.Infrastructure.Data;

public interface ISqliteConnectionFactory
{
    SqliteConnection CreateConnection();
}

public class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}
