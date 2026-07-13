# JH Query Studio

**Consulta, analiza y controla tus datos.**  
**JH Query Studio — Developed by Junior Hidalgo**

Aplicación local profesional para conectarse a Microsoft SQL Server, explorar objetos, escribir SQL con Monaco Editor, usar snippets, ejecutar consultas, ver resultados, exportar CSV, guardar historial y proteger operaciones peligrosas.

## Estructura

```text
JH.QueryStudio.sln
src/backend/JH.QueryStudio.Api       # ASP.NET Core 8 Web API
src/frontend                         # React + TypeScript + Vite
src/desktop/JH.QueryStudio.Desktop    # Aplicación de escritorio .NET 8 WPF + WebView2
database/001_initial.sql             # Script SQLite local
tests/JH.QueryStudio.Tests           # Pruebas básicas xUnit
docs/ARCHITECTURE.md                 # Arquitectura y decisiones
```

## Dependencias

- .NET SDK 8
- Node.js 20+
- SQL Server accesible para probar conexiones reales

## Ejecución

```bash
cd src/backend/JH.QueryStudio.Api
dotnet restore
dotnet run --urls http://localhost:5088
```

```bash
cd src/frontend
npm install
npm run dev
```

Abrir `http://localhost:5173`.

## Ejecución como aplicación de escritorio en VS Code

La opción recomendada para usar JH Query Studio como app de escritorio en Windows es el proyecto WPF + WebView2:

```bash
cd src/frontend
npm install
cd ../..
dotnet run --project src/desktop/JH.QueryStudio.Desktop/JH.QueryStudio.Desktop.csproj
```

La ventana desktop inicia o reutiliza los servicios locales:

- Backend ASP.NET Core: `http://localhost:5088`
- Frontend Vite: `http://localhost:5173`
- Shell nativo: `JH Query Studio` con WebView2 embebido

Desde VS Code también puedes ejecutar la tarea **JH Query Studio: desktop** incluida en `.vscode/tasks.json`.

## MVP implementado

- Solución Visual Studio, backend ASP.NET Core 8 y shell de escritorio .NET 8 WPF + WebView2.
- Persistencia local SQLite con script inicial.
- Cifrado AES para contraseñas; no se registran contraseñas en logs.
- Administración de conexiones SQL Server y prueba de conexión.
- Explorador jerárquico de esquemas, tablas/vistas/rutinas y columnas.
- Editor Monaco con resaltado SQL, snippets y autocompletado por palabras, metadatos y alias.
- Ejecución de consultas, múltiples result sets, mensajes, errores y duración.
- Grilla de resultados con manejo de `NULL` y exportación CSV al portapapeles.
- Historial local de consultas.
- Detección de operaciones críticas y confirmación manual.
- Tema visual moderno con marca JH, Inter y JetBrains Mono.

## Fases siguientes

1. Empaquetado Electron/Tauri.
2. Exportación XLSX/JSON avanzada.
3. Constructor visual de consultas completo.
4. Plan de ejecución estimado y estadísticas IO/TIME.
5. Proveedores PostgreSQL, MySQL, Oracle y SQLite.
