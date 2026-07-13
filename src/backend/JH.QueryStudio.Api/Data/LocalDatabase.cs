using Microsoft.Data.Sqlite;
namespace JH.QueryStudio.Api.Data;
public sealed class LocalDatabase(IConfiguration config){
 public string ConnectionString {get;} = new SqliteConnectionStringBuilder{DataSource=config["LocalStorage:DatabasePath"]??"Data/jh-query-studio.db"}.ToString();
 public async Task InitializeAsync(){var path=config["LocalStorage:DatabasePath"]??"Data/jh-query-studio.db";Directory.CreateDirectory(Path.GetDirectoryName(path)!);await using var c=new SqliteConnection(ConnectionString);await c.OpenAsync();var sql=await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"Data","schema.sql")).ContinueWith(t=>t.IsCompletedSuccessfully?t.Result:Schema);foreach(var s in sql.Split(";\n",StringSplitOptions.RemoveEmptyEntries))await new SqliteCommand(s,c).ExecuteNonQueryAsync();}
 const string Schema=@"CREATE TABLE IF NOT EXISTS Connections(Id TEXT PRIMARY KEY,Name TEXT NOT NULL,Server TEXT NOT NULL,Port INTEGER NOT NULL,AuthenticationType TEXT NOT NULL,UserName TEXT,PasswordCipher TEXT,DefaultDatabase TEXT,Timeout INTEGER NOT NULL,Encrypt INTEGER NOT NULL,TrustServerCertificate INTEGER NOT NULL,Color TEXT NOT NULL,Environment INTEGER NOT NULL,IsFavorite INTEGER NOT NULL,CreatedAt TEXT NOT NULL,LastUsedAt TEXT);
CREATE TABLE IF NOT EXISTS QueryHistory(Id TEXT PRIMARY KEY,ConnectionId TEXT,DatabaseName TEXT,Sql TEXT NOT NULL,DurationMs INTEGER,RowCount INTEGER,Success INTEGER,Error TEXT,ExecutedAt TEXT,IsFavorite INTEGER);
CREATE TABLE IF NOT EXISTS SavedQueries(Id TEXT PRIMARY KEY,Name TEXT,Description TEXT,Category TEXT,Tags TEXT,Sql TEXT,DatabaseName TEXT,Author TEXT,IsFavorite INTEGER,CreatedAt TEXT,UpdatedAt TEXT);
CREATE TABLE IF NOT EXISTS Snippets(Id TEXT PRIMARY KEY,Name TEXT,Prefix TEXT,Description TEXT,Category TEXT,BodyJson TEXT,IsEnabled INTEGER,IsBuiltIn INTEGER);";
}
