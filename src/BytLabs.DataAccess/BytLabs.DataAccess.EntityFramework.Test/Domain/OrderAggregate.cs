using BytLabs.Domain.Entities;

namespace BytLabs.DataAccess.EntityFramework.Test.Domain;

public class OrderAggregate : AggregateRootBase<Guid>
{
    public OrderAggregate(Guid id, string customerName, decimal totalAmount) : base(id)
    {
        CustomerName = customerName;
        TotalAmount = totalAmount;
        Status = OrderStatus.Created;
    }

    public string CustomerName { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }

    public void Complete() => Status = OrderStatus.Completed;
}

public enum OrderStatus
{
    Created,
    Completed
}
