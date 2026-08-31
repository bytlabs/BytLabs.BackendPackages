---
name: bytlabs-data-access
description: Use when persisting BytLabs aggregates - registering repositories, choosing between MongoDB and Entity Framework, IRepository/IUnitOfWork, transactions, domain-event dispatch, per-tenant databases, and dynamic-data queries.
---

# BytLabs data access

## Choosing a provider

A service uses **one**, never both.

| | `BytLabs.DataAccess.MongoDB` | `BytLabs.DataAccess.EntityFramework` |
|---|---|---|
| Status | Default | Alternative |
| Tenancy | Database per tenant, `"{DatabaseName}-{tenantId}"`, derived automatically | Explicit `Tenants` array of `{ TenantId, ConnectionString }` |
| You supply | Connection string + database name | Your own `DbContext` **and** a provider delegate |
| Dynamic data | Sorting only (see below) | Sorting only |

## Provider-agnostic core (`BytLabs.DataAccess`)

| Type | Purpose |
|------|---------|
| `DatabaseConfiguration` | Base config: `UseTransactions`, `IgnoreDatabaseNamingConvention` |
| `AddDatabase<TConfig>(config)` | Registers config, `UnitOfWorkOptions`, `UnitOfWorkFactory`, and (if enabled) the transaction decorator |
| `AddDomainEventsDecorator<TAgg,TId>()` | Wraps an `IRepository<,>` so domain events publish after successful persistence |
| `CommandTransactionDecorator<,>` | MediatR behavior running each command in a transaction (commit on success, rollback on error; nesting-aware) |
| `DomainEventDispatcherDecorator<,>` | Repository decorator dispatching `DomainEvents` via MediatR after save |
| `UnitOfWorkFactory`, `UnitOfWorkOptions` | Unit-of-work creation; `UseTransactions` toggle |

The provider packages contribute only provider-specific pieces plus DI wiring.

## MongoDB registration

```csharp
services.AddMongoDatabase(config.GetConfiguration<MongoDatabaseConfiguration>())
    .AddMongoRepository<Product, Guid>();
```

| Member | Notes |
|---|---|
| `AddMongoDatabase(config)` | Conventions, base class maps, `JsonElement` serializer, `IMongoClient`, per-tenant `IMongoDatabase`, `IUnitOfWork`, health checks |
| `AddMongoRepository<TEntity,TIdentity>(collectionName?, autoMap?, configureEntity?)` | Registers `IRepository<,>` (`MongoRepository`), the collection, and the domain-event decorator |
| `MongoDatabaseConfiguration` | `DatabaseName`, `ConnectionString` (+ inherited `UseTransactions`, `IgnoreDatabaseNamingConvention`) |
| `IMongoDatabase.GetCollection<TAggregate>()` | Collection by conventional name |
| `MongoDatabaseFactory`, `MongoDatabaseHelper` | `GetDatabaseNameForTenant`, `CreateCollectionName` |
| `JsonElementSerializer`, `MongoDbConventions` | Store `JsonElement` natively; camelCase naming |

## Entity Framework registration

```csharp
services.AddEntityFrameworkDatabase<AppDbContext>(efConfig,
        (options, connectionString) => options.UseNpgsql(connectionString))
    .AddEfRepository<Order, Guid>()
    .AddEfRepository<Product, Guid>();
```

- `EfDatabaseConfiguration.Tenants` is a `List<EfTenantConfiguration>` of
  `{ TenantId, ConnectionString }`. `EfDatabaseFactory<TDbContext>` resolves and caches the
  `DbContext` for the current tenant.
- The package is provider-agnostic, so **you** pass the provider delegate
  (`UseNpgsql`, `UseSqlServer`, …).
- `AddEfRepository<TEntity,TId>()` registers the write-side `IRepository<,>` **and** a
  no-tracking read-side `IQueryable<TEntity>`.
- `modelBuilder.IgnoreDomainEvents(assemblies)` **must** be called in `OnModelCreating`, or EF
  tries to map the `DomainEvents` property and the model build fails.
- `IQueryable<T>.IncludeAggregateEntities(context)` eager-loads an aggregate's navigations.

## Spelling — these are the real identifiers

Two of these are misspelled in the source, and the soft-delete one **differs between providers**.
Copy them exactly; "correcting" them produces code that does not compile.

| Provider | Soft delete | Dynamic-data sort |
|---|---|---|
| MongoDB (`IAggregateFluent<T>`) | `ExcludeSoftDeletedEntites()` — no second `i` | `AppySortingWithDynamicData(order)` — no `l` |
| Entity Framework (`IQueryable<T>`) | `ExcludeSoftDeletedEntities()` — correctly spelled | `AppySortingWithDynamicData(order)` — no `l` |

```csharp
db.GetCollection<Product>()
  .Aggregate()
  .ExcludeSoftDeletedEntites()
  .AppySortingWithDynamicData(order)
  .Project(Builders<Product>.Projection.As<ProductDto>())
  .AsExecutable();
```

**Dynamic-data filtering is not implemented in either provider.** The filter input types
(`InputFilteringDynamicData`, `DataOperationFilter`, `FilterOperation`, `ValueKind`) live in
`BytLabs.Application` and are exposed through GraphQL, but no translation layer consumes them.
Only sorting is wired up. Do not call a `FilterData`, `FilterDataField`, or
`ApplyDynamicDataFilteration` method — they do not exist.

## One repository per aggregate root

Registration is explicit and per-aggregate. A missing `AddMongoRepository<,>` /
`AddEfRepository<,>` is a **runtime** DI resolution failure, not a compile error — the most common
symptom is a handler that will not construct. Never register a repository for a non-aggregate
entity; sub-entities are reached through their aggregate root.

## Transactions and domain events

With `UseTransactions` enabled, each command runs in a transaction that commits on success and
rolls back on error, nesting-aware. Domain events dispatch through MediatR only after a
successful save, via the decorator that `AddMongoRepository`/`AddEfRepository` installs.

Handlers should manage neither. If domain events never fire, the usual cause is a repository
registered without going through those helpers.

## Multitenancy is automatic

The tenant comes from `ITenantIdProvider`; the data layer selects the database. **Never pass a
tenant id into a repository call, a command, or a query.** See `bytlabs-cross-cutting`.

For any signature not shown here, use `bytlabs-api-lookup` — do not guess.
