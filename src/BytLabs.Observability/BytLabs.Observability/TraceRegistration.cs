using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;

namespace BytLabs.Observability
{
    /// <summary>
    /// Provides extension methods for configuring OpenTelemetry tracing in an application.
    /// </summary>
    public static class TraceRegistration
    {
        /// <summary>
        /// Adds OpenTelemetry tracing to the service collection with standard instrumentation.
        /// </summary>
        /// <param name="services">The service collection to add tracing to.</param>
        /// <param name="observabilityConfiguration">Configuration settings for observability features.</param>
        /// <param name="configureTracing">Optional action to configure additional tracing options.</param>
        /// <returns>The service collection with tracing configured.</returns>
        /// <remarks>
        /// This method configures:
        /// - Service name and resource attributes
        /// - HTTP client and ASP.NET Core instrumentation
        /// - OTLP exporter with gRPC protocol
        /// - Exception tracking
        /// - Always-on sampling
        /// </remarks>
        public static IServiceCollection AddTracing(this IServiceCollection services,
            ObservabilityConfiguration observabilityConfiguration,
            Action<TracerProviderBuilder>? configureTracing = null)
        {
            services.AddOpenTelemetry().WithTracing(tracing =>
            {
                tracing
                    .AddSource(observabilityConfiguration.ServiceName)
                    .SetResourceBuilder(ResourceBuilderFactory.Create(observabilityConfiguration))
                    .SetErrorStatusOnException()
                    .SetSampler(new AlwaysOnSampler())
                    .AddHttpClientInstrumentation(options => { options.RecordException = true; })
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        //use env OTEL_DOTNET_EXPERIMENTAL_ASPNETCORE_ENABLE_GRPC_INSTRUMENTATION instead
                        //options.EnableGrpcAspNetCoreSupport = true;
                        options.RecordException = true;
                    });

                tracing
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = observabilityConfiguration.CollectorUri;
                        options.ExportProcessorType = ExportProcessorType.Batch;
                        options.Protocol = OtlpExportProtocol.Grpc;
                        options.TimeoutMilliseconds = observabilityConfiguration.Timeout;
                    });

                configureTracing?.Invoke(tracing);
            });

            return services;
        }
    }
}