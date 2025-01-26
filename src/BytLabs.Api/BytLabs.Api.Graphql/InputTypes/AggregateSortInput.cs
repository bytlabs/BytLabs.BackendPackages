using BytLabs.Domain.Entities;
using HotChocolate.Data.Sorting;
using HotChocolate.Types;

namespace BytLabs.Api.Graphql.InputTypes
{

    public class AggregateSortInput<TAggregate, TId> : SortInputType<TAggregate> where TAggregate : IAggregateRoot<TId>
    {
        protected override void Configure(ISortInputTypeDescriptor<TAggregate> descriptor)
        {
            descriptor.BindFieldsImplicitly();
        }
    }
}
