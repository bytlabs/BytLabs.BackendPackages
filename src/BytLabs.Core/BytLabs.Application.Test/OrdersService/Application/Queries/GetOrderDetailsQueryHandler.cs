using BytLabs.Application.CQS.Queries;
using BytLabs.Application.DataAccess;
using BytLabs.Application.Exceptions;

namespace BytLabs.Application.Test.OrdersService.Application.Queries
{
    public class GetOrderDetailsQueryHandler : IQueryHandler<GetOrderDetailsQuery, OrderDetails>
    {
        private readonly IRepository<OrderAggregate, Guid> _orderRepository;

        public GetOrderDetailsQueryHandler(IRepository<OrderAggregate, Guid> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDetails> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
                throw new EntityNotFoundException($"Order {request.OrderId} not found");

            return new OrderDetails
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                ProcessedAt = order.ProcessedAt
            };
        }
    }
}