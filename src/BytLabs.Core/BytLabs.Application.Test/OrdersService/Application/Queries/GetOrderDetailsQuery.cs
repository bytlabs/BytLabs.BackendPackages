using BytLabs.Application.CQS.Queries;

public class GetOrderDetailsQuery : IQuery<OrderDetails>
{
    public Guid OrderId { get; set; }
}

public class OrderDetails
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
} 