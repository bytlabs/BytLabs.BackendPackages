using BytLabs.Multitenancy;
using BytLabs.Application;
using BytLabs.Observability;
using BytLabs.Observability.HealthChecks;
using BytLabs.Api.UserContextResolvers;
using BytLabs.Api.TenantProvider;
using BytLabs.Api.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Hosting;
using Serilog;

using static BytLabs.Api.ApiBuilderSteps;
using BytLabs.Observability.Middlewares;
using BytLabs.Application.UserContext;

namespace BytLabs.Api
{
    /// <summary>
    /// Builder class for configuring and constructing a web API application with standard middleware and services.
    /// Implements a fluent interface pattern for step-by-step configuration.
    /// </summary>
    public class ApiServiceBuilder :
        IInitialStep,
        IMultiTenantStep,
        IMetricsStep,
        ITracingStep,
        IHealthCheckStep,
        ILoggingStep,
        IAdditionalConfigurationStep,
        IWebApplicationBuilder
    {
        private WebApplicationBuilder _webApplicationBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiServiceBuilder"/> class.
        /// </summary>
        /// <param name="webApplicationBuilder">The ASP.NET Core web application builder.</param>
        public ApiServiceBuilder(WebApplicationBuilder webApplicationBuilder)
        {
            _webApplicationBuilder = webApplicationBuilder;
        }

        /// <summary>
        /// Creates a new instance of the API service builder with the specified web application builder.
        /// </summary>
        /// <param name="webApplicationBuilder">The ASP.NET Core web application builder to use for configuration.</param>
        /// <returns>An initial configuration step to begin the builder chain.</returns>
        public static IInitialStep CreateBuilder(WebApplicationBuilder webApplicationBuilder) =>
            new ApiServiceBuilder(webApplicationBuilder);

        /// <summary>
        /// Configures HTTP context accessor and user context providers for the application.
        /// Enables header propagation and sets up user context resolution.
        /// </summary>
        /// <param name="configureUserContext">Optional action to configure additional user context settings.</param>
        /// <returns>The next configuration step for multi-tenant setup.</returns>
        public IMultiTenantStep WithHttpContextAccessor(Action<UserContextBuilder>? configureUserContext = null)
        {
            _webApplicationBuilder.Services
                .AddHeadersPropagations()
                .AddHttpContextAccessor();

            var userContexBuilder = _webApplicationBuilder.Services
                .AddUserContextProviders();

            configureUserContext?.Invoke(userContexBuilder);

            return this;
        }

        /// <summary>
        /// Configures multi-tenant support for the application, enabling tenant resolution and context management.
        /// </summary>
        /// <param name="configureMultitenancy">Optional action to configure additional multi-tenancy settings.</param>
        /// <returns>The next configuration step for logging setup.</returns>
        public ILoggingStep WithMultiTenantContext(Action<MultitenancyBuilder>? configureMultitenancy = null)
        {
            var multiTenancyBuilder = _webApplicationBuilder.Services
                .AddMultitenancy();

            configureMultitenancy?.Invoke (multiTenancyBuilder);

            return this;
        }

        /// <summary>
        /// Configures metrics collection and reporting using OpenTelemetry.
        /// Sets up meter providers and custom metrics based on the application's configuration.
        /// </summary>
        /// <param name="configureMetrics">Optional action to configure additional metrics options and custom meters.</param>
        /// <returns>The next configuration step for tracing setup.</returns>
        public ITracingStep WithMetrics(Action<MeterProviderBuilder>? configureMetrics = null)
        {
            var config = _webApplicationBuilder.Configuration.GetConfiguration<ObservabilityConfiguration>();
            _webApplicationBuilder.Services.AddMetrics(config, configureMetrics);
            return this;
        }

        /// <summary>
        /// Configures distributed tracing using OpenTelemetry.
        /// Sets up trace providers and sampling based on the application's configuration.
        /// </summary>
        /// <param name="configureTracing">Optional action to configure additional tracing options and custom spans.</param>
        /// <returns>The next configuration step for health check setup.</returns>
        public IHealthCheckStep WithTracing(Action<TracerProviderBuilder>? configureTracing = null)
        {
            var config = _webApplicationBuilder.Configuration.GetConfiguration<ObservabilityConfiguration>();
            _webApplicationBuilder.Services.AddTracing(config, configureTracing);
            return this;
        }

        /// <summary>
        /// Configures health checks for monitoring the application's status and dependencies.
        /// Adds standard health check endpoints and custom health checks as specified.
        /// </summary>
        /// <param name="healthCheckBuilder">Optional action to configure additional health checks and endpoints.</param>
        /// <returns>The next configuration step for additional service configuration.</returns>
        public IAdditionalConfigurationStep WithHealthChecks(Action<IHealthChecksBuilder>? healthCheckBuilder = null)
        {
            _webApplicationBuilder.Services.AddHealthChecks(healthCheckBuilder);
            return this;
        }

        /// <summary>
        /// Configures logging services using Serilog based on the application's configuration.
        /// Sets up log sinks, enrichers, and logging levels.
        /// </summary>
        /// <param name="loggerConfiguration">Optional action to configure additional logging options and sinks.</param>
        /// <returns>The next configuration step for metrics setup.</returns>
        public IMetricsStep WithLogging(Action<LoggerConfiguration>? loggerConfiguration = null)
        {
            var config = _webApplicationBuilder.Configuration.GetConfiguration<ObservabilityConfiguration>();
            _webApplicationBuilder.AddLogging(config, loggerConfiguration);
            return this;
        }

        /// <summary>
        /// Allows for additional service configuration and dependency injection setup.
        /// Use this method to register custom services or override existing ones.
        /// </summary>
        /// <param name="serviceCollection">Optional action to configure additional services in the dependency injection container.</param>
        /// <returns>The web application builder for final configuration.</returns>
        public IWebApplicationBuilder WithServiceConfiguration(Action<IServiceCollection>? serviceCollection = null)
        {
            serviceCollection?.Invoke(_webApplicationBuilder.Services);
            return this;
        }

        /// <summary>
        /// Builds and configures the web application with standard middleware components.
        /// Sets up header propagation, request logging, health checks, and development-specific features.
        /// </summary>
        /// <param name="appConfig">Optional action to configure additional middleware or application options.</param>
        /// <returns>A configured WebApplication instance ready to run.</returns>
        public WebApplication BuildWebApp(Action<WebApplication>? appConfig = null)
        {
            WebApplication app = _webApplicationBuilder.Build();
            
            app.UseHeaderPropagation();
            app.UseSerilogRequestLogging();
            app.UseHealthChecks();
            app.UseTraceIdResponseHeader();
            app.UseLoggerWithTenantId();

            if (app.Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();

            appConfig?.Invoke(app);

            return app;
        }
    }
}
