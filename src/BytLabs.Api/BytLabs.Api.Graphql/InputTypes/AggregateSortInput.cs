using BytLabs.Application.DynamicData;
using BytLabs.Domain.Entities;
using HotChocolate.Data.Sorting;
using HotChocolate.Types;

namespace BytLabs.Api.Graphql.InputTypes
{
    public class AggregateSortInput<TAggregate, TId> : InputObjectType<SortInput<TAggregate>> where TAggregate : IAggregateRoot<TId>
    {
        protected override void Configure(IInputObjectTypeDescriptor<SortInput<TAggregate>> descriptor)
        {
            descriptor.Name(typeof(TAggregate).Name.Replace("Aggregate", "") + "SortInput");
            descriptor.BindFieldsImplicitly();
            descriptor.Field(x => x.By).Type<EnumType<SortOrder>>();
        }
    }
}
