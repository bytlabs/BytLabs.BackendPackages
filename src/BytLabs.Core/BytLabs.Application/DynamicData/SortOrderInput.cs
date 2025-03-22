using BytLabs.Domain.Entities;

namespace BytLabs.Application.DynamicData;

public enum SortOrder
{
    Asc,
    Desc
}


public class SortInput<T>
{
    public string Path { get; set; } = default!;
    public SortOrder By { get; set; }
}
