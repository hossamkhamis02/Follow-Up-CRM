# ADR-007: Shared Database Multi-Tenancy

**Status**: Accepted  
**Date**: 2025-01-15  
**Deciders**: Engineering Team

## Context

FollowUp CRM is a multi-tenant SaaS application. Each tenant (workspace) must have isolated data — customers, follow-ups, permissions — that other tenants cannot access. We need a tenancy model that balances data isolation, operational cost, and query performance.

Three standard approaches were considered:

1. **Database-per-tenant** — each workspace gets its own PostgreSQL database
2. **Schema-per-tenant** — each workspace gets its own schema within a shared database
3. **Shared database with tenant discriminator** — all tenants share one database; every table has a `WorkspaceId` column that filters data

## Decision

We adopt **shared database with tenant discriminator** (also called "shared database, shared schema"). All tenant data lives in a single PostgreSQL database. Every entity that is tenant-scoped includes a `WorkspaceId` column. All queries are automatically filtered by the current tenant context.

Implementation strategy:

- EF Core global query filter on all tenant-scoped entities: `modelBuilder.Entity<Customer>().HasQueryFilter(e => e.WorkspaceId == _currentWorkspaceId)`
- `WorkspaceId` resolved from JWT claims and set via EF Core `DbContext` interceptor
- Middleware extracts `WorkspaceId` from the authenticated user's token and stores it in `HttpContext.Items`
- Tenant-scoped entities implement a `ITenantEntity` interface with `WorkspaceId` property for consistent enforcement

## Rationale

- **Cost efficiency**: One database instance (PostgreSQL container) serves all tenants. Database-per-tenant would require provisioning, migrating, and monitoring separate databases per workspace — unaffordable at early scale.
- **Operational simplicity**: One connection string, one migration path, one backup strategy. Schema and database-per-tenant models multiply operational complexity proportionally with tenant count.
- **Developer experience**: Adding a new tenant-scoped entity means adding `WorkspaceId` and `ITenantEntity`. No schema creation, no database provisioning, no migration-per-tenant.
- **Query performance**: PostgreSQL handles partitioned queries efficiently with proper indexing. A composite index on `(WorkspaceId, Id)` gives near-native performance for single-tenant queries.
- **Cross-tenant analytics**: For platform-level features (admin dashboard, billing), shared database allows querying across tenants without cross-database joins — impossible with database-per-tenant.

## Tradeoffs

| Aspect | Advantage | Disadvantage |
|--------|-----------|--------------|
| Data isolation | Logical isolation via filters | No physical isolation — bugs in filters leak data |
| Cost | Single database instance | Shared resource contention at scale |
| Migrations | One migration for all tenants | Schema changes affect all tenants simultaneously |
| Backup/restore | Simple — one database | Cannot restore individual tenant data |
| Compliance | Adequate for most SaaS | Some regulations require physical isolation |
| Scaling | Easy to start | Requires sharding/partitioning at high tenant count |

## Future Evolution

- **Row-level security (RLS)**: PostgreSQL supports RLS policies that enforce tenant isolation at the database engine level, not just application filters. We will add RLS policies as a safety net against application-level filter bugs.
- **Horizontal partitioning**: When a single database cannot handle tenant volume, partition by `WorkspaceId` using PostgreSQL declarative partitioning. This keeps the query interface identical while distributing storage.
- **Schema-per-tenant for premium**: If premium tenants require stronger isolation guarantees, offer schema-per-tenant as an upgrade tier. The EF Core infrastructure supports this via schema-per-entity configuration.
- **Tenant-specific indexes**: As query patterns diverge across tenants, add tenant-specific covering indexes without affecting other tenants' query plans.