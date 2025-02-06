using BytLabs.Application.DynamicData;
using BytLabs.Domain.DynamicData;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BytLabs.DataAccess.MongoDB.DynamicData
{
    public static class FilterDefinitionBuilderExtensions
    {
        public static FilterDefinition<T> FilterDataField<T>(this FilterDefinitionBuilder<T> filterBuilder, DataFieldFilter filter)
            where T : IHaveDynamicData
        {
            var fieldPath = $"data.{filter.Path}";
            var valueBson = filter.GetBsonValue();

            return filter.Operation switch
            {
                FilterOperation.Eq => filterBuilder.Eq(fieldPath, valueBson),
                FilterOperation.Ne => filterBuilder.Ne(fieldPath, valueBson),
                FilterOperation.Gt => filterBuilder.Gt(fieldPath, valueBson),
                FilterOperation.Lt => filterBuilder.Lt(fieldPath, valueBson),
                FilterOperation.Lte => filterBuilder.Lte(fieldPath, valueBson),
                FilterOperation.Gte => filterBuilder.Gte(fieldPath, valueBson),
                FilterOperation.Contains => filterBuilder.Regex(fieldPath, new BsonRegularExpression(filter.Value)),
                _ => throw new NotSupportedException($"Operation {filter.Operation} is not supported.")
            };
        }

        public static FilterDefinition<T> FilterData<T>(this FilterDefinitionBuilder<T> filterBuilder, DataFilter filterInput)
            where T : IHaveDynamicData
        {
            var emptyFilterList = new List<FilterDefinition<T>>() { filterBuilder.Empty };
            return filterBuilder.And(filterInput.And?.Select(field => filterBuilder.FilterDataField(field)) ?? emptyFilterList)
                             & filterBuilder.Or(filterInput.Or?.Select(field => filterBuilder.FilterDataField(field)) ?? emptyFilterList)
                             & (filterInput.Field is null ? filterBuilder.Empty : filterBuilder.FilterDataField(filterInput.Field));
        }

        internal static BsonValue GetBsonValue(this DataFieldFilter filter)
        {
            switch (filter.ValueType)
            {
                case ValueKind.Number:
                    if (double.TryParse(filter.Value, out double numberResult))
                        return BsonValue.Create(numberResult);
                    else
                        return BsonNull.Value;


                case ValueKind.String:
                    return BsonValue.Create(filter.Value);

                case ValueKind.Boolean:
                    if (bool.TryParse(filter.Value, out bool boolResult))
                        return BsonValue.Create(boolResult);
                    else
                        return BsonNull.Value;

                case ValueKind.DateTime:
                    //https://stackoverflow.com/a/3556188/3114005
                    if (DateTime.TryParse(filter.Value, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dateTimeResult))
                        return new BsonDateTime(dateTimeResult);
                    else
                        return BsonNull.Value;

                default:
                    return BsonNull.Value;
            }
        }
    }
}
