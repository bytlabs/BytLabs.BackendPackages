---
name: bytlabs-domain-modeling
description: Use when creating or changing a BytLabs domain model - aggregates, entities, value objects, domain events, business rules, soft delete, audit fields, dynamic data, or state machines in BytLabs.Domain and BytLabs.States.Domain.
---

# BytLabs domain modelling

The domain layer references no other BytLabs package and no infrastructure. Keep it that way.

## Type inventory

| Area | Types |
|------|-------|
| Entities | `Entity<TId>`, `AggregateRootBase<TId>`, `IEntity`, `IAggregateRoot<TId>`, `IEntityId<TId>`, `ISoftDeletable` |
| Value objects | `ValueObject`, `Metadata.EntityMetadata`, `Metadata.SubEntityMetadata` |
| Domain events | `IDomainEvent`, `IDomainEvent<TId>`, `IDomainEvent<TId, TData>`, `DomainEventBase`, `DomainEventBase<TId>`, `DomainEventBase<TId, TData>` |
| Audit | `IAuditable`, `AuditInfo` |
| Dynamic data | `IHaveDynamicData` |
| Business rules | `BusinessRule<T>`, `AggregateBusinessRule<T>`, `BusinessRuleException` |
| Exceptions | `DomainException` |

Namespaces: `BytLabs.Domain.Entities`, `.ValueObjects`, `.DomainEvents`, `.Audit`,
`.DynamicData`, `.BusinessRules`.

## Writing an aggregate

```csharp
public sealed class Product : AggregateRootBase<Guid>, ISoftDeletable
{
    public string Name { get; private set; }
    public bool IsDeleted { get; private set; }

    private Product(Guid id, string name) : base(id) => Name = name;

    public static Product Create(Guid id, string name)
    {
        var product = new Product(id, name);
        product.AddDomainEvent(new ProductCreatedEvent(id));
        return product;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");
        Name = name;
    }
}
```

Conventions:

- `sealed`, with a `private` constructor and a `static` factory.
- Every property `private set` (or `protected init` for `Id`, which `Entity<TId>` owns). Mutation
  happens only through methods.
- `AggregateRootBase<TId>` provides `AddDomainEvent(...)`, `DomainEvents`
  (`IReadOnlyCollection<IDomainEvent>`) and `ClearDomainEvents()`; the base constructor
  null-guards the id.
- **Aggregate roots are always auditable.** `IAggregateRoot<TId>` inherits `IAuditable`, and
  `AggregateRootBase` implements `AuditInfo` for you — do not add it yourself.
- `Guid` is the conventional identity type.

## Value objects

```csharp
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency) => (Amount, Currency) = (amount, currency);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

`GetEqualityComponents()` is `protected abstract IEnumerable<object>`. Equality, `==`/`!=` and
`GetHashCode` all derive from it. Value objects are immutable and have no identity.

## Domain events

Raise them inside aggregate methods with `AddDomainEvent`. They are **not** published immediately
— the data layer dispatches them through MediatR after the repository save succeeds, via
`AddDomainEventsDecorator`. Handlers live in the application layer:

```csharp
public class ProductCreatedHandler : DomainEventHandler<ProductCreatedEvent>
{
    protected override Task HandleDomainEvent(ProductCreatedEvent e, CancellationToken ct) { ... }
}
```

`DomainEventHandler<T>` is in `BytLabs.Application` — see `bytlabs-cqrs`. If nothing dispatches
(no `AddMongoRepository`/`AddEfRepository`, which register the decorator), events accumulate and
are never published.

## Business rules

`BusinessRule<T>` derives from FluentValidation's `AbstractValidator<T>`, so a rule is written
with the FluentValidation DSL:

```csharp
public class ProductNameRule : BusinessRule<Product>
{
    public ProductNameRule() =>
        RuleFor(p => p.Name).NotEmpty().MaximumLength(200);
}
```

`AggregateBusinessRule<T>` is a composite — `new AggregateBusinessRule<Product>(rule1, rule2)`
exposes them as `Policies`. Violations throw `BusinessRuleException`, which carries
`IEnumerable<ValidationFailure> Errors`. Prefer rules over `if`/`throw` scattered through methods;
`bytlabs-graphql` surfaces `BusinessRuleException` as a typed GraphQL error.

## Opt-in marker interfaces

| Interface | Member | Effect |
|---|---|---|
| `ISoftDeletable` | `bool IsDeleted { get; }` | The data layer filters deleted rows. Mongo also offers `ExcludeSoftDeletedEntites()` on aggregate fluents. |
| `IHaveDynamicData` | `JsonElement Data { get; }` | Schema-less JSON on the aggregate, queryable and filterable. See `bytlabs-data-access` and `bytlabs-graphql`. |
| `IAuditable` | `AuditInfo AuditInfo { get; }` | Populated automatically. Already inherited by every aggregate root. |

## State machines (`BytLabs.States.Domain`)

Optional package for aggregates with a formal lifecycle. Concepts:

- `StateMachineAggregateBase` owns `States` and `Transitions`, and exposes `Fire(trigger, entity)`
  and `Goto(state, entity)`.
- `StateBase` is a state; `TransitionBase` has `From?`, `To`, an optional `Trigger`, and a set of
  `TransitionRule`s.
- `Trigger` is a value object naming an event; `TransitionRule` is a named RulesEngine expression
  evaluated against the entity via `Evaluate(...)`.
- The aggregate derives from `StatefulAggregateBase<...>`, which takes **seven** type parameters.
  Do not write that declaration from memory — read it with `bytlabs-api-lookup`.

For any signature not shown here, use `bytlabs-api-lookup` — do not guess.
