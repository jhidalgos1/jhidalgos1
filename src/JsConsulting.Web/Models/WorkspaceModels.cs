using System.Data;

namespace JsConsulting.Web.Models;

public sealed record ConnectionInput(
    string Server,
    string Database,
    string User,
    string Password,
    bool Encrypt,
    bool TrustCertificate,
    int Timeout = 30);

public sealed record QueryRequest(ConnectionInput Connection, string Sql, int MaxRows = 500, bool ConfirmRisk = false);

public sealed record ObjectNode(string Name, string Kind, string FullName, IReadOnlyList<ObjectNode> Children);

public sealed record QueryResponse(
    bool Success,
    string Message,
    long DurationMs,
    int RowCount,
    int RowsAffected,
    IReadOnlyList<string> Columns,
    IReadOnlyList<Dictionary<string, object?>> Rows,
    IReadOnlyList<string> Risks);

public sealed record HistoryItem(string Id, string Database, string Sql, long DurationMs, int RowCount, bool Success, string? Error, DateTimeOffset ExecutedAt);
