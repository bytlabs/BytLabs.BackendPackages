using BytLabs.Domain.Entities;

namespace BytLabs.States.Domain.Tests.Orders.StateMachine
{

    public class OrderState : OrderAggregate.StateBase
    {
        public OrderState(OrderStateId id) : base(id)
        {

        }
    }
}
