using OrderTransitionId = System.Guid;

namespace BytLabs.States.Domain.Tests.Orders.StateMachine
{
    public class OrderTransition : OrderAggregate.TransitionBase
    {
        public OrderTransition(OrderTransitionId id, OrderStateId? from, OrderStateId to, Trigger? trigger, IReadOnlyCollection<TransitionRule> rules) : base(id, from, to, trigger, rules)
        {

        }


    }
}
