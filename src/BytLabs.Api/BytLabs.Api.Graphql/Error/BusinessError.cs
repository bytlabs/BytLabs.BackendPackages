using BytLabs.Domain.Exceptions;
using ApplicationException = BytLabs.Application.Exceptions.ApplicationException;

namespace BytLabs.Api.Graphql.Error
{
    /// <summary>
    /// Represents a business error that can occur during application execution.
    /// </summary>
    public record BusinessError
    {
        /// <summary>
        /// Gets or initializes the error code.
        /// </summary>
        public string? Code { get; init; }

        /// <summary>
        /// Gets or sets the property associated with the error.
        /// </summary>
        public string? Property { get; set; }

        /// <summary>
        /// Gets or initializes the error message.
        /// </summary>
        public required string Message { get; init; }

        /// <summary>
        /// Creates a BusinessError from an ApplicationException.
        /// </summary>
        /// <param name="ex">The application exception to convert.</param>
        /// <returns>A new BusinessError instance.</returns>
        public static BusinessError CreateErrorFrom(ApplicationException ex) =>
            new()
            {
                Message = ex.Message,
                Code = null,
                Property = null
            };

        /// <summary>
        /// Creates a BusinessError from a DomainException.
        /// </summary>
        /// <param name="ex">The domain exception to convert.</param>
        /// <returns>A new BusinessError instance.</returns>
        public static BusinessError CreateErrorFrom(DomainException ex) =>
            new()
            {
                Message = ex.Message,
                Code = null,
                Property = null
            };
    }
}