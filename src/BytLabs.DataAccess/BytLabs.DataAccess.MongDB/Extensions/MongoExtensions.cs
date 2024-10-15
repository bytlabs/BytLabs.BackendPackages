using BytLabs.Domain.Entities;
using MongoDB.Driver;

namespace BytLabs.DataAccess.MongDB.Extensions;

/// <summary>
/// Mongo DB Extensions
/// </summary>
public static class MongoExtensions
{
    /// <summary>
    /// Get Collection For TAggregate
    /// </summary>
    /// <param name="db"></param>
    /// <typeparam name="TAggregate"></typeparam>
    /// <returns></returns>
    public static IMongoCollection<TAggregate> GetCollection<TAggregate>(this IMongoDatabase db) where TAggregate : IEntity =>
        db.GetCollection<TAggregate>(MongoDatabaseHelper.CreateCollectionName<TAggregate>());
}