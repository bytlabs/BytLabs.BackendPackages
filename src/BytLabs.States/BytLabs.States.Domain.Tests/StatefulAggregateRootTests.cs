using BytLabs.States.Domain.Tests.Orders;
using BytLabs.States.Domain.Tests.Orders.StateMachine;
using RulesEngine.Models;
using System.Reflection;

namespace BytLabs.States.Domain.Tests
{
    public class StatefulAggregateRootTests
    {
        [Fact]
        public void GIVEN_StatefulAggregate_WHEN_TriggerOccurred_THEN_TransitionShouldTakePlace()
        {
            var orderStateMachine = new OrderStateMachineAggregate(Guid.NewGuid(),
                new HashSet<OrderState>
                {
                    new OrderState(OrderStateId.Created),
                    new OrderState(OrderStateId.Placed),
                    new OrderState(OrderStateId.Shipped),
                },
                new List<OrderTransition>
                {
                    new OrderTransition(
                        Guid.NewGuid(), 
                        OrderStateId.Placed, 
                        OrderStateId.Shipped,
                        OrderAggregate.Triggers.MarkAsShipped,
                        new List<TransitionRule>
                        { 
                            new TransitionRule(Guid.NewGuid(), "Check current state", "entity.StateId == OrderStateId.Placed")
                        }),
                });

            var order = new OrderAggregate(Guid.NewGuid(), orderStateMachine.Id, OrderStateId.Placed);
            order.MarkAsShipped(orderStateMachine);

            Assert.Equal(OrderStateId.Shipped, order.StateId);
        }
    }
}