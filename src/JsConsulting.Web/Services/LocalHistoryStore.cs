using JsConsulting.Web.Models;
using Microsoft.Data.Sqlite;

namespace JsConsulting.Web.Services;

public sealed class LocalHistoryStore(IConfiguration configuration)
{
    private string DatabasePath => configuration["JsConsulting:LocalDatabase"] ?? "Data/jsconsulting.db";

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        await using var connection = new SqliteConnection($"Data Source={DatabasePath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS QueryHistory(
                Id TEXT PRIMARY KEY,
                DatabaseName TEXT NOT NULL,
                Sql TEXT NOT NULL,
                DurationMs INTEGER NOT NULL,
                RowCount INTEGER NOT NULL,
                Success INTEGER NOT NULL,
                Error TEXT NULL,
                ExecutedAt TEXT NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync();
    }

    public async Task AddAsync(string databaseName, string sql, long durationMs, int rowCount, bool success, string? error)
    {
        await using var connection = new SqliteConnection($"Data Source={DatabasePath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO QueryHistory(Id, DatabaseName, Sql, DurationMs, RowCount, Success, Error, ExecutedAt)
            VALUES ($id, $databaseName, $sql, $durationMs, $rowCount, $success, $error, $executedAt);
            """;
        command.Parameters.AddWithValue("$id", Guid.NewGuid().ToString("N"));
        command.Parameters.AddWithValue("$databaseName", databaseName);
        command.Parameters.AddWithValue("$sql", sql);
        command.Parameters.AddWithValue("$durationMs", durationMs);
        command.Parameters.AddWithValue("$rowCount", rowCount);
        command.Parameters.AddWithValue("$success", success ? 1 : 0);
        command.Parameters.AddWithValue("$error", (object?)error ?? DBNull.Value);
        command.Parameters.AddWithValue("$executedAt", DateTimeOffset.UtcNow.ToString("O"));
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<HistoryItem>> ListAsync(int limit = 50)
    {
        var items = new List<HistoryItem>();
        await using var connection = new SqliteConnection($"Data Source={DatabasePath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, DatabaseName, Sql, DurationMs, RowCount, Success, Error, ExecutedAt
            FROM QueryHistory
            ORDER BY ExecutedAt DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", limit);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new HistoryItem(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt64(3),
                reader.GetInt32(4),
                reader.GetInt32(5) == 1,
                reader.IsDBNull(6) ? null : reader.GetString(6),
                DateTimeOffset.Parse(reader.GetString(7))));
        }

        return items;
    }
}
