---
name: bytlabs
description: Use when working in a .NET service that references the BytLabs backend packages (BytLabs.Domain, BytLabs.Application, BytLabs.DataAccess, BytLabs.Api, BytLabs.Api.Graphql, BytLabs.Multitenancy, BytLabs.Observability) - explains the architecture and routes to the right BytLabs skill.
---

# BytLabs backend packages

An opinionated foundation for .NET 8 microservices: DDD, CQRS, MongoDB or EF persistence,
database-per-tenant multitenancy, HotChocolate GraphQL, and OpenTelemetry observability.

**These skills were authored against package version 5.2.0.** When the service has a newer
version, the installed package is the authority — use `bytlabs-api-lookup`.

## Where to go

| The task | Skill |
|---|---|
| Exact signature, or does a type exist | `bytlabs-api-lookup` |
| New service, or adding BytLabs to an existing one; `Program.cs` wiring | `bytlabs-service-setup` |
| Aggregates, entities, value objects, domain events, business rules, state machines | `bytlabs-domain-modeling` |
| Commands, queries, handlers, validators, `AddCQS` | `bytlabs-cqrs` |
| Repositories, persistence, transactions, MongoDB or EF | `bytlabs-data-access` |
| GraphQL schema, mutations, typed errors, filtering | `bytlabs-graphql` |
| Tenant resolution, logging, tracing, metrics, health checks | `bytlabs-cross-cutting` |

## Architecture

Four layers. Dependencies point inward; the domain depends on nothing.

```
API             BytLabs.Api, BytLabs.Api.Graphql    GraphQL endpoints, host, typed errors
  ↓
Application     BytLabs.Application                 commands, queries, handlers, validation
  ↓
Domain          BytLabs.Domain (+ States.Domain)    aggregates, value objects, events, rules
  ↑
Infrastructure  BytLabs.DataAccess(.MongoDB|.EntityFramework)   persistence and DI wiring

Cross-cutting   BytLabs.Multitenancy, BytLabs.Observability
```

Rules that hold across every BytLabs service:

- The domain layer references no other BytLabs package and no infrastructure.
- Handlers depend on `IRepository<TAggregate, TId>`, never on a concrete `MongoRepository` or
  `DbContext`.
- Aggregates are mutated only through their own methods; setters stay `private`.
- One repository per aggregate root, registered explicitly.
- Tenancy is ambient — resolved per request and applied by the data layer. Never pass a tenant id
  through a command or a repository call.

## Package catalog

| Package | Role |
|---|---|
| `BytLabs.Domain` | DDD building blocks |
| `BytLabs.Application` | CQRS, repository abstractions, user context |
| `BytLabs.DataAccess` | Provider-agnostic unit-of-work, transactions, event dispatch |
| `BytLabs.DataAccess.MongoDB` | MongoDB persistence (the default) |
| `BytLabs.DataAccess.EntityFramework` | EF Core persistence (the alternative) |
| `BytLabs.Api` | `ApiServiceBuilder` fluent host |
| `BytLabs.Api.Graphql` | HotChocolate with BytLabs defaults |
| `BytLabs.Multitenancy` | Tenant resolution, database-per-tenant |
| `BytLabs.Observability` | Serilog + OpenTelemetry + health checks |
| `BytLabs.States.Domain` | State-machine aggregates (optional) |
| `BytLabs.Infrastructure` | Placeholder; nearly empty |

## Before writing code

Confirm which packages the service actually references (`Directory.Packages.props` or the
`.csproj` files). Persistence is either MongoDB or Entity Framework, and the GraphQL package is
optional — do not assume the full stack is present.

If you find a reference to `BytLabs.Hotchocolate`, that is the accidental rename published as
`5.2.0-alpha.123`–`.125`. It was reverted in `5.2.0`; the package, assembly and namespaces are all
`BytLabs.Api.Graphql` again. Tell the user to move back.
