namespace BytLabs.Domain.DomainEvents;

public abstract class DomainEventBase : IDomainEvent
{
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}