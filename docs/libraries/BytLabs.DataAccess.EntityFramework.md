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
| `AddEfRepository<TEntity,TId>()` | Registers `IRepository<TEntity,TId>` (write side) + domain-event decorator **and** `IQueryable<TEntity>` (no-tracking, read side) |
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

## Service wiring

`AddEntityFrameworkDatabase<TDbContext>(...)` registers the shared, tenant-aware infrastructure:

| Service | Lifetime | Notes |
|---|---|---|
| `EfDatabaseConfiguration` | Singleton | The bound config (tenants + `UseTransactions`) |
| `EfDatabaseFactory<TDbContext>` | Singleton | Resolves/caches a per-tenant `DbContext` |
| `DbContext` | Scoped | Built **per request** for the current tenant via `ITenantIdProvider` → factory |
| `TDbContext` | Scoped | Same instance as `DbContext`, so you can inject your concrete type |
| `IUnitOfWork` (`EfUnitOfWork`) | Scoped | Transaction + `SaveChanges` on commit |
| DbContext health check | — | `AddDbContextCheck<TDbContext>()` |
| Command transaction decorator | Scoped | From [BytLabs.DataAccess](BytLabs.DataAccess.md) (via `AddDatabase`) |

`AddEfRepository<TEntity,TId>()` then registers, **per aggregate root**:

| Service | Lifetime | Side | Use for |
|---|---|---|---|
| `IRepository<TEntity,TId>` (`EfRepository` + domain-event decorator) | Scoped | Write | Commands |
| `IQueryable<TEntity>` (`DbSet.AsNoTracking()` over the tenant `DbContext`) | Scoped | Read | Queries |

## Commands vs. queries (read/write split)

Keep the two sides separate:

- **Commands** inject `IRepository<TEntity,TId>`. It tracks changes, stamps audit info, dispatches
  domain events, and flushes inside the command's transaction commit. Do **not** query through it for
  read models — its methods materialize full, tracked aggregates.
- **Queries** inject `IQueryable<TEntity>`. It is a **no-tracking** `DbSet`, so it is cheaper and safe
  for read-only projections. Compose LINQ and project straight to a DTO.

```csharp
// Command handler — writes go through the repository
public async Task Handle(CompleteOrder command, CancellationToken ct)
{
    var order = await _repository.GetByIdAsync(command.OrderId, ct);  // tracked
    order.Complete();
    await _repository.UpdateAsync(order, ct);   // flushed on commit; events dispatch after
}

// Query handler — reads go through IQueryable<TEntity>
public class GetOrderSummaries
{
    private readonly IQueryable<Order> _orders;   // injected, AsNoTracking
    public GetOrderSummaries(IQueryable<Order> orders) => _orders = orders;

    public Task<List<OrderSummaryDto>> Handle(CancellationToken ct) =>
        _orders
            .Where(o => o.Status == OrderStatus.Completed)
            .Select(o => new OrderSummaryDto(o.Id, o.CustomerName, o.TotalAmount))
            .ToListAsync(ct);
}
```

> `IQueryable<TEntity>` resolves the same per-tenant `DbContext` as the repository, so queries are
> automatically scoped to the current tenant. Because it is no-tracking, never persist entities loaded
> through it — take them through `IRepository` for any write.

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

## Related packages
- [BytLabs.DataAccess](BytLabs.DataAccess.md) — transaction + domain-event decorators it builds on.
- [BytLabs.DataAccess.MongoDB](BytLabs.DataAccess.MongoDB.md) — the MongoDB sibling implementation.
- [BytLabs.Multitenancy](BytLabs.Multitenancy.md) — supplies the tenant id used for DbContext selection.
- [BytLabs.Application](BytLabs.Application.md) — `IRepository`/`IUnitOfWork` contracts.
