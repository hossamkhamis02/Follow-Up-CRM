# ADR-001: Modular Monolith over Microservices

**Status**: Accepted  
**Date**: 2025-01-15  
**Deciders**: Engineering Team

## Context

FollowUp CRM is a new SaaS application with six core modules: Identity, Workspaces, Permissions, Customers, FollowUps, and Dashboard. We need to choose an architectural style that supports multi-tenancy, domain separation, and future growth while keeping initial development velocity high.

Two primary options were considered:

1. **Microservices** — each module as an independent deployable service with its own database and API
2. **Modular Monolith** — single deployable unit with strict module boundaries enforced in code

## Decision

We adopt a **Modular Monolith** architecture. The application is deployed as a single .NET 8 web API, but internally organized into autonomous modules with explicit boundaries. Each module has its own:

- Feature endpoints (via `MapModule()` extension methods)
- MediatR handlers (commands/queries scoped to the module)
- EF Core entity configurations and database schema (via separate `DbContext` or schema prefixes)
- Dependency registration (`AddModule()` extension methods)

Modules communicate through well-defined interfaces — never by sharing internal implementation details.

## Rationale

- **Startup velocity**: A single deployment pipeline, one database, one debugging session. No distributed systems complexity while the team is small and the domain is still evolving.
- **Domain boundaries enforced in code**: Module-level encapsulation via static extension methods (`AddCustomersModule()`, `MapCustomersModule()`) makes it impossible to accidentally cross boundaries. This discipline is identical to what microservices enforce at the network level, but without the operational overhead.
- **Extractability**: When a module matures and has clear traffic/resource characteristics, it can be extracted into a separate service. The strict boundaries make extraction a refactoring exercise, not an architectural rewrite.
- **Operational simplicity**: One container, one health check endpoint, one set of logs. No service mesh, no distributed tracing infrastructure, no saga orchestration for early-stage CRUD operations.
- **Team alignment**: A small team (1-3 developers) cannot maintain six microservices effectively. A modular monolith lets a small team work on all modules without coordination overhead.

## Tradeoffs

| Aspect | Advantage | Disadvantage |
|--------|-----------|--------------|
| Deployment | Single pipeline, zero coordination | Entire app redeployed for any change |
| Scaling | Simple vertical scaling | Cannot scale individual modules independently |
| Failure isolation | One process to monitor | One module crash can bring down the whole app |
| Data consistency | Same database, ACID transactions | No module-level database isolation |
| Development speed | No inter-service contracts to maintain | Temptation to bypass module boundaries |
| Team scaling | Easy for small team | Large teams may conflict on shared deployable |

## Future Evolution

When the team grows beyond 5 developers or a module reaches clear operational separation (e.g., Dashboard analytics with heavy read traffic), we will:

1. **Extract that module** into a standalone service by:
   - Moving its MediatR handlers into a new project
   - Giving it its own database (with data migration)
   - Replacing in-process calls with HTTP/gRPC via `IHttpClientFactory` with Polly resilience
2. **Maintain the modular monolith pattern** for remaining modules — extraction is incremental, not all-at-once.
3. **Enforce module boundaries via architecture tests** (NetArchTest or similar) to prevent boundary violations before they reach production.

The modular monolith is not a compromise — it is the correct starting architecture for a domain that is still being discovered. Microservices are a scaling strategy, not a design strategy.