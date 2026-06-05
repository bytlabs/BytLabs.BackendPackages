# BytLabs.Application

The application layer: CQRS (commands/queries via MediatR), repository & unit-of-work abstractions,
domain-event handling, a validation + logging pipeline, user-context resolution, and dynamic-data
filter/sort inputs.

## Install

```xml
<PackageReference Include="BytLabs.Application" />
```

## What's inside

| Area | Types |
|------|-------|
| Commands | `ICommand`, `ICommand<TResult>`, `ICommandHandler<TCommand>`, `ICommandHandler<TCommand,TResponse>` |
| Queries | `IQuery<TResult>`, `IQueryHandler<TQuery,TResponse>` |
| Data access | `IRepository<TAggregateRoot,TIdentity>`, `IUnitOfWork` |
| Domain events | `DomainEventHandler<TDomainEvent>` |
| Pipeline | command/query validation decorators, `LoggingDecorator` (registered by `AddCQS`) |
| User context | `IUserContextProvider`, `IUserContextResolver`, `UserContextBuilder`, `AddUserContextProviders()` |
| Dynamic data | `InputFilteringDynamicData`, `DataOperationFilter`, `FilterOperation`, `SortOrderInput`, `ValueKind` |
| Exceptions | `ApplicationOperationException`, `EntityNotFoundException`, `CommandValidationException`, `QueryValidationException` |
| Registration | `AddCQS(assemblies, options?)`, `AddUserContextProviders()` |

## Registration

`AddCQS` wires MediatR (scanning your assemblies + this one), the FluentValidation pipeline for
commands and queries, and the logging decorator. Call it once in your infrastructure setup.

```csharp
services.AddCQS(new[] { typeof(CreateProductCommand).Assembly });
```

This registers: MediatR handlers, `CommandValidationDecorator`/`QueryValidationDecorator`
(auto-discovered `AbstractValidator<T>` run before handlers), and `LoggingDecorator`.

## Usage

### Command + handler

A command is a `record` implementing `ICommand<TResult>` (or `ICommand` for void). The handler
implements `ICommandHandler<,>` and uses `IRepository<,>`.

```csharp
public record CreateProductCommand(Guid Id, string Name) : ICommand<ProductDto>;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, ProductDto>
{
    private readonly IRepository<Product, Guid> _repository;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(IRepository<Product, Guid> repository, IMapper mapper)
    { _repository = repository; _mapper = mapper; }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = Product.Create(request.Id, request.Name);
        var saved = await _repository.InsertAsync(product, ct);
        return _mapper.Map<ProductDto>(saved);
    }
}
```

### Query + handler

```csharp
public record GetProductByIdQuery(Guid Id) : IQuery<ProductDto>;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IRepository<Product, Guid> _repository;
    public GetProductByIdQueryHandler(IRepository<Product, Guid> repository) => _repository = repository;

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
        => /* map */ default!;
}
```

> Note: most read endpoints in these services are GraphQL resolvers that query MongoDB directly with
> projection; `IQueryHandler` is available when you prefer the mediator path.

### Validation (automatic)

Define a FluentValidation `AbstractValidator<TCommand>` in a scanned assembly — the pipeline runs it
before the handler and throws `CommandValidationException` (→ GraphQL `ValidationError`).

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator() => RuleFor(c => c.Name).NotEmpty();
}
```

### Repository

`IRepository<TAggregateRoot, TIdentity>` (where `TAggregateRoot : IAggregateRoot<TIdentity>`) provides:
`GetByIdAsync` (throws `EntityNotFoundException` if missing), `FindByIdAsync` (null if missing),
`SingleOrDefaultAsync`, `InsertAsync`, `UpdateAsync`, `DeleteAsync` (soft delete), `FindAllAsync(ids)`,
`FindAllByAsync(predicate)`, and batch variants (`InsertBatchAsync`, `UpdateBatchAsync`,
`DeleteBatchAsync`). Inject it into handlers; the MongoDB implementation comes from
[BytLabs.DataAccess.MongoDB](BytLabs.DataAccess.MongoDB.md).

### Domain-event handler

React to domain events raised by aggregates. Implemented as MediatR notification handlers via
`DomainEventHandler<TEvent>`; discovered by `AddCQS`.

```csharp
public class SendWelcomeEmailHandler : DomainEventHandler<ProductCreated>
{
    protected override async Task HandleDomainEvent(ProductCreated e, CancellationToken ct)
    {
        // side effect: email, read-model update, integration event...
    }
}
```

### User context

Resolve "who is acting" from pluggable sources (HTTP, known/system user, etc.). Configure resolvers
with `AddUserContextProviders()` and inject `IUserContextProvider` to read `GetUserId()`.

```csharp
services.AddUserContextProviders()
    .AddResolver<HttpUserContextResolver>();      // from BytLabs.Api
```

The data-access layer uses the resolved user id to stamp `AuditInfo` on writes.

### Dynamic-data filtering inputs

`InputFilteringDynamicData`, `DataOperationFilter`, `FilterOperation`, `SortOrderInput`, and
`ValueKind` model filtering/sorting over an aggregate's schema-less `Data` (`IHaveDynamicData`). You
rarely construct these by hand — the GraphQL layer binds them from the query `where`/`order`
arguments and the MongoDB layer translates them into filters (see
[BytLabs.DataAccess.MongoDB](BytLabs.DataAccess.MongoDB.md)).

## Related packages
- [BytLabs.Domain](BytLabs.Domain.md) — the entities/events these abstractions operate on.
- [BytLabs.DataAccess](BytLabs.DataAccess.md) — transaction + domain-event-dispatch decorators around commands.
- [BytLabs.DataAccess.MongoDB](BytLabs.DataAccess.MongoDB.md) — `IRepository`/`IUnitOfWork` implementations.
