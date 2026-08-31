The response must follow BytLabs domain and CQRS conventions rather than improvising a
generic DDD answer.

Pass if the response:
- derives the aggregate from `AggregateRootBase<Guid>` and implements `ISoftDeletable`
- keeps property setters private and constructs through a static factory method
- defines the command as a record implementing `ICommand<TResult>` with a matching
  `ICommandHandler<TCommand, TResponse>`
- has the handler depend on `IRepository<Warehouse, Guid>`, not a concrete repository

Fail if the aggregate exposes public setters, if the handler contains business logic,
or if it references a `DbContext` or `MongoRepository` directly.
