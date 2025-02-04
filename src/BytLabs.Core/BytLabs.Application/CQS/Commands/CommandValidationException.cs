using FluentValidation.Results;

namespace BytLabs.Application.CQS.Commands
{
    [Serializable]
    public class CommandValidationException : Exception
    {
        public CommandValidationException(string? message, IEnumerable<ValidationFailure> errors) : base(message)
        {
            Errors = errors;
        }

        public IEnumerable<ValidationFailure> Errors { get; }
    }
}