using BytLabs.Domain.Entities;
using BytLabs.Domain.Exceptions;

namespace BytLabs.States.Domain
{
    public class StateMachine : AggregateRootBase<string>
    {
        public IReadOnlyCollection<State> States { get; private set; }
        public IReadOnlyCollection<Transition> Transitions { get; private set; }

        public StateMachine(string id, 
            IReadOnlyCollection<State> states, 
            IReadOnlyCollection<Transition> transitions) : base(id)
        {
            States = states;
            Transitions = transitions;

            var invalidStates = GetInvalidStates(transitions);
            if (invalidStates.Any())
            {
                throw new DomainException($"Transitions contain some invalid states: {string.Join(", ", invalidStates)}");
            }

        }

        private IReadOnlyCollection<string> GetInvalidStates(IReadOnlyCollection<Transition> transitions)
        {
            var possibleStateIds = transitions.SelectMany(t => new[] { t.From, t.To })
                                                            .Distinct()
                                                            .ToList();

            var validStateIds = States.Select(s => s.Id);

            return possibleStateIds
                .Where(possibleStateId => !validStateIds.Any(validStateId => validStateId == possibleStateId))
                .ToList()
                .AsReadOnly();
        }
    }

    public class Transition : Entity<string>
    {
        public string From { get; set; }
        public string To { get; set; }
        public string TriggerId { get; set; }
        public IReadOnlyCollection<TransitionRule> Rules { get; private set; }

        public Transition(string id, string from, string to, string triggerId, IReadOnlyCollection<TransitionRule> rules) : base(id)
        {
            From = from;
            To = to;
            TriggerId = triggerId;
            Rules = rules;
        }
    }

    public class TransitionRule
    {

    }

    public class State : Entity<string>
    {
        public string Name { get; private set; }
        public State(string id, string name) : base(id)
        {
            Name = name;
        }
    }
}
