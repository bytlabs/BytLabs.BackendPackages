using BytLabs.Domain.Entities;

namespace BytLabs.States.Domain
{
    public abstract partial class StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TState, TStateId>
    {
        public abstract class StateBase : Entity<TStateId>
        {
            public StateBase(TStateId id) : base(id)
            {

            }
        }
    }
}
