namespace JH.QueryStudio.Api.Domain;
public enum EnvironmentKind { Development, Quality, Production }
public sealed record ConnectionProfile(Guid Id,string Name,string Server,int Port,string AuthenticationType,string? UserName,string? PasswordCipher,string? DefaultDatabase,int Timeout,bool Encrypt,bool TrustServerCertificate,string Color,EnvironmentKind Environment,bool IsFavorite,DateTimeOffset CreatedAt,DateTimeOffset? LastUsedAt);
public sealed record DbObjectNode(string Id,string Name,string Type,string? Schema,IReadOnlyList<DbObjectNode> Children);
public sealed record QueryResultSet(IReadOnlyList<string> Columns,IReadOnlyList<Dictionary<string,object?>> Rows,int RowCount);
public sealed record QueryExecutionResult(Guid HistoryId,bool Success,long DurationMs,int RowsAffected,IReadOnlyList<QueryResultSet> ResultSets,IReadOnlyList<string> Messages,string? Error,int? ErrorLine);
public sealed record RiskFinding(string Code,string Severity,string Message);
public sealed record QueryHistory(Guid Id,Guid ConnectionId,string DatabaseName,string Sql,long DurationMs,int RowCount,bool Success,string? Error,DateTimeOffset ExecutedAt,bool IsFavorite);
public sealed record SavedQuery(Guid Id,string Name,string Description,string Category,string Tags,string Sql,string? DatabaseName,string Author,bool IsFavorite,DateTimeOffset CreatedAt,DateTimeOffset UpdatedAt);
public sealed record SqlSnippet(Guid Id,string Name,string Prefix,string Description,string Category,string BodyJson,bool IsEnabled,bool IsBuiltIn);
