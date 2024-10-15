using BytLabs.Domain.Audit;
using BytLabs.Domain.Exceptions;

namespace BytLabs.Domain.Entities;

/// <summary>
/// Base class for all entities in the domain model.
/// Provides basic entity functionality including identity and audit information.
/// </summary>
/// <typeparam name="TIdType">The type of the identifier for the entity</typeparam>
public abstract class Entity<TIdType> : EntityAuditInfo, IEntity<TIdType>
{
    /// <summary>
    /// Gets the unique identifier for this entity.
    /// The identifier can only be set during initialization.
    /// </summary>
    public TIdType Id { get; protected init; }

    /// <summary>
    /// Initializes a new instance of the entity
    /// </summary>
    /// <param name="id">The unique identifier for the entity</param>
    protected Entity(TIdType id)
    {
        Id = id;
    }
}