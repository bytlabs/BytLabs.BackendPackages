# BytLabs.Domain

Domain-driven design building blocks: entities, aggregate roots, value objects, domain events,
audit info, business rules, and dynamic-data support. This package has no infrastructure
dependencies — it is the pure domain core every service builds on.

## Install

```xml
<PackageReference Include="BytLabs.Domain" />
```

(Version is managed centrally via `Directory.Packages.props` → `BytLabsPackageVersion`.)

## What's inside

| Area | Types |
|------|-------|
| Entities | `Entity<TId>`, `AggregateRootBase<TId>`, `IEntity`, `IAggregateRoot<TId>`, `IEntityId<TId>`, `ISoftDeletable` |
| Value objects | `ValueObject`, `Metadata.EntityMetadata`, `Metadata.SubEntityMetadata` |
| Domain events | `IDomainEvent`, `DomainEventBase`, `DomainEventBase<TId, TData>` |
| Audit | `IAuditable`, `AuditInfo` |
| Dynamic data | `IHaveDynamicData` |
| Business rules | `BusinessRule<T>`, `AggregateBusinessRule<T>`, `BusinessRuleException` |
| Exceptions | `DomainException` |

## Core concepts & usage

### Entity

Base class for any entity with identity. `Id` is `protected init` (set once at construction).

```csharp
public sealed class ProductVariant : Entity<Guid>
{
    public string Sku { get; private set; }
    private ProductVariant(Guid id, string sku) : base(id) => Sku = sku;
    public static ProductVariant Create(Guid id, string sku) => new(id, sku);
}
```

### Aggregate root + domain events

`AggregateRootBase<TId>` extends `Entity<TId>`, owns a list of domain events, and carries an
`AuditInfo`. Raise events from factory methods/mutators with `AddDomainEvent`; the infrastructure
dispatches and clears them after persistence (`DomainEvents`, `ClearDomainEvents()`).

```csharp
public sealed class Product : AggregateRootBase<Guid>
{
    public string Name { get; private set; }

    private Product(Guid id, string name) : base(id) => Name = name;

    public static Product Create(Guid id, string name)
    {
        var product = new Product(id, name);
        product.AddDomainEvent(new ProductCreated(id, name));
        return product;
    }
}
```

`AggregateRootBase` exposes: `IReadOnlyCollection<IDomainEvent> DomainEvents`, `AddDomainEvent(...)`,
`ClearDomainEvents()`, and `AuditInfo AuditInfo` (created/modified/deleted stamps).

### Domain events

`IDomainEvent : INotification` (MediatR) and requires `DateTime? CreatedAt` / `string? CreatedBy`.
Derive events from `DomainEventBase` (these members are supplied) — note a **record cannot inherit a
non-record base**, so payload-less events must be classes.

```csharp
// With payload
public class ProductCreated(Guid id, CreateProduct data) : DomainEventBase<Guid, CreateProduct>(id, data);

// Without payload
public class ProductArchived(Guid productId) : DomainEventBase
{
    public Guid ProductId { get; } = productId;
}
```

Handle them with `BytLabs.Application.DomainEvents.DomainEventHandler<TEvent>`.

### Value objects

`ValueObject` gives value-based equality (`==`, `!=`, `Equals`, `GetHashCode`) — implement
`GetEqualityComponents()`.

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    public Money(decimal amount, string currency) { Amount = amount; Currency = currency; }
    protected override IEnumerable<object> GetEqualityComponents() { yield return Amount; yield return Currency; }
}
```

`EntityMetadata(type, id)` is a ready-made value object for referencing another entity by type+id.

### Soft delete

Implement `ISoftDeletable` (`bool IsDeleted`) so the entity can be flagged instead of physically
removed; query helpers in `BytLabs.DataAccess.MongoDB` exclude these.

```csharp
public sealed class Product : AggregateRootBase<Guid>, ISoftDeletable
{
    public bool IsDeleted { get; private set; }
    public void Remove() { IsDeleted = true; AddDomainEvent(new ProductArchived(Id)); }
}
```

### Dynamic data

Implement `IHaveDynamicData` (`JsonElement Data`) to carry schema-less, caller-defined fields. The
MongoDB and GraphQL packages can filter on this field.

```csharp
public sealed class Product : AggregateRootBase<Guid>, IHaveDynamicData
{
    public JsonElement Data { get; private set; }
}
```

### Audit

`IAuditable` exposes a nested `AuditInfo AuditInfo` with `CreatedAt/CreatedBy`,
`LastModifiedAt/LastModifiedBy`, `DeletedAt/DeletedBy`. `AggregateRootBase` already carries an
`AuditInfo`; the data-access layer stamps it automatically. Read DTOs typically implement `IAuditable`
too so audit data flows to the API.

### Business rules (FluentValidation-based)

`BusinessRule<T>` is an `AbstractValidator<T>` that throws `BusinessRuleException` (carrying the
`ValidationFailure`s) instead of FluentValidation's default. `AggregateBusinessRule<T>` composes
several rules and validates them together.

```csharp
public class ProductNameRule : BusinessRule<Product>
{
    public ProductNameRule() => RuleFor(p => p.Name).NotEmpty().MaximumLength(200);
}

public class ProductRules : AggregateBusinessRule<Product>
{
    public ProductRules() : base(new ProductNameRule(), /* ...more rules */) { }
}

// In the aggregate:
new ProductRules().ValidateAndThrow(product); // throws BusinessRuleException on failure
```

### Exceptions

`DomainException(message, code?, property?)` signals a violated domain invariant. The GraphQL layer
maps it to a `BusinessError` on mutation payloads.

```csharp
if (!items.Any())
    throw new DomainException("An order must have at least one item.");
```

## Related packages
- [BytLabs.Application](BytLabs.Application.md) — CQRS, repositories, domain-event handlers built on these types.
- [BytLabs.DataAccess.MongoDB](BytLabs.DataAccess.MongoDB.md) — persists aggregates; honors `ISoftDeletable`/`IHaveDynamicData`.
- [BytLabs.Api.Graphql](BytLabs.Api.Graphql.md) — exposes aggregates/DTOs; maps `DomainException` → `BusinessError`.
