using BytLabs.Domain.Audit;
using BytLabs.Domain.DomainEvents;
using GuardClauses;

namespace BytLabs.Domain.Entities;

/// <summary>
/// Base class for aggregate roots in the domain model.
/// Implements the aggregate root pattern from Domain-Driven Design.
/// </summary>
/// <typeparam name="TId">The type of the identifier for the aggregate root</typeparam>
public abstract class AggregateRootBase<TId> : Entity<TId>, IAggregateRoot<TId>
{
    /// <summary>
    /// List of domain events that have occurred on this aggregate root
    /// </summary>
    private List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Initializes a new instance of the aggregate root
    /// </summary>
    /// <param name="id">The unique identifier for the aggregate root</param>
    /// <exception cref="ArgumentNullException">Thrown when id is null</exception>
    protected AggregateRootBase(TId id) : base(id)
    {
        GuardClause.ArgumentIsNotNull(id, nameof(id));
    }

    /// <summary>
    /// Clears all domain events from the aggregate root
    /// </summary>
    public void ClearDomainEvents() =>
        _domainEvents.Clear();

    /// <summary>
    /// Gets a read-only collection of domain events that have occurred on this aggregate root
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents
    {
        get
        {
            _domainEvents ??= new List<IDomainEvent>();
            return _domainEvents.AsReadOnly();
        }
    }

    /// <summary>
    /// Adds a new domain event to this aggregate root
    /// </summary>
    /// <param name="domainEvent">The domain event to add</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        if(_domainEvents == null) _domainEvents = new List<IDomainEvent>();
        _domainEvents.Add(domainEvent);
    }
        
}