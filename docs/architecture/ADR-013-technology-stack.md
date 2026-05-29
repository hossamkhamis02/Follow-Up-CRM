# ADR-013: .NET 8 + PostgreSQL + Angular Stack

**Status**: Accepted  
**Date**: 2025-01-15  
**Deciders**: Engineering Team

## Context

FollowUp CRM needs a technology stack for the backend API, database, and frontend SPA. The team's expertise is in .NET (ASP.NET Core), SQL Server, and Angular. However, the project is a new SaaS product with no legacy constraints, so we can choose any combination.

Candidate stacks:

1. **.NET 8 + SQL Server + Angular** — team's primary expertise, enterprise-proven, but SQL Server licensing costs increase with scale
2. **.NET 8 + PostgreSQL + Angular** — .NET ecosystem with open-source database, reducing licensing costs
3. **Node.js (NestJS) + PostgreSQL + Angular** — unified TypeScript stack, but team lacks Node backend experience
4. **.NET 8 + PostgreSQL + React** — React's larger community, but team is Angular-focused

## Decision

We adopt **.NET 8 + PostgreSQL + Angular 19** as the technology stack.

- **Backend**: ASP.NET Core 8 Web API with EF Core 8 as the ORM
- **Database**: PostgreSQL 16 (via Docker `postgres:16-alpine`)
- **Frontend**: Angular 19 with standalone components, Tailwind CSS, and TypeScript
- **Infrastructure**: Docker Compose for local development (API + PostgreSQL + pgAdmin + nginx for frontend)

## Rationale

### .NET 8 (Backend)

- **Performance**: .NET 8 is the fastest ASP.NET Core version to date. Minimal API overhead, optimized memory management, native AOT compilation available for cold-start scenarios.
- **EF Core 8**: Mature ORM with PostgreSQL provider (`Npgsql.EntityFrameworkCore.PostgreSQL`). Supports global query filters (multi-tenancy), soft delete filters, and schema-per-entity configuration.
- **Team expertise**: The team builds .NET APIs daily. No learning curve for framework patterns, debugging, or deployment. Productivity is immediate.
- **Enterprise maturity**: ASP.NET Core is battle-tested in production at scale. Health checks, rate limiting, API versioning, CORS — all built-in or one-package-away.
- **MediatR + FluentValidation**: The CQRS and validation ecosystem is well-established in .NET. These libraries are the backbone of the vertical slice architecture (ADR-002).

### PostgreSQL (Database)

- **Zero licensing cost**: PostgreSQL is free. SQL Server licensing costs ($3,500+ per core for Standard Edition) would penalize a startup SaaS. PostgreSQL eliminates this entirely.
- **Feature parity for SaaS**: Row-level security (RLS) for multi-tenancy, partial unique indexes for soft delete, JSONB for flexible metadata, LISTEN/NOTIFY for real-time events — PostgreSQL has every feature a SaaS CRM needs.
- **Docker ecosystem**: `postgres:16-alpine` is a 80MB Docker image. SQL Server containers are 1.5GB+ and slower to start. PostgreSQL's lightweight container speeds up local development and CI.
- **EF Core compatibility**: Npgsql provider is first-party (maintained by the same team as PostgreSQL itself). Full feature support including migrations, complex types, and raw SQL.
- **Cloud options**: Every major cloud (AWS RDS, Azure Database for PostgreSQL, Google Cloud SQL) offers managed PostgreSQL. Migration between providers is straightforward — no vendor lock-in to Azure SQL.

### Angular 19 (Frontend)

- **Team expertise**: Angular is the team's daily framework. Standalone components, signals, and injectors in Angular 19 are the modern patterns the team already uses.
- **Opinionated structure**: Angular's module/component/service conventions enforce consistency across the team. In a SaaS product with many feature modules, this prevents ad-hoc architecture drift.
- **Lazy loading**: Angular's router-level lazy loading (`loadChildren`) naturally maps to the backend module structure. Each frontend feature module mirrors a backend module.
- **TypeScript everywhere**: Angular mandates TypeScript. Type-safe API calls via `HttpClient` with typed responses (`this.http.get<Customer[]>(url)`) catch integration errors at compile time.
- **Enterprise ecosystem**: Angular Material, NgRx (if needed), and CDK provide enterprise-grade UI components and state management. No need to build complex data tables, form controls, or accessibility features from scratch.

## Tradeoffs

| Aspect | Advantage | Disadvantage |
|--------|-----------|--------------|
| Licensing | PostgreSQL free, .NET free | No SQL Server enterprise features (Always On, CDC) |
| Team velocity | Immediate productivity | PostgreSQL patterns new to SQL Server-experienced team |
| Database features | RLS, JSONB, partial indexes | Some EF Core features lag behind SQL Server provider |
| Cloud flexibility | Any cloud with managed PG | Azure first-party tooling favors SQL Server |
| Frontend ecosystem | Angular opinionated = consistent | Smaller community vs. React |
| Docker size | PG Alpine = 80MB | Npgsql minor version lag behind EF Core releases |

## Future Evolution

- **PostgreSQL proficiency**: The team will invest in learning PostgreSQL-specific patterns: RLS policies, JSONB queries, `EXPLAIN ANALYZE` for query optimization, and `pg_stat_statements` for performance monitoring. This is a one-time learning investment.
- **Redis for caching**: Add Redis as a sidecar cache for session tokens, frequently-accessed customer lists, and workspace configuration. Redis also serves as the signalR backplane for real-time features.
- **SignalR for real-time**: When follow-up reminders need push delivery, add SignalR with PostgreSQL LISTEN/NOTIFY as the backplane. This provides real-time updates without polling.
- **Native AOT**: For cold-start optimization in containerized deployments, evaluate .NET 8 Native AOT compilation for the API. This reduces startup time from seconds to milliseconds, critical for auto-scaling scenarios.
- **Angular signals migration**: As Angular signals mature, migrate from RxJS-based state management to signal-based patterns for local component state. Keep RxJS for async operations (HTTP, events) where observables are the natural model.
- **Monitoring**: Add OpenTelemetry instrumentation for distributed tracing, metrics, and log correlation. Export to Prometheus + Grafana for observability as the deployment grows beyond single-instance.