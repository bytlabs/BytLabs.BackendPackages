---
name: bytlabs-graphql
description: Use when working on the GraphQL layer of a BytLabs service - AddGraphQLService, mutation conventions, registering command input types and DTO object types, typed business and validation errors, projections, filtering, sorting, and dynamic-data inputs in BytLabs.Api.Graphql.
---

# BytLabs GraphQL

Package: **`BytLabs.Api.Graphql`**, built on HotChocolate 16.3.

Versions `5.2.0-alpha.123`–`.125` were published as `BytLabs.Hotchocolate`, with the assembly and
namespaces renamed to match, by an accidental project rename. Reverted in `5.2.0`. A service with
`using BytLabs.Hotchocolate...` should move back to `BytLabs.Api.Graphql`.

## The real package surface

Verified against the source. Everything else you may have seen is consumer-side.

| Member | Purpose |
|---|---|
| `AddGraphQLService()` | `AddGraphQLServer()` + BytLabs defaults |
| `AddDynamicDataTypes()` | Registers `DataOperationFilterInputType`, the dynamic-data filter input |
| `AddDefaultQuerySettings(name)` | Paging (max 50) + projections, filtering, sorting, cursor paging |
| `AddCommandType<TCommand>()` | Registers a command as a GraphQL **input** type |
| `AddDtoType<T>()` | Registers a DTO as an **object** type via `DtoType<T>` |
| `AddDtoFilterType<TDto>()` / `AddDtoSortType<TDto>()` / `AddDtoDynamicSortType<TDto>()` | Filter and sort inputs for a DTO |
| `AddAggregateFilterType<TAgg,TId>()` / `AddAggregateSortType<TAgg,TId>()` | Dynamic-data-aware filter/sort inputs for an aggregate |

Public types: `DtoType<T>`, `DataOperationFilterInputType`, `AggregateFilterInput<TAgg,TId>`,
`AggregateSortInput<TAgg,TId>`, `DtoFilterInput<TDto>`, `DtoSortInput<TDto>`,
`DtoDynamicSortInput<TDto>`, `BusinessError` (record), `ValidationError` (record), `FieldError`,
`GlobalErrorFilter`, `ErrorLoggingDiagnosticsEventListener`.

> **`AddCommandTypes()`, `AddDtoTypes()`, `AddAggregateTypes()` and `AddMongoDbQuerySettings()`
> are not in this package.** The README's quick tour shows them unannotated, but they are
> consumer-side grouping helpers from the service template that batch the singular calls above.
> If the service has not defined them, call `AddCommandType<T>()` / `AddDtoType<T>()` directly.

## Registration

Inside `WithServiceConfiguration` (see `bytlabs-service-setup`):

```csharp
services.AddGraphQLService()
    .AddDynamicDataTypes()
    .AddCommandType<CreateProductCommand>()
    .AddDtoType<ProductDto>()
    .AddAggregateFilterType<Product, Guid>()
    .AddAggregateSortType<Product, Guid>()
    .AddMutationType<Mutation>()
    .AddQueryType<Query>()
    .ModifyPagingOptions(o => o.IncludeTotalCount = true);
```

and `app.MapGraphQL()` inside the `BuildWebApp` delegate.

`AddGraphQLService()` already enables **mutation conventions**, **authorization**,
**observability**, and runtime type mappings (`Guid` → `IdType`). Do **not** re-add mutation
conventions per service.

## Naming conventions

The registration helpers rename types for you. Do not hand-write the GraphQL names:

| C# type | Registered as | Rule |
|---|---|---|
| `CreateProductCommand` | input `CreateProductInput` | `Command`/`Dto` suffix → `Input` |
| `ProductDto` | object type `Product` | `Dto` suffix stripped |

Mutation conventions then generate `{Name}Input`, `{Name}Payload` and `{Name}Error` with an
`input` argument and an `errors` field.

## Mutations

A mutation method takes the command and returns the DTO; dispatch through MediatR and let the
conventions wrap the payload:

```csharp
public class Mutation
{
    public async Task<ProductDto> CreateProduct(CreateProductCommand input, [Service] IMediator mediator)
        => await mediator.Send(input);
}
```

Exceptions map to typed errors rather than generic GraphQL errors:

| Thrown | Surfaces as |
|---|---|
| `BusinessRuleException` (domain) | `BusinessError` |
| `CommandValidationException` | `ValidationError` + `FieldError` list |
| `EntityNotFoundException`, `ApplicationOperationException` | GraphQL error via `GlobalErrorFilter` |

`GlobalErrorFilter` and `ErrorLoggingDiagnosticsEventListener` handle translation and logging.
See `bytlabs-cqrs` for which layer throws what.

## Queries

`AddDefaultQuerySettings(name)` applies paging (max 50), projections, filtering, sorting and
cursor paging. Extend `DtoType<T>` to add computed or resolved fields:

```csharp
public class ProductType : DtoType<ProductDto>
{
    protected override void Configure(IObjectTypeDescriptor<ProductDto> descriptor)
    {
        base.Configure(descriptor);
        descriptor.Field("displayName").Resolve(ctx => ...);
    }
}
```

## Dynamic data

`AddDynamicDataTypes()` registers the filter input type, and
`AddAggregateFilterType<,>`/`AddAggregateSortType<,>` produce dynamic-data-aware inputs for an
aggregate. The filter tree (`InputFilteringDynamicData`, `DataOperationFilter`, `FilterOperation`,
`ValueKind`) lives in `BytLabs.Application`.

**Sorting is wired through to the data layer; filtering is not.** No provider translates a dynamic
-data filter into a query — see `bytlabs-data-access`. If a schema exposes such a filter, the
resolver has to apply it itself.

For any signature not shown here, use `bytlabs-api-lookup` — do not guess.
