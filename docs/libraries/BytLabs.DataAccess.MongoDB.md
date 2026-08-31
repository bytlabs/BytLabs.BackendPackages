# BytLabs.DataAccess.MongoDB

MongoDB implementation of the data-access abstractions: a generic repository, per-tenant database
resolution, BSON class-map/serializer setup, dynamic-data query helpers, and health checks.

## Install

```xml
<PackageReference Include="BytLabs.DataAccess.MongoDB" />
<PackageReference Include="HotChocolate.Data.MongoDb" />   <!-- if using GraphQL Mongo querying -->
```

> Requires a MongoDB server **≥ 4.2** (MongoDB.Driver 3.x). Use `mongo:7.0` for local/dev.

## What's inside

| Type / method | Purpose |
|---|---|
| `AddMongoDatabase(config)` | Registers conventions, base class maps, `JsonElement` serializer, `IMongoClient`, per-tenant `IMongoDatabase`, `IUnitOfWork`, health checks |
| `AddMongoRepository<TEntity,TId>(collectionName?, autoMap?, configureEntity?)` | Registers `IRepository<TEntity,TId>` (`MongoRepository`) + collection + domain-event decorator |
| `MongoDatabaseConfiguration` | `DatabaseName`, `ConnectionString` (+ inherited `UseTransactions`, `IgnoreDatabaseNamingConvention`) |
| `IMongoDatabase.GetCollection<T>()` (extension) | Gets the collection using the conventional name for `T` |
| `MongoDatabaseFactory` / `MongoDatabaseHelper` | Resolve `"{baseName}-{tenantId}"` database per tenant; collection naming |
| `IAggregateFluentExtensions` | `ExcludeSoftDeletedEntites()`, `AppySortingWithDynamicData(order)` (both spellings are as written) |
| `JsonElementSerializer`, `MongoDbConventions` | Store `JsonElement` natively; camelCase + naming conventions |

## Registration

```csharp
var mongoConfig = configuration.GetConfiguration<MongoDatabaseConfiguration>();

services.AddMongoDatabase(mongoConfig)
    .AddMongoRepository<Order, Guid>()
    .AddMongoRepository<Product, Guid>();
```

`appsettings.json`:

```jsonc
{
  "MongoDatabaseConfiguration": {
    "DatabaseName": "microserviceTemplate",
    "ConnectionString": "mongodb://localhost:27017?retryWrites=false",
    "UseTransactions": false
  }
}
```

`AddMongoDatabase` automatically:
- registers MongoDB conventions (unless `IgnoreDatabaseNamingConvention`),
- maps `Entity<string>`/`Entity<Guid>` ids and unmaps `AggregateRootBase.DomainEvents`,
- registers a `JsonElement` serializer (for `IHaveDynamicData.Data`),
- registers a **per-request, per-tenant** `IMongoDatabase` (resolved via `ITenantIdProvider` →
  `MongoDatabaseFactory.GetDatabaseForTenant`, database name `"{DatabaseName}-{tenantId}"`),
- wires `IUnitOfWork` and health checks.

`AddMongoRepository<TEntity,TId>` registers the repository and the domain-event dispatch decorator,
and lets you customize the collection name / class map:

```csharp
services.AddMongoRepository<Product, Guid>(
    collectionName: "products",
    configureEntity: cm => cm.MapMember(p => p.Name).SetIsRequired(true));
```

## Usage

### Repository (in command handlers)

Inject `IRepository<TEntity,TId>` (from [BytLabs.Application](BytLabs.Application.md)):

```csharp
var product = await _repository.GetByIdAsync(id, ct);   // throws EntityNotFoundException if missing
product.Rename("New name");
await _repository.UpdateAsync(product, ct);             // domain events dispatch after this succeeds
```

### Custom queries (GraphQL resolvers)

Use the `IMongoDatabase` + `GetCollection<T>()` extension and project to a DTO:

```csharp
public IExecutable<ProductDto> GetProducts([Service] IMongoDatabase db)
    => db.GetCollection<Product>()
         .Aggregate()
         .Project(Builders<Product>.Projection.As<ProductDto>())
         .AsExecutable();
```

### Soft-delete + dynamic-data filtering/sorting

On a Mongo aggregate pipeline (for `IHaveDynamicData` + `ISoftDeletable` aggregates):

```csharp
db.GetCollection<Product>()
  .Aggregate()
  .ExcludeSoftDeletedEntites()                  // filters IsDeleted == true
  .AppySortingWithDynamicData(order)            // sort, incl. dynamic-data paths
  .Project(Builders<Product>.Projection.As<ProductDto>())
  .AsExecutable();
```

Both method names are spelled as shown — `Entites` and `Appy` are the actual identifiers.

> **Dynamic-data *filtering* is not implemented for MongoDB.** This package provides dynamic-data
> **sorting** only. The filter input types (`InputFilteringDynamicData`, `DataOperationFilter`,
> `FilterOperation`, `ValueKind`) exist in `BytLabs.Application` and are exposed through GraphQL by
> [BytLabs.Api.Graphql](BytLabs.Api.Graphql.md), but no Mongo translation layer consumes them.
> Apply such filters in your own query code.

### Multitenancy

Tenant isolation is automatic and physical: each tenant gets database `"{DatabaseName}-{tenantId}"`,
resolved from the request via [BytLabs.Multitenancy](BytLabs.Multitenancy.md). No tenant field or
per-query filter is needed in your code.

### BSON class maps for value objects / immutables

Register class maps in your infrastructure for value objects or constructor-based types whose
parameters don't match members (see [BytLabs.Domain](BytLabs.Domain.md) value objects). Constructor
parameter **names and types** should match members so the creator auto-configures.

## Related packages
- [BytLabs.DataAccess](BytLabs.DataAccess.md) — transaction + domain-event decorators it builds on.
- [BytLabs.Multitenancy](BytLabs.Multitenancy.md) — supplies the tenant id used for database selection.
- [BytLabs.Application](BytLabs.Application.md) — `IRepository`/`IUnitOfWork` contracts.
