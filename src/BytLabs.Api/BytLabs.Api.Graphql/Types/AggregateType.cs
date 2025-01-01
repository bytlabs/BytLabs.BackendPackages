using BytLabs.Domain.Entities;

namespace BytLabs.Api.Graphql.Types
{
    public class AggregateType<TAggregate, TId> : ObjectType<TAggregate> where TAggregate : IAggregateRoot<TId>
    {
        protected override void Configure(IObjectTypeDescriptor<TAggregate> descriptor)
        {
            descriptor.Ignore(a => a.DomainEvents);
        }
    }
}
