using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using JH.QueryStudio.Core.Security;

namespace JH.QueryStudio.Desktop;

public partial class MainWindow : Window
{
    private readonly LocalHistoryStore _historyStore = new();
    private string? _activeConnectionString;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await _historyStore.InitializeAsync();
        KeyDown += MainWindow_KeyDown;
    }

    private async void TestConnection_Click(object sender, RoutedEventArgs e)
    {
        await RunUiActionAsync("Probando conexión...", async () =>
        {
            await using var connection = new SqlConnection(BuildConnectionString());
            await connection.OpenAsync();
            AppendMessage("Conexión exitosa.");
        });
    }

    private async void Connect_Click(object sender, RoutedEventArgs e)
    {
        await RunUiActionAsync("Conectando...", async () =>
        {
            _activeConnectionString = BuildConnectionString();
            ActiveConnectionText.Text = $"{ServerTextBox.Text} / {DatabaseTextBox.Text}";
            await LoadMetadataAsync();
            AppendMessage("Conexión activa y metadatos cargados.");
        });
    }

    private async void RefreshMetadata_Click(object sender, RoutedEventArgs e)
    {
        await RunUiActionAsync("Actualizando metadatos...", LoadMetadataAsync);
    }

    private async void Execute_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCurrentSqlAsync();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        MessagesTextBox.Clear();
        ResultsGrid.ItemsSource = null;
    }

    private async Task ExecuteCurrentSqlAsync()
    {
        var sql = SqlEditorTextBox.SelectedText.Length > 0 ? SqlEditorTextBox.SelectedText : SqlEditorTextBox.Text;
        if (string.IsNullOrWhiteSpace(sql))
        {
            AppendMessage("No hay SQL para ejecutar.");
            return;
        }

        var risks = QuerySafetyAnalyzer.Analyze(sql);
        if (risks.Count > 0)
        {
            var confirmation = MessageBox.Show(
                this,
                $"Riesgo detectado:\n{string.Join("\n", risks)}\n\nServidor: {ServerTextBox.Text}\nBase: {DatabaseTextBox.Text}\n\n¿Desea ejecutar de todos modos?",
                "Confirmación de seguridad",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                AppendMessage("Ejecución cancelada por seguridad.");
                return;
            }
        }

        await RunUiActionAsync("Ejecutando consulta...", async () =>
        {
            var connectionString = _activeConnectionString ?? BuildConnectionString();
            var stopwatch = Stopwatch.StartNew();
            var table = new DataTable();
            var rowsAffected = 0;

            try
            {
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                await using var command = new SqlCommand(sql, connection) { CommandTimeout = 60 };
                await using var reader = await command.ExecuteReaderAsync();
                table.Load(reader);
                rowsAffected = reader.RecordsAffected;
                ResultsGrid.ItemsSource = table.DefaultView;
                stopwatch.Stop();

                AppendMessage($"Consulta finalizada en {stopwatch.ElapsedMilliseconds} ms. Filas: {table.Rows.Count}. Afectadas: {rowsAffected}.");
                await _historyStore.AddAsync(sql, DatabaseTextBox.Text, stopwatch.ElapsedMilliseconds, table.Rows.Count, true, null);
            }
            catch (SqlException ex)
            {
                stopwatch.Stop();
                AppendMessage($"Error SQL línea {ex.LineNumber}: {ex.Message}");
                await _historyStore.AddAsync(sql, DatabaseTextBox.Text, stopwatch.ElapsedMilliseconds, 0, false, ex.Message);
            }
        });
    }

    private async Task LoadMetadataAsync()
    {
        var connectionString = _activeConnectionString ?? BuildConnectionString();
        ObjectTree.Items.Clear();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string metadataSql = """
            SELECT s.name AS SchemaName, o.name AS ObjectName, o.type_desc AS ObjectType, c.name AS ColumnName
            FROM sys.objects o
            INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
            LEFT JOIN sys.columns c ON c.object_id = o.object_id
            WHERE o.type IN ('U', 'V', 'P', 'FN', 'IF', 'TF', 'TR')
            ORDER BY s.name, o.name, c.column_id;
            """;

        await using var command = new SqlCommand(metadataSql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        var schemas = new Dictionary<string, TreeViewItem>();
        var objects = new Dictionary<string, TreeViewItem>();

        while (await reader.ReadAsync())
        {
            var schema = reader.GetString(0);
            var objectName = reader.GetString(1);
            var objectType = reader.GetString(2);
            var column = reader.IsDBNull(3) ? null : reader.GetString(3);

            if (!schemas.TryGetValue(schema, out var schemaNode))
            {
                schemaNode = new TreeViewItem { Header = schema };
                schemas[schema] = schemaNode;
                ObjectTree.Items.Add(schemaNode);
            }

            var objectKey = $"{schema}.{objectName}";
            if (!objects.TryGetValue(objectKey, out var objectNode))
            {
                objectNode = new TreeViewItem { Header = $"{objectName} ({NormalizeObjectType(objectType)})", Tag = objectKey };
                objectNode.MouseDoubleClick += (_, _) => InsertTextAtCaret(objectKey);
                objects[objectKey] = objectNode;
                schemaNode.Items.Add(objectNode);
            }

            if (!string.IsNullOrWhiteSpace(column))
            {
                var columnNode = new TreeViewItem { Header = column, Tag = $"{objectKey}.{column}" };
                columnNode.MouseDoubleClick += (_, _) => InsertTextAtCaret(column);
                objectNode.Items.Add(columnNode);
            }
        }
    }

    private string BuildConnectionString()
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = ServerTextBox.Text.Trim(),
            InitialCatalog = string.IsNullOrWhiteSpace(DatabaseTextBox.Text) ? "master" : DatabaseTextBox.Text.Trim(),
            UserID = UserTextBox.Text.Trim(),
            Password = PasswordBox.Password,
            Encrypt = EncryptCheckBox.IsChecked == true,
            TrustServerCertificate = TrustCertificateCheckBox.IsChecked == true,
            ConnectTimeout = 30
        };

        return builder.ConnectionString;
    }

    private async Task RunUiActionAsync(string status, Func<Task> action)
    {
        RuntimeText.Text = status;
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            AppendMessage(ex.Message);
            MessageBox.Show(this, ex.Message, "JH Query Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            RuntimeText.Text = "Desktop .NET 8 · Sin API · SQL Server directo";
        }
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            e.Handled = true;
            _ = ExecuteCurrentSqlAsync();
        }

        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.L)
        {
            e.Handled = true;
            MessagesTextBox.Clear();
        }
    }

    private void InsertTextAtCaret(string value)
    {
        SqlEditorTextBox.SelectedText = value;
        SqlEditorTextBox.Focus();
    }

    private void AppendMessage(string message)
    {
        MessagesTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        MessagesTextBox.ScrollToEnd();
    }

    private static string NormalizeObjectType(string objectType) => objectType switch
    {
        "USER_TABLE" => "tabla",
        "VIEW" => "vista",
        "SQL_STORED_PROCEDURE" => "procedimiento",
        "SQL_SCALAR_FUNCTION" or "SQL_INLINE_TABLE_VALUED_FUNCTION" or "SQL_TABLE_VALUED_FUNCTION" => "función",
        "SQL_TRIGGER" => "trigger",
        _ => objectType.ToLowerInvariant()
    };
}
