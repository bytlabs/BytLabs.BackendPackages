using FluentValidation.Results;

namespace BytLabs.Application.CQS.Queries
{
    [Serializable]
    public class QueryValidationException : Exception
    {
        public QueryValidationException(string? message, IEnumerable<ValidationFailure> errors) : base(message)
        {
            Errors = errors;
        }

        public IEnumerable<ValidationFailure> Errors { get; }
    }
}