using System.Data;
using System.Diagnostics;
using JsConsulting.Core.Security;
using JsConsulting.Web.Models;
using Microsoft.Data.SqlClient;

namespace JsConsulting.Web.Services;

public sealed class SqlWorkspaceService(LocalHistoryStore historyStore)
{
    public async Task<string> TestAsync(ConnectionInput input, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(BuildConnectionString(input));
        await connection.OpenAsync(cancellationToken);
        return $"Conectado a {input.Server}/{input.Database}";
    }

    public async Task<IReadOnlyList<ObjectNode>> LoadObjectsAsync(ConnectionInput input, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(BuildConnectionString(input));
        await connection.OpenAsync(cancellationToken);
        const string sql = """
            SELECT s.name AS SchemaName, o.name AS ObjectName, o.type_desc AS ObjectType, c.name AS ColumnName
            FROM sys.objects o
            INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
            LEFT JOIN sys.columns c ON c.object_id = o.object_id
            WHERE o.type IN ('U', 'V', 'P', 'FN', 'IF', 'TF', 'TR')
            ORDER BY s.name, o.name, c.column_id;
            """;
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var schemas = new Dictionary<string, Dictionary<string, (string Kind, List<string> Columns)>>(StringComparer.OrdinalIgnoreCase);

        while (await reader.ReadAsync(cancellationToken))
        {
            var schema = reader.GetString(0);
            var objectName = reader.GetString(1);
            var kind = NormalizeKind(reader.GetString(2));
            var column = reader.IsDBNull(3) ? null : reader.GetString(3);
            if (!schemas.TryGetValue(schema, out var objects))
            {
                objects = new(StringComparer.OrdinalIgnoreCase);
                schemas[schema] = objects;
            }

            if (!objects.TryGetValue(objectName, out var entry))
            {
                entry = (kind, []);
                objects[objectName] = entry;
            }

            if (!string.IsNullOrWhiteSpace(column))
            {
                entry.Columns.Add(column);
            }
        }

        return schemas.Select(schema => new ObjectNode(
            schema.Key,
            "schema",
            schema.Key,
            schema.Value.Select(obj => new ObjectNode(
                obj.Key,
                obj.Value.Kind,
                $"{schema.Key}.{obj.Key}",
                obj.Value.Columns.Distinct(StringComparer.OrdinalIgnoreCase).Select(col => new ObjectNode(col, "column", $"{schema.Key}.{obj.Key}.{col}", [])).ToList())).ToList())).ToList();
    }

    public async Task<QueryResponse> ExecuteAsync(QueryRequest request, CancellationToken cancellationToken)
    {
        var risks = QuerySafetyAnalyzer.Analyze(request.Sql);
        if (risks.Count > 0 && !request.ConfirmRisk)
        {
            return new(false, "Consulta bloqueada por seguridad. Confirme para ejecutar.", 0, 0, 0, [], [], risks);
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await using var connection = new SqlConnection(BuildConnectionString(request.Connection));
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(request.Sql, connection) { CommandTimeout = request.Connection.Timeout };
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var table = new DataTable();
            table.Load(reader);
            var columns = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            var rows = table.Rows.Cast<DataRow>()
                .Take(Math.Max(1, request.MaxRows))
                .Select(row => columns.ToDictionary(col => col, col => row[col] is DBNull ? null : row[col]))
                .ToList();
            stopwatch.Stop();
            await historyStore.AddAsync(request.Connection.Database, request.Sql, stopwatch.ElapsedMilliseconds, rows.Count, true, null);
            return new(true, "Consulta ejecutada correctamente.", stopwatch.ElapsedMilliseconds, rows.Count, reader.RecordsAffected, columns, rows, risks);
        }
        catch (SqlException ex)
        {
            stopwatch.Stop();
            await historyStore.AddAsync(request.Connection.Database, request.Sql, stopwatch.ElapsedMilliseconds, 0, false, ex.Message);
            return new(false, $"Error SQL línea {ex.LineNumber}: {ex.Message}", stopwatch.ElapsedMilliseconds, 0, 0, [], [], risks);
        }
    }

    private static string BuildConnectionString(ConnectionInput input)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = input.Server,
            InitialCatalog = string.IsNullOrWhiteSpace(input.Database) ? "master" : input.Database,
            UserID = input.User,
            Password = input.Password,
            Encrypt = input.Encrypt,
            TrustServerCertificate = input.TrustCertificate,
            ConnectTimeout = input.Timeout
        };
        return builder.ConnectionString;
    }

    private static string NormalizeKind(string kind) => kind switch
    {
        "USER_TABLE" => "table",
        "VIEW" => "view",
        "SQL_STORED_PROCEDURE" => "procedure",
        "SQL_SCALAR_FUNCTION" or "SQL_INLINE_TABLE_VALUED_FUNCTION" or "SQL_TABLE_VALUED_FUNCTION" => "function",
        "SQL_TRIGGER" => "trigger",
        _ => "object"
    };
}
