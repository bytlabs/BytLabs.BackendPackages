using BytLabs.Application.DynamicData;
using BytLabs.Domain.DynamicData;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BytLabs.DataAccess.MongoDB.DynamicData
{
    public static class FilterDefinitionBuilderExtensions
    {
        public static FilterDefinition<T> FilterDataField<T>(this FilterDefinitionBuilder<T> filterBuilder, DataOperationFilter operationInput)
            where T : IHaveDynamicData
        {
            var fieldPath = $"data.{operationInput.Path}";
            var valueBson = operationInput.GetBsonValue();

            return operationInput.Operation switch
            {
                FilterOperation.Eq => filterBuilder.Eq(fieldPath, valueBson),
                FilterOperation.Ne => filterBuilder.Ne(fieldPath, valueBson),
                FilterOperation.Gt => filterBuilder.Gt(fieldPath, valueBson),
                FilterOperation.Lt => filterBuilder.Lt(fieldPath, valueBson),
                FilterOperation.Lte => filterBuilder.Lte(fieldPath, valueBson),
                FilterOperation.Gte => filterBuilder.Gte(fieldPath, valueBson),
                FilterOperation.Contains => filterBuilder.Regex(fieldPath, new BsonRegularExpression(operationInput.Value)),
                _ => throw new NotSupportedException($"Operation {operationInput.Operation} is not supported.")
            };
        }

        public static FilterDefinition<T> FilterData<T>(this FilterDefinitionBuilder<T> filterBuilder, InputFilteringDynamicData input)
    where T : IHaveDynamicData
        {
            if (input == null)
                return filterBuilder.Empty;

            var filters = new List<FilterDefinition<T>>();

            // Process 'And' conditions recursively
            if (input.And?.Any() == true)
            {
                var andFilters = input.And.Select(field => filterBuilder.FilterData(field)).Where(f => f != filterBuilder.Empty);
                if (andFilters.Any())
                    filters.Add(filterBuilder.And(andFilters));
            }

            // Process 'Or' conditions recursively
            if (input.Or?.Any() == true)
            {
                var orFilters = input.Or.Select(field => filterBuilder.FilterData(field)).Where(f => f != filterBuilder.Empty);
                if (orFilters.Any())
                    filters.Add(filterBuilder.Or(orFilters));
            }

            // Process single filter data
            if (input.Data is not null)
            {
                var dataFilter = filterBuilder.FilterDataField(input.Data);
                if (dataFilter != filterBuilder.Empty)
                    filters.Add(dataFilter);
            }

            return filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;
        }


        internal static BsonValue GetBsonValue(this DataOperationFilter operationInput)
        {
            switch (operationInput.ValueType)
            {
                case ValueKind.Number:
                    if (double.TryParse(operationInput.Value, out double numberResult))
                        return BsonValue.Create(numberResult);
                    else
                        return BsonNull.Value;


                case ValueKind.String:
                    return BsonValue.Create(operationInput.Value);

                case ValueKind.Boolean:
                    if (bool.TryParse(operationInput.Value, out bool boolResult))
                        return BsonValue.Create(boolResult);
                    else
                        return BsonNull.Value;

                case ValueKind.DateTime:
                    //https://stackoverflow.com/a/3556188/3114005
                    if (DateTime.TryParse(operationInput.Value, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dateTimeResult))
                        return new BsonDateTime(dateTimeResult);
                    else
                        return BsonNull.Value;

                default:
                    return BsonNull.Value;
            }
        }
    }
}
