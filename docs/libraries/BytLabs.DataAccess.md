# BytLabs.DataAccess

Provider-agnostic persistence plumbing: unit-of-work, command-scoped transactions, and the
domain-event dispatch decorator. It defines the cross-cutting behaviors that a concrete provider
(e.g. [BytLabs.DataAccess.MongoDB](BytLabs.DataAccess.MongoDB.md)) plugs into. You normally don't
reference this directly — the MongoDB package calls it for you — but understanding it explains how
transactions and events work.

## Install

```xml
<PackageReference Include="BytLabs.DataAccess" />
```

## What's inside

| Type | Purpose |
|------|---------|
| `DatabaseConfiguration` | Base config: `UseTransactions`, `IgnoreDatabaseNamingConvention` |
| `AddDatabase<TConfig>(config)` | Registers config, `UnitOfWorkOptions`, `UnitOfWorkFactory`, and (if enabled) the transaction decorator |
| `AddDomainEventsDecorator<TAgg,TId>()` | Wraps an `IRepository<,>` so domain events publish after successful persistence |
| `CommandTransactionDecorator<,>` | MediatR pipeline behavior that runs each command in a transaction (commit on success, rollback on error; nesting-aware) |
| `DomainEventDispatcherDecorator<,>` | Repository decorator that dispatches `AggregateRootBase.DomainEvents` via MediatR after save |
| `UnitOfWorkFactory`, `UnitOfWorkOptions` | Unit-of-work creation; `UseTransactions` toggle |

## How it works

When a provider registers a repository it calls:

```csharp
services.AddDatabase(databaseConfiguration);          // UoW + (optional) transaction decorator
services.AddDomainEventsDecorator<Product, Guid>();   // events publish after persistence
```

- **Transactions** — if `DatabaseConfiguration.UseTransactions == true`, `AddDatabase` adds
  `CommandTransactionDecorator<,>` to the MediatR pipeline. Every command then runs inside a
  transaction: `BeforeRequest` → handler → `OnRequestFinished` (commit), or `OnRequestError`
  (rollback). The decorator is nesting-aware (only the outermost command owns the transaction).
- **Domain events** — `AddDomainEventsDecorator` wraps the repository with
  `DomainEventDispatcherDecorator`, which publishes each aggregate's `DomainEvents` through MediatR
  **after** the write succeeds, then clears them. This is what makes
  [`DomainEventHandler<T>`](BytLabs.Application.md) fire.

## Configuration

`DatabaseConfiguration` (and subclasses like `MongoDatabaseConfiguration`) controls behavior:

```jsonc
{
  "MongoDatabaseConfiguration": {
    "UseTransactions": false,                 // true → wrap commands in transactions (needs a replica set)
    "IgnoreDatabaseNamingConvention": false
  }
}
```

> Note: MongoDB transactions require a replica set. Keep `UseTransactions: false` for a standalone
> local Mongo.

## Related packages
- [BytLabs.DataAccess.MongoDB](BytLabs.DataAccess.MongoDB.md) — the MongoDB provider that consumes this.
- [BytLabs.Application](BytLabs.Application.md) — `IRepository`, `IUnitOfWork`, command pipeline.
