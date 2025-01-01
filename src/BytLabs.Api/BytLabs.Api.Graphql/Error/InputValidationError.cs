namespace BytLabs.Api.Graphql.Error
{
    /// <summary>
    /// Represents validation errors that occur during request processing.
    /// </summary>
    public record InputValidationError
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
        public static InputValidationError CreateErrorFrom(FluentValidation.ValidationException ex)
        {
            return new InputValidationError
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

        /// <summary>
        /// Creates a ValidationError from a FluentValidation ValidationException.
        /// </summary>
        /// <param name="ex">The validation exception to convert.</param>
        /// <returns>A new ValidationError instance containing all validation failures.</returns>
        public static InputValidationError CreateErrorFrom(System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            var fieldErrors = ex.ValidationResult?.MemberNames?
                                    .Select(memberName => new FieldError
                                    {
                                        Code = string.Empty,
                                        Field = memberName,
                                        Message = ex.ValidationResult?.ErrorMessage ?? ex.Message
                                    })
                                    .ToList() ?? new List<FieldError>();

            return new InputValidationError { Fields = fieldErrors };
        }
    }
}