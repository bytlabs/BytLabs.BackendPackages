using MediatR;

namespace BytLabs.Domain.DomainEvents;

/// <summary>
/// Represents a domain event in the system.
/// Implements INotification from MediatR to enable event publishing and handling.
/// Domain events are used to capture and communicate state changes or significant occurrences within the domain.
/// </summary>
public interface IDomainEvent : INotification
{
    public DateTimeOffset CreatedAt { get;  init; }
    public string CreatedBy { get; init; }
}

public interface IDomainEvent<TId> : IDomainEvent
{
    public TId Id { get; init; }    
}

public interface IDomainEvent<TId, TData> : IDomainEvent<TId>
{
    public TData Data { get; init; }
}
