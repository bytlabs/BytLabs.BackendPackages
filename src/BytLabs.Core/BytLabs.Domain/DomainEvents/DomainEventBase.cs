namespace BytLabs.Domain.DomainEvents;

public abstract class DomainEventBase : IDomainEvent
{
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public abstract class DomainEventBase<TId, TData> : DomainEventBase
{
    public DomainEventBase(TId id, TData data)
    {
        Id = id;
        Data = data;
    }

    public TId Id { get; private set; }
    public TData Data { get; private set; }
}