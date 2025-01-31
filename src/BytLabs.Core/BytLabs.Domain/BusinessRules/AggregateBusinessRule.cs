using FluentValidation;
using FluentValidation.Results;

namespace BytLabs.Domain.BusinessRules
{
    public class AggregateBusinessRule<T> : BusinessRule<T>
    {
        public AggregateBusinessRule(params BusinessRule<T>[] policies)
        {
            Policies = policies;
        }

        protected override bool PreValidate(ValidationContext<T> context, ValidationResult result)
        {
            var validationResults = Policies
                .Select(policy => policy.Validate(context.InstanceToValidate))
                .ToList();

            if (validationResults.All(validationResult => validationResult.IsValid))
            {
                return true;
            }

            result.Errors.AddRange(validationResults
                .SelectMany(validationResult => validationResult.Errors)
                .ToList());

            return false;
        }

        public BusinessRule<T>[] Policies { get; }

    }
}
