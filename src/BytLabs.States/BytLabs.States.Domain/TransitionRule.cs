using BytLabs.Domain.Entities;
using RulesEngine.ExpressionBuilders;
using RulesEngine.Models;

namespace BytLabs.States.Domain
{
    public class TransitionRule : Entity<Guid>
    {
        public TransitionRule(Guid id, string name, string expression) : base(id)
        {
            Name = name;
            Expression = expression;
        }

        public string Name { get; private set; }
        public string Expression { get; private set; }


        public bool Evaluate(ReSettings? settings = null, params RuleParameter[] @params)
        {
            var reParser = new RuleExpressionParser(settings);
            var result = reParser.Evaluate<bool>(Expression, @params);
            return result;
        }
    }
}
