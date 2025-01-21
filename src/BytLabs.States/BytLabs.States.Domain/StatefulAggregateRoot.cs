using BytLabs.Domain.Entities;

namespace BytLabs.States.Domain
{
    public abstract class StatefulAggregateRoot<TId> : AggregateRootBase<TId>
    {
        protected StatefulAggregateRoot(TId id, string stateMachineId, string stateId):base(id)
        {
            StateMachineId = stateMachineId;
            StateId = stateId;
        }

        public  string StateMachineId { get; private set; }
        public  string StateId { get; private set; }

        public void UpdateState(Trigger trigger, StateMachine stateMachine)
        {
            foreach (var transition in stateMachine.Transitions)
            {
                if (transition.TriggerId == trigger.Id)
                {
                    if (transition.From == StateId)
                    {
                        StateId = transition.To;
                    }
                }
            }
        }

    }    
}
