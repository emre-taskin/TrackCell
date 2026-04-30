using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace EdgeCollector.Data;

public class MessageRepository : IMessageRepository
{
    private readonly string _connectionString;

    public MessageRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=edge_buffer.db";
    }

    public async Task InsertMessageAsync(string payload)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO PendingMessages (Payload) VALUES (@Payload)";
        command.Parameters.AddWithValue("@Payload", payload);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<(long Id, string Payload)?> GetNextMessageAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Payload FROM PendingMessages ORDER BY Id ASC LIMIT 1";

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (reader.GetInt64(0), reader.GetString(1));
        }

        return null;
    }

    public async Task DeleteMessageAsync(long id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM PendingMessages WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync();
    }
}
