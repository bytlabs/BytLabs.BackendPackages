using BytLabs.Domain.Entities;

namespace BytLabs.States.Domain
{
    public abstract partial class StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TState, TStateId>
    {
        public abstract class TransitionBase : Entity<TTransitionId>
        {
            public TStateId From { get; set; }
            public TStateId To { get; set; }
            public Trigger Trigger { get; set; }
            public IReadOnlyCollection<TransitionRule> Rules { get; private set; }

            public TransitionBase(TTransitionId id, TStateId from, TStateId to, Trigger trigger, IReadOnlyCollection<TransitionRule> rules) : base(id)
            {
                From = from;
                To = to;
                Trigger = trigger;
                Rules = rules;
            }
        }
    }
}
