using BytLabs.Domain.Entities;

namespace BytLabs.Api.Graphql.Types
{
    /// <summary>
    /// Base GraphQL type for aggregate root entities.
    /// </summary>
    /// <typeparam name="TAggregate">The aggregate root type</typeparam>
    /// <typeparam name="TId">The type of the aggregate's identifier</typeparam>
    public class AggregateType<TAggregate, TId> : ObjectType<TAggregate> where TAggregate : IAggregateRoot<TId>
    {
        /// <summary>
        /// Configures the GraphQL type descriptor for the aggregate.
        /// </summary>
        /// <param name="descriptor">The descriptor to configure</param>
        protected override void Configure(IObjectTypeDescriptor<TAggregate> descriptor)
        {
            descriptor.Ignore(a => a.DomainEvents);
        }
    }
}
