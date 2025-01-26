using BytLabs.Domain.Entities;
using RulesEngine.Models;

namespace BytLabs.States.Domain
{
    public abstract partial class StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TState, TStateId> 
        : AggregateRootBase<TId>
        where TStateId : notnull
        where TState : StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TState, TStateId>.StateBase
        where TTransition : StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TState, TStateId>.TransitionBase
        where TStateMachine : StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TState, TStateId>.StateMachineAggregateBase
    {
        protected StatefulAggregateBase(TId id, TStateMachineId stateMachineId, TStateId stateId):base(id)
        {
            StateMachineId = stateMachineId;
            StateId = stateId;
        }

        public TStateMachineId StateMachineId { get; protected set; }
        public TStateId StateId { get; protected set; }
    }    
}
