using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace JH.QueryStudio.Desktop;

internal sealed class LocalHistoryStore
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);

    private bool _initialized;

    public LocalHistoryStore()
    {
        var localApplicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);

        var applicationFolder = Path.Combine(
            localApplicationData,
            "JH Query Studio");

        Directory.CreateDirectory(applicationFolder);

        var databasePath = Path.Combine(
            applicationFolder,
            "jh-query-studio.db");

        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        };

        _connectionString = connectionStringBuilder.ToString();
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _initializationLock.WaitAsync();

        try
        {
            if (_initialized)
            {
                return;
            }

            await using var connection =
                new SqliteConnection(_connectionString);

            await connection.OpenAsync();

            await using var command = connection.CreateCommand();

            command.CommandText =
                """
                CREATE TABLE IF NOT EXISTS QueryHistory
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ExecutedAtUtc TEXT NOT NULL,
                    SqlText TEXT NOT NULL,
                    DatabaseName TEXT NULL,
                    DurationMs INTEGER NOT NULL,
                    RowCount INTEGER NOT NULL,
                    WasSuccessful INTEGER NOT NULL,
                    ErrorMessage TEXT NULL
                );
                """;

            await command.ExecuteNonQueryAsync();

            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task AddAsync(
        string sql,
        string? databaseName,
        long durationMs,
        int rowCount,
        bool wasSuccessful,
        string? errorMessage)
    {
        await InitializeAsync();

        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText =
            """
            INSERT INTO QueryHistory
            (
                ExecutedAtUtc,
                SqlText,
                DatabaseName,
                DurationMs,
                RowCount,
                WasSuccessful,
                ErrorMessage
            )
            VALUES
            (
                $executedAtUtc,
                $sqlText,
                $databaseName,
                $durationMs,
                $rowCount,
                $wasSuccessful,
                $errorMessage
            );
            """;

        command.Parameters.AddWithValue(
            "$executedAtUtc",
            DateTime.UtcNow.ToString("O"));

        command.Parameters.AddWithValue(
            "$sqlText",
            sql);

        command.Parameters.AddWithValue(
            "$databaseName",
            string.IsNullOrWhiteSpace(databaseName)
                ? DBNull.Value
                : databaseName);

        command.Parameters.AddWithValue(
            "$durationMs",
            durationMs);

        command.Parameters.AddWithValue(
            "$rowCount",
            rowCount);

        command.Parameters.AddWithValue(
            "$wasSuccessful",
            wasSuccessful ? 1 : 0);

        command.Parameters.AddWithValue(
            "$errorMessage",
            string.IsNullOrWhiteSpace(errorMessage)
                ? DBNull.Value
                : errorMessage);

        await command.ExecuteNonQueryAsync();
    }
}