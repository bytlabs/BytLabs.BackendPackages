using BytLabs.Domain.Entities;

namespace BytLabs.States.Domain
{
    public abstract partial class StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TStateEntity, TStateId> where TStateEntity : StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TStateEntity, TStateId>.StateBase
        where TTransition : StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TStateEntity, TStateId>.TransitionBase
        where TStateMachine : StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TStateEntity, TStateId>.StateMachineAggregateBase
    {
        public abstract class StateBase : Entity<TStateId>
        {
            public StateBase(TStateId id) : base(id)
            {

            }
        }
    }    
}
