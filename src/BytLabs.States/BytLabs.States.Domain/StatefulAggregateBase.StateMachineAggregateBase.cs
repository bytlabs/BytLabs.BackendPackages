using BytLabs.Domain.Entities;
using BytLabs.Domain.Exceptions;
using RulesEngine.Models;

namespace BytLabs.States.Domain
{
    public abstract partial class StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TState, TStateId>
    {
        public abstract class StateMachineAggregateBase : AggregateRootBase<TStateMachineId>
        {
            public IReadOnlySet<TState> States { get; private set; } = new HashSet<TState>();
            public IReadOnlyCollection<TTransition> Transitions { get; private set; } = new List<TTransition>();

            public StateMachineAggregateBase(TStateMachineId id,
                IReadOnlySet<TState> states,
                IReadOnlyCollection<TTransition> transitions) : base(id)
            {
                States = states;
                Transitions = transitions;

                var invalidStates = GetInvalidStates(transitions);
                if (invalidStates.Any())
                {
                    throw new DomainException($"Transitions contain some invalid states: {string.Join(", ", invalidStates)}");
                }

            }

            private IReadOnlyCollection<TStateId> GetInvalidStates(IReadOnlyCollection<TTransition> transitions)
            {
                var possibleStateIds = transitions.SelectMany(t => new[] { t.From, t.To })
                                                                .Distinct()
                                                                .ToList();

                var validStateIds = States.Select(s => s.Id);

                return possibleStateIds
                    .Where(possibleStateId => !validStateIds.Any(validStateId => validStateId!.Equals(possibleStateId)))
                    .ToList()
                    .AsReadOnly();
            }


            public void Fire<TStatefulAggregate>(Trigger trigger, TStatefulAggregate entity, ReSettings? settings = null)
                where TStatefulAggregate : StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TState, TStateId>

            {
                foreach (var transition in Transitions)
                {
                    var triggerIsMatched = transition.Trigger == trigger;

                    var fromStateIsMatched = transition.From!.Equals(entity.StateId);

                    var allTransitionRulesArePassing = transition.Rules
                                            .All(rule => rule.Evaluate(settings, new RuleParameter("entity", entity)));

                    if (triggerIsMatched && fromStateIsMatched && allTransitionRulesArePassing)
                    {
                        entity.StateId = transition.To;
                    }
                }
            }
        }
    }
}
