using FluentValidation;
using FluentValidation.Results;

namespace BytLabs.Domain.BusinessRules
{
    public abstract class BusinessRule<T> : AbstractValidator<T>
    {
        protected override void RaiseValidationException(ValidationContext<T> context, ValidationResult result)
        {
            throw new BusinessRuleException(result.Errors);
        }
    }
}
