using BytLabs.Application.Exceptions;
using BytLabs.Domain.BusinessRules;
using BytLabs.Domain.Exceptions;

namespace BytLabs.Api.Graphql.ErrorTypes.Business
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
        public static BusinessError CreateErrorFrom(ApplicationOperationException ex) =>
            new()
            {
                Message = ex.Message,
                Code = ex.Code,
                Property = ex.Property
            };

        public static BusinessError CreateErrorFrom(EntityNotFoundException ex) =>
            new()
            {
                Message = ex.Message,
                Code = ex.Code,
                Property = ex.Property
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
                Code = ex.Code,
                Property = ex.Property
            };

        public static BusinessError CreateErrorFrom(BusinessRuleException ex) =>
            new()
            {
                Message = ex.Message,
                Code = ex.Errors.FirstOrDefault()?.ErrorCode,
                Property = ex.Errors.FirstOrDefault()?.PropertyName
            };
    }
}