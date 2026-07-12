# BytLabs.DataAccess.EntityFramework

Entity Framework Core implementation of the data-access abstractions: a generic repository,
config-driven per-tenant `DbContext` resolution, a unit of work over EF transactions, model helpers,
and a DbContext health check. Provider-agnostic — you choose Npgsql/SqlServer/etc. in your app.

## Install

```xml
<PackageReference Include="BytLabs.DataAccess.EntityFramework" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />   <!-- or your EF provider -->
```

## What's inside

| Type / method | Purpose |
|---|---|
| `AddEntityFrameworkDatabase<TDbContext>(config, configureProvider)` | Registers per-tenant `DbContext`, `IUnitOfWork`, command transactions, and a DbContext health check |
| `AddEfRepository<TEntity,TId>()` | Registers `IRepository<TEntity,TId>` (`EfRepository`) + domain-event decorator |
| `EfDatabaseConfiguration` | `Tenants` (`TenantId` + `ConnectionString`) + inherited `UseTransactions` |
| `EfDatabaseFactory<TDbContext>` | Resolves/caches a per-tenant `DbContext` from the tenants array |
| `ModelBuilder.IgnoreDomainEvents(assemblies)` | Unmaps `DomainEvents` for all aggregate roots in the given assemblies |
| `IQueryable<T>.IncludeAggregateEntities(context)` | Eager-loads an aggregate's navigations |

## Registration

```csharp
var efConfig = configuration.GetConfiguration<EfDatabaseConfiguration>();

services.AddEntityFrameworkDatabase<AppDbContext>(efConfig,
        (options, connectionString) => options.UseNpgsql(connectionString))
    .AddEfRepository<Order, Guid>()
    .AddEfRepository<Product, Guid>();
```

`appsettings.json`:

```jsonc
{
  "EfDatabaseConfiguration": {
    "UseTransactions": true,
    "Tenants": [
      { "TenantId": "tenant-a", "ConnectionString": "Host=...;Database=app_tenant_a" },
      { "TenantId": "tenant-b", "ConnectionString": "Host=...;Database=app_tenant_b" }
    ]
  }
}
```

Your `DbContext` must expose a `DbContext(DbContextOptions<TDbContext>)` constructor and unmap domain
events:

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ... entity configuration ...
        modelBuilder.IgnoreDomainEvents(typeof(Order).Assembly);
    }
}
```

## Multitenancy

Tenant isolation is physical and automatic: `AddEntityFrameworkDatabase` registers a **per-request**
`DbContext` resolved via `ITenantIdProvider` → `EfDatabaseFactory.GetDbContextForTenant`, using the
connection string configured for that tenant in `Tenants`. An unconfigured tenant throws
`InfrastructureException`.

## Transactions & persistence

Unlike MongoDB (where each operation writes immediately), EF **defers writes** to `SaveChangesAsync`,
which runs inside the unit of work's `CommitAsync` — invoked by the command transaction decorator from
[BytLabs.DataAccess](BytLabs.DataAccess.md). Therefore `UseTransactions` must be `true` (the default);
`AddEntityFrameworkDatabase` throws if it is `false`.

## Usage

Inject `IRepository<TEntity,TId>` in command handlers (from [BytLabs.Application](BytLabs.Application.md)):

```csharp
var order = await _repository.GetByIdAsync(id, ct);   // throws EntityNotFoundException if missing
order.Complete();
await _repository.UpdateAsync(order, ct);              // flushed on command commit; events dispatch after
```

## Related packages
- [BytLabs.DataAccess](BytLabs.DataAccess.md) — transaction + domain-event decorators it builds on.
- [BytLabs.DataAccess.MongoDB](BytLabs.DataAccess.MongoDB.md) — the MongoDB sibling implementation.
- [BytLabs.Multitenancy](BytLabs.Multitenancy.md) — supplies the tenant id used for DbContext selection.
- [BytLabs.Application](BytLabs.Application.md) — `IRepository`/`IUnitOfWork` contracts.
