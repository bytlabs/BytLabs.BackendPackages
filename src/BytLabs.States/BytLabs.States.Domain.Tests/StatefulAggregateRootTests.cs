using BytLabs.States.Domain.Tests.Orders;

namespace BytLabs.States.Domain.Tests
{
    public class StatefulAggregateRootTests
    {
        [Fact]
        public void GIVEN_StatefulAggregate_WHEN_TriggerOccurred_THEN_TransitionShouldTakePlace()
        {
            var entityChangedTrigger = new Trigger("entity_changed", "Entity Changed");

            var placedState = new State("placed", "Placed");
            var shippedState = new State("shipped", "Shipped");
            var orderStateMachine = new StateMachine("order_state_machine",
                new List<State>
                {
                    placedState,
                    shippedState,
                },
                new List<Transition>
                {
                    new Transition("placed_to_shipped", "placed", "shipped", entityChangedTrigger.Id),
                });

            var order = new OrderAggregate(Guid.NewGuid(), orderStateMachine.Id, placedState.Id);
            order.MarkAsShipped();

            order.UpdateState(entityChangedTrigger, orderStateMachine);

            Assert.Equal(shippedState.Id, order.StateId);
        }
    }
}