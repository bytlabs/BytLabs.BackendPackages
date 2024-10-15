using BytLabs.Domain.Audit;

namespace BytLabs.Domain.Entities;

/// <summary>
/// Represents a generic entity interface with a strongly-typed identifier.
/// Combines entity identification and audit capabilities.
/// </summary>
/// <typeparam name="TIdType">The type of the identifier for the entity</typeparam>
public interface IEntity<out TIdType> : IEntityId<TIdType>, IEntity
{
}

/// <summary>
/// Represents the base entity interface.
/// Provides auditing capabilities for all entities in the system.
/// </summary>
public interface IEntity : IAuditableEntity
{
}