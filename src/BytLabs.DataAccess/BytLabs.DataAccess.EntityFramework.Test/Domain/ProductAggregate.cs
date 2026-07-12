using BytLabs.Domain.Entities;

namespace BytLabs.DataAccess.EntityFramework.Test.Domain;

public class ProductAggregate : AggregateRootBase<Guid>
{
    public ProductAggregate(Guid id, string name, decimal price) : base(id)
    {
        Name = name;
        Price = price;
    }

    public string Name { get; private set; }
    public decimal Price { get; private set; }

    public void Rename(string name) => Name = name;
}
