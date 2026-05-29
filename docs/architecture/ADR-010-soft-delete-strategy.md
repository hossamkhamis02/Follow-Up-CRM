# ADR-010: Soft Delete Strategy

**Status**: Accepted  
**Date**: 2025-01-15  
**Deciders**: Engineering Team

## Context

FollowUp CRM manages business-critical data (customers, follow-ups, interactions). Deleted data may need to be recovered — a sales rep accidentally deletes a customer, an admin needs audit history, or a compliance requirement mandates data retention. We need a deletion strategy that balances data safety, query performance, and storage efficiency.

Options considered:

1. **Hard delete** — rows are physically removed from the database. No recovery possible.
2. **Soft delete with boolean flag** — rows remain in the table with `IsDeleted = true`. Excluded from normal queries via global filter.
3. **Soft delete with timestamp** — rows remain with `DeletedAt` timestamp. Same filtering as boolean, with additional metadata (when deleted, who deleted).
4. **Event sourcing / audit log** — all changes recorded as events. Current state derived from replay.

## Decision

We adopt **soft delete with timestamp** (`DeletedAt`). Every entity that can be deleted includes:

- `DeletedAt DateTime?` — null when active, set to `DateTime.UtcNow` on deletion
- `DeletedBy string?` — null when active, set to the deleting user's ID on deletion

Entities implement an `ISoftDeletable` interface:

```csharp
public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
```

EF Core global query filter excludes soft-deleted rows from all normal queries:

```csharp
modelBuilder.Entity<Customer>().HasQueryFilter(e => e.DeletedAt == null);
```

Recovery is done by setting `DeletedAt = null` via a dedicated "restore" command. Full deletion (hard delete) is only available to workspace owners through a separate "purge" operation.

## Rationale

- **Data recovery**: Accidental deletion is the most common data-loss scenario in CRM applications. Soft delete provides instant recovery without database backups.
- **Audit trail**: `DeletedAt` + `DeletedBy` captures who deleted and when. This satisfies compliance and debugging needs without a separate audit log infrastructure.
- **Query consistency**: EF Core global filters ensure that soft-deleted rows never appear in normal queries. No risk of "forgot to add `.Where(x => !x.IsDeleted)`" — the filter is enforced at the ORM level.
- **Timestamp vs boolean**: `DeletedAt` provides more information than `IsDeleted`. The timestamp tells us when and enables time-based queries ("show all customers deleted last week"). A boolean only tells us whether.
- **Simplicity over event sourcing**: Event sourcing provides full history but requires significant infrastructure (event store, projection rebuild, complex queries). For a CRM, soft delete covers 95% of data recovery needs at 5% of the complexity.

## Tradeoffs

| Aspect | Advantage | Disadvantage |
|--------|-----------|--------------|
| Data safety | Instant recovery, audit trail | Deleted data accumulates, increases table size |
| Query performance | Global filter excludes deleted rows | Filter adds conditions to every query |
| Unique constraints | Unique indexes must include `DeletedAt` | More complex index definitions |
| Storage | Data preserved | Tables grow — need periodic purge |
| Complexity | Simple interface + filter | Two "delete" concepts (soft vs. hard) |
| Reporting | Deleted data available for analytics | Must use `.IgnoreQueryFilters()` explicitly |

## Future Evolution

- **Purge policy**: Implement a scheduled job that hard-deletes rows where `DeletedAt` is older than a configurable retention period (e.g., 90 days). This prevents unbounded table growth. Workspace admins can configure their retention window.
- **Unique index strategy**: PostgreSQL partial unique indexes (`WHERE DeletedAt IS NULL`) enforce uniqueness only on active rows, allowing re-creation of previously-deleted entities with the same name/email.
- **Archive table**: For high-volume entities (follow-ups, interactions), move soft-deleted rows to archive tables after the retention period. This keeps active tables small while preserving data in cost-effective storage.
- **Audit log extension**: When audit requirements grow beyond deletion tracking, add a dedicated `AuditLog` table recording all mutations (create, update, delete) with before/after snapshots. Soft delete remains for recovery; the audit log provides full history.