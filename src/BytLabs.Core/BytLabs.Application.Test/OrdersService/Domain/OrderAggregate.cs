using BytLabs.Domain.Entities;

public class OrderAggregate : AggregateRootBase<Guid>
{
    public OrderAggregate(Guid id, string customerName, decimal totalAmount):base(id)
    {
        CustomerName = customerName;
        TotalAmount = totalAmount;
        Status = OrderStatus.Created;
    }

    public string CustomerName { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public void Process()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOperationException("Order must be in Created status to process");
            
        Status = OrderStatus.Processing;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Order must be in Processing status to complete");
            
        Status = OrderStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed orders");
            
        Status = OrderStatus.Cancelled;
    }
}

public enum OrderStatus
{
    Created,
    Processing,
    Completed,
    Cancelled
} 