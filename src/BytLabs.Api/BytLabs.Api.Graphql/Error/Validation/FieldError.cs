namespace BytLabs.Api.Graphql.Error.Validation
{
    public class FieldError
    {
        /// <summary>
        /// Gets the error code associated with the business error.
        /// </summary>
        public required string Code { get; init; }
        /// <summary>
        /// Gets the field or property associated with the business error.
        /// </summary>
        public required string Field { get; init; }
        /// <summary>
        /// Gets the descriptive message for the business error.
        /// </summary>
        public required string Message { get; init; }
    }
}