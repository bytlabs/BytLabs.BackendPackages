using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BytLabs.Api.Graphql.Observability
{
    /// <summary>
    /// Provides extension methods for configuring observability features in the GraphQL pipeline.
    /// </summary>
    public static class ObservabilityDependencyInjectionExtensions
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
        public static IRequestExecutorBuilder AddObservability(this IRequestExecutorBuilder requestExecutorBuilder)
        {
            return requestExecutorBuilder
                .AddDiagnosticEventListener<ErrorLoggingDiagnosticsEventListener>()
                .AddErrorFilter<GlobalErrorFilter>();
        }
    }
}
