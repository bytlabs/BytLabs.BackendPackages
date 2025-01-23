using BytLabs.Domain.Entities;
using BytLabs.Domain.Exceptions;
using RulesEngine.Models;

namespace BytLabs.States.Domain
{
    public abstract partial class StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TState, TStateId> 
    {
        public abstract class TransitionBase : Entity<TTransitionId>
        {
            public TStateId? From { get; set; }
            public TStateId To { get; set; }
            public Trigger? Trigger { get; set; }
            public IReadOnlyCollection<TransitionRule> Rules { get; private set; }

            public TransitionBase(TTransitionId id, TStateId? from, TStateId to, Trigger? trigger, IReadOnlyCollection<TransitionRule> rules) : base(id)
            {
                From = from;
                To = to;
                Trigger = trigger;
                Rules = rules;
            }

            public bool StartsFromAnywhere()
            {
                return From is null;
            }

            public bool StartsFrom(TStateId value)
            {
                return From is not null && From.Equals(value);
            }

            public bool HasNoTrigger()
            {
                return Trigger is null;
            }

            public bool CanGoto(TStateId state)
            {
                return To.Equals(state);
            }

            public void EvaluateAndThrowFailingRules<TStatefulAggregate>(TStatefulAggregate entity, ReSettings? settings)
                where TStatefulAggregate : StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TState, TStateId>
            {
                var failingRules = Rules
                                    .Where(rule => !rule.Evaluate(settings, new RuleParameter("entity", entity)))
                                    .Select(rule => rule.Name.TrimEnd('.'))
                                    .ToList();

                if (failingRules.Any())
                {
                    throw new DomainException($"Failing at rule(s): {string.Join(", ", failingRules)}.");
                }
            }

            public bool HasTrigger(Trigger value)
            {
                return Trigger is not null && Trigger == value;
            }
        }
    }
}
