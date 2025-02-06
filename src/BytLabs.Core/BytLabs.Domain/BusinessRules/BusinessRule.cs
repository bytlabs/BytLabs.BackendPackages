using FluentValidation;
using FluentValidation.Results;

namespace BytLabs.Domain.BusinessRules
{
    public abstract class BusinessRule<T> : AbstractValidator<T>
    {
        protected override void RaiseValidationException(ValidationContext<T> context, ValidationResult result)
        {
            var ex = new ValidationException(result.Errors);
            throw new BusinessRuleException(ex.Message, result.Errors);
        }
    }
}
