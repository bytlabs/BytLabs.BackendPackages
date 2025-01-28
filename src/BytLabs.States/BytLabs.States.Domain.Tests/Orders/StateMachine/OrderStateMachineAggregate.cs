using OrderStateMachineId = System.Guid;

namespace BytLabs.States.Domain.Tests.Orders.StateMachine
{
    public class OrderStateMachineAggregate : OrderAggregate.StateMachineAggregateBase
    {
        public OrderStateMachineAggregate(OrderStateMachineId id, IReadOnlySet<OrderState> states, IReadOnlyCollection<OrderTransition> transitions) : base(id, states, transitions)
        {

        }
    }
}
