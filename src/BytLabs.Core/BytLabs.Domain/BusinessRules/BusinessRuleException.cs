using FluentValidation.Results;

namespace BytLabs.Domain.BusinessRules
{
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string? message, IEnumerable<ValidationFailure> errors) : base(message)
        {
            Errors = errors;
        }

        public IEnumerable<ValidationFailure> Errors { get; }
    }
}
