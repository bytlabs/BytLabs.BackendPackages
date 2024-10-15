using BytLabs.Application.CQS.Commands;

public class ProcessOrderCommand : ICommand<OrderResult>
{
    public Guid OrderId { get; set; }
    public string? ExamplePreProcessorDecorator { get; set; }
}

public class OrderResult
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime ProcessedAt { get; set; }
} 