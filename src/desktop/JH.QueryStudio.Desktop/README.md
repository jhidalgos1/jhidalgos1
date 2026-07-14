# JH Query Studio Desktop

Esta es la versión de escritorio unificada de JH Query Studio. No levanta una API ni necesita un frontend web externo: la interfaz WPF, la conexión a SQL Server, el explorador, el editor, la ejecución de consultas y el historial local viven dentro del mismo proceso .NET 8.

## Ejecutar desde VS Code

```bash
dotnet run --project src/desktop/JH.QueryStudio.Desktop/JH.QueryStudio.Desktop.csproj
```

## Debug paso a paso

1. Abre la carpeta del repo en VS Code.
2. Instala C# Dev Kit.
3. Coloca breakpoints en `MainWindow.xaml.cs`.
4. Presiona F5 y elige `JH Query Studio Desktop`.
5. Usa F10/F11 para depurar línea por línea.

## Pruebas desde VS Code

Ejecuta la tarea `JH Query Studio: test desktop services` o usa:

```bash
dotnet test tests/JH.QueryStudio.Tests/JH.QueryStudio.Tests.csproj
```
