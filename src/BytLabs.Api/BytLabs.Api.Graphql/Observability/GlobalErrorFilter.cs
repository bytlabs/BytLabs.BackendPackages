using HotChocolate;

namespace BytLabs.Api.Graphql.Observability
{
    /// <summary>
    /// Global error filter for handling GraphQL errors.
    /// Implements IErrorFilter to provide centralized error processing for all GraphQL operations.
    /// </summary>
    public class GlobalErrorFilter : IErrorFilter
    {
        /// <summary>
        /// Processes errors that occur during GraphQL operations.
        /// </summary>
        /// <param name="error">The error to be processed.</param>
        /// <returns>The processed error, potentially modified or transformed.</returns>
        public IError OnError(IError error) =>
            error;
    }
}