using BytLabs.Application.CQS.Commands;
using BytLabs.Application.CQS.Queries;

namespace BytLabs.Api.Graphql.ErrorTypes.Validation
{
    /// <summary>
    /// Represents validation errors that occur during request processing.
    /// </summary>
    public record ValidationError
    {
        /// <summary>
        /// Gets or initializes the list of field-specific validation errors.
        /// </summary>
        public required List<FieldError> Fields { get; init; } = new();

        /// <summary>
        /// Gets or initializes the general validation error message.
        /// </summary>
        public string Message { get; init; } = "Invalid Data";

        /// <summary>
        /// Creates a ValidationError from a FluentValidation ValidationException.
        /// </summary>
        /// <param name="ex">The validation exception to convert.</param>
        /// <returns>A new ValidationError instance containing all validation failures.</returns>
        public static ValidationError CreateErrorFrom(CommandValidationException ex)
        {
            return new ValidationError
            {
                Fields = ex.Errors.Select(err =>
                    new FieldError
                    {
                        Code = err.ErrorCode,
                        Field = err.PropertyName,
                        Message = err.ErrorMessage
                    })
                    .ToList()
            };
        }

        public static ValidationError CreateErrorFrom(QueryValidationException ex)
        {
            return new ValidationError
            {
                Fields = ex.Errors.Select(err =>
                    new FieldError
                    {
                        Code = err.ErrorCode,
                        Field = err.PropertyName,
                        Message = err.ErrorMessage
                    })
                    .ToList()
            };
        }
    }
}