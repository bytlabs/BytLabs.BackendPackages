using BytLabs.Application.DynamicData;
using BytLabs.Domain.DynamicData;
using BytLabs.Domain.Entities;
using MongoDB.Driver;

namespace BytLabs.DataAccess.MongoDB.DynamicData;

public static class IAggregateFluentExtensions
{
    public static IAggregateFluent<T> ExcludeSoftDeletedEntites<T>(this IAggregateFluent<T> aggregateFluent)
        where T : ISoftDeletable
    {
        return aggregateFluent.Match(Builders<T>.Filter.Ne(entity => entity.IsDeleted, true));
    }

    public static IAggregateFluent<T> ApplyDynamicDataFilteration<T>(this IAggregateFluent<T> aggregateFluent, InputFilteringDynamicData dataFilter)
        where T : IHaveDynamicData
    {
        var matchDataFilter = dataFilter is null ? Builders<T>.Filter.Empty
            : Builders<T>.Filter.FilterData(dataFilter);

        return aggregateFluent.Match(matchDataFilter);
    }
}
