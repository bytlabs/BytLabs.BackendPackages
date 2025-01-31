using FluentValidation;
using FluentValidation.Results;

namespace BytLabs.Domain.BusinessRules
{
    public class BusinessRuleException : ValidationException
    {
        public BusinessRuleException(IEnumerable<ValidationFailure> errors) : base(errors)
        {
        }
    }
}
