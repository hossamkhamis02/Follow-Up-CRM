# ADR-002: Vertical Slice Architecture

**Status**: Accepted  
**Date**: 2025-01-15  
**Deciders**: Engineering Team

## Context

We need an organizational pattern for code within each module of FollowUp CRM. Traditional layered architecture (Domain/Application/Infrastructure/Presentation) and vertical slice architecture are the two primary candidates.

In **layered architecture**, code is grouped by technical concern: all controllers in one folder, all services in another, all repositories in a third. A single feature (e.g., "Create Customer") is scattered across multiple layers.

In **vertical slice architecture**, code is grouped by feature. Each feature (command or query) is a self-contained unit containing its handler, request, response, validator, and any module-specific logic — all in one file or folder.

## Decision

We adopt **Vertical Slice Architecture** using MediatR as the dispatch mechanism. Each feature is represented as:

- A **Command** or **Query** record (request DTO)
- A **Handler** class containing all logic for that feature
- A **Validator** (FluentValidation) co-located with the feature
- **Endpoint mapping** in the module's `Features` registration

Feature files are organized per module:

```
Modules/Customers/Features/
├── CreateCustomer/
│   ├── CreateCustomerCommand.cs    // Request + Handler in one file
│   └── CreateCustomerValidator.cs
├── GetCustomer/
│   ├── GetCustomerQuery.cs
└── GetCustomerList/
│   ├── GetCustomerListQuery.cs
```

Controllers are thin routing shells — they receive HTTP requests, dispatch to MediatR, and return `Result<T>` responses. No business logic lives in the presentation layer.

## Rationale

- **High cohesion**: Everything about "Create Customer" is in one place. No mental jumps across layers. A developer can understand a full feature by reading one file.
- **Low coupling**: Features do not reference each other. There is no `CustomerService` that aggregates multiple operations. Each handler stands alone.
- **Easy navigation**: Finding code is trivial — look in the module, find the feature folder. No "which layer does this belong to?" debates.
- **Deletes are safe**: Removing a feature means deleting its folder. No orphaned methods in shared service classes, no cascading changes across layers.
- **Natural CQRS alignment**: Commands and Queries are separate by definition. Write operations go through Command handlers; read operations go through Query handlers. No mixed read/write services.
- **Matches the modular monolith**: Modules are the macro-boundary; features are the micro-boundary. Together they create a two-level hierarchy that scales from "what does this module do?" to "how does this specific operation work?".

## Tradeoffs

| Aspect | Advantage | Disadvantage |
|--------|-----------|--------------|
| Discoverability | One file per feature | More files overall vs. layered |
| Code reuse | No accidental coupling | Intentional sharing must be explicit (Shared project) |
| Cross-cutting | MediatR behaviors (logging, validation) | Duplicate mapping logic across similar features |
| Testing | Test one feature at a time | Integration tests span multiple features naturally |
| Refactoring | Feature-local changes are safe | Cross-feature refactoring requires touching multiple slices |

## Future Evolution

- **Shared abstractions**: When patterns emerge across features (e.g., paginated list queries), extract them into `FollowUpCrm.Shared` as reusable base classes or extension methods — not as service classes.
- **Architecture tests**: Enforce that feature handlers only reference their own module's entities and `FollowUpCrm.Shared`. Cross-module calls must go through MediatR or explicit interfaces.
- **Minimal API endpoints**: Currently using controller-based routing. As features stabilize, migrate to Minimal API endpoint groups for reduced ceremony — each feature maps its own route directly from the handler file.
- **Feature flags**: Vertical slices make feature-flagging natural. Each feature's endpoint can be conditionally registered based on configuration.