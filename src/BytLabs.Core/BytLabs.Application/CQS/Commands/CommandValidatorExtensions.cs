using FluentValidation;

namespace BytLabs.Application.CQS.Commands
{
    public static class CommandValidatorExtensions
    {
        public static async Task ValidateCommandAndThrowAsync<TRequest>(this IValidator<TRequest> validator, TRequest instance, CancellationToken cancellationToken)
            where TRequest : ICommandBase
        {
            var res = await validator.ValidateAsync(instance, cancellationToken);

            if (!res.IsValid)
            {
                var ex = new ValidationException(res.Errors);
                throw new CommandValidationException(ex.Message, res.Errors);
            }
        }
    }
}