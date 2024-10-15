using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace BytLabs.Api
{
    /// <summary>
    /// Defines the builder steps for configuring an API application using the fluent builder pattern.
    /// </summary>
    public interface ApiBuilderSteps
    {
        /// <summary>
        /// Initial configuration step for the API builder.
        /// </summary>
        public interface IInitialStep
        {
            /// <summary>
            /// Configures HTTP context accessor services.
            /// </summary>
            /// <returns>The next configuration step for multi-tenant setup.</returns>
            IMultiTenantStep WithHttpContextAccessor();
        }

        /// <summary>
        /// Configuration step for multi-tenant setup.
        /// </summary>
        public interface IMultiTenantStep
        {
            /// <summary>
            /// Configures multi-tenant context services.
            /// </summary>
            /// <returns>The next configuration step for authentication setup.</returns>
            IAuthenticationStep WithMultiTenantContext();
        }

        /// <summary>
        /// Configuration step for authentication setup.
        /// </summary>
        public interface IAuthenticationStep
        {
            /// <summary>
            /// Configures authentication services.
            /// </summary>
            /// <returns>The next configuration step for logging setup.</returns>
            ILoggingStep WithAuthentication();
        }

        /// <summary>
        /// Configuration step for logging setup.
        /// </summary>
        public interface ILoggingStep
        {
            /// <summary>
            /// Configures logging services.
            /// </summary>
            /// <returns>The next configuration step for metrics setup.</returns>
            IMetricsStep WithLogging();
        }

        /// <summary>
        /// Configuration step for metrics setup.
        /// </summary>
        public interface IMetricsStep
        {
            /// <summary>
            /// Configures metrics services.
            /// </summary>
            /// <param name="configureMetrics">Optional action to configure the meter provider.</param>
            /// <returns>The next configuration step for tracing setup.</returns>
            ITracingStep WithMetrics(Action<MeterProviderBuilder>? configureMetrics = null);
        }

        /// <summary>
        /// Configuration step for tracing setup.
        /// </summary>
        public interface ITracingStep
        {
            /// <summary>
            /// Configures tracing services.
            /// </summary>
            /// <param name="configureMetrics">Optional action to configure the tracer provider.</param>
            /// <returns>The next configuration step for health check setup.</returns>
            IHealthCheckStep WithTracing(Action<TracerProviderBuilder>? configureMetrics = null);
        }

        /// <summary>
        /// Configuration step for health check setup.
        /// </summary>
        public interface IHealthCheckStep
        {
            /// <summary>
            /// Configures health check services.
            /// </summary>
            /// <param name="healthCheckBuilder">Optional action to configure health checks.</param>
            /// <returns>The next configuration step for additional service configuration.</returns>
            IAdditionalConfigurationStep WithHealthChecks(Action<IHealthChecksBuilder>? healthCheckBuilder = null);
        }

        /// <summary>
        /// Configuration step for additional service configuration.
        /// </summary>
        public interface IAdditionalConfigurationStep
        {
            /// <summary>
            /// Configures additional services.
            /// </summary>
            /// <param name="serviceCollection">Optional action to configure additional services.</param>
            /// <returns>The final web application builder step.</returns>
            IWebApplicationBuilder WithServiceConfiguration(Action<IServiceCollection>? serviceCollection = null);
        }

        /// <summary>
        /// Final configuration step for building the web application.
        /// </summary>
        public interface IWebApplicationBuilder
        {
            /// <summary>
            /// Builds and configures the web application.
            /// </summary>
            /// <param name="appConfig">Optional action to configure the web application.</param>
            /// <returns>The configured web application.</returns>
            WebApplication BuildWebApp(Action<WebApplication>? appConfig = null);
        }
    }
}
