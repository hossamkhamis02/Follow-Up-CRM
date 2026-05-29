# Onboarding — FollowUp CRM Developer Setup

## 1. System Requirements

Install the following before proceeding:

| Tool            | Minimum Version | Verify Command       | Download                                                    |
|-----------------|-----------------|----------------------|-------------------------------------------------------------|
| .NET SDK        | 8.0             | `dotnet --version`   | https://dotnet.microsoft.com/download                       |
| Node.js         | 20 LTS          | `node --version`     | https://nodejs.org                                          |
| Angular CLI     | 19              | `ng version`         | `npm install -g @angular/cli@19`                           |
| Docker Desktop  | 24              | `docker --version`   | https://docker.com/products/docker-desktop                  |
| Git             | 2.40            | `git --version`      | https://git-scm.com/downloads                               |

### Windows-specific

- Enable WSL2 for Docker Desktop (Settings → General → Use WSL 2 based engine)
- Install PowerShell 7+ for scripts (`winget install Microsoft.PowerShell`)

## 2. Clone & Configure

```bash
git clone <repo-url>
cd followup-crm
cp .env.example .env
```

**Required edits in `.env`:**

| Variable                     | Action                                           |
|------------------------------|--------------------------------------------------|
| `POSTGRES_PASSWORD`          | Set a secure password (never use default in prod)|
| `Jwt__Key`                   | Minimum 32 characters — generate with:           |
|                              | `openssl rand -base64 48`                        |
| `PGADMIN_PASSWORD`           | Set a secure password                            |

## 3. Database Setup

### Option A: Docker (recommended)

```bash
docker-compose up postgres pgadmin
```

Wait for healthcheck to pass (PG container logs show `database system is ready`).

pgAdmin → http://localhost:5050

### Option B: Local PostgreSQL

Install PostgreSQL 16 locally, then set `ConnectionStrings__DefaultConnection` in `.env`:

```
Host=localhost;Port=5432;Database=followupcrm;Username=postgres;Password=<your-password>
```

### Apply migrations

```bash
cd backend
dotnet ef database update --project src/FollowUpCrm.Api
```

## 4. Backend

```bash
cd backend
dotnet restore
dotnet run --project src/FollowUpCrm.Api
```

Verify:
- API responds: http://localhost:8080/api
- Swagger UI: http://localhost:8080/swagger

## 5. Frontend

```bash
cd frontend
npm install
ng serve
```

Verify: http://localhost:4200 loads the app shell.

## 6. Verify Full Stack

| Check                         | Expected                              |
|-------------------------------|---------------------------------------|
| API health                    | `GET http://localhost:8080/api` → 404 or Swagger redirect |
| Swagger UI                    | http://localhost:8080/swagger loads   |
| Frontend loads                | http://localhost:4200 renders shell    |
| Frontend → API proxy          | Network tab shows `/api` calls to 8080|
| pgAdmin                       | http://localhost:5050 connects to DB  |

## 7. Project Conventions

### Backend (.NET 8)

- **Architecture**: Modular monolith, vertical slices
- **CQRS**: MediatR — Commands in `Features/` folders, Handlers inline
- **Result<T>**: Always return `Result<T>` from handlers, never throw for expected failures
- **DbContext**: Direct usage in handlers (no repository pattern)
- **Soft delete**: Every entity has `IsDeleted` — no hard deletes
- **Naming**: PascalCase for C#, camelCase for JSON/API output
- **Validation**: FluentValidation on all command inputs

### Frontend (Angular 19)

- **Standalone components** — no NgModules
- **Feature-based structure** — `features/` directory per domain module
- **Reactive forms** — FormBuilder for all complex forms
- **Auth flow** — JWT via `AuthInterceptor`, `AuthGuard` on protected routes
- **API calls** — `ApiService` with typed `HttpClient.get<T>()`
- **Unsubscribe** — `takeUntilDestroyed()` or `async` pipe, never manual

### Database

- **EF Core** code-first with migrations
- **SET NOCOUNT ON** in all stored procedures
- **EXISTS** over `COUNT(*)` for existence checks
- **Soft deletes** — `IsDeleted` flag on all entities

### Git

- **Branch naming**: `feature/<module>-<description>`, `bugfix/<description>`
- **Commit messages**: Conventional Commits (`feat:`, `fix:`, `refactor:`, `docs:`)
- **No secrets** in commits — `.env`, `appsettings.Production.json`, `*.pfx` are gitignored

## 8. Useful Commands

```bash
# Backend — add migration
dotnet ef migrations add <Name> --project backend/src/FollowUpCrm.Api

# Backend — apply migrations
dotnet ef database update --project backend/src/FollowUpCrm.Api

# Backend — run tests
dotnet test

# Frontend — lint
cd frontend && ng lint

# Frontend — build production
cd frontend && ng build --configuration production

# Docker — full stack
docker-compose up

# Docker — destroy & recreate DB
docker-compose down -v && docker-compose up postgres pgadmin

# Scripts — automated setup
powershell ./scripts/setup.ps1
```

## 9. Troubleshooting

| Problem                            | Solution                                               |
|------------------------------------|---------------------------------------------------------|
| `dotnet ef` not found              | `dotnet tool install -g dotnet-ef`                     |
| Frontend build fails               | `rm -rf node_modules && npm install`                   |
| PostgreSQL container won't start   | Check port 5432 not in use; `docker-compose down -v`  |
| API won't connect to DB            | Check `.env` connection string matches PG credentials  |
| CORS errors in browser             | Verify `Cors__AllowedOrigins__0` in `.env`             |
| pgAdmin won't connect              | Host: `postgres`, Port: `5432`, user/pass from `.env`  |