using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace EdgeCollector.Data;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=edge_buffer.db";
    }

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS PendingMessages (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Payload TEXT NOT NULL,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        ";
        command.ExecuteNonQuery();
    }
}
