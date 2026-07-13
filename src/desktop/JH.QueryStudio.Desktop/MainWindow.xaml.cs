using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace JH.QueryStudio.Desktop;

public partial class MainWindow : Window
{
    private const string BackendUrl = "http://localhost:5088";
    private const string FrontendUrl = "http://localhost:5173";

    private readonly HttpClient _httpClient = new();
    private readonly List<Process> _ownedProcesses = [];

    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await StartLocalStudioAsync();
        Closing += (_, _) => StopOwnedProcesses();
    }

    private async Task StartLocalStudioAsync()
    {
        try
        {
            StatusText.Text = "Verificando backend ASP.NET Core...";
            if (!await IsHealthyAsync($"{BackendUrl}/swagger"))
            {
                StartProcess("dotnet", "run --urls http://localhost:5088", GetBackendPath());
            }

            StatusText.Text = "Verificando frontend React/Vite...";
            if (!await IsHealthyAsync(FrontendUrl))
            {
                StartProcess(GetNpmExecutable(), "run dev -- --host 127.0.0.1", GetFrontendPath());
            }

            StatusText.Text = "Esperando servicios locales...";
            await WaitUntilAvailableAsync(FrontendUrl, TimeSpan.FromSeconds(90));

            await StudioWebView.EnsureCoreWebView2Async();
            StudioWebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            StudioWebView.Source = new Uri(FrontendUrl);
            LoadingPanel.Visibility = Visibility.Collapsed;
            RuntimeText.Text = $"Desktop .NET 8 + WebView2 · {FrontendUrl}";
        }
        catch (Exception ex)
        {
            StatusText.Text = "No se pudo iniciar automáticamente. Ejecuta backend y frontend desde las tareas de VS Code.";
            MessageBox.Show(this, ex.Message, "JH Query Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static string GetRepositoryRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "JH.QueryStudio.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    }

    private static string GetBackendPath() => Path.Combine(GetRepositoryRoot(), "src", "backend", "JH.QueryStudio.Api");

    private static string GetFrontendPath() => Path.Combine(GetRepositoryRoot(), "src", "frontend");

    private static string GetNpmExecutable() => OperatingSystem.IsWindows() ? "npm.cmd" : "npm";

    private void StartProcess(string fileName, string arguments, string workingDirectory)
    {
        if (!Directory.Exists(workingDirectory))
        {
            throw new DirectoryNotFoundException($"No existe el directorio requerido: {workingDirectory}");
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        process.OutputDataReceived += (_, e) => Debug.WriteLine(e.Data);
        process.ErrorDataReceived += (_, e) => Debug.WriteLine(e.Data);

        if (!process.Start())
        {
            throw new InvalidOperationException($"No se pudo iniciar {fileName} {arguments}");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        _ownedProcesses.Add(process);
    }

    private async Task<bool> IsHealthyAsync(string url)
    {
        try
        {
            using var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task WaitUntilAvailableAsync(string url, TimeSpan timeout)
    {
        var startedAt = DateTimeOffset.UtcNow;
        while (DateTimeOffset.UtcNow - startedAt < timeout)
        {
            if (await IsHealthyAsync(url))
            {
                return;
            }

            await Task.Delay(1_000);
        }

        throw new TimeoutException($"El servicio {url} no respondió dentro del tiempo esperado.");
    }

    private void StopOwnedProcesses()
    {
        foreach (var process in _ownedProcesses.Where(p => !p.HasExited))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Best effort shutdown for local development processes.
            }
        }
    }

    private void ReloadButton_Click(object sender, RoutedEventArgs e)
    {
        StudioWebView.Reload();
    }

    private void OpenBrowserButton_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo(FrontendUrl) { UseShellExecute = true });
    }
}
