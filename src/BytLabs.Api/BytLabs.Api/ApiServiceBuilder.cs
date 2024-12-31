using BytLabs.Multitenancy;
using BytLabs.Application;
using BytLabs.Observability;
using BytLabs.Observability.HealthChecks;
using BytLabs.Observability.Enrichers;
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
        /// Creates a new instance of the API service builder.
        /// </summary>
        /// <param name="webApplicationBuilder">The ASP.NET Core web application builder.</param>
        /// <returns>The initial configuration step.</returns>
        public static IInitialStep CreateBuilder(WebApplicationBuilder webApplicationBuilder) =>
            new ApiServiceBuilder(webApplicationBuilder);

        /// <summary>
        /// Configures HTTP context accessor and user context providers.
        /// </summary>
        /// <returns>The next configuration step for multi-tenant setup.</returns>
        public IMultiTenantStep WithHttpContextAccessor()
        {
            _webApplicationBuilder.Services
                .AddHeadersPropagations()
                .AddHttpContextAccessor()
                .AddUserContextProviders()
                    .AddResolver<HttpUserContextResolver>();

            return this;
        }

        /// <summary>
        /// Configures multi-tenant context and tenant resolution.
        /// </summary>
        /// <returns>The next configuration step for authentication setup.</returns>
        public ILoggingStep WithMultiTenantContext()
        {
            _webApplicationBuilder.Services
                .AddMultitenancy()
                .AddResolver<FromHeaderTenantIdResolver>();

            return this;
        }

        /// <summary>
        /// Configures metrics collection and reporting.
        /// </summary>
        /// <param name="configureMetrics">Optional action to configure additional metrics options.</param>
        /// <returns>The next configuration step for tracing setup.</returns>
        public ITracingStep WithMetrics(Action<MeterProviderBuilder>? configureMetrics = null)
        {
            var config = _webApplicationBuilder.Configuration.GetConfiguration<ObservabilityConfiguration>();
            _webApplicationBuilder.Services.AddMetrics(config);
            return this;
        }

        /// <summary>
        /// Configures distributed tracing.
        /// </summary>
        /// <param name="configureMetrics">Optional action to configure additional tracing options.</param>
        /// <returns>The next configuration step for health check setup.</returns>
        public IHealthCheckStep WithTracing(Action<TracerProviderBuilder>? configureMetrics = null)
        {
            var config = _webApplicationBuilder.Configuration.GetConfiguration<ObservabilityConfiguration>();
            _webApplicationBuilder.Services.AddTracing(config);
            return this;
        }

        /// <summary>
        /// Configures health checks for the application.
        /// </summary>
        /// <param name="healthCheckBuilder">Optional action to configure additional health checks.</param>
        /// <returns>The next configuration step for additional service configuration.</returns>
        public IAdditionalConfigurationStep WithHealthChecks(Action<IHealthChecksBuilder>? healthCheckBuilder = null)
        {
            _webApplicationBuilder.Services.AddHealthChecks(healthCheckBuilder);
            return this;
        }

        /// <summary>
        /// Configures logging services.
        /// </summary>
        /// <returns>The next configuration step for metrics setup.</returns>
        public IMetricsStep WithLogging()
        {
            var config = _webApplicationBuilder.Configuration.GetConfiguration<ObservabilityConfiguration>();
            _webApplicationBuilder.AddLogging(config);
            return this;
        }

        /// <summary>
        /// Allows for additional service configuration.
        /// </summary>
        /// <param name="serviceCollection">Optional action to configure additional services.</param>
        /// <returns>The web application builder for final configuration.</returns>
        public IWebApplicationBuilder WithServiceConfiguration(Action<IServiceCollection>? serviceCollection = null)
        {
            serviceCollection?.Invoke(_webApplicationBuilder.Services);
            return this;
        }

        /// <summary>
        /// Builds and configures the web application with standard middleware.
        /// </summary>
        /// <param name="appConfig">Optional action to configure additional application options.</param>
        /// <returns>The configured web application ready to run.</returns>
        public WebApplication BuildWebApp(Action<WebApplication>? appConfig = null)
        {
            WebApplication app = _webApplicationBuilder.Build();
            
            app.UseHeaderPropagation();
            app.UseSerilogRequestLogging();
            app.UseHealthChecks();
            app.EnrichLoggerWithTenantId();

            if (app.Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();

            appConfig?.Invoke(app);

            return app;
        }
    }
}
