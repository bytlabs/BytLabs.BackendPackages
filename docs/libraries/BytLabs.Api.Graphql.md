# BytLabs.Api.Graphql

HotChocolate GraphQL server setup with BytLabs conventions baked in: observability, mutation
conventions, authorization, runtime type mappings, typed error results, DTO/command/aggregate type
registration helpers, and dynamic-data filter input types.

## Install

```xml
<PackageReference Include="BytLabs.Api.Graphql" />
<PackageReference Include="HotChocolate.Data.MongoDb" />
```

## What's inside

| Type / method | Purpose |
|---|---|
| `AddGraphQLService()` | `AddGraphQLServer()` + BytLabs defaults (observability, mutation conventions, runtime type mappings, authorization) |
| `AddDynamicDataTypes()` | Registers the dynamic-data filter input type (`DataOperationFilterInputType`) |
| `AddDefaultQuerySettings(name)` | Paging (max 50) + projections/filtering/sorting + cursor paging |
| `AddCommandType<TCommand>()` | Registers a command as a GraphQL **input** type (`...Command`/`...Dto` → `...Input`) |
| `AddDtoType<T>()` | Registers a DTO as an **object** type via `DtoType<T>` (strips `Dto` from the name) |
| `AddAggregateFilterType<TAgg,TId>()` / `AddAggregateSortType<TAgg,TId>()` | Dynamic-data-aware filter/sort input types for an aggregate |
| `DtoType<T>` | Base object type for DTOs (extend it to add computed/resolved fields) |
| `ErrorTypes.Business.BusinessError`, `ErrorTypes.Validation.ValidationError` / `FieldError` | Typed mutation error results |
| `Observability.GlobalErrorFilter`, `ErrorLoggingDiagnosticsEventListener` | Error logging/translation |

## Registration

```csharp
services.AddGraphQLService()
    .AddMongoDbQuerySettings()      // from your Api.Utils (Mongo filtering/projection/sorting/paging)
    .AddDynamicDataTypes()          // dynamic-data 'where' input
    .AddCommandTypes()              // your AddCommandType<...>() group
    .AddDtoTypes()                  // your AddDtoType<...>()/AddType<CustomDtoType>() group
    .AddAggregateTypes()            // your AddAggregateFilterType/AddAggregateSortType group
    .AddMutationType<Mutation>()
    .AddQueryType<Query>()
    .ModifyPagingOptions(o => o.IncludeTotalCount = true);
```

`AddGraphQLService()` already enables **mutation conventions** (`{Name}Input`/`{Name}Payload`/
`{Name}Error`, `input` argument, `errors` field), **authorization**, **observability**, and runtime
type mappings (`Guid` → `IdType`). You do **not** re-add mutation conventions per service.

## Usage

### Query (partial `Query` class)

```csharp
public partial class Query
{
    [UsePaging, UseProjection, UseFiltering(Type = typeof(Order)), UseSorting(Type = typeof(Order))]
    public IExecutable<OrderDto> GetOrders([Service] IMongoDatabase db)
        => db.GetCollection<Order>().Aggregate()
             .Project(Builders<Order>.Projection.As<OrderDto>())
             .AsExecutable();
}
```

### Mutation with typed errors (partial `Mutation` class)

```csharp
public partial class Mutation
{
    [Authorize]
    [Error(typeof(BusinessError))]
    [Error(typeof(ValidationError))]
    public async Task<ProductDto> CreateProduct(CreateProductCommand input, [Service] IMediator mediator, CancellationToken ct)
        => await mediator.Send(input, ct);
}
```

`DomainException` → `BusinessError`; FluentValidation/`CommandValidationException` → `ValidationError`
(with `FieldError[] fields`). These appear in the mutation payload's `errors` union.

### Registering types

```csharp
public static IRequestExecutorBuilder AddCommandTypes(this IRequestExecutorBuilder b) => b
    .AddCommandType<CreateProductCommand>()      // → CreateProductInput
    .AddCommandType<RemoveProductCommand>();

public static IRequestExecutorBuilder AddDtoTypes(this IRequestExecutorBuilder b) => b
    .AddDtoType<OrderDto>()                       // → object type "Order"
    .AddType<ProductDtoType>();                   // custom DtoType with computed fields

public static IRequestExecutorBuilder AddAggregateTypes(this IRequestExecutorBuilder b) => b
    .AddAggregateFilterType<Product, Guid>()     // → ProductFilterInput (dynamic-data aware)
    .AddAggregateSortType<Product, Guid>();      // → ProductSortInput
```

### Custom DTO object type (computed fields)

Extend `DtoType<T>` to add resolved fields:

```csharp
public class ProductDtoType : DtoType<ProductDto>
{
    protected override void Configure(IObjectTypeDescriptor<ProductDto> descriptor)
    {
        base.Configure(descriptor);
        descriptor.Field("variantCount")
            .Type<NonNullType<IntType>>()
            .Resolve(ctx => ctx.Parent<ProductDto>().Variants?.Count ?? 0);
    }
}
```

### Dynamic-data filtering

`AddDynamicDataTypes()` + `AddAggregateFilterType<TAgg,TId>()` expose a `where` argument that filters
over the aggregate's schema-less `Data`. Read it in a resolver via
`context.ArgumentValue<InputFilteringDynamicData?>("where")` and pass it to the MongoDB
`ApplyDynamicDataFilteration(...)` helper (see [BytLabs.DataAccess.MongoDB](BytLabs.DataAccess.MongoDB.md)).

## Related packages
- [BytLabs.Api](BytLabs.Api.md) — host that calls `AddGraphQLService()`.
- [BytLabs.DataAccess.MongoDB](BytLabs.DataAccess.MongoDB.md) — Mongo querying/filtering used by resolvers.
- [BytLabs.Application](BytLabs.Application.md) — commands sent from mutations; dynamic-data inputs.
