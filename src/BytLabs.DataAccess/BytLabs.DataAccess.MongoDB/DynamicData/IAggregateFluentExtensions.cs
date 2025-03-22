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

    public static IAggregateFluent<T> AppySortingWithDynamicData<T>(this IAggregateFluent<T> aggregate, List<SortInput>? order)
    {
        if (order is null || !order.Any()) return aggregate;

        return aggregate.Sort(Builders<T>.Sort.Combine(order.Select(input => input.By == SortOrder.Asc ?
            Builders<T>.Sort.Ascending(input.Path) : Builders<T>.Sort.Descending(input.Path))));
    }
}
