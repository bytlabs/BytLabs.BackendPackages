using BytLabs.Domain.Audit;
using BytLabs.Domain.DomainEvents;

namespace BytLabs.Domain.Entities;

/// <summary>
/// Represents an aggregate root in the domain model.
/// An aggregate root is an entity that acts as the entry point to an aggregate of domain objects.
/// </summary>
/// <typeparam name="TId">The type of the identifier for the aggregate root</typeparam>
public interface IAggregateRoot<out TId> : IEntity<TId>, IAuditable
{
    /// <summary>
    /// Gets a read-only collection of domain events that have occurred on this aggregate root
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events from the aggregate root
    /// </summary>
    public void ClearDomainEvents();
}