---
name: bytlabs-cqrs
description: Use when adding or changing a command, query, handler, or validator in a BytLabs service - ICommand/IQuery, ICommandHandler/IQueryHandler, FluentValidation, AddCQS registration, and the validation and logging pipeline in BytLabs.Application.
---

# BytLabs CQRS

Commands and queries run on MediatR, wrapped in validation and logging decorators.

## Type inventory

| Area | Types | Namespace |
|---|---|---|
| Commands | `ICommandBase`, `ICommand`, `ICommand<TResult>`, `ICommandHandler<TCommand>`, `ICommandHandler<TCommand,TResponse>` | `BytLabs.Application.CQS.Commands` |
| Queries | `IQuery<TResult>`, `IQueryHandler<TQuery,TResponse>` | `BytLabs.Application.CQS.Queries` |
| Data access | `IRepository<TAggregateRoot,TIdentity>`, `IUnitOfWork` | `BytLabs.Application.DataAccess` |
| Domain events | `DomainEventHandler<TDomainEvent>` | `BytLabs.Application.DomainEvents` |
| Pipeline | `CommandValidationDecorator<,>`, `QueryValidationDecorator<,>`, `LoggingDecorator<,>` | `BytLabs.Application.CQS.*` |
| User context | `IUserContextProvider`, `IUserContextResolver`, `UserContextBuilder` | `BytLabs.Application.UserContext` |
| Exceptions | `ApplicationOperationException`, `EntityNotFoundException`, `CommandValidationException` | `BytLabs.Application.Exceptions` |

## Registration

```csharp
services.AddCQS(new[] { typeof(CreateProductCommand).Assembly });
```

Signature: `AddCQS(IServiceCollection, Assembly[] assemblies, Action<MediatRServiceConfiguration>? options = null)`.
It wires MediatR over the given assemblies plus this one, AutoMapper, the command and query
validation decorators, and `LoggingDecorator`.

**A handler or validator in an assembly you did not pass to `AddCQS` will not resolve.** This is
the most common wiring failure. Pass every assembly that contains them.

## Command + handler

Commands are `record`s. One handler per command.

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

Use `ICommand` / `ICommandHandler<TCommand>` for the void case.

Handlers **orchestrate**: load, call aggregate methods, persist, map. Business logic belongs in
the aggregate (see `bytlabs-domain-modeling`). Depend on `IRepository<,>`, never on
`MongoRepository` or a `DbContext`.

## Query + handler

```csharp
public record GetProductQuery(Guid Id) : IQuery<ProductDto>;

public class GetProductQueryHandler : IQueryHandler<GetProductQuery, ProductDto>
{
    private readonly IRepository<Product, Guid> _repository;
    public GetProductQueryHandler(IRepository<Product, Guid> repository) => _repository = repository;

    public async Task<ProductDto> Handle(GetProductQuery request, CancellationToken ct)
        => _mapper.Map<ProductDto>(await _repository.GetByIdAsync(request.Id, ct));
}
```

## Validation

Write an `AbstractValidator<T>`; it is discovered automatically from the assemblies given to
`AddCQS` and runs *before* the handler.

```csharp
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator() => RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
}
```

Never hand-roll argument checks inside a handler. Command validation failures throw
`CommandValidationException`; query validation failures throw FluentValidation's
`ValidationException`.

## The pipeline

For each request:

```
validation decorator → logging decorator → transaction decorator (if UseTransactions)
    → handler → domain events dispatched after successful save
```

So a handler does **not** open a transaction, and does **not** publish domain events. Both are
handled around it. `LoggingDecorator` also logs a warning for any request over 3 seconds.

## Repository surface

`IRepository<TAggregateRoot, TIdentity>` offers `GetByIdAsync` (throws `EntityNotFoundException`),
`FindByIdAsync` (returns null), `SingleOrDefaultAsync`, `InsertAsync`, `UpdateAsync`,
`DeleteAsync` (soft delete), `FindAllAsync(List<TIdentity>)`,
`FindAllByAsync(Expression<Func<T,bool>>)`, and the batch forms `InsertBatchAsync`,
`UpdateBatchAsync`, `DeleteBatchAsync`. Details in `bytlabs-data-access`.

## Exceptions

| Throw | When | Surfaces as |
|---|---|---|
| `EntityNotFoundException` | Aggregate does not exist | GraphQL error |
| `ApplicationOperationException` | Application-level failure; carries `Code` and `Property` | GraphQL error |
| `BusinessRuleException` (domain) | Business rule violated | `BusinessError` |
| `CommandValidationException` | Command failed validation | `ValidationError` / `FieldError` |

See `bytlabs-graphql` for the typed error mapping.

For any signature not shown here, use `bytlabs-api-lookup` — do not guess.
