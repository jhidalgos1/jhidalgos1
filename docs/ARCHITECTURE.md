# Arquitectura de JH Query Studio

## Decisiones principales
- Backend ASP.NET Core 8 con controladores REST, servicios inyectados y responsabilidades separadas: conexiones, metadatos, ejecución, seguridad, historial y snippets.
- `Microsoft.Data.SqlClient` ejecuta consultas y obtiene metadatos de SQL Server. Las interfaces permiten agregar PostgreSQL, MySQL, Oracle y SQLite con nuevos proveedores.
- SQLite local persiste conexiones, historial, biblioteca y snippets. Las credenciales se cifran con AES y una clave local generada en `Data/jh-query-studio.key`.
- Frontend React + TypeScript + Vite con TanStack Query, Zustand, Tailwind y Monaco Editor.

## Autocompletado
Monaco registra un proveedor SQL que combina palabras reservadas, snippets del backend y metadatos del explorador. Al detectar `schema.` sugiere tablas/vistas y al detectar alias como `c.` resuelve columnas desde sentencias `FROM` y `JOIN`.

## Protección
Antes de ejecutar se analizan sentencias críticas: `UPDATE`/`DELETE` sin `WHERE`, `DROP`, `TRUNCATE` y cambios en Producción. El frontend solicita escribir `CONFIRMAR` y el backend bloquea si `confirmedRisk` no llega activo.
