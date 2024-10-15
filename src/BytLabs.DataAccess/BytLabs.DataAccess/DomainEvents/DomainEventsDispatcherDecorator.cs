using System.Linq.Expressions;
using BytLabs.Application.DataAccess;
using BytLabs.Domain.DomainEvents;
using BytLabs.Domain.Entities;
using MediatR;

namespace BytLabs.DataAccess.DomainEvents;

/// <summary>
/// A repository decorator that handles the dispatching of domain events after repository operations.
/// </summary>
/// <typeparam name="TAggregateRoot">The type of the aggregate root entity being managed.</typeparam>
/// <typeparam name="TIdentity">The type of the identifier used by the aggregate root.</typeparam>
/// <remarks>
/// This decorator ensures that:
/// - Domain events are published only after successful persistence operations
/// - Events are published in the order they were raised
/// - Events are cleared from the aggregate after successful publishing
/// - All repository operations maintain proper event handling consistency
/// 
/// The decorator follows the Unit of Work pattern, ensuring that domain events
/// are processed as part of the same transaction as the repository operation.
/// </remarks>
public class DomainEventDispatcherDecorator<TAggregateRoot, TIdentity>(
    IRepository<TAggregateRoot, TIdentity> repository,
    IMediator mediator) : IRepository<TAggregateRoot, TIdentity>
    where TAggregateRoot : IAggregateRoot<TIdentity>
{
    /// <inheritdoc />
    /// <remarks>
    /// Retrieves the aggregate by ID without publishing any domain events.
    /// </remarks>
    public async Task<TAggregateRoot> GetByIdAsync(TIdentity id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken);

    /// <inheritdoc />
    /// <remarks>
    /// Attempts to find an aggregate by ID without publishing any domain events.
    /// Returns null if the aggregate is not found.
    /// </remarks>
    public async Task<TAggregateRoot> FindByIdAsync(TIdentity id, CancellationToken cancellationToken) =>
        await repository.FindByIdAsync(id, cancellationToken);

    /// <inheritdoc />
    /// <remarks>
    /// Returns a single aggregate or default value without publishing any domain events.
    /// </remarks>
    public async Task<TAggregateRoot> SingleOrDefaultAsync(CancellationToken cancellationToken) =>
        await repository.SingleOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    /// <remarks>
    /// Retrieves multiple aggregates by their IDs without publishing any domain events.
    /// </remarks>
    public async Task<List<TAggregateRoot>> FindAllAsync(List<TIdentity> ids, CancellationToken cancellationToken) =>
        await repository.FindAllAsync(ids, cancellationToken);

    /// <inheritdoc />
    /// <remarks>
    /// Finds all aggregates matching the filter expression without publishing any domain events.
    /// </remarks>
    public async Task<IEnumerable<TAggregateRoot>> FindAllByAsync(
        Expression<Func<TAggregateRoot, bool>> filterExpression, 
        CancellationToken cancellationToken) =>
        await repository.FindAllByAsync(filterExpression, cancellationToken);

    /// <inheritdoc />
    /// <remarks>
    /// Inserts a new aggregate and publishes any domain events raised during the operation.
    /// Events are published only after successful persistence.
    /// </remarks>
    public async Task<TAggregateRoot> InsertAsync(TAggregateRoot entity, CancellationToken cancellationToken)
    {
        var result = await repository.InsertAsync(entity, cancellationToken);
        await PublishDomainEvents(entity, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Updates an existing aggregate and publishes any domain events raised during the operation.
    /// Events are published only after successful persistence.
    /// </remarks>
    public async Task<TAggregateRoot> UpdateAsync(TAggregateRoot entity, CancellationToken cancellationToken)
    {
        var result = await repository.UpdateAsync(entity, cancellationToken);
        await PublishDomainEvents(entity, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Deletes an aggregate by ID and publishes any domain events raised during the operation.
    /// The aggregate is loaded before deletion to ensure proper event handling.
    /// </remarks>
    public async Task DeleteAsync(TIdentity id, CancellationToken cancellationToken)
    {
        TAggregateRoot entity = await GetByIdAsync(id, cancellationToken);
        await repository.DeleteAsync(id, cancellationToken);
        await PublishDomainEvents(entity, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Inserts multiple aggregates in batch and publishes their domain events sequentially.
    /// Events are published only after all aggregates are successfully persisted.
    /// </remarks>
    public async Task InsertBatchAsync(List<TAggregateRoot> aggregates, CancellationToken cancellationToken)
    {
        await repository.InsertBatchAsync(aggregates, cancellationToken);
        foreach (TAggregateRoot aggregate in aggregates)
        {
            await PublishDomainEvents(aggregate, cancellationToken);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Updates multiple aggregates in batch and publishes their domain events sequentially.
    /// Events are published only after all aggregates are successfully updated.
    /// </remarks>
    public async Task UpdateBatchAsync(List<TAggregateRoot> aggregates, CancellationToken cancellationToken)
    {
        await repository.UpdateBatchAsync(aggregates, cancellationToken);
        foreach (TAggregateRoot aggregate in aggregates)
        {
            await PublishDomainEvents(aggregate, cancellationToken);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Deletes multiple aggregates by their IDs without publishing domain events.
    /// Use DeleteBatchAsync(List{TAggregateRoot}) if event publishing is required.
    /// </remarks>
    public async Task DeleteBatchAsync(List<TIdentity> ids, CancellationToken cancellationToken)
    {
        await repository.DeleteBatchAsync(ids, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Deletes multiple aggregates and publishes their domain events sequentially.
    /// Events are published only after all aggregates are successfully deleted.
    /// </remarks>
    public async Task DeleteBatchAsync(List<TAggregateRoot> aggregates, CancellationToken cancellationToken)
    {
        await repository.DeleteBatchAsync(aggregates, cancellationToken);
        foreach (TAggregateRoot aggregate in aggregates)
        {
            await PublishDomainEvents(aggregate, cancellationToken);
        }
    }

    /// <summary>
    /// Publishes all domain events raised by an aggregate and clears them afterwards.
    /// </summary>
    /// <param name="entity">The aggregate root containing domain events to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    /// This method:
    /// - Extracts all domain events from the aggregate
    /// - Clears the events from the aggregate to prevent duplicate publishing
    /// - Publishes each event sequentially through the mediator
    /// - Maintains the order of events as they were raised
    /// </remarks>
    private async Task PublishDomainEvents(TAggregateRoot entity, CancellationToken cancellationToken)
    {
        List<IDomainEvent> domainEvents = [.. entity.DomainEvents];
        entity.ClearDomainEvents();
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
