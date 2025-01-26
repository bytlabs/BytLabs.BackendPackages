using BytLabs.Domain.Entities;
using BytLabs.Domain.Exceptions;
using RulesEngine.Models;

namespace BytLabs.States.Domain
{
    public abstract partial class StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TState, TStateId>
    {
        /// <summary>
        /// Base class for state machine aggregates that manages state transitions and rules.
        /// </summary>
        public abstract class StateMachineAggregateBase : AggregateRootBase<TStateMachineId>
        {
            /// <summary>
            /// Gets the collection of valid states in this state machine.
            /// </summary>
            public IReadOnlySet<TState> States { get; protected set; } = new HashSet<TState>();

            /// <summary>
            /// Gets the collection of transitions that define the allowed state changes and their rules.
            /// </summary>
            public IReadOnlyCollection<TTransition> Transitions { get; protected set; } = new List<TTransition>();

            /// <summary>
            /// Initializes a new instance of the state machine aggregate.
            /// </summary>
            /// <param name="id">The unique identifier for this state machine.</param>
            /// <param name="states">The set of valid states for this state machine.</param>
            /// <param name="transitions">The collection of valid transitions between states.</param>
            /// <exception cref="ArgumentNullException">Thrown when states or transitions is null.</exception>
            /// <exception cref="DomainException">Thrown when transitions contain invalid states.</exception>
            public StateMachineAggregateBase(TStateMachineId id,
                IReadOnlySet<TState> states,
                IReadOnlyCollection<TTransition> transitions) : base(id)
            {
                if (transitions is null)
                {
                    throw new ArgumentNullException(nameof(transitions));
                }

                States = states ?? throw new ArgumentNullException(nameof(states));

                var invalidStates = GetInvalidStates(transitions);
                if (invalidStates.Any())
                {
                    throw new DomainException($"Transitions contain some invalid states: {string.Join(", ", invalidStates)}");
                }
                
                Transitions = transitions;
            }

            /// <summary>
            /// Changes the state of an entity based on a trigger event, evaluating transition rules.
            /// </summary>
            /// <typeparam name="TStatefulAggregate">The type of the stateful aggregate being modified.</typeparam>
            /// <param name="trigger">The trigger event initiating the state change.</param>
            /// <param name="entity">The entity whose state should be changed.</param>
            /// <param name="settings">Optional settings for rule evaluation.</param>
            /// <remarks>
            /// The method evaluates all transitions that match the trigger and the entity's current state.
            /// If a transition starts from anywhere (From state is null), it can be triggered from any state.
            /// All matching transitions must pass their rule evaluation before the state change occurs.
            /// </remarks>
            public void Fire<TStatefulAggregate>(Trigger trigger, TStatefulAggregate entity, ReSettings? settings = null)
                where TStatefulAggregate : StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TState, TStateId>
            {
                foreach (var transition in Transitions)
                {
                    if (transition.HasTrigger(trigger)
                        && (transition.StartsFromAnywhere() || transition.StartsFrom(entity.StateId)))
                    {
                        transition.EvaluateAndThrowFailingRules(entity, settings);

                        entity.StateId = transition.To;
                    }
                }
            }

            /// <summary>
            /// Directly transitions an entity to a specified state, evaluating any applicable rules.
            /// </summary>
            /// <typeparam name="TStatefulAggregate">The type of the stateful aggregate being modified.</typeparam>
            /// <param name="toState">The target state to transition to.</param>
            /// <param name="entity">The entity whose state should be changed.</param>
            /// <param name="settings">Optional settings for rule evaluation.</param>
            /// <remarks>
            /// This method evaluates transitions without triggers that match the current and target states.
            /// If a transition starts from anywhere (From state is null), it can be executed from any state.
            /// All matching transitions must pass their rule evaluation before the state change occurs.
            /// </remarks>
            public void Goto<TStatefulAggregate>(TStateId toState, TStatefulAggregate entity, ReSettings? settings = null)
                where TStatefulAggregate : StatefulAggregateBase<TId, TStateMachine, TStateMachineId, TTransition, TTransitionId, TState, TStateId>
            {
                foreach (var transition in Transitions)
                {
                    if (transition.CanGoto(toState)
                        && transition.HasNoTrigger() 
                        && (transition.StartsFromAnywhere() || transition.StartsFrom(entity.StateId)))
                    {
                        transition.EvaluateAndThrowFailingRules(entity, settings);

                        entity.StateId = transition.To;
                    }
                }

                entity.StateId = toState;
            }

            /// <summary>
            /// Identifies any states referenced in transitions that are not defined in the States collection.
            /// </summary>
            /// <param name="transitions">The collection of transitions to validate.</param>
            /// <returns>A collection of state IDs that are referenced in transitions but not defined in States.</returns>
            private IReadOnlyCollection<TStateId> GetInvalidStates(IReadOnlyCollection<TTransition> transitions)
            {
                var possibleStateIds = transitions
                                            .SelectMany(t => new[] { t.To, t.From! })
                                            .Where(state => state != null)
                                            .Distinct()
                                            .ToList();

                var validStateIds = States.Select(s => s.Id);

                return possibleStateIds
                    .Where(possibleStateId => !validStateIds.Any(validStateId => validStateId!.Equals(possibleStateId)))
                    .ToList()
                    .AsReadOnly();
            }
        }
    }
}
