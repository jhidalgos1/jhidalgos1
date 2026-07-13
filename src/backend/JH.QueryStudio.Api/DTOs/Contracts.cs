using JH.QueryStudio.Api.Domain;
namespace JH.QueryStudio.Api.DTOs;
public sealed record ConnectionUpsertRequest(string Name,string Server,int Port,string AuthenticationType,string? UserName,string? Password,string? DefaultDatabase,int Timeout,bool Encrypt,bool TrustServerCertificate,string Color,EnvironmentKind Environment,bool IsFavorite);
public sealed record ConnectionResponse(Guid Id,string Name,string Server,int Port,string AuthenticationType,string? UserName,string? DefaultDatabase,int Timeout,bool Encrypt,bool TrustServerCertificate,string Color,EnvironmentKind Environment,bool IsFavorite,DateTimeOffset? LastUsedAt);
public sealed record TestConnectionRequest(ConnectionUpsertRequest Connection);
public sealed record ExecuteQueryRequest(Guid ConnectionId,string? DatabaseName,string Sql,int MaxRows,bool ConfirmedRisk=false);
public sealed record AnalyzeQueryRequest(Guid? ConnectionId,string? DatabaseName,string Sql);
public sealed record SavedQueryRequest(string Name,string Description,string Category,string Tags,string Sql,string? DatabaseName,string Author,bool IsFavorite);
