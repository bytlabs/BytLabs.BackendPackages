using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Execution.Processing;
using HotChocolate.Resolvers;
using Microsoft.Extensions.Logging;

namespace BytLabs.Api.Graphql.Observability
{
    /// <summary>
    /// Diagnostic event listener for logging GraphQL execution errors.
    /// Extends ExecutionDiagnosticEventListener to provide detailed error logging for different GraphQL execution scenarios.
    /// </summary>
    public class ErrorLoggingDiagnosticsEventListener : ExecutionDiagnosticEventListener
    {
        private readonly ILogger<ErrorLoggingDiagnosticsEventListener> _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorLoggingDiagnosticsEventListener"/> class.
        /// </summary>
        /// <param name="log">The logger instance for recording error information.</param>
        public ErrorLoggingDiagnosticsEventListener(
            ILogger<ErrorLoggingDiagnosticsEventListener> log)
        {
            _log = log;
        }

        /// <summary>
        /// Logs errors that occur during field resolution.
        /// </summary>
        /// <param name="context">The middleware context containing information about the current execution state.</param>
        /// <param name="error">The error that occurred during resolution.</param>
        public override void ResolverError(IMiddlewareContext context, IError error) =>
            _log.LogError(error.Exception, "A resolver error occured: {Error}. Context path:{Path}, error path: {ErrorPath}",
                          (object)error.Message, (object)context.Path, (object?)error.Path);

        /// <summary>
        /// Logs errors that occur during task execution.
        /// </summary>
        /// <param name="task">The execution task that encountered an error.</param>
        /// <param name="error">The error that occurred during task execution.</param>
        public override void TaskError(
            IExecutionTask task,
            IError error) =>
            _log.LogError(error.Exception, "TaskError: {ErrorMessage}", error.Message);

        /// <summary>
        /// Logs errors that occur during request processing.
        /// </summary>
        /// <param name="context">The request context containing information about the current request.</param>
        /// <param name="exception">The exception that occurred during request processing.</param>
        public override void RequestError(
            IRequestContext context,
            Exception exception) =>
            _log.LogError(exception, "RequestError: {ErrorMessage}", exception.Message);

        /// <summary>
        /// Logs errors that occur during subscription event processing.
        /// </summary>
        /// <param name="context">The subscription event context containing information about the current event.</param>
        /// <param name="exception">The exception that occurred during event processing.</param>
        public override void SubscriptionEventError(
            SubscriptionEventContext context,
            Exception exception) =>
            _log.LogError(exception, "SubscriptionEventError: {ErrorMessage}", exception.Message);

        /// <summary>
        /// Logs errors that occur in the subscription transport layer.
        /// </summary>
        /// <param name="subscription">The subscription information.</param>
        /// <param name="exception">The exception that occurred in the transport layer.</param>
        public override void SubscriptionTransportError(
            ISubscription subscription,
            Exception exception) =>
            _log.LogError(exception, "SubscriptionTransportError: {ErrorMessage}", exception.Message);
    }
}