namespace BytLabs.Domain.Entities;

/// <summary>
/// Represents an interface for entities that have a unique identifier.
/// </summary>
/// <typeparam name="T">The type of the identifier</typeparam>
public interface IEntityId<out T>
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    /// <returns>The identifier value of type T</returns>
    T Id { get; }
}