using BytLabs.Domain.Entities;

namespace BytLabs.Application.DynamicData;

public enum SortOrder
{
    Asc,
    Desc
}


public class SortInput<TAggregate, TId> where TAggregate : IAggregateRoot<TId>
{
    public string Path { get; set; } = default!;
    public SortOrder By { get; set; }
}
