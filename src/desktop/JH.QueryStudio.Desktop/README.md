# JH Query Studio Desktop

Este proyecto convierte el MVP web local en una aplicación de escritorio .NET 8 para Windows usando WPF + WebView2.

## Ejecución desde VS Code

1. Instala .NET SDK 8, Node.js 20+ y WebView2 Runtime.
2. Ejecuta `npm install` en `src/frontend` una sola vez.
3. Desde la raíz del repo ejecuta:

```bash
dotnet run --project src/desktop/JH.QueryStudio.Desktop/JH.QueryStudio.Desktop.csproj
```

La ventana de escritorio inicia automáticamente:

- Backend ASP.NET Core en `http://localhost:5088` si no está activo.
- Frontend Vite en `http://localhost:5173` si no está activo.
- WebView2 embebido apuntando a JH Query Studio.

## Notas

La primera versión desktop usa el frontend React existente dentro de una ventana nativa. Esto permite iterar rápido y mantiene preparada la base para empaquetado posterior con instalador Windows.
