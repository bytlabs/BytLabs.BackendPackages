using BytLabs.Domain.ValueObjects;
using GuardClauses;

namespace BytLabs.Domain.Metadata
{
    /// <summary>
    /// Represents metadata information about an entity.
    /// Implements the Value Object pattern to ensure immutability and value-based equality.
    /// </summary>
    public class EntityMetadata : ValueObject
    {
        /// <summary>
        /// Initializes a new instance of the EntityMetadata class.
        /// </summary>
        /// <param name="type">The type name of the entity</param>
        /// <param name="id">The unique identifier of the entity</param>
        /// <exception cref="ArgumentException">Thrown when type or id is null, empty, or whitespace</exception>
        public EntityMetadata(string type, string id)
        {
            GuardClause.IsNullOrEmptyStringOrWhiteSpace(type, nameof(type));
            Type = type;
            GuardClause.IsNullOrEmptyStringOrWhiteSpace(id, nameof(id));
            Id = id;
        }

        /// <summary>
        /// Gets the type name of the entity
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the entity
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Returns the components that determine equality for this value object
        /// </summary>
        /// <returns>An enumerable of objects that together determine equality</returns>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Type;
            yield return Id;
        }
    }
}