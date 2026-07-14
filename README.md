# JsConsulting

**JsConsulting** es una aplicación web integrada para consultar SQL Server desde el navegador sin separar backend y frontend en proyectos distintos.

La aplicación corre en un solo proceso **ASP.NET Core 8 + Razor Pages**:

- UI web moderna, clara, colorida e interactiva.
- Backend integrado en el mismo proyecto web.
- Conexión directa a Microsoft SQL Server.
- Explorador de objetos.
- Editor SQL con sugerencias en vivo.
- Ejecución de consultas con F5.
- Grilla de resultados ordenable, compacta y adaptable.
- Historial local en SQLite.
- Protección contra sentencias peligrosas mediante `QuerySafetyAnalyzer`.

## Estructura

```text
JsConsulting.sln
src/JsConsulting.Web                  # Web integrada ASP.NET Core 8 + Razor Pages
src/shared/JsConsulting.Core           # Lógica compartida testeable
tests/JsConsulting.Tests              # Pruebas xUnit
```

## Ejecutar en VS Code

Requisitos:

- .NET SDK 8.
- VS Code.
- C# Dev Kit.
- SQL Server local o remoto.

Desde la raíz del repositorio:

```bash
dotnet restore JsConsulting.sln
dotnet run --project src/JsConsulting.Web/JsConsulting.Web.csproj --urls http://localhost:5198
```

Luego abre:

```text
http://localhost:5198
```

También puedes usar la tarea de VS Code:

```text
Terminal > Run Task > JsConsulting: run web
```

## Debug paso a paso en VS Code

1. Abre la carpeta del repositorio en VS Code.
2. Abre `JsConsulting.sln` si usas la vista de solución.
3. Coloca breakpoints en:
   - `src/JsConsulting.Web/Pages/Index.cshtml.cs`
   - `src/JsConsulting.Web/Services/SqlWorkspaceService.cs`
   - `src/shared/JsConsulting.Core/Security/QuerySafetyAnalyzer.cs`
4. Ve a **Run and Debug**.
5. Selecciona **JsConsulting Web**.
6. Presiona **F5**.
7. VS Code abrirá `http://localhost:5198` cuando la app esté lista.

## Pruebas

```bash
dotnet test tests/JsConsulting.Tests/JsConsulting.Tests.csproj
```

O desde VS Code:

```text
Terminal > Run Task > JsConsulting: test
```

## Uso rápido

1. Completa servidor, base de datos, usuario y contraseña.
2. Presiona **Probar**.
3. Presiona **Conectar** para cargar el explorador.
4. Escribe SQL en el editor.
5. Usa **Ctrl+Space** para sugerencias.
6. Presiona **F5** o **Ejecutar F5**.
7. Revisa resultados, mensajes e historial.

## Nota sobre el repositorio

En este entorno no existe remoto `origin`, por lo que no puedo crear un repositorio GitHub remoto automáticamente. Sí dejé creada la solución y estructura local con el nombre **JsConsulting** para que puedas subirla a un repositorio nuevo con:

```bash
git remote add origin https://github.com/TU_USUARIO/JsConsulting.git
git push origin work:main
```
