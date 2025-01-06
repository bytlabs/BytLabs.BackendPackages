namespace BytLabs.DataAccess.MongoDB;

/// <summary>
/// Stores configuration options for MongoDB repositories, specifically the collection name for an entity type.
/// </summary>
/// <typeparam name="T">The type of entity.</typeparam>
internal sealed class MongoRepositoryOptions<T>
{
    /// <summary>
    /// Gets or sets the name of the MongoDB collection where entities of type T are stored.
    /// </summary>
    public required string CollectionName { get; init; }
}
