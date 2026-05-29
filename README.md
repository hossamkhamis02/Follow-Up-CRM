# FollowUp CRM

SaaS CRM application — modular monolith with vertical slice architecture, .NET 8 backend, Angular 19 frontend.

## Prerequisites

| Tool          | Version   | Install                                          |
|---------------|-----------|--------------------------------------------------|
| .NET SDK      | 8.0+      | `dotnet --version`                               |
| Node.js       | 20 LTS+   | `node --version`                                 |
| Angular CLI   | 19+       | `npm install -g @angular/cli`                    |
| Docker        | 24+       | `docker --version`                               |
| PostgreSQL    | 16+       | via Docker or local                              |
| Git           | 2.40+     | `git --version`                                  |

## Project Structure

```
followup-crm/
├── backend/                .NET 8 API (Modular Monolith, Vertical Slice)
│   ├── FollowUpCrm.sln
│   └── src/
│       ├── FollowUpCrm.Api/              Presentation + feature slices
│       ├── FollowUpCrm.Infrastructure/   EF Core, DB, external services
│       └── FollowUpCrm.Shared/           Result<T>, constants, extensions
├── frontend/               Angular 19 (Standalone, feature-based)
│   └── src/
│       ├── app/
│       │   ├── core/        Auth, interceptors, guards, services
│       │   ├── features/    Identity, Dashboard, Customers, FollowUps
│       │   ├── layout/      App shell, sidebar, topbar, footer
│       │   └── shared/      Models, utilities
│       └── environments/    environment.ts / environment.prod.ts
├── docs/                   Architecture decisions & API docs
│   ├── architecture/       ADRs (Architecture Decision Records)
│   ├── api/                API documentation
│   └── deployment/         Deployment guides
├── docker/                 Dockerfiles & nginx config
│   ├── api/Dockerfile
│   └── nginx/Dockerfile + default.conf
├── scripts/                Dev setup & utility scripts
├── docker-compose.yml      Full stack: API + Frontend + PostgreSQL
├── .env.example            Environment variable template
├── .editorconfig           Cross-editor formatting rules
└── .gitignore              .NET + Angular + Docker ignores
```

## Architecture

- **Style**: Modular Monolith with Vertical Slice Architecture
- **Modules**: Identity, Workspaces, Permissions, Customers, FollowUps, Dashboard
- **No repository pattern** — direct DbContext via MediatR handlers
- **Result<T>** pattern for expected failures, not exceptions
- **CQRS** with MediatR — Commands mutate, Queries read
- **Soft delete** everywhere (`IsDeleted` flag)

## Modules

| Module       | Purpose                              |
|--------------|--------------------------------------|
| Identity     | Auth, JWT, user management           |
| Workspaces   | Multi-tenant workspace CRUD          |
| Permissions  | Role-based access control            |
| Customers    | Customer entity CRUD + search        |
| FollowUps    | Follow-up scheduling & tracking      |
| Dashboard    | Aggregated stats & summaries         |

## Quick Start

### 1. Clone & configure environment

```bash
git clone <repo-url> followup-crm
cd followup-crm
cp .env.example .env
# Edit .env — set POSTGRES_PASSWORD, Jwt__Key, etc.
```

### 2. Start infrastructure (PostgreSQL + pgAdmin)

```bash
docker-compose up postgres pgadmin
```

### 3. Backend

```bash
cd backend
dotnet restore
dotnet ef database update --project src/FollowUpCrm.Api
dotnet run --project src/FollowUpCrm.Api
# API → http://localhost:8080
# Swagger → http://localhost:8080/swagger
```

### 4. Frontend

```bash
cd frontend
npm install
ng serve
# App → http://localhost:4200
```

### 5. Full stack (Docker)

```bash
docker-compose up
# Uncomment api & frontend services in docker-compose.yml first
```

## Development Workflow

| Task                   | Command                                                    |
|------------------------|------------------------------------------------------------|
| Run backend            | `dotnet run --project backend/src/FollowUpCrm.Api`        |
| Run frontend           | `cd frontend && ng serve`                                  |
| Add EF migration       | `dotnet ef migrations add <Name> --project backend/src/FollowUpCrm.Api` |
| Apply migrations       | `dotnet ef database update --project backend/src/FollowUpCrm.Api`        |
| Run backend tests      | `dotnet test`                                              |
| Run frontend tests     | `cd frontend && ng test`                                   |
| Lint frontend          | `cd frontend && ng lint`                                   |
| Build frontend         | `cd frontend && ng build --configuration production`       |

## Ports

| Service     | Dev Port  | Docker Port |
|-------------|-----------|-------------|
| API         | 8080      | 8080        |
| Frontend    | 4200      | 80          |
| PostgreSQL  | 5432      | 5432        |
| pgAdmin     | 5050      | 5050        |

## Documentation

- [Architecture ADRs](docs/architecture/) — decision records
- [API docs](docs/api/) — endpoint documentation
- [Deployment guides](docs/deployment/) — production deployment
- [Onboarding guide](docs/onboarding.md) — new developer setup

## License

Private — All rights reserved.