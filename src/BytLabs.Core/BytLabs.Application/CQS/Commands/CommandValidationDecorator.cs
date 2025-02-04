using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BytLabs.Application.CQS.Commands
{
    /// <summary>
    /// Decorator for command validation using the pipeline behavior pattern.
    /// Implements validation logic for commands before they are handled.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command request</typeparam>
    /// <typeparam name="TResponse">The type of the command response</typeparam>
    internal sealed class CommandValidationDecorator<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICommandBase
        where TResponse : notnull
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CommandValidationDecorator<TRequest, TResponse>> _logger;

        /// <summary>
        /// Initializes a new instance of the CommandValidationDecorator class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for resolving validators</param>
        /// <param name="logger">The logger for validation operations</param>
        public CommandValidationDecorator(IServiceProvider serviceProvider,
            ILogger<CommandValidationDecorator<TRequest, TResponse>> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Handles the validation of the command before passing it to the next handler in the pipeline.
        /// </summary>
        /// <param name="request">The command request to validate</param>
        /// <param name="next">The next handler in the pipeline</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The response from the command handler</returns>
        /// <exception cref="CommandValidationException">Thrown when validation fails</exception>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            IValidator<TRequest>[]? validators = _serviceProvider.GetServices<IValidator<TRequest>>().ToArray();

            _logger.LogTrace(GetValidationMessage("Validation started"));
            {
                if (validators.Any())
                    foreach (IValidator<TRequest>? validator in validators)
                    {
                        _logger.LogTrace($"Executing validator: {validator.GetType().Name}");

                        await validator.ValidateCommandAndThrowAsync(request, cancellationToken);
                    }
                else
                    _logger.LogTrace(GetValidationMessage("Validators not found"));
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