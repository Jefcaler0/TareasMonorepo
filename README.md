# Tareas Monorepo

Aplicación de gestión de tareas compuesta por un backend en ASP.NET Core Minimal API, un frontend en React + Vite y una base de datos SQL Server desplegada mediante Docker. El objetivo es ofrecer un flujo completo para crear, listar, actualizar y eliminar tareas.

## Tecnologías principales
- Backend: .NET 8, ASP.NET Core Minimal API, C#
- Frontend: React 18, TypeScript, Vite
- Base de datos: SQL Server 2022 (contenedor Docker)
- Gestión de datos: Repositorio ADO.NET propio

## Estructura del repositorio
- `backend/`: solución .Net con clean arquitecture  `Tareas.API`, `Tareas.Application`, `Tareas.Infrastructure` y `Tareas.Domain`.
- `frontend/`: aplicación React + Vite que consume los endpoints del backend.
- `docker/`: recursos para el contenedor de SQL Server (`docker-compose.yml`, `mssql.Dockerfile`).
- `scripts_db/`: scripts SQL para crear el esquema (`001_create_tasks_schema.sql`).

## Requisitos previos
- .NET SDK 8.0+
- Node.js 18+ con Corepack habilitado (`corepack enable`) para usar pnpm
- pnpm 9+
- Docker Desktop (incluye Docker Compose) en ejecución
- `sqlcmd` (incluido en el contenedor de SQL Server o instalable localmente) para aplicar scripts manuales

## Configuración de variables de entorno
Centraliza las variables en un archivo `.env` en la raíz del repositorio. Ejemplo:

```
# Base de datos
SA_PASSWORD=YourStrong!Passw0rd
MSSQL_PID=Developer
DB_CONNECTION_STRING=Server=localhost,1433;Initial Catalog=Tareas;User Id=sa;Password=${SA_PASSWORD};TrustServerCertificate=True;Encrypt=False;

# Backend
FRONTEND_URL=http://localhost:5173

# Frontend
VITE_API_BASE_URL=http://localhost:5062
```

Antes de levantar los servicios:
1. Exporta el archivo (`source .env` en macOS/Linux o usa una herramienta tipo `direnv`).
2. Copia las variables necesarias si tu herramienta no soporta interpolación:
   - Backend: el archivo `.env` es leído automáticamente al ejecutar `dotnet run` (gracias a `Env.TraversePath()`).
   - Frontend: Vite lee variables con prefijo `VITE_`; si no se aplican automáticamente, crea `frontend/.env` con `VITE_API_BASE_URL`.

## Inicio rápido con script
En la raíz hay un script multiplataforma que levanta Docker, backend y frontend simultáneamente:

```bash
node start-monorepo.js
```

En sistemas Unix también puedes ejecutarlo directamente (`chmod +x start-monorepo.js && ./start-monorepo.js`). En Windows funciona con `node start-monorepo.js` o `.\start-monorepo.js`.

El script:
- Carga variables desde `.env` si está disponible.
- Ejecuta `docker compose up -d` usando `docker/docker-compose.yml`.
- Restaura dependencias si es necesario.
- Inicia `dotnet run --urls http://localhost:5062` (puedes cambiar el puerto via `ASPNETCORE_URLS`) en `backend/Tareas.API`.
- Inicia `pnpm dev` en `frontend`.
- Escucha `Ctrl+C` para detener procesos y ejecutar `docker compose down`.

## Puesta en marcha local

1. **Base de datos**
   ```bash
   docker compose -f docker/docker-compose.yml up -d
   ```
   Cuando el contenedor esté listo, ejecuta el script de schema:
   ```bash
   sqlcmd -S localhost,1433 -U sa -P "$SA_PASSWORD" -d master -i scripts_db/001_create_tasks_schema.sql
   ```

2. **Backend**
   ```bash
   cd backend/Tareas.API
   dotnet restore
   dotnet run
   ```
   La API escucha por defecto en `http://localhost:5062`. Swagger UI queda disponible en `http://localhost:5062/swagger`.

3. **Frontend**
   ```bash
   cd frontend
   pnpm install
   pnpm dev
   ```
   Vite expone la aplicación en `http://localhost:5173`.

4. **Verificación**
   - Abre el navegador en `http://localhost:5173` y crea tareas.
   - Swagger (`http://localhost:5062/swagger`) permite probar los endpoints manualmente.

## Scripts útiles
- Apagar y limpiar la base de datos:
  ```bash
  docker compose -f docker/docker-compose.yml down -v
  ```
- Compilar el frontend:
  ```bash
  pnpm --dir frontend build
  ```
- Ejecutar la API con hot reload:
  ```bash
  dotnet watch --project backend/Tareas.API/Tareas.API.csproj
  ```


