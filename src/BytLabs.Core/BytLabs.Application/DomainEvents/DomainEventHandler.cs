using BytLabs.Domain.DomainEvents;
using MediatR;

namespace BytLabs.Application.DomainEvents;

/// <summary>
/// Abstract base class for handling domain events in the application.
/// Implements MediatR's INotificationHandler to support the publish-subscribe pattern.
/// </summary>
/// <typeparam name="TDomainEvent">The type of domain event to handle</typeparam>
public abstract class DomainEventHandler<TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    /// <summary>
    /// Handles the domain event notification from MediatR.
    /// Delegates the actual handling to the abstract HandleDomainEvent method.
    /// </summary>
    /// <param name="notification">The domain event notification to handle</param>
    /// <param name="cancellationToken">Token for cancelling the operation</param>
    /// <returns>A task representing the asynchronous handling operation</returns>
    public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        await HandleDomainEvent(notification, cancellationToken);
    }

    /// <summary>
    /// Abstract method that must be implemented by concrete handlers to process domain events.
    /// Provides the core event handling logic for specific domain events.
    /// </summary>
    /// <param name="domainEvent">The domain event to handle</param>
    /// <param name="cancellationToken">Token for cancelling the operation</param>
    /// <returns>A task representing the asynchronous handling operation</returns>
    protected abstract Task HandleDomainEvent(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
