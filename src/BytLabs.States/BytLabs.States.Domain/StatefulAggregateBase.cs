using BytLabs.Domain.Entities;
using RulesEngine.Models;

namespace BytLabs.States.Domain
{
    public abstract partial class StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TStateEntity, TStateId> 
        : AggregateRootBase<TId> 
        where TStateEntity : StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TStateEntity, TStateId>.StateBase
        where TTransition : StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TStateEntity, TStateId>.TransitionBase
        where TStateMachine : StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TStateEntity, TStateId>.StateMachineAggregateBase
    {
        protected StatefulAggregateBase(TId id, TStateMachineId stateMachineId, TStateId stateId):base(id)
        {
            StateMachineId = stateMachineId;
            StateId = stateId;
        }

        public TStateMachineId StateMachineId { get; private set; }
        public TStateId StateId { get; private set; }
    }    
}
