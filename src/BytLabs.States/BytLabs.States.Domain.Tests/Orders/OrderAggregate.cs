using BytLabs.Domain.DomainEvents;
using BytLabs.States.Domain.Tests.Orders.StateMachine;
using OrderStateMachineId = System.Guid;
using OrderTransitionId = System.Guid;
using OrderAggregateId = System.Guid;
using RulesEngine.Models;
using System.Reflection;

namespace BytLabs.States.Domain.Tests.Orders
{
    public class OrderAggregate : StatefulAggregateBase<OrderAggregateId, 
        OrderStateMachineAggregate, OrderStateMachineId, 
        OrderTransition, OrderTransitionId, 
        OrderState, OrderStateId>
    {

        public static class Triggers
        {
            public static Trigger MarkAsShipped = new Trigger("MarkAsShipped");
        }


        public OrderAggregate(OrderAggregateId id, OrderStateMachineId stateMachineId, OrderStateId stateId) : base(id, stateMachineId, stateId)
        {

        }

        public void MarkAsShipped(OrderStateMachineAggregate orderStateMachine)
        {
            orderStateMachine.Fire(Triggers.MarkAsShipped, this, new ReSettings
            {
                CustomTypes = Assembly.GetExecutingAssembly().GetTypes()
            });
        }
    }

    public class OrderShippedEvent : DomainEventBase
    {

    }
}
