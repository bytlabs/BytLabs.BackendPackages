using System;
using System.Threading;
using System.Threading.Tasks;
using BytLabs.Application.CQS.Commands;
using BytLabs.Application.DataAccess;
using BytLabs.Application.Exceptions;

namespace BytLabs.Application.Test.OrdersService.Application.Commands
{
    public class ProcessOrderCommandHandler : ICommandHandler<ProcessOrderCommand, OrderResult>
    {
        private readonly IRepository<OrderAggregate, Guid> _orderRepository;

        public ProcessOrderCommandHandler(IRepository<OrderAggregate, Guid> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderResult> Handle(ProcessOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
                throw new EntityNotFoundException($"Order {request.OrderId} not found");

            order.Process();
            await _orderRepository.UpdateAsync(order, cancellationToken);

            return new OrderResult
            {
                OrderId = order.Id,
                Status = order.Status,
                ProcessedAt = order.ProcessedAt!.Value
            };
        }
    }
}