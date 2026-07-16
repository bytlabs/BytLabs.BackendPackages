using BytLabs.Application.DynamicData;
using BytLabs.Domain.DynamicData;
using BytLabs.Domain.Entities;
using MongoDB.Driver;

namespace BytLabs.DataAccess.MongoDB;

public static class IAggregateFluentExtensions
{
    public static IAggregateFluent<T> ExcludeSoftDeletedEntites<T>(this IAggregateFluent<T> aggregateFluent)
        where T : ISoftDeletable
    {
        return aggregateFluent.Match(Builders<T>.Filter.Ne(entity => entity.IsDeleted, true));
    }   
}
