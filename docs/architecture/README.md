# Architecture Decision Records

This folder contains ADRs for FollowUp CRM. Each ADR documents a significant architectural decision, including context, rationale, tradeoffs, and future evolution.

## ADR Index

| ADR  | Title                                    | Status   | File |
|------|------------------------------------------|----------|------|
| 001  | Modular Monolith over Microservices      | Accepted | [ADR-001-modular-monolith.md](ADR-001-modular-monolith.md) |
| 002  | Vertical Slice Architecture              | Accepted | [ADR-002-vertical-slice-architecture.md](ADR-002-vertical-slice-architecture.md) |
| 003  | No Repository Pattern                    | Accepted | *(pending)* |
| 004  | Result<T> over Exceptions for Failures   | Accepted | *(pending)* |
| 005  | MediatR CQRS separation                  | Accepted | *(pending)* |
| 006  | Angular Standalone Components            | Accepted | *(pending)* |
| 007  | Shared Database Multi-Tenancy            | Accepted | [ADR-007-shared-database-multi-tenancy.md](ADR-007-shared-database-multi-tenancy.md) |
| 008  | JWT Authentication Strategy              | Accepted | [ADR-008-jwt-authentication.md](ADR-008-jwt-authentication.md) |
| 009  | RBAC Permission Model                    | Accepted | [ADR-009-rbac-permission-model.md](ADR-009-rbac-permission-model.md) |
| 010  | Soft Delete Strategy                     | Accepted | [ADR-010-soft-delete-strategy.md](ADR-010-soft-delete-strategy.md) |
| 011  | Single Customer Ownership Model          | Accepted | [ADR-011-single-customer-ownership.md](ADR-011-single-customer-ownership.md) |
| 012  | Responsive SaaS UI Strategy              | Accepted | [ADR-012-responsive-saas-ui.md](ADR-012-responsive-saas-ui.md) |
| 013  | .NET 8 + PostgreSQL + Angular Stack      | Accepted | [ADR-013-technology-stack.md](ADR-013-technology-stack.md) |

## Creating New ADRs

1. Copy the template: `ADR-NNN-title-with-dashes.md`
2. Fill in: Status, Date, Deciders, Context, Decision, Rationale, Tradeoffs, Future Evolution
3. Add entry to the index above
4. Number sequentially from the last ADR