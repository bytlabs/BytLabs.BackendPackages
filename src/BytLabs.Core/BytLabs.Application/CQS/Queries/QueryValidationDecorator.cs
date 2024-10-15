using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BytLabs.Application.CQS.Queries
{
    /// <summary>
    /// Decorator for query validation using the pipeline behavior pattern.
    /// Implements validation logic for queries before they are handled.
    /// </summary>
    /// <typeparam name="TRequest">The type of the query request</typeparam>
    /// <typeparam name="TResponse">The type of the query response</typeparam>
    internal sealed class QueryValidationDecorator<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IQuery<TResponse>
        where TResponse : notnull
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QueryValidationDecorator<TRequest, TResponse>> _logger;

        /// <summary>
        /// Initializes a new instance of the QueryValidationDecorator class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for resolving validators</param>
        /// <param name="logger">The logger for validation operations</param>
        public QueryValidationDecorator(IServiceProvider serviceProvider,
            ILogger<QueryValidationDecorator<TRequest, TResponse>> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Handles the validation of the query before passing it to the next handler in the pipeline.
        /// </summary>
        /// <param name="request">The query request to validate</param>
        /// <param name="next">The next handler in the pipeline</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The response from the query handler</returns>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            IValidator<TRequest>[]? validators = _serviceProvider.GetServices<IValidator<TRequest>>().ToArray();

            _logger.LogTrace(GetValidationMessage("Validation started"));
            {
                if (validators.Any())
                {
                    foreach (IValidator<TRequest>? validator in validators)
                    {
                        _logger.LogTrace($"Executing validator: {validator.GetType().Name}");

                        await validator.ValidateAndThrowAsync(request, cancellationToken);
                    }
                }
            }
            _logger.LogTrace(GetValidationMessage("Validation completed"));

            return await next();
        }

        /// <summary>
        /// Creates a formatted validation message for logging.
        /// </summary>
        /// <param name="message">The message to format</param>
        /// <returns>A formatted string containing the request type and message</returns>
        private static string GetValidationMessage(string message) =>
            $"{typeof(TRequest).Name}: {message}";
    }
}