namespace BytLabs.Domain.DomainEvents;


public abstract record DomainEventBase() : IDomainEvent
{
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; init; } = "System";
}

public abstract record DomainEventBase<TId>(TId Id)
    : DomainEventBase, IDomainEvent<TId> { }

public abstract record DomainEventBase<TId, TData>(TId Id, TData Data) 
    : DomainEventBase<TId>(Id), IDomainEvent<TId, TData> { }