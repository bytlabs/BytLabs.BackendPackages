using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BytLabs.Application.CQS.Decorators
{
    /// <summary>
    /// Decorator for logging request handling using the pipeline behavior pattern.
    /// Measures and logs the execution time of request handlers.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being handled</typeparam>
    /// <typeparam name="TResponse">The type of the response being returned</typeparam>
    internal sealed class LoggingDecorator<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : notnull
    {
        private readonly ILogger<LoggingDecorator<TRequest, TResponse>> _logger;

        /// <summary>
        /// Initializes a new instance of the LoggingDecorator class.
        /// </summary>
        /// <param name="logger">The logger instance for recording timing and execution information</param>
        public LoggingDecorator(ILogger<LoggingDecorator<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the request by logging its execution time and any potential issues.
        /// </summary>
        /// <param name="request">The request being processed</param>
        /// <param name="next">The next handler in the pipeline</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The response from the request handler</returns>
        /// <remarks>
        /// Logs a warning if the request takes longer than 3 seconds to process.
        /// Execution time is logged in seconds with 4 decimal places of precision.
        /// </remarks>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling request {@CommandName} started", typeof(TRequest).Name);

            var timer = new Stopwatch();
            timer.Start();

            TResponse response;
            try
            {
                response = await next();
            }
            finally
            {
                timer.Stop();
                TimeSpan elapsed = timer.Elapsed;
                string? elapsedString = elapsed.ToString("ss\\.ffff");

                if (elapsed.Seconds > 3)
                    _logger.LogWarning("Handling request {@CommandName} finished in {@TimeTaken} seconds",
                                       typeof(TRequest).Name, elapsedString);

                _logger.LogInformation("Handling request {@CommandName} finished in {@TimeTaken} seconds",
                                       typeof(TRequest).Name, elapsedString);
            }

            return response;
        }
    }
}