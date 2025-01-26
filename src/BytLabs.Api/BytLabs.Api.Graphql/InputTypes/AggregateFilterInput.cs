using BytLabs.Domain.Entities;
using HotChocolate.Data.Filters;
using HotChocolate.Types;

namespace BytLabs.Api.Graphql.InputTypes
{
    public class AggregateFilterInput<TAggregate, TId> : FilterInputType<TAggregate> where TAggregate : IAggregateRoot<TId>
    {
        protected override void Configure(IFilterInputTypeDescriptor<TAggregate> descriptor)
        {
            descriptor.BindFieldsImplicitly();
            descriptor.Field(field => field.DomainEvents)
                .Ignore();
        }
    }
}
