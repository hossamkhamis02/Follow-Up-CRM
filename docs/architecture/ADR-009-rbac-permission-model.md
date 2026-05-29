# ADR-009: RBAC Permission Model

**Status**: Accepted  
**Date**: 2025-01-15  
**Deciders**: Engineering Team

## Context

FollowUp CRM needs an authorization model that controls what users can do within a workspace. Different roles need different capabilities — an admin can manage users and settings, a manager can view all customers, a sales rep can only see their own customers and follow-ups.

Two primary models were considered:

1. **Role-Based Access Control (RBAC)** — permissions are assigned to roles, roles are assigned to users. Users inherit permissions through their role membership.
2. **Attribute-Based Access Control (ABAC)** — access decisions are based on attributes (user role, resource owner, department, time of day, etc.). More flexible but significantly more complex.

## Decision

We adopt **RBAC** with workspace-scoped roles. Each workspace defines its own roles with associated permissions. A user's role determines their allowed operations within that workspace.

Structure:

```
Workspace (tenant)
  └── Role (defined per workspace)
      └── Permissions (module + action pairs)
  └── User → WorkspaceMembership → Role
```

Permission format: `{Module}.{Action}` — e.g., `Customers.Create`, `Customers.View`, `Customers.Edit`, `FollowUps.Delete`, `Permissions.ManageRoles`.

Default roles provided per workspace:

- **Owner**: full access (all permissions). Assigned to the workspace creator. Cannot be removed.
- **Admin**: all permissions except deleting the workspace and managing billing.
- **Manager**: view all customers/follow-ups, create/edit, cannot manage users or roles.
- **Member**: view/edit own customers/follow-ups only, limited create capabilities.

Implementation:

- ASP.NET Core `[Authorize(Roles = "Admin")]` for coarse role checks
- Custom `[RequirePermission("Customers.Create")]` attribute for fine-grained checks
- Permission claims loaded into JWT on login (see ADR-008)
- Permission module provides a `IPermissionService` for runtime checks in handlers

## Rationale

- **Simplicity**: RBAC is the simplest model that covers CRM authorization needs. "Is this user an Admin?" is an easy question to answer. ABAC would require evaluating multiple attributes per request — overkill for a CRM.
- **Workspace isolation**: Roles are scoped to each workspace. A user can be an Admin in Workspace A and a Member in Workspace B. No global roles that leak across tenants.
- **Audit clarity**: "User X accessed Customer Y because they have role Manager" is a clear audit trail. ABAC decisions are harder to explain.
- **UI mapping**: Role names map directly to UI sections. "Managers see the team dashboard; Members see their personal dashboard." This is straightforward to implement in Angular route guards.
- **Manageability**: Workspace admins can create custom roles by selecting permission checkboxes. The permission grid (`Module × Action`) is a natural UX pattern that non-technical users understand.

## Tradeoffs

| Aspect | Advantage | Disadvantage |
|--------|-----------|--------------|
| Flexibility | Simple, predictable | Cannot express conditional rules (e.g., "edit only if owner") |
| Scalability | Easy for small-medium orgs | Role explosion for fine-grained needs (many custom roles) |
| Owner-based access | Not supported by pure RBAC | Must layer ownership checks on top (see ADR-011) |
| Cross-module permissions | Module-level granularity | Cannot express "edit customers but not follow-ups" within a sub-module |
| Admin UX | Role = permission set, easy to manage | Adding a permission requires updating all affected roles |
| Performance | Claims in JWT, no DB lookup per request | JWT size grows with many permissions |

## Future Evolution

- **Ownership layer**: RBAC does not express "user can only edit customers they own." We layer ownership checks on top of RBAC (see ADR-011). The `RequirePermission` attribute grants module-level access; the handler enforces ownership within the operation.
- **Custom roles**: Allow workspace admins to define roles with arbitrary permission combinations. The Permissions module stores `Role → Permission` mappings in the database, loaded into JWT claims on login.
- **Permission groups**: When permission count grows (e.g., per-sub-module actions), introduce permission groups as aggregations. `Customers.FullAccess = Customers.{Create,View,Edit,Delete}`. This reduces JWT claim count and admin checkbox fatigue.
- **ABAC bridge**: If future requirements demand conditional access (e.g., "sales reps cannot delete follow-ups older than 30 days"), add ABAC-style policy evaluations as handler-level checks. The RBAC structure remains as the coarse-grained gate; ABAC policies refine access within the handler.