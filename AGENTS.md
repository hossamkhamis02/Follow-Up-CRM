# Repository Guidelines

## Project Structure & Module Organization

FollowUp CRM is a .NET 8 and Angular 19 SaaS CRM. Backend code lives in `backend/src`: `FollowUpCrm.Api` contains presentation, middleware, configuration, and vertical feature slices under `Modules`; `FollowUpCrm.Infrastructure` contains EF Core persistence, migrations, entities, configurations, and health checks; `FollowUpCrm.Shared` contains shared constants, extensions, and `Result<T>` utilities. Frontend code lives in `frontend/src/app`, organized into `core`, `features`, `layout`, and `shared`. Documentation is in `docs`, Docker assets are in `docker`, and utility scripts are in `scripts`.

## Build, Test, and Development Commands

- `dotnet restore backend/FollowUpCrm.sln`: restore backend dependencies.
- `dotnet build backend/FollowUpCrm.sln`: compile the backend solution.
- `dotnet run --project backend/src/FollowUpCrm.Api`: run the API on the configured development port.
- `dotnet ef database update --project backend/src/FollowUpCrm.Api`: apply EF Core migrations.
- `cd frontend && npm install`: install Angular dependencies from `package-lock.json`.
- `cd frontend && npm start`: run `ng serve` for local frontend development.
- `cd frontend && npm run build`: create a production Angular build.
- `docker-compose up postgres pgadmin`: start local database services.

## Coding Style & Naming Conventions

Follow `.editorconfig`: UTF-8, LF endings, final newline, spaces only. Use 4-space indentation for C# and 2-space indentation for TypeScript, HTML, SCSS, JSON, YAML, SQL, and Dockerfiles. Keep C# nullable annotations enabled and sort `System` directives first. Match namespaces under `FollowUpCrm.*`. Backend feature work should stay inside the relevant module slice, for example `Modules/Customers/Features`. Angular feature folders use kebab case, such as `follow-ups`.

## Testing Guidelines

Run `dotnet test` from the repository root when backend test projects are added. Place backend tests in dedicated `*.Tests` projects mirroring the production module or feature name. Run `cd frontend && npm test` for Angular tests; place specs beside implementation files as `*.spec.ts`. Add tests for command/query handlers, validation, permissions, tenant scoping, soft delete behavior, and user-visible Angular flows.

## Commit & Pull Request Guidelines

Recent commits are short, imperative summaries such as `Set up the database foundation...`. Keep commits focused and describe the behavior or migration added. Pull requests should include a concise description, linked issue or task, migration notes, commands run, and screenshots for UI changes. Call out `.env` or Docker changes explicitly.

## Security & Configuration Tips

Do not commit real secrets. Copy `.env.example` to `.env` and keep local database passwords, JWT keys, and connection strings out of source control. Prefer Docker Compose for local PostgreSQL consistency.
