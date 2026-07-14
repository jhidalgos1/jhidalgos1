# JH Query Studio

**Consulta, analiza y controla tus datos.**  
**JH Query Studio — Developed by Junior Hidalgo**

JH Query Studio ahora es una **aplicación de escritorio .NET 8 unificada**. No necesitas levantar una API separada ni ejecutar un frontend web para usar el MVP: la interfaz, la conexión a SQL Server, el explorador, el editor SQL, la ejecución de consultas, la grilla de resultados, la protección de consultas peligrosas y el historial local viven dentro del mismo proceso WPF.

## Estructura

```text
JH.QueryStudio.sln
src/desktop/JH.QueryStudio.Desktop    # Aplicación WPF de escritorio
src/shared/JH.QueryStudio.Core        # Reglas reutilizables y lógica testeable
tests/JH.QueryStudio.Tests            # Pruebas xUnit ejecutables desde VS Code
```

## Dependencias

- Windows 10/11.
- .NET SDK 8.
- SQL Server local o remoto para probar conexiones reales.
- VS Code con C# Dev Kit para depurar paso a paso.

## Ejecutar desde VS Code

```bash
dotnet run --project src/desktop/JH.QueryStudio.Desktop/JH.QueryStudio.Desktop.csproj
```

También puedes usar:

```text
Terminal > Run Task > JH Query Studio: run desktop
```


## Si no ves los cambios visuales nuevos

Si todavía ves el tema oscuro anterior, casi siempre estás ejecutando un binario viejo. Haz esto desde la raíz del repositorio:

```bash
dotnet clean src/desktop/JH.QueryStudio.Desktop/JH.QueryStudio.Desktop.csproj
dotnet build src/desktop/JH.QueryStudio.Desktop/JH.QueryStudio.Desktop.csproj
dotnet run --project src/desktop/JH.QueryStudio.Desktop/JH.QueryStudio.Desktop.csproj
```

En VS Code también puedes ejecutar:

```text
Terminal > Run Task > JH Query Studio: clean desktop
Terminal > Run Task > JH Query Studio: run desktop
```

La versión nueva muestra una etiqueta visible **UI clara v2** y el texto **Tema claro activo** en la barra superior para confirmar que estás ejecutando el binario actualizado.

## Debug paso a paso

1. Abre la carpeta del repositorio en VS Code.
2. Instala la extensión **C# Dev Kit**.
3. Abre `src/desktop/JH.QueryStudio.Desktop/MainWindow.xaml.cs`.
4. Coloca breakpoints en `Connect_Click`, `ExecuteCurrentSqlAsync`, `LoadMetadataAsync` o `QuerySafetyAnalyzer.Analyze`.
5. Ve a **Run and Debug**.
6. Selecciona **JH Query Studio Desktop**.
7. Presiona **F5**.
8. Usa **F10** para avanzar línea por línea y **F11** para entrar a métodos.

## Ejecutar pruebas desde VS Code

```bash
dotnet test tests/JH.QueryStudio.Tests/JH.QueryStudio.Tests.csproj
```

También puedes usar:

```text
Terminal > Run Task > JH Query Studio: test desktop services
```

## MVP desktop implementado

- Ventana WPF nativa con identidad visual clara, viva y moderna de JH Query Studio.
- Formulario de conexión directa a Microsoft SQL Server.
- Explorador de esquemas, tablas, vistas, procedimientos, funciones, triggers y columnas.
- Editor SQL desktop con sugerencias en vivo de palabras SQL, snippets básicos y objetos cargados desde la base de datos.
- Ejecución con F5 de consulta completa o selección.
- Grilla de resultados compacta, redimensionable, ordenable, reordenable y virtualizable con colores claros.
- Panel de mensajes.
- Detección de `UPDATE` sin `WHERE`, `DELETE` sin `WHERE`, `DROP` y `TRUNCATE`.
- Confirmación visual antes de ejecutar consultas peligrosas.
- Historial local en SQLite bajo `%LOCALAPPDATA%/JH Query Studio/jh-query-studio.db`.
- Pruebas xUnit de reglas de seguridad.

## Nota de arquitectura

El MVP quedó simplificado a una sola app desktop porque el objetivo actual es trabajar sin API y depurar todo desde VS Code. La lógica compartida testeable vive en `JH.QueryStudio.Core`; la UI y la integración SQL Server/SQLite viven en `JH.QueryStudio.Desktop`.
