using MediatR;

namespace BytLabs.Domain.DomainEvents;

/// <summary>
/// Represents a domain event in the system.
/// Implements INotification from MediatR to enable event publishing and handling.
/// Domain events are used to capture and communicate state changes or significant occurrences within the domain.
/// </summary>
public interface IDomainEvent : INotification
{
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
