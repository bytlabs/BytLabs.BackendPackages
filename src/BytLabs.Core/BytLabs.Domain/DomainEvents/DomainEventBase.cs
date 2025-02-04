namespace BytLabs.Domain.DomainEvents;

public abstract class DomainEventBase : IDomainEvent
{
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public abstract class DomainEventBase<T> : DomainEventBase
{
    public DomainEventBase(Guid id, T data)
    {
        Id = id;
        Data = data;
    }

    public Guid Id { get; private set; }
    public T Data { get; private set; }
}