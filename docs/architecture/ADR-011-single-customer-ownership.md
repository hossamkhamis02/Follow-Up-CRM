# ADR-011: Single Customer Ownership Model

**Status**: Accepted  
**Date**: 2025-01-15  
**Deciders**: Engineering Team

## Context

In a CRM, customers (leads, contacts, accounts) are the core business entity. We must decide how customer records relate to users within a workspace. Two models are common:

1. **Shared ownership** — all workspace members can see and edit all customers. Collaboration happens on shared records.
2. **Single ownership** — each customer has one assigned owner (a user). The owner is primarily responsible for that customer's relationship. Others may have limited access based on their role.

3. **Team ownership** — customers are owned by a team/group, not an individual. All team members have full access.

## Decision

We adopt **single customer ownership**. Each customer record has an `OwnerId` (foreign key to a user within the workspace). The owner is the primary responsible party for that customer's follow-ups and communications.

Access rules:

- **Owner**: full access to their own customers (view, edit, delete, assign follow-ups)
- **Manager**: view and edit all customers in the workspace (oversight capability)
- **Admin**: all operations on all customers, including reassignment and purge
- **Member**: view and edit only their own customers. Cannot view other members' customers.

The `OwnerId` is set on customer creation (defaults to the creating user) and can be reassigned by managers/admins via a "transfer" operation.

Implementation:

- `OwnerId` column on `Customer` entity with foreign key to `User`
- EF Core query filter for Members: `e => e.OwnerId == currentUserId` (layered on top of workspace filter)
- Manager/Admin filters exclude ownership restriction: they see all workspace customers
- "Transfer customer" command available to managers/admins only

## Rationale

- **Accountability**: In sales-driven CRM workflows, a single owner means a single accountable person. "Who is responsible for this customer?" has one answer. Shared ownership creates diffusion of responsibility.
- **Data filtering**: Single ownership enables natural query partitioning. Each member sees only their portfolio. This reduces visual noise and improves focus — a sales rep's dashboard shows their 50 customers, not the workspace's 500.
- **Simple access control**: Ownership + RBAC (see ADR-009) creates a two-tier authorization model. RBAC determines what operations you can perform; ownership determines which customers you can perform them on. This is more intuitive than complex permission matrices.
- **Reassignment workflow**: When a sales rep leaves or a territory is reassigned, the "transfer" command moves all their customers to a new owner. This is a batch operation, not per-customer manual editing.
- **Reporting**: Ownership enables per-user performance metrics — "how many customers does User X manage?", "what is User X's follow-up completion rate?" — critical for sales management.

## Tradeoffs

| Aspect | Advantage | Disadvantage |
|--------|-----------|--------------|
| Accountability | Clear single responsible person | No collaborative editing on a customer |
| Filtering | Natural per-user data partitioning | Members cannot see colleagues' work |
| Onboarding | New rep gets an empty portfolio | No gradual handoff — must transfer explicitly |
| Team visibility | Managers see everything | Members feel isolated from team context |
| Flexibility | Simple model, easy to understand | Cannot model team-based accounts |
| Reassignment | Explicit transfer operation | Transfer latency during role changes |

## Future Evolution

- **Co-owners / collaborators**: Add a `CustomerCollaborator` table linking additional users to a customer with limited permissions (view-only, comment-only). The primary owner remains; collaborators have auxiliary access. This bridges the gap between single ownership and shared editing.
- **Team ownership**: Add a `Team` entity that groups users. Customers can be owned by a team instead of an individual. All team members inherit owner-level access. This is useful for account-based selling where multiple reps collaborate on large accounts.
- **Territory-based assignment**: Instead of manual reassignment, implement territory rules that auto-assign customers based on geography, industry, or size. New customers matching a territory rule are assigned to that territory's owner automatically.
- **Shared read access**: Allow members to view (but not edit) other members' customers for reference. The `CustomerViewPolicy` grants read access to all workspace members while restricting write access to the owner and managers. This reduces isolation while preserving edit accountability.