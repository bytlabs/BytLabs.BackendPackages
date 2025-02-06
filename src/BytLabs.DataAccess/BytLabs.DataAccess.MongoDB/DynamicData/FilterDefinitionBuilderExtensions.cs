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
            var emptyFilterList = new List<FilterDefinition<T>>() { filterBuilder.Empty };
            return filterBuilder.And(input.And?.Select(field => field.Data is null? filterBuilder.Empty : filterBuilder.FilterDataField(field.Data)) )
                             & filterBuilder.Or(input.Or?.Select(field => field.Data is null? filterBuilder.Empty : filterBuilder.FilterDataField(field.Data)))
                             & (input.Data is null ? filterBuilder.Empty : filterBuilder.FilterDataField(input.Data));
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
