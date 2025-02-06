using BytLabs.Application.DynamicData;
using BytLabs.Domain.DynamicData;
using BytLabs.Domain.Entities;
using HotChocolate.Data.Filters;

namespace BytLabs.Api.Graphql.InputTypes
{
    public class AggregateFilterInput<TAggregate, TId> : FilterInputType<TAggregate> where TAggregate : IAggregateRoot<TId>
    {
        protected override void Configure(IFilterInputTypeDescriptor<TAggregate> descriptor)
        {
            descriptor.BindFieldsImplicitly();
            descriptor.Field(field => field.DomainEvents).Ignore();

            if(CheckIfImplementsInterface<TAggregate, IHaveDynamicData>())
            {
                descriptor.Field(nameof(IHaveDynamicData.Data).ToLower()).Type<DataFilterInputType>();
            }
        }

        public static bool CheckIfImplementsInterface<TType, TInterface>()
        {
            return typeof(TInterface).IsInterface && typeof(TInterface).IsAssignableFrom(typeof(TType));
        }
    }

    /// <summary>
    /// Helper class to retrieve dynamic data filter object
    /// </summary>
    public sealed class FilterHavingDynamicData
    {
        public FilterHavingDynamicData(DataFilter data)
        {
            Data = data;
        }

        public DataFilter Data { get; private set; }
    }
}
