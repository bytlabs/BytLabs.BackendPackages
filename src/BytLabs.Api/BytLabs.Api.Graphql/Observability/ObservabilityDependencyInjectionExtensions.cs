using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BytLabs.Api.Graphql.Observability
{
    /// <summary>
    /// Provides extension methods for configuring observability features in the GraphQL pipeline.
    /// </summary>
    internal static class ObservabilityDependencyInjectionExtensions
    {
        /// <summary>
        /// Adds observability features to the GraphQL request executor.
        /// </summary>
        /// <param name="requestExecutorBuilder">The GraphQL request executor builder.</param>
        /// <returns>The configured request executor builder with added observability features.</returns>
        /// <remarks>
        /// This method configures:
        /// - Error logging through <see cref="ErrorLoggingDiagnosticsEventListener"/>
        /// - Global error filtering through <see cref="GlobalErrorFilter"/>
        /// </remarks>
        internal static IRequestExecutorBuilder AddObservability(this IRequestExecutorBuilder requestExecutorBuilder)
        {
            return requestExecutorBuilder
                // HotChocolate v16 activates diagnostic listeners from the schema service provider, which
                // does not expose application services (e.g. ILogger) by default. Bridge the logger the
                // listener needs so it can be constructed. See the v15→v16 "Service Provider Separation".
                .AddApplicationService<ILogger<ErrorLoggingDiagnosticsEventListener>>()
                .AddDiagnosticEventListener<ErrorLoggingDiagnosticsEventListener>()
                .AddErrorFilter<GlobalErrorFilter>();
        }
    }
}
