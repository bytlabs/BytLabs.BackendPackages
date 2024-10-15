using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;

namespace BytLabs.Observability
{
    /// <summary>
    /// Provides extension methods for configuring metrics in the application.
    /// </summary>
    public static class MetricsRegistration
    {
        /// <summary>
        /// Adds and configures OpenTelemetry metrics to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add metrics to.</param>
        /// <param name="observabilityConfiguration">Configuration for observability features.</param>
        /// <param name="configureMetrics">Optional action to further configure the metrics.</param>
        /// <returns>The service collection with metrics configured.</returns>
        public static IServiceCollection AddMetrics(this IServiceCollection services,
            ObservabilityConfiguration observabilityConfiguration,
            Action<MeterProviderBuilder>? configureMetrics = null)
        {

            services.AddOpenTelemetry().WithMetrics(metrics =>
            {
                var meter = new Meter(observabilityConfiguration.ServiceName);

                MeterProviderBuilder meterProviderBuilder = metrics
                    .AddMeter(meter.Name)
                    .SetResourceBuilder(ResourceBuilderFactory.Create(observabilityConfiguration))
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation();

                configureMetrics?.Invoke(meterProviderBuilder);

                metrics
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = observabilityConfiguration.CollectorUri;
                        options.ExportProcessorType = ExportProcessorType.Batch;
                        options.Protocol = OtlpExportProtocol.Grpc;
                        options.TimeoutMilliseconds = observabilityConfiguration.Timeout;
                    });
            });

            return services;
        }
    }
}